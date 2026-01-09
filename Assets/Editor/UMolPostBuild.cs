using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

#if UNITY_STANDALONE_OSX
using UnityEditor.OSXStandalone;
#endif


/// <summary>
/// PostBuild class to handle the different external binaries used in StreamingAssets folder.
/// This class keep only the correct binary depending on the build architecture.
/// Currently, the external binaries are : ffmpeg, haad, reduce, msms.
/// It assumes the hierarchy of external binaries are : "StreamingAssets/XXX/{Windows,Linux,OSX-arm64,OSX-x64}/binary".
/// </summary>
public class UMolPostBuild : IPostprocessBuildWithReport {
    public int callbackOrder { get; }

    /// <summary>
    /// Return the absolute path of StreamingAssets folder if found inside the "folder"
    /// </summary>
    /// <param name="folder">path of the folder to find inside the StreamingAssets</param>
    /// <returns>the absolute path of StreamingAssets folder. if not found, return null.</returns>
    private static string findStreamingAssetsFolder(string folder) {
        foreach (string f in Directory.GetDirectories(folder)) {
            if (f.EndsWith("Data")) {
                return Path.Combine(f, "StreamingAssets");
            }
        }
        return null;
    }

    /// <summary>
    /// Callback automatically called at the end of a build process.
    /// For each external tool, keep only the folder containing the binary corresponding to the BuildTarget by removing
    /// the others platform folders.
    /// </summary>
    /// <param name="report"></param>
    public void OnPostprocessBuild(BuildReport report) {
        string buildDirectory =  Path.GetDirectoryName(report.summary.outputPath);
        BuildTarget target = report.summary.platform;
        string[] foldersToDelete;

        switch (target)
        {
            case BuildTarget.StandaloneOSX:
                buildDirectory = Path.Combine(report.summary.outputPath, "Contents", "Resources");
                foldersToDelete = new[] { "Windows", "Linux" };
                break;
            case BuildTarget.StandaloneLinux64:
                foldersToDelete = new[] { "Windows", "OSX-x64", "OSX-arm64" };
                break;
            case BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64:
                foldersToDelete = new[] { "Linux", "OSX-x64", "OSX-arm64" };
                break;
            default:
                Debug.LogError("Post build not supported for this platform");
                return;
        }

        string streamingAssetsFolder = findStreamingAssetsFolder(buildDirectory);

        if (streamingAssetsFolder == null) {
            Debug.LogError("Couldn't find Streaming asset folder");
            return;
        }

        // Remove platform specific subfolders in the folder from StreamingAssets
        foreach (string folder in Directory.GetDirectories(streamingAssetsFolder)) {
            foreach (string subfolder in Directory.GetDirectories(folder)) {
                foreach (string platformFolder in foldersToDelete) {
                    //subfolder contains the absolute path.
                    if (new DirectoryInfo(subfolder).Name == platformFolder) {
                        Directory.Delete(subfolder, true);
                    }
                }
            }
        }
#if UNITY_STANDALONE_OSX
        if (target == BuildTarget.StandaloneOSX) {
            processOSXPlatform(streamingAssetsFolder, UserBuildSettings.architecture);
        }
#endif
    }

#if UNITY_STANDALONE_OSX
    /// <summary>
    /// FOR OSX platform, 2 architectures are possibles during build (x64 or arm64).
    /// This function rename the correct folder (OSX-arm64 or OSX-x64) to a generic one (OSX) for the external binaries
    /// used in StreamingAssets. The other folder (OSX-arm64 or OSX-x64) is deleted.
    /// </summary>
    /// <param name="streamingAssetsFolder">the build path of StreamingAssets folder</param>
    /// <param name="architecture">the OSX Architecture</param>

    private static void processOSXPlatform(string streamingAssetsFolder, MacOSArchitecture architecture) {

        // Remove platform specific subfolders in the folder from StreamingAssets
        foreach (string folder in Directory.GetDirectories(streamingAssetsFolder)) {
            foreach (string subfolder in Directory.GetDirectories(folder)) {
                //subfolder contains the absolute path.
                switch (new DirectoryInfo(subfolder).Name)
                {
                    case "OSX-x64" when architecture == MacOSArchitecture.x64:
                    case "OSX-arm64" when architecture == MacOSArchitecture.ARM64:
                        string osxFolder = Path.Combine(Directory.GetParent(subfolder)!.FullName, "OSX");
                        //Remove previous OSX directory if present.
                        if (Directory.Exists(osxFolder)) {
                            Directory.Delete(osxFolder, true);
                        }
                        Directory.Move(subfolder, osxFolder);
                        break;
                    case "OSX-arm64" when architecture == MacOSArchitecture.x64:
                    case "OSX-x64"  when architecture == MacOSArchitecture.ARM64:
                        Directory.Delete(subfolder, true);
                        break;

                }
            }
        }

    }
#endif
}
