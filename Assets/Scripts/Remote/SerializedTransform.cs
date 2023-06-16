using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Remote
{
    /// <summary>
    /// Serializable class of a transform.
    /// </summary>
    [Serializable]
    public class SerializedTransform
    {
        /// <summary>
        /// Position of the transform
        /// </summary>
        public float[] position = new float[3];
        /// <summary>
        /// Rotation of the transform.
        /// </summary>
        public float[] rotation = new float[4];
        /// <summary>
        /// Scale of the transform.
        /// </summary>
        public float[] scale = new float[3];

        /// <summary>
        /// Empty Constructor.
        /// </summary>
        public SerializedTransform()
        {

        }
        /// <summary>
        /// Constructor for a serialized transform.
        /// </summary>
        /// <param name="transform">Transform to be serialized.</param>
        /// <param name="worldSpace">Use worldspace default = false.</param>
        public SerializedTransform(Transform transform, bool worldSpace = false)
        {
            position[0] = transform.localPosition.x;
            position[1] = transform.localPosition.y;
            position[2] = transform.localPosition.z;

            rotation[0] = transform.localRotation.w;
            rotation[1] = transform.localRotation.x;
            rotation[2] = transform.localRotation.y;
            rotation[3] = transform.localRotation.z;

            scale[0] = transform.localScale.x;
            scale[1] = transform.localScale.y;
            scale[2] = transform.localScale.z;
        }
        /// <summary>
        /// Deserialises the transform.
        /// </summary>
        /// <param name="result">Transform that should be filed.</param>
        /// <returns>Deserialized transform.</returns>
        public Transform DeserializedTransform(Transform result)
        {
            result.position = new Vector3(position[0], position[1], position[2]);
            result.rotation = new Quaternion(rotation[1], rotation[2], rotation[3], rotation[0]);
            result.localScale = new Vector3(scale[0], scale[1], scale[2]); ;
            return result;
        }
    }
}