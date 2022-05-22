using System.Collections;
using System.Collections.Generic;
using Static;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

namespace Others
{
    /// <summary>
    /// Adds basic functionality like leave and reset application and scenarios.
    /// </summary>
    public class LeaveAndResetARScene : MonoBehaviour
    {
        /// <summary>
        /// Reference to the AR-Session-Object of this scene.
        /// </summary>
        /// <value>One per scene.</value>
        [SerializeField]
        [Tooltip("Reference to the AR-Session-Object of this scene.")]
        private ARSession arSessionObject;
        /// <summary>
        /// Reference to the loading screen.
        /// </summary>
        /// <value>One per scene.</value>
        [SerializeField]
        [Tooltip("Reference to the loading screen.")]
        private GameObject loadingScreen;

        /// <summary>
        /// Resets the static variables and returns to the main menu scene.
        /// </summary>
        public void LeaveAndReset()
        {
            Debug.Log("ApplicationController: The user is leaving the scenario.");
            loadingScreen.SetActive(true);
            ResetStaticClassesAndVariables();
            arSessionObject.Reset();
            StartCoroutine(StartMenuSceneDelayed());
        }
        /// <summary>
        /// Switches back to the menu scene after a small delay
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartMenuSceneDelayed()
        {
            yield return new WaitForSeconds(0.1f);
            SceneManager.LoadScene("Scenes/Tutorial", LoadSceneMode.Single);
        }
        /// <summary>
        /// Resets the static classes and variables defined in this function
        /// </summary>
        private void ResetStaticClassesAndVariables()
        {
            StatemachineConnector.Instance.Reset();
        }
        
        /// <summary>
        /// Closes the application.
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
