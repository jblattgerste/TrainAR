//
//  Outline.cs
//  QuickOutline
//
//  Originally created by Chris Nolet on 3/30/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//
//  Altered by Jonas Blattgerste for the usage in TrainAR.
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interaction
{
  /// <summary>
  /// The Outline script is automatically attached to TrainAR objects on conversion and handles the outlining
  /// for e.g. selection, success or failure. It is called and handled by the MaterialController.
  /// </summary>
  [DisallowMultipleComponent]
  public class Outline : MonoBehaviour {
    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();
   
    /// <summary>
    /// Differentiates the type of the outlines used.
    /// </summary>
    public enum Mode {
      OutlineAll,
      OutlineVisible,
      OutlineHidden,
      OutlineAndSilhouette,
      SilhouetteOnly
    }

    /// <summary>
    /// The type of outline used. (e.g. if it shows through other objects)
    /// </summary>
    public Mode OutlineMode {
      get { return outlineMode; }
      set {
        outlineMode = value;
        needsUpdate = true;
      }
    }
    
    /// <summary>
    /// The color of the outline.
    /// </summary>
    public Color OutlineColor {
      get { return outlineColor; }
      set {
        outlineColor = value;
        needsUpdate = true;
      }
    }
    
    /// <summary>
    /// The width/thickness of the outline.
    /// </summary>
    public float OutlineWidth {
      get { return outlineWidth; }
      set {
        outlineWidth = value;
        needsUpdate = true;
      }
    }

    [Serializable]
    private class ListVector3 {
      public List<Vector3> data;
    }

    [SerializeField]
    private Mode outlineMode;

    [SerializeField]
    private Color outlineColor = Color.blue;

    [SerializeField, Range(0f, 10f)]
    private float outlineWidth = 7f;

    [Header("Optional")]

    [SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
                             + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
#pragma warning disable 0649
    private bool precomputeOutline = true;
#pragma warning restore 0649

    [SerializeField, HideInInspector]
    private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector]
    private List<ListVector3> bakeValues = new List<ListVector3>();

    private Renderer[] renderers;
    [SerializeField]
    private Material outlineMaskMaterial;
    [SerializeField]
    private Material outlineFillMaterial;

    private bool needsUpdate;
    private bool initialized;
    private bool placementSuppressed;

  void Awake() {
    Initialize();
  }

  private void Initialize() {
    if (initialized) return;
    renderers = GetComponentsInChildren<Renderer>(true);
    if (outlineMaskMaterial == null || outlineFillMaterial == null) {
      Debug.LogError("Outline requires a mask and fill material.", this);
      return;
    }

    // Use per-object instances, otherwise color changes (e.g. highlight vs. selection) apply to all TrainAR objects
    outlineMaskMaterial = Instantiate(outlineMaskMaterial);
    outlineFillMaterial = Instantiate(outlineFillMaterial);

    outlineMaskMaterial.name = "OutlineMask (Instance)";
    outlineFillMaterial.name = "OutlineFill (Instance)";
    initialized = true;

    // Retrieve or generate smooth normals
    LoadSmoothNormals();

    // Apply material properties immediately
    UpdateMaterialProperties();
  }

  void OnEnable() {
    Initialize();
    SynchronizeMaterials();
  }

  /// <summary>
  /// Hides only the rendering passes while placement owns the renderer materials.
  /// Selection, highlights and feedback can continue to update their logical state.
  /// </summary>
  public void SetPlacementSuppressed(bool suppressed) {
    placementSuppressed = suppressed;
    SynchronizeMaterials();
  }

  /// <summary>
  /// Repairs material slots after another system restores or replaces materials.
  /// Only this component's material instances are removed, so nested outlines and
  /// selection materials retain their ownership.
  /// </summary>
  public void SynchronizeMaterials() {
    if (!initialized) return;
    bool visible = isActiveAndEnabled && !placementSuppressed;
    foreach (var renderer in renderers) {
      if (renderer == null) continue;
      var current = renderer.sharedMaterials;
      int maskCount = 0, fillCount = 0, maskIndex = -1, fillIndex = -1;
      for (int i = 0; i < current.Length; i++) {
        if (current[i] == outlineMaskMaterial) { maskCount++; maskIndex = i; }
        if (current[i] == outlineFillMaterial) { fillCount++; fillIndex = i; }
      }
      // An intact pair may sit alongside another object's outline or selection
      // material. Keep that order stable instead of moving our pair every frame.
      if (visible && maskCount == 1 && fillCount == 1 && fillIndex == maskIndex + 1) continue;
      if (!visible && maskCount == 0 && fillCount == 0) continue;
      var materials = current.Where(material => material != outlineMaskMaterial && material != outlineFillMaterial).ToList();
      if (visible) {
        materials.Add(outlineMaskMaterial);
        materials.Add(outlineFillMaterial);
      }
      renderer.sharedMaterials = materials.ToArray();
    }
  }

  /// <summary>
  /// Prepares a replacement mesh and refreshes the passes without changing the
  /// current selection/highlight/feedback state.
  /// </summary>
  public void RefreshMesh() {
    if (!initialized) return; // Awake will prepare a previously inactive object.
    renderers = GetComponentsInChildren<Renderer>(true);
    LoadSmoothNormals();
    SynchronizeMaterials();
  }

  void OnValidate() {

    // Update material properties
    needsUpdate = true;

    // Clear cache when baking is disabled or corrupted
    if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count) {
      bakeKeys.Clear();
      bakeValues.Clear();
    }

    // Generate smooth normals when baking is enabled
    if (precomputeOutline && bakeKeys.Count == 0) {
      Bake();
    }
  }

  void Update() {
    if (initialized && needsUpdate) {
      needsUpdate = false;

      UpdateMaterialProperties();
    }
  }

  void OnDisable() {
    SynchronizeMaterials();
  }

  void OnDestroy() {

    // Destroy material instances
    if (initialized) {
      Destroy(outlineMaskMaterial);
      Destroy(outlineFillMaterial);
    }
  }

  void Bake() {

    // Generate smooth normals for each mesh
    var bakedMeshes = new HashSet<Mesh>();

    foreach (var meshFilter in GetComponentsInChildren<MeshFilter>(true)) {
      if (meshFilter.sharedMesh == null || !meshFilter.sharedMesh.isReadable) continue;

      // Skip duplicates
      if (!bakedMeshes.Add(meshFilter.sharedMesh)) {
        continue;
      }

      // Serialize smooth normals
      var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

      bakeKeys.Add(meshFilter.sharedMesh);
      bakeValues.Add(new ListVector3() { data = smoothNormals });
    }
  }

  void LoadSmoothNormals() {

    // Retrieve or generate smooth normals
    foreach (var meshFilter in GetComponentsInChildren<MeshFilter>(true)) {
      if (meshFilter.sharedMesh == null || !meshFilter.sharedMesh.isReadable) continue;

      // Skip if smooth normals have already been adopted
      if (!registeredMeshes.Add(meshFilter.sharedMesh)) {
        continue;
      }

      // Retrieve or generate smooth normals
      var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
      var smoothNormals = index >= 0 && index < bakeValues.Count &&
        bakeValues[index] != null && bakeValues[index].data != null &&
        bakeValues[index].data.Count == meshFilter.sharedMesh.vertexCount
        ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

      // Store smooth normals in UV3
      meshFilter.sharedMesh.SetUVs(3, smoothNormals);

      // Combine submeshes
      var renderer = meshFilter.GetComponent<Renderer>();

      if (renderer != null) {
        CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
      }
    }

    // Clear UV3 on skinned mesh renderers
    foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
      if (skinnedMeshRenderer.sharedMesh == null || !skinnedMeshRenderer.sharedMesh.isReadable) continue;

      // Skip if UV3 has already been reset
      if (!registeredMeshes.Add(skinnedMeshRenderer.sharedMesh)) {
        continue;
      }

      // Clear UV3
      skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];

      // Combine submeshes
      CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
    }
  }

  List<Vector3> SmoothNormals(Mesh mesh) {

    if (mesh.normals.Length != mesh.vertexCount) mesh.RecalculateNormals();

    // Group vertices by location
    var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

    // Copy normals to a new list
    var smoothNormals = new List<Vector3>(mesh.normals);

    // Average normals for grouped vertices
    foreach (var group in groups) {

      // Skip single vertices
      if (group.Count() == 1) {
        continue;
      }

      // Calculate the average normal
      var smoothNormal = Vector3.zero;

      foreach (var pair in group) {
        smoothNormal += smoothNormals[pair.Value];
      }

      smoothNormal.Normalize();

      // Assign smooth normal to each vertex
      foreach (var pair in group) {
        smoothNormals[pair.Value] = smoothNormal;
      }
    }

    return smoothNormals;
  }

  void CombineSubmeshes(Mesh mesh, Material[] materials) {

    // Skip meshes with a single submesh
    if (mesh.subMeshCount <= 1) {
      return;
    }

    // Skip if submesh count exceeds material count
    if (mesh.subMeshCount > materials.Length) {
      return;
    }

    // Append combined submesh
    mesh.subMeshCount++;
    mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
  }

  void UpdateMaterialProperties() {

    // Apply properties according to mode
    outlineFillMaterial.SetColor("_OutlineColor", outlineColor);

    switch (outlineMode) {
      case Mode.OutlineAll:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;

      case Mode.OutlineVisible:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;

      case Mode.OutlineHidden:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;

      case Mode.OutlineAndSilhouette:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
        break;

      case Mode.SilhouetteOnly:
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
        outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
        break;
    }
  }
  }
}
