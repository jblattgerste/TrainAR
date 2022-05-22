using System;
using System.Collections.Generic;
using Static;
using TMPro;
using UnityEngine;

namespace Others
{
    /// <summary>
    /// Handles the completion overlay that is shown at the end of the training.
    /// </summary>
    public class ScenarioCompleted : MonoBehaviour
    {
        /// <summary>
        /// Reference to the feedback text UI.
        /// </summary>
        /// <value>Once per scene.</value>
        [Header("Text References: ")]
        [SerializeField]
        [Tooltip("Reference to the feedback text UI.")]
        private TextMeshProUGUI feedbackText;
        /// <summary>
        /// Reference to the interaction buttons.
        /// </summary>
        /// <value>Once per scene.</value>
        [Header("Object References: ")]
        [SerializeField]
        [Tooltip("Reference to the interaction buttons.")]
        private GameObject interactionButtons;
        /// <summary>
        /// Reference to the overlay-UI.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        [Tooltip("Reference to the overlay-UI.")]
        private GameObject completionOverlay;
        /// <summary>
        /// Reference to the crosshair UI.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        [Tooltip("Reference to the crosshair UI.")]
        private GameObject crosshair;
        /// <summary>
        /// Reference to the Gameobject holding the assistance instruction panel.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        [Tooltip("Reference to the Gameobject holding the assistance instruction panel.")]
        private GameObject topPanel;
        /// <summary>
        /// Reference to the Rect transform of the error indicator in the scenario conclusion overlay.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        [Tooltip("Reference to the Rect transform of the error indicator in the scenario conclusion overlay")]
        private RectTransform errorIndicator;
    
        /// <summary>
        /// Stores the real time to calculate the needed time for the scenario.
        /// </summary>
        /// <value>Set on runtime to the real time.</value>
        private DateTime startTime;
        /// <summary>
        /// Constant to position the bar on the endscreen.
        /// </summary>
        /// <value>Fix to -80.</value>
        private const int OffsetPerError = -80;
        /// <summary>
        /// Sets the startTime.
        /// </summary>
        private void OnEnable()
        {
            startTime = DateTime.Now;
        }
        /// <summary>
        /// Adds listener to completionOverlay event.
        /// </summary>
        private void Awake()
        {
            StatemachineConnector.Instance.TriggerScenarioCompletionOverlay += ShowCompletionOverlay;
        }
        /// <summary>
        /// Removes listener to completionOverlay event.
        /// </summary>
        private void OnDisable()
        {
            StatemachineConnector.Instance.TriggerScenarioCompletionOverlay -= ShowCompletionOverlay;
        }
        /// <summary>
        /// Handles activation/deactivation of UI elements then the completion overlay is shown.
        /// </summary>
        private void ShowCompletionOverlay()
        {
            //Position the error indicator.
            PositionErrorIndicator();
            
            //Activate completion overlay
            completionOverlay.SetActive(true);
            
            //Deactivate interaction buttons, crosshair and topPanel
            interactionButtons.SetActive(false);
            crosshair.SetActive(false);
            topPanel.SetActive(false);
            
            //Set the Completion overlay text
            SetFeedbackText();
        }
        /// <summary>
        /// Calculates the needed time for the scenario.
        /// </summary>
        /// <returns>Needed time in format "x minutes y seconds"</returns>
        private string GetTimeString()
        {
            var endTime = DateTime.Now - startTime;
            var timeString = "";
            switch (endTime.Minutes)
            {
                case 1:
                    timeString += "<b>" + endTime.Minutes + " minute </b>";
                    break;
                case > 1:
                    timeString += "<b>" + endTime.Minutes + " minutes </b>";
                    break;
            }
            if (endTime.Seconds == 1)
            {
                timeString += "<b>" + endTime.Seconds + " second</b>";
            } else {
                timeString += "<b>" + endTime.Seconds +  " seconds</b>";
            }
            return timeString;
        }
        /// <summary>
        /// Sets the feedback text in the endscreen UI with error count and needed time.
        /// </summary>
        private void SetFeedbackText()
        {
            feedbackText.text = "Congratulations, you have completed the training! You did "
                                + StatemachineConnector.Instance.errorCounter
                                + " mistake(s). Time required for the training: "
                                + GetTimeString();
            Debug.Log("Training Summary: " + feedbackText);
        }

        /// <summary>
        /// Positions the error indicator in the conclusion graph, depending on how many errors have been made.
        /// </summary>
        private void PositionErrorIndicator()
        {
            int errorOffset;
            
            // 15 is the maximum amount of errors indicated on the graph.
            if (StatemachineConnector.Instance.errorCounter > 15)
            {
                errorOffset = 15 * OffsetPerError;
            }
            else
            {
                errorOffset = StatemachineConnector.Instance.errorCounter * OffsetPerError;
            }
            
            errorIndicator.anchoredPosition = new Vector2(errorOffset, errorIndicator.anchoredPosition.y);
        }
    }
}
