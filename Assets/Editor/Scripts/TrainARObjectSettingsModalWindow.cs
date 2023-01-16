using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{
    public class TrainARObjectSettingsModalWindow : EditorWindow
    {
        /// <summary>
        /// Instance of the Editor Window with enables the user to specify options for the TrainAR Object and initializes
        /// the conversion process.
        /// </summary>
        
        private string trainARObjectName = "TrainAR Object Name";
        private float meshQuality = 1.0f;
        private List<Mesh> originalMeshes = new List<Mesh>();
        private GameObject trainARObject;
        
        private void OnEnable()
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
            
            maxSize = new Vector2(500,150);
        }

        void OnGUI()
        {
            // Set the TrainAR Object name
            GUILayout.Label("TrainAR Object Name: ");
            trainARObjectName = GUILayout.TextField(trainARObjectName, 25);
            
            //Set the quality based on the slider position
            GUILayout.Label("Mesh Quality: " + Math.Round(meshQuality*100, 2) + " %");
            float changedQuality = GUILayout.HorizontalSlider(meshQuality, 0f, 1.0f, GUILayout.ExpandHeight(true));
            
            // If the mesh-quality slider has been moved.
            if(Math.Abs(changedQuality - meshQuality) > 0.001)
            {
                Debug.Log("Mesh Quality Changed");
                
                // Apply Mesh simplification on the mesh filters of the original selection
                meshQuality = changedQuality;
                ConvertToTrainARObject.SimplifyMeshes(originalMeshes, trainARObject, meshQuality);
            }
            
            // Initializes the conversion process with specified options.
            if (GUILayout.Button("Convert to TrainAR Object"))
            {   
                ConvertToTrainARObject.InitConversion(trainARObject, trainARObjectName);
                Close();
            }
            
            // Undoes the Mesh Changes and closes the editor window.
            if (GUILayout.Button("Abort"))
            {
                // Reapply the meshes with original quality.
                ConvertToTrainARObject.SimplifyMeshes(originalMeshes, trainARObject, 1.0f);
                Close();
            }

        }
        
    }
}
