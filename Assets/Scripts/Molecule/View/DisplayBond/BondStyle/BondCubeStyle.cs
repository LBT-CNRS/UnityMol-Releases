/// @file BondCubeStyle.cs
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
/// $Id: BondCubeStyle.cs 239 2013-04-07 19:31:48Z baaden $
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


namespace Molecule.View.DisplayBond
{
	
	using UnityEngine;
	using System.Collections;
	using Molecule.Model;
	using Molecule.Control;
	using Config;
	using UI;
	using Molecule.View.DisplayAtom;
	public class BondCubeStyle:IBondStyle
	{
		public int number=1;
		public ArrayList bondList;
		public ArrayList bondEPList;

		public GameObject BondCubeParent = new GameObject("BondCubeParent");
		
		public BondCubeStyle()
		{
			number=FunctionConfig.number;
		}
		
		
		public void DisplayBonds()
		{
			
			if(UIData.bondtype==UIData.BondType.cube)
			{
				bondList=MoleculeModel.bondList;
				MoleculeModel.bondsnumber=bondList.Count;
				if(UIData.secondarystruct)
				{
					bondEPList=MoleculeModel.bondCAList;
				}
				else
				{
					bondEPList=MoleculeModel.bondEPList;
				}
				int Number=bondEPList.Count/number;
				
				Debug.Log("DisplayBonds??bondList.Count "  + bondList.Count);

				for(int i=0;i<Number;i++)
				{
					CreateCylinder(i*number);
				}
			}
			else if(UIData.bondtype==UIData.BondType.hyperstick)
			{
				if(UIData.secondarystruct)
				{
					bondEPList=MoleculeModel.bondCAList;
				}
				else
				{
					bondEPList=MoleculeModel.bondEPList;
				}
				Debug.Log("Bonds?? bondEPList.Count :: "  + bondEPList.Count);
				int Number=bondEPList.Count/number;

				for(int i=0;i<Number;i++)
				{
					CreateCylinderByShader(i*number);
				}
			}
			else if(UIData.bondtype==UIData.BondType.bbhyperstick)
			{
				if(UIData.secondarystruct)
				{
					bondEPList=MoleculeModel.bondCAList;
				}
				else
				{
					bondEPList=MoleculeModel.bondEPList;
				}
				Debug.Log("DisplayBonds??bondEPList.Count "  + bondEPList.Count);
				int Number=bondEPList.Count/number;

				for(int i=0;i<Number;i++)
				{

						CreateBBCylinderByShader(i*number);
				}
			}
		}

		//Hypersticks
		private void CreateCylinderByShader(int start)
		{
				GameObject Stick;
				int i = start;

				int[] atomsIds = bondEPList[i] as int[];
				Stick=GameObject.CreatePrimitive(PrimitiveType.Cube);
				RuntimePlatform platform = Application.platform;
				switch(platform)
				{
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsWebPlayer:
					case RuntimePlatform.WindowsEditor:
						Stick.renderer.material.shader=Shader.Find("FvNano/Stick HyperBalls D3D");
						break;
					default :
						Stick.renderer.material.shader=Shader.Find("FvNano/Stick HyperBalls OpenGL");
						break;				
				}
				StickUpdate comp = Stick.AddComponent<StickUpdate>();
				comp.atompointer1=(GameObject)MoleculeModel.atoms[atomsIds[0]];
				comp.atompointer2=(GameObject)MoleculeModel.atoms[atomsIds[1]];
				comp.enabled = true;										
				Stick.renderer.material.SetFloat("_Shrink", 0.01f);
				Stick.tag="Club";
				Stick.collider.enabled = false;
				Stick.transform.position = comp.atompointer1.transform.position;
				Stick.transform.parent = BondCubeParent.transform;
		}


		//Cubes
		private void CreateCylinder(int i)
		{		
			
//			GameObject cylinder;
//			MeshFilter filter;
//			Mesh   cylinderMesh;
//			cylinder=GameObject.CreatePrimitive(PrimitiveType.Cube);
//			filter=cylinder.GetComponent<MeshFilter>();
//			cylinderMesh=filter.mesh;
//			filter.mesh=new Mesh();
//			CombineInstance []instances=new CombineInstance[end-start];

			int[] atomsIds = bondEPList[i] as int[];
			GameObject o=GameObject.CreatePrimitive(PrimitiveType.Cube);
			o.renderer.material=(Material)Resources.Load("Materials/CubeBoneMaterial");
			BondCubeUpdate comp = o.AddComponent<BondCubeUpdate>();
			comp.atompointer1=(GameObject)MoleculeModel.atoms[atomsIds[0]];
			comp.atompointer2=(GameObject)MoleculeModel.atoms[atomsIds[1]];
			// o.transform.position = location[0];
			// o.transform.LookAt(location[1]);
			o.transform.localScale=new Vector3(0.1f,0.1f,1f);
			o.tag="Club";
			o.transform.parent = BondCubeParent.transform;
		}
		

		//Billboard hypersticks
		private void CreateBBCylinderByShader(int i)
		{
			

			GameObject Stick;
			if(UIData.toggleClip)
			{
				Stick=Clip4HyperStick.CreateClip();
			}
			else
			{
				Stick=GameObject.CreatePrimitive(PrimitiveType.Plane);
			}
						
			int[] atomsIds = bondEPList[i] as int[];	
					
			Stick.transform.Rotate(new Vector3(0,-180,0));
			Stick.AddComponent("CameraFacingBillboard");
			Stick.GetComponent<CameraFacingBillboard>().cameraToLookAt = GameObject.Find("Camera").camera;
			RuntimePlatform platform = Application.platform;
			switch(platform)
			{
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.WindowsWebPlayer:
				case RuntimePlatform.WindowsEditor:
					Stick.renderer.material.shader=Shader.Find("FvNano/Stick HyperBalls D3D");
					break;
				default :
					Stick.renderer.material.shader=Shader.Find("FvNano/Stick HyperBalls OpenGL");
					break;				
			}
			Stick.AddComponent("StickUpdate");
			
			StickUpdate comp = Stick.GetComponent<StickUpdate>();
			comp.atompointer1=(GameObject)MoleculeModel.atoms[atomsIds[0]];
			comp.atompointer2=(GameObject)MoleculeModel.atoms[atomsIds[1]];
			
			comp.enabled = true;										
			Stick.renderer.material.SetFloat("_Shrink", 0.01f);
			Stick.tag="Club";
			Stick.transform.parent = BondCubeParent.transform;
		}
		
	}

}
