using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Molecule.Model;
using UI;
using Molecule.Control;

public class AlphaChainSmoother {
	public static List<float[]> backupCaList = MoleculeModel.CatomsLocationlist ;
	public static List<string> backupCaChainList = MoleculeModel.backupCaSplineChainList ;
//	private ArrayList caChainlist;
	

	void Start () {
//		backupCAlist = (ArrayList)MoleculeModel.bondCAList.Clone();
	//	calist = backupCAlist.Clone();
	//	caChainlist = MoleculeModel.bondCAList.Clone();
	}
		
	/// <summary>
	/// Trace interpolation points from C-alpha positions.
	/// Recreate interpolation points for carbon alpha splines.
	/// </summary>
	public static void ReSpline () {
		List<float[]>   alist           = MoleculeModel.atomsLocationlist ;
		List<AtomModel> typelist        = MoleculeModel.atomsTypelist ;
		List<string>    atomsNameList   = MoleculeModel.atomsNamelist ;
		List<float[]>   calist          = new List<float[]>(MoleculeModel.CatomsLocationlist);
		List<string>    caChainlist     = new List<string>(MoleculeModel.backupCaSplineChainList);
		List<int>       residlist       =   MoleculeModel.residueIds;
		//List<string>	resnamelist	    =	MoleculeModel.atomsResnamelist;
			
		// Trace interpolation from C-alpha positions
		// Only if there are more than 2 C-alpha
		if(calist.Count > 2) {

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
			}

			GenInterpolationArray geninterpolationarray = new GenInterpolationArray();
			geninterpolationarray.InputKeyNodes = calist ;
			geninterpolationarray.InputTypeArray = caChainlist ;
			geninterpolationarray.CalculateSplineArray();
			calist=null;
			caChainlist=null;
			calist=geninterpolationarray.OutputKeyNodes;
			caChainlist=geninterpolationarray.OutputTypeArray;
		}
		MoleculeModel.CaSplineList=calist;
		MoleculeModel.CaSplineTypeList = new List<AtomModel>();
		for (int k=0; k<calist.Count; k++)
			MoleculeModel.CaSplineTypeList.Add(AtomModel.GetModel("chain" + caChainlist[k]));
		MoleculeModel.CaSplineChainList=caChainlist;
		
		if(UIData.ffType == UIData.FFType.HiRERNA)
			MoleculeModel.bondEPList=ControlMolecule.CreateBondsList_HiRERNA(atomsNameList);
		else {
			//MoleculeModel.bondList=ControlMolecule.CreateBondsEPList(alist,typelist);
			MoleculeModel.bondEPList=ControlMolecule.CreateBondsEPList(alist,typelist);
			MoleculeModel.bondCAList=ControlMolecule.CreateBondsCAList(caChainlist);	
		}
		
		MoleculeModel.atomsnumber = alist.Count;
		MoleculeModel.bondsnumber = MoleculeModel.bondEPList.Count;
		MoleculeModel.CaSplineChainList = caChainlist ;
	}
}
