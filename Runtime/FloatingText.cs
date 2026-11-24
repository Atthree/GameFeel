using System.Collections.Generic;
using UnityEngine;

namespace Atthree.GameFeel
{
    /// <summary>
    /// Uçuşan yazılar: hasar sayıları, "+10 altın", "KRİTİK!". Kameraya döner, yükselir, büyüyüp söner.
    /// Objeler havuzdan gelir (yaratılıp silinmez). Ek paket gerektirmez (Unity'nin dahili fontu).
    ///
    ///   FloatingText.Spawn("120", hitPoint, Color.yellow);
    ///   Feel.Text("KRİTİK!", hitPoint, Color.red, size: 1.6f);
    /// </summary>
    public static class FloatingText
    {
        static readonly Stack<FloatingTextItem> pool = new Stack<FloatingTextItem>();
        static Transform root;
        static Font font;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            pool.Clear();
            root = null;
        }

        public static void Spawn(string text, Vector3 position, Color color, float size = 1f, float duration = 0.9f)
        {
            var runner = TweenRunner.Instance;
            if (runner == null) return;
            if (root == null)
            {
                root = new GameObject("Floating Texts").transform;
                root.SetParent(runner.transform, false);
                pool.Clear();
            }

            var item = pool.Count > 0 ? pool.Pop() : Create();
            item.gameObject.SetActive(true);
            item.Play(text, position, color, size, duration);
        }

        internal static void Return(FloatingTextItem item)
        {
            item.gameObject.SetActive(false);
            pool.Push(item);
        }

        internal static int PooledCount => pool.Count;

        static FloatingTextItem Create()
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go = new GameObject("Text");
            go.transform.SetParent(root, false);
            var item = go.AddComponent<FloatingTextItem>();
            item.Build(font);
            return item;
        }
    }
}
