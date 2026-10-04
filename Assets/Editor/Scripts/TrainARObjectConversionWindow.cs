using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityMeshDecimation;
using UnityMeshDecimation.Internal;
using Mesh = UnityEngine.Mesh;

namespace Editor.Scripts
{
    /// <summary>
    /// Instance of the Editor Window with enables the user to specify options for the TrainAR Object and initializes
    /// the conversion process.
    /// </summary>
    public class TrainARObjectConversionWindow : EditorWindow
    {
        //Shader Selection dropdown, shaderPath references and properties
        private string[] _dropdownShaderOptions = new string[] { "Mesh", "Texture", "Shaded" };
        private int _selectedDropdownShaderIndex = 1;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private Material[] sourceMaterials;
        private Material[] texturePreviewMaterials;
        private Material[] shadedPreviewMaterials;
        private Material wirePreviewMaterial;
        private readonly List<Material> ownedPreviewMaterials = new List<Material>();
        private Mesh wirePreviewMesh;
        private Mesh wireSourceMesh;
        private string[] _dropdownObjectSimplificationAlgorithms = new string[] { "Vcglib Tridecimator", "Quadric Error Metrics" };
        private int _selectedObjectSimplificationAlgorithm = 0;

        //Mesh properties
        private int triangleCount;
        private int vertexCount;
        private int indexCount;
        private Vector3 meshDimensions;

        //GUI elements and variables
        private string trainARObjectName = "TrainAR Object Name";
        private bool pivotWasCentered = false;
        private float changedQuality = 1.0f;
        private int targetMeshPolygons = 0;
        private bool preserveBorderEdges = false;
        private bool preserveSurfaceCurvature = false;
        private bool preserveUVSeamEdges = false;
        private bool preserveUVFoldoverEdges = false;
        private List<Mesh> originalMeshes = new List<Mesh>();
        private GameObject originalTrainARObjectReferenceInScene;
        private GameObject trainARObject;
        private UnityEditor.Editor gameObjectEditor;
        private PreviewRenderUtility previewRendererUtility;
        private Vector2 currentUserOptionsScrollPosition;
        private Quaternion accumulatedRotation = Quaternion.Euler(0, 0, 0);
        private Vector3 accumulatedTranslation = Vector3.zero;
        private bool userIsDragging = false;
        private Vector2 lastMousePosition;
        private float zoomDistance = -10f;
        private int prevButton = -1;

        //Axis lines gizmo
        private GameObject axisLinesObject;
        private bool showGizmoLines = true;
        private bool showPreviewInformation = true;
        private float initialLineWidth = 0.005f;

        // List of all active TrainARObjectConversionWindows to check if the window is already open
        private static List<TrainARObjectConversionWindow> activeWindows = new List<TrainARObjectConversionWindow>();

        void OnEnable()
        {
            if (Selection.activeTransform == null) return;

            // Get the selected TrainAR Object when Editor Window is created
            originalTrainARObjectReferenceInScene = Selection.activeTransform.gameObject;
            trainARObject = GameObject.Instantiate(originalTrainARObjectReferenceInScene);
            trainARObject.transform.position = Vector3.zero;
            trainARObject.hideFlags = HideFlags.None;
            
            // Add the window to the list of active windows
            activeWindows.Add(this);
            
            // Title of the window
            titleContent = new GUIContent("Convert TrainAR Object");

            // Dimensions of window
            minSize = new Vector2(1200, 700);

            // Focus this window
            this.Focus();

            EditorUtility.DisplayProgressBar("Preparing GameObject", "Unpacking the GameObject and combining all meshes...", 0.99f); //At least this is some indication that something is happening...
            //Combine all meshes of the TrainAR Object into one mesh
            trainARObject = ConvertToTrainARObject.CombineMeshes(trainARObject);
            EditorUtility.ClearProgressBar();

            // Safe the original Meshfilters
            foreach (MeshFilter meshFilter in trainARObject.GetComponentsInChildren<MeshFilter>())
            {
                originalMeshes.Add(meshFilter.sharedMesh);
            }

            // Set the name of the original GameObject as the default TrainAR Object name
            trainARObjectName = originalTrainARObjectReferenceInScene.gameObject.name;

            // Load the 3D render preview Utility
            previewRendererUtility = new PreviewRenderUtility();
            InitializePreviewMaterials();
            CreateAxisLines(trainARObject);
            SetupPreviewScene(trainARObject);
            ExtractMeshInfo(trainARObject);
            UpdateGizmoLineWidths();
        }

        private void OnDisable()
        {
            if (wirePreviewMesh != null) DestroyImmediate(wirePreviewMesh);
            foreach (Material material in ownedPreviewMaterials)
            {
                if (material != null) DestroyImmediate(material);
            }
            ownedPreviewMaterials.Clear();

            if (previewRendererUtility != null)
                previewRendererUtility.Cleanup();

            activeWindows.Remove(this);

            if (gameObjectEditor != null)
            {
                DestroyImmediate(gameObjectEditor);
            }

            // Destroy the instantiated trainARObject
            if (trainARObject != null)
            {
                DestroyImmediate(trainARObject);
            }
        }

        void OnGUI()
        {
            if (trainARObject == null || previewRendererUtility == null) return;
            EditorGUI.BeginChangeCheck();

            // On first pass, create the custom editor with the to be converted TrainAR object
            if (gameObjectEditor == null)
                gameObjectEditor = UnityEditor.Editor.CreateEditor(trainARObject);

            // Handle the users mouse movements on the utility window
            HandleMouseInputsOnRenderPreview();

            // Draw the mesh preview onto a black texture on the left side of the preview window
            Rect rect = new Rect(0, 0, base.position.width * 0.75f, base.position.height);
            trainARObject.transform.rotation = accumulatedRotation;
            previewRendererUtility.camera.transform.position = new Vector3(0f, 0f, zoomDistance);
            if (Event.current.type == EventType.Repaint)
                RenderPreview(rect);

            // Call the methods to draw overlay information if activated
            if (showPreviewInformation)
            {
                ExtractMeshInfo(trainARObject);
                DrawMeshDimensions(rect);
                DrawMeshInfo(rect);
            }

            // Construct the options are on the right side of the utility window
            GUILayout.BeginArea(new Rect(base.position.width * 0.75f, 0, base.position.width * 0.25f,
                base.position.height));

            //Preview renderer selection: Texture/Shaded/Mesh
            GUILayout.Space(10);
            GUILayout.Box("Preview", GUILayout.ExpandWidth(true));
            GUILayout.Space(10);
            _selectedDropdownShaderIndex =
                EditorGUILayout.Popup("Preview Mode", _selectedDropdownShaderIndex, _dropdownShaderOptions);
            //GUILayout.Space(10);
            //showGizmoLines = GUILayout.Toggle(showGizmoLines, "Show X/Y/Z axes");
            //showPreviewInformation = GUILayout.Toggle(showPreviewInformation, "Show mesh information");
            axisLinesObject.SetActive(showGizmoLines);
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Reset Camera Position",
                    "Resets the preview utilities camera position, rotation and zoom levels.")))
            {
                ResetPreviewUtilityCamera();
            }

            //TrainAR Options
            GUILayout.Space(20);
            GUILayout.Box("TrainAR Object Conversion", GUILayout.ExpandWidth(true));

            // Set the TrainAR Object name
            GUILayout.Space(10);
            GUILayout.Label("TrainAR Object Name: ", EditorStyles.boldLabel);
            trainARObjectName = GUILayout.TextField(trainARObjectName, 25);
            EditorGUILayout.HelpBox(
                "The unique TrainAR Object name, which is used to reference the object in the TrainAR Stateflow.",
                MessageType.Info);
            GUILayout.Space(20);

            // Move the pivot point of the mesh to its center on button press
            GUILayout.Label("Grabbing Point: ", EditorStyles.boldLabel);
            if (GUILayout.Button(new GUIContent("Move Pivot to Center",
                    "Moves the pivot point of the Mesh to its center")))
            {
                trainARObject.GetComponent<MeshFilter>().sharedMesh =
                    CenterPivot(trainARObject.GetComponent<MeshFilter>().sharedMesh);
                pivotWasCentered = true;
            }

            EditorGUILayout.HelpBox(
                "The \"Pivot Point\" is the point at which TrainAR Objects are grabbed during the training.",
                MessageType.Info);

            // Quality conversion options for the mesh with information to advise the user on what would be good options
            GUILayout.Space(20);
            GUILayout.Label("Object Simplification: ", EditorStyles.boldLabel);
            _selectedObjectSimplificationAlgorithm =
                EditorGUILayout.Popup("Algorithm", _selectedObjectSimplificationAlgorithm, _dropdownObjectSimplificationAlgorithms);
            var verticesCount = CountTotalVertices(trainARObject);
            var polygonCount = CountTotalTriangles(trainARObject);
            
            //Simplification options
            GUILayout.BeginVertical("box"); // Using a box for better visual grouping
            if (_selectedObjectSimplificationAlgorithm == 0) //Vcglib Tridecimator
            {
                //If 0, just put in the meshes current vertice count
                if(targetMeshPolygons == 0)
                    targetMeshPolygons = triangleCount;
                //If the user enters a value, calculate the percentage of the current mesh and display it
                targetMeshPolygons = EditorGUILayout.IntField("Target polygon count", targetMeshPolygons);
                
                //Calculate how much the mesh would be reduced by in percent
                var originalPolygonCount = originalMeshes.Sum(mesh => mesh.triangles.Length / 3);
                var simplificationPercentage = Math.Round((1 - (float)targetMeshPolygons / originalPolygonCount) * 100, 2);
                
                if (targetMeshPolygons <= 0)
                {
                    targetMeshPolygons = 1;
                }
                else if (targetMeshPolygons >= originalPolygonCount)
                {
                    targetMeshPolygons = originalPolygonCount;
                }
                
                GUILayout.Label("Reduction: " + simplificationPercentage + "% (" + targetMeshPolygons + "/" + originalPolygonCount +" polygons)" );
                
                //Simplify the mesh
                if (GUILayout.Button(new GUIContent("Simplify")))
                {
                    trainARObject.GetComponent<MeshFilter>().sharedMesh = ConvertToTrainARObject.SimplifyMeshesUsingTridecimator(originalMeshes[0], targetMeshPolygons);
                    
                    if(pivotWasCentered) //If the pivot was centered before, center it again
                        trainARObject.GetComponent<MeshFilter>().sharedMesh =
                            CenterPivot(trainARObject.GetComponent<MeshFilter>().sharedMesh);
                    ExtractMeshInfo(trainARObject);
                }
            }
            else //Fast Quadric Error Metrics
            {
                var simplificationPercentage = Math.Round((1 - changedQuality) * 100, 2);
                GUILayout.Label("Target mesh quality");
                changedQuality = GUILayout.HorizontalSlider(changedQuality, 0f, 1.0f, GUILayout.ExpandWidth(true), GUILayout.Height(15)); 
                GUILayout.Label("Reduction: " + simplificationPercentage + "%");
                if (GUILayout.Button(new GUIContent("Simplify")))
                {
                    // Apply Mesh simplification on the mesh filters of the original selection
                    ConvertToTrainARObject.SimplifyMeshesUsingQuadrics(originalMeshes, trainARObject, changedQuality,
                        preserveBorderEdges, preserveSurfaceCurvature, preserveUVSeamEdges, preserveUVFoldoverEdges);
                    
                    if(pivotWasCentered) //If the pivot was centered before, center it again
                        trainARObject.GetComponent<MeshFilter>().sharedMesh =
                            CenterPivot(trainARObject.GetComponent<MeshFilter>().sharedMesh);

                    ExtractMeshInfo(trainARObject);
                }
            }
            GUILayout.EndVertical();
            EditorGUILayout.HelpBox(
                "Either simplify the mesh by reducing the number of triangles to the specified number (Tridecimator), or by specifying a quality reduction level (Quadric Error Metrics). In most cases, the Tridecimator algorithm should produce better results. Quadric Error Metrics is faster, but can produce artifacts.",
                MessageType.Info);
            
            /* 
            GUILayout.Space(20);
            advancedQualityOptionstatus =
                EditorGUILayout.Foldout(advancedQualityOptionstatus, "Advanced Quality Options");
            
            if (advancedQualityOptionstatus)
            {
                if (Selection.activeTransform)
                {
                    GUILayout.Space(5);
                    preserveBorderEdges = GUILayout.Toggle(preserveBorderEdges, "Preserve mesh border edges");
                    preserveSurfaceCurvature =
                        GUILayout.Toggle(preserveSurfaceCurvature, "Preserve mesh surface curvature");
                    preserveUVSeamEdges = GUILayout.Toggle(preserveUVSeamEdges, "Preserve UV seam edges");
                    preserveUVFoldoverEdges = GUILayout.Toggle(preserveUVFoldoverEdges, "Preserve UV foldover edges");
                    GUILayout.Space(10);
                }
            }*/
            
            // End the options area
            GUILayout.FlexibleSpace();
            
            // Display a warning if the polygon count is too high
            if (polygonCount > 50000)
            {
                EditorGUILayout.HelpBox(
                    "This object has more than 50.000 polygons! The conversion would be very slow. This object will cause performance problems on a Smartphone.",
                    MessageType.Error);
            }
            else if (polygonCount > 10000)
            {
                EditorGUILayout.HelpBox(
                    "This object has more than 10.000 polygons. It might take some time to convert but is ok for large or detailed objects. TrainAR trainings should not exceed 100.000 polygons in total. Consider Simplification.",
                    MessageType.Warning);
            }
            
            // Continuously update the preview window
            if (EditorGUI.EndChangeCheck())
            {
                // Reload Preview View with modified object
                gameObjectEditor.ReloadPreviewInstances();
            }
            
            // Initializes the conversion process with specified options.
            GUILayout.Space(10);
            GUIStyle convertButtonStyle = new GUIStyle(EditorStyles.miniButton);
            convertButtonStyle.normal.textColor = Color.green;
            if (GUILayout.Button("Convert to TrainAR Object", convertButtonStyle))
            {
                // Destroy the Axes lines before conversion
                trainARObject.transform.Cast<Transform>().ToList().ForEach(child => GameObject.DestroyImmediate(child.gameObject));
                // Preview materials and line topology must never become part of the converted object.
                trainARObject.GetComponent<Renderer>().sharedMaterials = PrepareMaterialsForConversion();
                // Finalize the conversion process
                ConvertToTrainARObject.FinalizeConversion(originalTrainARObjectReferenceInScene,  trainARObject, trainARObjectName);
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
                Close();
            }

            GUILayout.Space(10);
            GUILayout.EndArea();
        }

        /// <summary>
        /// Creates materials once, keeping the source array in the combined mesh's submesh order.
        /// </summary>
        private void InitializePreviewMaterials()
        {
            sourceMaterials = trainARObject.GetComponent<Renderer>().sharedMaterials;
            texturePreviewMaterials = new Material[sourceMaterials.Length];
            shadedPreviewMaterials = new Material[sourceMaterials.Length];
            for (int i = 0; i < sourceMaterials.Length; i++)
            {
                Material source = sourceMaterials[i];
                Material shaded = source != null
                    ? new Material(source)
                    : new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (source != null && (source.shader.name == "Standard" || source.shader.name == "Standard (Specular setup)"))
                {
                    new UnityEditor.Rendering.Universal.StandardUpgrader(source.shader.name).Upgrade(
                        shaded, UnityEditor.Rendering.MaterialUpgrader.UpgradeFlags.None);
                    // URP's Premultiply mode now expects premultiplied source textures. Legacy Standard
                    // instead premultiplied diffuse in the shader and preserved specular highlights.
                    int sourceMode = (int)source.GetFloat("_Mode");
                    if (sourceMode == 2 || sourceMode == 3)
                    {
                        shaded.SetFloat("_Blend", 0); // Alpha
                        shaded.SetFloat("_BlendModePreserveSpecular", sourceMode == 3 ? 1 : 0);
                    }
                    UnityEditor.BaseShaderGUI.SetMaterialKeywords(shaded,
                        UnityEditor.Rendering.Universal.ShaderGUI.LitGUI.SetMaterialKeywords);
                }
                else if (source != null && (source.shader.name == "Unlit/Texture" || source.shader.name == "Unlit/Color"))
                {
                    shaded.shader = Shader.Find("Universal Render Pipeline/Unlit");
                    CopyBaseProperties(source, shaded);
                }
                shadedPreviewMaterials[i] = OwnPreviewMaterial(shaded);

                Material texture = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                CopyBaseProperties(shaded, texture);
                texturePreviewMaterials[i] = OwnPreviewMaterial(texture);
            }
            wirePreviewMaterial = OwnPreviewMaterial(new Material(Shader.Find("Universal Render Pipeline/Unlit")));
        }

        private Material OwnPreviewMaterial(Material material)
        {
            material.hideFlags = HideFlags.HideAndDontSave;
            ownedPreviewMaterials.Add(material);
            return material;
        }

        private static void CopyBaseProperties(Material source, Material destination)
        {
            if (source == null) return;
            string textureProperty = source.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
            if (source.HasProperty(textureProperty))
            {
                destination.SetTexture(BaseMap, source.GetTexture(textureProperty));
                destination.SetTextureScale("_BaseMap", source.GetTextureScale(textureProperty));
                destination.SetTextureOffset("_BaseMap", source.GetTextureOffset(textureProperty));
            }
            string colorProperty = source.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
            if (source.HasProperty(colorProperty))
                destination.SetColor(BaseColor, source.GetColor(colorProperty));
        }

        private void RenderPreview(Rect rect)
        {
            MeshFilter filter = trainARObject.GetComponent<MeshFilter>();
            Renderer renderer = trainARObject.GetComponent<Renderer>();
            Mesh triangleMesh = filter.sharedMesh;
            Material[] materials = renderer.sharedMaterials;
            previewRendererUtility.BeginPreview(rect, GUIStyle.none);
            try
            {
                if (_selectedDropdownShaderIndex == 0)
                {
                    EnsureWirePreviewMesh(triangleMesh);
                    filter.sharedMesh = wirePreviewMesh;
                    wirePreviewMaterial.SetColor(BaseColor, EditorGUIUtility.isProSkin ? Color.white : Color.black);
                    renderer.sharedMaterials = new[] { wirePreviewMaterial };
                }
                else
                {
                    renderer.sharedMaterials = _selectedDropdownShaderIndex == 1
                        ? texturePreviewMaterials : shadedPreviewMaterials;
                }
                previewRendererUtility.Render(allowScriptableRenderPipeline: true);
            }
            finally
            {
                // Mesh statistics, simplification and conversion always operate on triangles.
                filter.sharedMesh = triangleMesh;
                renderer.sharedMaterials = materials;
                GUI.DrawTexture(rect, previewRendererUtility.EndPreview());
            }
        }

        private void EnsureWirePreviewMesh(Mesh source)
        {
            if (wireSourceMesh == source && wirePreviewMesh != null) return;
            if (wirePreviewMesh != null) DestroyImmediate(wirePreviewMesh);

            var edges = new HashSet<ulong>();
            var indices = new List<int>();
            int[] triangles = source.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                AddWireEdge(triangles[i], triangles[i + 1], edges, indices);
                AddWireEdge(triangles[i + 1], triangles[i + 2], edges, indices);
                AddWireEdge(triangles[i + 2], triangles[i], edges, indices);
            }
            wirePreviewMesh = new Mesh
            {
                name = "TrainAR Wire Preview",
                hideFlags = HideFlags.HideAndDontSave,
                indexFormat = source.indexFormat,
                vertices = source.vertices,
                bounds = source.bounds
            };
            wirePreviewMesh.SetIndices(indices, MeshTopology.Lines, 0);
            wireSourceMesh = source;
        }

        private static void AddWireEdge(int a, int b, HashSet<ulong> edges, List<int> indices)
        {
            ulong key = ((ulong)(uint)Mathf.Min(a, b) << 32) | (uint)Mathf.Max(a, b);
            if (!edges.Add(key)) return;
            indices.Add(a);
            indices.Add(b);
        }

        private Material[] PrepareMaterialsForConversion()
        {
            var result = new Material[sourceMaterials.Length];
            var converted = new Dictionary<Material, Material>();
            for (int i = 0; i < result.Length; i++)
            {
                Material source = sourceMaterials[i];
                Material preview = shadedPreviewMaterials[i];
                if (source != null && source.shader == preview.shader)
                {
                    result[i] = source;
                    continue;
                }
                if (source != null && converted.TryGetValue(source, out Material existing))
                {
                    result[i] = existing;
                    continue;
                }

                // New Standard imports get their own URP asset; source assets remain unchanged.
                const string folder = "Assets/Materials/TrainAR Objects";
                if (!AssetDatabase.IsValidFolder(folder))
                    AssetDatabase.CreateFolder("Assets/Materials", "TrainAR Objects");
                Material material = new Material(preview) { hideFlags = HideFlags.None };
                string materialName = source != null ? source.name : "TrainAR Material";
                foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
                    materialName = materialName.Replace(invalid, '_');
                materialName = materialName.Replace('/', '_').Replace('\\', '_');
                material.name = materialName + " URP";
                AssetDatabase.CreateAsset(material, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + material.name + ".mat"));
                result[i] = material;
                if (source != null) converted.Add(source, material);
            }
            AssetDatabase.SaveAssets();
            return result;
        }

        /// <summary>
        /// Resets the preview utility camera to the default position, rotation and zoom level.
        /// </summary>
        private void ResetPreviewUtilityCamera()
        {
            accumulatedRotation = Quaternion.Euler(0, 0, 0);
            trainARObject.transform.rotation = accumulatedRotation;
            if (!trainARObject.TryGetComponent<Renderer>(out Renderer renderer)) return;
            Vector3 center = renderer.bounds.center;
            accumulatedTranslation = -center + trainARObject.transform.position;
            trainARObject.transform.position = accumulatedTranslation;
            zoomDistance = CalculateZoomDistanceBasedOnSize(renderer);
            UpdateGizmoLineWidths();
        }

        /// <summary>
        /// Draws the mesh dimensions onto the preview window.
        /// </summary>
        /// <param name="previewRect">the rect to paint onto</param>
        private void DrawMeshDimensions(Rect previewRect)
        {
            GUIStyle redTextStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.red } };
            GUIStyle greenTextStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.green } };
            GUIStyle blueTextStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.blue } };

            // Offset from the bottom-right corner for the dimensions
            Vector2 offset = new Vector2(10, 20);

            // Draw the dimensions labels
            GUI.Label(new Rect(previewRect.xMax - offset.x - 80, previewRect.yMax - offset.y, 80, 20),
                $"Z: {meshDimensions.z:F2} cm", blueTextStyle);
            GUI.Label(new Rect(previewRect.xMax - offset.x - 80, previewRect.yMax - offset.y * 2, 80, 20),
                $"Y: {meshDimensions.y:F2} cm", greenTextStyle);
            GUI.Label(new Rect(previewRect.xMax - offset.x - 80, previewRect.yMax - offset.y * 3, 80, 20),
                $"X: {meshDimensions.x:F2} cm", redTextStyle);
        }

        /// <summary>
        /// Draws the mesh info (vertices, faces, indices) onto the preview window.
        /// </summary>
        /// <param name="previewRect">the rect to paint onto</param>
        private void DrawMeshInfo(Rect previewRect)
        {
            //Set the text color to white or black depending on the editor skin
            GUIStyle whiteTextStyle = new GUIStyle(GUI.skin.label);
            whiteTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            
            // Offset from the bottom-left corner for the mesh information
            Vector2 offset = new Vector2(10, 20);

            // Draw the mesh information labels

            GUI.Label(new Rect(previewRect.x + offset.x, previewRect.yMax - offset.y, 100, 20),
                $"Indices: {indexCount}", whiteTextStyle);
            GUI.Label(new Rect(previewRect.x + offset.x, previewRect.yMax - offset.y * 2, 100, 20),
                $"Polygons: {triangleCount}", whiteTextStyle);
            GUI.Label(new Rect(previewRect.x + offset.x, previewRect.yMax - offset.y * 3, 100, 20),
                $"Vertices: {vertexCount}", whiteTextStyle);
        }

        /// <summary>
        /// Continuously updates the preview window and the gizmo lines.
        /// </summary>
        private void UpdateGizmoLineWidths()
        {
            if (axisLinesObject == null) return;

            // Adjusting the factor as necessary to get the desired visual effect.
            float scaleFactor = Mathf.Abs(zoomDistance / -10f);

            // Iterate over all child objects with LineRenderer and update the line width.
            foreach (LineRenderer lr in axisLinesObject.GetComponentsInChildren<LineRenderer>())
            {
                lr.startWidth = initialLineWidth * scaleFactor;
                lr.endWidth = initialLineWidth * scaleFactor;
            }
        }

        /// <summary>
        /// Sets up the preview scene by adding the targetObject to the preview scene, positioning the camera and setting the background.
        /// </summary>
        /// <param name="targetObject">the target GameObject</param>
        private void SetupPreviewScene(GameObject targetObject)
        {
            //Set the ambient color and lights for the preview scene
            previewRendererUtility.ambientColor = Color.white;
            
            //Set the background color of the preview scene to transparent
            previewRendererUtility.camera.backgroundColor = new Color(0, 0, 0, 0);
            
            //Set the camera position and clipping planes
            previewRendererUtility.camera.transform.position = new Vector3(0f, 0f, -10f);
            previewRendererUtility.camera.nearClipPlane = 0.01f;
            previewRendererUtility.camera.farClipPlane = 100f;
            
            //Add the target object to the preview scene
            previewRendererUtility.AddSingleGO(targetObject);
            
            // Check if the object has a renderer
            if (!targetObject.TryGetComponent<Renderer>(out Renderer renderer)) return;
            
            // Position the the object in the center of the preview, rotate it to (0,0,0) and zoom in so it fills out the screen
            targetObject.transform.rotation = accumulatedRotation;
            Vector3 center = renderer.bounds.center;
            accumulatedTranslation = -center;
            targetObject.transform.position = accumulatedTranslation;
            zoomDistance = CalculateZoomDistanceBasedOnSize(renderer);
        }

        /// <summary>
        /// Adjust the zoom distance based on the size of the object
        /// </summary>
        /// <param name="renderer">The targetObjects Renderer</param>
        /// <returns></returns>
        private float CalculateZoomDistanceBasedOnSize(Renderer renderer)
        {
            float objectSize = renderer.bounds.extents.magnitude;
            float cameraFieldOfView = previewRendererUtility.camera.fieldOfView;
            return -objectSize / Mathf.Tan(cameraFieldOfView * 0.5f * Mathf.Deg2Rad);
        }

        /// <summary>
        /// Create the axes of the preview gizmos and add them to the preview scene.
        /// This is done by creating a LineRenderer for each axis and direction.
        /// </summary>
        /// <param name="target">The target GameObject</param>
        private void CreateAxisLines(GameObject target)
        {
            axisLinesObject = new GameObject("AxisLines");
            axisLinesObject.transform.SetParent(target.transform, false);
            CreateLineRenderer(axisLinesObject, "XAxis", Color.red, Vector3.right * 10000);
            CreateLineRenderer(axisLinesObject, "XAxis", Color.red, Vector3.left * 10000);
            CreateLineRenderer(axisLinesObject, "YAxis", Color.green, Vector3.up * 10000);
            CreateLineRenderer(axisLinesObject, "YAxis", Color.green, Vector3.down * 10000);
            CreateLineRenderer(axisLinesObject, "ZAxis", Color.blue, Vector3.forward * 10000);
            CreateLineRenderer(axisLinesObject, "ZAxis", Color.blue, Vector3.back * 10000);
        }

        /// <summary>
        /// Creates a LineRenderer with the given parameters. This is used in the CreateAxisLines method to create the axis lines.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <param name="endPosition"></param>
        private void CreateLineRenderer(GameObject parent, string name, Color color, Vector3 endPosition)
        {
            GameObject lineObject = new GameObject(name);
            lineObject.transform.SetParent(parent.transform, false);
            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = initialLineWidth;
            lineRenderer.endWidth = initialLineWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, endPosition);

            // Create a basic material and assign it to the LineRenderer
            Material lineMaterial = OwnPreviewMaterial(new Material(Shader.Find("Universal Render Pipeline/Unlit")));
            lineMaterial.SetColor(BaseColor, color);
            lineRenderer.sharedMaterial = lineMaterial;
        }

        /// <summary>
        /// Extracts the mesh information from the targetObject and stores it in the corresponding variables.
        /// </summary>
        /// <param name="targetObject">The Object with the mesh</param>
        private void ExtractMeshInfo(GameObject targetObject)
        {
            //Return if the mesh doesnt have a meshFilter
            if (!targetObject.TryGetComponent<MeshFilter>(out MeshFilter meshFilter)) return;

            Mesh mesh = meshFilter.sharedMesh;
            triangleCount = mesh.triangles.Length / 3;
            vertexCount = mesh.vertices.Length;
            indexCount = mesh.triangles.Length;
            meshDimensions = mesh.bounds.size * 100;
        }

        /// <summary>
        /// Recalculates the pivot point of the mesh to the center of the bounding box and returns the new mesh.
        /// </summary>
        /// <param name="originalMesh">Mesh to center the pivot point for</param>
        /// <returns>The mesh with the centered pivot point</returns>
        private static Mesh CenterPivot(Mesh originalMesh)
        {
            // Copy the mesh if if exits
            if (originalMesh == null)
            {
                Debug.LogWarning("Provided mesh is null.");
                return null;
            }

            Mesh centeredMesh = Mesh.Instantiate(originalMesh);

            // Compute the bounding box of the mesh and get the center of the bounding box.
            Bounds bounds = centeredMesh.bounds;
            Vector3 center = bounds.center;

            // Update each vertex of the mesh by subtracting the center from them
            // This should set the pivot point to the center similarly to the scene view pivot/center option
            Vector3[] vertices = centeredMesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] -= center;
            }

            centeredMesh.vertices = vertices;

            // Recalculate the bounding box after changing vertices.
            centeredMesh.RecalculateBounds();

            // Return the new mesh
            return centeredMesh;
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
            return activeWindows.Any(window => ReferenceEquals(window.trainARObject, gameObject));
        }


        /// <summary>
        /// Handles the mouse inputs on the render preview to rotate, translate and zoom the objects.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The triggered event is unknown.</exception>
        private void HandleMouseInputsOnRenderPreview()
        {
            Event currentEvent = Event.current;

            //Return if we are hovering over the options area
            Rect previewRect = new Rect(0, 0, base.position.width * 0.75f, base.position.height);
            if (!previewRect.Contains(currentEvent.mousePosition)) return;

            //If we dont hover over the options area handle the users input
            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    if (prevButton >= 0)
                    {
                        userIsDragging = false;
                    }
                    else if (currentEvent.button is 0 or 1)
                    {
                        userIsDragging = true;
                        lastMousePosition = currentEvent.mousePosition;
                        prevButton = currentEvent.button;
                    }

                    break;
                case EventType.MouseUp:
                    if (currentEvent.button is 0 or 1)
                    {
                        userIsDragging = false;
                        prevButton = -1;
                    }

                    break;
                case EventType.MouseDrag: //Dragging means we are interacting the the preview
                    if (userIsDragging && prevButton == currentEvent.button)
                    {
                        Vector2 delta = currentEvent.mousePosition - lastMousePosition;
                        lastMousePosition = currentEvent.mousePosition;
                        switch (currentEvent.button)
                        {
                            case 0: // Left button for rotation
                                Quaternion horizontalRotation = Quaternion.AngleAxis(-delta.x, Vector3.up);
                                Quaternion verticalRotation = Quaternion.AngleAxis(-delta.y, Vector3.right);
                                accumulatedRotation = horizontalRotation * verticalRotation * accumulatedRotation;
                                Repaint();
                                break;
                            case 1: // Right button for translation
                                accumulatedTranslation += new Vector3(delta.x, -delta.y, 0) * 0.001f;
                                trainARObject.transform.position = accumulatedTranslation;
                                Repaint();
                                break;
                        }
                    }

                    break;
                case EventType.ScrollWheel: // Handles zooming until we reach the objects origin
                    zoomDistance -= currentEvent.delta.y * 0.1f;
                    zoomDistance = Mathf.Clamp(zoomDistance, -100f, -0.01f);
                    UpdateGizmoLineWidths();
                    Repaint();
                    break;
                case EventType.MouseMove:
                    break;
                case EventType.KeyDown:
                    break;
                case EventType.KeyUp:
                    break;
                case EventType.Repaint:
                    break;
                case EventType.Layout:
                    break;
                case EventType.DragUpdated:
                    break;
                case EventType.DragPerform:
                    break;
                case EventType.DragExited:
                    break;
                case EventType.Ignore:
                    break;
                case EventType.Used:
                    break;
                case EventType.ValidateCommand:
                    break;
                case EventType.ExecuteCommand:
                    break;
                case EventType.ContextClick:
                    break;
                case EventType.MouseEnterWindow:
                    break;
                case EventType.MouseLeaveWindow:
                    break;
                case EventType.TouchDown:
                    break;
                case EventType.TouchUp:
                    break;
                case EventType.TouchMove:
                    break;
                case EventType.TouchEnter:
                    break;
                case EventType.TouchLeave:
                    break;
                case EventType.TouchStationary:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
