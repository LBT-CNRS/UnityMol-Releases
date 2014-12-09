/// @file GenInterpolationArray.cs
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
/// $Id: GenInterpolationArray.cs 405 2014-04-09 08:26:05Z roudier $
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

public class GenInterpolationArray {

	public List<float[]> InputKeyNodes;
	public List<string> InputTypeArray;
	
	public List<float[]> OutputKeyNodes;
	public List<string> OutputTypeArray;

	// Use this for initialization
	void Start () {}

	public void CalculateSplineArray() {
		Debug.Log("Entering :: CalculateSplineArray()");
		OutputKeyNodes=new List<float[]>();
		OutputTypeArray=new List<string>();
		ArrayList ChainAtomList=new ArrayList();
		List<string> CaSplineTypeList=new List<string>();
		for(int i=1;i<InputKeyNodes.Count;i++) {
			//Debug.Log ("InputType " + InputTypeArray[i] + " " + InputTypeArray.Count);
			if(((string)(InputTypeArray[i-1])==(string)(InputTypeArray[i]))&&(i<InputKeyNodes.Count-1)) {
				ChainAtomList.Add(InputKeyNodes[i]);
				CaSplineTypeList.Add(InputTypeArray[i]);
				//Debug.Log ("InputTypeArray ==");
			}
			else {
				GenInterpolationPoint geninterpolationpoint = new GenInterpolationPoint();
				geninterpolationpoint.InputKeyNodes=ChainAtomList;
				geninterpolationpoint.InputTypeArray=CaSplineTypeList;
				geninterpolationpoint.CalculateSpline();
									
				OutputKeyNodes.AddRange(geninterpolationpoint.OutputKeyNodes);
				OutputTypeArray.AddRange(geninterpolationpoint.OutputTypeArray);
				ChainAtomList=null;
				ChainAtomList=new ArrayList();
				CaSplineTypeList=null;
				CaSplineTypeList=new List<string>();
				ChainAtomList.Add(InputKeyNodes[i]);
				CaSplineTypeList.Add(InputTypeArray[i]);
				Debug.Log("(string)(InputTypeArray[i-1])!=(string)(InputTypeArray[i])");
			}
		}
	}
	
	
	/*
	void Update()
	{
		if (UI.UIData.resetSpline)
		{
			CalculateSplineArray();
			UI.UIData.resetSpline = false;
		}
	} */
}
