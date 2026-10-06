using System.Collections.Generic;
using Others;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// Handles the interactions, i.e. grabbing, releasing, interacting and combining of the TrainARObjects.
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        /// <summary>
        /// The maximum distance at which an object may be picked up in real meter scale.
        /// </summary>
        /// <value>Default is 0.4f.</value>
        [SerializeField]
        [Tooltip("The maximum distance at which an object may be picked up.")]
        private float maxGrabbingDistance = 0.4f;

        /// <summary>
        /// Reference to the PrefabSpawningController.
        /// </summary>
        /// <value>Holds reference to the prefabAnchor gameObject.</value>
        [Header("Script References: ")]
        [SerializeField]
        [Tooltip("Reference to the PrefabSpawningController.")]
        private PrefabSpawningController objectPlacementController;
        /// <summary>
        /// Reference to the camera associated with the AR Device.
        /// </summary>
        /// <value>Default the main camera.</value>
        [Header("Object References: ")]
        [SerializeField]
        [Tooltip("Reference to the camera associated with the AR Device.")]
        private Camera arCamera;
        /// <summary>
        /// Reference to the grabber object, which becomes the parent of a picked up (grabbed) TrainAR-object.
        /// </summary>
        /// <value>One grabber per scene.</value>
        [SerializeField]
        [Tooltip("Reference to the grabber object, which becomes the parent of a picked up (grabbed) TrainAR-object.")]
        public GameObject grabber;
        /// <summary>
        /// The object that is currently grabbed by the user.
        /// </summary>
        /// <value>Changed when TrainAR object is grabbed/released.</value>
        [HideInInspector]
        [Tooltip("The object that is currently grabbed by the user.")]
        public GameObject grabbedObject;
        /// <summary>
        /// Changes value when an object is grabbed/released.
        /// </summary>
        /// <value>True if a object is grabbed.</value>
        [HideInInspector]
        [Tooltip("True if a object is grabbed.")]
        public bool isGrabbingObject;
        /// <summary>
        /// Value is changed when the grab button is pressed.
        /// </summary>
        /// <value>True if a grabbing of an object was not successfull.</value>
        [HideInInspector]
        [Tooltip("True if a grabbing of an object was not successfull.")]
        public bool tryedGrabbingObjectUnsuccessfully;
        /// <summary>
        /// Reference to the current selected(aimed at) TrainAR object.
        /// </summary>
        /// <value>Changed on selection.</value>
        [HideInInspector]
        [Tooltip("Reference to the current selected(aimed at) TrainAR object.")]
        public GameObject selectedObject;
        /// <summary>
        /// Reference to the last selected TrainAR object.
        /// </summary>
        /// <value>Changed on selection.</value>
        [Tooltip("Reference to the last selected TrainAR object.")]
        private GameObject lastSelectedObject;
        /// <summary>
        /// Changed when TrainAR object is selected/deselected.
        /// </summary>
        /// <value>True if a object is selected.</value>
        [HideInInspector]
        [Tooltip("True if a object is selected.")]
        public bool isSelectingObject;
        /// <summary>
        /// Reference to the intersected TrainAR object.
        /// </summary>
        /// <value>Changed when TrainAR object are intersecting.</value>
        [HideInInspector]
        [Tooltip("Reference to the intersected TrainAR object.")]
        public GameObject intersectedObject;
        /// <summary>
        /// Changed on intersection.
        /// </summary>
        /// <value>True if TrainAR objects ar intersecting.</value>
        [HideInInspector]
        [Tooltip("True if two TrainAR objects ar intersecting.")]
        public bool isIntersecting;
        /// <summary>
        /// Raycast from the center of the screen to detected if a TrainAR object is aimed at.
        /// </summary>
        /// <value>Null if no gameObject is hit.</value>
        [Tooltip("Raycast from the center of the screen to detected if a TrainAR object is aimed at.")]
        [System.NonSerialized]
        public RaycastHit hit;

        /// <summary>
        /// Calls functions for selecting or combining depending if a TrainAR object is grabbed.
        /// </summary>
        private void Update()
        {
            //Check if there is an object selected right now
            if (!isGrabbingObject)
            {
                //Select Objects through raycasting
                SelectARInteractable();
            }
            else
            {
                //Combine Objects through overlapping them
                CombineARInteractables();
            }
        }
        /// <summary>
        /// Checks for detected intersections and updates the affected GameObjects
        /// </summary>
        private void CombineARInteractables()
        {
            //Deactivate all the box colliders of the CollisionControllers belonging to grabbed object and it's children
            DeactivateAllBoxColliderTrigger(grabbedObject);

            //Update isIntersectingObject
            isIntersecting = grabbedObject.GetComponent<TrainARObject>().Intersection
                .GetIntersectionDetected();

            if (isIntersecting)
            {
                //Get the intersected object
                intersectedObject = grabbedObject.GetComponent<TrainARObject>().Intersection.GetIntersectedObject();

                grabbedObject.GetComponent<MaterialController>().ChangeToCombineMaterial();

                //Select intersected object
                intersectedObject.GetComponent<TrainARObject>().Select();

                //Deselect the grabbed object
                grabbedObject.GetComponent<TrainARObject>().Deselect();
            }
            if (!isIntersecting)
            {
                grabbedObject.GetComponent<TrainARObject>().Select();
            }
        }

        /// <summary>
        /// Checks on each frame if an ARInteractable is selectable and selects/deselects it.
        /// </summary>
        private void SelectARInteractable()
        {

            if (!objectPlacementController.objectWasSpawned) return;

            //Check if the ray hits anything at all, return when no hit
            Ray ray = arCamera.ViewportPointToRay(new Vector3(0.5f,0.5f,0f));
            if (!RaycastVisible(ray, out hit))
            {
                //Reset the Selection state
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<TrainARObject>().Deselect();
                    selectedObject = null;
                }
                isSelectingObject = false;
                return;
            }

            //Check if the hit object hit is an TrainARObject or the distance is greater than maxGrabbingDistance
            if (!hit.transform.CompareTag("TrainARObject") || hit.distance >= maxGrabbingDistance)
            {
                //Reset the Selection state
                if (selectedObject != null)
                {
                    selectedObject.GetComponent<TrainARObject>().Deselect();
                    selectedObject = null;
                }
                isSelectingObject = false;
                return;
            }
            
            //Make the selected object to the lastSelected one
            lastSelectedObject = selectedObject != null ? selectedObject : null;

            //Finds the TrainARObject that is the parent (distance unknown) of the TrainARObjectCollider that was hit
            //selectedObject = FindARInteractable(hit.transform.gameObject);
            selectedObject = hit.transform.gameObject;
            
            if (lastSelectedObject == null)
            {
                
                //If an object is hit, its state is set to selected
                selectedObject.GetComponent<TrainARObject>().Select();
                
                //An TrainARObjectCollider is selected
                isSelectingObject = true;
            }
            //Return from this function if the selected object is the same as last frame
            //Deselect the last selected object if there was one
            else
            {
                if(selectedObject == lastSelectedObject) return;
                //If an object is hit, its state is set to selected
                selectedObject.GetComponent<TrainARObject>().Select();
                lastSelectedObject.GetComponent<TrainARObject>().Deselect();
            }
        }

        /// <summary>
        /// Raycast used for selecting TrainAR objects. Convex colliders can enclose other TrainAR objects (e.g. a pot
        /// standing inside a coffee machine), so when the ray hits more than one TrainAR object, the one whose rendered
        /// mesh is hit first is selected instead of the one with the nearest collider. Otherwise, or if none of their
        /// meshes is hit, this is a regular raycast. Other colliders in front still block as before.
        /// </summary>
        private bool RaycastVisible(Ray ray, out RaycastHit result)
        {
            var count = Physics.RaycastNonAlloc(ray, rayHits);
            result = default;

            //Find the nearest hit (the regular raycast result) and whether more than one TrainAR object is on the ray.
            //Each TrainAR object has several colliders, so this compares objects rather than hits.
            Transform firstObject = null;
            var ambiguous = false;
            for (var i = 0; i < count; i++)
            {
                if (i == 0 || rayHits[i].distance < result.distance) result = rayHits[i];
                var hitObject = rayHits[i].transform;
                if (!hitObject.CompareTag("TrainARObject")) continue;
                if (firstObject == null) firstObject = hitObject;
                else if (hitObject != firstObject) ambiguous = true;
            }
            if (!ambiguous) return count > 0;

            var visible = result;
            var meshHit = false;
            float best = float.MaxValue, bestCollider = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var hit = rayHits[i];
                var distance = hit.distance;
                if (hit.transform.CompareTag("TrainARObject"))
                {
                    //Test each object's mesh only once, even if several of its colliders were hit
                    var j = 0;
                    while (rayHits[j].transform != hit.transform) j++;
                    distance = rayHitDistances[i] = j < i ? rayHitDistances[j] : MeshHitDistance(ray, hit);
                    meshHit |= distance < float.MaxValue;
                }
                //On equal distances (several colliders of one object) the nearest collider wins
                if (distance > best || (distance == best && hit.distance >= bestCollider)) continue;
                best = distance;
                bestCollider = hit.distance;
                visible = hit;
            }
            if (meshHit) result = visible;
            return true;
        }

        private readonly RaycastHit[] rayHits = new RaycastHit[32];
        private readonly float[] rayHitDistances = new float[32];
        private readonly Dictionary<Mesh, (Vector3[] v, int[] t)> meshCache = new Dictionary<Mesh, (Vector3[] v, int[] t)>();

        /// <summary>
        /// Distance along the ray to the nearest triangle of the hit object's rendered mesh (Möller–Trumbore test), or
        /// float.MaxValue if the ray misses it. Falls back to the collider distance if the mesh can't be read.
        /// </summary>
        private float MeshHitDistance(Ray ray, RaycastHit hit)
        {
            var target = hit.transform;
            if (!target.TryGetComponent(out MeshFilter meshFilter) || meshFilter.sharedMesh == null || !meshFilter.sharedMesh.isReadable)
                return hit.distance;
            var mesh = meshFilter.sharedMesh;
            if (!meshCache.TryGetValue(mesh, out var m)) meshCache[mesh] = m = (mesh.vertices, mesh.triangles);
            //The ray is transformed without normalising, so the hit parameter stays in world units
            Vector3 o = target.worldToLocalMatrix.MultiplyPoint3x4(ray.origin), d = target.worldToLocalMatrix.MultiplyVector(ray.direction);
            var nearest = float.MaxValue;
            for (var i = 0; i < m.t.Length; i += 3)
            {
                Vector3 a = m.v[m.t[i]], e1 = m.v[m.t[i + 1]] - a, e2 = m.v[m.t[i + 2]] - a, p = Vector3.Cross(d, e2), s = o - a;
                float inv = 1f / Vector3.Dot(e1, p), u = Vector3.Dot(s, p) * inv;
                if (u < 0 || u > 1) continue;
                var q = Vector3.Cross(s, e1);
                float w = Vector3.Dot(d, q) * inv, t = Vector3.Dot(e2, q) * inv;
                if (w >= 0 && u + w <= 1 && t > 0 && t < nearest) nearest = t;
            }
            return nearest;
        }

        /// <summary>
        /// Grab the object that is currently selected by the raycast.
        /// </summary>
        public void GrabObject()
        {
            //Important: This method assumes there is an object selected!

            //Invokes the Grab events on the grabbed object and returns if that failed
            if (tryedGrabbingObjectUnsuccessfully) return;
            if (hit.distance >= maxGrabbingDistance) return;
            if (!selectedObject.GetComponent<TrainARObject>().Grab())
            {
                tryedGrabbingObjectUnsuccessfully = true;
                return;
            }

            //Store the currently grabbed object
            grabbedObject = selectedObject;

            //Set the parent to the camera (so it follows the cameras movement)
            grabbedObject.transform.parent = grabber.transform;
            grabbedObject.transform.localPosition = grabbedObject.transform.GetComponent<TrainARObject>().pivotOffsetPosition;
            grabbedObject.transform.localRotation = Quaternion.Euler(grabbedObject.transform.GetComponent<TrainARObject>().pivotOffsetRotation);

            //Is grabbing an object
            isGrabbingObject = true;

            grabbedObject.GetComponent<TrainARObject>().Deselect();

             if (intersectedObject != null)
             {
                 intersectedObject.GetComponent<TrainARObject>().Deselect();
             }
        }

        /// <summary>
        /// Release the currently grabbed object.
        /// </summary>
        public void ReleaseGrabbedObject(bool fusedObject=false)
        {
            //Return if no object is currently grabbed
            if (!isGrabbingObject) return;

            //Deactivate the trigger for the BoxColliders
            ActivateAllBoxColliderTrigger(grabbedObject);
            
            //In case this is called in the context of fusing objects via the Object Helper, these steps have be skipped
            if (!fusedObject)
            {
                //Set the parent back to be a child of the anchored object that was originally spawned
                grabbedObject.transform.parent = objectPlacementController.GetSpawnedObject().transform;

                //Invokes the Release events on the grabbed object
                grabbedObject.GetComponent<TrainARObject>().Release();
            }


            //Is no longer grabbing an object
            isGrabbingObject = false;
            isIntersecting = false;

            if (intersectedObject != null)
            {
                intersectedObject.GetComponent<TrainARObject>().Deselect();
            }

            //Reset the rotation of the grabber (which might be modified by the GrabbedRotationController)
            grabber.transform.localRotation = Quaternion.identity;
        }
        
        /// <summary>
        /// Invokes the Interact on the TrainARObject.
        /// Important: This method assumes there is an object selected!
        /// It also assumes that ARInteractables have an ObjectInteraktionhandler.
        /// </summary>
        public void Interact()
        {
            selectedObject.GetComponent<TrainARObject>().Interact();
        }
        /// <summary>
        /// Invokes the Combine on the TrainARObject.
        /// Important: This method assumes there is an object selected and it is currently intersecting with another.
        /// It also assumes that ARInteractables have an ObjectInteraktionhandler.
        /// </summary>
        public void Combine()
        {
            selectedObject.GetComponent<TrainARObject>().Combine(intersectedObject.GetComponent<TrainARObject>().interactableName, intersectedObject);
        }

        /// <summary>
        /// Activates the Box-Collider-Trigger of each of the collision controllers.
        /// </summary>
        private static void ActivateAllBoxColliderTrigger(GameObject objectToActivate)
        {
            var collisionControllers = objectToActivate.GetComponents<CollisionController>();
            foreach (var collisionController in collisionControllers)
            {
                collisionController.boxCollider.isTrigger = true;
            }
        }

        /// <summary>
        /// Deactivates the Box-Collider-Trigger of each of the collision controllers.
        /// </summary>
        private static void DeactivateAllBoxColliderTrigger(GameObject objectToDeactivate)
        {
            var collisionControllers = objectToDeactivate.GetComponents<CollisionController>();
            foreach (CollisionController collisionController in collisionControllers)
            {
                collisionController.boxCollider.isTrigger = false;
            }
        }
        
    }
}
