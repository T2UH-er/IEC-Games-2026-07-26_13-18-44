#if UNITY_EDITOR
using System;
using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameBuildHelper
{
    private static readonly string[] Scenes = new string[]
    {
        "Assets/Scenes/HomeScence.unity",
        "Assets/Scenes/LevelList.unity",
        "Assets/Scenes/test.unity"
    };

    [MenuItem("Tools/Build/1. Build Android APK (Tự động xuất APK)", false, 10)]
    public static void BuildAndroidApk()
    {
        string buildFolder = Path.Combine(Directory.GetCurrentDirectory(), "Builds/Android");
        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }

        string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string apkPath = Path.Combine(buildFolder, $"Game_{timeStamp}.apk");

        Debug.Log($"[BuildHelper] Bắt đầu quá trình Build APK tới: {apkPath}");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"<color=green>[BuildHelper] Build APK THÀNH CÔNG!</color> Kích thước: {summary.totalSize / (1024 * 1024)} MB");
            EditorUtility.RevealInFinder(apkPath);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"<color=red>[BuildHelper] Build APK THẤT BẠI!</color> Số lỗi: {summary.totalErrors}");
        }
    }

    [MenuItem("Tools/Build/2. Build WebGL (Tự động xuất Web HTML5)", false, 11)]
    public static void BuildWebGl()
    {
        string buildFolder = Path.Combine(Directory.GetCurrentDirectory(), "Builds/WebGL");
        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }

        string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string webGlPath = Path.Combine(buildFolder, $"WebGL_{timeStamp}");

        Debug.Log($"[BuildHelper] Bắt đầu quá trình Build WebGL tới: {webGlPath}");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = webGlPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"<color=green>[BuildHelper] Build WebGL THÀNH CÔNG!</color> Thư mục: {webGlPath}");
            EditorUtility.RevealInFinder(webGlPath);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"<color=red>[BuildHelper] Build WebGL THẤT BẠI!</color> Số lỗi: {summary.totalErrors}");
        }
    }

    [MenuItem("Tools/Build/3. Chạy thử WebGL trên Trình duyệt (Local Server)", false, 12)]
    public static void RunLatestWebGl()
    {
        string baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "Builds/WebGL");
        if (!Directory.Exists(baseFolder))
        {
            Debug.LogError("[BuildHelper] Chưa tìm thấy thư mục Builds/WebGL. Hãy build WebGL trước!");
            return;
        }

        string[] subDirs = Directory.GetDirectories(baseFolder);
        Array.Sort(subDirs);
        Array.Reverse(subDirs);

        string targetDir = null;
        foreach (string dir in subDirs)
        {
            if (File.Exists(Path.Combine(dir, "index.html")))
            {
                targetDir = dir;
                break;
            }
        }

        if (targetDir == null)
        {
            Debug.LogError("[BuildHelper] Không tìm thấy bản build WebGL có file index.html nào!");
            return;
        }

        int port = 8080;
        try
        {
            Process process = new Process();
            process.StartInfo.FileName = "python3";
            process.StartInfo.Arguments = $"-m http.server {port} --directory \"{targetDir}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            Debug.Log($"<color=green>[BuildHelper] Đang chạy Local Server tại http://localhost:{port}</color>");
            Application.OpenURL($"http://localhost:{port}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[BuildHelper] Lỗi khi khởi động Server: {e.Message}");
        }
    }

    [MenuItem("Tools/Build/Mở thư mục tổng Builds (Finder)", false, 20)]
    public static void OpenBuildFolder()
    {
        string buildFolder = Path.Combine(Directory.GetCurrentDirectory(), "Builds");
        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }
        EditorUtility.RevealInFinder(buildFolder);
    }
}
#endif
