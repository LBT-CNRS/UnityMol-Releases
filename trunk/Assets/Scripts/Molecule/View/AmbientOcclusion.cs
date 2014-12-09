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
/// $Id: AmbientOcclusion.cs
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
using System.Collections;
using System.Collections.Generic;
using Molecule.Model;
//using UI;

public class AmbientOcclusion {
	private static GameObject pdb2den;
	private static PDBtoDEN genDensity;
	private static float[,,] density;
	private static float[,,] occlusion;
	private static BallUpdate[] balls;
	private static Vector3 delta;
	private static Vector3 origin = MoleculeModel.MinValue;
	private static Vector3 fudgeFactor;
	
	private static Vector3 inverseDelta ;
	private static int xDim, yDim, zDim;
	private static List<Color> originalColors;
	
	private static float minDensity = float.MaxValue; // Unused ?
	private static float maxDensity = -float.MaxValue;
	
	public static bool isEnabled = false;
	public static bool reset = false;
	
	private const float faceCoef = 1f;
	private const float diagCoef = 0.707106781186548f; // 1/sqrt(2)
	private const float cornerCoef = 0.25f;
	private const float totalWeightFactor = 1f/(6f*faceCoef + 12f*diagCoef + 8f*cornerCoef);
	
	private static bool weighted = true;
	private static float compensationFactor = 1.30f;
	//private static float compensationTerm = 0.12f;
	//private static Color compensationColor = new Color(compensationTerm, compensationTerm, compensationTerm);
	
	
	/// <summary>
	/// Computes the sum of the surrounding cubes. That is the eight cubes around the cube of interest at the same Z coordinate,
	/// the 9 cubes right below (z-1) and the 9 cubes right above (z+1).
	/// </summary>
	/// <returns>
	/// The of surrounding cubes.
	/// </returns>
	/// <param name='i'>
	/// X coordinate of the cube
	/// </param>
	/// <param name='j'>
	/// Y coordinate of the cube
	/// </param>
	/// <param name='k'>
	/// Z coordinate of the cube
	/// </param>
	private static float AverageOfSurroundingCubes(int i, int j, int k) {
		// j-1					// i-1						// i						// i+1
		float lowerNine		=	density[i-1, j-1, k-1]	+	density[i, j-1, k-1]	+	density[i+1, j-1, k-1]	+ // k-1
								density[i-1, j-1, k]	+	density[i, j-1, k]		+	density[i+1, j-1, k]	+ // k
								density[i-1, j-1, k+1]	+	density[i, j-1, k+1]	+	density[i+1, j-1, k+1]	; // k+1
		
		// j					// i-1						// i						// i+1
		float middleEight	=	density[i-1, j, k-1]	+	density[i, j, k-1]		+	density[i+1, j, k-1]	+ // k-1
								density[i-1, j, k]		+	0f						+	density[i+1, j, k]		+ // k
								density[i-1, j, k+1]	+	density[i, j, k+1]		+	density[i+1, j, k+1]	; // k+1
		
		// j+1					// i-1						// i						// i+1
		float upperNine		=	density[i-1, j+1, k-1]	+	density[i, j+1, k-1]	+	density[i+1, j+1, k-1]	+ // k-1
								density[i-1, j+1, k]	+	density[i, j+1, k]		+	density[i+1, j+1, k]	+ // k
								density[i-1, j+1, k+1]	+	density[i, j+1, k+1]	+	density[i+1, j+1, k+1]	; // k+1
		
		float result = lowerNine + middleEight + upperNine;
		result /= 26;
		return result;
	}
	
	
	private static float WeightedAverageOfSurroundingCubes(int i, int j, int k) {
		// j-1					// i-1						// i						// i+1
		float lowerNine		=   cornerCoef	* density[i-1, j-1, k-1]	+   diagCoef	* density[i, j-1, k-1]	+	cornerCoef	* density[i+1, j-1, k-1]	+ // k-1
								diagCoef	* density[i-1, j-1, k]		+	faceCoef	* density[i, j-1, k]	+	diagCoef	* density[i+1, j-1, k]		+ // k
								cornerCoef	* density[i-1, j-1, k+1]	+	diagCoef	* density[i, j-1, k+1]	+	cornerCoef	* density[i+1, j-1, k+1]	; // k+1
		
		// j					// i-1						// i						// i+1
		float middleEight	=	diagCoef	* density[i-1, j, k-1]		+	faceCoef	* density[i, j, k-1]	+	diagCoef	* density[i+1, j, k-1]		+ // k-1
								faceCoef	* density[i-1, j, k]		+	0f									+	faceCoef	* density[i+1, j, k]		+ // k
								diagCoef	* density[i-1, j, k+1]		+	faceCoef	* density[i, j, k+1]	+	diagCoef	* density[i+1, j, k+1]		; // k+1
		
		// j+1					// i-1						// i						// i+1
		float upperNine		=	cornerCoef	* density[i-1, j+1, k-1]	+	diagCoef	* density[i, j+1, k-1]	+	cornerCoef	* density[i+1, j+1, k-1]	+ // k-1
								diagCoef	* density[i-1, j+1, k]		+	faceCoef	* density[i, j+1, k]	+	diagCoef	* density[i+1, j+1, k]		+ // k
								cornerCoef	* density[i-1, j+1, k+1]	+	diagCoef	* density[i, j+1, k+1]	+	cornerCoef	* density[i+1, j+1, k+1]	; // k+1
		
		float result = lowerNine + middleEight + upperNine;
		result *= totalWeightFactor;
		return result;
	}
	
	/// <summary>
	/// Builds the list of the original colors, that is before the Depth Cueing effect is applied.
	/// </summary>
	private static void BuildColorList() {
		originalColors = new List<Color>(MoleculeModel.atomsColorList);
	}
	
	/// <summary>
	/// This fills the "faces" of the cube with zeros, because they do not have the necessary number of neighbors for a proper sum.
	/// In practice the grid is probably much larger than the molecules, which means these locations do not contain any atom
	/// and therefore do not matter, but just in case...
	/// </summary>
	private static void FillFacesWithZeros() {
		for(int i=0; i<xDim; i++) {
			for(int j=0; j<yDim; j++) {
				occlusion[i,		j,			0		] = 0f; // front face
				occlusion[i,		j,			zDim-1	] = 0f; // back face
			}
			for(int k=0; k<zDim; k++) {
				occlusion[i,		0,			k		] = 0f; // bottom face
				occlusion[i,		yDim-1,		k		] = 0f; // top face
			}
		}
		
		for(int j=0; j<yDim; j++) {
			for(int k=0; k<zDim; k++) {
				occlusion[0,		j,			k		] = 0f; // left face
				occlusion[xDim-1,	j,			k		] = 0f; // right face
			}
		}
	}
	
	/// <summary>
	/// Sets the occlusion grid bounds.
	/// I.e. this function determines the highest and lowest densities found.
	/// </summary>
	private static void SetOcclusionGridBounds() {
		minDensity = float.MaxValue;
		maxDensity = -float.MaxValue;
		foreach(float f in density) {
			if(f < minDensity)
				minDensity = f;
			if(f > maxDensity)
				maxDensity = f;
		}
		
		/*
		float threshold = maxDensity * 0.3f;
		foreach(float f in occlusion)
			if(f > threshold)
				Debug.Log(f.ToString());
		*/
		
		Debug.Log("Density bounds, min and max:");
		Debug.Log(minDensity.ToString());
		Debug.Log(maxDensity.ToString());
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="AmbientOcclusion"/> class.
	/// </summary>
	public AmbientOcclusion () {
		// First we use PDBtoDen to create a density grid.
		pdb2den = new GameObject();
		pdb2den.AddComponent<PDBtoDEN>();
		genDensity = pdb2den.GetComponent<PDBtoDEN>();
		
		
		float delta_scalar;
		int moleculeSize = MoleculeModel.atomsLocationlist.Count;
		if(moleculeSize < 2000)
			delta_scalar = 0.2f;
		else if (moleculeSize < 5000)
			delta_scalar = 0.4f;
		else if (moleculeSize < 10000)
			delta_scalar = 0.2f;
		else
			delta_scalar = 0.16f;
		//fudgeFactor = PDBtoDEN.fudgeFactor /delta_scalar; ???
		delta = new Vector3(delta_scalar, delta_scalar, delta_scalar);
		genDensity.TranPDBtoDEN(delta_scalar, false);
		density = PDBtoDEN.GridS;
		fudgeFactor = PDBtoDEN.fudgeFactor;
		
		// We need to refresh the molecule's origin when it's not the first molecule for which we generate a volumetric density.
		origin = MoleculeModel.MinValue;
		
		xDim = density.GetLength(0);
		yDim = density.GetLength(1);
		zDim = density.GetLength(2);
		
		Debug.Log("Density grid size :");
		Debug.Log(xDim.ToString());
		Debug.Log(yDim.ToString());
		Debug.Log(zDim.ToString());
		
		occlusion = new float[xDim, yDim, zDim];
		
		for(int i=1; i<(xDim-1); i++)
			for(int j=1;  j<(yDim-1); j++)
				for(int k=1; k<(zDim-1); k++)
					if(weighted)
						occlusion[i,j,k] = WeightedAverageOfSurroundingCubes(i,j,k);
					else
						occlusion[i,j,k] = AverageOfSurroundingCubes(i,j,k);
		
		FillFacesWithZeros();
		SetOcclusionGridBounds();
		balls = GameObject.FindObjectsOfType(typeof(BallUpdate)) as BallUpdate[];
		BuildColorList();
		reset = false;
	}
	
	/// <summary>
	/// Perfoms the actual Ambient Occlusion operations.
	/// </summary>
	public void Occlude() {
		int x, y, z;
		Vector3 position = Vector3.zero;
		Vector3 indices = Vector3.zero;
		float colorFactor;
		Debug.Log(maxDensity.ToString());
		//float threshold = 0.3f * maxDensity;
		
		/*
		foreach(float f in occlusion) {
			if(f > threshold)
				Debug.Log(f.ToString());
		}
		*/
		Color color;
		foreach(BallUpdate bu in balls) {
			position = bu.transform.position;
			//indices = Vector3.Scale(delta, (position + fudgeFactor - origin));
			indices = Vector3.Scale(delta, (position - origin)) + fudgeFactor;
			/*
			x = (int) indices.x;
			y = (int) indices.y;
			z = (int) indices.z;
			*/
			x = Mathf.RoundToInt(indices.x);
			y = Mathf.RoundToInt(indices.y);
			z = Mathf.RoundToInt(indices.z);
			colorFactor = (maxDensity - occlusion[x,y,z]) / maxDensity;
			
			/*
			if(occlusion[x,y,z] > threshold) {
				Debug.Log(occlusion[x,y,z].ToString());
				Debug.Log(colorFactor.ToString());
			}

			
			//if(colorFactor > 0.1f)
			//Debug.Log(colorFactor + " - " + maxDensity);
			*/
			//bu.atomcolor = bu.atomcolor * colorFactor;
			color = MoleculeModel.atomsColorList[(int)bu.number];
			color = (color * colorFactor * compensationFactor);// + compensationColor;
			MoleculeModel.atomsColorList[(int)bu.number] = color;
			/*
			Color test = bu.atomcolor;
			test.a = test.a * (1 - colorFactor);
			bu.atomcolor = test;
			*/
		}
		BallUpdate.resetColors = true;
		isEnabled = true;
	}
	
	/// <summary>
	/// Reverts the molecule to its original colors, that is before the Depth Cueing effect was applied.
	/// </summary>
	public void Revert() {
		Debug.Log("AmbientOcclusion: reverting to original colors.");
		MoleculeModel.atomsColorList = originalColors;
		BallUpdate.resetColors = true;
		isEnabled = false;
	}
}
