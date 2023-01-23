using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts
{
    [Overlay(typeof(SceneView), "TrainAR Object Position")]
    public class TrainARObjectPositionToolbar : Overlay
    {
        Label m_Label = new Label("No TrainAR Object Selected");

        public override VisualElement CreatePanelContent()
        {
            return m_Label;
        }

        public override void OnCreated()
        {
            SceneView.duringSceneGui += UpdatePosition;
        }

        public override void OnWillBeDestroyed()
        {
            SceneView.duringSceneGui -= UpdatePosition;
        }

        void UpdatePosition(SceneView sceneView)
        {
            {
                Transform selectedObject = Selection.activeTransform;

                if (selectedObject == null || Selection.activeTransform.gameObject == null)
                {
                    m_Label.text = "No TrainAR Object Selected";
                    return;
                }

                if (Selection.gameObjects.Length > 1)
                {
                    m_Label.text = "More than one Object selected";
                    return;
                }

                if (Selection.count > 1)
                {
                    m_Label.text = "More than one Object selected";
                    return;
                }

                if (!Selection.activeTransform.CompareTag("TrainARObject"))
                {
                    m_Label.text = "No TrainAR Object Selected";
                    return;
                }


                Vector3 selectedObjectTransformPosition = selectedObject.position;
                m_Label.text = $"TrainAR Object Coordinates: x: {selectedObjectTransformPosition.x} y: {selectedObjectTransformPosition.y} z: {selectedObjectTransformPosition.z}";

            }
        }
    }
}