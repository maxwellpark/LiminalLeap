using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// WebGL build for itch.io. Menu for hand use, BuildFromCommandLine for headless.
public static class WebBuild
{
    private const string DefaultOutput = "Build/WebGL";

    [MenuItem("Liminal Leap/Build for Web")]
    public static void BuildDefault()
    {
        Build(DefaultOutput, false);
    }

    // -executeMethod entry. Args: -out <dir> -dev
    public static void BuildFromCommandLine()
    {
        var args = Environment.GetCommandLineArgs();
        var output = ArgValue(args, "-out", DefaultOutput);
        var development = args.Contains("-dev");

        var ok = Build(output, development);
        if (!ok)
        {
            EditorApplication.Exit(1);
        }
    }

    public static bool Build(string output, bool development)
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("BUILD FAILED: no enabled scenes in Build Settings");
            return false;
        }

        Directory.CreateDirectory(output);
        ConfigureWebGl(development);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = output,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = development ? BuildOptions.Development : BuildOptions.None,
        };

        Debug.Log("BUILD START " + output + " scenes=" + string.Join(", ", scenes));
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"BUILD FAILED: {summary.result}, {summary.totalErrors} errors");
            return false;
        }

        Debug.Log($"BUILD OK {output} size={summary.totalSize / 1024 / 1024}MB time={summary.totalTime.TotalSeconds:F0}s");
        return true;
    }

    private static void ConfigureWebGl(bool development)
    {
        // Gzip plus the fallback: itch serves it fine, and the fallback covers hosts
        // that send the wrong content-encoding, which is the usual cause of a blank page.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.exceptionSupport = development
            ? WebGLExceptionSupport.FullWithStacktrace
            : WebGLExceptionSupport.None;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.runInBackground = false;
        PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL, Il2CppCompilerConfiguration.Release);
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
