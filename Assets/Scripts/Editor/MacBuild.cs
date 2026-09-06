using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// macOS standalone. Same scene list and the same argument handling as WebBuild, so the two
// cannot drift into disagreeing about what "the game" is.
public static class MacBuild
{
    [MenuItem("Liminal Leap/Build macOS")]
    public static void BuildFromMenu()
    {
        Build(Path.Combine(Directory.GetCurrentDirectory(), "Build/Mac"), false);
    }

    // -executeMethod entry. Args: -out <dir> [-dev]
    public static void BuildFromCommandLine()
    {
        var args = Environment.GetCommandLineArgs();
        var output = ArgValue(args, "-out", Path.Combine(Directory.GetCurrentDirectory(), "Build/Mac"));
        var development = Array.IndexOf(args, "-dev") >= 0;

        Build(output, development);
    }

    private static void Build(string output, bool development)
    {
        var scenes = Array.ConvertAll(
            Array.FindAll(EditorBuildSettings.scenes, s => s.enabled),
            s => s.path);

        if (scenes.Length == 0)
        {
            Debug.LogError("BUILD FAILED: no enabled scenes in Build Settings");
            EditorApplication.Exit(1);
            return;
        }

        Directory.CreateDirectory(output);

        // Full exceptions here regardless: this build exists to be debugged, not shipped,
        // and the size and speed cost that matters on the web does not matter locally.
        PlayerSettings.SetIl2CppCompilerConfiguration(
            NamedBuildTarget.Standalone,
            development ? Il2CppCompilerConfiguration.Debug : Il2CppCompilerConfiguration.Release);

        var target = Path.Combine(output, PlayerSettings.productName + ".app");

        Debug.Log("BUILD START " + target + " scenes=" + string.Join(", ", scenes));

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = target,
            target = BuildTarget.StandaloneOSX,
            options = development
                ? BuildOptions.Development | BuildOptions.AllowDebugging
                : BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"BUILD OK {target} size={summary.totalSize / (1024 * 1024)}MB " +
                $"time={(int)summary.totalTime.TotalSeconds}s");
            EditorApplication.Exit(0);
            return;
        }

        Debug.LogError($"BUILD FAILED {summary.result} errors={summary.totalErrors}");
        EditorApplication.Exit(1);
    }

    private static string ArgValue(string[] args, string flag, string fallback)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == flag)
            {
                return args[i + 1];
            }
        }

        return fallback;
    }
}
