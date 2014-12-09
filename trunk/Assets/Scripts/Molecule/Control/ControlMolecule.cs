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
/// $Id: ControlMolecule.cs 660 2014-08-26 13:46:34Z sebastien $
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
using System.Text;

namespace Molecule.Control {
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using Molecule.Model;
	using ParseData.ParsePDB;
	using UI;
	
	public class ControlMolecule {
	
		public ControlMolecule() {}
		
		public static void CreateMolecule(TextReader sr) {
			List<float[]>	alist			=	new List<float[]>();
			List<float[]>	calist			=	new List<float[]>();
			
			List<float>		BFactorList		=	new List<float>();
         	
         	List<string>	resnamelist		=	new List<string>();
         	List<string>	atomsNameList	=	new List<string>();
         	List<string>	caChainlist		=	new List<string>();
			
			List<AtomModel>	typelist		=	new List<AtomModel>();
			List<string>	chainlist		= 	new List<string>();
			
			List<Color>		colorList		= new List<Color>();

			List<float[]>   sshelixlist     = new List<float[]> ();
			List<float[]>   sssheetlist     = new List<float[]> ();
			
			RequestPDB.ReadPDB(sr, alist, calist, BFactorList, resnamelist, atomsNameList, caChainlist, typelist, chainlist, colorList, sshelixlist, sssheetlist);
			
			BuildMoleculeComponents();
			CreateSplines();
			CreateResidues();
			CreateResiduesSugar2();
			
			if(UIData.loadHireRNA)
			{
				BuildHireRnaHydrogenBondsStructures();
			}

			if(GUIDisplay.pdbID == "3EI0")
				UIData.isGLIC = true;
			
			UIData.isParticlesInitialized = false ; // new file, new particle system needed
			MoleculeModel.networkLoaded = false ; // protein loaded, network removed if present
		}
		
		public static void CreateResidues() {
			List<float[]>						alist				=	MoleculeModel.atomsLocationlist;
			List<string>						resNames			=	MoleculeModel.atomsResnamelist;
			List<string>						aNamesList			=	MoleculeModel.atomsNamelist;
			List<int>							residueIds			=	MoleculeModel.residueIds;
			
			List<string>						resNamesPerResidue	=	new List<string>();
			List<Vector3>						residues			=	new List<Vector3>();
			List<Dictionary<string, Vector3>>	residueDictList		=	new List<Dictionary<string, Vector3>>();
			Dictionary<string, Vector3>			residueDict			=	new Dictionary<string, Vector3>();
			
			string currRes = "";
			string prevRes = "";
			float nbAtoms = 0f;
			Vector3 currAtomSum = Vector3.zero;
			Vector3 currAtom = Vector3.zero;
			
			int currResId = int.MinValue+1;
			int prevResId = int.MinValue+1;
			
			Vector3 testVector = Vector3.zero;


			
			int index=0;
			for(int i=0; i<alist.Count; i++) {

				currRes = resNames[i];
				currResId = residueIds[i];
				// New residue encountered
				if(!string.Equals(currRes, prevRes)) {
					resNamesPerResidue.Add(prevRes);
					currAtomSum *= (1f/nbAtoms);
					residues.Add(currAtomSum);
					currAtomSum = Vector3.zero;
					nbAtoms = 0;
				}
				prevRes = currRes;

				if(currResId != prevResId) {
					if(residueDict != null && residueDict.Count > 0){
						residueDictList.Add(residueDict);
					}
					residueDict = new Dictionary<string, Vector3>();
					index++;

				}
				prevResId = currResId;
				
				// We flip the x-coordinates because Unity is right-handed
				currAtom = new Vector3(-alist[i][0], alist[i][1], alist[i][2]);// - MoleculeModel.Offset; 
				currAtomSum += currAtom;
				//Debug.Log(aNamesList[i] + ": " + currAtom.ToString());
				//Debug.Log(currResId.ToString() + " :: " + prevResId.ToString());
				// Sometimes there are multiple conformations for a residue in a PDB
				// therefore multiple atoms of the same type. That generates collisions
				// in the dictionary. C# doesn't like that.
				if(!residueDict.TryGetValue(aNamesList[i], out testVector)){
					residueDict.Add(aNamesList[i], currAtom);
				}
				nbAtoms +=1f;


			}
			MoleculeModel.residueDictionaries = residueDictList;

			/*
			for(int i=0; i<residues.Count; i++) {
				Debug.Log("Residue " + resNamesPerResidue[i] + ": " + residues[i].ToString());
			}
			*/
		}

		public static void CreateResiduesSugar2(){
			List<float[]>						alist				=	MoleculeModel.atomsLocationlist;
			List<string>						aNamesList			=	MoleculeModel.atomsNamelist;
			List<int>							residueIds			=	MoleculeModel.residueIds;

			
			List<Dictionary<string, Vector3>>	residueDictList		=	new List<Dictionary<string, Vector3>>();
			Dictionary<string, Vector3>			residueDict			=	new Dictionary<string, Vector3>();

			Vector3 currAtom;
			Vector3 testVector;

			for (int i = 0 ; i<alist.Count; i++){

				if((i==alist.Count-1) || (residueIds[i] != residueIds[i+1])){
					if(residueDict != null && residueDict.Count > 0){
						residueDictList.Add(residueDict);
					}
					residueDict = new Dictionary<string, Vector3>();
				}

				currAtom = new Vector3(-alist[i][0], alist[i][1], alist[i][2]);

				if(!residueDict.TryGetValue(aNamesList[i], out testVector)){
					residueDict.Add(aNamesList[i], currAtom);
				}
			}

			MoleculeModel.residueDictionariesSugar = residueDictList;

		}
		
		/// <summary>
		/// Checks whether the loaded PDB file follows the HiRE-RNA coarse-grain model or not.
		/// </summary>
		public static void CheckHiRERNAModel()
		{
			Debug.Log("CheckHiRERNAModel");
			if (MoleculeModel.atomsNamelist.Exists(x => x == "C1" || x == "G1" || x == "G2" || x == "U1" || x == "A1" || x == "A2"))
			{
				UIData.ffType = UIData.FFType.HiRERNA;
				UIData.loadHireRNA = true;
			}
			Debug.Log (UIData.loadHireRNA);
		}

		static void BuildHireRnaHydrogenBondsStructures ()
		{
			foreach (KeyValuePair<int, ArrayList> entry in MoleculeModel.residues) 
			{
				int nbOfAtoms = entry.Value.Count;
				int i = 0;
				while (i != nbOfAtoms)
				{
					int atomIndex = (int)entry.Value[i];
					string atomName = MoleculeModel.atomsNamelist[atomIndex];
					if (atomName == "C1" || atomName == "G2" || atomName == "U1" || atomName == "A2")
					{
						MoleculeModel.baseIdx.Add(atomIndex);
						i = nbOfAtoms;
					}
					else
					{
						i++;
					}
				}
			}
			
			StreamReader sr = new StreamReader(GUIDisplay.directorypath + "/scale_RNA.dat");
			string s;
//			Debug.Log ("------------");
			float num;
			int t = 0;
			string substr;
			while((s=sr.ReadLine())!=null) {
				substr = s.Substring (7, 10);
//				Debug.Log(substr);
				num = float.Parse(substr);
//				Debug.Log ("Scale RNA " + t + " " + num);
				MoleculeModel.scale_RNA.Add(num);
				t++;
			}
			Debug.Log (MoleculeModel.scale_RNA.Count);
			RNAView.RNAView_init();
//			Debug.Log ("------------");
		}

		/// <summary>
		/// Builds the molecule's components.
		/// This is called after reading a PDB. It fills everything that always needs to be filled in MoleculeModel.
		/// </summary>
		public static void BuildMoleculeComponents() {
			List<float[]>	alist			=	MoleculeModel.atomsLocationlist;
			List<float[]>	alistSugar		=	MoleculeModel.atomsSugarLocationlist;
			List<float[]>	calist			=	MoleculeModel.CatomsLocationlist;
			
			Vector3 minPoint= new Vector3(float.MaxValue,float.MaxValue,float.MaxValue);
    		Vector3 maxPoint= new Vector3(float.MinValue,float.MinValue,float.MinValue);
			Vector3 bary = Vector3.zero;
			
			for(int i=0; i<alist.Count; i++) {
    			float[] position= alist[i];
    			minPoint = Vector3.Min(minPoint, new Vector3(position[0],position[1],position[2]));
	        	maxPoint = Vector3.Max(maxPoint, new Vector3(position[0],position[1],position[2]));
				bary = bary+(new Vector3(position[0],position[1],position[2]));
    		}
			Vector3 centerPoint = bary/alist.Count;
			MoleculeModel.target = Vector3.zero;
			Debug.Log("centerPoint:"+centerPoint + " min/max " + minPoint + "/" + maxPoint);
			
			MoleculeModel.Offset = -centerPoint;

			bary = Vector3.zero;
			Debug.Log("alist.Count:"+alist.Count);
			for(int i=0; i<alist.Count; i++) {
				float[] position= alist[i];
				float[] vect=new float[3];
				vect[0]=position[0]+MoleculeModel.Offset.x;
				vect[1]=position[1]+MoleculeModel.Offset.y;
				vect[2]=position[2]+MoleculeModel.Offset.z;
				alist[i]=vect;
				bary = bary+(new Vector3(vect[0],vect[1],vect[2]));
			}

			for(int i=0; i<alistSugar.Count; i++) {
				float[] position= alistSugar[i];
				float[] vect=new float[3];
				vect[0]=position[0]+MoleculeModel.Offset.x;
				vect[1]=position[1]+MoleculeModel.Offset.y;
				vect[2]=position[2]+MoleculeModel.Offset.z;
				alistSugar[i]=vect;

			}


			bary = bary/alist.Count;
			Debug.Log("Bary center :" + bary);
			
			MoleculeModel.MinValue = minPoint+MoleculeModel.Offset;
			MoleculeModel.MaxValue = maxPoint+MoleculeModel.Offset;
			MoleculeModel.Center = bary;	
			
			for(int i=0; i<calist.Count; i++) {
				float[] position= calist[i] as float[];
				float[] vect=new float[4];
				vect[0] = position[0]+MoleculeModel.Offset.x;
				vect[1] = position[1]+MoleculeModel.Offset.y;
				vect[2] = position[2]+MoleculeModel.Offset.z;
				vect[3] = 0;
				calist[i]=vect;
			}
			
			MoleculeModel.atomsLocationlist			=	alist;
			MoleculeModel.atomsSugarLocationlist	=	alistSugar;
			MoleculeModel.CatomsLocationlist		=	calist;
			
			MoleculeModel.cameraLocation.x=0;
			MoleculeModel.cameraLocation.y=0;
			MoleculeModel.cameraLocation.z=MoleculeModel.target.z-(Vector3.Distance(maxPoint,minPoint));
		} // End of BuildMoleculeComponents

		/// <summary>
		/// Creates the carbon alpha splines.
		/// </summary>
		/// <param name='alist'>
		/// List of atoms.
		/// </param>
		/// <param name='calist'>
		/// List of Carbon Alpha atoms.
		/// </param>
		/// <param name='caChainlist'>
		/// List of carbon alpha chain.
		/// </param>
		/// <param name='typelist'>
		/// The type of each atom.
		/// </param>
		public static void CreateSplines() {
			List<float[]>	alist			=	MoleculeModel.atomsLocationlist;
			List<float[]>	calist			=	MoleculeModel.CatomsLocationlist;
			List<string>	caChainlist		=	MoleculeModel.CaSplineChainList;
			List<string>	atomsNameList	=	MoleculeModel.atomsNamelist;
			List<AtomModel>	typelist		=	MoleculeModel.atomsTypelist;
			
			// Trace interpolation from C-alpha positions
			// Only if there are more than 2 C-alpha
			if(calist.Count > 2) {
				
				MoleculeModel.backupCaSplineChainList = caChainlist;
				MoleculeModel.backupCatomsLocationlist = calist;
				calist          = 	new List<float[]>(MoleculeModel.backupCatomsLocationlist);
				caChainlist     = 	new List<string>(MoleculeModel.backupCaSplineChainList);
				/*
				int j = 0;
				for(int i=1;i<residlist.Count;i++){
					
					if(atomsNameList[i] == "CA"){
						if((atomsNameList[i-1] == atomsNameList[i]) && (residlist[i-1] == residlist[i])){
							calist.RemoveAt(j);
							caChainlist.RemoveAt(j);
							Debug.Log ("Remove");
						}
						j++;
					}
				} */

				GenInterpolationArray geninterpolationarray = new GenInterpolationArray();
				geninterpolationarray.InputKeyNodes=calist;
				geninterpolationarray.InputTypeArray=caChainlist;
				geninterpolationarray.CalculateSplineArray();
				calist=null;
				caChainlist=null;
				calist=geninterpolationarray.OutputKeyNodes;
				caChainlist=geninterpolationarray.OutputTypeArray;
			}
			MoleculeModel.CaSplineList=calist;
			MoleculeModel.CaSplineTypeList = new List<AtomModel>();
			
			for(int k=0; k<calist.Count; k++){
				MoleculeModel.CaSplineTypeList.Add(AtomModel.GetModel("chain"+caChainlist[k]));}
			MoleculeModel.CaSplineChainList=caChainlist;	
			
			if(UIData.ffType == UIData.FFType.HiRERNA)
			{
				MoleculeModel.sequence = ControlMolecule.CreateSequenceString();
				MoleculeModel.bondEPList=ControlMolecule.CreateBondsList_HiRERNA(atomsNameList);
			}
			else {
				//MoleculeModel.bondList=ControlMolecule.CreateBondsList(alist,typelist);
				MoleculeModel.bondEPList=ControlMolecule.CreateBondsEPList(alist,typelist);
				MoleculeModel.bondEPSugarList = ControlMolecule.CreateBondsEPList(MoleculeModel.atomsSugarLocationlist,MoleculeModel.atomsSugarTypelist);
				MoleculeModel.bondCAList=ControlMolecule.CreateBondsCAList(caChainlist);	
			}
			MoleculeModel.atomsnumber = alist.Count;
			MoleculeModel.bondsnumber = MoleculeModel.bondEPList.Count;
		} // End of CreateSplines

		public static List<int[]> CreateBondsList(List<float[]> atomsLocationlist, List<AtomModel> atomsTypelist) {
			List<int[]> bond=new List<int[]>();
			int test=0;
			for(int i=0;i<atomsLocationlist.Count;i++) {
				float [] atom0 = atomsLocationlist[i];
				string atomtype0 = (atomsTypelist[i]).type;
				float x0 = atom0[0];
				float y0 = atom0[1];
				float z0 = atom0[2];
				
				for(int j=1;j<80;j++) {
					if(i+j < atomsLocationlist.Count) {
						float[] atom1 = atomsLocationlist[i+j];
						string atomtype1 = (atomsTypelist[i+j]).type;
						
				        float cutoff = 1.6f;		
						if ((atomtype0=="H") && (atomtype1=="H")) continue;
						if ((atomtype0=="S") || (atomtype1=="S")) cutoff = 1.84f;
						if ((atomtype0=="O" && atomtype1=="P") || (atomtype1=="O" && atomtype0=="P")) cutoff = 1.84f;
						if ((atomtype0=="O" && atomtype1=="H") || (atomtype1=="O" && atomtype0=="H")) cutoff = 1.84f;
						
						float x1=atom1[0];
						float y1=atom1[1];
						float z1=atom1[2];
						
						float sqDist = (x0-x1)*(x0-x1) + (y0-y1)*(y0-y1) + (z0-z1)*(z0-z1);
						// faster than sqrt
						if(sqDist <= cutoff*cutoff) {
							int [] atomsIds = {i,i+j};
							bond.Add(atomsIds);
							test=test+1;
						}
					}
				}
			}
			return bond;
		}
		
		
		public static List<int[]> CreateBondsEPList(List<float[]> atomsLocationlist, List<AtomModel> atomsTypelist) {
			List<int[]> bond=new List<int[]>();
			List<int> h_already_in=new List<int>(); // T.T. to not link a hydrogen 2 times

			if (UIData.connectivity_PDB && !UIData.connectivity_calc){
				return MoleculeModel.BondListFromPDB;
			}

			for(int i=0;i<atomsLocationlist.Count;i++) {
				float[] atom0 = atomsLocationlist[i];
				string atomtype0 = (atomsTypelist[i]).type;

				float x0=atom0[0];
				float y0=atom0[1];
				float z0=atom0[2];
				
				/*
				 * UPDATE BY T.T. but it will lost in eficiency...
				 * for sugar, it can have 600 distance atoms between 2 bounded atom, 
				 * so we can't stop to search a neighbor after 150 atoms (for example).
				 * So I change the way to calcul bounded atoms, but for verry big complexe, it can
				 * be longer to calcul bounded atoms.
				 * there is the old code :
				 * for (int j=1;j<150;j++){
				 *    if(i+j<atomsLocationlist.Count) {
				 *    .....
				 *    .....
				 *    then replace all indexes [j] by [i+j]
				 * 
				 */
				//for(int j=1;j<700;j++) {
				//	if(i+j<atomsLocationlist.Count) {
				for(int j=i+1; j<atomsLocationlist.Count; j++){
					float[] atom1 = atomsLocationlist[j];
					string atomtype1 = (atomsTypelist[j]).type;
					string a1name = MoleculeModel.atomsNamelist[j];
					
					float cutoff = 1.7f;
					if ((atomtype0=="H") && (atomtype1=="H")) continue;
                    else if ((a1name == "CAL") && (atomtype0=="O")) cutoff = 3.5f;
					else if ((atomtype0=="S") || (atomtype1=="S")) cutoff = 1.91f;
					else if ((atomtype0=="P") || (atomtype1=="P")) cutoff = 1.7f;
                    else if ((atomtype0=="O" && atomtype1=="H") || (atomtype1=="O" && atomtype0=="H")) cutoff = 1.84f;
					
					float x1=atom1[0];
					float y1=atom1[1];
					float z1=atom1[2];

					float sqDist = (x0-x1)*(x0-x1) + (y0-y1)*(y0-y1) + (z0-z1)*(z0-z1);
					// faster than sqrt
					
					/*** T.T.
					 * I change it to evoid to add a H elready bounded in the bond list.
					 * old code : 
					 * if(sqDist <= cutoff*cutoff) {
					 	    bond.Add(atomsIds);
						    h_already_in.Add(j);
						}
					 * */
					if(sqDist <= cutoff*cutoff) {

						if (atomtype0=="H"){
					    	if (!h_already_in.Contains(i)){
								int [] atomsIds = {i,j};
								bond.Add(atomsIds);
								h_already_in.Add(i);

								if (MoleculeModel.bondEPDict.ContainsKey(i))
									MoleculeModel.bondEPDict[i].Add (j);
								else
									MoleculeModel.bondEPDict[i]=new List<int>(j);
							}
						}else if (atomtype1=="H"){
							if (!h_already_in.Contains(j)){
								int [] atomsIds = {i,j};
								bond.Add(atomsIds);
								h_already_in.Add(j);

								if (MoleculeModel.bondEPDict.ContainsKey(i))
									MoleculeModel.bondEPDict[i].Add (j);
								else
									MoleculeModel.bondEPDict[i]=new List<int>(j);
							}
						}
						else{
							int [] atomsIds = {i,j};
							bond.Add(atomsIds);

							if (MoleculeModel.bondEPDict.ContainsKey(i))
								MoleculeModel.bondEPDict[i].Add (j);
							else
								MoleculeModel.bondEPDict[i]=new List<int>(j);
						}
								
					}
				//}
				}
			}
			//In the case where we want to calculate bonds, and then add other bond inside the PDB.
			if (UIData.connectivity_PDB && UIData.connectivity_calc){
				for (int i=0; i<MoleculeModel.BondListFromPDB.Count; i++){
					int atom1 = MoleculeModel.BondListFromPDB[i][0];
					int atom2 = MoleculeModel.BondListFromPDB[i][1];

					if (MoleculeModel.bondEPDict.ContainsKey(atom1)){
						if (!MoleculeModel.bondEPDict[atom1].Contains(atom2)) // If the bond is not already added in the bond list
							bond.Add (new int[2] {atom1, atom2});
					}
				}
			}

			return bond;
		}




		/// <summary>
		/// Creates a bond list for the carbon alpha splines.
		/// </summary>
		/// <param name='CaChainlist'>
		/// List of string. The chain of each carbon alpha.
		/// </param>
		/// <param name='bond'>
		/// List of bonds to create.
		/// </param>
		public static List<int[]> CreateBondsCAList(List<string> caChainlist) {
			List<int[]> bond=new List<int[]>();
			
			for(int i=1; i<caChainlist.Count; i++) {
				if(caChainlist[i-1] == caChainlist[i]) {
					int[] splineIds	= {i-1,i};
					bond.Add(splineIds);
				}
			}
			Debug.Log("bond.Count:"+bond.Count);
			return bond;
		}
		
		public static string CreateSequenceString() {
			int nucleotide_count = MoleculeModel.residues.Count;
			StringBuilder sequence = new StringBuilder(new string('.', nucleotide_count), nucleotide_count);
			int i = 0;
			foreach(KeyValuePair<int, ArrayList> entry in MoleculeModel.residues)
			{
				int atomId = (int)entry.Value[0];
				sequence[i] = MoleculeModel.atomsResnamelist[atomId][0];
				i++;
			}
			return sequence.ToString();
		}

		public static List<int[]> CreateBondsList_HiRERNA(List<string> atomnames) {
			//We suppose the names are ordered as this:
			//P O5* C5* CA CY b1 [b2]
			List<int[]> bonds = new List<int[]>();
			int N = atomnames.Count;
			int k;
//			int[] bond;
			for(int i=0; i<N-1; ++i) {
				string a1 = atomnames[i];
				if(a1 == "P") {
					//Backward search for "CA"
					for(k=i; k>=0 && k>=i-5 && (atomnames[k])!="CA"; --k);
					if(k>=0 && k>=i-5) bonds.Add(new int[] {i,k});

					//Forward search for "O5*"
					if(atomnames[i+1] == "O5*")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "O5* missing");
				}
				else if(a1 == "O5*") {
					//Forward search for "C5*"
					if(atomnames[i+1] == "C5*")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "C5* missing");
				}
				else if(a1 == "C5*") {
					//Forward search for "CA"
					if(atomnames[i+1] == "CA")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "CA missing");
				}
				else if(a1 == "CA") {
					//Forward search for "CY"
					if(atomnames[i+1] == "CY")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "CY missing");
				}
				else if(a1 == "CY") {
					//Forward search for G1, A1, U1 or C1
					string a2 = atomnames[i+1];
					if(a2 == "G1" || a2 == "A1" || a2 == "U1" || a2 == "C1")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "b1 missing");
				}
				else if(a1 == "G1") {
					//Forward search for "G2"
					if(atomnames[i+1] == "G2")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "G2 missing");
				}
				else if(a1 == "A1") {
					//Forward search for "A2"
					if(atomnames[i+1] == "A2")
						bonds.Add(new int[] {i,i+1});
					else
						Debug.Log("Atom "+ (i+1) as string + "A2 missing");
				}
			}
			Debug.Log("HiRERNA bond count : " + bonds.Count);
			return bonds;
		}

		
		// not the atomsLocationList from MoleculeModel
		public static List<int[]> CreateBondsCSList(List<int[]> atomsLocationlist) {
			//int k=0;
			//string clubs="";
			List<int[]> bond=new List<int[]>();
//			Debug.Log("atomsLocationlist.Count "  + atomsLocationlist.Count);
//			Debug.Log("atomsTypelist.Count "  + atomsTypelist.Count);

			Debug.Log("atomsLocationlist.Count:"+atomsLocationlist.Count);

//			int[] ary = (int[])((MoleculeModel.CSidList).ToArray(typeof(int)));
			for(int i=0;i<atomsLocationlist.Count;i++) {
//				Debug.Log("atomsLocationlist[i][0]="+(atomsLocationlist[i] as float[])[0]);
				int[] atom0 = atomsLocationlist[i];
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
				for(int j=0;j<MoleculeModel.CSidList.Count;j++) {
					
//					Debug.Log("source="+source+",target="+target);
					int [] number=MoleculeModel.CSidList[j];
//					Debug.Log("number[0]="+number[0]);
//					int number=ary[j];
//					int number=int.Parse(MoleculeModel.CSidList[j] as string);

					if(source==number[0])
						atom0sign=j;
					if(target==number[0])
						atom1sign=j;
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
