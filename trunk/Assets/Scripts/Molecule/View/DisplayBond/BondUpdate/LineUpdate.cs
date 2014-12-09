/// @file LineUpdate.cs
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
/// $Id: LineUpdate.cs 329 2013-08-06 13:47:40Z erwan $
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

public class LineUpdate : MonoBehaviour {

	public GameObject atompointer1=null;
	public GameObject atompointer2=null;
	
	public static float scale = 1.0f;
	
	
	public Color oldatomcolor1=Color.black;
//	private Color oldatomcolor2=Color.black;
	
//	private Vector3 oldatomposition1=new Vector3(0,0,0);
//	private Vector3 oldatomposition2=new Vector3(0,0,0);
	
	public float oldscale = 2.0f;
	
	public float oldradius1 = 1.0f;
	public float oldradius2 = 1.0f;
		
	public static float width = GUIMoleculeController.bondWidth ;
	public static float oldWidth = GUIMoleculeController.bondWidth ;
		
	public LineRenderer lineRenderer;
	
	// Only check for d3d once
	void  Start ()
	{
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.SetPosition(0, atompointer1.transform.position);
		lineRenderer.SetPosition(1, atompointer2.transform.position);
		lineRenderer.SetWidth(width, width);
//		oldatomposition1=atompointer1.transform.position;
//		oldatomposition2=atompointer2.transform.position;
	}
	
		
/*
	void  Update ()
	{
		//lineRenderer.SetVertexCount(2);
		if(UIData.EnableUpdate)
		{
				if((oldatomcolor1!=atompointer1.renderer.material.GetColor("_Color"))||(oldatomcolor1!=atompointer2.renderer.material.GetColor("_Color")))
				{
					lineRenderer.SetColors(atompointer1.renderer.material.GetColor("_Color"), atompointer2.renderer.material.GetColor("_Color"));   
					
					oldatomcolor1=atompointer1.renderer.material.GetColor("_Color");
	//				oldatomcolor2=atompointer2.renderer.material.GetColor("_Color");
				}
				if((oldatomposition1!=atompointer1.transform.position)||(oldatomposition2!=atompointer2.transform.position))
				{
					lineRenderer.SetPosition(0, atompointer1.transform.position);
					lineRenderer.SetPosition(1, atompointer2.transform.position);
					oldatomposition1=atompointer1.transform.position;
					oldatomposition2=atompointer2.transform.position;
				}
				
				if(oldscale!=scale)
				{
					if(atompointer1.renderer.material.HasProperty("_Rayon")&&atompointer2.renderer.material.HasProperty("_Rayon"))
						lineRenderer.SetWidth(	scale*atompointer1.renderer.material.GetFloat("_Rayon"), 
												scale*atompointer2.renderer.material.GetFloat("_Rayon")); 
					else
						lineRenderer.SetWidth(	scale*atompointer1.transform.lossyScale.x/2, 
												scale*atompointer2.transform.lossyScale.x/2); 
					oldscale=scale;
				}
				
				if(atompointer1.renderer.material.HasProperty("_Rayon") && atompointer2.renderer.material.HasProperty("_Rayon"))
				{
					if((oldradius1!=atompointer1.renderer.material.GetFloat("_Rayon"))||(oldradius2!=atompointer2.renderer.material.GetFloat("_Rayon")))
					{
						lineRenderer.SetWidth(	scale*atompointer1.renderer.material.GetFloat("_Rayon"), 
												scale*atompointer2.renderer.material.GetFloat("_Rayon")); 
						oldradius1=atompointer1.renderer.material.GetFloat("_Rayon");
						oldradius2=atompointer2.renderer.material.GetFloat("_Rayon");
					}
				}
				else
				{
					
					width = GUIMoleculeController.bondWidth ;
					if (width != oldWidth) {
						lineRenderer.SetWidth(width, width);
					}
					
					if((oldradius1!=atompointer1.transform.lossyScale.x/2)||(oldradius2!=atompointer2.transform.lossyScale.x/2))
					{
						lineRenderer.SetWidth(	scale*atompointer1.transform.lossyScale.x/2, 
												scale*atompointer2.transform.lossyScale.x/2); 
						oldradius1=atompointer1.transform.lossyScale.x/2;
						oldradius2=atompointer2.transform.lossyScale.x/2;
					}
				}
		}
	}
*/
}