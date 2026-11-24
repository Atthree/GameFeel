using UnityEngine;

namespace Atthree.GameFeel
{
    /// <summary>
    /// Objeyi bir anlığına bir renge (genelde beyaz) boyayıp eski rengine döndürür: "vuruldum" hissi.
    ///
    /// Materyali kopyalamaz (MaterialPropertyBlock kullanır): bellek harcamaz, diğer objeleri etkilemez.
    /// URP/Lit (_BaseColor) ve Built-in Standard (_Color) materyallerde çalışır.
    /// Not: zaten beyaz bir objeyi beyaza boyamak görünmez; o durumda flash rengini kırmızı yap.
    /// </summary>
    [AddComponentMenu("Game Feel/Hit Flash")]
    public class HitFlash : MonoBehaviour
    {
        public Color color = Color.white;
        public float duration = 0.12f;
        [Tooltip("Alt objelerdeki renderer'ları da boya.")]
        public bool includeChildren = true;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        Renderer[] renderers;
        Color[][] originals; // [renderer][material]
        MaterialPropertyBlock block;
        float timer, currentDuration;
        Color flashColor;

        public bool IsFlashing => timer > 0f;

        void Awake() => Cache();

        void Cache()
        {
            block = new MaterialPropertyBlock();
            var all = includeChildren ? GetComponentsInChildren<Renderer>(true) : GetComponents<Renderer>();
            // Sadece mesh renderer'lar (parçacık ve yazı renderer'ları hariç).
            renderers = System.Array.FindAll(all, r => r is MeshRenderer || r is SkinnedMeshRenderer);
            originals = new Color[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                var materials = renderers[i].sharedMaterials;
                originals[i] = new Color[materials.Length];
                for (int m = 0; m < materials.Length; m++)
                    originals[i][m] = ColorOf(materials[m]);
            }
            enabled = false;
        }

        static Color ColorOf(Material material)
        {
            if (material == null) return Color.white;
            if (material.HasProperty(BaseColorId)) return material.GetColor(BaseColorId);
            if (material.HasProperty(ColorId)) return material.GetColor(ColorId);
            return Color.white;
        }

        public void Flash() => Flash(color, duration);

        public void Flash(Color flashColor, float seconds)
        {
            if (renderers == null) Cache();
            this.flashColor = flashColor;
            currentDuration = Mathf.Max(0.01f, seconds);
            timer = currentDuration;
            enabled = true;
            Apply(1f);
        }

        // Ölçeksiz zaman: hit stop sırasında (timeScale = 0) da flash görünür ve biter.
        void Update()
        {
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f) { Clear(); return; }
            float t = timer / currentDuration;
            Apply(t * t); // hızlıca söner
        }

        void Apply(float amount)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;
                for (int m = 0; m < originals[i].Length; m++)
                {
                    var c = Color.Lerp(originals[i][m], flashColor, amount);
                    r.GetPropertyBlock(block, m);
                    block.SetColor(BaseColorId, c);
                    block.SetColor(ColorId, c);
                    r.SetPropertyBlock(block, m);
                }
            }
        }

        void Clear()
        {
            timer = 0f;
            block.Clear();
            foreach (var r in renderers)
            {
                if (r == null) continue;
                for (int m = 0; m < r.sharedMaterials.Length; m++) r.SetPropertyBlock(block, m);
            }
            enabled = false;
        }

        void OnDisable()
        {
            if (timer > 0f) Clear();
        }
    }
}
