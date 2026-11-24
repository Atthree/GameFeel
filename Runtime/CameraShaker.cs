using UnityEngine;

namespace Atthree.GameFeel
{
    /// <summary>
    /// "Trauma" tabanlı kamera sarsıntısı (GDC: Squirrel Eiserloh, "Math for Game Programmers: Juicing Your Cameras").
    ///
    ///  - Darbe geldikçe trauma (0..1) artar, zamanla azalır.
    ///  - Sarsıntı = trauma² : küçük darbeler hafif, büyükler çok daha sert hissedilir.
    ///  - Rastgele sayı yerine Perlin gürültüsü: sarsıntı titrek değil, akıcı olur.
    ///
    /// Başka bir script kamerayı her kare yerleştiriyorsa (takip kamerası) çakışmaz: sarsıntı karenin
    /// en sonunda eklenir, bir sonraki karenin en başında geri alınır.
    ///
    /// Kameraya ekle ya da hiç ekleme: Feel.Shake() ilk çağrıda Camera.main'e kendisi ekler.
    /// </summary>
    [DefaultExecutionOrder(32000)] // tüm LateUpdate'lerden sonra
    [AddComponentMenu("Game Feel/Camera Shaker")]
    public class CameraShaker : MonoBehaviour
    {
        [Tooltip("Tam sarsıntıda en fazla konum kayması (m).")]
        public float maxOffset = 0.35f;
        [Tooltip("Tam sarsıntıda en fazla dönme (derece). Yalpalamak çoğu zaman kaymaktan daha iyi hissettirir.")]
        public float maxAngle = 4f;
        [Tooltip("Sarsıntının hızı.")]
        public float frequency = 22f;
        [Tooltip("Trauma'nın saniyede azalma miktarı.")]
        public float traumaDecay = 1.4f;
        [Tooltip("Sarsıntı = trauma ^ bu değer. 2 veya 3 iyi sonuç verir.")]
        public float power = 2f;
        [Tooltip("Hit stop / duraklatma sırasında da sarsılsın.")]
        public bool unscaledTime = true;

        [Range(0f, 1f)] [SerializeField] float trauma;

        public float Trauma => trauma;

        /// <summary>Sahnedeki ana kameranın sarsıcısı (yoksa ekler).</summary>
        public static CameraShaker Main
        {
            get
            {
                var cam = Camera.main;
                if (cam == null) return null;
                return cam.TryGetComponent(out CameraShaker s) ? s : cam.gameObject.AddComponent<CameraShaker>();
            }
        }

        Vector3 restPosition;
        Quaternion restRotation;
        bool applied;
        float time;
        float seed;

        void Awake()
        {
            seed = Random.value * 100f;
            // Geri alma işini, her şeyden önce çalışan küçük bir yardımcı yapar.
            if (!TryGetComponent(out CameraShakeRestorer restorer)) restorer = gameObject.AddComponent<CameraShakeRestorer>();
            restorer.hideFlags = HideFlags.HideInInspector;
            restorer.shaker = this;
        }

        /// <summary>Sarsıntı ekle (0..1). 0.2 hafif vuruş, 0.5 güçlü vuruş, 1 patlama.</summary>
        public void AddTrauma(float amount) => trauma = Mathf.Clamp01(trauma + amount);

        public void Stop()
        {
            trauma = 0f;
            Restore();
        }

        void LateUpdate()
        {
            float dt = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (trauma <= 0f) return;

            time += dt * frequency;
            float shake = Mathf.Pow(trauma, power);

            restPosition = transform.position;
            restRotation = transform.rotation;

            Vector3 offset = new Vector3(Noise(0f), Noise(1f), Noise(2f) * 0.5f) * (maxOffset * shake);
            Vector3 angles = new Vector3(Noise(3f), Noise(4f), Noise(5f)) * (maxAngle * shake);

            transform.SetPositionAndRotation(
                restPosition + restRotation * offset,
                restRotation * Quaternion.Euler(angles));
            applied = true;

            trauma = Mathf.Max(0f, trauma - traumaDecay * dt);
        }

        float Noise(float channel) => Mathf.PerlinNoise(seed + channel * 17.3f, time) * 2f - 1f;

        /// <summary>Kamerayı sarsıntısız konumuna geri koyar (bir sonraki karenin en başında).</summary>
        internal void Restore()
        {
            if (!applied) return;
            transform.SetPositionAndRotation(restPosition, restRotation);
            applied = false;
        }

        void OnDisable() => Restore();
    }
}
