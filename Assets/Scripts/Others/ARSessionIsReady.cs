using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using Remote;
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
        /// Reference to the LoadTrainARScenario.
        /// </summary>
        [SerializeField]
        private LoadTrainARScenario loadScenario;
        /// <summary>
        /// Reference to the PrefabSpawnwingController.
        /// </summary>
        [SerializeField]
        private PrefabSpawningController prefabSpawningController;
        /// <summary>
        /// Checks if the ARSession is ready to disable the loading screen.
        /// </summary>
        void Update()
        {
            if (ARSession.state == ARSessionState.SessionTracking && loadScenario.setupDone)
            {
                loadingScreen.SetActive(false);
                prefabSpawningController.StartPrefabSpawning();
            }
        }
    }
}
