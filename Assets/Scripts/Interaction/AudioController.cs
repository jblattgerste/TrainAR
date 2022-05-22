using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Interaction
{
    /// <summary>
    /// The AudioController is a behaviour automatically attached to a TrainAR Object on conversion. It triggers
    /// its sounds for selection, deselection, grabbing, releasing and correct/incorrect interactions/combinations
    /// specific to this object.
    /// 
    /// When attached, it holds default sounds. Those can be replaced in the Unity Editor Inspector.
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        /// <summary>
        /// The audioclip that is played when a TrainAR Object is selected.
        /// </summary>
        /// <value>Default clip is referenced. If none is referenced it gets ignored.</value>
        [Tooltip("The audioclip that is played when a TrainAR Object is selected")]
        public AudioClip SelectSound;
        /// <summary>
        /// The audioclip that is played when a TrainAR Object is deselected.
        /// </summary>
        /// <value>Default clip is referenced. If none is referenced it gets ignored.</value>
        [Tooltip("The audioclip that is played when a TrainAR Object is deselected")]
        public AudioClip DeselectSound;
        /// <summary>
        /// The audioclip that is played when a TrainAR Object is grabbed.
        /// </summary>
        /// <value>Default clip is referenced. If none is referenced it gets ignored.</value>
        [Tooltip("The audioclip that is played when a TrainAR Object is grabbed")]
        public AudioClip GrabSound;
        /// <summary>
        /// The audioclip that is played when a TrainAR Object is released.
        /// </summary>
        /// <value>Default clip is referenced. If none is referenced it gets ignored.</value>
        [Tooltip("The audioclip that is played when a TrainAR Object is released")]
        public AudioClip ReleaseSound;
        /// <summary>
        /// The audioclip that is played when a TrainAR Object is interacted with.
        /// This only plays when the statemachine accepts the statechange.
        /// </summary>
        /// <value>Default clip is referenced. If none is referenced it gets ignored.</value>
        [Tooltip("The audioclip that is played when a TrainAR Object is interacted with")]
        public AudioClip InteractSound;
        /// <summary>
        /// The audioclip that is played when a TrainAR Object is combined with.
        /// This only plays when the statemachine accepts the statechange.
        /// </summary>
        /// <value>Default clip is referenced. If none is referenced it gets ignored.</value>
        [Tooltip("The audioclip that is played when a TrainAR Object is combined with.")]
        public AudioClip CombineSound;
        /// <summary>
        /// The audioclip that is played when a requested interaction with this object was denied by the statemachine.
        /// </summary>
        /// <value>Default clip is referenced. If none is referenced it gets ignored.</value>
        [Tooltip("The audioclip that is played when a interaction was false.")]
        public AudioClip ErrorSound;
        
        /// <summary>
        /// A static List of AudioClips that were shot and the time they were shot at. This is a workaround
        /// to the problem that AudioSource.PlayClipAtPoint() calls are unmanaged and we dont want to play sounds
        /// for e.g. combinations when they are exactly the same on both objects but we do want to play both sounds
        /// if they differ. probably not the cleanest solution but it works.
        /// </summary>
        private static List<Tuple<AudioClip, float>> audioShotHirstory = new List<Tuple<AudioClip, float>>();

        /// <summary>
        /// The AudioController of a TrainAR object adds listener to trainar events to play the specific audioclips.
        /// </summary>
        private void Start()
        {
            GetComponent<TrainARObject>().OnSelect.AddListener(playSoundSelect);
            GetComponent<TrainARObject>().OnDeselect.AddListener(playSoundDeselect);
            GetComponent<TrainARObject>().OnGrabbed.AddListener(playSoundGrab);
            GetComponent<TrainARObject>().OnReleased.AddListener(playSoundRelease);
            GetComponent<TrainARObject>().error.AddListener(playSoundError);
            GetComponent<TrainARObject>().OnCombination.AddListener(playSoundCombine);
            GetComponent<TrainARObject>().OnInteraction.AddListener(playSoundInteract);
        }

        /// <summary>
        /// Plays the select audioclip, if not null, once.
        /// </summary>
        private void playSoundSelect()
        {
            //Check if this AudioController even wants to play a sound for this action
            if (SelectSound == null) return;
            
            //Check if this exact clip is already playing
            if (audioShotHirstory.Count > 0)
            {
                if (audioShotHirstory[^1].Item1 == SelectSound && (Time.time - audioShotHirstory[^1].Item2 <= 0.1f))
                {
                    audioShotHirstory.Add(new Tuple<AudioClip, float>(SelectSound, Time.time));
                    return;
                }
            }
            //Add the clip to the audioShotHistory
            audioShotHirstory.Add(new Tuple<AudioClip, float>(SelectSound, Time.time));
            
            //Play the Clip
            AudioSource.PlayClipAtPoint(SelectSound, gameObject.transform.position);
        }
        /// <summary>
        /// Plays the deselect audioclip, if not null, once.
        /// </summary>
        private void playSoundDeselect()
        {
            //Check if this AudioController even wants to play a sound for this action
            if (DeselectSound == null) return;
            
            //Check if this exact clip is already playing
            if (audioShotHirstory.Count > 0)
            {
                if (audioShotHirstory[^1].Item1 == DeselectSound && (Time.time - audioShotHirstory[^1].Item2 <= 0.1f))
                {
                    audioShotHirstory.Add(new Tuple<AudioClip, float>(DeselectSound, Time.time));
                    return;
                }
            }
            //Add the clip to the audioShotHistory
            audioShotHirstory.Add(new Tuple<AudioClip, float>(DeselectSound, Time.time));
            
            //Play the Clip
            AudioSource.PlayClipAtPoint(DeselectSound, gameObject.transform.position);
        }
        /// <summary>
        /// Plays the grab audioclip, if not null, once.
        /// </summary>
        private void playSoundGrab()
        {
            //Check if this AudioController even wants to play a sound for this action
            if (GrabSound == null) return;
            
            //Check if this exact clip is already playing
            if (audioShotHirstory.Count > 0)
            {
                if (audioShotHirstory[^1].Item1 == GrabSound && (Time.time - audioShotHirstory[^1].Item2 <= 0.1f))
                {
                    audioShotHirstory.Add(new Tuple<AudioClip, float>(GrabSound, Time.time));
                    return;
                }
            }
            //Add the clip to the audioShotHistory
            audioShotHirstory.Add(new Tuple<AudioClip, float>(GrabSound, Time.time));
            
            //Play the Clip
            AudioSource.PlayClipAtPoint(GrabSound, gameObject.transform.position);
        }
        /// <summary>
        /// Plays the release audioclip, if not null, once.
        /// </summary>
        private void playSoundRelease()
        {
            //Check if this AudioController even wants to play a sound for this action
            if (ReleaseSound == null) return;
            
            //Check if this exact clip is already playing
            if (audioShotHirstory.Count > 0)
            {
                if (audioShotHirstory[^1].Item1 == ReleaseSound && (Time.time - audioShotHirstory[^1].Item2 <= 0.1f))
                {
                    audioShotHirstory.Add(new Tuple<AudioClip, float>(ReleaseSound, Time.time));
                    return;
                }
            }
            //Add the clip to the audioShotHistory
            audioShotHirstory.Add(new Tuple<AudioClip, float>(ReleaseSound, Time.time));
            
            //Play the Clip
            AudioSource.PlayClipAtPoint(ReleaseSound, gameObject.transform.position);
        }
        /// <summary>
        /// Plays the interact audioclip, if not null, once.
        /// </summary>
        private void playSoundInteract()
        {
            //Check if this AudioController even wants to play a sound for this action
            if (InteractSound == null) return;
            
            //Check if this exact clip is already playing
            if (audioShotHirstory.Count > 0)
            {
                if (audioShotHirstory[^1].Item1 == InteractSound && (Time.time - audioShotHirstory[^1].Item2 <= 0.1f))
                {
                    audioShotHirstory.Add(new Tuple<AudioClip, float>(InteractSound, Time.time));
                    return;
                }
            }
            //Add the clip to the audioShotHistory
            audioShotHirstory.Add(new Tuple<AudioClip, float>(InteractSound, Time.time));

            //Play the Clip
            AudioSource.PlayClipAtPoint(InteractSound, gameObject.transform.position);
        }
        /// <summary>
        /// Plays the combine audioclip, if not null, once.
        /// </summary>
        private void playSoundCombine(string combinedWith)
        {
            //Check if this AudioController even wants to play a sound for this action
            if (CombineSound == null) return;
            
            //Check if this exact clip is already playing
            if (audioShotHirstory.Count > 0)
            {
                if (audioShotHirstory[^1].Item1 == CombineSound && (Time.time - audioShotHirstory[^1].Item2 <= 0.1f))
                {
                    audioShotHirstory.Add(new Tuple<AudioClip, float>(CombineSound, Time.time));
                    return;
                }
            }
            //Add the clip to the audioShotHistory
            audioShotHirstory.Add(new Tuple<AudioClip, float>(CombineSound, Time.time));

            //Play the Clip
            AudioSource.PlayClipAtPoint(CombineSound, gameObject.transform.position);
        }
        /// <summary>
        /// Plays the error audioclip, if not null, once.
        /// </summary>
        private void playSoundError()
        {
            //Check if this AudioController even wants to play a sound for this action
            if (ErrorSound == null) return;
            
            //Check if this exact clip is already playing
            if (audioShotHirstory.Count > 0)
            {
                if (audioShotHirstory[^1].Item1 == ErrorSound && (Time.time - audioShotHirstory[^1].Item2 <= 0.1f))
                {
                    audioShotHirstory.Add(new Tuple<AudioClip, float>(ErrorSound, Time.time));
                    return;
                }
            }
            //Add the clip to the audioShotHistory
            audioShotHirstory.Add(new Tuple<AudioClip, float>(ErrorSound, Time.time));
            
            //Play the Clip
            AudioSource.PlayClipAtPoint(ErrorSound, gameObject.transform.position);
        }
    }
}
