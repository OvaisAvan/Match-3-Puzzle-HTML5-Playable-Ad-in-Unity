using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MatchBlitz.Managers
{
    /// <summary>
    /// Spawns and pools particle burst effects when gems are destroyed.
    /// Uses a simple GameObject pool to avoid GC spikes in WebGL.
    /// </summary>
    public class EffectsManager : MonoBehaviour
    {
        public static EffectsManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private ParticleSystem gemBurstPrefab;
        [SerializeField] private int            poolSize = 12;

        [Header("Score Label Popup")]
        [SerializeField] private GameObject matchLabelPrefab;   // "SWEET!" "COMBO!" etc.
        [SerializeField] private Transform  labelParent;

        private readonly Queue<ParticleSystem> pool = new();
        private readonly string[] comboLabels = { "NICE!", "SWEET!", "COMBO!", "AMAZING!" };

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Pre-warm pool
            for (int i = 0; i < poolSize; i++)
            {
                ParticleSystem ps = Instantiate(gemBurstPrefab, transform);
                ps.gameObject.SetActive(false);
                pool.Enqueue(ps);
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void SpawnMatchBurst(Vector3 worldPos, Color color)
        {
            ParticleSystem ps = GetFromPool();
            ps.transform.position = worldPos;

            // Tint the particles to match the gem colour
            var main = ps.main;
            main.startColor = color;

            ps.gameObject.SetActive(true);
            ps.Play();
            StartCoroutine(ReturnToPool(ps, main.duration + main.startLifetime.constantMax));
        }

        /// <summary>
        /// Shows a floating combo label ("NICE!", "SWEET!") at a world position.
        /// </summary>
        public void SpawnComboLabel(Vector3 worldPos, int comboCount)
        {
            if (matchLabelPrefab == null || labelParent == null) return;
            string label = comboLabels[Mathf.Clamp(comboCount - 1, 0, comboLabels.Length - 1)];
            GameObject go = Instantiate(matchLabelPrefab, labelParent);
            go.transform.position = worldPos;

            TMPro.TMP_Text txt = go.GetComponentInChildren<TMPro.TMP_Text>();
            if (txt) txt.text = label;

            StartCoroutine(FloatAndFade(go, 1.0f));
        }

        // ── Pool helpers ──────────────────────────────────────────────────────

        private ParticleSystem GetFromPool()
        {
            if (pool.Count > 0) return pool.Dequeue();
            // Pool exhausted — create extra (shouldn't happen in normal play)
            ParticleSystem ps = Instantiate(gemBurstPrefab, transform);
            return ps;
        }

        private IEnumerator ReturnToPool(ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);
            ps.Stop();
            ps.gameObject.SetActive(false);
            pool.Enqueue(ps);
        }

        private IEnumerator FloatAndFade(GameObject go, float duration)
        {
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            Vector3     startPos = go.transform.position;
            float       elapsed  = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                go.transform.position = startPos + Vector3.up * (1.5f * t);
                if (cg) cg.alpha = 1f - t;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Destroy(go);
        }
    }
}
