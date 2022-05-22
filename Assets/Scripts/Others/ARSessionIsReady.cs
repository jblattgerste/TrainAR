using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;

namespace Others
{
    /// <summary>
    /// Detects if the ARSession is loaded and deactivates the loading screen.
    /// </summary>
    public class ARSessionIsReady : MonoBehaviour
    {
        /// <summary>
        /// Reference to the loadingScreen.
        /// </summary>
        /// <value>Disabled when ARSession is ready.</value>
        [SerializeField]
        private GameObject loadingScreen;
        
        /// <summary>
        /// Checks if the ARSession is ready to disable the loading screen.
        /// </summary>
        void Update()
        {
            if (ARSession.state == ARSessionState.SessionTracking)
                loadingScreen.SetActive(false);
        }
    }
}
