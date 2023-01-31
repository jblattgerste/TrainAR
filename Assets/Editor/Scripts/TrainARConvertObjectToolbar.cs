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
    [Overlay(typeof(SceneView), "Convert to TrainAR Object", true)]
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
            convertButton.text = "Click to Convert";
            convertButton.clicked += ConvertToTrainARObject.AddConvertionContextItem;
            convertButton.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            convertButton.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
            convertButton.style.height = 30;
            return convertButton;
        }

        public override void OnCreated()
        {
            // When selection changes update the offsets
            Selection.selectionChanged += UpdateContent;
            // By default this toolbar is disabled, and instead the EditorTrainARObjectToolbar is shown
            displayed = false;
            Undock();
            collapsed = false;
        }
        
        /// <summary>
        /// Called whenever the selection in the TrainAR Editor is changed. Updates the positional and rotational offset
        /// displayed in the toolbar according to the selected objects.
        /// </summary>
        void UpdateContent()
        {
            Undock();
            {
                // If nothing is selected, return
                if (Selection.activeTransform == null)
                {
                    displayed = false;
                    return;
                }

                if (Selection.gameObjects.Length != 1)
                {
                    displayed = false;
                    return; 
                }

                if (Selection.activeTransform.CompareTag("TrainARObject"))
                {
                    displayed = false;
                    return;
                }

                displayed = true;
            }
        }
    }
}