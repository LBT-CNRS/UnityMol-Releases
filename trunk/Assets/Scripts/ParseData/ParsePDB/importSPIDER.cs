/// @file importSPIDER.cs
/// @brief Details to be specified
/// @author FvNano/LBT team
/// @author Marc Baaden <baaden@smplinux.de>
/// @date   2013-4
///
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
/// $Id: importSPIDER.cs 213 2013-04-06 21:13:42Z baaden $
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

//using UnityEngine;
//using System.Collections;
//using System;
//using System.IO;
//using System.Text;
//
//public class importSPIDER : MonoBehaviour {
//	
//	public string filePath = "Assets/Resources/f2YFEgrad.dx";
//	public string fileName = "2YFEgrad.dx";
//	public float GoSize = 10; // size of GOs
//	public float shaderRayon = 0.0f; // size of spheres using shader
//	public float sliderThres = 0.265f; // nb of points to display selecter, through slider
//	public float slideMin; // threshold slider bounds
//	public float slideMax;
//	private bool toggleSurface = false;
//	private bool toggleDiffSize = false;
//	private bool cubesHidden;
//	
//	private int nbObj; // nb of selected points to display
//	public float[,,] record; // whole file coordinates, raw data
//	public GameObject[,,] GOlist; // referencing objects
//	private float NZ; // grid positions
//	private float NY;
//	private float NX;
//	public int centrage; // used for grid positioning to 0,0,0
//	public Transform objectsTransform;
//	public Vector3 position;
//	
//	public int NBline;
//	
//	public float[,,] Grille;
//	
//	void Start () {
//		
//		// to remove test animation objs from the scene, line to remove when implementing UnityMol
//		DestroyAllOldInstance();
//		
//		// read whole file, limited to 2GB
//		// Use .bytes extension for the SPIDER file to ensure the whole file is read ?
////		TextAsset file = Resources.Load(fileName) as TextAsset;
////		Stream s = new MemoryStream(file.bytes);
////		BinaryReader inputBytes = new BinaryReader(s);
////		byte[] inputBytes = File.ReadAllBytes(filePath);
////		int bytesSize = inputBytes.Length;
////		byte[] bytes = new byte[bytesSize];
//		
////		// check for format little endian
////		if(System.BitConverter.IsLittleEndian) {
////			Debug.Log ("File is little Endian formated.");
////			// no conversion
////			Array.Copy (inputBytes, bytes, bytesSize);
////		}
////		else {
////			Debug.Log ("File is big Endian formated.");
////			// convert to little endian
////			// TODO : Traduire le header en little endian, inversion pour les différents types
////			// TODO : Traduire les coordonnées en little endian, inversion par 4 bytes (float)
////			float tmp_float = BitConverter.ToSingle(inputBytes, 0);
////			Debug.Log (tmp_float);
////			byte[] temp = BitConverter.GetBytes(tmp_float);
////			Array.Reverse (temp);
////			float res = BitConverter.ToSingle(temp, 0);
////			Debug.Log (res);
////		}
//		
////		int header = 1; // assume header is valid until shown otherwise
////		
////		// NZ 1, NY 2, NX 12, LABREC 13 (labels and positions)
////		NZ = System.BitConverter.ToSingle(bytes, (1-1)*4);
////		NY = System.BitConverter.ToSingle(bytes, (2-1)*4);
////		NX = System.BitConverter.ToSingle(bytes, (12-1)*4);	
////		float LABREC = System.BitConverter.ToSingle(bytes, (13-1)*4);
////		NX = 193;
////		NY = 193;
////		NZ = 193;
//////		// minimum size is 1024 bytes
////		if (LABREC < 1024)
////			LABREC = 1024;
//////		
////		// center points on position 0,0,0 if grid is a cube
////		if (NX == NY && NY == NZ) {
////			centrage = (int)NX / 2;
////		}
////		else {
////			centrage = 0;
////		}
//		
//		// TODO : Check SPIDER format validity using header
//		Debug.Log("NZ:"+NZ);
//		Debug.Log("NY:"+NY);
//		Debug.Log("NX:"+NX);
////		Debug.Log("LABREC:"+LABREC);
//		
//		// Listing records
//		// NZ : number of slices
//		// NY : number of rows per slices
//		// NX : number of pixels per line
//		record = new float[(int)NZ,(int)NY,(int)NX];
////		int pos;
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		int curentline=11;
////		FileInfo file=new FileInfo(UI.GUIDisplay.file_base_name+".dx");
//        string text;
//		StreamReader sr=new StreamReader(UI.GUIDisplay.file_base_name+".dx");
//		using (sr)
//        {              
//            text = sr.ReadToEnd();                 
//       	}
//		
//		string[] lines = text.Split("\n".ToCharArray());
//		string[] splite = lines[4].Split();
//		NZ = int.Parse(splite[5]);
//		NY = int.Parse(splite[6]);
//		NX = int.Parse(splite[7]);
//		
//		Grille = new float[(int)NX,(int)NY,(int)NZ];
//		
//		NBline = (int)(12 + (NX*NZ*NY/3));
//		int i=0;
//		int j=0;
//		int k=0;
//		
//		while (i<NX){
//			if (j<NY-1){
//				string[] temp = splite[curentline].Split();
//				curentline++;
//				if (k == NZ-1){
//					Grille[i,j,k] = float.Parse(temp[0]); k = 0;
//					j++;
//					Grille[i,j,k] = float.Parse(temp[1]); k++;
//					Grille[i,j,k] = float.Parse(temp[2]); k++;
//				}
//				else if (k == NZ-2){
//					Grille[i,j,k] = float.Parse(temp[0]); k++;
//					Grille[i,j,k] = float.Parse(temp[1]); k=0;
//					j++;
//					Grille[i,j,k] = float.Parse(temp[2]); k++;
//				}
//				else if (k == NZ-3){
//					Grille[i,j,k] = float.Parse(temp[0]); k++;
//					Grille[i,j,k] = float.Parse(temp[1]); k++;	
//					Grille[i,j,k] = float.Parse(temp[2]); k = 0;
//					j++;
//				}
//				else {
//					Grille[i,j,k] = float.Parse(temp[0]); k++;
//					Grille[i,j,k] = float.Parse(temp[1]); k++;
//					Grille[i,j,k] = float.Parse(temp[1]); k++;
//				}
//			}
//			else if (j<NY){
//				string[] temp = splite[curentline].Split();
//				curentline++;
//				if (k == NZ-1){
//					Grille[i,j,k] = float.Parse(temp[0]); k = 0;
//					j=0;
//					i++;
//					Grille[i,j,k] = float.Parse(temp[1]); k++;
//					Grille[i,j,k] = float.Parse(temp[2]); k++;
//				}
//				else if (k == NZ-2){
//					Grille[i,j,k] = float.Parse(temp[0]); k++;
//					Grille[i,j,k] = float.Parse(temp[1]); k=0;
//					j=0;
//					i++;
//					Grille[i,j,k] = float.Parse(temp[2]); k++;
//				}
//				else if (k == NZ-3){
//					Grille[i,j,k] = float.Parse(temp[0]); k++;
//					Grille[i,j,k] = float.Parse(temp[1]); k++;	
//					Grille[i,j,k] = float.Parse(temp[2]); k = 0;
//					j=0;
//					i++;
//				}
//				else {
//					Grille[i,j,k] = float.Parse(temp[0]); k++;
//					Grille[i,j,k] = float.Parse(temp[1]); k++;
//					Grille[i,j,k] = float.Parse(temp[2]); k++;
//				
//				}
//			
//			}	
//			
//		}
//		
//		record= Grille;
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
//		
////		pos = (int)LABREC;
////		int SPos = 0;
//////		float[] sliderPos = new float[(int)NX * (int)NY * (int)NZ];
////		float bit;
////		for (int i = 0; i < NZ; i++) {
////			for (int j = 0; j < NY; j++) {
////				for (int k = 0; k < NX; k++) {
////					bit = System.BitConverter.ToSingle(bytes, pos);
////					record[i,j,k] = bit;
////					pos += 4;
//////					sliderPos[SPos] = bit;
//////					SPos++;
////				}
////			}
////		}
//		
//		// TODO : determine GUI slider range dynamically, too long using sorting, find better way to do it
//		// using only one (middle) layer or a few ?
//
////		Array.Sort(sliderPos);
////		slideMin = sliderPos[sliderPos.Length-25000];
////		slideMax = sliderPos[sliderPos.Length-80];
//		slideMin = -0.8f;
//		slideMax = 0.8f;
//		
//		// list of active gameobjects (displayed)
//		GOlist = new GameObject[(int)NZ,(int)NY,(int)NX];
//		
//		SelectPoints(sliderThres, record, GOlist, NZ, NY, NX,shaderRayon, toggleDiffSize);
//		// TODO : Create all of the cubes on the beginning, then display or hide
//	}
//	
//	void Update () {	
//		
//	}
//	
//	void OnGUI () {
//		
//		GUI.Box(new Rect(10,10,200,160), "Settings");
//		GUI.Label(new Rect(18,30,200,60), "Thres : "+sliderThres);
//		sliderThres = GUI.HorizontalSlider(new Rect(15, 55, 190, 30), sliderThres, slideMin, slideMax);
//		GUI.Label(new Rect(18,129,200,60), "Rayon : "+shaderRayon);
//		shaderRayon = GUI.HorizontalSlider(new Rect(15, 152, 190, 30), shaderRayon, 0f, 1.8f);
//		toggleSurface = GUI.Toggle(new Rect(15, 90, 180, 15), toggleSurface, " Isosurface");
//		toggleDiffSize = GUI.Toggle(new Rect(15, 110, 180, 15), toggleDiffSize, " Diff. size");
//		
//		if (GUI.changed) {
//			DestroySurface ();
//			if (toggleSurface) {
//				toggleDiffSize = false;
//				if (!cubesHidden) {
//					HideAllOldInstance();
//				}
////				nbObj = MarchingCubes.MC((int)NZ, (int)NY, (int)NX, sliderThres, record, centrage);
//			}
//			else {
//				cubesHidden = false;
//				if (toggleDiffSize) {
//					SelectPoints(sliderThres, record, GOlist, NZ, NY, NX, shaderRayon, toggleDiffSize);
//				}
//				else {
//					SelectPoints(sliderThres, record, GOlist, NZ, NY, NX, shaderRayon, toggleDiffSize);
//				}
//			}
//			Debug.Log ("nbObj : "+nbObj);
//		}
//		GUI.Label(new Rect(18,70,200,60), "Nb points : "+nbObj);
//	}
//	
//	void SelectPoints (float sliderThres, float[,,] record, GameObject[,,] GOlist, float NZ, float NY, float NX, float shaderRayon, bool toggleDiffSize) {
//		
//		nbObj = 0;
//		// select nb of points to display with threshold, then display it or destroy precedent objects
//		for (int i = 0; i < NZ; i++) {
//			for (int j = 0; j < NY; j++) {
//				for (int k = 0; k < NX; k++) {
//					if (Math.Abs(record[i,j,k]) >= sliderThres) {
//						Vector3 size = new Vector3(record[i,j,k] * GoSize, record[i,j,k] * GoSize, record[i,j,k] * GoSize);
//						Vector3 position = new Vector3(i-centrage, j-centrage, k-centrage);
//						if (GOlist[i,j,k] != null) {
//							DisplayInstance(i,j,k,toggleDiffSize,size,shaderRayon);
//						}
//						else {
//							MakeInstance(i, j, k, size, position, GOlist, shaderRayon, toggleDiffSize);
//							// TODO : Remplacer par de l'instantiation classique ?
//						}
//					}
//					else if (Math.Abs(record[i,j,k]) < sliderThres && GOlist[i,j,k] != null) {
////						DestroyInstance(i,j,k);
//						HideInstance (i,j,k);
//					}
//					// TODO : Fix pb with conditions, cases must be lacking, can sometimes bug program. DONE ?
//					if (GOlist[i,j,k] != null && GOlist[i,j,k].active == true)
//						nbObj++;
//				}
//			}
//		}
//	}
//
//	void MakeInstance(int NZ, int NY, int NX, Vector3 size, Vector3 position, GameObject[,,] GOlist, float shaderRayon, bool toggleDiffSize) {
//		GOlist[NZ,NY,NX] = GameObject.CreatePrimitive(PrimitiveType.Cube);
//		GOlist[NZ,NY,NX].tag = "cubesOBJ";
//		GOlist[NZ,NY,NX].transform.localScale = size;
//		GOlist[NZ,NY,NX].transform.localPosition = position;
//		GOlist[NZ,NY,NX].transform.localRotation = Quaternion.identity;
//		GOlist[NZ,NY,NX].renderer.material = new Material(Shader.Find("CubeShader"));
//		GOlist[NZ,NY,NX].transform.parent = GameObject.Find("Objects").transform;
//		
//		if (toggleDiffSize) {
//			GOlist[NZ,NY,NX].renderer.material.SetFloat("_Rayon", (float)Math.Pow(Math.Log10(shaderRayon*1.6), 5) * 50);
//		}
//		else {
//			GOlist[NZ,NY,NX].renderer.material.SetFloat("_Rayon", shaderRayon);
//		} 
//	}
//	
//	void DisplayInstance(int NZ, int NY, int NX, bool toggleDiffSize, Vector3 size, float shaderRayon) {
//		GOlist[NZ, NY, NX].active = true;
//		if (toggleDiffSize) {
//			GOlist[NZ,NY,NX].renderer.material.SetFloat("_Rayon", (float)Math.Pow(Math.Log10(shaderRayon*1.6), 5) * 50);
//		}
//		else {
//			GOlist[NZ,NY,NX].renderer.material.SetFloat("_Rayon", shaderRayon);
//		}
//	}
//	
//	void DestroyAllOldInstance() {
//		GameObject[] activeOBJ = GameObject.FindGameObjectsWithTag("cubesOBJ");
//		for (int l = 0; l < activeOBJ.Length; l++) {
//			Destroy(activeOBJ[l]);
//		}
//	}
//	
//	void DestroyInstance(int NZ, int NY, int NX) {
//		Destroy(GOlist[NZ, NY, NX]);
//	}
//	
//	void HideInstance(int NZ, int NY, int NX) {
//		GOlist[NZ, NY, NX].active = false;
//	}
//	
//	void DestroySurface() {
//		GameObject[] surfaceOBJ = GameObject.FindGameObjectsWithTag("surfaceOBJ");
//		for (int l = 0; l < surfaceOBJ.Length; l++) {
//			Destroy(surfaceOBJ[l]);
//		}
//	}
//	
//	void HideAllOldInstance() {
//		GameObject[] activeOBJ = GameObject.FindGameObjectsWithTag("cubesOBJ");
//		for (int l = 0; l < activeOBJ.Length; l++) {
//			activeOBJ[l].active = false;
//		}	
//		cubesHidden = true;
//	}
//	
//}