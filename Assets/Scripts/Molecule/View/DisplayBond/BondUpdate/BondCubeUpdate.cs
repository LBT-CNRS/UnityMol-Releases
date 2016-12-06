/// @file BondCubeUpdate.cs
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
/// $Id: BondCubeUpdate.cs 350 2013-08-23 13:55:39Z kouyoumdjian $
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
using UI;

public class BondCubeUpdate : MonoBehaviour {
	
	public static float scale = 1.0f;
	public static float radiusFactor = 1.0f;
	public GameObject atompointer1=null;
	public GameObject atompointer2=null;
	public int atomnumber1;
	public int atomnumber2;
	
	public static float oldscale = 1.0f;
	public static float width = GUIMoleculeController.bondWidth ;
	public static float oldWidth = GUIMoleculeController.bondWidth ;
//	private float oldrayon1 = 2.0f;
//	private float oldrayon2 = 2.0f;



	//Only check for d3d once
//	private bool d3d= false;
	void  Start () {
//		d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
//		Debug.Log("BondCubeUpdate: Start()");
		width = GUIMoleculeController.bondWidth ;
		if (atompointer1 != null && atompointer2 != null) {
			Vector3 v_dist = atompointer2.transform.position - atompointer1.transform.position ;
			float length = v_dist.magnitude ;
			Vector3 lscale = new Vector3(transform.localScale.x, transform.localScale.y, length);
			this.transform.localScale = lscale ;
		}
		transform.position = (atompointer1.transform.position + atompointer2.transform.position)/2.0f;
		transform.LookAt(atompointer2.transform.position);
	
		Vector3 pos1 = atompointer1.transform.position;
		GetComponent<Renderer>().material.SetVector("_Pos1", pos1);
		
		Vector3 pos2 = atompointer2.transform.position;
		GetComponent<Renderer>().material.SetVector("_Pos2", pos2);
		
		Color32 color1 = atompointer1.GetComponent<Renderer>().material.GetColor("_Color");
		//Debug.Log(color1.ToString());
		GetComponent<Renderer>().material.SetColor("_Color1", atompointer1.GetComponent<Renderer>().material.GetColor("_Color"));
		
		Color32 color2 = atompointer2.GetComponent<Renderer>().material.GetColor("_Color");
		//Debug.Log(color2.ToString());
		GetComponent<Renderer>().material.SetColor("_Color2", atompointer2.GetComponent<Renderer>().material.GetColor("_Color"));
		
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		Color32[] colors = new Color32[vertices.Length];
		float dist1, dist2;
		Matrix4x4 localToWorld = transform.localToWorldMatrix;
		
		Vector3 pos;
		for(int i=0; i<vertices.Length; i++) {
			pos = localToWorld.MultiplyPoint3x4(vertices[i]);
			dist1 = Vector3.Distance(pos1, pos);
			dist2 = Vector3.Distance(pos2, pos);
			if( dist1 < dist2 )
				colors[i] = color1;
			else
				colors[i] = color2;
		}
		
		mesh.colors32 = colors;
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<Renderer>().material.shader = Shader.Find("Custom/Ribbons");
		
	}

/*
	void  Update ()
	{
		if (!lengthAdjusted && atompointer1 != null && atompointer2 != null) {
				Vector3 v_dist = atompointer2.transform.position - atompointer1.transform.position ;
				float length = v_dist.magnitude ;
				Vector3 lscale = new Vector3(transform.localScale.x, transform.localScale.y, length);
				this.transform.localScale = lscale ;
				lengthAdjusted = true ;
			}
		if (width != oldWidth) {
			Vector3 lscale = new Vector3(width, width, transform.localScale.z); 
			this.transform.localScale = lscale ;	
		}
		if(atompointer1==null||atompointer2==null)
		{
			DestroyImmediate(this);
		}
	
		if(oldscale!=scale)
		{
			renderer.material.SetFloat("_Scale",scale);
			oldscale=scale;
		}
		if(UIData.EnableUpdate)
		{	
			transform.position = (atompointer1.transform.position + atompointer2.transform.position)/2.0f;
			transform.LookAt(atompointer2.transform.position);
	
			renderer.material.SetVector("_Pos1", atompointer1.transform.position);
			renderer.material.SetVector("_Pos2", atompointer2.transform.position);
			renderer.material.SetColor("_Color1", atompointer1.renderer.material.GetColor("_Color"));
			renderer.material.SetColor("_Color2", atompointer2.renderer.material.GetColor("_Color"));
		}//if(UIData.EnableUpdate)
	}//Update()
*/
}