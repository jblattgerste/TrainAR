using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Others
{
    /// <summary>
    /// All trainings in the build (Resources/TrainARTrainings.asset), kept up to date by the editor tools. The home
    /// screen lists them.
    /// </summary>
    public class TrainingCatalog : ScriptableObject
    {
        /// <summary>
        /// The trainings in the order of the home screen.
        /// </summary>
        public List<TrainingInfo> trainings = new List<TrainingInfo>();
        /// <summary>
        /// Whether the home screen shows the TrainAR footer (links, creator, funding, contact) below the trainings.
        /// </summary>
        public bool showFooter = true;

        private static TrainingCatalog instance;

        /// <summary>
        /// The catalog of this build.
        /// </summary>
        public static TrainingCatalog Instance
        {
            get
            {
                if (instance == null) instance = Resources.Load<TrainingCatalog>("TrainARTrainings");
                return instance;
            }
        }

        /// <summary>
        /// The training whose scene has the given name, or null.
        /// </summary>
        public TrainingInfo ForScene(string sceneName) =>
            trainings.FirstOrDefault(t => t != null && t.sceneName == sceneName);
    }
}
