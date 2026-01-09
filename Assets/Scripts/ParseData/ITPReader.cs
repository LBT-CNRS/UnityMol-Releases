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
//
// New classes to handle ITP format
// Xavier Martinez
//

// Unity Classes
using UnityEngine;
using Unity.Mathematics;

// C# Classes
using System.Collections.Generic;
using System.IO;
using System.Linq;


/// <summary>
/// Only reads ITP files and add them to a dictionary, does NOT create a UnityMolStructure
/// </summary>
namespace UMol {

public static class ITPReader {

    public static Dictionary<string, UnityMolResidue.secondaryStructureType> martiniSSToSSType =
    new Dictionary<string, UnityMolResidue.secondaryStructureType> {
        {"F", UnityMolResidue.secondaryStructureType.Coil},
        {"E", UnityMolResidue.secondaryStructureType.Strand},
        {"H", UnityMolResidue.secondaryStructureType.Helix},
        {"1", UnityMolResidue.secondaryStructureType.Helix},
        {"2", UnityMolResidue.secondaryStructureType.Helix},
        {"3", UnityMolResidue.secondaryStructureType.Helix},
        {"T", UnityMolResidue.secondaryStructureType.Coil},
        {"S", UnityMolResidue.secondaryStructureType.Coil},
        {"C", UnityMolResidue.secondaryStructureType.Coil},

        {"EXT", UnityMolResidue.secondaryStructureType.Strand},
        {"HLX", UnityMolResidue.secondaryStructureType.Helix},
        {"TRN", UnityMolResidue.secondaryStructureType.Coil},
        {"BND", UnityMolResidue.secondaryStructureType.Coil},
        {"COI", UnityMolResidue.secondaryStructureType.Coil}
    };

    //Read all the itp files from the StreamingAssets folder
    public static Dictionary< string, Dictionary<string, CGAtomITP>> InitITPDict() {

        var res = new Dictionary< string, Dictionary<string, CGAtomITP>>();
        // static string martiniITPLipid = Path.Combine(Application.streamingAssetsPath , "martini_v2.0_lipids_all_201506.itp");
        // static string martiniITPDNA = Path.Combine(Application.streamingAssetsPath , "martini_custom_dna.itp");
        // static string martiniITPAA = Path.Combine(Application.streamingAssetsPath , "martini_v2.2_aminoacids.itp");

        if (Application.platform == RuntimePlatform.Android) {
            string path = Path.Combine(Application.streamingAssetsPath, "martini_custom_dna.itp");
            loadITPFile(path, res);
            path = Path.Combine(Application.streamingAssetsPath, "martini_v2.0_lipids_all_201506.itp");
            loadITPFile(path, res);
            path = Path.Combine(Application.streamingAssetsPath, "martini_v2.2_aminoacids.itp");
            loadITPFile(path, res);
        }
        else {
            DirectoryInfo d = new DirectoryInfo(Application.streamingAssetsPath);
            var Files = d.GetFiles("*.itp").Where(f => f.Extension == ".itp");
            foreach (var file in Files)
            {
                string path = Path.Combine(Application.streamingAssetsPath, file.Name);
                loadITPFile(path, res);
            }
        }

        return res;

    }

    public static void loadITPFile(string path, Dictionary< string, Dictionary<string, CGAtomITP>> dicITP) {

        StreamReader sr;

        if (Application.platform == RuntimePlatform.Android) {
            var textStream = new StringReaderStream(AndroidUtils.GetFileText(path));
            sr = new StreamReader(textStream);
        }
        else {
            sr = new StreamReader(path);
        }

        string[] lines = sr.ReadToEnd().Split(new [] { "\r", "\n", System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
        sr.Close();
        sr.Dispose();

        bool isMoleculeType = false;
        bool isAtoms = false;
        bool isBonds = false;
        bool isElasticBonds = false;
        char[] splitters = { '\t', ' ' };
        string curCGMolName = null;
        Dictionary<int, string> idToStr = new Dictionary<int, string>();


        for (int i = 0; i < lines.Length; i++) {
            if (lines[i].Trim().Length > 0 && !lines[i].StartsWith("#")) {
                if (lines[i].StartsWith(";")) {
                    if (isElasticBonds) {
                        isElasticBonds = false;
                    }
                    if (lines[i].Contains("elastic bonds")) {
                        isElasticBonds = true;
                    }
                    continue;
                }


                bool containsmolecule = lines[i].Contains("moleculetype");
                bool containsatoms = lines[i].Contains("atoms");
                bool containsbonds = lines[i].Contains("bonds");
                bool containsbrakets = lines[i].Contains("[") && lines[i].Contains("]");

                containsbonds |= lines[i].Contains("constraints");

                if (containsmolecule) {
                    isAtoms = false;
                    isBonds = false;
                    isMoleculeType = true;
                    isElasticBonds = false;
                    continue;
                }


                if (containsbonds) {
                    isMoleculeType = false;
                    isBonds = true;
                    isAtoms = false;
                    isElasticBonds = false;
                    continue;
                }

                if (containsatoms) {
                    isBonds = false;
                    isAtoms = true;
                    isMoleculeType = false;
                    isElasticBonds = false;
                    continue;
                }

                if (containsbrakets) {
                    isAtoms = false;
                    isBonds = false;
                    isMoleculeType = false;
                    isElasticBonds = false;
                    continue;//Ignore line with other information
                }



                if (isMoleculeType) {
                    string[] split = lines[i].Split(splitters, System.StringSplitOptions.RemoveEmptyEntries);
                    curCGMolName = split[0];

                    // if(UIData.file_extension!="gro" && curCGMolName.Length > 4){
                    //     curCGMolName = curCGMolName.Substring(0,4);
                    // }
                    dicITP[curCGMolName] = new Dictionary<string, CGAtomITP>();
                    isMoleculeType = false;
                    idToStr.Clear();
                }

                if (isBonds) {

                    string s = lines[i].Trim();
                    string[] split = s.Split(splitters, System.StringSplitOptions.RemoveEmptyEntries);
                    int idAtom1 = int.Parse(split[0]);
                    int idAtom2 = int.Parse(split[1]);

                    string nameAtom1 = idToStr[idAtom1];
                    string nameAtom2 = idToStr[idAtom2];


                    CGAtomITP curCGAtom ;
                    if (dicITP[curCGMolName].TryGetValue(nameAtom1, out curCGAtom)) {
                        curCGAtom.bonds.Add(nameAtom2);
                        curCGAtom.elasticBonds.Add(isElasticBonds);
                    }
                    if (dicITP[curCGMolName].TryGetValue(nameAtom2, out curCGAtom)) {
                        curCGAtom.bonds.Add(nameAtom1);
                        curCGAtom.elasticBonds.Add(isElasticBonds);
                    }
                }


                if (isAtoms) {
                    string s = lines[i].Trim();

                    string[] split = s.Split(splitters, System.StringSplitOptions.RemoveEmptyEntries);

                    CGAtomITP newCG;
                    newCG.id = int.Parse(split[0]);
                    newCG.resName = split[3];
                    newCG.resID = int.Parse(split[2]);
                    newCG.name = split[4];
                    newCG.bonds = new List<string>();
                    newCG.elasticBonds = new List<bool>();
                    idToStr[newCG.id] = newCG.name;
                    dicITP[curCGMolName][newCG.name] = newCG;
                }
            }
        }
        Debug.Log("Loaded ITP file '" + path + "'");
    }

    public static void loadSystemITPFile(string path, UnityMolStructure stru) {
        StreamReader sr;
        sr = new StreamReader(path);
        string[] lines = sr.ReadToEnd().Split(new [] { "\r", "\n", System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);

        char[] splitters = { '\t', ' '};
        List<UnityMolResidue.secondaryStructureType> ssList = new List<UnityMolResidue.secondaryStructureType>();
        List<int2> elasticBonds = new List<int2>();
        bool isAtoms = false;
        bool isElasticBonds = false;
        bool isMoleculeType = false;

        string chainName = "";

        foreach (string s in lines) {
            if (s.Trim().Length > 0 && !s.StartsWith("#") && !s.StartsWith(";")) {

                bool containsbrakets = s.Contains("[") && s.Contains("]");
                bool containsmolecule = containsbrakets && s.Contains("moleculetype");
                bool containsatoms = containsbrakets && s.Contains("atoms");
                bool containsbonds = containsbrakets && s.Contains("bonds");

                if (containsbonds) {
                    isAtoms = false;
                    isMoleculeType = false;
                    isElasticBonds = true;
                    continue;
                }
                if (containsmolecule) {
                    isMoleculeType = true;
                    isAtoms = false;
                    isElasticBonds = false;
                    continue;
                }
                if (containsatoms) {
                    isAtoms = true;
                    isMoleculeType = false;
                    isElasticBonds = false;
                    continue;
                }

                if (containsbrakets) {
                    isAtoms = false;
                    isMoleculeType = false;
                    isElasticBonds = false;
                    continue;
                }

                if (isMoleculeType) {

                    string[] split = s.Split(splitters, System.StringSplitOptions.RemoveEmptyEntries);

                    chainName = split[0].Split(new char[] {'_'}, System.StringSplitOptions.RemoveEmptyEntries).Last();
                    isMoleculeType = false;
                }


                if (isAtoms && s.Length > 2) { //Get secondary structure
                    string ss = s.Substring(s.Length - 2).Trim();
                    UnityMolResidue.secondaryStructureType sstype = UnityMolResidue.secondaryStructureType.Coil;
                    if (martiniSSToSSType.ContainsKey(ss)) {
                        sstype = martiniSSToSSType[ss];
                    }
                    else { //Try with 3 letters code
                        string[] split = s.Split(splitters, System.StringSplitOptions.RemoveEmptyEntries);
                        ss = split.Last().Trim();
                        if (martiniSSToSSType.ContainsKey(ss)) {
                            sstype = martiniSSToSSType[ss];
                        }
                    }

                    ssList.Add(sstype);
                }

                if (isElasticBonds) { //Get elastic bonds
                    // if (s.Contains("RUBBER_FC*")) { //Get elastic bonds

                    string[] tok = s.Trim().Split(new [] { ' ', '\t'}, System.StringSplitOptions.RemoveEmptyEntries);
                    int id1 = int.Parse(tok[0]);
                    int id2 = int.Parse(tok[1]);
                    int2 duo;
                    duo.x = id1;
                    duo.y = id2;
                    elasticBonds.Add(duo);


                }
            }
        }
        if (!stru.currentModel.chains.ContainsKey(chainName)) {
            chainName = stru.currentModel.chains.Keys.First();
        }
        UnityMolChain c = stru.currentModel.chains[chainName];
        List<UnityMolAtom> chainAtoms = c.AllAtoms;

        if (ssList.Count <= chainAtoms.Count) {

            int i = 0;
            foreach (UnityMolAtom a in chainAtoms) {
                a.residue.secondaryStructure = ssList[i++];
                if (i >= ssList.Count) {
                    break;
                }
            }
            Debug.Log("Read secondary structure information from the ITP file");
            UnityMolMain.getPrecompRepManager().Clear(stru.name);
        }

        if (elasticBonds.Count != 0) {
            UnityMolBonds elaBonds = new UnityMolBonds();
            elaBonds.NBBONDS = 128;

            foreach (int2 duo in elasticBonds) {
                try {
                    UnityMolResidue r1 = stru.currentModel.chains[chainName].residues[duo.x];
                    UnityMolAtom a1 = null;
                    if (r1.atoms.ContainsKey("BB"))
                        a1 = r1.atoms["BB"];
                    else if (r1.atoms.ContainsKey("BB1"))
                        a1 = r1.atoms["BB1"];

                    UnityMolResidue r2 = stru.currentModel.chains[chainName].residues[duo.y];
                    UnityMolAtom a2 = null;
                    if (r2.atoms.ContainsKey("BB"))
                        a2 = r2.atoms["BB"];
                    else if (r2.atoms.ContainsKey("BB1"))
                        a2 = r2.atoms["BB1"];

                    if (a1 != null && a2 != null)
                        elaBonds.Add(a1, a2);
                }
                catch {

                }

            }

            Debug.Log("Read " + elaBonds.Count + " elastic bonds");

            UnityMolSelection sel = new UnityMolSelection(stru.currentModel.allAtoms,
                    elaBonds, stru.name + "_MartiniElasticBonds");

            sel.canUpdateBonds = false;
            UnityMolMain.getSelectionManager().Add(sel);

            API.APIPython.showSelection(sel.name, "hbondtube", true);

        }


    }
}

public struct CGAtomITP {
    public string resName;
    public int resID;
    public int id;
    public string name;
    public List<string> bonds;
    public List<bool> elasticBonds;
}
}