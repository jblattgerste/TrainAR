using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI
{
    /// <summary>
    /// StreamVideo is a wrapper for the Videoplayer class and handles playing of the onboarding animations.
    /// </summary>
    public class StreamVideo : MonoBehaviour {
        /// <summary>
        /// Reference to the rawImage.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        private RawImage rawImage;
        /// <summary>
        /// Reference to the videoPlayer.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        private VideoPlayer videoPlayer;
        /// <summary>
        /// Should the video start when the gameObject is active.
        /// </summary>
        /// <value>Default is false.</value>
        [SerializeField]
        private bool autoStartVideo = false;
        /// <summary>
        /// Starts the video when the object is enabled.
        /// </summary>
        private void Start()
        {
            if(autoStartVideo) StartVideo();
        }

        /// <summary>
        /// Starts playing the specified video.
        /// </summary>
        public void StartVideo()
        {
            rawImage.enabled = false;
            StartCoroutine(PlayVideo());
        }
        /// <summary>
        /// Play the video in a coroutine.
        /// </summary>
        /// <returns>None</returns>
        private IEnumerator PlayVideo()
        {
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }
            rawImage.texture = videoPlayer.texture;
            videoPlayer.Play();
            rawImage.enabled = true;
        }
    
        //TODO: OnDisable braucht wahrscheinlich einen videoplayer.stop oder stopped das automatisch wenn das Objekt deaktiviert wird?
    }
}
