using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI;


public class LineManager : GenericManager {
	private LineUpdate[] lines;
	LineRenderer lineRenderer;

	// Use this for initialization
	public override void Init () {
		lines = GameObject.FindObjectsOfType(typeof(LineUpdate)) as LineUpdate[];
		BallUpdate.bondsReadyToBeReset = true;
		enabled = true;
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
	
	private void ResetColors() {
		if(UIData.bondtype == UIData.BondType.line){
			foreach(LineUpdate lu in lines)	{
				lineRenderer = lu.GetComponent<LineRenderer>();
				lineRenderer.SetColors(lu.atompointer1.GetComponent<Renderer>().material.GetColor("_Color"), lu.atompointer2.GetComponent<Renderer>().material.GetColor("_Color"));   
//				lu.oldatomcolor1 = lu.atompointer1.renderer.material.GetColor("_Color");
			}
			BallUpdate.bondsReadyToBeReset = false;
		}
	}
	
	private void AdjustWidths() {
		float width = LineUpdate.width;
		foreach(LineUpdate lu in lines) {
			lineRenderer = lu.GetComponent<LineRenderer>();
			lineRenderer.SetWidth(width, width);
		}
		LineUpdate.oldWidth = width;
	}
	
	public override void EnableRenderers() {
		foreach(LineUpdate lu in lines)
			lu.GetComponent<Renderer>().enabled = true;
		enabled = true;
	}
	
	public override void DisableRenderers() {
		foreach(LineUpdate lu in lines)
			lu.GetComponent<Renderer>().enabled = false;
		enabled = false;
	}
	
	
	// Update is called once per frame
	void Update () {
		if(UIData.bondtype != UIData.BondType.line)	{
			enabled = false;
			return;
		}
		if(BallUpdate.bondsReadyToBeReset)
			ResetColors();

		if(LineUpdate.width != LineUpdate.oldWidth)
			AdjustWidths();
	}
	
	public override void ResetPositions()	{
		lines = GameObject.FindObjectsOfType(typeof(LineUpdate)) as LineUpdate[];
		for (int i=0; i< lines.Length; i++) {
			lineRenderer = lines[i].GetComponent<LineRenderer>();
			lineRenderer.SetPosition(0, lines[i].atompointer1.transform.position);
			lineRenderer.SetPosition(1, lines[i].atompointer2.transform.position);
		}
	}
	
	public override void ResetMDDriverPositions() {
		
	}
}


/*
 			if((oldatomposition1!=atompointer1.transform.position)||(oldatomposition2!=atompointer2.transform.position))
			{
				lineRenderer.SetPosition(0, atompointer1.transform.position);
				lineRenderer.SetPosition(1, atompointer2.transform.position);
				oldatomposition1=atompointer1.transform.position;
				oldatomposition2=atompointer2.transform.position;
			}
*/