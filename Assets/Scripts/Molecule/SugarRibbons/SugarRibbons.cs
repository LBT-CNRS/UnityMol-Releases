using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Molecule.Model;


	public class SugarRibbons 
	{


	public List<Mesh> CycleMesh = new List<Mesh>();
	public List<Mesh> CycleBIGMesh = new List<Mesh>();


	public List<Mesh> meshesRingUp =new List<Mesh>();
	public List<Mesh> meshesRingDown = new List<Mesh>();
	public List<Mesh> meshesSideRing = new List<Mesh> ();
	
	public List<Mesh> meshesBIGRingUp =new List<Mesh>();
	public List<Mesh> meshesBIGRingDown = new List<Mesh>();
	public List<Mesh> meshesBIGSideRing = new List<Mesh> ();


	public List<Mesh> meshesBondUp  = new List<Mesh>();
	public List<Mesh> meshesBondDown  = new List<Mesh>();
	public List<Mesh> meshesBondSide  = new List<Mesh>();


	public List<GraphVertex> molAsGraph = new List<GraphVertex>(); //convert the molecule as a graph to after find cycle.
	public List<List<int>> cycles = new List<List<int>>(); // each element countains a list of atoms ID who compose a cycle.
	public List<string> residueslist = new List<string>(); // list of resname to get the color of each cycle
	public List<int> residuesNumberlist = new List<int>(); // list of resname to get the color of each cycle

	public List<Vector3> barylist = new List<Vector3>(); // list of all barycenters (for each cycle)


	private Vector3 LinearComb(float scalar0, Vector3 vector0, float scalar1, Vector3 vector1) {
		return scalar0*vector0 + scalar1*vector1;
	}

	public List<Vector3> vertices = new List<Vector3>();
	public List<Color32> colors = new List<Color32>();
	public List<Vector3> normals = new List<Vector3>();
	public List<int> triangles = new List<int>();

	public  float THICKNESS;
	public  float THICKNESS_LITTLE_MESH;
	public  float THICKNESS_BIG_MESH;
	public float THICKNESS_BOND_6_C1_C4;
	public float THICKNESS_BOND_6_OTHER;
	public float THICKNESS_BOND_5;
	public float LIGHTER_COLOR_FACTOR_RING;
	public float LIGHTER_COLOR_FACTOR_BOND;
	public float OXYSPHERESIZE;
	public bool SUGAR_ONLY;
	public int COLOR_MODE_RING;
	public int COLOR_MODE_BOND;
	public int SUGAR=0;
	public int CHAIN=1;
	public int PICKER=2;
	public ColorObject BONDCOLOR;
	public ColorObject RINGCOLOR;
	public ColorObject OXYSPHERECOLOR;

	public static float[] wideness = {1f, 2f, 1f};

	public int nbRes;
	public List<string> sugarResname = new List<string>();


	private Vector3 float3toVector3(float[] point){
		Vector3 returnVector = new Vector3(point[0], point[1], point[2]);
		return returnVector;
	}


	public List<float[]> atomsLocationlist = new List<float[]>();
	public List<string> atomsResnamelist = new List<string>();
	public List<string> atomsNamelist = new List<string>();
	public List<string> resChainList = new List<string>();
	public List<int[]> bondEPList = new List<int[]>();

	public Dictionary <int,List<int>> connectivityList = new Dictionary<int, List<int>>();
	public Dictionary <int, Dictionary<int, int[]> > MeshIndexForAtom = new Dictionary<int, Dictionary<int, int[]>>();
	public Dictionary <int, string> res_for_bond = new Dictionary<int, string>(); 


	public SugarRibbons(bool SugarOnly){
		if (SugarOnly){
			atomsLocationlist = MoleculeModel.atomsSugarLocationlist;
			atomsResnamelist = MoleculeModel.atomsSugarResnamelist;
			atomsNamelist = MoleculeModel.atomsSugarNamelist;
			bondEPList = MoleculeModel.bondEPSugarList;
			resChainList = MoleculeModel.resSugarChainList;


		}else{
			atomsLocationlist = MoleculeModel.atomsLocationlist;
			atomsResnamelist = MoleculeModel.atomsResnamelist;
			atomsNamelist = MoleculeModel.atomsNamelist;
			bondEPList = MoleculeModel.bondEPList;
			resChainList = MoleculeModel.resChainList;
		}
		MeshIndexForAtom[6]=new Dictionary<int, int[]> () ;
		MeshIndexForAtom[6][0] = new int[2]{1,2};
		MeshIndexForAtom[6][1] = new int[2]{3,4};
		MeshIndexForAtom[6][2] = new int[2]{5,6};
		MeshIndexForAtom[6][3] = new int[2]{7,8};
		MeshIndexForAtom[6][4] = new int[2]{9,10};
		MeshIndexForAtom[6][5] = new int[2]{11,0};

		MeshIndexForAtom[5]=new Dictionary<int, int[]> () ;
		MeshIndexForAtom[5][0]=new int[2]{0,1};
		MeshIndexForAtom[5][1]=new int[2]{2,3};
		MeshIndexForAtom[5][2]=new int[2]{4,5};
		MeshIndexForAtom[5][3]=new int[2]{6,7};
		MeshIndexForAtom[5][4]=new int[2]{8,9};




		for (int i=0; i<atomsLocationlist.Count; i++){

				GraphVertex tempVertex = new GraphVertex();
				tempVertex.coordinate=(new Vector3(atomsLocationlist[i][0],
				                                   atomsLocationlist[i][1],
				                                   atomsLocationlist[i][2]));

				tempVertex.resname=atomsResnamelist[i];
				tempVertex.type=atomsNamelist[i][0];
				tempVertex.id=i;
				molAsGraph.Add(tempVertex);
		}

		/* We connect all dots
		 * But we don't need to calculate over 30 atoms after the atom 'i'
		 * and an hydrogen can't be a part of a cycle because an hydrogen can only have 1 bond.
		 */
		for (int i=0; i < bondEPList.Count; i++){
			//Debug.Log("0= "+MoleculeModel.bondList[i][0]+" 1="+MoleculeModel.bondList[i][1]);

		//	if(molAsGraph[bondEPList[i][1]].type!='H'){
		//		if (bondEPList[i][1] < i+30){

					molAsGraph[bondEPList[i][0]].neighbor.Add(molAsGraph[bondEPList[i][1]]);
					molAsGraph[bondEPList[i][1]].neighbor.Add(molAsGraph[bondEPList[i][0]]);
		//		}
		//	}
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

		for (int i=0; i<cycles.Count; i++){
			for (int j=0; j<cycles[i].Count; j++){
				molAsGraph[cycles[i][j]].idRing = i;
			}
		}

		//We reset all flag.

		for (int i=0; i<molAsGraph.Count; i++){
			List<int> trashlish = new List<int>();
			molAsGraph[i].SearchConnection(connectivityList,trashlish);

			for (int j=0; j<molAsGraph.Count; j++)
				molAsGraph[j].flag=false;
		}

		CalculBarycenter();

	}



	public void createSugarRibs(float RibbonsThickness, bool sugarOnly, float thickness_Little, float thickness_BIG,
	                            float thickness_bond_6_C1_C4, float thickness_6_other, float thickness_bond_5, 
	                            float lighter_color_factor_ring, float lighter_color_factor_bond, int color_mode_ring, int color_mode_bond,
	                            ColorObject bondcolor, ColorObject ringcolor, float OxySphereSize, ColorObject OxySphereColor){
		sugarResname.AddMany("ABE","ACE","ALT","API","ARA","DHA","FRU","FUC","GAL","GLC","GUL","IDO","DKN","KDO","MAN","NEG","RHA","RIB","SIA","TAG","TAL","XYL");
		SUGAR_ONLY=sugarOnly;
		THICKNESS_LITTLE_MESH=thickness_Little;
		THICKNESS_BIG_MESH=thickness_BIG;
		THICKNESS_BOND_6_C1_C4=thickness_bond_6_C1_C4;
		THICKNESS_BOND_6_OTHER=thickness_6_other;
		THICKNESS_BOND_5=thickness_bond_5;
		LIGHTER_COLOR_FACTOR_RING = lighter_color_factor_ring;
		LIGHTER_COLOR_FACTOR_BOND = lighter_color_factor_bond;
		COLOR_MODE_RING = color_mode_ring;
		COLOR_MODE_BOND = color_mode_bond;
		BONDCOLOR = bondcolor;
		RINGCOLOR = ringcolor;
		OXYSPHERESIZE = OxySphereSize;
		OXYSPHERECOLOR = OxySphereColor;
		THICKNESS = RibbonsThickness;
		//We initialize all vertex (an atom is a vertex here)


		createSugarRibbons();
		/* we finaly draw all mesh, now that we detect all cycle in the molecule*/
	} /* End of CreateTwister*/


	public void changeColor(int which){
		/* which=0 -> SUGAR RING
		 * which=1 -> BOND
		 * which=2 -> OXYGEN SPHERE
		 */

		switch (which){
		case 0:
			//for (int i=0; i<meshesRingUp.Count; i++){
				//meshesRingUp[i].colors32 = Color.red;
			//}
			break;

		case 1:
			break;

		case 2:
			break;
		}
	}



	private void generateCycleMesh2(){

		string resname = "";
		for (int i=0; i<cycles.Count; i++){
			Mesh sugarMesh = new Mesh ();
			List<int> trianglesSM = new List<int> ();
			List<Vector3> verticesSM = new List<Vector3> ();
			List<Color32> colorsSM = new List<Color32> ();
			resname=residueslist[i];


			for (int j=0; j<cycles[i].Count; j++){
				verticesSM.Add(CycleAtomToVector3(i,j));

				if (UI.GUIDisplay.colorByResiduesDict.ContainsKey(resname))
					colorsSM.Add(UI.GUIDisplay.colorByResiduesDict[resname]);
				else
					colorsSM.Add(new Color(0.7f,0.7f,0.7f,0.5f));
			}

			int size = cycles[i].Count;
			int nbiter = (size-2)/2;
			int reste = (size-2)%2;

			for (int j=0; j<nbiter; j++){
				trianglesSM.AddMany(j,size-1-j,(size-1)-(j+1));
				trianglesSM.AddMany(j,(size-1)-(j+1),j+1);
				if ((reste==1) && (j==nbiter-1)){
					trianglesSM.AddMany(j+1,(size-1)-(j+1),j+2);
				}
			}

			

			sugarMesh.vertices = verticesSM.ToArray();
			sugarMesh.triangles = trianglesSM.ToArray();
			sugarMesh.colors32 = colorsSM.ToArray();

			CycleMesh.Add(sugarMesh);

		}
	}

	public Vector3 calcMidPoint(Vector3 a, Vector3 b){
		return ((a+b)/2);
	}

	public List<Vector3> enlargeMesh(int i, float scaling){
		List<Vector3> verticesEn = new List<Vector3>();
		Vector3 vectDir; 
		Vector3 midpoint;

		if (cycles[i].Count == 6){
			vectDir = CycleAtomToVector3(i,0) - CycleAtomToVector3(i,4);
			vectDir.Normalize();
			verticesEn.Add(CycleAtomToVector3(i,0) +vectDir * (scaling-0.1f));

			vectDir = CycleAtomToVector3(i,1) - CycleAtomToVector3(i,3);
			vectDir.Normalize();
			verticesEn.Add(CycleAtomToVector3(i,1) +vectDir * (scaling-0.1f));

			midpoint = calcMidPoint(CycleAtomToVector3(i,1), CycleAtomToVector3(i,3));
			vectDir = CycleAtomToVector3(i,2) - midpoint;
			vectDir.Normalize();
			verticesEn.Add(CycleAtomToVector3(i,2) +vectDir * (scaling+0.3f));

			vectDir = CycleAtomToVector3(i,3) - CycleAtomToVector3(i,1);
			vectDir.Normalize();
			verticesEn.Add(CycleAtomToVector3(i,3) +vectDir * (scaling-0.1f));

			vectDir = CycleAtomToVector3(i,4) - CycleAtomToVector3(i,0);
			vectDir.Normalize();
			verticesEn.Add(CycleAtomToVector3(i,4) +vectDir * (scaling-0.1f));

			midpoint = calcMidPoint(CycleAtomToVector3(i,0), CycleAtomToVector3(i,4));
			vectDir = CycleAtomToVector3(i,5) - midpoint;
			vectDir.Normalize();
			verticesEn.Add(CycleAtomToVector3(i,5) + vectDir * (scaling+0.3f));


		}else if (cycles[i].Count == 5){
			midpoint = calcMidPoint(CycleAtomToVector3(i,4), CycleAtomToVector3(i,1));
			vectDir = CycleAtomToVector3(i,0) - barylist[i];
			vectDir.Normalize();
			verticesEn.Add (CycleAtomToVector3(i,0) + vectDir * (scaling));
			
			midpoint = calcMidPoint(CycleAtomToVector3(i,0), CycleAtomToVector3(i,2));
			vectDir = CycleAtomToVector3(i,1) - barylist[i];
			vectDir.Normalize();
			verticesEn.Add (CycleAtomToVector3(i,1) + vectDir * (scaling));
			
			midpoint = calcMidPoint(CycleAtomToVector3(i,1), CycleAtomToVector3(i,3));
			vectDir = CycleAtomToVector3(i,2) - barylist[i];
			vectDir.Normalize();
			verticesEn.Add (CycleAtomToVector3(i,2) + vectDir * (scaling));
			
			midpoint = calcMidPoint(CycleAtomToVector3(i,2), CycleAtomToVector3(i,4));
			vectDir = CycleAtomToVector3(i,3) - barylist[i];
			vectDir.Normalize();
			verticesEn.Add (CycleAtomToVector3(i,3) + vectDir * (scaling));
			
			midpoint = calcMidPoint(CycleAtomToVector3(i,3), CycleAtomToVector3(i,0));
			vectDir = CycleAtomToVector3(i,4) - barylist[i];
			vectDir.Normalize();
			verticesEn.Add (CycleAtomToVector3(i,4) + vectDir * (scaling));
			


		}else{
			Debug.Log("WARNING ==> RING WITH "+cycles[i].Count+" :: REPRESENTATON NOT IMPLEMENTED");
		}

		return verticesEn;
	}


	private List<Vector3> makeFlatVertex( List<Vector3> verticeEn){

		List<Vector3> newVertices = new List<Vector3>();

		if (verticeEn.Count == 6){
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[0], verticeEn[5], 1-THICKNESS_BOND_6_C1_C4));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[0], verticeEn[5], THICKNESS_BOND_6_OTHER));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[0], verticeEn[1], THICKNESS_BOND_6_OTHER));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[1], verticeEn[0], THICKNESS_BOND_6_OTHER));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[1], verticeEn[2], THICKNESS_BOND_6_OTHER));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[1], verticeEn[2], 1-THICKNESS_BOND_6_C1_C4));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[3], verticeEn[2], 1-THICKNESS_BOND_6_C1_C4));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[3], verticeEn[2], THICKNESS_BOND_6_OTHER));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[3], verticeEn[4], THICKNESS_BOND_6_OTHER));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[4], verticeEn[3], THICKNESS_BOND_6_OTHER));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[4], verticeEn[5], THICKNESS_BOND_6_OTHER));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[4], verticeEn[5], 1-THICKNESS_BOND_6_C1_C4));

		}else{
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[0], verticeEn[4],THICKNESS_BOND_5));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[0], verticeEn[1],THICKNESS_BOND_5));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[1], verticeEn[0],THICKNESS_BOND_5));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[1], verticeEn[2],THICKNESS_BOND_5));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[2], verticeEn[1],THICKNESS_BOND_5));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[2], verticeEn[3],THICKNESS_BOND_5));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[3], verticeEn[2],THICKNESS_BOND_5));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[3], verticeEn[4],THICKNESS_BOND_5));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[4], verticeEn[3],THICKNESS_BOND_5));
			newVertices.Add(calcullittleRibbonsControlPoint(verticeEn[4], verticeEn[0],THICKNESS_BOND_5));
		}

		return newVertices;


	}


	private void generateBiggerCycleMesh(float scaling=0.60f){
		string resname = "";
		string chain = "";
		for (int i=0; i<cycles.Count; i++){
			Mesh sugarMesh = new Mesh ();
			List<int> trianglesSM = new List<int> ();
			List<Vector3> verticesSM = new List<Vector3> ();
			List<Color32> colorsSM = new List<Color32> ();
			resname=residueslist[i];	
			if (MoleculeModel.resChainList.Count>0)
				chain=MoleculeModel.resChainList[i];

			verticesSM = enlargeMesh(i, scaling);
			verticesSM = makeFlatVertex(verticesSM);
			for (int j=0; j<verticesSM.Count; j++){
				if (COLOR_MODE_RING == SUGAR){
					if (UI.GUIDisplay.colorByResiduesDict.ContainsKey(resname))
						colorsSM.Add(lightColor(UI.GUIDisplay.colorByResiduesDict[resname], LIGHTER_COLOR_FACTOR_RING));
					else
						colorsSM.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_RING));
				}else if (COLOR_MODE_RING == CHAIN){
					if (UI.GUIDisplay.ChainColorDict.ContainsKey(chain))
						colorsSM.Add(lightColor(UI.GUIDisplay.ChainColorDict[chain], LIGHTER_COLOR_FACTOR_RING));
					else
						colorsSM.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_RING));
				}else if (COLOR_MODE_RING == PICKER){
					colorsSM.Add(lightColor(RINGCOLOR.color, LIGHTER_COLOR_FACTOR_RING));
				}
			}


			int size = verticesSM.Count;
			int nbiter = (size-2)/2;
			int reste = (size-2)%2;


			for (int j=0; j<nbiter; j++){
				trianglesSM.AddMany(j,size-1-j,(size-1)-(j+1));
				trianglesSM.AddMany(j,(size-1)-(j+1),j+1);
				if ((reste==1) && (j==nbiter-1)){
					trianglesSM.AddMany(j+1,(size-1)-(j+1),j+2);
				}
			}
			
			
			
			sugarMesh.vertices = verticesSM.ToArray();
			sugarMesh.triangles = trianglesSM.ToArray();

			sugarMesh.colors32 = colorsSM.ToArray();
			
			CycleBIGMesh.Add(sugarMesh);
			
		}
	}









	private void generateRibbonsMesh(Mesh centralMesh, int resnumber, float thicknessCenter=1f, bool center=true){
		Mesh meshUP = new Mesh();
		Mesh meshDOWN = new Mesh();
		Mesh meshSide = new Mesh ();
		List<Vector3> verticesSide = new List<Vector3>();
		List<Color32> sideColors = new List<Color32> ();


		/*RING MESH*/
		Vector3 normalMean = calculMeanNormalMesh(centralMesh);
		Vector3[] newVerticesUP = new Vector3[centralMesh.vertices.Length];
		Vector3[] newVerticesDOWN = new Vector3[centralMesh.vertices.Length];
		List<Color32> colors = new List<Color32>();
		string resname = residueslist[resnumber];



		for (int j=0; j<centralMesh.vertices.Length; j++){
			newVerticesUP[j].x = centralMesh.vertices[j].x + (normalMean.x*(THICKNESS*thicknessCenter));
			newVerticesUP[j].y = centralMesh.vertices[j].y + (normalMean.y)*(THICKNESS*thicknessCenter);
			newVerticesUP[j].z = centralMesh.vertices[j].z + (normalMean.z)*(THICKNESS*thicknessCenter);

			newVerticesDOWN[j].x = centralMesh.vertices[j].x - (normalMean.x*(THICKNESS*thicknessCenter));
			newVerticesDOWN[j].y = centralMesh.vertices[j].y - (normalMean.y)*(THICKNESS*thicknessCenter);
			newVerticesDOWN[j].z = centralMesh.vertices[j].z - (normalMean.z)*(THICKNESS*thicknessCenter);
		}

		//COLORS SET
		for (int j=0; j<centralMesh.vertices.Length; j++){
			if (UI.GUIDisplay.colorByResiduesDict.ContainsKey(resname))
				colors.Add(UI.GUIDisplay.colorByResiduesDict[resname]);
			else
				colors.Add(new Color(0.7f,0.7f,0.7f,0.5f));
		}

		meshUP.vertices = newVerticesUP;
		meshUP.triangles = centralMesh.triangles;
		meshUP.RecalculateNormals();
		meshUP.colors32 = centralMesh.colors32;



		meshDOWN.vertices = newVerticesDOWN;
		meshDOWN.triangles = invertTrianglesMesh(centralMesh);
		meshDOWN.RecalculateNormals();
		meshDOWN.colors32 = centralMesh.colors32;

		/*SIDE MESH*/

	

		//SIDE VERTICES

		for (int i=0; i<meshUP.vertices.Length; i++){
			verticesSide.AddMany(meshUP.vertices[i],meshDOWN.vertices[i]);
			sideColors.AddMany(meshUP.colors32[i], meshDOWN.colors32[i]);
		}

		meshSide.vertices = verticesSide.ToArray ();
		meshSide.triangles = generateTriangleSide (meshSide);
		meshSide.RecalculateNormals ();
		meshSide.colors32 = sideColors.ToArray();


		if (center){
			meshesRingUp.Add (meshUP);
			meshesRingDown.Add (meshDOWN);
			meshesSideRing.Add (meshSide);
		}else{
			meshesBIGRingUp.Add (meshUP);
			meshesBIGRingDown.Add (meshDOWN);
			meshesBIGSideRing.Add (meshSide);
		}
	}


	private void create_Bond(){
		int key_index_for_bond_dict=-1;
		foreach (int i in connectivityList.Keys){
			string resname1 = residueslist[i];
			string chain1 = MoleculeModel.resChainList[i];
			for (int j=0; j<connectivityList[i].Count / 3; j++){
				key_index_for_bond_dict++;
				List<Vector3> verticesUP = new List<Vector3>();
				List<Vector3> verticesDown = new List<Vector3>();
				List<Vector3> VerticesSide = new List<Vector3>();
				
				List<int> TrianglesUP = new List<int>();
				List<int> TrianglesDown = new List<int>();
				List<int> TrianglesSide = new List<int>();
				
				
				List<Color32> colorsUP = new List<Color32>();
				List<Color32> colorsDown = new List<Color32>();
				List<Color32> colorsSide = new List<Color32>();



				Mesh BondUP = new Mesh();
				Mesh BondDown = new Mesh();
				Mesh bondSide = new Mesh();

				Vector3 Au,Bu,Cu,Du, Ad,Bd, Cd, Dd;

				int atom1 = findRingAtomIndex(i, connectivityList[i][0+(3*j)]);
				int atom2 = findRingAtomIndex(connectivityList[i][1+(3*j)], connectivityList[i][2+(3*j)]);
				int res2 = connectivityList[i][1+(3*j)];
				int size1 = cycles[i].Count;
				int size2 = cycles[res2].Count;
				string resname2 = residueslist[res2];
				string chain2 = MoleculeModel.resChainList[res2];


				Au=meshesBIGRingUp[i].vertices[MeshIndexForAtom[size1][atom1][0]];
				Bu=meshesBIGRingUp[i].vertices[MeshIndexForAtom[size1][atom1][1]];
				Cu=meshesBIGRingUp[connectivityList[i][1+(3*j)]].vertices[MeshIndexForAtom[size2][atom2][1]];
				Du=meshesBIGRingUp[connectivityList[i][1+(3*j)]].vertices[MeshIndexForAtom[size2][atom2][0]];

				Ad=meshesBIGRingDown[i].vertices[MeshIndexForAtom[size1][atom1][0]];
				Bd=meshesBIGRingDown[i].vertices[MeshIndexForAtom[size1][atom1][1]];
				Cd=meshesBIGRingDown[connectivityList[i][1+(3*j)]].vertices[MeshIndexForAtom[size2][atom2][1]];
				Dd=meshesBIGRingDown[connectivityList[i][1+(3*j)]].vertices[MeshIndexForAtom[size2][atom2][0]];

				//Angle for twist
				float AngleU = Vector3.Angle((Au-Bu), (Cu-Du));


				//Debug.Log("res:"+(i+1)+ "connected to+"+(connectivityList[i][1+(3*j)]+1) );
				if (AngleU>70){
					verticesUP.AddMany(Au,Bu,Dd,Cd);
					verticesDown.AddMany(Ad,Bd,Du,Cu);
				}else{
					verticesUP.AddMany(Au,Bu,Cu,Du);
					verticesDown.AddMany(Ad,Bd,Cd,Dd);
				}

				VerticesSide.AddMany(verticesUP[0],verticesDown[0]);
				VerticesSide.AddMany(verticesUP[1],verticesDown[1]);
				VerticesSide.AddMany(verticesUP[3],verticesDown[3]);
				VerticesSide.AddMany(verticesUP[2],verticesDown[2]);

				TrianglesUP.AddMany(0,1,2,
				                    2,1,3);

				TrianglesDown.AddMany(3,2,1,
				                      1,2,0);

				TrianglesSide.AddMany(0,1,2, 2,1,3, 2,3,4, 4,3,5,
				                      4,5,6, 6,5,7, 6,7,0, 0,7,1);

				 

				if (COLOR_MODE_BOND == SUGAR){

					if (UI.GUIDisplay.colorByResiduesDict.ContainsKey(resname1)){
						colorsUP.Add(lightColor(UI.GUIDisplay.colorByResiduesDict[resname1], LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(UI.GUIDisplay.colorByResiduesDict[resname1], LIGHTER_COLOR_FACTOR_BOND));
						colorsUP.Add(lightColor(UI.GUIDisplay.colorByResiduesDict[resname1], LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(UI.GUIDisplay.colorByResiduesDict[resname1], LIGHTER_COLOR_FACTOR_BOND));
					}else{
						colorsUP.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsUP.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
					}

					if (UI.GUIDisplay.colorByResiduesDict.ContainsKey(resname2)){
						colorsUP.Add(lightColor(UI.GUIDisplay.colorByResiduesDict[resname2], LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(UI.GUIDisplay.colorByResiduesDict[resname2], LIGHTER_COLOR_FACTOR_BOND));
						colorsUP.Add(lightColor(UI.GUIDisplay.colorByResiduesDict[resname2], LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(UI.GUIDisplay.colorByResiduesDict[resname2], LIGHTER_COLOR_FACTOR_BOND));
					}else{
						colorsUP.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsUP.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
					}

				}else if (COLOR_MODE_BOND == CHAIN){
					if (UI.GUIDisplay.ChainColorDict.ContainsKey(chain1)){
						colorsUP.Add(lightColor(UI.GUIDisplay.ChainColorDict[chain1], LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(UI.GUIDisplay.ChainColorDict[chain1], LIGHTER_COLOR_FACTOR_BOND));
						colorsUP.Add(lightColor(UI.GUIDisplay.ChainColorDict[chain1], LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(UI.GUIDisplay.ChainColorDict[chain1], LIGHTER_COLOR_FACTOR_BOND));
					}else{
						colorsUP.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsUP.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
					}
					if (UI.GUIDisplay.ChainColorDict.ContainsKey(chain2)){
						colorsUP.Add(lightColor(UI.GUIDisplay.ChainColorDict[chain2], LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(UI.GUIDisplay.ChainColorDict[chain2], LIGHTER_COLOR_FACTOR_BOND));
						colorsUP.Add(lightColor(UI.GUIDisplay.ChainColorDict[chain2], LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(UI.GUIDisplay.ChainColorDict[chain2], LIGHTER_COLOR_FACTOR_BOND));
					}else{
						colorsUP.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsUP.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
					}
					
				}else if (COLOR_MODE_BOND == PICKER){
					for (int k=0; k<4;k++){
						colorsUP.Add(lightColor(BONDCOLOR.color, LIGHTER_COLOR_FACTOR_BOND));
						colorsDown.Add(lightColor(BONDCOLOR.color, LIGHTER_COLOR_FACTOR_BOND));
					}


				}

				colorsSide.AddMany(colorsUP[0], colorsDown[0]);
				colorsSide.AddMany(colorsUP[1], colorsDown[1]);
				colorsSide.AddMany(colorsUP[2], colorsDown[2]);
				colorsSide.AddMany(colorsUP[3], colorsDown[3]);

				BondUP.vertices = verticesUP.ToArray();
				BondUP.triangles = TrianglesUP.ToArray();
				BondUP.colors32 = colorsUP.ToArray();

				BondDown.vertices = verticesDown.ToArray();
				BondDown.triangles = TrianglesDown.ToArray();
				BondDown.colors32 = colorsDown.ToArray();

				bondSide.vertices = VerticesSide.ToArray();
				bondSide.triangles = TrianglesSide.ToArray();//generateTriangleSide(BondUP);
				bondSide.colors32 = colorsSide.ToArray();

				BondUP.RecalculateNormals();
				BondDown.RecalculateNormals();
				bondSide.RecalculateNormals();
				meshesBondUp.Add(BondUP);
				meshesBondDown.Add(BondDown);
				meshesBondSide.Add(bondSide);

				res_for_bond[key_index_for_bond_dict] = resname1 +"_"+chain1+"_"+resname2+"_"+chain2;
			}
		}

	}




	public void CreateOxygenSphere(int pos, int res, int sugartype=0){
		GameObject oxysphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		oxysphere.tag = "OxySphere";
		Vector3 point = new Vector3(atomsLocationlist [pos] [0],atomsLocationlist [pos] [1],atomsLocationlist [pos] [2]);
		oxysphere.transform.position = point;
		string resname = residueslist[res];
		string chain = resChainList[res];
		if (sugartype==1){
			if (UI.GUIDisplay.colorByResiduesDict.ContainsKey(resname))
				oxysphere.GetComponent<Renderer>().material.color = UI.GUIDisplay.colorByResiduesDict[resname];
			else
				oxysphere.GetComponent<Renderer>().material.color = new Color(0.7f,0.7f,0.7f,0.5f);
		}else if (sugartype==2){
			oxysphere.GetComponent<Renderer>().material.color = UI.GUIDisplay.ChainColorDict[chain];
		}else{
			oxysphere.GetComponent<Renderer>().material.color = Color.red;
		}
		oxysphere.transform.localScale = new Vector3(OXYSPHERESIZE, OXYSPHERESIZE, OXYSPHERESIZE);

	}


	public void ShowOxySphere(int colortype=0){
		for (int i=0; i<this.cycles.Count; i++) {
			for (int j=0; j<this.cycles[i].Count; j++) {
				if (atomsNamelist[cycles[i][j]][0] == 'O'){
					CreateOxygenSphere (cycles [i] [j], i,colortype);
				}
			}
		}
	}




	/// <summary>
	/// For each mesh, we create Two mesh (to have a ribbon with tchickness)
	/// </summary>
	/// <param name="mesh">Mesh.</param>
	/// <param name="res">Res.</param>
	public void CreateSugarRibbonsObj(Mesh mesh, string res="", string chain="", string tag="SugarRibbons", int id=0){
		GameObject SRobj;
		if (tag=="SugarRibbons_BOND")
			SRobj = new GameObject("SugarRibbons_"+res+"_"+chain+"_BOND_"+id);
		else
			SRobj = new GameObject("SugarRibbons_"+res+"_"+chain);
		SRobj.tag = tag;
		SRobj.AddComponent<MeshFilter>();
		SRobj.AddComponent<MeshRenderer>();
		SRobj.GetComponent<MeshFilter> ().mesh = mesh;
		SRobj.GetComponent<Renderer>().material = new Material (Shader.Find ("Custom/Ribbons"));

	} 

	private void debugline(Vector3 a, Vector3 b){
		GameObject lineObject;
		LineRenderer line;
		lineObject = new GameObject("measureline");
		line = lineObject.AddComponent<LineRenderer>();
		line.material = new Material (Shader.Find("Particles/Alpha Blended"));
		line.SetColors(Color.red, Color.red);
		line.SetWidth(0.3f, 0.3f);
		line.SetPosition(0, a);
		line.SetPosition(1,b);

	}
	



	public void createSugarRibbons(){
		generateCycleMesh2();
		generateBiggerCycleMesh();

		for (int i=0; i<CycleMesh.Count; i++) {
			//For center ring
			generateRibbonsMesh(CycleMesh[i], i, THICKNESS_LITTLE_MESH,true);

			//For outer ring
			generateRibbonsMesh(CycleBIGMesh[i], i, THICKNESS_BIG_MESH,false);
		}

		create_Bond();
		for (int i=0; i<meshesRingUp.Count; i++){
			//For center ring
			CreateSugarRibbonsObj (meshesRingUp[i],residueslist[i],resChainList[i], "SugarRibbons_RING_little");
			CreateSugarRibbonsObj (meshesRingDown[i],residueslist[i],resChainList[i],"SugarRibbons_RING_little");
			CreateSugarRibbonsObj(meshesSideRing[i],residueslist[i],resChainList[i],"SugarRibbons_RING_little");

			//For outer ring
			CreateSugarRibbonsObj (meshesBIGRingUp[i],residueslist[i],resChainList[i],"SugarRibbons_RING_BIG");
			CreateSugarRibbonsObj (meshesBIGRingDown[i],residueslist[i],resChainList[i],"SugarRibbons_RING_BIG");
			CreateSugarRibbonsObj(meshesBIGSideRing[i],residueslist[i],resChainList[i],"SugarRibbons_RING_BIG");
		}

		for (int i=0; i<meshesBondUp.Count; i++){
			CreateSugarRibbonsObj (meshesBondUp[i],residueslist[i],resChainList[i],"SugarRibbons_BOND",i);
			CreateSugarRibbonsObj (meshesBondDown[i],residueslist[i],resChainList[i],"SugarRibbons_BOND",i);
			CreateSugarRibbonsObj (meshesBondSide[i],residueslist[i],resChainList[i],"SugarRibbons_BOND",i);
		}

	}



	/***********************************************
	 ************** TOOL FUNCTIONS *****************
	 ***********************************************/

	public void updateColor(string tag, ColorObject newColor){
		GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
		for (int i=0; i<objs.Length; i++){
			Color32[] colours = objs[i].GetComponent<MeshFilter> ().mesh.colors32;
			
			for (int j=0; j< colours.Length; j++){
				if (tag=="SugarRibbons_BOND")
					colours[j]=lightColor(newColor.color, LIGHTER_COLOR_FACTOR_BOND);
				else
					colours[j]=lightColor(newColor.color, LIGHTER_COLOR_FACTOR_RING);

			}
			objs[i].GetComponent<MeshFilter> ().mesh.colors32 = colours;
		}
	}

	public void updateColor(string tag, int type){

		GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
		if (tag=="SugarRibbons_BOND"){
			for (int i=0; i<objs.Length; i++){
				Color32[] colours = objs[i].GetComponent<MeshFilter> ().mesh.colors32;
				int SIZE          = objs[i].GetComponent<MeshFilter> ().mesh.vertices.Length;
				int ID            = int.Parse(objs[i].ToString().Split('_')[4].Split(' ')[0]);
				string resname1   = res_for_bond[ID].Split('_')[0];
				string resname2   = res_for_bond[ID].Split('_')[2];
				string chain1     = res_for_bond[ID].Split('_')[1];
				string chain2     = res_for_bond[ID].Split('_')[3];
				Color32 col1;
				Color32 col2;
				if (type == SUGAR){
					if (UI.GUIDisplay.colorByResiduesDict.ContainsKey(resname1))
						col1 =(lightColor(UI.GUIDisplay.colorByResiduesDict[resname1], LIGHTER_COLOR_FACTOR_BOND));
					else
						col1 =lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND);
					if (UI.GUIDisplay.colorByResiduesDict.ContainsKey(resname1))
						col2=(lightColor(UI.GUIDisplay.colorByResiduesDict[resname2], LIGHTER_COLOR_FACTOR_BOND));
					else
						col2 =lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND);
				}else {
					if (UI.GUIDisplay.ChainColorDict.ContainsKey(chain1))
						col1=(lightColor(UI.GUIDisplay.ChainColorDict[chain1], LIGHTER_COLOR_FACTOR_BOND));
					else
						col1 = (lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
					if (UI.GUIDisplay.ChainColorDict.ContainsKey(chain2))
						col2=(lightColor(UI.GUIDisplay.ChainColorDict[chain2], LIGHTER_COLOR_FACTOR_BOND));
					else
						col2 = (lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_BOND));
				}
				if (SIZE == 4){
					colours[0]=col1;
					colours[1]=col1;
					colours[2]=col2;
					colours[3]=col2;
				}else {
					colours[0]=col1;
					colours[1]=col1;
					colours[2]=col1;
					colours[3]=col1;
					colours[4]=col2;
					colours[5]=col2;
					colours[6]=col2;
					colours[7]=col2;
				}
				objs[i].GetComponent<MeshFilter> ().mesh.colors32 = colours;
			}
		}else{
			for (int i=0; i<objs.Length; i++){
				Color32[] colours = objs[i].GetComponent<MeshFilter> ().mesh.colors32;
				string resname = objs[i].ToString().Split('_')[1];
				string chain = objs[i].ToString().Split('_')[2].Split(' ')[0];
				for (int j=0; j<colours.Length; j++){
					if (type == SUGAR){
						if (UI.GUIDisplay.colorByResiduesDict.ContainsKey(resname))
							colours[j]=(lightColor(UI.GUIDisplay.colorByResiduesDict[resname], LIGHTER_COLOR_FACTOR_RING));
						else
							colours[j]=(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_RING));
					}else if (type == CHAIN){
						if (UI.GUIDisplay.ChainColorDict.ContainsKey(chain))
							colours[j]=(lightColor(UI.GUIDisplay.ChainColorDict[chain], LIGHTER_COLOR_FACTOR_RING));
						else
							colours[j]=(lightColor(new Color(0.7f,0.7f,0.7f,0.5f), LIGHTER_COLOR_FACTOR_RING));
						
					}
				}
				objs[i].GetComponent<MeshFilter> ().mesh.colors32 = colours;
			}
		}
	}
	
	
	private void CalculBarycenter(){
		Debug.Log("Calculation of cycles barycenters");
		Vector3 barycenter = new Vector3();
		//we save all coordinate and prepare barycenter calculation
		float xTot, yTot ,zTot;
		for (int i=0; i<cycles.Count; i++){
			xTot=0;
			yTot=0;
			zTot=0;
			for (int j=0; j<cycles[i].Count; j++){
				xTot=xTot+atomsLocationlist [cycles [i] [j]] [0];
				yTot=yTot+atomsLocationlist [cycles [i] [j]] [1];
				zTot=zTot+atomsLocationlist [cycles [i] [j]] [2];
			}
			//calculation of barycenter
			barycenter.x=xTot/(cycles[i].Count);
			barycenter.y=yTot/(cycles[i].Count);
			barycenter.z=zTot/(cycles[i].Count);
			barylist.Add(barycenter);
		}
		
	}
	
	private Vector3 calculMeshBarycenter(Mesh mesh){
		float totX, totY, totZ;
		totX = totY = totZ = 0;
		Vector3 barycenter = new Vector3();
		
		for (int i=0; i<mesh.vertices.Length; i++){
			totX+=mesh.vertices[i][0];
			totY+=mesh.vertices[i][1];
			totZ+=mesh.vertices[i][2];
		}
		barycenter.x = totX/mesh.vertices.Length;
		barycenter.y = totY/mesh.vertices.Length;
		barycenter.z = totZ/mesh.vertices.Length;
		
		return barycenter;
	}
	
	private Vector3 calculMidPoint(Vector3 a, Vector3 b){
		float x, y, z;
		x = (a.x + b.x)/2;
		y = (a.y + b.y)/2;
		z = (a.z + b.z)/2;
		
		return new Vector3(x,y,z);
	}
	
	private int findRingAtomIndex(int ringIndex, int AtomNumber){
		for (int i=0; i<cycles[ringIndex].Count; i++){
			if (cycles[ringIndex][i] == AtomNumber)
				return i;
		}
		return -1;
	}
	
	private Vector3 CycleAtomToVector3(int index_res, int number){
		Vector3 v = new Vector3 (atomsLocationlist[cycles[index_res][number]][0],
		                         atomsLocationlist[cycles[index_res][number]][1],
		                         atomsLocationlist[cycles[index_res][number]][2]);
		
		return v;
		
	}
	
	private Vector3 calcullittleRibbonsControlPoint(Vector3 a, Vector3 b, float size=0.7f){
		Vector3 v = new Vector3 ();
		Vector3 dir= new Vector3();
		
		dir = a - b;

		v = a - (dir * size);
		return v;
		
	}
	
	
	private void createDebugSphere(Vector3 point, Color color, float size=1f){
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		
		go.AddComponent<Animation>();
		go.transform.position = point;
		go.GetComponent<Renderer>().material.color = color;
		go.transform.localScale = new Vector3(size, size, size);	
	}


	private Vector3 calculMeanNormalMesh(Mesh mesh){
		float xTot, yTot, zTot;
		xTot = yTot = zTot = 0;
		Vector3 normalMean = new Vector3();
		mesh.RecalculateNormals();
		for (int i=0; i<mesh.normals.Length; i++){
			xTot+=mesh.normals[i].x;
			yTot+=mesh.normals[i].y;
			zTot+=mesh.normals[i].z;
		}
		
		normalMean.x = xTot/mesh.normals.Length;
		normalMean.y = yTot/mesh.normals.Length;
		normalMean.z = zTot/mesh.normals.Length;
		
		return normalMean;
		
	}
	
	
	private int[] invertTrianglesMesh(Mesh mesh){
		List<int> newTriangles = new List<int>();
		
		for (int i=mesh.triangles.Length-1; i>=0; i--){
			newTriangles.Add(mesh.triangles[i]);
		}
		
		return newTriangles.ToArray();
	}
	
	
	private int[] generateTriangleSide(Mesh mesh){
		List<int> triangles = new List<int>();
		int size = mesh.vertices.Length;
		
		for (int i=0; i<size -2; i=i+2){
			triangles.AddMany(i, i+1, i+2);
			triangles.AddMany(i+2, i+1, i+3);
		}
		
		triangles.AddMany(size-2, size-1, 0);
		triangles.AddMany (0, size-1, 1);
		
		return triangles.ToArray();
	}

	public Color lightColor(Color color, float correctionFactor = 0.35f){
		if (correctionFactor>=0)
			return Color.Lerp(color, Color.white, correctionFactor);
		else{
			return Color.Lerp(color, Color.black, -correctionFactor);
		}
	}
	
	public void cleanup(){
		CycleMesh.Clear();
		CycleBIGMesh.Clear();
		meshesRingUp.Clear();
		meshesRingDown.Clear();
		meshesSideRing.Clear();
		meshesBIGRingUp.Clear();
		meshesBIGRingDown.Clear();
		meshesBIGSideRing.Clear();
		meshesBondUp.Clear();
		meshesBondDown.Clear();
		meshesBondSide.Clear();
	}
	



}
