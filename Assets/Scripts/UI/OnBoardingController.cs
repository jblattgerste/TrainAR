using Others;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Video;

namespace UI
{    /// <summary>
     /// The OnBoardingController handles displaying the onboarding animations for scanning the environment and placing the
     /// training assembly by automaticaly detecting at which point and where sufficient feature points for a plane were detected.
     /// </summary>
    public class OnBoardingController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the AR Camera Manager of this scene.
        /// </summary>
        /// <value>Set in inspector.</value>
        [Header("References: ")]
        [SerializeField]
        [Tooltip("Reference to the AR Camera Manager of this scene.")]
        private ARCameraManager m_CameraManager;
        /// <summary>
        /// Reference to the AR Session of this scene.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("Reference to the AR Session of this scene.")]
        private ARSession session;
        /// <summary>
        /// Trackingstate
        /// </summary>
        /// <value>Depending on the subsystem info.</value>
        private string stateText = "None";
        /// <summary>
        /// Reason for false tracking.
        /// </summary>
        /// <value>Depending on the subsystem info.</value>
        private string reasonText = "None";
        /// <summary>
        /// Reference to the Prefab Spawning Controller.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("Reference to the Prefab Spawning Controller")]
        private PrefabSpawningController PrefabSpawningController;
        /// <summary>
        /// The Video Frame for the 'Move Device' clip.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Video Frame for the 'Move Device' clip.")]
        private GameObject videoFrameMoveDevice;
        /// <summary>
        /// The Video for the 'Move Device' clip.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The 'Move Device'-clip.")]
        private StreamVideo videoClipMoveDevice;
        /// <summary>
        /// The Video Player for the 'Move Device' clip.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Video Player.")]
        private VideoPlayer videoPlayerMoveDevice;
        /// <summary>
        /// The Video Frame for the 'Tap to Place'-clip.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Video Frame for the 'Tap to Place'-clip.")]
        private GameObject videoFrameTapToPlace;
        /// <summary>
        /// The Video Frame for the 'Tap to Place'-clip.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The 'Move Device'-clip.")]
        private StreamVideo videoClipTapToPlace;
        /// <summary>
        /// The Gameobject holding the UI-Elements for the 'Insufficient Light'-screen.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Gameobject holding the UI-Elements for the 'Insufficient Light'-screen.")]
        private GameObject insufficientLight;
        /// <summary>
        /// The Gameobject holding the UI-Elements for the 'Insufficient Light'-screen during onboarding.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Gameobject holding the UI-Elements for the 'Insufficient Light'-screen during onboarding.")]
        private GameObject insufficientLightOb;
        /// <summary>
        /// The Gameobject holding the UI-Elements for the 'Excessive Motion'-screen.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Gameobject holding the UI-Elements for the 'Excessive Motion'-screen.")]
        private GameObject excessiveMotion;
        /// <summary>
        /// The Gameobject holding the UI-Elements for the 'Excessive Motion'-screen during onboarding.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Gameobject holding the UI-Elements for the 'Excessive Motion'-screen during onboarding.")]
        private GameObject excessiveMotionOb;
        /// <summary>
        /// The Gameobject holding the UI-Elements for the 'insufficient Features'-screen.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Gameobject holding the UI-Elements for the 'insufficient Features'-screen.")]
        private GameObject insufficientFeatures;
        /// <summary>
        /// The Gameobject holding the UI-Elements for the 'insufficient Features'-screen during onboarding.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Gameobject holding the UI-Elements for the 'insufficient Features'-screen during onboarding.")]
        private GameObject insufficientFeaturesOb;
        /// <summary>
        /// The Gameobject holding the UI-Elements shown when other tracking issues occure.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Gameobject holding the UI-Elements shown when other tracking issues occure.")]
        private GameObject defaultTracking;
        /// <summary>
        /// Set to true if the moveDeviceClip is loaded and ready to be played.
        /// </summary>
        /// <value>False on start.</value>
        private bool moveDeviceClipWasLoaded = false;
        /// <summary>
        /// Set to true if the tatpToPlaceClip is loaded and ready to be played.
        /// </summary>
        /// <value>False on start.</value>
        private bool TapToPlaceClipWasLoaded = false;
        /// <summary>
        /// Holder for trackingReason.
        /// </summary>
        /// <value>"None" on start.</value>
        private string tempTrackingReason = "None";
        /// <summary>
        /// Counter to check when to disable the move device animation
        /// </summary>
        /// <value>"0" on start.</value>
        private int moveDeviceLoops = 0;
        /// <summary>
        /// Reference to text field.
        /// </summary>
        /// <value>Resolved from the fallback tracking panel in Awake.</value>
        private Text otherReasons;

        private void Awake()
        {
            if (defaultTracking != null)
                otherReasons = defaultTracking.GetComponentInChildren<Text>(true);
        }

        /// <summary>
        /// Toggles the MoveDevice animation on and off
        /// </summary>
        /// <param name="toggleState">whether to toggle on or off</param>
        private void ToggleMoveDeviceAnimation(bool toggleState)
        {
            if (toggleState) //On
            {
                //Initial start, load the clip
                if (!moveDeviceClipWasLoaded)
                {
                    moveDeviceClipWasLoaded = true;
                    videoClipMoveDevice.StartVideo();
                    videoFrameMoveDevice.transform.localScale = Vector3.one;
                }
                else
                {
                    videoFrameMoveDevice.transform.localScale = Vector3.one;
                }
            }
            else // Off
            {
                videoFrameMoveDevice.transform.localScale = Vector3.zero;
                moveDeviceLoops = 0;
            }
        }

        /// <summary>
        /// Toggles the TapToPlace animation on and off
        /// </summary>
        /// <param name="toggleState">whether to toggle on or off</param>
        private void ToggleTapToPlaceAnimation(bool toggleState)
        {
            if (toggleState) //On
            {
                //Initial start, load the clip
                if (!TapToPlaceClipWasLoaded)
                {
                    TapToPlaceClipWasLoaded = true;
                    videoClipTapToPlace.StartVideo();
                    videoFrameTapToPlace.transform.localScale = Vector3.one;
                }
                else
                {
                    videoFrameTapToPlace.transform.localScale = Vector3.one;
                }
            }
            else // Off
            {
                videoFrameTapToPlace.transform.localScale = Vector3.zero;
            }
        }
        /// <summary>
        /// Start different functions.
        /// </summary>
        private void Update()
        {
            TrackingstateDependentOnboarding();
            ShowTrackingInformation();
            ShowTrackingInformationDuringOnboarding();
        }
        /// <summary>
        /// Adds listener to prefabSpawned and repostion.
        /// </summary>
        void OnEnable()
        {
            if (m_CameraManager != null)
                m_CameraManager.frameReceived += FrameChanged;
            if (videoPlayerMoveDevice != null)
                videoPlayerMoveDevice.loopPointReached += EndReached;

            PrefabSpawningController.prefabSpawned += PlacedObject;
            PrefabSpawningController.RepositionPrefab += OnRepositionPrefab;

            ToggleMoveDeviceAnimation(true);


        }
        /// <summary>
        /// Removes listener to prefabSpawned and repostion.
        /// </summary>
        void OnDisable()
        {
            if (m_CameraManager != null)
                m_CameraManager.frameReceived -= FrameChanged;
            if (videoPlayerMoveDevice != null)
                videoPlayerMoveDevice.loopPointReached -= EndReached;

            PrefabSpawningController.prefabSpawned -= PlacedObject;
            PrefabSpawningController.RepositionPrefab -= OnRepositionPrefab;
        }
        /// <summary>
        /// Check on frame change if the moveDevice animation needs to played or the tapToPlace animation.
        /// </summary>
        /// <param name="args"></param>
        void FrameChanged(ARCameraFrameEventArgs args)
        {
            if (PrefabSpawningController.placementPoseIsValid && moveDeviceLoops>2)
            {
                ToggleMoveDeviceAnimation(false);
                ToggleTapToPlaceAnimation(true);
            }
            else if (!PrefabSpawningController.placementPoseIsValid)
            {
                ToggleTapToPlaceAnimation(false);
                ToggleMoveDeviceAnimation(true);

            }
        }
        /// <summary>
        /// Increases the mveDeviceLoops after the video is played.
        /// </summary>
        /// <param name="vp"></param>
        void EndReached(UnityEngine.Video.VideoPlayer vp)
        {
          moveDeviceLoops++;
        }

        /// <summary>
        /// The setup Prefab was spawned
        /// </summary>
        void PlacedObject()
        {
            //if (!displayTapToPlace) return;
            ToggleMoveDeviceAnimation(false);
            ToggleTapToPlaceAnimation(false);

            //Deactivate the videoframes
            videoFrameMoveDevice.SetActive(false);
            videoFrameTapToPlace.SetActive(false);

            //Clip onload automatically when the frame is disabled
            moveDeviceClipWasLoaded = false;
            TapToPlaceClipWasLoaded = false;
        }

        /// <summary>
        /// Starting the repositioningphase of the prefab spawning
        /// </summary>
        void OnRepositionPrefab()
        {
            //Activate the videoframes
            videoFrameMoveDevice.SetActive(true);
            videoFrameTapToPlace.SetActive(true);

            //Toggle the animation on
            ToggleMoveDeviceAnimation(true);
        }

        /// <summary>
        /// Function that is used to track the current reason for tracking problems
        /// </summary>
        void TrackingstateDependentOnboarding()
        {
            // A session can temporarily have no subsystem during startup or an
            // interruption. Clear stale warnings and read this frame's state once.
            var subsystem = session != null ? session.subsystem : null;
            if (subsystem == null)
            {
                stateText = "None";
                reasonText = "None";
                return;
            }

            stateText = subsystem.trackingState.ToString();
            reasonText = subsystem.notTrackingReason.ToString();
        }

        /// <summary>
        /// Shows overlay dependend on the tracking status
        /// </summary>
        void ShowTrackingInformation()
        {
            bool showTracking = PrefabSpawningController != null && PrefabSpawningController.objectWasSpawned;
            bool showDefault = showTracking && !string.IsNullOrEmpty(reasonText) && reasonText != "None" &&
                reasonText != "InsufficientLight" && reasonText != "InsufficientFeatures" && reasonText != "ExcessiveMotion";

            // The fallback panel uses a legacy UI Text. Keep its existing label,
            // and tolerate an omitted label without throwing every Update.
            if (showDefault && otherReasons != null && otherReasons.text != reasonText)
                otherReasons.text = reasonText;

            SetTrackingPanelActive(insufficientLight, showTracking && reasonText == "InsufficientLight");
            SetTrackingPanelActive(insufficientFeatures, showTracking && reasonText == "InsufficientFeatures");
            SetTrackingPanelActive(excessiveMotion, showTracking && reasonText == "ExcessiveMotion");
            SetTrackingPanelActive(defaultTracking, showDefault);

            if (!showTracking) return;

            switch (reasonText){
              case "InsufficientFeatures":
                  if(reasonText != tempTrackingReason)
                    Debug.Log("TrackingController: Lost tracking because of insufficient features");
                  break;

              case "InsufficientLight":
                  if(reasonText != tempTrackingReason)
                    Debug.Log("TrackingController: Lost tracking because of insufficient light");
                  break;

              case "ExcessiveMotion":
                  if(reasonText != tempTrackingReason)
                    Debug.Log("TrackingController: Lost tracking because of excessive motion");
                  break;

              default:
                  if(showDefault)
                  {
                    if(reasonText != tempTrackingReason)
                      Debug.Log("TrackingController: Lost tracking because of " + reasonText);
                  }
                  break;
            }
            tempTrackingReason = reasonText;
        }

        /// <summary>
        /// Shows overlay dependend on the tracking status
        /// </summary>
        void ShowTrackingInformationDuringOnboarding()
        {
            bool showOnboarding = PrefabSpawningController != null && !PrefabSpawningController.objectWasSpawned;
            SetTrackingPanelActive(insufficientLightOb, showOnboarding && reasonText == "InsufficientLight");
            SetTrackingPanelActive(insufficientFeaturesOb, showOnboarding && reasonText == "InsufficientFeatures");
            SetTrackingPanelActive(excessiveMotionOb, showOnboarding && reasonText == "ExcessiveMotion");

            if(showOnboarding)
            {
                switch (reasonText){
                    case "InsufficientFeatures":
                        if(reasonText != tempTrackingReason)
                            Debug.Log("TrackingController: Lost tracking because of insufficient features - during onboarding.");
                        break;

                    case "InsufficientLight":
                        if(reasonText != tempTrackingReason)
                            Debug.Log("TrackingController: Lost tracking because of insufficient light - during onboarding.");
                        break;

                    case "ExcessiveMotion":
                        if(reasonText != tempTrackingReason)
                            Debug.Log("TrackingController: Lost tracking because of excessive motion - during onboarding.");
                        break;
                }
                tempTrackingReason = reasonText;
            }
        }

        private static void SetTrackingPanelActive(GameObject panel, bool active)
        {
            // Avoid disabling/re-enabling a visible warning every frame, which
            // repeatedly rebuilds its UI while tracking is interrupted.
            if (panel != null && panel.activeSelf != active)
                panel.SetActive(active);
        }
    }
}
