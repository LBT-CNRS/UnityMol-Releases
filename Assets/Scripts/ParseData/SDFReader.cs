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


// Unity Classes
using UnityEngine;

// C# Classes
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Net;

//Adapted from https://github.com/eharpste/molparser

namespace UMol {

/// <summary>
/// Creates a UnityMolStructure object from a local SDF file
/// </summary>
public class SDFReader: Reader {

    public static string[] SDFextensions = {"sdf", "mol"};


    // public static Dictionary<int, string> radicalDict = new Dictionary<int, string>() {
    //     {0, "no_radical"},
    //     {1, "singlet"},
    //     {2, "doublet"},
    //     {3, "triplet"}
    // };

    // public static Dictionary<int, string> chargeDict = new Dictionary<int, string>() {
    //     {0, "outside_limits"},
    //     {1, "+3"},
    //     {2, "+2"},
    //     {3, "+1"},
    //     {4, "doublet_radical"},
    //     {5, "-1"},
    //     {6, "-2"},
    //     {7, "-3"}
    // };

    // public static Dictionary<int, string> stereoParityDict = new Dictionary<int, string>() {
    //     {0, "not_stereo"},
    //     {1, "odd"},
    //     {2, "even"},
    //     {3, "unmarked"}
    // };

    // public static Dictionary<int, string> hCountDict = new Dictionary<int, string>() {
    //     {0, "H0"},
    //     {1, "H0"},
    //     {2, "H1"},
    //     {3, "H2"},
    //     {4, "H3"},
    //     {5, "H4"}
    // };

    // public static Dictionary<int, string> bondTypeDict = new Dictionary<int, string>() {
    //     {1, "Single"},
    //     {2, "Double"},
    //     {3, "Triple"},
    //     {4, "Aromatic"},
    //     {5, "Single_or_Double"},
    //     {6, "Single_or_Aromatic"},
    //     {7, "Double_or_Aromatic"},
    //     {8, "Any"}
    // };

    // public static Dictionary<int, string> singleBondStereoDict = new Dictionary<int, string>() {
    //     {0, "Not_stereo"},
    //     {1, "Up"},
    //     {4, "Either"},
    //     {6, "Down"}
    // };
    // public static Dictionary<int, string> doubleBondStereoDict = new Dictionary<int, string>() {
    //     {0, "Use_coordinates"},
    //     {3, "Cis_or_trans"}
    // };



    public SDFReader(string fileName = ""): base(fileName) {}


    /// <summary>
    /// Parses a SDF file file to a Molecule object
    /// </summary>
    protected override UnityMolStructure ReadData(StreamReader sr, bool readHET, bool readWater, bool simplyParse = false) {
        List<UnityMolModel> models = new List<UnityMolModel>();

        using (sr) { // Don't use garbage collection but free temp memory after reading the pdb file

            List<string> curLines = new List<string>();
            int count = 0;
            string line = "";
            while ((line = sr.ReadLine()) != null) {
                if (!line.StartsWith("$$$$")) {
                    curLines.Add(line);
                }
                else { //New compound => store it in a model
                    if (curLines.Count > 0) {

                        try {
                            models.Add(parseMol(curLines));
                        }
                        catch (Exception e) {
                            string message = "SDF Parser failed";
                            throw new ParsingException(message, e);
                        }
                        count++;
                        curLines.Clear();
                    }
                }

            }
        }
        UnityMolStructure newStruct = new UnityMolStructure(models, this.fileNameWithoutExtension);
        foreach (UnityMolModel m in models) {
            m.structure = newStruct;
            if (!simplyParse) {
                m.ComputeCenterOfGravity();
            }
        }

        if (simplyParse) {
            return newStruct;
        }
        UnityMolSelection sel = newStruct.ToSelection();
        
        if (newStruct.models.Count != 1) {
            for (int i = 1; i < newStruct.models.Count; i++) {
                CreateColliders(new UnityMolSelection(newStruct.models[i].allAtoms, newBonds: null, sel.name, newStruct.uniqueName));
            }
        }
        CreateColliders(sel);
        newStruct.surfThread = startSurfaceThread(sel);


        UnityMolMain.getStructureManager().AddStructure(newStruct);
        UnityMolMain.getSelectionManager().Add(sel);

        return newStruct;
    }

    private UnityMolModel parseMol(List<string> lines) {

        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();
        List<UnityMolResidue> residues = new List<UnityMolResidue>();
        List<UnityMolChain> chains = new List<UnityMolChain>();

        string name = lines[0];
        // string soft = lines[1].Trim();
        // string comment = "";
        // if (lines[2].Length > 0) {
        //     comment = lines[2].Trim();
        // }

        int nAtoms = 0;
        int nBonds = 0;
        int nLists = 0;

        int atomDex = 0;
        int bondDex = 0;

        parseCountsLine(lines[3], ref nAtoms, ref nBonds, ref nLists);

        int lcpt = 4;
        if (nAtoms == 0) {

            while (lcpt < lines.Count) {
                string line = lines[lcpt];
                if (line.Contains("END ATOM")) {
                    break;
                }
                try {

                    UnityMolAtom atom = parseAtomLinev2(line);
                    allAtoms.Add(atom);
                }
                catch {
                }
                lcpt++;
            }
        }
        else {

            atomDex = 4 + nAtoms;
            bondDex = atomDex + nBonds;

            //----------------- Parse Atoms

            for (int l = 4; l < atomDex; l++) {
                string line = lines[l];
                UnityMolAtom atom = parseAtomLine(line, l - 3);
                allAtoms.Add(atom);
            }
        }


        UnityMolResidue uniqueRes = new UnityMolResidue(0, allAtoms, "SDF");
        foreach (UnityMolAtom a in allAtoms) {
            a.SetResidue(uniqueRes);
        }

        residues.Add(uniqueRes);


        chains.Add(new UnityMolChain(residues, "A"));
        uniqueRes.chain = chains[0];

        UnityMolModel model = new UnityMolModel(chains, "0");
        chains[0].model = model;
        model.allAtoms.AddRange(allAtoms);
        model.fillIdAtoms();

        //-----------------Parse Bonds
        UnityMolBonds bonds = new UnityMolBonds();
        if (nAtoms == 0) {
            while (lcpt < lines.Count) {
                string line = lines[lcpt];
                if (line.Contains("END BOND")) {
                    break;
                }
                try {
                    parseBondLinev2(line, allAtoms, ref bonds);
                }
                catch {

                }
                lcpt++;
            }
        }
        else {

            for (int l = atomDex; l < bondDex; l++) {
                string line = lines[l];
                parseBondLine(line, allAtoms, ref bonds);
            }
        }

        model.bonds = bonds;


        return model;
    }

    //Parses the counts line of a molecule and returns it asd a dictionary
    // aaabbblllfffcccsssxxxrrrpppiiimmmvvvvvv
    // aaa = number of atoms (current max 255)*
    // bbb = number of bonds (current max 255)*
    // lll = number of atom lists (max 30)*
    // fff = (obsolete)
    // ccc = chiral flag: 0=not chiral, 1=chiral
    // sss = number of stext entries
    // xxx = (obsolete)
    // rrr = (obsolete)
    // ppp = (obsolete)
    // iii = (obsolete)
    // mmm = number of lines of additional properties,
    // vvvvv = version for the format
    private void parseCountsLine(string l, ref int nAtoms, ref int nBonds, ref int nLists) {
        nAtoms = (int)(float.Parse(l.Substring(0, 3), System.Globalization.CultureInfo.InvariantCulture));
        nBonds = (int)(float.Parse(l.Substring(3, 3), System.Globalization.CultureInfo.InvariantCulture));
        nLists = (int)(float.Parse(l.Substring(6, 3), System.Globalization.CultureInfo.InvariantCulture));
        // result["ccc"] = (int)(float.Parse(l.Substring(12,3)));
        // result["sss"] = (int)(float.Parse(l.Substring(15,3)));
        // result["mmm"] = (int)(float.Parse(l.Substring(18,3)));
        // result["vvvvv"] = line[-5:];
    }


    // Parses a line from the atom block and returns it as a dictionary
    // xxxxx.xxxxyyyyy.yyyyzzzzz.zzzz aaaddcccssshhhbbbvvvHHHrrriiimmmnnneee
    // [0:10] xxxxx.xxxx = x-coordinate
    // [10:20] yyyyy.yyyy = y-coordinate
    // [20:30] zzzzz.zzzz = z-coordinate
    // [31:34] aaa = atomic symbol
    // [34:36] dd = mass difference, i.e. difference from standard mass
    // [36:39] ccc = charge 0 = uncharged or value other than these, 1 = +3, 2 = +2, 3 = +1, 4 = doublet radical, 5 = -1, 6 = -2, 7 = -3
    // [39:42] sss = atom stereo parity 0 = not stereo, 1 = odd, 2 = even, 3 = either or unmarked stereo center
    // [42:45] hhh = INGORED hydrogen count +1
    // [45:48] bbb = IGNORED stereo care box
    // [48:51] vvv = valence
    // [51:54] HHH = IGNORED H0 designator
    // [54:57] rrr = Not used
    // [57:60] iii = Not used
    // [60:63] mmm = IGNORED atom-atom mapping number 1 - number of atoms
    // [63:66] nnn = IGNORED inversion/retention flag g 0 = property not applied 1 = configuration is inverted,2 = configuration is retained
    // [66:69] eee = IGNORED 0 = property not applied, 1 = change on atom must be exactly as shown

    private UnityMolAtom parseAtomLine(string line, int atomSerial) {
        //Unity uses a reversed x axis !
        float x = -float.Parse(line.Substring(0, 10), System.Globalization.CultureInfo.InvariantCulture);
        float y = float.Parse(line.Substring(10, 10), System.Globalization.CultureInfo.InvariantCulture);
        float z = float.Parse(line.Substring(20, 10), System.Globalization.CultureInfo.InvariantCulture);
        string atomType = line.Substring(31, 3).Trim();
        // int dd = int.Parse(line.Substring(34, 2));
        // int charge = int.Parse(line.Substring(36, 3));
        // int stereo = int.Parse(line.Substring(39, 3));
        // int valence = int.Parse(line.Substring(48, 3));

        Vector3 coord = new Vector3(x, y, z);
        float bfactor = 0.0f;

        // string chargeAtom = chargeDict[charge];
        // string stereoAtom = stereoParityDict[stereo];

        //Stored as hetatm
        UnityMolAtom newAtom = new UnityMolAtom(atomType + atomSerial, atomType, coord, bfactor, atomSerial, _isHET: true);

        return newAtom;
    }

    private UnityMolAtom parseAtomLinev2(string line) {
        string[] sp = line.Split(new [] { ' ', '\t'}, System.StringSplitOptions.RemoveEmptyEntries);

        //Unity uses a reversed x axis !
        float x = -float.Parse(sp[4], System.Globalization.CultureInfo.InvariantCulture);
        float y = float.Parse(sp[5], System.Globalization.CultureInfo.InvariantCulture);
        float z = float.Parse(sp[6], System.Globalization.CultureInfo.InvariantCulture);
        string atomType = sp[3].Trim();
        Vector3 coord = new Vector3(x, y, z);
        float bfactor = 0.0f;
        int atomSerial = int.Parse(sp[2]);

        //Stored as hetatm
        UnityMolAtom newAtom = new UnityMolAtom(atomType + atomSerial, atomType, coord, bfactor, atomSerial, _isHET: true);

        return newAtom;
    }

    // Parses a line from a bondblock and turns it into a dict
    // 111222tttsssxxxrrrccc
    // 111 = number of atom 1
    // 222 = number of atom 2
    // ttt = bond type
    // sss = bond stereo
    // xxx = not used
    // rrr = bond topology
    // ccc = reacting center status
    private void parseBondLine(string l, List<UnityMolAtom> atoms, ref UnityMolBonds bonds) {
        int idAtom1 = int.Parse(l.Substring(0, 3)) - 1; //Convert to base 0 instead of base 1
        int idAtom2 = int.Parse(l.Substring(3, 3)) - 1;
        bonds.Add(atoms[idAtom1], atoms[idAtom2]);

        // ret["ttt"] = int(float(line[6:9]))
        // ret["sss"] = int(float(line[9:12]))
        // ret["xxx"] = int(float(line[12:15]))
        // ret["rrr"] = int(float(line[15:18]))
        // ret["ccc"] = int(float(line[18:21]))
    }

    private void parseBondLinev2(string line, List<UnityMolAtom> atoms, ref UnityMolBonds bonds) {
        string[] sp = line.Split(new [] { ' ', '\t'}, System.StringSplitOptions.RemoveEmptyEntries);

        int idAtom1 = int.Parse(sp[4]) - 1; //Convert to base 0 instead of base 1
        int idAtom2 = int.Parse(sp[5]) - 1;
        bonds.Add(atoms[idAtom1], atoms[idAtom2]);
    }

    /// <summary>
    /// SDF writer
    /// Uses a selection
    /// Uses the molecule name of the first atom
    /// </summary>
    public static string Write(UnityMolSelection select, string structName = "") {

        List<UnityMolAtom> atoms = select.atoms;

        StringBuilder sw = new StringBuilder();

        //Header
        string name = structName;
        if (name == "") {
            name = select.atoms[0].residue.chain.model.structure.name;
        }
        sw.Append(name);
        sw.Append("\n");
        sw.Append("\tUnityMol version ");
        sw.Append(UnityMolMain.version);
        sw.Append("\n\n");

        //Header, connection table
        sw.Append(String.Format("{0,3}", select.atoms.Count));
        sw.Append(String.Format("{0,3}", select.bonds.Count));
        sw.Append("  0  0  1  0  0  0  0  0999 V200\n");


        //Atoms
        foreach (UnityMolAtom a in atoms) {
            sw.Append(String.Format("{0,10:N4}", (-1 * a.oriPosition.x)));
            sw.Append(String.Format("{0,10:N4}", a.oriPosition.y));
            sw.Append(String.Format("{0,10:N4}", a.oriPosition.z));
            sw.Append(String.Format("{0,2}", a.type));
            sw.Append("   0  0  0  0  0\n");
        }

        int idA = 1;
        //Bonds
        foreach (UnityMolAtom a in atoms) {
            try {
                foreach (UnityMolAtom b in select.bonds.bonds[a]) {
                    if (b != null) {
                        int idB = atoms.IndexOf(b) + 1;
                        sw.Append(String.Format("{0,3}", idA));
                        sw.Append(String.Format("{0,3}", idB));
                        sw.Append(String.Format("{0,3}", 8)); //Single = 1 / Double = 2 ... / 8 = Any
                        sw.Append("  0  0  0\n");
                    }
                }
            }
            catch {

            }
            idA++;
        }

        sw.Append("M  END\n$$$$\n");

        return sw.ToString();
    }

    /// <summary>
    /// Write SDF File
    /// Uses a structure and output a MOL file for all the models
    /// </summary>

    public static string Write(UnityMolStructure structure) {
        StringBuilder sw = new StringBuilder();

        foreach (UnityMolModel m in structure.models) {
            sw.Append(Write(m.ToSelection(), (structure.name + "_" + m.name)));
        }

        return sw.ToString();
    }
}
}