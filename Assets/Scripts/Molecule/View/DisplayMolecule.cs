/// @file DisplayMolecule.cs
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
/// $Id: DisplayMolecule.cs 227 2013-04-07 15:21:09Z baaden $
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

namespace Molecule.View
{
	using UnityEngine;
	using System.Collections;
	using Config;
	using Molecule.Model;
	using Molecule.Control;
	using Molecule.View.DisplayAtom;
	using Molecule.View.DisplayBond;
	using UI;


	public class DisplayMolecule 
	{
		public static void ResetDisplay()
		{
			DestroyObject();
			
			IAtomStyle displayAtom;
			//In case we changed the color of the atoms,
			//we destroy the "permanent" particles and create new ones with the new colors
			if(UIData.isConfirm || UIData.changeStructure)
			{
				DestroyParticles();
				displayAtom = new AtomCubeStyle();
				displayAtom.DisplayAtoms(UIData.AtomType.particleball,true);
			}

			displayAtom=new AtomCubeStyle();
			displayAtom.DisplayAtoms(UIData.atomtype);
			Debug.Log(UIData.atomtype);
			
			if(UIData.bondtype==UIData.BondType.cube||UIData.bondtype==UIData.BondType.hyperstick||UIData.bondtype==UIData.BondType.bbhyperstick)
			{
				IBondStyle displayBond=new BondCubeStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.line)
			{
				IBondStyle displayBond=new BondLineStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.tubestick)
			{
				IBondStyle displayBond=new BondTubeStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.particlestick)
			{
				IBondStyle displayBond=new BondParticleStyle();
				displayBond.DisplayBonds();

			}

			CreatGameObjectArray();
		}
		
		public static void AmendDisplay()
		{
			
			Change(MoleculeModel.Ces,0);
			Change(MoleculeModel.Nes,1);
			Change(MoleculeModel.Oes,2);
			Change(MoleculeModel.Ses,3);
			Change(MoleculeModel.Pes,4);
			Change(MoleculeModel.Hes,5);
			
			Change(MoleculeModel.NOes,6);

		}

		public static void HideAtoms()
		{
			ParticleEffect.radiusFactor = 0;
			if(MoleculeModel.clubs!=null)
			{
				foreach(GameObject box in MoleculeModel.clubs)
				{
					box.renderer.enabled = false;
				}
			}
		}

		public static void ShowAtoms()
		{
			if(UIData.atomtype==UIData.AtomType.particleball)
				ParticleEffect.radiusFactor = GUIMoleculeController.rayon;

			if(MoleculeModel.clubs!=null)
			{
				foreach(GameObject box in MoleculeModel.clubs)
				{
					box.renderer.enabled = true;
				}
			}
		}
		
		public static void ToParticle()
		{
			if(UIData.atomtype!=UIData.AtomType.particleball)
			{
				UIData.atomtype=UIData.AtomType.particleball;
				//UIData.resetDisplay=true;
				UIData.isCubeToSphere=false;
				UIData.isSphereToCube=true;

				ParticleEffect.radiusFactor = GUIMoleculeController.rayon;

				if(MoleculeModel.clubs!=null)//Will be displayed before the atoms are removed
				{
					foreach(GameObject box in MoleculeModel.clubs)
					{
						box.renderer.enabled = false;
					}
				}
				if(MoleculeModel.atoms!=null)//Will be displayed before the atoms are removed
				{
					foreach(GameObject box in MoleculeModel.atoms)
					{
						box.renderer.enabled = false;
					}
				}

				Debug.Log("ToParticle()" );
			}

		}
		
		public static void ToNotParticle(UIData.AtomType previous_type)
		{
			if(previous_type != UIData.AtomType.noatom && UIData.atomtype != previous_type)
			{

				UIData.atomtype = previous_type;
				//UIData.resetDisplay=true;
				UIData.isCubeToSphere=false;
				UIData.isSphereToCube=true;

				ParticleEffect.radiusFactor = 0;

				if(MoleculeModel.clubs!=null)//Will be displayed before the atoms are removed
				{
					foreach(GameObject box in MoleculeModel.clubs)
					{
						box.renderer.enabled = true;
					}
				}
				if(MoleculeModel.atoms!=null)//Will be displayed before the atoms are removed
				{
					foreach(GameObject box in MoleculeModel.atoms)
					{
						box.renderer.enabled = true;
					}
				}

				Debug.Log("ToHyperBall()" );
			}
		}
		
		public static void CubeToSphere()
		{
			
			UIData.resetDisplay=false;
			DestroyObject();
//			Debug.Log("111111111111111111111111111111111111111 " );

			IAtomStyle displayAtom=new AtomSphereStyle();
			displayAtom.DisplayAtoms(UIData.atomtype);			
//			Debug.Log("22222222222222222222222222222222222222  " );
			
			if(UIData.bondtype==UIData.BondType.cube||UIData.bondtype==UIData.BondType.hyperstick||UIData.bondtype==UIData.BondType.bbhyperstick)
			{
				IBondStyle displayBond=new BondCubeStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.line)
			{
				IBondStyle displayBond=new BondLineStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.tubestick)
			{
				IBondStyle displayBond=new BondTubeStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.particlestick)
			{
				IBondStyle displayBond=new BondParticleStyle();
				displayBond.DisplayBonds();

			}			
			CreatGameObjectArray();
		}
		
		
		public static void SphereToCube()
		{
			UIData.resetDisplay=false;
			DestroyObject();
			IAtomStyle displayAtom;

			Debug.Log("UIData.atomtype :: "+UIData.atomtype);
			displayAtom = new AtomCubeStyle();
			displayAtom.DisplayAtoms(UIData.atomtype);
			
			if(UIData.bondtype==UIData.BondType.cube||UIData.bondtype==UIData.BondType.hyperstick||UIData.bondtype==UIData.BondType.bbhyperstick)
			{
				IBondStyle displayBond=new BondCubeStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.line)
			{
				IBondStyle displayBond=new BondLineStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.tubestick)
			{
				IBondStyle displayBond=new BondTubeStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.particlestick)
			{
				IBondStyle displayBond=new BondParticleStyle();
				displayBond.DisplayBonds();

			}
			CreatGameObjectArray();
			Debug.Log("Exiting :: SphereToCube");

		}
		
		public static void ResetBondDisplay()
		{
			UIData.resetBondDisplay=false;
			
			DestroyBondObject();
			if(UIData.bondtype==UIData.BondType.cube||UIData.bondtype==UIData.BondType.hyperstick||UIData.bondtype==UIData.BondType.bbhyperstick)
			{
				IBondStyle displayBond=new BondCubeStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.line)
			{
				IBondStyle displayBond=new BondLineStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.tubestick)
			{
				IBondStyle displayBond=new BondTubeStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.particlestick)
			{
				IBondStyle displayBond=new BondParticleStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.nobond)
			{
				
			}
			CreatBondGameObjectArray();			
	
		}
		
		private static void Change(GameObject[] oes,int type)
		{
			foreach(GameObject o in oes)
			{
				BallUpdate comp = o.GetComponent<BallUpdate>();
				switch(type)
				{
					case 0:
						comp.atomcolor = MoleculeModel.carbonColor;
						comp.SetRayonFactor((MoleculeModel.carbonScale)/100);
						break;
					case 1:
						comp.atomcolor = MoleculeModel.nitrogenColor;
						comp.SetRayonFactor((MoleculeModel.nitrogenScale)/100);
						
						break;
					case 2:
						comp.atomcolor = MoleculeModel.oxygenColor;
						comp.SetRayonFactor((MoleculeModel.oxygenScale)/100);
						
						break;
					case 3:
						comp.atomcolor = MoleculeModel.sulphurColor;
						comp.SetRayonFactor((MoleculeModel.sulphurScale)/100);
						
						break;
					case 4:
						comp.atomcolor = MoleculeModel.phosphorusColor;
						comp.SetRayonFactor((MoleculeModel.phosphorusScale)/100);
						
						break;
					case 5:
						comp.atomcolor = MoleculeModel.hydrogenColor;
						comp.SetRayonFactor((MoleculeModel.hydrogenScale)/100);
						
						break;						
					case 6:
						comp.atomcolor = MoleculeModel.unknownColor;
						comp.SetRayonFactor((MoleculeModel.unknownScale)/100);
						
						break;
					default :break;
				}
				MoleculeModel.p[comp.number].color = comp.atomcolor;	
			}
			
		}
		
		
		public static void Display()
		{
			//UIData.EnableUpdate=false;
			IAtomStyle displayAtom;
			if(UIData.isSphereToCube)
			{
				displayAtom=new AtomCubeStyle();
			}
			
			else
			{
				displayAtom=new AtomSphereStyle();
			}
			displayAtom.DisplayAtoms(UIData.atomtype);
			if(UIData.bondtype==UIData.BondType.cube||UIData.bondtype==UIData.BondType.hyperstick||UIData.bondtype==UIData.BondType.bbhyperstick)
			{
				IBondStyle displayBond=new BondCubeStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.line)
			{
				IBondStyle displayBond=new BondLineStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.tubestick)
			{
				IBondStyle displayBond=new BondTubeStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.particlestick)
			{
				IBondStyle displayBond=new BondParticleStyle();
				displayBond.DisplayBonds();

			}
			CreatGameObjectArray();
		
			
			UIData.hasMoleculeDisplay=true;
			//UIData.EnableUpdate=true;
			

		}
		
		public static void DisplayFieldLine()
		{
			//FieldLineStyle fieldlinestyle=new FieldLineStyle();
			FieldLineStyle.DisplayFieldLine();				
		}
		
		private static void CreatGameObjectArray()
		{
			MoleculeModel.Ces=GameObject.FindGameObjectsWithTag("C");
			MoleculeModel.Nes=GameObject.FindGameObjectsWithTag("N");
			MoleculeModel.Oes=GameObject.FindGameObjectsWithTag("O");
			MoleculeModel.Ses=GameObject.FindGameObjectsWithTag("S");
			MoleculeModel.Pes=GameObject.FindGameObjectsWithTag("P");
			MoleculeModel.Hes=GameObject.FindGameObjectsWithTag("H");			
			MoleculeModel.NOes=GameObject.FindGameObjectsWithTag("X");
			MoleculeModel.clubs=GameObject.FindGameObjectsWithTag("Club");
			
			
			// MoleculeModel.boxes=ControlMolecule.SetBoxes(MoleculeModel.Ces,MoleculeModel.Nes,MoleculeModel.Oes,
			// MoleculeModel.Ses, MoleculeModel.Pes,MoleculeModel.Hes,MoleculeModel.NOes);
			
		}
		
		public static void DestroyFieldLine()
		{
			GameObject FieldLineManager=GameObject.Find("FieldLineManager");
			FieldLineModel Line=FieldLineManager.transform.GetComponent<FieldLineModel>();
			Line.killCurrentEffects();
			MoleculeModel.FieldLineFileExist=false;
		}

		public static void DestroyObject()
		{
			GameObject 	SpriteManager;
			if(!GameObject.Find("SpriteManager"))
			{
				SpriteManager=new GameObject();
				SpriteManager.name="SpriteManager";
			}
			else
			{
				SpriteManager=GameObject.Find("SpriteManager");
			}
			SpriteManager.GetComponent <MeshRenderer>().enabled=false;
			
			if(!UIData.openBound||UIData.atomtype!=UIData.AtomType.particleball)//Disappear the BoxBound;
			{
				GameObject[] TransparentCube;
				TransparentCube = GameObject.FindGameObjectsWithTag("TransparentCube");
				for(int k=0;k<TransparentCube.Length;k++)
				{
					Object.Destroy(TransparentCube[k]);
				}
				UIData.openBound=false;
			}


			if(UIData.atomtype!=UIData.AtomType.particleball || UIData.isclear || UIData.changeStructure)
			{

				Debug.Log("Entering :: Destroy ALL");
				if(MoleculeModel.atoms!=null)//Will be displayed before the atoms are removed
				{
					foreach(GameObject box in MoleculeModel.atoms)
					{
						Object.DestroyImmediate(box,true);
					}
					MoleculeModel.atoms.Clear();
					MoleculeModel.atoms=null;
				}
				if(MoleculeModel.clubs!=null)//Will be displayed before the atoms are removed
				{
					foreach(GameObject box in MoleculeModel.clubs)
					{
						Object.DestroyImmediate(box,true);
					}
					MoleculeModel.clubs=null;
				}

				
				// if(MoleculeModel.Ces!=null)//Will be displayed before the atoms are removed
				// {
				// 	foreach(GameObject box in MoleculeModel.Ces)
				// 	{
				// 		Object.DestroyImmediate(box,true);//Destroy function has delay，so you must use DestroyImmediate function
				// 	}
				// 	MoleculeModel.Ces=null;
				// }
				
				// if(MoleculeModel.Nes!=null)//Will be displayed before the atoms are removed
				// {
				// 	foreach(GameObject box in MoleculeModel.Nes)
				// 	{
				// 		Object.DestroyImmediate(box,true);
				// 	}
				// 	MoleculeModel.Nes=null;
				// }
				
				// if(MoleculeModel.Oes!=null)//Will be displayed before the atoms are removed
				// {
				// 	foreach(GameObject box in MoleculeModel.Oes)
				// 	{
				// 		Object.DestroyImmediate(box,true);
				// 	}
				// 	MoleculeModel.Oes=null;
				// }
				
				// if(MoleculeModel.Ses!=null)//Will be displayed before the atoms are removed
				// {
				// 	foreach(GameObject box in MoleculeModel.Ses)
				// 	{
				// 		Object.DestroyImmediate(box,true);
				// 	}
				// 	MoleculeModel.Ses=null;
				// }
				
				// if(MoleculeModel.Pes!=null)//Will be displayed before the atoms are removed
				// {
				// 	foreach(GameObject box in MoleculeModel.Pes)
				// 	{
				// 		Object.DestroyImmediate(box,true);
				// 	}
				// 	MoleculeModel.Pes=null;
				// }
				// if(MoleculeModel.Hes!=null)//Will be displayed before the atoms are removed
				// {
				// 	foreach(GameObject box in MoleculeModel.Hes)
				// 	{
				// 		Object.DestroyImmediate(box,true);
				// 	}
				// 	MoleculeModel.Hes=null;
				// }
				// if(MoleculeModel.NOes!=null)//Will be displayed before the atoms are removed
				// {
				// 	foreach(GameObject box in MoleculeModel.NOes)
				// 	{
				// 		Object.DestroyImmediate(box,true);
				// 	}
				// 	MoleculeModel.Nes=null;
				// }
				
						
				// if(MoleculeModel.boxes!=null)//Will be displayed before the atoms are removed
				// {
				// 	foreach(GameObject box in MoleculeModel.boxes)
				// 	{
				// 		Object.DestroyImmediate(box,true);
				// 	}
				// 	MoleculeModel.boxes=null;
				// }
			}

			//21_08_2012 Alex : we want to keep the particles all the time ! Just hide it.
			//DestroyParticles();
			ParticleEffect.radiusFactor = 0;
		}

		public static void DestroyParticles()
		{
			GameObject ParticleManager=GameObject.Find("ParticleManager");
			ParticleEffect particleeffect=ParticleManager.transform.GetComponent<ParticleEffect>();
			particleeffect.killCurrentEffects();
		}

		
		public static void DestroyBondObject()
		{
			if(MoleculeModel.clubs!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.clubs)
				{
					Object.DestroyImmediate(box,true);
				}
				MoleculeModel.clubs=null;
			}
		}


		public static void DestroySurfaces()
		{
			GameObject [] SurfaceManager = GameObject.FindGameObjectsWithTag("SurfaceManager");
			foreach (GameObject Surface in SurfaceManager)
				Object.Destroy(Surface);
		}

		public static void DestroyElectIso()
		{
			GameObject [] ElecIso = GameObject.FindGameObjectsWithTag("Elect_iso_positive");
			foreach (GameObject Surface in ElecIso)
				Object.Destroy(Surface);

			ElecIso = GameObject.FindGameObjectsWithTag("Elect_iso_negative");
			foreach (GameObject Surface in ElecIso)
				Object.Destroy(Surface);
		}


		private static void CreatBondGameObjectArray()
		{
			MoleculeModel.clubs=GameObject.FindGameObjectsWithTag("Club");
		}
		public static void DeleteAllPhysics()
		{
			/*
			if(MoleculeModel.clubs!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.clubs)
				{
					Object.DestroyImmediate(box,true);
				}
				MoleculeModel.clubs=null;
			}
			*/
			UIData.resetInteractive=false;
			
			if(MoleculeModel.Ces!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Ces)
				{
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
					
				}
			}
			
			if(MoleculeModel.Nes!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Nes)
				{
					
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}
			
			if(MoleculeModel.Oes!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Oes)
				{
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}
			
			if(MoleculeModel.Ses!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Ses)
				{

					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}
			
			if(MoleculeModel.Pes!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Pes)
				{
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}
			if(MoleculeModel.Hes!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Hes)
				{
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}

			if(MoleculeModel.NOes!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.NOes)
				{
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}

		}
		public static void AddAllPhysics()
		{
			
			UIData.resetInteractive=false;
//			Debug.Log("AddAllPhysics");
			if(MoleculeModel.Ces!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Ces)
				{

					if(box&&box.GetComponent <Rigidbody>()==null)
					{
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null)
					{
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
			
			if(MoleculeModel.Nes!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Nes)
				{

					if(box&&box.GetComponent <Rigidbody>()==null)
					{
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null)
					{
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
			
			if(MoleculeModel.Oes!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Oes)
				{

					if(box&&box.GetComponent <Rigidbody>()==null)
					{
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null)
					{
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
			
			if(MoleculeModel.Ses!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Ses)
				{

					if(box&&box.GetComponent <Rigidbody>()==null)
					{
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null)
					{
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
			
			if(MoleculeModel.Pes!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Pes)
				{

					if(box&&box.GetComponent <Rigidbody>()==null)
					{
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null)
					{
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
			if(MoleculeModel.Hes!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.Hes)
				{

					if(box&&box.GetComponent <Rigidbody>()==null)
					{
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null)
					{
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}

			if(MoleculeModel.NOes!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject box in MoleculeModel.NOes)
				{

					if(box&&box.GetComponent <Rigidbody>()==null)
					{
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null)
					{
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}

		}
		
		public static void AddCombineMesh()
		{
			UIData.resetMeshcombine=false;
			GameObject 	MeshCombineManager;
			if(!GameObject.Find("MeshCombineManager"))
			{

				MeshCombineManager=GameObject.CreatePrimitive(PrimitiveType.Cube);
				MeshCombineManager.name="MeshCombineManager";

			}
			else
			{
				MeshCombineManager=GameObject.Find("MeshCombineManager");
			}
			
			if(MoleculeModel.Ces!=null)//Will be displayed before the atoms are removed
			{
				foreach(GameObject Atom in MoleculeModel.Oes)
				{
					Atom.transform.parent=MeshCombineManager.transform;

				}
				Meshcombine combineComp = MeshCombineManager.GetComponent<Meshcombine>();
    				combineComp.GoOn();

				
			}


		}
		
		public static void ClearMemory()
		{
			MoleculeModel.atomsLocationlist=null;
			MoleculeModel.atomsTypelist=null;
			MoleculeModel.atomsResnamelist=null;
			MoleculeModel.CSidList=null;
			
			MoleculeModel.CSLabelList=null;
			MoleculeModel.CSRadiusList=null;
			MoleculeModel.CSColorList=null;
			MoleculeModel.CSSGDList=null;
			MoleculeModel.bondEPList=null;
			MoleculeModel.bondCAList=null;
			MoleculeModel.bondList = null;//The list of the bond by position and rotation.
			
			MoleculeModel.CaSplineList=null;
			MoleculeModel.CaSplineTypeList = null;
			MoleculeModel.CaSplineChainList = null;
			MoleculeModel.CatomsLocationlist = null;//CA atoms coordinates
			
			MoleculeModel.FieldLineList=null;
			MoleculeModel.FieldLineDist = null;// Field lines distance arrays
			
			MoleculeModel.BFactorList = null;
		}
		
		public static void DeleteCombineMesh()
		{
			
		}
		
	
	}
	
	
}
