# 🎯 MatchBlitz — Unity Match-3 Playable Ad

> A production-ready, open-source **playable ad** built in Unity — fully self-contained HTML5 output compatible with every major ad network.

![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-black?logo=unity)
![Platform](https://img.shields.io/badge/platform-HTML5%20%7C%20WebGL-orange)
![Networks](https://img.shields.io/badge/networks-Meta%20%7C%20Mintegral%20%7C%20AppLovin%20%7C%20IronSource%20%7C%20Unity%20Ads-blue)
![License](https://img.shields.io/badge/license-MIT-green)

---

## 🎮 What is this?

A **playable ad** lets potential players try a mini-version of your game before installing. This repo is a complete, battle-tested Match-3 playable ad engine you can drop into any game project.

**How it works:**
1. Player sees the ad — the Match-3 board loads in ~1s
2. An animated hand shows them how to swap gems
3. They play for up to 15 seconds
4. A CTA overlay appears → "INSTALL FREE" → opens your store page

```
┌─────────────────────────┐
│  Score: 120   ⏱ 12s    │  ← HUD
│                         │
│  🔴 🔵 🟢 🟡 🟣 🟠 🔴 │
│  🔵 🟢 🟡 🟣 🟠 🔴 🔵 │
│  🟢 🟡 🟣 🟠 🔴 🔵 🟢 │  ← 7×7 Match-3 Board
│  🔴 🔵 🟢 🟡 🟣 🟠 🔴 │
│  🟡 🟣 🟠 🔴 🔵 🟢 🟡 │
│  🟠 🔴 🔵 🟢 🟡 🟣 🟠 │
│  🔵 🟢 🟡 🟣 🟠 🔴 🔵 │
│                         │
│  [ 👆 Tap & drag gems ] │  ← Tutorial hand
└─────────────────────────┘
         ↓ after 15s
┌─────────────────────────┐
│    Score: 350   ⭐⭐⭐   │
│  "Can you beat this?"   │
│                         │
│   [ INSTALL FREE 🎮 ]   │  ← CTA Button
└─────────────────────────┘
```

---

## 🗂️ Project Structure

```
MatchBlitzAd/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameBoard.cs           ← Grid authority, swap/resolve loop
│   │   │   ├── GemController.cs       ← Per-gem data, animations
│   │   │   ├── MatchDetector.cs       ← 3-in-a-row, L/T shape detection
│   │   │   ├── BoardFiller.cs         ← Gravity, cascade, procedural fill
│   │   │   ├── SwapController.cs      ← Mouse + touch drag-to-swap input
│   │   │   └── GemColorConfig.cs      ← ScriptableObject: gem sprites/colours
│   │   ├── Ad/
│   │   │   ├── AdController.cs        ← Ad lifecycle: init → play → CTA
│   │   │   ├── CTAHandler.cs          ← Store URL, install button, JS bridge
│   │   │   ├── TimerController.cs     ← Radial countdown, urgent pulse
│   │   │   └── AdNetworkConfig.cs     ← ScriptableObject: per-network settings
│   │   ├── UI/
│   │   │   ├── AdUIManager.cs         ← HUD, score popups, CTA overlay
│   │   │   ├── TutorialHand.cs        ← Animated swipe hint (first 3.5s)
│   │   │   └── ScorePopup.cs          ← Floating +score label component
│   │   ├── Managers/
│   │   │   ├── AudioManager.cs        ← SFX: match pop, swap, CTA jingle
│   │   │   ├── EffectsManager.cs      ← Particle burst pool, combo labels
│   │   │   ├── BoardLoader.cs         ← Loads preset board from JSON
│   │   │   ├── CameraFit.cs           ← Auto-fits ortho camera to any screen
│   │   │   └── WebGLSettings.cs       ← WebGL perf + audio unlock on gesture
│   │   └── Editor/
│   │       ├── PlayableAdBuilder.cs   ← One-click build window
│   │       └── AdSizeValidator.cs     ← Post-build size checker (warns >3MB)
│   ├── Plugins/
│   │   └── WebGL/
│   │       └── PlayableAdBridge.jslib ← JS bridge (Meta, Mintegral, AppLovin…)
│   ├── Resources/
│   │   └── Levels/
│   │       └── ad_board.json          ← Pre-designed board w/ guaranteed matches
│   └── WebGLTemplates/
│       └── PlayableAd/
│           └── index.html             ← Custom WebGL template (phone preview UI)
├── BuildConfig/
│   └── build_html5.py                 ← Python inliner → single HTML file
├── SCENE_SETUP.md                     ← Step-by-step Unity scene guide
├── NETWORK_SUBMISSION.md              ← Per-network requirements & tips
├── .gitignore
└── README.md
```

---

## 🚀 Getting Started

### Requirements
- **Unity 2022.3 LTS** or newer
- **TextMeshPro** (Window → TextMeshPro → Import TMP Essential Resources)
- **Python 3.8+** (for the HTML inliner script)

### 1. Clone

```bash
git clone https://github.com/YOUR_USERNAME/MatchBlitzAd.git
cd MatchBlitzAd
```

### 2. Open in Unity

Open Unity Hub → **Open Project** → select `MatchBlitzAd/`.

### 3. Scene Setup

Follow **[SCENE_SETUP.md](SCENE_SETUP.md)** to wire up prefabs, layers, and Inspector references.

### 4. Play in Editor

Hit **Play** — the board fills, the tutorial hand appears, and you can swap gems. After 15 seconds the CTA overlay slides in.

### 5. Build

**Option A — Editor Window (recommended):**
```
Window → MatchBlitz → Playable Ad Builder → ▶ Build Playable Ad
```

**Option B — Manual:**
```bash
# 1. File → Build Settings → Build → Builds/WebGL/
# 2. Run inliner:
python3 BuildConfig/build_html5.py Builds/WebGL Builds/Playable
```

Output: `Builds/Playable/MatchBlitz_Playable.html` (~2–4 MB)

---

## 🌐 Network Compatibility

| Network | Status | API |
|---------|--------|-----|
| Meta / Facebook | ✅ | `FbPlayableAd.onCTAClick()` |
| Mintegral | ✅ | `gameReady()` + `gameEnd()` |
| AppLovin MAX | ✅ | `max_playable.openStoreUrl()` |
| IronSource | ✅ | MRAID 2.0 fallback |
| Unity Ads | ✅ | Generic HTML5 |
| Vungle / Liftoff | ✅ | MRAID 2.0 fallback |
| Generic HTML5 | ✅ | `postMessage` to parent frame |

The JS bridge in `PlayableAdBridge.jslib` **auto-detects** the active network at runtime. You build once and submit the same file everywhere.

See **[NETWORK_SUBMISSION.md](NETWORK_SUBMISSION.md)** for per-network checklists.

---

## ⚙️ Configuration

### Change the CTA store URL

In the Inspector on the `CTAHandler` component:
```
Store Url: https://play.google.com/store/apps/details?id=com.yourcompany.yourgame
```

Or override at runtime from JavaScript:
```js
unityInstance.SendMessage('CTAHandler', 'SetStoreUrl', 'https://your-store-link.com');
```

### Change play duration (default: 15s)

On `AdController` in the Inspector:
```
Play Duration: 15
```

### Use a preset board

`BoardLoader → Board Resource Path` points to `Resources/Levels/ad_board.json`.  
Edit `ad_board.json` to guarantee a satisfying first match for the player.

### Tune gem colours / sprites

Create a **GemColorConfig** ScriptableObject (Assets → Create → MatchBlitz → GemColorConfig) and assign it to `BoardFiller`.

---

## 📐 Architecture

```
AdController (lifecycle owner)
    │
    ├── GameBoard ──► MatchDetector ──► BoardFiller
    │       │
    │       └── SwapController (input)
    │
    ├── BoardLoader ──► Resources/Levels/ad_board.json
    │
    ├── AdUIManager ──► TimerController
    │       │           TutorialHand
    │       └────────── ScorePopup (pooled)
    │
    ├── EffectsManager (particle pool)
    ├── AudioManager   (SFX)
    ├── CTAHandler     (store URL + JS bridge)
    └── CameraFit      (responsive ortho camera)
```

---

## 🤝 Contributing

PRs welcome! Good first contributions:

- New board layouts in `ad_board.json`
- Additional special gems (bomb, row-clear, colour-blast)
- Landscape orientation support
- Additional network bridges in `PlayableAdBridge.jslib`
- Automated size optimisation tips

---

## 📄 License

MIT © 2025 — free to use in commercial and personal projects. See [LICENSE](LICENSE).

---

## 🙏 Acknowledgements

Inspired by the playable ad best practices from  
[Meta for Developers](https://developers.facebook.com/docs/audience-network/guides/ad-formats/playable/) and the open-source Unity community.
