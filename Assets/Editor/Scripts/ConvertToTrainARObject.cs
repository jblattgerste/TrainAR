using Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Others;

namespace Editor.Scripts
{
    /// <summary>
    /// ConvertARInteractable is an Editor script that adds a right-click context menu to GameObjects in the hierarchy named
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

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "The GameObject is selected inside of the Prefab view. Please unpack the Prefab into an active scene before converting it.", "ok");
                return;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(selectedObject))
            {
                EditorUtility.DisplayDialog("Unable to convert to TrainAR Object", "The selected GameObject is part of a prefab. Please unpack the Prefab before converting it.", "ok");
                return;
            }

            //Register an undo action so the conversion can be undone
            Undo.RegisterFullObjectHierarchyUndo(selectedObject, "Convert to TrainAR Object");

            //Enables the read/write of vertices/indeces of shared meshes 
            EnableReadWriteOnMeshes(selectedObject);
            
            //Combine all meshes into one
            GameObject newSelectedObject = CombineMeshes(selectedObject);
            Undo.RegisterCreatedObjectUndo(newSelectedObject.gameObject, "Convert to TrainAR Object");

            //Convert the object to a TrainAR object
            //TrainARObject.cs automatically imports dependencies and all the other necessary scripts when attached
            newSelectedObject.AddComponent<TrainARObject>();
            newSelectedObject.tag = "TrainARObject";

            //Reset the selection to the newly converted GameObject
            Selection.activeTransform = newSelectedObject.transform;
            Selection.selectionChanged.Invoke();
            Debug.Log("Successfully converted GameObject to TrainAR Object.");
        }

        /// <summary>
        /// Combines all meshes of the selected object (e.g. all meshes in child structures) into a singular mesh,
        /// saving it into the Models folder and deleting all the children to make the mesh structure work with TrainAR.
        /// </summary>
        /// <param name="objectToCombineAllMeshesFor">The GameObject which meshes (parent and child) should be combined</param>
        /// <returns></returns>
        private static GameObject CombineMeshes(GameObject objectToCombineAllMeshesFor)
        {
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
