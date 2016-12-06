/// @file AtomCubeStyle.cs
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
/// $Id: AtomCubeStyle.cs 604 2014-07-18 13:00:14Z sebastien $
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


namespace Molecule.View.DisplayAtom {
	using System;
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using Molecule.Model;
	using Config;
	using UI;
	using Molecule.Control;
	using System.Globalization;
	
	public class AtomCubeStyle : IAtomStyle {

		private float[] scale=new float[7];
		public static ArrayList atomOrderList;
		public static ArrayList atomOrderListArray;

		public static GameObject AtomCubeParent = new GameObject("AtomCubeParent");

		private UIData.AtomType atomtype;
		//private GameObject[] hyperballs;
		
		public AtomCubeStyle() {
			if(UIData.secondarystruct)
				MoleculeModel.p = new Particle[MoleculeModel.CaSplineList.Count];
			else
				MoleculeModel.p = new Particle[MoleculeModel.atomsLocationlist.Count];

			scale[0]=(MoleculeModel.carbonScale)/100;
			scale[1]=(MoleculeModel.nitrogenScale)/100;
			scale[2]=(MoleculeModel.oxygenScale)/100;
			scale[3]=(MoleculeModel.sulphurScale)/100;
			scale[4]=(MoleculeModel.phosphorusScale)/100;
			scale[5]=(MoleculeModel.hydrogenScale)/100;			
			scale[6]=(MoleculeModel.unknownScale)/100;			
		}
		
		//Create atoms according to type. Particles are created only once except if display is forced
		public void DisplayAtoms (UIData.AtomType type_atom, bool force_display = false) {
			if(type_atom != UIData.AtomType.particleball || !UIData.isParticlesInitialized ||force_display) {
				//if(!UIData.isCubeLoaded){
					if(AtomCubeParent == null)
						AtomCubeParent = new GameObject("AtomCubeParent");
					
					if(MoleculeModel.atoms != null)	{
						MoleculeModel.atoms.Clear();
						MoleculeModel.atoms=null;
					}
					
					MoleculeModel.atoms=new ArrayList();
					Debug.Log("DisplayAtoms :: ***clear MolecularModel**** " );
					atomtype = type_atom;
					DisplayAtomMethodByCube();
					UIData.isParticlesInitialized = true;
					if(UIData.atomtype == UIData.AtomType.cube)
						UIData.isCubeLoaded = true;
					else if(UIData.atomtype == UIData.AtomType.hyperball)
						UIData.isHBallLoaded = true;
				//}
			}
		}
		
		private  void DisplayAtomMethodByCube() {
//			ArrayList atomListByType;
			if(atomtype==UIData.AtomType.particleball) {
				ArrayList atomListByAxisOrder;
				ArrayList atomListByType;
				
				if(UIData.secondarystruct){
					atomListByType=AtomListByType(MoleculeModel.CaSplineList,MoleculeModel.CaSplineTypeList);}
				else{
					atomListByType=AtomListByType(MoleculeModel.atomsLocationlist,MoleculeModel.atomsTypelist);}
				
				for(int iType=0; iType<atomListByType.Count; iType++)
					DisplayAtomCube(atomListByType[iType] as ArrayList, iType);		
				
				if(UIData.secondarystruct)
					atomListByAxisOrder=AtomListByAxisOrder(MoleculeModel.CaSplineList);
				else
					atomListByAxisOrder=AtomListByAxisOrder(MoleculeModel.atomsLocationlist);

				DisplayAtomParticle(atomListByAxisOrder);
			}
			else if(atomtype==UIData.AtomType.particleballalphablend) {
				ArrayList atomListByAxisOrder;
				if(UIData.secondarystruct) {
					AtomListByType(MoleculeModel.CaSplineList,MoleculeModel.CaSplineTypeList);
					atomListByAxisOrder=AtomListByAxisOrder(MoleculeModel.CaSplineList);
				}
				else {
					AtomListByType(MoleculeModel.atomsLocationlist,MoleculeModel.atomsTypelist);
					atomListByAxisOrder=AtomListByAxisOrder(MoleculeModel.atomsLocationlist);
				}
				DisplayAtomParticle(atomListByAxisOrder);
			}
			else {
				if(UIData.secondarystruct)
					DisplayAtomCube(MoleculeModel.CaSplineList, MoleculeModel.CaSplineTypeList);
				else
					DisplayAtomCube(MoleculeModel.atomsLocationlist, MoleculeModel.atomsTypelist);
			}

			if(UIData.secondarystruct)
				MoleculeModel.atomsnumber=MoleculeModel.CaSplineList.Count;
			else
				MoleculeModel.atomsnumber=MoleculeModel.atomsLocationlist.Count;

			if(atomtype==UIData.AtomType.combinemeshball) {
				GameObject SpriteManager=GameObject.Find("SpriteManager");
				Meshcombine combineComp = SpriteManager.GetComponent<Meshcombine>();
				combineComp.GoOn();
			} else if (atomtype==UIData.AtomType.particleball||atomtype==UIData.AtomType.particleballalphablend) {		
				Debug.Log("This is the particle system we actually use.");
				GameObject shurikenParticleManagerObj = GameObject.FindGameObjectWithTag("ShurikenParticleManager");
				ShurikenParticleManager shManager = shurikenParticleManagerObj.GetComponent<ShurikenParticleManager>();
				shManager.Init();
			}
		}
		
		public static Color HexToColor(string hexColor)	{
			//Remove # if present
			if (hexColor.IndexOf('#') != -1)
				hexColor = hexColor.Replace("#", "");

			int red = 0;
			int green = 0;
			int blue = 0;

			if (hexColor.Length == 6) {
				//#RRGGBB
				red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
				green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
				blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
			} else if (hexColor.Length == 3) {
				//#RGB
				red = int.Parse(hexColor[0].ToString() + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier);
				green = int.Parse(hexColor[1].ToString() + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier);
				blue = int.Parse(hexColor[2].ToString() + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier);
			}
			
			Color hexcolor= new Color(red/255f,green/255f,blue/255f);
			return hexcolor;
		}

//		private ArrayList AtomListByAxisOrder(ArrayList alist)
		public ArrayList AtomListByAxisOrder(IList alist) {
			Debug.Log("Entering :: AtomListByAxisOrder");
			ArrayList atomListByOrder=new ArrayList();

			for(int i=0;i<alist.Count;i++) {
				float typenumber=0f;
				string  type;
				if(UIData.secondarystruct)
					type = (MoleculeModel.CaSplineTypeList[i] as AtomModel).type;
				else
					type = (MoleculeModel.atomsTypelist[i] as AtomModel).type;

				switch(type) {
						case "C": 
								typenumber=0f;
								break;
						case "N":
								typenumber=1f;
								break;
						case "O":
								typenumber=2f;
								break;
						case "S":
								typenumber=3f;
								break;
						case "P":
								typenumber=4f;
								break;
						case "H":
								typenumber=5f;
								break;
						default:
								typenumber=6f;
								break;
					}
				float[] atom=new float[5];
				atom[0]=(alist[i] as float[])[0];
				atom[1]=(alist[i] as float[])[1];
				atom[2]=(alist[i] as float[])[2];
				atom[3]=typenumber;
				atom[4]=i;
				atomListByOrder.Add(atom);
			}
//			atomListByOrder=alist;//if we write like this directly, it will change the order of alist.
			atomListByOrder.Sort(new StructComparer());
			
			return atomListByOrder;
		}
		
		

		private void DisplayAtomParticle(ArrayList atomsLocation) {
			Color c=new Color();
			Vector3 v=new Vector3();
						
			int Number=atomsLocation.Count;
			
			for(int k=0;k<Number;k++) {
				int i = (int)(atomsLocation[k] as float[])[4];
				int order=1;
				
				float typenumber=(atomsLocation[k] as float[])[3];
				c = MoleculeModel.atomsColorList[i];
				switch ( (int) typenumber ) {
					case 0: 
							//c= (MoleculeModel.carbonColor.color);
							v=new Vector3(1.7f*scale[0],1.7f*scale[0],1.7f*scale[0]);
							break;
					case 1:
							//c= (MoleculeModel.nitrogenColor.color);
							v=new Vector3(1.55f*scale[1],1.55f*scale[1],1.55f*scale[1]);
							break;
					case 2:
							//c= (MoleculeModel.oxygenColor.color);
							v=new Vector3(1.52f*scale[2],1.52f*scale[2],1.52f*scale[2]);
							break;
					case 3:
							//c= (MoleculeModel.sulphurColor.color);
							v=new Vector3(2.27f*scale[3],2.27f*scale[3],2.27f*scale[3]);
							break;
					case 4:
							//c= (MoleculeModel.phosphorusColor.color);	
							v=new Vector3(1.18f*scale[4],1.18f*scale[4],1.18f*scale[4]);
							break;
					case 5:
							//c= (MoleculeModel.hydrogenColor.color);
							v=new Vector3(1.2f*scale[5],1.2f*scale[5],1.2f*scale[5]);
							break;
					default:
							//c= (MoleculeModel.unknownColor.color);
							v=new Vector3(1.0f*scale[6],1.0f*scale[6],1.0f*scale[6]);
							break;
				}
				CreateParticleBall(i,k,atomsLocation,c,v,order);
			}
		}

		private void DisplayAtomCube(IList coordinates, IList atomModels) {
			for(int i=0; i < coordinates.Count; i++)
				CreateAtomHB(i, coordinates, atomModels);
			
			
			
			if(UIData.atomtype == UIData.AtomType.hyperball) {
				GameObject hbManagerObj = GameObject.FindGameObjectWithTag("HBallManager");
				HBallManager hbManager = hbManagerObj.GetComponent<HBallManager>();
				Debug.Log("HBall Manager INIT OUTSIDE");
				hbManager.Init();
			}
			else {
				GameObject cubeManagerObj = GameObject.FindGameObjectWithTag("CubeManager");
				CubeManager cubeManager = cubeManagerObj.GetComponent<CubeManager>();
				Debug.Log("Cube Manager INIT OUTSIDE");
				cubeManager.Init();
			}
		}
		
		//iType : Atom category
		private void DisplayAtomCube(ArrayList atomsLocation,int iType) {			
			Color c = new Color();
			Vector3 v = new Vector3();
//			int atom1number = 0;
//			int atom2number = 0;
			
			if(UI.GUIDisplay.file_extension=="xgmml") {
//				atom1number=AtomCubeStyle.atomOrderList.IndexOf(oratom1number);
//				atom2number=AtomCubeStyle.atomOrderList.IndexOf(oratom2number);
			}
			else {
				switch(iType) {
					case 0:c= (MoleculeModel.carbonColor.color);v=new Vector3(1.7f*scale[0],1.7f*scale[0],1.7f*scale[0]);break;//c
					case 1:c= (MoleculeModel.nitrogenColor.color);v=new Vector3(1.55f*scale[1],1.55f*scale[1],1.55f*scale[1]);break;//n
					case 2:c= (MoleculeModel.oxygenColor.color);v=new Vector3(1.52f*scale[2],1.52f*scale[2],1.52f*scale[2]);break;//o
					case 3:c= (MoleculeModel.sulphurColor.color);v=new Vector3(2.27f*scale[3],2.27f*scale[3],2.27f*scale[3]);break;//s
					case 4:c= (MoleculeModel.phosphorusColor.color);v=new Vector3(1.18f*scale[4],1.18f*scale[4],1.18f*scale[4]);break;//p
					case 5:c= (MoleculeModel.hydrogenColor.color);v=new Vector3(1.2f*scale[5],1.2f*scale[5],1.2f*scale[5]);break;//h
					default:c= (MoleculeModel.unknownColor.color);v=new Vector3(1.0f*scale[6],1.0f*scale[6],1.0f*scale[6]);break;//unknown
				}
			}
			
			int Number=atomsLocation.Count;
//			CreateAtomArray(Number);
			for(int k=0;k<Number;k++) {
				int order=(int)((atomOrderListArray[iType] as ArrayList)[k]);
				if(UI.GUIDisplay.file_extension=="xgmml") {
					float pointradius=((MoleculeModel.CSRadiusList[order]) as float[])[0];
					c=HexToColor((MoleculeModel.CSColorList[order] as string[])[0]);
					v=new Vector3(pointradius,pointradius,pointradius);
				}
//				Debug.Log(order);
				
				if(atomtype==UIData.AtomType.hyperball||atomtype==UIData.AtomType.cube) {
					CreateAtomHB(iType,k,atomsLocation,c,v,order);
//					EfficientCreateAtomHB(iType, k, atomsLocation, c, v, order);
				}
					
				else if(atomtype==UIData.AtomType.raycasting)
					CreateAtomRC(iType,k,(k+1),atomsLocation,c,v,order);
				else if(atomtype==UIData.AtomType.rcbillboard)
					CreateAtomRCBB(iType,k,(k+1),atomsLocation,c,v,order);
				else if(atomtype==UIData.AtomType.hbbillboard)
					CreateAtomHBBB(iType,k,(k+1),atomsLocation,c,v,order);
				else if(atomtype==UIData.AtomType.rcsprite)
					CreateAtomRCSprite(iType,k,(k+1),atomsLocation,c,v,order);
				else if(atomtype==UIData.AtomType.multihyperball)
					CreateAtomMtHyperBall(iType,k,(k+1),atomsLocation,c,v,order);
				else if(atomtype==UIData.AtomType.combinemeshball)
					CreateCombineMeshHyperBall(iType,k,(k+1),atomsLocation,c,v,order);
				else if(atomtype==UIData.AtomType.particleball)
					CreateAtomHB(iType,k,atomsLocation,c,v,order);
			}
		}
		
/*
		private void CreateAtomArray(int size) {
			Debug.Log("AtomArray");
			Debug.Log(size);
			hyperballs = new GameObject[size];
			GameObject first = GameObject.CreatePrimitive(PrimitiveType.Cube);
			first.transform.parent = AtomCubeParent.transform;
			hyperballs[0] = first;
			for(int i=1; i<size; i++)
				hyperballs[i] = (GameObject) GameObject.Instantiate(first);
		}
*/
		
		//iAtom : Atom index in atomLocationalist
		private void CreateAtomHB(int iAtom, IList coordinates, IList atomModels) {	
			GameObject Atom;
			Atom=GameObject.CreatePrimitive(PrimitiveType.Cube);
			float[]  fLocation = coordinates[iAtom] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);

			AtomModel atomModel = atomModels[iAtom] as AtomModel;

			Atom.transform.Translate(location);
			Atom.transform.parent = AtomCubeParent.transform;

			MoleculeModel.atoms.Add(Atom);		

			if(atomtype==UIData.AtomType.hyperball) {
				//Test platform to choose the good shader
				RuntimePlatform platform = Application.platform;
				switch(platform) {
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsWebPlayer:
					case RuntimePlatform.WindowsEditor:
					Atom.GetComponent<Renderer>().material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
						break;
					default :
						Atom.GetComponent<Renderer>().material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
//						Atom.renderer.material.shader=Shader.Find("Custom/BallImprovedZ");
						break;				
				}

				Atom.AddComponent<BallUpdateHB>();
				if(UI.GUIDisplay.file_extension=="xgmml")
					Atom.GetComponent<BallUpdateHB>().z=(float)(fLocation[2]);
			}
			else {
				Atom.AddComponent<BallUpdateCube>();
				BallUpdateCube comp1 = Atom.GetComponent<BallUpdateCube>();
				comp1.SetRayonFactor(atomModel.scale/100);
			}

			BallUpdate comp = Atom.GetComponent<BallUpdate>();
//			Debug.Log ("%%%%%%%%%%%%%%%%%%%%% Creating Atom");
//			Debug.Log (iAtom);
			comp.rayon = atomModel.radius;
			comp.atomcolor = atomModel.baseColor;
			comp.number = iAtom;
//			Debug.Log (comp.number);
			comp.oldrayonFactor = atomModel.scale/100;  // Why division by 100 ???

			if(UI.GUIDisplay.file_extension=="xgmml") {
				comp.rayon = ((MoleculeModel.CSRadiusList[iAtom]) as float[])[0];
				comp.atomcolor = HexToColor((MoleculeModel.CSColorList[iAtom] as string[])[0]);
			}

			Atom.GetComponent<Renderer>().material.SetColor("_Color", comp.atomcolor);

//			Projector proj = Atom.AddComponent<Projector>();
			Atom.AddComponent<Projector>();
			comp.enabled = true;
			comp.isSplineNode = UIData.secondarystruct;
			Atom.tag = atomModel.type;

			if(atomtype==UIData.AtomType.particleball)
				Atom.GetComponent<Renderer>().enabled = false;
			
			BoxCollider collider = Atom.GetComponent<Collider>() as BoxCollider;
			float newSize = comp.rayon * 60 / 100;
			collider.size = new Vector3(newSize,newSize,newSize);
		}

//		private ArrayList AtomListByType(ArrayList alist, ArrayList typelist)
		private ArrayList AtomListByType(IList alist, IList typelist) {
			ArrayList atomListByType=new ArrayList();
			
			ArrayList CList=new ArrayList();
			ArrayList NList=new ArrayList();
			ArrayList OList=new ArrayList();
			ArrayList SList=new ArrayList();
			ArrayList PList=new ArrayList();
			ArrayList HList=new ArrayList();			
			ArrayList NOList=new ArrayList();
			
			ArrayList COrderList=new ArrayList();
			ArrayList NOrderList=new ArrayList();
			ArrayList OOrderList=new ArrayList();
			ArrayList SOrderList=new ArrayList();
			ArrayList POrderList=new ArrayList();
			ArrayList HOrderList=new ArrayList();			
			ArrayList NOOrderList=new ArrayList();
			if(atomOrderList!=null) {
				atomOrderList.Clear();
				atomOrderList=null;
			}
			if(atomOrderListArray!=null) {
				atomOrderListArray.Clear();
				atomOrderListArray=null;
			}
			atomOrderList=new ArrayList();			
			atomOrderListArray=new ArrayList();	
			
			for(int i=0;i<alist.Count;i++) {
				string  type=(typelist[i] as AtomModel).type;
				switch(type) {
					case "C": 
							CList.Add(alist[i] as float[]);
							COrderList.Add(i);
							break;
					case "N":
							NList.Add(alist[i] as float[]);
							NOrderList.Add(i);
							
							break;
					case "O":
							OList.Add(alist[i] as float[]);
							OOrderList.Add(i);

							break;
					case "S":
							SList.Add(alist[i] as float[]);
							SOrderList.Add(i);

							break;
					case "P":
							PList.Add(alist[i] as float[]);
							POrderList.Add(i);

							break;
					case "H":
							HList.Add(alist[i] as float[]);
							HOrderList.Add(i);

							break;
					default:
							NOList.Add(alist[i] as float[]);
							NOOrderList.Add(i);

							break;
				}
			}
			atomListByType.Add(CList);
			atomListByType.Add(NList);
			atomListByType.Add(OList);
			atomListByType.Add(SList);
			atomListByType.Add(PList);
			atomListByType.Add(HList);
			atomListByType.Add(NOList);
			
			atomOrderList.AddRange(COrderList);
			atomOrderList.AddRange(NOrderList);
			atomOrderList.AddRange(OOrderList);
			atomOrderList.AddRange(SOrderList);
			atomOrderList.AddRange(POrderList);
			atomOrderList.AddRange(HOrderList);
			atomOrderList.AddRange(NOOrderList);
			
			atomOrderListArray.Add(COrderList);
			atomOrderListArray.Add(NOrderList);
			atomOrderListArray.Add(OOrderList);
			atomOrderListArray.Add(SOrderList);
			atomOrderListArray.Add(POrderList);
			atomOrderListArray.Add(HOrderList);
			atomOrderListArray.Add(NOOrderList);

			MoleculeModel.carbonNumber=CList.Count.ToString();
			MoleculeModel.nitrogenNumber=NList.Count.ToString();
			MoleculeModel.oxygenNumber=OList.Count.ToString();
			MoleculeModel.sulphurNumber=SList.Count.ToString();
			MoleculeModel.phosphorusNumber=PList.Count.ToString();
			MoleculeModel.hydrogenNumber=HList.Count.ToString();
			MoleculeModel.unknownNumber=NOList.Count.ToString();

			return atomListByType;
		}

		//iType : Atom type index (element)
		//iAtom : Atom index in atomLocationalist
		private void CreateAtomHB(int iType, int iAtom, ArrayList atomLocationalist, Color c, Vector3 v, int order) {
			GameObject Atom;
			Atom=GameObject.CreatePrimitive(PrimitiveType.Cube);
/*
			UnityEngine.Object.Destroy(Atom.collider);
			Object.DestroyImmediate(Atom.collider);
			GameObject.DestroyImmediate(Atom.collider); // "good" one
			Atom.AddComponent<Rigidbody>();
			Atom.rigidbody.isKinematic = true;
			Debug.Log("Atom collider?");
			Debug.Log(Atom.collider.ToString());
			GameObject.DestroyObject(Atom.collider);
*/
			float[]  fLocation=atomLocationalist[iAtom] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);

			Atom.transform.Translate(location);
			Atom.GetComponent<Renderer>().material.SetColor("_Color",c);
//			Atom.transform.parent = AtomCubeParent.transform;
//			Debug.Log(Atom.collider.ToString());

			MoleculeModel.atoms.Add(Atom);		
			
			BallUpdate script;
			if(atomtype==UIData.AtomType.hyperball) {
				RuntimePlatform platform = Application.platform;
				switch(platform) {
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsWebPlayer:
					case RuntimePlatform.WindowsEditor:
					Atom.GetComponent<Renderer>().material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
						break;
					default :
						Atom.GetComponent<Renderer>().material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
						break;				
				}
			
				Atom.AddComponent<BallUpdateHB>();
				BallUpdateHB comp = Atom.GetComponent<BallUpdateHB>();
				script = comp;
				comp.rayon=(float)(v[0]);
				comp.atomcolor=c;
				comp.number=order;
				comp.z=(float)(fLocation[2]);
//				Projector proj = Atom.AddComponent<Projector>();
				Atom.AddComponent<Projector>();
//				if(UI.GUIDisplay.file_extension=="xgmml")comp.xgmml=true;
				comp.enabled = true;		
				switch(iType) {
					case 0:
						comp.oldrayonFactor=(MoleculeModel.carbonScale)/100;break;
					case 1:
						comp.oldrayonFactor=(MoleculeModel.nitrogenScale)/100;break;
					case 2:
						comp.oldrayonFactor=(MoleculeModel.oxygenScale)/100;break;
					case 3:
						comp.oldrayonFactor=(MoleculeModel.sulphurScale)/100;break;
					case 4:
						comp.oldrayonFactor=(MoleculeModel.phosphorusScale)/100;break;
					case 5:
						comp.oldrayonFactor=(MoleculeModel.hydrogenScale)/100;break;
					default:
						comp.oldrayonFactor=2.0f;break;
				}
			}
			else {
				Atom.AddComponent<BallUpdateCube>();
				BallUpdateCube comp1 = Atom.GetComponent<BallUpdateCube>();
				script = comp1;
//				comp1.rayon = Atom.transform.localScale.x*2;
				comp1.rayon = (float)(v[0])*2;
				float rayonfactor;
				switch(iType) {
					case 0:
						rayonfactor=(MoleculeModel.carbonScale)/100;break;
					case 1:
						rayonfactor=(MoleculeModel.nitrogenScale)/100;break;
					case 2:
						rayonfactor=(MoleculeModel.oxygenScale)/100;break;
					case 3:
						rayonfactor=(MoleculeModel.sulphurScale)/100;break;
					case 4:
						rayonfactor=(MoleculeModel.phosphorusScale)/100;break;
					case 5:
						rayonfactor=(MoleculeModel.hydrogenScale)/100;break;
					default:
						rayonfactor=1.0f;break;
				}
				comp1.SetRayonFactor(rayonfactor);
				comp1.number=order;
				comp1.enabled = true;		
			}

			switch(iType) {
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;
			}
			if(atomtype==UIData.AtomType.particleball)
				Atom.GetComponent<Renderer>().enabled = false;
			
			BoxCollider collider = Atom.GetComponent<Collider>() as BoxCollider;
			float newSize = script.rayon * 60 / 100;
			collider.size = new Vector3(newSize,newSize,newSize);
		}
		
		private void CreateAtomRC(int type, int start, int end, ArrayList atomLocationalist, Color c, Vector3 v, int order) {		
			GameObject Atom;
			Atom=GameObject.CreatePrimitive(PrimitiveType.Cube);
			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
			Atom.transform.Translate(location);
			Atom.GetComponent<Renderer>().material.SetColor("_Color",c);
			MoleculeModel.atoms.Add(Atom);		
			Atom.GetComponent<Renderer>().material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
			Atom.AddComponent<BallUpdateRC>();
			BallUpdateRC comp = Atom.GetComponent<BallUpdateRC>();
			comp.rayon=(float)(v[0]);
			comp.atomcolor=c;
			comp.number=order;
			comp.enabled = true;		
			switch(type) {
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;
			}
		}

		private void CreateAtomRCBB(int type,int start ,int end,ArrayList atomLocationalist,Color c,Vector3 v,int order) {		
			GameObject Atom;
			if(UIData.toggleClip)
				Atom=Clip4RayCasting.CreateClip();
			else
				Atom=GameObject.CreatePrimitive(PrimitiveType.Plane);

			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
		
			Atom.transform.Translate(location);
			Atom.GetComponent<Renderer>().material.SetColor("_Color",c);
			MoleculeModel.atoms.Add(Atom);		
			Atom.AddComponent<CameraFacingBillboard>();
			Atom.GetComponent<CameraFacingBillboard>().cameraToLookAt = GameObject.Find("Camera").GetComponent<Camera>();
			Atom.transform.GetComponent<Renderer>().material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
			Atom.AddComponent<BallUpdateRC>();
			BallUpdateRC comp = Atom.GetComponent<BallUpdateRC>();
			comp.rayon=(float)(v[0]);
			comp.atomcolor=c;
			comp.number=order;
			comp.enabled = true;		
			switch(type) {
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;	
			}
		}
		
		private void CreateAtomHBBB(int type,int start ,int end,ArrayList atomLocationalist,Color c,Vector3 v,int order) {		
			GameObject Atom;
			if(UIData.toggleClip)
				Atom=Clip4RayCasting.CreateClip();
			else
				Atom=GameObject.CreatePrimitive(PrimitiveType.Plane);
			
			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
				
			Atom.transform.Translate(location);
			Atom.GetComponent<Renderer>().material.SetColor("_Color",c);
			MoleculeModel.atoms.Add(Atom);		
			Atom.AddComponent<CameraFacingBillboard>();
			Atom.GetComponent<CameraFacingBillboard>().cameraToLookAt = GameObject.Find("Camera").GetComponent<Camera>();
			Atom.transform.GetComponent<Renderer>().material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
			Atom.AddComponent<BallUpdateHB>();
			BallUpdateHB comp = Atom.GetComponent<BallUpdateHB>();
			comp.rayon=(float)(v[0]);
			comp.atomcolor=c;
//			Debug.Log(start+"/"+c);
			comp.number=order;
			comp.enabled = true;		
			
			switch(type) {
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;
			}			
		}
		
		private void CreateAtomRCSprite(int type,int start ,int end,ArrayList atomLocationalist,Color c,Vector3 v,int order) {		
			LinkedSpriteManager linkedSpriteManager=null; 		
			GameObject 	SpriteManager;
			if(!GameObject.Find("SpriteManager")) {
				SpriteManager=new GameObject();
				SpriteManager.name="SpriteManager";
			}
			else
				SpriteManager=GameObject.Find("SpriteManager");

			SpriteManager.GetComponent <MeshRenderer>().enabled=true;

			if(SpriteManager.GetComponent <LinkedSpriteManager>()==null) {
				SpriteManager.AddComponent<LinkedSpriteManager>();
				linkedSpriteManager= (LinkedSpriteManager)Component.FindObjectOfType(typeof(LinkedSpriteManager));			
				Material mat = Resources.Load("Materials/hyperballshader", typeof(Material)) as Material;				
				linkedSpriteManager.material=mat;
				linkedSpriteManager.allocBlockSize=atomLocationalist.Count;
			}		
			else {
				linkedSpriteManager= (LinkedSpriteManager)Component.FindObjectOfType(typeof(LinkedSpriteManager));
				linkedSpriteManager.allocBlockSize=atomLocationalist.Count;
				linkedSpriteManager.enabled=true;
			}
			GameObject Atom;
			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
			UnityEngine.Object o;				
			o=MonoBehaviour.Instantiate(Resources.Load("HBspriteplane"),location,new Quaternion(0f,0f,0f,0f));		
			Atom=(GameObject)o;
			MoleculeModel.atoms.Add(Atom);		
			Atom.GetComponent<Renderer>().material.SetColor("_Color",c);
			BallUpdateHB comp = Atom.GetComponent<BallUpdateHB>();
			comp.rayon=(float)(v[0]);
			comp.atomcolor=c;
			comp.number=order;
			comp.enabled = true;
			linkedSpriteManager.AddSprite(Atom,	1f, 1f, 0, 0, 256, 256, true);	
			switch(type) {
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;
			}
		}
		
		private void CreateAtomMtHyperBall(int type,int start ,int end,ArrayList atomLocationalist,Color c,Vector3 v,int order) {		
			GameObject 	Atom;
			if(!GameObject.Find("MultiHBCube")) {
				Atom=GameObject.CreatePrimitive(PrimitiveType.Cube);
				Atom.name="MultiHBCube";
			}
			else
				Atom=GameObject.Find("MultiHBCube");
			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
			Material newMat = new Material(Shader.Find("FvNano/Ball HyperBalls OpenGL"));
			newMat.SetVector("_TexPos",location);
			newMat.SetFloat("_Rayon",(float)(v[0])*0.1f);
			newMat.SetColor("_Color",c);
			
			ArrayList materialArray= new ArrayList(Atom.transform.GetComponent<Renderer>().materials);
			materialArray.Add(newMat);
			Material[] array = new Material[materialArray.Count];
			materialArray.CopyTo( array );
			Atom.transform.GetComponent<Renderer>().materials = array;	
			switch(type) {
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;		
			}		
		}	
				
		private void CreateCombineMeshHyperBall(int type,int start ,int end,ArrayList atomLocationalist,Color c,Vector3 v,int order) {		
			GameObject 	SpriteManager;
			if(!GameObject.Find("SpriteManager")) {
				SpriteManager=GameObject.CreatePrimitive(PrimitiveType.Cube);
				SpriteManager.name="SpriteManager";
			}
			else
				SpriteManager=GameObject.Find("SpriteManager");

			GameObject Atom;
			if(UIData.toggleClip)
				Atom=Clip4RayCasting.CreateClip();
			else
				Atom=GameObject.CreatePrimitive(PrimitiveType.Plane);

			Atom.transform.parent=SpriteManager.transform;
			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
			Atom.transform.Translate(location);
			Atom.GetComponent<Renderer>().material.SetColor("_Color",c);
			MoleculeModel.atoms.Add(Atom);		
			Atom.AddComponent<CameraFacingBillboard>();
			Atom.GetComponent<CameraFacingBillboard>().cameraToLookAt = GameObject.Find("Camera").GetComponent<Camera>();
			Atom.transform.GetComponent<Renderer>().material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
			Atom.AddComponent<BallUpdateHB>();
			BallUpdateHB comp = Atom.GetComponent<BallUpdateHB>();
			comp.rayon=(float)(v[0]);
			comp.atomcolor=c;
			comp.number=order;
			comp.enabled = true;		
			switch(type) {
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;	
			}
		}
		
		private void CreateParticleBall(int type,int start,ArrayList atomLocationalist,Color c,Vector3 v,int order) {		
//			Debug.Log("CreateParticleBall"+start);
			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
			MoleculeModel.p[start].position=location;
			MoleculeModel.p[start].size=(float)(v[0]);
			MoleculeModel.p[start].color=c;
			MoleculeModel.p[start].energy=1000;
		}
	}
}
