using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Remote
{
    /// <summary>
    /// Serializable class that holds meta data from the scenario.
    /// </summary>
    [Serializable]
    public class ScenarioInformation
    {
        /// <summary>
        /// Name of the scenario.
        /// </summary>
        public string name;
        /// <summary>
        /// Name of the preview image.
        /// </summary>
        public string previewImageName;
        /// <summary>
        /// Description of the scenario.
        /// </summary>
        public string description;
        /// <summary>
        /// Array of names of all trainar objects in the scenario.
        /// </summary>
        public string[] trainARObjects;
        /// <summary>
        /// Empty constructor
        /// </summary>
        public ScenarioInformation()
        {

        }
        /// <summary>
        /// Constructor for the scenario information.
        /// </summary>
        /// <param name="name">Name of the scenario.</param>
        /// <param name="previewImageName">Name of the preview image.</param>
        /// <param name="description">Description of the scenario.</param>
        /// <param name="trainARObjects">Names of all trainar objects.</param>
        public ScenarioInformation(string name, string previewImageName, string description, string[] trainARObjects)
        {
            this.name = name;
            this.previewImageName = previewImageName;
            this.description = description;
            this.trainARObjects = trainARObjects;
        }
    }
}