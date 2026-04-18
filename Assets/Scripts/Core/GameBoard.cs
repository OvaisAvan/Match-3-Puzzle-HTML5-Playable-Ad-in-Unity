using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MatchBlitz.Core;
using MatchBlitz.Ad;
using MatchBlitz.UI;

namespace MatchBlitz.Core
{
    /// <summary>
    /// Central authority for the Match-3 board.
    /// Owns the gem grid, orchestrates swap → detect → destroy → fill loop.
    /// </summary>
    public class GameBoard : MonoBehaviour
    {
        public static GameBoard Instance { get; private set; }

        [Header("Board Config")]
        [SerializeField] private int columns = 7;
        [SerializeField] private int rows    = 7;
        [SerializeField] private float cellSize = 1.0f;

        [Header("Prefabs")]
        [SerializeField] private GemController[] gemPrefabs;   // one per gem type (5–6 types)

        [Header("Parents")]
        [SerializeField] private Transform gemParent;

        public int Columns => columns;
        public int Rows    => rows;
        public float CellSize => cellSize;

        private GemController[,] grid;
        private bool isBusy;          // blocks input during animations

        public bool IsBusy => isBusy;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            grid = new GemController[columns, rows];
            StartCoroutine(InitialFill());
        }

        // ── Initialisation ────────────────────────────────────────────────────

        private IEnumerator InitialFill()
        {
            isBusy = true;
            BoardFiller.Instance.FillBoard(grid, gemPrefabs, gemParent, columns, rows, cellSize);
            yield return new WaitForSeconds(0.3f);

            // Resolve any accidental matches in the starting board
            yield return StartCoroutine(ResolveBoard(silent: true));

            isBusy = false;
            TutorialHand.Instance?.Show();
        }

        // ── Swap API (called by SwapController) ───────────────────────────────

        public void RequestSwap(Vector2Int a, Vector2Int b)
        {
            if (isBusy) return;
            if (!IsAdjacent(a, b)) return;
            StartCoroutine(DoSwap(a, b));
        }

        private IEnumerator DoSwap(Vector2Int a, Vector2Int b)
        {
            isBusy = true;
            TutorialHand.Instance?.Hide();

            SwapInGrid(a, b);
            yield return StartCoroutine(AnimateSwap(a, b));

            List<List<Vector2Int>> matches = MatchDetector.Instance.FindAllMatches(grid, columns, rows);

            if (matches.Count == 0)
            {
                // Invalid swap — slide back
                SwapInGrid(a, b);
                yield return StartCoroutine(AnimateSwap(a, b));
            }
            else
            {
                yield return StartCoroutine(ResolveBoard());
            }

            isBusy = false;
        }

        // ── Board Resolution Loop ─────────────────────────────────────────────

        private IEnumerator ResolveBoard(bool silent = false)
        {
            List<List<Vector2Int>> matches;

            do
            {
                matches = MatchDetector.Instance.FindAllMatches(grid, columns, rows);
                if (matches.Count == 0) break;

                int gemsDestroyed = DestroyMatches(matches, silent);
                if (!silent) AdUIManager.Instance?.AddScore(gemsDestroyed * 10);

                yield return new WaitForSeconds(0.25f);

                yield return StartCoroutine(BoardFiller.Instance.ApplyGravity(grid, columns, rows, cellSize));
                yield return new WaitForSeconds(0.15f);

                yield return StartCoroutine(BoardFiller.Instance.RefillBoard(grid, gemPrefabs, gemParent, columns, rows, cellSize));
                yield return new WaitForSeconds(0.2f);

            } while (matches.Count > 0);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private int DestroyMatches(List<List<Vector2Int>> matches, bool silent)
        {
            HashSet<Vector2Int> toDestroy = new();
            foreach (var match in matches)
                foreach (var pos in match)
                    toDestroy.Add(pos);

            foreach (var pos in toDestroy)
            {
                if (grid[pos.x, pos.y] == null) continue;
                if (!silent)
                {
                    EffectsManager.Instance?.SpawnMatchBurst(GridToWorld(pos), grid[pos.x, pos.y].GemColor);
                    MatchBlitz.Managers.AudioManager.Instance?.PlayMatchPop();
                }
                Destroy(grid[pos.x, pos.y].gameObject);
                grid[pos.x, pos.y] = null;
            }
            return toDestroy.Count;
        }

        private void SwapInGrid(Vector2Int a, Vector2Int b)
        {
            GemController temp = grid[a.x, a.y];
            grid[a.x, a.y]    = grid[b.x, b.y];
            grid[b.x, b.y]    = temp;

            if (grid[a.x, a.y] != null) grid[a.x, a.y].SetGridPos(a);
            if (grid[b.x, b.y] != null) grid[b.x, b.y].SetGridPos(b);
        }

        private IEnumerator AnimateSwap(Vector2Int a, Vector2Int b)
        {
            GemController gemA = grid[a.x, a.y];
            GemController gemB = grid[b.x, b.y];

            Vector3 worldA = GridToWorld(a);
            Vector3 worldB = GridToWorld(b);

            float duration = 0.2f, elapsed = 0f;
            Vector3 startA = gemA ? gemA.transform.position : worldA;
            Vector3 startB = gemB ? gemB.transform.position : worldB;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                if (gemA) gemA.transform.position = Vector3.Lerp(startA, worldB, t);
                if (gemB) gemB.transform.position = Vector3.Lerp(startB, worldA, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (gemA) gemA.transform.position = worldB;
            if (gemB) gemB.transform.position = worldA;
        }

        private bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        public Vector3 GridToWorld(Vector2Int pos) =>
            new Vector3((pos.x - columns / 2f + 0.5f) * cellSize,
                        (pos.y - rows    / 2f + 0.5f) * cellSize, 0f);

        public Vector2Int WorldToGrid(Vector3 world) =>
            new Vector2Int(
                Mathf.RoundToInt(world.x / cellSize + columns / 2f - 0.5f),
                Mathf.RoundToInt(world.y / cellSize + rows    / 2f - 0.5f));

        public bool IsInBounds(Vector2Int pos) =>
            pos.x >= 0 && pos.x < columns && pos.y >= 0 && pos.y < rows;

        public GemController GetGem(Vector2Int pos) =>
            IsInBounds(pos) ? grid[pos.x, pos.y] : null;

        public void SetGem(Vector2Int pos, GemController gem)
        {
            if (IsInBounds(pos)) grid[pos.x, pos.y] = gem;
        }
    }
}
