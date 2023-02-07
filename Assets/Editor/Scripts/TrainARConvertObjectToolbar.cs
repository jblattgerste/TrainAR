using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts
{
    /// <summary>
    /// The TrainARObjectOffsetToolbar is a toolbar in the Scene view when using the TrainAR authoring overlay. It is displayed
    /// instead of the TrainARObjectToolbar when two objects are selected in the editor.
    /// It displays the positional and rotational offsets between the two selected objects, so the user can use them,
    /// e.g. for the FuseObjects functionality of the object helper. 
    /// </summary>
    [Overlay(typeof(SceneView), "Object Conversion", defaultDockZone = DockZone.Floating, defaultLayout = Layout.Panel)]
    public class TrainARConvertObjectToolbar : Overlay
    {
        /// <summary>
        /// the label that contains the positional and rotational offsets
        /// </summary>
        Button convertButton = new ToolbarButton();

        /// <summary>
        /// Creates the Panel that displays the toolbar
        /// </summary>
        /// <returns></returns>
        public override VisualElement CreatePanelContent()
        {
            convertButton.text = "Convert to TrainAR Object";
            convertButton.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            convertButton.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
            convertButton.style.height = 30;
            return convertButton;
        }

        public override void OnCreated()
        {
            // When selection changes update the offsets
            Selection.selectionChanged += UpdateContent;
            // What happens, when the Convert button in the overlay is pressed
            convertButton.clicked += ConvertToTrainARObject.AddConvertionContextItem;
        }

        public override void OnWillBeDestroyed()
        {
            convertButton.clicked -= ConvertToTrainARObject.AddConvertionContextItem;
            Selection.selectionChanged -= UpdateContent;
        }

        /// <summary>
        /// Called whenever the selection in the TrainAR Editor is changed. Updates the positional and rotational offset
        /// displayed in the toolbar according to the selected objects.
        /// </summary>
        void UpdateContent()
        {
            displayed = false;
            
            // check if anything is selected at all
            if (Selection.activeTransform == null)
            {
                return;
            }
            
            // When not exactly one object is selected
            if (Selection.gameObjects.Length != 1)
            {
                return; 
            }
            
            // If the chosen object is not TrainAR Object.
            if (Selection.activeTransform.CompareTag("TrainARObject"))
            {
                return;
            }
            // If all of the conditions are met, the toolbar is activated and set an adjusted position (bottom left corner, for now)
            displayed = true;
            floatingPosition = new Vector2(10f, 510);
        }
    }
}