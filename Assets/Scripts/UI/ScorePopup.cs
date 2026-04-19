using UnityEngine;
using TMPro;

namespace MatchBlitz.UI
{
    /// <summary>
    /// Attach to the score popup prefab.
    /// AdUIManager drives the animation; this component just holds refs
    /// and provides a quick colour-by-points helper.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScorePopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        [Header("Colour Tiers")]
        [SerializeField] private Color tier1Color = new Color(1f, 1f, 0.4f);    // < 30 pts
        [SerializeField] private Color tier2Color = new Color(0.4f, 1f, 0.5f);  // 30–59 pts
        [SerializeField] private Color tier3Color = new Color(0.4f, 0.8f, 1f);  // 60+ pts

        public CanvasGroup   CanvasGroup   { get; private set; }
        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            CanvasGroup   = GetComponent<CanvasGroup>();
            RectTransform = GetComponent<RectTransform>();
        }

        public void Setup(int points)
        {
            if (label) label.text = $"+{points}";

            Color c = points < 30  ? tier1Color
                    : points < 60  ? tier2Color
                    : tier3Color;

            if (label) label.color = c;
        }
    }
}
