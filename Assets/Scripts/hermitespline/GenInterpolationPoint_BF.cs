/// @file GenInterpolationPoint_BF.cs
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
/// $Id: GenInterpolationPoint.cs 387 2014-04-02 08:21:11Z roudier $
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

public class GenInterpolationPoint_BF {
	
	//public parameters
	public GameObject SplineParent;
	public float Duration = 1.0f;
	public int lineCount=1000;
	public ArrayList InputKeyNodes;
	public List<string> InputTypeArray;
	//C.R
	public List<float> InputBfactArray;
	
	public List<float[]> OutputKeyNodes;
	public List<string> OutputTypeArray;
	//C.R
	public List<float> OutputBfactArray;

//	private Transform[] mTransforms;
	private List<SplineNode> mNodes;
	
	public static int smoothnessFactor = 8 ;
	
	//test the node data varibles
	
	public GameObject temp;
	public float moveSpeed=10f;
//	private int curIdx=0;
//	private float curTime=0;
//	private float moveTime=0;
	private Vector3 NextPos;
	private Vector3 CurPos;
	private Quaternion CurRot;
	private Quaternion NextRot;
	
	private Vector3[] Nodes;
	

	public void CalculateSpline() {
			lineCount=InputKeyNodes.Count*smoothnessFactor;

		    SetupSplineInterpolator();
		    OutputKeyNodes=new List<float[]>();
		    OutputTypeArray=new List<string>();
			//C.R
			OutputBfactArray = new List<float> ();
		    
			Gizmos.color = new Color(0.0f, 0.0f, 1.0f);
		    for (int c=1; c <lineCount; c++) {
		        float currTime = c * Duration / lineCount;
		        Vector3 currPos = GetHermiteAtTime(currTime);
		        
		        float[] currposfloat=new float[3];
		        currposfloat[0]=currPos.x;
		        currposfloat[1]=currPos.y;
		        currposfloat[2]=currPos.z;
		        OutputKeyNodes.Add(currposfloat);
		        OutputTypeArray.Add(InputTypeArray[0]);
//				MonoBehaviour.print("CalculateSpline");
		    }
	}

	void SetupSplineInterpolator() {
		int c;
		float step;
	    mNodes = new List<SplineNode>();
	    step = Duration/InputKeyNodes.Count;
	    
	    for (c = 0; c < InputKeyNodes.Count; c++) {
	    	float[] vect=(float[])InputKeyNodes[c];
	    	Vector3 inputnode=new Vector3(vect[0],vect[1],vect[2]);
			mNodes.Add(new SplineNode(inputnode, step*c));
	    }
//		SetAutoCloseMode(step*c);
		
		mNodes.Insert(0,mNodes[0]);
	    mNodes.Add(mNodes[mNodes.Count-1]);
	}
	
		
	public Vector3 GetHermiteInternal(int idxFirstPoint ,float t){
	    float t2 = t*t; float t3 = t2*t;
	    
	    Vector3 P0 = mNodes[idxFirstPoint-1].Point;
	    Vector3 P1 = mNodes[idxFirstPoint].Point;
	    Vector3 P2 = mNodes[idxFirstPoint+1].Point;
	    Vector3 P3 = mNodes[idxFirstPoint+2].Point;
	    
	    float tension = 0.5f;  // 0.5 equivale a catmull-rom
	    
	    Vector3 T1 = tension * (P2 - P0);
	    Vector3 T2 = tension * (P3 - P1);
	    
	    float Blend1 =  2*t3 - 3*t2 + 1;
	    float Blend2 = -2*t3 + 3*t2;
	    float Blend3 =    t3 - 2*t2 + t;
	    float Blend4 =    t3 -   t2;

		//C.R

		if (idxFirstPoint < mNodes.Count - 3) {
			OutputBfactArray.Add (InputBfactArray [idxFirstPoint] + ((InputBfactArray [idxFirstPoint+1] - InputBfactArray [idxFirstPoint]) / smoothnessFactor) * (t * smoothnessFactor));
	//		Debug.Log ("resultat " + ((InputBfactArray [idxFirstPoint] - InputBfactArray [idxFirstPoint + 1]) / smoothnessFactor)* (t * smoothnessFactor));
		}else
			OutputBfactArray.Add (InputBfactArray [idxFirstPoint]);

		// C.R

	    return Blend1*P1 + Blend2*P2 + Blend3*T1 + Blend4*T2;
	}
	
	public Vector3 GetHermiteAtTime(float timeParam){   
	    int c;
		if (timeParam >= mNodes [mNodes.Count - 2].Time) {
		//  C.R
			OutputBfactArray.Add (InputBfactArray[InputBfactArray.Count-1]);
		//C.R
			return mNodes [mNodes.Count - 2].Point;
				}
	    
	    for (c = 1; c < mNodes.Count-2; c++){
	        if (mNodes[c].Time > timeParam)
	            break;
	    }
	    
	    int idx = c-1;
	    float param = (timeParam - mNodes[idx].Time) / (mNodes[idx+1].Time - mNodes[idx].Time);                                                                                                                                                                                                                                                                                                                                        
	    param = MathUtils.Ease(param, 0.0f, 0.1f);
	    
	    return GetHermiteInternal(idx, param);
	}
	
	//define the node class
	class SplineNode{
	    public Vector3 Point;
	    public float Time;
	    	    
	    public SplineNode(Vector3 p,float t){ 
			Point=p; 
			Time=t; 
		}
		
	    public SplineNode(SplineNode o){ 
			Point=o.Point; 
			Time=o.Time; 
		}
	}

}