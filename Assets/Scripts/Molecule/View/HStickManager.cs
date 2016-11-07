using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI;

public class HStickManager : GenericManager {
	public static StickUpdate[] sticks;
	
	public static bool xgmml = false;
	public static float depthFactor = 1.0f;
	public static bool resetBrightness = false;
	private static float oldDepthFactor = 1.0f;

	// Use this for initialization
	public override void Init () {
		if(UI.GUIDisplay.file_extension=="xgmml")
			xgmml = true;
		
		sticks = GameObject.FindObjectsOfType(typeof(StickUpdate)) as StickUpdate[];
		BallUpdate.bondsReadyToBeReset = true;
		enabled = true;
		//for (int i=0; i< sticks.Length; i++) {
		for (int i=0; i< sticks.Length; i++){
			sticks[i].GetComponent<Renderer>().enabled = true;
			sticks[i].GetComponent<Renderer>().castShadows = false;
			sticks[i].GetComponent<Renderer>().receiveShadows = false;
		}
	}
	
	public override void DestroyAll() {
		
	}
	
	public override void SetColor(Color col, List<string> atoms, string residue = "All", string chain = "All"){}
	public override void SetColor(Color col, int atomNum){}
	public override void SetRadii(List<string> atoms, string residue = "All", string chain = "All"){}
	public override void SetRadii(int atomNum){}
	
	public override GameObject GetBall(int id){
		return null;
	}
	
	public override void ToggleDistanceCueing(bool enabling) {
		if(UIData.bondtype != UIData.BondType.hyperstick)
			return;
		float attenuation;
		attenuation = enabling? 1f : 0f;
		
		//for (int i=0; i< sticks.Length; i++)
		for (int i=0; i< sticks.Length; i++)
			sticks[i].GetComponent<Renderer>().material.SetFloat("_Attenuation", attenuation);
	}
	
	
	private void ResetColors() {
		if(UIData.bondtype == UIData.BondType.hyperstick){
			sticks = GameObject.FindObjectsOfType(typeof(StickUpdate)) as StickUpdate[];

			for (int i=0; i< sticks.Length; i++) {
				sticks[i].GetComponent<Renderer>().material.SetColor("_Color", sticks[i].atompointer1.GetComponent<Renderer>().material.GetColor("_Color"));
				sticks[i].GetComponent<Renderer>().material.SetColor("_Color2", sticks[i].atompointer2.GetComponent<Renderer>().material.GetColor("_Color"));
			}
			
			BallUpdate.bondsReadyToBeReset = false;
		}
	}
	
	private void ResetTextures() {
		if(UIData.bondtype == UIData.BondType.hyperstick){
			sticks = GameObject.FindObjectsOfType(typeof(StickUpdate)) as StickUpdate[];
			if(UIData.atomtype == UIData.AtomType.hyperball){
				for (int i=0; i< sticks.Length; i++) {
					sticks[i].GetComponent<Renderer>().material.SetTexture("_MatCap", sticks[i].atompointer1.GetComponent<Renderer>().material.GetTexture("_MatCap"));
					sticks[i].GetComponent<Renderer>().material.SetTexture("_MatCap2", sticks[i].atompointer2.GetComponent<Renderer>().material.GetTexture("_MatCap"));
				}
			}
			else{
			for (int i=0; i< sticks.Length; i++) {
					sticks[i].GetComponent<Renderer>().material.SetTexture("_MatCap", (Texture)Resources.Load("lit_spheres/divers/daphz05"));
					sticks[i].GetComponent<Renderer>().material.SetTexture("_MatCap2", (Texture)Resources.Load("lit_spheres/divers/daphz05"));
				}	
			}
			BallUpdate.bondsReadyToBeReset = false;
		}
	}
	
	/// <summary>
	/// Resets the bond vectors, which is necessary when the balls move.
	/// Not very optimized: transform.position is actually quite costly, and buffering might be better.
	/// </summary>
	public override void ResetPositions()	{
		Vector3 atomOne = Vector3.zero;
		sticks = GameObject.FindObjectsOfType(typeof(StickUpdate)) as StickUpdate[];
		for (int i=0; i< sticks.Length; i++) {
			atomOne = sticks[i].atompointer1.transform.position; // transform.position is costly; this way, we do it twice instead of thrice
			sticks[i].GetComponent<Renderer>().material.SetVector("_TexPos1", atomOne);
			sticks[i].transform.position = atomOne;
			sticks[i].GetComponent<Renderer>().material.SetVector("_TexPos2", sticks[i].atompointer2.transform.position);
		}
	}
	
	public override void ResetMDDriverPositions() {
		
	}
	
	/// <summary>
	/// Adjusts the stick radii, which is needed to match the size of the balls when their radii are modified.
	/// </summary>
	private void AdjustStickRadii() {
		sticks = GameObject.FindObjectsOfType(typeof(StickUpdate)) as StickUpdate[];
		if(UIData.atomtype == UIData.AtomType.hyperball) {
			for (int i=0; i< sticks.Length; i++) {
				//if it's not a network
				if(!xgmml){
					sticks[i].GetComponent<Renderer>().material.SetFloat("_Rayon1", sticks[i].atompointer1.GetComponent<Renderer>().material.GetFloat("_Rayon"));
					sticks[i].GetComponent<Renderer>().material.SetFloat("_Rayon2", sticks[i].atompointer2.GetComponent<Renderer>().material.GetFloat("_Rayon"));
				//if it's a network, we had to reduce de size of stick (otherwise it looks like a licornice representation)
				}else{
				//	Debug.Log(sticks[i].atompointer1.renderer.material.GetFloat("_Rayon")/2);
					sticks[i].GetComponent<Renderer>().material.SetFloat("_Rayon1", sticks[i].atompointer1.GetComponent<Renderer>().material.GetFloat("_Rayon")/2);
					sticks[i].GetComponent<Renderer>().material.SetFloat("_Rayon2", sticks[i].atompointer2.GetComponent<Renderer>().material.GetFloat("_Rayon")/2);
				}
				// Might not be necessary anymore.
				sticks[i].oldrayon1 = sticks[i].atompointer1.GetComponent<Renderer>().material.GetFloat("_Rayon");
				sticks[i].oldrayon2 = sticks[i].atompointer2.GetComponent<Renderer>().material.GetFloat("_Rayon");
			}
		}
		else {
			//sticks = GameObject.FindObjectsOfType(typeof(StickUpdate)) as StickUpdate[];
			for (int i=0; i< sticks.Length; i++) {
				sticks[i].GetComponent<Renderer>().material.SetFloat("_Rayon1", sticks[i].atompointer1.transform.lossyScale.x/2);
				sticks[i].GetComponent<Renderer>().material.SetFloat("_Rayon2", sticks[i].atompointer2.transform.lossyScale.x/2);
				
				sticks[i].oldrayon1 = sticks[i].atompointer1.transform.lossyScale.x/2;
				sticks[i].oldrayon2 = sticks[i].atompointer2.transform.lossyScale.x/2;
			}			
		}
	}
	
	public override void EnableRenderers() {
		for (int i=0; i< sticks.Length; i++)
			sticks[i].GetComponent<Renderer>().enabled = true;
		enabled = true;
	}
	
	public override void DisableRenderers() {
		sticks = GameObject.FindObjectsOfType(typeof(StickUpdate)) as StickUpdate[];
		Debug.Log("StickManager: DisableRenderers()");
		for (int i=0; i< sticks.Length; i++)
			sticks[i].GetComponent<Renderer>().enabled = false;
		enabled = false;
	}
	
	private void ResetBrightness() {
		for(int i=0; i<sticks.Length; i++)
			sticks[i].GetComponent<Renderer>().material.SetFloat("_Brightness", HBallManager.brightness);
		
		resetBrightness = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(UIData.bondtype != UIData.BondType.hyperstick) {
			enabled = false;
			return;
		}
		
		if(BallUpdate.bondsReadyToBeReset){
			ResetColors();
			ResetTextures();
		}
		
		if(resetBrightness)
			ResetBrightness();
		
		if(GUIMoleculeController.toggle_NA_INTERACTIVE && UIData.toggleGray)
			ResetColors();
		
		if (xgmml && (oldDepthFactor!=depthFactor)) {
//			ResetBondVectors();
			ResetPositions();
			oldDepthFactor=depthFactor;
		}
		
		if(GUIMoleculeController.toggle_NA_INTERACTIVE) 
//			ResetBondVectors();
			ResetPositions();
		
		if(BallUpdate.resetRadii || (StickUpdate.shrink != StickUpdate.oldshrink)) {
			for (int i=0; i< sticks.Length; i++)
				sticks[i].GetComponent<Renderer>().material.SetFloat("_Shrink", StickUpdate.shrink);
			StickUpdate.oldshrink = StickUpdate.shrink;
		}
		if(BallUpdate.resetRadii || (StickUpdate.scale != StickUpdate.oldscale)) {
			for (int i=0; i< sticks.Length; i++)
				sticks[i].GetComponent<Renderer>().material.SetFloat("_Scale",StickUpdate.scale);
			StickUpdate.oldscale = StickUpdate.scale;
		}

		AdjustStickRadii();
		
	}
	
}
