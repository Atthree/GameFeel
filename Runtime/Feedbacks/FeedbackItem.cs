using System;
using UnityEngine;
using UnityEngine.Events;

namespace Atthree.GameFeel
{
    /// <summary>
    /// FeedbackPlayer listesindeki tek bir efekt. Kendi efektini yazmak için bundan türet:
    ///
    ///   [Serializable] public class MyFeedback : FeedbackItem
    ///   {
    ///       public float power = 1f;
    ///       public override string Label => "Benim efektim";
    ///       public override void Play(FeedbackPlayer player, Vector3 position) { ... }
    ///   }
    ///
    /// Inspector'daki "Efekt Ekle" menüsünde otomatik görünür.
    /// </summary>
    [Serializable]
    public abstract class FeedbackItem
    {
        public bool enabled = true;
        [Tooltip("Oynatma emrinden kaç saniye sonra (ölçeksiz zaman).")]
        public float delay;

        public abstract string Label { get; }

        /// <param name="player">Efekti çalan bileşen (hedef verilmemişse onun objesi kullanılır).</param>
        /// <param name="position">Efektin olduğu yer (vuruş noktası); verilmemişse oyuncunun konumu.</param>
        public abstract void Play(FeedbackPlayer player, Vector3 position);

        protected static Transform Or(Transform target, FeedbackPlayer player) => target != null ? target : player.transform;
    }

    [Serializable]
    public class ShakeFeedback : FeedbackItem
    {
        [Range(0f, 1f)] public float trauma = 0.3f;
        public override string Label => $"Kamera Sarsıntısı ({trauma:0.##})";
        public override void Play(FeedbackPlayer player, Vector3 position) => Feel.Shake(trauma);
    }

    [Serializable]
    public class HitStopFeedback : FeedbackItem
    {
        public float seconds = 0.06f;
        [Range(0f, 1f)] public float timeScale = 0f;
        public override string Label => $"Hit Stop ({seconds:0.###} sn)";
        public override void Play(FeedbackPlayer player, Vector3 position) => Feel.HitStop(seconds, timeScale);
    }

    [Serializable]
    public class SlowMotionFeedback : FeedbackItem
    {
        [Range(0.01f, 1f)] public float timeScale = 0.3f;
        public float seconds = 0.4f;
        public float recoverSeconds = 0.3f;
        public override string Label => $"Ağır Çekim (%{timeScale * 100:0}, {seconds:0.##} sn)";
        public override void Play(FeedbackPlayer player, Vector3 position) => Feel.SlowMotion(timeScale, seconds, recoverSeconds);
    }

    [Serializable]
    public class FlashFeedback : FeedbackItem
    {
        [Tooltip("Boşsa FeedbackPlayer'ın objesi.")]
        public GameObject target;
        public Color color = Color.white;
        public float seconds = 0.12f;
        public override string Label => "Flash";
        public override void Play(FeedbackPlayer player, Vector3 position) =>
            Feel.Flash(target != null ? target : player.gameObject, color, seconds);
    }

    [Serializable]
    public class PunchScaleFeedback : FeedbackItem
    {
        public Transform target;
        [Tooltip("Basıl: (+, -, +). Uza: (-, +, -).")]
        public Vector3 punch = new Vector3(0.25f, -0.25f, 0.25f);
        public float seconds = 0.3f;
        public int vibrato = 6;
        public override string Label => "Punch Scale (basıl-uza)";
        public override void Play(FeedbackPlayer player, Vector3 position) => Tween.PunchScale(Or(target, player), punch, seconds, vibrato);
    }

    [Serializable]
    public class PunchPositionFeedback : FeedbackItem
    {
        public Transform target;
        public Vector3 punch = new Vector3(0f, 0f, -0.3f);
        public float seconds = 0.25f;
        public int vibrato = 4;
        public override string Label => "Punch Position (geri tepme)";
        public override void Play(FeedbackPlayer player, Vector3 position) => Tween.PunchPosition(Or(target, player), punch, seconds, vibrato);
    }

    [Serializable]
    public class ShakeObjectFeedback : FeedbackItem
    {
        public Transform target;
        public float strength = 0.15f;
        public float seconds = 0.3f;
        public override string Label => "Obje Sarsıntısı";
        public override void Play(FeedbackPlayer player, Vector3 position) => Tween.ShakePosition(Or(target, player), strength, seconds);
    }

    [Serializable]
    public class ParticlesFeedback : FeedbackItem
    {
        [Tooltip("Sahnedeki bir parçacık sistemi ya da bir prefab.")]
        public ParticleSystem particles;
        [Tooltip("Açık: efekt noktasına bir kopyasını yarat (prefab için). Kapalı: olduğu yerde oynat.")]
        public bool spawnCopyAtPosition = true;
        public override string Label => particles != null ? $"Parçacık ({particles.name})" : "Parçacık";

        public override void Play(FeedbackPlayer player, Vector3 position)
        {
            if (particles == null) return;
            if (!spawnCopyAtPosition) { particles.Play(true); return; }
            var copy = UnityEngine.Object.Instantiate(particles, position, particles.transform.rotation);
            copy.gameObject.SetActive(true);
            copy.Play(true);
            var main = copy.main;
            UnityEngine.Object.Destroy(copy.gameObject, main.duration + main.startLifetime.constantMax + 0.5f);
        }
    }

    [Serializable]
    public class SoundFeedback : FeedbackItem
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 0.5f)] public float pitchVariation = 0.08f;
        [Tooltip("Açık: efekt noktasında 3D. Kapalı: 2D.")]
        public bool positional = true;
        public override string Label => clip != null ? $"Ses ({clip.name})" : "Ses";

        // Bağımlılık olmasın diye basit çalma. Audio Manager kullanıyorsan "Olay (UnityEvent)" efektinden
        // AudioManager.Play'i çağırabilirsin.
        public override void Play(FeedbackPlayer player, Vector3 position)
        {
            if (clip == null) return;
            var go = new GameObject("Feedback Sound");
            go.transform.position = position;
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            source.spatialBlend = positional ? 1f : 0f;
            source.minDistance = 3f;
            source.Play();
            UnityEngine.Object.Destroy(go, clip.length / Mathf.Max(0.1f, source.pitch) + 0.1f);
        }
    }

    [Serializable]
    public class TextFeedback : FeedbackItem
    {
        public string text = "!";
        public Color color = Color.white;
        public float size = 1f;
        public Vector3 offset = new Vector3(0f, 0.5f, 0f);
        public override string Label => $"Uçuşan Yazı (\"{text}\")";
        public override void Play(FeedbackPlayer player, Vector3 position) => Feel.Text(text, position + offset, color, size);
    }

    [Serializable]
    public class EventFeedback : FeedbackItem
    {
        [Tooltip("İstediğin herhangi bir şeyi çağır: AudioManager, animasyon, puan...")]
        public UnityEvent onPlay = new UnityEvent();
        public override string Label => "Olay (UnityEvent)";
        public override void Play(FeedbackPlayer player, Vector3 position) => onPlay.Invoke();
    }
}
