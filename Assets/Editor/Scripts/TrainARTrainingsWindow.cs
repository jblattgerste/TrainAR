using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{
    /// <summary>
    /// The TrainAR Trainings tab (docked next to the simulator): one list with a box per training (preview with the round
    /// expert, name, description). Every box can be opened in the authoring tool, edited, duplicated and deleted; new
    /// trainings are added at the bottom. All trainings are part of the build and shown on the home screen; previews are
    /// rendered by every build.
    /// </summary>
    public class TrainARTrainingsWindow : EditorWindow
    {
        private const float PreviewSize = 110, ExpertSize = 58;

        private string editing, editName, editDescription;
        private Sprite editExpert;
        private int pickerId;
        private Vector2 scroll;
        private GUIStyle nameStyle;

        /// <summary>
        /// Shows the trainings tab (docked next to the simulator if it is not part of the layout yet).
        /// </summary>
        [MenuItem("TrainAR/Options/Show Trainings List", false, 900)]
        public static void Open()
        {
            var simulator = SimulatorWindowType();
            var window = simulator != null
                ? GetWindow<TrainARTrainingsWindow>("TrainAR Trainings", true, simulator)
                : GetWindow<TrainARTrainingsWindow>("TrainAR Trainings");
            window.minSize = new Vector2(420, 300);
        }

        /// <summary>
        /// Unity's device simulator window (internal type).
        /// </summary>
        public static Type SimulatorWindowType() =>
            TypeCache.GetTypesDerivedFrom<EditorWindow>().FirstOrDefault(t => t.Name == "SimulatorWindow");

        private void OnGUI()
        {
            nameStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
            scroll = EditorGUILayout.BeginScrollView(scroll);
            var trainings = TrainARTrainings.All();
            foreach (var name in trainings) DrawTraining(name, trainings.Count);
            EditorGUILayout.Space();
            if (GUILayout.Button("+ Add new training", GUILayout.Height(30)))
                NameDialog.Ask("New training", "Name of the new training:", "", display =>
                {
                    StartEditing(TrainARTrainings.Create(display));
                    Repaint();
                });
            EditorGUILayout.EndScrollView();

            // pinned to the bottom of the tab
            EditorGUILayout.Space(4);
            var footer = TrainARTrainings.ShowHomeFooter;
            var show = EditorGUILayout.ToggleLeft(new GUIContent("Show the TrainAR footer on the home screen",
                "Links to GitHub, Mixality and the documentation, the creator, funding, credits, citation and contact, " +
                "below the trainings."), footer);
            if (show != footer) TrainARTrainings.ShowHomeFooter = show;
            EditorGUILayout.Space(4);
        }

        private void DrawTraining(string name, int count)
        {
            var info = TrainARTrainings.InfoFor(name);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            // the expert sits left of the preview and overlaps it by a third (as on the phone)
            var art = GUILayoutUtility.GetRect(ExpertSize * 2 / 3 + PreviewSize, PreviewSize,
                GUILayout.Width(ExpertSize * 2 / 3 + PreviewSize), GUILayout.Height(PreviewSize));
            if (editing == name) DrawExpertPicker(art);
            else
            {
                var previewRect = new Rect(art.xMax - PreviewSize, art.y, PreviewSize, PreviewSize);
                if (info.preview != null) GUI.DrawTexture(previewRect, info.preview.texture, ScaleMode.ScaleToFit);
                else GUI.Label(previewRect, "Preview after\nthe next build", EditorStyles.centeredGreyMiniLabel);
                DrawRoundExpert(new Rect(art.x, art.center.y - ExpertSize / 2, ExpertSize, ExpertSize), info.expert);
            }

            EditorGUILayout.BeginVertical(GUILayout.MinHeight(PreviewSize));
            if (editing == name) DrawEditor(info);
            else DrawInfo(name, info, count);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// The expert as in the training: round (cut by a circle), white behind transparent pictures, with an outline and
        /// a soft shadow.
        /// </summary>
        private static void DrawRoundExpert(Rect rect, Sprite expert)
        {
            var radius = rect.width / 2;
            var shadow = new Rect(rect.x + 1, rect.y + 2, rect.width, rect.height);
            GUI.DrawTexture(shadow, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0, 0, 0, 0.35f), 0, radius);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.white, 0, radius);
            if (expert != null)
                GUI.DrawTexture(rect, expert.texture, ScaleMode.ScaleAndCrop, true, 0, Color.white, 0, radius);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0.55f, 0.55f, 0.55f), 2, radius);
        }

        /// <summary>
        /// While editing, the picture area only shows the expert: click it to pick another picture, or drop one on it.
        /// </summary>
        private void DrawExpertPicker(Rect art)
        {
            var size = PreviewSize - 22;
            var rect = new Rect(art.center.x - size / 2, art.y, size, size);
            DrawRoundExpert(rect, editExpert);
            GUI.Label(new Rect(art.x, rect.yMax + 4, art.width, 16), "Click or drop to change",
                EditorStyles.centeredGreyMiniLabel);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            var id = GUIUtility.GetControlID(FocusType.Passive);
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                pickerId = id;
                EditorGUIUtility.ShowObjectPicker<Sprite>(editExpert, false, "", id);
                e.Use();
            }
            else if (e.type == EventType.ExecuteCommand && e.commandName == "ObjectSelectorUpdated" &&
                     EditorGUIUtility.GetObjectPickerControlID() == pickerId)
            {
                editExpert = EditorGUIUtility.GetObjectPickerObject() as Sprite;
                Repaint();
            }
            else if ((e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && rect.Contains(e.mousePosition))
            {
                var sprite = DragAndDrop.objectReferences
                    .Select(o => o as Sprite ?? AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(o)))
                    .FirstOrDefault(s => s != null);
                if (sprite == null) return;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    editExpert = sprite;
                }
                e.Use();
            }
        }

        private void DrawInfo(string name, Others.TrainingInfo info, int count)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(info.displayName, nameStyle);
            if (name == TrainARTrainings.Current) GUILayout.Label("(open in the authoring tool)", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(string.IsNullOrEmpty(info.description) ? "No description yet." : info.description,
                EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open", GUILayout.Width(70))) OpenInAuthoringTool(name);
            if (GUILayout.Button("Edit", GUILayout.Width(70))) StartEditing(name);
            if (GUILayout.Button("Duplicate", GUILayout.Width(70)))
                NameDialog.Ask("Duplicate training", "Name of the copy:", info.displayName + " Copy", display =>
                {
                    StartEditing(TrainARTrainings.Duplicate(name, display));
                    Repaint();
                });
            using (new EditorGUI.DisabledScope(count <= 1))
            using (new BackgroundColor(new Color(1f, 0.45f, 0.45f)))
                if (GUILayout.Button(new GUIContent("Delete", count <= 1 ? "The last training cannot be deleted." : ""),
                        GUILayout.Width(70)) &&
                    EditorUtility.DisplayDialog("Delete training", $"Delete \"{info.displayName}\" with its scene, " +
                        "statemachine and everything in its folder? This cannot be undone.", "Delete", "Cancel"))
                {
                    TrainARTrainings.Delete(name);
                    GUIUtility.ExitGUI();
                }
            EditorGUILayout.EndHorizontal();
        }

        private void StartEditing(string name)
        {
            var info = TrainARTrainings.InfoFor(name);
            editing = name;
            editName = info.displayName;
            editDescription = info.description;
            editExpert = info.expert;
        }

        private void DrawEditor(Others.TrainingInfo info)
        {
            editName = EditorGUILayout.TextField("Name", editName);
            EditorGUILayout.LabelField("Description");
            editDescription = EditorGUILayout.TextArea(editDescription, EditorStyles.textArea, GUILayout.MinHeight(54));
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", GUILayout.Width(70))) editing = null;
            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(editName)))
            using (new BackgroundColor(new Color(0.45f, 1f, 0.45f)))
                if (GUILayout.Button("Save", GUILayout.Width(70)))
                {
                    Undo.RecordObject(info, "Edit training");
                    info.displayName = editName.Trim();
                    info.description = editDescription;
                    info.expert = editExpert;
                    EditorUtility.SetDirty(info);
                    AssetDatabase.SaveAssets();
                    TrainARTrainings.Sync();
                    editing = null;
                }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Opens a training in the authoring tool (scene, statemachine, default view) without changing the layout.
        /// </summary>
        private static void OpenInAuthoringTool(string name)
        {
            TrainARTrainings.Current = name;
            TrainAREditorMenu.OpenTrainARScene();
            TrainAREditorMenu.ResetTrainARSceneToAuthoringToolDefault();
            TrainAREditorMenu.ResetStatemachineToDefault();
            TrainAREditorMenu.HideFrameworkHierarchy();
            GUIUtility.ExitGUI();
        }

        /// <summary>
        /// Tints the buttons drawn while it exists (red delete, green save).
        /// </summary>
        private readonly struct BackgroundColor : IDisposable
        {
            private readonly Color previous;

            public BackgroundColor(Color color)
            {
                previous = GUI.backgroundColor;
                GUI.backgroundColor = color;
            }

            public void Dispose() => GUI.backgroundColor = previous;
        }

        /// <summary>
        /// A small dialog asking for a training's name.
        /// </summary>
        private class NameDialog : EditorWindow
        {
            private string label, value;
            private Action<string> done;

            public static void Ask(string title, string label, string value, Action<string> done)
            {
                var dialog = CreateInstance<NameDialog>();
                dialog.titleContent = new GUIContent(title);
                dialog.label = label;
                dialog.value = value;
                dialog.done = done;
                dialog.minSize = dialog.maxSize = new Vector2(320, 90);
                dialog.ShowUtility();
            }

            private void OnGUI()
            {
                EditorGUILayout.LabelField(label);
                GUI.SetNextControlName("name");
                value = EditorGUILayout.TextField(value);
                EditorGUI.FocusTextInControl("name");
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Cancel", GUILayout.Width(80))) Close();
                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(value)))
                {
                    if (GUILayout.Button("OK", GUILayout.Width(80)) ||
                        Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return &&
                        !string.IsNullOrWhiteSpace(value))
                    {
                        var result = value.Trim();
                        Close();
                        done(result);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
