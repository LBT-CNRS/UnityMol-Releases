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
/// Creates a UnityMolStructure object from a local XYZ file
/// Records several molecules in different models
/// </summary>
public class XYZReader: Reader {

    public static string[] XYZextensions = {"xyz"};


    public XYZReader(string fileName = ""): base(fileName) {}

    protected override UnityMolStructure ReadData(StreamReader sr, bool readHET, bool readWater, bool simplyParse = false) {
        List<UnityMolModel> models = new List<UnityMolModel>();
        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();
        List<UnityMolResidue> residues = new List<UnityMolResidue>();
        List<UnityMolChain> chains = new List<UnityMolChain>();


        int nbAtomsToParse = 0;
        int idAtom = 1;
        bool readAtomLine = false;
        bool commentLine = false;
        int curMol = 0;
        using (sr) { // Don't use garbage collection but free temp memory after reading the pdb file
            StringBuilder debug = new StringBuilder();

            string line = "";
            while ((line = sr.ReadLine()) != null) {
                int dummy = 0;
                if (line.Length > 0 && int.TryParse(line, out dummy)) { //New molecule
                    if (allAtoms.Count != 0) { //Record the current Model

                        UnityMolResidue uniqueRes = new UnityMolResidue(0, allAtoms, "XYZ");
                        foreach (UnityMolAtom a in allAtoms) {
                            a.SetResidue(uniqueRes);
                        }

                        residues.Add(uniqueRes);

                        UnityMolChain c = new UnityMolChain(residues, "A");
                        chains.Add(c);
                        uniqueRes.chain = c;

                        UnityMolModel model = new UnityMolModel(chains, curMol.ToString());
                        model.allAtoms.AddRange(allAtoms);

                        models.Add(model);
                        curMol++;
                        idAtom = 1;
                    }
                    nbAtomsToParse = dummy;
                    allAtoms.Clear();
                    residues.Clear();
                    chains.Clear();

                    readAtomLine = false;
                    commentLine = true;
                    continue;
                }
                if (commentLine) {
                    commentLine = false;
                    readAtomLine = true;
                    continue;
                }
                if (readAtomLine) {
                    if (line.Length == 0) {
                        continue;
                    }
                    if (idAtom == nbAtomsToParse + 1) {
                        debug.Append(String.Format("More atoms in the files than specified {0} / {1}\n", idAtom, nbAtomsToParse));
                        // Debug.LogWarning("More atoms in the files than specified "+idAtom+" / "+nbAtomsToParse);
                    }
                    allAtoms.Add(parseAtomLine(line, idAtom));
                    idAtom++;
                }
            }
            Debug.LogWarning(debug.ToString());
        }


        UnityMolResidue uRes = new UnityMolResidue(0, allAtoms, "XYZ");
        foreach (UnityMolAtom a in allAtoms) {
            a.SetResidue(uRes);
        }

        residues.Add(uRes);

        UnityMolChain newC = new UnityMolChain(residues, "A");
        chains.Add(newC);
        uRes.chain = newC;

        UnityMolModel lastModel = new UnityMolModel(chains, curMol.ToString());
        newC.model = lastModel;
        lastModel.allAtoms.AddRange(allAtoms);

        models.Add(lastModel);


        UnityMolStructure newStruct = new UnityMolStructure(models, this.fileNameWithoutExtension);
        identifyStructureMolecularType(newStruct);

        foreach (UnityMolModel m in models) {
            m.structure = newStruct;
            if (!simplyParse) {
                m.bonds = ComputeUnityMolBonds.ComputeBondsByResidue(m.allAtoms);
                m.ComputeCenterOfGravity();
                m.fillIdAtoms();

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

    private UnityMolAtom parseAtomLine(string line, int idAtom) {
        string[] splits = line.Split(new [] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

        int atomSerial = idAtom;
        string atomElement = splits[0];
        string atomName = atomElement + idAtom.ToString();

        float x = -1 * float.Parse(splits[1], System.Globalization.CultureInfo.InvariantCulture);
        float y = float.Parse(splits[2], System.Globalization.CultureInfo.InvariantCulture);
        float z = float.Parse(splits[3], System.Globalization.CultureInfo.InvariantCulture);
        Vector3 coord = new Vector3(x, y, z);

        float bfactor = 0.0f;

        //Stored as hetatm
        UnityMolAtom newAtom = new UnityMolAtom(atomName, atomElement, coord, bfactor, atomSerial, _isHET: true);
        return newAtom;
    }


    /// <summary>
    /// XYZ writer
    /// Uses a selection
    /// Uses the molecule name of the first atom
    /// </summary>
    public static string Write(UnityMolSelection select, string structName = "") {

        StringBuilder sw = new StringBuilder();

        sw.Append(select.atoms.Count);
        sw.Append("\n");
        sw.Append(select.atoms[0].residue.chain.model.structure.name);
        sw.Append("\n");

        //Atoms
        foreach (UnityMolAtom a in select.atoms) {
            sw.Append(String.Format("{0,3}", a.type));
            sw.Append(String.Format("{0,15:F5}", (-a.oriPosition.x)));
            sw.Append(String.Format("{0,15:F5}", (a.oriPosition.y)));
            sw.Append(String.Format("{0,15:F5}", (a.oriPosition.z)));
            sw.Append("\n");
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