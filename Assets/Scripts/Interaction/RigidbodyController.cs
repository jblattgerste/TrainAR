using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// The RigidbodyController is attached to TrainAR object on conversion. It handles the activation and
    /// deactivation of physics, as we do want TrainAR objects to elicit physics when released but not when
    /// stationary as this could e.g. cause users to "knock over" the assembly with grabbed objects.
    /// </summary>
    public class RigidbodyController : MonoBehaviour
    {
        /// <summary>
        /// Not acted on by physics if true.
        /// </summary>
        /// <value>Default is true</value>
        [Header("Options:")]
        [SerializeField]
        private bool kinematicWhenStatic = true;
        /// <summary>
        /// Reference to rigidbody.
        /// </summary>
        /// <value>Is set on Awake.</value>
        private Rigidbody thisRigidbody;
        /// <summary>
        /// Counter to reset kinematic state.
        /// </summary>
        /// <value>0 at start.</value>
        private int kinematicFrameCounter = 0;

        /// <summary>
        /// Sets needed refernces.
        /// </summary>
        private void Awake()
        {
            thisRigidbody = GetComponent<Rigidbody>();
            thisRigidbody.isKinematic = true;
        }
        /// <summary>
        /// Adds listener to TrainAR events.
        /// </summary>
        void Start()
        {
            GetComponent<TrainARObject>().OnGrabbed.AddListener(DeactivatePhysics);
            GetComponent<TrainARObject>().OnReleased.AddListener(ActivatePhysics);
        }
        
        /// <summary>
        /// Checks for needed changes to kinematic state.
        /// </summary>
        void Update()
        {
            //Make the object kinematic if it is sleeping right now (therefore not acted on by physics) and didnt last frame 
            if (!kinematicWhenStatic) return;

            //Return if this rigidbody is currently kinematic
            if (thisRigidbody.isKinematic == true) return;

            //Use the kinematicFrameCounter to skip the first x frames (1 should be sufficient?!), reset it afterwards
            kinematicFrameCounter++;
            if (kinematicFrameCounter < 2) return;
            kinematicFrameCounter = 0;

            //Check if the object is currently moving, return if it is still moving
            if (thisRigidbody.velocity.sqrMagnitude  > 0.001f) return;

            //Make Object kinematic
            this.thisRigidbody.isKinematic = true;
        }
        /// <summary>
        /// isKinematic is set false.
        /// </summary>
        private void ActivatePhysics()
        {
            thisRigidbody.isKinematic = false;
        }
        /// <summary>
        /// isKinematic is set true.
        /// </summary>
        private void DeactivatePhysics()
        {
            thisRigidbody.isKinematic = true;
        }
        /// <summary>
        /// Sets isKinematic true and disables collisions.
        /// </summary>
        private void DisablePhysics()
        {
            thisRigidbody.isKinematic = true;
            thisRigidbody.detectCollisions = false;
        }
    }
}
