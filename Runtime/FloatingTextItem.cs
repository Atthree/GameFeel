using UnityEngine;

namespace Atthree.GameFeel
{
    /// <summary>Tek bir uçuşan yazı (FloatingText yönetir). Gölgesi okunabilirlik için var.</summary>
    [AddComponentMenu("")]
    public class FloatingTextItem : MonoBehaviour
    {
        TextMesh text, shadow;
        Vector3 start;
        Color color;
        float size, duration, age;
        Vector3 drift;

        internal void Build(Font font)
        {
            text = CreateMesh(transform, font, "Text", Vector3.zero, 0);
            shadow = CreateMesh(transform, font, "Shadow", new Vector3(0.04f, -0.04f, 0.02f), -1);
        }

        static TextMesh CreateMesh(Transform parent, Font font, string name, Vector3 offset, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = offset;
            var mesh = go.AddComponent<TextMesh>();
            mesh.font = font;
            mesh.fontSize = 64;
            mesh.characterSize = 0.05f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.fontStyle = FontStyle.Bold;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
            renderer.sortingOrder = order;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return mesh;
        }

        internal void Play(string value, Vector3 position, Color color, float size, float duration)
        {
            text.text = shadow.text = value;
            start = position;
            this.color = color;
            this.size = size;
            this.duration = Mathf.Max(0.1f, duration);
            age = 0f;
            drift = new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f);
            Update();
        }

        void Update()
        {
            age += Time.unscaledDeltaTime;
            float t = age / duration;
            if (t >= 1f) { FloatingText.Return(this); return; }

            // Yüksel (yavaşlayarak), önce büyü sonra hafif küçül, son %40'ta sön.
            float rise = EaseFunctions.Evaluate(Ease.OutCubic, t) * 1.2f * size;
            float pop = t < 0.15f ? EaseFunctions.Evaluate(Ease.OutBack, t / 0.15f) : Mathf.Lerp(1f, 0.85f, (t - 0.15f) / 0.85f);
            float alpha = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;

            transform.position = start + Vector3.up * rise + drift * t;
            transform.localScale = Vector3.one * (size * pop);

            var cam = Camera.main;
            if (cam != null) transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position, cam.transform.up);

            text.color = new Color(color.r, color.g, color.b, color.a * alpha);
            shadow.color = new Color(0f, 0f, 0f, 0.6f * alpha);
        }
    }
}
