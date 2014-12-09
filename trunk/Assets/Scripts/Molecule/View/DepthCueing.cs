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
/// $Id: DepthCueing.cs
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
using UI;
using Molecule.Model;


public class DepthCueing {
	private ReadDX readDx;
	private float[,,] depth;
	private static Vector3 invDelta;
	private static Vector3 origin = MoleculeModel.MinValue;
	private BallUpdate[] balls;
	private int xDim, yDim, zDim;
	private float depthAmplitude;
	
	private static float OFFSET = 1.0f;
	private static float MAX_DEPTH = -10.0f;
	private static List<Color> originalColors;
	
	public static bool isEnabled = false;
	public static bool reset = false;
	public static float POWER = 2;
	
	
	/// <summary>
	/// Initializes a new instance of the <see cref="DepthCueing"/> class.
	/// </summary>
	public DepthCueing() {
		// We get the ReadDX object and from it, the values we need.
		readDx = GUIMoleculeController.readdx;
		depth = readDx._grid;
		xDim = depth.GetLength(0);
		yDim = depth.GetLength(1);
		zDim = depth.GetLength(2);
		
		Debug.Log("Dimensions:");
		Debug.Log(xDim.ToString());
		Debug.Log(yDim.ToString());
		Debug.Log(zDim.ToString());
		
		Vector3 delta = readDx.GetDelta();
		invDelta = delta;
		
		invDelta.x = 1f/delta.x;
		invDelta.y = 1f/delta.y;
		invDelta.z = 1f/delta.z;
		
		origin = readDx.GetOrigin();
		
		// This is needed to correctly localize things. Unity's convention is different from our input data.
		origin.x = -origin.x;
		
		balls = GameObject.FindObjectsOfType(typeof(BallUpdate)) as BallUpdate[];
		
		BuildColorList();
		DepthCueing.reset = false;
	}
	
	/// <summary>
	/// Builds the list of the original colors, that is before the Depth Cueing effect is applied.
	/// </summary>
	private void BuildColorList() {
		originalColors = new List<Color>(MoleculeModel.atomsColorList);
	}
	
	
	/// <summary>
	/// This function is currently not used, but can be useful for debugging, to check whether the index computed matches an actual location
	/// in the grid, and use various prints if that is not the case.
	/// </param>
	private bool InBounds(int x, int y, int z){
		if(x<0 || y<0 || z<0)
			return false;
		else
			return true;
	}
	
	/// <summary>
	/// Darkens the molecule. This performs the actual Depth Cueing effect.
	/// </summary>
	public void Darken() {
		int x, y, z;
		float pointDepth;
		float depthPercentage;
		float colorFactor;
		Vector3 position = Vector3.zero;
		Vector3 indices = Vector3.zero;
		foreach(BallUpdate bu in balls) {
			position = bu.transform.position;
			indices = Vector3.Scale(invDelta, (position - origin));
			x = (int)indices.x;
			x = -x; // Again, the X axis must be flipped.
			y = (int)indices.y;
			z = (int)indices.z;
			
			pointDepth = depth[x,y,z];
			
			if(pointDepth>=0f)
				continue; // The values that are inside the molecule are negative.
			
			pointDepth = pointDepth + OFFSET;
			
			if(pointDepth <= MAX_DEPTH)
				bu.atomcolor = Color.black;
			else {
				depthPercentage = Mathf.Abs(pointDepth/MAX_DEPTH);
				colorFactor = 1f - depthPercentage;
				colorFactor = Mathf.Pow(colorFactor, 2);
				//bu.atomcolor = bu.atomcolor * colorFactor * (1.25f + POWER/10f);
				MoleculeModel.atomsColorList[(int)bu.number] *= colorFactor * (1.25f + POWER/10f);
			}
		}
		BallUpdate.resetColors = true;
		isEnabled = true;
	}
	
	/// <summary>
	/// Reverts the molecule to its original colors, that is before the Depth Cueing effect was applied.
	/// </summary>
	public void Revert() {
		Debug.Log("DepthCueing: Reverting to original colors");
		MoleculeModel.atomsColorList = originalColors;
		BallUpdate.resetColors = true;
		isEnabled = false;
	}
	
}
