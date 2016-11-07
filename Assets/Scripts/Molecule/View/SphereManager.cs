using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI;
using Molecule.Model;

public class SphereManager : GenericManager {
	private BallUpdateSphere[] balls;
	private static bool mouseOvers = false;

	/// <summary>
	/// Initializes this sphere manager.
	/// </summary>
	public override void Init () {
		balls = GameObject.FindObjectsOfType(typeof(BallUpdateSphere)) as BallUpdateSphere[];
		BallUpdate.resetColors = true;
		BallUpdate.resetRadii = true;
		enabled = true;
	}
	
	public override void DestroyAll() {
		Debug.Log("Destroying Spheres");
//		Debug.Log(balls.Length.ToString());

		balls = GameObject.FindObjectsOfType(typeof(BallUpdateSphere)) as BallUpdateSphere[];
		foreach(BallUpdateSphere sb in balls) {
			//sb.renderer.enabled = false;
			//DestroyImmediate(sb);
			GameObject.Destroy(sb);
		}
	}
	
	public override void ToggleDistanceCueing(bool enabling) {
		
	}
	
	
	public override void EnableRenderers() {
		balls = GameObject.FindObjectsOfType(typeof(BallUpdateSphere)) as BallUpdateSphere[];
		foreach(BallUpdateSphere sp in balls){
			sp.GetComponent<Renderer>().enabled = true;
			sp.GetComponent<Collider>().enabled = true;
		}
		enabled = true;
	}
	
	public override void DisableRenderers() {
		balls = GameObject.FindObjectsOfType(typeof(BallUpdateSphere)) as BallUpdateSphere[];
		foreach(BallUpdateSphere sp in balls){
			sp.GetComponent<Renderer>().enabled = false;
			if(UIData.atomtype != UIData.AtomType.particleball) // Particles don't have their own collider so we must keep it
				sp.GetComponent<Collider>().enabled = false; // Disable the collider at the same time to avoid ghost-clicking
		}
		enabled = false;
	}
	
	/// <summary>
	/// Resets the colors of all spheres and sticks. Uses the colors sets in BallUpdateSphere.
	/// </summary>
	private void ResetColors() {
		if(UIData.atomtype == UIData.AtomType.sphere){
	//		balls = GameObject.FindObjectsOfType(typeof(BallUpdateSphere)) as BallUpdateSphere[];
			foreach(BallUpdateSphere sp in balls) {
				sp.GetComponent<Renderer>().material.SetColor("_Color", Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number]);
	//			sp.oldatomcolor = sp.atomcolor;
			}
			BallUpdate.resetColors = false;
			BallUpdate.bondsReadyToBeReset = true;
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
//		Debug.Log("SetColor spall");
		if(!atom.Contains ("All")){
			if(residue == "All"){
				if(chain == "All"){
		// ---------- ATOM
//					Debug.Log("ATOM");
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateSphere sp in balls){
							if(atom.Contains(sp.tag))
								Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;
						}
					}else{
						foreach(BallUpdateSphere sp in balls){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)sp.number]))
								Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;
						}
					}
				}
				else{
		// ---------- ATOM + CHAIN
//					Debug.Log("ATOM+CHAIN");
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateSphere sp in balls){
							if(atom.Contains(sp.tag)
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;	
						}
					}else{
						foreach(BallUpdateSphere sp in balls){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)sp.number])
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;	
							}
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- ATOM + RESIDUE
//					Debug.Log("ATOM+RESIDUE");
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateSphere sp in balls){
							if(atom.Contains(sp.tag)
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue)
									Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;
						}
					}else{
						foreach(BallUpdateSphere sp in balls){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)sp.number])
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue)
									Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;
						}
					}
				}
				else{
		// ---------- ATOM + RESIDUE + CHAIN
//					Debug.Log("ATOM+RESIDUE+CHAIN");
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateSphere sp in balls){
							if(atom.Contains(sp.tag)
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;	
						}
					}else{
						foreach(BallUpdateSphere sp in balls){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)sp.number])
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;	
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
					foreach(BallUpdateSphere sp in balls)
						Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;
				}
				else{
		// ---------- CHAIN
//					Debug.Log("CHAIN");
					foreach(BallUpdateSphere sp in balls){
						if(Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
							Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;	
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- RESIDUE
//					Debug.Log("RESIDUE");
					foreach(BallUpdateSphere sp in balls){
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue){
							Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;		
						}
					}
				}
				else{
		// ---------- CHAIN + RESIDUE
//					Debug.Log("RESIDUE+CHAIN");
					foreach(BallUpdateSphere sp in balls){
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue
							&& Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
								Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;
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
//		Debug.Log("SphereManager object: SetColor(int, Color) : " + atomNum + " - " + col.ToString());
		foreach(BallUpdateSphere sp in balls){
			if(sp.GetComponent<BallUpdate>().number == atomNum)
				Molecule.Model.MoleculeModel.atomsColorList[(int)sp.number] = col;
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
						foreach(BallUpdateSphere sp in balls) {
							if(atom.Contains(sp.tag))
								Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);
						}
					}else{
						foreach(BallUpdateSphere sp in balls) {
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)sp.number]))
								Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);
							}
					}
				}
				else{
		// ---------- ATOM + CHAIN
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateSphere sp in balls) {
							if(atom.Contains(sp.tag)
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);		
						}
					}else{
						foreach(BallUpdateSphere sp in balls) {
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)sp.number])
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);		
						}
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- ATOM + RESIDUE
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateSphere sp in balls) {
							if(atom.Contains(sp.tag)
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);
						}
					}else{
						foreach(BallUpdateSphere sp in balls) {
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)sp.number])
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);
						}
					}
				}
				else{
		// ---------- ATOM + CHAIN + RESIDUE
					if(GUIDisplay.quickSelection){
						foreach(BallUpdateSphere sp in balls) {
							if(atom.Contains(sp.tag) 
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);		
						}
					}else{
						foreach(BallUpdateSphere sp in balls) {
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)sp.number])
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);		
						}
					}
				}
			}
		}
		else{
			if(residue == "All"){
				if(chain == "All"){
		// ---------- EVERYTHING
					foreach(BallUpdateSphere sp in balls) {
						Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);
					}
				}
				else{
		// ---------- CHAIN
					foreach(BallUpdateSphere sp in balls) {
						if(Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
							Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);		
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- RESIDUE
					foreach(BallUpdateSphere sp in balls) {
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue)
							Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);
					}
				}
				else{
		// ---------- CHAIN + RESIDUE
					foreach(BallUpdateSphere sp in balls) {
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)sp.number] == residue
							&& Molecule.Model.MoleculeModel.atomsChainList[(int)sp.number] == chain)
								Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);	
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
		foreach(BallUpdateSphere sp in balls) {
			if((int)sp.number == id)
				Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number] = (GUIDisplay.newScale);
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
		foreach(BallUpdateSphere sp in balls)
			if(sp.transform.position == pos)
				return sp.GetComponent<Renderer>().material.GetColor("_Color");
		return Color.white;
	}
	
	/// <summary>
	/// Gets the GameObject from balls at the position "id".
	/// </summary>
	/// <returns>
	/// The ball.
	/// </returns>
	/// <param name='id'>
	/// Identifier.
	/// </param>
	public override GameObject GetBall(int id){
		return balls[id].gameObject;
	}
	
	private void ResetRadii() {
		if(UIData.atomtype == UIData.AtomType.sphere){
			foreach(BallUpdateSphere sp in balls) {
				float buffer = Molecule.Model.MoleculeModel.atomsTypelist[(int)sp.number].radius
								* (GUIMoleculeController.globalRadius)
								* (Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)sp.number]/100);
				sp.transform.localScale = new Vector3(	buffer, buffer, buffer);
				sp.oldrayonFactor = sp.rayonFactor;
			}
			BallUpdate.oldRadiusFactor = BallUpdate.radiusFactor;
			BallUpdate.resetRadii = false;
		}
	}
	
	/// <summary>
	/// Creates the mouse overs, needed when mddriver simulation and mouse interaction are required
	/// </summary>
	private void CreateMouseOversMDDriver() {
		for (int i=0; i<balls.Length; i++) {
			balls[i].gameObject.AddComponent<MouseOverMDDriverMolecule>();
		}
		mouseOvers = true;
	}
	
	/// <summary>
	/// Destroys the mouse overs
	/// </summary>
	private void DestroyMouseOversMDDriver() {
		balls = GameObject.FindObjectsOfType(typeof(BallUpdateSphere)) as BallUpdateSphere[];
		for (int i=0; i<balls.Length; i++) {
			Destroy(balls[i].GetComponent<MouseOverMDDriverMolecule>());
		}
		mouseOvers = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(UIData.atomtype != UIData.AtomType.sphere) {
			if(GameObject.FindObjectsOfType(typeof(BallUpdateSphere)).Length >0) // Sometimes they're already destroyed at this point.
				DisableRenderers();
			return;
		}
		
		if(BallUpdate.resetColors)
			ResetColors();
		if(BallUpdate.resetRadii ||  (BallUpdate.oldRadiusFactor != BallUpdate.radiusFactor))
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
		for (int j=0; j<balls.Length; j++){
			int i = (int)balls[j].number;
			balls[j].transform.localPosition = new Vector3(MoleculeModel.atomsLocationlist[i][0], 
			                                               MoleculeModel.atomsLocationlist[i][1],
			                                               MoleculeModel.atomsLocationlist[i][2]);
		}
	}
	
	public override void ResetMDDriverPositions(){
		for (int j=0; j<balls.Length; j++){
			int i = (int)balls[j].number;
			balls[j].transform.localPosition = MoleculeModel.atomsMDDriverLocationlist[i];
		}
	}
}