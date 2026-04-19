using UnityEngine;
using System;
using System.Collections.Generic;
using MatchBlitz.Core;

namespace MatchBlitz.Managers
{
    [Serializable]
    public class BoardData
    {
        public string   boardName;
        public int      columns;
        public int      rows;
        public int[][]  tiles;
        public SwapHint tutorialHintSwap;
    }

    [Serializable]
    public class SwapHint
    {
        public int[] from;
        public int[] to;
    }

    /// <summary>
    /// Optionally loads a hand-crafted board layout from JSON
    /// instead of relying on procedural generation.
    /// This guarantees the player sees an immediate satisfying match.
    /// </summary>
    public class BoardLoader : MonoBehaviour
    {
        public static BoardLoader Instance { get; private set; }

        [SerializeField] private string boardResourcePath = "Levels/ad_board";
        [SerializeField] private bool   usePresetBoard    = true;

        public BoardData LoadedData { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// Loads board JSON and returns it. Returns null if disabled or missing.
        /// </summary>
        public BoardData LoadBoard()
        {
            if (!usePresetBoard) return null;

            TextAsset asset = Resources.Load<TextAsset>(boardResourcePath);
            if (asset == null)
            {
                Debug.LogWarning($"[BoardLoader] No board found at Resources/{boardResourcePath}");
                return null;
            }

            LoadedData = JsonUtility.FromJson<BoardData>(asset.text);
            Debug.Log($"[BoardLoader] Loaded board: {LoadedData.boardName} ({LoadedData.columns}x{LoadedData.rows})");
            return LoadedData;
        }

        /// <summary>
        /// Returns the tutorial hint swap as two grid positions (from, to).
        /// Returns false if no hint is defined.
        /// </summary>
        public bool TryGetTutorialHint(out Vector2Int from, out Vector2Int to)
        {
            from = to = Vector2Int.zero;
            if (LoadedData?.tutorialHintSwap == null) return false;
            from = new Vector2Int(LoadedData.tutorialHintSwap.from[0], LoadedData.tutorialHintSwap.from[1]);
            to   = new Vector2Int(LoadedData.tutorialHintSwap.to[0],   LoadedData.tutorialHintSwap.to[1]);
            return true;
        }
    }
}
