using System;
using System.Collections;
using Others;
using Static;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// The TopPanelController handpes updates to the UI panel of the trainign scenario, showing the instructions and progress percentage of the training.
    /// </summary>
    public class TopPanelController : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI instruction;

        [SerializeField]
        private TextMeshProUGUI progressPercentage;

        [SerializeField]
        private Image progressBar;

        [SerializeField]
        private PrefabSpawningController prefabSpawningController;

        [Header("Options: ")]
        /// <summary>
        /// The color of the UI animation played on wrong actions.
        /// </summary>
        public Color errorColor = Color.red;
        /// <summary>
        /// The color of the UI animation played on correct actions.
        /// </summary>
        public Color successColor = Color.green;

        private bool successAnimationPlaying;
        private bool errorAnimationPlaying;
        private readonly Color initialColor = new Color(0.1215686f, 0.1215686f, 0.1215686f, 0.5882353f);


        [SerializeField]
        private Image successIcon;
        [SerializeField]
        private Image errorIcon;
        [SerializeField]
        private GameObject progressIcon;

        private void Awake()
        {
            StartCoroutine(EnableInstructionTextOnPrefabSpawned());
            StatemachineConnector.Instance.TriggerTopPanelChange += OnTriggerTopPanelChange;
            StatemachineConnector.Instance.TriggerAcceptedStateChange += OnTriggerAcceptedStateChange;
        }
        
        private void OnDisable()
        {
            StatemachineConnector.Instance.TriggerTopPanelChange -= OnTriggerTopPanelChange;
            StatemachineConnector.Instance.TriggerAcceptedStateChange -= OnTriggerAcceptedStateChange;
        }
        
        private void OnTriggerTopPanelChange(string newInstructionText, int newCompletionPercentage)
        {
            UpdateInstructionText(newInstructionText);
            UpdatePercentage(newCompletionPercentage);
        }
        
        private void OnTriggerAcceptedStateChange(bool acceptedStateChange)
        {
            if (acceptedStateChange) StartCoroutine(PlaySuccessAnimation());
            else StartCoroutine(PlayErrorAnimation());
        }


        private IEnumerator PlaySuccessAnimation()
        {
            //indicates animation is playing.
            successAnimationPlaying = true;

            //disable the progress icon while enabling the success icon.
            //disable also error icon, in case this method was called while PlayErrorAnimation was playing.
            progressIcon.SetActive(false);
            successIcon.enabled = true;
            errorIcon.enabled = false;

            //----Start the animation
            successIcon.color = successColor;
            yield return new WaitForSeconds(0.3f);
            successIcon.color = initialColor;
            yield return new WaitForSeconds(0.3f);
            successIcon.color = successColor;
            yield return new WaitForSeconds(0.3f);
            successIcon.color = initialColor;
            yield return new WaitForSeconds(0.3f);
            successIcon.color = successColor;
            yield return new WaitForSeconds(0.3f);
            successIcon.color = initialColor;
            yield return new WaitForSeconds(0.3f);
            //----End the animation

            //Animation done, so disable icon.
            successIcon.enabled = false;

            //In case the error animation was called while this one was playing, do not activate the progress Icon.
            if (!errorAnimationPlaying)
            {
                progressIcon.SetActive(true);
            }

            //animation done playing
            successAnimationPlaying = false;
        }

        /// <summary>
        /// Starts the error animation.
        /// </summary>
        private IEnumerator PlayErrorAnimation()
        {
            //indicates animation is playing.
            errorAnimationPlaying = true;

            //disable the progress icon while enabling the success icon.
            //also, disable success icon, in case this method was called while PlaySuccessAnimation was playing.
            progressIcon.SetActive(false);
            errorIcon.enabled = true;
            successIcon.enabled = false;

            //----Start the animation
            errorIcon.color = errorColor;
            yield return new WaitForSeconds(0.3f);
            errorIcon.color = initialColor;
            yield return new WaitForSeconds(0.3f);
            errorIcon.color = errorColor;
            yield return new WaitForSeconds(0.3f);
            errorIcon.color = initialColor;
            yield return new WaitForSeconds(0.3f);
            errorIcon.color = errorColor;
            yield return new WaitForSeconds(0.3f);
            errorIcon.color = initialColor;
            yield return new WaitForSeconds(0.3f);
            errorIcon.color = errorColor;
            yield return new WaitForSeconds(0.3f);
            //----End the animation

            //Animation done, so disable icon.
            errorIcon.enabled = false;

            //In case the success animation was called while this one was playing, do not activate the progress Icon.
            if (!successAnimationPlaying)
            {
                progressIcon.SetActive(true);
            }

            //animation done playing.
            errorAnimationPlaying = false;
        }

        /// <summary>
        /// Disables and then enables the instruction text on the top panel after the prefab was spawned
        /// </summary>
        /// <returns>nothing</returns>
        private IEnumerator EnableInstructionTextOnPrefabSpawned()
        {
            instruction.enabled = false;
            
            //Wait until the prefab was spawned
            while (!prefabSpawningController.objectWasSpawned) {
                yield return null;
            }
            instruction.enabled = true;
        }
    
        /// <summary>
        /// Updates the Progressbar of the Top Panel.
        /// </summary>
        /// <param name="newPercentage">The new percentage Value.</param>
        private void UpdatePercentage(int newPercentage)
        {
            progressPercentage.text = newPercentage + " %";
            progressBar.fillAmount = (float)newPercentage/100;
        }
    
        /// <summary>
        /// Updates the instruction text at the top of the assistance menu
        /// </summary>
        /// <param name="text">The text to put into the assistance menu</param>
        private void UpdateInstructionText(string text)
        {
            instruction.text = text;
        }
    }
}
