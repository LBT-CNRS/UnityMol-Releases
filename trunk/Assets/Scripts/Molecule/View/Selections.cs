using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Molecule.Model;
using UI;



public class Selections : MonoBehaviour {
	
	private List<BallUpdate> selections = new List<BallUpdate>();
	
	public void selectResidue(int resid)
	{
		if(MoleculeModel.residues.ContainsKey(resid) == false)
			return;
		
		clearSelection();
		
		ArrayList atomIds = MoleculeModel.residues[resid];	
		foreach(int id in atomIds)
		{
			GameObject atom = MoleculeModel.atoms[id] as GameObject;
			BallUpdate comp = atom.GetComponent<BallUpdate>();
			selections.Add(comp);
//			comp.independant = true;
			comp.AtomColor = Color.magenta;
			comp.SetRayonFactor(3.0f);
		}
	}
	
	public void clearSelection()
	{
		foreach(BallUpdate comp in selections)
		{
//			comp.independant = false;
			comp.SetRayonFactor(UI.GUIMoleculeController.globalRadius);
			comp.AtomColor = MoleculeModel.GetAtomColor(MoleculeModel.atomsTypelist[(int)comp.number].type);
		}
		selections.Clear();
	}
	
}

