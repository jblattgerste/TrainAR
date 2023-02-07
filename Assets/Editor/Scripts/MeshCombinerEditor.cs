using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Others;

namespace Editor.Scripts
{
	/// <summary>
	/// The MeshCombinerEditor is a utility class that helps to combine meshes.
	/// </summary>
	[CustomEditor(typeof(MeshCombiner))]
	public class MeshCombinerEditor : UnityEditor.Editor
	{
		/// <summary>
		/// The inspector of the MeshCombinerEditor adds the option to reference meshes which are skipped in the combine process.
		/// The combined mesh is stored in the location which is defined in this inspector.
		/// </summary>
		public override void OnInspectorGUI()
		{
			MeshCombiner meshCombiner = (MeshCombiner)target;
			Mesh mesh = meshCombiner.GetComponent<MeshFilter>().sharedMesh;

			#region Script:
			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MeshCombiner)target), typeof(MeshCombiner), false);
			GUI.enabled = true;
			#endregion Script.

			#region MeshFiltersToSkip array:
			SerializedProperty meshFiltersToSkip = serializedObject.FindProperty("meshFiltersToSkip");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(meshFiltersToSkip, true);
			if(EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}
			#endregion MeshFiltersToSkip array.

			#region Button which combine Meshes into one Mesh & Toggles with combine options:
			// Button:
			if(GUILayout.Button("Combine Meshes"))
			{
				meshCombiner.CombineMeshes(true);
			}

			// Toggles:
			meshCombiner.CreateMultiMaterialMesh = GUILayout.Toggle(meshCombiner.CreateMultiMaterialMesh, "Create Multi-Material Mesh");
			meshCombiner.CombineInactiveChildren = GUILayout.Toggle(meshCombiner.CombineInactiveChildren, "Combine Inactive Children");

			meshCombiner.DeactivateCombinedChildren = GUILayout.Toggle(meshCombiner.DeactivateCombinedChildren, "Deactivate Combined Children");
			meshCombiner.DeactivateCombinedChildrenMeshRenderers = GUILayout.Toggle(meshCombiner.DeactivateCombinedChildrenMeshRenderers,
				"Deactivate Combined Children's MeshRenderers");

			meshCombiner.GenerateUVMap = GUILayout.Toggle(meshCombiner.GenerateUVMap, new GUIContent("Generate UV Map", "It is a slow operation that "+
				"generates a UV map (required for the lightmap).\n\nCan be used only in the Editor."));

			// The last (6) "Destroy Combined Children" Toggle:
			GUIStyle style = new GUIStyle(EditorStyles.toggle);
			if(meshCombiner.DestroyCombinedChildren)
			{
				style.onNormal.textColor = new Color(1, 0.15f, 0);
			}
			meshCombiner.DestroyCombinedChildren = GUILayout.Toggle(meshCombiner.DestroyCombinedChildren,
				new GUIContent("Destroy Combined Children", "In the editor this operation can NOT be undone!\n\n"+
				                                            "If you want to bring back destroyed GameObjects, you have to load again the scene without saving."), style);
			#endregion Button which combine Meshes into one Mesh & Toggles with combine options.

			#region Path to the folder where combined Meshes will be saved:
			// Create Labels:
			GUILayout.Label("");
			GUILayout.Label(new GUIContent("Folder path:", "Folder path to save combined Mesh."));

			// Create style wherein text color will be red if folder path is not valid:
			style = new GUIStyle(EditorStyles.textField);
			bool isValidPath = IsValidPath(meshCombiner.FolderPath);
			if(!isValidPath)
			{
				style.normal.textColor = Color.red;
				style.focused.textColor = Color.red;
			}

			// Create TextField with custom style:
			meshCombiner.FolderPath = EditorGUILayout.TextField(meshCombiner.FolderPath, style);
			#endregion Path to the folder where combined Meshes will be saved.

			#region Button which save/show combined Mesh:
			bool meshIsSaved = (mesh != null && AssetDatabase.Contains(mesh));
			GUI.enabled = (mesh != null && (isValidPath || meshIsSaved)); // Valid path is required for not saved Mesh.
			string saveMeshButtonText = (meshIsSaved) ? "Show Saved Combined Mesh" : "Save Combined Mesh";

			if(GUILayout.Button(saveMeshButtonText))
			{
				meshCombiner.FolderPath = SaveCombinedMesh(mesh, meshCombiner.FolderPath);
			}
			GUI.enabled = true;
			#endregion Button which save/show combined Mesh.
		}
		/// <summary>
		/// Checks if the given string contains following character "[:*?\"<>|] and returns false if it does."
		/// </summary>
		/// <param name="folderPath">Any string to check for characters</param>
		/// <returns>Is true when the string doesn't include the characters "[:*?\"<>|]". </returns>
		private bool IsValidPath(string folderPath)
		{
			string pattern = "[:*?\"<>|]"; // Prohibited characters.
			Regex regex = new Regex(pattern);
			return (!regex.IsMatch(folderPath));
		}
		/// <summary>
		/// Saves the given mesh at the given location.
		/// Creates the directories if the given path doesn't exits.
		/// </summary>
		/// <param name="mesh">Any to store mesh.</param>
		/// <param name="folderPath">The path where to store the given mesh.</param>
		/// <returns>The path where the mesh is stored</returns>
		public static string SaveCombinedMesh(Mesh mesh, string folderPath)
		{
			bool meshIsSaved = AssetDatabase.Contains(mesh); // If is saved then only show it in the project view.

			#region Create directories if Mesh and path doesn't exists:
			folderPath = folderPath.Replace('\\', '/');
			if(!meshIsSaved && !AssetDatabase.IsValidFolder("Assets/"+folderPath))
			{
				string[] folderNames = folderPath.Split('/');
				folderNames = folderNames.Where((folderName) => !folderName.Equals("")).ToArray();
				folderNames = folderNames.Where((folderName) => !folderName.Equals(" ")).ToArray();

				folderPath = "/"; // Reset folder path.
				for(int i = 0; i < folderNames.Length; i++)
				{
					folderNames[i] = folderNames[i].Trim();
					if(!AssetDatabase.IsValidFolder("Assets"+folderPath+folderNames[i]))
					{
						string folderPathWithoutSlash = folderPath.Substring(0, folderPath.Length-1); // Delete last "/" character.
						AssetDatabase.CreateFolder("Assets"+folderPathWithoutSlash, folderNames[i]);
					}
					folderPath += folderNames[i]+"/";
				}
				folderPath = folderPath.Substring(1, folderPath.Length-2); // Delete first and last "/" character.
			}
			#endregion Create directories if Mesh and path doesn't exists.

			#region Save Mesh:
			if(!meshIsSaved)
			{
				string meshPath = "Assets/"+folderPath+"/"+"Converted TrainAR Object Mesh"+".asset";
				int assetNumber = 1;
				while(AssetDatabase.LoadAssetAtPath(meshPath, typeof(Mesh)) != null) // If Mesh with same name exists, change name.
				{
					meshPath = "Assets/"+folderPath+"/"+"Converted TrainAR Object Mesh"+" ("+assetNumber+").asset";
					assetNumber++;
				}

				AssetDatabase.CreateAsset(mesh, meshPath);
				AssetDatabase.SaveAssets();
			}
			#endregion Save Mesh.

			EditorGUIUtility.PingObject(mesh); // Show Mesh in the project view.
			return folderPath;
		}
	}
}
