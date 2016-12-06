/// @file ReadDX.cs
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
/// $Id: ReadDX.cs 480 2014-05-02 14:51:10Z tubiana $
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
	using System.IO;
	using ParseData.IParsePDB;
	using System.Net;
	using System;
	using Molecule.Model;
	using Molecule.Control;
	using System.Xml;
	using System.Text;	
	using System.Text.RegularExpressions;
	using UI;
public class ReadDX : MonoBehaviour {
//	
//	public int NX;
//	public int NY;
//	public int NZ;
//	public int NBline;
//	
//	public float[,,] Grille;
//	
	// Use this for initialization
	
	public float[,,] _grid;
//	public float[] _gridplate;
	private Vector3[,,] gradient;
	private int[] _dim;
	private Vector3 _delta;
	private Vector3 _origin;
	public int X;
	public int Y;
	public int Z;

	private bool loaded = false;
	public bool Loaded {
		get{ return loaded; }
	}
//	
	public Vector3 GetDelta() {
			Debug.Log (_delta);
			return _delta;
	}
	
	public Vector3 GetOrigin() {
			Debug.Log(" ori: "+ _origin);
			return _origin;
	}
	
	
	public float getVal(int x, int y, int z) {
			return _grid[x,y,z];
	}
	
	public Vector3 getGradient(int x, int y, int z) {
			return gradient[x,y,z];
	}
//
	public void ReadFile(string file_name) {
		ReadFile(file_name, Vector3.zero);
	}

	public void ReadFile(string file_name, Vector3 offset){

		// lecture en-tête du fichier .dx

//		Debug.Log("temps : "+temps);
		
		StreamReader sr;

		#if UNITY_WEBPLAYER
			HttpWebRequest request =(HttpWebRequest) WebRequest.Create(file_name);
			// If required by the server, set the credentials.
			request.Credentials = CredentialCache.DefaultCredentials;
			// Get the response.
		    HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
		    // Get the stream containing content returned by the server.
     	    Stream dataStream = response.GetResponseStream ();
            // Open the stream using a StreamReader for easy access.
            sr = new StreamReader (dataStream);
		#else
				try {
			FileInfo file = new FileInfo(file_name);
			sr = file.OpenText();

				} catch (Exception e) {
				                // Something went wrong, so lets get information about it.
						Debug.Log(e.ToString());
			return;
		}

		#endif
		
		ReadFile(sr, offset);
//		String all = sr.ReadToEnd();    // Grille plate
//		string[] allline = all.Split('\n');
	}
	

	public void ReadFile(TextReader sr, Vector3 offset) {
		DateTime temps = DateTime.Now;
		string line = "#"; // decalage jusqu'a la taille de la grille
		while (line[0] == '#')line = sr.ReadLine(); // lecture de la taille de la grille
		string[] size = line.Split(' ');
//		string[] size = allline[4].Split(' ');  // grille plate (GP)
		
		X = int.Parse(size[5]);
		Y = int.Parse(size[6]);
		Z = int.Parse(size[7]);
//		int nScalar;
//		nScalar = X*Y*Z;
		DateTime temps1 = DateTime.Now;
		Debug.Log(" taille de la grille : "+ X +" "+ Y +" "+ Z);

		line = sr.ReadLine(); // lecture de l'origine
		string[] origs = line.Split(' ');
//		string[] origs = allline[5].Split(' '); 		//GP
		float oX = float.Parse(origs[1]);
		float oY = float.Parse(origs[2]);
		float oZ = float.Parse(origs[3]);
		
		DateTime temps2 = DateTime.Now;

		// lecture du delta
		float dX,dY,dZ;
		line = sr.ReadLine();
		string[] delt = line.Split(' ');
//		string[] delt = allline[6].Split(' ');    // GP

		dX = float.Parse(delt[1]);
		line = sr.ReadLine();
		delt= line.Split(' ');
//		delt = allline[7].Split(' ');			// GP

		dY = float.Parse(delt[2]);
		line = sr.ReadLine();
		delt= line.Split(' ');
//		delt = allline[8].Split(' ');       // GP

		dZ = float.Parse(delt[3]);
		
		DateTime temps3 = DateTime.Now;
		//lecture des commentaire
		line = sr.ReadLine();
		line = sr.ReadLine();
		
		DateTime temps4 = DateTime.Now;
		// test du nScalaile a faire pour plus de securité
//		int test_nScalar;
//		test_nScalar = 0;

//		line = sr.ReadLine();
		
		
		// declaration de la grille
		
		_dim = new int[3];
		_dim[0] = X; _dim[1] = Y; _dim[2] = Z;

		_grid = new float[X,Y,Z];
//		_gridplate = new float[X*Y*Z]; 						GP
//		for(int i=0; i<X; i++)
//		{
//			for(int j=0; j<Y; j++)
//			{
//				for(int k=0; k<Z; k++)
//				{
//					_grid[i,j,k] = 0.0f;
//				}
//			}
//		}
		_delta = new Vector3();
		_delta.x = dX;
		_delta.y = dY;
		_delta.z = dZ;
		Debug.Log("ReadDX :: Delta DX - " + _delta);

		_origin = new Vector3();
		_origin.x = oX - offset.x;
		_origin.y = oY + offset.y;
		_origin.z = oZ + offset.z;
		Debug.Log("ReadDX :: Origin DX - " + _origin);
		Debug.Log("ReadDX :: Offset DX - " + offset);


//		line = sr.ReadLine();
		string[] vals=  line.Split(' ');
		Debug.Log (line);
		float val ;
		int l=10;
//		int nbline = 11;  								// GP
//		string[] vals = allline[nbline].Split(' ');
		int test_compteur=0;
		for(int i=0; i<X; i++){
			for(int j=0; j<Y; j++){
				for(int k=0; k<Z; k++){
					
					if (l<3){
						if (test_compteur<(X*Y*Z)-1){
							val = float.Parse(vals[l]);
							_grid[i,j,k]=val;
	////						if (i==50 && j==50)
	////						if (val < 1.75)
	////							Debug.Log(" ijk" +i +" "+j+" "+k+" valeur dans la grille : "+ _grid[i,j,k]+"  vals: "+ vals[l] + " val: " + val);
	//
							l++;   
						}
					}else{
						line = sr.ReadLine();
						test_compteur+=3;
						vals = line.Split(' ');
						val = float.Parse(vals[0]);
						_grid[i,j,k]=val;
						l=1;
					}
				}
			}
		}
		Debug.Log("ReadDX :: test compteur - "+test_compteur);
		
		// lecture sur grille plate avec _gridplate 
//		for (int i = 0 ;i < X*Y*Z/3;i++){
//			vals = allline[nbline++].Split(' ');
//			_gridplate[i*3] = float.Parse(vals[0]);
//			_gridplate[i*3+1]= float.Parse(vals[1]);
//			_gridplate[i*3+2] = float.Parse(vals[2]);
//		}
//		
		
		DateTime temps5 = DateTime.Now;
		Debug.Log ("ReadDX :: taille - " +(temps1-temps));
		Debug.Log("ReadDX :: lecture origine - " + (temps2 -temps1));
		Debug.Log("ReadDX :: lecture de delta - " + (temps3 -temps2));
		Debug.Log("ReadDX :: ligne vide - "+(temps4 -temps3));
		Debug.Log("ReadDX :: grille lu - " + (temps5-temps4));
		loaded = true;
	}

	
	public void calGradient(){
		float scale = 1f;
		float valprec;
		float valnext;
		float val;

		
		gradient = new Vector3[X,Y,Z];
		
		for(int i=0; i<X; i++){
			for(int j=0; j<Y; j++){
				for(int k=0; k<Z; k++){
					if (i==0 || j==0 || k==0 || i ==(X-1) || j ==(Y-1) || k == (Z-1)){
						gradient[i,j,k]=new Vector3(0f,0f,0f);
					}else{
					val = getVal(i,j,k);
					valprec=getVal(i-1,j,k);
					valnext=getVal(i+1,j,k);
					//grad.setX(((val - valprec) + (valsucc - val))/(_deltax*ForceField::ANGSTROM2METER*2.0));
					gradient[i,j,k].x = ((val - valprec) + (valnext - val))/(_delta[0]*1.0f);
					
			
					val=getVal(i,j,k);
					valprec=getVal(i,j-1,k);
					valnext=getVal(i,j+1,k);
					//grad.setY(((val - valprec) + (valsucc - val))/(_deltay*ForceField::ANGSTROM2METER*2.0));
					gradient[i,j,k].y = ((val - valprec) + (valnext - val))/(_delta[1]*1.0f);
			
					val=getVal(i,j,k);
					valprec=getVal(i,j,k-1);
					valnext=getVal(i,j,k+1);
					//grad.setZ(((val - valprec) + (valsucc - val))/(_deltaz*ForceField::ANGSTROM2METER*2.0));
					gradient[i,j,k].z = ((val - valprec) + (valnext - val))/(_delta[2]*1.0f);				
					
					gradient[i,j,k] = -(gradient[i,j,k]*scale);
//					if (i==1 && j==1){
//							Debug.Log(" ijk" +i +" "+j+" "+k+" valeur dans la grille : "+ gradient[i,j,k]);
//						}
					}
				}
			}
		}
	
	}

	private void RightHandedToLeftHanded(Mesh m) {
		Vector3[] vertices = m.vertices;
		for(int i=0; i < vertices.Length; i++)
			vertices[i].x = -vertices[i].x;
		
		int[] triangles = m.triangles;
		
		for(int tri=0; tri < triangles.Length; tri=tri+3) {
	        int tmp = triangles[tri];
	        triangles[tri] = triangles[tri+2];
	        triangles[tri+2] = tmp;
	    }
		
	    Color[] colors = m.colors;
	    m.Clear();
	    m.vertices = vertices;
	    m.triangles = triangles;
	    m.colors = colors;
	    m.RecalculateNormals();
	}	
	
	public void isoSurface(float threshold, Color color, string tag="SurfaceManager", bool transparency=false){
		
		
		Vector4[] points;
		points = new Vector4[ (X) * (Y) * (Z)];

		for (int j = 0; j < Y; j++) {
			for (int i = 0; i < Z; i++) {
				for (int k = 0; k < X; k++) {
//					if (_grid[k,j,i] > 0) Debug.Log ("superieur a 0");
//					if (_grid[k,j,i] > -0.5	) Debug.Log ("superieur a -0,5:" + _grid[k,j,i]);
//					if (i==1 || j==1 || k==1 || i ==Z-2 || j ==Y-2 || k == X-2)
//						points[j*(Z)*(X) + i*(X) + k] = new Vector4 (i, j, k,-1000f);
//					else
						points[j*(Z)*(X) + i*(X) + k] = new Vector4 (k, j, i , _grid[k,j,i]);
				}
			}
		}
		
		
	
		
//		NZ = 4;
//			NY = 6;
//			NX = 5;
//		
//			float[,,] Gdata = new float[4,6,5] {
//			{{ 0,0,0,0,0},{0,0,0,0,0},{0,0,0,0,0},{0,0,0,0,0},{0,0,0,0,0},{0,0,0,0,0}},
//			{{ 0,0,0,0,0},{0,0,0,0,0},{0,0,1,0,0},{0,0,1,0,0},{0,0,0,0,0},{0,0,0,0,0}},
//			{{ 0,0,0,0,0},{0,0,0,0,0},{0,0,1,0,0},{0,0,1,0,0},{0,0,0,0,0},{0,0,0,0,0}},
//			{{ 0,0,0,0,0},{0,0,0,0,0},{0,0,0,0,0},{0,0,0,0,0},{0,0,0,0,0},{0,0,0,0,0}},
//			} ;
//		
//		points = new Vector4[(int)(NZ+1)*(int)(NY+1)*(int)(NX+1)];
		
//		for (int i = 0; i < NZ; i++) {
//			for (int j = 0; j < NY; j++) {
//				for (int k = 0; k < NX; k++) {
//					points[(i*(int)NX*(int)NY)+(j*(int)NY)+k] = new Vector4 (i,j,k,Gdata[i,j,k]);
//				}
//			}
//		}

		Color[] colors = new Color[(X) * (Y) * (Z)];
		for (int j = 0; j < Y; j++) {
			for (int i = 0; i < Z; i++) {
				for (int k = 0; k < X; k++) {
					colors[j*(Z)*(X) + i*(X) + k] = color;
				}
			}
		}
		
		
		
		
		Debug.Log("Entering :: before marching cubes instance");
		//MarchingCubesRec MCInstance;
		//MCInstance = new MarchingCubesRec();
		// PDBtoDEN.DestroySurface();
		
		
		/*
		
		if (threshold >= 0){
			MCInstance.MCRecMain(X, Y, Z, threshold, points, 0f,false, _delta, _origin,colors,tag);
		}else {
			MCInstance.MCRecMain(X, Y, Z, threshold, points, 0f,true , _delta, _origin,colors,tag);
			// GameObject iso_neg = GameObject.FindGameObjectWithTag("Elect_iso_negative");
			// Mesh mesh_neg = iso_neg.GetComponent<MeshFilter>().mesh;
			// //Unity has a left-handed coordinates system while Molecular obj are right-handed
			// //So we have to negate the x axis and change the winding order
			// RightHandedToLeftHanded(mesh_neg);
		}
		
		
		*/
		
		
		// Works for a single mesh
		//GenerateMesh.RegularGM(_grid, threshold, _delta, _origin, colors, tag);
		
		
		GenerateMesh.CreateSurfaceObjects(_grid, threshold, _delta, _origin, colors, tag, true);
		
			
		points = null;
		GC.GetTotalMemory(true);
		GC.Collect();
		Debug.Log("Exiting :: finished marching cubes");

		GameObject[] iso_pos = GameObject.FindGameObjectsWithTag(tag);
		foreach(GameObject iso in iso_pos)
		{
			iso.transform.parent = GameObject.Find("ElectIsoManager").transform;
			Mesh mesh_pos = iso.GetComponent<MeshFilter>().mesh;
			// //Unity has a left-handed coordinates system while Molecular obj are right-handed
			// //So we have to negate the x axis and change the winding order
			RightHandedToLeftHanded(mesh_pos);
			if(transparency) {
				// TransparentZ is a custom shader that uses transparent objects, but with per-triangle transparency sorting,
				// versus Unity's default per-object transparency sorting. This is necessary because of our intersecting,
				// non-convex transparent objects.
				iso.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Zsorted");
				color.a = 0.5f;
			}
			else {
				iso.GetComponent<Renderer>().material.shader = Shader.Find("Diffuse");
				//iso.renderer.material.shader =	Shader.Find("Mat Cap Cut");
				//iso.renderer.material.SetTexture("_MatCap",(Texture)Resources.Load("lit_spheres/divers/daphz1"));
			}
			iso.GetComponent<Renderer>().material.SetColor("_Color", color);
		}
//		MCInstance.MCRecMain(NX, NY, NZ, 0.5f, points, 0f);

		
//		Vector4[] points;
//		points = new Vector4[ (X+1) * (X+1) * (Z+1)];
//
//		for (int i = 0; i < Z; i++) {
//			for (int j = 0; j < X; j++) {
//				for (int k = 0; k < X; k++) {
//					
//					points[i*(X)*(X) + j*(X) + k] = new Vector4 (i, j, k, _grid[k][j][i]);
////					pos += 4;
//				}
//			}
//		}
//		
//		MarchingCubesRec MCInstance;
//		MCInstance = new MarchingCubesRec();
//		MCInstance.MCRecMain(0, Z, X, X, 2.0f, points, 0);
//		
//	
		
//		Vector4[] points;
//		points = new Vector4[ 129 * 129 * 129];
//
//		for (int i = 0; i < 128; i++) {
//			for (int j = 0; j < 128; j++) {
//				for (int k = 0; k < 128; k++) {
//					
//					points[i*(128)*(128) + j*(128) + k] = new Vector4 (i, j, k, _grid[k][j][i]);
////					pos += 4;
//				}
//			}
//		}
//		
//		MarchingCubesRec MCInstance;
//		MCInstance = new MarchingCubesRec();
//		MCInstance.MCRecMain(0, 128, 128, 128, -10.0f, points, 0);
		
	}



}
