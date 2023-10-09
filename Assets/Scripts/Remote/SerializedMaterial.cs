using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Remote
{
    /// <summary>
    /// Serializable class for a material.
    /// </summary>
    [Serializable]
    public class SerializedMaterial
    {
        /// <summary>
        /// Name of the shader.
        /// </summary>
        public string shader;
        /// <summary>
        /// Name of the material.
        /// </summary>
        public string name;
        /// <summary>
        /// Name of the baseMap image.
        /// </summary>
        public string baseMap;
        /// <summary>
        /// Path to the baseMap image in the Unity project.
        /// </summary>
        public string baseMapPath;
        /// <summary>
        /// Name of the metalMap image.
        /// </summary>
        public string metalMap;
        /// <summary>
        /// Path to the metalMap image in the Unity project.
        /// </summary>
        public string metalMapPath;
        /// <summary>
        /// Name of the normalMap image.
        /// </summary>
        public string normalMap;
        /// <summary>
        /// Path to the normalMap image in the Unity project.
        /// </summary>
        public string normalMapPath;
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public SerializedMaterial()
        {

        }
        /// <summary>
        /// Constructor to create a serialized material.
        /// </summary>
        /// <param name="material">Material to be serialized.</param>
        public SerializedMaterial(Material material)
        {
            shader = material.shader.name;
            name = material.name;
            baseMap = material.GetTexture("_MainTex").name;
            normalMap = material.GetTexture("_BumpMap").name;
            metalMap = material.GetTexture("_MetallicGlossMap").name;
#if UNITY_EDITOR
            baseMapPath = AssetDatabase.GetAssetPath(material.GetTexture("_MainTex"));
            normalMapPath = AssetDatabase.GetAssetPath(material.GetTexture("_BumpMap"));
            metalMapPath = AssetDatabase.GetAssetPath(material.GetTexture("_MetallicGlossMap"));
#endif
        }
        /// <summary>
        /// Creates an array of serialized materials.
        /// </summary>
        /// <param name="number">Number of materials.</param>
        /// <param name="material">Materials that will be serialized.</param>
        /// <returns></returns>
        public SerializedMaterial[] getSerializedMaterials(int number, Material[] material)
        {
            SerializedMaterial[] result = new SerializedMaterial[number];
            int materialCount = 0;
            foreach (Material each in material)
            {
                result[materialCount] = new SerializedMaterial(each);
                materialCount++;
            }
            return result;
        }
        /// <summary>
        /// Deserialize an array of materials.
        /// </summary>
        /// <param name="serializedMaterials">Array to be deserialized.</param>
        /// <returns>Array of deserialized materials.</returns>
        public Material[] getMaterials(SerializedMaterial[] serializedMaterials)
        {
            Material[] result = new Material[serializedMaterials.Length];
            int materialCount = 0;
            foreach (SerializedMaterial material in serializedMaterials)
            {
                result[materialCount] = material.DeserializedMaterial();
                materialCount++;
            }
            return result;
        }
        /// <summary>
        /// Deserilises the material.
        /// </summary>
        /// <returns>Deserialized material.</returns>
        public Material DeserializedMaterial()
        {
            Material result = new Material(Shader.Find(shader));
            result.name = name;
            Texture maintex = LoadTextureFromFile(baseMapPath);
            result.SetTexture("_MainTex", maintex);
            Texture normalmap = LoadTextureFromFile(normalMapPath);
            result.SetTexture("_BumpMap", normalmap);
            Texture metalmap = LoadTextureFromFile(metalMapPath);
            result.SetTexture("_MetallicGlossMap", metalmap);
            return result;
        }
        /// <summary>
        /// Load the image file into a Texture.
        /// </summary>
        /// <param name="path">Path to the image.</param>
        /// <returns>Texture from image.</returns>
        public Texture LoadTextureFromFile(string path)
        {
            byte[] bytes;
            bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            return texture;
        }
    }
}