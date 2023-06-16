using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Remote;

namespace Editor.Scripts
{
    /// <summary>
    /// Handles the export of a trainAR scenario.
    /// </summary>
    public class UploadTrainARScenario : EditorWindow
    {
        /// <summary>
        /// Name of the scenario.
        /// </summary>
        private string scenarioName = "TrainAR Scenario";
        /// <summary>
        /// Description of the scenario.
        /// </summary>
        private string scenarioDescription = "A new TrainAR scenario.";
        /// <summary>
        /// Path where to store the scenario.
        /// </summary>
        public string sceneFolderPath = "Local path to store the scenario.";
        /// <summary>
        /// Reference to the main camera to get the preview image.
        /// </summary>
        private RenderTexture mainCameraRenderer;
        /// <summary>
        /// Stores the preview image.
        /// </summary>
        private Texture2D mainCameraImage;
        /// <summary>
        /// Window handler.
        /// </summary>
        private static List<UploadTrainARScenario> activeWindows = new List<UploadTrainARScenario>();
        /// <summary>
        /// Holder for the stategraph.
        /// </summary>
        public ScriptGraphAsset stategraph;
        /// <summary>
        /// Creates the window.
        /// </summary>
        void OnEnable()
        {
            activeWindows.Add(this);

            // Title of the window
            titleContent = new GUIContent("TrainAR Scenario");
            Camera main = Camera.main;
            int width = main.pixelWidth;
            int height = main.pixelHeight;
            mainCameraRenderer = new RenderTexture(width, height, 24);
            main.targetTexture = mainCameraRenderer;
            RenderTexture.active = mainCameraRenderer;
            main.Render();

            mainCameraImage = new Texture2D(width, height);
            mainCameraImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            mainCameraImage.Apply();
            // Dimensions of window
            minSize = new Vector2(350, 580);
            maxSize = new Vector2(350, 580);
        }


        private void OnDisable()
        {
            activeWindows.Remove(this);
        }
        /// <summary>
        /// Starts the window to describe the scenario and start the export.
        /// </summary>
        void OnGUI()
        {
            // Set background color of the preview window
            GUIStyle bgColor = new GUIStyle { normal = { background = EditorGUIUtility.whiteTexture } };

            // The interactive preview GUI
            //gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), bgColor);
            EditorGUI.BeginChangeCheck();
            // Set the TrainAR Scenario name
            GUILayout.Space(20);
            GUILayout.Label("Scenario Name ", EditorStyles.boldLabel);
            scenarioName = GUILayout.TextField(scenarioName, 25);
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("The scenario name that is shown in the scenario list.", MessageType.Info);

            GUILayout.Label("Scenario Preview ", EditorStyles.boldLabel);
            GUI.DrawTexture(new Rect(30, 130, 300, 150), mainCameraImage, ScaleMode.ScaleToFit, false, 2.0f);

            GUILayout.Space(180);
            EditorGUILayout.HelpBox("A preview Image that is shown in the scenario list.", MessageType.Info);

            GUILayout.Space(10);
            GUILayout.Label("Scenario Description ", EditorStyles.boldLabel);
            scenarioDescription = GUILayout.TextArea(scenarioDescription, 300);

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("The scenario description that is shown in the scnenario list.", MessageType.Info);

            GUILayout.Space(10);
            GUILayout.Label("Path to save the trainar file", EditorStyles.boldLabel);
            sceneFolderPath = GUILayout.TextField(sceneFolderPath, 50);

            //Add a flexible space so the layout works on all displays
            GUILayout.FlexibleSpace();
            // Initializes the conversion process with specified options.
            GUIStyle uploadButtonStyle = new GUIStyle(EditorStyles.miniButton);
            uploadButtonStyle.normal.textColor = Color.green;
            if (GUILayout.Button("Build TrainAR File", uploadButtonStyle))
            {
                CreateSceneFolder(scenarioName, mainCameraImage, scenarioDescription);
                // Editors created this way need to be destroyed explicitly
                //DestroyImmediate(gameObjectEditor);
                List<GameObject> trainarObjects = ListOfTrainARObjects();
                int objectCount = 0;
                string[] trainARObjectNames = new string[trainarObjects.Count()];
                foreach (GameObject trainARObject in trainarObjects)
                {
                    trainARObjectNames[objectCount] = trainARObject.name;
                    CreateTrainARObjectFolder(trainARObject, scenarioName);
                    objectCount++;
                }
                CreateSceneDescriptionFile(new ScenarioInformation(scenarioName, "preview.png", scenarioDescription, trainARObjectNames));
                CreatePreviewImage(scenarioName);
                SaveStatemachineToFolder(scenarioName);
                CreateZipFileForUpload(scenarioName);
                Close();
            }
            // Closes the editor window.
            GUIStyle cancelButtonStyle = new GUIStyle(EditorStyles.miniButton);
            cancelButtonStyle.normal.textColor = Color.red;
            if (GUILayout.Button("Cancel", cancelButtonStyle))
            {
                // Editors created this way need to be destroyed explicitly
                //DestroyImmediate(gameObjectEditor);
                Close();
            }
        }

        public void CreateZipFileForUpload(string scenarioName)
        {
            ZipFile.CreateFromDirectory(sceneFolderPath + "/" + scenarioName, scenarioName + ".trainar");
        }
        /// <summary>
        /// Creates a folder to store all files before exporting.
        /// </summary>
        /// <param name="scenarioName">Name of the scneario.</param>
        /// <param name="previewImage">Name of the preview Image.</param>
        /// <param name="scenarioDescription">Description of the scenario.</param>
        public void CreateSceneFolder(string scenarioName, Texture previewImage, string scenarioDescription)
        {
            Directory.CreateDirectory(sceneFolderPath + "/" + scenarioName);
        }
        /// <summary>
        /// Creates a file with metadata in form of a XML file.
        /// </summary>
        /// <param name="scenarioInformation">The to serialize scenarioInformation.</param>
        public void CreateSceneDescriptionFile(ScenarioInformation scenarioInformation)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ScenarioInformation)); //Create serializer
            FileStream xmlstream = new FileStream(sceneFolderPath + "/" + scenarioInformation.name + "/" + "ScenarioInformation.xml", FileMode.Create); //Create file at this path
            serializer.Serialize(xmlstream, scenarioInformation);//Write the data in the xml file
            xmlstream.Close();//Close the stream
        }
        /// <summary>
        /// Create a preview image and save it in the folder.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        public void CreatePreviewImage(string scenarioName)
        {
            byte[] previewImage = mainCameraImage.EncodeToPNG();
            FileStream pngStream = new FileStream(sceneFolderPath + "/" + scenarioName + "/" + "preview.png", FileMode.Create);
            pngStream.Write(previewImage);
            pngStream.Close();
        }
        /// <summary>
        /// Save the statemachine as a file in the folder.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        public void SaveStatemachineToFolder(string scenarioName)
        {
            File.WriteAllText(sceneFolderPath + "/" + scenarioName + "/" + "statemachine.state", JsonUtility.ToJson(stategraph));
        }
        /// <summary>
        /// Creates a list of all trainar objects in the scene.
        /// </summary>
        /// <returns>List of TrainAR objects.</returns>
        public List<GameObject> ListOfTrainARObjects()
        {
            UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            List<GameObject> objectsInScene = rootObjects.ToList();
            List<GameObject> trainARObjectsInScene = objectsInScene.Where(o => !o.CompareTag("TrainAR")).ToList();
            return trainARObjectsInScene;
        }
        /// <summary>
        /// Create a folder for a TrainAR object and save all needed files for that object in the created folder.
        /// </summary>
        /// <param name="trainARObject">TrainAR object to save</param>
        /// <param name="scenarioName">Name of the scenario.</param>
        public void CreateTrainARObjectFolder(GameObject trainARObject, string scenarioName)
        {
            TrainARObjectValues data = new TrainARObjectValues(trainARObject);
            string path = sceneFolderPath + "/" + scenarioName + "/" + trainARObject.name;
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
            XmlSerializer serializer = new XmlSerializer(typeof(TrainARObjectValues)); //Create serializer
            FileStream xmlstream = new FileStream(path + "/" + trainARObject.name + ".xml", FileMode.Create); //Create file at this path
            serializer.Serialize(xmlstream, data);//Write the data in the xml file
            xmlstream.Close();//Close the stream
            foreach (SerializedMaterial material in data.materials)
            {
                Debug.Log(material.baseMapPath + " kopieren nach " + path + "/" + material.baseMap + ".png");
                FileUtil.CopyFileOrDirectory(material.baseMapPath, path + "/" + material.baseMap + ".png");
                FileUtil.CopyFileOrDirectory(material.metalMapPath, path + "/" + material.metalMap + ".png");
                FileUtil.CopyFileOrDirectory(material.normalMapPath, path + "/" + material.normalMap + ".png");
            }
        }
    }
}