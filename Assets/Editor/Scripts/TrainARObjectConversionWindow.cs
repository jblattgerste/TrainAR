using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{
    /// <summary>
    /// Instance of the Editor Window with enables the user to specify options for the TrainAR Object and initializes
    /// the conversion process.
    /// </summary>
    public class TrainARObjectConversionWindow : EditorWindow
    {
        private string trainARObjectName = "TrainAR Object Name";
        private float changedQuality = 1.0f;
        private bool preserveBorderEdges = false;
        private bool preserveSurfaceCurvature = false;
        private bool preserveUVSeamEdges = false;
        private bool preserveUVFoldoverEdges = false;
        private List<Mesh> originalMeshes = new List<Mesh>();
        private GameObject trainARObject;
        private UnityEditor.Editor gameObjectEditor;
        bool advancedQualityOptionstatus = false;
        
        private static List<TrainARObjectConversionWindow> activeWindows = new List<TrainARObjectConversionWindow>();

        void OnEnable()
        {
            // Get the selected TrainAR Object when Editor Window is created
            trainARObject = Selection.activeTransform.gameObject;
            activeWindows.Add(this);
            
            // Safe the original Meshfilters
            foreach(MeshFilter meshFilter in trainARObject.GetComponentsInChildren<MeshFilter>())
            {
                originalMeshes.Add(meshFilter.sharedMesh);
            }
            
            // Set the name of the GameObject as the default TrainAR Object name
            trainARObjectName = trainARObject.gameObject.name;
            
            // Title of the window
            titleContent = new GUIContent("Convert TrainAR Object");
            
            // Dimensions of window
            minSize = new Vector2(350,580);
            maxSize = new Vector2(350,580);
        }

        private void OnDisable()
        {
            activeWindows.Remove(this);
            if (gameObjectEditor != null)
            {
                DestroyImmediate(gameObjectEditor);
            }
        }

        void OnGUI()
        {
            // Set background color of the preview window
            GUIStyle bgColor = new GUIStyle {normal = {background = EditorGUIUtility.whiteTexture}};
            // On first pass, create the custom editor with the to be converted TrainAR object
            if (gameObjectEditor == null)
                    gameObjectEditor = UnityEditor.Editor.CreateEditor(trainARObject);
            
            // The interactive preview GUI
            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), bgColor);
            EditorGUI.BeginChangeCheck();

            // Set the TrainAR Object name
            GUILayout.Space(20);
            GUILayout.Label("TrainAR Object Name: ", EditorStyles.boldLabel);
            trainARObjectName = GUILayout.TextField(trainARObjectName, 25);
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("The unique TrainAR Object name, which is used to reference the object in the TrainAR Stateflow.", MessageType.Info);

            // Quality conversion options for the mesh with information to advise the user on what would be good options
            GUILayout.Space(20);
            GUILayout.Label("Object Quality: ", EditorStyles.boldLabel);
            GUIStyle qualityLabelStyle = new GUIStyle(EditorStyles.label);
            var simplificationPercentage = Math.Round((1 - changedQuality) * 100, 2);
            var verticesCount = CountTotalVertices(trainARObject);
            var polygonCount = CountTotalTriangles(trainARObject);
            if (polygonCount >= 50000) qualityLabelStyle.normal.textColor = Color.red;
            else if (polygonCount >= 10000) qualityLabelStyle.normal.textColor = Color.yellow;
            else qualityLabelStyle.normal.textColor = Color.green;
            GUILayout.Label("Reduction: " + simplificationPercentage + "% = " + "Vertices: " + verticesCount + ", Polygons: " + polygonCount, qualityLabelStyle);
            changedQuality = GUILayout.HorizontalSlider(changedQuality, 0f, 1.0f, GUILayout.ExpandHeight(true));
            
            // Advanced Quality options which replace the hints if the user wants more control over the conversion process
            GUILayout.Space(20);
            advancedQualityOptionstatus = EditorGUILayout.Foldout(advancedQualityOptionstatus, "Advanced Quality Options");
            if (advancedQualityOptionstatus)
            {
                if (Selection.activeTransform)
                {
                    GUILayout.Space(5);
                    preserveBorderEdges = GUILayout.Toggle(preserveBorderEdges,"Preserve mesh border edges");
                    preserveSurfaceCurvature = GUILayout.Toggle(preserveSurfaceCurvature,"Preserve mesh surface curvature");
                    preserveUVSeamEdges = GUILayout.Toggle(preserveUVSeamEdges,"Preserve UV seam edges");
                    preserveUVFoldoverEdges = GUILayout.Toggle(preserveUVFoldoverEdges, "Preserve UV foldover edges");
                    GUILayout.Space(10);
                }
            }
            else
            {
                // If we use the simple overlay without advanced options, show tipts and hints based on the current polygon count
                GUILayout.Space(10);
                if (polygonCount > 50000)
                {
                    EditorGUILayout.HelpBox("This object has more than 50.000 polygons! The conversion would be very slow (or crash the Editor) and it will cause performance problems on a Smartphone.", MessageType.Error);
                }else if (polygonCount > 10000)
                {
                    EditorGUILayout.HelpBox("This object has more than 10.000 polygons. It might take some time to convert but is ok for large or detailed objects. TrainAR trainings should not exceed 100.000 polygons in total.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox(polygonCount + " polygons looks good. Note: It is advised to not exceed 100.000 polygons for the entire training. Polygon counts >500.000 will cause performance problems.", MessageType.Info);
                }
            }
                
            if (!Selection.activeTransform)
            {
                // Set the display status of the advanced options to false
                advancedQualityOptionstatus = false;
            }
            
            //Add a flexible space so the layout works on all displays
            GUILayout.FlexibleSpace();
            
            // If the mesh-quality slider has been moved.
            if(EditorGUI.EndChangeCheck())
            {
                // Apply Mesh simplification on the mesh filters of the original selection
                ConvertToTrainARObject.SimplifyMeshes(originalMeshes, trainARObject, changedQuality,
                    preserveBorderEdges, preserveSurfaceCurvature, preserveUVSeamEdges, preserveUVFoldoverEdges);
                
                // Reload Preview View with modified object
                gameObjectEditor.ReloadPreviewInstances();
            }

            // Initializes the conversion process with specified options.
            GUIStyle convertButtonStyle = new GUIStyle(EditorStyles.miniButton);
            convertButtonStyle.normal.textColor = Color.green;
            if (GUILayout.Button("Convert to TrainAR Object", convertButtonStyle))
            {
                ConvertToTrainARObject.InitConversion(trainARObject, trainARObjectName);
                // Editors created this way need to be destroyed explicitly
                DestroyImmediate(gameObjectEditor);
                Close();
            }
            // Undoes the Mesh Changes and closes the editor window.
            GUIStyle cancelButtonStyle = new GUIStyle(EditorStyles.miniButton);
            cancelButtonStyle.normal.textColor = Color.red;
            if (GUILayout.Button("Cancel", cancelButtonStyle))
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

        /// <summary>
        /// Checks whether or not a TrainARObjectConversionWindow with the given Gameobject is already active.
        /// </summary>
        /// <param name="gameObject">The Gameobject to be checked.</param>
        /// <returns>True if a TrainARObjectConversionWindow with the given Gameobject already exists.
        /// </returns>
        public static bool WindowWithObjectAlreadyExists(GameObject gameObject)
        {
            foreach (TrainARObjectConversionWindow window in activeWindows)
            {
                if(ReferenceEquals(window.trainARObject, gameObject))
                {
                    return true;
                }   
            }
            return false;
        }
    }
}
