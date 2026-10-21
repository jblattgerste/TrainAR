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
        /// Name of the scene that is started when no training was chosen (normally the chosen training's scene is
        /// started, see TrainingSelection).
        /// </summary>
        /// <value>Default is "Coffee"</value>
        [SerializeField] private string sceneName = "Coffee";
        
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
            var training = TrainingSelection.Current;
            SceneManager.LoadScene(training != null ? training.sceneName : sceneName, LoadSceneMode.Single);
        }
    
        /// <summary>
        /// Goes back to the home screen with all trainings.
        /// </summary>
        public void LoadHome()
        {
            SceneManager.LoadScene("Scenes/Home", LoadSceneMode.Single);
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
