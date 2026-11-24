using UnityEngine;

namespace Atthree.GameFeel
{
    /// <summary>
    /// Zamanla oynayan efektler: hit stop (vuruşta kısa donma) ve ağır çekim.
    ///
    /// Kurallar:
    ///  - Efekt başlarken oyunun o anki Time.timeScale'i hatırlanır, bitince geri konur.
    ///  - Üst üste gelen efektler birleşir: en yavaş olan geçerli olur, süreler uzar.
    ///  - Efekt sürerken OYUN Time.timeScale'i kendisi değiştirirse (ör. duraklatma menüsü),
    ///    efektler iptal edilir ve timeScale'e dokunulmaz. Böylece hit stop pause'u bozmaz.
    ///  - Ağır çekimde Time.fixedDeltaTime da ölçeklenir; fizik kesik kesik görünmez.
    /// </summary>
    public static class TimeEffects
    {
        static bool active;
        static float baseScale = 1f;
        static float baseFixedDelta = 0.02f;
        static float lastApplied = -1f;

        static float hitStopUntil;
        static float hitStopScale;

        static float slowScale = 1f;
        static float slowUntil;
        static float slowRecover;

        public static bool IsActive => active;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            active = false;
            lastApplied = -1f;
            hitStopUntil = slowUntil = 0f;
        }

        static float Now => Time.unscaledTime;

        public static void HitStop(float seconds, float timeScale = 0f)
        {
            if (seconds <= 0f || !Begin()) return;
            hitStopScale = hitStopUntil > Now ? Mathf.Min(hitStopScale, timeScale) : timeScale;
            hitStopUntil = Mathf.Max(hitStopUntil, Now + seconds);
            Apply();
        }

        public static void SlowMotion(float timeScale, float seconds, float recoverSeconds = 0.3f)
        {
            if (seconds <= 0f || !Begin()) return;
            slowScale = slowUntil + slowRecover > Now ? Mathf.Min(slowScale, timeScale) : timeScale;
            slowUntil = Mathf.Max(slowUntil, Now + seconds);
            slowRecover = Mathf.Max(0f, recoverSeconds);
            Apply();
        }

        /// <summary>Tüm zaman efektlerini hemen bitirir ve timeScale'i eski haline getirir.</summary>
        public static void Cancel()
        {
            if (!active) return;
            if (Mathf.Approximately(Time.timeScale, lastApplied))
            {
                Time.timeScale = baseScale;
                Time.fixedDeltaTime = baseFixedDelta;
            }
            End();
        }

        static bool Begin()
        {
            if (!Application.isPlaying) return false;
            if (active) return true;
            if (TweenRunner.Instance == null) return false;
            baseScale = Time.timeScale;
            baseFixedDelta = Time.fixedDeltaTime;
            active = true;
            lastApplied = -1f;
            TweenRunner.UnscaledUpdate += Tick;
            return true;
        }

        static void End()
        {
            active = false;
            lastApplied = -1f;
            hitStopUntil = slowUntil = 0f;
            slowRecover = 0f;
            TweenRunner.UnscaledUpdate -= Tick;
        }

        static void Tick(float unscaledDelta)
        {
            // Oyun timeScale'i kendisi değiştirdi (pause menüsü vb.): karışma, iptal et.
            if (lastApplied >= 0f && !Mathf.Approximately(Time.timeScale, lastApplied))
            {
                Time.fixedDeltaTime = baseFixedDelta;
                End();
                return;
            }
            Apply();
        }

        static void Apply()
        {
            float now = Now;
            float scale = baseScale;
            bool slowing = false;

            if (now < slowUntil)
            {
                scale = Mathf.Min(scale, baseScale * slowScale);
                slowing = true;
            }
            else if (now < slowUntil + slowRecover)
            {
                float t = (now - slowUntil) / slowRecover;
                scale = Mathf.Min(scale, baseScale * Mathf.Lerp(slowScale, 1f, t * t));
                slowing = true;
            }

            if (now < hitStopUntil) scale = Mathf.Min(scale, baseScale * hitStopScale);

            bool anything = now < hitStopUntil || slowing;
            if (!anything)
            {
                Time.timeScale = baseScale;
                Time.fixedDeltaTime = baseFixedDelta;
                End();
                return;
            }

            Time.timeScale = scale;
            // Ağır çekimde fizik de sık adımlansın (yoksa hareket kesik görünür). Tam donmada gerek yok.
            Time.fixedDeltaTime = slowing && scale > 0.01f ? baseFixedDelta * (scale / Mathf.Max(baseScale, 0.0001f)) : baseFixedDelta;
            lastApplied = scale;
        }
    }
}
