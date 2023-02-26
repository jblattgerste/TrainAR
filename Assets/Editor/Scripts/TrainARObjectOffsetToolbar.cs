using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts
{
    /// <summary>
    /// The TrainARObjectOffsetToolbar is a toolbar in the Scene view when using the TrainAR authoring overlay. It is displayed
    /// instead of the TrainARObjectToolbar when two objects are selected in the editor.
    /// It displays the positional and rotational offsets between the two selected objects, so the user can use them,
    /// e.g. for the FuseObjects functionality of the object helper node. 
    /// </summary>
    [Overlay(typeof(SceneView), "TrainAR Object Offset", defaultDockZone = DockZone.Floating, defaultLayout = Layout.Panel)]
    public class TrainARObjectOffsetToolbar : Overlay
    {
        /// <summary>
        /// the label that contains the positional and rotational offsets
        /// </summary>
        Label label = new Label("No TrainAR Object Selected");

        /// <summary>
        /// Creates the Panel that displays the toolbar
        /// </summary>
        /// <returns></returns>
        public override VisualElement CreatePanelContent()
        {
            return label;
        }

        public override void OnCreated()
        {
            // When selection changes update the offsets
            Selection.selectionChanged += UpdateOffsets;
        }

        public override void OnWillBeDestroyed()
        {
            Selection.selectionChanged -= UpdateOffsets;
        }

        /// <summary>
        /// Called whenever the selection in the TrainAR Editor is changed. Updates the positional and rotational offset
        /// displayed in the toolbar according to the selected objects.
        /// </summary>
        void UpdateOffsets()
        {
            {
                displayed = false;
                // If nothing is selected, return
                if (Selection.activeTransform == null)
                {
                    return;
                }
                // Only show the Toolbar, when exactly two objects are chosen
                if (Selection.gameObjects.Length != 2)
                {
                    return;
                }
                
                // The two selected objects
                Transform firstSelectedGameObject = Selection.gameObjects[0].transform;
                Transform secondSelectedGameObject = Selection.gameObjects[1].transform;
                
                // Are the selected objects actually TrainAR Objects?
                if (!firstSelectedGameObject.CompareTag("TrainARObject") || !secondSelectedGameObject.CompareTag("TrainARObject"))
                {
                    return;
                }
                
                // // If all of the conditions are met, the toolbar is activated and set an adjusted position (bottom left corner, for now)
                displayed = true;
                floatingPosition = new Vector2(10f, 510);
                
                // Calculate rotational offset
                Vector3 rotationOffset = firstSelectedGameObject.eulerAngles - secondSelectedGameObject.eulerAngles;
                
                // Calclulate positional offset
                Vector3 positionOffset = firstSelectedGameObject.position - secondSelectedGameObject.position;
                
                // Update the label
                label.text = $"Position-Offset:\t x: {positionOffset.x} y: {positionOffset.y} z: {positionOffset.z}\n"
                    + $"Rotation-Offset:\t x: {rotationOffset.x} y: {rotationOffset.y} z:{rotationOffset.z}";
            }
        }
    }
}