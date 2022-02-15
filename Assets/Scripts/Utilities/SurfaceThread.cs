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
			if (_isMSMSRunning == false) {
				break;
			}

			//Don't compute the chain if already there
			string keyPrecomputedRep = sub.atoms[0].residue.chain.model.structure.uniqueName + "_" + sub.atoms[0].residue.chain.name + "_" + SurfMethod.MSMS.ToString();
			if (UnityMolMain.getPrecompRepManager().precomputedRep.ContainsKey(keyPrecomputedRep)) {
				continue;
			}

			//Default value from the MSMSWrapper
			float density = 15.0f;
			float probeRad = 1.4f;
			int[] meshAtomVert = null;

			MeshData tmp = MSMSWrapper.callMSMS(sub, density, probeRad, tempPath, ref meshAtomVert);
			if (tmp != null) {

				UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep] = tmp;
				UnityMolMain.getPrecompRepManager().precomputedSurfAsso[keyPrecomputedRep] = meshAtomVert;

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
			string keyPrecomputedRep = sub.atoms[0].residue.chain.model.structure.uniqueName + "_" + sub.atoms[0].residue.chain.name + "_" + SurfMethod.EDTSurf.ToString();
			if (UnityMolMain.getPrecompRepManager().precomputedRep.ContainsKey(keyPrecomputedRep)) {
				continue;
			}

			Vector3[] atomPos = new Vector3[sub.Count];
			int id = 0;
			foreach (UnityMolAtom a in sub.atoms) {
				atomPos[id++] = a.position;
			}

			string pdbLines = PDBReader.Write(sub, overridedPos: atomPos);


			if (pdbLines.Length == 0 || EDTSurfWrapper.countAtomInPDBLines(pdbLines) == 0) {
				//Try to write HET as Atoms
				pdbLines = PDBReader.Write(sub, writeModel: false, writeHET: true, forceHetAsAtom: true);
			}
			int[] meshAtomVert = null;
			MeshData tmp = EDTSurfWrapper.callEDTSurf(sel.name + "_" + idSub.ToString(), pdbLines, ref meshAtomVert);
			if (tmp != null) {
				tmp.colors = new Color32[tmp.vertices.Length];
				Color32 w = Color.white;
				for (int i = 0; i < tmp.colors.Length; i++) {
					tmp.colors[i] = w;
				}
				UnityMolMain.getPrecompRepManager().precomputedRep[keyPrecomputedRep] = tmp;
				UnityMolMain.getPrecompRepManager().precomputedSurfAsso[keyPrecomputedRep] = meshAtomVert;

			}

			idSub++;
		}

		Debug.Log("(Thread) Done computing EDTSurf surface"+sel.name);
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

