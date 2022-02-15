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

namespace UMol {

/// <summary>
/// Creates a UnityMolStructure object from a local Mol2 file
/// Format file described here http://chemyang.ccnu.edu.cn/ccb/server/AIMMS/mol2.pdf
/// </summary>
public class MOL2Reader: Reader {

    public static string[] MOL2extensions = {"mol2"};


    public MOL2Reader(string fileName = ""): base(fileName) {}

    protected override UnityMolStructure ReadData(StreamReader sr, bool readHET, bool readWater, bool simplyParse = false) {
        List<UnityMolModel> models = new List<UnityMolModel>();
        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();
        List<UnityMolResidue> residues = new List<UnityMolResidue>();
        List<UnityMolChain> chains = new List<UnityMolChain>();


        UnityMolBonds bonds = new UnityMolBonds();
        StringBuilder debug = new StringBuilder();


        bool readAtomLine = false;
        bool readBondLine = false;
        using (sr) { // Don't use garbage collection but free temp memory after reading the pdb file

            string line = "";
            while ((line = sr.ReadLine()) != null) {
                if (line.Length == 0) {
                    continue;
                }
                if (line.StartsWith("@<TRIPOS>ATOM")) {
                    readAtomLine = true;
                    readBondLine = false;
                    continue;
                }
                if (line.StartsWith("@<TRIPOS>BOND")) {
                    readBondLine = true;
                    readAtomLine = false;
                    continue;
                }
                if (line.StartsWith("@<TRIPOS>")) {
                    readBondLine = false;
                    readAtomLine = false;
                }

                if (readAtomLine) {
                    try {
                        allAtoms.Add(parseAtomLine(line));
                    }
                    catch {
                        debug.Append(String.Format("Could not parse line {0}\n", line));
                        // Debug.LogError("Could not parse line '"+line+"'");
                    }
                }

                if (readBondLine) {
                    try {
                        parseBondLine(line, allAtoms, ref bonds);
                    }

                    catch {
                        debug.Append(String.Format("Could not parse line {0}\n", line));
                        // Debug.LogError("Could not parse line '"+line+"'");
                    }
                }
            }
        }
        if (debug.Length > 0)
            Debug.LogError(debug.ToString());


        UnityMolResidue uniqueRes = new UnityMolResidue(0, allAtoms, "MOL2");
        foreach (UnityMolAtom a in allAtoms) {
            a.SetResidue(uniqueRes);
        }

        residues.Add(uniqueRes);


        chains.Add(new UnityMolChain(residues, "A"));
        uniqueRes.chain = chains[0];

        UnityMolModel model = new UnityMolModel(chains, "0");
        chains[0].model = model;
        model.allAtoms.AddRange(allAtoms);

        model.bonds = bonds;
        models.Add(model);

        UnityMolStructure newStruct = new UnityMolStructure(models, this.fileNameWithoutExtension);
        foreach (UnityMolModel m in models) {
            m.structure = newStruct;
            if (!simplyParse) {
                m.ComputeCenterOfGravity();
                m.fillIdAtoms();
            }
        }

        if (!simplyParse) {
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
        }

        return newStruct;
    }

    private UnityMolAtom parseAtomLine(string line) {
        string[] splits = line.Split(new [] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

        int atomSerial = int.Parse(splits[0]);
        string atomName = splits[1];
        string atomElement = splits[5].Split(new [] {'.'}, System.StringSplitOptions.RemoveEmptyEntries)[0];

        float x = -1 * float.Parse(splits[2], System.Globalization.CultureInfo.InvariantCulture);
        float y = float.Parse(splits[3], System.Globalization.CultureInfo.InvariantCulture);
        float z = float.Parse(splits[4], System.Globalization.CultureInfo.InvariantCulture);
        Vector3 coord = new Vector3(x, y, z);

        float bfactor = 0.0f;

        //Stored as hetatm
        UnityMolAtom newAtom = new UnityMolAtom(atomName, atomElement, coord, bfactor, atomSerial, _isHET: true);
        return newAtom;
    }

    private void parseBondLine(string line, List<UnityMolAtom> atoms, ref UnityMolBonds bonds) {
        string[] splits = line.Split(new [] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
        int idAtom1 = int.Parse(splits[1]) - 1;
        int idAtom2 = int.Parse(splits[2]) - 1;
        bonds.Add(atoms[idAtom1], atoms[idAtom2]);
    }



    /// <summary>
    /// Mol2 writer
    /// Uses a selection
    /// Uses the molecule name of the first atom
    /// </summary>
    public static string Write(UnityMolSelection select, string structName = "") {

        StringBuilder sw = new StringBuilder();

        sw.Append("@<TRIPOS>MOLECULE\n");
        string name = structName;
        if (name == "") {
            name = select.atoms[0].residue.chain.model.structure.name;
        }
        sw.Append(name);
        sw.Append("\n@<TRIPOS>ATOM\n");

        //Atoms
        foreach (UnityMolAtom a in select.atoms) {
            sw.Append(a.number);
            sw.Append(" ");
            sw.Append(a.name);
            sw.Append(" ");
            sw.Append((-a.oriPosition.x).ToString("F4"));
            sw.Append(" ");
            sw.Append(a.oriPosition.y.ToString("F4"));
            sw.Append(" ");
            sw.Append(a.oriPosition.z.ToString("F4"));
            sw.Append(" ");
            sw.Append(a.type);
            sw.Append(" 1 <1> 0.0000\n");
        }
        sw.Append("@<TRIPOS>BOND\n");


        int idA = 1;
        int countB = 1;
        //Bonds
        foreach (UnityMolAtom a in select.atoms) {
            try {
                foreach (UnityMolAtom b in select.bonds.bonds[a]) {
                    if (b != null) {
                        int idB = select.atoms.IndexOf(b) + 1;
                        sw.Append(countB);
                        sw.Append(" ");
                        sw.Append(idA);
                        sw.Append(" ");
                        sw.Append(idB);
                        sw.Append(" un\n");//Unknown bond
                        countB++;
                    }
                }
            }
            catch {

            }
            idA++;
        }


        return sw.ToString();

    }

    public static string Write(UnityMolStructure structure) {
        StringBuilder sw = new StringBuilder();

        foreach (UnityMolModel m in structure.models) {
            sw.Append(Write(m.ToSelection(), (structure.name + "_" + m.name)));
        }

        return sw.ToString();
    }
}
}