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
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UMol {

/// <summary>
/// Wrapper to handle the Stride external library
/// </summary>
public static class StrideWrapper {

	[DllImport ("StrideLib")]
    private static extern IntPtr  runStride(string pdbContent);

	[DllImport ("StrideLib")]
	private static extern IntPtr  runStridePath(string pdbPath);

    /// <summary>
    /// Handle & Manage the call to the Stride external function
    /// </summary>
    /// <param name="model">the UnityMol model to compute Stride secondary structures</param>
	public static void callStride(UnityMolModel model){
		List<UnityMolAtom> atoms = model.allAtoms;
		Debug.Log("Calling stride with "+atoms.Count+" atoms");
		string filecontent = PDBReader.Write(model.ToSelection(), writeHET: false);

        string ssString;
        try {
            ssString = Marshal.PtrToStringAnsi(runStride(filecontent));
        } catch (DllNotFoundException) {
            Debug.LogError("Stride failed: Missing external library.");
            return;
        }

        if(string.IsNullOrEmpty(ssString) || ssString.StartsWith("Error")){
			Debug.LogWarning("Stride failed");
			return;
		}

		Dictionary<UnityMolResidue,bool> doneResi = new();
		foreach(UnityMolAtom a in atoms){
			doneResi[a.residue] = false;
		}

		List<UnityMolResidue> allResi = doneResi.Keys.ToList();

		string[] lines = ssString.Split(new [] {"\r", "\n", Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
		foreach (string line in lines)
        {
            bool isHelixLine = line.StartsWith ("HELIX");
            bool isSheetLine = line.StartsWith("SHEET");
            bool isAccLine = line.StartsWith("ACC");
            bool isDonLine = line.StartsWith("DNR");


            if(isHelixLine){
                string chainh = line.Substring (19,2).Trim ();
                string initr = line.Substring(22,4);
                string termr = line.Substring (34,4);
                string classH = line.Substring(39,2);

                int initres = int.Parse (initr);
                int termres = int.Parse (termr);

                int classhelix;
                try{
                    classhelix = int.Parse (classH);
                }catch{
                    classhelix = int.Parse (line.Substring(38,2));
                }

                foreach(UnityMolResidue r in allResi){
                    bool done = doneResi[r];

                    if(!done){//Residue not already set
                        if(r.chain.name == chainh){
                            if(r.id >= initres && r.id <= termres){
                                r.secondaryStructure = (UnityMolResidue.secondaryStructureType)classhelix;
                                doneResi[r] = true;
                            }
                        }
                    }
                }
            }

            if(isSheetLine){
                string chainS = line.Substring (21, 2).Trim ();
                string initr = line.Substring (23, 4);
                string termr = line.Substring (34, 4);
                int initres = int.Parse (initr);
                int termres = int.Parse (termr);

                foreach(UnityMolResidue r in allResi){
                    bool done = doneResi[r];
                    if(!done){//Residue not already set
                        if(r.chain.name == chainS){
                            if(r.id >= initres && r.id <= termres){
                                r.secondaryStructure = UnityMolResidue.secondaryStructureType.Strand;
                                doneResi[r] = true;
                            }
                        }
                    }
                }
            }
        }
        Debug.Log("Stride finished.");
	}
}
}
