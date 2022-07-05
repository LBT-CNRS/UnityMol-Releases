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



using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

namespace UMol {

public class APBS_Hbonds {
    //reads salt bridge file
    public static void SaltBridge_APBS(string strucName, string SBFP)
    {
        string saltBridgeFilePath = Path.Combine(SBFP, strucName + ".salt");

        if (string.IsNullOrEmpty(saltBridgeFilePath) || !System.IO.File.Exists(saltBridgeFilePath)) {
            Debug.LogWarning("Salt bridge file does not exist (" + saltBridgeFilePath + ")");
            return;
        }

        StreamReader sr = new StreamReader(saltBridgeFilePath);
        List<UnityMolAtom> atoms = new List<UnityMolAtom>();

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.GetStructure(strucName);
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        UnityMolBonds bonds = new UnityMolBonds();
        int id = 1;
        UnityMolSelection selBridge = new UnityMolSelection(new List<UnityMolAtom>(), "SaltBridges");

        using (sr)
        {
            string line;

            try
            {
                while ((line = sr.ReadLine()) != null)
                {
                    string[] splits = line.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

                    string bridgeA_resid = splits[3];// (line.Substring(14, 3)); //resnum
                    int bridgeA_residInt = int.Parse(bridgeA_resid); //resnum string to int
                    string bridgeA_AtomName = splits[4];// (line.Substring(17, 3)); //name of atom
                    string bridgeA_ChainName = splits[2];// (line.Substring(12, 1)); // chain name
                    //Debug.Log(bridgeA_ChainName + bridgeA_residInt + bridgeA_AtomName);

                    string bridgeB_resid = splits[8];// (line.Substring(33, 3));
                    int bridgeB_residInt = int.Parse(bridgeB_resid);
                    string bridgeB_AtomName = splits[9];// (line.Substring(36, 3));
                    string bridgeB_ChainName = splits[7];// (line.Substring(31, 1));
                    //Debug.Log(bridgeB_ChainName + bridgeB_residInt + bridgeB_AtomName);

                    UnityMolAtom aA = null;
                    UnityMolAtom aB = null;

                    if (!s.currentModel.chains.ContainsKey(bridgeA_ChainName)) {
                        bridgeA_ChainName = "_";//If the PDB file does not contain chain info, UnityMol uses "_" whereas APBS uses "A"
                    }
                    if (!s.currentModel.chains.ContainsKey(bridgeB_ChainName)) {
                        bridgeB_ChainName = "_";//If the PDB file does not contain chain info, UnityMol uses "_" whereas APBS uses "A"
                    }
                    try {
                        aA = s.currentModel.chains[bridgeA_ChainName].residues[bridgeA_residInt].atoms[bridgeA_AtomName];
                    }
                    catch {
                        Debug.LogWarning("Atom not found: chain " + bridgeA_ChainName + " / residue " + bridgeA_residInt + " / atom name " + bridgeA_AtomName);
                        continue;
                    }
                    try {
                        aB = s.currentModel.chains[bridgeB_ChainName].residues[bridgeB_residInt].atoms[bridgeB_AtomName];
                    }
                    catch {
                        Debug.LogWarning("Atom not found: chain " + bridgeB_ChainName + " / residue " + bridgeB_residInt + " / atom name " + bridgeB_AtomName);
                        continue;
                    }
                    atoms.Add(aA);
                    atoms.Add(aB);
                    bonds.Add(aA, aB);
                    //Debug.Log("added bond between " + aA.name + " and " + aB.name);

                    id++;
                }

            }
            catch (System.Exception e)
            {
                Debug.LogError("error with salt bridges " + e);
                return;
            }
        }
        if (atoms.Count != 0) {
            selBridge.bonds = bonds;
            selBridge.atoms = atoms;

            selM.Add(selBridge);
            API.APIPython.showSelection(selBridge.name, "hbond", true);

        }
    }


    //reads salt bridge file
    public static void HBOND_APBS(string strucName, string HBFP)
    {
        string hBondAPBSPath = HBFP + strucName + ".hbond";

        if (string.IsNullOrEmpty(hBondAPBSPath) || !System.IO.File.Exists(hBondAPBSPath)) {
            Debug.LogWarning("Hbond file does not exist (" + hBondAPBSPath + ")");
            return;
        }

        StreamReader sr = new StreamReader(hBondAPBSPath);
        List<UnityMolAtom> atoms = new List<UnityMolAtom>();
        UnityMolBonds bonds = new UnityMolBonds();
        UnityMolSelection HbondsAPBS = new UnityMolSelection(new List<UnityMolAtom>(), "Hbonds APBS");

        int id = 1;

        using (sr)
        {
            string line;

            try
            {
                while ((line = sr.ReadLine()) != null)
                {

                    string[] splits = line.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

                    string hbondA_resid = splits[3];// (line.Substring(14, 3)); //resnum
                    int hbondA_residInt = int.Parse(hbondA_resid); //resnum string to int
                    string hbondA_atomName = splits[4];// (line.Substring(17, 3)); //name of atom
                    string hbondA_chainName = splits[2];// (line.Substring(12, 1)); // chain name
                    //Debug.Log(hbondA_chainName + hbondA_residInt + hbondA_atomName);

                    string hbondB_resid = splits[8];// (line.Substring(33, 3));
                    int hbondB_residInt = int.Parse(hbondB_resid);
                    string hbondB_atomName = splits[9];// (line.Substring(36, 3));
                    string hbondB_chainName = splits[7];// (line.Substring(31, 1));
                    //Debug.Log(hbondB_chainName + hbondB_residInt + hbondB_atomName);

                    UnityMolStructureManager sm = UnityMolMain.getStructureManager();
                    UnityMolStructure s = sm.GetStructure(strucName);

                    UnityMolAtom HbondA = null;
                    UnityMolAtom HbondB = null;

                    if (!s.currentModel.chains.ContainsKey(hbondA_chainName)) {
                        hbondA_chainName = "_";//If the PDB file does not contain chain info, UnityMol uses "_" whereas APBS uses "A"
                    }
                    if (!s.currentModel.chains.ContainsKey(hbondB_chainName)) {
                        hbondB_chainName = "_";//If the PDB file does not contain chain info, UnityMol uses "_" whereas APBS uses "A"
                    }
                    try {
                        HbondA = s.currentModel.chains[hbondA_chainName].residues[hbondA_residInt].atoms[hbondA_atomName];
                    }
                    catch {
                        Debug.LogWarning("Atom not found: chain " + hbondA_chainName + " / residue " + hbondA_residInt + " / atom name " + hbondA_atomName);
                        continue;
                    }
                    try {
                        HbondB = s.currentModel.chains[hbondB_chainName].residues[hbondB_residInt].atoms[hbondB_atomName];
                    }
                    catch {
                        Debug.LogWarning("Atom not found: chain " + hbondB_chainName + " / residue " + hbondB_residInt + " / atom name " + hbondB_atomName);
                        continue;
                    }



                    atoms.Add(HbondA);
                    atoms.Add(HbondB);
                    bonds.Add(HbondA, HbondB);
                    //Debug.Log("added bond between " + HbondA.name + " and " + HbondB.name);

                    id++;
                }

            }
            catch (System.Exception e)
            {
                Debug.LogError("Error with hbonds APBS. " + e);
            }

        }
        if (atoms.Count != 0) {
            HbondsAPBS.atoms = atoms;
            HbondsAPBS.bonds = bonds;

            UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

            selM.Add(HbondsAPBS);
            API.APIPython.showSelection(HbondsAPBS.name, "hbond", true);
        }
        else {
            Debug.LogWarning("No hbond read from the file " + hBondAPBSPath);
        }


    }
}
}