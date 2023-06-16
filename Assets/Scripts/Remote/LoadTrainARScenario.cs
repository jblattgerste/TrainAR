using Interaction;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Remote;
using Static;

namespace Remote {
    /// <summary>
    /// Scirpt that loads the trainAR scenario from file at the start of the scene.
    /// Starts prefab placement when scenario is loaded.
    /// </summary>
    public class LoadTrainARScenario : MonoBehaviour
    {
        /// <summary>
        /// Refernce to the script machine:
        /// </summary>
        public ScriptMachine scriptMachine;
        /// <summary>
        /// Value that is set to true when all objects are instantiated.
        /// </summary>
        public bool setupDone = false;
        /// <summary>
        /// Starts the loading of the scenario.
        /// </summary>
        private void Awake()
        {
            if (ActiveScenarioInformation.scenarioName.Equals("default"))
            {
                setupDone = true;
                return;
            }
            setupDone = false;
            loadTrainARScenario(ActiveScenarioInformation.scenarioName);
        }
        /// <summary>
        /// Handels the different parts that need to be loaded for given scenario.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario that should be loaded.</param>
        public async void loadTrainARScenario(string scenarioName)
        {
            ScenarioInformation scenarioInformation = await LoadScenarioInformation(scenarioName);
            foreach (string trainARObjectName in scenarioInformation.trainARObjects)
            {
                await BuildTrainARObject(trainARObjectName, scenarioName);
            }
            scriptMachine.nest.SwitchToMacro(await LoadStatemachine(scenarioName));
            setupDone = true;
            await Task.Yield();

        }
        /// <summary>
        /// Loads the statemachine from file.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        /// <returns>Statemachine of the scenario.</returns>
        public async Task<ScriptGraphAsset> LoadStatemachine(string scenarioName)
        {
            StreamReader www = new StreamReader(Application.persistentDataPath + "/" + scenarioName + "/Statemachine.state");
            ScriptGraphAsset data = ScriptableObject.CreateInstance<ScriptGraphAsset>();
            JsonUtility.FromJsonOverwrite(www.ReadToEnd(), data);
            await Task.Yield();
            www.Close();
            return data;
        }
        /// <summary>
        /// Loads the Metafile for scenario.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        /// <returns>Scenarioinformation of the scenario that is loaded.</returns>
        public async Task<ScenarioInformation> LoadScenarioInformation(string scenarioName)
        {
            StreamReader www = new StreamReader(Application.persistentDataPath + "/" + scenarioName + "/ScenarioInformation.xml");
            XmlSerializer serializer = new XmlSerializer(typeof(ScenarioInformation)); //Create serializer
            ScenarioInformation data = serializer.Deserialize(www) as ScenarioInformation;
            www.Close();//Close the stream
            await Task.Yield();
            return data;
        }
        /// <summary>
        /// Loads the Metafile for a TrainAR object.
        /// </summary>
        /// <param name="path">Path where the file is located.</param>
        /// <returns>Struct with TrainAR values.</returns>
        public async Task<TrainARObjectValues> LoadTrainARObjectValuesFile(string path)
        {
            StreamReader www = new StreamReader(path + ".xml");
            XmlSerializer serializer = new XmlSerializer(typeof(TrainARObjectValues)); //Create serializer
            TrainARObjectValues data = serializer.Deserialize(www) as TrainARObjectValues;
            await Task.Yield();
            www.Close();
            return data;
        }
        /// <summary>
        /// Loads Material fpr a TrainAR object.
        /// </summary>
        /// <param name="path">Path where the texture files are located.</param>
        /// <param name="material">Serialized material.</param>
        /// <param name="shader">Name of the material shader.</param>
        /// <returns>Loaded material.</returns>
        public async Task<Material> loadMaterial(string path, SerializedMaterial material, string shader = "Standard")
        {
            Material result = new Material(Shader.Find(shader));
            Texture maintex = await loadTexture(path + material.baseMap + ".png");
            result.SetTexture("_MainTex", maintex);
            Texture normalmap = await loadTexture(path + material.normalMap + ".png");
            result.SetTexture("_BumpMap", normalmap);
            Texture metalmap = await loadTexture(path + material.metalMap + ".png");
            result.SetTexture("_MetallicGlossMap", metalmap);
            return result;
        }
        /// <summary>
        /// Loads a texture from file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>Loaded texture.</returns>
        public async Task<Texture> loadTexture(string path)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(File.ReadAllBytes(path));
            await Task.Yield();
            return texture;
        }
        /// <summary>
        /// Creates a TrainAR Objekt from file.
        /// </summary>
        /// <param name="objectName">Name of the TrainAR Object.</param>
        /// <param name="scenarioName">Name of the scenario.</param>
        /// <returns></returns>
        public async Task<GameObject> BuildTrainARObject(string objectName, string scenarioName)
        {
            GameObject result = new GameObject(objectName);
            TrainARObjectValues trainARObjectValues = await LoadTrainARObjectValuesFile(Application.persistentDataPath + "/" + scenarioName + "/" + objectName + "/" + objectName);
            result.name = trainARObjectValues.name;
            result.transform.position = trainARObjectValues.transform.DeserializedTransform(result.transform).position;
            result.transform.rotation = trainARObjectValues.transform.DeserializedTransform(result.transform).rotation;
            result.transform.localScale = trainARObjectValues.transform.DeserializedTransform(result.transform).localScale;
            result.AddComponent<MeshFilter>();
            result.GetComponent<MeshFilter>().mesh = trainARObjectValues.mesh.DeserializedMesh();
            result.AddComponent<MeshRenderer>();
            Material[] materials = new Material[trainARObjectValues.materials.Length];
            int materialCounter = 0;
            foreach (SerializedMaterial serializedMaterial in trainARObjectValues.materials)
            {
                materials[materialCounter] = await loadMaterial(Application.persistentDataPath + "/" + scenarioName + "/" + objectName + "/", serializedMaterial, serializedMaterial.shader);
                materialCounter++;
            }
            result.GetComponent<MeshRenderer>().materials = materials;
            result.AddComponent<TrainARObject>();
            result.GetComponent<TrainARObject>().setObjectValues(trainARObjectValues.isGrabbable, trainARObjectValues.isInteractable, trainARObjectValues.isCombinable, trainARObjectValues.disabledOnStart, trainARObjectValues.LerpingDistance);

            await Task.Yield();
            return result;
        }
    }
}