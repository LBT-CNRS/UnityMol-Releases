using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI;

public class CubeBondManager : GenericManager {
	private BondCubeUpdate[] bonds;

	// Use this for initialization
	public override void Init () {
		bonds = GameObject.FindObjectsOfType(typeof(BondCubeUpdate)) as BondCubeUpdate[];
		BallUpdate.bondsReadyToBeReset = true;
		enabled = true;
		foreach(BondCubeUpdate bu in bonds)
			bu.GetComponent<Collider>().enabled = false;
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
		
	}
	
	private void AdjustWidths() {
		float width = BondCubeUpdate.width;
		foreach(BondCubeUpdate bcu in bonds) {
			Vector3 lscale = new Vector3(width, width, bcu.transform.localScale.z); 
			bcu.transform.localScale = lscale ;	
		}
		BondCubeUpdate.oldWidth = width;
	}
	
	private void AdjustScales() {
		foreach(BondCubeUpdate bcu in bonds)
			bcu.GetComponent<Renderer>().material.SetFloat("_Scale", BondCubeUpdate.scale);
		BondCubeUpdate.oldscale = BondCubeUpdate.scale;
	}
	
	private void ResetColors() {
		if(UIData.bondtype == UIData.BondType.cube){
			bonds = GameObject.FindObjectsOfType(typeof(BondCubeUpdate)) as BondCubeUpdate[];
			foreach(BondCubeUpdate bcu in bonds) {
				//bcu.renderer.material.SetColor("_Color1", bcu.atompointer1.renderer.material.GetColor("_Color"));
				//bcu.renderer.material.SetColor("_Color2", bcu.atompointer2.renderer.material.GetColor("_Color"));
				
				Mesh mesh = bcu.GetComponent<MeshFilter>().mesh;
				Vector3[] vertices = mesh.vertices;
				Color32[] colors = new Color32[vertices.Length];
				float dist1, dist2;
				Matrix4x4 localToWorld = bcu.transform.localToWorldMatrix;
				
				Vector3 pos;
				Vector3 pos1 = bcu.atompointer1.transform.position;
				Vector3 pos2 = bcu.atompointer2.transform.position;
				Color32 color1 = bcu.atompointer1.GetComponent<Renderer>().material.GetColor("_Color");
				Color32 color2 = bcu.atompointer2.GetComponent<Renderer>().material.GetColor("_Color");
				for(int i=0; i<vertices.Length; i++) {
					pos = localToWorld.MultiplyPoint3x4(vertices[i]);
					dist1 = Vector3.Distance(pos1, pos);
					dist2 = Vector3.Distance(pos2, pos);
					if( dist1 < dist2 )
						colors[i] = color1;
					else
						colors[i] = color2;
				}
				
				mesh.colors32 = colors;
				bcu.GetComponent<MeshFilter>().mesh = mesh;
			}
			BallUpdate.bondsReadyToBeReset = false;
		}
	}
	
	public override void EnableRenderers() {
		foreach(BondCubeUpdate bcu in bonds)
			bcu.GetComponent<Renderer>().enabled = true;
		enabled = true;
	}
	
	public override void DisableRenderers() {
		Debug.Log("CubeBondManager: DisableRenderers()");
		foreach(BondCubeUpdate bcu in bonds)
			bcu.GetComponent<Renderer>().enabled = false;
		enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(UIData.bondtype != UIData.BondType.cube) {
			enabled = false;
			return;
		}
		
		if(BondCubeUpdate.width != BondCubeUpdate.oldWidth)
			AdjustWidths();
		
		if(BondCubeUpdate.scale != BondCubeUpdate.oldscale)
			AdjustScales();
		
		if(BallUpdate.bondsReadyToBeReset)
			ResetColors();
	}
	
	
	// TODO ?
/*
	if(UIData.EnableUpdate)
	{	
		transform.position = (atompointer1.transform.position + atompointer2.transform.position)/2.0f;
		transform.LookAt(atompointer2.transform.position);

		renderer.material.SetVector("_Pos1", atompointer1.transform.position);
		renderer.material.SetVector("_Pos2", atompointer2.transform.position);
		renderer.material.SetColor("_Color1", atompointer1.renderer.material.GetColor("_Color"));
		renderer.material.SetColor("_Color2", atompointer2.renderer.material.GetColor("_Color")); 
	}
*/
	public override void ResetPositions()	{
		Vector3 atomOne = Vector3.zero;
		bonds = GameObject.FindObjectsOfType(typeof(BondCubeUpdate)) as BondCubeUpdate[];
		for (int i=0; i< bonds.Length; i++) {
			atomOne = bonds[i].atompointer1.transform.position; // transform.position is costly; this way, we do it twice instead of thrice
			bonds[i].GetComponent<Renderer>().material.SetVector("_Pos1", atomOne);
			bonds[i].transform.position = atomOne;
			bonds[i].GetComponent<Renderer>().material.SetVector("_Pos2", bonds[i].atompointer2.transform.position);
		}
	}
	
	public override void ResetMDDriverPositions() {
	
	}
}
