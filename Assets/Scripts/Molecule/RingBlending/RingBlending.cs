using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Molecule.Model;
using System;
using System.Runtime.InteropServices;


/// <summary>
/// this class is made to fill cycle inside a molecule with a semi-transparent polygon to have a effect 
/// of filling and locate more easylli aromatic residues or sugar.
/// 
/// </summary>
public class RingBlending
{
	
	public ArrayList listOfPolygon = new ArrayList();
	public int nbrMono;
	public bool showRingBlending;

	public List<float[]> coordinates = new List<float[]>();
	public float[] points = new float[3];

	public List<Mesh> meshList = new List<Mesh>();
	
	public static int index = 0;
	
	public List<GraphVertex> molAsGraph = new List<GraphVertex>(); //convert the molecule as a graph to after find cycle.
	public List<List<int>> cycles = new List<List<int>>(); // each element countains a list of atoms ID who compose a cycle.
	public List<string> residueslist = new List<string>(); // list of resname to get the color of each cycle

	public List<float[]> atomsLocationlist = new List<float[]>();


	public RingBlending(){
		atomsLocationlist = MoleculeModel.atomsLocationlist;

		//We initialize all vertex (an atom is a vertex here)
		for (int i=0; i<atomsLocationlist.Count; i++){
			GraphVertex tempVertex = new GraphVertex();
			tempVertex.coordinate=(new Vector3(atomsLocationlist[i][0] ,
			                                   atomsLocationlist[i][1],
			                                   atomsLocationlist[i][2]));
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
			if(molAsGraph[MoleculeModel.bondEPList[i][1]].type!='H'){
				if (MoleculeModel.bondEPList[i][1] < i+30){
				if ((MoleculeModel.bondEPList[i][0]==123) || (MoleculeModel.bondEPList[i][0]==129))
					Debug.Log ("connect : "+MoleculeModel.bondEPList[i][0]+" >>> "+MoleculeModel.bondEPList[i][1]); 
					molAsGraph[MoleculeModel.bondEPList[i][0]].neighbor.Add(molAsGraph[MoleculeModel.bondEPList[i][1]]);
					molAsGraph[MoleculeModel.bondEPList[i][1]].neighbor.Add(molAsGraph[MoleculeModel.bondEPList[i][0]]);
				}
			}
		}
		
		
		/* We search cycle
		 * It's a "search in graph" algorithm. we search inside each object (atoms here).
		 */
		for (int i=0; i<molAsGraph.Count; i++){
			List<int> indexCycle = new List<int>();
			molAsGraph[i].SearchCycle(indexCycle, i);
			
			
			//We reset all flag.
			for (int j=0; j<molAsGraph.Count; j++)
				molAsGraph[j].flag=false;
			
			if (indexCycle.Count>0){
				//We add vertex in the list wo will countain all vertex for all cycle
				if(!cycles.Any (x => x.OrderBy(y => y).SequenceEqual(indexCycle.OrderBy(z=>z)))){
					cycles.Add(indexCycle);
					residueslist.Add(molAsGraph[i].resname);
				} 
			}
		}


	}


	/// <summary>
	/// This function will convert the molecule to a "graph" (an object "GraphVertex" is made from each atoms
	/// and countains a list of all atoms neighbor). It's use to search and locate cycle inside a molecule.
	/// Once that we add the each neighbor, we searh cycle.
	/// Once we now where is the cycle, we draw polygon to fill them at the right position.
	/// </summary>
	public void CreateRingBlending(){



		/* we finaly draw all mesh, now that we detect all cycle in the molecule*/
		createAllMesh ();
	} /* End of CreateRingBlending*/


	/// <summary>
	/// this fonction will call the other functions to create all mesh and 
	/// also create gameobject.
	/// </summary>
	public void createAllMesh(){
	float x, y, z;

	for (int i=0; i< cycles.Count; i++) {
		for (int j=0; j<cycles[i].Count; j++) {
				x = atomsLocationlist [cycles [i] [j]] [0];//+ MoleculeModel.Offset.x;
				y = atomsLocationlist [cycles [i] [j]] [1];//+ MoleculeModel.Offset.y;
				z = atomsLocationlist [cycles [i] [j]] [2];//+ MoleculeModel.Offset.z;

			coordinates.Add (new float[3]{x,y,z});
		}
		string res = residueslist[i];
		Mesh[] twomesh =createAMeshPC (coordinates);
		//Mesh mesh = createAMeshPC (coordinates);
		//Mesh backMesh=returnBackMesh(mesh);
		Mesh mesh = twomesh[0];
		Mesh backMesh = twomesh[1];
		createPCObj (mesh, res);
		createPCObj (backMesh, res);
		
			
		coordinates.Clear ();
	}
} /* end for CeateAllMesh*/
	
	
	/// <summary>
	/// This function will create two mesh. Each mesh represent a pololygon which fill a residu cycle (ex: sugar or aromatic residues).
    /// it returns a list of 2 mesh because with the shader transparent/diffuse don't show two face of a mesh.
	/// So there is a mesh for one side and another mesh for the other side.
	/// </summary>
	/// <returns>The A mesh P.</returns>
	/// <param name="cxLocation"> list for atoms location..</param>
	public static Mesh[] createAMeshPC(List<float[]> cxLocation)
	{
		List<Color32> ColorList = new List<Color32>();
		List<Vector3> atomPositionList = new List<Vector3>(); //list of atoms position (x,y,z)
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<int> trianglesback = new List<int>();
		
		Mesh mymesh = new Mesh();
		Mesh backmesh = new Mesh();
		float xTot=0, yTot=0, zTot=0;
		
		Vector3 barycenter = new Vector3();
		//we save all coordinate and prepare barycenter calculation
		for (int i=0; i<cxLocation.Count; i++){

			atomPositionList.Add(new Vector3(cxLocation[i][0],cxLocation[i][1],cxLocation[i][2]));
			xTot=xTot+cxLocation[i][0];
			yTot=yTot+cxLocation[i][1];
			zTot=zTot+cxLocation[i][2];
		}
			//calculation of barycenter
		barycenter.x=xTot/cxLocation.Count;
		barycenter.y=yTot/cxLocation.Count;
		barycenter.z=zTot/cxLocation.Count;
	

	
		//we create all vertex
		for (int i=0; i < atomPositionList.Count; i++){
			vertices.Add(atomPositionList[i]);
			
			if(i==atomPositionList.Count-1){ //If it's the last triangles
				vertices.Add(barycenter); // we add the barycenter
				triangles.Add(i); //P0
				triangles.Add(0);  //P1
				triangles.Add(atomPositionList.Count); //Barycenter
				trianglesback.Add (atomPositionList.Count);
				trianglesback.Add(0);
				trianglesback.Add (i);
			}else{	
				triangles.Add(i); //P0
				triangles.Add(i+1);  //P1
				triangles.Add(atomPositionList.Count); //Barycenter
				trianglesback.Add (atomPositionList.Count);
				trianglesback.Add(i+1);
				trianglesback.Add(i);
			}
		}
		
		index=index+atomPositionList.Count;	
	
		mymesh.vertices=vertices.ToArray();
		mymesh.triangles=triangles.ToArray();
		mymesh.RecalculateNormals();
		mymesh.colors32=ColorList.ToArray();
		backmesh.vertices=vertices.ToArray();
		backmesh.triangles=trianglesback.ToArray();
		backmesh.RecalculateNormals();
		backmesh.colors32=ColorList.ToArray();
		
		Mesh[] TWOMESH= new Mesh[2]{mymesh, backmesh};
		return TWOMESH;
	} /* end of createAMeshPC */	


	
	/// <summary>
	/// Create an object "RingBlending" (polygon which fill a cycle).
	/// Also add a color (official residue color)
	/// and the shader.
	/// </summary>
	/// <param name="mesh"> the mesh (the polygon).</param>
	/// <param name="res">the residue name, to get the color.</param>
	public void createPCObj(Mesh mesh, string res){
		GameObject papobj = new GameObject("RingBlending");
		papobj.tag = "RingBlending";
		papobj.AddComponent<MeshFilter>();
		papobj.AddComponent<MeshRenderer>();
		papobj.GetComponent<MeshFilter>().mesh = mesh;
		papobj.GetComponent<Renderer>().material = new Material(Shader.Find("Transparent/Diffuse"));
		if (UI.GUIDisplay.colorByResiduesDict.ContainsKey(res))
			papobj.GetComponent<Renderer>().material.color=UI.GUIDisplay.colorByResiduesDict[res];
		else
			papobj.GetComponent<Renderer>().material.color=new Color(0.7f,0.7f,0.7f,0.5f);

	}  //end of createPCObj


	public void CreateOxygenSphere(int pos){
		GameObject oxysphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		oxysphere.tag = "OxySphere";
		Vector3 point = new Vector3(atomsLocationlist [pos] [0],atomsLocationlist [pos] [1],atomsLocationlist [pos] [2]);
		//		oxysphere.AddComponent<Animation>();
		oxysphere.transform.position = point;
		oxysphere.GetComponent<Renderer>().material.color = Color.red;
		oxysphere.transform.localScale = new Vector3(1, 1, 1);
		
	}
	
	public void ShowOxySphere(){
		
		for (int i=0; i<this.cycles.Count; i++) {
			for (int j=0; j<this.cycles[i].Count; j++) {
				if (MoleculeModel.atomsNamelist[cycles[i][j]][0] == 'O'){
					CreateOxygenSphere (cycles [i] [j]);
				}
			}
		}
	}




}