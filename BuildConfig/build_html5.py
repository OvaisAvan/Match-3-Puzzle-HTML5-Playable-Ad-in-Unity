#!/usr/bin/env python3
"""
build_html5.py
==============
Inlines a Unity WebGL build into a SINGLE self-contained HTML file
suitable for submission to playable ad networks.

Supported networks:
  - Meta / Facebook Audience Network
  - Mintegral
  - AppLovin MAX
  - IronSource (Levelplay)
  - Unity Ads
  - Vungle / Liftoff
  - Any network accepting a single HTML5 file under 5 MB

Usage:
  python3 build_html5.py <webgl_build_dir> <output_dir>

Example:
  python3 build_html5.py Builds/WebGL Builds/Playable

Output:
  Builds/Playable/MatchBlitz_Playable.html   (~2–4 MB)

Requirements:
  Python 3.8+  (no third-party packages needed)
"""

import sys
import os
import base64
import gzip
import re
import shutil
import json
from pathlib import Path
from datetime import datetime

# ── Config ────────────────────────────────────────────────────────────────────

OUTPUT_FILENAME = "MatchBlitz_Playable.html"
WARN_SIZE_MB    = 3.0
ERROR_SIZE_MB   = 5.0

# ── Helpers ───────────────────────────────────────────────────────────────────

def log(msg: str): print(f"[build_html5] {msg}")

def encode_file_b64(path: Path) -> str:
    with open(path, "rb") as f:
        return base64.b64encode(f.read()).decode("utf-8")

def read_text(path: Path) -> str:
    with open(path, "r", encoding="utf-8") as f:
        return f.read()

def find_file(directory: Path, pattern: str) -> Path | None:
    matches = list(directory.rglob(pattern))
    return matches[0] if matches else None

def file_size_mb(path: Path) -> float:
    return path.stat().st_size / (1024 * 1024)

# ── Core inliner ──────────────────────────────────────────────────────────────

def inline_build(build_dir: Path, output_dir: Path):
    log(f"Source : {build_dir}")
    log(f"Output : {output_dir}")
    output_dir.mkdir(parents=True, exist_ok=True)

    # Locate Unity WebGL files
    build_sub = build_dir / "Build"
    if not build_sub.exists():
        log("ERROR: No 'Build' subfolder found. Is this a Unity WebGL output?")
        sys.exit(1)

    loader_js   = find_file(build_sub, "*.loader.js")
    framework_js = find_file(build_sub, "*.framework.js.gz") or find_file(build_sub, "*.framework.js")
    data_file   = find_file(build_sub, "*.data.gz") or find_file(build_sub, "*.data")
    wasm_file   = find_file(build_sub, "*.wasm.gz") or find_file(build_sub, "*.wasm")

    if not all([loader_js, framework_js, data_file, wasm_file]):
        log("ERROR: Could not find all required Unity WebGL build files.")
        log(f"  loader   : {loader_js}")
        log(f"  framework: {framework_js}")
        log(f"  data     : {data_file}")
        log(f"  wasm     : {wasm_file}")
        sys.exit(1)

    log(f"  loader   : {loader_js.name}")
    log(f"  framework: {framework_js.name}")
    log(f"  data     : {data_file.name}")
    log(f"  wasm     : {wasm_file.name}")

    # Encode binary assets
    log("Encoding assets to base64...")
    framework_b64 = encode_file_b64(framework_js)
    data_b64      = encode_file_b64(data_file)
    wasm_b64      = encode_file_b64(wasm_file)
    loader_text   = read_text(loader_js)

    is_gz = framework_js.suffix == ".gz"

    framework_mime = "application/octet-stream"
    data_mime      = "application/octet-stream"
    wasm_mime      = "application/wasm" if not is_gz else "application/octet-stream"

    # Build the single HTML file
    log("Building inlined HTML...")
    html = build_html(loader_text, framework_b64, framework_mime,
                      data_b64, data_mime, wasm_b64, wasm_mime, is_gz)

    out_path = output_dir / OUTPUT_FILENAME
    with open(out_path, "w", encoding="utf-8") as f:
        f.write(html)

    size_mb = file_size_mb(out_path)
    log(f"Output : {out_path}")
    log(f"Size   : {size_mb:.2f} MB")

    if size_mb > ERROR_SIZE_MB:
        log(f"ERROR  : Exceeds {ERROR_SIZE_MB} MB — most ad networks will reject this!")
        log("         Reduce texture sizes, enable texture compression, or strip unused assets.")
    elif size_mb > WARN_SIZE_MB:
        log(f"WARNING: Over {WARN_SIZE_MB} MB — verify your target network's size limit.")
    else:
        log("Size check PASSED ✅")

    return out_path


def build_html(loader_js: str,
               framework_b64: str, framework_mime: str,
               data_b64: str,      data_mime: str,
               wasm_b64: str,      wasm_mime: str,
               is_compressed: bool) -> str:

    decompress_snippet = """
    function decodeB64(b64) {
      var bin = atob(b64), buf = new Uint8Array(bin.length);
      for (var i = 0; i < bin.length; i++) buf[i] = bin.charCodeAt(i);
      return buf.buffer;
    }
    """ if not is_compressed else """
    async function decodeB64(b64) {
      var bin = atob(b64), buf = new Uint8Array(bin.length);
      for (var i = 0; i < bin.length; i++) buf[i] = bin.charCodeAt(i);
      var ds = new DecompressionStream('gzip');
      var writer = ds.writable.getWriter();
      writer.write(buf); writer.close();
      var out = [], reader = ds.readable.getReader();
      while (true) { var {done, value} = await reader.read(); if (done) break; out.push(value); }
      var total = out.reduce((s,a) => s + a.length, 0), merged = new Uint8Array(total), offset = 0;
      out.forEach(a => { merged.set(a, offset); offset += a.length; });
      return merged.buffer;
    }
    """

    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

    return f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1, user-scalable=no">
  <title>MatchBlitz — Playable Ad</title>
  <!--
    MatchBlitz Playable Ad
    Generated : {timestamp}
    Networks  : Meta, Mintegral, AppLovin MAX, IronSource, Unity Ads, Vungle, Generic HTML5
    Inliner   : build_html5.py
  -->
  <style>
    * {{ margin: 0; padding: 0; box-sizing: border-box; }}
    html, body {{ width: 100%; height: 100%; overflow: hidden; background: #1a1a2e; }}
    #unity-canvas {{
      width: 100%; height: 100%;
      display: block;
      touch-action: none;
    }}
    #loading-overlay {{
      position: fixed; inset: 0;
      display: flex; flex-direction: column;
      align-items: center; justify-content: center;
      background: #1a1a2e; color: #fff;
      font-family: sans-serif; font-size: 18px;
      transition: opacity 0.4s;
      z-index: 999;
    }}
    #loading-bar-wrap {{
      width: 200px; height: 8px;
      background: rgba(255,255,255,0.15);
      border-radius: 4px; margin-top: 16px; overflow: hidden;
    }}
    #loading-bar {{
      height: 100%; width: 0%;
      background: linear-gradient(90deg, #a855f7, #3b82f6);
      border-radius: 4px; transition: width 0.2s;
    }}
  </style>
</head>
<body>

<!-- Loading screen -->
<div id="loading-overlay">
  <div>Loading…</div>
  <div id="loading-bar-wrap"><div id="loading-bar"></div></div>
</div>

<canvas id="unity-canvas" tabindex="-1"></canvas>

<script>
// ── Ad Network Detection ───────────────────────────────────────────────────
(function detectNetwork() {{
  if (typeof FbPlayableAd !== 'undefined')   console.log('[Ad] Network: Meta');
  else if (typeof gameReady === 'function')  console.log('[Ad] Network: Mintegral');
  else if (window.max_playable)             console.log('[Ad] Network: AppLovin MAX');
  else if (typeof mraid !== 'undefined')    console.log('[Ad] Network: MRAID');
  else                                      console.log('[Ad] Network: Generic / Preview');
}})();

// ── Unity Loader (inlined) ─────────────────────────────────────────────────
{loader_js}

// ── Asset Data (base64 inlined) ────────────────────────────────────────────
var _frameworkB64 = "{framework_b64}";
var _dataB64      = "{data_b64}";
var _wasmB64      = "{wasm_b64}";

// ── Decode helpers ─────────────────────────────────────────────────────────
{decompress_snippet}

// ── Bootstrap ─────────────────────────────────────────────────────────────
(async function bootstrap() {{
  var canvas   = document.getElementById('unity-canvas');
  var bar      = document.getElementById('loading-bar');
  var overlay  = document.getElementById('loading-overlay');

  bar.style.width = '10%';

  try {{
    var frameworkData = await decodeB64(_frameworkB64);
    var gameData      = await decodeB64(_dataB64);
    var wasmData      = await decodeB64(_wasmB64);

    bar.style.width = '40%';

    var frameworkBlob = new Blob([frameworkData], {{ type: '{framework_mime}' }});
    var dataBlob      = new Blob([gameData],      {{ type: '{data_mime}'      }});
    var wasmBlob      = new Blob([wasmData],      {{ type: '{wasm_mime}'      }});

    var frameworkUrl  = URL.createObjectURL(frameworkBlob);
    var dataUrl       = URL.createObjectURL(dataBlob);
    var wasmUrl       = URL.createObjectURL(wasmBlob);

    bar.style.width = '55%';

    var config = {{
      dataUrl          : dataUrl,
      frameworkUrl     : frameworkUrl,
      codeUrl          : wasmUrl,
      streamingAssetsUrl: 'StreamingAssets',
      companyName      : 'YourStudio',
      productName      : 'MatchBlitz',
      productVersion   : '1.0',
    }};

    var unityInstance = await createUnityInstance(canvas, config, function(progress) {{
      bar.style.width = (55 + progress * 45) + '%';
    }});

    // Hide loading overlay
    overlay.style.opacity = '0';
    setTimeout(function() {{ overlay.style.display = 'none'; }}, 400);

    // Expose instance for JS→Unity messaging (e.g. SetStoreUrl)
    window.unityInstance = unityInstance;

    // Notify network the ad is ready
    if (typeof gameReady === 'function') gameReady();   // Mintegral
    window.parent.postMessage({{ type:'playableAd', event:'adReady' }}, '*');

    console.log('[MatchBlitz] Unity instance ready.');

  }} catch (err) {{
    console.error('[MatchBlitz] Load failed:', err);
    overlay.innerHTML = '<div style="color:#f87171">Failed to load.<br>Please refresh.</div>';
  }}
}})();
</script>
</body>
</html>"""


# ── Entry point ───────────────────────────────────────────────────────────────

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Usage: python3 build_html5.py <webgl_build_dir> <output_dir>")
        sys.exit(1)

    build_dir  = Path(sys.argv[1])
    output_dir = Path(sys.argv[2])

    if not build_dir.exists():
        log(f"ERROR: Build directory not found: {build_dir}")
        sys.exit(1)

    inline_build(build_dir, output_dir)
