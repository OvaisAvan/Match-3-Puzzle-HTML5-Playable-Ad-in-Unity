using UnityEngine;
using System.Collections.Generic;

namespace MatchBlitz.Core
{
    /// <summary>
    /// Stateless match-finding service.
    /// Detects horizontal and vertical runs of 3+ same-type gems.
    /// Overlapping runs are returned as separate match groups so
    /// the caller can calculate bonus scores for L/T shapes.
    /// </summary>
    public class MatchDetector : MonoBehaviour
    {
        public static MatchDetector Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// Returns a list of match groups. Each group is a list of grid positions.
        /// </summary>
        public List<List<Vector2Int>> FindAllMatches(GemController[,] grid, int cols, int rows)
        {
            List<List<Vector2Int>> results = new();
            results.AddRange(FindHorizontalMatches(grid, cols, rows));
            results.AddRange(FindVerticalMatches(grid, cols, rows));
            return results;
        }

        // ── Horizontal ────────────────────────────────────────────────────────

        private List<List<Vector2Int>> FindHorizontalMatches(GemController[,] grid, int cols, int rows)
        {
            var results = new List<List<Vector2Int>>();

            for (int y = 0; y < rows; y++)
            {
                int x = 0;
                while (x < cols - 2)
                {
                    GemController gem = grid[x, y];
                    if (gem == null) { x++; continue; }

                    List<Vector2Int> run = new() { new Vector2Int(x, y) };
                    int nx = x + 1;

                    while (nx < cols && grid[nx, y] != null &&
                           grid[nx, y].GemType == gem.GemType)
                    {
                        run.Add(new Vector2Int(nx, y));
                        nx++;
                    }

                    if (run.Count >= 3) results.Add(run);
                    x = nx;
                }
            }
            return results;
        }

        // ── Vertical ──────────────────────────────────────────────────────────

        private List<List<Vector2Int>> FindVerticalMatches(GemController[,] grid, int cols, int rows)
        {
            var results = new List<List<Vector2Int>>();

            for (int x = 0; x < cols; x++)
            {
                int y = 0;
                while (y < rows - 2)
                {
                    GemController gem = grid[x, y];
                    if (gem == null) { y++; continue; }

                    List<Vector2Int> run = new() { new Vector2Int(x, y) };
                    int ny = y + 1;

                    while (ny < rows && grid[x, ny] != null &&
                           grid[x, ny].GemType == gem.GemType)
                    {
                        run.Add(new Vector2Int(x, ny));
                        ny++;
                    }

                    if (run.Count >= 3) results.Add(run);
                    y = ny;
                }
            }
            return results;
        }

        // ── Shape classifier (for scoring / UI feedback) ──────────────────────

        public enum MatchShape { Line3, Line4, Line5Plus, LShape, TShape }

        /// <summary>
        /// Classifies the combined shape of two overlapping match groups.
        /// Used for bonus score display.
        /// </summary>
        public MatchShape ClassifyShape(List<Vector2Int> h, List<Vector2Int> v)
        {
            if (h == null && v == null) return MatchShape.Line3;
            if (h == null) return v.Count >= 5 ? MatchShape.Line5Plus : v.Count == 4 ? MatchShape.Line4 : MatchShape.Line3;
            if (v == null) return h.Count >= 5 ? MatchShape.Line5Plus : h.Count == 4 ? MatchShape.Line4 : MatchShape.Line3;

            // Both present → L or T
            HashSet<Vector2Int> hSet = new(h), vSet = new(v);
            int shared = 0;
            foreach (var p in hSet) if (vSet.Contains(p)) shared++;
            return shared == 1 ? MatchShape.LShape : MatchShape.TShape;
        }
    }
}
