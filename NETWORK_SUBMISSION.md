# Ad Network Submission Guide

Checklist and requirements for submitting MatchBlitz to major playable ad networks.

---

## Universal Checklist (all networks)

- [ ] Single `.html` file produced by `build_html5.py`
- [ ] File size ≤ 5 MB (aim for ≤ 3 MB)
- [ ] Tested on Chrome desktop and Safari mobile (BrowserStack or real device)
- [ ] CTA button visible and triggers store redirect
- [ ] Audio works after first user tap (WebGL gesture requirement)
- [ ] No external CDN / network requests at runtime (fully self-contained)
- [ ] No `localStorage` or `sessionStorage` usage
- [ ] Portrait orientation works on 375×812 (iPhone SE) and 390×844 (iPhone 14)

---

## Meta / Facebook Audience Network

| Requirement      | Value |
|------------------|-------|
| Format           | Single HTML file |
| Max size         | 2 MB  |
| MRAID            | Not required |
| CTA API          | `FbPlayableAd.onCTAClick()` ✅ wired in `PlayableAdBridge.jslib` |
| Orientation      | Portrait + Landscape both tested |
| Submission       | Meta Ads Manager → Creative → Playable Ad |

**Tips:**
- Meta is the strictest on file size. Enable **Crunch compression** for all textures.
- Use `FbPlayableAd.onCTAClick()` — do NOT use `window.open()` directly.

---

## Mintegral

| Requirement      | Value |
|------------------|-------|
| Format           | Single HTML file |
| Max size         | 5 MB  |
| API              | `gameReady()` + `gameEnd()` ✅ wired |
| Submission       | Mintegral Dashboard → Creative Library → Playable |

**Tips:**
- Call `gameReady()` as soon as the Unity canvas is interactive (done in `index.html`).
- Call `gameEnd()` when the CTA appears (done in `AdController.ShowCTA()`).

---

## AppLovin MAX

| Requirement      | Value |
|------------------|-------|
| Format           | Single HTML file (or ZIP) |
| Max size         | 5 MB  |
| API              | `window.max_playable.openStoreUrl()` ✅ wired |
| Submission       | AppLovin Dashboard → Creatives → Upload |

---

## IronSource / Levelplay

| Requirement      | Value |
|------------------|-------|
| Format           | Single HTML file |
| Max size         | 5 MB  |
| MRAID            | Optional |
| Submission       | IronSource Dashboard → Creatives → Playable Ads |

**Tips:**
- Test with IronSource's [Creative Validator](https://developers.is.com/ironsource-mobile/unity/playable-ads/) tool before submitting.

---

## Unity Ads

| Requirement      | Value |
|------------------|-------|
| Format           | Single HTML file |
| Max size         | 5 MB  |
| Submission       | Unity Dashboard → Monetization → Placements → Creative Pack |

---

## Vungle / Liftoff

| Requirement      | Value |
|------------------|-------|
| Format           | ZIP containing `index.html` + assets, OR single HTML |
| Max size         | 5 MB  |
| API              | MRAID 2.0 ✅ wired as fallback |
| Submission       | Vungle Dashboard → Creatives |

---

## Build & Submit Workflow

```bash
# 1. Build WebGL from Unity (or use the Playable Ad Builder window)
#    File → Build Settings → Build → Builds/WebGL/

# 2. Run the inliner
python3 BuildConfig/build_html5.py Builds/WebGL Builds/Playable

# 3. Check output size
ls -lh Builds/Playable/MatchBlitz_Playable.html

# 4. Test locally
open Builds/Playable/MatchBlitz_Playable.html        # macOS
start Builds/Playable/MatchBlitz_Playable.html       # Windows

# 5. Upload to your ad network dashboard
```

---

## Size Reduction Tips

| Technique | Savings |
|-----------|---------|
| Texture compression (ASTC/DXT) | 30–60% |
| Crunch compression on all sprites | 20–40% |
| Strip Engine Code (Player Settings) | 5–15% |
| Exception Support → None | 5–10% |
| Remove unused audio / assets | Varies |
| Use SpritePacker / Atlas | Reduces draw calls |
| Disable development build | ~200 KB |

Target: **≤ 2.5 MB** for Meta compatibility across all networks.
