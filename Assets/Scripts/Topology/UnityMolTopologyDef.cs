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


using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Xml;

namespace UMol {

public class UnityMolTopologyDef {
    public Dictionary<string, List<pairString>> bondedAtomsPerResidue;
    public Dictionary<UnityMolStructure.MolecularType, string> prefixMolType =
    new Dictionary<UnityMolStructure.MolecularType, string>() {
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
                    if(o == null){
                        pair.order = 1;
                    }
                    else{
                        pair.order = float.Parse(o);
                    }
                    result[curRes].Add(pair);
                }
            }
        }

        return result;
    }
    public List<UnityMolAtom> getBondedAtomsInResidue(UnityMolAtom curAtom) {
        string prefix = prefixMolType[curAtom.residue.chain.model.structure.structureType];

        string resName = prefix + curAtom.residue.name;

        List<pairString> bondedS;
        List<UnityMolAtom> result = new List<UnityMolAtom>();
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