using UnityEngine;
using System.Collections;
using MatchBlitz.UI;

namespace MatchBlitz.Ad
{
    public enum AdPhase { Initialising, Playing, CTAShown }

    /// <summary>
    /// Master controller for the playable ad lifecycle.
    ///
    /// Flow:
    ///   Initialising (0.5s) → Playing (timer countdown) → CTAShown
    ///
    /// Fires JS bridge calls so ad networks can track events.
    /// Compatible with: Mintegral, AppLovin MAX, Unity Ads,
    ///                  IronSource, Vungle, and any MRAID-compliant network.
    /// </summary>
    public class AdController : MonoBehaviour
    {
        public static AdController Instance { get; private set; }

        [Header("Ad Settings")]
        [SerializeField] private float playDuration = 15f;   // seconds before forced CTA
        [SerializeField] private bool  forceCtaOnTimerEnd = true;

        public AdPhase CurrentPhase  { get; private set; } = AdPhase.Initialising;
        public float   TimeRemaining { get; private set; }
        public float   PlayDuration  => playDuration;

        private bool ctaShown;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private IEnumerator Start()
        {
            // Brief init phase — board fills during this time
            yield return new WaitForSeconds(0.5f);
            StartPlaying();
        }

        // ── Phase Transitions ─────────────────────────────────────────────────

        private void StartPlaying()
        {
            CurrentPhase  = AdPhase.Playing;
            TimeRemaining = playDuration;
            FireJsBridge("adStarted");
            AdUIManager.Instance?.OnAdStarted();
            TutorialHand.Instance?.Show();
            StartCoroutine(Countdown());
        }

        private IEnumerator Countdown()
        {
            while (TimeRemaining > 0f && CurrentPhase == AdPhase.Playing)
            {
                TimeRemaining -= Time.deltaTime;
                AdUIManager.Instance?.UpdateTimer(TimeRemaining);
                yield return null;
            }

            if (forceCtaOnTimerEnd && !ctaShown)
                ShowCTA();
        }

        /// <summary>
        /// Call this to immediately show the CTA (e.g. after a satisfying match).
        /// </summary>
        public void ShowCTA()
        {
            if (ctaShown) return;
            ctaShown     = true;
            CurrentPhase = AdPhase.CTAShown;
            FireJsBridge("adCompleted");
            Managers.AudioManager.Instance?.PlayCtaJingle();
            AdUIManager.Instance?.ShowCTAOverlay();
            TutorialHand.Instance?.Hide();
        }

        /// <summary>
        /// Called when the user taps the Install / Download button.
        /// </summary>
        public void OnInstallTapped()
        {
            FireJsBridge("installClicked");
            CTAHandler.Instance?.OpenStoreUrl();
        }

        // ── JS Bridge ─────────────────────────────────────────────────────────

        /// <summary>
        /// Sends events to the hosting ad network via a JS call.
        /// Works in WebGL builds; safely no-ops in editor.
        /// </summary>
        private void FireJsBridge(string eventName)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            PlayableAdBridge.Fire(eventName);
#else
            Debug.Log($"[AdController] JS Bridge → {eventName}");
#endif
        }
    }
}
