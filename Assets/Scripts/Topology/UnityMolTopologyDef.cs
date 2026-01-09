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

namespace UMol {

public class UnityMolTopologyDef {
    public Dictionary<string, List<pairString>> bondedAtomsPerResidue;
    public Dictionary<UnityMolStructure.MolecularType, string> prefixMolType =
    new Dictionary<UnityMolStructure.MolecularType, string>() {
        {UnityMolStructure.MolecularType.Martini, "Martini_"},
        {UnityMolStructure.MolecularType.HIRERNA, "HIRERNA_"},
        {UnityMolStructure.MolecularType.OPEP, ""},
        {UnityMolStructure.MolecularType.standard, ""}
    };

    public UnityMolTopologyDef(string path = null) {
        if (path == null) {
            path = Path.Combine(Application.streamingAssetsPath , "residues.xml");
        }
        bondedAtomsPerResidue = parseOpenMMTopologyFile(path);
    }

    public Dictionary<string, List<pairString>> parseOpenMMTopologyFile(string path) {
        Dictionary<string, List<pairString>> result = new Dictionary<string, List<pairString>>();

        // Debug.Log("Reading OpenMM topology file: '"+path+"'");
        StreamReader sr;

        if (Application.platform == RuntimePlatform.Android)
        {
            var textStream = new StringReaderStream(AndroidUtils.GetFileText(path));
            sr = new StreamReader(textStream);
        }
        else
        {
            sr = new StreamReader(path);
        }

        using (sr)
        {

            string curRes = "";
            XmlTextReader xmlR = new XmlTextReader(sr);
            while (xmlR.Read()) {
                if (xmlR.Name == "Residue") {
                    string tmpA = xmlR.GetAttribute("name");
                    if (tmpA != null && tmpA != "") {
                        curRes = tmpA;
                        result[curRes] = new List<pairString>();
                    }

                }
                if (xmlR.Name == "Bond") {

                    pairString pair;
                    pair.s1 = xmlR.GetAttribute("from");
                    // pair.s1 = xmlR.GetAttribute("from").Replace("-","");
                    pair.s2 = xmlR.GetAttribute("to");
                    string o = xmlR.GetAttribute("order");
                    if (o == null) {
                        pair.order = 1;
                    }
                    else {
                        pair.order = float.Parse(o);
                    }
                    result[curRes].Add(pair);
                }
            }
        }

        return result;
    }
    public List<UnityMolAtom> getBondedAtomsInResidue(UnityMolAtom curAtom, ref List<UnityMolAtom> result) {
        string prefix = prefixMolType[curAtom.residue.chain.model.structure.structureType];
        result.Clear();
        
        string resName = null;
        if (!string.IsNullOrEmpty(prefix)) {
            resName = prefix + curAtom.residue.name;
        }
        else {
            resName = curAtom.residue.name;
        }


        List<pairString> bondedS;

        if (bondedAtomsPerResidue.TryGetValue(resName, out bondedS)) {
            foreach (pairString s in bondedS) {

                if (s.s1 == curAtom.name) {
                    if (curAtom.residue.atoms.ContainsKey(s.s1) && curAtom.residue.atoms.ContainsKey(s.s2)) {
                        result.Add(curAtom.residue.atoms[s.s1]);
                        result.Add(curAtom.residue.atoms[s.s2]);
                    }
                }
                if (s.s2 == curAtom.name) {
                    if (curAtom.residue.atoms.ContainsKey(s.s1) && curAtom.residue.atoms.ContainsKey(s.s2)) {
                        result.Add(curAtom.residue.atoms[s.s2]);
                        result.Add(curAtom.residue.atoms[s.s1]);
                    }
                }
            }
        }

        return result;
    }
    public List<UnityMolAtom> getBondedAtomsInResidue(UnityMolAtom curAtom, out List<float> bondOrders) {
        string prefix = prefixMolType[curAtom.residue.chain.model.structure.structureType];

        string resName = prefix + curAtom.residue.name;
        bondOrders = new List<float>();

        List<pairString> bondedS = new List<pairString>();
        List<UnityMolAtom> result = new List<UnityMolAtom>();
        if (bondedAtomsPerResidue.TryGetValue(resName, out bondedS)) {
            foreach (pairString s in bondedS) {

                if (s.s1 == curAtom.name) {
                    if (curAtom.residue.atoms.ContainsKey(s.s1) && curAtom.residue.atoms.ContainsKey(s.s2)) {
                        result.Add(curAtom.residue.atoms[s.s1]);
                        result.Add(curAtom.residue.atoms[s.s2]);
                        bondOrders.Add(s.order);
                        bondOrders.Add(s.order);
                    }
                }
                if (s.s2 == curAtom.name) {
                    if (curAtom.residue.atoms.ContainsKey(s.s1) && curAtom.residue.atoms.ContainsKey(s.s2)) {
                        result.Add(curAtom.residue.atoms[s.s2]);
                        result.Add(curAtom.residue.atoms[s.s1]);
                        bondOrders.Add(s.order);
                        bondOrders.Add(s.order);
                    }
                }
            }
        }

        return result;
    }

    public pairString getPreviousAtomToLink(UnityMolAtom curAtom) {
        string prefix = prefixMolType[curAtom.residue.chain.model.structure.structureType];

        string resName = prefix + curAtom.residue.name;
        List<pairString> bondedS;
        pairString result; result.s1 = null; result.s2 = null; result.order = 0;
        if (bondedAtomsPerResidue.TryGetValue(resName, out bondedS)) {
            foreach (pairString s in bondedS) {
                if (s.s1.StartsWith("-")) {
                    return s;
                }
            }
        }
        return result;
    }

}

public struct pairString {
    public string s1;
    public string s2;
    public float order;
}
}