using UMol;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Class to customize the pre-process build
/// </summary>
internal class UMolPreBuildProcess: IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    /// <summary>
    /// Callback invoked during Player builds.
    /// Use to set the <see cref="Application.version"/> to match the UnityMol version.
    /// See the class <see cref="UnityMolVersion"/> for handling the UnityMol version.
    /// </summary>
    /// <param name="report"></param>
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("UMolPreBuildProcess.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
        string version = UnityMolVersion.GetVersion();
        Debug.Log("Set UnityMol version to " + version);
        PlayerSettings.bundleVersion = version;
    }
}
