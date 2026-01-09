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
using System.Runtime.InteropServices;
using System.Text;
using System;

namespace UMol {

/// <summary>
/// Wrapper to handle the EDTSurf external library
/// </summary>
public class EDTSurfWrapper {

	[DllImport("EDTSurfLib")]
    private static extern IntPtr getEDTSurfLibrary();

	[DllImport("EDTSurfLib")]
    private static extern void ComputeSurfaceMesh(IntPtr instance, string name, string lines, out int vertnumber, out int facenumber);

	[DllImport("EDTSurfLib")]
    private static extern IntPtr getVertices(IntPtr instance);

	[DllImport("EDTSurfLib")]
    private static extern IntPtr getColors(IntPtr instance);

	[DllImport("EDTSurfLib")]
    private static extern IntPtr getTriangles(IntPtr instance);

	[DllImport("EDTSurfLib")]
    private static extern IntPtr getAtomVert(IntPtr instance);

	[DllImport("EDTSurfLib")]
	private static extern void freeMeshData(IntPtr instance);

	[ThreadStatic] //Ensure thread safe
	private static StringBuilder sbEDT;

    /// <summary>
    /// Handle & Manage the call to the EDTSurf external function
    /// </summary>
    /// <param name="idF">the Frame ID</param>
    /// <param name="name">name of the structure</param>
    /// <param name="select">the selection to compute the surface on</param>
    /// <param name="mData"> the MeshData generated from the computation</param>
    public static void createEDTSurface(int idF, string name, UnityMolSelection select, ref MeshData mData) {
		if (select.atoms.Count < 10) {
			Debug.LogError("Cannot create an EDTSurf surface for a selection containing less than 10 atoms");
			return;
		}

        if (select.CountHeavyAtoms() == 0) {
            Debug.LogError("Cannot create an EDTSurf surface for a selection containing no heavy atoms");
            return;
        }

		Vector3[] atomPos;
		if (idF != -1) {
			atomPos = select.extractTrajFramePositions[idF];
		}
		else {
			atomPos = new Vector3[select.atoms.Count];
			int id = 0;
			foreach (UnityMolAtom a in select.atoms) {
				atomPos[id++] = a.position;
			}
		}

		sbEDT ??= new StringBuilder();

        string pdbLines = PDBReader.Write(select, overridedPos: atomPos, sw: sbEDT);

		if (pdbLines.Length == 0 || EmptyAtomLines(pdbLines)) {
			//Try to write HET as Atoms
			pdbLines = PDBReader.Write(select, writeModel: false, writeHET: true, forceHetAsAtom: true, sw: sbEDT);
		}

		callEDTSurf(ref mData, name, pdbLines);

	}



	/// <summary>
	/// Calls native plugin EDTSurf to create meshes
	/// </summary>
	/// <param name="mData">the MeshData generated from the computation</param>
	/// <param name="pdbName">the name of the structure</param>
	/// <param name="pdbLines">the string containing the structure in PDB format.</param>
	public static void callEDTSurf(ref MeshData mData, string pdbName, string pdbLines) {

		if (pdbLines.Length == 0 || EmptyAtomLines(pdbLines)) {
			Debug.LogWarning("No atoms for surface");
			return;
		}

		int vertNumber;
		int faceNumber;

        IntPtr EDTSurfObj;
		IntPtr IntArrayPtrVertices;
		IntPtr IntArrayPtrColors;
		IntPtr IntArrayPtrTriangles;
		IntPtr IntArrayPtrAtomVert;


        try {
            EDTSurfObj = getEDTSurfLibrary();
            if (EDTSurfObj == IntPtr.Zero) {
                Debug.LogError("Something went wrong when initializing EDTSurf library");
                return;
            }
        } catch (DllNotFoundException) {
            Debug.LogError("EDTSurf failed: Missing external library.");
            return;
        }

        try {
            ComputeSurfaceMesh(EDTSurfObj, pdbName, pdbLines, out vertNumber, out faceNumber);
        } catch (Exception) {
            Debug.LogError("EDTSurf failed");
            return;
        }

		IntArrayPtrVertices = getVertices(EDTSurfObj);
		IntArrayPtrColors = getColors(EDTSurfObj);
		IntArrayPtrTriangles = getTriangles(EDTSurfObj);
		IntArrayPtrAtomVert = getAtomVert(EDTSurfObj);

		if (mData == null) {
			mData = new MeshData {
                triangles = new int[faceNumber * 3],
                vertices = new Vector3[vertNumber],
                vertBuffer = new float[vertNumber * 3],
                colBuffer = new int[vertNumber * 3],
                colors = new Color32[vertNumber],
                normals = new Vector3[vertNumber],
                atomByVert = new int[vertNumber],
                nVert = vertNumber,
                nTri = faceNumber
            };
        }
		else {
			if (vertNumber > mData.nVert) { //New mesh is bigger than previous one => allocate more
				mData.triangles = new int[faceNumber * 3];
				mData.vertices =  new Vector3[vertNumber];
				mData.vertBuffer = new float[vertNumber * 3];
				mData.colBuffer = new int[vertNumber * 3];
				mData.colors = new Color32[vertNumber];
				mData.normals = new Vector3[vertNumber];
				mData.atomByVert = new int[vertNumber];
			}

			mData.nVert = vertNumber;
			mData.nTri = faceNumber;
		}


		Marshal.Copy(IntArrayPtrVertices, mData.vertBuffer, 0, 3 * vertNumber);
		Marshal.Copy(IntArrayPtrColors, mData.colBuffer, 0, 3 * vertNumber);
		Marshal.Copy(IntArrayPtrTriangles, mData.triangles, 0, 3 * faceNumber);
		Marshal.Copy(IntArrayPtrAtomVert, mData.atomByVert, 0, vertNumber);


		Marshal.FreeCoTaskMem(IntArrayPtrVertices);
		Marshal.FreeCoTaskMem(IntArrayPtrColors);
		Marshal.FreeCoTaskMem(IntArrayPtrTriangles);
		Marshal.FreeCoTaskMem(IntArrayPtrAtomVert);

		mData.FillWhite();
		mData.CopyVertBufferToVert();

		mData.InvertX();
		mData.InvertTri();

        //Make Unity crash when calling:
        //freeMeshData(EDTSurfObj);
	}

    /// <summary>
    /// Test if there is "ATOM" field in lines
    /// </summary>
    /// <param name="lines">the string containing the PDB lines</param>
    /// <returns>True if ATOM found. False otherwise</returns>
	public static bool EmptyAtomLines(string lines) {
		return lines.IndexOf("ATOM", StringComparison.Ordinal) == -1;
	}
}
}
