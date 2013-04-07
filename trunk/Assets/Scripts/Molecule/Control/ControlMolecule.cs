/// @file ControlMolecule.cs
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
/// $Id: ControlMolecule.cs 224 2013-04-06 23:00:34Z baaden $
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

namespace Molecule.Control
{
	using UnityEngine;
	using System.Collections;
	using Molecule.Model;
	public class ControlMolecule 
	{
	
		public ControlMolecule()
		{
			
		}
		
		
		public  static ArrayList CreateBondsList(ArrayList atomsLocationlist,ArrayList atomsTypelist)
		{
			//int k=0;
			//string clubs="";
			

			ArrayList bond=new ArrayList();
			for(int i=0;i<atomsLocationlist.Count;i++)
			{
//			Debug.Log("atomsLocationlist:"+"x:"+(atomsLocationlist[i] as float[])[0]+"|y:"+(atomsLocationlist[i] as float[])[1]+"|z:"+(atomsLocationlist[i] as float[])[2]);
				float [] atom0=atomsLocationlist[i] as float[];
				string atomtype0=(atomsTypelist[i] as AtomModel).type;
				float x0=atom0[0];
				float y0=atom0[1];
				float z0=atom0[2];				
//				Debug.Log("x0="+x0+"y0="+y0+"z0="+z0);
				for(int j=1;j<80;j++)
				{
					if(i+j<atomsLocationlist.Count)
					{
						float[] atom1 = atomsLocationlist[i+j] as float[];
						string atomtype1 = (atomsTypelist[i+j] as AtomModel).type;
						float cutoff = 1.6f;
						
						if((atomtype0=="H")&&(atomtype1=="H"))continue;
						if((atomtype0=="S")||(atomtype1=="S"))cutoff = 1.84f;
						if((atomtype0=="O"&&atomtype1=="P")||(atomtype1=="O"&&atomtype0=="P"))cutoff = 1.84f;
						if((atomtype0=="O"&&atomtype1=="H")||(atomtype1=="O"&&atomtype0=="H"))cutoff = 1.84f;
						
						float x1=atom1[0];
						float y1=atom1[1];
						float z1=atom1[2];
						
//				Debug.Log("x1="+x1+"y1="+y1+"z1="+z1);
						// Vector3 atomLocation=new Vector3();
						// atomLocation.x=(x0+x1)/2.0f;
						// atomLocation.y=(y0+y1)/2.0f;
						// atomLocation.z=(z0+z1)/2.0f;
						
//						atomLocation.x=x0;
//						atomLocation.y=y0;
//						atomLocation.z=z0;
						
						
//						Debug.Log("x0="+x0+"x1"+x1+"y0"+y0+"y1"+y1+"z0"+z0+"z1"+z1);
//						Debug.Log("(x0+x1)/2.0f?"+(x0+x1)/2.0f);
//						Debug.Log("atomLocation.x"+atomLocation.x);
//						Debug.Log("atomLocation="+atomLocation);
//						Debug.Log("atomLocation.x="+atomLocation.x+";atomLocation.y="+atomLocation.y+";atomLocation.z="+atomLocation.z);
						
						float dist = (x0-x1)*(x0-x1)+(y0-y1)*(y0-y1)+(z0-z1)*(z0-z1);
						if(Mathf.Sqrt(dist) <= cutoff)
						{
							int [] atomsIds = {i,i+j};
							bond.Add(atomsIds);
							
// 							Vector3 bondLookAt=new Vector3(x1,y1,z1);							
// 							Vector3 [] location=new Vector3[2];
// 							location[0]=atomLocation;
// 							location[1]=bondLookAt;
// //							Debug.Log("location[0].x="+location[0].x);
// //							Debug.Log("location[1].x="+location[1].x);
// 							bond.Add(location);
						}
					}
					
					
				}
			}
			
			return bond;
		}
		
		
		public  static ArrayList CreateBondsEPList(ArrayList atomsLocationlist,ArrayList atomsTypelist)
		{
			//int k=0;
			//string clubs="";
			ArrayList bond=new ArrayList();
			Debug.Log("atomsLocationlist.Count "  + atomsLocationlist.Count);
			Debug.Log("atomsTypelist.Count "  + atomsTypelist.Count);

			for(int i=0;i<atomsLocationlist.Count;i++)
			{
				float [] atom0=atomsLocationlist[i] as float[];
				string atomtype0=(atomsTypelist[i] as AtomModel).type;
				
				//Debug.Log("i ********** "  + i);

				float x0=atom0[0];
				float y0=atom0[1];
				float z0=atom0[2];
				
				for(int j=1;j<150;j++)
				{
					if(i+j<atomsLocationlist.Count)
					{
						float[] atom1 = atomsLocationlist[i+j] as float[];
						string atomtype1 = (atomsTypelist[i+j] as AtomModel).type;
						string a1name = MoleculeModel.atomsNamelist[i+j] as string;
						float cutoff = 1.7f;
						
						if((atomtype0=="H")&&(atomtype1=="H"))continue;
						if((atomtype0=="S")||(atomtype1=="S"))cutoff = 1.84f;
						if((atomtype0=="P")||(atomtype1=="P"))cutoff = 1.7f;
						if((a1name == "CAL") && (atomtype0=="O")) cutoff = 3.5f;

						
						float x1=atom1[0];
						float y1=atom1[1];
						float z1=atom1[2];

						float dist = (x0-x1)*(x0-x1)+(y0-y1)*(y0-y1)+(z0-z1)*(z0-z1);
						if(Mathf.Sqrt(dist) <= cutoff)
						{
							int [] atomsIds = {i,i+j};
							bond.Add(atomsIds);
						}
					}
					
					
				}
			}
			Debug.Log("bond.Count:"+bond.Count);
			return bond;
		}
		
		public  static ArrayList CreateBondsCAList(ArrayList caChainlist)
		{
			//int k=0;
			//string clubs="";
			ArrayList bond=new ArrayList();
			
			for(int i=1; i<caChainlist.Count; i++)
			{
				if((string)(caChainlist[i-1])==(string)(caChainlist[i]))
				{
					int[] splineIds	= {i-1,i};
					bond.Add(splineIds);
					// Debug.Log("CaSplineTypeList[i] = "  + MoleculeModel.CaSplineTypeList[i]+"CaSplineTypeList[i+1] = "  + MoleculeModel.CaSplineTypeList[i+1]);
				}
			}
			Debug.Log("bond.Count:"+bond.Count);
			return bond;
		}


		public static ArrayList CreateBondsList_HiRERNA(ArrayList atomnames)
		{
			//We suppose the names are ordered as this:
			//P O5* C5* CA CY b1 [b2]
			ArrayList bonds = new ArrayList();
			int N = atomnames.Count;
			int k;
//			int[] bond;
			for(int i=0; i<N-1; ++i)
			{
				string a1 = atomnames[i] as string;
				if(a1 == "P")
				{
					//Backward search for "CA"
					for(k=i; k>=0 && k>=i-5 && (atomnames[k] as string)!="CA"; --k);
					if(k>=0 && k>=i-5) bonds.Add(new int[] {i,k});

					//Forward search for "O5*"
					if(atomnames[i+1] as string == "O5*")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "O5* missing");
				}
				else if(a1 == "O5*")
				{
					//Forward search for "C5*"
					if(atomnames[i+1] as string == "C5*")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "C5* missing");
				}
				else if(a1 == "C5*")
				{
					//Forward search for "CA"
					if(atomnames[i+1] as string == "CA")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "CA missing");
				}
				else if(a1 == "CA")
				{
					//Forward search for "CY"
					if(atomnames[i+1] as string == "CY")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "CY missing");
				}
				else if(a1 == "CY")
				{
					//Forward search for G1, A1, U1 or C1
					string a2 = atomnames[i+1] as string;
					if(a2 == "G1" || a2 == "A1" || a2 == "U1" || a2 == "C1")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "b1 missing");
				}
				else if(a1 == "G1")
				{
					//Forward search for "G2"
					if(atomnames[i+1] as string == "G2")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "G2 missing");
				}
				else if(a1 == "A1")
				{
					//Forward search for "A2"
					if(atomnames[i+1] as string == "A2")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "A2 missing");
				}
			}
			Debug.Log("HiRERNA bond count : " + bonds.Count);
			return bonds;
		}


		public  static ArrayList CreateBondsCSList(ArrayList atomsLocationlist)
		{
			//int k=0;
			//string clubs="";
			ArrayList bond=new ArrayList();
//			Debug.Log("atomsLocationlist.Count "  + atomsLocationlist.Count);
//			Debug.Log("atomsTypelist.Count "  + atomsTypelist.Count);



			Debug.Log("atomsLocationlist.Count:"+atomsLocationlist.Count);

//			int[] ary = (int[])((MoleculeModel.CSidList).ToArray(typeof(int)));
			for(int i=0;i<atomsLocationlist.Count;i++)
			{
//				Debug.Log("atomsLocationlist[i][0]="+(atomsLocationlist[i] as float[])[0]);
				int [] atom0=atomsLocationlist[i] as int[];
//				string atomtype0=atomsTypelist[i] as string;
				
				//Debug.Log("i ********** "  + i);

				int source=atom0[0];
				int target=atom0[1];
//				Vector3 atom0position=new Vector3();
//				Vector3 atom1position=new Vector3();
				int atom0sign=0;
				int atom1sign=0;
				
				//Vector3 atomtype=new Vector3();
//				Debug.Log("source="+source+",target="+target);
				for(int j=0;j<MoleculeModel.CSidList.Count;j++)
				{
					
//					Debug.Log("source="+source+",target="+target);
					int [] number=MoleculeModel.CSidList[j] as int[];
//					Debug.Log("number[0]="+number[0]);
//					int number=ary[j];
//					int number=int.Parse(MoleculeModel.CSidList[j] as string);

					if(source==number[0])
					{
						atom0sign=j;
					}
					if(target==number[0])
					{
						atom1sign=j;
					}	
									
				}
				bond.Add(new int[] {atom0sign, atom1sign});
// 				atomtype.x=atom0sign;
// 				atomtype.y=atom1sign;
// 				atomtype.z=0;
				
// //				Debug.Log("atom0sign="+atom0sign+",atom1sign="+atom1sign);
				
// 				float [] atom00=(MoleculeModel.atomsLocationlist[atom0sign]) as float[];
// 				float [] atom11=(MoleculeModel.atomsLocationlist[atom1sign]) as float[];
				
// 				atom0position.x=atom00[0];
// 				atom0position.y=atom00[1];
// 				atom0position.z=atom00[2];
							
// 				atom1position.x=atom11[0];
// 				atom1position.y=atom11[1];
// 				atom1position.z=atom11[2];


// 				Vector3 [] location=new Vector3[3];
// 				location[0]=atom0position;
// 				location[1]=atom1position;
// 				location[2]=atomtype;
							
// 				bond.Add(location);



									
				
			}
			
			return bond;
		}

		
		// public static GameObject[] SetBoxes(GameObject[] Ces,GameObject[] Nes,GameObject[] Oes,
		// 	GameObject[] Ses,GameObject[] Pes,GameObject[] Hes,GameObject[] NOes)
		// {
			
		// 	GameObject[] boxes=new GameObject[Ces.Length+Nes.Length+Oes.Length+
		// 		Ses.Length+Pes.Length+Hes.Length+NOes.Length];
		// 	int i=0;
		// 	for(int ci=0;ci<Ces.Length;ci++,i++)
		// 	{
		// 		boxes[i]=Ces[ci];
				
		// 	}
			
		// 	for(int ni=0;ni<Nes.Length;ni++,i++)
		// 	{
		// 		boxes[i]=Nes[ni];
				
		// 	}
			
		// 	for(int oi=0;oi<Oes.Length;oi++,i++)
		// 	{
		// 		boxes[i]=Oes[oi];
				
		// 	}
			
		// 	for(int si=0;si<Ses.Length;si++,i++)
		// 	{
		// 		boxes[i]=Ses[si];
				
		// 	}
			
		// 	for(int pi=0;pi<Pes.Length;pi++,i++)
		// 	{
		// 		boxes[i]=Pes[pi];
				
		// 	}
			
		// 	for(int hi=0;hi<Hes.Length;hi++,i++)
		// 	{
		// 		boxes[i]= Hes[hi];
				
		// 	}		
			
		// 	for(int noi=0;noi<NOes.Length;noi++,i++)
		// 	{
		// 		boxes[i]=NOes[noi];
				
		// 	}
			

		// 	return boxes;
		// }
		
		

		
	}
}
