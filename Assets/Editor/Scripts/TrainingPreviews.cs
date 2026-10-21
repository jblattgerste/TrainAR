using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Editor.Scripts
{
    /// <summary>
    /// Renders every training's start setup (its objects as placed in the scene, no table, transparent background, seen
    /// over the trainee's left shoulder) into &lt;Name&gt;_Preview.png and sets it as the training's preview. Runs before every
    /// build (TrainAR build and Unity's build window).
    /// </summary>
    [InitializeOnLoad]
    public static class TrainingPreviews
    {
        private const int Size = 1024;

        static TrainingPreviews()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(options =>
            {
                PrepareBuild();
                options.scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
            });
        }

        /// <summary>
        /// Before a build: catalog and build scenes up to date, every preview re-rendered.
        /// </summary>
        public static void PrepareBuild()
        {
            EditorSceneManager.SaveOpenScenes();
            TrainARTrainings.Sync();
            foreach (var name in TrainARTrainings.All()) Render(name);
            AssetDatabase.SaveAssets();
        }

        private static IEnumerable<Vector3> Corners(Bounds b)
        {
            for (var i = 0; i < 8; i++)
                yield return b.center + Vector3.Scale(b.extents, new Vector3((i & 1) * 2 - 1, (i & 2) - 1, (i & 4) / 2 - 1));
        }

        private static Rect VisibleSquare(Color32[] pixels, int size)
        {
            int x0 = size, y0 = size, x1 = 0, y1 = 0;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var c = pixels[y * size + x];
                if (c.a < 8) continue;                                   // background
                if (x < x0) x0 = x; if (x > x1) x1 = x; if (y < y0) y0 = y; if (y > y1) y1 = y;
            }
            if (x1 < x0) return new Rect(0, 0, size, size);
            var side = Mathf.Min(size, Mathf.Max(x1 - x0, y1 - y0) * 1.1f);
            var cx = Mathf.Clamp((x0 + x1) / 2f, side / 2, size - side / 2);
            var cy = Mathf.Clamp((y0 + y1) / 2f, side / 2, size - side / 2);
            return new Rect(cx - side / 2, cy - side / 2, side, side);
        }

        private static bool Fits(Transform camera, List<Vector3> points, float limit)
        {
            foreach (var p in points)
            {
                var v = p - camera.position;
                var z = Vector3.Dot(v, camera.forward);
                if (z <= 0.01f) return false;
                if (Mathf.Abs(Vector3.Dot(v, camera.right)) / z > limit || Mathf.Abs(Vector3.Dot(v, camera.up)) / z > limit)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Renders one training's preview and assigns it to its info asset.
        /// </summary>
        public static void Render(string name)
        {
            var path = TrainARTrainings.ScenePath(name);
            var scene = SceneManager.GetSceneByPath(path);
            var wasOpen = scene.isLoaded;
            if (!wasOpen) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            var renderers = scene.GetRootGameObjects().Where(g => g.name != "TrainAR Framework" && g.activeInHierarchy)
                .SelectMany(g => g.GetComponentsInChildren<Renderer>()).Where(r => r.enabled).ToList();
            if (renderers.Count == 0)
            {
                if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
                Debug.Log($"TrainAR: no objects to preview in \"{name}\".");
                return;
            }
            // only the training objects: every other renderer of the scene is hidden while rendering (objects keep their
            // layers, so the render pipeline draws them as usual)
            var others = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                .Where(s => s.isLoaded).SelectMany(s => s.GetRootGameObjects())
                .SelectMany(g => g.GetComponentsInChildren<Renderer>(true))
                .Where(r => r.enabled && !renderers.Contains(r)).ToList();      // also other open trainings
            foreach (var r in others) r.enabled = false;
            // only the training's own lights: other open scenes (e.g. the training open in the authoring tool) would light
            // the objects a second time
            var otherLights = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
                .Where(s => s.isLoaded && s != scene).SelectMany(s => s.GetRootGameObjects())
                .SelectMany(g => g.GetComponentsInChildren<Light>(true)).Where(l => l.enabled).ToList();
            foreach (var l in otherLights) l.enabled = false;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);

            var cameraObject = new GameObject("TrainAR Preview Camera") { hideFlags = HideFlags.HideAndDontSave };
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(1, 1, 1, 0);      // transparent (white edges, no dark fringes)
            camera.cullingMask = renderers.Aggregate(0, (mask, r) => mask | 1 << r.gameObject.layer);
            camera.fieldOfView = 30f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100f;
            // from the trainee's view over the left shoulder: on placement the setup's +Z points towards the phone, so
            // the trainee stands on the +Z side looking towards -Z; the camera is behind them, to their left (+X) and
            // 30 degrees above; the distance is the smallest at which every object fits into 90 % of the picture
            camera.transform.rotation = Quaternion.Euler(30, 225, 0);
            var corners = renderers.SelectMany(r => Corners(r.bounds)).ToList();
            var limit = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 0.9f;
            float near = 0.01f, far = 50f;
            for (var i = 0; i < 40; i++)
            {
                var distance = (near + far) / 2;
                camera.transform.position = bounds.center - camera.transform.forward * distance;
                if (Fits(camera.transform, corners, limit)) far = distance; else near = distance;
            }
            camera.transform.position = bounds.center - camera.transform.forward * far;

            var active = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);                 // the training's lighting and reflections
            var target = RenderTexture.GetTemporary(Size * 2, Size * 2, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            camera.targetTexture = target;
            camera.Render();

            // crop a square around what is actually visible (bounds can be larger than the visible objects)
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var full = new Texture2D(Size * 2, Size * 2, TextureFormat.RGBA32, false);
            full.ReadPixels(new Rect(0, 0, Size * 2, Size * 2), 0, 0);
            full.Apply();
            var crop = VisibleSquare(full.GetPixels32(), Size * 2);
            var cropped = RenderTexture.GetTemporary(Size, Size, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(target, cropped, new Vector2(crop.width / (Size * 2f), crop.height / (Size * 2f)),
                new Vector2(crop.x / (Size * 2f), crop.y / (Size * 2f)));
            RenderTexture.active = cropped;
            var picture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            picture.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
            picture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(cropped);
            File.WriteAllBytes(TrainARTrainings.PreviewPath(name), picture.EncodeToPNG());
            Object.DestroyImmediate(picture);
            Object.DestroyImmediate(full);
            camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(target);
            Object.DestroyImmediate(cameraObject);

            foreach (var r in others) r.enabled = true;
            foreach (var l in otherLights) l.enabled = true;
            if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
            if (!wasOpen) EditorSceneManager.CloseScene(scene, true);

            AssetDatabase.ImportAsset(TrainARTrainings.PreviewPath(name));
            var importer = (TextureImporter)AssetImporter.GetAtPath(TrainARTrainings.PreviewPath(name));
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            var info = TrainARTrainings.InfoFor(name);
            info.preview = AssetDatabase.LoadAssetAtPath<Sprite>(TrainARTrainings.PreviewPath(name));
            EditorUtility.SetDirty(info);
            Debug.Log($"TrainAR: preview of \"{name}\" rendered.");
        }
    }
}
