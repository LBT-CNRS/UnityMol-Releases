using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UMol {
public class HaadWrapper {

    public static string BinaryPath
    {
        get {
            var basePath = Application.streamingAssetsPath + "/HaadExe";

            if (Application.platform == RuntimePlatform.OSXPlayer ||
                    Application.platform == RuntimePlatform.OSXEditor)
                return basePath + "/OSX/haad";

            if (Application.platform == RuntimePlatform.LinuxPlayer ||
                    Application.platform == RuntimePlatform.LinuxEditor)
                return basePath + "/Linux/haad";

            return basePath + "/Windows/haad.exe";
        }
    }

    public static void callHaadOnStructure(UnityMolStructure s, bool force = false) {

        if (!force && s.currentModel.hasHydrogens()) {
            UnityEngine.Debug.LogWarning("Current model already contains hydrogens");
            return;
        }

        string binPath = BinaryPath;

        StringBuilder bigLineSB = new StringBuilder();

        //Execute haad for each chain
        foreach (UnityMolChain c in s.currentModel.chains.Values) {
            string path = Application.temporaryCachePath + "/tmphaad" + s.uniqueName + "_" + c.name + ".pdb";
            string pathOut = path + ".h";


            UnityMolSelection select = c.ToSelection(false);
            string pdbLines = PDBReader.Write(select, writeHET: false);


            //Write to temporary file
            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(pdbLines);
            writer.Close();

            if (pdbLines.Length < 10) {
                continue;
            }

            //Start haad
            string opt = path;
            var info = new ProcessStartInfo(binPath, opt);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            // info.WindowStyle = ProcessWindowStyle.Hidden;
            // info.RedirectStandardInput = true;
            // info.RedirectStandardOutput = true;
            // info.RedirectStandardError = true;

            Process _subprocess = Process.Start(info);

            // string log = _subprocess.StandardError.ReadToEnd();
            // string log = _subprocess.StandardOutput.ReadToEnd();
            // UnityEngine.Debug.Log(log);

            _subprocess.WaitForExit();
            // _subprocess.Dispose();

            //Wait a little
            System.Threading.Thread.Sleep(100);

            try{
                //Read output file
                using(StreamReader sr = new StreamReader(pathOut)) {
                    string content = sr.ReadToEnd();
                    string[] lines = content.Split("\n" [0]);
                    foreach (string l in lines) {
                        if (l.Length >= 57) {
                            if (l[56] != ' ') { //New atom from haad
                                string atomName = l.Substring(12, 4);
                                //Keep only H atoms
                                if (atomName.StartsWith("H") || ( atomName.Length >= 2 && System.Char.IsDigit(atomName[0]) && atomName[1] == 'H')) {
                                    //Set the chain name to the line
                                    string newAtomLine = l.Substring(0, 21) + c.name + l.Substring(22, 32) + "\n";
                                    // string newAtomLine = l.Substring(0, 12) + " H   "+ l.Substring(17, 4) + c.name + l.Substring(22, 32) + "\n";

                                    bigLineSB.Append(newAtomLine);
                                }
                            }
                        }
                    }
                }
            }
            catch{
                UnityEngine.Debug.LogError("Couldn't process the chain "+c.name);
            }
        }

        foreach(UnityMolChain c in s.currentModel.chains.Values) {
            string path = Application.temporaryCachePath + "/tmphaad" + s.uniqueName + "_" + c.name + ".pdb";
            string pathOut = path + ".h";
            File.Delete(path);
            File.Delete(pathOut);
        }


        if (bigLineSB.Length != 0) {
            PDBReader.AddToStructure(bigLineSB.ToString(), s);
            UnityMolMain.getSelectionManager().updateSelectionsWithMDA(s, forceModif: true);
        }
        else {
            UnityEngine.Debug.LogWarning("No hydrogen added");
        }

    }
}
}
