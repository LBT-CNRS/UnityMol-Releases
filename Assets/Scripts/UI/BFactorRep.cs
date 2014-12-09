using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Molecule.Model;
using UI;
using Molecule.Control;

public class BFactorRep {
	public static List<float[]> backupCaList = MoleculeModel.backupCatomsLocationlist ;
	public static List<string> backupCaChainList = MoleculeModel.backupCaSplineChainList ;
	public static List<float> backupBfactCAList;
	public static float minValue;
	public static float maxValue;
	public static string minval = "";
	public static string maxval = "";

	public static void CreateBFRep() {
		List<float[]>	alist			=	MoleculeModel.atomsLocationlist;
		List<float[]>	calist			=	new List<float[]>(MoleculeModel.backupCatomsLocationlist);
		List<string>	caChainlist		=	new List<string>(MoleculeModel.backupCaSplineChainList);
		List<string>	atomsNameList	=	MoleculeModel.atomsNamelist;
		List<AtomModel>	typelist		=	MoleculeModel.atomsTypelist;
			//C.R
		List<float>	    Bfactorlist		=	MoleculeModel.BFactorList;
		List<int>       residlist       =   MoleculeModel.residueIds;
		List<float>     BfactCAlist     =   new List<float> ();
		//List<string>	resnamelist	    =	MoleculeModel.atomsResnamelist;

			
		// Trace interpolation from C-alpha positions
		// Only if there are more than 2 C-alpha

		if(calist.Count > 2) {

			// Compute bfactor mean by residue
			int comp = 0; //counter number of residues
			float b = 0;
			float bfac = residlist[0];
			int j = 0; //counter CA

			for(int i=1;i<residlist.Count;i++){

				if(i+1 == residlist.Count){
					bfac = b/comp;
					BfactCAlist.Add (bfac);
				}

				if((atomsNameList[i-1] == atomsNameList[i]) && (residlist[i-1] == residlist[i])){
					if (atomsNameList[i] == "CA"){
						calist.RemoveAt(j);
						caChainlist.RemoveAt(j);
						//Debug.Log ("Remove");
					}
				}

				if(residlist[i-1] == residlist[i]){
					b += Bfactorlist[i];
					comp++;
				}

				else{
					bfac = b/comp;
					BfactCAlist.Add (bfac);
					j++;
					b = Bfactorlist[i];
					comp = 1;
				}
			}

			// Compute bfactor min
			if(UIData.isRescale)
				minValue = float.Parse (minval);
			else{
				minValue = GetMin(BfactCAlist);
			}

			for(int i=0; i<BfactCAlist.Count;i++)
				BfactCAlist[i] = BfactCAlist[i] - minValue;

			// End of bfactor min

			// Compute Bfactor max
			if(UIData.isRescale)
				maxValue = (float.Parse (maxval) - float.Parse (minval));
			else{
				maxValue = GetMax(BfactCAlist);
			}

			//bfactor value between 0 and 1
			for(int i=0; i<BfactCAlist.Count;i++){
				BfactCAlist[i] = BfactCAlist[i]/maxValue;
			}

			GenInterpolationArray_BF geninterpolationarray = new GenInterpolationArray_BF();
			geninterpolationarray.InputKeyNodes=calist;
			geninterpolationarray.InputTypeArray=caChainlist;
			geninterpolationarray.InputBfactArray=BfactCAlist;

			geninterpolationarray.CalculateSplineArray();

			calist=null;
			caChainlist=null;
			Bfactorlist=null;

			calist=geninterpolationarray.OutputKeyNodes;
			caChainlist=geninterpolationarray.OutputTypeArray;
			Bfactorlist=geninterpolationarray.OutputBfactArray;
		}

		MoleculeModel.CaSplineList=calist;
		MoleculeModel.CaSplineTypeList = new List<AtomModel>();

		string typebf;

		for (int k=0; k<calist.Count; k++) {
			typebf = GetBFStyle(Bfactorlist[k]);
			MoleculeModel.CaSplineTypeList.Add (AtomModel.GetModel (typebf));
			}

		MoleculeModel.CaSplineChainList=caChainlist;
			
		if(UIData.ffType == UIData.FFType.HiRERNA)
			MoleculeModel.bondEPList=ControlMolecule.CreateBondsList_HiRERNA(atomsNameList);
		else {
			//MoleculeModel.bondList=ControlMolecule.CreateBondsList(alist,typelist);
			MoleculeModel.bondEPList=ControlMolecule.CreateBondsEPList(alist,typelist);
			MoleculeModel.bondCAList=ControlMolecule.CreateBondsCAList(caChainlist);	
		}
		MoleculeModel.atomsnumber = alist.Count;
		MoleculeModel.bondsnumber = MoleculeModel.bondEPList.Count;
		MoleculeModel.CaSplineChainList=caChainlist;
		minval = minValue.ToString ();
		maxval = (maxValue+minValue).ToString ();
	}  

	public static string GetBFStyle(float BFValue){
		string typebf;
		if (BFValue < 0.1)
			typebf = "BF1";
		else if (BFValue <0.2)
			typebf = "BF2";
		else if (BFValue <0.3)
			typebf = "BF3";
		else if (BFValue <0.4)
			typebf = "BF4";
		else if (BFValue <0.5)
			typebf = "BF5";
		else if (BFValue <0.6)
			typebf = "BF6";
		else if (BFValue <0.7)
			typebf = "BF7";
		else if (BFValue <0.8)
			typebf = "BF8";
		else if (BFValue <0.9)
			typebf = "BF9";
		else
			typebf = "BF10";
		return typebf;
	}

	/// <summary>
	/// Find the min value of a list of float.
	/// </summary>
	/// <returns>The minimum.</returns>
	/// <param name="ListValues">List values.</param>
	public static float GetMin (List<float> ListValues){
		float mymin = ListValues[0];
		foreach (float g in ListValues)
			if (g<mymin)
				mymin =g;
		return mymin;
	}

	/// <summary>
	/// Find the max value of a list of float.
	/// </summary>
	/// <returns>The max.</returns>
	/// <param name="ListValues">List values.</param>
	public static float GetMax (List<float> ListValues){
		float mymax = ListValues [0];
		foreach (float f in ListValues)
			if (f>mymax)
				mymax =f;
		return mymax;
	}

}
