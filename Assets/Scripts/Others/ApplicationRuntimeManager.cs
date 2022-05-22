using UnityEngine;
using UnityEngine.SceneManagement;

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
        /// Starts a Training with the provided ID
        /// </summary>
        /// <param name="trainingID"></param>
        public void StartTraining(int trainingID)
        {
            switch (trainingID)
            {
                default:
                    Debug.LogError("ApplicationRuntimeManager: Scene with this ID could not be found.");
                    break;
            }
        }
    }
}
