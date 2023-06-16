using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace Remote
{
    /// <summary>
    /// Handles the download of a TrainAR scenario from a server.
    /// </summary>
    public class DownloadTrainARScenario : MonoBehaviour
    {
        /// <summary>
        /// Reference to the list manager to add a training.
        /// </summary>
        [SerializeField]
        private ScenarioListManager scenarioManager;
        /// <summary>
        /// Reference to the server url input field.
        /// </summary>
        [SerializeField]
        private TMP_InputField serverURLInputField;

        /// <summary>
        /// Checks if the scenario is already downloaded and if not initalises the download.
        /// </summary>
        public async void DownloadScenarioFromServer()
        {
            string scenarioServerPath = serverURLInputField.text;
            Debug.Log(scenarioServerPath.Length);
            scenarioServerPath = scenarioServerPath.TrimEnd();
            scenarioServerPath = scenarioServerPath.TrimStart();
            Debug.Log(scenarioServerPath.Length);
            Debug.Log("https://raw.githubusercontent.com/Shilaila/WhatsUpWithAdressables/main/TrainARScenario.trainar".Length);
            if (scenarioManager.localScenarios.Contains(Path.GetFileNameWithoutExtension(scenarioServerPath)))
            {
                Debug.Log("Already downloaded " + Path.GetFileNameWithoutExtension(scenarioServerPath));
                return;
            }
            LoadAndSafeScenario(scenarioServerPath);
            scenarioManager.CreateListOfSavedScenarios();
            await Task.Yield();
        }
        /// <summary>
        /// Download a trainar scenario from a server and save it in the application data.
        /// </summary>
        /// <param name="path">Path to the file on the server.</param>
        public async void LoadAndSafeScenario(string path)
        {
            Debug.Log("Start downloading scenario at path " + path);
            UnityWebRequest www = UnityWebRequest.Get(path);
            www.SendWebRequest();
            while (!www.isDone)
            {
                Debug.Log(www.downloadProgress * 100);
                await Task.Yield();
            }
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                return;
            }
            FileStream safeDownload = new FileStream(Application.persistentDataPath + "/" + Path.GetFileName(path), FileMode.Create);
            safeDownload.Write(www.downloadHandler.data);
            safeDownload.Close();
            await Task.Yield();
            ZipFile.ExtractToDirectory(Application.persistentDataPath + "/" + Path.GetFileName(path), Application.persistentDataPath + "/" + Path.GetFileNameWithoutExtension(path));
            File.Delete(Application.persistentDataPath + "/" + Path.GetFileName(path));
            await Task.Yield();
        }
    }
}