using UnityEngine;

namespace MatchBlitz.Ad
{
    /// <summary>
    /// Manages the store URL and opens it when the user taps Install.
    /// Supports both iOS App Store and Google Play deep links.
    /// The active URL is set via the Inspector or via a JS postMessage
    /// so the same build can be reused across platforms.
    /// </summary>
    public class CTAHandler : MonoBehaviour
    {
        public static CTAHandler Instance { get; private set; }

        [Header("Store URLs")]
        [Tooltip("Full App Store / Google Play URL for your game")]
        [SerializeField] private string storeUrl = "https://play.google.com/store/apps/details?id=com.yourcompany.yourgame";

        [Header("Fallback")]
        [SerializeField] private string fallbackUrl = "https://www.yourgame.com";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── URL Override from JS ───────────────────────────────────────────────

        /// <summary>
        /// Called from JavaScript to override the store URL at runtime.
        /// Usage in JS: unityInstance.SendMessage('CTAHandler', 'SetStoreUrl', 'https://...');
        /// </summary>
        public void SetStoreUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                storeUrl = url;
                Debug.Log($"[CTAHandler] Store URL overridden to: {url}");
            }
        }

        // ── Open Store ────────────────────────────────────────────────────────

        public void OpenStoreUrl()
        {
            string url = string.IsNullOrEmpty(storeUrl) ? fallbackUrl : storeUrl;

#if UNITY_WEBGL && !UNITY_EDITOR
            // In WebGL, open in parent frame (ad network standard)
            OpenUrlInParent(url);
#else
            Application.OpenURL(url);
            Debug.Log($"[CTAHandler] Opening: {url}");
#endif
        }

        // ── WebGL JS interop ──────────────────────────────────────────────────

#if UNITY_WEBGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void OpenUrlInParent(string url);
#else
        private static void OpenUrlInParent(string url) => Application.OpenURL(url);
#endif
    }
}
