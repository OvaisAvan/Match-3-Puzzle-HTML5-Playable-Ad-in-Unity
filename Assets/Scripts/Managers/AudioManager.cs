using UnityEngine;

namespace MatchBlitz.Managers
{
    /// <summary>
    /// Minimal audio manager for the playable ad.
    /// Kept lightweight — no music, just punchy SFX.
    /// All clips assigned in Inspector.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("SFX Clips")]
        [SerializeField] private AudioClip matchPopClip;      // gem explosion
        [SerializeField] private AudioClip swapClip;          // swap whoosh
        [SerializeField] private AudioClip invalidSwapClip;   // fail bump
        [SerializeField] private AudioClip ctaJingleClip;     // short win fanfare
        [SerializeField] private AudioClip cascadeClip;       // chain match sound

        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.85f;

        private AudioSource source;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            source        = GetComponent<AudioSource>();
            source.volume = sfxVolume;
            source.playOnAwake = false;
        }

        public void PlayMatchPop()    => Play(matchPopClip,    Random.Range(0.9f, 1.1f));
        public void PlaySwap()        => Play(swapClip);
        public void PlayInvalidSwap() => Play(invalidSwapClip);
        public void PlayCtaJingle()   => Play(ctaJingleClip);
        public void PlayCascade()     => Play(cascadeClip,     Random.Range(0.95f, 1.05f));

        private void Play(AudioClip clip, float pitch = 1f)
        {
            if (clip == null || source == null) return;
            source.pitch = pitch;
            source.PlayOneShot(clip, sfxVolume);
        }
    }
}
