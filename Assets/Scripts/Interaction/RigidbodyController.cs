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
        /// Physics steps in a row in which the released object did not move.
        /// </summary>
        /// <value>0 at start.</value>
        private int restingSteps = 0;
        /// <summary>
        /// Physics steps in a row an object has to rest before it becomes kinematic (0.2 s at the default 50 Hz).
        /// </summary>
        private const int StepsToRest = 10;

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
        /// Checks for needed changes to kinematic state, once per physics step.
        /// </summary>
        void FixedUpdate()
        {
            //Return if the object is not acted on by physics right now
            if (!kinematicWhenStatic || thisRigidbody.isKinematic)
            {
                restingSteps = 0;
                return;
            }

            //Only a resting object becomes kinematic: a released or bouncing object is also still for a single moment
            //(right after the release, at the top of a bounce), so it has to be still for several physics steps in a
            //row. Counting physics steps instead of frames keeps this independent of the frame rate (e.g. 120 Hz).
            if (thisRigidbody.linearVelocity.sqrMagnitude > 0.001f || thisRigidbody.angularVelocity.sqrMagnitude > 0.01f)
            {
                restingSteps = 0;
                return;
            }
            if (++restingSteps < StepsToRest) return;
            restingSteps = 0;

            //Make Object kinematic
            thisRigidbody.isKinematic = true;
        }
        /// <summary>
        /// isKinematic is set false.
        /// </summary>
        private void ActivatePhysics()
        {
            restingSteps = 0;
            thisRigidbody.isKinematic = false;
            thisRigidbody.WakeUp();
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
