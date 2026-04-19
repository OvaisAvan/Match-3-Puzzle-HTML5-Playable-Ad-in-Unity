#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

namespace MatchBlitz.Editor
{
    /// <summary>
    /// Post-build processor that checks the WebGL output size.
    /// Most ad networks have a 2–5 MB limit on playable ad bundles.
    /// Logs a warning (or error) if the build exceeds the threshold.
    /// </summary>
    public class AdSizeValidator : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        // ── Configurable limits ───────────────────────────────────────────────
        private const long WarnLimitBytes  = 3 * 1024 * 1024;   // 3 MB
        private const long ErrorLimitBytes = 5 * 1024 * 1024;   // 5 MB

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL) return;

            string buildPath = report.summary.outputPath;
            long   totalSize = GetDirectorySize(buildPath);
            float  totalMB   = totalSize / (1024f * 1024f);

            string msg = $"[AdSizeValidator] WebGL build size: {totalMB:F2} MB";

            if (totalSize > ErrorLimitBytes)
                Debug.LogError($"{msg} ⛔ Exceeds 5 MB — most ad networks will REJECT this build!");
            else if (totalSize > WarnLimitBytes)
                Debug.LogWarning($"{msg} ⚠️ Over 3 MB — check network limits before submitting.");
            else
                Debug.Log($"{msg} ✅ Within recommended limits.");

            // Also log the single largest files for quick debugging
            LogLargestFiles(buildPath, 5);
        }

        private long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long size = 0;
            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                size += new FileInfo(file).Length;
            return size;
        }

        private void LogLargestFiles(string path, int count)
        {
            if (!Directory.Exists(path)) return;
            var files = new System.Collections.Generic.List<FileInfo>();
            foreach (string f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                files.Add(new FileInfo(f));

            files.Sort((a, b) => b.Length.CompareTo(a.Length));
            Debug.Log("[AdSizeValidator] Largest files:");
            for (int i = 0; i < Mathf.Min(count, files.Count); i++)
                Debug.Log($"  {files[i].Name} — {files[i].Length / 1024f:F1} KB");
        }
    }
}
#endif
