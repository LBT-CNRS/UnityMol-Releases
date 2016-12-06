using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI;
using Molecule.Model;

public class CubeManager : GenericManager {
	private BallUpdateCube[] cubes;
	private static bool mouseOvers = false;

	public override void Init () {
		cubes = GameObject.FindObjectsOfType(typeof(BallUpdateCube)) as BallUpdateCube[];
		BallUpdate.resetColors = true;
		BallUpdate.resetRadii = true;
		enabled = true;
		Debug.Log("Cube Manager INIT");
	}
	
	public override void DestroyAll() {
		Debug.Log("Destroying Cubes");
//		Debug.Log(cubes.Length.ToString());

		cubes = GameObject.FindObjectsOfType(typeof(BallUpdateCube)) as BallUpdateCube[];
		foreach(BallUpdateCube cb in cubes) {
			//cb.renderer.enabled = false;
			//DestroyImmediate(cb);
			GameObject.Destroy(cb);
		}
	}
	
	public override void ToggleDistanceCueing(bool enabling) {
		
	}
	
	/// <summary>
	/// Resets the colors of all cubes and sticks. Uses the color sets in BallUpdateCube.
	/// </summary>
	private void ResetColors() {
		if(UIData.atomtype == UIData.AtomType.cube){
//		cubes = GameObject.FindObjectsOfType(typeof(BallUpdateCube)) as BallUpdateCube[];
		foreach(BallUpdateCube bc in cubes)
			bc.GetComponent<Renderer>().material.SetColor("_Color", Molecule.Model.MoleculeModel.atomsColorList[(int)bc.number]);
		
		BallUpdate.resetColors = false;
		BallUpdate.bondsReadyToBeReset = true;
		//Debug.Log("RESET CUBE COLOR");
		}
	}
	
	
	/// <summary>
	/// Sets the color of atoms.
	/// </summary>
	/// <param name='col'>
	/// Color.
	/// </param>
	/// <param name='atom'>
	/// Atom type.
	/// </param>
	/// <param name='residue'>
	/// Residue.
	/// </param>
	/// <param name='chain'>
	/// Chain.
	/// </param>
	public override void SetColor(Color col, List<string> atom, string residue = "All", string chain = "All") {
//		Debug.Log("SetColor cball");
		if(!atom.Contains ("All")){
			if(residue == "All"){
				if(chain == "All"){
		// ---------- ATOM
//					Debug.Log("ATOM");
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateCube cb in cubes){
							if(atom.Contains(cb.tag))
								Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;
						}
					}else{
						foreach(BallUpdateCube cb in cubes){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)cb.number]))
								Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;
						}
					}
				}
				else{
		// ---------- ATOM + CHAIN
//					Debug.Log("ATOM+CHAIN");
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateCube cb in cubes){
							if(atom.Contains(cb.tag)
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;	
						}
					}else{
						foreach(BallUpdateCube cb in cubes){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)cb.number])
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;	
							}
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- ATOM + RESIDUE
//					Debug.Log("ATOM+RESIDUE");
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateCube cb in cubes){
							if(atom.Contains(cb.tag)
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue)
									Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;
						}
					}else{
						foreach(BallUpdateCube cb in cubes){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)cb.number])
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue)
									Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;
						}
					}
				}
				else{
		// ---------- ATOM + RESIDUE + CHAIN
//					Debug.Log("ATOM+RESIDUE+CHAIN");
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateCube cb in cubes){
							if(atom.Contains(cb.tag)
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;	
						}
					}else{
						foreach(BallUpdateCube cb in cubes){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)cb.number])
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;	
							}
					}
				}
			}
		}
		else{
			if(residue == "All"){
				if(chain == "All"){
		// ---------- EVERYTHING
//					Debug.Log("EVERYTHING");
					foreach(BallUpdateCube cb in cubes)
						Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;
				}
				else{
		// ---------- CHAIN
//					Debug.Log("CHAIN");
					foreach(BallUpdateCube cb in cubes){
						if(Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
							Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;	
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- RESIDUE
//					Debug.Log("RESIDUE");
					foreach(BallUpdateCube cb in cubes){
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue){
							Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;		
						}
					}
				}
				else{
		// ---------- CHAIN + RESIDUE
//					Debug.Log("RESIDUE+CHAIN");
					foreach(BallUpdateCube cb in cubes){
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue
							&& Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
								Molecule.Model.MoleculeModel.atomsColorList[(int)cb.number] = col;
					}
				}
			}
		}
		
		BallUpdate.resetColors = true;
//		BallUpdate.bondsReadyToBeReset = true;
	}
	
	
	
	/// <summary>
	/// Sets the color of a specific atom.
	/// </summary>
	/// <param name='atomNum'>
	/// Atom number.
	/// </param>
	/// <param name='col'>
	/// Color.
	/// </param>
	public override void SetColor(Color col, int atomNum) {
		foreach(BallUpdateCube bc in cubes){
			if(bc.GetComponent<BallUpdate>().number == atomNum)
				Molecule.Model.MoleculeModel.atomsColorList[(int)bc.number] = col;
		}
		BallUpdate.resetColors = true;
	}
	
	/// <summary>
	/// Changes the scale of the atoms.
	/// </summary>
	/// <param name='atom'>
	/// Atom type.
	/// </param>
	/// <param name='residue'>
	/// Residue.
	/// </param>
	/// <param name='chain'>
	/// Chain.
	/// </param>
	public override void SetRadii(List<string> atom, string residue = "All", string chain = "All") {
		if(!atom.Contains ("All")){
			if(residue == "All"){
				if(chain == "All"){
		// ---------- ATOM
					if(GUIDisplay.quickSelection){
					foreach(BallUpdateCube cb in cubes) {
							if(atom.Contains(cb.tag))
								Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);
						}
					}else{
						foreach(BallUpdateCube cb in cubes) {
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)cb.number]))
								Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);
							}
					}
				}
				else{
		// ---------- ATOM + CHAIN
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateCube cb in cubes) {
							if(atom.Contains(cb.tag)
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);		
						}
					}else{
						foreach(BallUpdateCube cb in cubes) {
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)cb.number])
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);		
						}
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- ATOM + RESIDUE
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateCube cb in cubes) {
							if(atom.Contains(cb.tag)
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);
						}
					}else{
						foreach(BallUpdateCube cb in cubes) {
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)cb.number])
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);
						}
					}
				}
				else{
		// ---------- ATOM + CHAIN + RESIDUE
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateCube cb in cubes) {
							if(atom.Contains(cb.tag) 
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);		
						}
					}else{
						foreach(BallUpdateCube cb in cubes) {
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)cb.number])
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);		
						}
					}
				}
			}
		}
		else{
			if(residue == "All"){
				if(chain == "All"){
		// ---------- EVERYTHING
					foreach(BallUpdateCube cb in cubes) {
						Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);
					}
				}
				else{
		// ---------- CHAIN
					foreach(BallUpdateCube cb in cubes) {
						if(Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
							Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);		
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- RESIDUE
					foreach(BallUpdateCube cb in cubes) {
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue)
							Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);
					}
				}
				else{
		// ---------- CHAIN + RESIDUE
					foreach(BallUpdateCube cb in cubes) {
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)cb.number] == residue
							&& Molecule.Model.MoleculeModel.atomsChainList[(int)cb.number] == chain)
								Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);	
					}
				}
			}
		}
		BallUpdate.resetRadii = true;
		BallUpdate.bondsReadyToBeReset = true;
	}

	/// <summary>
	/// Changes the scale for Atom selection.
	/// </summary>
	/// <param name='id'>
	/// Identifier of the selected atom.
	/// </param>
	public override void SetRadii(int id) {
		foreach(BallUpdateCube cb in cubes) {
			if((int)cb.number == id)
				Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number] = (GUIDisplay.newScale);
		}
		BallUpdate.resetRadii = true;
		BallUpdate.bondsReadyToBeReset = true;
	}
	
	/// <summary>
	/// Gets the color of the atom at the location "pos".
	/// </summary>
	/// <returns>
	/// The color.
	/// </returns>
	/// <param name='pos'>
	/// Position.
	/// </param>
	public Color GetColor(Vector3 pos){
		foreach(BallUpdateCube cb in cubes)
			if(cb.transform.position == pos)
				return cb.GetComponent<Renderer>().material.GetColor("_Color");
		return Color.white;
	}
	
	/// <summary>
	/// Gets the GameObject from cubes at the position "id".
	/// </summary>
	/// <returns>
	/// The ball.
	/// </returns>
	/// <param name='id'>
	/// Identifier .
	/// </param>
	public override GameObject GetBall(int id){
		return cubes[id].gameObject;
	}

	public override void DisableRenderers() {
		cubes = GameObject.FindObjectsOfType(typeof(BallUpdateCube)) as BallUpdateCube[];
		foreach(BallUpdateCube bc in cubes){
			bc.GetComponent<Renderer>().enabled = false;
			if(UIData.atomtype != UIData.AtomType.particleball) // Particles don't have their own collider so we must keep it
				bc.GetComponent<Collider>().enabled = false; // Disable the collider at the same time to avoid ghost-clicking with atom selection
		}
		enabled = false;
	}
	
	public override void EnableRenderers() {
		cubes = GameObject.FindObjectsOfType(typeof(BallUpdateCube)) as BallUpdateCube[];
		foreach(BallUpdateCube bc in cubes){
			bc.GetComponent<Renderer>().enabled = true;
			bc.GetComponent<Collider>().enabled = true;
		}
		enabled = true;
	}
	
	private void ResetRadii() {
		if(UIData.atomtype == UIData.AtomType.cube){
			foreach(BallUpdateCube cb in cubes) {
				float buffer = Molecule.Model.MoleculeModel.atomsTypelist[(int)cb.number].radius
								* (GUIMoleculeController.globalRadius)
								* (Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)cb.number]/100);
				cb.transform.localScale = new Vector3(buffer, buffer, buffer);
			}
			BallUpdate.oldRadiusFactor = BallUpdate.radiusFactor;
			BallUpdate.resetRadii = false;
		}
	}
	
	/// <summary>
	/// Creates the mouse overs, needed when mddriver simulation and mouse interaction are required
	/// </summary>
	private void CreateMouseOversMDDriver() {
		for (int i=0; i<cubes.Length; i++) {
			cubes[i].gameObject.AddComponent<MouseOverMDDriverMolecule>();
		}
		mouseOvers = true;
	}
	
	/// <summary>
	/// Destroys the mouse overs
	/// </summary>
	private void DestroyMouseOversMDDriver() {
		cubes = GameObject.FindObjectsOfType(typeof(BallUpdateCube)) as BallUpdateCube[];
		for (int i=0; i<cubes.Length; i++) {
			Destroy(cubes[i].GetComponent<MouseOverMDDriverMolecule>());
		}
		mouseOvers = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(UIData.atomtype != UIData.AtomType.cube) {
			if(GameObject.FindObjectsOfType(typeof(BallUpdateCube)).Length >0) // Sometimes they're already destroyed at this point. Maybe. Can't hurt to make sure.
				DisableRenderers();
			return;
		}
	
		if(BallUpdate.resetColors){
			//Debug.Log("CUBE RESETCOLOR CALL" + UIData.atomtype);
			ResetColors();
		}
		
		if(BallUpdate.resetRadii || (BallUpdate.oldRadiusFactor != BallUpdate.radiusFactor))
			ResetRadii();
		
		if(GUIMoleculeController.toggle_MDDRIVER) {
			if(!mouseOvers)
			{
				CreateMouseOversMDDriver();
			}
		}
		else
		{
			if(mouseOvers)
			{
				if (GUIMoleculeController.toggle_MDDRIVER)
				{
					DestroyMouseOversMDDriver();
				}
			}
		}
	}
	
	public override void ResetPositions(){
		for (int j=0; j<cubes.Length; j++){
			int i = (int)cubes[j].number;
			cubes[j].transform.localPosition = new Vector3(MoleculeModel.atomsLocationlist[i][0], 
			                                               MoleculeModel.atomsLocationlist[i][1],
			                                               MoleculeModel.atomsLocationlist[i][2]);
		}
	}
	
	public override void ResetMDDriverPositions(){
		for (int j=0; j<cubes.Length; j++){
			int i = (int)cubes[j].number;
			cubes[j].transform.localPosition = MoleculeModel.atomsMDDriverLocationlist[i];
		}
	}
}
