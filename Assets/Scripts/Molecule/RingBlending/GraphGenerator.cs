using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Molecule.Model;

public class GraphGenerator : MonoBehaviour {
	public static List<GraphVertex> GenerateGraph(bool keepHydrogens = false) {
		List<GraphVertex> molAsGraph = new List<GraphVertex>();
		
		//We initialize all vertex (an atom is a vertex here)
		for (int i=0; i<MoleculeModel.atomsLocationlist.Count; i++){
			GraphVertex tempVertex = new GraphVertex();
			tempVertex.coordinate=(new Vector3(MoleculeModel.atomsLocationlist[i][0],
			                                   MoleculeModel.atomsLocationlist[i][1],
			                                   MoleculeModel.atomsLocationlist[i][2]));
			tempVertex.resname=MoleculeModel.atomsResnamelist[i];
			tempVertex.type=MoleculeModel.atomsNamelist[i][0];
			tempVertex.id=i;
			molAsGraph.Add(tempVertex);
		}
		
		/* We connect all dots
		 * But we don't need to calculate over 30 atoms after the atom 'i'
		 * and an hydrogen can't be a part of a cycle because an hydrogen can only have 1 bond.
		 */
		for (int i=0; i < MoleculeModel.bondEPList.Count; i++){
			//Debug.Log("0= "+MoleculeModel.bondList[i][0]+" 1="+MoleculeModel.bondList[i][1]);
			if(keepHydrogens || molAsGraph[MoleculeModel.bondEPList[i][1]].type!='H'){
				if (MoleculeModel.bondEPList[i][1] < i+30){
					molAsGraph[MoleculeModel.bondEPList[i][0]].neighbor.Add(molAsGraph[MoleculeModel.bondEPList[i][1]]);
					molAsGraph[MoleculeModel.bondEPList[i][1]].neighbor.Add(molAsGraph[MoleculeModel.bondEPList[i][0]]);
				}
			}
		}
		
		return molAsGraph;
	}
}
