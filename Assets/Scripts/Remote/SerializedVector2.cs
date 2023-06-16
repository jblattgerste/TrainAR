using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Remote
{
    /// <summary>
    /// Serializable class of a Vector2.
    /// </summary>
    [Serializable]
    public class SerializedVector2
    {
        /// <summary>
        /// X Value.
        /// </summary>
        public float x;
        /// <summary>
        /// Y Value.
        /// </summary>
        public float y;
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public SerializedVector2()
        {

        }
        /// <summary>
        /// Constructor for serializable Vector2.
        /// </summary>
        /// <param name="vector">Vector2 to serialize.</param>
        public SerializedVector2(Vector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }
        /// <summary>
        /// Deserializes the Vector2.
        /// </summary>
        /// <returns>Vector2 usable for Unity.</returns>
        public Vector2 DeserializedVector2()
        {
            return new Vector2(x, y);
        }
    }
}