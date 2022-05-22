using System;
using System.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Interaction
{
    /// <summary>
    /// The ResetLostObjectController resets TrainAR objects back to the original spawn point when they are released
    /// too far away of the assembly. This is to prevent objects from falling through the infinityPlane or disapearing
    /// when release while the training assembly has tracking problems and "flaots" away.
    /// </summary>
    [RequireComponent(typeof(TrainARObject))]
    public class ResetLostObjectController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the trainar transform.
        /// </summary>
        /// <value>Gets set in start.</value>
        private Transform trainar;
        /// <summary>
        /// Saves the spawn position.
        /// </summary>
        /// <value>Gets set in start.</value>
        private Vector3 startPosition;
        /// <summary>
        /// Saves the spawn rotation.
        /// </summary>
        /// <value>Gets set in start.</value>
        private Vector3 startRotation;
        /// <summary>
        /// Reference to the spawnedPrefab transform.
        /// </summary>
        /// <value>Gets set in start.</value>
        private Transform spawnedPrefab;
    
        /// <summary>
        /// Sets the spawn position and rotation and adds listener to the OnReleased event. 
        /// </summary>
        private void Start()
        {
            //Store the transform of this object and its local start position/rotation
            trainar = this.gameObject.transform;
            startPosition = trainar.localPosition;
            startRotation = trainar.localRotation.eulerAngles;

            //Store the transform of the aufbau
            spawnedPrefab = GameObject.FindWithTag("Setup").transform;
            
            //Listen to this objects TrainARObject events
            GetComponent<TrainARObject>().OnReleased.AddListener(RestoreObjectIfLost);
        }

        /// <summary>
        /// Resets the object if it was lost either because it was release too far away or is in free fall
        /// </summary>
        private void RestoreObjectIfLost()
        {
            //if (CheckReleaseDistance()) return;
            StartCoroutine(CheckDistanceDelayed());
        }

        /// <summary>
        /// Checks after secondsUntilfreefallDistanceCheck if the object is too far away from the aufbau
        /// </summary>
        /// <returns>nothing</returns>
        private IEnumerator CheckDistanceDelayed()
        {
            for (int i = 0; i < 3; i++)
            {
                //Wait for 1 second
                yield return new WaitForSeconds(1);

                //As soon as something is 1.5 meters or further away from the initial prefab fire this
                if (Vector3.Distance(spawnedPrefab.position, trainar.position) >= 1.5f)
                {
                    //Reset the position and rotation
                    trainar.localPosition = startPosition;
                    trainar.localRotation = Quaternion.Euler(startRotation);
                
                    //Output the result to the console
                    Debug.Log("ResetLostObjectController: Object "
                              + trainar.GetComponent<TrainARObject>().interactableName
                              + " was too far away (Distance: "
                              +Vector3.Distance(spawnedPrefab.position, trainar.position)
                              + "). It was reset to its initial position.");
                    
                    //Break the coroutine if this was executed
                    yield break;
                }   
            }
        }
    }
}
