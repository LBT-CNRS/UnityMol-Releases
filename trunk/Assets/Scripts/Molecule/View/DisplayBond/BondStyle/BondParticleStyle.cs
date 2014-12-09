/// @file BondParticleStyle.cs
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
/// $Id: BondParticleStyle.cs 329 2013-08-06 13:47:40Z erwan $
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
	using System.Collections.Generic;
	using Molecule.Model;
	using Molecule.Control;
	using Config;
	using UI;
	using Molecule.View.DisplayAtom;
	public class BondParticleStyle:IBondStyle
	{
		public int number=1;
		public List<int[]> bondList=new List<int[]>();
		public List<int[]> bondEPList=new List<int[]>();
		
		Particle[] p = new Particle[MoleculeModel.bondEPList.Count/FunctionConfig.number];
		int bondindex=0;

		public BondParticleStyle()
		{
			number=FunctionConfig.number;
		}

		
		public void DisplayBonds()
		{
				bondEPList=MoleculeModel.bondEPList;
				Debug.Log("DisplayBonds??bondEPList.Count "  + bondEPList.Count);
				int Number=bondEPList.Count/number;

				for(int i=0;i<Number;i++)
				{
					if(i!=number-1)
					{
						CreateCylinderByParticle(i*number,(i+1)*number);
					}
					else
					{
						CreateCylinderByParticle(i*number,bondEPList.Count);
					}
				}
				GameObject ParticleStickManager=GameObject.Find("ParticleStickManager");
						ParticleEffect particleeffect=ParticleStickManager.transform.GetComponent<ParticleEffect>();
						particleeffect.atomcount=MoleculeModel.bondEPList.Count/FunctionConfig.number;
						particleeffect.p=p;
						particleeffect.SpawnEffect ();
						Debug.Log("the length of p ="+p.Length);

								
			
			
		}
		
		private void CreateCylinderByParticle(int start, int end) // Apparently unused function. Would make ParticleSticks.
		{
			
			
			GameObject ParticleManager=GameObject.Find("ParticleStickManager");
//			ParticleEffect particleeffect=ParticleManager.transform.GetComponent<ParticleEffect>();
			ParticleManager.transform.GetComponent<ParticleEffect>();
			for(int i=start;i<end;i++)
			{	
//				Vector3[]  location=bondEPList[i] as Vector3[];
				Vector3[] location = new Vector3[bondEPList.Count];
				for(int j=0; i<(bondEPList.Count); j++)
				{
					Vector3 v = new Vector3(	(bondEPList[i][0]),
												(bondEPList[i][1]),
												(bondEPList[i][2])	);
					location[i] = v;
				}
				
				Vector3 atom0position=new Vector3();
				Vector3 atom1position=new Vector3();
				Vector3 atomtype=new Vector3();		
				atom0position=location[0];
				atom1position=location[1];
				atomtype=location[2];
//				string atom0type="X";
//				string atom1type="X";
//				Color atom0color=Color.red;
//				Color atom1color=Color.red;
				float atomradius0=1f;
				float atomradius1=1f;
//				int oratom1number=0;
//				int oratom2number=0;
			
//				int atom1number;
//				int atom2number;
			
				if(UI.GUIDisplay.file_extension=="xgmml")
				{
//					oratom1number=(int)atomtype.x;
//					oratom2number=(int)atomtype.y;
				}
				else
				{
//					oratom1number=(int)atomtype.z/100;
//					oratom2number=(int)atomtype.z-oratom1number*99;
				}
				
				if(UIData.isSphereToCube==true)
				{
//					atom1number=AtomCubeStyle.atomOrderList.IndexOf(oratom1number);
//					atom2number=AtomCubeStyle.atomOrderList.IndexOf(oratom2number);
				}
				else
				{
//					atom1number=AtomSphereStyle.atomOrderList.IndexOf(oratom1number);
//					atom2number=AtomSphereStyle.atomOrderList.IndexOf(oratom2number);
				}
				switch((int)atomtype.x)
				{
								case 1:
//									atom0type="C";
//									atom0color= (MoleculeModel.carbonColor);
									atomradius0=1.7f;
									break;
								case 2:
//									atom0type="N";
//									atom0color= (MoleculeModel.nitrogenColor);
									atomradius0=1.55f;
									
									break;
								case 3:
//									atom0type="O";
//									atom0color= (MoleculeModel.oxygenColor);
									atomradius0=1.52f;
									
									break;
								case 4:
//									atom0type="S";
//									atom0color= (MoleculeModel.sulphurColor);
									atomradius0=2.27f;
									
									break;
								case 5:
//									atom0type="P";
//									atom0color= (MoleculeModel.phosphorusColor);
									atomradius0=1.18f;
									
									break;
								case 6:
//									atom0type="H";
//									atom0color= (MoleculeModel.hydrogenColor);
									atomradius0=1.2f;
									
									break;
								case 7:
//									atom0type="X";
//									atom0color= (MoleculeModel.unknownColor);
									atomradius0=1.0f;
									
									break;						
					}
			
					switch((int)atomtype.y)
					{
								case 1:
//									atom1type="C";
//									atom1color= (MoleculeModel.carbonColor);
									atomradius1=1.7f;

									break;
								case 2:
//									atom1type="N";
//									atom1color= (MoleculeModel.nitrogenColor);
									atomradius1=1.55f;
									
									break;
								case 3:
//									atom1type="O";
//									atom1color= (MoleculeModel.oxygenColor);
									atomradius1=1.52f;
									
									break;
								case 4:
//									atom1type="S";
//									atom1color= (MoleculeModel.sulphurColor);
									atomradius1=2.27f;
									
									break;
								case 5:
//									atom1type="P";
//									atom1color= (MoleculeModel.phosphorusColor);
									atomradius1=1.18f;
									
									break;
								case 6:
//									atom1type="H";
//									atom1color= (MoleculeModel.hydrogenColor);
									atomradius1=1.2f;
									
									break;
								case 7:
//									atom1type="X";
//									atom1color= (MoleculeModel.unknownColor);
									atomradius1=1.0f;
									
									break;						
					}
			
					Quaternion q=Quaternion.identity;
					Vector3 v3=new Vector3();
					v3.x=location[1].x*180f/3.1416f;
					v3.y=location[1].y*180f/3.1416f;
					v3.z=location[1].z*180f/3.1416f;
					q.eulerAngles=v3;					
					p[bondindex+start].position=(atom0position+atom1position)/2;
					p[bondindex+start].size=(float)(atomradius0+atomradius1)/2;
					p[bondindex+start].color=Color.yellow;
					p[bondindex+start].energy=1000;
					
			}

		}
		
	}

}
