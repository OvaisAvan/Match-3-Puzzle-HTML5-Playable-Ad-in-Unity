using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MatchBlitz.Core
{
    /// <summary>
    /// Handles board population, gravity (gems fall into gaps),
    /// and refilling empty columns from the top.
    /// </summary>
    public class BoardFiller : MonoBehaviour
    {
        public static BoardFiller Instance { get; private set; }

        [SerializeField] private float fallSpeed  = 9f;
        [SerializeField] private float spawnYOffset = 8f;   // world units above board top

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Initial Fill ──────────────────────────────────────────────────────

        /// <summary>
        /// Populates the entire grid with random gems, avoiding pre-made matches.
        /// </summary>
        public void FillBoard(GemController[,] grid, GemController[] prefabs,
                              Transform parent, int cols, int rows, float cellSize)
        {
            for (int x = 0; x < cols; x++)
                for (int y = 0; y < rows; y++)
                    if (grid[x, y] == null)
                        grid[x, y] = SpawnGem(prefabs, parent, new Vector2Int(x, y),
                                              GameBoard.Instance.GridToWorld(new Vector2Int(x, y)),
                                              cols, rows, grid);
        }

        // ── Gravity ───────────────────────────────────────────────────────────

        /// <summary>
        /// Slides gems down to fill gaps. Returns when all animations finish.
        /// </summary>
        public IEnumerator ApplyGravity(GemController[,] grid, int cols, int rows, float cellSize)
        {
            List<Coroutine> falls = new();

            for (int x = 0; x < cols; x++)
            {
                int writeY = 0;
                for (int readY = 0; readY < rows; readY++)
                {
                    if (grid[x, readY] == null) continue;
                    if (readY != writeY)
                    {
                        grid[x, writeY] = grid[x, readY];
                        grid[x, readY]  = null;
                        grid[x, writeY].SetGridPos(new Vector2Int(x, writeY));
                        Vector3 target = GameBoard.Instance.GridToWorld(new Vector2Int(x, writeY));
                        falls.Add(StartCoroutine(grid[x, writeY].FallTo(target)));
                    }
                    writeY++;
                }
            }

            // Wait for all falls
            foreach (var c in falls) yield return c;
        }

        // ── Refill ────────────────────────────────────────────────────────────

        /// <summary>
        /// Spawns new gems above the board for every empty cell, then falls them in.
        /// </summary>
        public IEnumerator RefillBoard(GemController[,] grid, GemController[] prefabs,
                                       Transform parent, int cols, int rows, float cellSize)
        {
            List<Coroutine> falls = new();

            for (int x = 0; x < cols; x++)
            {
                int spawnOffset = 0;
                for (int y = rows - 1; y >= 0; y--)
                {
                    if (grid[x, y] != null) continue;

                    // Spawn above board
                    Vector3 spawnPos = GameBoard.Instance.GridToWorld(new Vector2Int(x, rows - 1))
                                       + Vector3.up * (spawnYOffset + spawnOffset * 1.1f);
                    Vector3 targetPos = GameBoard.Instance.GridToWorld(new Vector2Int(x, y));

                    GemController gem = SpawnGem(prefabs, parent, new Vector2Int(x, y),
                                                 spawnPos, cols, rows, grid);
                    grid[x, y] = gem;
                    falls.Add(StartCoroutine(gem.FallTo(targetPos)));
                    spawnOffset++;
                }
            }

            foreach (var c in falls) yield return c;
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private GemController SpawnGem(GemController[] prefabs, Transform parent,
                                        Vector2Int gridPos, Vector3 worldPos,
                                        int cols, int rows, GemController[,] grid)
        {
            int typeIndex  = PickSafeType(prefabs.Length, gridPos, cols, rows, grid);
            GemController prefab = prefabs[typeIndex];

            GemController gem = Instantiate(prefab, worldPos, Quaternion.identity, parent);
            gem.Initialise(prefab.GemType, prefab.GemColor, gridPos);
            gem.name = $"Gem_{gridPos.x}_{gridPos.y}";
            return gem;
        }

        /// <summary>
        /// Picks a gem type that won't create an immediate 3-in-a-row at gridPos.
        /// Falls back to random after 20 attempts.
        /// </summary>
        private int PickSafeType(int typeCount, Vector2Int pos,
                                  int cols, int rows, GemController[,] grid)
        {
            List<int> candidates = new();
            for (int i = 0; i < typeCount; i++) candidates.Add(i);

            for (int attempt = 0; attempt < 20; attempt++)
            {
                int idx = candidates[Random.Range(0, candidates.Count)];
                GemType t = (GemType)idx;
                if (!WouldMakeMatch(t, pos, cols, rows, grid)) return idx;
                candidates.Remove(idx);
                if (candidates.Count == 0) break;
            }
            return Random.Range(0, typeCount);
        }

        private bool WouldMakeMatch(GemType type, Vector2Int pos,
                                     int cols, int rows, GemController[,] grid)
        {
            // Horizontal check
            if (pos.x >= 2)
            {
                var l1 = grid[pos.x - 1, pos.y];
                var l2 = grid[pos.x - 2, pos.y];
                if (l1 != null && l2 != null && l1.GemType == type && l2.GemType == type)
                    return true;
            }
            // Vertical check
            if (pos.y >= 2)
            {
                var d1 = grid[pos.x, pos.y - 1];
                var d2 = grid[pos.x, pos.y - 2];
                if (d1 != null && d2 != null && d1.GemType == type && d2.GemType == type)
                    return true;
            }
            return false;
        }
    }
}
