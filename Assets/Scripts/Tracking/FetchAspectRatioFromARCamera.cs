using UnityEngine;

namespace Tracking
{
    /// <summary>
    /// Fetches the projectionmatrix from one camera and diretly applies it to another one.
    /// </summary>
    public class FetchAspectRatioFromARCamera : MonoBehaviour
    {
        /// <summary>
        /// Reference to the receiverCamera.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        private Camera receiverCamera;
        /// <summary>
        /// Reference to the arCamera.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        private Camera arCamera;
        /// <summary>
        /// Sets the projectionMatrix of the arCamera on the receiverCamera.
        /// </summary>
        private void Update()
        {
            receiverCamera.projectionMatrix = arCamera.projectionMatrix;
        }
    }
}
