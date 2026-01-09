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
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace UMol {

/// <summary>
/// Creates a UnityMolStructure object from a local XYZ file
/// Records several molecules in different models
/// </summary>
public class XYZReader: Reader {

    /// <summary>
    /// List of XYZ file extensions
    /// </summary>
    public static string[] XYZextensions = {"xyz"};


    /// <summary>
    /// Create a XYZReader with an optional filename
    /// </summary>
    /// <param name="fileName">the filename</param>
    public XYZReader(string fileName = ""): base(fileName) {}


    /// <summary>
    /// Parse a XYZ file contained in a StreamReader to a UnityMolStructure object
    /// </summary>
    protected override UnityMolStructure ReadData(StreamReader sr, bool readHET, bool readWater,
            bool simplyParse = false, UnityMolStructure.MolecularType? forceStructureType = null) {
        List<UnityMolModel> models = new List<UnityMolModel>();
        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();
        List<UnityMolResidue> residues = new List<UnityMolResidue>();
        List<UnityMolChain> chains = new List<UnityMolChain>();


        int nbAtomsToParse = 0;
        int idAtom = 1;
        bool readAtomLine = false;
        bool commentLine = false;
        int curMol = 0;
        using (sr) { // Don't use garbage collection but free temp memory after reading the xyz file
            StringBuilder debug = new StringBuilder();

            string line = "";
            while ((line = sr.ReadLine()) != null) {
                int dummy = 0;
                if (line.Length > 0 && int.TryParse(line, out dummy)) { //New molecule
                    if (allAtoms.Count != 0) { //Record the current Model

                        UnityMolResidue uniqueRes = new UnityMolResidue(0, 0, allAtoms, "XYZ");
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
                        debug.AppendFormat("More atoms in the files than specified {0} / {1}\n", idAtom, nbAtomsToParse);
                        // Debug.LogWarning("More atoms in the files than specified "+idAtom+" / "+nbAtomsToParse);
                    }
                    allAtoms.Add(parseAtomLine(line, idAtom));
                    idAtom++;
                }
            }
            Debug.LogWarning(debug.ToString());
        }


        UnityMolResidue uRes = new UnityMolResidue(0, 0, allAtoms, "XYZ");
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


        UnityMolStructure newStruct = new UnityMolStructure(models, this.FileNameWithoutExtension);

        if (forceStructureType.HasValue) {
            newStruct.structureType = forceStructureType.Value;
        }
        else {
            newStruct.SetStructureMolecularType();
        }

        foreach (UnityMolModel m in models) {
            m.structure = newStruct;
            if (!simplyParse) {
                m.fillIdAtoms();
                m.bonds = ComputeUnityMolBonds.ComputeBondsByResidue(m.allAtoms);
                m.ComputeCentroid();

            }
        }

        if (simplyParse) {
            return newStruct;
        }

        UnityMolSelection sel = newStruct.ToSelection();

        if (newStruct.models.Count != 1) {
            for (int i = 1; i < newStruct.models.Count; i++) {
                CreateUnityObjects(newStruct.ToSelectionName(), new UnityMolSelection(newStruct.models[i].allAtoms, newBonds: null, sel.name, newStruct.name));
            }
        }
        CreateUnityObjects(newStruct.ToSelectionName(), sel);
        newStruct.surfThread = StartSurfaceThread(sel);

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
    /// XYZ writer using a UnityMolSelection.
    /// if comment is empty use the name of the structure of the first atome
    /// Returns a string containing all the lines
    /// </summary>
    /// <param name="select">the selection</param>
    /// <param name="comment">string for the comment line</param>
    /// <returns>string holding all the lines</returns>
    public static string Write(UnityMolSelection select, string comment = "")
    {
        StringBuilder sw = new StringBuilder();

        if (string.IsNullOrEmpty(comment))
        {
            comment = select.atoms[0].residue.chain.model.structure.name;
        }

        sw.Append(select.atoms.Count);
        sw.Append("\n");
        sw.Append(comment);
        sw.Append("\n");

        //Atoms
        foreach (UnityMolAtom a in select.atoms) {
            sw.AppendFormat("{0,3}", a.type);
            sw.AppendFormat("{0,15:F5}", -a.oriPosition.x);
            sw.AppendFormat("{0,15:F5}", a.oriPosition.y);
            sw.AppendFormat("{0,15:F5}", a.oriPosition.z);
            sw.Append("\n");
        }

        return sw.ToString();

    }

    /// <summary>
    /// XYZ writer using a UnityMolStructure.
    /// Returns a string containing all the lines
    /// </summary>
    /// <param name="structure">the structure</param>
    /// <returns>string holding all the lines</returns>
    public static string Write(UnityMolStructure structure) {
        StringBuilder sw = new StringBuilder();

        foreach (UnityMolModel m in structure.models) {
            sw.Append(Write(m.ToSelection(), structure.name + "_" + m.name));
        }

        return sw.ToString();
    }
}
}
