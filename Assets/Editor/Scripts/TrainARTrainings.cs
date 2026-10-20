using System.Collections.Generic;
using System.IO;
using System.Linq;
using Others;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Scripts
{
    /// <summary>
    /// Where the trainings live and which one is edited in the authoring tool. Every training has its own folder
    /// Assets/Trainings/&lt;Name&gt;/ with &lt;Name&gt;.unity, &lt;Name&gt;_Statemachine.asset and &lt;Name&gt;_Info.asset.
    /// </summary>
    public static class TrainARTrainings
    {
        /// <summary>
        /// The folder that holds one folder per training.
        /// </summary>
        public const string Root = "Assets/Trainings";
        /// <summary>
        /// The training the authoring tool opens when nothing else was chosen.
        /// </summary>
        public const string DefaultTraining = "Coffee";
        private const string CurrentKey = "TrainAR.CurrentTraining";

        /// <summary>
        /// The training that is edited in the authoring tool (remembered per user, falls back to the default).
        /// </summary>
        public static string Current
        {
            get
            {
                var name = EditorPrefs.GetString(CurrentKey, DefaultTraining);
                return File.Exists(ScenePath(name)) ? name : DefaultTraining;
            }
            set => EditorPrefs.SetString(CurrentKey, value);
        }

        public static string Folder(string name) => $"{Root}/{name}";
        public static string ScenePath(string name) => $"{Root}/{name}/{name}.unity";
        public static string StatemachinePath(string name) => $"{Root}/{name}/{name}_Statemachine.asset";
        public static string InfoPath(string name) => $"{Root}/{name}/{name}_Info.asset";
        public static string PreviewPath(string name) => $"{Root}/{name}/{name}_Preview.png";

        /// <summary>
        /// All trainings: every folder in Assets/Trainings that contains its scene, sorted by name.
        /// </summary>
        public static List<string> All()
        {
            if (!Directory.Exists(Root)) return new List<string>();
            return Directory.GetDirectories(Root).Select(Path.GetFileName).Where(n => File.Exists(ScenePath(n)))
                .OrderBy(n => n).ToList();
        }

        /// <summary>
        /// All trainings in the order of the home screen: the examples first (Coffee, then the Four-Stroke Engine), then
        /// every other training in the order it was created (new trainings are added at the end).
        /// </summary>
        public static List<string> Ordered() =>
            All().OrderBy(n => InfoFor(n).created).ThenBy(n => n == DefaultTraining ? 0 : 1).ThenBy(n => n).ToList();

        /// <summary>
        /// Whether a scene path is the scene of a training.
        /// </summary>
        public static bool IsTrainingScene(string scenePath) =>
            All().Any(n => ScenePath(n) == scenePath);

        public const string HomeScene = "Assets/Scenes/Home.unity";
        public const string OnboardingScene = "Assets/Scenes/Tutorial.unity";
        private const string TemplateScene = "Assets/Editor/TrainingTemplate/Template.unity";
        private const string TemplateStatemachine = "Assets/Editor/TrainingTemplate/Template_Statemachine.asset";
        private const string CatalogPath = "Assets/Resources/TrainARTrainings.asset";
        private const string DefaultExpert = "Assets/Sprites/Icons/Expert Icon.png";

        /// <summary>
        /// A free folder name for a training's display name (letters and digits only, numbered if taken).
        /// </summary>
        public static string FolderNameFor(string displayName)
        {
            var baseName = new string((displayName ?? "").Where(char.IsLetterOrDigit).ToArray());
            if (baseName.Length == 0) baseName = "Training";
            var name = baseName;
            for (var i = 2; AssetDatabase.IsValidFolder(Folder(name)); i++) name = baseName + i;
            return name;
        }

        /// <summary>
        /// Creates an empty training: the framework scene without objects and a statemachine with only the start and
        /// end events.
        /// </summary>
        public static string Create(string displayName)
        {
            var name = FolderNameFor(displayName);
            CreateFrom(name, TemplateScene, TemplateStatemachine, null, displayName);
            return name;
        }

        /// <summary>
        /// Duplicates a training (scene, statemachine and info; the copy keeps using the original's models).
        /// </summary>
        public static string Duplicate(string source, string displayName)
        {
            var name = FolderNameFor(displayName);
            CreateFrom(name, ScenePath(source), StatemachinePath(source), InfoPath(source), displayName);
            return name;
        }

        private static void CreateFrom(string name, string scene, string statemachine, string info, string displayName)
        {
            AssetDatabase.CreateFolder(Root, name);
            AssetDatabase.CopyAsset(scene, ScenePath(name));
            AssetDatabase.CopyAsset(statemachine, StatemachinePath(name));
            if (info != null) AssetDatabase.CopyAsset(info, InfoPath(name));
            var opened = EditorSceneManager.OpenScene(ScenePath(name), OpenSceneMode.Single);
            var graph = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(StatemachinePath(name));
            foreach (var machine in Object.FindObjectsByType<ScriptMachine>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                machine.nest.macro = graph;
            EditorSceneManager.MarkSceneDirty(opened);
            EditorSceneManager.SaveScene(opened);
            var created = InfoFor(name);
            created.displayName = displayName;
            created.preview = null;
            created.created = System.DateTime.UtcNow.Ticks;
            Current = name;                                     // its scene is open now
            EditorUtility.SetDirty(created);
            Sync();
            ShowInProjectWindow(name);
        }

        /// <summary>
        /// Shows the training's folder in the Project window (by pinging its scene, which opens the folder and
        /// highlights the scene without changing the selection).
        /// </summary>
        public static void ShowInProjectWindow(string name)
        {
            // after a layout change the Project window only exists in the next editor update
            EditorApplication.delayCall += () =>
            {
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath(name));
                if (scene != null) EditorGUIUtility.PingObject(scene);
            };
        }

        /// <summary>
        /// Deletes a training's folder (scene, statemachine, info and everything else in it).
        /// </summary>
        public static void Delete(string name)
        {
            // never delete a training whose scene is open: switch to another training first (else Unity saves it again)
            var open = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                .Any(s => s.path.StartsWith(Folder(name) + "/"));
            if (open)
            {
                var other = All().FirstOrDefault(n => n != name);
                if (other != null) EditorSceneManager.OpenScene(ScenePath(other), OpenSceneMode.Single);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
            AssetDatabase.DeleteAsset(Folder(name));
            if (EditorPrefs.GetString(CurrentKey, DefaultTraining) == name) Current = DefaultTraining;
            Sync();
        }

        /// <summary>
        /// The training's info asset; created with defaults (name, default expert) if it is missing.
        /// </summary>
        public static TrainingInfo InfoFor(string name)
        {
            var info = AssetDatabase.LoadAssetAtPath<TrainingInfo>(InfoPath(name));
            if (info == null)
            {
                info = ScriptableObject.CreateInstance<TrainingInfo>();
                info.displayName = name;
                info.description = "In this training, you do X, Y and Z.";
                info.expert = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultExpert);
                AssetDatabase.CreateAsset(info, InfoPath(name));
            }
            if (info.sceneName != name)
            {
                info.sceneName = name;
                EditorUtility.SetDirty(info);
            }
            return info;
        }

        /// <summary>
        /// Whether the home screen shows the TrainAR footer (links, creator, funding, contact) below the trainings.
        /// </summary>
        public static bool ShowHomeFooter
        {
            get
            {
                var catalog = AssetDatabase.LoadAssetAtPath<TrainingCatalog>(CatalogPath);
                return catalog == null || catalog.showFooter;
            }
            set
            {
                Sync();
                var catalog = AssetDatabase.LoadAssetAtPath<TrainingCatalog>(CatalogPath);
                catalog.showFooter = value;
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Brings the runtime catalog (Resources/TrainARTrainings.asset) and the build scenes (home screen,
        /// onboarding, every training) in line with the training folders.
        /// </summary>
        public static void Sync()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            var catalog = AssetDatabase.LoadAssetAtPath<TrainingCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<TrainingCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            var names = Ordered();
            catalog.trainings = names.Select(InfoFor).ToList();
            EditorUtility.SetDirty(catalog);

            var scenes = new List<EditorBuildSettingsScene>();
            if (File.Exists(HomeScene)) scenes.Add(new EditorBuildSettingsScene(HomeScene, true));
            scenes.Add(new EditorBuildSettingsScene(OnboardingScene, true));
            scenes.AddRange(names.Select(n => new EditorBuildSettingsScene(ScenePath(n), true)));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }
    }
}
