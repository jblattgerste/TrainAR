using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// The abstract class for questions to share the same UI functions.
    /// </summary>
    public abstract class Question
    {
        /// <summary>
        /// Every question needs a questionText.
        /// </summary>
        public string questionText;
    }
}
