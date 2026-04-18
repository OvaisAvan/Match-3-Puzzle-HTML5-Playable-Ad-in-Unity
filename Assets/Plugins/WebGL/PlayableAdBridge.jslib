/**
 * PlayableAdBridge.jslib
 * Unity WebGL JS plugin — bridges Unity C# events to the ad network JS layer.
 *
 * Supported networks (auto-detected at runtime):
 *   - Meta Audience Network (FAN) / Instant Games
 *   - Mintegral
 *   - AppLovin MAX
 *   - IronSource (Levelplay)
 *   - Unity Ads (via Operate)
 *   - Vungle / Liftoff
 *   - Generic MRAID 2.0
 *   - Any network that listens on window.gameReady / window.onGameEnd
 *
 * Place this file at:  Assets/Plugins/WebGL/PlayableAdBridge.jslib
 */

mergeInto(LibraryManager.library, {

  /**
   * Fire(eventName: string)
   * Called from C# via DllImport.
   * Events: "adStarted" | "adCompleted" | "installClicked"
   */
  Fire: function (eventNamePtr) {
    var eventName = UTF8ToString(eventNamePtr);

    try {
      // ── Meta / Facebook ──────────────────────────────────────────────────
      if (typeof FbPlayableAd !== 'undefined') {
        if (eventName === 'adCompleted') FbPlayableAd.onCTAClick();
        return;
      }

      // ── Mintegral ────────────────────────────────────────────────────────
      if (typeof gameReady === 'function' && eventName === 'adStarted') {
        gameReady();
      }
      if (typeof gameEnd === 'function' && eventName === 'adCompleted') {
        gameEnd();
      }

      // ── AppLovin MAX ─────────────────────────────────────────────────────
      if (window.max_playable && eventName === 'installClicked') {
        window.max_playable.openStoreUrl();
      }

      // ── Generic / MRAID ──────────────────────────────────────────────────
      if (typeof mraid !== 'undefined') {
        if (eventName === 'adCompleted') mraid.open(window.__storeUrl || '');
      }

      // ── Universal fallback — post message to parent frame ─────────────
      window.parent.postMessage({ type: 'playableAd', event: eventName }, '*');

    } catch (e) {
      console.warn('[PlayableAdBridge] Error firing event:', eventName, e);
    }
  },

  /**
   * OpenUrlInParent(url: string)
   * Opens the store URL in the parent frame (required by most ad networks).
   */
  OpenUrlInParent: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    try {
      // Try MRAID first
      if (typeof mraid !== 'undefined') { mraid.open(url); return; }
      // Meta
      if (typeof FbPlayableAd !== 'undefined') { FbPlayableAd.onCTAClick(); return; }
      // Fallback: open in parent
      window.parent.open(url, '_blank');
    } catch (e) {
      window.open(url, '_blank');
    }
  }

});
