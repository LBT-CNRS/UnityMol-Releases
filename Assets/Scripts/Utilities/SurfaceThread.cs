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
using System.Runtime.InteropServices;
using System.Threading;

namespace UMol {
public class SurfaceThread {

	public bool _isEDTRunning;
	public bool _isMSMSRunning;
	private Thread _EDTthread;
	private Thread _MSMSthread;
	public UnityMolSelection sel;

	private string tempPath;

	public void StartThread() {
		tempPath = Application.temporaryCachePath;
		_EDTthread = new Thread(ComputeEDTSurface);
		_EDTthread.Start();
		_MSMSthread = new Thread(ComputeMSMSSurface);
		_MSMSthread.Start();
	}
	void ComputeMSMSSurface() {
		_isMSMSRunning = true;

		List<UnityMolSelection> subSels = ISurfaceRepresentation.cutSelection(sel);
		int idSub = 0;
		foreach (UnityMolSelection sub in subSels) {

			//Thread stopping from outside
			if (!_isMSMSRunning) {
				break;
			}

			//Don't compute the chain if already there
			string keyPrecomputedRep = sub.atoms[0].residue.chain.model.structure.name + "_" + sub.atoms[0].residue.chain.name + "_" + SurfMethod.MSMS.ToString();
			if (UnityMolMain.getPrecompRepManager().precomputedRep.ContainsKey(keyPrecomputedRep)) {
				continue;
			}

			//Default value from the MSMSWrapper
			float density = 15.0f;
			float probeRad = 1.4f;
			MeshData mdata = null;
			MSMSWrapper.createMSMSSurface(-1, sub, ref mdata, density, probeRad, tempPath);

			if (mdata != null) {
				UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep] = mdata;
                UnityMolMain.getPrecompRepManager().precomputedSurfAsso[keyPrecomputedRep] = mdata.atomByVert;
			}

			idSub++;
		}
		Debug.Log("(Thread) Done computing MSMS surface " + sel.name);
		_isMSMSRunning = false;
	}

	void ComputeEDTSurface() {
		_isEDTRunning = true;

		List<UnityMolSelection> subSels = ISurfaceRepresentation.cutSelection(sel);
		int idSub = 0;
		foreach (UnityMolSelection sub in subSels) {
			//Thread stopping from outside
			if (_isEDTRunning == false) {
				break;
			}

			//Don't compute the chain if already there
			string keyPrecomputedRep = sub.atoms[0].residue.chain.model.structure.name + "_" + sub.atoms[0].residue.chain.name + "_" + SurfMethod.EDTSurf.ToString();
			if (UnityMolMain.getPrecompRepManager().precomputedRep.ContainsKey(keyPrecomputedRep)) {
                idSub++;
				continue;
			}

			Vector3[] atomPos = new Vector3[sub.Count];
			int id = 0;
			foreach (UnityMolAtom a in sub.atoms) {
				atomPos[id++] = a.position;
			}

			string pdbLines = PDBReader.Write(sub, overridedPos: atomPos);


			if (pdbLines.Length == 0 || EDTSurfWrapper.EmptyAtomLines(pdbLines)) {
				//Try to write HET as Atoms
				pdbLines = PDBReader.Write(sub, writeModel: false, writeHET: true, forceHetAsAtom: true);
			}

            if (sub.CountHeavyAtoms() == 0) {
                Debug.LogError("Cannot create an EDTSurf surface for a selection containing no heavy atoms");
                idSub++;
                continue;
			}

			MeshData mdata = null;
			EDTSurfWrapper.callEDTSurf(ref mdata, sel.name + "_" + idSub.ToString(), pdbLines);
			int[] meshAtomVert = mdata.atomByVert;
			if (mdata != null) {
				UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep] = mdata;
				UnityMolMain.getPrecompRepManager().precomputedSurfAsso[keyPrecomputedRep] = meshAtomVert;

			}

			idSub++;
		}

		Debug.Log("(Thread) Done computing EDTSurf surface" + sel.name);
		_isEDTRunning = false;
	}
	public void Clear() {
		if (_isEDTRunning) {

			// Force thread to quit
			_isEDTRunning = false;

			// wait for thread to finish and clean up
			_EDTthread.Abort();
		}
		if (_isMSMSRunning) {

			// Force thread to quit
			_isMSMSRunning = false;

			// wait for thread to finish and clean up
			_MSMSthread.Abort();
		}
	}
}
}
