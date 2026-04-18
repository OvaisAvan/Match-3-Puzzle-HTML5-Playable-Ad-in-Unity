using UnityEngine;

namespace MatchBlitz.Core
{
    /// <summary>
    /// ScriptableObject that maps each GemType to its sprite and colour.
    /// Create via: Assets → Create → MatchBlitz → GemColorConfig
    /// Assign to BoardFiller in the Inspector.
    /// </summary>
    [CreateAssetMenu(menuName = "MatchBlitz/GemColorConfig", fileName = "GemColorConfig")]
    public class GemColorConfig : ScriptableObject
    {
        [System.Serializable]
        public struct GemEntry
        {
            public GemType type;
            public Color   color;
            public Sprite  sprite;
            public Sprite  glowSprite;    // used when gem is on a goal / selected
        }

        [SerializeField] private GemEntry[] entries;

        public GemEntry Get(GemType type)
        {
            foreach (var e in entries)
                if (e.type == type) return e;

            Debug.LogWarning($"[GemColorConfig] No entry for {type}, returning default.");
            return new GemEntry { type = type, color = Color.white };
        }

        public int Count => entries?.Length ?? 0;

        // Editor helper
        private void OnValidate()
        {
            if (entries == null) return;
            // Assign default colours if none set
            Color[] defaults = {
                new Color(0.95f, 0.25f, 0.25f), // Red
                new Color(0.25f, 0.55f, 0.95f), // Blue
                new Color(0.25f, 0.85f, 0.35f), // Green
                new Color(0.97f, 0.85f, 0.15f), // Yellow
                new Color(0.65f, 0.25f, 0.95f), // Purple
                new Color(0.97f, 0.55f, 0.10f), // Orange
            };
            for (int i = 0; i < entries.Length; i++)
                if (entries[i].color == Color.clear)
                    entries[i].color = i < defaults.Length ? defaults[i] : Color.white;
        }
    }
}
