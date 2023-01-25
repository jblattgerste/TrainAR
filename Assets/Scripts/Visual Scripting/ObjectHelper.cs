using System;
using Interaction;
using Others;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Visual_Scripting
{
    /// <summary>
    /// The Object helper provides utility methods to toggle visibility, grabbability, interactability, and combinability of TrainAR objects, to destroy them, to get GameObject references to TrainAR object in the stateflow and many other utility functions.
    /// </summary>
    [UnitTitle("TrainAR: Object Helper")] //The title of the unit
    [UnitSubtitle("Provides helper utility for TrainAR\nobjects. For details on specific helper\nutility, consult the documentation.")] //Explanation of the unit
    [UnitCategory("TrainAR")] //The folder it is stored at in the flow graph unit menu
    [TypeIcon(typeof(Resources))]
    public class ObjectHelper : Unit
    {
        /// <summary>
        /// The available helper utility to select on the "TrainAR: Object Helper" nodes menu.
        /// </summary>
        public enum TrainARHelperChoices
        {
            ToggleVisibility,
            ToggleGrabbable,
            ToggleInteractable,
            ToggleCombinable,
            ChangeInteractionText,
            ChangeLerpingDistance,
            ReplaceMeshAndTexture,
            GetObjectReference,
            FuseTwoObjects,
            DestroyObject,
            ReplaceTrainARObject,
        }
        
        [UnitHeaderInspectable("Helper: ")]
        public TrainARHelperChoices helperChoice = TrainARHelperChoices.ToggleVisibility;

        /// <summary>
        /// The Input port of the Unit that triggers the internal logic.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ControlInput InputFlow { get; private set; }

        /// <summary>
        /// The Output port of the Unit that is tirggered after executing the units logic.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ControlOutput OutputFlow { get; private set; }

        /// <summary>
        /// Name of the Object we want to get the reference for.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput TrainARObjectName { get; private set; }

        /// <summary>
        /// Toggle for the visibility that is displayed when this helper is selected.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput VisibilityToggle { get; private set; }

        /// <summary>
        /// Toggle for the grabbability that is displayed when this helper is selected.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput GrabbabilityToggle { get; private set; }

        /// <summary>
        /// Toggle for the interactability that is displayed when this helper is selected.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput InteractabilityToggle { get; private set; }

        /// <summary>
        /// Toggle for the combinability that is displayed when this helper is selected.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput CombinabilityToggle { get; private set; }

        /// <summary>
        /// The reference to the GameObject in the scene.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueOutput objectReference { get; private set; }
        /// <summary>
        /// The reference to the GameObject in the scene.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        private GameObject realObjectReference;

        /// <summary>
        /// Mesh to replace.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput ObjectMesh { get; private set; }

        /// <summary>
        /// Texture to replace.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput ObjectMaterial { get; private set; }

        /// <summary>
        /// The interactable text that is displayed on the interaction button.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput InteractableText { get; private set; }

        /// <summary>
        /// The lerping distance of the object to the smartphone.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput Lerpingdistance { get; private set; }

        /// <summary>
        /// The second object to fuse together (with the original one).
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput ObjectTwo { get; private set; }

        /// <summary>
        /// The positional offset of the fusion of the objects.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput FusionOffsetPosition { get; private set; }

        /// <summary>
        /// The rotational offset of the fusion of the objects.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput FusionOffsetRotation { get; private set; }

        /// <summary>
        /// Defines the Nodes input, output and value ports
        /// </summary>
        protected override void Definition()
        {
            //Defining the Input port of the flow & Node Logic
            InputFlow = ControlInput("", NodeLogic);
            
            //Output flow (this is only a paththrough for an output Unit)
            OutputFlow = ControlOutput("");

            //Fixed ValueInputs of the unit
            TrainARObjectName = ValueInput<string>("Object name", string.Empty);

            //Define ValueInputs and ValueOutputs based on the selected helper choice
            switch (helperChoice)
            {
                case TrainARHelperChoices.ToggleVisibility:
                    VisibilityToggle = ValueInput<bool>("Visible", true);
                    break;
                case TrainARHelperChoices.ToggleGrabbable:
                    GrabbabilityToggle = ValueInput<bool>("Grabbable", true);
                    break;
                case TrainARHelperChoices.ToggleInteractable:
                    InteractabilityToggle = ValueInput<bool>("Interactable", true);
                    break;
                case TrainARHelperChoices.ToggleCombinable:
                    CombinabilityToggle = ValueInput<bool>("Combinable", true);
                    break;
                case TrainARHelperChoices.DestroyObject:
                    break;
                case TrainARHelperChoices.GetObjectReference:
                    objectReference = ValueOutput<GameObject>("Object reference", flow => realObjectReference);
                    break;
                case TrainARHelperChoices.ReplaceMeshAndTexture:
                    ObjectMesh = ValueInput<Mesh>("Mesh", null);
                    ObjectMaterial = ValueInput<Material>("Material", null);
                    break;
                case TrainARHelperChoices.ChangeInteractionText:
                    InteractableText = ValueInput<string>("Interaction text", string.Empty);
                    break;
                case TrainARHelperChoices.ChangeLerpingDistance:
                    Lerpingdistance = ValueInput<float>("Lerping distance", 0.2f);
                    break;
                case TrainARHelperChoices.FuseTwoObjects:
                    ObjectTwo = ValueInput<string>("Fuse to Object", string.Empty);
                    FusionOffsetPosition = ValueInput<Vector3>("Offset Position", Vector3.zero);
                    FusionOffsetRotation = ValueInput<Vector3>("Offset Rotation", Vector3.zero);
                    break;
                case TrainARHelperChoices.ReplaceTrainARObject:
                    ObjectTwo = ValueInput<string>("Replace with", string.Empty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    
        /// <summary>
        /// The NodeLogic that is triggered when an input flow is detected on the controlInput.
        ///
        /// Searches for a TrainAR Object with a specific name and activates it
        /// </summary>
        /// <param name="flow">The current flow of the graph</param>
        /// <returns>Returns to the output flow immediatly after triggering its internal logic</returns>
        private ControlOutput NodeLogic(Flow flow)
        {
            Transform trainARObject = null;

            //Initially store the name of the currently grabbed object
            GameObject trainARObjectHolder = Object.FindObjectOfType<InteractionController>().grabbedObject;
           
            //Check if the grabbed object is the one we are searching for, otherwise search all other spawned Objects and override it   
            if (trainARObjectHolder != null && trainARObjectHolder.name == flow.GetValue<string>(TrainARObjectName))
            {
                trainARObject = trainARObjectHolder.transform;
            }
            else
            {
                foreach (var foundTrainARObject in PrefabSpawningController.instantiatedPrefab.GetComponentsInChildren<Transform>(true))
                {
                    //Search until we get an object matching the one specified in the ValueInput
                    if (foundTrainARObject.gameObject.name != flow.GetValue<string>(TrainARObjectName)) continue;

                    //Save it as the temporary variable and break the foreach
                    trainARObject = foundTrainARObject;
                    break;
                }
            }    
            //perform the selected option on the object
            switch (helperChoice)
            {
                case TrainARHelperChoices.ToggleVisibility:
                    //If the object to have visibility disabled is currently grabbed, call Release on the InteractionController
                    if (trainARObject.gameObject.GetComponent<TrainARObject>().isGrabbed)
                    {
                        //Yea this is probably not best practise but will be alright
                        Object.FindObjectOfType<InteractionController>().ReleaseGrabbedObject();
                    }
                    trainARObject.gameObject.SetActive(flow.GetValue<bool>(VisibilityToggle));
                    break;
                case TrainARHelperChoices.ToggleGrabbable:
                    //If the object to have grabbing disabled is currently grabbed, call Release on the InteractionController
                    if (trainARObject.gameObject.GetComponent<TrainARObject>().isGrabbed)
                    {
                        //Yea this is probably not best practise but will be alright
                        Object.FindObjectOfType<InteractionController>().ReleaseGrabbedObject();
                    }
                    trainARObject.gameObject.GetComponent<TrainARObject>().isGrabbable = flow.GetValue<bool>(GrabbabilityToggle);
                    break;
                case TrainARHelperChoices.ToggleInteractable:
                    trainARObject.gameObject.GetComponent<TrainARObject>().isInteractable = flow.GetValue<bool>(InteractabilityToggle);
                    break;
                case TrainARHelperChoices.ToggleCombinable:
                    trainARObject.gameObject.GetComponent<TrainARObject>().isCombineable = flow.GetValue<bool>(CombinabilityToggle);
                    break;
                case TrainARHelperChoices.DestroyObject:
                    //If the object to be destroyed is currently grabbed, call Release on the InteractionController
                    if (trainARObject.gameObject.GetComponent<TrainARObject>().isGrabbed)
                    {
                        //Yea this is probably not best practise but will be alright
                        Object.FindObjectOfType<InteractionController>().ReleaseGrabbedObject();
                    }
                    Object.Destroy(trainARObject.gameObject);
                    break;
                case TrainARHelperChoices.GetObjectReference:
                    realObjectReference = trainARObject.gameObject;
                    break;
                case TrainARHelperChoices.ChangeInteractionText:
                    trainARObject.gameObject.GetComponent<TrainARObject>().interactableName = flow.GetValue<string>(InteractableText);
                    break;
                case TrainARHelperChoices.ChangeLerpingDistance:
                    trainARObject.gameObject.GetComponent<TrainARObject>().lerpingDistance = flow.GetValue<float>(Lerpingdistance);
                    break;
                case TrainARHelperChoices.ReplaceMeshAndTexture:
                    trainARObject.gameObject.GetComponent<MeshRenderer>().material = flow.GetValue<Material>(ObjectMaterial);
                    trainARObject.gameObject.GetComponent<MaterialController>().setNewOriginalMaterial(trainARObject.gameObject , flow.GetValue<Material>(ObjectMaterial));
                    trainARObject.gameObject.GetComponent<MeshFilter>().mesh = flow.GetValue<Mesh>(ObjectMesh);
                    //trainARObject.gameObject.GetComponent<MeshRenderer>().materials[0] = flow.GetValue<Material>(ObjectMaterial);
                    break;
                case TrainARHelperChoices.FuseTwoObjects:
                    //If the object to be destroyed is currently grabbed, call Release on the InteractionController
                    if (trainARObject.gameObject.GetComponent<TrainARObject>().isGrabbed)
                    {
                        //Yea this is probably not best practise but will be alright
                        Object.FindObjectOfType<InteractionController>().ReleaseGrabbedObject(true);
                    }
                    //Find the secondary trainAR object
                    foreach (var secondaryTrainARObject in PrefabSpawningController.instantiatedPrefab
                        .GetComponentsInChildren<Transform>(true))
                    {
                        //Search until we get an object matching the one specified in the ValueInput
                        if (secondaryTrainARObject.gameObject.name != flow.GetValue<string>(ObjectTwo)) continue;
                        //Fuse the two objects and set the local positional and rotational offsets of the node
                        trainARObject.GetComponent<MaterialController>().resetOriginalMaterial();
                        trainARObject.GetComponent<TrainARObject>().isGrabbable = false;
                        trainARObject.SetParent(secondaryTrainARObject);
                        trainARObject.localPosition = flow.GetValue<Vector3>(FusionOffsetPosition);
                        trainARObject.localRotation = Quaternion.Euler(flow.GetValue<Vector3>(FusionOffsetRotation));
                        break;
                    }
                    break;
                case TrainARHelperChoices.ReplaceTrainARObject:
                    foreach (var secondaryTrainARObject in PrefabSpawningController.instantiatedPrefab
                        .GetComponentsInChildren<Transform>(true))
                    {
                        //Search until we get an object matching the one specified in the ValueInput
                        if (secondaryTrainARObject.gameObject.name != flow.GetValue<string>(ObjectTwo)) continue;
                        secondaryTrainARObject.SetParent(trainARObject.transform.parent);
                        secondaryTrainARObject.transform.localPosition = trainARObject.transform.localPosition;
                        secondaryTrainARObject.gameObject.SetActive(true);
                        if (trainARObject.gameObject.GetComponent<TrainARObject>().isGrabbed)
                        {
                            //Again, not best practice, but works...
                            InteractionController interactionController = Object.FindObjectOfType<InteractionController>();
                            interactionController.ReleaseGrabbedObject();
                            interactionController.selectedObject = secondaryTrainARObject.gameObject;
                            interactionController.GrabObject();
                        }
                        trainARObject.GetComponent<MaterialController>().resetOriginalMaterial();
                        //Set replaced object inactive
                        trainARObject.gameObject.SetActive(false);
                        break;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
            //Return the outputflow, therefore instantly after triggering its logic continues the graph
            return OutputFlow;
        }
    }
}
