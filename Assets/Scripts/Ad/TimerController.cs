using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MatchBlitz.Ad;

namespace MatchBlitz.Ad
{
    /// <summary>
    /// Drives the countdown timer display.
    /// Uses a radial Image fill + TMP label.
    /// Pulses red in the final 3 seconds.
    /// </summary>
    public class TimerController : MonoBehaviour
    {
        public static TimerController Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private TMP_Text   timerLabel;
        [SerializeField] private Image      radialFill;       // Image type = Filled, Radial 360

        [Header("Visual Feedback")]
        [SerializeField] private Color normalColor  = Color.white;
        [SerializeField] private Color urgentColor  = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private float urgentThreshold = 5f;             // seconds
        [SerializeField] private float pulseSpeed       = 3f;

        private float totalTime;
        private bool  pulsing;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialise(float duration)
        {
            totalTime = duration;
            UpdateDisplay(duration);
        }

        public void UpdateDisplay(float remaining)
        {
            remaining = Mathf.Max(0f, remaining);

            // Label: ceil so "1" shows for the last second
            int secs = Mathf.CeilToInt(remaining);
            if (timerLabel) timerLabel.text = secs.ToString();

            // Radial fill
            if (radialFill) radialFill.fillAmount = remaining / totalTime;

            // Colour urgency
            bool urgent = remaining <= urgentThreshold;
            Color target = urgent
                ? Color.Lerp(normalColor, urgentColor, Mathf.PingPong(Time.time * pulseSpeed, 1f))
                : normalColor;

            if (timerLabel) timerLabel.color = target;
            if (radialFill)  radialFill.color  = target;
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
