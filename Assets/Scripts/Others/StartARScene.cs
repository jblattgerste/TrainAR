using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Others
{
    /// <summary>
    /// Starts the ARScene after the tutorial is finished.
    /// </summary>
    public class StartARScene : MonoBehaviour
    {
        /// <summary>
        /// Name of the scene that should be started
        /// </summary>
        /// <value>Default is "Scene"</value>
        [SerializeField] private string sceneName = "Scene";
        
        /// <summary>
        /// Loads the (main) AR scene
        /// </summary>
        public void LoadArScene()
        {
            StartCoroutine(StartARSceneDelayed());
        }
        /// <summary>
        /// Starts the ARScene with 0.1f seconds delay.
        /// </summary>
        /// <returns>None.</returns>
        private IEnumerator StartARSceneDelayed()
        {
            yield return new WaitForSeconds(0.1f);
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    
        /// <summary>
        /// Closes the Application.
        /// </summary>
        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
