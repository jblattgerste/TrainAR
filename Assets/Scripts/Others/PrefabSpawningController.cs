using System.Collections.Generic;
using System.Linq;
using Interaction;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Visual_Scripting;
using Action = System.Action;

namespace Others
{
    /// <summary>
    /// Handles the initial spawning and positioning as well as the repositioning of the training assembly. 
    /// </summary>
    public class PrefabSpawningController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the camera associated with the AR Device. 
        /// </summary>
        /// <value>Once per scene.</value>
        [Header("Script References:")]
        [SerializeField]
        [Tooltip("Reference to the camera associated with the AR Device.")]
        private Camera arCamera;
        /// <summary>
        /// The AR Session Origin of this scene.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        [Tooltip("The AR Session Origin of this scene.")]
        private GameObject aRSessionOrigin;
        /// <summary>
        /// The AR Plane Manager of this scene.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        [Tooltip("The AR Plane Manager of this scene.")]
        private ARPlaneManager arPlaneManager;
        /// <summary>
        /// The AR Anchor Manager of this scene.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        [Tooltip("The AR Anchor Manager of this scene.")]
        private ARAnchorManager arAnchorManager;
        /// <summary>
        /// The AR Raycast Manager of this scene.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        [Tooltip("The AR Raycast Manager of this scene.")]
        private ARRaycastManager arRaycastManager;
        /// <summary>
        /// This infinity plane is placed under the setup containing the TrainAR-Objects. It acts as a collider for the TrainAR-Objects, so they won't fall into the ground.
        /// </summary>
        /// <value>Spawned with the prefab.</value>
        [Header("Prefabs:")]
        [SerializeField]
        [Tooltip("This infinity plane is placed under the setup containing the TrainAR-Objects. It acts as a collider for the TrainAR-Objects, so they won't fall into the ground.")]
        private GameObject infinityPlanePrefab;
        /// <summary>
        /// The material for the placement marker of the setup.
        /// </summary>
        /// <value>Default is set.</value>
        [Header("Options:")]
        [SerializeField]
        [Tooltip("The material for the placement marker of the setup.")]
        private Material spawningMaterial;
        /// <summary>
        /// The minimal detected size of the real-world-plane on which the AR-Setup is to be placed upon.
        /// </summary>
        /// <value>Default is 0.5f.</value>
        [Range(0.0f, 2.0f)]
        [SerializeField]
        [Tooltip("The minimal detected size of the real-world-plane on which the AR-Setup is to be placed upon.")]
        private float minPlanearea =0.5f;
        /// <summary>
        /// Is the infinity plane spawned.
        /// </summary>
        /// <value>True if spawned.</value>
        private bool infinityPlaneWasSpawned = false;
        /// <summary>
        /// Is the prefab spawned?
        /// </summary>
        /// <value>True if spawned.</value>
        [HideInInspector]
        public bool objectWasSpawned = false;
        /// <summary>
        /// Are the materials of the setup reset to original after spawning.
        /// </summary>
        /// <value>True if reset.</value>
        private bool materialsWereReset = false;
        /// <summary>
        /// Is the attempted placement pose valid?
        /// </summary>
        /// <value>True if valid.</value>
        [HideInInspector]
        public bool placementPoseIsValid = false;
        /// <summary>
        /// Reference to the spawned training assembly.
        /// </summary>
        /// <value>Gets created on runtime.</value>
        public static GameObject instantiatedPrefab;
        /// <summary>
        /// Reference to the infinity plane that is spawned with the setup.
        /// </summary>
        /// <value>Default prefab is referenced.</value>
        private GameObject instantiatedInfinityPlanePrefab;
        /// <summary>
        /// The position and rotation of the spawned prefab.
        /// </summary>
        /// <value>Set on runtime.</value>
        private Pose placementPose;
        /// <summary>
        /// Holder for found ARPlanes.
        /// </summary>
        private ARPlane plane;
        /// <summary>
        /// On initial placement of the prefab, we trigger the visual scripting flow to start, we dont do this on replacements.
        /// </summary>
        /// <value>Default is false.</value>
        private bool VisualScriptingFlowWasTriggerd = false;
        /// <summary>
        /// Holder to store all original materials to reset them after placing.
        /// </summary>
        /// <value>Set on runtime.</value>
        private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
        /// <summary>
        /// Event that is triggered after the assembly is spawned.
        /// </summary>
        public static event Action prefabSpawned;
        /// <summary>
        /// Event that is triggered after repositioning of the training assembly.
        /// </summary>
        public static event Action RepositionPrefab;

        /// <summary>
        /// Sets references to the ARRaycastmanager and triggers the creation of the TrainAR setup.
        /// </summary>
        private void Start()
        {
            arRaycastManager = FindObjectOfType<ARRaycastManager>();
            //Instantiate the prefab 2m behind the camera
            instantiatedPrefab = CreateTrainARSetup();
            instantiatedPrefab.transform.position = new Vector3(0, 0, -2.0f);
            MakeMaterialsInvisibleForPlacement();
            Debug.Log("PrefabSpawningController: Starting the positioning of the Prefab.");
        }

        /// <summary>
        /// Searches through the complete list of objects of the active scene and returns everything tagged as a
        /// TrainARObject as a list
        /// </summary>
        /// <returns>A list of all TrainARObject tagged objects</returns>
        private static List<GameObject> FindAllTrainARObjectsInScene()
        {
            /*UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            List<GameObject> objectsInScene = rootObjects.ToList();
            objectsInScene.AddRange(allObjects.Where(t => t.transform.root).Where(t => rootObjects.Any(t1 => t.transform.root == t1.transform && t != t1)));
            List<GameObject> trainARObjectsInScene = objectsInScene.Where(o => o.CompareTag("TrainARObject")).ToList();
            return trainARObjectsInScene;*/

            UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            List<GameObject> objectsInScene = rootObjects.ToList();
            List<GameObject> trainARObjectsInScene = objectsInScene.Where(o => !o.CompareTag("TrainAR")).ToList();
            return trainARObjectsInScene;
        }

        /// <summary>
        /// Creates the trainAR Setup by searching for all TrainAR tagged objects in the scene and setting
        /// their parent to the setup GameObject.
        /// </summary>
        /// <returns>The completed trainAR GameObject</returns>
        private GameObject CreateTrainARSetup()
        {
            //Create a setup
            GameObject setup = new GameObject("Setup");

            //Get all trainAR objects in the scene and parent them
            var trainARObjects = FindAllTrainARObjectsInScene();
            foreach (var trainARObject in trainARObjects)
            {
                trainARObject.transform.SetParent(setup.transform);
            }
            
            return setup;
        }
        /// <summary>
        /// Checks for inputs for the placement of the prefab and repositing.
        /// </summary>
        private void Update()
        {
            if(!objectWasSpawned)
            {
                PositionPrefab();

                //Spawn the construction with a touch on the screen
                if (Input.touchCount < 1) return;
                Touch touch = Input.GetTouch(0);
                //If Touch has been registered and placement pose has been declared valid, spawn the prefab
                if (touch.phase == TouchPhase.Began && placementPoseIsValid)
                {
                    SpawnPrefab();
                }
            }
            else if(infinityPlaneWasSpawned && objectWasSpawned && !materialsWereReset)
            {
                //Invoke the Event that the prefab was spawned
                prefabSpawned?.Invoke();
                //Let the Visual scripting know that we now positioned the prefab for the first time
                if (!VisualScriptingFlowWasTriggerd)
                {
                    EventBus.Trigger(VisualScriptingEventNames.OnboardingAndSetupCompleted, true);
                    VisualScriptingFlowWasTriggerd = true;
                }
                //Reset the materials back to original
                ResetMaterialsToOriginal();
                materialsWereReset = true;
                var points = aRSessionOrigin.GetComponent<ARPointCloudManager>().trackables;
                aRSessionOrigin.GetComponent<ARPointCloudManager>().enabled = false;
                foreach(var pts in points)
                {
                    pts.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Scans the area for an appropriate plane and initiates the positioning process for the prefab.
        /// </summary>
        private void PositionPrefab()
        {
            if (Camera.current == null) return;
            var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var hits = new List<ARRaycastHit>();

            arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinBounds);

          //  placementPoseIsValid = hits.Count > 0;
            if (hits.Count > 0)
            {
                plane = arPlaneManager.GetPlane(hits[0].trackableId);

                if (plane.size.x * plane.size.y >= minPlanearea && plane.subsumedBy == null && plane.alignment == PlaneAlignment.HorizontalUp)
                {
                    placementPose = hits[0].pose;
                    placementPoseIsValid = true;
                }
                else
                {
                    instantiatedPrefab.transform.SetPositionAndRotation(new Vector3(0, 0, -2), Quaternion.identity);
                    placementPoseIsValid = false;
                    return;
                }
                //Set the new position and rotation to the object
                instantiatedPrefab.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
                //Let the object face the camera
                instantiatedPrefab.transform.LookAt(arCamera.transform);
                
                //Delete every rotation except the y rotation (to have it face towards the ground
                Quaternion newRotation = Quaternion.Euler(0, instantiatedPrefab.transform.rotation.eulerAngles.y, 0);
                //Set the new position and rotation to the object
                instantiatedPrefab.transform.SetPositionAndRotation(placementPose.position, newRotation);
            }
            else
            {
                instantiatedPrefab.transform.SetPositionAndRotation(new Vector3(0, 0, -2), Quaternion.identity);
                placementPoseIsValid = false;
            }
        }

        /// <summary>
        /// Spawns the infinity plane at the position of the placement pose and sets the prefab as a child of the infinity plane.
        /// </summary>
        private void SpawnPrefab()
        {
            //Add an anchor attached to this plane for better tracking
            ARAnchor planeAnchor = arAnchorManager.AttachAnchor(plane, placementPose);

            //Instantiate the infinityPlanePrefab at this position
            instantiatedInfinityPlanePrefab = Instantiate(infinityPlanePrefab, placementPose.position, placementPose.rotation, planeAnchor.gameObject.transform);
            //Register that the infinity plane was spawned
            infinityPlaneWasSpawned = true;

            instantiatedPrefab.transform.SetParent(instantiatedInfinityPlanePrefab.transform);
            instantiatedPrefab.transform.localPosition = new Vector3(instantiatedPrefab.transform.localPosition.x, 0f , instantiatedPrefab.transform.localPosition.z);
            instantiatedPrefab.transform.localRotation = Quaternion.Euler(0, instantiatedPrefab.transform.localRotation.eulerAngles.y, 0);

            //Register that the prefab was positioned
            objectWasSpawned = true;
            Debug.Log("PrefabSpawningController: Successfully (re)positioned the Prefab.");
        }

        /// <summary>
        /// Returns the instantiated prefab.
        /// </summary>
        /// <returns>The instantiated prefab.</returns>
        public GameObject GetSpawnedObject()
        {
            return instantiatedPrefab;
        }

        /// <summary>
        /// Replaces all materials on the spawned prefab with a transparent material and stores the reference
        /// to the original one to reset them later.
        /// </summary>
        private void MakeMaterialsInvisibleForPlacement()
        {
            //Get all the renderers in the childs of the spawned prefab
            Renderer[] objectRenderers = instantiatedPrefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer objectRenderer in objectRenderers)
            {

                //Store the reference to the object and its original material to reset them after placement
                originalMaterials.Add(objectRenderer.gameObject, objectRenderer.materials);

                //Replace all the materials with the transparent spawning material
                Material[] thisRenderersMaterials = new Material[objectRenderer.materials.Length];
                for (int j = 0; j < objectRenderer.materials.Length; j++)
                {
                    thisRenderersMaterials[j] = spawningMaterial;
                }
                objectRenderer.materials = thisRenderersMaterials;
            }
        }

        /// <summary>
        /// Resets the materials of objects to their original ones.
        /// </summary>
        private void ResetMaterialsToOriginal()
        {
            foreach (KeyValuePair<GameObject, Material[]> obj in originalMaterials)
            {
                obj.Key.GetComponent<Renderer>().materials = obj.Value;
            }
        }

        /// <summary>
        /// Restarts the positioning process to make reposition of the prefab possible.
        /// </summary>
        public void Reposition()
        {
            if (objectWasSpawned == false)
            {
                Debug.LogWarning("PrefabSpawningController: Tried to reset prefab while already spawning/positioning it. Terminating function.");
                return;
            }
            
            Debug.Log("PrefabSpawningController: The user started repositioning of the Prefab.");

            //Unparent the instantiated prefab from the infinity plane and hide it behind the camera.
            instantiatedPrefab.transform.parent = null;
            instantiatedPrefab.transform.SetPositionAndRotation(new Vector3(0, 0, -2), Quaternion.identity);

            //Reset the originalMaterials container and remake the instantiated prefab transparent for placement.
            ResetMaterialsToOriginal();
            originalMaterials = new Dictionary<GameObject, Material[]>();
            MakeMaterialsInvisibleForPlacement();
            materialsWereReset = false;

            //Destroy the now unused infinity plane.
            Destroy(instantiatedInfinityPlanePrefab);

            //Set these booleans accordingly.
            infinityPlaneWasSpawned = false;
            objectWasSpawned = false;
            //isValidPlaneSize = false;

            //Invoke the Reposition Prefab event.
            RepositionPrefab?.Invoke();
        }
    }
}
