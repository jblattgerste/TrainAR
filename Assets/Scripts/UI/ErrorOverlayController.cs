using System.Collections;
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
        /// The Gameobject holding the interaction buttons. They are hidden while the error overlay is open,
        /// because its Understood button takes their place.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Gameobject holding the interaction buttons, hidden while the error overlay is open.")]
        private GameObject interactionButtons;
        /// <summary>
        /// How long the error overlay takes to fade in and out.
        /// </summary>
        /// <value>Default is 0.25 seconds.</value>
        [Tooltip("How long the error overlay takes to fade in and out.")]
        public float fadeDuration = 0.25f;

        private CanvasGroup canvasGroup;
        private Coroutine fade;
        private bool hidInteractionButtons;

        /// <summary>
        /// Adds listener to TriggerErrorOverlay event and sets the canvas group used for fading.
        /// </summary>
        private void Awake()
        {
            StatemachineConnector.Instance.TriggerErrorOverlay += OpenErrorOverlay;
            canvasGroup = errorOverlayUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = errorOverlayUI.AddComponent<CanvasGroup>();
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
            headerText.text = header;
            feedbackText.text = feedback;
            if (!errorOverlayUI.activeSelf)
            {
                canvasGroup.alpha = 0f;
                errorOverlayUI.SetActive(true);
            }
            if (interactionButtons != null && interactionButtons.activeSelf)
            {
                interactionButtons.SetActive(false);
                hidInteractionButtons = true;
            }
            StartFade(1f);
        }

        /// <summary>
        /// Closes the error overlay, called by its Understood button.
        /// </summary>
        public void CloseErrorOverlay()
        {
            if (errorOverlayUI.activeSelf)
                StartFade(0f);
        }

        private void StartFade(float targetAlpha)
        {
            if (fade != null)
                StopCoroutine(fade);
            fade = StartCoroutine(Fade(targetAlpha));
        }

        /// <summary>
        /// Fades the error overlay to the given alpha and deactivates it, restoring the interaction buttons, once it is faded out.
        /// </summary>
        /// <param name="targetAlpha">1 to fade in, 0 to fade out.</param>
        /// <returns></returns>
        private IEnumerator Fade(float targetAlpha)
        {
            // No second tap on Understood while fading out; the overlay still blocks taps on the AR scene.
            canvasGroup.interactable = targetAlpha > 0f;
            float startAlpha = canvasGroup.alpha;
            for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = targetAlpha;
            fade = null;

            if (targetAlpha > 0f) yield break;
            errorOverlayUI.SetActive(false);
            if (hidInteractionButtons)
            {
                interactionButtons.SetActive(true);
                hidInteractionButtons = false;
            }
        }
    }
}
