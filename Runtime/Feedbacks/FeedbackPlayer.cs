using System.Collections.Generic;
using UnityEngine;

namespace Atthree.GameFeel
{
    /// <summary>
    /// Kod yazmadan "juice" birleştir: Inspector'da efekt listesi kur (sarsıntı, hit stop, flash, punch,
    /// parçacık, ses, yazı, olay...), tek bir Play() ile hepsini birlikte (gecikmeleriyle) oynat.
    ///
    ///   [SerializeField] FeedbackPlayer hitFeedback;
    ///   void OnHit(Vector3 point) => hitFeedback.Play(point);
    ///
    /// UnityEvent'lerden de çağrılabilir (Button.OnClick → FeedbackPlayer.Play).
    /// </summary>
    [AddComponentMenu("Game Feel/Feedback Player")]
    public class FeedbackPlayer : MonoBehaviour
    {
        [SerializeReference] public List<FeedbackItem> items = new List<FeedbackItem>();

        [Tooltip("Obje açılınca otomatik oynat.")]
        public bool playOnEnable;
        [Tooltip("Açıksa Play() hiçbir şey yapmaz (bu objenin efektlerini geçici kapatmak için).")]
        public bool muted;

        void OnEnable()
        {
            if (playOnEnable) Play();
        }

        /// <summary>Tüm efektleri objenin konumunda oynat.</summary>
        public void Play() => Play(transform.position);

        /// <summary>Tüm efektleri verilen noktada oynat (ör. vuruş noktası).</summary>
        public void Play(Vector3 position)
        {
            if (muted || !Application.isPlaying) return;
            foreach (var item in items)
            {
                if (item == null || !item.enabled) continue;
                if (item.delay <= 0f) PlaySafe(item, position);
                else
                {
                    var captured = item;
                    Tween.Delay(item.delay, () => PlaySafe(captured, position), this).SetUnscaled();
                }
            }
        }

        void PlaySafe(FeedbackItem item, Vector3 position)
        {
            try { item.Play(this, position); }
            catch (System.Exception e) { Debug.LogException(e, this); }
        }

        /// <summary>Kodla efekt ekle.</summary>
        public T Add<T>(T item) where T : FeedbackItem
        {
            items.Add(item);
            return item;
        }

        /// <summary>Bir türdeki tüm efektleri aç/kapat (ör. ayarlarda "ekran sarsıntısı kapalı").</summary>
        public void SetEnabled<T>(bool value) where T : FeedbackItem
        {
            foreach (var item in items) if (item is T) item.enabled = value;
        }
    }
}
