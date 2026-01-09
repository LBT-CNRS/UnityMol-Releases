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
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UMol {

/// <summary>
/// Wrapper to the Reduce binary which add hydrogens to a structure
/// </summary>
public static class ReduceWrapper {

    private static readonly string reduceFolder = Application.streamingAssetsPath + "/ReduceExe";

    private const string windowsBinary = "reduce.exe";
    private const string linuxBinary = "reduce";
    private const string osxBinary = "reduce";

    /// <summary>
    /// Path the Reduce binary
    /// </summary>
    private static string binaryPath
    {
        get {
            return Application.platform switch {
                RuntimePlatform.OSXPlayer or RuntimePlatform.OSXEditor => reduceFolder + "/OSX/" + osxBinary,
                RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor => reduceFolder + "/Linux/" + linuxBinary,
                RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor => reduceFolder + "/Windows/" +
                    windowsBinary,
                _ => ""
            };
        }
    }

    /// <summary>
    /// Call Reduce to the UnityMolStructure 's'
    /// </summary>
    /// <param name="s">UnityMolStructure where hydrogens will be added </param>
    /// <param name="force">Replace current hydrogens by the ones added by Reduce</param>
    public static void callReduceOnStructure(UnityMolStructure s, bool force = false)
    {

        if (string.IsNullOrEmpty(binaryPath))
        {
            UnityEngine.Debug.LogWarning("Could not find a Reduce executable for the plateform: " + Application.platform);
        }

        if (!File.Exists(binaryPath)) {
            UnityEngine.Debug.LogError("Reduce failed: couldn't locate binary " + binaryPath);
            return;
        }

        if (!force && s.currentModel.HasHydrogens())
        {
            UnityEngine.Debug.LogWarning("Current model already contains hydrogens and force flag is not set: nothing to be done.");
            return;
        }

        float start = Time.realtimeSinceStartup;

        // Write the structure to a tempory PDB file
        string temporyPDBpath = Application.temporaryCachePath + "/tmpreduce.pdb";
        UnityMolSelection select = s.ToSelection();
        string pdbLines = PDBReader.Write(select);
        StreamWriter writer = new(temporyPDBpath, false);
        writer.WriteLine(pdbLines);
        writer.Close();


        string opt = "-DB \"" + reduceFolder + "/reduce_wwPDB_het_dict.txt\" -Quiet \"" + temporyPDBpath+"\"";

        // Execute Reduce binary
        ProcessStartInfo info = new(binaryPath, opt) {
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true
        };

        Process subprocess = Process.Start(info);

        string log = subprocess.StandardOutput.ReadToEnd();

        subprocess.WaitForExit();
        subprocess.Close();


        UnityEngine.Debug.Log("Time for reduce: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");


        // Retrieve only newly added ATOM/HETATM lines
        string[] lines = log.Split("\n" [0]);
        StringBuilder linesToBeAdded = new();
        foreach (string line in lines)
        {
            if ( ((line.StartsWith("ATOM") || line.StartsWith("HETATM")) && line.EndsWith("new"))
                 || line.StartsWith("MODEL") || line.StartsWith("ENDMDL"))
            {
                linesToBeAdded.Append(line + "\n");
            }
        }

        // Add the lines to the structure and update the existing selections
        if (linesToBeAdded.Length != 0)
        {
            PDBReader.AddToStructure(linesToBeAdded.ToString(), s);
            UnityMolMain.getSelectionManager().updateSelectionsWithMDA(s, forceModif: true);
        }
        else
        {
            UnityEngine.Debug.LogWarning("No hydrogen added");
        }

        File.Delete(temporyPDBpath);
    }
}
}
