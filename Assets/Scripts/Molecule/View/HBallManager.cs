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
/// $Id: HBallManager.cs 247 2013-04-07 20:38:19Z baaden $
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
using UI;

// Foreach statements are bad, they should all be removed! Unfortunately, I discovered that a little late. --- Alexandre
using Molecule.Model;
using System.Linq;


public class HBallManager : GenericManager {
	
	public static string[] allowedAtomsForPosition = {"CY", "C1", "U1", "G1", "G2", "A1", "A2"};
	

    public static BallUpdateHB[] hballs;
	private GameObject[] hballsGameObjects;
	private EllipsoidUpdateHB[] ellipsoids;
	
	public static bool xgmml = false;
	public static float depthFactor = 1.0f;
	public static float brightness = 1.0f;
	public static bool resetBrightness = false;
	private static float oldDepthFactor = 1.0f;
	private static bool mouseOvers = false;
	
	private bool ellipsoidView = false;
	private bool ellipsoidsInitialized = false;
	public bool ellipsoidViewEnabled() {
		return ellipsoidView;
	}

	/// <summary>
	/// Initalizes this instance.
	/// </summary>
	public override void Init () {
		hballs = GameObject.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[];
		if(UI.GUIDisplay.file_extension=="xgmml")
			xgmml = true;
		
		// Useful for second molecule
		BallUpdate.resetColors = true;
		BallUpdate.resetRadii = true;
		for (int i=0; i<hballs.Length; i++) {
			hballs[i].GetComponent<Renderer>().enabled = true;
			hballs[i].GetComponent<Renderer>().castShadows = false;
			hballs[i].GetComponent<Renderer>().receiveShadows = false;
//			hballs[i].renderer.material.shader = Shader.Find("Diffuse");
		}
		hballsGameObjects = new GameObject[MoleculeModel.atoms.Count];
		MoleculeModel.atoms.CopyTo(hballsGameObjects);
		enabled = true;
		Debug.Log("HBall Manager INIT INSIDE");
	}
	
	/// <summary>
	/// Disable and destroys all the Hyperballs and Sticks.
	/// </summary>
	public override void DestroyAll() {
		Debug.Log("Destroying Hyperboloids");
//		Debug.Log(hballs.Length.ToString());

		hballs = GameObject.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[];
		for (int i=0; i<hballs.Length; i++) {
//			hballs[i].renderer.enabled = false;
//			DestroyImmediate(hb);
			GameObject.Destroy(hballs[i]);
		}
	}
	
	/// <summary>
	/// Initializes the list of sticks.
	/// </summary>
/*	private void InitSticks() {
		sticks = GameObject.FindObjectsOfType(typeof(StickUpdate)) as StickUpdate[];
		Debug.Log("HBallManager: Init sticks");
		foreach(StickUpdate stu in sticks) {
			stu.renderer.enabled = true;
			stu.renderer.castShadows = false;
			stu.renderer.receiveShadows = false;
//			stu.renderer.material.shader = Shader.Find("Diffuse");
		}
		initSticks = true;
	}
*/
	
/*	
	/// <summary>
	/// Switch the shader of the HyperSticks : Single texture shader when Atom Texture is disabled, Double textures shader when Atom Texture is enabled.
	/// </summary>
	public void SetStickShader(){
		if(!GUIMoleculeController.toggle_NA_TEXATOM)
		{
			if(UIData.bondtype == UIData.BondType.hyperstick)
				foreach(StickUpdate stu in sticks)
					stu.renderer.material.shader = Shader.Find("FvNano/Stick HyperBalls OpenGL");
		}
		else
		{
			if(UIData.bondtype == UIData.BondType.hyperstick)
				foreach(StickUpdate stu in sticks)
					stu.renderer.material.shader = Shader.Find("FvNano/Stick HyperBalls 2 OpenGL");
		}
			
	}
*/
	
	public override void ToggleDistanceCueing(bool enabling) {
		if(UIData.bondtype != UIData.BondType.hyperstick)
			return;
		float attenuation;
		attenuation = enabling? 1f : 0f;
		
		for (int i=0; i<hballs.Length; i++)
			hballs[i].GetComponent<Renderer>().material.SetFloat("_Attenuation", attenuation);
	}
	
	
	
	/// <summary>
	/// Return the grayscale version of a texture
	/// </summary>
	/// <returns>
	/// Grayscale version of the texture. Texture2D.
	/// </returns>
	/// <param name='texture'>
	/// Texture to transform. Texture.
	/// </param>
	public Texture2D ToGray(Texture texture){ // Should be moved out of HBallManager since it's also used for Surface Textures
		Texture2D tex2D = (Texture2D)texture;
		Texture2D grayTex = new Texture2D(tex2D.width, tex2D.height);
		float grayScale;
		float alpha;
		for (int y  = 0; y < tex2D.height; ++y) {
            for (int x  = 0; x < tex2D.width; ++x) {
				grayScale = tex2D.GetPixel(x, y).r * 0.21f + tex2D.GetPixel(x, y).g * 0.71f + tex2D.GetPixel(x, y).b * 0.07f;
				alpha =  tex2D.GetPixel(x, y).a;
                grayTex.SetPixel (x, y, new Color(grayScale, grayScale, grayScale, alpha));
            }
        }
		grayTex.Apply();
		return grayTex;
	}
	
	/// <summary>
	/// Return the grayscale version of a texture
	/// </summary>
	/// <returns>
	/// Grayscale version of the texture. Texture2D.
	/// </returns>
	/// <param name='resource_name'>
	/// Path of the Texture to transform. String.
	/// </param>
	public Texture2D ToGray(string resource_name){
		Texture text2D = (Texture)Resources.Load(resource_name);
		return(ToGray(text2D));
	}
		
	
	
/*	/// <summary>
	/// Sets the MatCap texture for all balls and sticks.
	/// </summary>
	/// <param name='texture'>
	/// Texture to use. Texture.
	/// </param>
	public void SetTexture(Texture texture)	{
		Debug.Log("HBallManager object: SetTexture(Texture)");
		
		for (int i=0; i<hballs.Length; i++)
			if(hb!=null)
				hballs[i].renderer.material.SetTexture("_MatCap", texture);
		BallUpdate.bondsReadyToBeReset = true;
	}
*/
	
/*	/// <summary>
	/// Sets the MatCap texture for all balls and sticks.
	/// </summary>
	/// <param name='resource_name'>
	/// Path of the texture to use. String.
	/// </param>
	public void SetTexture(string resource_name) {
		Debug.Log("HBallManager object: SetTexture(String) : " + resource_name);
		
		Texture text2D = (Texture)Resources.Load(resource_name);
		SetTexture(text2D);
	}
*/	
	
	
	/// <summary>
	/// Sets the texture of atoms.
	/// </summary>
	/// <param name='texture'>
	/// Texture.
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
	public void SetTexture(Texture texture, List<string> atom, string residue = "All", string chain = "All") {
		Debug.Log("SetTexture HBall");
		for (int i = 0; i < MoleculeModel.ellipsoids.Count; i++)
		{
			MoleculeModel.ellipsoids[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);
		}
		if(!atom.Contains ("All")){
			if(residue == "All"){
				if(chain == "All"){
	// ---------- ATOM
//					Debug.Log("ATOM");
					if(GUIDisplay.quickSelection){
						for (int i=0; i<hballs.Length; i++)
							if(atom.Contains(hballs[i].tag))
								hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);
					}else{
						for (int i=0; i<hballs.Length; i++)
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)hballs[i].number]))
								hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);
					}
				}
				else{
		// ---------- ATOM + CHAIN
//					Debug.Log("ATOM+CHAIN");
					if(GUIDisplay.quickSelection){
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(hballs[i].tag)
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
									hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);		
						}
					}else{
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)hballs[i].number])
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
									hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);		
						}
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- ATOM + RESIDUE
//					Debug.Log("ATOM+RESIDUE");
					if(GUIDisplay.quickSelection){
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(hballs[i].tag)
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue)
									hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);
						}
					}else{
						for (int i=0; i<hballs.Length; i++){
						if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)hballs[i].number])
							&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue)
								hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);
					}
					}
				}
				else{
		// ---------- ATOM + RESIDUE + CHAIN
//					Debug.Log("ATOM+RESIDUE+CHAIN");
					if(GUIDisplay.quickSelection){
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(hballs[i].tag)
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
									hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);		
						}
					}else{
						for (int i=0; i<hballs.Length; i++){
						if(atom.Contains(hballs[i].tag)
							&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue
							&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
								hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);		
					}
					}
				}
			}
		}
		else{
			if(residue == "All"){
				if(chain == "All"){
		// ---------- ALL
//					Debug.Log("ALL");
					for (int i=0; i<hballs.Length; i++)
						hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);
				}
				else{
		// ---------- CHAIN
//					Debug.Log("CHAIN");
					for (int i=0; i<hballs.Length; i++){
						if(Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
							hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);		
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- RESIDUE
//					Debug.Log("RESIDUE");
					for (int i=0; i<hballs.Length; i++){
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue)
							hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);
					}
				}
				else{
		// ---------- RESIDUE + CHAIN
//					Debug.Log("RESIDUE.CHAIN");
					for (int i=0; i<hballs.Length; i++){
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue
							&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
								hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);		
					}
				}
			}
		}
		
		BallUpdate.bondsReadyToBeReset = true;
	}
	
	/// <summary>
	/// Sets the texture of one atom.
	/// </summary>
	/// <param name='resource_name'>
	/// Path of the texture.
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
	public void SetTexture(string resource_name, List<string> atom, string residue = "All", string chain = "All") {
		Texture text2D = (Texture)Resources.Load(resource_name);
		SetTexture(text2D, atom, residue, chain);
	}
	
	/// <summary>
	/// Sets the MatCap texture for one atom.
	/// </summary>
	/// <param name='tex'>
	/// Texture to use. Texture.
	/// </param>
	/// <param name='atomNum'>
	/// Number of the atom. Int.
	/// </param>
	public void SetTexture(Texture texture, int atomNum) {
		Debug.Log("HBallManager object: SetTexture(Texture, int) : " + texture.name + " - " + atomNum);
		
		for (int i=0; i<hballs.Length; i++)
			if(hballs[i].GetComponent<BallUpdate>().number == atomNum)
				hballs[i].GetComponent<Renderer>().material.SetTexture("_MatCap", texture);
		BallUpdate.bondsReadyToBeReset = true;
	}
	
	/// <summary>
	/// Sets the MatCap texture for one atom.
	/// </summary>
	/// <param name='resource_name'>
	/// Path of the texture to use. String.
	/// </param>
	/// <param name='atomNum'>
	/// Number of the atom. Int.
	/// </param>
	public void SetTexture(string resource_name, int atomNum) {
		Texture text2D = (Texture)Resources.Load(resource_name);
		SetTexture(text2D, atomNum);
	}
	
	
	/// <summary>
	/// Resets the colors of all balls and sticks. Uses the colors set in BallUpdateHB and StickUpdate objects.
	/// Whose classes probably ought to be renamed.
	/// </summary>
	private void ResetColors() {
//		Debug.Log("Resetting HBall colors");
		if(UIData.atomtype == UIData.AtomType.hyperball) {

			for (int i=0; i<hballs.Length; i++)
				//C.R
				if(UIData.secondarystruct){
					hballs[i].GetComponent<Renderer>().material.SetColor("_Color", hballs[i].atomcolor);}
				else
					hballs[i].GetComponent<Renderer>().material.SetColor("_Color", Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number]);
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
//		Debug.Log("SetColor HBall");
		if(!atom.Contains ("All")){
			if(residue == "All"){
				if(chain == "All"){
		// ---------- ATOM
//					Debug.Log("ATOM");
					if(GUIDisplay.quickSelection){
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(hballs[i].tag))
								Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;
						}
					}else{
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)hballs[i].number]))
								Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;
						}
					}
				}
				else{
		// ---------- ATOM + CHAIN
//					Debug.Log("ATOM+CHAIN");
					if(GUIDisplay.quickSelection){
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(hballs[i].tag)
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;	
						}
					}else{
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)hballs[i].number])
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;	
							}
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- ATOM + RESIDUE
//					Debug.Log("ATOM+RESIDUE");
					if(GUIDisplay.quickSelection){
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(hballs[i].tag)
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue)
									Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;
						}
					}else{
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)hballs[i].number])
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue)
									Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;
						}
					}
				}
				else{
		// ---------- ATOM + RESIDUE + CHAIN
//					Debug.Log("ATOM+RESIDUE+CHAIN");
					if(GUIDisplay.quickSelection){
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(hballs[i].tag)
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;	
						}
					}else{
						for (int i=0; i<hballs.Length; i++){
							if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)hballs[i].number])
								&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
									Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;	
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
					for (int i=0; i<hballs.Length; i++)
						Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;
				}
				else{
		// ---------- CHAIN
//					Debug.Log("CHAIN");
					for (int i=0; i<hballs.Length; i++){
						if(Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
							Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;	
					}
				}
			}
			else{
				if(chain == "All"){
		// ---------- RESIDUE
//					Debug.Log("RESIDUE");
					for (int i=0; i<hballs.Length; i++){
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue){
							Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;		
						}
					}
				}
				else{
		// ---------- CHAIN + RESIDUE
//					Debug.Log("RESIDUE+CHAIN");
					for (int i=0; i<hballs.Length; i++){
						if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue
							&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
								Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;
					}
				}
			}
		}
		
		BallUpdate.resetColors = true;
//		BallUpdate.bondsReadyToBeReset = true;
	}
	
	/// <summary>
	/// Sets the color of a specific atom. (Atom selection)
	/// </summary>
	/// <param name='col'>
	/// Color.
	/// </param>
	/// <param name='atomNum'>
	/// Atom number.
	/// </param>
	public override void SetColor(Color col, int atomNum) {	
		for (int i=0; i<hballs.Length; i++){
			if(hballs[i].GetComponent<BallUpdate>().number == atomNum)
				Molecule.Model.MoleculeModel.atomsColorList[(int)hballs[i].number] = col;
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
							for (int i=0; i<hballs.Length; i++) {
								if(atom.Contains(hballs[i].tag))
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);
							}
						}else{
							for (int i=0; i<hballs.Length; i++) {
								if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)hballs[i].number]))
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);
								}
						}
					}
					else{
			// ---------- ATOM + CHAIN
						if(GUIDisplay.quickSelection){
							for (int i=0; i<hballs.Length; i++) {
								if(atom.Contains(hballs[i].tag)
									&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
										Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);		
							}
						}else{
							for (int i=0; i<hballs.Length; i++) {
								if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)hballs[i].number])
									&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
										Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);		
							}
						}
					}
				}
				else{
					if(chain == "All"){
			// ---------- ATOM + RESIDUE
						if(GUIDisplay.quickSelection){
							for (int i=0; i<hballs.Length; i++) {
								if(atom.Contains(hballs[i].tag)
									&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue)
										Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);
							}
						}else{
							for (int i=0; i<hballs.Length; i++) {
								if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)hballs[i].number])
									&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue)
										Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);
							}
						}
					}
					else{
			// ---------- ATOM + CHAIN + RESIDUE
						if(GUIDisplay.quickSelection){
							for (int i=0; i<hballs.Length; i++) {
								if(atom.Contains(hballs[i].tag) 
									&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue
									&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
										Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);		
							}
						}else{
							for (int i=0; i<hballs.Length; i++) {
								if(atom.Contains(Molecule.Model.MoleculeModel.atomsNamelist[(int)hballs[i].number])
									&& Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue
									&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
										Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);		
							}
						}
					}
				}
			}
			else{
				if(residue == "All"){
					if(chain == "All"){
			// ---------- EVERYTHING
						for (int i=0; i<hballs.Length; i++) {
							Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);
						}
					}
					else{
			// ---------- CHAIN
						for (int i=0; i<hballs.Length; i++) {
							if(Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
								Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);		
						}
					}
				}
				else{
					if(chain == "All"){
			// ---------- RESIDUE
						for (int i=0; i<hballs.Length; i++) {
							if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue)
								Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);
						}
					}
					else{
			// ---------- CHAIN + RESIDUE
						for (int i=0; i<hballs.Length; i++) {
							if(Molecule.Model.MoleculeModel.atomsResnamelist[(int)hballs[i].number] == residue
								&& Molecule.Model.MoleculeModel.atomsChainList[(int)hballs[i].number] == chain)
									Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);	
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
			for (int i=0; i<hballs.Length; i++) {
				if((int)hballs[i].number == id)
					Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number] = (GUIDisplay.newScale);
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
		for (int i=0; i<hballs.Length; i++)
			if(hballs[i].transform.position == pos)
				return hballs[i].GetComponent<Renderer>().material.GetColor("_Color");
		return Color.white;
	}
	
	/// <summary>
	/// Gets the GameObject from hballs at the position "id".
	/// </summary>
	/// <returns>
	/// The ball.
	/// </returns>
	/// <param name='id'>
	/// Identifier.
	/// </param>
	public override GameObject GetBall(int id){
		return hballs[id].gameObject;
	}
	
	
	/// <summary>
	/// Moves the network nodes when the depth factor is changed.
	/// </summary>
	private void MoveNetworkNodes()	{
		for (int i=0; i<hballs.Length; i++)
			hballs[i].transform.localPosition = new Vector3(hballs[i].transform.localPosition.x,hballs[i].transform.localPosition.y, hballs[i].z * depthFactor);
	}
	
	/// <summary>
	/// Resets the positions of all balls. (Useful after interactive mode)
	/// </summary>
	public override void ResetPositions(){
		for (int j=0; j<hballs.Length; j++){
			int i = (int)hballs[j].number;
			hballs[j].transform.localPosition = new Vector3(MoleculeModel.atomsLocationlist[i][0], 
			                                                MoleculeModel.atomsLocationlist[i][1],
			                                                MoleculeModel.atomsLocationlist[i][2]);
		}
		BallUpdate.bondsReadyToBeReset = true;
		UIData.resetBondDisplay = true;
	}
	
	/// <summary>
	/// Resets the positions of all balls. (Useful after interactive mode)
	/// </summary>
	public override void ResetMDDriverPositions(){
		if (hballs.Length == 0) return;
		for (int j=0; j<hballs.Length; j++){
			if (hballs[j] == null) Debug.Log("Null hball: " + j);
			if (j >= hballs.Length) Debug.Log ("Out of range: " + j);
			int i = (int)hballs[j].number;
			hballs[j].transform.localPosition = MoleculeModel.atomsMDDriverLocationlist[i];
		}
	}
	
	/// <summary>
	/// Resets the radii of all balls when it is changed via the GUI.
	/// </summary>
	private void ResetRadii() {
		if(UIData.atomtype == UIData.AtomType.hyperball){
			hballs = GameObject.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[];
			//T.T sometimes this list is not initialized. So we initialise it here.
			
			if (Molecule.Model.MoleculeModel.atomsLocalScaleList.Count==0){
				for (int i=0; i<(Molecule.Model.MoleculeModel.atomsTypelist.Count); i++){
					Molecule.Model.MoleculeModel.atomsLocalScaleList.Add(GUIDisplay.newScale);
				}
			}

			//C.R 
			if (Molecule.Model.MoleculeModel.atomsLocalScaleList.Count < hballs[0].number+1){
				for(int i=(Molecule.Model.MoleculeModel.atomsLocalScaleList.Count)+1; i<=hballs[0].number+1; i++){
					Molecule.Model.MoleculeModel.atomsLocalScaleList.Add(GUIDisplay.newScale);
				}
			}			
			 
			for (int i=0; i<hballs.Length; i++) {
				if (UIData.secondarystruct){
					if(GUIMoleculeController.structType == "B Factor"){
						if (hballs[i].rayon == 3.7f){
							hballs[i].GetComponent<Renderer>().material.SetFloat("_Rayon", hballs[i].rayon 
							                              * (GUIMoleculeController.highBFradius)
							                              * (GUIMoleculeController.globalRadius)
							                              * (Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number]/100));}
						else{
							hballs[i].GetComponent<Renderer>().material.SetFloat("_Rayon", hballs[i].rayon 
							                              * (GUIMoleculeController.globalRadius)
							                              * (Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number]/100));
						}
					}
					else
					hballs[i].GetComponent<Renderer>().material.SetFloat("_Rayon", hballs[i].rayon 
					                              * (GUIMoleculeController.globalRadius)
					                              * (Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number]/100));
				}
				else{
				hballs[i].GetComponent<Renderer>().material.SetFloat("_Rayon", Molecule.Model.MoleculeModel.atomsTypelist[(int)hballs[i].number].radius 
												  * (GUIMoleculeController.globalRadius)
					                              * (Molecule.Model.MoleculeModel.atomsLocalScaleList[(int)hballs[i].number]/100));
				}
			}
			BallUpdate.oldRadiusFactor = BallUpdate.radiusFactor;
		}
	}
	
	/// <summary>
	/// Creates the mouse overs, needed when interactive mode is triggered.
	/// </summary>
	private void CreateMouseOvers() {
		
		for (int i=0; i<hballs.Length; i++) {
			hballs[i].gameObject.AddComponent<MouseOverMolecule>();
			hballs[i].hasMouseOverMolecule = true;
		}
		mouseOvers = true;
	}
	
	/// <summary>
	/// Destroys the mouse overs, needed when interactive mode is turned off.
	/// </summary>
	private void DestroyMouseOvers() {
		hballs = GameObject.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[];
		for (int i=0; i<hballs.Length; i++) {
			Destroy(hballs[i].GetComponent<MouseOverMolecule>());
			hballs[i].hasMouseOverMolecule = false;
		}
		mouseOvers = false;
	}
	
	/// <summary>
	/// Creates the mouse overs, needed when mddriver simulation and mouse interaction are required
	/// </summary>
	private void CreateMouseOversMDDriver() {
		for (int i=0; i<hballs.Length; i++) {
			hballs[i].gameObject.AddComponent<MouseOverMDDriverMolecule>();
			hballs[i].hasMouseOverMolecule = true;
		}
		mouseOvers = true;
	}
	
	/// <summary>
	/// Destroys the mouse overs
	/// </summary>
	private void DestroyMouseOversMDDriver() {
		hballs = GameObject.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[];
		for (int i=0; i<hballs.Length; i++) {
			Destroy(hballs[i].GetComponent<MouseOverMDDriverMolecule>());
			hballs[i].hasMouseOverMolecule = false;
		}
		mouseOvers = false;
	}
	
	/// <summary>
	/// Manages the rigidbody physics.
	/// Mostly just creates rigid bodies, spring joints, and sets a few parameters.
	/// </summary>
	private void ManagePhysics() {
		for (int i=0; i<hballs.Length; i++) {
			// Might be better to do this with booleans.
			if(!hballs[i].GetComponent<Rigidbody>())
				hballs[i].gameObject.AddComponent<Rigidbody>();
			if(!hballs[i].GetComponent<SpringJoint>())
				hballs[i].gameObject.AddComponent<SpringJoint>();
			
			float v = hballs[i].GetComponent<Rigidbody>().velocity.magnitude;
			
			if(UIData.toggleGray) {
				Color c = Color.Lerp(Color.white, Color.black, v);
				hballs[i].GetComponent<Renderer>().material.SetColor("_Color", c); // ugly
			}
		}
	}
	
	
	/// <summary>
	/// Disables the renderers for the entire set of balls and sticks.
	/// </summary>
	public override void DisableRenderers() {
		Debug.Log("HBallManager: Disabling renderers");
		hballs = GameObject.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[];
		for (int i=0; i<hballs.Length; i++){
			//if(hb)
			hballs[i].GetComponent<Renderer>().enabled = false;
			if(UIData.atomtype != UIData.AtomType.particleball) // Particles don't have their own collider so we must keep it
				hballs[i].GetComponent<Collider>().enabled = false; // Disable the collider at the same time to avoid ghost-clicking with atom selection
		}
//		DeactivateBases();
		DeactivateEllipsoids();
		
		enabled = false;
	}

	
	/// <summary>
	/// Enables the renderers for the entire set of balls and sticks.
	/// </summary>
	public override void EnableRenderers(){
		hballs = GameObject.FindObjectsOfType(typeof(BallUpdateHB)) as BallUpdateHB[];
		if(UIData.atomtype != UIData.AtomType.hyperball)
			return;
		
		for (int i=0; i<hballs.Length; i++){
			//if(hb)
			hballs[i].GetComponent<Renderer>().enabled = true;
			hballs[i].GetComponent<Collider>().enabled = true;
		}
		
		Debug.Log("HBallManager: Enabling renderer");
		enabled = true;
	}
	
	private void ResetBrightness() {
		for(int i=0; i<hballs.Length; i++)
			hballs[i].GetComponent<Renderer>().material.SetFloat("_Brightness", brightness);
		
		HStickManager.resetBrightness = true;
		resetBrightness = false;
	}
	

	void Update () {
		if (ellipsoidsInitialized == false && UIData.ffType == UIData.FFType.HiRERNA)
		{
			GenerateEllipsoids();
		}
		
		if(UIData.atomtype != UIData.AtomType.hyperball) {
			DisableRenderers();
		}
			
		if(BallUpdate.resetColors && !Molecule.Model.MoleculeModel.networkLoaded){
//			Debug.Log("HBALL RESETCOLOR CALL" + UIData.atomtype);
			ResetColors();
		}
		
		if(resetBrightness)
			ResetBrightness();
		
		if (xgmml && (oldDepthFactor!=depthFactor)) {
			MoveNetworkNodes();
			oldDepthFactor=depthFactor;
		}
		
		if(GUIMoleculeController.toggle_NA_INTERACTIVE) {
			if(!mouseOvers)
			{
				CreateMouseOvers();
			}
			ManagePhysics();
		}
		else if(GUIMoleculeController.toggle_MDDRIVER) {
			if(!mouseOvers)
			{
				CreateMouseOversMDDriver();
			}
		}
		else
		{
			if(mouseOvers)
			{
				if (GUIMoleculeController.toggle_NA_INTERACTIVE)
				{
					DestroyMouseOvers();
				}
				if (GUIMoleculeController.toggle_MDDRIVER)
				{
					DestroyMouseOversMDDriver();
				}
			}
		}
		
		if( ( (BallUpdate.oldRadiusFactor != BallUpdate.radiusFactor) || BallUpdate.resetRadii ||
			(UIData.bondtype == UIData.BondType.hyperstick && (StickUpdate.shrink != StickUpdate.oldshrink)
												|| (StickUpdate.scale != StickUpdate.oldscale)) ) ) {
			if(UIData.atomtype == UIData.AtomType.hyperball) {
				ResetRadii();
				BallUpdate.resetRadii = false;
			}
		}
		
		if (GUIMoleculeController.toggle_MDDRIVER && ellipsoidView)
		{
//			Debug.Log(">>>>>>>>>>>>>>>Update Ellipsoids");
			UpdateEllipsoids();
		}
		
/*		if(UIData.EnableUpdate){ // not sure what that's for
			
		}
*/
	}
	
	//
	// HiRERNA
	//
	
	/// <summary>
	/// Compute an ellipsoid position based on its associated atoms
	/// </summary>
	/// <returns>The ellipsoid position.</returns>
	/// <param name="residueId">Residue identifier.</param>
	public Vector3 computeEllipsoidPosition(int residueId)
	{
		int[] atoms = MoleculeModel.atomsForEllipsoidsPerResidue[residueId];
		
		Vector3 sum = new Vector3(0.0f, 0.0f, 0.0f);
		int currentAtomId = 0;
		int nbOfAtoms = 0;
		for (int i = 0; i < 3; i++) {
			currentAtomId = atoms[i];
			if (allowedAtomsForPosition.Contains(MoleculeModel.atomsNamelist[currentAtomId])) {
				GameObject go = hballsGameObjects[atoms[i]] as GameObject;
				sum += go.transform.position;
				nbOfAtoms += 1;
			}
		}
		
		return sum / nbOfAtoms;
	}
	
	/// <summary>
	///  Compute the plane normal for an ellipsoid (given a residue)
	/// </summary>
	/// <returns>The ellipsoid normal.</returns>
	/// <param name="residueId">Residue identifier.</param>
	public Vector3 computeEllipsoidNormal(int residueId)
	{
		int[] atoms = MoleculeModel.atomsForEllipsoidsPerResidue[residueId];
		
		GameObject go = hballsGameObjects[atoms[0]] as GameObject;
		Vector3 point1 = go.transform.position;
		
		go = hballsGameObjects[atoms[1]] as GameObject;
		Vector3 point2 = go.transform.position;
		
		go = hballsGameObjects[atoms[2]] as GameObject;
		Vector3 point3 = go.transform.position;
		
		//		Vector3 planeNormal = 
		
		//		float angleXY = 180 * Mathf.Acos(Vector3.Dot (planeNormal, new Vector3(0.0f, 0.0f, 1.0f)) / planeNormal.magnitude) / Mathf.PI;
		//		float angleXZ = 180 * Mathf.Acos(Vector3.Dot (planeNormal, new Vector3(0.0f, 1.0f, 0.0f)) / planeNormal.magnitude) / Mathf.PI;
		//		float angleYZ = 180 * Mathf.Acos(Vector3.Dot (planeNormal, new Vector3(1.0f, 0.0f, 0.0f)) / planeNormal.magnitude) / Mathf.PI;
		
		//		Debug.Log (angleXY);
		//		Debug.Log (angleXZ);
		//		Debug.Log (angleYZ);
		
		return Vector3.Cross(point2 - point1, point3 - point1).normalized;
	}
	
	public Vector3 computeEllipsoidOrientation(int residueId)
	{
		return (hballsGameObjects[MoleculeModel.atomsForEllipsoidsOrientationPerResidue[residueId]].transform.position - MoleculeModel.ellipsoidsPerResidue[residueId].transform.position).normalized;
	}
	
	/// <summary>
	/// Find atoms used to compute the ellipsoid plane
	/// </summary>
	public void findAtoms()
	{
		// Traverse "residues" (in fact, these are nucleotides)
		Debug.Log("Finding atoms");
		foreach (KeyValuePair<int, ArrayList> entry in MoleculeModel.residues)
		{
			int nbOfAtoms = entry.Value.Count;
			
			int currentAtomId = 0;
			string currentAtomName;
			string currentResidueName;
			
			int i = 0;
			List<int> triplet = new List<int>();
			while (i != nbOfAtoms)
			{
				currentAtomId = (int)entry.Value[i];
				currentAtomName = MoleculeModel.atomsNamelist[currentAtomId];
				currentResidueName = MoleculeModel.atomsResnamelist[currentAtomId]; // Could potentially be out of this loop
				
				// For bases G and A, ellipsoid is computed from the triplet CY-G1-G2 or CY-A1-A2
				// For U and C, we use CA-CY-C1 or CA-CY-U1
				if (currentResidueName == "G" || currentResidueName == "A") {
					if (currentAtomName == "CY" || currentAtomName == "G1" || currentAtomName == "G2" || currentAtomName == "A1" || currentAtomName == "A2") {
						triplet.Add (currentAtomId);
					}
				}
				else
				{
					if (currentAtomName == "CY" || currentAtomName == "CA" || currentAtomName == "C1" || currentAtomName == "U1") {
						triplet.Add (currentAtomId);
					}
				}
				
				if (triplet.Count == 3)
				{
					i = nbOfAtoms;
				}
				else
				{
					i++;
				}
			}
			MoleculeModel.atomsForEllipsoidsPerResidue[entry.Key] = triplet.ToArray();
			
			i = 0;
			while (i != nbOfAtoms)
			{
				currentAtomId = (int)entry.Value[i];
				currentAtomName = MoleculeModel.atomsNamelist[currentAtomId];
				if (currentAtomName == "CA")
				{
//					Debug.Log ("Residue: " + entry.Key);
//					Debug.Log ("Found CA atom");
					MoleculeModel.atomsForEllipsoidsOrientationPerResidue[entry.Key] = currentAtomId;
					i = nbOfAtoms;
				}
				else
				{
					i++;
				}
			}
		}
	}
	
	/// <summary>
	/// Find bonds to be hidden to render ellipsoids
	/// </summary>
	public void findBonds()
	{
		// Find bonds associated with base atoms
		StickUpdate[] sticks = GameObject.FindObjectsOfType(typeof(StickUpdate)) as StickUpdate[];
		MoleculeModel.bondsForReplacedAtoms.Clear();
		int nbOfSticks = sticks.Length;
		for (int i = 0; i < nbOfSticks; i++)
		{	
			int atomNumber1 = (int)sticks[i].atompointer1.GetComponent<BallUpdate>().number;
			int atomNumber2 = (int)sticks[i].atompointer2.GetComponent<BallUpdate>().number;
			
			if (   MoleculeModel.atomsTypelist[atomNumber1].type == "X"
			    || MoleculeModel.atomsTypelist[atomNumber2].type == "X"
			    || MoleculeModel.atomsNamelist[atomNumber1] == "CY"
			    || MoleculeModel.atomsNamelist[atomNumber2] == "CY")
			{
				MoleculeModel.bondsForReplacedAtoms.Add(sticks[i].gameObject);
			}
		}
	}
	
	public void UpdateEllipsoids()
	{
		foreach (KeyValuePair<int, GameObject> entry in MoleculeModel.ellipsoidsPerResidue)
		{
			GameObject Atom = entry.Value;
//			BallUpdate comp = Atom.GetComponent<BallUpdate>();
			
			Vector3 pos = computeEllipsoidPosition(entry.Key);
			Atom.transform.position = pos;
			
			Vector3 normal = computeEllipsoidNormal(entry.Key);
			Atom.transform.forward = normal;
			
//			Vector3 orientation = computeEllipsoidOrientation(entry.Key);

			Atom.transform.LookAt(hballsGameObjects[MoleculeModel.atomsForEllipsoidsOrientationPerResidue[entry.Key]].transform.position, normal);
//			if (entry.Key == 1) {
//				
//				Renderer r = Atom.GetComponent<Renderer>() as Renderer;
//				r.material.color = Color.green;
//				Debug.Log (180 * Mathf.Acos(Vector3.Dot(Atom.transform.right, orientation)) / Mathf.PI, Atom);
//			}
//			float angle = 180 * Mathf.Acos(Vector3.Dot(Atom.transform.right, orientation)) / Mathf.PI;
//			Vector3 cross = Vector3.Cross(Atom.transform.right, orientation);
			
//			Atom.transform.Rotate(Vector3.forward, angle);
//			Atom.transform.right = orientation;
		}
	}
	
	/// <summary>
	/// Generates the ellipsoids based on coarse-grain model
	/// </summary>
	public void GenerateEllipsoids()
	{
		findAtoms();
		
		findBonds();
		
		string currentResidueName;
		
		// Create ellipsoid game objects and bonds
		foreach (KeyValuePair<int, ArrayList> entry in MoleculeModel.residues)
		{			
			GameObject Atom;
			Atom = GameObject.CreatePrimitive(PrimitiveType.Cube);
			
			//			Atom.renderer.material.shader = Shader.Find("Custom/EllipsoidBase");
			Atom.GetComponent<Renderer>().material.shader = Shader.Find("FvNano/Ball HyperBalls OpenGL");
			
			Atom.AddComponent<EllipsoidUpdateHB>();
			BallUpdate comp = Atom.GetComponent<BallUpdate>();
			
			currentResidueName = MoleculeModel.atomsResnamelist[(int)entry.Value[0]];
			
			if (currentResidueName == "C") {
				Atom.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
			}
			else if (currentResidueName == "G") {
				Atom.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
			}
			else if (currentResidueName == "A") {
				Atom.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
			}
			else if (currentResidueName == "U") {
				Atom.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.7f, 0.7f, 0.98f));
			}
			
			comp.rayon = 1.0f;
			comp.SetRayonFactor(1.0f);
			Atom.GetComponent<Renderer>().material.SetFloat("_Rayon", 1.0f);
			Atom.GetComponent<Renderer>().material.SetVector("_Equation", new Vector4(0.2f, 0.8f, 0.1f, 1.0f));
			
			Atom.GetComponent<Renderer>().transform.localScale = new Vector3(3.0f, 1.5f, 4.0f);
			
//			Atom.AddComponent<Projector>();
			comp.enabled = true;
			
			MoleculeModel.ellipsoidsPerResidue[entry.Key] = Atom;
			MoleculeModel.ellipsoids.Add(Atom);
		}
		
		UpdateEllipsoids();
		
		ellipsoids = GameObject.FindObjectsOfType(typeof(EllipsoidUpdateHB)) as EllipsoidUpdateHB[];
		ellipsoidsInitialized = true;
		DeactivateEllipsoids();
	}
	
	public void ActivateBases()
	{
		// Enable base atoms and bonds, then disable ellipsoids
//		GameObject[] baseAtoms = MoleculeModel.atomsByChar["X"];
//		for (int i = 0; i < baseAtoms.Length; i++)
//		{
//				baseAtoms[i].SetActive(true);
//			baseAtoms[i].renderer.enabled = true;
//		}
		foreach (KeyValuePair<int, int[]> entry in MoleculeModel.atomsForEllipsoidsPerResidue)
		{
			for(int i = 0; i < entry.Value.Length; i++)
			{
				if (allowedAtomsForPosition.Contains(MoleculeModel.atomsNamelist[entry.Value[i]])) {
					GameObject go = hballsGameObjects[entry.Value[i]] as GameObject;
					go.GetComponent<Renderer>().enabled = true;
				}
			}
		}
		
		for (int i = 0; i < MoleculeModel.bondsForReplacedAtoms.Count; i++)
		{
			//				MoleculeModel.bondsForReplacedAtoms[i].SetActive(true);
			MoleculeModel.bondsForReplacedAtoms[i].GetComponent<Renderer>().enabled = true;
		}
	}
	
	public void DeactivateBases()
	{
		// Disable base atoms and bonds, then enable ellipsoids
//		GameObject[] baseAtoms = MoleculeModel.atomsByChar["X"];
//		for (int i = 0; i < baseAtoms.Length; i++)
//		{
			//				baseAtoms[i].SetActive(false);
//			baseAtoms[i].renderer.enabled = false;
//		}
		foreach (KeyValuePair<int, int[]> entry in MoleculeModel.atomsForEllipsoidsPerResidue)
		{
			for(int i = 0; i < entry.Value.Length; i++)
			{
				if (allowedAtomsForPosition.Contains(MoleculeModel.atomsNamelist[entry.Value[i]])) {
					GameObject go = hballsGameObjects[entry.Value[i]] as GameObject;
					go.GetComponent<Renderer>().enabled = false;
				}
			}
		}
		
		for (int i = 0; i < MoleculeModel.bondsForReplacedAtoms.Count; i++)
		{
//			MoleculeModel.bondsForReplacedAtoms[i].SetActive(false);
			if (MoleculeModel.bondsForReplacedAtoms[i])
			{
				MoleculeModel.bondsForReplacedAtoms[i].GetComponent<Renderer>().enabled = false;
			}
		}
	}
	
	public void ActivateEllipsoids()
	{
		if (ellipsoidsInitialized == false)
			return;
		for (int i = 0; i < ellipsoids.Length; i++)
		{
			ellipsoids[i].GetComponent<Renderer>().enabled = true;
		}
	}
	
	public void DeactivateEllipsoids()
	{
		if (ellipsoidsInitialized == false)
			return;
		for (int i = 0; i < ellipsoids.Length; i++)
		{
			ellipsoids[i].GetComponent<Renderer>().enabled = false;
		}
	}
	
	public void RenderEllipsoids()
	{
		DeactivateBases();
		
		ActivateEllipsoids();
	}
	
	public void RenderAtoms()
	{
		DeactivateEllipsoids();
		
		ActivateBases();
	}
	
	public void SwitchRendering()
	{
		ellipsoidView = !ellipsoidView;
		if (ellipsoidView == true)
		{
			RenderEllipsoids();
		}
		else
		{
			RenderAtoms();
		}
	}
}
