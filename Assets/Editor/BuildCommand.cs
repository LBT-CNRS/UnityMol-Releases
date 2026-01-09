using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System;

/// <summary>
/// Handle a custom build command executed from the command line.
/// Used to generate builds through CI.
/// See UnityMolX/ci/build.sh for a working example
/// </summary>
public static class BuildCommand
{

	/// <summary>
	/// Return the argument called from the commandline with the name "name"
	/// </summary>
    private static string getArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains(name))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    // https://stackoverflow.com/questions/1082532/how-to-tryparse-for-enum-value
    private static bool tryConvertToEnum<TEnum>(this string strEnumValue, out TEnum value)
    {
        if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
        {
            value = default;
            return false;
        }

        value = (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
        return true;
    }


	/// <summary>
	/// Return the correct BuildTarget object enum detected from the commandline argument "-customBuildTarget"
	/// </summary>
    private static BuildTarget getBuildTarget()
    {
        string buildTargetName = getArgument("customBuildTarget");
        Console.WriteLine(":: Received customBuildTarget " + buildTargetName);

        if (buildTargetName.tryConvertToEnum(out BuildTarget target)) {
            return target;
        }

        Console.WriteLine($":: {nameof(buildTargetName)} \"{buildTargetName}\" not defined on enum {nameof(BuildTarget)}, " +
                          $"using {nameof(BuildTarget.NoTarget)} enum to build");

        return BuildTarget.NoTarget;
    }

	/// <summary>
	/// Return the name of the build extracted from the commandline argument "-customBuildName"
	/// </summary>
    private static string getBuildName()
    {
        string buildName = getArgument("customBuildName");
        Console.WriteLine(":: Received customBuildName " + buildName);
        if (buildName == "") {
            throw new Exception("customBuildName argument is missing");
        }
        return buildName;
    }

    /// <summary>
    /// Return the OSX architecture of the build extracted from the commandline argument "-customOSXArch"
    /// </summary>
    private static string getOSXArchitecture()
    {
        string osxArch = getArgument("customOSXArch");
        Console.WriteLine(":: Received customOSXArch " + osxArch);
        return osxArch;
    }

	/// <summary>
	/// Return the path of the build extracted from the commandline argument "-customBuildPath"
	/// </summary>
    private static string getBuildPath(BuildTarget buildTarget, string buildName)
    {
        string buildPath = getArgument("customBuildPath");
        Console.WriteLine(":: Received customBuildPath " + buildPath);
        if (buildPath == "")
        {
            throw new Exception("customBuildPath argument is missing");
        }

        switch (buildTarget)
        {
            case BuildTarget.StandaloneWindows64:
                buildName += ".exe";
                break;
            case BuildTarget.StandaloneOSX:
                buildName += ".app";
                break;
        }

        return buildPath + buildName;
    }


	/// <summary>
	/// Perform the build by retrieving all information from the commandline arguments -customXXX
    /// Build only the "MainUIScene.unity".
	/// </summary>
    public static void PerformBuild()
    {
        BuildTarget buildTarget    = getBuildTarget();
        if (buildTarget == BuildTarget.NoTarget) {
            return;
        }

        string buildPath;
        try {
            string buildName      = getBuildName();
            buildPath      = getBuildPath(buildTarget, buildName);
        } catch (Exception e) {
            Console.WriteLine(e);
            return;
        }

        BuildPlayerOptions buildPlayerOptions = new() {
            scenes = new[] { "Assets/Scenes/MainUIScene.unity"},
            locationPathName = buildPath,
            target = buildTarget
        };

        // Set the correct OSX Architecture (retrieved from the command line) in the User settings
        if (buildTarget == BuildTarget.StandaloneOSX) {
            string osxArch   = getOSXArchitecture();
            switch (osxArch)
            {
                case "x64":
                    EditorUserBuildSettings.SetPlatformSettings(BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneOSX), "Architecture", "x64");
                    break;
                case "arm64":
                    EditorUserBuildSettings.SetPlatformSettings(BuildPipeline.GetBuildTargetName(BuildTarget.StandaloneOSX), "Architecture", "arm64");
                    break;
            }
        }

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        switch (summary.result)
        {
            case BuildResult.Succeeded:
                Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                break;
            case BuildResult.Failed:
                Debug.Log("Build failed");
                break;
            case BuildResult.Unknown:
                Debug.Log("Build unsuccessful");
                break;
            case BuildResult.Cancelled:
                Debug.Log("Build cancelled");
                break;
            default:
                return;
        }
    }
}
