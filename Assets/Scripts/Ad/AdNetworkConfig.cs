using UnityEngine;

namespace MatchBlitz.Ad
{
    public enum AdNetwork
    {
        Generic,
        MetaAudienceNetwork,
        Mintegral,
        AppLovinMAX,
        IronSource,
        UnityAds,
        Vungle
    }

    /// <summary>
    /// ScriptableObject holding per-network configuration.
    /// Create via: Assets → Create → MatchBlitz → AdNetworkConfig
    ///
    /// Switch the active network in the Inspector before building.
    /// The JS bridge auto-detects at runtime, but this config
    /// lets you override store URLs and creative copy per-submission.
    /// </summary>
    [CreateAssetMenu(menuName = "MatchBlitz/AdNetworkConfig", fileName = "AdNetworkConfig")]
    public class AdNetworkConfig : ScriptableObject
    {
        [Header("Target Network")]
        public AdNetwork targetNetwork = AdNetwork.Generic;

        [Header("Store Links")]
        [Tooltip("iOS App Store link")]
        public string iosStoreUrl     = "https://apps.apple.com/app/idYOURAPPID";
        [Tooltip("Google Play link")]
        public string androidStoreUrl = "https://play.google.com/store/apps/details?id=com.yourcompany.yourgame";

        [Header("Ad Creative Copy")]
        public string ctaButtonText   = "INSTALL FREE";
        public string taglineText     = "Can you beat the puzzle?";

        [Header("Gameplay Tuning")]
        [Tooltip("How long the player can interact before the CTA is forced")]
        public float playDurationSeconds = 15f;
        [Tooltip("Show CTA immediately after a cascade of this many chains")]
        public int   cascadeCtaTrigger   = 3;

        [Header("Network-specific")]
        [Tooltip("MRAID version required by the network (2 or 3)")]
        public int mraidVersion = 2;

        // Runtime helper
        public string GetStoreUrl()
        {
#if UNITY_IOS
            return iosStoreUrl;
#else
            return androidStoreUrl;
#endif
        }
    }
}
