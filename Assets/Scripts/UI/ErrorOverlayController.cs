using Static;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// The error overlay controller handles the error overlay, triggers it on actions from the statemachine and sets its texts.
    /// It is triggerd through invocation of an action in the StatemachineConnector and therefore has no public methods.
    /// </summary>
    public class ErrorOverlayController : MonoBehaviour
    {
        /// <summary>
        /// The headertext of the error overlay.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField] 
        [Tooltip("The headertext of the error overlay.")]
        private TextMeshProUGUI headerText;
        /// <summary>
        /// The feedback text of the error overlay.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField] 
        [Tooltip("The feedback text of the error overlay.")]
        private TextMeshProUGUI feedbackText;
        /// <summary>
        /// The Gameobject holding the UI for the error overlay.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField] 
        [Tooltip("The Gameobject holding the UI for the error overlay.")]
        private GameObject errorOverlayUI;
        /// <summary>
        /// Adds listener to TriggerErrorOverlay event.
        /// </summary>
        private void Awake()
        {
            StatemachineConnector.Instance.TriggerErrorOverlay += OpenErrorOverlay;
        }
        /// <summary>
        /// Removes listener to TriggerErrorOverlay event.
        /// </summary>
        private void OnDisable()
        {
            StatemachineConnector.Instance.TriggerErrorOverlay -= OpenErrorOverlay;
        }
        
        /// <summary>
        /// Opens the error overlay.
        /// </summary>
        private void OpenErrorOverlay(string header, string feedback)
        {
            errorOverlayUI.SetActive(true);
            headerText.text = header;
            feedbackText.text = feedback;
        }
    }
}
