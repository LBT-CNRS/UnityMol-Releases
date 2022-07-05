using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.Threading;

namespace UMol {


public class EDTSurfWrapper {

	[DllImport("EDTSurfLib")]
	public static extern IntPtr getEDTSurfLibrary();

	[DllImport("EDTSurfLib")]
	public static extern void ComputeSurfaceMesh(IntPtr instance, string name, string lines, out int vertnumber, out int facenumber);

	[DllImport("EDTSurfLib")]
	public static extern IntPtr getVertices(IntPtr instance);

	[DllImport("EDTSurfLib")]
	public static extern IntPtr getColors(IntPtr instance);

	[DllImport("EDTSurfLib")]
	public static extern IntPtr getTriangles(IntPtr instance);

	[DllImport("EDTSurfLib")]
	public static extern IntPtr getAtomVert(IntPtr instance);

	[DllImport("EDTSurfLib")]
	public static extern void freeMeshData(IntPtr instance);

	[DllImport("EDTSurfLib")]
	public static extern void Destroy(IntPtr instance);


	public static GameObject createEDTSurface(Transform meshPar, string name, UnityMolSelection select, ref int[] meshAtomVert, ref MeshData mData) {
		if (select.Count < 10) {
			Debug.LogError("Cannot create an EDTSurf surface for a selection containing less than 10 atoms");
			return null;
		}
		Vector3[] atomPos = new Vector3[select.Count];
		int id = 0;
		foreach(UnityMolAtom a in select.atoms){
			atomPos[id++] = a.position;
		}

		string pdbLines = PDBReader.Write(select, overridedPos: atomPos);

		if (pdbLines.Length == 0 || countAtomInPDBLines(pdbLines) == 0) {
			//Try to write HET as Atoms
			pdbLines = PDBReader.Write(select, writeModel: false, writeHET: true, forceHetAsAtom: true);
		}

		// Debug.Log("'"+pdbLines+"'");

		mData = callEDTSurf(name, pdbLines, ref meshAtomVert);


		if (mData != null) {

			mData.colors = null;
			GameObject newMeshGo = new GameObject(name);
			newMeshGo.transform.parent = meshPar;
			newMeshGo.transform.localPosition = Vector3.zero;
			newMeshGo.transform.localRotation = Quaternion.identity;
			newMeshGo.transform.localScale = Vector3.one;

			Mesh newMesh = new Mesh();
			newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			newMesh.vertices = mData.vertices;
			newMesh.triangles = mData.triangles;
			// newMesh.colors32 = mData.colors;

			newMesh.RecalculateNormals();


			MeshFilter mf = newMeshGo.AddComponent<MeshFilter>();
			mf.mesh = newMesh;

			MeshRenderer mr = newMeshGo.AddComponent<MeshRenderer>();

			Material mat = new Material(Shader.Find("Custom/SurfaceVertexColor"));
			mat.SetFloat("_Glossiness", 0.0f);
			mat.SetFloat("_Metallic", 0.0f);
			mat.SetFloat("_AOIntensity", 1.03f);
			mat.SetFloat("_AOPower", 8.0f);

			mr.material = mat;

			return newMeshGo;
		}
		return null;

	}



	//Calls native plugin EDTSurf to create meshes and return the number of meshes created
	public static MeshData callEDTSurf(string pdbName, string pdbLines, ref int[] atomVert) {

		if (pdbLines.Length == 0 || countAtomInPDBLines(pdbLines) == 0) {
			Debug.LogWarning("No atoms for surface");
			return null;
		}

		int vertNumber;
		int faceNumber;

		IntPtr IntArrayPtrVertices;
		IntPtr IntArrayPtrColors;
		IntPtr IntArrayPtrTriangles;
		IntPtr IntArrayPtrAtomVert;

		IntPtr EDTSurfObj = getEDTSurfLibrary();

		if(EDTSurfObj == IntPtr.Zero){
			Debug.LogError("Something went wrong when initializing EDTSurf library");
			return null;
		}


		ComputeSurfaceMesh(EDTSurfObj, pdbName, pdbLines, out vertNumber, out faceNumber);

		IntArrayPtrVertices = getVertices(EDTSurfObj);
		IntArrayPtrColors = getColors(EDTSurfObj);
		IntArrayPtrTriangles = getTriangles(EDTSurfObj);
		IntArrayPtrAtomVert = getAtomVert(EDTSurfObj);

		float[] vertices = new float[vertNumber * 3];
		int[] colors = new int[vertNumber * 3];
		int[] triangles = new int[faceNumber * 3];
		atomVert = new int[vertNumber];

		Marshal.Copy(IntArrayPtrVertices, vertices, 0, 3 * vertNumber);
		Marshal.Copy(IntArrayPtrColors, colors, 0, 3 * vertNumber);
		Marshal.Copy(IntArrayPtrTriangles, triangles, 0, 3 * faceNumber);
		Marshal.Copy(IntArrayPtrAtomVert, atomVert, 0, vertNumber);


		Marshal.FreeCoTaskMem(IntArrayPtrVertices);
		Marshal.FreeCoTaskMem(IntArrayPtrColors);
		Marshal.FreeCoTaskMem(IntArrayPtrTriangles);
		Marshal.FreeCoTaskMem(IntArrayPtrAtomVert);

		// freeMeshData(EDTSurfObj);
		// Destroy(EDTSurfObj);

		Vector3[] allVertices = new Vector3[vertNumber];
		Color32[] allColors = new Color32[vertNumber];
		Vector3[] normals = new Vector3[vertNumber];
		for (long i = 0; i < vertNumber; i++) {
			Vector3 v = new Vector3(-vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2]);
			allVertices[i] = v;
			allColors[i] = new Color32((byte)colors[i * 3], (byte)colors[i * 3 + 1], (byte)colors[i * 3 + 2], (byte)255);
			normals[i] = Vector3.zero;
		}
		for (long i = 0; i < faceNumber; i++) { //Revert the triangles
			int save = triangles[i * 3];
			triangles[i * 3] = triangles[i * 3 + 1];
			triangles[i * 3 + 1] = save;
		}

		MeshData mData = new MeshData();
		mData.triangles = triangles;
		mData.vertices = allVertices;
		mData.colors = allColors;
		mData.normals = normals;

		return mData;

	}

	public static int countAtomInPDBLines(string lines) {
		// int count = source.Length - source.Replace("/", "").Length;
		return CountStringOccurrences(lines, "ATOM");
	}

	/// <summary>
	/// Count occurrences of strings.
	/// </summary>
	public static int CountStringOccurrences(string text, string pattern)
	{
		// Loop through all instances of the string 'text'.
		int count = 0;
		int i = 0;
		while ((i = text.IndexOf(pattern, i)) != -1)
		{
			i += pattern.Length;
			count++;
		}
		return count;
	}

	public static void launchThreads(List<UnityMolSelection> subSelections) {
		List<EDTSurfThread> threads = new List<EDTSurfThread>();
		foreach (UnityMolSelection s in subSelections) {
			EDTSurfThread t = new EDTSurfThread();
			t.subSel = s;
			t.name = s.name;
			t.tempPath = Application.temporaryCachePath;

			t.StartThread();
			threads.Add(t);

		}

		foreach(EDTSurfThread t in threads){
			t.EndThread();
		}
	}
	
}

public class EDTSurfThread {
	public bool isSetup = false;
	public bool _isRunning;
	public bool _isDone = true;
	public string tempPath;
	private Thread _thread;

	public UnityMolSelection subSel;
	public string name;

	public MeshData mData;
	public int[] meshAtomVert;

	// Create new Thread
	public void StartThread() {
		_thread = new Thread(CalculateMesh);
		_thread.Start();
	}

	void CalculateMesh() {
		_isDone = false;
		_isRunning = true;

		// string pdbLines = PDBReader.Write(subSel);

		// if (pdbLines.Length == 0 || EDTSurfWrapper.countAtomInPDBLines(pdbLines) == 0) {
		// 	//Try to write HET as Atoms
		// 	pdbLines = PDBReader.Write(subSel, writeModel: false, writeHET: true, forceHetAsAtom: true);
		// }

		// mData = EDTSurfWrapper.callEDTSurf(name, pdbLines, ref meshAtomVert);
		// mData = MSMSWrapper.callMSMS(subSel, 10.0f, tempPath);

		_isRunning = false;
		_isDone = true;

	}

	public void EndThread() {
		if (_isRunning) {

			// Force thread to quit
			_isRunning = false;

			// wait for thread to finish and clean up
			_thread.Join();
		}

		subSel = null;
		mData = null;
		meshAtomVert = null;
		isSetup = false;
	}
}


}

