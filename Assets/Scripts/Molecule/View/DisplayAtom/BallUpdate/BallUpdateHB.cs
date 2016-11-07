/// @file BallUpdateHB.cs
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
/// $Id: BallUpdateHB.cs 329 2013-08-06 13:47:40Z erwan $
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
using System.Globalization;

using Molecule.Model;


public class BallUpdateHB : BallUpdate {
	// Only check for d3d once

	public float z=0.0f;
//	public static float depthfactor = 1.0f;
	public static bool hide = false;	

//	public  bool xgmml=false;

	public static float maxV = 0;

	public static float drag=0.6f;
	public static float spring=5;
	public static Color EnergyGrayColor=Color.black;
	public bool hasMouseOverMolecule = false;

	
//	private float olddepthfactor = 2.0f;

//	public long number=0;

//	private bool d3d= false;
		
		
	public static string texture = "lit_spheres/divers/daphz05";
	public static Texture text2D = (Texture)Resources.Load(texture); 

	public static Vector4 cutplane = Vector4.zero;
	public static float cut = 0f;

	public static void SetTexture(Texture text){
		text2D = text;
	}
	
/*
	public static void SetTexture(string resource_name)
	{
		Debug.Log("BallUpdateHB: SetTexture()");
		texture = resource_name;
		text2D = (Texture)Resources.Load(texture);
		BallUpdateHB[] hyperBalls = Object.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[];
		foreach(BallUpdateHB hb in hyperBalls){
			hb.renderer.material.SetTexture("_MatCap", text2D);
		}
	}
*/
	
	public static void ResetColors()
	{
		BallUpdateHB[] hyperBalls = Object.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[];
		foreach(BallUpdateHB hb in hyperBalls)
		{
			hb.GetComponent<Renderer>().material.SetColor("_Color", hb.atomcolor);
		}
		resetColors = false;
	}

	public static Color HexToColor(string hexColor)
	{
		//Remove # if present
		if (hexColor.IndexOf('#') != -1)
			hexColor = hexColor.Replace("#", "");

		int red = 0;
		int green = 0;
		int blue = 0;

		if (hexColor.Length == 6)
		{
			//#RRGGBB
			red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
			green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
			blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
		}
		else if (hexColor.Length == 3)
		{
			//#RGB
			red = int.Parse(hexColor[0].ToString() + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier);
			green = int.Parse(hexColor[1].ToString() + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier);
			blue = int.Parse(hexColor[2].ToString() + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier);
		}
		
		Color hexcolor= new Color(red/255f,green/255f,blue/255f);
		

		return hexcolor;
	}
		
	void  Start (){
//		d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
		atomcolor=GetComponent<Renderer>().material.GetColor("_Color");
//		atomcolor = new Color(0.5f,0.5f,0.5f);
		GetComponent<Renderer>().material.SetFloat("_Cut", 0f);
		GetComponent<Renderer>().material.SetTexture("_MatCap", text2D);
	}

	public override void SetRayonFactor(float rf)
	{
		if(oldrayonFactor!=rf)
		{
			rayon /= oldrayonFactor;
			rayon *= rf;
			GetComponent<Renderer>().material.SetFloat("_Rayon",rayon*radiusFactor);
			oldrayonFactor=rf;
		}
	}

	public override float GetRealRadius()
	{
		return rayon*radiusFactor*2.0f;
	}
	
/*
	void  Update (){
		if(independant)
			return;

		// if(cut == 1f)
		// {
		// 	renderer.material.SetFloat("_Cut",cut);
		// 	renderer.material.SetVector("_Cutplane",cutplane);
		// }

		// if(cut == 0f && renderer.material.GetFloat("_Cut") == 1f)
		// 	renderer.material.SetFloat("_Cut", cut);

		//if(!rigidbody)
		if(!GUIMoleculeController.toggle_NA_INTERACTIVE)
		{
			//if(resetColors)
			//	ResetColors();
			
			if(oldradiusFactor!=radiusFactor)
			{
				renderer.material.SetFloat("_Rayon",rayon*radiusFactor);
				oldradiusFactor=radiusFactor;
			}
			

			if(xgmml) // move this
			{
				if(xgmml)this.transform.localPosition = new Vector3(this.transform.localPosition.x,this.transform.localPosition.y,z*depthfactor);

			
			//renderer.material.SetVector("_Rotation",MoleculeModel.Center);
			
		}
		

		if(GUIMoleculeController.toggle_NA_INTERACTIVE)
		{
			if(!GetComponent<MouseOverMolecule>())
			{
				gameObject.AddComponent<MouseOverMolecule>();
				hasMouseOverMolecule = true;
			}
		}
		else
		{
			//if(GetComponent<MouseOverMolecule>())
			if(hasMouseOverMolecule)
			{
				Destroy(GetComponent<MouseOverMolecule>());
				hasMouseOverMolecule = false;
			}	
		}

		
		//if(rigidbody)

		if(GUIMoleculeController.toggle_NA_INTERACTIVE)
		{
			GetComponent<Rigidbody>().drag=drag;
			
			GetComponent<SpringJoint>().spring=spring;

			float v = GetComponent<Rigidbody>().velocity.magnitude;

			if(UIData.toggleGray)
			{
				Color c = Color.Lerp(Color.white, Color.black, v);
				renderer.material.SetColor("_Color", c);
			}
			else
			{
			 	renderer.material.SetColor("_Color", atomcolor);
			}
			
			renderer.material.SetFloat("_Rayon",radiusFactor*rayon*(1.0f+v*0.01f));
	//		oldradiusFactor=radiusFactor;
	//		oldradiusFactor = 2.0f;
	//		oldatomcolor = Color.black;
	//		olddepthfactor = 2.0f;
		}
		
		renderer.enabled = (UIData.atomtype == UIData.AtomType.hyperball);
	}
*/

}