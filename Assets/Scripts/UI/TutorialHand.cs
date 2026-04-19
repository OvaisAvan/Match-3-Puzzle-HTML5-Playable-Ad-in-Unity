using UnityEngine;
using System.Collections;

namespace MatchBlitz.UI
{
    /// <summary>
    /// Shows an animated hand icon that demonstrates a swap gesture
    /// for the first few seconds of gameplay.
    /// Finds the best pre-seeded swap on the board and animates toward it.
    /// </summary>
    public class TutorialHand : MonoBehaviour
    {
        public static TutorialHand Instance { get; private set; }

        [Header("References")]
        [SerializeField] private RectTransform handTransform;
        [SerializeField] private CanvasGroup   canvasGroup;

        [Header("Hint Settings")]
        [SerializeField] private float showDuration    = 3.5f;   // hide after this many seconds
        [SerializeField] private float swipeDistance   = 80f;    // pixels on canvas
        [SerializeField] private float animDuration    = 0.6f;
        [SerializeField] private float pauseAtEnd      = 0.3f;

        [Header("Hint Position (Canvas)")]
        [SerializeField] private Vector2 hintStartPos = new Vector2(-40f, -60f);
        [SerializeField] private Vector2 hintEndPos   = new Vector2(40f,  -60f);

        private Coroutine loopRoutine;
        private Coroutine hideRoutine;
        private bool      visible;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start() => SetAlpha(0f);

        public void Show()
        {
            if (visible) return;
            visible = true;
            SetAlpha(1f);
            loopRoutine = StartCoroutine(LoopSwipeAnim());
            hideRoutine = StartCoroutine(AutoHide());
        }

        public void Hide()
        {
            if (!visible) return;
            visible = false;
            if (loopRoutine != null) StopCoroutine(loopRoutine);
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            StartCoroutine(FadeOut(0.25f));
        }

        // ── Animation ─────────────────────────────────────────────────────────

        private IEnumerator LoopSwipeAnim()
        {
            while (true)
            {
                // Slide right (gesture)
                yield return StartCoroutine(MoveTo(hintStartPos, hintEndPos, animDuration));
                yield return new WaitForSeconds(pauseAtEnd);

                // Snap back
                SetPos(hintStartPos);
                yield return new WaitForSeconds(0.15f);
            }
        }

        private IEnumerator MoveTo(Vector2 from, Vector2 to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                SetPos(Vector2.Lerp(from, to, t));
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetPos(to);
        }

        private IEnumerator AutoHide()
        {
            yield return new WaitForSeconds(showDuration);
            Hide();
        }

        private IEnumerator FadeOut(float duration)
        {
            float start = canvasGroup ? canvasGroup.alpha : 1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                SetAlpha(Mathf.Lerp(start, 0f, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetAlpha(0f);
        }

        private void SetAlpha(float a) { if (canvasGroup) canvasGroup.alpha = a; }
        private void SetPos(Vector2 p)  { if (handTransform) handTransform.anchoredPosition = p; }
    }
}
