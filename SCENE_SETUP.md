# Scene Setup Guide

Step-by-step instructions for setting up the Unity scenes from scratch.

---

## Required Unity Version

**Unity 2022.3 LTS** (minimum). TextMeshPro must be imported (Window → TextMeshPro → Import TMP Essential Resources).

---

## Scene: `PlayableAd` (the only scene)

### 1. Root GameObjects hierarchy

```
PlayableAd (Scene)
├── _Managers
│   ├── GameBoard           → GameBoard.cs, BoardFiller.cs, MatchDetector.cs, SwapController.cs
│   ├── BoxManager          → BoxManager.cs  (not used in Match-3 — can omit)
│   ├── BoardLoader         → BoardLoader.cs
│   ├── EffectsManager      → EffectsManager.cs  (+ ParticleSystem children in pool)
│   ├── AudioManager        → AudioManager.cs  (+ AudioSource component)
│   ├── WebGLSettings       → WebGLSettings.cs
│   └── CameraFit           → CameraFit.cs  (on Main Camera OR separate GO)
├── _Ad
│   ├── AdController        → AdController.cs
│   ├── CTAHandler          → CTAHandler.cs
│   └── TimerController     → TimerController.cs
├── _Board
│   └── GemParent           (empty Transform — parent for all gem GameObjects)
├── Main Camera             (Orthographic, CameraFit.cs attached)
└── UI (Canvas — Screen Space Overlay, Scale Mode: Scale with Screen Size 1080x1920)
    ├── HUD
    │   ├── ScoreText        (TMP_Text)
    │   ├── TimerWidget
    │   │   ├── TimerLabel   (TMP_Text)
    │   │   └── RadialFill   (Image, type=Filled, Radial360)
    │   └── TapHint          (TMP_Text "Tap & drag to swap!")
    ├── CTAPanel             (disabled by default)
    │   ├── Background       (Image — semi-opaque overlay)
    │   ├── FinalScoreText   (TMP_Text)
    │   ├── TaglineText      (TMP_Text "Can you beat the puzzle?")
    │   └── InstallButton    (Button → AdController.OnInstallTapped)
    │       └── ButtonLabel  (TMP_Text "INSTALL FREE")
    ├── TutorialHand         → TutorialHand.cs  (CanvasGroup + RectTransform)
    │   └── HandIcon         (Image — finger/cursor sprite)
    └── ScorePopupContainer  (empty RectTransform — parent for score popup prefabs)
```

---

### 2. Gem Prefabs

Create **6 Gem prefabs** (one per colour) in `Assets/Prefabs/Gems/`:

Each prefab needs:
- `SpriteRenderer` (assign gem sprite + colour)
- `GemController.cs`
- `CircleCollider2D` (for raycasting — Layer: **Gem**)
- Size: `0.9 × 0.9` (leaves a small gap between cells)

Gem colours:

| Prefab name   | Colour hex |
|---------------|------------|
| Gem_Red       | `#F24444`  |
| Gem_Blue      | `#3B82F6`  |
| Gem_Green     | `#22C55E`  |
| Gem_Yellow    | `#FACC15`  |
| Gem_Purple    | `#A855F7`  |
| Gem_Orange    | `#F97316`  |

Assign all 6 prefabs to `GameBoard → Gem Prefabs[]` in the Inspector.

---

### 3. Particle Burst Prefab

Create `Assets/Prefabs/FX/GemBurst.prefab`:
- `ParticleSystem`
  - Duration: 0.4s, Looping: off
  - Start Lifetime: 0.4, Start Speed: 4–8
  - Start Size: 0.1–0.3
  - Shape: Sphere, Radius 0.1
  - Emission: Burst → Count 12 at t=0
  - Color over Lifetime: white → transparent
  - Renderer: Billboard

Assign to `EffectsManager → Gem Burst Prefab`.

---

### 4. Score Popup Prefab

Create `Assets/Prefabs/UI/ScorePopup.prefab`:
- `RectTransform` (100×40)
- `CanvasGroup`
- `ScorePopup.cs`
- Child: `TMP_Text` ("+" score, bold, 28pt, centred)

Assign to `AdUIManager → Score Popup Prefab`.

---

### 5. Physics Layer

Add a **Gem** layer (Edit → Project Settings → Tags and Layers).  
Set `SwapController → Gem Layer` to the Gem layer.

---

### 6. WebGL Template

In **Edit → Project Settings → Player → WebGL → Resolution and Presentation**:
- Set `WebGL Template` to **PlayableAd** (the custom template in `Assets/WebGLTemplates/PlayableAd/`)

---

### 7. Build Settings

**File → Build Settings:**
- Platform: WebGL
- Add scene: `Assets/Scenes/PlayableAd.unity`

**Player Settings → WebGL:**
- Compression Format: **Gzip** (best network compatibility)
- Publish Build: **Development Build OFF** for final submission
- Exception Support: **None** (reduces size)
- Strip Engine Code: **ON**

---

### 8. Quick Test

1. Hit Play in the Editor.
2. The board fills, the tutorial hand appears, and you can swap gems.
3. After 15 seconds (or after a match), the CTA panel appears.
4. Console should show `[AdController] JS Bridge → adStarted` and eventually `adCompleted`.
