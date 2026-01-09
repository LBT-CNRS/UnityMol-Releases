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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace UMol {

/// <summary>
/// Wrapper to handle the CEAlign external library
/// </summary>
public class CEAlignWrapper {

	[DllImport("CEAlign")]
    private static extern IntPtr cealign(Vector3[] prot1, int lp1, Vector3[] prot2, int lp2,
	                                    out int resLen, out float resRMSD, float d0,
	                                    float d1, int windowSize, int gapMax);

    /// <summary>
    /// Handle & Manage the call to the CEAlign external function
    /// </summary>
    /// <param name="target">selection of target object</param>
    /// <param name="mobile">selection of mobile object</param>
	public static void alignWithCEAlign(UnityMolSelection target, UnityMolSelection mobile) {
		List<Vector3> tmp1 = new();
		List<Vector3> tmp2 = new();

		Transform totarget = UnityMolMain.getStructureManager().GetStructureGameObject(
		                         target.structures[0].name).transform;
		Transform torotate = UnityMolMain.getStructureManager().GetStructureGameObject(
		                         mobile.structures[0].name).transform;


		Transform savedPar = torotate.parent;
		torotate.parent = totarget;
		torotate.localRotation = Quaternion.identity;

		Vector3 invertX = new Vector3(-1.0f, 1.0f, 1.0f);
		foreach (UnityMolAtom a in target.atoms) {
			if (a.name == "CA" && a.type == "C") {
                tmp1.Add(Vector3.Scale(totarget.InverseTransformPoint(a.curWorldPosition), invertX));
            }
        }

        if (tmp1.Count == 0) {
            Debug.LogWarning("No C-alpha atoms found in " + target.name);
            Debug.LogWarning("Exiting CEAlign...");
            return;
        }
		foreach (UnityMolAtom a in mobile.atoms) {
			if (a.name == "CA" && a.type == "C") {
                tmp2.Add(Vector3.Scale(totarget.InverseTransformPoint(a.curWorldPosition), invertX));
            }
        }

        if (tmp2.Count == 0) {
            Debug.LogWarning("No C-alpha atoms found in " + mobile.name);
            Debug.LogWarning("Exiting CEAlign...");
            return;
        }

		float timer = Time.realtimeSinceStartup;

		float resRMSD = 0.0f;
		int resLen = 0;

		const float d0 = 3.0f;
		const float d1 = 4.0f;
		const int gapMax = 30;
		const int windowSize = 8;

        IntPtr ptrMatrixResult;

        try {
            ptrMatrixResult = cealign(tmp1.ToArray(), tmp1.Count, tmp2.ToArray(),
                tmp2.Count, out resLen, out resRMSD, d0, d1, windowSize, gapMax);
        } catch (DllNotFoundException) {
            Debug.LogError("CEAlign failed: Missing external library.");
            return;
        }

        float[] resultMatrix = new float[16];
		try {
			Marshal.Copy(ptrMatrixResult, resultMatrix, 0, 16);
			Marshal.FreeCoTaskMem(ptrMatrixResult);
		}
		catch {
			torotate.parent = savedPar;
			Debug.LogError("CEAlign failed");
            return;
        }
		Debug.Log("Time for cealign : " + (1000.0f * (Time.realtimeSinceStartup - timer)).ToString("f3") + " ms");

		Matrix4x4 m = new() {
            [0, 0] = resultMatrix[0],
            [0, 1] = resultMatrix[1],
            [0, 2] = resultMatrix[2],
            [0, 3] = resultMatrix[3],
            [1, 0] = resultMatrix[4],
            [1, 1] = resultMatrix[5],
            [1, 2] = resultMatrix[6],
            [1, 3] = resultMatrix[7],
            [2, 0] = resultMatrix[8],
            [2, 1] = resultMatrix[9],
            [2, 2] = resultMatrix[10],
            [2, 3] = resultMatrix[11],
            [3, 0] = resultMatrix[12],
            [3, 1] = resultMatrix[13],
            [3, 2] = resultMatrix[14],
            [3, 3] = resultMatrix[15]
        };


        Vector3 cog1_1 = target.structures[0].currentModel.centroid;
		Vector3 cog2_2 = mobile.structures[0].currentModel.centroid;


		Quaternion q = ExtractRotation(m);

		torotate.localRotation *= q;

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
