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
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Xml;
using System.Text;
using System.Linq;
using Unity.Mathematics;

namespace UMol {

public class TOPReader {

    public static UnityMolBonds readTopologyFromTOP(string path, UnityMolStructure s, string otherBondContent) {
        StreamReader sr;
        Dictionary<string, List<int2>> bondedIdsPerMoltype = new Dictionary<string, List<int2>>();
        Dictionary<string, List<int2>> otherbondedIdsPerMoltype = new Dictionary<string, List<int2>>();
        Dictionary<string, int> atomCountPerMoltype = new Dictionary<string, int>();
        List<molCount> moleculeCount = new List<molCount>();
        bool doOtherBonds = !string.IsNullOrEmpty(otherBondContent);

        if (Application.platform == RuntimePlatform.Android) {
            var textStream = new StringReaderStream(AndroidUtils.GetFileText(path));
            sr = new StreamReader(textStream);
        }
        else {
            sr = new StreamReader(path);
        }

        using (sr) {
            string line = "";
            string curMolType = "";
            bool readBonds = false;
            bool readAtoms = false;
            bool readMoltype = false;
            bool readMolecules = false;

            while ((line = sr.ReadLine()) != null) {
                if (line.Contains("[ moleculetype ]") || line.Contains("[moleculetype]")) {
                    readMoltype = true;
                    readBonds = false;
                    readMolecules = false;
                    readAtoms = false;
                    continue;
                }
                if (line.Contains("[ molecules ]") || line.Contains("[molecules]")) {
                    readMolecules = true;
                    readMoltype = false;
                    readBonds = false;
                    readAtoms = false;
                    continue;
                }
                if (line.Contains("[ atoms ]") || line.Contains("[atoms]")) {
                    readMolecules = false;
                    readMoltype = false;
                    readBonds = false;
                    readAtoms = true;
                    continue;
                }

                if (line.Contains("[ bonds ]") || line.Contains("[bonds]")) {
                    readMolecules = false;
                    readMoltype = false;
                    readBonds = true;
                    readAtoms = false;
                    continue;
                }

                if (readMoltype) {
                    if (line.Trim().Length == 0 || line.Trim().StartsWith(";")) {
                        continue;
                    }
                    curMolType = line.Split(new [] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries)[0];
                    bondedIdsPerMoltype[curMolType] = new List<int2>();
                    otherbondedIdsPerMoltype[curMolType] = new List<int2>();
                    atomCountPerMoltype[curMolType] = 0;
                    readMoltype = false;
                }

                if (readMolecules) {
                    if (line.Trim().Length == 0 || line.Trim().StartsWith(";")) {
                        continue;
                    }
                    if (line.Contains("[") && line.Contains("]")) {
                        readMolecules = false;
                        continue;
                    }
                    string[] molc = line.Split(new [] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                    molCount mc;
                    mc.molType = molc[0];
                    mc.count = int.Parse(molc[1]);
                    moleculeCount.Add(mc);
                }

                if (readAtoms) {
                    if (line.Trim().Length == 0 || line.Trim().StartsWith(";")) {
                        continue;
                    }
                    if (line.Contains("[") && line.Contains("]")) {
                        readAtoms = false;
                        continue;
                    }
                    atomCountPerMoltype[curMolType]++;
                }


                if (readBonds) {
                    if (line.Trim().Length == 0 || line.Trim().StartsWith(";")) {
                        continue;
                    }
                    if (line.Contains("[") && line.Contains("]")) {
                        readBonds = false;
                        continue;
                    }
                    if (doOtherBonds && line.Contains(otherBondContent)) {
                        string[] botherSplit = line.Split(new [] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                        int2 other1; other1.x = int.Parse(botherSplit[0]); other1.y = int.Parse(botherSplit[1]);

                        if (!otherbondedIdsPerMoltype.ContainsKey(curMolType)) {
                            otherbondedIdsPerMoltype[curMolType] = new List<int2>();
                        }
                        otherbondedIdsPerMoltype[curMolType].Add(other1);
                        continue;
                    }
                    string[] bSplit = line.Split(new [] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                    int2 b1; b1.x = int.Parse(bSplit[0]); b1.y = int.Parse(bSplit[1]);
                    if (!bondedIdsPerMoltype.ContainsKey(curMolType)) {
                        bondedIdsPerMoltype[curMolType] = new List<int2>();
                    }
                    bondedIdsPerMoltype[curMolType].Add(b1);

                }
            }
        }

        UnityMolBonds bonds = new UnityMolBonds();
        UnityMolBonds otherBonds = new UnityMolBonds();

        //First pass to compute max bond per atom
        Dictionary<UnityMolAtom, int> bondPerA = new Dictionary<UnityMolAtom, int>();
        int maxBondPerA = 0;
        int offset = 0;
        foreach (molCount mc in moleculeCount) {
            for (int i = 0; i < mc.count; i++) {
                if (!bondedIdsPerMoltype.ContainsKey(mc.molType)) {
                    Debug.LogError("Something went wrong reading the TOP file, the moleculetype '" + mc.molType + "' is not defined");
                    return null;
                }
                foreach (int2 b in bondedIdsPerMoltype[mc.molType]) {
                    UnityMolAtom a1 = s.currentModel.allAtoms[b.x - 1 + offset];
                    UnityMolAtom a2 = s.currentModel.allAtoms[b.y - 1 + offset];
                    if (!bondPerA.ContainsKey(a1)) bondPerA[a1] = 0;
                    if (!bondPerA.ContainsKey(a2)) bondPerA[a2] = 0;
                    bondPerA[a1]++;
                    bondPerA[a2]++;
                    maxBondPerA = Mathf.Max(bondPerA[a1], Mathf.Max(bondPerA[a2], maxBondPerA));


                }
                foreach (int2 b in otherbondedIdsPerMoltype[mc.molType]) {
                    UnityMolAtom a1 = s.currentModel.allAtoms[b.x - 1 + offset];
                    UnityMolAtom a2 = s.currentModel.allAtoms[b.y - 1 + offset];
                    if (!bondPerA.ContainsKey(a1)) bondPerA[a1] = 0;
                    if (!bondPerA.ContainsKey(a2)) bondPerA[a2] = 0;
                    bondPerA[a1]++;
                    bondPerA[a2]++;
                    maxBondPerA = Mathf.Max(bondPerA[a1], Mathf.Max(bondPerA[a2], maxBondPerA));
                }
                offset += atomCountPerMoltype[mc.molType];
            }
        }
        if(maxBondPerA > bonds.NBBONDS || maxBondPerA > otherBonds.NBBONDS){
            bonds.NBBONDS = maxBondPerA;
            otherBonds.NBBONDS = maxBondPerA;
        }

        offset = 0;
        foreach (molCount mc in moleculeCount) {
            for (int i = 0; i < mc.count; i++) {

                foreach (int2 b in bondedIdsPerMoltype[mc.molType]) {
                    UnityMolAtom a1 = s.currentModel.allAtoms[b.x - 1 + offset];
                    UnityMolAtom a2 = s.currentModel.allAtoms[b.y - 1 + offset];
                    if (!bonds.isBondedTo(a1, a2)) {
                        bonds.Add(a1, a2);
                    }
                }
                foreach (int2 b in otherbondedIdsPerMoltype[mc.molType]) {
                    UnityMolAtom a1 = s.currentModel.allAtoms[b.x - 1 + offset];
                    UnityMolAtom a2 = s.currentModel.allAtoms[b.y - 1 + offset];
                    if (!otherBonds.isBondedTo(a1, a2)) {
                        otherBonds.Add(a1, a2);
                    }
                }
                offset += atomCountPerMoltype[mc.molType];
            }
        }

        if (otherBonds.Count != 0) {

            UnityMolSelection sel = new UnityMolSelection(otherBonds.getAtomList(s.currentModel),
                    otherBonds, s.name + "_TOPOtherBonds");

            sel.canUpdateBonds = false;
            UnityMolMain.getSelectionManager().Add(sel);

            API.APIPython.showSelection(sel.name, "hbondtube", true);
        }

        return bonds;
    }

    struct molCount {
        public string molType;
        public int count;
    }
}
}