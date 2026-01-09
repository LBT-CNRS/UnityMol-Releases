/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2022
        Hubert Santuz, 2022-2026
        Marc Baaden, 2010-2026
        unitymol@gmail.com
        https://unity.mol3d.tech/

        This file is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications based on the Unity3D game engine.
        More details about UnityMol are provided at the following URL: https://unity.mol3d.tech/

        This program is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        This program is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with this program. If not, see <https://www.gnu.org/licenses/>.

        To help us with UnityMol development, we ask that you cite
        the research papers listed at https://unity.mol3d.tech/cite-us/.
    ================================================================================
*/
using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace UMol {

/// <summary>
/// Class handling the version number of UnityMol
/// Don't use the PlayerSettings GUI for setting the version
/// This class handles that and use git describe and git tags commands to
/// retrieve a proper version number based on git repository.
/// Uses <see cref="UMolPreBuildProcess"/> for setting the version during build process.
/// </summary>
public class UnityMolVersion
{

    /// <summary>
    /// Get current a last commit id of the current branch.
    /// </summary>
    /// <param name="shortVersion">Returns short commit id if this is true.</param>
    /// <returns>Commit ID. If there are no commits or git is not available, this function returns empty string.</returns>
    public static string GetLastCommitId(bool shortVersion = true)
    {
        try {
            return RunGit($"rev-parse{(shortVersion ? " --short" : "")} HEAD").Replace("\n", string.Empty);
        }
        catch (Exception e) {
            UnityEngine.Debug.LogException(e);
            return "";
        }
    }

    /// <summary>
    /// Get a result of git describe
    /// </summary>
    /// <param name="enableLightweightTagMatch">Add light tags to the describe command if this is true.</param>
    /// <returns>A result of git describe. If git is not available, this function returns empty string</returns>
    public static string GetGitDescription(bool enableLightweightTagMatch = true)
    {
        try {
            return RunGit(enableLightweightTagMatch ? "describe --tags" : "describe").Replace("\n", string.Empty);
        }
        catch (Exception e) {
            UnityEngine.Debug.LogException(e);
            return "";
        }
    }


    /// <summary>
    /// Runs git.exe with the specified arguments and returns the output.
    /// </summary>
    public static string RunGit(string arguments)
    {
        using (Process process = new Process())
        {
            int exitCode = process.Run(@"git", arguments, Application.dataPath,
                out string output, out string errors);
            if (exitCode == 0) {
                return output;
            }
            else {
                throw new Exception($"Git Exit Code: {exitCode} - {errors}");
            }
        }
    }


    /// <summary>
    /// Return the UnityMol version based on different elements:
    ///  - in Player mode, return the value written in Application.version
    ///  - in Editor mode, return the result of the 'git describe' command.
    ///    If git fails, return the value in Application.version
    /// Note: The initial 'v' is removed if present.
    /// </summary>
    /// <remarks>
    /// The value of Application.version is set during the build process.
    /// see <see cref="UMolPreBuildProcess"/>.
    /// </remarks>
    /// <returns>the UnityMol version</returns>
    public static string GetVersion() {

        string version;

        if(Application.isEditor) {

            version = GetGitDescription();

            // Either git is not installed or it's not a git repository
            if (string.IsNullOrEmpty(version)) {
                version = Application.version;  // Use the version from the application
            }
        }
        else { //Player mode
            // Use the version from the application
            version = Application.version;
        }

        // Remove initial 'v' as in 'v1.1.1' if present
        if(version[0] == 'v') {
            version = version.Substring(1, version.LastIndexOf('.') - 1);
        }

        return version;
    }

}

/// <summary>
/// Extension of the Process class to use for git commands
/// Taken from https://blog.redbluegames.com/version-numbering-for-games-in-unity-and-git-1d05fca83022
/// </summary>
public static class ProcessExtensions
{
    /// <summary>
    /// Runs the specified process and waits for it to exit. Its output and errors are
    /// returned as well as the exit code from the process.
    /// See: https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
    /// Note that if any deadlocks occur, read the above thread (cubrman's response).
    /// </summary>
    /// <remarks>
    /// This should be run from a using block and disposed after use. It won't
    /// work properly to keep it around.
    /// </remarks>
    public static int Run(this Process process, string application,
        string arguments, string workingDirectory, out string output,
        out string errors) {

        process.StartInfo = new ProcessStartInfo {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = application,
            Arguments = arguments,
            WorkingDirectory = workingDirectory
        };

        // Use the following event to read both output and errors output.
        StringBuilder outputBuilder = new StringBuilder();
        StringBuilder errorsBuilder = new StringBuilder();
        process.OutputDataReceived += (_, args) => outputBuilder.AppendLine(args.Data);
        process.ErrorDataReceived += (_, args) => errorsBuilder.AppendLine(args.Data);

        // Start the process and wait for it to exit.
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        output = outputBuilder.ToString().TrimEnd();
        errors = errorsBuilder.ToString().TrimEnd();
        return process.ExitCode;
    }
}

}
