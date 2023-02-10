using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts
{
    /// <summary>
    /// Displays and initializes the editor toolbar which contains a button that initializes the conversion process
    /// for a TrainAR Object of the currently selected object.
    /// </summary>
    [Overlay(typeof(SceneView), "Object Conversion", defaultDockZone = DockZone.Floating, defaultLayout = Layout.Panel)]
    public class TrainARConvertObjectToolbar : Overlay
    {
        /// <summary>
        /// the label that contains the positional and rotational offsets
        /// </summary>
        Button convertButton = new ToolbarButton();

        /// <summary>
        /// Creates the Panel that displays the toolbar.
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
        /// Called whenever the selection in the TrainAR Editor is changed. Displays or hides the Toolbar, depending
        /// on the Editor-Selection.
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