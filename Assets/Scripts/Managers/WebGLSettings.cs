using UnityEngine;

namespace MatchBlitz.Managers
{
    /// <summary>
    /// Applies performance and compatibility settings needed for
    /// WebGL playable ad builds at runtime.
    /// Attach to the root Manager GameObject in the scene.
    /// </summary>
    public class WebGLSettings : MonoBehaviour
    {
        [Header("Performance")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool disableVSync   = true;

        [Header("Audio")]
        [Tooltip("WebGL requires a user gesture before audio can play. " +
                 "This flag delays the first audio until after the first tap.")]
        [SerializeField] private bool waitForGestureBeforeAudio = true;

        private static bool audioUnlocked;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;

            if (disableVSync)
                QualitySettings.vSyncCount = 0;

            // Reduce texture memory — ads must stay under ~5 MB compressed
            QualitySettings.globalTextureMipmapLimit = 0;

            // Prevent sleep on mobile browsers
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Debug.Log("[WebGLSettings] Applied WebGL ad build settings.");
        }

        private void Update()
        {
            // Unlock audio on first user interaction
            if (!audioUnlocked && waitForGestureBeforeAudio)
            {
                bool tapped = Input.GetMouseButtonDown(0) || Input.touchCount > 0;
                if (tapped)
                {
                    audioUnlocked = true;
                    AudioListener.volume = 1f;
                    Debug.Log("[WebGLSettings] Audio unlocked by user gesture.");
                }
            }
        }

        private void OnEnable()
        {
            // Mute until gesture on WebGL
#if UNITY_WEBGL && !UNITY_EDITOR
            if (waitForGestureBeforeAudio) AudioListener.volume = 0f;
#endif
        }
    }
}
