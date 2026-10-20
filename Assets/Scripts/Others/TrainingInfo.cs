using UnityEngine;

namespace Others
{
    /// <summary>
    /// Describes one training (Assets/Trainings/&lt;Name&gt;/&lt;Name&gt;_Info.asset): shown on the home screen and the first
    /// onboarding page, and its expert picture is used for the insights and the end screen.
    /// </summary>
    public class TrainingInfo : ScriptableObject
    {
        /// <summary>
        /// The name of the training as shown to the trainee.
        /// </summary>
        [Tooltip("The name of the training as shown to the trainee.")]
        public string displayName;
        /// <summary>
        /// What the training is about (first onboarding page).
        /// </summary>
        [TextArea(3, 10)]
        [Tooltip("What the training is about (first onboarding page).")]
        public string description;
        /// <summary>
        /// The expert's picture (insights and end screen). Always shown round: transparent pictures on a white circle,
        /// others cut to a circle.
        /// </summary>
        [Tooltip("The expert's picture (insights and end screen). Always shown round.")]
        public Sprite expert;
        /// <summary>
        /// The training's start setup, rendered automatically before every build.
        /// </summary>
        [Tooltip("The training's start setup, rendered automatically before every build.")]
        public Sprite preview;
        /// <summary>
        /// The name of the training's scene (set by the editor tools).
        /// </summary>
        [HideInInspector]
        public string sceneName;
        /// <summary>
        /// When the training was created (UTC ticks; 0 for the example trainings): trainings are listed in the order they were created.
        /// </summary>
        [HideInInspector]
        public long created;
    }
}
