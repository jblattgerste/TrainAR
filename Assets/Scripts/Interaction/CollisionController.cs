using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// Detects and handles collisions of TrainAR Objects. This is e.g. used for combining GameObjects and
    /// visualizing interseted states through outlining and shading.
    /// </summary>
    public class CollisionController : MonoBehaviour
    {
        /// <summary>
        /// Holds a reference to the BoxCollider of the attached Gameobject.
        /// </summary>
        /// <value>Boundingbox with dimensions of the mesh.</value>
        [Tooltip("Holds the Boxcollider of the attached Gameobject.")]
        [HideInInspector]
        public BoxCollider boxCollider;
        
        /// <summary>
        /// Holds a reference to the currently grabbed TrainAR object.
        /// </summary>
        /// <value>Is changed depending on the grabbed object.</value>
        [Tooltip("Holds a reference to the current grabbed TrainAR object.")]
        [HideInInspector]
        public GameObject grabbedObject;

        /// <summary>
        /// Sets script references to GameObjects on Awake
        /// </summary>
        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }

        /// <summary>
        /// Each detected collision if it is a possible combination of ARInteractables.
        /// </summary>
        /// <param name="other">Provided by Unity</param>
        private void OnTriggerStay(Collider other)
        {
            //If the other collider is not an Boxcollider or if there already is an intersection, do nothing.
            if (!other.gameObject.CompareTag("TrainARObject")) return;
            if (!(other is BoxCollider) || other.gameObject.GetComponent<TrainARObject>().Intersection.GetIntersectionDetected()) return;
            //Is the other object the grabbed object?
            if (!other.gameObject.GetComponent<TrainARObject>().isGrabbed) return;
            grabbedObject = other.gameObject;
            //Is this object a child of the grabbed object?
            if (gameObject.transform.IsChildOf(grabbedObject.transform)) return;
            //Set intersectedObject and isIntersecting.
            other.gameObject.GetComponent<TrainARObject>().Intersection.SetIntersectedObject(this.gameObject);
            other.gameObject.GetComponent<TrainARObject>().Intersection.SetIntersectionDetected(true);
        }
        /// <summary>
        /// Resets combination state of the GameObject.
        /// </summary>
        /// <param name="other">Provided by Unity</param>
        private void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.CompareTag("TrainARObject")) return;
            if (!(other is BoxCollider) || !other.gameObject.GetComponent<TrainARObject>().Intersection.GetIntersectionDetected()) return;
            this.gameObject.GetComponent<TrainARObject>().Deselect();
            other.gameObject.GetComponent<TrainARObject>().Deselect();
            this.gameObject.GetComponent<MaterialController>().resetOriginalMaterial();
            other.gameObject.GetComponent<MaterialController>().resetOriginalMaterial();
            other.gameObject.GetComponent<TrainARObject>().Intersection.SetIntersectedObject(null);
            other.gameObject.GetComponent<TrainARObject>().Intersection.SetIntersectionDetected(false);
        }

        /// <summary>
        /// The struct that holds references of intersections.
        /// </summary>
        public struct Intersection
        {
            /// <summary>
            /// Reference to the intersected object.
            /// </summary>
            /// <value>Is null when no object is intersected.</value>
            private GameObject intersectedObject;
            
            /// <summary>
            /// Changes value depending if an intersection is detected.
            /// </summary>
            /// <value>True if intersection detected.</value>
            private bool intersectionDetected;

            /// <summary>
            /// Constructer to create a struct to store references for a detected intersection.
            /// </summary>
            /// <param name="intersectedObject">The intersected TrainAR object.</param>
            /// <param name="intersectionDetected">If a intersection is detected</param>
            public Intersection(GameObject intersectedObject, bool intersectionDetected)
            {
                this.intersectedObject = intersectedObject;
                this.intersectionDetected = intersectionDetected;
            }
            /// <summary>
            /// Set the intersected TrainAR object reference in the struct.
            /// </summary>
            /// <param name="intersectedObject">The intersected Object.</param>
            public void SetIntersectedObject(GameObject intersectedObject)
            {
                this.intersectedObject = intersectedObject;
            }
            /// <summary>
            /// Set if a intersection is detected.
            /// </summary>
            /// <param name="intersectionDetected">True if intersection is detected.</param>
            public void SetIntersectionDetected(bool intersectionDetected)
            {
                this.intersectionDetected = intersectionDetected;
            }
            /// <summary>
            /// Get the intersected TrainAR object reference in the struct.
            /// </summary>
            /// <returns>The TrainAR gameobject.</returns>
            public GameObject GetIntersectedObject()
            {
                return intersectedObject;
            }
            /// <summary>
            /// Get if a intersection is detected.
            /// </summary>
            /// <returns>If a intersection is detected.</returns>
            public bool GetIntersectionDetected()
            {
                return intersectionDetected;
            }
        }
    }
}
