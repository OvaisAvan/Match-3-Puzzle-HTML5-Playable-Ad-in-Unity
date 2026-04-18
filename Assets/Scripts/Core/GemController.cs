using UnityEngine;

namespace MatchBlitz.Core
{
    public enum GemType  { Red, Blue, Green, Yellow, Purple, Orange }

    /// <summary>
    /// Represents one gem on the board.
    /// Holds type/color metadata and handles visual scale-pop animations.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class GemController : MonoBehaviour
    {
        [Header("Gem Data")]
        [SerializeField] private GemType  gemType;
        [SerializeField] private Color    gemColor = Color.white;

        [Header("Animation")]
        [SerializeField] private float spawnScaleDuration = 0.18f;
        [SerializeField] private AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private SpriteRenderer sr;
        private Vector2Int gridPos;

        public GemType GemType  => gemType;
        public Color   GemColor => gemColor;
        public Vector2Int GridPos => gridPos;

        private void Awake() => sr = GetComponent<SpriteRenderer>();

        public void Initialise(GemType type, Color color, Vector2Int pos)
        {
            gemType  = type;
            gemColor = color;
            gridPos  = pos;
            if (sr) sr.color = color;
            PlaySpawnAnim();
        }

        public void SetGridPos(Vector2Int pos) => gridPos = pos;

        // ── Animations ────────────────────────────────────────────────────────

        private void PlaySpawnAnim() =>
            StartCoroutine(ScalePop(Vector3.zero, Vector3.one, spawnScaleDuration));

        public void PlaySelectAnim() =>
            StartCoroutine(ScalePop(Vector3.one, Vector3.one * 1.2f, 0.1f, true));

        public void PlayDeselect() =>
            StartCoroutine(ScalePop(transform.localScale, Vector3.one, 0.1f));

        private System.Collections.IEnumerator ScalePop(Vector3 from, Vector3 to,
                                                          float duration, bool bounce = false)
        {
            float elapsed = 0f;
            transform.localScale = from;

            while (elapsed < duration)
            {
                float t = spawnCurve.Evaluate(elapsed / duration);
                transform.localScale = Vector3.Lerp(from, to, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = to;

            if (bounce)
                StartCoroutine(ScalePop(to, Vector3.one, 0.08f));
        }

        // ── Falling animation ─────────────────────────────────────────────────

        public System.Collections.IEnumerator FallTo(Vector3 targetWorld, float speed = 8f)
        {
            Vector3 start   = transform.position;
            float   dist    = Vector3.Distance(start, targetWorld);
            float   dur     = dist / speed;
            float   elapsed = 0f;

            while (elapsed < dur)
            {
                transform.position = Vector3.Lerp(start, targetWorld, elapsed / dur);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetWorld;
        }
    }
}
