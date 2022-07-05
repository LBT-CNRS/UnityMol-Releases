/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

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
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using VRTK;

namespace UMol {

public class ParsingException: Exception {
    public ParsingException() {}
    public ParsingException(string message)
    : base(message) {}
    public ParsingException(string message, Exception inner)
    : base(message, inner) {}
}



public abstract class Reader {


    ///Read PDB/mmCIF/GRO frames as trajectory
    public bool modelsAsTraj = true;

    protected string fileName;
    protected string fileNameWithoutExtension;

    public static int limitBigMolecule = 5000;

    public struct secStruct {
        public string chain;
        public int start;
        public int end;
        public UnityMolResidue.secondaryStructureType type;
    }


    public Reader() {}

    public Reader(string fileName) {

        this.fileName = fileName;
        updateFileNames();

    }

    public void updateFileNames() {
        //Get the filename without extensions and without the path
        if ( fileName != "") {

            FileInfo f = new FileInfo(fileName);

            this.fileNameWithoutExtension = f.Name.Substring(0, f.Name.IndexOf("."));
            if (this.fileNameWithoutExtension == "") {
                this.fileNameWithoutExtension = f.Name;
            }
        }
        else {
            this.fileNameWithoutExtension = "";
        }
    }

    /// <summary>
    /// Reads a file from local HDD and parses the data
    /// </summary>
    public UnityMolStructure Read(bool readHet = true, bool readWater = true, bool justParse = false) {
        UnityMolStructure structure = null;

        StreamReader sr;
        //Detect compressed files
        if ( fileName.ToLower().EndsWith("gz") ) {
            GZipStream flatStream = new GZipStream(File.OpenRead(fileName), CompressionMode.Decompress);
            sr = new StreamReader(flatStream);
        }
        else
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                Stream textStream;
                textStream = new StringReaderStream(AndroidUtils.GetFileText(fileName));
                sr = new StreamReader(textStream);
            }
            else
            {
                FileInfo LocalFile = new FileInfo(fileName);
                if (!LocalFile.Exists)
                {
                    throw new FileNotFoundException("File not found: " + fileName);
                }
                sr = new StreamReader(fileName);
            }
        }

        using(sr) {
            try {
                structure = ReadData(sr, readHet, readWater, justParse);
            }
            catch (Exception err) {
                Debug.LogError("Something went wrong when parsing your file: " + err);
                throw err;
            }
        }

        return structure;
    }

    /// <summary>
    /// Reads a file from string and parses the data
    /// </summary>
    public UnityMolStructure ReadFromString(string content, bool readHet = true, bool readWater = true) {
        UnityMolStructure structure = null;

        byte[] bytes = Encoding.UTF8.GetBytes(content);
        MemoryStream ms = new MemoryStream(bytes);
        StreamReader sr = new StreamReader(ms, System.Text.Encoding.UTF8, true);

        using(sr) {
            try {
                structure = ReadData(sr, readHet, readWater);
            }
            catch (Exception err) {
                Debug.LogError("Something went wrong when parsing your file: " + err);
                throw err;
            }
        }

        return structure;
    }

    /// <summary>
    /// Fills secondary structure types for each residue of each model
    /// </summary>
    public static void FillSecondaryStructure(UnityMolStructure structure, List<secStruct> secStructsList) {
        StringBuilder sb = new StringBuilder();

        //Set everythig to coil
        foreach (UnityMolModel model in structure.models) {
            foreach (UnityMolChain c in model.chains.Values) {
                foreach (UnityMolResidue r in c.residues.Values) {
                    r.secondaryStructure = UnityMolResidue.secondaryStructureType.Coil;
                }
            }
        }

        //Use the parsed secondary structure list to fill ss types
        foreach (secStruct ss in secStructsList) {
            foreach (UnityMolModel model in structure.models) {
                try {
                    UnityMolChain c = model.chains[ss.chain];
                    foreach (UnityMolResidue r in c.residues.Values) {
                        if (r.id >= ss.start && r.id <= ss.end) {
                            r.secondaryStructure = ss.type;
                        }
                    }
                }
                catch {
                    sb.Append("Secondary Structure parsing : No chain ");
                    sb.Append(ss.chain);
                    sb.Append(" parsed in the PDB\n");
                    break;
                }
            }
        }
        if (sb.Length != 0) {
            Debug.LogWarning(sb.ToString());
        }

        if (secStructsList.Count != 0) {
            structure.ssInfoFromFile = true;
        }
    }

    /// <summary>
    /// Creates a gameobject for each atom of the selection
    /// </summary>
    public static void CreateColliders(UnityMolSelection sel, bool useFullAtomName = false) {
        List<SphereCollider> colliders = new List<SphereCollider>();

        GameObject loadedMolGO = UnityMolMain.getRepresentationParent();

        Transform repParent = loadedMolGO.transform.Find(sel.name);
        if (UnityMolMain.inVR() && repParent == null) {

            try {
                Transform clref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
                Transform crref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
                if (clref != null) {
                    repParent = clref.Find(sel.name);
                }
                if (repParent == null && crref != null) {
                    repParent = crref.Find(sel.name);
                }
            }
            catch { //VRTK didn't start so ignore this error

            }
        }

        if (repParent == null) {
            repParent = (new GameObject(sel.name).transform);
            repParent.parent = loadedMolGO.transform;
            repParent.localPosition = Vector3.zero;
            repParent.localRotation = Quaternion.identity;
            repParent.localScale = Vector3.one;
        }
        Transform collidersT = repParent.Find("Colliders");
        if (collidersT == null) {
            collidersT = new GameObject("Colliders").transform;
        }
        collidersT.parent = repParent;

        Dictionary<UnityMolAtom, GameObject> atomToGo = new Dictionary<UnityMolAtom, GameObject>();
        foreach (UnityMolAtom a in sel.atoms) {

            GameObject curA = null;
            if (useFullAtomName)
                curA = new GameObject(a.ToString());
            else
                curA = new GameObject("Atom");

            curA.transform.parent = collidersT.transform;
            curA.transform.localPosition = a.position;
            curA.transform.localScale = Vector3.one;
            atomToGo[a] = curA;
        }
        if (sel.structures[0].atomToGo == null || sel.structures[0].atomToGo.Count == 0) {
            sel.structures[0].atomToGo = atomToGo;
        }
        else { //Merge
            foreach (UnityMolAtom a in atomToGo.Keys) {
                sel.structures[0].atomToGo[a] = atomToGo[a];
            }
        }

        collidersT.transform.localPosition = Vector3.zero;
        collidersT.transform.localRotation = Quaternion.identity;
        collidersT.transform.localScale = Vector3.one;

    }
    public static SurfaceThread startSurfaceThread(UnityMolSelection sel) {
        Transform t = sel.structures[0].currentModel.allAtoms[0].correspondingGo.transform.parent;
        SurfaceThread sf = new SurfaceThread();
        sf.sel = sel;
        sf.StartThread();
        return sf;
    }



    /// <summary>
    /// Fills the structureType field in the UnityMolStructure class based on atom names, uses the 5000 first atoms
    /// </summary>
    public static void identifyStructureMolecularType(UnityMolStructure s) {

        //WARNING Not published yet
        return;

        // int count = 0;
        // const int limitTest = 5000;
        // foreach (UnityMolAtom a in s.currentModel.allAtoms) {


        //     if (a.name == "BB" || a.name == "BB1" || a.name == "SC1" || //Martini 2.2P
        //             a.name == "BAS" || a.name == "SID" || a.name == "SI1" || //Martini 2.2 & 2.1
        //             a.name == "DC" || a.name == "DG" ) { //Martini DNA
        //         s.structureType = UnityMolStructure.MolecularType.Martini;
        //         break;
        //     }

        //     string martiniAtomName = "Martini_" + a.residue.name + "_" + a.name;
        //     if (UnityMolMain.atomColors.isKnownAtom(martiniAtomName.ToUpper())) {
        //         s.structureType = UnityMolStructure.MolecularType.Martini;
        //         break;
        //     }

        //     if (a.name == "C5*" || a.name == "O5*" || a.name == "G1" || a.name == "G2" || a.name == "A1" || a.name == "A2") {
        //         s.structureType = UnityMolStructure.MolecularType.HIRERNA;
        //         break;
        //     }

        //     if (MDAnalysisSelection.isProtein(a) && a.name == a.residue.name) { //OPEP
        //         s.structureType = UnityMolStructure.MolecularType.OPEP;
        //         break;
        //     }

        //     if(count >= limitTest){
        //         break;
        //     }

        //     //Check non-protein Martini ?
        //     // if(!MDAnalysisSelection.isProtein(a) && ()){//Martini 2.2 & 2.1
        //     //     s.structureType = UnityMolStructure.MolecularType.Martini;
        //     //     break;
        //     // }
        //     count++;
        // }
        // Debug.Log("Molecule type identified : " + s.structureType);
    }


    /// <summary>
    /// Check if the string full contains at the begining, the string comp
    /// </summary>
    protected static bool QuickStartWith(string full, string comp) {
        // return comp == full.Substring(0, Mathf.Min(comp.Length, full.Length));
        return full.StartsWith(comp, StringComparison.Ordinal);
    }


    //Methods which needs to be implemented in child classes :
    protected abstract UnityMolStructure ReadData(StreamReader sr, bool readHet = true, bool readWater = true, bool simplyParse = false);

    // Return a Reader according to either the filename extension or the format argument given.
    public static Reader GuessReaderFrom(string filename, string format = "") {

        string type = "";

        //Parse the filename extension and obtain a type for the switch
        if (format != "") {
            type = format.ToLower();
        }
        else {

            if (PDBReader.PDBextensions.Any( x => filename.ToLower().EndsWith(x))) {
                type = "pdb";
            }
            else if (PDBxReader.PDBxextensions.Any( x => filename.ToLower().EndsWith(x))) {
                type = "cif";
            }
            else if (GROReader.GROextensions.Any( x => filename.ToLower().EndsWith(x))) {
                type = "gro";
            }
            else if (SDFReader.SDFextensions.Any( x => filename.ToLower().EndsWith(x))) {
                type = "sdf";
            }
            else if (MOL2Reader.MOL2extensions.Any( x => filename.ToLower().EndsWith(x))) {
                type = "mol2";
            }
            else if (XYZReader.XYZextensions.Any( x => filename.ToLower().EndsWith(x))) {
                type = "xyz";
            }
            else {
                type = Path.GetExtension(filename).ToLower();
            }
        }


        switch (type) {
        case "pdb":
            return new PDBReader(filename);
        case "cif":
            return new PDBxReader(filename);
        case "gro":
            return new GROReader(filename);
        case "sdf":
            return new SDFReader(filename);
        case "mol2":
            return new MOL2Reader(filename);
        case "xyz":
            return new XYZReader(filename);
        default:
            Debug.LogWarning("The file extension '" + type + "' is not supported");
            break;
        }

        return null;
    }

    protected static string findNewAtomName(HashSet<string> residueAtoms, string name) {
        int toAdd = 2;

        Regex reg = new Regex(@"_[0-9]*$");
        Match match = reg.Match(name);

        if (match.Success) {
            name = name.Substring(0, match.Index);
        }
        string result = name + "_" + toAdd.ToString();
        while (residueAtoms.Contains(result)) {
            toAdd++;
            result = name + "_" + toAdd.ToString();
        }
        return result;
    }
}
}
