using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Remote;

namespace Remote
{
    /// <summary>
    /// Creates and manages the list of all installed scenarios on the device.
    /// </summary>
    public class ScenarioListManager : MonoBehaviour
    {
        /// <summary>
        /// Gameobject to copy for each added training.
        /// </summary>
        [SerializeField]
        private GameObject patternToCopy;
        /// <summary>
        /// Gamemobject that holds the ui elemnts of all trainings.
        /// </summary>
        [SerializeField]
        private GameObject trainingsHolderGameobject;
        /// <summary>
        /// Reference to runtimeManager to start a scene.
        /// </summary>
        [SerializeField]
        private Others.ApplicationRuntimeManager runtimeManager;
        /// <summary>
        /// Names of all local scenarios.
        /// </summary>
        [HideInInspector]
        public string[] localScenarios;
        /// <summary>
        /// Starts the inital list.
        /// </summary>
        void Awake()
        {
            localScenarios = CreateListOfSavedScenarios();
            foreach (string scenario in localScenarios)
            {
                CreateTrainingUI(scenario);
            }
        }
        /// <summary>
        /// Deletes a scenario from the list and device.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario to delete.</param>
        public void DeleteScenario(string scenarioName)
        {
            Directory.Delete(Application.persistentDataPath + "/" + scenarioName, true);
            CreateListOfSavedScenarios();
        }
        /// <summary>
        /// Creates the UI element for a training.
        /// </summary>
        /// <param name="name">Name of the training.</param>
        private void CreateTrainingUI(string name)
        {
            GameObject createdTraining = Instantiate(patternToCopy, trainingsHolderGameobject.transform);
            ReferenceHolderTrainingUI references = createdTraining.GetComponent<ReferenceHolderTrainingUI>();
            references.scenarioName.text = name;
            Texture2D loadPreview = new Texture2D(1, 1);
            loadPreview.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/" + name + "/preview.png"));
            references.background.texture = loadPreview;
            references.startScenario.onClick.AddListener(delegate { runtimeManager.SwitchScene(name); });
            references.deleteScenario.onClick.AddListener(delegate { DeleteScenario(name); });
            references.deleteScenario.onClick.AddListener(delegate { Destroy(createdTraining); });
            createdTraining.SetActive(true);
        }
        /// <summary>
        /// Creates a list of all local saved scenarios.
        /// </summary>
        /// <returns>List of all scenarios.</returns>
        public string[] CreateListOfSavedScenarios()
        {
            string[] listOfScenarios = Directory.GetDirectories(Application.persistentDataPath);
            string[] namesOfScenarios = new string[listOfScenarios.Length];
            int count = 0;
            foreach (string path in listOfScenarios)
            {
                namesOfScenarios[count] = Path.GetFileNameWithoutExtension(path);
                Debug.Log(namesOfScenarios[count]);
                count++;
            }
            return namesOfScenarios;
        }
    }
}