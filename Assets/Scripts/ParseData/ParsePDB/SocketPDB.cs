/// @file SocketPDB.cs
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
/// $Id: SocketPDB.cs 564 2014-07-01 12:46:16Z tubiana $
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
	using System.Collections.Generic;
	using System.IO;
	using ParseData.IParsePDB;
	using System.Net;
	using System;
	//using System.Net.Sockets;

	using SocketConnect.UnitySocket;
	using Cmd;
	using Molecule.Control;
	using Molecule.Model;
namespace  ParseData.ParsePDB
{
	
	public class SocketPDB
	{
		
		public List<float[]> alist=new List<float[]>();
		
		private List<AtomModel> typelist=new List<AtomModel>();
		
		public ArrayList clubLocationalist=new ArrayList();
		private ArrayList clubRotationList =new ArrayList();
		private string Atoms="";
		private string Clubs="";
		public float progress;
		//private string id="";
		public SocketPDB(string id)
		{
			loadPDB(id);
		}
		
	
		
		public  void loadPDB(string id)
		{
			    
	        UnitySocket.Send(CommandID.GETPDB);
			UnitySocket.Send(0);
			UnitySocket.Send(id);
	        int num0=UnitySocket.ReceiveInt();
			int num1=UnitySocket.ReceiveInt();
			//print(num);
			
			Debug.Log(num0+"|"+num1);

			for(int k=0;k<num0+num1;k++)
			{

				UnitySocket.Send(CommandID.GETPDB);
				UnitySocket.Send(k+1);
				if(k<num0)
				{
					string sonAtoms=UnitySocket.ReceiveString(60000);

					Atoms+=sonAtoms.Trim();
//					Debug.Log("sonAtoms is: "+sonAtoms);
			
					
				}
				
				else
				{
					string sonClubs=UnitySocket.ReceiveString(68000);
					Clubs+=sonClubs.Trim();					
				}
					
			}
			string [] sArray=Atoms.Split('$');
			Debug.Log("length:"+sArray.Length);
			for(int i=0;i<sArray.Length-1;i++)
			{
						if(sArray[i]=="")continue;
						string [] ssArray=sArray[i].Split('#');

//						Debug.Log(i+"|"+sArray[i]+"////");

						float[] vect=new float[3];

						float.TryParse(ssArray[0],out vect[0]);
						float.TryParse(ssArray[1],out vect[1]);
						float.TryParse(ssArray[2],out vect[2]);
//						for(int kk=0;kk<vect.Length;kk++)
//						{
//							Debug.Log(i+"|"+vect[kk]+"////");
//						}
						typelist.Add(AtomModel.GetModel(ssArray[3]));
						alist.Add(vect);
				
			}

			Debug.Log(Clubs);
			Vector3 minPoint=Vector3.zero;
    		Vector3 maxPoint=Vector3.zero;

			MoleculeModel.atomsLocationlist=alist;
			MoleculeModel.atomsTypelist=typelist;
			//MoleculeModel.bondList=ControlMolecule.CreateBondsList(alist,typelist);
			//Debug.Log("======================= Bond List" + MoleculeModel.bondList.ToString());
			MoleculeModel.bondEPList=ControlMolecule.CreateBondsEPList(alist,typelist);
			MoleculeModel.bondEPSugarList = ControlMolecule.CreateBondsEPList(MoleculeModel.atomsSugarLocationlist,MoleculeModel.atomsSugarTypelist);
			
//			float [] a0=alist[0] as float[];
//			MoleculeModel.cameraLocation.x=MoleculeModel.target.x=a0[0];
//			MoleculeModel.cameraLocation.y=MoleculeModel.target.y=a0[1];
//			MoleculeModel.target.z=a0[2];
//			MoleculeModel.cameraLocation.z=a0[2]-150;
//			

			for(int i=0; i<alist.Count; i++)
    		{
    			float[] position= alist[i] as float[];
    			minPoint = Vector3.Min(minPoint, new Vector3(position[0],position[1],position[2]));
	        	maxPoint = Vector3.Max(maxPoint, new Vector3(position[0],position[1],position[2]));
    		}
    		Vector3 centerPoint = minPoint + ((maxPoint - minPoint) / 2);
			//MoleculeModel.target = centerPoint;
			
			MoleculeModel.Offset = -centerPoint;
			
			
			for(int i=0; i<alist.Count; i++)
			{
				float[] position= alist[i] as float[];
				float[] vect=new float[3];
				vect[0]=position[0]+MoleculeModel.Offset.x;
				vect[1]=position[1]+MoleculeModel.Offset.y;
				vect[2]=position[2]+MoleculeModel.Offset.z;

				alist[i]=vect;
			}
			
			
//			Debug.Log("MoleculeModel.target "+MoleculeModel.target);
			MoleculeModel.cameraLocation.x=MoleculeModel.target.x;
			MoleculeModel.cameraLocation.y=MoleculeModel.target.y;
//			MoleculeModel.cameraLocation.z=MoleculeModel.target.z-((maxPoint - minPoint) ).z;
			MoleculeModel.cameraLocation.z=MoleculeModel.target.z-((maxPoint - minPoint) ).z;

			
			
			//MoleculeModel.bondList=ControlMolecule.CreateBondsList(alist,typelist);
			MoleculeModel.bondEPList=ControlMolecule.CreateBondsEPList(alist,typelist);
			MoleculeModel.bondEPSugarList = ControlMolecule.CreateBondsEPList(MoleculeModel.atomsSugarLocationlist,MoleculeModel.atomsSugarTypelist);
			
			
			
			
			MoleculeModel.atomsnumber = alist.Count;
			MoleculeModel.bondsnumber = MoleculeModel.bondEPList.Count;
			
			
						string [] sClubArray=Clubs.Split('$');
			for(int i=0;i<sClubArray.Length-1;i++)
			{
						string [] ssClubArray=sClubArray[i].Split('#');
						float[] vect=new float[3];
						vect[0]=float.Parse(ssClubArray[0]);
						vect[1]=float.Parse(ssClubArray[1]);
						vect[2]=float.Parse(ssClubArray[2]);
						clubLocationalist.Add(vect);
				

					    float[] vectRotation=new float[3];
						vectRotation[0]=float.Parse(ssClubArray[3]);
						vectRotation[1]=float.Parse(ssClubArray[4]);
						vectRotation[2]=0f;

						clubRotationList.Add(vectRotation);			
			}
			
			Debug.Log(clubRotationList.Count);
			
		}
		
		public List<float[]> getAtoms()
		{
			
			return alist;
			
		}
		
		
		public List<AtomModel> getTypes()
		{
			return typelist;
		}
		
		public ArrayList getClubLocation()
		{
			return clubLocationalist;
		}
		
		
		public ArrayList getClubRotation()
		{
			return clubRotationList;
		}
	}
	
}
