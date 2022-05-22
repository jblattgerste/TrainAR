using Others;
using Static;
using UnityEngine;
using UnityEngine.Events;

namespace Interaction
{
    /// <summary>
    /// The main behaviour script for a TrainAR object.
    /// This is automatically added when converting GameObject to TrainAR objects
    /// After adding this script all following RequiredComponents are added.
    /// SelectionBase, MeshCollider, BoxCollider, RigidbodyController,
    /// MaterialController, AudioController, Rigidbody, Outline,
    /// CollisionController.
    /// The TrainAR events that are triggered on actions are also defined in this script.
    /// </summary>
    [RequireComponent(typeof(SelectionBase))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(RigidbodyController))]
    [RequireComponent(typeof(MaterialController))]
    [RequireComponent(typeof(AudioController))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Outline))]
    [RequireComponent(typeof(CollisionController))]
    public class TrainARObject : MonoBehaviour
    {
        //public bool isInteractable = true; //Not used yet
        //public bool isCombinable = true; //Not used yet
        /// <summary>
        /// Is the object grabbable?
        /// </summary>
        /// <value>If true it is grabbable.</value>
        public bool isGrabbable = true;
        /// <summary>
        /// Is the object interactable?
        /// </summary>
        /// <value>If true it is interactable.</value>
        public bool isInteractable = true;
        /// <summary>
        /// Is the object combineable?
        /// </summary>
        /// <value>If true it is combinable.</value>
        public bool isCombineable = true;
        /// <summary>
        /// The name of the TrainAR object that is used for the statemachine check.
        /// By default, this is the name of the TrainAR object.
        /// </summary>
        /// <value>String based on the object name.</value>
        public string interactableName;
        /// <summary>
        /// If true the object is not selectable, grabbable, interactable, combineable but might be visible.
        /// </summary>
        /// <value>Default is false.</value>
        public bool TrainARObjectDisabled = false;
        /// <summary>
        /// The distance in front of the camera, where the object is lerped to.
        /// This can e.g. be useful, if objects are larger than usual.
        /// </summary>
        /// <value>Default is 0.2f</value>
        [Range(0.0f, 1f)]
        public float lerpingDistance =  0.2f;
        /// <summary>
        /// Offset of the pivot point. Use this, if the given pivot point by the model is weird.
        /// </summary>
        /// <value>Default is (0, 0, 0).</value>
        [SerializeField]
        public Vector3 pivotOffsetPosition = new Vector3(0, 0, 0);
        /// <summary>
        /// Offset of the pivot rotation. Use this, if the given pivot point by the model is weird.
        /// </summary>
        /// <value>Default is (0, 0, 0).</value>
        [SerializeField]
        public Vector3 pivotOffsetRotation = new Vector3(0, 0, 0);
        /// <summary>
        /// The Object this object is currently intersecting with.
        /// </summary>
        /// <value>Is set on runtime.</value>
        [HideInInspector]
        public CollisionController.Intersection Intersection = new CollisionController.Intersection(null, false);
        
        /// <summary>
        /// SELECT & DESELECT
        /// </summary>
        [HideInInspector]
        //Suppress "no value assigned" warning, as this is intended behaviour
#pragma warning disable 0649
        public UnityEvent OnSelect;
#pragma warning restore 0649

        [HideInInspector]
        //Suppress "no value assigned" warning, as this is intended behaviour
#pragma warning disable 0649
        public UnityEvent OnDeselect;
#pragma warning restore 0649

        [HideInInspector] public bool isSelected = false;


        /// <summary>
        /// GRAB & RELEASE
        /// </summary>
        [SerializeField]
        //Suppress "no value assigned" warning, as this is intended behaviour
#pragma warning disable 0649
        public UnityEvent OnGrabbed;
#pragma warning restore 0649

        [SerializeField]
        //Suppress "no value assigned" warning, as this is intended behaviour
#pragma warning disable 0649
        public UnityEvent OnReleased;
#pragma warning restore 0649

        [HideInInspector] public bool isGrabbed = false;


        /// <summary>
        /// INTERACT & COMBINE
        /// </summary>
        [SerializeField]
        //Suppress "no value assigned" warning, as this is intended behaviour
#pragma warning disable 0649
        public UnityEvent OnInteraction;
#pragma warning restore 0649

        /// <summary>
        /// The CustomUnityEvent that is used for the OnCombination event. This Event also passes a string with
        /// its invocation indicating the other to be combined with TrainAR objects name.
        /// </summary> 
        [System.Serializable]
        public class CustomUnityEvent : UnityEvent<string>
        {
        }

        [SerializeField]
        //Suppress "no value assigned" warning, as this is intended behaviour
#pragma warning disable 0649
        public CustomUnityEvent OnCombination;
#pragma warning restore 0649
        
        
        
        /// <summary>
        /// Error event. Triggered when an action is triggered on this TrainAR object that was not accepted by the statemachine.
        /// </summary>
         [SerializeField]
        //Suppress "no value assigned" warning, as this is intended behaviour
#pragma warning disable 0649
        [HideInInspector] public UnityEvent error;
#pragma warning restore 0649

        private void OnValidate()
        {
            interactableName = gameObject.name;
            GetComponent<Outline>().enabled = false;
            GetComponent<MeshCollider>().convex = true;
        }

        private void Awake()
        {
            //interactableName = gameObject.name;
            //GetComponent<Outline>().enabled = false;
            //GetComponent<MeshCollider>().convex = true;
            this.gameObject.tag = "TrainARObject";
            if (TrainARObjectDisabled)
            {
                DisableTrainARObject();
            }
        }
        
        /// <summary>
        /// Invokes the select event of this TrainARObject and sets it's isSelected bool accordingly.
        /// </summary>
        public void Select()
        {
                    if (gameObject.activeSelf)
                    {
                        OnSelect.Invoke();
                        isSelected = true;
                        return;
                    }
        }

        /// <summary>
        /// Invokes the Deselect event of this TrainARObject and sets it's isSelected bool accordingly.
        /// </summary>
        public void Deselect()
        {
                    if (gameObject.activeSelf)
                    {
                        OnDeselect.Invoke();
                        isSelected = false;
                        return;
                    }
        }

        /// <summary>
        /// Sends a request to the statemachine to check if this interaction was valid.
        /// If so, the interact event of this TrainARObject is invoked
        /// otherwise the error event is invoked instead.
        /// </summary>
        /// <param name="parameter">A string parameter which is passed to the statemachine.</param>
        public void Interact(string parameter = " ")
        {
            
                    if (gameObject.activeSelf)
                    {
                        if (!isInteractable)
                        {
                            error.Invoke();
                            return;
                        }
                        if (StatemachineConnector.Instance.RequestStateChange(new StateInformation(
                            interactableName,
                            null, InteractionType.Interact, parameter,this.gameObject)))
                        {
                            OnInteraction.Invoke();
                        }
                        else
                        {
                            error.Invoke();
                        }
                        return;
                    }
        }

        /// <summary>
        /// Sends a request to the statemachine to check if this combine was valid.
        /// If so, the combine event of this TrainARObject is invoked
        /// otherwise the error event is invoked instead.
        ///
        /// This does NOT physically combine the object, which has to be handled by hand or e.g. the TrainAR Object
        /// helper in the visual scripting.
        /// </summary>
        /// <param name="parameter">A string parameter which is passed to the statemachine.</param>
        public void Combine(string combinedWithName, GameObject intersectedObject)
        {
                    if (gameObject.activeSelf)
                    {
                        if (!isCombineable)
                        {
                            error.Invoke();
                            if (intersectedObject != null)
                            {
                                intersectedObject.GetComponent<TrainARObject>().error.Invoke();
                            }
                            return;
                        }
                        if (StatemachineConnector.Instance.RequestStateChange(new StateInformation(
                            interactableName,
                            combinedWithName, InteractionType.Combine, "" , this.gameObject, Intersection.GetIntersectedObject())))
                        {
                            Intersection.SetIntersectedObject(null);
                            Intersection.SetIntersectionDetected(false);
                            OnCombination.Invoke(combinedWithName);
                            if (intersectedObject != null)
                            {
                                intersectedObject.GetComponent<TrainARObject>().OnCombination.Invoke(interactableName);
                            }
                        }
                        else
                        {
                            if (intersectedObject != null)
                            {
                                intersectedObject.GetComponent<TrainARObject>().error.Invoke();
                            }
                            
                            error.Invoke();
                        }
                        return;
                    }
        }

        /// <summary>
        /// Sends a request to the statemachine to check if this grab was valid.
        /// If so, the grab event of this TrainARObject is invoked
        /// otherwise the error event is invoked instead.
        ///
        /// This does NOT grab the object, which is handled in the InteractionController
        /// </summary>
        /// <param name="parameter">A string parameter which is passed to the statemachine.</param>
        public bool Grab()
        {
                    if (gameObject.activeSelf)
                    {
                        if (!isGrabbable)
                        {
                            error.Invoke();
                            return false;
                        }
                        else if (StatemachineConnector.Instance.RequestStateChange(new StateInformation(
                            interactableName, null, InteractionType.Grab)))
                        {
                            OnGrabbed.Invoke();
                            isGrabbed = true;
                            return true;
                        }
                        else
                        {
                            error.Invoke();
                            return false;
                        }
                    }
                    return false;
        }

        /// <summary>
        /// Invokes the Release event of this TrainARObject and sets it's isGrabbed bool accordingly.
        ///
        /// This does NOT release the object, which is handled in the InteractionController
        /// </summary>
        public void Release()
        {
                    if (gameObject.activeSelf)
                    {
                        OnReleased.Invoke();
                        isGrabbed = false;
                        return;
                    }
        }

        /// <summary>
        /// Invokes the error event of this TrainARObject.
        /// </summary>
        public void Error()
        {
            error.Invoke();
        }
        
        /// <summary>
        /// Disables interactions with this TrainARObject by disabling it's colliders and Mesh Renderer.
        /// </summary>
        public void DisableTrainARObject()
        {
            TrainARObjectDisabled = true;
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<MeshCollider>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
        }
        
        /// <summary>
        /// Enables interactions with this TrainARObject by disabling it's colliders and Mesh Renderer.
        /// </summary>
        public void EnableTrainARObject()
        {
            TrainARObjectDisabled = false;
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<MeshCollider>().enabled = true;
            GetComponent<BoxCollider>().enabled = true;
        }
        
    }
}
