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
using System.Text;
using System;
using System.Threading;

namespace UMol {
public class CEAlignWrapper {

	[DllImport("CEAlign")]
	public static extern IntPtr cealign(Vector3[] prot1, int lp1, Vector3[] prot2, int lp2,
	                                    out int resLen, out float resRMSD, float d0,
	                                    float d1, int windowSize, int gapMax);

	public static void alignWithCEAlign(UnityMolSelection target, UnityMolSelection mobile) {
		List<Vector3> tmp1 = new List<Vector3>();
		List<Vector3> tmp2 = new List<Vector3>();

		Transform totarget = target.structures[0].getAtomGos()[0].transform.parent.parent;
		Transform torotate = mobile.structures[0].getAtomGos()[0].transform.parent.parent;

		Transform savedPar = torotate.parent;
		torotate.parent = totarget;
		torotate.localRotation = Quaternion.identity;

		Vector3 invertX = new Vector3(-1.0f, 1.0f, 1.0f);
		foreach (UnityMolAtom a in target.atoms) {
			if (a.name == "CA" && a.type == "C")
				tmp1.Add(Vector3.Scale(totarget.InverseTransformPoint(a.curWorldPosition), invertX));
		}
		foreach (UnityMolAtom a in mobile.atoms) {
			if (a.name == "CA" && a.type == "C")
				tmp2.Add(Vector3.Scale(totarget.InverseTransformPoint(a.curWorldPosition), invertX));
		}

		float timercea = Time.realtimeSinceStartup;

		float resRMSD = 0.0f;
		int resLen = 0;

		float d0 = 3.0f;
		float d1 = 4.0f;
		int gapMax = 30;
		int windowSize = 8;

		IntPtr PtrMatrixResult = cealign(tmp1.ToArray(), tmp1.Count, tmp2.ToArray(),
		                                 tmp2.Count, out resLen, out resRMSD, d0, d1, windowSize, gapMax);


		float[] resultMatrix = new float[16];
		try {
			Marshal.Copy(PtrMatrixResult, resultMatrix, 0, 16);
			Marshal.FreeCoTaskMem(PtrMatrixResult);
		}
		catch {
			torotate.parent = savedPar;
			throw new System.Exception("CEAlign failed");
		}
		Debug.Log("Time for cealign : " + (1000.0f * (Time.realtimeSinceStartup - timercea)).ToString("f3") + " ms");

		Matrix4x4 m = new Matrix4x4();
		m[0, 0] = resultMatrix[0];
		m[0, 1] = resultMatrix[1];
		m[0, 2] = resultMatrix[2];
		m[0, 3] = resultMatrix[3];

		m[1, 0] = resultMatrix[4];
		m[1, 1] = resultMatrix[5];
		m[1, 2] = resultMatrix[6];
		m[1, 3] = resultMatrix[7];

		m[2, 0] = resultMatrix[8];
		m[2, 1] = resultMatrix[9];
		m[2, 2] = resultMatrix[10];
		m[2, 3] = resultMatrix[11];

		m[3, 0] = resultMatrix[12];
		m[3, 1] = resultMatrix[13];
		m[3, 2] = resultMatrix[14];
		m[3, 3] = resultMatrix[15];


		Vector3 cog1_1 = target.structures[0].currentModel.centerOfGravity;
		Vector3 cog2_2 = mobile.structures[0].currentModel.centerOfGravity;

		// Vector3 cog1 = new Vector3(-m[3, 0], m[3, 1], m[3, 2]);
		// Vector3 cog2 = new Vector3(-m[0, 3], m[1, 3], m[2, 3]) * -1.0f;

		Quaternion q = ExtractRotation(m);

		torotate.localRotation = torotate.localRotation * q;

		Vector3 cog1W = totarget.TransformPoint(cog1_1);
		Vector3 cog2W = torotate.TransformPoint(cog2_2);

		Vector3 trans = cog1W - cog2W;

		torotate.Translate(trans, Space.World);
		torotate.parent = savedPar;

		Debug.Log("RMSD = " + resRMSD.ToString("F4"));
	}


	public static Quaternion ExtractRotation(Matrix4x4 matrix)
	{
		Vector4 c1 = matrix.GetColumn(2);
		c1.x = -c1.x;
		Vector4 c2 = matrix.GetColumn(1);
		c2.x = -c2.x;
		return Quaternion.LookRotation(c1, c2);

	}
}
}