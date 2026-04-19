using UnityEngine;

namespace MatchBlitz.Managers
{
    /// <summary>
    /// Adjusts the orthographic camera size so the game board
    /// always fits within the screen with optional padding.
    /// Run once at Start (board size is fixed for an ad).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        [Header("Fit Settings")]
        [SerializeField] private float paddingUnits = 1.2f;   // world-unit margin around board

        [Header("References")]
        [SerializeField] private Core.GameBoard board;

        private Camera cam;

        private void Awake() => cam = GetComponent<Camera>();

        private void Start()
        {
            if (board == null) board = Core.GameBoard.Instance;
            if (board == null || cam == null) return;
            FitCamera();
        }

        private void FitCamera()
        {
            float boardW = board.Columns * board.CellSize;
            float boardH = board.Rows    * board.CellSize;

            float aspectRatio  = (float)Screen.width / Screen.height;
            float sizeForHeight = (boardH / 2f) + paddingUnits;
            float sizeForWidth  = (boardW / 2f) / aspectRatio + paddingUnits;

            cam.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth);

            // Centre on board
            float centreX = 0f; // board is centred at origin
            float centreY = 0f;
            transform.position = new Vector3(centreX, centreY, -10f);

            Debug.Log($"[CameraFit] OrthoSize set to {cam.orthographicSize:F2}");
        }

#if UNITY_EDITOR
        private void OnValidate() { if (Application.isPlaying) FitCamera(); }
#endif
    }
}
