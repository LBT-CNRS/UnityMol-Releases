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
using System.Linq;

namespace UMol {
public class RibbonMesh {

	public static int splineSteps = 16;
	public static int profileDetail = 8;

	public static int trajSplineSteps = 4;
	public static int trajProfileDetail = 4;

	public static bool testDistanceCA_CA = true;

	public static float ribbonWidth = 2.0f;
	public static float ribbonHeight = 0.125f;
	public static float ribbonOffset = 1.5f;
	public static float arrowHeadWidth = 3.0f;
	public static float arrowWidth = 2.0f;
	public static float arrowHeight = 0.5f;
	public static float tubeSize = 0.25f;

	public static string helixCol = "#e69f00";//Colorblind ready
	public static string helixPi = "#fae442";
	public static string helix310 = "#d55e00";
	public static string helixOther = "#cc79a7";

	public static string strandCol = "#0072b2";
	public static string coilCol = "#9b9b9b";


	private static float[] powersOfTen = {1e0f, 1e1f, 1e2f, 1e3f, 1e4f, 1e5f, 1e6f,
	                                      1e7f, 1e8f, 1e9f, 1e10f, 1e11f, 1e12f, 1e13f, 1e14f, 1e15f, 1e16f
	                                     };

    public static bool useFastMethod = true;


public static MeshData createChainMesh(List<UnityMolResidue> residues,
	                                       ref Dictionary<UnityMolResidue, List<int>> residueToVert, bool isTraj = false)  {

		if(useFastMethod){
			//WARNING Not published yet
		}

		PeptidePlane[] planes = new PeptidePlane[residues.Count + 3];
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Color32> colors = new List<Color32>();


		int nbPlane = 0;
        for (int i = -2; i <= residues.Count; i++) {

            int id = Mathf.Clamp(i, 0, residues.Count - 1);
            int id1 = Mathf.Clamp(i + 1, 0, residues.Count - 1);
            int id2 = Mathf.Clamp(i + 2, 0, residues.Count - 1);

			UnityMolResidue r1 = residues[id];
			UnityMolResidue r2 = residues[id1];
			UnityMolResidue r3 = residues[id2];
			PeptidePlane plane = new PeptidePlane(r1, r2, r3);


			if (plane == null || plane.r1 == null) {
				continue;
			}
			//Make sure to start at the first CA position
			if (i <= 0) {
				plane.position = r1.atoms["CA"].position;
				plane.position.x = -r1.atoms["CA"].position.x;
			}
			//Make sure to end at the last CA position
			if (i >= residues.Count - 2) {
				plane.position = r3.atoms["CA"].position;
				plane.position.x = -r3.atoms["CA"].position.x;
			}

			if (plane != null && plane.r1 != null) {
				// TODO: better handling missing required atoms
				planes[nbPlane++] = plane;
			}
		}
		Vector3 previous = Vector3.zero;

		for (int i = 0; i < nbPlane; i++) {
			PeptidePlane p = planes[i];
			if (i > 0 && Vector3.Dot(p.side, previous) < 0.0f) {
				p.Flip();
			}
			previous = p.side;
		}

		Dictionary<Vector3, int> verticesDict = new Dictionary<Vector3, int>();
		int n = nbPlane - 3;

		PeptidePlane pp1 = null;
		PeptidePlane pp2 = null;
		PeptidePlane pp3 = null;
		PeptidePlane pp4 = null;

		for (int i = 0; i < n; i++) {
			pp1 = planes[i];
			pp2 = planes[i + 1];
			pp3 = planes[i + 2];
			pp4 = planes[i + 3];

			if (discontinuity(pp1, pp2, pp3, pp4)) {
				//Discontinuity
				continue;
			}
			if(testDistanceCA_CA){
				if (!checkDistanceCA_CA(pp1)) {
					continue;
				}
				if (!checkDistanceCA_CA(pp2)) {
					continue;
				}
				if (!checkDistanceCA_CA(pp3)) {
					continue;
				}
				if (!checkDistanceCA_CA(pp4)) {
					continue;
				}
			}
			createSegmentMesh(i, n, pp1, pp2, pp3, pp4, ref vertices, ref triangles,
			                  ref colors, ref residueToVert, ref verticesDict, isTraj);
		}

		MeshData mesh = new MeshData();
		mesh.triangles = triangles.ToArray();
		mesh.vertices = vertices.ToArray();
		mesh.colors = colors.ToArray();

		return mesh;
	}

	static Vector3[] ellipseProfile(int n, float w, float h) {
		Vector3[] result = new Vector3[n];
		for (int i = 0; i < n; i++) {
			float t = i / (float)n;
			float a = t * 2.0f * Mathf.PI + Mathf.PI / 4.0f;
			float x = Mathf.Cos(a) * w / 2.0f;
			float y = Mathf.Sin(a) * h / 2.0f;
			result[i] = new Vector3(x, y, 0.0f);
		}
		return result;
	}


	static Vector3[] rectangleProfile(int n, float w, float h) {

		Vector3[] result = new Vector3[n];
		float hw = w / 2.0f;
		float hh = h / 2.0f;
		Vector3[,] segments = new Vector3[4, 2];
		segments[0, 0] = new Vector3(hw,   hh, 0.0f);
		segments[0, 1] = new Vector3(-hw,  hh, 0.0f);

		segments[1, 0] = new Vector3(-hw,  hh, 0.0f);
		segments[1, 1] = new Vector3(-hw, -hh, 0.0f);

		segments[2, 0] = new Vector3(-hw, -hh, 0.0f);
		segments[2, 1] = new Vector3(hw,  -hh, 0.0f);

		segments[3, 0] = new Vector3(hw,  -hh, 0.0f);
		segments[3, 1] = new Vector3(hw,   hh, 0.0f);

		int m = n / 4;
		int cpt = 0;
		for (int a = 0; a < 4; a++) {
			for (int i = 0; i < m; i++) {
				float t = (float)i / (float)m;
				Vector3 p = Vector3.Lerp(segments[a, 0], segments[a, 1], t);
				result[cpt++] = p;

			}
		}
		return result;
	}



	static Vector3[] roundedRectangleProfile(int n, float w, float h) {

		Vector3[] result = new Vector3[n];

		float r = h / 2.0f;
		float hw = w / 2.0f - r;
		float hh = h / 2.0f;

		Vector3[,] segments = new Vector3[4, 2];

		segments[0, 0] = new Vector3(hw, hh, 0);
		segments[0, 1] = new Vector3(-hw, hh, 0);
		segments[1, 0] = new Vector3(-hw, 0, 0);
		segments[1, 1] = Vector3.zero;
		segments[2, 0] = new Vector3(-hw, -hh, 0);
		segments[2, 1] = new Vector3(hw, -hh, 0);
		segments[3, 0] = new Vector3(hw, 0, 0);
		segments[3, 1] = Vector3.zero;


		int m = n / 4;
		int cpt = 0;
		for (int si = 0; si < 4; si++) {
			for (int i = 0; i < m; i++) {
				float t = (float)i / (float)m;
				Vector3 p = Vector3.zero;
				if ( si == 0 || si == 2) {
					p = Vector3.Lerp(segments[si, 0], segments[si, 1], t);
				}
				else if (si == 1) {
					float a = Mathf.PI / 2.0f + Mathf.PI * t;
					float x = Mathf.Cos(a) * r;
					float y = Mathf.Sin(a) * r;
					p = segments[si, 0] + new Vector3(x, y, 0.0f);
				}
				else if (si == 3) {
					float a = 3 * Mathf.PI / 2 + Mathf.PI * t;
					float x = Mathf.Cos(a) * r;
					float y = Mathf.Sin(a) * r;
					p = segments[si, 0] + new Vector3(x, y, 0.0f);
				}
				result[cpt++] = p;

			}
		}
		return result;
	}


	static void scaleProfile(ref Vector3[] p, float s) {

		for (int i = 0; i < p.Length; i++) {
			p[i] = p[i] * s;
		}
	}

	static void translateProfile(ref Vector3[] p, float dx, float dy) {

		Vector3 dp = new Vector3(dx, dy, 0.0f);
		for (int i = 0; i < p.Length; i++) {
			p[i] = p[i] + dp;
		}
	}



	static void segmentProfiles(PeptidePlane pp1, PeptidePlane pp2, int n,
	                            ref Vector3[] p1, ref Vector3[] p2) {
		UnityMolResidue.secondaryStructureType type0 = pp1.r1.secondaryStructure;
		UnityMolResidue.secondaryStructureType type1 = 0;
		UnityMolResidue.secondaryStructureType type2 = 0;
		pp1.Transition(ref type1, ref type2);

		float offset1 = ribbonOffset;
		float offset2 = ribbonOffset;

		if (pp1.flipped) {
			offset1 = -offset1;
		}
		if (pp2.flipped) {
			offset2 = -offset2;
		}

		switch (type1) {
		case UnityMolResidue.secondaryStructureType.Helix:
		case UnityMolResidue.secondaryStructureType.HelixRightOmega:
		case UnityMolResidue.secondaryStructureType.Helix310:
		case UnityMolResidue.secondaryStructureType.HelixRightGamma:
		case UnityMolResidue.secondaryStructureType.HelixRightPi:
			if (type0 == UnityMolResidue.secondaryStructureType.Strand) {
				p1 = roundedRectangleProfile(n, 0.0f, 0.0f);
			}
			else {
				p1 = roundedRectangleProfile(n, ribbonWidth, ribbonHeight);
			}
			translateProfile(ref p1, 0.0f, offset1);
			break;
		case UnityMolResidue.secondaryStructureType.Strand:
			if (type2 == UnityMolResidue.secondaryStructureType.Strand) {
				p1 = rectangleProfile(n, arrowWidth, arrowHeight);
			}
			else {
				p1 = rectangleProfile(n, arrowHeadWidth, arrowHeight);
			}
			break;
		default:
			if (type0 == UnityMolResidue.secondaryStructureType.Strand) {
				p1 = ellipseProfile(n, 0.0f, 0.0f);
			}
			else {
				p1 = ellipseProfile(n, tubeSize, tubeSize);
			}
			break;
		}
		switch (type2) {
		case UnityMolResidue.secondaryStructureType.Helix:
		case UnityMolResidue.secondaryStructureType.Helix310:
		case UnityMolResidue.secondaryStructureType.HelixRightOmega:
		case UnityMolResidue.secondaryStructureType.HelixRightPi:
		case UnityMolResidue.secondaryStructureType.HelixRightGamma:
			p2 = roundedRectangleProfile(n, ribbonWidth, ribbonHeight);
			translateProfile(ref p2, 0.0f, offset2);
			break;
		case UnityMolResidue.secondaryStructureType.Strand:
			p2 = rectangleProfile(n, arrowWidth, arrowHeight);
			break;
		default:
			p2 = ellipseProfile(n, tubeSize, tubeSize);
			break;
		}
		if (type1 == UnityMolResidue.secondaryStructureType.Strand && type2 != UnityMolResidue.secondaryStructureType.Strand) {
			p2 = rectangleProfile(n, 0.0f, arrowHeight);
		}
	}

	static void segmentColors(PeptidePlane pp, ref Color32 c1, ref Color32 c2) {


		UnityMolResidue.secondaryStructureType type1 = 0;
		UnityMolResidue.secondaryStructureType type2 = 0;
		pp.Transition(ref type1, ref type2);

		Color col1;
		Color col2;
		switch (type1) {

		case UnityMolResidue.secondaryStructureType.HelixRightOmega:
		case UnityMolResidue.secondaryStructureType.HelixRightGamma:
		case UnityMolResidue.secondaryStructureType.HelixLeftAlpha:
		case UnityMolResidue.secondaryStructureType.HelixLeftOmega:
		case UnityMolResidue.secondaryStructureType.HelixLeftGamma:
		case UnityMolResidue.secondaryStructureType.PolyProline:
		case UnityMolResidue.secondaryStructureType.Helix27:
			ColorUtility.TryParseHtmlString(helixOther, out col1);
			break;
		case UnityMolResidue.secondaryStructureType.HelixRightPi:
			ColorUtility.TryParseHtmlString(helixPi, out col1);
			break;
		case UnityMolResidue.secondaryStructureType.Helix310:
			ColorUtility.TryParseHtmlString(helix310, out col1);
			break;
		case UnityMolResidue.secondaryStructureType.Helix:
			ColorUtility.TryParseHtmlString(helixCol, out col1);
			break;
		case UnityMolResidue.secondaryStructureType.Strand:
			ColorUtility.TryParseHtmlString(strandCol, out col1);
			break;
		default:
			ColorUtility.TryParseHtmlString(coilCol, out col1);
			break;
		}
		switch (type2) {

		case UnityMolResidue.secondaryStructureType.HelixRightOmega:
		case UnityMolResidue.secondaryStructureType.HelixRightGamma:
		case UnityMolResidue.secondaryStructureType.HelixLeftAlpha:
		case UnityMolResidue.secondaryStructureType.HelixLeftOmega:
		case UnityMolResidue.secondaryStructureType.HelixLeftGamma:
		case UnityMolResidue.secondaryStructureType.PolyProline:
		case UnityMolResidue.secondaryStructureType.Helix27:
			ColorUtility.TryParseHtmlString(helixOther, out col2);
			break;
		case UnityMolResidue.secondaryStructureType.HelixRightPi:
			ColorUtility.TryParseHtmlString(helixPi, out col2);
			break;
		case UnityMolResidue.secondaryStructureType.Helix310:
			ColorUtility.TryParseHtmlString(helix310, out col2);
			break;
		case UnityMolResidue.secondaryStructureType.Helix:
			ColorUtility.TryParseHtmlString(helixCol, out col2);
			break;
		case UnityMolResidue.secondaryStructureType.Strand:
			ColorUtility.TryParseHtmlString(strandCol, out col2);
			break;
		default:
			ColorUtility.TryParseHtmlString(coilCol, out col2);
			break;
		}
		if (type1 == UnityMolResidue.secondaryStructureType.Strand) {
			col2 = col1;
		}
		c1 = col1;
		c2 = col2;
	}



	static void createSegmentMesh(int i, int n, PeptidePlane pp1, PeptidePlane pp2, PeptidePlane pp3, PeptidePlane pp4,
	                              ref List<Vector3> verticesList, ref List<int> trianglesList, ref List<Color32> colorsList,
	                              ref Dictionary<UnityMolResidue, List<int>> residueToVert, ref Dictionary<Vector3, int> verticesDict, bool isTraj) {


		UnityMolResidue.secondaryStructureType type0 = pp2.r1.secondaryStructure;
		UnityMolResidue.secondaryStructureType type1 = 0;
		UnityMolResidue.secondaryStructureType type2 = 0;

		pp2.Transition(ref type1, ref type2);

		Color32 c1 = Color.black;
		Color32 c2 = Color.black;
		segmentColors(pp2, ref c1, ref c2);

		Vector3[] profile1 = null;
		Vector3[] profile2 = null;

		int pdetail = profileDetail;
		int ssteps = splineSteps;
		if (isTraj) {
			pdetail = trajProfileDetail;
			ssteps = trajSplineSteps;
			// testDistanceCA_CA = false;
		}

		segmentProfiles(pp2, pp3, pdetail, ref profile1, ref profile2);

		int linearQuadOutcircOrIncirc = 0;//0 linear / 1 Quad / 2 Out Circ / 3 In Circ

		if ( !(type1 == UnityMolResidue.secondaryStructureType.Strand && type2 != UnityMolResidue.secondaryStructureType.Strand)) {
			linearQuadOutcircOrIncirc = 1;
		}
		if (type0 == UnityMolResidue.secondaryStructureType.Strand && type1 != UnityMolResidue.secondaryStructureType.Strand) {
			linearQuadOutcircOrIncirc = 2;
		}
		// if type1 != pdb.ResidueTypeStrand && type2 == pdb.ResidueTypeStrand {
		// 	easeFunc = ease.InOutSquare
		// }
		if (i == 0) {
			profile1 = ellipseProfile(pdetail, 0.0f, 0.0f);
			linearQuadOutcircOrIncirc = 2;
		}
		else if (i == n - 1) {
			profile2 = ellipseProfile(pdetail, 0.0f, 0.0f);
			linearQuadOutcircOrIncirc = 3;
		}
		List<Vector3[]> splines1 = new List<Vector3[]>(profile1.Length);
		List<Vector3[]> splines2 = new List<Vector3[]>(profile2.Length);

		for (int a = 0; a < profile1.Length; a++) {
			Vector3 p1 = profile1[a];
			Vector3 p2 = profile2[a];
			splines1.Add(splineForPlanes(pp1, pp2, pp3, pp4, ssteps, p1.x, p1.y));
			splines2.Add(splineForPlanes(pp1, pp2, pp3, pp4, ssteps, p2.x, p2.y));
		}
		int startV = Mathf.Max(verticesList.Count - 1, 0);

		for (int a = 0; a < ssteps; a++) {

			float t0 = easeFunc( ((float)a) / ssteps, linearQuadOutcircOrIncirc);
			float t1 = easeFunc( ((float)(a + 1)) / ssteps, linearQuadOutcircOrIncirc);

			if (a == 0 && type1 == UnityMolResidue.secondaryStructureType.Strand
			        && type2 != UnityMolResidue.secondaryStructureType.Strand ) {

				Vector3 p00 = splines1[0][a];
				Vector3 p10 = splines1[pdetail / 4][a];
				Vector3 p11 = splines1[2 * pdetail / 4][a];
				Vector3 p01 = splines1[3 * pdetail / 4][a];
				triangulateQuad(p00, p01, p11, p10,
				                c1, c1, c1, c1,
				                ref verticesList, ref colorsList, ref trianglesList, ref verticesDict);
			}
			for (int j = 0; j < pdetail; j++) {
				Vector3 p100 = splines1[j][a];
				Vector3 p101 = splines1[j][a + 1];
				Vector3 p110 = splines1[(j + 1) % pdetail][a];
				Vector3 p111 = splines1[(j + 1) % pdetail][a + 1];
				Vector3 p200 = splines2[j][a];
				Vector3 p201 = splines2[j][a + 1];
				Vector3 p210 = splines2[(j + 1) % pdetail][a];
				Vector3 p211 = splines2[(j + 1) % pdetail][a + 1];

				Vector3 p00 = Vector3.Lerp(p100, p200, t0);
				Vector3 p01 = Vector3.Lerp(p101, p201, t1);
				Vector3 p10 = Vector3.Lerp(p110, p210, t0);
				Vector3 p11 = Vector3.Lerp(p111, p211, t1);

				Color32 c00 = Color32.Lerp(c1, c2, t0);
				Color32 c01 = Color32.Lerp(c1, c2, t1);
				Color32 c10 = Color32.Lerp(c1, c2, t0);

				Color32 c11 = Color32.Lerp(c1, c2, t1);
				triangulateQuad(p10, p11, p01, p00,
				                c10, c11, c01, c00,
				                ref verticesList, ref colorsList, ref trianglesList, ref verticesDict);
			}
		}

		List<int> listVertId = new List<int>();

		for(int sV = startV; sV < verticesList.Count; sV++){
			listVertId.Add(sV);
		}

		// try {
			AddVertToResidueDict(ref residueToVert, pp1.r3, listVertId);
		// }
		// catch {}
	}

	public static void AddVertToResidueDict(ref Dictionary<UnityMolResidue, List<int>> residueToVert, UnityMolResidue r, List<int> newVs) {
		if(!residueToVert.ContainsKey(r)){
			residueToVert[r] = new List<int>(newVs.Count);
		}
		residueToVert[r].AddRange(newVs);
	}
	


	// static int vertInVertexList(Vector3 v, List<Vector3> verticesList, int lookFor = 50){
	// 	for(int i=verticesList.Count-1; i >= Mathf.Max(0,verticesList.Count - lookFor); i--){
	// 		if(Mathf.Abs(verticesList[i].x - v.x) < 0.0001f &&
	// 			Mathf.Abs(verticesList[i].y - v.y) < 0.0001f &&
	// 			Mathf.Abs(verticesList[i].z - v.z) < 0.0001f ){
	// 				return i;
	// 		}
	// 	}
	// 	return -1;
	// }

	static void triangulateQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,
	                            Color32 c1, Color32 c2, Color32 c3, Color32 c4, ref List<Vector3> verticesList,
	                            ref List<Color32> colorsList, ref List<int> trianglesList, ref Dictionary<Vector3, int> verticesDict) {

		const float tolerance = 1e-4f;
		p1.x = -p1.x;
		p2.x = -p2.x;
		p3.x = -p3.x;
		p4.x = -p4.x;

		p1.x = Mathf.Floor(p1.x / tolerance) * tolerance;
		p1.y = Mathf.Floor(p1.y / tolerance) * tolerance;
		p1.z = Mathf.Floor(p1.z / tolerance) * tolerance;

		p2.x = Mathf.Floor(p2.x / tolerance) * tolerance;
		p2.y = Mathf.Floor(p2.y / tolerance) * tolerance;
		p2.z = Mathf.Floor(p2.z / tolerance) * tolerance;

		p3.x = Mathf.Floor(p3.x / tolerance) * tolerance;
		p3.y = Mathf.Floor(p3.y / tolerance) * tolerance;
		p3.z = Mathf.Floor(p3.z / tolerance) * tolerance;

		p4.x = Mathf.Floor(p4.x / tolerance) * tolerance;
		p4.y = Mathf.Floor(p4.y / tolerance) * tolerance;
		p4.z = Mathf.Floor(p4.z / tolerance) * tolerance;

		//Version with unique vertices (2)
		int res1 = 0;
		int res2 = 0;
		int res3 = 0;
		int res4 = 0;

		int idp1 = res1;
		int idp2 = res2;
		int idp3 = res3;
		int idp4 = res4;


		if (verticesDict.TryGetValue(p1, out res1)) {
			idp1 = res1;
		}
		else {
			verticesList.Add(p1);
			idp1 = verticesList.Count - 1;
			colorsList.Add(c1);
			verticesDict[p1] = idp1;
		}

		if (verticesDict.TryGetValue(p2, out res2)) {
			idp2 = res2;
		}
		else {
			verticesList.Add(p2);
			idp2 = verticesList.Count - 1;
			colorsList.Add(c2);
			verticesDict[p2] = idp2;
		}

		if (verticesDict.TryGetValue(p3, out res3)) {
			idp3 = res3;
		}
		else {
			verticesList.Add(p3);
			idp3 = verticesList.Count - 1;
			colorsList.Add(c3);
			verticesDict[p3] = idp3;
		}

		if (verticesDict.TryGetValue(p4, out res4)) {
			idp4 = res4;
		}
		else {
			verticesList.Add(p4);
			idp4 = verticesList.Count - 1;
			colorsList.Add(c4);
			verticesDict[p4] = idp4;
		}

		trianglesList.Add(idp2);
		trianglesList.Add(idp1);
		trianglesList.Add(idp3);

		trianglesList.Add(idp3);
		trianglesList.Add(idp1);
		trianglesList.Add(idp4);

		// //Version with duplicate vertices
		// verticesList.Add(p1);
		// int idp1 = verticesList.Count - 1;
		// verticesList.Add(p2);
		// verticesList.Add(p3);
		// verticesList.Add(p4);

		// colorsList.Add(c1);
		// colorsList.Add(c2);
		// colorsList.Add(c3);
		// colorsList.Add(c4);

		// trianglesList.Add(idp1+1);
		// trianglesList.Add(idp1);
		// trianglesList.Add(idp1+2);

		// trianglesList.Add(idp1+2);
		// trianglesList.Add(idp1);
		// trianglesList.Add(idp1+3);
	}


	static Vector3[] splineForPlanes(PeptidePlane p1, PeptidePlane p2, PeptidePlane p3, PeptidePlane p4,
	                                 int n, float u, float v) {
		Vector3 g1 = p1.position + ((p1.side * u) + (p1.normal * v));
		Vector3 g2 = p2.position + ((p2.side * u) + (p2.normal * v));
		Vector3 g3 = p3.position + ((p3.side * u) + (p3.normal * v));
		Vector3 g4 = p4.position + ((p4.side * u) + (p4.normal * v));
		return spline(g1, g2, g3, g4, n);
	}

	static float easeFunc(float t, int idEase) {
		float res = t;
		//0 linear / 1 Quad / 2 Out Circ / 3 In Circ
		switch (idEase) {
		case 0:
			return t;
		// break;
		case 1:
			if (t < 0.5f) {
				return 2.0f * t * t;
			}
			t = 2.0f * t - 1;
			return -0.5f * (t * (t - 2) - 1);
		// break;
		case 2:
			t -= 1.0f;
			return Mathf.Sqrt(1 - (t * t));
		// break;
		case 3:
			return -1 * (Mathf.Sqrt(1 - t * t) - 1);
		// break;
		default:
			break;
		}
		return res;
	}

	static Vector3[] spline(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4 , int n ) {

		float n2 = (n * n);
		float n3 = (n * n * n);

		Matrix4x4 s = new Matrix4x4();
		s.SetRow(0, new Vector4(6.0f / n3, 0.0f, 0.0f, 0.0f));
		s.SetRow(1, new Vector4(6.0f / n3, 2.0f / n2, 0.0f, 0.0f));
		s.SetRow(2, new Vector4(1.0f / n3, 1.0f / n2, 1.0f / n, 0.0f));
		s.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

		Matrix4x4 b = new Matrix4x4();
		b.SetRow(0, new Vector4(-1.0f, 3.0f, -3.0f , 1.0f ) * 1.0f / 6.0f);
		b.SetRow(1, new Vector4(3.0f, -6.0f, 3.0f , 0.0f )  * 1.0f / 6.0f);
		b.SetRow(2, new Vector4(-3.0f, 0.0f, 3.0f, 0.0f)    * 1.0f / 6.0f);
		b.SetRow(3, new Vector4(1.0f, 4.0f, 1.0f, 0.0f)     * 1.0f / 6.0f);

		Matrix4x4 g = new Matrix4x4();
		g.SetRow(0, new Vector4(v1.x, v1.y, v1.z, 1.0f));
		g.SetRow(1, new Vector4(v2.x, v2.y, v2.z, 1.0f));
		g.SetRow(2, new Vector4(v3.x, v3.y, v3.z, 1.0f));
		g.SetRow(3, new Vector4(v4.x, v4.y, v4.z, 1.0f));

		Matrix4x4 m = s * b * g;

		Vector3[] result = new Vector3[n + 1];

		Vector3 v = new Vector3(m.m30 / m.m33, m.m31 / m.m33, m.m32 / m.m33 );

		v = RoundPlaces(v, 10);
		int id = 0;
		result[id] = v;
		id++;
		for (int k = 0; k < n; k++) {
			m.m30 = m.m30 + m.m20;
			m.m31 = m.m31 + m.m21;
			m.m32 = m.m32 + m.m22;
			m.m33 = m.m33 + m.m23;
			m.m20 = m.m20 + m.m10;
			m.m21 = m.m21 + m.m11;
			m.m22 = m.m22 + m.m12;
			m.m23 = m.m23 + m.m13;
			m.m10 = m.m10 + m.m00;
			m.m11 = m.m11 + m.m01;
			m.m12 = m.m12 + m.m02;
			m.m13 = m.m13 + m.m03;

			v.x = m.m30 / m.m33;
			v.y = m.m31 / m.m33;
			v.z = m.m32 / m.m33;
			v = RoundPlaces(v, 10);
			result[id] = v;
			id++;
		}
		return result;
	}

	static float RoundPlaces(float a, int places) {
		float shift = powersOfTen[places];
		return (float)(Mathf.Round(a * shift) / shift);
	}
	static Vector3 RoundPlaces(Vector3 v, int n) {
		v.x = RoundPlaces(v.x, n);
		v.y = RoundPlaces(v.y, n);
		v.z = RoundPlaces(v.z, n);
		return v;
	}

	static bool discontinuity(PeptidePlane pp1, PeptidePlane pp2, PeptidePlane pp3, PeptidePlane pp4) {
		if (diffPP(pp1.r1.id, pp1.r2.id) || diffPP(pp1.r2.id, pp1.r3.id)) {
			return true;
		}
		if (diffPP(pp2.r1.id, pp2.r2.id) || diffPP(pp2.r2.id, pp2.r3.id)) {
			return true;
		}
		if (diffPP(pp3.r1.id, pp3.r2.id) || diffPP(pp3.r2.id, pp3.r3.id)) {
			return true;
		}
		if (diffPP(pp4.r1.id, pp4.r2.id) || diffPP(pp4.r2.id, pp4.r3.id)) {
			return true;
		}

		return false;
	}

	static bool checkDistanceCA_CA(PeptidePlane pp) {
		const float limitDistCA_CA = 5.0f;//Max distance between 2 CA in Angstrom

		if (Vector3.Distance(pp.r1.atoms["CA"].position, pp.r2.atoms["CA"].position) >= limitDistCA_CA) {
			return false;
		}
		if(Vector3.Distance(pp.r2.atoms["CA"].position, pp.r3.atoms["CA"].position) >= limitDistCA_CA){
			return false;
		}
		return true;
	}
	static bool diffPP(int id1, int id2) {
		int diff = 1;
		if (id1 < 0 && id2 > 0) {
			diff = 2;
		}
		if (id2 - id1 > diff) {
			return true;
		}
		return false;
	}


}

public class PeptidePlane {

	public UnityMolResidue r1;
	public UnityMolResidue r2;
	public UnityMolResidue r3;
	public Vector3 position;
	public Vector3 normal;
	public Vector3 forward;
	public Vector3 side;
	public bool flipped;

	public PeptidePlane(UnityMolResidue res1, UnityMolResidue res2, UnityMolResidue res3) {

		r1 = res1;
		r2 = res2;
		r3 = res3;

		if (!r1.atoms.ContainsKey("CA")) {
			r1 = null;
			// Debug.LogError("Cannot allocate a PeptidePlane because residue "+r1+" does not contain 'CA' atom");
			return;
		}
		if (!r1.atoms.ContainsKey("O")) {
			r1 = null;
			// Debug.LogError("Cannot allocate a PeptidePlane because residue "+r1+" does not contain 'O' atom");
			return;
		}
		if (!r2.atoms.ContainsKey("CA")) {
			r1 = null;
			// Debug.LogError("Cannot allocate a PeptidePlane because residue "+r2+" does not contain 'CA' atom");
			return;
		}
		if (!r3.atoms.ContainsKey("CA")) {
			r1 = null;
			// Debug.LogError("Cannot allocate a PeptidePlane because residue "+r2+" does not contain 'CA' atom");
			return;
		}
		Vector3 ca1 = r1.atoms["CA"].position;
		ca1.x = -ca1.x;
		Vector3 ca2 = r2.atoms["CA"].position;
		ca2.x = -ca2.x;
		Vector3 o1  = r1.atoms["O"].position;
		o1.x = -o1.x;

		Vector3 a = (ca2 - ca1).normalized;
		Vector3 b = (o1 -  ca1).normalized;
		Vector3 c = Vector3.Cross(a, b).normalized;
		Vector3 d = Vector3.Cross(c, a).normalized;
		Vector3 p = (ca1 + ca2) / 2.0f;

		position = p;
		// position = ca1;
		normal = c;
		forward = a;
		side = d;
		flipped = false;

	}
	public void Flip() {
		side = -side;
		normal = -normal;
		flipped = !flipped;
	}

	public void Transition(ref UnityMolResidue.secondaryStructureType ss1, ref UnityMolResidue.secondaryStructureType ss2) {

		ss1 = r2.secondaryStructure;
		ss2 = r2.secondaryStructure;
		if (r2.secondaryStructure > r1.secondaryStructure && r2.secondaryStructure == r3.secondaryStructure) {
			ss1 = r1.secondaryStructure;
		}
		if (r2.secondaryStructure > r3.secondaryStructure && r1.secondaryStructure == r2.secondaryStructure) {
			ss2 = r3.secondaryStructure;
		}
	}
}
}