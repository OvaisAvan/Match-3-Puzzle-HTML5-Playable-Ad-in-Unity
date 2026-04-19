using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using MatchBlitz.Ad;

namespace MatchBlitz.UI
{
    /// <summary>
    /// Manages all UI elements in the playable ad:
    ///   - Score counter and animated score popups
    ///   - Timer display (delegates to TimerController)
    ///   - CTA overlay with install button
    ///   - "Tap to play" hint
    /// </summary>
    public class AdUIManager : MonoBehaviour
    {
        public static AdUIManager Instance { get; private set; }

        [Header("HUD")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private GameObject scorePopupPrefab;   // floats up and fades
        [SerializeField] private Canvas     hudCanvas;

        [Header("Timer")]
        [SerializeField] private TimerController timerController;

        [Header("CTA Overlay")]
        [SerializeField] private GameObject ctaPanel;
        [SerializeField] private Button     installButton;
        [SerializeField] private TMP_Text   installButtonLabel;
        [SerializeField] private TMP_Text   finalScoreText;
        [SerializeField] private CanvasGroup ctaCanvasGroup;

        [Header("Tutorial")]
        [SerializeField] private GameObject tapHint;            // "Tap & drag to swap"

        [Header("CTA Copy")]
        [SerializeField] private string ctaButtonText  = "INSTALL FREE";
        [SerializeField] private string ctaScorePrefix = "Score: ";

        private int currentScore;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            ctaPanel?.SetActive(false);
            if (tapHint) tapHint.SetActive(false);
            UpdateScoreDisplay();
            if (installButton)
                installButton.onClick.AddListener(() => AdController.Instance?.OnInstallTapped());
            if (installButtonLabel) installButtonLabel.text = ctaButtonText;
        }

        // ── Called by AdController ────────────────────────────────────────────

        public void OnAdStarted()
        {
            timerController?.Initialise(AdController.Instance.PlayDuration);
            if (tapHint) tapHint.SetActive(true);
        }

        public void UpdateTimer(float remaining)
        {
            timerController?.UpdateDisplay(remaining);
        }

        public void ShowCTAOverlay()
        {
            timerController?.Hide();
            if (tapHint) tapHint.SetActive(false);
            if (finalScoreText) finalScoreText.text = ctaScorePrefix + currentScore;
            ctaPanel?.SetActive(true);
            if (ctaCanvasGroup) StartCoroutine(FadeIn(ctaCanvasGroup, 0.35f));
        }

        public void HideTapHint() { if (tapHint) tapHint.SetActive(false); }

        // ── Score ─────────────────────────────────────────────────────────────

        public void AddScore(int points)
        {
            currentScore += points;
            UpdateScoreDisplay();
            SpawnScorePopup(points);
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText) scoreText.text = currentScore.ToString("N0");
        }

        private void SpawnScorePopup(int points)
        {
            if (scorePopupPrefab == null || hudCanvas == null) return;
            GameObject popup = Instantiate(scorePopupPrefab, hudCanvas.transform);
            TMP_Text   label = popup.GetComponentInChildren<TMP_Text>();
            if (label) label.text = $"+{points}";
            StartCoroutine(AnimatePopup(popup));
        }

        private IEnumerator AnimatePopup(GameObject popup)
        {
            CanvasGroup cg  = popup.GetComponent<CanvasGroup>();
            RectTransform rt = popup.GetComponent<RectTransform>();
            Vector2 start    = rt.anchoredPosition;
            float duration   = 0.9f, elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                rt.anchoredPosition = start + Vector2.up * (60f * t);
                if (cg) cg.alpha = 1f - t;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Destroy(popup);
        }

        // ── Utilities ─────────────────────────────────────────────────────────

        private IEnumerator FadeIn(CanvasGroup cg, float duration)
        {
            cg.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                cg.alpha = elapsed / duration;
                elapsed += Time.deltaTime;
                yield return null;
            }
            cg.alpha = 1f;
        }
    }
}
