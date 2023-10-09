using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Remote
{
    /// <summary>
    /// Serializable Vector3 class.
    /// </summary>
    [Serializable]
    public class SerializedVector3
    {
        /// <summary>
        /// X value.
        /// </summary>
        public float x;
        /// <summary>
        /// Y value.
        /// </summary>
        public float y;
        /// <summary>
        /// Z value.
        /// </summary>
        public float z;
        /// <summary>
        /// Empty Contructor.
        /// </summary>
        public SerializedVector3()
        {

        }
        /// <summary>
        /// Constructor for serializable Vector3.
        /// </summary>
        /// <param name="vector">Vector3 to serialize</param>
        public SerializedVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
        /// <summary>
        /// Deserializes the Vector3.
        /// </summary>
        /// <returns>Vector3 usable for Unity.</returns>
        public Vector3 DeserializedVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}

