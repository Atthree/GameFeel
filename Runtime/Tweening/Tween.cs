using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Atthree.GameFeel
{
    /// <summary>
    /// Küçük, bağımlılıksız tween motoru: bir değeri zaman içinde yumuşakça değiştirir.
    ///
    ///   Tween.Move(transform, new Vector3(0, 2, 0), 0.5f).SetEase(Ease.OutBack);
    ///   Tween.Scale(button, Vector3.one * 1.2f, 0.2f).SetLoops(2, yoyo: true);
    ///   Tween.PunchScale(transform, new Vector3(0.3f, -0.2f, 0.3f), 0.25f);
    ///   Tween.Float(0f, 1f, 2f, v => canvasGroup.alpha = v).OnComplete(() => Debug.Log("bitti"));
    ///   Tween.Delay(1f, () => SpawnEnemy());
    ///
    /// Sahibi (owner) yok edilen tween'ler kendiliğinden durur. Tween.Kill(owner) ile elle durdurulur.
    /// Varsayılan olarak Time.timeScale'den etkilenir; oyun duraklarken de çalışması için .SetUnscaled().
    /// </summary>
    public class Tween
    {
        // ------------------------------------------------------------------ ayarlar
        internal Object owner;
        internal bool hasOwner;
        internal object tag;             // aynı türden tween'leri bulmak için (ör. "punchScale")
        internal float duration;
        internal float delay;
        internal float elapsed;
        internal Ease ease = Ease.OutQuad;
        internal bool unscaled;
        internal int loops = 1;          // -1 = sonsuz
        internal bool yoyo;
        internal int completedLoops;
        internal Action<float> apply;    // yumuşatılmış ilerleme (0..1) ile değeri uygula
        internal Action onComplete;

        public bool IsActive { get; internal set; }
        /// <summary>Şu anki ilerleme (0..1, yumuşatma öncesi).</summary>
        public float Progress => duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);

        // ------------------------------------------------------------------ zincirleme ayarlar
        public Tween SetEase(Ease value) { ease = value; return this; }
        public Tween SetDelay(float seconds) { delay = Mathf.Max(0f, seconds); return this; }
        /// <summary>Time.timeScale'i yok say (duraklatma menüsü, hit stop sırasında çalışan efektler).</summary>
        public Tween SetUnscaled(bool value = true) { unscaled = value; return this; }
        /// <summary>Tekrar sayısı (-1 = sonsuz). yoyo: gidip geri gelir.</summary>
        public Tween SetLoops(int count, bool yoyo = false) { loops = count == 0 ? 1 : count; this.yoyo = yoyo; return this; }
        public Tween OnComplete(Action action) { onComplete += action; return this; }

        /// <summary>Durdurur. complete = true ise son değeri uygulayıp OnComplete'i çağırır.</summary>
        public void Kill(bool complete = false)
        {
            if (!IsActive) return;
            IsActive = false;
            if (complete)
            {
                apply?.Invoke(yoyo && loops > 0 && loops % 2 == 0 ? 0f : 1f);
                onComplete?.Invoke();
            }
        }

        /// <summary>Bir kare ilerlet. Bitince false döner.</summary>
        internal bool Step(float dt)
        {
            if (!IsActive) return false;
            if (hasOwner && owner == null) { IsActive = false; return false; } // sahibi yok oldu

            if (delay > 0f)
            {
                delay -= dt;
                if (delay > 0f) return true;
                dt = -delay;
                delay = 0f;
            }

            elapsed += dt;
            while (true)
            {
                float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                bool reverse = yoyo && completedLoops % 2 == 1;
                apply?.Invoke(EaseFunctions.Evaluate(ease, reverse ? 1f - t : t));
                if (t < 1f) return true;

                completedLoops++;
                if (loops > 0 && completedLoops >= loops)
                {
                    IsActive = false;
                    onComplete?.Invoke();
                    return false;
                }
                elapsed -= Mathf.Max(duration, 0.0001f);
                if (elapsed < 0f) elapsed = 0f;
                if (duration <= 0f) return true;
            }
        }

        // ================================================================== oluşturucular
        static Tween Create(Object owner, float duration, Action<float> apply, object tag = null)
        {
            var tween = new Tween
            {
                owner = owner,
                hasOwner = owner != null,
                duration = Mathf.Max(0f, duration),
                apply = apply,
                tag = tag,
                IsActive = true,
            };
            TweenRunner.Add(tween);
            return tween;
        }

        public static Tween Float(float from, float to, float duration, Action<float> setter, Object owner = null) =>
            Create(owner, duration, t => setter(Mathf.LerpUnclamped(from, to, t)));

        public static Tween Vector(Vector3 from, Vector3 to, float duration, Action<Vector3> setter, Object owner = null) =>
            Create(owner, duration, t => setter(Vector3.LerpUnclamped(from, to, t)));

        public static Tween Color(Color from, Color to, float duration, Action<Color> setter, Object owner = null) =>
            Create(owner, duration, t => setter(UnityEngine.Color.LerpUnclamped(from, to, t)));

        /// <summary>Belirli bir süre sonra bir şey yap (zamanlayıcı).</summary>
        public static Tween Delay(float seconds, Action action, Object owner = null) =>
            Create(owner, 0f, null).SetDelay(seconds).OnComplete(action);

        public static Tween Move(Transform target, Vector3 to, float duration, bool local = false)
        {
            Vector3 from = local ? target.localPosition : target.position;
            return Create(target, duration, t =>
            {
                var p = Vector3.LerpUnclamped(from, to, t);
                if (local) target.localPosition = p; else target.position = p;
            });
        }

        public static Tween Scale(Transform target, Vector3 to, float duration)
        {
            Vector3 from = target.localScale;
            return Create(target, duration, t => target.localScale = Vector3.LerpUnclamped(from, to, t));
        }

        public static Tween Rotate(Transform target, Quaternion to, float duration, bool local = false)
        {
            Quaternion from = local ? target.localRotation : target.rotation;
            return Create(target, duration, t =>
            {
                var r = Quaternion.SlerpUnclamped(from, to, t);
                if (local) target.localRotation = r; else target.rotation = r;
            });
        }

        // ------------------------------------------------------------------ punch & shake
        // Punch: değeri bir yöne "it", sonra yay gibi titreyerek başlangıca dön.
        // Aynı objede yeni bir punch başlarsa eskisi bitirilir: değerler kaymaz.

        public static Tween PunchScale(Transform target, Vector3 punch, float duration, int vibrato = 6) =>
            Punch(target, "punchScale", () => target.localScale, v => target.localScale = v, punch, duration, vibrato);

        public static Tween PunchPosition(Transform target, Vector3 punch, float duration, int vibrato = 6) =>
            Punch(target, "punchPosition", () => target.localPosition, v => target.localPosition = v, punch, duration, vibrato);

        public static Tween PunchRotation(Transform target, Vector3 punchEuler, float duration, int vibrato = 6) =>
            Punch(target, "punchRotation", () => target.localEulerAngles, v => target.localEulerAngles = v, punchEuler, duration, vibrato);

        static Tween Punch(Transform target, string kind, Func<Vector3> get, Action<Vector3> set, Vector3 punch, float duration, int vibrato)
        {
            TweenRunner.Complete(target, kind); // önceki punch bitsin, taban değer temiz kalsın
            Vector3 baseValue = get();
            return Create(target, duration, t =>
            {
                float wave = Mathf.Sin(t * Mathf.PI * vibrato) * (1f - t); // sönen titreşim
                set(baseValue + punch * wave);
            }, kind).SetEase(Ease.Linear);
        }

        /// <summary>Objeyi rastgele sarsar ve başlangıç konumuna döner (UI elemanları, sandıklar).</summary>
        public static Tween ShakePosition(Transform target, float strength, float duration, float frequency = 25f)
        {
            TweenRunner.Complete(target, "shakePosition");
            Vector3 basePosition = target.localPosition;
            float seed = UnityEngine.Random.value * 100f;
            return Create(target, duration, t =>
            {
                float amount = strength * (1f - t);
                float time = t * duration * frequency;
                var offset = new Vector3(
                    Mathf.PerlinNoise(seed, time) * 2f - 1f,
                    Mathf.PerlinNoise(seed + 10f, time) * 2f - 1f,
                    Mathf.PerlinNoise(seed + 20f, time) * 2f - 1f);
                target.localPosition = basePosition + offset * amount;
            }, "shakePosition").SetEase(Ease.Linear).OnComplete(() => target.localPosition = basePosition);
        }

        // ------------------------------------------------------------------ yönetim
        /// <summary>Bu sahibe ait tüm tween'leri durdurur.</summary>
        public static void Kill(Object owner, bool complete = false) => TweenRunner.KillOwner(owner, complete);

        public static void KillAll() => TweenRunner.KillAll();

        public static int ActiveCount => TweenRunner.Count;
    }
}
