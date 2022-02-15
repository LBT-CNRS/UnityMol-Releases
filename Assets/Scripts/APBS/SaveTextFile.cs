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


// Classes:
//   Fetch: requests/downloads a PDB file from RCSB.org
//   ReadData: parses a PDB string
//   Write writes a structure in PDB format to a string

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

namespace UMol
{
public class SaveTextFile : MonoBehaviour
{
    private static int savedState = 0;
    private static int multiValue = 0;

    static string getSavingFolder()
    {
        string directorypath = "";

        directorypath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "UnityMolAPBS");

        if (!System.IO.Directory.Exists(directorypath))
        {
            // Create the folder
            System.IO.Directory.CreateDirectory(directorypath);
        }

        return directorypath;
    }
    private static Vector3[] getColliderPositions(List<UnityMolStructure> structures) {
        List<Vector3> allColliderPos = new List<Vector3>();
        foreach (UnityMolStructure s in structures) {
            allColliderPos.AddRange(s.getColliderPositions());
        }
        return allColliderPos.ToArray();
    }

    /// Saves all the loaded molecules in a PDB file
    public static void saveState()
    {

        string pdbPath = getSavingFolder();

        string filename = "UnitymolAPBS_" + savedState.ToString() + ".pdb";
        string path = Path.Combine(pdbPath, filename);

        while (File.Exists(path))
        {
            savedState++;
            filename = "UnitymolAPBS" + savedState.ToString() + ".pdb";
            path = Path.Combine(pdbPath, filename);
        }

        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        Vector3[] positions = getColliderPositions(sm.loadedStructures);

        List<UnityMolAtom> structuresAtoms = sm.loadedStructures[0].currentModel.ToAtomList();

        for (int i = 1; i < sm.loadedStructures.Count; i++)
        {
            structuresAtoms.AddRange(sm.loadedStructures[i].currentModel.ToAtomList());
        }
        UnityMolSelection structuresSel = new UnityMolSelection(structuresAtoms, null, "APBSWrite");

        string pdbData = PDBReader.Write(structuresSel, overridedPos: positions);

        StreamWriter writer = new StreamWriter(path, true);

        writer.WriteLine("REMARK From UnityMol");
        // Write entire PDB file
        writer.Write(pdbData);
        writer.Close();

        Debug.Log("Saved APBS state to: '" + path + "'");
        savedState++;
    }

    public static Tuple<string, string> writeXYZ_fromSelection(string selName, int multiValue)
    {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

        if (!selM.selections.ContainsKey(selName))
        {
            return null;
        }
        UnityMolSelection sel = selM.selections[selName];
        UnityMolStructure s = sel.structures[0];//Get the UnityMolStructure of the selection

        //Sets directory
        string pdbPath = getSavingFolder();
        string mol = s.uniqueName;
        // string savePath = (pdbPath.Substring(0, pdbPath.Length - (mol.Length + 4)));
        // string directory = savePath;
        string filename = (mol + "-multiValue-" + multiValue.ToString() + ".txt");
        string path = Path.Combine(pdbPath, filename);
        try
        {
            while (File.Exists(path))
            {
                multiValue++;
                filename = (mol + "-multiValue-" + multiValue.ToString() + ".txt");
                path = Path.Combine(pdbPath, filename);
            }
        }
        catch
        {
            Debug.LogError("No file found");
            return null;

        }


        Vector3[] positions = new Vector3[sel.Count];
        int id = 0;
        foreach (UnityMolAtom a in sel.atoms)
        {
            positions[id++] = a.position;
        }
        string pdbData = PDBReader.Write(sel, overridedPos: positions);

        StreamWriter writer = new StreamWriter(path, true);
        // Write selected atoms of PDB file
        writer.Write(pdbData);
        writer.Close();

        string fixedFileName = (mol + "-multiValue-" + multiValue.ToString()); //filename without extension
        return fixMultiValue(path, fixedFileName, mol);

    }

    static Tuple<string, string> fixMultiValue(string path, string filename, string mol)
    {
        string writePath = System.IO.Path.ChangeExtension(path, null) + "-fixed.txt";
        string fixedFileName = (filename + "-fixed.txt");

        StreamReader sr = new StreamReader(path);
        StreamWriter writer = new StreamWriter(writePath, true);

        using (sr)
        {
            string line;

            try
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length > 46 + 8)
                    {
                        string sx = line.Substring(30, 8).Trim();
                        string sy = line.Substring(38, 8).Trim();
                        string sz = line.Substring(46, 8).Trim();
                        // Fix selected atoms of PDB file for multivalue
                        writer.WriteLine(sx + "," + sy + "," + sz);
                    }

                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Could not read the file " + path + " " + e);
                writer.Close();
                return null;
            }

            writer.Close();
        }

        return new Tuple<string, string>(writePath, fixedFileName);
        // PDB2PQR_APBS.multiValue(writePath, fixedFileName, mol);
    }

}
}
