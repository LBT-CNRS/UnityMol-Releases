using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Molecule.Model;

public class SurfaceManager : MonoBehaviour {
	public static float brightness = 1f;
	public static float colorWeight = 0.5f;
	public static bool resetBrightness = false;
	public static bool resetColorWeight = false;
		
	private GameObject[] surfaceObjs;
	private AtomTree atomTree;
	private float bTime;
	
	// If true, a strictly correct algorithm will be used instead of the AtomTree
	// heuristics. I don't think there's much point in doing that, since AtomTree
	// seems to produce the same results in most cases.
	private static bool slowColoring = false;
	private static List<Vector3> atomLocations;
	private static List<Color> atomColors;
	private bool init = false;

	private float valmin = 0;
	private float valmax = 0;
	private string bftype = "";
	
	
	//private List<Vector3> locations;
	//private List<string> types;
	
	private void Init() {
		surfaceObjs = GameObject.FindGameObjectsWithTag("SurfaceManager");
		init = true;
	}
	
	public void InitTree() {
		surfaceObjs = GameObject.FindGameObjectsWithTag("SurfaceManager");
		Debug.Log ("Init tree");
		
		bTime = Time.realtimeSinceStartup;
		atomTree = AtomTree.Build();
		
		/*
		List<AtomModel> atomModels = MoleculeModel.atomsTypelist;
		List<float[]> atomLocations = MoleculeModel.atomsLocationlist;
		
		locations = new List<Vector3>();
		types = new List<string>();

		for(int i=0; i<atomModels.Count; i++) {
			string type = atomModels[i].type;
			Vector3 pos = new Vector3(atomLocations[i][0], atomLocations[i][1], atomLocations[i][2]);
			
			locations.Add(pos);
			types.Add(type);
		}
		*/
	}
	
	/*
	private string GetClosest(Vector3 pos) {
		float minDist = float.MaxValue;
		float dist;
		string type = "";
		for(int i=0; i<locations.Count; i++) {
			dist = Vector3.SqrMagnitude(pos - locations[i]);
			if (dist < minDist) {
				minDist = dist;
				type = types[i];
			}
		}
		return type;
	}
	
	private void ColorVerticesWithLists(Mesh mesh) {
		int nbVertices = mesh.vertices.Length;
		Vector3[] vertices = mesh.vertices;
		Color32[] meshColors = new Color32[nbVertices];
		string type;
		
		for(int i=0; i<nbVertices; i++) {
			type = GetClosest(vertices[i]);
			meshColors[i] = AtomModel.GetAtomColor(type);
		}	
		mesh.colors32 = meshColors;
	}
	*/	
	
	private void ColorVertices(Mesh mesh) {
		int nbVertices = mesh.vertices.Length;
		float valtype;
		List<float> BFactorList = MoleculeModel.BFactorList;
		Vector3[] vertices = mesh.vertices;
		Color32[] meshColors = new Color32[nbVertices];
		
		if(slowColoring) {
			List<float[]> atomLocs = MoleculeModel.atomsLocationlist;
			atomLocations = new List<Vector3>();
			for(int i=0; i<atomLocs.Count; i++) {
				atomLocations.Add(new Vector3(atomLocs[i][0], atomLocs[i][1], atomLocs[i][2]));
			}
			atomColors = MoleculeModel.atomsColorList;
		}
		
		string type;
		for(int i=0; i<nbVertices; i++) {
			//if(UI.UIData.atomtype == UI.UIData.AtomType.particleball){
				type = atomTree.GetClosestAtomType(vertices[i]);
				if(UI.UIData.surfColChain){
					Ribbons.InitCol();
					meshColors[i] = Ribbons.GetColorChain(type);
				}
				else if(UI.UIData.surfColHydroKD){
					HydrophobicScales.InitKyteDoo();
					meshColors[i] = HydrophobicScales.GetColorHydro(type);
				}
				else if(UI.UIData.surfColHydroEng){
					HydrophobicScales.InitEngleman();
					meshColors[i] = HydrophobicScales.GetColorHydro(type);
				}

				else if(UI.UIData.surfColHydroEis){
					HydrophobicScales.InitEisenberg();
					meshColors[i] = HydrophobicScales.GetColorHydro(type);
				}
				else if(UI.UIData.surfColPChim){
					HydrophobicScales.InitPhysChim();
					meshColors[i] = HydrophobicScales.GetColorHydro(type);
				}
				else if(UI.UIData.surfColHydroWO){
					HydrophobicScales.InitWhiteOct();
					meshColors[i] = HydrophobicScales.GetColorHydro(type);
				}
				else if(UI.UIData.surfColBF){
					valtype = float.Parse (type);
					if(valmax == 0){
						valmin = BFactorRep.GetMin(BFactorList);
						valmax = BFactorRep.GetMax(BFactorList);
					}
					valtype = (valtype - valmin) / (valmax -valmin);
					bftype = BFactorRep.GetBFStyle(valtype);
					meshColors[i] = AtomModel.GetModel (bftype).baseColor;
				}
				else
					meshColors[i] = MoleculeModel.GetAtomColor(type);

			//This part of the code wasn't working
			//Anyway i dunno why we want to use another way to color surfaces when not in particles mode
			//}
			/*else
				if(slowColoring){
					meshColors[i] = GetClosestAtomColor(vertices[i]);
				}else{
					type = atomTree.GetClosestAtomType(vertices[i]);
					meshColors[i] = MoleculeModel.GetAtomColor(type);
					//meshColors[i] = atomTree.GetClosestAtomColor(vertices[i]);
			}*/
		}
		mesh.colors32 = meshColors;
	}
	
	private Color GetClosestAtomColor(Vector3 pos) {
		float minDist = float.MaxValue;
		Color minColor = Color.magenta;
		float dist;
		for(int i=0; i<atomLocations.Count; i++) {
			dist = Vector3.SqrMagnitude(pos - atomLocations[i]);
			Debug.Log ("dist " + dist);
			if(dist < minDist) {
				minDist = dist;
				minColor = atomColors[i];
			}
		}
		return minColor;
	}
	
	public void ColorVertices() {
		//float bTime = Time.realtimeSinceStartup;
		foreach(GameObject surfaceObj in surfaceObjs) {
			ColorVertices(surfaceObj.GetComponent<MeshFilter>().mesh);
			//ColorVerticesWithLists(surfaceObj.GetComponent<MeshFilter>().mesh);
			//surfaceObj.renderer.material = new Material(Shader.Find("Vertex Colored"));
		}
		float elapsed = 100f * (Time.realtimeSinceStartup - bTime);
		Debug.Log("SurfaceManager::Update Atom Color total processing time: " + elapsed.ToString());
	}
	
	private void ResetBrightness() {
		if(!init)
			Init();
		
		for(int i=0; i<surfaceObjs.Length; i++)
			surfaceObjs[i].GetComponent<Renderer>().material.SetFloat("_Brightness", brightness);
		
		resetBrightness = false;
	}
	
	private void ResetColorWeight() {
		if(!init)
			Init ();
		
		for(int i=0; i<surfaceObjs.Length; i++)
			surfaceObjs[i].GetComponent<Renderer>().material.SetFloat("_ColorWeight", colorWeight);
		
		resetColorWeight = false;
	}
	
	void Update() {
		if(resetBrightness)
			ResetBrightness();
		
		if(resetColorWeight)
			ResetColorWeight();
	}
}
