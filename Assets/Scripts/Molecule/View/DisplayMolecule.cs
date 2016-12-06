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
/// $Id: DisplayMolecule.cs 672 2014-10-02 08:13:56Z tubiana $
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

namespace Molecule.View {
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using Config;
	using Molecule.Model;
	using Molecule.Control;
	using Molecule.View.DisplayAtom;
	using Molecule.View.DisplayBond;
	using UI;


	public class DisplayMolecule {
		
		public static void ResetDisplay() {
			DestroyObject();
			
			IAtomStyle displayAtom;
			// In case we changed the color of the atoms,
			// we destroy the "permanent" particles and create new ones with the new colors
			if(UIData.isConfirm || UIData.changeStructure) {
//				DestroyParticles();
				displayAtom = new AtomCubeStyle();
				displayAtom.DisplayAtoms(UIData.atomtype, true);
//				displayAtom.DisplayAtoms(UIData.AtomType.particleball,true);
			}
			displayAtom=new AtomCubeStyle();
			
			displayAtom.DisplayAtoms(UIData.atomtype);
			Debug.Log(UIData.atomtype);
			
			// This fixes a nasty bug that made carbon alpha chains/entire molecules disappear when switching between the two in sphere mode
			if(UIData.atomtype == UIData.AtomType.sphere) {
				UIData.isCubeToSphere = true;
				UIData.isSphereToCube = false;
			} else if (UIData.atomtype == UIData.AtomType.cube) {
				UIData.isCubeToSphere = false ;
				UIData.isSphereToCube = true ;
			}
			
			if (UIData.bondtype==UIData.BondType.cube||UIData.bondtype==UIData.BondType.hyperstick||UIData.bondtype==UIData.BondType.bbhyperstick) {
				IBondStyle displayBond=new BondCubeStyle();
				displayBond.DisplayBonds();
			} else if (UIData.bondtype==UIData.BondType.line) {
				IBondStyle displayBond=new BondLineStyle();
				displayBond.DisplayBonds();
			} else if (UIData.bondtype==UIData.BondType.tubestick) {
				IBondStyle displayBond=new BondTubeStyle();
				displayBond.DisplayBonds();
			} else if (UIData.bondtype==UIData.BondType.particlestick) {
				IBondStyle displayBond=new BondParticleStyle();
				displayBond.DisplayBonds();
			}
			CreatGameObjectArray();
		}
		
		/// <summary>
		/// Gets the list of active managers based on the atom type provided and based on the bond type in UIData.
		/// </summary>
		/// <returns>
		/// The list of managers. A List<GenericManager> object that should contains the "atom" manager in position 0 and the "bond" manager in position 1 (if there is one).
		/// </returns>
		public static List<GenericManager> GetManagers() {
			UIData.AtomType aType = UIData.atomtype;
			List<GenericManager> managerList = new List<GenericManager>();
			if(aType == UIData.AtomType.hyperball) {  //||  UIData.bondtype == UIData.BondType.hyperstick) {
				GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
				HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
				managerList.Add(hbManager);
			}else if (aType == UIData.AtomType.sphere) {
				GameObject spManagerObj = GameObject.FindGameObjectWithTag("SphereManager");
				SphereManager spManager = spManagerObj.GetComponent<SphereManager>();
				managerList.Add(spManager);
			} else if (aType == UIData.AtomType.cube) {
				GameObject cubeManagerObj = GameObject.FindGameObjectWithTag("CubeManager");
				CubeManager cubeManager = cubeManagerObj.GetComponent<CubeManager>();
				managerList.Add(cubeManager);
			} else if (aType == UIData.AtomType.particleball) {
				GameObject psObj = GameObject.FindGameObjectWithTag("ShurikenParticleManager");
				ShurikenParticleManager shManager =  psObj.GetComponent<ShurikenParticleManager>();
				managerList.Add(shManager);
			}
			if (UIData.bondtype == UIData.BondType.line) {
				GameObject lineManagerObj = GameObject.FindGameObjectWithTag("LineManager");
				LineManager lineManager = lineManagerObj.GetComponent<LineManager>();
				managerList.Add(lineManager);
			} else if (UIData.bondtype == UIData.BondType.cube) {
				GameObject cubeBondManagerObj = GameObject.FindGameObjectWithTag("CubeBondManager");
				CubeBondManager cubeBondManager = cubeBondManagerObj.GetComponent<CubeBondManager>();
				managerList.Add(cubeBondManager);
			} else if (UIData.bondtype == UIData.BondType.hyperstick) {
				GameObject hStickManagerObj = GameObject.FindGameObjectWithTag("HStickManager");
				HStickManager hStickManager = hStickManagerObj.GetComponent<HStickManager>();
				managerList.Add(hStickManager);
			}
			return managerList;
		}
		
		public static void ToggleDistanceCueing(bool enabling) {
			List<GenericManager> managers = GetManagers();
			foreach(GenericManager manager in managers)
				manager.ToggleDistanceCueing(enabling);
		}
		
		public static void DestroyAtomsAndBonds() {
			List<GenericManager> managers = GetManagers();
			foreach(GenericManager manager in managers)
				manager.DestroyAll();
		}
		
		public static void InitManagers() {
			List<GenericManager> managers = GetManagers();
			foreach(GenericManager manager in managers)
				manager.Init();
		}

		public static void HideAtoms() {
			List<GenericManager> managers = GetManagers();
			foreach(GenericManager manager in managers)
				manager.DisableRenderers();
		}

		public static void ShowAtoms() {
			List<GenericManager> managers = GetManagers();
			foreach(GenericManager manager in managers)
				manager.EnableRenderers();
		}
		
		public static void ToParticle() { // Actually buggy with hyperstick since they don't use "enable/disable renderer" but are destroyed and recreated
			if(UIData.atomtype!=UIData.AtomType.particleball) {
				UIData.atomtype = UIData.AtomType.particleball;
				UIData.bondtype = UIData.BondType.nobond ;
				UIData.resetBondDisplay = true;
				UIData.resetDisplay=true;
				UIData.isCubeToSphere=false;
				UIData.isSphereToCube=true;

//				ParticleEffect.radiusFactor = GUIMoleculeController.rayon;
				ShurikenParticleManager.radiusFactor = BallUpdate.radiusFactor;
				GameObject npmObject = GameObject.FindGameObjectWithTag("ShurikenParticleManager");
				ShurikenParticleManager shManager = npmObject.GetComponent<ShurikenParticleManager>();
				shManager.enabled = true;
				shManager.pSystem.GetComponent<Renderer>().enabled = true;

				Debug.Log("ToParticle()" );
			}
		}
		
		public static void ToNotParticle(UIData.AtomType previousType, UIData.BondType previousBondType) {
			if(previousType != UIData.AtomType.noatom && UIData.atomtype != previousType) {
				GameObject shObject = GameObject.FindGameObjectWithTag("ShurikenParticleManager");
				ShurikenParticleManager shManager = shObject.GetComponent<ShurikenParticleManager>();
				shManager.pSystem.GetComponent<Renderer>().enabled = false;
				shManager.enabled = false;
				
				UIData.atomtype = previousType;
				UIData.bondtype = previousBondType;
				UIData.resetBondDisplay = true ;
				UIData.resetDisplay=true;
				if(UIData.atomtype == UIData.AtomType.sphere){
					UIData.isCubeToSphere=true;
					UIData.isSphereToCube=false;
				}
				else{
					UIData.isCubeToSphere=false;
					UIData.isSphereToCube=true;
				}

				if(UIData.atomtype == UIData.AtomType.hyperball) {
					GameObject hbmObject = GameObject.FindGameObjectWithTag("HBallManager");
					HBallManager hbManager = hbmObject.GetComponent<HBallManager>();
					BallUpdate.resetRadii = true;
					hbManager.EnableRenderers();
					hbManager.enabled = true;
				}
				
				Debug.Log("ToHyperBall()" );
				Debug.Log(UIData.atomtype.ToString());
				Debug.Log(UIData.bondtype.ToString());
			}
		}
		
		public static void CubeToSphere() {
			
			UIData.resetDisplay=false;
//			DestroyObject();
			HideObject();
			if(UIData.atomtype == UIData.AtomType.sphere){
				GameObject spmObject = GameObject.FindGameObjectWithTag("SphereManager");
				SphereManager spManager = spmObject.GetComponent<SphereManager>();
				spManager.EnableRenderers();
			}
			
			if(!UIData.isSphereLoaded){
				IAtomStyle displayAtom;
				
				Debug.Log("UIData.atomtype :: "+UIData.atomtype);
				displayAtom = new AtomSphereStyle();
				displayAtom.DisplayAtoms(UIData.atomtype);	
			}
			
			if(UIData.bondtype==UIData.BondType.cube||UIData.bondtype==UIData.BondType.hyperstick||UIData.bondtype==UIData.BondType.bbhyperstick) {
				IBondStyle displayBond=new BondCubeStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.line) {
				IBondStyle displayBond=new BondLineStyle();
				displayBond.DisplayBonds();

			}
			else if(UIData.bondtype==UIData.BondType.tubestick) {
				IBondStyle displayBond=new BondTubeStyle();
				displayBond.DisplayBonds();
			}
			else if(UIData.bondtype==UIData.BondType.particlestick) {
				IBondStyle displayBond=new BondParticleStyle();
				displayBond.DisplayBonds();
			}			
			CreatGameObjectArray();
			BallUpdate.resetRadii = true;
			//BallUpdate.resetColors = true;
			Debug.Log("Exiting :: CubeToSphere");
		}
		
		
		public static void SphereToCube() {
			UIData.resetDisplay=false;
//			DestroyObject();
			HideObject();
			if(UIData.atomtype == UIData.AtomType.cube){
				GameObject cbmObject = GameObject.FindGameObjectWithTag("CubeManager");
				CubeManager cbManager = cbmObject.GetComponent<CubeManager>();
				cbManager.EnableRenderers();
				
				if(!UIData.isCubeLoaded){
					IAtomStyle displayAtom;

					Debug.Log("UIData.atomtype :: "+UIData.atomtype);
					displayAtom = new AtomCubeStyle();
					displayAtom.DisplayAtoms(UIData.atomtype);
				}
			}
			else if(UIData.atomtype == UIData.AtomType.hyperball){
				GameObject hbmObject = GameObject.FindGameObjectWithTag("HBallManager");
				HBallManager hbManager = hbmObject.GetComponent<HBallManager>();
				hbManager.EnableRenderers();
				
				if(!UIData.isHBallLoaded){
					IAtomStyle displayAtom;

					Debug.Log("UIData.atomtype :: "+UIData.atomtype);
					displayAtom = new AtomCubeStyle();
					displayAtom.DisplayAtoms(UIData.atomtype);
				}
			}
			
			
			if(UIData.bondtype==UIData.BondType.cube||UIData.bondtype==UIData.BondType.hyperstick||UIData.bondtype==UIData.BondType.bbhyperstick) {
				IBondStyle displayBond=new BondCubeStyle();
				displayBond.DisplayBonds();
			}
			else if(UIData.bondtype==UIData.BondType.line) {
				IBondStyle displayBond=new BondLineStyle();
				displayBond.DisplayBonds();
			}
			else if(UIData.bondtype==UIData.BondType.tubestick)	{
				IBondStyle displayBond=new BondTubeStyle();
				displayBond.DisplayBonds();
			}
			else if(UIData.bondtype==UIData.BondType.particlestick)	{
				IBondStyle displayBond=new BondParticleStyle();
				displayBond.DisplayBonds();
			}
			CreatGameObjectArray();
			BallUpdate.resetRadii = true;
			//BallUpdate.resetColors = true;
			Debug.Log("Exiting :: SphereToCube");
		}
		
		public static void ResetBondDisplay() {
			UIData.resetBondDisplay=false;
			
			DestroyBondObject();
			if(UIData.bondtype==UIData.BondType.cube||UIData.bondtype==UIData.BondType.hyperstick||UIData.bondtype==UIData.BondType.bbhyperstick) {
				IBondStyle displayBond=new BondCubeStyle();
				displayBond.DisplayBonds();
			}
			else if(UIData.bondtype==UIData.BondType.line) {
				IBondStyle displayBond=new BondLineStyle();
				displayBond.DisplayBonds();
			}
			else if(UIData.bondtype==UIData.BondType.tubestick)	{
				IBondStyle displayBond=new BondTubeStyle();
				displayBond.DisplayBonds();
			}
			else if(UIData.bondtype==UIData.BondType.particlestick)	{
				IBondStyle displayBond=new BondParticleStyle();
				displayBond.DisplayBonds();
			}
//			else if(UIData.bondtype==UIData.BondType.nobond) {}
			CreatBondGameObjectArray();
			
			if(UIData.bondtype==UIData.BondType.cube||UIData.bondtype==UIData.BondType.hyperstick||UIData.bondtype==UIData.BondType.bbhyperstick)
			{
					GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
					HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
					hbManager.findBonds();
			}
		}

		
		public static void Display() {
//			UIData.EnableUpdate=false;
			IAtomStyle displayAtom;
			if(UIData.isSphereToCube) {
				displayAtom=new AtomCubeStyle();
				Debug.Log("DisplayMolecule.Display(): New atom cube style");
			}
			
			else {
				displayAtom=new AtomSphereStyle();
				Debug.Log("DisplayMolecule.Display(): New atom sphere style");
			}
			Debug.Log("DisplayAtoms here DisplayAtoms here DisplayAtoms here DisplayAtoms here");
			displayAtom.DisplayAtoms(UIData.atomtype);
			if(UIData.bondtype==UIData.BondType.cube||UIData.bondtype==UIData.BondType.hyperstick||UIData.bondtype==UIData.BondType.bbhyperstick) {
				IBondStyle displayBond=new BondCubeStyle();
				displayBond.DisplayBonds();
			}
			else if(UIData.bondtype==UIData.BondType.line) {
				IBondStyle displayBond=new BondLineStyle();
				displayBond.DisplayBonds();
			}
			else if(UIData.bondtype==UIData.BondType.tubestick) {
				IBondStyle displayBond=new BondTubeStyle();
				displayBond.DisplayBonds();
			}
			else if(UIData.bondtype==UIData.BondType.particlestick) {
				IBondStyle displayBond=new BondParticleStyle();
				displayBond.DisplayBonds();
			}
			CreatGameObjectArray();
			CheckResidues();
		
			if (GUIMoleculeController.HYPERBALLSDEFAULT){
				GUIMoleculeController.toggle_NA_HBALLSMOOTH = !GUIMoleculeController.toggle_NA_HBALLSMOOTH;
				UIData.hballsmoothmode = GUIMoleculeController.toggle_NA_HBALLSMOOTH;
			}
 
			UIData.hasMoleculeDisplay=true;
//			UIData.EnableUpdate=true;

		}
		
		public static void DisplayFieldLine() {
//			FieldLineStyle fieldlinestyle=new FieldLineStyle();
			FieldLineStyle.DisplayFieldLine();				
		}
		
		private static void CreatGameObjectArray() {
			MoleculeModel.atomsByChar.Clear();
			MoleculeModel.atomsByChar.Add("C", GameObject.FindGameObjectsWithTag("C"));
			MoleculeModel.atomsByChar.Add("N", GameObject.FindGameObjectsWithTag("N"));
			MoleculeModel.atomsByChar.Add("O", GameObject.FindGameObjectsWithTag("O"));
			MoleculeModel.atomsByChar.Add("S", GameObject.FindGameObjectsWithTag("S"));
			MoleculeModel.atomsByChar.Add("P", GameObject.FindGameObjectsWithTag("P"));
			MoleculeModel.atomsByChar.Add("H", GameObject.FindGameObjectsWithTag("H"));			
			MoleculeModel.atomsByChar.Add("X", GameObject.FindGameObjectsWithTag("X"));
			MoleculeModel.clubs = GameObject.FindGameObjectsWithTag("Club");
			
//			MoleculeModel.boxes=ControlMolecule.SetBoxes(MoleculeModel.Ces,MoleculeModel.Nes,MoleculeModel.Oes,
//			MoleculeModel.Ses, MoleculeModel.Pes,MoleculeModel.Hes,MoleculeModel.NOes);
		}
		
		private static void CheckResidues() {
			if(!UIData.hasResidues)
				return;
			Debug.Log("Looking for protonated HIS"); // Doesn't handle N-ter and C-ter HIP
			for(int i = 0; i < MoleculeModel.atomsResnamelist.Count; i++){
				if(MoleculeModel.atomsResnamelist[i] == "HIS"){
					if(MoleculeModel.residues[MoleculeModel.residueIds[i]].Count == 18){ // 8 H and 10 others in protonated HIS
						MoleculeModel.atomsResnamelist[i] = "HIP";
					}
				}
			}
			foreach(string res in MoleculeModel.atomsResnamelist)
				if(!MoleculeModel.existingRes.Contains(res))
					MoleculeModel.existingRes.Add(res);
			
			MoleculeModel.existingRes.Sort();
				
		}
		
		public static void DestroyFieldLine() {
			GameObject FieldLineManager=GameObject.Find("FieldLineManager");
			FieldLineModel Line=FieldLineManager.transform.GetComponent<FieldLineModel>();
			Line.killCurrentEffects();
			MoleculeModel.fieldLineFileExists = false;
		}
		
		public static void HideObject() {
			if(UIData.atomtype != UIData.AtomType.hyperball){
				GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
				HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
				hbManager.DisableRenderers();
			}
			if(UIData.atomtype != UIData.AtomType.sphere){
				GameObject spManagerObj = GameObject.FindGameObjectWithTag("SphereManager");
				SphereManager spManager = spManagerObj.GetComponent<SphereManager>();
				spManager.DisableRenderers();
			}
			if(UIData.atomtype != UIData.AtomType.cube){
				GameObject cbManagerObj = GameObject.FindGameObjectWithTag("CubeManager");
				CubeManager cbManager = cbManagerObj.GetComponent<CubeManager>();
				cbManager.DisableRenderers();
			}
			
			DestroyBondObject();
		}
		
		public static void DestroyObject() {
			
			if(MoleculeModel.atomsByChar!=null) {
				foreach(string key in MoleculeModel.atomsByChar.Keys)
					foreach(GameObject box in MoleculeModel.atomsByChar[key])
						Object.DestroyImmediate(box,true);
			}
			
			DestroyBondObject();
			
			GameObject 	SpriteManager;
			if(!GameObject.Find("SpriteManager")) {
				SpriteManager=new GameObject();
				SpriteManager.name="SpriteManager";
			}
			else
				SpriteManager=GameObject.Find("SpriteManager");

			SpriteManager.GetComponent <MeshRenderer>().enabled=false;
			
			//Disappear the BoxBound;
			if(!UIData.openBound||UIData.atomtype!=UIData.AtomType.particleball) {
				GameObject[] TransparentCube;
				TransparentCube = GameObject.FindGameObjectsWithTag("TransparentCube");
				for(int k=0;k<TransparentCube.Length;k++)
					Object.Destroy(TransparentCube[k]);

				UIData.openBound=false;
			}

//			if(UIData.atomtype!=UIData.AtomType.particleball || UIData.isclear || UIData.changeStructure) {
			if(UIData.isclear || UIData.changeStructure) {
				
				HideAtoms();
				Debug.Log("AtomType:");
				Debug.Log(UIData.atomtype.ToString());
				Debug.Log(UIData.bondtype.ToString());
				
				Debug.Log("Entering :: Destroy ALL");
				Debug.Log(UIData.atomtype);
				 
				//Will be displayed before the atoms are removed
				if(MoleculeModel.atoms!=null) {
					foreach(GameObject box in MoleculeModel.atoms)
						Object.DestroyImmediate(box,true);

					MoleculeModel.atoms.Clear();
					//MoleculeModel.atoms=null;
				}
				//Will be displayed before the atoms are removed
				DestroyBondObject();
			}
		}
		
		public static void DestroyBondObject() {
			//Will be displayed before the atoms are removed
			if(MoleculeModel.clubs!=null) {
				foreach(GameObject box in MoleculeModel.clubs)
					Object.DestroyImmediate(box,true);

				//MoleculeModel.clubs=null;
			}
		}

		public static void DestroySurfaces() {
			GameObject [] SurfaceManager = GameObject.FindGameObjectsWithTag("SurfaceManager");
			foreach (GameObject Surface in SurfaceManager)
				Object.Destroy(Surface);
		}
		
		public static void DestroyRingBlending() {
			GameObject [] PaperChains = GameObject.FindGameObjectsWithTag("RingBlending");
			foreach (GameObject PaperChain in PaperChains)
				Object.Destroy(PaperChain);
		}

		public static void DestroySugarRibbons() {
			GameObject [] SugarRibbons;
			SugarRibbons = GameObject.FindGameObjectsWithTag("SugarRibbons_RING_BIG");
			foreach (GameObject SugarRibbon in SugarRibbons)
				Object.Destroy(SugarRibbon);
			SugarRibbons = GameObject.FindGameObjectsWithTag("SugarRibbons_RING_little");
			foreach (GameObject SugarRibbon in SugarRibbons)
				Object.Destroy(SugarRibbon);
					SugarRibbons = GameObject.FindGameObjectsWithTag("SugarRibbons_BOND");
			foreach (GameObject SugarRibbon in SugarRibbons)
				Object.Destroy(SugarRibbon);
		}


		public static void DestroyOxySpheres(){
			GameObject [] oxyspheres = GameObject.FindGameObjectsWithTag("OxySphere");
			foreach (GameObject oxysphere in oxyspheres)
				Object.Destroy(oxysphere);
		}
		
		public static void DestroyElectIso() {
			GameObject [] ElecIso = GameObject.FindGameObjectsWithTag("Elect_iso_positive");
			foreach (GameObject Surface in ElecIso)
				Object.Destroy(Surface);

			ElecIso = GameObject.FindGameObjectsWithTag("Elect_iso_negative");
			foreach (GameObject Surface in ElecIso)
				Object.Destroy(Surface);
		}


		private static void CreatBondGameObjectArray() {
			MoleculeModel.clubs=GameObject.FindGameObjectsWithTag("Club");
		}
		
		public static void DeleteAllPhysics() {
			UIData.resetInteractive=false;
			if(MoleculeModel.atomsByChar!=null) {
				foreach(string key in MoleculeModel.atomsByChar.Keys){
					foreach(GameObject box in MoleculeModel.atomsByChar[key]) {
						if(box&&box.GetComponent <SpringJoint>())
							Object.Destroy (box.GetComponent <SpringJoint>());
						if(box&&box.GetComponent <Rigidbody>())
							Object.Destroy (box.GetComponent <Rigidbody>());
					}
				}
			}

/*
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Ces!=null) {
				foreach(GameObject box in MoleculeModel.Ces) {
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Nes!=null)	{
				foreach(GameObject box in MoleculeModel.Nes) {
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Oes!=null)	{
				foreach(GameObject box in MoleculeModel.Oes) {
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Ses!=null)	{
				foreach(GameObject box in MoleculeModel.Ses) {
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Pes!=null) {
				foreach(GameObject box in MoleculeModel.Pes) {
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Hes!=null) {
				foreach(GameObject box in MoleculeModel.Hes) {
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.NOes!=null) {
				foreach(GameObject box in MoleculeModel.NOes) {
					if(box&&box.GetComponent <SpringJoint>())
						Object.Destroy (box.GetComponent <SpringJoint>());
					if(box&&box.GetComponent <Rigidbody>())
						Object.Destroy (box.GetComponent <Rigidbody>());
				}
			}
*/
		}
		
		public static void AddAllPhysics() {
			UIData.resetInteractive=false;
//			Debug.Log("AddAllPhysics");
			
			if(MoleculeModel.atomsByChar!=null) {
				foreach(string key in MoleculeModel.atomsByChar.Keys){
					foreach(GameObject box in MoleculeModel.atomsByChar[key]) {
						if(box&&box.GetComponent <Rigidbody>()==null) {
							box.AddComponent<Rigidbody>();
	    	   				box.GetComponent<Rigidbody>().useGravity = false;
	    	   				box.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
	    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
						}
						if(box&&box.GetComponent <SpringJoint>()==null) {
	    	   				box.AddComponent<SpringJoint>();
							box.GetComponent<SpringJoint>().spring = 5;
						}
					}
				}
			}
			
/*
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Ces!=null) {
				foreach(GameObject box in MoleculeModel.Ces) {
					if(box&&box.GetComponent <Rigidbody>()==null) {
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null) {
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Nes!=null) {
				foreach(GameObject box in MoleculeModel.Nes) {
					if(box&&box.GetComponent <Rigidbody>()==null) {
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null) {
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Oes!=null) {
				foreach(GameObject box in MoleculeModel.Oes) {
					if(box&&box.GetComponent <Rigidbody>()==null) {
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null) {
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Ses!=null) {
				foreach(GameObject box in MoleculeModel.Ses) {
					if(box&&box.GetComponent <Rigidbody>()==null) {
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null) {
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Pes!=null) {
				foreach(GameObject box in MoleculeModel.Pes) {
					if(box&&box.GetComponent <Rigidbody>()==null) {
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null) {
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Hes!=null) {
				foreach(GameObject box in MoleculeModel.Hes) {
					if(box&&box.GetComponent <Rigidbody>()==null) {
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null) {
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.NOes!=null) {
				foreach(GameObject box in MoleculeModel.NOes) {
					if(box&&box.GetComponent <Rigidbody>()==null) {
						box.AddComponent<Rigidbody>();
    	   				box.rigidbody.useGravity = false;
    	   				box.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    	   				box.GetComponent<Rigidbody>().drag = 0.6f;
					}
					if(box&&box.GetComponent <SpringJoint>()==null) {
    	   				box.AddComponent<SpringJoint>();
						box.GetComponent<SpringJoint>().spring = 5;
					}
				}
			}
*/
		}
		
		public static void AddCombineMesh() {
			UIData.resetMeshcombine=false;
			GameObject 	MeshCombineManager;
			if(!GameObject.Find("MeshCombineManager")) {
				MeshCombineManager=GameObject.CreatePrimitive(PrimitiveType.Cube);
				MeshCombineManager.name="MeshCombineManager";
			}
			else
				MeshCombineManager=GameObject.Find("MeshCombineManager");
			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.atomsByChar["C"]!=null) {
				foreach(GameObject Atom in MoleculeModel.atomsByChar["O"])
					Atom.transform.parent=MeshCombineManager.transform;
/*			
			//Will be displayed before the atoms are removed
			if(MoleculeModel.Ces!=null) {
				foreach(GameObject Atom in MoleculeModel.Oes)
					Atom.transform.parent=MeshCombineManager.transform;
*/
				Meshcombine combineComp = MeshCombineManager.GetComponent<Meshcombine>();
    			combineComp.GoOn();
			}
		}
		
		public static void ClearMemory() {
			MoleculeModel.atomsLocationlist=null;
			MoleculeModel.atomsTypelist=null;
			MoleculeModel.atomsResnamelist=null;
			MoleculeModel.CSidList=null;
			//MoleculeModel.sortedResIndexByList = null;

			MoleculeModel.CSLabelList=null;
			MoleculeModel.CSRadiusList=null;
			MoleculeModel.CSColorList=null;
			MoleculeModel.CSSGDList=null;
			MoleculeModel.bondEPList=null;
			MoleculeModel.bondCAList=null;
			MoleculeModel.bondList = null;// The list of the bond by position and rotation.
			MoleculeModel.resChainList = new List<string> (); //For theses 3, we add directly on the list, so can't "null" them.
			MoleculeModel.resChainList2 =new List<string> ();
			MoleculeModel.residues = new Dictionary<int, ArrayList> ();
			MoleculeModel.atomsSugarTypelist = new List<AtomModel>();
			MoleculeModel.bondEPDict= new Dictionary<int, List<int>>();
			MoleculeModel.BondListFromPDB = new List<int[]>();
			MoleculeModel.bondEPSugarList=new List<int[]>();
			MoleculeModel.atomsSugarNamelist = new List<string>();
			MoleculeModel.atomsNumberList = new List<int>();
			MoleculeModel.atomHetTypeList = new List<string>();
			MoleculeModel.atomsSugarResnamelist = new List<string>();
			MoleculeModel.atomsSugarLocationlist = new List<float[]>();
			MoleculeModel.resSugarChainList = new List<string>();
			MoleculeModel.CaSplineList=null;
			MoleculeModel.CaSplineTypeList = null;
			MoleculeModel.CaSplineChainList = null;
			MoleculeModel.CatomsLocationlist = null;// CA atoms coordinates
			
			MoleculeModel.FieldLineList=null;
//			MoleculeModel.FieldLineDist = null;// Field lines distance arrays

			MoleculeModel.backupCaSplineChainList = new List<string>();
			MoleculeModel.backupCatomsLocationlist = new List<float[]>();
			MoleculeModel.BFactorList = new List<float>();
			MoleculeModel.ssHelixList = new List<float[]>();
			MoleculeModel.ssStrandList = new List<float[]>();
			MoleculeModel.helixChainList = new List<string>() ;
			MoleculeModel.strandChainList = new List<string> ();

			MoleculeModel.atomsForEllipsoidsOrientationPerResidue = new Dictionary<int, int>();
			MoleculeModel.atomsForEllipsoidsPerResidue = new Dictionary<int, int[]>();
			MoleculeModel.ellipsoidsPerResidue = new Dictionary<int, GameObject>();
			MoleculeModel.bondsForReplacedAtoms = new List<GameObject>();
			MoleculeModel.baseIdx = new List<int>();
		}
		
		// ???
		public static void DeleteCombineMesh() {}
	}
	
}
