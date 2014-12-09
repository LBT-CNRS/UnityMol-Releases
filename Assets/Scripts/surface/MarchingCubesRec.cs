/// @file MarchingCubesRec.cs
/// @brief Details to be specified
/// @author FvNano/LBT team
/// @author Marc Baaden <baaden@smplinux.de>
/// @date   2013-4
///
/// Copyright Centre National de la Recherche Scientifique (CNRS)
///
/// contributors :
/// FvNano/LBT team, 2010-13
/// Marc Baaden, 2010-13
///
/// baaden@smplinux.de
/// http://www.baaden.ibpc.fr
///
/// This software is a computer program based on the Unity3D game engine.
/// It is part of UnityMol, a general framework whose purpose is to provide
/// a prototype for developing molecular graphics and scientific
/// visualisation applications. More details about UnityMol are provided at
/// the following URL: "http://unitymol.sourceforge.net". Parts of this
/// source code are heavily inspired from the advice provided on the Unity3D
/// forums and the Internet.
///
/// This software is governed by the CeCILL-C license under French law and
/// abiding by the rules of distribution of free software. You can use,
/// modify and/or redistribute the software under the terms of the CeCILL-C
/// license as circulated by CEA, CNRS and INRIA at the following URL:
/// "http://www.cecill.info".
/// 
/// As a counterpart to the access to the source code and rights to copy, 
/// modify and redistribute granted by the license, users are provided only 
/// with a limited warranty and the software's author, the holder of the 
/// economic rights, and the successive licensors have only limited 
/// liability.
///
/// In this respect, the user's attention is drawn to the risks associated 
/// with loading, using, modifying and/or developing or reproducing the 
/// software by the user in light of its specific status of free software, 
/// that may mean that it is complicated to manipulate, and that also 
/// therefore means that it is reserved for developers and experienced 
/// professionals having in-depth computer knowledge. Users are therefore 
/// encouraged to load and test the software's suitability as regards their 
/// requirements in conditions enabling the security of their systems and/or 
/// data to be ensured and, more generally, to use and operate it in the 
/// same conditions as regards security.
///
/// The fact that you are presently reading this means that you have had 
/// knowledge of the CeCILL-C license and that you accept its terms.
///
/// $Id: MarchingCubesRec.cs 325 2013-07-23 17:36:53Z kouyoumdjian $
///
/// References : 
/// If you use this code, please cite the following reference : 	
/// Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
/// "Game on, Science - how video game technology may help biologists tackle
/// visualization challenges" (2013), PLoS ONE 8(3):e57990.
/// doi:10.1371/journal.pone.0057990
///
/// If you use the HyperBalls visualization metaphor, please also cite the
/// following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
/// B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
/// using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
/// J. Comput. Chem., 2011, 32, 2924
///

using UnityEngine;
using System.Collections.Generic;
using System;
using Molecule.Model;

public class MarchingCubesRec {
	
	private int ncellsX; // dimensions of the grid
	private int ncellsY;
	private int ncellsZ;
//	private float toleranceValue; // range around the isosurface threshold value "minValue"
	private float minValue; // threshold value to determine whether a point is in or out of the surface
	private Vector4[] points;
	
	private int depthSize; // dimension of a side
	private int sliceSize; // dimension of a slice
	private Vector3 center; // value used to center the objects to be drawn on 0,0,0 on the scene
	
	private int nbIndTriangles; // size of mesh.triangles
	private int nbIndVertices; // size of mesh.vertices
	private bool[] marchedCubes; // list of length "dataLength", indicates if cubes already have been marched
	private bool[] marchedCubes2;
	private bool[] queuedCubes;
	private bool[] queuedCubes2;
	private int[] vertIndexGrid; // list of length "dataLength", records indexes of previously created vertices, so we can build the triangle indexes list using unique vertices
	private int[] vertIndexGrid2;
	private int dataLength; // nb of points in 2 slices, = ncellsX * ncellsZ * 2
//	private int fullDataLength; // total nb of points, = ncellsX * ncellsY * ncellsZ
	private int indFound;
	
	private int numVertices; // indice of created vertices
	private int[] TMPtriangles; // to store mesh.triangles, before we know the size
	private Vector3[] TMPvertices; // to store mesh.vertices, before we know the size
	private int[] Mtriangles; // to store final array of mesh.triangles
	private Vector3[] Mvertices; // to store final array of mesh.vertices
	
	private Vector4[] verts;
	private Vector3[] intVerts;
	private int cubeIndex; // list cubeIndex of already marched cubes
	private int[] index;
	private int edgeIndex;
	private Vector4 p; // Vector4 returned by LinearInterp, to calculate vertices position
	private int surfaceNb; // identify each part of the created surface, split by parts of ~64900 vertices
	private GenerateMesh GMInstance;
	
	private bool[] foundVert;
	private Queue<int> IndQueue;
	private Queue<int> IndQueue2;
//	private Queue<int[]> PosQueue;
//	private Queue<int[]> PosQueue2;
	
	private Queue<int> XPosQueue;
	private Queue<int> YPosQueue;
	private Queue<int> ZPosQueue;
	private Queue<int> XPosQueue2;
	private Queue<int> YPosQueue2;
	private Queue<int> ZPosQueue2;	
	
	private int ind;
	private int fullInd;
//	private int[] gridPos;
	private int indice;
//	private int[] gridTemp;
	private int sliceNb;
	
	private int XPos;
	private int YPos;
	private int ZPos;
	
	private Vector3 Delta;
	private Vector3 Origin;
	private string tag;

	private Color[] colors;
	
	public void MCRecMain(int NX, int NY, int NZ, float minV, Vector4[] P, float tolV,
						  bool reversedThreshold, Vector3 delta, Vector3 origin, 
						  Color[] colors = null,
						  string tag = "SurfaceManager") {

		
		// TODO : Séparer les variables par type d'utilisation (MC, global data, ...)

		this.tag = tag;
		this.colors = colors;
		
//		gridTemp = new int[3];
		// /!\ ORIENTATION OF THE GRID : X, Y classical, Z as depth (may be reversed as : Z,Y,X)
		ncellsX = NX;
		ncellsY = NY;
		ncellsZ = NZ;
//		toleranceValue = tolV;
		minValue = minV;
		
		foundVert = new bool[12];
		depthSize = ncellsX; // fix depth axe
		sliceSize = (ncellsZ) * depthSize;
		
		dataLength = ncellsX * ncellsZ * 2;
//		fullDataLength = ncellsX * ncellsY * ncellsZ;
		
		surfaceNb = 0;
		
//		center = new Vector3 (-ncellsX/2.0f, -ncellsY/2.0f, -ncellsZ/2.0f); // value used to center the objects to be drawn on 0,0,0 on the scene
		center = new Vector3(0f,0f,0f);
		
		nbIndVertices = 0;
		nbIndTriangles = 0;
		numVertices = 0;
		MarchingModel.TMPvertices = new Vector3[64950]; // unity limit
		MarchingModel.TMPColors = new Color[64950];
		MarchingModel.TMPtriangles = new int[65000*6]; // size is arbitrary, it can be changed to less or more depending of the type of data (for triangles)
		IndQueue = new Queue<int>();
		IndQueue2 = new Queue<int>();
//		PosQueue = new Queue<int[]>();
//		PosQueue2 = new Queue<int[]>();
		
		XPosQueue = new Queue<int>();
		YPosQueue = new Queue<int>();
		ZPosQueue = new Queue<int>();
		XPosQueue2 = new Queue<int>();
		YPosQueue2 = new Queue<int>();
		ZPosQueue2 = new Queue<int>();
		
		verts = new Vector4[8];
		points = new Vector4[dataLength];
		intVerts = new Vector3[12];
		index = new int[3];
//		gridPos = new int[3];
		p = new Vector4();
		vertIndexGrid = new int[sliceSize * 12];
		vertIndexGrid2 = new int[sliceSize * 12];
		GMInstance = new GenerateMesh();
		
		marchedCubes = new bool[sliceSize]; // initialize to false, no cubes have been marched nor queued
		marchedCubes2 = new bool[sliceSize];
		queuedCubes = new bool[sliceSize];
		queuedCubes2 = new bool[sliceSize];
		
//		Debug.Log("MC tables test : "+MarchingCubesTables.edgeTable[1]);
		Delta = delta;
		Origin = origin;
		
//		Debug.Log("Origin: "+ Origin);
		
		sliceNb = ncellsY - 2;
		
		for (int i = 0; i < sliceSize; i++) { // TODO : remove useless ones
			marchedCubes[i] = false;
			marchedCubes2[i] = false;
			queuedCubes2[i] = false;
			queuedCubes[i] = false;
		}
		for (int i=0; i < sliceSize * 12; i++) {
			vertIndexGrid[i] = -1;
			vertIndexGrid2[i] = -1;
		}
		MarchingModel.Mtriangles = new int[nbIndTriangles]; // allocate mesh triangles and vertices, final size
		MarchingModel.Mvertices = new Vector3[nbIndVertices];
		MarchingModel.MColors = new Color[nbIndVertices];
		
		if (reversedThreshold)
			MCRecReversedThres(P);
		else
			MCRec(P); // begin marching cubes
		
		MarchingModel.Mtriangles = new int[nbIndTriangles]; // allocate mesh triangles and vertices, final size
		MarchingModel.Mvertices = new Vector3[nbIndVertices];
		MarchingModel.MColors = new Color[nbIndVertices];
		
		for (int i=0; i < nbIndTriangles; i++)
			MarchingModel.Mtriangles[i] = MarchingModel.TMPtriangles[i];
		
		for (int i=0; i < nbIndVertices; i++){
			MarchingModel.Mvertices[i] = MarchingModel.TMPvertices[i];
			MarchingModel.MColors[i] = MarchingModel.TMPColors[i];
		}		
		GMInstance.GM(MarchingModel.Mvertices, MarchingModel.Mtriangles, center, surfaceNb,MarchingModel.MColors,tag); // generate display, potentially split on several objects
		
		SetAllVariablesToNull() ;
	}
	
	void MCInitAndFindEachIndex (Vector4[] P) { // find all points inside the surface, stored in "indList"
		
//		Debug.Log ("MCInitAnd.., sliceNb : "+sliceNb);
		
		for (int j = 0; j < dataLength; j++) { // record density values for the current slice
			fullInd = sliceNb * sliceSize + j;
			points[j] = P[fullInd];	
		}
		
		for (int j = 0; j < sliceSize; j++) {
			if (points[j].w >= minValue && !queuedCubes[j]) { // queue intersecting cubes
				if ((int)points[j].x != ncellsX-1 && (int)points[j].y != ncellsY-1 && (int)points[j].z != ncellsZ-1) {
					IndQueue.Enqueue(j);
					XPosQueue.Enqueue((int)points[j].x);
					YPosQueue.Enqueue((int)points[j].y);
					ZPosQueue.Enqueue((int)points[j].z);
//					Debug.Log ("MCINit and find queued : "+(int)points[j].x+", "+(int)points[j].y+", "+(int)points[j].z);
					queuedCubes[j] = true;
				}
			}
		}
//		Debug.Log ("IndQueue.Count : "+IndQueue.Count);
	}
	
	void MCInitAndFindEachIndex_2 (Vector4[] P) { // find all points inside the surface, stored in "indList"
		
//		Debug.Log ("MCInitAnd..2, sliceNb : "+sliceNb);
		
		for (int j = 0; j < dataLength; j++) {
			fullInd = sliceNb * sliceSize + j;
			points[j] = P[fullInd];
		}
		
		for (int j = 0; j < sliceSize; j++) {	
			if (points[j].w >= minValue && !queuedCubes2[j]) {
				if ((int)points[j].x != ncellsX-1 && (int)points[j].y != ncellsY-1 && (int)points[j].z != ncellsZ-1) {
					IndQueue2.Enqueue(j);
					XPosQueue2.Enqueue((int)points[j].x);
					YPosQueue2.Enqueue((int)points[j].y);
					ZPosQueue2.Enqueue((int)points[j].z);
//					Debug.Log ("MCINit and find 2 queued : "+(int)points[j].x+", "+(int)points[j].y+", "+(int)points[j].z);
					queuedCubes2[j] = true;
				}
			}
		}
//		Debug.Log ("IndQueue2.Count : "+IndQueue2.Count);
	}
	
	void MCRec(Vector4[] P) { // proceed recursive marching cubes on yet unproceeded cubes, starting cube at index : "ind"
			
		MCInitAndFindEachIndex (P); // find all points inside the surface at first execution
		
		while (sliceNb >= 2) {
			
//			Debug.Log("IndQueue.Count : "+IndQueue.Count);
			while (IndQueue.Count != 0) {
				ind = IndQueue.Dequeue ();
				XPos = XPosQueue.Dequeue();
				YPos = YPosQueue.Dequeue();
				ZPos = ZPosQueue.Dequeue();
				
				MarchOneCube();
				
				TestFace0 ();
				TestFace2 ();
				TestFace3 ();
				TestFace4 ();
				TestFace5 ();
			}
			sliceNb --;
			
			MCInitAndFindEachIndex_2 (P); // find all points inside the surface
			for (int i = 0; i < sliceSize; i++) {
				queuedCubes[i] = false;
				marchedCubes2[i] = false;
			}
			for (int i=0; i < sliceSize * 12; i++)
				vertIndexGrid2[i] = -1;
			
//			Debug.Log("IndQueue2.Count : "+IndQueue2.Count);
			while (IndQueue2.Count != 0) {
				ind = IndQueue2.Dequeue ();
				XPos = XPosQueue2.Dequeue();
				YPos = YPosQueue2.Dequeue();
				ZPos = ZPosQueue2.Dequeue();
				
				MarchOneCube_2();
				
				TestFace0_2 ();
				TestFace2_2 ();
				TestFace3_2 ();
				TestFace4_2 ();
				TestFace5_2 ();
			}
			sliceNb --;
			
			MCInitAndFindEachIndex (P); // find all points inside the surface
			for (int i = 0; i < sliceSize; i++) {
				queuedCubes2[i] = false;
				marchedCubes[i] = false;
			}
			for (int i=0; i < sliceSize * 12; i++)
				vertIndexGrid[i] = -1;
			// end of normal cycle
			
			if (sliceNb == 0) { // last step
				
				while (IndQueue.Count != 0) {
					ind = IndQueue.Dequeue ();
					XPos = XPosQueue.Dequeue();
					YPos = YPosQueue.Dequeue();
					ZPos = ZPosQueue.Dequeue();
					
					MarchOneCube();
					
					TestFace0 ();
					TestFace2 ();
					TestFace4 ();
					TestFace5 ();
				}
			}
		}
	}
	
	void TestFace0 () { // check which adjacent cubes have to be checked depending on previous cube index and grid limits
		if (XPos > 0 && ((cubeIndex & 1) > 0 || (cubeIndex & 2) > 0 || (cubeIndex & 16) > 0 || (cubeIndex & 32) > 0)) {
			if (!queuedCubes[ind-1]) {
//				Debug.Log ("TestFace0 Queued "+((int)points[ind].x-1)+", "+((int)points[ind].y)+", "+(int)points[ind].z);
				IndQueue.Enqueue(ind - 1);
				XPosQueue.Enqueue((int)points[ind].x-1);
				YPosQueue.Enqueue((int)points[ind].y);
				ZPosQueue.Enqueue((int)points[ind].z);
				queuedCubes[ind-1] = true;
			}
		}
	}
	void TestFace0_2 () { // check which adjacent cubes have to be checked depending on previous cube index and grid limits
		if (XPos > 0 && ((cubeIndex & 1) > 0 || (cubeIndex & 2) > 0 || (cubeIndex & 16) > 0 || (cubeIndex & 32) > 0)) {
			if (!queuedCubes2[ind-1]) {
//				Debug.Log ("TestFace0_2 Queued "+((int)points[ind].x-1)+", "+((int)points[ind].y)+", "+(int)points[ind].z);
				IndQueue2.Enqueue(ind - 1);
				XPosQueue2.Enqueue((int)points[ind].x-1);
				YPosQueue2.Enqueue((int)points[ind].y);
				ZPosQueue2.Enqueue((int)points[ind].z);
				queuedCubes2[ind-1] = true;
			}
		}
	}
	void TestFace2 () { // check which adjacent cubes have to be checked depending on previous cube index and grid limits
		if (XPos < ncellsX-2 && ((cubeIndex & 4) > 0 || (cubeIndex & 8) > 0 || (cubeIndex & 64) > 0 || (cubeIndex & 128) > 0)) {
			if (!queuedCubes[ind+1]) {
//				Debug.Log ("TestFace2 Queued "+((int)points[ind].x+1)+", "+((int)points[ind].y)+", "+(int)points[ind].z);
				IndQueue.Enqueue(ind + 1);
				XPosQueue.Enqueue((int)points[ind].x+1);
				YPosQueue.Enqueue((int)points[ind].y);
				ZPosQueue.Enqueue((int)points[ind].z);
				queuedCubes[ind+1] = true;
			}
		}
	}
	void TestFace2_2 () { // check which adjacent cubes have to be checked depending on previous cube index and grid limits
		if (XPos < ncellsX-2 && ((cubeIndex & 4) > 0 || (cubeIndex & 8) > 0 || (cubeIndex & 64) > 0 || (cubeIndex & 128) > 0)) {
			if (!queuedCubes2[ind+1]) {
//				Debug.Log ("TestFace2_2 Queued "+((int)points[ind].x+1)+", "+((int)points[ind].y)+", "+(int)points[ind].z);
				IndQueue2.Enqueue(ind + 1);
				XPosQueue2.Enqueue((int)points[ind].x+1);
				YPosQueue2.Enqueue((int)points[ind].y);
				ZPosQueue2.Enqueue((int)points[ind].z);
				queuedCubes2[ind+1] = true;
			}
		}
	}
	void TestFace3 () { // check which adjacent cubes have to be checked depending on previous cube index and grid limits
		// Face 3 : changing slice, invert queues
		if (YPos > 0 && ((cubeIndex & 1) > 0 || (cubeIndex & 8) > 0 || (cubeIndex & 16) > 0 || (cubeIndex & 128) > 0)) {
			if (!queuedCubes2[ind]) { // TODO : Useless check ?
//				Debug.Log ("TestFace3 Queued "+(int)points[ind].x+", "+((int)points[ind].y-1)+", "+((int)points[ind].z));
				IndQueue2.Enqueue(ind);
				XPosQueue2.Enqueue((int)points[ind].x);
				YPosQueue2.Enqueue((int)points[ind].y-1);
				ZPosQueue2.Enqueue((int)points[ind].z);
				queuedCubes2[ind] = true;
			}
		}
	}
	void TestFace3_2 () { // check which adjacent cubes have to be checked depending on previous cube index and grid limits
		// Face 3 : changing slice, invert queues
		if (YPos > 0 && ((cubeIndex & 1) > 0 || (cubeIndex & 8) > 0 || (cubeIndex & 16) > 0 || (cubeIndex & 128) > 0)) {
			if (!queuedCubes[ind]) { // TODO : Useless check ?
//				Debug.Log ("TestFace3_2 Queued "+(int)points[ind].x+", "+((int)points[ind].y-1)+", "+((int)points[ind].z));
				IndQueue.Enqueue(ind);
				XPosQueue.Enqueue((int)points[ind].x);
				YPosQueue.Enqueue((int)points[ind].y-1);
				ZPosQueue.Enqueue((int)points[ind].z);
				queuedCubes[ind] = true;
			}
		}
	}
	void TestFace4 () { // check which adjacent cubes have to be checked depending on previous cube index and grid limits
		if (ZPos < depthSize-2 && ((cubeIndex & 16) > 0 || (cubeIndex & 32) > 0 || (cubeIndex & 64) > 0 || (cubeIndex & 128) > 0)) {
			if (!queuedCubes[ind+depthSize]) {
//				Debug.Log ("TestFace4 Queued "+((int)points[ind].x)+", "+((int)points[ind].y)+", "+((int)points[ind].z+1));
				IndQueue.Enqueue(ind+depthSize);
				XPosQueue.Enqueue((int)points[ind].x);
				YPosQueue.Enqueue((int)points[ind].y);
				ZPosQueue.Enqueue((int)points[ind].z+1);
				queuedCubes[ind+depthSize] = true;
			}
		}
	}
	void TestFace4_2 () { // check which adjacent cubes have to be checked depending on previous cube index and grid limits
		if (ZPos < depthSize-2 && ((cubeIndex & 16) > 0 || (cubeIndex & 32) > 0 || (cubeIndex & 64) > 0 || (cubeIndex & 128) > 0)) {
			if (!queuedCubes2[ind+depthSize]) {
//				Debug.Log ("TestFace4_2 Queued "+((int)points[ind].x)+", "+((int)points[ind].y-1)+", "+((int)points[ind].z+1));
				IndQueue2.Enqueue(ind+depthSize);
				XPosQueue2.Enqueue((int)points[ind].x);
				YPosQueue2.Enqueue((int)points[ind].y);
				ZPosQueue2.Enqueue((int)points[ind].z+1);
				queuedCubes2[ind+depthSize] = true;
			}
		}
	}
	void TestFace5 () { // check which adjacent cubes have to be checked depending on previous cube index and grid limits
		if (ZPos > 0 && ((cubeIndex & 1) > 0 || (cubeIndex & 2) > 0 || (cubeIndex & 4) > 0 || (cubeIndex & 8) > 0)) {
			if (!queuedCubes[ind-depthSize]) {
//				Debug.Log ("TestFace5 Queued "+((int)points[ind].x)+", "+((int)points[ind].y)+", "+((int)points[ind].z-1));
				IndQueue.Enqueue(ind-depthSize);
				XPosQueue.Enqueue((int)points[ind].x);
				YPosQueue.Enqueue((int)points[ind].y);
				ZPosQueue.Enqueue((int)points[ind].z-1);
				queuedCubes[ind-depthSize] = true;
			}
		}
	}
	void TestFace5_2 () { // check which adjacent cubes have to be checked depending on previous cube index and grid limits
		if (ZPos > 0 && ((cubeIndex & 1) > 0 || (cubeIndex & 2) > 0 || (cubeIndex & 4) > 0 || (cubeIndex & 8) > 0)) {
			if (!queuedCubes2[ind-depthSize]) {
//				Debug.Log ("TestFace5_2 Queued "+((int)points[ind].x)+", "+((int)points[ind].y)+", "+((int)points[ind].z-1));
				IndQueue2.Enqueue(ind-depthSize);
				XPosQueue2.Enqueue((int)points[ind].x);
				YPosQueue2.Enqueue((int)points[ind].y);
				ZPosQueue2.Enqueue((int)points[ind].z-1);
				queuedCubes2[ind-depthSize] = true;
			}
		}
	}
	
	void MarchOneCube() { // check how to draw triangles inside current cube, referenced by its indice "ind"
		
//		Debug.LogWarning ("MarchOneCube ind : "+ind+", gridPos : "+XPos+", "+YPos+", "+ZPos);
		
		if (nbIndVertices > 64900) { // generate several parts of ~64900 vertices if it's going further (Unity limitation at 65000 vertices per GameObject)
			
			MarchingModel.Mtriangles = new int[nbIndTriangles]; // allocate mesh triangles and vertices
			MarchingModel.Mvertices = new Vector3[nbIndVertices];
			MarchingModel.MColors = new Color[nbIndVertices];
			for (int i=0; i < nbIndTriangles; i++)
				MarchingModel.Mtriangles[i] = MarchingModel.TMPtriangles[i];
			
			for (int i=0; i < nbIndVertices; i++){
				MarchingModel.MColors[i] = MarchingModel.TMPColors[i];
				MarchingModel.Mvertices[i] = MarchingModel.TMPvertices[i];
			}
			GMInstance.GM(MarchingModel.Mvertices, MarchingModel.Mtriangles, center, surfaceNb,MarchingModel.MColors,tag); // generate display, eventually split on several objects
			
			
			
			MarchingModel.TMPtriangles=null;
			MarchingModel.TMPvertices=null;
			MarchingModel.TMPColors = null;
			MarchingModel.TMPtriangles = new int[65000*6]; // reset temporary triangles and vertices
			MarchingModel.TMPvertices = new Vector3[64950];
			MarchingModel.TMPColors = new Color[64950];
			surfaceNb++;
			nbIndVertices = 0;
			nbIndTriangles = 0;
			numVertices = 0;
			for (int i=0; i < sliceSize * 12; i++){
				vertIndexGrid[i] = -1;
				vertIndexGrid2[i] = -1;
			}
		}
		
		for (int i=0; i < 12; i++)
			foundVert[i] = false;
		
		verts[0] = points[ind]; // list values of current cube's vertex (cube at index : "ind")
		verts[1] = points[ind + sliceSize];
		verts[2] = points[ind + sliceSize + 1];
		verts[3] = points[ind + 1];
		verts[4] = points[ind + depthSize];
		verts[5] = points[ind + sliceSize + depthSize];
		verts[6] = points[ind + sliceSize + depthSize + 1];
		verts[7] = points[ind + depthSize + 1];
		
		cubeIndex = 0;
		marchedCubes[ind] = true; // make sure this cube is listed as already marched
		
//		for (int i=ind*12; i < ind*12+12; i++)
//			vertIndexGrid[i] = -1;
		
		for (int n = 0; n < 8; n++)
//			if ((verts[n].w >= minValue - toleranceValue) && (verts[n].w <= minValue + toleranceValue))
			if (verts[n].w >= minValue)
				cubeIndex |= (1 << n);
		
//		Debug.Log("cubeIndex : "+cubeIndex);
		if (cubeIndex != 0 && cubeIndex != 255) { // if this cube intersects the surface

			edgeIndex = MarchingCubesTables.edgeTable[cubeIndex];
			
			if ((edgeIndex & 1) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes[ind-1] && vertIndexGrid[(ind-1)*12+2] != -1) {
						vertIndexGrid[ind*12] = vertIndexGrid[(ind-1)*12+2];
						foundVert[0] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes[ind-depthSize] && vertIndexGrid[(ind-depthSize)*12+4] != -1) {
						vertIndexGrid[ind*12] = vertIndexGrid[(ind-depthSize)*12+4];
						foundVert[0] = true;
					}
				}
				if (!foundVert[0]) {
					LinearInterp(verts[0], verts[1]);
					intVerts[0] = p;
				}
			}

			if ((edgeIndex & 2) > 0) {
				if (sliceNb < ncellsY-2) {
					if (marchedCubes2[ind] && vertIndexGrid2[ind*12+3] != - 1) {
						vertIndexGrid[ind*12+1] = vertIndexGrid2[ind*12+3];
						foundVert[1] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes[ind-depthSize] && vertIndexGrid[(ind-depthSize)*12+5] != -1) {
						vertIndexGrid[ind*12+1] = vertIndexGrid[(ind-depthSize)*12+5];
						foundVert[1] = true;
					}
				}
				if (!foundVert[1]) {
					LinearInterp(verts[1], verts[2]);
					intVerts[1] = p;
				}
			}

			if ((edgeIndex & 4) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes[ind+1] && vertIndexGrid[(ind+1)*12] != -1) {
						vertIndexGrid[ind*12+2] = vertIndexGrid[(ind+1)*12];
						foundVert[2] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes[ind-depthSize] && vertIndexGrid[(ind-depthSize)*12+6] != -1) {
						vertIndexGrid[ind*12+2] = vertIndexGrid[(ind-depthSize)*12+6];
						foundVert[2] = true;
					}
				}
				if (!foundVert[2]) {
					LinearInterp(verts[2], verts[3]);
					intVerts[2] = p;
				}
			}

			if ((edgeIndex & 8) > 0) {
				if (ind - depthSize >= 0) {
					if (marchedCubes[ind-depthSize] && vertIndexGrid[(ind-depthSize)*12+7] != -1) {
						vertIndexGrid[ind*12+3] = vertIndexGrid[(ind-depthSize)*12+7];
						foundVert[3] = true;
					}
				}
				if (!foundVert[3]) {
					LinearInterp(verts[3], verts[0]);
					intVerts[3] = p;
				}
			}

			if ((edgeIndex & 16) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes[ind-1] && vertIndexGrid[(ind-1)*12+6] != -1) {
						vertIndexGrid[ind*12+4] = vertIndexGrid[(ind-1)*12+6];
						foundVert[4] = true;
					}
				}
				if (ind+depthSize <= dataLength) {
					if (marchedCubes[ind+depthSize] && vertIndexGrid[(ind+depthSize)*12] != -1) {
						vertIndexGrid[ind*12+4] = vertIndexGrid[(ind+depthSize)*12];
						foundVert[4] = true;
					}
				}
				if (!foundVert[4]) {
					LinearInterp(verts[4], verts[5]);
					intVerts[4] = p;
				}
			}
				
			if ((edgeIndex & 32) > 0) {
				if (sliceNb < ncellsY-2) {
					if (marchedCubes2[ind] && vertIndexGrid2[ind*12+7] != -1) {
						vertIndexGrid[ind*12+5] = vertIndexGrid2[ind*12+7];
						foundVert[5] = true;
					}
				}
				if (ind + depthSize <= dataLength) {
					if (marchedCubes[ind+depthSize] && vertIndexGrid[(ind+depthSize)*12+1] != -1) {
						vertIndexGrid[ind*12+5] = vertIndexGrid[(ind+depthSize)*12+1];
						foundVert[5] = true;
					}
				}
				if (!foundVert[5]) {
					LinearInterp(verts[5], verts[6]);
					intVerts[5] = p;
				}
			}
			
			if ((edgeIndex & 64) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes[ind+1] && vertIndexGrid[(ind+1)*12+4] != -1) {
						vertIndexGrid[ind*12+6] = vertIndexGrid[(ind+1)*12+4];
						foundVert[6] = true;
					}
				}
				if (ind+ depthSize <= dataLength) {
					if (marchedCubes[ind+depthSize] && vertIndexGrid[(ind+depthSize)*12+2] != -1) {
						vertIndexGrid[ind*12+6] = vertIndexGrid[(ind+depthSize)*12+2];
						foundVert[6] = true;
					}
				}
				if (!foundVert[6]) {
					LinearInterp(verts[6], verts[7]);
					intVerts[6] = p;
				}
			}

			if ((edgeIndex & 128) > 0) {
				if (ind+depthSize <= dataLength) {
					if (marchedCubes[ind+depthSize] && vertIndexGrid[(ind+depthSize)*12+3] != -1) {
						vertIndexGrid[ind*12+7] = vertIndexGrid[(ind+depthSize)*12+3];
						foundVert[7] = true;
					}
				}
				if (!foundVert[7]) {
					LinearInterp(verts[7], verts[4]);
					intVerts[7] = p;
				}
			}

			if ((edgeIndex & 256) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes[ind-1] && vertIndexGrid[(ind-1)*12+11] != -1) {
						vertIndexGrid[ind*12+8] = vertIndexGrid[(ind-1)*12+11];
						foundVert[8] = true;
					}
				}
				if (!foundVert[8]) {
					LinearInterp(verts[0], verts[4]);
					intVerts[8] = p;
				}
			}

			if ((edgeIndex & 512) > 0) {
				if (ind - 1 >= 0) {
					if (marchedCubes[ind-1] && vertIndexGrid[(ind-1)*12+10] != -1) {
						vertIndexGrid[ind*12+9] = vertIndexGrid[(ind-1)*12+10];
						foundVert[9] = true;
					}
				}
				if (sliceNb < ncellsY-2) {
					if (marchedCubes2[ind] && vertIndexGrid2[ind*12+8] != -1) {
						vertIndexGrid[ind*12+9] = vertIndexGrid2[ind*12+8];
						foundVert[9] = true;
					}
				}
				if (!foundVert[9]) {
					LinearInterp(verts[1], verts[5]);
					intVerts[9] = p;
				}
			}

			if ((edgeIndex & 1024) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes[ind+1] && vertIndexGrid[(ind+1)*12+9] != -1) {
						vertIndexGrid[ind*12+10] = vertIndexGrid[(ind+1)*12+9];
						foundVert[10] = true;
					}
				}
				if (sliceNb < ncellsY-2) {
					if (marchedCubes2[ind] && vertIndexGrid2[ind*12+11] != -1) {
						vertIndexGrid[ind*12+10] = vertIndexGrid2[ind*12+11];
						foundVert[10] = true;
					}
				}
				if (!foundVert[10]) {
					LinearInterp(verts[2], verts[6]);
					intVerts[10] = p;
				}
			}

			if ((edgeIndex & 2048) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes[ind+1] && vertIndexGrid[(ind+1)*12+8] != -1) {
						vertIndexGrid[ind*12+11] = vertIndexGrid[(ind+1)*12+8];
						foundVert[11] = true;
					}
				}
				if (!foundVert[11]) {
					LinearInterp(verts[3], verts[7]);
					intVerts[11] = p;
				}
			}

			for (int n = 0; MarchingCubesTables.triTable[cubeIndex, n] != -1; n += 3) {
				
				// TODO : Problem of reversing normals if density threshold < 0
				index[0] = MarchingCubesTables.triTable[cubeIndex, n];
				index[1] = MarchingCubesTables.triTable[cubeIndex, n + 1];
				index[2] = MarchingCubesTables.triTable[cubeIndex, n + 2];
				for (int h = 0; h < 3; h++)
				{
//					Debug.Log("check -1 : "+vertIndexGrid[ind*12+index[h]]);
					if (vertIndexGrid[ind*12+index[h]] != -1) {
//						Debug.Log("New triangle index : "+vertIndexGrid[ind*12+index[h]]);
//						Debug.Log("nbIndTriangles : "+nbIndTriangles);
						MarchingModel.TMPtriangles[nbIndTriangles] = vertIndexGrid[ind*12+index[h]];
					}
					else {
//						Debug.Log("Triangle index : "+numVertices);
						MarchingModel.TMPtriangles[nbIndTriangles] = numVertices;
						vertIndexGrid[ind*12+index[h]] = numVertices;
						numVertices++;
						if (Delta.x ==2f){
							intVerts[index[h]].x -= 18;
							intVerts[index[h]].y -= 18;
							intVerts[index[h]].z -= 18;
							intVerts[index[h]].x /=Delta.x;
							intVerts[index[h]].y /=Delta.y;
							intVerts[index[h]].z /=Delta.z;
						}else{
				 			intVerts[index[h]].x *=Delta.x;
							intVerts[index[h]].y *=Delta.y;
							intVerts[index[h]].z *=Delta.z;
						}
						intVerts[index[h]].x += Origin.x;
						intVerts[index[h]].y += Origin.y;
						intVerts[index[h]].z += Origin.z;
//						Debug.Log("Origin:"+ Origin);
						
						
						// intVerts[index[h]].x += MoleculeModel.Offset.x;//+MoleculeModel.MinValue.x;
						// intVerts[index[h]].y += MoleculeModel.Offset.y;//+MoleculeModel.MinValue.y;
						// intVerts[index[h]].z += MoleculeModel.Offset.z;//+MoleculeModel.MinValue.z;
//						Debug.Log("New vert : "+intVerts[index[h]]);
						MarchingModel.TMPvertices[nbIndVertices] = intVerts[index[h]];
						
						if(colors != null)
							MarchingModel.TMPColors[nbIndVertices] = colors[sliceNb*sliceSize+ind];
						else
							MarchingModel.TMPColors[nbIndVertices] = Color.white;
						nbIndVertices++;
					}
					nbIndTriangles++;
				}
			}		
		}
	}
	
	void MarchOneCube_2() { // check how to draw triangles inside current cube, referenced by its indice "ind"
		
//		Debug.LogWarning ("MarchOneCube_2 ind : "+ind);
		
		if (nbIndVertices > 64900) { // generate several parts of ~64900 vertices if it's going further (Unity limitation at 65000 vertices per GameObject)
			
			MarchingModel.Mtriangles = new int[nbIndTriangles]; // allocate mesh triangles and vertices
			MarchingModel.Mvertices = new Vector3[nbIndVertices];
			MarchingModel.MColors = new Color[nbIndVertices];
			
			for (int i=0; i < nbIndTriangles; i++)
				MarchingModel.Mtriangles[i] = MarchingModel.TMPtriangles[i];
			
			for (int i=0; i < nbIndVertices; i++){
				MarchingModel.Mvertices[i] = MarchingModel.TMPvertices[i];
				MarchingModel.MColors[i]=MarchingModel.TMPColors[i];
			}
			GMInstance.GM(MarchingModel.Mvertices, MarchingModel.Mtriangles, center, surfaceNb,MarchingModel.MColors,tag); // generate display, eventually split on several objects
			
			MarchingModel.TMPtriangles = null; // reset temporary triangles and vertices
			MarchingModel.TMPvertices = null;
			MarchingModel.TMPColors = null;
			MarchingModel.TMPtriangles = new int[65000*6]; // reset temporary triangles and vertices
			MarchingModel.TMPvertices = new Vector3[64950];
			MarchingModel.TMPColors = new Color[64950];
			surfaceNb++;
			nbIndVertices = 0;
			nbIndTriangles = 0;
			numVertices = 0;
			for (int i=0; i < sliceSize * 12; i++){
				vertIndexGrid[i] = -1;
				vertIndexGrid2[i] = -1;
			}
		}
		
		for (int i=0; i < 12; i++)
			foundVert[i] = false;
		
		verts[0] = points[ind]; // list values of current cube's vertex (cube at index : "ind")
		verts[1] = points[ind + sliceSize];
		verts[2] = points[ind + sliceSize + 1];
		verts[3] = points[ind + 1];
		verts[4] = points[ind + depthSize];
		verts[5] = points[ind + sliceSize + depthSize];
		verts[6] = points[ind + sliceSize + depthSize + 1];
		verts[7] = points[ind + depthSize + 1];
		
		cubeIndex = 0;
		marchedCubes2[ind] = true; // make sure this cube is listed as already marched
		
//		for (int i=ind*12; i < ind*12+12; i++)
//			vertIndexGrid[i] = -1;
		
		for (int n = 0; n < 8; n++)
//			if ((verts[n].w >= minValue - toleranceValue) && (verts[n].w <= minValue + toleranceValue))
			if (verts[n].w >= minValue)
				cubeIndex |= (1 << n);
		
//		Debug.Log("cubeIndex : "+cubeIndex);
		if (cubeIndex != 0 && cubeIndex != 255) { // if this cube intersects the surface

			edgeIndex = MarchingCubesTables.edgeTable[cubeIndex];
			
			if ((edgeIndex & 1) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes2[ind-1] && vertIndexGrid2[(ind-1)*12+2] != -1) {
						vertIndexGrid2[ind*12] = vertIndexGrid2[(ind-1)*12+2];
						foundVert[0] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes2[ind-depthSize] && vertIndexGrid2[(ind-depthSize)*12+4] != -1) {
						vertIndexGrid2[ind*12] = vertIndexGrid2[(ind-depthSize)*12+4];
						foundVert[0] = true;
					}
				}
				if (!foundVert[0]) {
					LinearInterp(verts[0], verts[1]);
					intVerts[0] = p;
				}
			}

			if ((edgeIndex & 2) > 0) {
				if (sliceNb < ncellsY-2) {
					if (marchedCubes[ind] && vertIndexGrid[ind*12+3] != - 1) {
						vertIndexGrid2[ind*12+1] = vertIndexGrid[ind*12+3];
						foundVert[1] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes2[ind-depthSize] && vertIndexGrid2[(ind-depthSize)*12+5] != -1) {
						vertIndexGrid2[ind*12+1] = vertIndexGrid2[(ind-depthSize)*12+5];
						foundVert[1] = true;
					}
				}
				if (!foundVert[1]) {
					LinearInterp(verts[1], verts[2]);
					intVerts[1] = p;
				}
			}

			if ((edgeIndex & 4) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes2[ind+1] && vertIndexGrid2[(ind+1)*12] != -1) {
						vertIndexGrid2[ind*12+2] = vertIndexGrid2[(ind+1)*12];
						foundVert[2] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes2[ind-depthSize] && vertIndexGrid2[(ind-depthSize)*12+6] != -1) {
						vertIndexGrid2[ind*12+2] = vertIndexGrid2[(ind-depthSize)*12+6];
						foundVert[2] = true;
					}
				}
				if (!foundVert[2]) {
					LinearInterp(verts[2], verts[3]);
					intVerts[2] = p;
				}
			}

			if ((edgeIndex & 8) > 0) {
				if (ind - depthSize >= 0) {
					if (marchedCubes2[ind-depthSize] && vertIndexGrid2[(ind-depthSize)*12+7] != -1) {
						vertIndexGrid2[ind*12+3] = vertIndexGrid2[(ind-depthSize)*12+7];
						foundVert[3] = true;
					}
				}
				if (!foundVert[3]) {
					LinearInterp(verts[3], verts[0]);
					intVerts[3] = p;
				}
			}

			if ((edgeIndex & 16) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes2[ind-1] && vertIndexGrid2[(ind-1)*12+6] != -1) {
						vertIndexGrid2[ind*12+4] = vertIndexGrid2[(ind-1)*12+6];
						foundVert[4] = true;
					}
				}
				if (ind+depthSize <= dataLength) {
					if (marchedCubes2[ind+depthSize] && vertIndexGrid2[(ind+depthSize)*12] != -1) {
						vertIndexGrid2[ind*12+4] = vertIndexGrid2[(ind+depthSize)*12];
						foundVert[4] = true;
					}
				}
				if (!foundVert[4]) {
					LinearInterp(verts[4], verts[5]);
					intVerts[4] = p;
				}
			}
				
			if ((edgeIndex & 32) > 0) {
				if (sliceNb < ncellsY-2) {
					if (marchedCubes[ind] && vertIndexGrid[ind*12+7] != -1) {
						vertIndexGrid2[ind*12+5] = vertIndexGrid[ind*12+7];
						foundVert[5] = true;
					}
				}
				if (ind + depthSize <= dataLength) {
					if (marchedCubes2[ind+depthSize] && vertIndexGrid2[(ind+depthSize)*12+1] != -1) {
						vertIndexGrid2[ind*12+5] = vertIndexGrid2[(ind+depthSize)*12+1];
						foundVert[5] = true;
					}
				}
				if (!foundVert[5]) {
					LinearInterp(verts[5], verts[6]);
					intVerts[5] = p;
				}
			}
			
			if ((edgeIndex & 64) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes2[ind+1] && vertIndexGrid2[(ind+1)*12+4] != -1) {
						vertIndexGrid2[ind*12+6] = vertIndexGrid2[(ind+1)*12+4];
						foundVert[6] = true;
					}
				}
				if (ind+ depthSize <= dataLength) {
					if (marchedCubes2[ind+depthSize] && vertIndexGrid2[(ind+depthSize)*12+2] != -1) {
						vertIndexGrid2[ind*12+6] = vertIndexGrid2[(ind+depthSize)*12+2];
						foundVert[6] = true;
					}
				}
				if (!foundVert[6]) {
					LinearInterp(verts[6], verts[7]);
					intVerts[6] = p;
				}
			}

			if ((edgeIndex & 128) > 0) {
				if (ind+depthSize <= dataLength) {
					if (marchedCubes2[ind+depthSize] && vertIndexGrid2[(ind+depthSize)*12+3] != -1) {
						vertIndexGrid2[ind*12+7] = vertIndexGrid2[(ind+depthSize)*12+3];
						foundVert[7] = true;
					}
				}
				if (!foundVert[7]) {
					LinearInterp(verts[7], verts[4]);
					intVerts[7] = p;
				}
			}

			if ((edgeIndex & 256) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes2[ind-1] && vertIndexGrid2[(ind-1)*12+11] != -1) {
						vertIndexGrid2[ind*12+8] = vertIndexGrid2[(ind-1)*12+11];
						foundVert[8] = true;
					}
				}
				if (!foundVert[8]) {
					LinearInterp(verts[0], verts[4]);
					intVerts[8] = p;
				}
			}

			if ((edgeIndex & 512) > 0) {
				if (ind - 1 >= 0) {
					if (marchedCubes2[ind-1] && vertIndexGrid2[(ind-1)*12+10] != -1) {
						vertIndexGrid2[ind*12+9] = vertIndexGrid2[(ind-1)*12+10];
						foundVert[9] = true;
					}
				}
				if (sliceNb < ncellsY-2) {
					if (marchedCubes[ind] && vertIndexGrid[ind*12+8] != -1) {
						vertIndexGrid2[ind*12+9] = vertIndexGrid[ind*12+8];
						foundVert[9] = true;
					}
				}
				if (!foundVert[9]) {
					LinearInterp(verts[1], verts[5]);
					intVerts[9] = p;
				}
			}

			if ((edgeIndex & 1024) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes2[ind+1] && vertIndexGrid2[(ind+1)*12+9] != -1) {
						vertIndexGrid2[ind*12+10] = vertIndexGrid2[(ind+1)*12+9];
						foundVert[10] = true;
					}
				}
				if (sliceNb < ncellsY-2) {
					if (marchedCubes[ind] && vertIndexGrid[ind*12+11] != -1) {
						vertIndexGrid2[ind*12+10] = vertIndexGrid[ind*12+11];
						foundVert[10] = true;
					}
				}
				if (!foundVert[10]) {
					LinearInterp(verts[2], verts[6]);
					intVerts[10] = p;
				}
			}

			if ((edgeIndex & 2048) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes2[ind+1] && vertIndexGrid2[(ind+1)*12+8] != -1) {
						vertIndexGrid2[ind*12+11] = vertIndexGrid2[(ind+1)*12+8];
						foundVert[11] = true;
					}
				}
				if (!foundVert[11]) {
					LinearInterp(verts[3], verts[7]);
					intVerts[11] = p;
				}
			}

			for (int n = 0; MarchingCubesTables.triTable[cubeIndex, n] != -1; n += 3) {
				
				// TODO : Problem of reversing normals if density threshold < 0
				index[0] = MarchingCubesTables.triTable[cubeIndex, n];
				index[1] = MarchingCubesTables.triTable[cubeIndex, n + 1];
				index[2] = MarchingCubesTables.triTable[cubeIndex, n + 2];
				for (int h = 0; h < 3; h++)
				{
					if (vertIndexGrid2[ind*12+index[h]] != -1) {
//						Debug.Log("New triangle index : "+vertIndexGrid2[ind*12+index[h]]);
						MarchingModel.TMPtriangles[nbIndTriangles] = vertIndexGrid2[ind*12+index[h]];
					}
					else {
//						Debug.Log("Triangle index : "+numVertices);
						MarchingModel.TMPtriangles[nbIndTriangles] = numVertices;
						vertIndexGrid2[ind*12+index[h]] = numVertices;
						numVertices++;
						if (Delta.x ==2f){
							intVerts[index[h]].x -= 18;
							intVerts[index[h]].y -= 18;
							intVerts[index[h]].z -= 18;
							intVerts[index[h]].x /=Delta.x;
							intVerts[index[h]].y /=Delta.y;
							intVerts[index[h]].z /=Delta.z;
						}else{
				 				intVerts[index[h]].x *=Delta.x;
							intVerts[index[h]].y *=Delta.y;
							intVerts[index[h]].z *=Delta.z;
						}
						intVerts[index[h]].x += Origin.x;
						intVerts[index[h]].y += Origin.y;
						intVerts[index[h]].z += Origin.z;
						
						// intVerts[index[h]].x += MoleculeModel.Offset.x;//+MoleculeModel.MinValue.x;
						// intVerts[index[h]].y += MoleculeModel.Offset.y;//+MoleculeModel.MinValue.y;
						// intVerts[index[h]].z += MoleculeModel.Offset.z;//+MoleculeModel.MinValue.z;
						
//						Debug.Log("New vert : "+intVerts[index[h]]);
						MarchingModel.TMPvertices[nbIndVertices] = intVerts[index[h]];
						if(colors != null)
							MarchingModel.TMPColors[nbIndVertices] = colors[sliceNb*sliceSize+ind];
						else
							MarchingModel.TMPColors[nbIndVertices] = Color.white;
							
						nbIndVertices++;
					}
					nbIndTriangles++;		
				}
			}		
		}
	}
	
	void LinearInterp(Vector4 p1, Vector4 p2) { // linear interpolation between 2 vertex, to locate vertice position
		
		if (Math.Abs(p1.w - p2.w) > 0.00001) {
			p = p1 + (p2 - p1) / (p2.w - p1.w) * (minValue - p1.w);
//			p = (p1 + p2) / 2; // no interpolation, each vertex at the middle of each edge, funky results
		}
		else
			p = p1;
//		Debug.Log("p : "+p);
	}	
	
	void SetAllVariablesToNull () {
		points = null;
		marchedCubes = null; // list of length "dataLength", indicates if cubes already have been marched
		marchedCubes2 = null;
		queuedCubes = null;
		queuedCubes2 = null;
		vertIndexGrid = null; // list of length "dataLength", records indexes of previously created vertices, so we can build the triangle indexes list using unique vertices
		vertIndexGrid2 = null;
		MarchingModel.TMPtriangles = null; // to store mesh.triangles, before we know the size
		MarchingModel.TMPvertices = null; // to store mesh.vertices, before we know the size
		MarchingModel.Mtriangles = null; // to store final array of mesh.triangles
		MarchingModel.Mvertices = null; // to store final array of mesh.vertices
		verts = null;
		intVerts = null;
		index = null;
		GMInstance = null;
		foundVert = null;
		IndQueue = null;
		IndQueue2 = null;
//		PosQueue = null;
//		PosQueue2 = null;
//		gridPos = null;
//		gridTemp = null;
		XPosQueue = null;
		YPosQueue = null;
		ZPosQueue = null;
		XPosQueue2 = null;
		YPosQueue2 = null;
		ZPosQueue2 = null;
	}

	
	
// ------------------------------------------------------------
// Duplicated code to avoid many checks that could slow the surface creation
// Used to inverse threshold for points and cubes selection
// The only differences in the code concern the selection of points
// using minValue
// ------------------------------------------------------------
	
	
	
	void MCInitAndFindEachIndex_Rev (Vector4[] P) { // find all points inside the surface, stored in "indList"
		
//		Debug.Log ("MCInitAnd.., sliceNb : "+sliceNb);
		
		for (int j = 0; j < dataLength; j++) { // record density values for the current slice
			fullInd = sliceNb * sliceSize + j;
			points[j] = P[fullInd];	
		}
		
		for (int j = 0; j < sliceSize; j++) {
			if (points[j].w <= minValue && !queuedCubes[j]) { // queue intersecting cubes
				if ((int)points[j].x != ncellsX-1 && (int)points[j].y != ncellsY-1 && (int)points[j].z != ncellsZ-1) {
					IndQueue.Enqueue(j);
					XPosQueue.Enqueue((int)points[j].x);
					YPosQueue.Enqueue((int)points[j].y);
					ZPosQueue.Enqueue((int)points[j].z);
//					Debug.Log ("MCINit and find queued : "+(int)points[j].x+", "+(int)points[j].y+", "+(int)points[j].z);
					queuedCubes[j] = true;
				}
			}
		}
//		Debug.Log ("IndQueue.Count : "+IndQueue.Count);
	}
	
	void MCInitAndFindEachIndex_2_Rev (Vector4[] P) { // find all points inside the surface, stored in "indList"
		
//		Debug.Log ("MCInitAnd..2, sliceNb : "+sliceNb);
		
		for (int j = 0; j < dataLength; j++) {
			fullInd = sliceNb * sliceSize + j;
			points[j] = P[fullInd];
		}
		
		for (int j = 0; j < sliceSize; j++) {	
			if (points[j].w <= minValue && !queuedCubes2[j]) {
				if ((int)points[j].x != ncellsX-1 && (int)points[j].y != ncellsY-1 && (int)points[j].z != ncellsZ-1) {
					IndQueue2.Enqueue(j);
					XPosQueue2.Enqueue((int)points[j].x);
					YPosQueue2.Enqueue((int)points[j].y);
					ZPosQueue2.Enqueue((int)points[j].z);
//					Debug.Log ("MCINit and find 2 queued : "+(int)points[j].x+", "+(int)points[j].y+", "+(int)points[j].z);
					queuedCubes2[j] = true;
				}
			}
		}
//		Debug.Log ("IndQueue2.Count : "+IndQueue2.Count);
	}
	
	void MCRecReversedThres(Vector4[] P) { // proceed recursive marching cubes on yet unproceeded cubes, starting cube at index : "ind"
			
		MCInitAndFindEachIndex_Rev (P); // find all points inside the surface at first execution
		
		while (sliceNb >= 2) {
			
//			Debug.Log("IndQueue.Count : "+IndQueue.Count);
			while (IndQueue.Count != 0) {
				ind = IndQueue.Dequeue ();
				XPos = XPosQueue.Dequeue();
				YPos = YPosQueue.Dequeue();
				ZPos = ZPosQueue.Dequeue();
				
				MarchOneCube_Rev();
				
				TestFace0 ();
				TestFace2 ();
				TestFace3 ();
				TestFace4 ();
				TestFace5 ();
			}
			sliceNb --;
			
			MCInitAndFindEachIndex_2_Rev (P); // find all points inside the surface
			for (int i = 0; i < sliceSize; i++) {
				queuedCubes[i] = false;
				marchedCubes2[i] = false;
			}
			for (int i=0; i < sliceSize * 12; i++)
				vertIndexGrid2[i] = -1;
			
//			Debug.Log("IndQueue2.Count : "+IndQueue2.Count);
			while (IndQueue2.Count != 0) {
				ind = IndQueue2.Dequeue ();
				XPos = XPosQueue2.Dequeue();
				YPos = YPosQueue2.Dequeue();
				ZPos = ZPosQueue2.Dequeue();
				
				MarchOneCube_2_Rev();
				
				TestFace0_2 ();
				TestFace2_2 ();
				TestFace3_2 ();
				TestFace4_2 ();
				TestFace5_2 ();
			}
			sliceNb --;
			
			MCInitAndFindEachIndex_Rev (P); // find all points inside the surface
			for (int i = 0; i < sliceSize; i++) {
				queuedCubes2[i] = false;
				marchedCubes[i] = false;
			}
			for (int i=0; i < sliceSize * 12; i++)
				vertIndexGrid[i] = -1;
			// end of normal cycle
			
			if (sliceNb == 0) { // last step
				
				while (IndQueue.Count != 0) {
					ind = IndQueue.Dequeue ();
					XPos = XPosQueue.Dequeue();
					YPos = YPosQueue.Dequeue();
					ZPos = ZPosQueue.Dequeue();
					
					MarchOneCube_Rev();
					
					TestFace0 ();
					TestFace2 ();
					TestFace4 ();
					TestFace5 ();
				}
			}
		}
	}
	
	void MarchOneCube_Rev() { // check how to draw triangles inside current cube, referenced by its indice "ind"
		
//		Debug.LogWarning ("MarchOneCube ind : "+ind);
		
		if (nbIndVertices > 64900) { // generate several parts of ~64900 vertices if it's going further (Unity limitation at 65000 vertices per GameObject)
			
			MarchingModel.Mtriangles = new int[nbIndTriangles]; // allocate mesh triangles and vertices
			MarchingModel.Mvertices = new Vector3[nbIndVertices];
			MarchingModel.MColors = new Color[nbIndVertices];
			for (int i=0; i < nbIndTriangles; i++)
				MarchingModel.Mtriangles[i] = MarchingModel.TMPtriangles[i];
			
			for (int i=0; i < nbIndVertices; i++){
				MarchingModel.MColors[i] = MarchingModel.TMPColors[i];
				MarchingModel.Mvertices[i] = MarchingModel.TMPvertices[i];
			}
			GMInstance.GM(MarchingModel.Mvertices, MarchingModel.Mtriangles, center, surfaceNb,MarchingModel.MColors,tag); // generate display, eventually split on several objects
			
			
			
			MarchingModel.TMPtriangles=null;
			MarchingModel.TMPvertices=null;
			MarchingModel.TMPColors = null;
			MarchingModel.TMPtriangles = new int[65000*6]; // reset temporary triangles and vertices
			MarchingModel.TMPvertices = new Vector3[64950];
			MarchingModel.TMPColors = new Color[64950];
			surfaceNb++;
			nbIndVertices = 0;
			nbIndTriangles = 0;
			numVertices = 0;
			for (int i=0; i < sliceSize * 12; i++){
				vertIndexGrid[i] = -1;
				vertIndexGrid2[i] = -1;
			}
		}
		
		for (int i=0; i < 12; i++)
			foundVert[i] = false;
		
		verts[0] = points[ind]; // list values of current cube's vertex (cube at index : "ind")
		verts[1] = points[ind + sliceSize];
		verts[2] = points[ind + sliceSize + 1];
		verts[3] = points[ind + 1];
		verts[4] = points[ind + depthSize];
		verts[5] = points[ind + sliceSize + depthSize];
		verts[6] = points[ind + sliceSize + depthSize + 1];
		verts[7] = points[ind + depthSize + 1];
		
		cubeIndex = 0;
		marchedCubes[ind] = true; // make sure this cube is listed as already marched
		
//		for (int i=ind*12; i < ind*12+12; i++)
//			vertIndexGrid[i] = -1;
		
		for (int n = 0; n < 8; n++)
//			if ((verts[n].w >= minValue - toleranceValue) && (verts[n].w <= minValue + toleranceValue))
			if (verts[n].w <= minValue)
				cubeIndex |= (1 << n);
		
//		Debug.Log("cubeIndex : "+cubeIndex);
		if (cubeIndex != 0 && cubeIndex != 255) { // if this cube intersects the surface

			edgeIndex = MarchingCubesTables.edgeTable[cubeIndex];
			
			if ((edgeIndex & 1) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes[ind-1] && vertIndexGrid[(ind-1)*12+2] != -1) {
						vertIndexGrid[ind*12] = vertIndexGrid[(ind-1)*12+2];
						foundVert[0] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes[ind-depthSize] && vertIndexGrid[(ind-depthSize)*12+4] != -1) {
						vertIndexGrid[ind*12] = vertIndexGrid[(ind-depthSize)*12+4];
						foundVert[0] = true;
					}
				}
				if (!foundVert[0]) {
					LinearInterp(verts[0], verts[1]);
					intVerts[0] = p;
				}
			}

			if ((edgeIndex & 2) > 0) {
				if (sliceNb < ncellsY-2) {
					if (marchedCubes2[ind] && vertIndexGrid2[ind*12+3] != - 1) {
						vertIndexGrid[ind*12+1] = vertIndexGrid2[ind*12+3];
						foundVert[1] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes[ind-depthSize] && vertIndexGrid[(ind-depthSize)*12+5] != -1) {
						vertIndexGrid[ind*12+1] = vertIndexGrid[(ind-depthSize)*12+5];
						foundVert[1] = true;
					}
				}
				if (!foundVert[1]) {
					LinearInterp(verts[1], verts[2]);
					intVerts[1] = p;
				}
			}

			if ((edgeIndex & 4) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes[ind+1] && vertIndexGrid[(ind+1)*12] != -1) {
						vertIndexGrid[ind*12+2] = vertIndexGrid[(ind+1)*12];
						foundVert[2] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes[ind-depthSize] && vertIndexGrid[(ind-depthSize)*12+6] != -1) {
						vertIndexGrid[ind*12+2] = vertIndexGrid[(ind-depthSize)*12+6];
						foundVert[2] = true;
					}
				}
				if (!foundVert[2]) {
					LinearInterp(verts[2], verts[3]);
					intVerts[2] = p;
				}
			}

			if ((edgeIndex & 8) > 0) {
				if (ind - depthSize >= 0) {
					if (marchedCubes[ind-depthSize] && vertIndexGrid[(ind-depthSize)*12+7] != -1) {
						vertIndexGrid[ind*12+3] = vertIndexGrid[(ind-depthSize)*12+7];
						foundVert[3] = true;
					}
				}
				if (!foundVert[3]) {
					LinearInterp(verts[3], verts[0]);
					intVerts[3] = p;
				}
			}

			if ((edgeIndex & 16) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes[ind-1] && vertIndexGrid[(ind-1)*12+6] != -1) {
						vertIndexGrid[ind*12+4] = vertIndexGrid[(ind-1)*12+6];
						foundVert[4] = true;
					}
				}
				if (ind+depthSize <= dataLength) {
					if (marchedCubes[ind+depthSize] && vertIndexGrid[(ind+depthSize)*12] != -1) {
						vertIndexGrid[ind*12+4] = vertIndexGrid[(ind+depthSize)*12];
						foundVert[4] = true;
					}
				}
				if (!foundVert[4]) {
					LinearInterp(verts[4], verts[5]);
					intVerts[4] = p;
				}
			}
				
			if ((edgeIndex & 32) > 0) {
				if (sliceNb < ncellsY-2) {
					if (marchedCubes2[ind] && vertIndexGrid2[ind*12+7] != -1) {
						vertIndexGrid[ind*12+5] = vertIndexGrid2[ind*12+7];
						foundVert[5] = true;
					}
				}
				if (ind + depthSize <= dataLength) {
					if (marchedCubes[ind+depthSize] && vertIndexGrid[(ind+depthSize)*12+1] != -1) {
						vertIndexGrid[ind*12+5] = vertIndexGrid[(ind+depthSize)*12+1];
						foundVert[5] = true;
					}
				}
				if (!foundVert[5]) {
					LinearInterp(verts[5], verts[6]);
					intVerts[5] = p;
				}
			}
			
			if ((edgeIndex & 64) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes[ind+1] && vertIndexGrid[(ind+1)*12+4] != -1) {
						vertIndexGrid[ind*12+6] = vertIndexGrid[(ind+1)*12+4];
						foundVert[6] = true;
					}
				}
				if (ind+ depthSize <= dataLength) {
					if (marchedCubes[ind+depthSize] && vertIndexGrid[(ind+depthSize)*12+2] != -1) {
						vertIndexGrid[ind*12+6] = vertIndexGrid[(ind+depthSize)*12+2];
						foundVert[6] = true;
					}
				}
				if (!foundVert[6]) {
					LinearInterp(verts[6], verts[7]);
					intVerts[6] = p;
				}
			}

			if ((edgeIndex & 128) > 0) {
				if (ind+depthSize <= dataLength) {
					if (marchedCubes[ind+depthSize] && vertIndexGrid[(ind+depthSize)*12+3] != -1) {
						vertIndexGrid[ind*12+7] = vertIndexGrid[(ind+depthSize)*12+3];
						foundVert[7] = true;
					}
				}
				if (!foundVert[7]) {
					LinearInterp(verts[7], verts[4]);
					intVerts[7] = p;
				}
			}

			if ((edgeIndex & 256) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes[ind-1] && vertIndexGrid[(ind-1)*12+11] != -1) {
						vertIndexGrid[ind*12+8] = vertIndexGrid[(ind-1)*12+11];
						foundVert[8] = true;
					}
				}
				if (!foundVert[8]) {
					LinearInterp(verts[0], verts[4]);
					intVerts[8] = p;
				}
			}

			if ((edgeIndex & 512) > 0) {
				if (ind - 1 >= 0) {
					if (marchedCubes[ind-1] && vertIndexGrid[(ind-1)*12+10] != -1) {
						vertIndexGrid[ind*12+9] = vertIndexGrid[(ind-1)*12+10];
						foundVert[9] = true;
					}
				}
				if (sliceNb < ncellsY-2) {
					if (marchedCubes2[ind] && vertIndexGrid2[ind*12+8] != -1) {
						vertIndexGrid[ind*12+9] = vertIndexGrid2[ind*12+8];
						foundVert[9] = true;
					}
				}
				if (!foundVert[9]) {
					LinearInterp(verts[1], verts[5]);
					intVerts[9] = p;
				}
			}

			if ((edgeIndex & 1024) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes[ind+1] && vertIndexGrid[(ind+1)*12+9] != -1) {
						vertIndexGrid[ind*12+10] = vertIndexGrid[(ind+1)*12+9];
						foundVert[10] = true;
					}
				}
				if (sliceNb < ncellsY-2) {
					if (marchedCubes2[ind] && vertIndexGrid2[ind*12+11] != -1) {
						vertIndexGrid[ind*12+10] = vertIndexGrid2[ind*12+11];
						foundVert[10] = true;
					}
				}
				if (!foundVert[10]) {
					LinearInterp(verts[2], verts[6]);
					intVerts[10] = p;
				}
			}

			if ((edgeIndex & 2048) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes[ind+1] && vertIndexGrid[(ind+1)*12+8] != -1) {
						vertIndexGrid[ind*12+11] = vertIndexGrid[(ind+1)*12+8];
						foundVert[11] = true;
					}
				}
				if (!foundVert[11]) {
					LinearInterp(verts[3], verts[7]);
					intVerts[11] = p;
				}
			}

			for (int n = 0; MarchingCubesTables.triTable[cubeIndex, n] != -1; n += 3) {
				
				// TODO : Problem of reversing normals if density threshold < 0
				index[0] = MarchingCubesTables.triTable[cubeIndex, n];
				index[1] = MarchingCubesTables.triTable[cubeIndex, n + 1];
				index[2] = MarchingCubesTables.triTable[cubeIndex, n + 2];
				for (int h = 0; h < 3; h++)
				{
//					Debug.Log("check -1 : "+vertIndexGrid[ind*12+index[h]]);
					if (vertIndexGrid[ind*12+index[h]] != -1) {
//						Debug.Log("New triangle index : "+vertIndexGrid[ind*12+index[h]]);
//						Debug.Log("nbIndTriangles : "+nbIndTriangles);
						MarchingModel.TMPtriangles[nbIndTriangles] = vertIndexGrid[ind*12+index[h]];
					}
					else {
//						Debug.Log("Triangle index : "+numVertices);
						MarchingModel.TMPtriangles[nbIndTriangles] = numVertices;
						vertIndexGrid[ind*12+index[h]] = numVertices;
						numVertices++;
						if (Delta.x ==2f){
							intVerts[index[h]].x -= 18;
							intVerts[index[h]].y -= 18;
							intVerts[index[h]].z -= 18;
							intVerts[index[h]].x /=Delta.x;
							intVerts[index[h]].y /=Delta.y;
							intVerts[index[h]].z /=Delta.z;
						}else{
				 			intVerts[index[h]].x *=Delta.x;
							intVerts[index[h]].y *=Delta.y;
							intVerts[index[h]].z *=Delta.z;
						}
						intVerts[index[h]].x += Origin.x;
						intVerts[index[h]].y += Origin.y;
						intVerts[index[h]].z += Origin.z;
						
						
						// intVerts[index[h]].x += MoleculeModel.Offset.x;//+MoleculeModel.MinValue.x;
						// intVerts[index[h]].y += MoleculeModel.Offset.y;//+MoleculeModel.MinValue.y;
						// intVerts[index[h]].z += MoleculeModel.Offset.z;//+MoleculeModel.MinValue.z;
//						Debug.Log("New vert : "+intVerts[index[h]]);
						MarchingModel.TMPvertices[nbIndVertices] = intVerts[index[h]];
						
						if(colors != null)
							MarchingModel.TMPColors[nbIndVertices] = colors[sliceNb*sliceSize+ind];
						else
							MarchingModel.TMPColors[nbIndVertices] = Color.white;
						nbIndVertices++;
					}
					nbIndTriangles++;
				}
			}		
		}
	}
	
	void MarchOneCube_2_Rev() { // check how to draw triangles inside current cube, referenced by its indice "ind"
		
//		Debug.LogWarning ("MarchOneCube_2 ind : "+ind+", gridPos : "+XPos+", "+YPos+", "+ZPos);
		
		if (nbIndVertices > 64900) { // generate several parts of ~64900 vertices if it's going further (Unity limitation at 65000 vertices per GameObject)
			
			MarchingModel.Mtriangles = new int[nbIndTriangles]; // allocate mesh triangles and vertices
			MarchingModel.Mvertices = new Vector3[nbIndVertices];
			MarchingModel.MColors = new Color[nbIndVertices];
			for (int i=0; i < nbIndTriangles; i++)
				MarchingModel.Mtriangles[i] = MarchingModel.TMPtriangles[i];
			
			for (int i=0; i < nbIndVertices; i++){
				MarchingModel.Mvertices[i] = MarchingModel.TMPvertices[i];
				MarchingModel.MColors[i]=MarchingModel.TMPColors[i];
			}
			GMInstance.GM(MarchingModel.Mvertices, MarchingModel.Mtriangles, center, surfaceNb,MarchingModel.MColors,tag); // generate display, eventually split on several objects
			
			MarchingModel.TMPtriangles = null; // reset temporary triangles and vertices
			MarchingModel.TMPvertices = null;
			MarchingModel.TMPColors = null;
			MarchingModel.TMPtriangles = new int[65000*6]; // reset temporary triangles and vertices
			MarchingModel.TMPvertices = new Vector3[64950];
			MarchingModel.TMPColors = new Color[64950];
			surfaceNb++;
			nbIndVertices = 0;
			nbIndTriangles = 0;
			numVertices = 0;
			for (int i=0; i < sliceSize * 12; i++) {
				vertIndexGrid2[i] = -1;
				vertIndexGrid[i] = -1;
			}
		}
		
		for (int i=0; i < 12; i++)
			foundVert[i] = false;
		
		verts[0] = points[ind]; // list values of current cube's vertex (cube at index : "ind")
		verts[1] = points[ind + sliceSize];
		verts[2] = points[ind + sliceSize + 1];
		verts[3] = points[ind + 1];
		verts[4] = points[ind + depthSize];
		verts[5] = points[ind + sliceSize + depthSize];
		verts[6] = points[ind + sliceSize + depthSize + 1];
		verts[7] = points[ind + depthSize + 1];
		
		cubeIndex = 0;
		marchedCubes2[ind] = true; // make sure this cube is listed as already marched
		
//		for (int i=ind*12; i < ind*12+12; i++)
//			vertIndexGrid[i] = -1;
		
		for (int n = 0; n < 8; n++)
//			if ((verts[n].w >= minValue - toleranceValue) && (verts[n].w <= minValue + toleranceValue))
			if (verts[n].w <= minValue)
				cubeIndex |= (1 << n);
		
//		Debug.Log("cubeIndex : "+cubeIndex);
		if (cubeIndex != 0 && cubeIndex != 255) { // if this cube intersects the surface

			edgeIndex = MarchingCubesTables.edgeTable[cubeIndex];
			
			if ((edgeIndex & 1) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes2[ind-1] && vertIndexGrid2[(ind-1)*12+2] != -1) {
						vertIndexGrid2[ind*12] = vertIndexGrid2[(ind-1)*12+2];
						foundVert[0] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes2[ind-depthSize] && vertIndexGrid2[(ind-depthSize)*12+4] != -1) {
						vertIndexGrid2[ind*12] = vertIndexGrid2[(ind-depthSize)*12+4];
						foundVert[0] = true;
					}
				}
				if (!foundVert[0]) {
					LinearInterp(verts[0], verts[1]);
					intVerts[0] = p;
				}
			}

			if ((edgeIndex & 2) > 0) {
				if (sliceNb < ncellsY-2) {
					if (marchedCubes[ind] && vertIndexGrid[ind*12+3] != - 1) {
						vertIndexGrid2[ind*12+1] = vertIndexGrid[ind*12+3];
						foundVert[1] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes2[ind-depthSize] && vertIndexGrid2[(ind-depthSize)*12+5] != -1) {
						vertIndexGrid2[ind*12+1] = vertIndexGrid2[(ind-depthSize)*12+5];
						foundVert[1] = true;
					}
				}
				if (!foundVert[1]) {
					LinearInterp(verts[1], verts[2]);
					intVerts[1] = p;
				}
			}

			if ((edgeIndex & 4) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes2[ind+1] && vertIndexGrid2[(ind+1)*12] != -1) {
						vertIndexGrid2[ind*12+2] = vertIndexGrid2[(ind+1)*12];
						foundVert[2] = true;
					}
				}
				if (ind-depthSize >= 0) {
					if (marchedCubes2[ind-depthSize] && vertIndexGrid2[(ind-depthSize)*12+6] != -1) {
						vertIndexGrid2[ind*12+2] = vertIndexGrid2[(ind-depthSize)*12+6];
						foundVert[2] = true;
					}
				}
				if (!foundVert[2]) {
					LinearInterp(verts[2], verts[3]);
					intVerts[2] = p;
				}
			}

			if ((edgeIndex & 8) > 0) {
				if (ind - depthSize >= 0) {
					if (marchedCubes2[ind-depthSize] && vertIndexGrid2[(ind-depthSize)*12+7] != -1) {
						vertIndexGrid2[ind*12+3] = vertIndexGrid2[(ind-depthSize)*12+7];
						foundVert[3] = true;
					}
				}
				if (!foundVert[3]) {
					LinearInterp(verts[3], verts[0]);
					intVerts[3] = p;
				}
			}

			if ((edgeIndex & 16) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes2[ind-1] && vertIndexGrid2[(ind-1)*12+6] != -1) {
						vertIndexGrid2[ind*12+4] = vertIndexGrid2[(ind-1)*12+6];
						foundVert[4] = true;
					}
				}
				if (ind+depthSize <= dataLength) {
					if (marchedCubes2[ind+depthSize] && vertIndexGrid2[(ind+depthSize)*12] != -1) {
						vertIndexGrid2[ind*12+4] = vertIndexGrid2[(ind+depthSize)*12];
						foundVert[4] = true;
					}
				}
				if (!foundVert[4]) {
					LinearInterp(verts[4], verts[5]);
					intVerts[4] = p;
				}
			}
				
			if ((edgeIndex & 32) > 0) {
				if (sliceNb < ncellsY-2) {
					if (marchedCubes[ind] && vertIndexGrid[ind*12+7] != -1) {
						vertIndexGrid2[ind*12+5] = vertIndexGrid[ind*12+7];
						foundVert[5] = true;
					}
				}
				if (ind + depthSize <= dataLength) {
					if (marchedCubes2[ind+depthSize] && vertIndexGrid2[(ind+depthSize)*12+1] != -1) {
						vertIndexGrid2[ind*12+5] = vertIndexGrid2[(ind+depthSize)*12+1];
						foundVert[5] = true;
					}
				}
				if (!foundVert[5]) {
					LinearInterp(verts[5], verts[6]);
					intVerts[5] = p;
				}
			}
			
			if ((edgeIndex & 64) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes2[ind+1] && vertIndexGrid2[(ind+1)*12+4] != -1) {
						vertIndexGrid2[ind*12+6] = vertIndexGrid2[(ind+1)*12+4];
						foundVert[6] = true;
					}
				}
				if (ind+ depthSize <= dataLength) {
					if (marchedCubes2[ind+depthSize] && vertIndexGrid2[(ind+depthSize)*12+2] != -1) {
						vertIndexGrid2[ind*12+6] = vertIndexGrid2[(ind+depthSize)*12+2];
						foundVert[6] = true;
					}
				}
				if (!foundVert[6]) {
					LinearInterp(verts[6], verts[7]);
					intVerts[6] = p;
				}
			}

			if ((edgeIndex & 128) > 0) {
				if (ind+depthSize <= dataLength) {
					if (marchedCubes2[ind+depthSize] && vertIndexGrid2[(ind+depthSize)*12+3] != -1) {
						vertIndexGrid2[ind*12+7] = vertIndexGrid2[(ind+depthSize)*12+3];
						foundVert[7] = true;
					}
				}
				if (!foundVert[7]) {
					LinearInterp(verts[7], verts[4]);
					intVerts[7] = p;
				}
			}

			if ((edgeIndex & 256) > 0) {
				if (ind-1 >= 0) {
					if (marchedCubes2[ind-1] && vertIndexGrid2[(ind-1)*12+11] != -1) {
						vertIndexGrid2[ind*12+8] = vertIndexGrid2[(ind-1)*12+11];
						foundVert[8] = true;
					}
				}
				if (!foundVert[8]) {
					LinearInterp(verts[0], verts[4]);
					intVerts[8] = p;
				}
			}

			if ((edgeIndex & 512) > 0) {
				if (ind - 1 >= 0) {
					if (marchedCubes2[ind-1] && vertIndexGrid2[(ind-1)*12+10] != -1) {
						vertIndexGrid2[ind*12+9] = vertIndexGrid2[(ind-1)*12+10];
						foundVert[9] = true;
					}
				}
				if (sliceNb < ncellsY-2) {
					if (marchedCubes[ind] && vertIndexGrid[ind*12+8] != -1) {
						vertIndexGrid2[ind*12+9] = vertIndexGrid[ind*12+8];
						foundVert[9] = true;
					}
				}
				if (!foundVert[9]) {
					LinearInterp(verts[1], verts[5]);
					intVerts[9] = p;
				}
			}

			if ((edgeIndex & 1024) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes2[ind+1] && vertIndexGrid2[(ind+1)*12+9] != -1) {
						vertIndexGrid2[ind*12+10] = vertIndexGrid2[(ind+1)*12+9];
						foundVert[10] = true;
					}
				}
				if (sliceNb < ncellsY-2) {
					if (marchedCubes[ind] && vertIndexGrid[ind*12+11] != -1) {
						vertIndexGrid2[ind*12+10] = vertIndexGrid[ind*12+11];
						foundVert[10] = true;
					}
				}
				if (!foundVert[10]) {
					LinearInterp(verts[2], verts[6]);
					intVerts[10] = p;
				}
			}

			if ((edgeIndex & 2048) > 0) {
				if (ind+1 <= dataLength) {
					if (marchedCubes2[ind+1] && vertIndexGrid2[(ind+1)*12+8] != -1) {
						vertIndexGrid2[ind*12+11] = vertIndexGrid2[(ind+1)*12+8];
						foundVert[11] = true;
					}
				}
				if (!foundVert[11]) {
					LinearInterp(verts[3], verts[7]);
					intVerts[11] = p;
				}
			}

			for (int n = 0; MarchingCubesTables.triTable[cubeIndex, n] != -1; n += 3) {
				
				// TODO : Problem of reversing normals if density threshold < 0
				index[0] = MarchingCubesTables.triTable[cubeIndex, n];
				index[1] = MarchingCubesTables.triTable[cubeIndex, n + 1];
				index[2] = MarchingCubesTables.triTable[cubeIndex, n + 2];
			for (int h = 0; h < 3; h++)
				{
					if (vertIndexGrid2[ind*12+index[h]] != -1) {
//						Debug.Log("New triangle index : "+vertIndexGrid2[ind*12+index[h]]);
						MarchingModel.TMPtriangles[nbIndTriangles] = vertIndexGrid2[ind*12+index[h]];
					}
					else {
//						Debug.Log("Triangle index : "+numVertices);
						MarchingModel.TMPtriangles[nbIndTriangles] = numVertices;
						vertIndexGrid2[ind*12+index[h]] = numVertices;
						numVertices++;
						if (Delta.x ==2f){
							intVerts[index[h]].x -= 18;
							intVerts[index[h]].y -= 18;
							intVerts[index[h]].z -= 18;
							intVerts[index[h]].x /=Delta.x;
							intVerts[index[h]].y /=Delta.y;
							intVerts[index[h]].z /=Delta.z;
						}else{
				 			intVerts[index[h]].x *=Delta.x;
							intVerts[index[h]].y *=Delta.y;
							intVerts[index[h]].z *=Delta.z;
						}
						
						intVerts[index[h]].x += Origin.x;
						intVerts[index[h]].y += Origin.y;
						intVerts[index[h]].z += Origin.z;
						
						
						// intVerts[index[h]].x += MoleculeModel.Offset.x;//+MoleculeModel.MinValue.x;
						// intVerts[index[h]].y += MoleculeModel.Offset.y;//+MoleculeModel.MinValue.y;
						// intVerts[index[h]].z += MoleculeModel.Offset.z;//+MoleculeModel.MinValue.z;
						
//						Debug.Log("New vert : "+intVerts[index[h]]);
						MarchingModel.TMPvertices[nbIndVertices] = intVerts[index[h]];
						if(colors != null)
							MarchingModel.TMPColors[nbIndVertices] = Color.black;//colors[sliceNb*sliceSize+ind];
						else
							MarchingModel.TMPColors[nbIndVertices] = Color.white;
						nbIndVertices++;
					}
					nbIndTriangles++;	
				}
			}		
		}
	}
	
}
