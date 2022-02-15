using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UMol {
public class ReduceWrapper {

    public static string BinaryPath
    {
        get {
            var basePath = Application.streamingAssetsPath + "/ReduceExe";

            if (Application.platform == RuntimePlatform.OSXPlayer ||
                    Application.platform == RuntimePlatform.OSXEditor)
                return basePath + "/OSX/reduce";

            if (Application.platform == RuntimePlatform.LinuxPlayer ||
                    Application.platform == RuntimePlatform.LinuxEditor)
                return basePath + "/Linux/reduce";

            return basePath + "/Windows/reduce.exe";
        }
    }

    public static void callReduceOnStructure(UnityMolStructure s, bool force = false) {

        if (!force && s.currentModel.hasHydrogens()) {
            UnityEngine.Debug.LogWarning("Current model already contains hydrogens");
            return;
        }
        float start = Time.realtimeSinceStartup;


        string binPath = BinaryPath;

        string path = Application.temporaryCachePath + "/tmpreduce.pdb";
        UnityMolSelection select = s.ToSelection();
        string pdbLines = PDBReader.Write(select);
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(pdbLines);
        writer.Close();


        string baseP = Application.streamingAssetsPath + "/ReduceExe";

        string opt = "-DB " + baseP + "/reduce_wwPDB_het_dict.txt -Quiet " + path;


        var info = new ProcessStartInfo(binPath, opt);
        info.UseShellExecute = false;
        info.CreateNoWindow = true;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        // info.RedirectStandardInput = true;
        info.RedirectStandardOutput = true;
        // info.RedirectStandardError = true;

        Process _subprocess = Process.Start(info);

        // string Error = _subprocess.StandardError.ReadToEnd();
        string log = _subprocess.StandardOutput.ReadToEnd();

        _subprocess.WaitForExit();

        // int ExitCode = _subprocess.ExitCode;

        // _subprocess.StandardOutput.Close();

        _subprocess.Close();


        // _subprocess.Close();
        // _subprocess.Dispose();

        UnityEngine.Debug.Log("Time for reduce: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");



        string[] lines = log.Split("\n" [0]);
        StringBuilder bigLineSB = new StringBuilder();
        for (int i = 0; i < lines.Length ; i++) {
            if ( ((lines[i].StartsWith("ATOM") || lines[i].StartsWith("HETATM")) && lines[i].EndsWith("new"))
                    || lines[i].StartsWith("MODEL") || lines[i].StartsWith("ENDMDL")) {
                bigLineSB.Append(lines[i] + "\n");
            }
        }


        if (bigLineSB.Length != 0) {
            PDBReader.AddToStructure(bigLineSB.ToString(), s);
            UnityMolMain.getSelectionManager().updateSelectionsWithMDA(s, forceModif: true);
        }
        else {
            UnityEngine.Debug.LogWarning("No hydrogen added");
        }

        File.Delete(path);


    }
}
}
