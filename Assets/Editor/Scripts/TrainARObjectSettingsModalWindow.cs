using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts
{
    /// <summary>
    /// Instance of the Editor Window with enables the user to specify options for the TrainAR Object and initializes
    /// the conversion process.
    /// </summary>
    public class TrainARObjectSettingsModalWindow : EditorWindow
    {
        private string trainARObjectName = "TrainAR Object Name";
        private float changedQuality = 1.0f;
        private List<Mesh> originalMeshes = new List<Mesh>();
        private GameObject trainARObject;
        private UnityEditor.Editor gameObjectEditor;

        
        void OnEnable()
        {
            // Get the selected TrainAR Object when Editor Window is created
            trainARObject = Selection.activeTransform.gameObject;
            
            // Safe the original Meshfilters
            foreach(MeshFilter meshFilter in trainARObject.GetComponentsInChildren<MeshFilter>())
            {
                originalMeshes.Add(meshFilter.sharedMesh);
            }
            
            // Set the name of the Gameobject as the default TrainAR Object name
            trainARObjectName = trainARObject.gameObject.name;
            
            // Dimensions of window
            maxSize = new Vector2(500,450);
        }

        private void OnDisable()
        {
            if (gameObjectEditor != null)
            {
                DestroyImmediate(gameObjectEditor);
            }
        }

        void OnGUI()
        {
            // Create the field and pass the to be converted object
            trainARObject = (GameObject) EditorGUILayout.ObjectField(trainARObject, typeof(GameObject), true);
            
            // Set background color of the preview window
            GUIStyle bgColor = new GUIStyle {normal = {background = EditorGUIUtility.whiteTexture}};
            // On first pass, create the custom editor with the to be converted TrainAR object
            if (gameObjectEditor == null)
                    gameObjectEditor = UnityEditor.Editor.CreateEditor(trainARObject);
            
            // The interactive preview GUI
            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), bgColor);
            
            EditorGUI.BeginChangeCheck();
            // Set the TrainAR Object name
            GUILayout.Label("TrainAR Object Name: ");
            trainARObjectName = GUILayout.TextField(trainARObjectName, 25);
            //Set the quality based on the slider position
            GUILayout.Label("Mesh Quality: " + Math.Round(changedQuality*100, 2) + " %");
            GUILayout.Label("Vertices: " + CountTotalVertices(trainARObject));
            GUILayout.Label("Triangles: " + CountTotalTriangles(trainARObject));
            changedQuality = GUILayout.HorizontalSlider(changedQuality, 0f, 1.0f, GUILayout.ExpandHeight(true));

            // If the mesh-quality slider has been moved.
            if(EditorGUI.EndChangeCheck())
            {
                // Apply Mesh simplification on the mesh filters of the original selection
                ConvertToTrainARObject.SimplifyMeshes(originalMeshes, trainARObject, changedQuality);
                // Reload Preview View with modified object
                gameObjectEditor.ReloadPreviewInstances();
            }

            // Initializes the conversion process with specified options.
            if (GUILayout.Button("Convert to TrainAR Object"))
            {
                ConvertToTrainARObject.InitConversion(trainARObject, trainARObjectName);
                // Editors created this way need to be destroyed explicitly
                DestroyImmediate(gameObjectEditor);
                Close();
            }
            // Undoes the Mesh Changes and closes the editor window.
            if (GUILayout.Button("Cancel"))
            {
                // Editors created this way need to be destroyed explicitly
                DestroyImmediate(gameObjectEditor);
                // Reapply the meshes with original quality.
                ConvertToTrainARObject.SimplifyMeshes(originalMeshes, trainARObject, 1.0f);
                Close();
            }
        }

        /// <summary>
        /// Returns the sum of total vertices of all mesh filters that are a part of the passed Gameobject
        /// </summary>
        /// <param name="gameObject">The Gameobject whose vertices are to be counted</param>
        /// <returns></returns>
        private int CountTotalVertices(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<MeshFilter>().Sum(mesh => mesh.sharedMesh.vertices.Length);
        }

        /// <summary>
        /// Returns the sum of total triangles of all mesh filters that are a part of the passed Gameobject
        /// </summary>
        /// <param name="gameObject">The Gameobject whose triangles are to be counted</param>
        /// <returns></returns>
        private int CountTotalTriangles(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<MeshFilter>().Sum(mesh => mesh.sharedMesh.triangles.Length / 3);
        }
    }
}
