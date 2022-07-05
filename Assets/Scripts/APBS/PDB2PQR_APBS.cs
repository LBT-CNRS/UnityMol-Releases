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


using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using System.Text;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;
using UMol.API;
using System.Diagnostics;
using UnityEngine.XR;

using Debug = UnityEngine.Debug;

namespace UMol
{
public class PDB2PQR_APBS : MonoBehaviour
{

    public APBS_Tools APBS_ToolsInstance;
    public string PDB2PQR_exePath;

    public static bool apbs_in = true;
    public static bool nodebump = false;
    public static bool noopt = false;
    public static bool chain_pdb2pqr = false;
    public static bool assignOnly = false;
    public static bool whitespace_pdb2pqr = false;
    public static bool typemap = false;
    public static bool neutralN = false;
    public static bool neutralC = false;
    public static bool dropWater = false;
    public static bool IncludeHeader = false;
    public static bool pHCalc = false;
    public static bool withpH = false;
    public static bool rama = false;
    public static bool phi_only = false;
    public static bool psi_only = false;
    public static bool resinter = false;
    public static bool ResidComb = false;
    public static bool AllResidComb = false;
    public static bool hbond = false;
    public static bool whatif = false;
    public static bool oldDisMethod = false;
    public static bool chi = false;
    public static bool summary = false;
    public static bool contact = false;
    public static bool newresinter = false;
    public static bool salt = false;
    public static bool pdb2pka_resume = false;
    public static bool propka_verbose = false;
    public static bool output_file = false;
    public static bool output_format = false;

    private UnityMolSelectionManager selM;

    public GameObject ffDropdownLabel;
    public GameObject pHLabelDropdownLabel;
    public GameObject pdb2pka_outInputFieldPlaceholder;
    public GameObject ffOutDropdownLabel;

    //File browser fields
    public InputField topBrowserPDB2PQR;
    public InputField bottomBrowserPDB2PQR;
    public InputField topBrowserCustomFF;
    public InputField bottomBrowserCustomFF;
    public InputField topBrowserCustomNames;
    public InputField bottomBrowserCustomNames;
    public InputField topBrowserSavePath;

    //pdb2pqr inputfields
    public InputField pdb2pqr_path; //.exe path
    public InputField pdb2pqr_save_path;
    public InputField FindFF_in;
    public InputField FindFFnames_in;
    public InputField withPH_in;
    public InputField pdie_in;
    public InputField sdie_in;
    public InputField pairene_in;
    public InputField angleCut_in;
    public InputField distCut_in;

    private bool born_v = false;
    private bool born_f = false;
    private bool coulomb_e = false;
    private bool coulomb_f = false;

    private int savedState_APBS = 0;
    private int pdb2pqr_count = 0;


    void Start() {
        selM = UnityMolMain.getSelectionManager();


        if (APBS_ToolsInstance == null) {
            APBS_Tools[] apbsts = FindObjectsOfType<APBS_Tools>();
            if (apbsts.Length >= 1) {
                APBS_ToolsInstance = apbsts[0];
            }
        }

        if (String.IsNullOrEmpty(APBS_Tools.APBSMainFolder)) {
            string mainAPBSFolder = PlayerPrefs.GetString("APBSFolder");
            if (!String.IsNullOrEmpty(mainAPBSFolder)) {
                APBS_Tools.APBSMainFolder = mainAPBSFolder.Replace("\\", "/");
            }
        }

        if (APBS_ToolsInstance != null)
            APBS_ToolsInstance.udapteAPBSToolPaths();

        if (APBS_Tools.APBSMainFolder == null) {
            APBS_Tools.APBSMainFolder = "C:/APBS_PDB2PQR/";
        }


        if (FindFF_in != null) {
            FindFF_in.text = "none";
        }
        if (FindFFnames_in != null) {
            FindFFnames_in.text = "none";
        }
        if (withPH_in != null) {
            withPH_in.text = "7.0";
        }
        if (pdie_in != null) {
            pdie_in.text = "8";
        }
        if (sdie_in != null) {
            sdie_in.text = "80";
        }
        if (pairene_in != null) {
            pairene_in.text = "1.0";
        }
        if (angleCut_in != null) {
            angleCut_in.text = "30.0";
        }
        if (distCut_in != null) {
            distCut_in.text = "3.4";
        }

        updatePDB2PQRPaths();
    }
    public void updatePDB2PQRPaths() {
        if (pdb2pqr_path != null) {
            bool found = findPDB2PQRPath();
            if (!found) {
                PDB2PQR_exePath = Path.Combine(APBS_Tools.APBSMainFolder, "pdb2pqr/pdb2pqr.exe").Replace("\\", "/");
            }
            else {
                APBS_Tools.APBSMainFolder = (Directory.GetParent(Directory.GetParent(PDB2PQR_exePath).FullName).FullName).Replace("\\", "/");
                PlayerPrefs.SetString("APBSFolder", APBS_Tools.APBSMainFolder);
            }
            pdb2pqr_path.text = PDB2PQR_exePath;
        }

        if (pdb2pqr_save_path != null && !System.IO.Directory.Exists(pdb2pqr_save_path.text)) {
            pdb2pqr_save_path.text = Path.Combine(APBS_Tools.APBSMainFolder, "OutputFiles/").Replace("\\", "/");
        }
    }

    bool findPDB2PQRPath() {
        if (string.IsNullOrEmpty(PDB2PQR_exePath) || !System.IO.File.Exists(PDB2PQR_exePath)) {
            if (System.IO.Directory.Exists(APBS_Tools.APBSMainFolder)) {
                string[] subdirs = Directory.GetDirectories(APBS_Tools.APBSMainFolder);
                foreach (string sd in subdirs) {
                    if (new DirectoryInfo(sd).Name.StartsWith("pdb2pqr")) {
                        string[] files = Directory.GetFiles(sd, "pdb2pqr*");
                        if (files.Length >= 1) {
                            PDB2PQR_exePath = files[0].Replace("\\", "/");
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void setAPBSMainFolder(InputField inf) {
        string fullpath = inf.text;
        APBS_Tools.APBSMainFolder = Directory.GetParent(Directory.GetParent(fullpath).FullName).FullName.Replace("\\", "/");
        PlayerPrefs.SetString("APBSFolder", APBS_Tools.APBSMainFolder);

        updatePDB2PQRPaths();

        if (APBS_ToolsInstance != null) {
            APBS_ToolsInstance.udapteAPBSToolPaths();
        }

    }

    public void desktopGetFolder(InputField inf) {
        if (APBS_ToolsInstance != null) {
            APBS_ToolsInstance.desktopGetFolder(inf);
        }
    }

    public void desktopGetFile(InputField inf) {
        if (APBS_ToolsInstance != null) {
            APBS_ToolsInstance.desktopGetFile(inf);
        }
    }

    public static void writePDB_fromSelection(string structureName, string pdb2pqr_save_path) {

        if (!System.IO.Directory.Exists(pdb2pqr_save_path)) {
            Debug.LogWarning("Please specify the path to the APBS/PDB2PQR folder");
            return;
        }


        //get structure manager, send to selection, name file with structure name, write pdb file
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);
        if (s == null) {
            Debug.LogError("Structure not found, couldn't write the PDB file");
            return;
        }
        UnityMolSelection select = s.ToSelection();
        string pdbLines = PDBReader.Write(select);
        string pdb2pqrOutFileName = pdb2pqr_save_path + structureName + ".pdb";
        Debug.Log("pdb2pqrOutFileName = " + pdb2pqrOutFileName);
        StreamWriter writer = new StreamWriter(pdb2pqrOutFileName, false);
        writer.WriteLine(pdbLines);
        writer.Close();
    }

    public void pdb2pqr_fromSelection(string structureName, int count) {

        if (!System.IO.Directory.Exists(pdb2pqr_save_path.text)) {
            //Try to create it
            try {
                System.IO.Directory.CreateDirectory(pdb2pqr_save_path.text);
            }
            catch {
                Debug.LogWarning("Please specify the path to the APBS/PDB2PQR folder. '" + pdb2pqr_save_path.text + "' not found");
                return;
            }
        }
        if (!System.IO.File.Exists(pdb2pqr_path.text)) {
            Debug.LogWarning("Please specify the path to the PDB2PQR executable. '" + pdb2pqr_path.text + "' not found");
            return;
        }

        //get structure manager, send to selection, name file with structure name, write pdb file
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(structureName);

        string pdb2pqrOutFileName = pdb2pqr_save_path.text + structureName + ".pdb";
        Debug.Log("pdb2pqrOutFileName = " + pdb2pqrOutFileName);
        Debug.Log("structurename = " + structureName);
        writePDB_fromSelection(structureName, pdb2pqr_save_path.text);

        //pdb2pqr arguments
        string Mandatory_Args = ("--ff=" + ffDropdownLabel.GetComponent<Text>().text);
        string General_Args = "";
        string pHCalc_Args = null;
        string Final_Args = "";
        string Extensions_Args = "";
        string HBond_Args = "";
        string Resinter_Args = "";
        string Rama_Args = "";
        string pdb2pka_out = null;
        string pdie = null;
        string sdie = null;
        string pairene = null;
        string ligand = null;
        string ffout_Args = null;
        string userff_Args = null;
        string userNames_Args = null;

        Debug.Log("ph dropdown = " + GameObject.Find("pHLabelDropdownLabel").GetComponent<Text>().text);

        //Ask for result of toggles, dropdowns, and text boxes
        if (GameObject.Find("pHLabelDropdownLabel").GetComponent<Text>().text == "none") {
            pHCalc_Args = null;
        }
        else if (GameObject.Find("pHLabelDropdownLabel").GetComponent<Text>().text == "propka") {
            string method = GameObject.Find("pHLabelDropdownLabel").GetComponent<Text>().text;
            string pH = GameObject.Find("with-pH_InputText").GetComponent<Text>().text;
            pHCalc_Args = (" --ph-calc-method=" + method + " --with-ph=" + pH);
        }

        else if (GameObject.Find("pHLabelDropdownLabel").GetComponent<Text>().text == "pdb2pka") {
            string method = GameObject.Find("pHLabelDropdownLabel").GetComponent<Text>().text;
            string pH = GameObject.Find("with-pH_InputText").GetComponent<Text>().text;
            pHCalc_Args = (" --ph-calc-method=" + method + " --with-ph=" + pH);
        }

        if (angleCut_in.text != "30.0") {
            HBond_Args = String.Concat(HBond_Args, " --angle_cutoff=" + angleCut_in.text);
        }

        if (distCut_in.text != "3.4") {
            HBond_Args = String.Concat(HBond_Args, " --distance_cutoff=" + distCut_in.text);
        }

        if (pdie_in.text != "8") {
            pdie = String.Concat(pdie, " --pdie=" + pdie_in.text);
        }
        if (sdie_in.text != "80") {
            sdie = String.Concat(sdie, " --sdie=" + sdie_in.text);
        }
        if (pairene_in.text != "1.0") {
            pairene = String.Concat(pairene, " --pairene=" + pairene_in.text);
        }

        if (GameObject.Find("ffOutDropdownLabel").GetComponent<Text>().text != "none") {
            ffout_Args = " --ffout=" + GameObject.Find("ffOutDropdownLabel").GetComponent<Text>().text;
        }

        if (FindFF_in.text != "none") {
            Mandatory_Args = "";
            userff_Args = " --userff=" + FindFF_in.text;
        }

        if (FindFFnames_in.GetComponentInChildren<Text>().text != "none") {
            userNames_Args = " --usernames=" + FindFFnames_in.text;
        }

        if (apbs_in) {
            General_Args = String.Concat(General_Args, " --apbs-input");
        }
        if (nodebump) {
            General_Args = String.Concat(General_Args, " --nodebump");
        }
        if (noopt) {
            General_Args = String.Concat(General_Args, " --noopt");
        }
        if (chain_pdb2pqr) {
            General_Args = String.Concat(General_Args, " --chain");
        }
        if (assignOnly) {
            General_Args = String.Concat(General_Args, " --assign-only");
        }
        if (whitespace_pdb2pqr) {
            General_Args = String.Concat(General_Args, " --whitespace");
        }
        if (typemap) {
            General_Args = String.Concat(General_Args, " --typemap");
        }
        if (neutralN) {
            General_Args = String.Concat(General_Args, " --neutraln");
        }
        if (neutralC) {
            General_Args = String.Concat(General_Args, " --neutralc");
        }
        if (dropWater) {
            General_Args = String.Concat(General_Args, " --drop-water");
        }
        if (IncludeHeader) {
            General_Args = String.Concat(General_Args, " --include-header");
        }

        if (rama) {
            Rama_Args = String.Concat(Rama_Args, " --rama");
        }
        if (phi_only) {
            Rama_Args = String.Concat(Rama_Args, " --phi_only");
        }
        if (psi_only) {
            Rama_Args = String.Concat(Rama_Args, " --psi_only");
        }
        if (resinter) {
            Resinter_Args = String.Concat(Resinter_Args, " --resinter");
        }
        if (ResidComb) {
            Resinter_Args = String.Concat(Resinter_Args, " --residue_combinations");
        }
        if (AllResidComb) {
            Resinter_Args = String.Concat(Resinter_Args, " --all_residue_combinations");
        }
        //
        if (hbond) {
            HBond_Args = String.Concat(HBond_Args, " --hbond");
        }
        if (whatif) {
            HBond_Args = String.Concat(HBond_Args, " --whatif");
        }
        if (oldDisMethod) {
            HBond_Args = String.Concat(HBond_Args, " --old_distance_method");
        }
        if (chi) {
            Extensions_Args = String.Concat(Extensions_Args, " --chi");
        }
        if (summary) {
            Extensions_Args = String.Concat(Extensions_Args, " --summary");
        }
        if (contact) {
            Extensions_Args = String.Concat(Extensions_Args, " --contact");
        }
        if (newresinter) {
            Extensions_Args = String.Concat(Extensions_Args, " --newresinter");
        }
        if (salt) {
            Extensions_Args = String.Concat(Extensions_Args, " --salt");
        }
        if (pdb2pka_resume) {
            Extensions_Args = String.Concat(Extensions_Args, " --pdb2pka-resume");
        }
        if (propka_verbose) {
            Extensions_Args = String.Concat(Extensions_Args, " --propka-verbose");
        }

        //set pqr file name
        string cutPDBFileName_forPQR = Path.Combine(pdb2pqr_save_path.text, structureName + "_APBS.pqr").Replace("\\", "/");

        //build final args from if statements above
        StringBuilder sb = new StringBuilder();
        sb.Append(Mandatory_Args);
        sb.Append(userff_Args);
        sb.Append(userNames_Args);
        sb.Append(General_Args);
        sb.Append(pHCalc_Args);
        sb.Append(ffout_Args);
        sb.Append(Extensions_Args);
        sb.Append(HBond_Args);
        sb.Append(Resinter_Args);
        sb.Append(Rama_Args);
        sb.Append(pdb2pka_out);
        sb.Append(pdie);
        sb.Append(sdie);
        sb.Append(ligand);
        sb.Append(pairene);
        sb.Append(" ");
        sb.Append(pdb2pqrOutFileName);
        sb.Append(" ");
        sb.Append(cutPDBFileName_forPQR);

        Final_Args = sb.ToString();

        string writePath = pdb2pqrOutFileName + "_pdb2pqr_out_" + count + ".txt";  //set write path wtih iteration of pdb2pqr out
        StreamWriter write_pdb2pqr = new StreamWriter(writePath, true);  //setup SR
        write_pdb2pqr.WriteLine("PDB2PQR out from UnityMolX");
        write_pdb2pqr.WriteLine("Terminal Command: " + pdb2pqr_path.text + " " + Final_Args);

        bool success = false;
        Debug.Log("Starting PDB2PQR");
        try {
            using (Process process = new Process())
            {
                // System.IO.Directory.SetCurrentDirectory(pdb2pqrOutFolder); // change to pqr folder
                string pdb2pqrPath = pdb2pqr_path.text;
                process.StartInfo.WorkingDirectory = pdb2pqr_save_path.text;
                process.StartInfo.FileName = pdb2pqrPath;
                process.StartInfo.Arguments = Final_Args;
                Debug.Log("command = " + pdb2pqrPath + " " + Final_Args);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                StreamReader reader = process.StandardOutput; //read process
                string output = reader.ReadToEnd(); //read all

                write_pdb2pqr.WriteLine(output); //write output
                process.WaitForExit();
                success = true;
            }
        }
        catch (System.Exception e) {
            Debug.LogWarning("pdb2pqr went wrong");
#if UNITY_EDITOR
            Debug.LogWarning(e);
#endif
            success = false;
        }

        write_pdb2pqr.Close();

        General_Args = "";
        Final_Args = "";
        Extensions_Args = "";
        HBond_Args = "";
        Resinter_Args = "";
        Rama_Args = "";
        ffout_Args = "";
        userff_Args = "";
        userNames_Args = "";

        if (success) {
            loadNewPQR(cutPDBFileName_forPQR);
            APIPython.hideStructureAllRepresentations(structureName);

            PlayerPrefs.SetString("APBSFolder", (Directory.GetParent(Directory.GetParent(PDB2PQR_exePath)
                                                 .FullName).FullName).Replace("\\", "/"));

        }
    }

    public static void loadNewPQR(string pqrFileName) {
        APIPython.load(pqrFileName);

        changeInFile(pqrFileName);
    }

    public static void changeInFile(string inFileName) {
        string pathWithoutExt = System.IO.Path.ChangeExtension(inFileName, null);
        string cutInFileName_forDX = pathWithoutExt + ".in";
        string writePath = pathWithoutExt + "_.in";

        StreamReader sr = new StreamReader(cutInFileName_forDX);
        StreamWriter writer = new StreamWriter(writePath, true);

        using (sr) {
            string line;

            try
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("    write"))
                    {
                        string cutOutPQR = line.Substring(0, (line.Length - 4));
                        // Debug.Log(cutOutPQR);
                        writer.WriteLine(cutOutPQR);
                    }
                    else
                    {
                        writer.WriteLine(line);
                        // Debug.Log(line);
                    }

                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to load the input file " + cutInFileName_forDX + "\n" + e);
            }

            writer.Close();
        }
    }

    public void sendPDb2PQR_path() {
        pdb2pqr_path.text = (topBrowserPDB2PQR.text + "/" + bottomBrowserPDB2PQR.text);
        PDB2PQR_exePath = pdb2pqr_path.text;
        Debug.Log(topBrowserCustomFF.text + "/" + bottomBrowserCustomFF.text);

        if (APBS_ToolsInstance != null)
            APBS_ToolsInstance.udapteAPBSToolPaths();

        pdb2pqr_save_path.text = Path.Combine(APBS_Tools.APBSMainFolder, "OutputFiles/").Replace("\\", "/");
    }

    public void sendPDb2PQR_save_path() {
        pdb2pqr_save_path.text = (topBrowserSavePath.text + "/");
        Debug.Log(topBrowserCustomFF.text + "/");
    }

    public void sendCustomFF() {
        FindFF_in.text = (topBrowserCustomFF.text + "/" + bottomBrowserCustomFF.text);
        Debug.Log(topBrowserCustomFF.text + "/" + bottomBrowserCustomFF.text);
    }

    public void sendCustomNames() {
        FindFFnames_in.text = (topBrowserCustomNames.text + "/" + bottomBrowserCustomNames.text);
        Debug.Log(topBrowserCustomNames.text + "/" + bottomBrowserCustomNames.text);
    }

    // UI
    public void turnOnAPBS_In(Toggle t) {
        apbs_in = t.isOn;
    }
    public void turnOnNodeBump(Toggle t) {
        nodebump = t.isOn;
    }
    public void turnOnNoOpt(Toggle t) {
        noopt = t.isOn;
    }
    public void turnOnChain(Toggle t) {
        chain_pdb2pqr = t.isOn;
    }
    public void turnOnAssignOnly(Toggle t) {
        assignOnly = t.isOn;
    }
    public void turnOnWhitespace(Toggle t) {
        whitespace_pdb2pqr = t.isOn;
    }
    public void turnOntypemap(Toggle t) {
        typemap = t.isOn;
    }
    public void turnOnNeutralN(Toggle t) {
        neutralN = t.isOn;
    }
    public void turnOnNeutralC(Toggle t) {
        neutralC = t.isOn;
    }
    public void turnOnDropWater(Toggle t) {
        dropWater = t.isOn;
    }
    public void turnOnIncludeHeader(Toggle t) {
        IncludeHeader = t.isOn;
    }
    public void turnOnpHCalc(Toggle t) {
        pHCalc = t.isOn;
    }
    public void turnOnRama(Toggle t) {
        rama = t.isOn;
    }
    public void turnOnPhiOnly(Toggle t) {
        phi_only = t.isOn;
    }
    public void turnOnPsiOnly(Toggle t) {
        psi_only = t.isOn;
    }
    public void turnOnResinter(Toggle t) {
        resinter = t.isOn;
    }
    public void turnOnResidComb(Toggle t) {
        ResidComb = t.isOn;
    }
    public void turnOnAllResidComb(Toggle t) {
        AllResidComb = t.isOn;
    }
    public void turnOnHBond(Toggle t) {
        hbond = t.isOn;
    }
    public void turnOnWhatIf(Toggle t) {
        whatif = t.isOn;
    }
    public void turnOnOldDisMeas(Toggle t) {
        oldDisMethod = t.isOn;
    }
    public void turnOnChi(Toggle t) {
        chi = t.isOn;
    }
    public void turnOnSummary(Toggle t) {
        summary = t.isOn;
    }
    public void turnOnContact(Toggle t) {
        contact = t.isOn;
    }
    public void turnOnNewResinter(Toggle t) {
        newresinter = t.isOn;
    }
    public void turnOnSalt(Toggle t) {
        salt = t.isOn;
    }
    public void turnOnpdb2pkaResume(Toggle t) {
        pdb2pka_resume = t.isOn;
    }
    public void turnOnpropka_verbose(Toggle t) {
        propka_verbose = t.isOn;
    }

    public void saveToPDB() {
        SaveTextFile.saveState();
    }

    public void pdb2pqr() {
        if (selM == null) {
            selM = UnityMolMain.getSelectionManager();
        }
        UnityMolSelection sel = selM.currentSelection;
        if (UnityMolMain.getStructureManager().loadedStructures.Count == 0) {
            Debug.LogWarning("No molecule loaded");
            return;
        }
        if (sel == null) {
            Debug.LogWarning("Using the last loaded molecule as input");
            sel = APIPython.last().ToSelection();
        }
        string cutCurSel = sel.structures[0].uniqueName;
        Debug.LogWarning("Starting pdb2pqr");
        pdb2pqr_count++;
        pdb2pqr_fromSelection(cutCurSel, pdb2pqr_count);
    }
}
}
