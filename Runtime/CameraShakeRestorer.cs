using UnityEngine;

namespace Atthree.GameFeel
{
    /// <summary>Sarsıntıyı her karenin en başında geri alır (CameraShaker kendisi ekler).</summary>
    [DefaultExecutionOrder(-32000)]
    [AddComponentMenu("")]
    public class CameraShakeRestorer : MonoBehaviour
    {
        internal CameraShaker shaker;

        void Update()
        {
            if (shaker != null) shaker.Restore();
        }
    }
}
