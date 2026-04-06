#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEditor.Build.Reporting;
namespace MatchBlitz.Editor
{
    /// <summary>
    /// Custom Editor Window: Window → MatchBlitz → Playable Ad Builder
    ///
    /// Provides a one-click workflow:
    ///   1. Build WebGL
    ///   2. Run build_html5.py to inline all assets into a single .html file
    ///   3. Open the output folder
    /// </summary>
    public class PlayableAdBuilder : EditorWindow
    {
        private string buildOutputPath = "Builds/WebGL";
        private string inlineOutputPath = "Builds/Playable";
        private string pythonPath = "python3";
        private bool   autoOpenFolder = true;
        private bool   runInliner     = true;

        [MenuItem("Window/MatchBlitz/Playable Ad Builder")]
        public static void ShowWindow() =>
            GetWindow<PlayableAdBuilder>("Playable Ad Builder");

        private void OnGUI()
        {
            GUILayout.Label("MatchBlitz — Playable Ad Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            EditorGUILayout.HelpBox(
                "Builds a WebGL project and optionally inlines all assets into a single HTML file " +
                "compatible with most ad networks (Mintegral, AppLovin, IronSource, Meta, etc.).",
                MessageType.Info);

            EditorGUILayout.Space(8);
            GUILayout.Label("Paths", EditorStyles.boldLabel);
            buildOutputPath  = EditorGUILayout.TextField("WebGL Build Output",  buildOutputPath);
            inlineOutputPath = EditorGUILayout.TextField("Inlined HTML Output", inlineOutputPath);
            pythonPath       = EditorGUILayout.TextField("Python Executable",   pythonPath);

            EditorGUILayout.Space(8);
            GUILayout.Label("Options", EditorStyles.boldLabel);
            runInliner     = EditorGUILayout.Toggle("Run HTML Inliner After Build", runInliner);
            autoOpenFolder = EditorGUILayout.Toggle("Open Output Folder When Done",  autoOpenFolder);

            EditorGUILayout.Space(12);

            GUI.backgroundColor = new Color(0.4f, 0.9f, 0.5f);
            if (GUILayout.Button("▶  Build Playable Ad", GUILayout.Height(40)))
                Build();

            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(4);

            if (GUILayout.Button("Run HTML Inliner Only"))
                RunInliner();

            if (GUILayout.Button("Open Output Folder"))
                OpenFolder(inlineOutputPath);
        }

        // ── Build ─────────────────────────────────────────────────────────────

        private void Build()
        {
            string[] scenes = GetScenePaths();
            if (scenes.Length == 0) { Debug.LogError("[Builder] No scenes in Build Settings!"); return; }

            Directory.CreateDirectory(buildOutputPath);

            BuildPlayerOptions opts = new BuildPlayerOptions
            {
                scenes           = scenes,
                locationPathName = buildOutputPath,
                target           = BuildTarget.WebGL,
                options          = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError("[Builder] WebGL build FAILED.");
                return;
            }

            Debug.Log("[Builder] WebGL build succeeded.");

            if (runInliner) RunInliner();
            if (autoOpenFolder) OpenFolder(inlineOutputPath);
        }

        private void RunInliner()
        {
            string scriptPath = Path.GetFullPath("BuildConfig/build_html5.py");
            if (!File.Exists(scriptPath)) { Debug.LogError($"[Builder] Inliner script not found: {scriptPath}"); return; }

            string args = $"\"{scriptPath}\" \"{Path.GetFullPath(buildOutputPath)}\" \"{Path.GetFullPath(inlineOutputPath)}\"";
            var psi = new ProcessStartInfo(pythonPath, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            var proc = Process.Start(psi);
            string stdout = proc.StandardOutput.ReadToEnd();
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (!string.IsNullOrEmpty(stdout)) Debug.Log("[Inliner] " + stdout);
            if (!string.IsNullOrEmpty(stderr)) Debug.LogWarning("[Inliner] " + stderr);
            Debug.Log(proc.ExitCode == 0 ? "[Builder] Inliner complete ✅" : "[Builder] Inliner failed ❌");
        }

        private string[] GetScenePaths()
        {
            var list = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.enabled) list.Add(scene.path);
            return list.ToArray();
        }

        private void OpenFolder(string path)
        {
            Directory.CreateDirectory(path);
            EditorUtility.RevealInFinder(Path.GetFullPath(path));
        }
    }
}
#endif
