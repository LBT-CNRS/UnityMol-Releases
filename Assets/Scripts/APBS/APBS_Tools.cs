/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Joseph Laurenti, 2019-2020
        Xavier Martinez, 2017-2021
        Marc Baaden, 2010-2021
        baaden@smplinux.de
        http://www.baaden.ibpc.fr

        This software is a computer program based on the Unity3D game engine.
        It is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications. More details about UnityMol are provided at
        the following URL: "http://unitymol.sourceforge.net". Parts of this
        source code are heavily inspired from the advice provided on the Unity3D
        forums and the Internet.

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

        References : 
        If you use this code, please cite the following reference :         
        Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
        "Game on, Science - how video game technology may help biologists tackle
        visualization challenges" (2013), PLoS ONE 8(3):e57990.
        doi:10.1371/journal.pone.0057990
       
        If you use the HyperBalls visualization metaphor, please also cite the
        following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
        B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
        using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
        J. Comput. Chem., 2011, 32, 2924

    Please contact unitymol@gmail.com
    ================================================================================
*/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMol.API;
using UnityEngine.UI;
using System;
using System.IO;
using System.Diagnostics;
using SFB;
using Debug = UnityEngine.Debug;


namespace UMol
{
public class APBS_Tools : MonoBehaviour
{
    private UnityMolSelectionManager selM;

    public static string APBSMainFolder;

    static string APBSBinPath;
    static string APBSToolsPath;

    private int multiValue_APBS = 0;
    private int born_APBS = 0;
    private int coulomb_APBS = 0;
    private int apbs_count = 0;
    private int inFileAmmendCount = 0;

    public static bool CoulombE = false;
    public static bool CoulombF = false;
    public static bool BornAI = false;
    public static bool BornF = false;

    //File browser fields
    public InputField topBrowserAPBS_Exe;
    public InputField bottomBrowserAPBS_EXE;
    public InputField topBrowserAPBS_save;
    public InputField topBrowserAPBS_Ammend;
    public InputField bottomBrowserAPBS_Ammend;
    public InputField topBrowserMultivalue;
    public InputField bottomBrowserMultivalue;
    public InputField topBrowserCoulomb;
    public InputField bottomBrowserCoulomb;
    public InputField topBrowserBorn;
    public InputField bottomBrowserBorn;
    public InputField topBrowserHB;
    public InputField topBrowserSB;

    //exe path inputs
    public InputField APBS_path;
    public InputField APBS_save_path;
    public InputField APBS_ammendInFile;
    public InputField Multivalue_path;
    public InputField Coulomb_path;
    public InputField Born_path;
    public InputField HB_in;
    public InputField SB_in;

    //input file inputs
    public InputField dime_x;
    public InputField dime_y;
    public InputField dime_z;
    public InputField cglen_x;
    public InputField cglen_y;
    public InputField cglen_z;
    public InputField fglen_x;
    public InputField fglen_y;
    public InputField fglen_z;
    public InputField pdie;
    public InputField sdie;
    public InputField sdens;
    public InputField srad;
    public InputField swin;
    public InputField temp;
    public InputField epsilon;


    public void udapteAPBSToolPaths() {
        findAPBSBinPath();
        findAPBSToolsPath();

        if (APBS_path != null) {
            if (!string.IsNullOrEmpty(APBSBinPath)) {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                APBS_path.text = Path.Combine(APBSBinPath, "apbs.exe").Replace("\\", "/");
#else
                APBS_path.text = Path.Combine(APBSBinPath, "apbs").Replace("\\", "/");
#endif
            }
            else {
                APBS_path.text = APBSMainFolder + "/apbs/bin/apbs.exe";
            }
        }
        if (APBS_save_path != null) {
            APBS_save_path.text = APBSMainFolder + "/OutputFiles/";
        }
        if (Multivalue_path != null) {
            if (!string.IsNullOrEmpty(APBSToolsPath)) {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                Multivalue_path.text = Path.Combine(APBSToolsPath, "multivalue.exe").Replace("\\", "/");
#else
                Multivalue_path.text = Path.Combine(APBSToolsPath, "multivalue").Replace("\\", "/");
#endif
            }
            else {
                Multivalue_path.text = APBSMainFolder + "/apbs/tools/multivalue.exe";
            }
        }
        if (Coulomb_path != null) {
            if (!string.IsNullOrEmpty(APBSToolsPath)) {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                Coulomb_path.text = Path.Combine(APBSToolsPath, "coulomb.exe").Replace("\\", "/");
#else
                Coulomb_path.text = Path.Combine(APBSToolsPath, "coulomb").Replace("\\", "/");
#endif
            }
            else {
                Coulomb_path.text = APBSMainFolder + "/apbs/tools/coulomb.exe";
            }
        }
        if (Born_path != null) {
            if (!string.IsNullOrEmpty(APBSToolsPath)) {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                Born_path.text = Path.Combine(APBSToolsPath, "born.exe").Replace("\\", "/");
#else
                Born_path.text = Path.Combine(APBSToolsPath, "born").Replace("\\", "/");
#endif
            }
            else {
                Born_path.text = APBSMainFolder + "/apbs/tools/born.exe";
            }
        }
        if (HB_in != null) {
            HB_in.text = APBSMainFolder + "/OutputFiles/";
        }
        if (SB_in != null) {
            SB_in.text = APBSMainFolder + "/OutputFiles/";
        }
    }

    void findAPBSBinPath() {
        if (string.IsNullOrEmpty(APBSBinPath) || !System.IO.Directory.Exists(APBSBinPath)) {
            if (System.IO.Directory.Exists(APBS_Tools.APBSMainFolder)) {
                string[] subdirs = Directory.GetDirectories(APBS_Tools.APBSMainFolder);
                foreach (string sd in subdirs) {
                    string[] subsubdirs = Directory.GetDirectories(sd);
                    foreach (string ssd in subsubdirs) {
                        if (ssd.EndsWith("bin")) {
                            string[] files = Directory.GetFiles(ssd);
                            foreach (string f in files) {
                                if (f.EndsWith("apbs.exe") || f.EndsWith("apbs")) {
                                    APBSBinPath = ssd.Replace("\\", "/");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void findAPBSToolsPath() {
        if (string.IsNullOrEmpty(APBSToolsPath) || !System.IO.Directory.Exists(APBSToolsPath)) {
            if (System.IO.Directory.Exists(APBS_Tools.APBSMainFolder)) {
                string[] subdirs = Directory.GetDirectories(APBS_Tools.APBSMainFolder);
                foreach (string sd in subdirs) {
                    string[] subsubdirs = Directory.GetDirectories(sd);
                    foreach (string ssd in subsubdirs) {
                        if (ssd.EndsWith("tools")) {
                            string[] files = Directory.GetFiles(ssd);
                            foreach (string f in files) {
                                if (f.EndsWith("born.exe") || f.EndsWith("born") || f.EndsWith("coulomb.exe") || f.EndsWith("coulomb")) {
                                    APBSToolsPath = ssd.Replace("\\", "/");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void Start() {
        selM = UnityMolMain.getSelectionManager();

        if (String.IsNullOrEmpty(APBSMainFolder)) {
            string mainAPBSFolder = PlayerPrefs.GetString("APBSFolder");
            if (!String.IsNullOrEmpty(mainAPBSFolder)) {
                APBSMainFolder = mainAPBSFolder.Replace("\\", "/");
            }
        }

        if (APBSMainFolder == null) {
            APBSMainFolder = "C:/APBS_PDB2PQR/";
        }

        udapteAPBSToolPaths();

        if (dime_x != null) {
            dime_x.text = "97";
        }
        if (dime_y != null) {
            dime_y.text = "129";
        }
        if (dime_z != null) {
            dime_z.text = "129";
        }
        if (cglen_x != null) {
            cglen_x.text = "60.7855";
        }
        if (cglen_y != null) {
            cglen_y.text = "62.2628";
        }
        if (cglen_z != null) {
            cglen_z.text = "64.9607";
        }
        if (fglen_x != null) {
            fglen_x.text = "55.7562";
        }
        if (fglen_y != null) {
            fglen_y.text = "56.6252";
        }
        if (fglen_z != null) {
            fglen_z.text = "58.2122";
        }
        if (pdie != null) {
            pdie.text = "8";
        }
        if (sdie != null) {
            sdie.text = "80";
        }
        if (sdens != null) {
            sdens.text = "10.00";
        }
        if (srad != null) {
            srad.text = "1.40";
        }
        if (swin != null) {
            swin.text = "0.30";
        }
        if (temp != null) {
            temp.text = "298.15";
        }
        if (epsilon != null) {
            epsilon.text = "80";
        }
    }

    public void desktopGetFolder(InputField inf) {
        string path = null;
#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_WEBGL )
        if (!UnityMolMain.inVR()) {
            string[] paths = StandaloneFileBrowser.OpenFolderPanel("Get Folder", "", false);
            if (paths != null && paths.Length > 0) {

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                path = (paths[0] + "/").Replace("file:/", "");
#else
                path = (paths[0] + "/").Replace("\\", "/");
#endif
            }
        }
#else

#endif
        if (path != null) {
            inf.text = path;
        }
    }
    public void desktopGetFile(InputField inf) {
        string path = null;
#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_WEBGL )
        if (!UnityMolMain.inVR()) {
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Get Folder", "", "", false);
            if (paths != null && paths.Length > 0)
            {

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                path = paths[0].Replace("file:/", "");
#else
                path = paths[0].Replace("\\", "/");
#endif
            }
        }
#else

#endif
        if (path != null) {
            inf.text = path;
        }
    }

    public void sendAPBS_path() {
        APBS_path.text = (topBrowserAPBS_Exe.text + "/" + bottomBrowserAPBS_EXE.text);
    }

    public void sendAPBS_save_path() {
        APBS_save_path.text = (topBrowserAPBS_save.text + "/");
    }

    public void sendAPBS_ammendInFile_path() {
        APBS_ammendInFile.text = (topBrowserAPBS_Ammend.text + "/" + bottomBrowserAPBS_Ammend.text);
    }

    public void sendMultivalue_path() {
        Multivalue_path.text = (topBrowserMultivalue.text + "/" + bottomBrowserMultivalue.text);
    }

    public void sendCoulomb_path() {
        Coulomb_path.text = (topBrowserCoulomb.text + "/" + bottomBrowserCoulomb.text);
    }

    public void sendBorn_path() {
        Born_path.text = (topBrowserBorn.text + "/" + bottomBrowserBorn.text);
    }

    public void sendHBPath() {
        HB_in.text = (topBrowserHB.text + "/");
        Debug.Log(topBrowserHB.text + "/");
    }

    public void sendSBPath() {
        SB_in.text = (topBrowserSB.text + "/");
        Debug.Log(topBrowserSB.text + "/");
    }

    public void APBS() {
        if (selM == null) {
            selM = UnityMolMain.getSelectionManager();
        }

        UnityMolSelection sel = selM.currentSelection;

        if (sel == null) {
            UnityEngine.Debug.LogWarning("Using the last loaded molecule as input");
            sel = APIPython.last().ToSelection();
        }
        string cutCurSel = sel.structures[0].uniqueName;

        apbs_count++;
        apbs_fromSelection(cutCurSel, apbs_count);
    }

    public void APBS_multiValue() {
        if (selM == null) {
            selM = UnityMolMain.getSelectionManager();
        }
        multiValue_APBS++;

        UnityMolSelection sel = selM.currentSelection;
        if (sel == null || sel.Count == 0) {
            Debug.LogWarning("Please select atoms !");
            return;
        }

        string molName = sel.structures[0].uniqueName;

        Tuple<string, string> pathAndName = SaveTextFile.writeXYZ_fromSelection(sel.name, multiValue_APBS);

        if (pathAndName != null) {
            multiValue(pathAndName.Item1, pathAndName.Item2, molName);
        }
        else
        {
            Debug.LogWarning("multivalue failed");
        }

    }

    public void APBS_born() {
        born_APBS++;

        if (selM == null) {
            selM = UnityMolMain.getSelectionManager();
        }
        UnityMolSelection sel = selM.currentSelection;
        if (sel == null) {
            Debug.LogWarning("Using the last loaded molecule as input");
            sel = APIPython.last().ToSelection();
        }
        string cutCurSel = sel.structures[0].uniqueName;

        run_born(cutCurSel, BornF, BornAI, born_APBS);
    }

    public void APBS_coulomb() {
        coulomb_APBS++;

        if (selM == null) {
            selM = UnityMolMain.getSelectionManager();
        }
        UnityMolSelection sel = selM.currentSelection;
        if (sel == null) {
            Debug.LogWarning("Using the last loaded molecule as input");
            sel = APIPython.last().ToSelection();
        }
        string cutCurSel = sel.structures[0].uniqueName;

        run_coulomb_process(cutCurSel, CoulombE, CoulombF, coulomb_APBS);
    }

    public void apbs_fromSelection(string structureName, int count)
    {

        if (!System.IO.Directory.Exists(APBS_save_path.text)) {
            //Try to create it
            try {
                System.IO.Directory.CreateDirectory(APBS_save_path.text);
            }
            catch {
                Debug.LogWarning("Please specify the path to the APBS output folder. '" + APBS_save_path.text + "' not found");
                return;
            }
        }
        if (!System.IO.File.Exists(APBS_path.text)) {
            Debug.LogWarning("Please specify the path to the APBS executable. '" + APBS_path.text + "' not found");
            return;
        }

        string inFileName = (structureName + "_.in"); // file for apbs to load

        string finalArgs = Path.Combine(APBS_save_path.text, inFileName).Replace("\\", "/");

        string writePath = Path.Combine(APBS_save_path.text, structureName + "_APBS_out_" + count + ".txt").Replace("\\", "/"); // text file created from terminal run

        StreamWriter writer = new StreamWriter(writePath, true);

        Debug.Log("Running apbs: " + APBS_path.text + " " + finalArgs);

        try
        {
            using (Process process = new Process())
            {
                // System.IO.Directory.SetCurrentDirectory(pdb2pqrOutFolder); // change to pqr folder

                process.StartInfo.WorkingDirectory = APBS_save_path.text.Replace("\\", "/");
                process.StartInfo.FileName = APBS_path.text.Replace("\\", "/");
                process.StartInfo.Arguments = finalArgs.Replace("\\", "/");
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                StreamReader reader = process.StandardOutput; //read process
                string output = reader.ReadToEnd(); //read all

                writer.Write(output); //write output
                process.WaitForExit();
                writer.Close();
            }
        }
        catch
        {
            Debug.LogWarning("APBS went wrong");
        }

        string dxToLoad = Path.Combine(APBS_save_path.text, structureName + ".dx").Replace("\\", "/");

        APIPython.loadDXmap(structureName, dxToLoad);
        APIPython.showSelection(UnityMolMain.getStructureManager().GetStructure(structureName).ToSelectionName(), "fl");
    }

    public void apbs_fromSelection_changedIn(string structureName, int count, int inFileAmmendCt, string dxToLoad) {
        string inFileName = (structureName + "_UserChange_" + inFileAmmendCt + ".in"); // file for apbs to load

        string finalArgs = Path.Combine(APBS_save_path.text, inFileName).Replace("\\", "/");

        string writePath = Path.Combine(APBS_save_path.text, structureName + "_APBS_out_" + count + ".txt").Replace("\\", "/"); // text file created from terminal run

        StreamWriter writer = new StreamWriter(writePath, true);

        Debug.Log("Running apbs: " + APBS_path.text + " " + finalArgs);

        try
        {
            using (Process process = new Process())
            {
                // System.IO.Directory.SetCurrentDirectory(pdb2pqrOutFolder); // change to pqr folder

                process.StartInfo.WorkingDirectory = APBS_save_path.text;
                process.StartInfo.FileName = APBS_path.text;
                process.StartInfo.Arguments = finalArgs;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                StreamReader reader = process.StandardOutput; //read process
                string output = reader.ReadToEnd(); //read all

                writer.Write(output); //write output
                process.WaitForExit();
                writer.Close();
            }
        }
        catch
        {
            Debug.LogWarning("APBS went wrong");
        }

        //string dxToLoad = Path.Combine(APBS_save_path.text, structureName + ".dx");
        //try to unload current dx if already loaded
        try
        {
            APIPython.unloadDXmap(structureName);
        }
        catch //don't freak out if no dx loaded
        {
            Debug.Log("No dx currently loaded");
        }
        //load new dx map
        APIPython.loadDXmap(structureName, (dxToLoad + ".dx"));
        Debug.Log("new dx loaded is: " + dxToLoad + ".dx");
    }

    public void multiValue(string pathToMulti, string fixedFileName, string mol) {
        //Usage: multivalue <csvCoordinatesFile> <dxFormattedFile> <outputFile> [outputformat]

        string dirToMultiTextFile = (pathToMulti.Substring(0, (pathToMulti.Length - fixedFileName.Length)));
        string dxFileName = (mol + ".dx");
        string outputFile = (fixedFileName.Substring(0, (fixedFileName.Length - 4)) + "_multiValue_out.txt"); //save file as

        string multiValueArgs = (fixedFileName + " " + dxFileName + " " + outputFile);

        string writePath = APBS_save_path.text + mol + "_multivalueRaw_out.txt";

        StreamWriter writer = new StreamWriter(writePath, true);
        writer.WriteLine("MultiValue out from UnityMol");
        writer.WriteLine("Terminal Command: " + Multivalue_path.text + " " + multiValueArgs);

        try
        {
            using (Process process = new Process())
            {
                process.StartInfo.WorkingDirectory = APBS_save_path.text;
                process.StartInfo.FileName = Multivalue_path.text;
                process.StartInfo.Arguments = multiValueArgs;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                StreamReader reader = process.StandardOutput; //read process
                string output = reader.ReadToEnd(); //read all

                writer.WriteLine(output); //write output

                process.WaitForExit();
                writer.Close();
            }
        }
        catch
        {
            Debug.LogWarning("multivalue went wrong");
            return;
        }
        Debug.Log("Multivalue done");

    }

    public void run_born(string mol, bool born_f, bool born_ai, int count) {
        string v_input = "";
        string f_input = "";

        if (born_f) {
            f_input = "-f ";
        }
        if (born_ai) {
            v_input = "-v ";
        }

        //needs born [-v] [-f] <epsilon> <molecule.pqr>
        string bornArgs = (f_input + v_input + epsilon.text + " " + mol + ".pqr");
        string writePath = Path.Combine(APBS_save_path.text, mol + "_born_out_" + count + ".txt").Replace("\\", "/");

        StreamWriter writer = new StreamWriter(writePath, true);
        writer.WriteLine("MultiValue out from UnityMol");
        writer.WriteLine("Terminal Command: " + Born_path.text + " " + bornArgs);

        try
        {
            using (Process process = new Process())
            {
                // System.IO.Directory.SetCurrentDirectory(pdb2pqrOutFolder); // change to pqr folder

                process.StartInfo.WorkingDirectory = APBS_save_path.text;
                process.StartInfo.FileName = Born_path.text;
                process.StartInfo.Arguments = bornArgs;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                StreamReader reader = process.StandardOutput; //read process
                string output = reader.ReadToEnd(); //read all

                writer.WriteLine(output); //write output

                process.WaitForExit();
                writer.Close();
            }
        }
        catch
        {
            Debug.LogWarning("born went wrong");
        }

        Debug.Log("born done");
    }

    public void run_coulomb_process(string mol, bool clm_e, bool clm_f, int count) {
        string e_input = "";
        string f_input = "";

        if (clm_e) {
            e_input = "-e ";
        }
        if (clm_f) {
            f_input = "-f ";
        }

        //needs coulomb [-e] [-f] <molecule.pqr>

        string coulombArgs = (e_input + f_input + mol + ".pqr");
        string writePath = APBS_save_path.text + mol + "_coulomb_out_" + count + ".txt";

        StreamWriter writer = new StreamWriter(writePath, true);
        writer.WriteLine("MultiValue out from UnityMol");
        writer.WriteLine("Terminal Command: " + Coulomb_path.text + " " + coulombArgs);

        try
        {
            using (Process process = new Process())
            {
                // System.IO.Directory.SetCurrentDirectory(pdb2pqrOutFolder); // change to pqr folder

                process.StartInfo.WorkingDirectory = APBS_save_path.text;
                process.StartInfo.FileName = Coulomb_path.text;
                process.StartInfo.Arguments = coulombArgs;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                StreamReader reader = process.StandardOutput; //read process
                string output = reader.ReadToEnd(); //read all

                writer.WriteLine(output); //write output

                process.WaitForExit();
                writer.Close();
            }
        }
        catch
        {
            Debug.LogWarning("coulomb went wrong");
        }
        Debug.Log("coulomb done");
    }

    public void changeInFile() {
        //setup selection
        if (selM == null) {
            selM = UnityMolMain.getSelectionManager();
        }
        UnityMolSelection sel = selM.currentSelection;
        if (sel == null) {
            UnityEngine.Debug.LogWarning("Using the last loaded molecule as input");
            sel = APIPython.last().ToSelection();
        }

        //set unique name for structure
        string StructureName = sel.structures[0].uniqueName;
        //.in file to ammend
        string FileToChange = APBS_ammendInFile.text;
        if (string.IsNullOrEmpty(FileToChange) || !System.IO.File.Exists(FileToChange)) {
            Debug.LogWarning("Please specity a path to the input file to edit");
            return;
        }

        //New .in file name
        string writePath = APBS_save_path.text + StructureName + "_UserChange_" + inFileAmmendCount + ".in";
        //new dx file name
        string newDXFileName = APBS_save_path.text + StructureName + "_UserChange_" + inFileAmmendCount;

        StreamReader sr = new StreamReader(FileToChange);
        StreamWriter writer = new StreamWriter(writePath, true);

        using (sr) {
            string line;

            try
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("    dime"))
                    {
                        string dimeInput = ("    dime " + dime_x.text + " " + dime_y.text + " " + dime_z.text);
                        Debug.Log(dimeInput);
                        writer.WriteLine(dimeInput);
                    }
                    else if (line.StartsWith("    cglen"))
                    {
                        string cglenInput = ("    cglen " + cglen_x.text + " " + cglen_y.text + " " + cglen_z.text);
                        Debug.Log(cglenInput);
                        writer.WriteLine(cglenInput);
                    }
                    else if (line.StartsWith("    fglen"))
                    {
                        string fglenInput = ("    fglen " + fglen_x.text + " " + fglen_y.text + " " + fglen_z.text);
                        Debug.Log(fglenInput);
                        writer.WriteLine(fglenInput);
                    }
                    else if (line.StartsWith("    pdie"))
                    {
                        string pdieInput = ("    pdie " + pdie.text);
                        Debug.Log(pdieInput);
                        writer.WriteLine(pdieInput);
                    }
                    else if (line.StartsWith("    sdie"))
                    {
                        string sdieInput = ("    sdie " + sdie.text);
                        Debug.Log(sdieInput);
                        writer.WriteLine(sdieInput);
                    }
                    else if (line.StartsWith("    sdens"))
                    {
                        string sdensInput = ("    sdens " + sdens.text);
                        Debug.Log(sdensInput);
                        writer.WriteLine(sdensInput);
                    }
                    else if (line.StartsWith("    srad"))
                    {
                        string sradInput = ("    srad " + srad.text);
                        Debug.Log(sradInput);
                        writer.WriteLine(sradInput);
                    }
                    else if (line.StartsWith("    swin"))
                    {
                        string swinInput = ("    swin " + swin.text);
                        Debug.Log(swinInput);
                        writer.WriteLine(swinInput);
                    }
                    else if (line.StartsWith("    temp"))
                    {
                        string tempInput = ("    temp " + temp.text);
                        Debug.Log(tempInput);
                        writer.WriteLine(tempInput);
                    }
                    else if (line.StartsWith("    write"))
                    {
                        string renameNewDX = ("    write pot dx " + newDXFileName);
                        Debug.Log(renameNewDX);
                        writer.WriteLine(renameNewDX);
                    }
                    else
                    {
                        writer.WriteLine(line);
                        Debug.Log(line);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to load the input file " + FileToChange + "\n" + e);
            }

            writer.Close();
        }
        inFileAmmendCount++;
        apbs_fromSelection_changedIn(StructureName, apbs_count, (inFileAmmendCount - 1), newDXFileName);
    }


    // UI
    public void turnOnCoulombE() {
        CoulombE = !CoulombE;
    }
    public void turnOnCoulombF() {
        CoulombF = !CoulombF;
    }
    public void turnOnBornAtomInfo() {
        BornAI = !BornAI;
    }
    public void turnOnBornForces() {
        BornF = !BornF;
    }

    public void ShowSaltBridge_APBS() {
        //check current selection !null
        if (selM == null) {
            selM = UnityMolMain.getSelectionManager();
        }
        if (UnityMolMain.getStructureManager().loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        //Set sel to current selection
        UnityMolSelection sel = selM.currentSelection;

        //If nothing selected set to last selection
        if (sel == null) {
            Debug.LogWarning("Using the last loaded molecule as input");
            sel = APIPython.last().ToSelection();
        }

        //Current selection unique name
        string cutCurSel = sel.structures[0].uniqueName;

        //Location of Salt bridge directory from inputField
        string SBFP = SB_in.text;
        APBS_Hbonds.SaltBridge_APBS(cutCurSel, SBFP);
    }

    public void ShowHbonds_APBS() {
        //check current selection !null
        if (selM == null) {
            selM = UnityMolMain.getSelectionManager();
        }
        if (UnityMolMain.getStructureManager().loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        //Set sel to current selection
        UnityMolSelection sel = selM.currentSelection;

        //If nothing selected set to last selection
        if (sel == null) {
            Debug.LogWarning("Using the last loaded molecule as input");
            sel = APIPython.last().ToSelection();
        }

        //Current selection unique name
        string cutCurSel = sel.structures[0].uniqueName;

        //Location of HBond directory from inputField
        string HBFP = HB_in.text;
        APBS_Hbonds.HBOND_APBS(cutCurSel, HBFP);
    }

    public void unLoadDX() {
        //check current selection !null
        if (selM == null) {
            selM = UnityMolMain.getSelectionManager();
        }
        if (UnityMolMain.getStructureManager().loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        //Set sel to current selection
        UnityMolSelection sel = selM.currentSelection;

        //If nothing selected set to last selection
        if (sel == null) {
            Debug.LogWarning("Using the last loaded molecule as input");
            sel = APIPython.last().ToSelection();
        }

        //Current selection unique name
        string cutCurSel = sel.structures[0].uniqueName;

        APIPython.unloadDXmap(cutCurSel);
    }
}
}