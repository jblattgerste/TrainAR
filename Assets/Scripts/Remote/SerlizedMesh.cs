using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Remote
{
    /// <summary>
    /// Mesh class that is able to be serialized.
    /// </summary>
    [Serializable]
    public class SerializedMesh
    {
        /// <summary>
        /// Name of the Mesh.
        /// </summary>
        public string name;
        /// <summary>
        /// Array that holds all vertices of the mesh.
        /// </summary>
        public SerializedVector3[] vertices;
        /// <summary>
        /// Array that holds all uv vectors of the mesh.
        /// </summary
        public SerializedVector2[] uv;
        /// <summary>
        /// Array that holds numbers of triangles.
        /// </summary>
        public int[] triangles;
        /// <summary>
        /// Array thats holds number and lenght of submeshes.
        /// </summary>
        public int[][] submeshes;
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public SerializedMesh()
        {

        }
        /// <summary>
        /// Constructor to create a serialized mesh.
        /// </summary>
        /// <param name="mesh">Mesh to serialize.</param>
        public SerializedMesh(Mesh mesh)
        {
            name = mesh.name;
            vertices = SerializeVector3Array(mesh.vertices);
            uv = SerializeVector2Array(mesh.uv);
            triangles = mesh.triangles;
            submeshes = submeshIndizies(mesh);
        }
        /// <summary>
        /// Creates the indicies of the submesh vectors.
        /// </summary>
        /// <param name="mesh">Source mesh for submesh indicies.</param>
        /// <returns>Array with submesh indicies.</returns>
        public int[][] submeshIndizies(Mesh mesh)
        {
            int count = mesh.subMeshCount;
            int[][] result = new int[count][];
            for (int i = 0; i < count; i++)
            {
                result[i] = mesh.GetIndices(i);
            }
            return result;
        }
        /// <summary>
        /// Serializes a given Vector2 array.
        /// </summary>
        /// <param name="array">Array of Vector2.</param>
        /// <returns>Serialized Vector2 array.</returns>
        public SerializedVector2[] SerializeVector2Array(Vector2[] array)
        {
            SerializedVector2[] result = new SerializedVector2[array.Length];
            int i = 0;
            foreach (Vector2 vector in array)
            {
                result[i] = new SerializedVector2(vector);
                i++;
            }
            return result;
        }
        /// <summary>
        /// Deserializes a given Vector2 array.
        /// </summary>
        /// <param name="array">Array of Serialized Vector2.</param>
        /// <returns>Vector2 array.</returns>
        public Vector2[] DeserializeVector2Array(SerializedVector2[] array)
        {
            Vector2[] result = new Vector2[array.Length];
            int i = 0;
            foreach (SerializedVector2 vector in array)
            {
                result[i] = new Vector2(vector.x, vector.y);
                i++;
            }
            return result;
        }
        /// <summary>
        /// Serializes a given Vector3 array.
        /// </summary>
        /// <param name="array">Array of Vector3.</param>
        /// <returns>Serialized Vector3 array.</returns>
        public SerializedVector3[] SerializeVector3Array(Vector3[] array)
        {
            SerializedVector3[] result = new SerializedVector3[array.Length];
            int i = 0;
            foreach (Vector3 vector in array)
            {
                result[i] = new SerializedVector3(vector);
                i++;
            }
            return result;
        }
        /// <summary>
        /// Deserializes a given Vector3 array.
        /// </summary>
        /// <param name="array">Array of Serialized Vector3.</param>
        /// <returns>Vector3 array.</returns>
        public Vector3[] DeserializeVector3Array(SerializedVector3[] array)
        {
            Vector3[] result = new Vector3[array.Length];
            int i = 0;
            foreach (SerializedVector3 vector in array)
            {
                result[i] = new Vector3(vector.x, vector.y, vector.z);
                i++;
            }
            return result;
        }
        /// <summary>
        /// Deserializes the Mesh.
        /// </summary>
        /// <returns>Mesh that is usable for Unity.</returns>
        public Mesh DeserializedMesh()
        {
            Mesh result = new Mesh();
            result.name = name;
            result.vertices = DeserializeVector3Array(vertices);
            result.uv = DeserializeVector2Array(uv);
            result.triangles = triangles;
            result.subMeshCount = submeshes.Length;
            int submeshCount = 0;
            foreach (int[] submesh in submeshes)
            {
                result.SetIndices(submesh, MeshTopology.Triangles, submeshCount);
                submeshCount++;
            }
            result.RecalculateNormals();
            result.RecalculateTangents();
            result.RecalculateBounds();
            return result;
        }
    }
}