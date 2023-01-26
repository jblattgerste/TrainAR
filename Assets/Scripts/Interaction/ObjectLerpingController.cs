using System;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// When TrainAR objects are grabbed, they lerp towards to correct position attached to the camera instead of appearing there. 
    /// The ObjectLerpingController handles the lerping towards the camera when the object is grabbed.
    /// </summary>
    public class ObjectLerpingController : MonoBehaviour
    {
        /// <summary>
        /// The rotation type of the object lerping.
        /// Default is in relation to plane
        /// </summary>
        private enum RotationType
        {
            freeRotation,
            rotationInRelationToPlane
        }

        /// <summary>
        /// Different options to set the offset of the grabber in relation to the camera.
        /// </summary>
        private enum CameraOffset
        {
            staticOffset,
            groupedSizeOffset,
            dynamicSizeOffset
        }

        /// <summary>
        /// Reference to the grabber object, which becomes the parent of picked up (grabbed) TrainAR-objects.
        /// </summary>
        /// <value>One per scene.</value>
        [Header("Object References: ")]
        [SerializeField]
        [Tooltip("Reference to the grabber object, which becomes the parent of picked up (grabbed) TrainAR-objects.")]
        private GameObject grabber;

        /// <summary>
        /// The lerping speed a grabbed object is moving towards the grabber.
        /// </summary>
        /// <value>Default is 0.1f.</value>
        [Header("Options: ")]
        [Range(0.0f, 0.5f)]
        [SerializeField]
        [Tooltip("The lerping speed a grabbed object is moving towards the grabber.")]
        private float movementSpeed = 0.1f;
        /// <summary>
        /// Defines the offset a grabbed object has towards the camera.
        /// </summary>
        /// <value>Default is groupedSizeOffset.</value>
        [SerializeField]
        [Tooltip("Defines the offset a grabbed object has towards the camera.")]
        private CameraOffset cameraOffsetType = CameraOffset.groupedSizeOffset;
        /// <summary>
        /// Default offset a grabbed object has towards the camera.
        /// </summary>
        /// <value>Default is 0.2f.</value>
        [Range(0.0f, 1f)]
        [SerializeField]
        [Tooltip("Default offset a grabbed object has towards the camera.")]
        private float defaultStaticOffset = 0.2f;

        /// <summary>
        /// Should the Slerping be done freely or only on the Y axis
        /// </summary>
        /// <value>Default is rotationInRelationToPlane.</value>
        [Tooltip("Should the Slerping be done freely or only on the Y axis.")]
        private RotationType rotationType = RotationType.rotationInRelationToPlane;

        //Private Variables
        /// <summary>
        /// Reference holder for the grabbed object.
        /// </summary>
        /// <value>Changed on grab/release.</value>
        [Tooltip("Reference holder for the grabbed object.")]
        private GameObject grabbedObject;
        /// <summary>
        /// Reference holder for the ARCamera.
        /// </summary>
        /// <value>Set on Start.</value>
        [Tooltip("Reference holder for the ARCamera.")]
        private Camera arCamera;

        /// <summary>
        /// Gets the refernce to the main camera and sets a default offset.
        /// </summary>
        private void Start()
        {
            arCamera = Camera.main;
            ChangeOffsetToCamera(defaultStaticOffset);
        }

        /// <summary>
        /// Changes the offset of the grabber.
        /// </summary>
        /// <param name="offset">The offset in meter.</param>
        private void ChangeOffsetToCamera(float offset)
        {
            //Set the grabber to the desired offset location
            grabber.transform.localPosition = new Vector3(0,0,offset);
        }

        /// <summary>
        /// Updates the offset of the grabber.
        /// </summary>
        private void FixedUpdate()
        {
            //Check if an object is selected
            if (grabber.transform.childCount == 0) return;
        
            //Store the selected object
            grabbedObject = grabber.transform.GetChild(0).gameObject;
            TrainARObject interactable = grabbedObject.GetComponent<TrainARObject>();

            switch (cameraOffsetType)
            {
                case CameraOffset.groupedSizeOffset:
                    ChangeOffsetToCamera(interactable.lerpingDistance);
                    break;
                case CameraOffset.dynamicSizeOffset:
                    break;
                case CameraOffset.staticOffset:
                    //nothing. Keep the default static offset
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ClipIntoPlane();

            //Lerp the object into the position of the grabber
            Vector3 smoothLerpedPosition = Vector3.Lerp(grabbedObject.transform.position, grabber.transform.position, movementSpeed);
            grabbedObject.transform.position = smoothLerpedPosition;
            
            //Make the object face the camera
            Quaternion smoothSlerpedRotation = Quaternion.Lerp(grabbedObject.transform.rotation, grabber.transform.rotation, movementSpeed);
            
            //Rotate the object
            if (rotationType == RotationType.rotationInRelationToPlane)
            {
                //Delete the lerped values for X and Z to always have it rotated towards the ground
                Quaternion correctedRotation = Quaternion.Euler(0, smoothSlerpedRotation.eulerAngles.y, 0);
                grabbedObject.transform.rotation = correctedRotation;
            }
            else
            {
                grabbedObject.transform.rotation = smoothSlerpedRotation;
            }
        }
        
        /// <summary>
        /// Prevents that grabbed objects can clip into the infinity plane below the spawned prefab.
        /// </summary>
        private void ClipIntoPlane()
        {
            Ray ray = arCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit[] hits = Physics.RaycastAll(ray, 2.0F);

            //Return if the raycast hit nothing at all
            if (hits.Length == 0) return;

            //Check which index is the infinityplane
            int hitIndex = Array.FindIndex(hits, rHit => rHit.transform.CompareTag("AR_InfinityPlane"));

            //Return if the infinityplane was not hit at all
            if (hitIndex == -1) return;

            //Utilize the correct hit = the Infinity plane
            RaycastHit hit = hits[hitIndex];
            if (hit.distance < 0.4f)
            {
                // Place grabber at impact point of raycast
                grabber.transform.position = hit.point;
            }
        }
    }
}
