using UnityEngine;

namespace Atthree.GameFeel.Demo
{
    /// <summary>
    /// Demo hedefi: tıklanınca "vurulur". Bütün juice, objedeki FeedbackPlayer'da Inspector'dan kurulu;
    /// bu script sadece Play'i çağırır ve hasar sayısını yazar.
    /// </summary>
    [AddComponentMenu("Game Feel/Demo/Demo Target")]
    public class DemoTarget : MonoBehaviour
    {
        public FeedbackPlayer hitFeedback;
        public static bool ShowDamageNumbers = true;

        public void Hit(Vector3 point)
        {
            if (hitFeedback != null) hitFeedback.Play(point);

            if (ShowDamageNumbers)
            {
                bool critical = Random.value < 0.2f;
                int damage = Random.Range(8, 25) * (critical ? 3 : 1);
                Feel.Text(critical ? $"{damage}!" : damage.ToString(), point + Vector3.up * 0.3f,
                    critical ? new Color(1f, 0.3f, 0.2f) : new Color(1f, 0.9f, 0.3f), critical ? 1.6f : 1f);
            }
        }
    }
}
