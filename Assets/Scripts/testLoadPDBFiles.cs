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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UMol.API;

namespace UMol {
public class testLoadPDBFiles : MonoBehaviour {

	public List<string> pdbIds = new List<string>();
	public bool usemmCIF = true;

	// Use this for initialization
	IEnumerator Start() {
		float start = Time.realtimeSinceStartup;
		UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();


		for (int i = 0; i < pdbIds.Count; i++) {
			UnityMolStructure newStruct = null;
			// try {
			// 	// string filePath = Application.dataPath + "/../samples/" + pdbIds[i].ToUpper();
			// 	// newStruct = UnityMolPDBParser.LoadPDB(filePath);
			// 	// newStruct = PDBHandler.ReadLocalFile(filePath);
			// } catch {
			// 	// newStruct = UnityMolPDBParser.FetchPDB(pdbIds[i].ToUpper());*
			// 	// StartCoroutine(PDBHandler.ReadRemoteFile(pdbIds[i].ToUpper(), value => newStruct = value));

			// }


			if (newStruct == null) {
				newStruct = APIPython.fetch(pdbIds[i].ToUpper(), usemmCIF);
				// if(usemmCIF) {
				// 	PDBxReader r = new PDBxReader();
				// 	yield return StartCoroutine(r.Fetch(pdbIds[i].ToUpper(), value => newStruct = value));
				// }
				// else {
				// 	PDBReader r = new PDBReader();
				// 	yield return StartCoroutine(r.Fetch(pdbIds[i].ToUpper(), value => newStruct = value));
				// }
			}
			if (newStruct != null) {

				// repManager.AddRepresentation(newStruct, AtomType.optihb, BondType.optihs);

				// // repManager.AddRepresentation(newStruct, AtomType.EDTSurface, BondType.nobond);

				// StrideWrapper.callStride(newStruct.currentModel);
				// repManager.AddRepresentation(newStruct, AtomType.cartoon, BondType.nobond);

				// Debug.Log("Number of models : "+newStruct.models.Count);
			}
			else {
				Debug.Log("Could not load pdb id " + pdbIds[i]);
			}
		}
		float stop = Time.realtimeSinceStartup;

		Debug.Log("Needed " + (stop - start) + " to parse and show pdbs");
		yield return null;

	}
}
}
