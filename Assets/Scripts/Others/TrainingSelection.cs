using UnityEngine.SceneManagement;

namespace Others
{
    /// <summary>
    /// The training chosen on the home screen. When a training scene is started directly (e.g. in the editor), it is
    /// the training of that scene.
    /// </summary>
    public static class TrainingSelection
    {
        private static TrainingInfo current;

        /// <summary>
        /// The chosen training, else the training of the active scene, else the first training of the build (e.g.
        /// when the onboarding is started directly), or null if there is none.
        /// </summary>
        public static TrainingInfo Current
        {
            get
            {
                if (current != null) return current;
                var catalog = TrainingCatalog.Instance;
                if (catalog == null) return null;
                var ofScene = catalog.ForScene(SceneManager.GetActiveScene().name);
                if (ofScene != null) return ofScene;
                return catalog.trainings.Count > 0 ? catalog.trainings[0] : null;
            }
            set => current = value;
        }
    }
}
