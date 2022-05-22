using System.Collections;
using Static;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// The DirectInfoBoxController handles the displaying of direct info boxes that are used to display insishgts to the user.
    /// It is triggerd through invocation of an action in the StatemachineConnector and therefore has no public methods.
    /// </summary>
    public class DirectInfoController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the audioplayer supposed to play the info-clips.
        /// </summary>
        /// <value>Set 8in inspector.</value>
        [SerializeField]
        [Tooltip("Reference to the audioplayer supposed to play the info-clips.")]
        private AudioSource audioPlayer;
        /// <summary>
        /// Reference to the animator of the direct info panel.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("Reference to the animator of the direct info panel.")]
        private Animator animator;
        /// <summary>
        /// Reference to the direct info text.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("Reference to the direct info text.")]
        private TextMeshProUGUI directInfotext;
        /// <summary>
        /// Reference to the direct info image.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("Reference to the direct info image.")]
        private Image directInfoPicture;
        /// <summary>
        /// Reference to a default directInfoPicture.
        /// </summary>
        /// <value>Set on runtime.</value>
        private Sprite defaultDirectInfoPicture;
        /// <summary>
        /// Reference to the direct info sound symbol.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("Reference to the direct info sound symbol.")]
        private Image directInfoSoundSymbol;
        /// <summary>
        /// Sets references and adds listener to TriggerExpertInsights event.
        /// </summary>
        private void Awake()
        {
            //Store the default sprite for the direct info box
            defaultDirectInfoPicture = directInfoPicture.sprite;
            
            StatemachineConnector.Instance.TriggerExpertInsights += OpenInfobox;
        }
        /// <summary>
        /// Removes listener to TriggerExpertInsights event.
        /// </summary>
        private void OnDisable()
        {
            StatemachineConnector.Instance.TriggerExpertInsights -= OpenInfobox;
        }

        /// <summary>
        /// Opens and displays the infobox-text and plays the corresponding soundfile. Infobox closes by itself
        /// after the clip is done playing.
        /// </summary>
        /// <param name="audioClip">The audio clip to play for this expert insight</param>
        /// <param name="infoBoxImage">The image displayed next to the info box</param>
        /// <param name="insightText">The text displayed on this expert insight</param>
        private void OpenInfobox(AudioClip audioClip, Sprite infoBoxImage, string insightText)
        {
            //Play the audioclip if one is attached and display the sound symbol
            if (audioClip != null)
            {
                audioPlayer.clip = audioClip;
                audioPlayer.Play();
                directInfoSoundSymbol.enabled = true;
            }
            else
            {
                directInfoSoundSymbol.enabled = false;
            }

            if (infoBoxImage != null)
            {
                directInfoPicture.sprite = infoBoxImage;
            }
            else
            {
                directInfoPicture.sprite = defaultDirectInfoPicture;
            }

            //Slide out the insight UI element and set the text
            animator.SetBool("open", true);
            directInfotext.text = insightText;
            
            //Show the audioclip for the duration of the clip if one is attached or by default 5 seconds
            StartCoroutine(CloseAfterSeconds(audioClip != null? audioClip.length : 5));
        }
    
        /// <summary>
        /// Close the insights overlay delayed
        /// </summary>
        /// <param name="seconds">how long the overlay is displayed</param>
        /// <returns></returns>
        private IEnumerator CloseAfterSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            //Play the closing animation
            animator.SetBool("open", false);
        }
    }
}
