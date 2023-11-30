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
        //Shader Selection dropdown, shaderPath references and properties
        private Texture storedMainTex;
        private string[] _dropdownShaderOptions = new string[] { "Mesh", "Texture", "Shaded" };
        private readonly string[] _shaderPaths = { "SuperSystems/Wireframe", "Unlit/Texture", "Standard" };
        private int _selectedDropdownShaderIndex = 1;
        private static readonly int WireColor = Shader.PropertyToID("_WireColor");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int WireThickness = Shader.PropertyToID("_WireThickness");
        private static readonly int WireSmoothness = Shader.PropertyToID("_WireSmoothness");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private string[] _dropdownObjectSimplificationAlgorithms = new string[] { "Vcglib Tridecimator", "Quadric Error Metrics" };
        private int _selectedObjectSimplificationAlgorithm = 1;

        //Mesh properties
        private int triangleCount;
        private int vertexCount;
        private int indexCount;
        private Vector3 meshDimensions;

        //GUI elements and variables
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
            // Get the selected TrainAR Object when Editor Window is created
            trainARObject = GameObject.Instantiate(Selection.activeTransform.gameObject);
            trainARObject.transform.position = Vector3.zero;
            trainARObject.hideFlags = HideFlags.HideAndDontSave;

            // Add the window to the list of active windows
            activeWindows.Add(this);

            // Safe the original Meshfilters
            foreach (MeshFilter meshFilter in trainARObject.GetComponentsInChildren<MeshFilter>())
            {
                originalMeshes.Add(meshFilter.sharedMesh);
            }

            // Set the name of the GameObject as the default TrainAR Object name
            trainARObjectName = trainARObject.gameObject.name;

            // Title of the window
            titleContent = new GUIContent("Convert TrainAR Object");

            // Dimensions of window
            minSize = new Vector2(1200, 700);


            // Focus this window
            this.Focus();

            // Load the 3D render preview Utility
            previewRendererUtility = new PreviewRenderUtility();
            CreateAxisLines(trainARObject);
            SetupPreviewScene(trainARObject);
            ExtractMeshInfo(trainARObject);
            UpdateGizmoLineWidths();
        }

        private void OnDisable()
        {
            if (previewRendererUtility != null)
                previewRendererUtility.Cleanup();

            activeWindows.Remove(this);

            if (gameObjectEditor != null)
            {
                DestroyImmediate(gameObjectEditor);
            }
        }

        void OnGUI()
        {
            // On first pass, create the custom editor with the to be converted TrainAR object
            if (gameObjectEditor == null)
                gameObjectEditor = UnityEditor.Editor.CreateEditor(trainARObject);

            // Handle the users mouse movements on the utility window
            HandleMouseInputsOnRenderPreview();

            // Draw the mesh preview onto a black texture on the left side of the preview window
            Rect rect = new Rect(0, 0, base.position.width * 0.75f, base.position.height);
            previewRendererUtility.BeginPreview(rect, previewBackground: GUIStyle.none);
            trainARObject.transform.rotation = accumulatedRotation;
            previewRendererUtility.camera.transform.position = new Vector3(0f, 0f, zoomDistance);
            previewRendererUtility.Render();
            var texture = previewRendererUtility.EndPreview();
            GUI.DrawTexture(rect, texture);

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
            ApplyShaderToTarget(_shaderPaths[_selectedDropdownShaderIndex]);
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
            GUILayout.Space(20);
            if (_selectedObjectSimplificationAlgorithm == 0) //Vcglib Tridecimator
            {
                
            }
            else //Fast Quadric Error Metrics
            {
                var simplificationPercentage = Math.Round((1 - changedQuality) * 100, 2);
                GUILayout.Label("Reduction: " + simplificationPercentage + "%");
                GUILayout.Label("Polygons: ~" + Math.Round(polygonCount*(100-simplificationPercentage)/100));
                changedQuality = GUILayout.HorizontalSlider(changedQuality, 0f, 1.0f, GUILayout.ExpandWidth(true), GUILayout.Height(15)); 
                
                if (GUILayout.Button(new GUIContent("Simplify")))
                {
                    // Apply Mesh simplification on the mesh filters of the original selection
                    ConvertToTrainARObject.SimplifyMeshes(originalMeshes, trainARObject, changedQuality,
                        preserveBorderEdges, preserveSurfaceCurvature, preserveUVSeamEdges, preserveUVFoldoverEdges);

                    ExtractMeshInfo(trainARObject);
                }
            }
            GUILayout.EndVertical();
            
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
            GUILayout.Space(20);
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
            else
            {
                EditorGUILayout.HelpBox(
                    polygonCount +
                    " polygons looks good. Note: It is advised to not exceed 100.000 polygons for the entire training. Polygon counts >500.000 will cause performance problems.",
                    MessageType.Info);
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
                Close();
            }

            GUILayout.Space(10);
            GUILayout.EndArea();
        }

        /// <summary>
        /// Applies the shader with the given shaderPath to the targetObject.
        /// </summary>
        /// <param name="shaderPath"></param>
        private void ApplyShaderToTarget(string shaderPath)
        {
            //If the Object doesnt have a renderer, abort
            if (!trainARObject.TryGetComponent<Renderer>(out Renderer renderer)) return;

            //Search for the shader specified in the shaderPath
            Shader shader = Shader.Find(shaderPath);
            if (shader == null)
            {
                Debug.LogWarning($"Shader '{shaderPath}' not found.");
                return;
            }

            if (renderer.sharedMaterial && renderer.sharedMaterial.HasProperty("_MainTex"))
            {
                storedMainTex = renderer.sharedMaterial.mainTexture;
            }

            //Create a new material which we will use for the preview with the currently selected shader
            Material mat = new Material(shader);

            //Apply the main texture if the shader has this property (e.g. the wireframe shader doesnt)
            if (mat.HasProperty("_MainTex")) mat.SetTexture(MainTex, storedMainTex);

            //Properties used by the wireframe shader
            if (EditorGUIUtility.isProSkin) // If the editor is in dark mode use white wireframe, black otherwise
            {
                if (mat.HasProperty("_WireColor")) mat.SetColor(WireColor, Color.white);
                if (mat.HasProperty("_BaseColor")) mat.SetColor(BaseColor, Color.black); //This is probably going to break at some point because its a bug in the previewUtility/shader that doesnt display the baseColor correctly
            }
            else
            {
                if (mat.HasProperty("_WireColor")) mat.SetColor(WireColor, Color.black);
                if (mat.HasProperty("_BaseColor")) mat.SetColor(BaseColor, Color.white);
            }
            if (mat.HasProperty("_WireThickness")) mat.SetFloat(WireThickness, 1.0f);
            if (mat.HasProperty("_WireSmoothness")) mat.SetFloat(WireSmoothness, 3.0f);

            //Apply the material to the meshs renderer
            renderer.sharedMaterial = mat;
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
            GUIStyle whiteTextStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.white } };

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
            Material lineMaterial = new Material(Shader.Find("Unlit/Color"));
            lineMaterial.color = color;
            lineRenderer.material = lineMaterial;
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