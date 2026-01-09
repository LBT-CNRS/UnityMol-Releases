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
using Debug = UnityEngine.Debug;

namespace UMol {

/// <summary>
/// Wrapper to handle the Haad binary
/// </summary>
public static class HaadWrapper {
    private static string binaryPath
    {
        get {
            string basePath = Application.streamingAssetsPath + "/HaadExe";

            return Application.platform switch {
                RuntimePlatform.OSXPlayer or RuntimePlatform.OSXEditor => basePath + "/OSX/haad",
                RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor => basePath + "/Linux/haad",
                RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor => basePath + "/Windows/haad.exe",
                _ => null
            };
        }
    }

    /// <summary>
    /// Handle & Manage the call to the HAAD external binary
    /// </summary>
    /// <param name="s">the structure to add hydrogens</param>
    /// <param name="force">If True, force add the hydrogens even if hydrogens are present.</param>
    public static void callHaadOnStructure(UnityMolStructure s, bool force = false) {

        if (!force && s.currentModel.HasHydrogens()) {
            Debug.LogWarning("Current model already contains hydrogens");
            return;
        }

        if (s.ContainsDNA()) {
            Debug.LogWarning("Structure " + s.name + " contains DNA/RNA. Haad is for protein only.");
            Debug.LogWarning("Exiting Haad...");
            return;
        }

        if (binaryPath == null) {
            Debug.LogError("Haad failed: no binary for platform " + Application.platform);
            return;
        }
        if (!File.Exists(binaryPath)) {
            Debug.LogError("Haad failed: couldn't locate binary " + binaryPath);
            return;
        }

        StringBuilder bigLineSB = new();

        //Execute haad for each chain
        foreach (UnityMolChain c in s.currentModel.chains.Values) {
            string path = Application.temporaryCachePath + "/tmphaad" + s.name + "_" + c.name + ".pdb";
            string pathOut = path + ".h";


            UnityMolSelection select = c.ToSelection(false);
            string pdbLines = PDBReader.Write(select, writeHET: false);


            //Write to temporary file
            StreamWriter writer = new(path, false);
            writer.WriteLine(pdbLines);
            writer.Close();

            if (pdbLines.Length < 10) {
                continue;
            }

            //Start haad
            string opt = "\"" + path + "\"";
            ProcessStartInfo info = new(binaryPath, opt) {
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process subprocess = Process.Start(info);
            subprocess.WaitForExit();
            //Wait a little
            System.Threading.Thread.Sleep(100);

            try {
                //Read output file
                using(StreamReader sr = new(pathOut)) {
                    string content = sr.ReadToEnd();
                    string[] lines = content.Split("\n" [0]);
                    foreach (string l in lines) {
                        if (l.Length >= 57) {
                            if (l[56] != ' ') { //New atom from haad
                                string atomName = l.Substring(12, 4);
                                //Keep only H atoms
                                if (atomName.StartsWith("H") || ( atomName.Length >= 2 && char.IsDigit(atomName[0]) && atomName[1] == 'H')) {
                                    //Set the chain name to the line
                                    string newAtomLine = l.Substring(0, 21) + c.name + l.Substring(22, 32) + "\n";

                                    bigLineSB.Append(newAtomLine);
                                }
                            }
                        }
                    }
                }
            }
            catch {
                Debug.LogError("Couldn't process the chain " + c.name);
            }
        }

        foreach (UnityMolChain c in s.currentModel.chains.Values) {
            string path = Application.temporaryCachePath + "/tmphaad" + s.name + "_" + c.name + ".pdb";
            string pathOut = path + ".h";
            File.Delete(path);
            File.Delete(pathOut);
        }

        if (bigLineSB.Length != 0) {
            PDBReader.AddToStructure(bigLineSB.ToString(), s);
            UnityMolMain.getSelectionManager().updateSelectionsWithMDA(s, forceModif: true);
        }
        else {
            Debug.LogWarning("No hydrogen added");
        }

    }
}
}
