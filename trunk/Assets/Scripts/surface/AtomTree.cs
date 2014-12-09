using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Molecule.Model;

public class AtomTree {
	private List<string> types;
	private List<Vector3> positions;
	private List<Color> colors;
	private Vector3 bound0, bound1, split;
	private AtomTree[] children;
	private bool isLeaf = true;
	private static int MAX_ATOMS = 8;
	//private static Vector3 candidatePos;
	private static Color candidateCol;
	private static float candidateDist;
	
	public AtomTree(Vector3 b0, Vector3 b1) {
		split = 0.5f * (b0 + b1); // Middle of the cube
		bound0 = b0;
		bound1 = b1;
		positions = new List<Vector3>();
		types = new List<string>();
		colors = new List<Color>();
		children = new AtomTree[8];
	}
	
	private int GetChildIndex(Vector3 p) {
		int childIndex = 0;
		
		if(p.x > split.x)
			childIndex |= 1;
		if(p.y > split.y)
			childIndex |= 2;
		if(p.z > split.z)
			childIndex |= 4;
		
		return childIndex;
	}
	
	private Vector3 GetOffset(Vector3 b0, Vector3 b1, int i) {
		Vector3 result;
		switch(i) {
		case 0:
			result = Vector3.zero;
			break;
		case 1:
			result = new Vector3(b1.x,	0,		0);
			break;
		case 2:
			result = new Vector3(0,		b1.y,	0);
			break;
		case 3:
			result = new Vector3(b1.x,	b1.y,	0);
			break;
		case 4:
			result = new Vector3(0,		0,		b1.z);
			break;
		case 5:
			result = new Vector3(b1.x,	0,		b1.z);
			break;
		case 6:
			result = new Vector3(0,		b1.y,	b1.z);
			break;
		case 7:
			result = b1;
			break;
		default:
			Debug.Log("VertexTree::Offset() > Something is very, very wrong here.");
			result = Vector3.zero;
			break;
		}
		return 0.5f * result;
	}
	
	private void AddAtomToLeaf(Vector3 pos, string type, Color col) {
		positions.Add(pos);
		types.Add(type);
		colors.Add(col);
		
		// If the addition brings the cube to the limit of atoms
		if(positions.Count >= MAX_ATOMS)
			Subdivide();
	}
	
	private void Subdivide() {
		Vector3 oppositeBound = bound1 - bound0;
		Vector3 offset;
		
		// Creating the sub-cubes.
		for(int i=0; i<8; i++) {
			offset = GetOffset(bound0, oppositeBound, i);
			children[i] = new AtomTree(bound0+offset, split+offset);
		}
		
		// Sending the atoms (and corresponding types) to the correct sub-cubes.
		int childIndex;
		for(int i=0; i<MAX_ATOMS; i++) {
			childIndex = GetChildIndex(positions[i]);			
			children[childIndex].AddAtomToLeaf(positions[i], types[i], colors[i]);
		}
		
		// Now this cube is not a leaf anymore, but a node.
		// It must be cleared, and no more atoms should be added to it.
		positions.Clear();
		types.Clear();
		colors.Clear();
		
		isLeaf = false;
	}
	
	private void AddAtom(Vector3 pos, string type, Color col) {
		if(isLeaf) {
			positions.Add(pos);
			types.Add(type);
			colors.Add(col);
			
			// If the addition brings the cube to the limit of atoms
			if(positions.Count >= MAX_ATOMS)
				Subdivide();
			return;
		}
		
		// If the function has not returned yet, then this is a node, not a leaf.
		// So we just find the correct sub-cube for the recursive call.
		int childIndex = GetChildIndex(pos);
		children[childIndex].AddAtom(pos, type, col);
	}
	
	private bool IsEmpty() {
		return(isLeaf && positions.Count == 0);
	}
	
	private AtomTree GetOptimalChild(Vector3 pos) {
		AtomTree optimal = null;
		float minDist = float.MaxValue;
		float dist;
		foreach(AtomTree child in children) {
			if(!child.IsEmpty()) {
				dist = Vector3.SqrMagnitude(child.split - pos);
				//dist = Vector3.Distance(child.split, pos);
				if(dist < minDist) {
					minDist = dist;
					optimal = child;
				}
			}
		}
		return optimal;		
	}
	
	public string GetClosestAtomType(Vector3 pos) {
		string type=""; // it should not remain empty
		if(isLeaf) {
			float lowestDistance = float.MaxValue;
			float dist;
			for(int	i=0; i<positions.Count; i++) {
				dist = Vector3.SqrMagnitude(pos - positions[i]);
				//dist = Vector3.Distance(pos, positions[i]);
				if(dist < lowestDistance) {
					lowestDistance = dist;
					type = types[i];
				}
			}
			return type;
		}
		
		// If the function has not returned yet, then this is a node, not a leaf.
		// So we just find the correct sub-cube for the recursive call.
		int childIndex = GetChildIndex(pos);
		type = children[childIndex].GetClosestAtomType(pos);
		if (type != "")
			return type;
		else
			return GetOptimalChild(pos).GetClosestAtomType(pos);
	}
	
	public Color GetClosestAtomColor(Vector3 pos) {
		candidateCol = Color.magenta;
		//		candidatePos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		candidateDist = float.MaxValue;
		SubGetClosestAtomColor(pos);
		return candidateCol;
		//return SubGetClosestAtomColor(pos);
	}
	
	public void SubGetClosestAtomColor(Vector3 pos) {
		Debug.Log ("isLeaf " + isLeaf);
		Debug.Log ("Taille colors " + colors.Count);
		Debug.Log ("Positions " + positions.Count);
		if(isLeaf) {
			float dist;
			for(int	i=0; i<positions.Count; i++) {
				dist = Vector3.SqrMagnitude(pos - positions[i]);
				if(dist < candidateDist) {
					candidateDist = dist;
					candidateCol = colors[i];
					//	candidatePos = positions[i];
				}
			}
		}
		return;
		
		// If the function has not returned yet, then this is a node, not a leaf.
		// So we just find the correct sub-cube for the recursive call.
		int childIndex = GetChildIndex(pos);
		AtomTree optChild = children[childIndex];
		if(optChild.isLeaf) {
			foreach(AtomTree child in children) {
				child.SubGetClosestAtomColor(pos);
			}
		} else {
			optChild.SubGetClosestAtomColor(pos);
		}
	}
	
	public void Print() {
		if(isLeaf) {
			for(int i=0; i<positions.Count; i++) {
				Debug.Log("Type: " + types[i] + " and position: " + positions[i].ToString());
			}
			return;
		}
		
		// If the function has not returned yet, then this is a node, not a leaf.
		// So we just call the function on ALL children.
		foreach(AtomTree atomTree in children)
			if(atomTree != null)
				atomTree.Print();
	}

	public static AtomTree Build() {
		//List<string> typeList = new List<string>();
		//List<Vector3> posList = new List<Vector3>();
		AtomTree atomTree = new AtomTree(MoleculeModel.MinValue, MoleculeModel.MaxValue);
		List<AtomModel> atomModels = MoleculeModel.atomsTypelist;
		List<string> atomChain = MoleculeModel.atomsChainList;
		List<float[]> atomLocations = MoleculeModel.atomsLocationlist;
		List<string> atomResname = MoleculeModel.atomsResnamelist;
		List<float> BfactorList = MoleculeModel.BFactorList;
		List<int> atomsNumberList = MoleculeModel.atomsNumberList;
		int nbres = 0;
		string type;
		
		System.Diagnostics.Debug.Assert(atomModels.Count == atomLocations.Count);
		//System.Diagnostics.Debug.Assert(atomChain.Count == atomLocations.Count);
		for(int i=0; i<atomModels.Count; i++) {
			//for(int i=0; i<atomChain.Count; i++) {
			//string type = atomModels[i].type;
			if(UI.UIData.surfColChain && !UI.UIData.surfColHydroKD){
				if(i > 0 && atomResname[i] != atomResname[i-1])
					nbres++;
				if(i>0 && atomChain[i] != atomChain[i-1])
					nbres = 1;
				// Coloration by domains (only for GLIC)
				if(nbres > 182 && UI.UIData.isGLIC)
					type = atomChain[i] + "L";
				else
					type = atomChain[i];
			}
			else if((UI.UIData.surfColHydroKD || UI.UIData.surfColPChim || UI.UIData.surfColHydroEng || UI.UIData.surfColHydroEis || UI.UIData.surfColHydroWO) && !UI.UIData.surfColChain)
				type = atomResname[i];
			else if (UI.UIData.surfColBF)
				type = BfactorList[i].ToString();
			else if (UI.UIData.spread_tree) // For Spreading when camera near the structure
				type = atomsNumberList[i].ToString();
			else
				type = atomModels[i].type;
			Vector3 pos = new Vector3(atomLocations[i][0], atomLocations[i][1], atomLocations[i][2]);
			//typeList.Add(type);
			//posList.Add(pos);
			Color col = MoleculeModel.atomsColorList[i];
			atomTree.AddAtom(pos, type, col);
		}
		return atomTree;
	}
	
}
