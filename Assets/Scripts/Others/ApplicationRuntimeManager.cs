using UnityEngine;
using UnityEngine.SceneManagement;
using Static;

namespace Others
{
    /// <summary>
    /// The ApplicationRuntimeManager handles the TrainAR menu lifecycle. It start and quits Trainings and Application.
    /// </summary>
    public class ApplicationRuntimeManager : MonoBehaviour
    {
        /// <summary>
        /// Quits the whole Application
        ///
        /// When in Editor, just stops the preview
        /// </summary>
        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        /// <summary>
        /// Load tutorial scence of given scenario.
        /// </summary>
        /// <param name="scenarioName"></param>
        public void SwitchScene(string scenarioName)
        {
            ActiveScenarioInformation.scenarioName = scenarioName;
            SceneManager.LoadScene("Scenes/Tutorial", LoadSceneMode.Single);
        }
    }
}
