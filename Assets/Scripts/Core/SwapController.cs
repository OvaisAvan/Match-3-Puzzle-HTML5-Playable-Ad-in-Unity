using UnityEngine;
using MatchBlitz.Ad;

namespace MatchBlitz.Core
{
    /// <summary>
    /// Handles mouse and touch input for gem swapping.
    /// Detects drag direction from a selected gem and requests a swap.
    /// </summary>
    public class SwapController : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private float dragThreshold = 0.3f;   // world units before swap fires
        [SerializeField] private LayerMask gemLayer;

        private GemController selectedGem;
        private Vector3       dragStartWorld;
        private bool          hasFired;
        private Camera        cam;

        private void Awake() => cam = Camera.main;

        private void Update()
        {
            if (AdController.Instance != null &&
                AdController.Instance.CurrentPhase != AdPhase.Playing) return;

            if (GameBoard.Instance.IsBusy) return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        // ── Mouse ─────────────────────────────────────────────────────────────

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
                OnPointerDown(Input.mousePosition);
            else if (Input.GetMouseButton(0) && selectedGem != null)
                OnPointerDrag(Input.mousePosition);
            else if (Input.GetMouseButtonUp(0))
                OnPointerUp();
        }

        // ── Touch ─────────────────────────────────────────────────────────────

        private void HandleTouchInput()
        {
            if (Input.touchCount == 0) return;
            Touch t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:   OnPointerDown(t.position); break;
                case TouchPhase.Moved:   OnPointerDrag(t.position); break;
                case TouchPhase.Ended:   OnPointerUp();             break;
                case TouchPhase.Canceled: ClearSelection();         break;
            }
        }

        // ── Pointer Handlers ──────────────────────────────────────────────────

        private void OnPointerDown(Vector2 screenPos)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, gemLayer);
            if (!hit.collider) return;

            selectedGem   = hit.collider.GetComponent<GemController>();
            dragStartWorld = cam.ScreenToWorldPoint(screenPos);
            dragStartWorld.z = 0f;
            hasFired = false;
            selectedGem?.PlaySelectAnim();
        }

        private void OnPointerDrag(Vector2 screenPos)
        {
            if (selectedGem == null || hasFired) return;

            Vector3 current = cam.ScreenToWorldPoint(screenPos);
            current.z = 0f;
            Vector3 delta = current - dragStartWorld;

            if (delta.magnitude < dragThreshold) return;

            Vector2Int dir = GetDominantDir(delta);
            Vector2Int target = selectedGem.GridPos + dir;

            GameBoard.Instance.RequestSwap(selectedGem.GridPos, target);
            hasFired = true;
            ClearSelection();
        }

        private void OnPointerUp() => ClearSelection();

        private void ClearSelection()
        {
            selectedGem?.PlayDeselect();
            selectedGem = null;
            hasFired    = false;
        }

        private Vector2Int GetDominantDir(Vector3 delta)
        {
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }
}
