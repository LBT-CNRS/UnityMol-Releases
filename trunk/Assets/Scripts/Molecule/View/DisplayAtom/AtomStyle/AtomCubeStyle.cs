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
/// $Id: AtomCubeStyle.cs 239 2013-04-07 19:31:48Z baaden $
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


namespace Molecule.View.DisplayAtom
{
	using System;
	using UnityEngine;
	using System.Collections;
	using Molecule.Model;
	using Config;
	using UI;
	using Molecule.Control;
	using System.Globalization;
	
	public class AtomCubeStyle : IAtomStyle
	{

		private float[] scale=new float[7];
		public static ArrayList atomOrderList;
		public static ArrayList atomOrderListArray;
		//int atomindex=0;//It's for Cumulative of order by type, but it's unuseful for particleball now, because of the Axis order of particleball.
		private static int UnityParticleLimit=16250;
		private static int blockbegincount=10000;

		public static GameObject AtomCubeParent = new GameObject("AtomCubeParent");

		private UIData.AtomType atomtype;
		
		public AtomCubeStyle()
		{
			if(UIData.secondarystruct)
			{
				MoleculeModel.p = new Particle[MoleculeModel.CaSplineList.Count];
			}
			else
			{
				MoleculeModel.p = new Particle[MoleculeModel.atomsLocationlist.Count];
			}
			scale[0]=(MoleculeModel.carbonScale)/100;
			scale[1]=(MoleculeModel.nitrogenScale)/100;
			scale[2]=(MoleculeModel.oxygenScale)/100;
			scale[3]=(MoleculeModel.sulphurScale)/100;
			scale[4]=(MoleculeModel.phosphorusScale)/100;
			scale[5]=(MoleculeModel.hydrogenScale)/100;			
			scale[6]=(MoleculeModel.unknownScale)/100;
			
		}
		
		//Create atoms according to type. Particles are created only once except if display is forced
		public void DisplayAtoms(UIData.AtomType type_atom, bool force_display = false)
		{
			if(type_atom != UIData.AtomType.particleball || !UIData.isParticlesInitialized ||force_display)
			{
				if(AtomCubeParent == null)
					AtomCubeParent = new GameObject("AtomCubeParent");
				if(MoleculeModel.atoms != null)
				{
					MoleculeModel.atoms.Clear();
					MoleculeModel.atoms=null;
				}
				MoleculeModel.atoms=new ArrayList();
				
				Debug.Log("DisplayAtoms :: ***clear MolecularModel**** " );
				
				atomtype = type_atom;

				DisplayAtomMethodByCube();
				UIData.isParticlesInitialized = true;
			}
		}
		
		private  void DisplayAtomMethodByCube()
		{
			// ArrayList atomListByType;
			if(atomtype==UIData.AtomType.particleball)
			{
				ArrayList atomListByAxisOrder;
				
				//just for particleball' hyperstick///////////////////////////////////////////////////////////////////////////////				
				// if(UIData.openAllMenu)
				{
					ArrayList atomListByType;
					if(UIData.secondarystruct)
					{
						atomListByType=AtomListByType(MoleculeModel.CaSplineList,MoleculeModel.CaSplineTypeList);
					}
					else
					{
						atomListByType=AtomListByType(MoleculeModel.atomsLocationlist,MoleculeModel.atomsTypelist);
					}
					for(int iType=0; iType<atomListByType.Count; iType++)
					{
						DisplayAtomCube(atomListByType[iType] as ArrayList, iType);
					}
				}
				// else
				// {
				// 	if(UIData.secondarystruct)
				// 	{
				// 		AtomListByType(MoleculeModel.CaSplineList,MoleculeModel.CaSplineTypeList);//just for get the count of every type atom
				// 	}
				// 	else
				// 	{
				// 		AtomListByType(MoleculeModel.atomsLocationlist,MoleculeModel.atomsTypelist);//just for get the count of every type atom
				// 	}
				// }
				//just for particleball' hyperstick//////////////////////////////////////////////////////////////////////////////				
				
				if(UIData.secondarystruct)
				{
					atomListByAxisOrder=AtomListByAxisOrder(MoleculeModel.CaSplineList);
				}
				else
				{
					atomListByAxisOrder=AtomListByAxisOrder(MoleculeModel.atomsLocationlist);
				}
				DisplayAtomParticle(atomListByAxisOrder);
				
				
				
				// atomindex+=((ArrayList)atomListByAxisOrder).Count;
			}
			else if(atomtype==UIData.AtomType.particleballalphablend)
			{
				ArrayList atomListByAxisOrder;
				if(UIData.secondarystruct)
				{
					AtomListByType(MoleculeModel.CaSplineList,MoleculeModel.CaSplineTypeList);
					atomListByAxisOrder=AtomListByAxisOrder(MoleculeModel.CaSplineList);
				}
				else
				{
					AtomListByType(MoleculeModel.atomsLocationlist,MoleculeModel.atomsTypelist);
					atomListByAxisOrder=AtomListByAxisOrder(MoleculeModel.atomsLocationlist);
				}
				
				DisplayAtomParticle(atomListByAxisOrder);
			}
			else
			{
//				ArrayList atomListByType;
//				if(UIData.secondarystruct)
//				{
//					atomListByType=AtomListByType(MoleculeModel.CaSplineList,MoleculeModel.CaSplineTypeList);
//				}
//				else
//				{
//					atomListByType=AtomListByType(MoleculeModel.atomsLocationlist,MoleculeModel.atomsTypelist);
//				}
			// 	for(int iType=0; iType<atomListByType.Count;iType++)
			// 	{
			
			// // Debug.Log("DisplayAtomCube ***atomListByType[i]**** "  + (atomListByType[i] as ArrayList).Count);
				
			// 		DisplayAtomCube(atomListByType[iType] as ArrayList,iType);
			// 		// atomindex+=((ArrayList)atomListByType[i]).Count;
			// 	}
				if(UIData.secondarystruct)
					DisplayAtomCube(MoleculeModel.CaSplineList, MoleculeModel.CaSplineTypeList);
				else
					DisplayAtomCube(MoleculeModel.atomsLocationlist, MoleculeModel.atomsTypelist);

			}

			if(UIData.secondarystruct)
			{
				MoleculeModel.atomsnumber=MoleculeModel.CaSplineList.Count;
			}
			else
			{
				MoleculeModel.atomsnumber=MoleculeModel.atomsLocationlist.Count;
			}

			// Debug.Log("DisplayAtomCube ***atomListByType**** "  + atomListByType.Count);

			if(atomtype==UIData.AtomType.combinemeshball)
			{
				GameObject SpriteManager=GameObject.Find("SpriteManager");
		
				Meshcombine combineComp = SpriteManager.GetComponent<Meshcombine>();
				combineComp.GoOn();
			}
			else if(atomtype==UIData.AtomType.particleball||atomtype==UIData.AtomType.particleballalphablend)
			{
				string particlemanagername;
				if(atomtype==UIData.AtomType.particleball)
				{
					particlemanagername="ParticleManager";
				}
				else
				{
					particlemanagername="ParticleManagerAlphaBlend";
				}

				if(MoleculeModel.atomsnumber>blockbegincount)
				{
					//depart the molecule particle system into 64 blocks//////////////////////////////////////////////////////////////////////////////				
					
					Vector3 minPoint=Vector3.zero;
					Vector3 maxPoint=Vector3.zero;
					
					int Particlecount=MoleculeModel.p.Length;
					for(int i=0; i<Particlecount; i++)
					{
						//Vector3 location;
						
						minPoint = Vector3.Min(minPoint, new Vector3(MoleculeModel.p[i].position.x,MoleculeModel.p[i].position.y,MoleculeModel.p[i].position.z));
						maxPoint = Vector3.Max(maxPoint, new Vector3(MoleculeModel.p[i].position.x,MoleculeModel.p[i].position.y,MoleculeModel.p[i].position.z));
					}
					Vector3 AxisSpan=maxPoint-minPoint;
					Debug.Log("AxisSpan:"+AxisSpan);
					
					Vector3 AxisBlockLength=AxisSpan/4;
					int xid=0,yid=0,zid=0;
					float xcenter=1.0f,ycenter=1.0f,zcenter=1.0f;
					float xscale=1.0f,yscale=1.0f,zscale=1.0f;
					ArrayList[] AxisBlock = new ArrayList[64];
					ArrayList[] BlockCenter = new ArrayList[64];
					ArrayList[] BlockScale = new ArrayList[64];
					for(int i=0; i<Particlecount; i++)
					{
						if((MoleculeModel.p[i].position.x>=minPoint.x)&&(MoleculeModel.p[i].position.x<=minPoint.x+AxisBlockLength.x))
						{
							xid=1;
							xcenter=minPoint.x+0.5f*AxisBlockLength.x;
						}
						else if((MoleculeModel.p[i].position.x>minPoint.x+AxisBlockLength.x)&&(MoleculeModel.p[i].position.x<=minPoint.x+2*AxisBlockLength.x))
						{
							xid=2;
							xcenter=minPoint.x+1.5f*AxisBlockLength.x;
						}
						else if((MoleculeModel.p[i].position.x>minPoint.x+2*AxisBlockLength.x)&&(MoleculeModel.p[i].position.x<=minPoint.x+3*AxisBlockLength.x))
						{
							xid=3;
							xcenter=minPoint.x+2.5f*AxisBlockLength.x;
						}
						else if((MoleculeModel.p[i].position.x>minPoint.x+3*AxisBlockLength.x)&&(MoleculeModel.p[i].position.x<=maxPoint.x))
						{
							xid=4;
							xcenter=minPoint.x+3.5f*AxisBlockLength.x;
						}
						else
						{
							Debug.Log("X axis span is wrong");
						}
						
						
						if((MoleculeModel.p[i].position.y>=minPoint.y)&&(MoleculeModel.p[i].position.y<=minPoint.y+AxisBlockLength.y))
						{
							yid=1;
							ycenter=minPoint.y+0.5f*AxisBlockLength.y;
						}
						else if((MoleculeModel.p[i].position.y>minPoint.y+AxisBlockLength.y)&&(MoleculeModel.p[i].position.y<=minPoint.y+2*AxisBlockLength.y))
						{
							yid=2;
							ycenter=minPoint.y+1.5f*AxisBlockLength.y;
						}
						else if((MoleculeModel.p[i].position.y>minPoint.y+2*AxisBlockLength.y)&&(MoleculeModel.p[i].position.y<=minPoint.y+3*AxisBlockLength.y))
						{
							yid=3;
							ycenter=minPoint.y+2.5f*AxisBlockLength.y;
						}
						else if((MoleculeModel.p[i].position.y>minPoint.y+3*AxisBlockLength.y)&&(MoleculeModel.p[i].position.y<=maxPoint.y))
						{
							yid=4;
							ycenter=minPoint.y+3.5f*AxisBlockLength.y;
						}
						else
						{
							Debug.Log("Y axis span is wrong");
						} 
						
						
						if((MoleculeModel.p[i].position.z>=minPoint.z)&&(MoleculeModel.p[i].position.z<=minPoint.y+AxisBlockLength.z))
						{
							zid=1;
							zcenter=minPoint.z+0.5f*AxisBlockLength.z;
						}
						else if((MoleculeModel.p[i].position.z>minPoint.z+AxisBlockLength.z)&&(MoleculeModel.p[i].position.z<=minPoint.z+2*AxisBlockLength.z))
						{
							zid=2;
							zcenter=minPoint.z+1.5f*AxisBlockLength.z;
						}
						else if((MoleculeModel.p[i].position.z>minPoint.z+2*AxisBlockLength.z)&&(MoleculeModel.p[i].position.z<=minPoint.z+3*AxisBlockLength.z))
						{
							zid=3;
							zcenter=minPoint.z+2.5f*AxisBlockLength.z;
						}
						else if((MoleculeModel.p[i].position.z>minPoint.z+3*AxisBlockLength.z)&&(MoleculeModel.p[i].position.z<=maxPoint.z))
						{
							zid=4;
							zcenter=minPoint.z+3.5f*AxisBlockLength.z;
						}
						else
						{
						// Debug.Log("Z axis span is wrong");
						}  
						xscale=AxisBlockLength.x;
						yscale=AxisBlockLength.y;
						zscale=AxisBlockLength.z;
						
						// Debug.Log("16*(zid-1)+4*(yid-1)+xid-1:"+(16*(zid-1)+4*(yid-1)+xid-1));
						
						if(AxisBlock[16*(zid-1)+4*(yid-1)+xid-1]==null)AxisBlock[16*(zid-1)+4*(yid-1)+xid-1]=new ArrayList();
						AxisBlock[16*(zid-1)+4*(yid-1)+xid-1].Add(MoleculeModel.p[i]);  
						
						if(BlockCenter[16*(zid-1)+4*(yid-1)+xid-1]==null)BlockCenter[16*(zid-1)+4*(yid-1)+xid-1]=new ArrayList();
						BlockCenter[16*(zid-1)+4*(yid-1)+xid-1].Add(new Vector3(xcenter,ycenter,zcenter));
						
						if(BlockScale[16*(zid-1)+4*(yid-1)+xid-1]==null)BlockScale[16*(zid-1)+4*(yid-1)+xid-1]=new ArrayList();
						BlockScale[16*(zid-1)+4*(yid-1)+xid-1].Add(new Vector3(xscale,yscale,zscale));      				
					}
					
					int groupcount=AxisBlock.Length;
					Debug.Log("groupcount:"+groupcount);
					for(int i=0;i<groupcount;i++)
					{
						if(AxisBlock[i]!=null)
						{
							GameObject ParticleManager=new GameObject();
							ParticleManager.name="ParticleEffectManager"+i;
							ParticleManager.tag="ParticleManager";
							ParticleManager.AddComponent("ParticleEffect");
							ParticleEffect particleeffect = ParticleManager.GetComponent<ParticleEffect>();
							
							GameObject ParticleManager2=GameObject.Find(particlemanagername);
							ParticleEffect particleeffect2=ParticleManager2.transform.GetComponent<ParticleEffect>();
							
							particleeffect.particleEffect = particleeffect2.particleEffect;
							Particle[] p;
						// Debug.Log("i:"+i);
						// Debug.Log("AxisBlock[i].Count:"+AxisBlock[i].Count);
							particleeffect.atomcount=AxisBlock[i].Count;
							
							p = new Particle[particleeffect.atomcount];
							for(int j=0;j<particleeffect.atomcount;j++)
							{
								p[j]=(Particle)AxisBlock[i][j];
							}
							particleeffect.p=p;
							particleeffect.SpawnEffect ();
							
							//For show the departing method/////////////////////////
							if(UIData.openBound)
							{
								GameObject o;
							
								Vector3 vl=new Vector3();
							
								vl=(Vector3)BlockCenter[i][0];
							
							// Debug.Log(vl);
								
								o=(GameObject)MonoBehaviour.Instantiate(Resources.Load("transparentcube"),vl,new Quaternion(0f,0f,0f,0f));
							
								o.transform.localScale = (Vector3)BlockScale[i][0];
								
								o.name="BoundBox"+i;
								
								o.tag="TransparentCube";
							}            			
							//For show the departing method/////////////////////////
						}
							
					}
					
					//depart the molecule particle system into 64 blocks//////////////////////////////////////////////////////////////////////////////	
				}
				else
				{			
					//16250 atoms every particle system//////////////////////////////////////////////////////////////////////////////				
					int particlelimit=UnityParticleLimit;//limited to 16250
					int groupcount=MoleculeModel.p.Length/particlelimit;
					if(groupcount>=1)
					{
						for(int i=0;i<groupcount+1;i++)
						{
							GameObject ParticleManager=new GameObject();
							ParticleManager.name="ParticleEffectManager";
							ParticleManager.tag="ParticleManager";
							ParticleManager.AddComponent("ParticleEffect");
							ParticleEffect particleeffect = ParticleManager.GetComponent<ParticleEffect>();
							
							GameObject ParticleManager2=GameObject.Find(particlemanagername);
							ParticleEffect particleeffect2=ParticleManager2.transform.GetComponent<ParticleEffect>();
							
							particleeffect.particleEffect = particleeffect2.particleEffect;
							Particle[] p;
							if(i==groupcount)
							{
								if(UIData.secondarystruct)
								{
									particleeffect.atomcount=MoleculeModel.CaSplineList.Count-particlelimit*i;	
								}
								else
								{
									particleeffect.atomcount=MoleculeModel.atomsLocationlist.Count-particlelimit*i;	
								}						
							}
							else
							{
								particleeffect.atomcount=particlelimit;	
							}
							
							p = new Particle[particleeffect.atomcount];
							for(int j=0;j<particleeffect.atomcount;j++)
							{
								p[j]=MoleculeModel.p[particlelimit*i+j];
							}
							// particleeffect.radiuschange=true;
							particleeffect.p=p;
							particleeffect.SpawnEffect ();
							
						}
					}
					else
					{
						GameObject ParticleManager=GameObject.Find(particlemanagername);
						ParticleEffect particleeffect=ParticleManager.transform.GetComponent<ParticleEffect>();
						if(UIData.secondarystruct)
						{
							particleeffect.atomcount=MoleculeModel.CaSplineList.Count;
						}
						else
						{
							particleeffect.atomcount=MoleculeModel.atomsLocationlist.Count;
						}
						particleeffect.p=MoleculeModel.p;
						// particleeffect.radiuschange=true;
						particleeffect.SpawnEffect ();
					}
					/////16250 atoms every particle system//////////////////////////////////////////////////////////////////////////////	
				}			

			}
			
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


		
		private ArrayList AtomListByAxisOrder(ArrayList alist)
		{
			Debug.Log("Entering :: AtomListByAxisOrder");
			ArrayList atomListByOrder=new ArrayList();

			for(int i=0;i<alist.Count;i++)
			{
				float typenumber=0f;
				string  type;
				if(UIData.secondarystruct)
				{
					type = (MoleculeModel.CaSplineTypeList[i] as AtomModel).type;
				}
				else
				{
					type = (MoleculeModel.atomsTypelist[i] as AtomModel).type;
				}
				switch(type)
					{
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
				// atomListByOrder=alist;//if we write like this directly, it will change the order of alist.
			atomListByOrder.Sort(new StructComparer());
			
			return atomListByOrder;
		}
		
		
		private void DisplayAtomParticle(ArrayList atomsLocation)//i:原子的类别
		{
			//Debug.Log("DisplayAtomParticle");
			Color c=new Color();
			Vector3 v=new Vector3();
						
			int Number=atomsLocation.Count;
			
			for(int k=0;k<Number;k++)
			{
				
				int i=1;
				int order=1;
				
				float typenumber=(atomsLocation[k] as float[])[3];
				switch((int)typenumber)
				{
						case 0: 
								c= (MoleculeModel.carbonColor);v=new Vector3(1.7f*scale[0],1.7f*scale[0],1.7f*scale[0]);
								break;
						case 1:
								c= (MoleculeModel.nitrogenColor);v=new Vector3(1.55f*scale[1],1.55f*scale[1],1.55f*scale[1]);
								
								break;
						case 2:
								c= (MoleculeModel.oxygenColor);v=new Vector3(1.52f*scale[2],1.52f*scale[2],1.52f*scale[2]);

								break;
						case 3:
								c= (MoleculeModel.sulphurColor);v=new Vector3(2.27f*scale[3],2.27f*scale[3],2.27f*scale[3]);

								break;
						case 4:
								c= (MoleculeModel.phosphorusColor);v=new Vector3(1.18f*scale[4],1.18f*scale[4],1.18f*scale[4]);

								break;
						case 5:
								c= (MoleculeModel.hydrogenColor);v=new Vector3(1.2f*scale[5],1.2f*scale[5],1.2f*scale[5]);

								break;
						default:
								c= (MoleculeModel.unknownColor);
								
								v=new Vector3(1.0f*scale[6],1.0f*scale[6],1.0f*scale[6]);

								break;
					
				}
				CreateParticleBall(i,k,atomsLocation,c,v,order);
			}
		}

		private void DisplayAtomCube(ArrayList coordinates, ArrayList atomModels)
		{
			for(int i=0; i < coordinates.Count; i++)
				CreateAtomHB(i, coordinates, atomModels);
		}
		
		//iType : Atom category
		private void DisplayAtomCube(ArrayList atomsLocation,int iType)
		{			
			Color c = new Color();
			Vector3 v = new Vector3();
//			int atom1number = 0;
//			int atom2number = 0;
			
			if(UI.GUIDisplay.file_extension=="xgmml")
			{
					// atom1number=AtomCubeStyle.atomOrderList.IndexOf(oratom1number);
					// atom2number=AtomCubeStyle.atomOrderList.IndexOf(oratom2number);


			}
			else
			{
				switch(iType)
				{
					case 0:c= (MoleculeModel.carbonColor);v=new Vector3(1.7f*scale[0],1.7f*scale[0],1.7f*scale[0]);break;//c
					case 1:c= (MoleculeModel.nitrogenColor);v=new Vector3(1.55f*scale[1],1.55f*scale[1],1.55f*scale[1]);break;//n
					case 2:c= (MoleculeModel.oxygenColor);v=new Vector3(1.52f*scale[2],1.52f*scale[2],1.52f*scale[2]);break;//o
					case 3:c= (MoleculeModel.sulphurColor);v=new Vector3(2.27f*scale[3],2.27f*scale[3],2.27f*scale[3]);break;//s
					case 4:c= (MoleculeModel.phosphorusColor);v=new Vector3(1.18f*scale[4],1.18f*scale[4],1.18f*scale[4]);break;//p
					case 5:c= (MoleculeModel.hydrogenColor);v=new Vector3(1.2f*scale[5],1.2f*scale[5],1.2f*scale[5]);break;//h
				
					default:c= (MoleculeModel.unknownColor);v=new Vector3(1.0f*scale[6],1.0f*scale[6],1.0f*scale[6]);break;//unknown
				
				}
			}
			
			
			int Number=atomsLocation.Count;
			
			for(int k=0;k<Number;k++)
			{
				int order=(int)((atomOrderListArray[iType] as ArrayList)[k]);
				if(UI.GUIDisplay.file_extension=="xgmml")
				{
					float pointradius=((MoleculeModel.CSRadiusList[order]) as float[])[0];
					c=HexToColor((MoleculeModel.CSColorList[order] as string[])[0]);
					v=new Vector3(pointradius,pointradius,pointradius);
				}
					// Debug.Log(order);
				
				if(atomtype==UIData.AtomType.hyperball||atomtype==UIData.AtomType.cube)
				{
					CreateAtomHB(iType,k,atomsLocation,c,v,order);
				}
				else if(atomtype==UIData.AtomType.raycasting)
				{
					CreateAtomRC(iType,k,(k+1),atomsLocation,c,v,order);
				}
				else if(atomtype==UIData.AtomType.rcbillboard)
				{
					CreateAtomRCBB(iType,k,(k+1),atomsLocation,c,v,order);
				}
				else if(atomtype==UIData.AtomType.hbbillboard)
				{
					CreateAtomHBBB(iType,k,(k+1),atomsLocation,c,v,order);
				}
				else if(atomtype==UIData.AtomType.rcsprite)
				{
					CreateAtomRCSprite(iType,k,(k+1),atomsLocation,c,v,order);
				}		
				else if(atomtype==UIData.AtomType.multihyperball)
				{
					CreateAtomMtHyperBall(iType,k,(k+1),atomsLocation,c,v,order);
				}			
				else if(atomtype==UIData.AtomType.combinemeshball)
				{
					CreateCombineMeshHyperBall(iType,k,(k+1),atomsLocation,c,v,order);
				}			
				else if(atomtype==UIData.AtomType.particleball)
				{
					CreateAtomHB(iType,k,atomsLocation,c,v,order);
							// CreateParticleBall(i,k,(k+1),atomsLocation,c,v,order);
				}
			}
		}
		
		
		//iAtom : Atom index in atomLocationalist
		private void CreateAtomHB(int iAtom, ArrayList coordinates, ArrayList atomModels)
		{			
			GameObject Atom;
			Atom=GameObject.CreatePrimitive(PrimitiveType.Cube);
			float[]  fLocation = coordinates[iAtom] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);

			AtomModel atomModel = atomModels[iAtom] as AtomModel;

			Atom.transform.Translate(location);
			Atom.transform.parent = AtomCubeParent.transform;

			MoleculeModel.atoms.Add(Atom);		

			if(atomtype==UIData.AtomType.hyperball)
			{
				//Test platform to choose the good shader
				RuntimePlatform platform = Application.platform;
				switch(platform)
				{
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsWebPlayer:
					case RuntimePlatform.WindowsEditor:
						Atom.renderer.material.shader=Shader.Find("FvNano/Ball HyperBalls D3D");
						break;
					default :
						Atom.renderer.material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
						break;				
				}
			

				Atom.AddComponent("BallUpdateHB");
				if(UI.GUIDisplay.file_extension=="xgmml")
				{
					Atom.GetComponent<BallUpdateHB>().xgmml=true;
					Atom.GetComponent<BallUpdateHB>().z=(float)(fLocation[2]);
				}
			}
			else
			{
				Atom.AddComponent("BallUpdateCube");
				BallUpdateCube comp1 = Atom.GetComponent<BallUpdateCube>();
				comp1.SetRayonFactor(atomModel.scale/100);
				// comp1.rayon = atomModel.radius;
				// comp1.SetRayonFactor(atomModel.scale/100);
				// comp1.number = iAtom;
				// comp1.enabled = true;		
			}

			BallUpdate comp = Atom.GetComponent<BallUpdate>();
			comp.rayon = atomModel.radius;
			comp.atomcolor = atomModel.baseColor;
			comp.number = iAtom;
			comp.oldrayonFactor = atomModel.scale/100;  //Why division by 100 ???

			if(UI.GUIDisplay.file_extension=="xgmml")
			{
				comp.rayon = ((MoleculeModel.CSRadiusList[iAtom]) as float[])[0];
				comp.atomcolor = HexToColor((MoleculeModel.CSColorList[iAtom] as string[])[0]);
			}

			Atom.renderer.material.SetColor("_Color", comp.atomcolor);

//			Projector proj = Atom.AddComponent<Projector>();
			Atom.AddComponent<Projector>();
			
			comp.enabled = true;	

			Atom.tag = atomModel.type;

			if(atomtype==UIData.AtomType.particleball)
			{	
				Atom.renderer.enabled = false;
			}
		}

		


		
		private ArrayList AtomListByType(ArrayList alist,ArrayList typelist)
		{
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
			if(atomOrderList!=null)
			{
				atomOrderList.Clear();
				atomOrderList=null;
					
			}
			if(atomOrderListArray!=null)
			{
				atomOrderListArray.Clear();
				atomOrderListArray=null;
					
			}
			atomOrderList=new ArrayList();			
			atomOrderListArray=new ArrayList();	
			
					
			for(int i=0;i<alist.Count;i++)
			{
				string  type=(typelist[i] as AtomModel).type;
				switch(type)
					{
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
		private void CreateAtomHB(int iType,int iAtom , ArrayList atomLocationalist,Color c,Vector3 v,int order)
		{			
			GameObject Atom;
			Atom=GameObject.CreatePrimitive(PrimitiveType.Cube);
			float[]  fLocation=atomLocationalist[iAtom] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
			Atom.transform.Translate(location);
			Atom.renderer.material.SetColor("_Color",c);
			Atom.transform.parent = AtomCubeParent.transform;

			MoleculeModel.atoms.Add(Atom);		

			if(atomtype==UIData.AtomType.hyperball)
			{
				RuntimePlatform platform = Application.platform;
				switch(platform)
				{
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsWebPlayer:
					case RuntimePlatform.WindowsEditor:
						Atom.renderer.material.shader=Shader.Find("FvNano/Ball HyperBalls D3D");
						break;
					default :
						Atom.renderer.material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
						break;				
				}
			
				Atom.AddComponent("BallUpdateHB");
				BallUpdateHB comp = Atom.GetComponent<BallUpdateHB>();
				comp.rayon=(float)(v[0]);
				comp.atomcolor=c;
				comp.number=order;
				comp.z=(float)(fLocation[2]);
//				Projector proj = Atom.AddComponent<Projector>();
				Atom.AddComponent<Projector>();
				if(UI.GUIDisplay.file_extension=="xgmml")comp.xgmml=true;
				comp.enabled = true;		
				switch(iType)
				{
					case 0:
						// comp.rayonFactor=(MoleculeModel.carbonScale)/100;break;
						comp.oldrayonFactor=(MoleculeModel.carbonScale)/100;break;
					case 1:
						// comp.rayonFactor=(MoleculeModel.nitrogenScale)/100;break;
						comp.oldrayonFactor=(MoleculeModel.nitrogenScale)/100;break;
					case 2:
						// comp.rayonFactor=(MoleculeModel.oxygenScale)/100;break;
						comp.oldrayonFactor=(MoleculeModel.oxygenScale)/100;break;
					case 3:
						// comp.rayonFactor=(MoleculeModel.sulphurScale)/100;break;
						comp.oldrayonFactor=(MoleculeModel.sulphurScale)/100;break;
					case 4:
						// comp.rayonFactor=(MoleculeModel.phosphorusScale)/100;break;
						comp.oldrayonFactor=(MoleculeModel.phosphorusScale)/100;break;
					case 5:
						// comp.rayonFactor=(MoleculeModel.hydrogenScale)/100;break;
						comp.oldrayonFactor=(MoleculeModel.hydrogenScale)/100;break;
					default:
						// comp.rayonFactor=1.0f;break;
						comp.oldrayonFactor=2.0f;break;
			
				}				
			}
			else
			{
				Atom.AddComponent("BallUpdateCube");
				BallUpdateCube comp1 = Atom.GetComponent<BallUpdateCube>();
						// comp1.rayon = Atom.transform.localScale.x*2;
				comp1.rayon = (float)(v[0])*2;
				float rayonfactor;
				switch(iType)
				{
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

			switch(iType)
			{
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;
				
			}
			if(atomtype==UIData.AtomType.particleball)
			{	
				Atom.renderer.enabled = false;
					// Atom.active = false;
			}

			
			
		}
		
		private void CreateAtomRC(int type,int start ,int end,ArrayList atomLocationalist,Color c,Vector3 v,int order)
		{		
			
			GameObject Atom;
			Atom=GameObject.CreatePrimitive(PrimitiveType.Cube);
			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
			Atom.transform.Translate(location);
			Atom.renderer.material.SetColor("_Color",c);
			MoleculeModel.atoms.Add(Atom);		
			Atom.renderer.material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
			Atom.AddComponent("BallUpdateRC");
			BallUpdateRC comp = Atom.GetComponent<BallUpdateRC>();
			comp.rayon=(float)(v[0]);
			comp.atomcolor=c;
			comp.number=order;
			comp.enabled = true;		
			switch(type)
			{
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;
				
			}

		}

		private void CreateAtomRCBB(int type,int start ,int end,ArrayList atomLocationalist,Color c,Vector3 v,int order)
		{		
			
			GameObject Atom;
			if(UIData.toggleClip)
			{
				Atom=Clip4RayCasting.CreateClip();
			}
			else
			{
				Atom=GameObject.CreatePrimitive(PrimitiveType.Plane);
			}

			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
		
			Atom.transform.Translate(location);
			Atom.renderer.material.SetColor("_Color",c);
			MoleculeModel.atoms.Add(Atom);		
			Atom.AddComponent("CameraFacingBillboard");
			Atom.GetComponent<CameraFacingBillboard>().cameraToLookAt = GameObject.Find("Camera").camera;
			Atom.transform.renderer.material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
			Atom.AddComponent("BallUpdateRC");
			BallUpdateRC comp = Atom.GetComponent<BallUpdateRC>();
			comp.rayon=(float)(v[0]);
			comp.atomcolor=c;
			comp.number=order;
			comp.enabled = true;		
			switch(type)
			{
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;
				
			}
		}
		private void CreateAtomHBBB(int type,int start ,int end,ArrayList atomLocationalist,Color c,Vector3 v,int order)
		{		
			GameObject Atom;
			if(UIData.toggleClip)
			{
				Atom=Clip4RayCasting.CreateClip();
			}
			else
			{
				Atom=GameObject.CreatePrimitive(PrimitiveType.Plane);
			}
			
			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
				
			Atom.transform.Translate(location);
			Atom.renderer.material.SetColor("_Color",c);
			MoleculeModel.atoms.Add(Atom);		
			Atom.AddComponent("CameraFacingBillboard");
			Atom.GetComponent<CameraFacingBillboard>().cameraToLookAt = GameObject.Find("Camera").camera;
			Atom.transform.renderer.material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
			Atom.AddComponent("BallUpdateHB");
			BallUpdateHB comp = Atom.GetComponent<BallUpdateHB>();
			comp.rayon=(float)(v[0]);
			comp.atomcolor=c;
				// Debug.Log(start+"/"+c);
			comp.number=order;
			comp.enabled = true;		
			
			switch(type)
			{
				
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;
				
			}			
		}
		
		private void CreateAtomRCSprite(int type,int start ,int end,ArrayList atomLocationalist,Color c,Vector3 v,int order)
		{		
			LinkedSpriteManager linkedSpriteManager=null; 		
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
			SpriteManager.GetComponent <MeshRenderer>().enabled=true;

			if(SpriteManager.GetComponent <LinkedSpriteManager>()==null)
			{
				SpriteManager.AddComponent("LinkedSpriteManager");
				linkedSpriteManager= (LinkedSpriteManager)Component.FindObjectOfType(typeof(LinkedSpriteManager));			
				Material mat = Resources.Load("Materials/hyperballshader", typeof(Material)) as Material;				
				linkedSpriteManager.material=mat;
				linkedSpriteManager.allocBlockSize=atomLocationalist.Count;
			}		
			else
			{
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
			Atom.renderer.material.SetColor("_Color",c);
			BallUpdateHB comp = Atom.GetComponent<BallUpdateHB>();
			comp.rayon=(float)(v[0]);
			comp.atomcolor=c;
			comp.number=order;
			comp.enabled = true;
			linkedSpriteManager.AddSprite(Atom,
				1f,1f,
				0,0,
				256,256,
				true
				);	
			switch(type)
			{
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;
				
			}		
			
		}
		

		private void CreateAtomMtHyperBall(int type,int start ,int end,ArrayList atomLocationalist,Color c,Vector3 v,int order)
		{		
			GameObject 	Atom;
			if(!GameObject.Find("MultiHBCube"))
			{
				Atom=GameObject.CreatePrimitive(PrimitiveType.Cube);
				Atom.name="MultiHBCube";
			}
			else
			{
				Atom=GameObject.Find("MultiHBCube");
			}
			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
			Material newMat = new Material(Shader.Find("FvNano/Ball HyperBalls OpenGL"));
			newMat.SetVector("_TexPos",location);
			newMat.SetFloat("_Rayon",(float)(v[0])*0.1f);
			newMat.SetColor("_Color",c);
			
			ArrayList materialArray= new ArrayList(Atom.transform.renderer.materials);
			materialArray.Add(newMat);
			Material[] array = new Material[materialArray.Count];
			materialArray.CopyTo( array );
			Atom.transform.renderer.materials = array;	
			switch(type)
			{
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;		
			}
		
		}	
				
		private void CreateCombineMeshHyperBall(int type,int start ,int end,ArrayList atomLocationalist,Color c,Vector3 v,int order)
		{		
			GameObject 	SpriteManager;
			if(!GameObject.Find("SpriteManager"))
			{
				SpriteManager=GameObject.CreatePrimitive(PrimitiveType.Cube);
				SpriteManager.name="SpriteManager";
			}
			else
			{
				SpriteManager=GameObject.Find("SpriteManager");
			}

			GameObject Atom;
			if(UIData.toggleClip)
			{
				Atom=Clip4RayCasting.CreateClip();
			}
			else
			{
				Atom=GameObject.CreatePrimitive(PrimitiveType.Plane);
			}

			Atom.transform.parent=SpriteManager.transform;
			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
			Atom.transform.Translate(location);
			Atom.renderer.material.SetColor("_Color",c);
			MoleculeModel.atoms.Add(Atom);		
			Atom.AddComponent("CameraFacingBillboard");
			Atom.GetComponent<CameraFacingBillboard>().cameraToLookAt = GameObject.Find("Camera").camera;
			Atom.transform.renderer.material.shader=Shader.Find("FvNano/Ball HyperBalls OpenGL");
			Atom.AddComponent("BallUpdateHB");
			BallUpdateHB comp = Atom.GetComponent<BallUpdateHB>();
			comp.rayon=(float)(v[0]);
			comp.atomcolor=c;
			comp.number=order;
			comp.enabled = true;		
			switch(type)
			{
				case 0:Atom.tag="C";break;
				case 1:Atom.tag="N";break;
				case 2:Atom.tag="O";break;
				case 3:Atom.tag="S";break;
				case 4:Atom.tag="P";break;
				case 5:Atom.tag="H";break;
				default:Atom.tag="X";break;	
			}	
		}
		private void CreateParticleBall(int type,int start,ArrayList atomLocationalist,Color c,Vector3 v,int order)
		{		
			// Debug.Log("CreateParticleBall"+start);
			float[]  fLocation=atomLocationalist[start] as float[];
			Vector3 location=new Vector3(fLocation[0],fLocation[1],fLocation[2]);
			MoleculeModel.p[start].position=location;
			MoleculeModel.p[start].size=(float)(v[0]);
			MoleculeModel.p[start].color=c;
			MoleculeModel.p[start].energy=1000;		
			// MoleculeModel.p[atomindex+start].position=location;
			// MoleculeModel.p[atomindex+start].size=(float)(v[0]);
			// MoleculeModel.p[atomindex+start].color=c;
			// MoleculeModel.p[atomindex+start].energy=1000;		
		}
		
	}

}
