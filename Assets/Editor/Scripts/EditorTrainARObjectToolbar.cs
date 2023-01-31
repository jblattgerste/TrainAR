using Interaction;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;

namespace Editor.Scripts
{
    /// <summary>
    /// The EditorTrainARObjectToolbar is the toolbar in the sceneview when using the TrainAR authoring overlay. It allows
    /// setting TrainAR Object states like active/inactive, grabbable, interactable, or combinable without opening an inspector.
    /// </summary>
    [Overlay(typeof(SceneView), "TrainAR Object State")]
    public class EditorTrainARObjectToolbar : ToolbarOverlay
    {
        private EditorTrainARObjectToolbar() : base(ActiveToggle.id, GrabbableToggle.id, InteractableToggle.id, CombinableToggle.id) {}

        public override void OnCreated()
        {
            Selection.selectionChanged += UpdateActivityState;
        }

        void UpdateActivityState()
        {
            if (Selection.activeTransform == null)
            {
                displayed = true;
                return;
            }
            if (Selection.gameObjects.Length > 1)
            {
                displayed = false;
                return;
            }
            if (!Selection.activeTransform.CompareTag("TrainARObject"))
            {
                displayed = false;
                return;
            }
            displayed = true;
        }
    }
    
    [EditorToolbarElement(id, typeof(SceneView))]
    internal class ActiveToggle : EditorToolbarToggle, IAccessContainerWindow
    {
        private static string defaultString = "   -";
        private static string activeStateString = "   Visible";
        private static string inactiveStateString = "   Invisible";
        public const string id = "TrainAR/VisibilityToggle";
        public EditorWindow containerWindow { get; set; }
    
        /// <summary>
        /// Constructor of the toggle
        /// </summary>
        public ActiveToggle()
        {
            text = defaultString;
            tooltip = "Toggles the visibility state of a TrainAR Object. Invisible Objects still exist but are not grabbable/interactable/combinable and do not elicit physics.";
        
            //Update this when the selection changed or the user undid/redid actions
            Selection.selectionChanged += HierarchySelectionChangedActiveToggle;
            Undo.undoRedoPerformed += HierarchySelectionChangedActiveToggle;
            
            this.RegisterValueChangedCallback(ToggleObjectVisibility);
        }

        private void HierarchySelectionChangedActiveToggle()
        {
            //Ensure there is only one object selected
            if (Selection.gameObjects.Length <= 0)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (Selection.gameObjects.Length < 1)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (!Selection.gameObjects[0].CompareTag("TrainARObject"))
            {
                UpdateToggleStatus(defaultString, false);
            }
            else
            {
                if (Selection.gameObjects[0].activeSelf)
                {
                    UpdateToggleStatus(activeStateString, true);
                }
                    
                else
                {
                    UpdateToggleStatus(inactiveStateString, false);
                }
            }
        }

        private void ToggleObjectVisibility(ChangeEvent<bool> newSelectionEvent)
        {
            if (Selection.gameObjects.Length <= 0)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (Selection.gameObjects.Length < 1)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (!Selection.gameObjects[0].CompareTag("TrainARObject"))
            {
                UpdateToggleStatus(defaultString, false);
            }
            else
            {
                if (newSelectionEvent.newValue &&  !Selection.gameObjects[0].activeSelf)
                {
                    //Register changes on the undo stack
                    Undo.RegisterCompleteObjectUndo(Selection.gameObjects[0], "Toggle Visibility");
                    
                    Selection.gameObjects[0].SetActive(true);
                    UpdateToggleStatus(activeStateString, true);
                }
                else if(!newSelectionEvent.newValue &&  Selection.gameObjects[0].activeSelf)
                {
                    //Register changes on the undo stack
                    Undo.RegisterCompleteObjectUndo(Selection.gameObjects[0], "Toggle Visibility");
                    
                    Selection.gameObjects[0].SetActive(false);
                    UpdateToggleStatus(inactiveStateString, false);
                }
                else
                {
                    UpdateToggleStatus(newSelectionEvent.newValue? activeStateString : inactiveStateString, newSelectionEvent.newValue);
                }
            }
        }
    
        private void UpdateToggleStatus(string toggleText, bool toggleValue)
        {
            text = toggleText;
            value = toggleValue;
        }
    }

    [EditorToolbarElement(id, typeof(SceneView))]
    internal class GrabbableToggle : EditorToolbarToggle, IAccessContainerWindow
    {
        private static string defaultString = "   -";
        private static string activeStateString = "   Grabbable";
        private static string inactiveStateString = "   Not Grabbable";
        public const string id = "TrainAR/GrabbableToggle";
        public EditorWindow containerWindow { get; set; }
    
        /// <summary>
        /// Constructor of the toggle
        /// </summary>
        public GrabbableToggle()
        {
            text = defaultString;
            tooltip = "Toggles whether a TrainAR Object is grabbable by the TrainAR interaction concept or not";
        
            //Update this when the selection changed or the user undid/redid actions
            Selection.selectionChanged += HierarchySelectionChangedGrabbableToggle;
            Undo.undoRedoPerformed += HierarchySelectionChangedGrabbableToggle;
            
            this.RegisterValueChangedCallback(ToggleObjectVisibility);
        }

        private void HierarchySelectionChangedGrabbableToggle()
        {
            //Ensure there is only one object selected
            if (Selection.gameObjects.Length <= 0)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (Selection.gameObjects.Length < 1)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (!Selection.gameObjects[0].CompareTag("TrainARObject"))
            {
                UpdateToggleStatus(defaultString, false);
            }
            else
            {
                if (Selection.gameObjects[0].GetComponent<TrainARObject>().isGrabbable)
                {
                    UpdateToggleStatus(activeStateString, true);
                }
                    
                else
                {
                    UpdateToggleStatus(inactiveStateString, false);
                }
            }
        }

        private void ToggleObjectVisibility(ChangeEvent<bool> newSelectionEvent)
        {
            if (Selection.gameObjects.Length <= 0)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (Selection.gameObjects.Length < 1)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (!Selection.gameObjects[0].CompareTag("TrainARObject"))
            {
                UpdateToggleStatus(defaultString, false);
            }
            else
            {
                if (newSelectionEvent.newValue &&  !Selection.gameObjects[0].GetComponent<TrainARObject>().isGrabbable)
                {
                    //Register changes on the undo stack
                    Undo.RegisterFullObjectHierarchyUndo(Selection.gameObjects[0], "Toggle Grabbability");
                    
                    Selection.gameObjects[0].GetComponent<TrainARObject>().isGrabbable = true;
                    UpdateToggleStatus(activeStateString, true);
                }
                else if(!newSelectionEvent.newValue &&  Selection.gameObjects[0].GetComponent<TrainARObject>().isGrabbable)
                {
                    //Register changes on the undo stack
                    Undo.RegisterFullObjectHierarchyUndo(Selection.gameObjects[0], "Toggle Grabbability");
                    
                    Selection.gameObjects[0].GetComponent<TrainARObject>().isGrabbable = false;
                    UpdateToggleStatus(inactiveStateString, false);
                }
                else
                {
                    UpdateToggleStatus(newSelectionEvent.newValue? activeStateString : inactiveStateString, newSelectionEvent.newValue);
                }
            }
        }
    
        private void UpdateToggleStatus(string toggleText, bool toggleValue)
        {
            text = toggleText;
            value = toggleValue;
        }
    }


    [EditorToolbarElement(id, typeof(SceneView))]
    internal class InteractableToggle : EditorToolbarToggle, IAccessContainerWindow
    {
        private static string defaultString = "   -";
        private static string activeStateString = "   Interactable";
        private static string inactiveStateString = "   Not Interactable";
        public const string id = "TrainAR/InteractableToggle";
        public EditorWindow containerWindow { get; set; }
    
        /// <summary>
        /// Constructor of the toggle
        /// </summary>
        public InteractableToggle()
        {
            text = defaultString;
            tooltip = "Toggles whether a TrainAR Object is interactable by the TrainAR interaction concept or not";
        
            //Update this when the selection changed or the user undid/redid actions
            Selection.selectionChanged += HierarchySelectionChangedInteractableToggle;
            Undo.undoRedoPerformed += HierarchySelectionChangedInteractableToggle;
            
            this.RegisterValueChangedCallback(ToggleObjectVisibility);
        }

        private void HierarchySelectionChangedInteractableToggle()
        {
            //Ensure there is only one object selected
            if (Selection.gameObjects.Length <= 0)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (Selection.gameObjects.Length < 1)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (!Selection.gameObjects[0].CompareTag("TrainARObject"))
            {
                UpdateToggleStatus(defaultString, false);
            }
            else
            {
                if (Selection.gameObjects[0].GetComponent<TrainARObject>().isInteractable)
                {
                    UpdateToggleStatus(activeStateString, true);
                }
                    
                else
                {
                    UpdateToggleStatus(inactiveStateString, false);
                }
            }
        }

        private void ToggleObjectVisibility(ChangeEvent<bool> newSelectionEvent)
        {
            if (Selection.gameObjects.Length <= 0)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (Selection.gameObjects.Length < 1)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (!Selection.gameObjects[0].CompareTag("TrainARObject"))
            {
                UpdateToggleStatus(defaultString, false);
            }
            else
            {
                if (newSelectionEvent.newValue &&  !Selection.gameObjects[0].GetComponent<TrainARObject>().isInteractable)
                {
                    //Register changes on the undo stack
                    Undo.RegisterFullObjectHierarchyUndo(Selection.gameObjects[0], "Toggle Interactability");
                    
                    Selection.gameObjects[0].GetComponent<TrainARObject>().isInteractable = true;
                    UpdateToggleStatus(activeStateString, true);
                }
                else if(!newSelectionEvent.newValue &&  Selection.gameObjects[0].GetComponent<TrainARObject>().isInteractable)
                {
                    //Register changes on the undo stack
                    Undo.RegisterFullObjectHierarchyUndo(Selection.gameObjects[0], "Toggle Interactability");
                    
                    Selection.gameObjects[0].GetComponent<TrainARObject>().isInteractable = false;
                    UpdateToggleStatus(inactiveStateString, false);
                }
                else
                {
                    UpdateToggleStatus(newSelectionEvent.newValue? activeStateString : inactiveStateString, newSelectionEvent.newValue);
                }
            }
        }
    
        private void UpdateToggleStatus(string toggleText, bool toggleValue)
        {
            text = toggleText;
            value = toggleValue;
        }
    }


    [EditorToolbarElement(id, typeof(SceneView))]
    internal class CombinableToggle : EditorToolbarToggle, IAccessContainerWindow
    {
        private static string defaultString = "   -";
        private static string activeStateString = "   Combinable";
        private static string inactiveStateString = "   Not Combinable";
        public const string id = "TrainAR/CombinableToggle";
        public EditorWindow containerWindow { get; set; }
    
        /// <summary>
        /// Constructor of the toggle
        /// </summary>
        public CombinableToggle()
        {
            text = defaultString;
            tooltip = "Toggles whether a TrainAR Object is combinable by the TrainAR interaction concept or not";
        
            //Update this when the selection changed or the user undid/redid actions
            Selection.selectionChanged += HierarchySelectionChangedCombinableToggle;
            Undo.undoRedoPerformed += HierarchySelectionChangedCombinableToggle;
            
            this.RegisterValueChangedCallback(ToggleObjectVisibilityCombinableToggle);
        }

        private void HierarchySelectionChangedCombinableToggle()
        {
            //Ensure there is only one object selected
            if (Selection.gameObjects.Length <= 0)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (Selection.gameObjects.Length < 1)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (!Selection.gameObjects[0].CompareTag("TrainARObject"))
            {
                UpdateToggleStatus(defaultString, false);
            }
            else
            {
                if (Selection.gameObjects[0].GetComponent<TrainARObject>().isCombineable)
                {
                    UpdateToggleStatus(activeStateString, true);
                }
                    
                else
                {
                    UpdateToggleStatus(inactiveStateString, false);
                }
            }
        }

        private void ToggleObjectVisibilityCombinableToggle(ChangeEvent<bool> newSelectionEvent)
        {
            if (Selection.gameObjects.Length <= 0)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (Selection.gameObjects.Length < 1)
            {
                UpdateToggleStatus(defaultString, false);
            }
            else if (!Selection.gameObjects[0].CompareTag("TrainARObject"))
            {
                UpdateToggleStatus(defaultString, false);
            }
            else
            {
                if (newSelectionEvent.newValue &&  !Selection.gameObjects[0].GetComponent<TrainARObject>().isCombineable)
                {
                    //Register changes on the undo stack
                    Undo.RegisterFullObjectHierarchyUndo(Selection.gameObjects[0], "Toggle Combinability");
                    
                    Selection.gameObjects[0].GetComponent<TrainARObject>().isCombineable = true;
                    UpdateToggleStatus(activeStateString, true);
                }
                else if(!newSelectionEvent.newValue &&  Selection.gameObjects[0].GetComponent<TrainARObject>().isCombineable)
                {
                    //Register changes on the undo stack
                    Undo.RegisterFullObjectHierarchyUndo(Selection.gameObjects[0], "Toggle Combinability");
                    
                    Selection.gameObjects[0].GetComponent<TrainARObject>().isCombineable = false;
                    UpdateToggleStatus(inactiveStateString, false);
                }
                else
                {
                    UpdateToggleStatus(newSelectionEvent.newValue? activeStateString : inactiveStateString, newSelectionEvent.newValue);
                }
            }
        }
    
        private void UpdateToggleStatus(string toggleText, bool toggleValue)
        {
            text = toggleText;
            value = toggleValue;
        }
    }
}
