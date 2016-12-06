/// @file StickUpdate.cs
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
/// $Id: StickUpdate.cs 346 2013-08-19 18:14:34Z kouyoumdjian $
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

public class StickUpdate : MonoBehaviour {
	
	public static bool resetColors = false;
	
	public static float shrink = 0.01f;
	public static float oldshrink = 0.01f;
	
	public static float scale = 1.0f;
	public static float oldscale = 1.0f;
	
	public static float radiusFactor = 1.0f;
	public GameObject atompointer1=null;
	public GameObject atompointer2=null;
	public int atomnumber1;
	public int atomnumber2;
	
	
	
	public float oldrayon1 = 2.0f;
	public float oldrayon2 = 2.0f;
	
	public ParticleEmitter emitter;
	
	public bool independant = false;
	
/*
	public static void SetTexture()
	{
		StickUpdate[] stickUpdates = Object.FindObjectsOfType(typeof(StickUpdate)) as StickUpdate[];
		foreach(StickUpdate stu in stickUpdates)
		{
			stu.renderer.material.SetTexture("_MatCap", BallUpdateHB.text2D);
		}
	}
*/
	
	public static void ResetColors() {
		StickUpdate[] stickUpdates = Object.FindObjectsOfType(typeof(StickUpdate)) as StickUpdate[];
		foreach(StickUpdate stu in stickUpdates) {
			stu.GetComponent<Renderer>().material.SetColor("_Color", stu.atompointer1.GetComponent<Renderer>().material.GetColor("_Color"));
			stu.GetComponent<Renderer>().material.SetColor("_Color2", stu.atompointer2.GetComponent<Renderer>().material.GetColor("_Color"));
		}
		resetColors = false;
	}
	
	// Only check for d3d once
//	private bool d3d= false;
	void  Start (){
//		d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
		GetComponent<Renderer>().material.SetColor("_Color", atompointer1.GetComponent<Renderer>().material.GetColor("_Color"));
		GetComponent<Renderer>().material.SetColor("_Color2", atompointer2.GetComponent<Renderer>().material.GetColor("_Color"));
		
		GetComponent<Renderer>().material.SetVector("_TexPos1", atompointer1.transform.position);
		transform.position = atompointer1.transform.position;
		GetComponent<Renderer>().material.SetVector("_TexPos2", atompointer2.transform.position);
//		renderer.material.SetTexture("_MatCap",BallUpdateHB.text2D);
		if(UIData.atomtype == UIData.AtomType.hyperball) {
			GetComponent<Renderer>().material.SetTexture("_MatCap", atompointer1.GetComponent<Renderer>().material.GetTexture("_MatCap"));
			GetComponent<Renderer>().material.SetTexture("_MatCap2", atompointer2.GetComponent<Renderer>().material.GetTexture("_MatCap"));
		}
		else {
			GetComponent<Renderer>().material.SetTexture("_MatCap", (Texture)Resources.Load("lit_spheres/divers/daphz05"));
			GetComponent<Renderer>().material.SetTexture("_MatCap2", (Texture)Resources.Load("lit_spheres/divers/daphz05"));
		}
	}
	
/*
	void  Update ()
	{
		if(independant)
			return;
			
		if(atompointer1==null||atompointer2==null)
		{
			DestroyImmediate(this);
		}
		
		
		if(UIData.EnableUpdate)
		{	
			if(oldshrink!=shrink)
			{
				renderer.material.SetFloat("_Shrink",shrink);
				oldshrink=shrink;
			}
			if(oldscale!=scale)
			{
				renderer.material.SetFloat("_Scale",scale);
				oldscale=scale;
			}
			//if(resetColors)
			//	ResetColors();
			
			if(GUIMoleculeController.toggle_NA_INTERACTIVE)
			{
				renderer.material.SetVector("_TexPos1", atompointer1.transform.position);
				transform.position = atompointer1.transform.position;
				renderer.material.SetVector("_TexPos2", atompointer2.transform.position);
			}

			//If atoms are hyperballs
			//if(atompointer1.renderer.material.HasProperty("_Rayon") && atompointer2.renderer.material.HasProperty("_Rayon"))
			
			if(UIData.atomtype == UIData.AtomType.hyperball)
			{
				if(oldrayon1!=atompointer1.renderer.material.GetFloat("_Rayon"))
				{
					renderer.material.SetFloat("_Rayon1",atompointer1.renderer.material.GetFloat("_Rayon"));
					oldrayon1=atompointer1.renderer.material.GetFloat("_Rayon");
				}
				if(oldrayon2!=atompointer2.renderer.material.GetFloat("_Rayon"))
				{
					if(atompointer2.renderer.material.HasProperty("_Rayon")) // ????
					renderer.material.SetFloat("_Rayon2",atompointer2.renderer.material.GetFloat("_Rayon"));
					oldrayon2=atompointer2.renderer.material.GetFloat("_Rayon");
				}
			}
			else
			{
				if(oldrayon1!=atompointer1.transform.lossyScale.x/2)
				{	
					renderer.material.SetFloat("_Rayon1",atompointer1.transform.lossyScale.x/2);
					oldrayon1=atompointer1.transform.lossyScale.x/2;
				}
				if(oldrayon2!=atompointer2.transform.lossyScale.x/2)
				{
					renderer.material.SetFloat("_Rayon2",atompointer2.transform.lossyScale.x/2);
					oldrayon2=atompointer2.transform.lossyScale.x/2;
				}
			}
		}
	}
*/
}