using System.Collections.Generic;
using System.Linq;
using Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityMeshDecimation;
using UnityMeshDecimation.Internal;
using UnityMeshSimplifier;
using Mesh = UnityEngine.Mesh;
using MeshCombiner = Others.MeshCombiner;

namespace Editor.Scripts
{
    /// <summary>
    /// ConvertToTrainARObjects is an Editor script that adds a right-click context menu to GameObjects in the hierarchy named
    /// "Convert to TrainAR Object". When the object is eligible (therefore has a transform, MeshFilter and MeshRenderer), this can
    /// be used to convert GameObjects to TrainAR Objects, where behaviours (e.g. TrainAR Object) are automatically added and the mesh
    /// is combined an simplyfied.
    /// </summary>
    public class ConvertToTrainARObject : UnityEditor.Editor
    {
        /// <summary>
        /// Adds the "Convert to TrainAR Object" menu Item to the context menu in the editor and handles when it was clicked.
        /// </summary>
        [MenuItem("GameObject/Convert to \"TrainAR Object\"", false, -1000)]
        public static void AddConvertionContextItem()
        {
            //Pre-checks to ensure this is something we want to convert
            
            //No object selected
            if (Selection.activeTransform == null || Selection.activeTransform.gameObject == null)
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "No GameObject was selected.", "ok");
                return;
            }
            
            //Store the selection
            GameObject selectedObject = Selection.activeTransform.gameObject;
            
            //Multiple GameObjects were selected
            if (Selection.gameObjects.Length > 1)
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "More than one GameObject was selected.", "ok");
                return;
            }
            
            //Selected object does not have a Transform component (not a gameobject)
            if (selectedObject.GetComponent<Transform>() == null)
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "No GameObject was selected.", "ok");
                return;
            }
            
            //Seleted object has a SkinnedMeshRenderer, which is not supported
            if (selectedObject.GetComponent<SkinnedMeshRenderer>() != null
                || selectedObject.GetComponentInChildren<SkinnedMeshRenderer>() != null)
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "The selected GameObject (or at least one of its children) contains a SkinnedMeshRenderer. This MeshRenderer is not supported, please use a MeshFilter and MeshRenderer.", "ok");
                return;
            }
            
            //Selected object has no MeshRenderer
            if (selectedObject.GetComponent<MeshRenderer>() == null
                && selectedObject.GetComponentInChildren<MeshRenderer>() == null)
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "The selected GameObject (or at least one of its children) does not have a MeshRenderer attached.", "ok");
                return;
            }
            
            //Selected object has no MeshFilter
            if (selectedObject.GetComponent<MeshFilter>() == null
                && selectedObject.GetComponentInChildren<MeshFilter>() == null)
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "The selected GameObject (or at least one of its children) does not have a MeshFilter attached.", "ok");
                return;
            }
            
            //Selected object is part of the framework itself
            if (selectedObject.CompareTag("AR_Assembly") || selectedObject.CompareTag("TrainAR"))
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "The TrainAR framework itself can not be converted to a TrainAR object.", "ok");
                return;
            }
            
            //Selected object is already converted to TrainAR object
            if (selectedObject.GetComponent<TrainARObject>() != null || selectedObject.CompareTag("TrainARObject"))
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "The selected GameObject was already converted and tagged as a TrainAR object.", "ok");
                return;
            }
            
            // Conversion in Prefab-View
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "The GameObject is selected inside of the Prefab view. Please unpack the Prefab into an active scene before converting it.", "ok");
                return;
            }

            if (TrainARObjectConversionWindow.WindowWithObjectAlreadyExists(selectedObject))
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "A conversion process for this Gameobject is already active.", "ok");
                return;
            }
            
            //Register an undo action so the conversion can be undone
            Undo.RegisterFullObjectHierarchyUndo(selectedObject, "Convert to TrainAR Object");
            
            // Create and show the Modal-Window with options for creating a TraiAR Object
            if (CreateInstance(typeof(TrainARObjectConversionWindow)) is TrainARObjectConversionWindow settingsModalWindow) settingsModalWindow.Show();
        }

        /// <summary>
        /// Finalizes the conversion process for the given object, replacing the original object in the scene.
        /// </summary>
        /// <param name="originalObject">The original GameObject in the scene.</param>
        /// <param name="instantiatedPreviewObject">The instantiated copy of the GameObject.</param>
        /// <param name="trainARObjectName">The specified name of the TrainAR Object.</param>
        /// <param name="pivotPointOffset">The offset of the pivot if it was moved (0,0,0 otherwise)</param>
        public static void FinalizeConversion(GameObject originalObject, GameObject instantiatedPreviewObject, string trainARObjectName)
        {
            instantiatedPreviewObject = Instantiate(instantiatedPreviewObject);
            
            // Assuming both original and instantiated objects have a Renderer component in the same structure
            Renderer originalRenderer = originalObject.GetComponent<Renderer>();
            Renderer instantiatedRenderer = instantiatedPreviewObject.GetComponent<Renderer>();

            if (originalRenderer != null && instantiatedRenderer != null)
            {
                // Copy the materials array from the original object to the instantiated object
                instantiatedRenderer.sharedMaterials = originalRenderer.sharedMaterials;
            }
            
            // Convert the instantiated object to a TrainAR object
            instantiatedPreviewObject.AddComponent<TrainARObject>();
            instantiatedPreviewObject.tag = "TrainARObject";

            // Apply the TrainAR Object name
            instantiatedPreviewObject.name = trainARObjectName;

            // Replace original object in the scene with the instantiated object
            if (originalObject != null)
            {
                instantiatedPreviewObject.transform.position = originalObject.transform.position;
                instantiatedPreviewObject.transform.rotation = originalObject.transform.rotation;
                instantiatedPreviewObject.transform.localScale = Vector3.one;
        
                GameObject.DestroyImmediate(originalObject);
            }

            // Reset the selection to the newly converted GameObject
            Selection.activeTransform = instantiatedPreviewObject.transform;
            Selection.selectionChanged.Invoke();
            Debug.Log("Successfully converted GameObject to TrainAR Object.");
        }


        /// <summary>
        /// Combines all meshes of the selected object (e.g. all meshes in child structures) into a singular mesh,
        /// saving it into the Models folder and deleting all the children to make the mesh structure work with TrainAR.
        /// </summary>
        /// <param name="objectToCombineAllMeshesFor">The GameObject which meshes (parent and child) should be combined</param>
        /// <returns></returns>
        public static GameObject CombineMeshes(GameObject objectToCombineAllMeshesFor)
        {
            Undo.RegisterCreatedObjectUndo(objectToCombineAllMeshesFor.gameObject, "Convert to TrainAR Object");
            
            // If selected object is a part of a prefab instance, unpack it completely.
            if (PrefabUtility.IsPartOfPrefabInstance(objectToCombineAllMeshesFor))
            {
                PrefabUtility.UnpackPrefabInstance(objectToCombineAllMeshesFor, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            }

            //Enables the read/write of vertices/indeces of shared meshes 
            EnableReadWriteOnMeshes(objectToCombineAllMeshesFor);
            
            //Create a new empty parent for the combination
            GameObject newGameObjectWithCombinedMeshes = new GameObject(objectToCombineAllMeshesFor.name, typeof(MeshFilter), typeof(MeshRenderer));
            newGameObjectWithCombinedMeshes.transform.SetPositionAndRotation(objectToCombineAllMeshesFor.transform.position, objectToCombineAllMeshesFor.transform.rotation);
            
            //Unpack all children and the original parent into the newly created object as a first generation child
            foreach (var childTransform in objectToCombineAllMeshesFor.GetComponentsInChildren<Transform>(true))
            {
                childTransform.parent = newGameObjectWithCombinedMeshes.transform;
            }

            //Combine all meshes of the children into a singular one in the parent
            MeshCombiner meshCombiner = newGameObjectWithCombinedMeshes.AddComponent<MeshCombiner>();
            meshCombiner.CombineInactiveChildren = false;
            meshCombiner.CreateMultiMaterialMesh = true;
            meshCombiner.GenerateUVMap = true;
            meshCombiner.FolderPath = "Models/";
            meshCombiner.CombineMeshes(false);
            MeshCombinerEditor.SaveCombinedMesh(newGameObjectWithCombinedMeshes.GetComponent<MeshFilter>().sharedMesh, "Models/");
            
            //Cleanup/delete the children of the combined mesh
            foreach (var childTransform in newGameObjectWithCombinedMeshes.GetComponentsInChildren<Transform>(true))
            {
                //Ignore if this was already deleted
                if (childTransform.gameObject == null) continue;
                //Ignore if this is the parent itself
                if (childTransform == newGameObjectWithCombinedMeshes.transform) continue;
                //Destroy this child
                DestroyImmediate(childTransform.gameObject);
            }
            
            //Cleanup the meshCombiner utility script
            DestroyImmediate(meshCombiner);

            //Return the new object as the new selectedObject
            return newGameObjectWithCombinedMeshes;
        }

        /// <summary>
        /// Simplifies one given mesh (careful, this is different to the SimplifyMeshesUsingQuadrics method, which simplifies all meshes of a given object)
        /// to the given target polygon count using the Tridecimator method from the UnityMeshDecimation package.
        /// </summary>
        /// <param name="providedMesh">The Mesh to simplify</param>
        /// <param name="targetMeshPolygons">the target polygon count to simplify to</param>
        /// <returns>The simplified mesh</returns>
        public static Mesh SimplifyMeshesUsingTridecimator(Mesh providedMesh, int targetMeshPolygons)
        {
            var conditions= new TargetConditions
            {
                faceCount = targetMeshPolygons
            };
            var parameter = new EdgeCollapseParameter
            {
                UsedProperty = VertexProperty.UV0
            };
            
            var meshDecimation = new UnityMeshDecimation.UnityMeshDecimation();
            meshDecimation.Execute(providedMesh, parameter, conditions);
            return meshDecimation.ToMesh();
        }

        ///  <summary>
        ///  Uses the Meshsimplifier to decimate the mesh of the passed Gameobject as well as all of it's children's meshes.
        ///
        ///  Note: In the current implementation this only simplifies one single mesh but it works so i leave this as it is here.
        ///  </summary>
        ///  <param name="currentSelectedObject">The current Gameobject, to which the mesh changes are applied to</param>
        ///  <param name="quality">The desired quality of the simplification. Must be between 0 and 1.</param>
        ///  <param name="originalMeshes">The meshes as they were, when the object was originally selected,
        /// before any mesh changes were applied</param>
        ///  <param name="preserveBorderEdges">Optional parameter: Should mesh edges be preserved?</param>
        ///  <param name="preserveSurfaceCurvature">Optional parameter: Should surface curvature be preserved?</param>
        ///  <param name="preserveUVSeamEdges">Optional parameter: Should UV seam edges be preserved?</param>
        ///  <param name="preserveUVFoldoverEdges">Optional parameter: Should UV foldover edges be preserved?</param>
        public static void SimplifyMeshesUsingQuadrics(IEnumerable<Mesh> originalMeshes, GameObject currentSelectedObject, float quality,
            bool preserveBorderEdges = false, bool preserveSurfaceCurvature = false, bool preserveUVSeamEdges = false, bool preserveUVFoldoverEdges = false)
        {
            // Create instance of Unity Mesh Simplifier
            var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
            
            // Get the current Mesh Filters of the Gameobject
            var currentSelectedMeshFilters = currentSelectedObject.GetComponentsInChildren<MeshFilter>();
            
            // Create a tupel of the original meshes and the current meshes. 
            var originalAndCurrent = originalMeshes.Zip(currentSelectedMeshFilters,
                (o, c) => new {Original = o, Current = c});
            
            // Iterate over the original and current meshes.
            foreach (var originalAndCurrenTupel in originalAndCurrent)
            {
                // The original mesh
                Mesh originalMesh = originalAndCurrenTupel.Original;
                
                if (originalMesh == null) // verify that the mesh filter actually has a mesh
                    return;

                // Initialize mesh simplifier with the original mesh
                meshSimplifier.Initialize(originalMesh);
                
                //Set up custom options for the simplification, apply its defaults and then set the values from the the modal window
                SimplificationOptions simplificationOptions = new SimplificationOptions();
                simplificationOptions = SimplificationOptions.Default;
                simplificationOptions.PreserveBorderEdges = preserveBorderEdges;
                simplificationOptions.PreserveSurfaceCurvature = preserveSurfaceCurvature;
                simplificationOptions.PreserveUVSeamEdges = preserveUVSeamEdges;
                simplificationOptions.PreserveUVFoldoverEdges = preserveUVFoldoverEdges;
                meshSimplifier.SimplificationOptions = simplificationOptions;
                

                // Simplifies the mesh according to quality value
                meshSimplifier.SimplifyMesh(quality);

                // Apply the simplified mesh to the GameObject
                originalAndCurrenTupel.Current.mesh = meshSimplifier.ToMesh();
            }
        }

        /// <summary>
        /// Enables the read/write of vertices/indeces of shared meshes (the asset) for all meshes on TrainAR objects.
        /// This is needed for collision detection and outlining
        /// </summary>
        /// <param name="trainARObject">The parent objects (with all its children) that is to be converted</param>
        private static void EnableReadWriteOnMeshes(GameObject trainARObject)
        {
            MeshFilter[] meshFilters = trainARObject.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(meshFilter.sharedMesh)) is ModelImporter
                    modelImporter){
                    modelImporter.isReadable = true;
                    modelImporter.SaveAndReimport();
                }
            }
        }
    }
}
