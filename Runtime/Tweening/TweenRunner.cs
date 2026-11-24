using System.Collections.Generic;
using UnityEngine;

namespace Atthree.GameFeel
{
    /// <summary>
    /// Tween'leri ve zamanlanmış efektleri her kare ilerleten görünmez obje ("[Game Feel]").
    /// İlk kullanımda kendiliğinden oluşur, sahne değişince yok olmaz.
    /// </summary>
    [AddComponentMenu("")]
    [DefaultExecutionOrder(-100)]
    public class TweenRunner : MonoBehaviour
    {
        static TweenRunner instance;
        static bool quitting;

        readonly List<Tween> tweens = new List<Tween>();
        readonly List<Tween> adding = new List<Tween>();
        bool updating;

        /// <summary>Her kare (ölçeksiz zamanla) çağrılır: hit stop, ağır çekim gibi zaman efektleri için.</summary>
        internal static event System.Action<float> UnscaledUpdate;

        internal static int Count => instance != null ? instance.tweens.Count + instance.adding.Count : 0;

        internal static TweenRunner Instance
        {
            get
            {
                if (instance != null || quitting || !Application.isPlaying) return instance;
                var go = new GameObject("[Game Feel]");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<TweenRunner>();
                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            instance = null;
            quitting = false;
            UnscaledUpdate = null;
            Application.quitting -= OnQuit;
            Application.quitting += OnQuit;
        }

        static void OnQuit() => quitting = true;

        internal static void Add(Tween tween)
        {
            var runner = Instance;
            if (runner == null) { tween.IsActive = false; return; } // editör modunda / kapanırken
            (runner.updating ? runner.adding : runner.tweens).Add(tween);
        }

        void Update()
        {
            float dt = Time.deltaTime, udt = Time.unscaledDeltaTime;
            updating = true;
            for (int i = tweens.Count - 1; i >= 0; i--)
            {
                var t = tweens[i];
                if (!t.Step(t.unscaled ? udt : dt)) tweens.RemoveAt(i);
            }
            updating = false;
            if (adding.Count > 0) { tweens.AddRange(adding); adding.Clear(); }

            UnscaledUpdate?.Invoke(udt);
        }

        // ------------------------------------------------------------------ yönetim
        // Kopya liste: Kill(complete) OnComplete'i çağırır, o da yeni tween ekleyebilir;
        // asıl listeyi gezerken değiştirmek hata verirdi.
        List<Tween> All()
        {
            var all = new List<Tween>(tweens.Count + adding.Count);
            all.AddRange(tweens);
            all.AddRange(adding);
            return all;
        }

        internal static void Complete(Object owner, object tag)
        {
            if (instance == null) return;
            foreach (var t in instance.All())
                if (t.IsActive && t.owner == owner && Equals(t.tag, tag)) t.Kill(complete: true);
        }

        internal static void KillOwner(Object owner, bool complete)
        {
            if (instance == null) return;
            foreach (var t in instance.All())
                if (t.IsActive && t.hasOwner && t.owner == owner) t.Kill(complete);
        }

        internal static void KillAll()
        {
            if (instance == null) return;
            foreach (var t in instance.All()) t.IsActive = false;
        }
    }
}
