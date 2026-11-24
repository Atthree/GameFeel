using UnityEngine;

namespace Atthree.GameFeel
{
    /// <summary>
    /// Tek satırlık "juice" kısayolları:
    ///
    ///   Feel.Shake(0.4f);                         // kamera sarsıntısı (0..1)
    ///   Feel.HitStop(0.06f);                      // vuruşta kısa donma
    ///   Feel.SlowMotion(0.3f, 0.5f);              // yarım saniye %30 hızda
    ///   Feel.Flash(enemy);                        // beyaz yanıp sönme
    ///   Feel.Squash(player.transform);            // zıplarken basıl-uza (squash & stretch)
    ///   Feel.Text("120", hitPoint, Color.yellow); // uçuşan hasar sayısı
    ///
    /// Bir vuruşu "hissettirmek" için genelde birkaçı birlikte kullanılır. Bunları kod yerine
    /// Inspector'dan birleştirmek için <see cref="FeedbackPlayer"/>.
    /// </summary>
    public static class Feel
    {
        // ------------------------------------------------------------------ erişilebilirlik
        // Bazı oyuncular sarsıntı ve yanıp sönmeden rahatsız olur (baş dönmesi, ışığa duyarlılık).
        // Bunları oyunun ayarlar menüsüne bağla; paket her yerde bu değerlere uyar.

        /// <summary>Kamera sarsıntısının şiddeti (0 = kapalı, 1 = normal).</summary>
        public static float ShakeIntensity = 1f;
        /// <summary>Flash efektleri açık mı.</summary>
        public static bool FlashEnabled = true;
        /// <summary>Hit stop ve ağır çekim açık mı.</summary>
        public static bool TimeEffectsEnabled = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            ShakeIntensity = 1f;
            FlashEnabled = true;
            TimeEffectsEnabled = true;
        }

        /// <summary>Ana kamerayı sarsar. 0.2 hafif, 0.5 güçlü, 1 patlama. Üst üste eklenir.</summary>
        public static void Shake(float trauma)
        {
            if (ShakeIntensity <= 0f) return;
            var shaker = CameraShaker.Main;
            if (shaker != null) shaker.AddTrauma(trauma * ShakeIntensity);
        }

        /// <summary>Oyunu çok kısa dondurur (0.03–0.1 sn). Vuruşa ağırlık verir.</summary>
        public static void HitStop(float seconds, float timeScale = 0f)
        {
            if (TimeEffectsEnabled) TimeEffects.HitStop(seconds, timeScale);
        }

        /// <summary>Ağır çekim: timeScale (ör. 0.3) ile seconds kadar, sonra recoverSeconds'ta normale döner.</summary>
        public static void SlowMotion(float timeScale, float seconds, float recoverSeconds = 0.3f)
        {
            if (TimeEffectsEnabled) TimeEffects.SlowMotion(timeScale, seconds, recoverSeconds);
        }

        /// <summary>Objeyi bir anlığına renge boyar (varsayılan beyaz). Objeye HitFlash yoksa ekler.</summary>
        public static void Flash(GameObject target, Color? color = null, float seconds = -1f)
        {
            if (target == null || !FlashEnabled) return;
            if (!target.TryGetComponent(out HitFlash flash)) flash = target.AddComponent<HitFlash>();
            flash.Flash(color ?? flash.color, seconds > 0f ? seconds : flash.duration);
        }

        /// <summary>Basıl-uza (squash and stretch): yatayda genişler, dikeyde basılır, sonra yaylanır.</summary>
        public static Tween Squash(Transform target, float amount = 0.25f, float seconds = 0.3f) =>
            Tween.PunchScale(target, new Vector3(amount, -amount, amount), seconds);

        /// <summary>Uza-basıl: zıplamanın kalkış anı gibi (dikeyde uzar).</summary>
        public static Tween Stretch(Transform target, float amount = 0.25f, float seconds = 0.3f) =>
            Tween.PunchScale(target, new Vector3(-amount * 0.6f, amount, -amount * 0.6f), seconds);

        /// <summary>Uçuşan yazı (hasar sayısı, "+10" vb.).</summary>
        public static void Text(string text, Vector3 position, Color? color = null, float size = 1f, float seconds = 0.9f) =>
            FloatingText.Spawn(text, position, color ?? Color.white, size, seconds);
    }
}
