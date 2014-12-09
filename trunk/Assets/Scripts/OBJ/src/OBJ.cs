/// @file OBJ.cs
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
/// $Id: OBJ.cs 317 2013-06-24 13:32:30Z erwan $
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

// this code is from http://www.everyday3d.com/blog/index.php/2010/05/24/loading-3d-models-runtime-unity3d/
// which was released under the MIT license
// Modified by Da silva franck IBPC Paris
//       to open .obj file in a local Disk

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;	
using System.Text.RegularExpressions;
using System.IO;
using ParseData.IParsePDB;
using System.Net;
using Molecule.Model;
using Molecule.Control;
using System.Xml;
using UI;
public class OBJ : MonoBehaviour {
	
	public string objPath;
	private TextReader sr;
	private TextReader mtl_reader = null;
	int compteurvertice = 0;

	/* OBJ file tags */
	private const string O 	= "o";
	private const string G 	= "g";
	private const string V 	= "v";
	private const string VT = "vt";
	private const string VN = "vn";
	private const string F 	= "f";
	private const string MTL = "mtllib";
	private const string UML = "usemtl";

	/* MTL file tags */
	private const string NML = "newmtl";
	private const string NS = "Ns"; // Shininess
	private const string KA = "Ka"; // Ambient component (not supported)
	private const string KD = "Kd"; // Diffuse component
	private const string KS = "Ks"; // Specular component
	private const string D = "d"; 	// Transparency (not supported)
	private const string TR = "Tr";	// Same as 'd'
	private const string ILLUM = "illum"; // Illumination model. 1 - diffuse, 2 - specular
	private const string MAP_KD = "map_Kd"; // Diffuse texture (other textures are not supported)
	
//	private string basepath;
	private string mtllib;
	private GeometryBuffer buffer;
	
	
	public OBJ (){
		buffer = new GeometryBuffer ();
		sr = null;
		mtl_reader = null;
	}

	//Construct from the file content
	public OBJ(TextReader reader) : this()
	{
		sr = reader;
		mtl_reader = null;
	}
	//Construct from the file content
	public OBJ(TextReader reader, TextReader mtl_reader) : this(reader)
	{
		this.mtl_reader = mtl_reader;
	}

	//Construct from a file path
	public OBJ(string path) : this()
	{
		sr = new StreamReader(path);
		mtl_reader = null;
	}

	//Construct from a file path
	public OBJ(string path, string mtl_path) : this(path)
	{
		mtl_reader = new StreamReader(mtl_path);
	}
	
	
	public void Load() {
        string text;
       	using (sr)
        {              
            text = sr.ReadToEnd();                 
       	}
		
		SetGeometryData(text);

		if(hasMaterials && mtl_reader != null) {
//			Debug.Log("J'ai du matos");
//			loader = new WWW(basepath + mtllib);
//			yield return loader;
			//string[] mtlpath = objPath.Split("."[0]);
			//sr=new StreamReader(mtlpath[0]+".mtl");		
       		using (mtl_reader)
        	{              
            	text = mtl_reader.ReadToEnd();
//				Debug.Log("Jai lu du matos");

       		}
			
			SetMaterialData(text);
			Debug.Log("Jai set du matos");

			foreach(MaterialData m in materialData) {
				
				if(m.diffuseTexPath != null) {
					Debug.Log(m.diffuseTexPath+"Oups");
//					WWW texloader = new WWW(basepath + m.diffuseTexPath);
					WWW texloader = new WWW(m.diffuseTexPath);
//					yield return texloader;
					m.diffuseTex = texloader.texture;
				}
			}
		}
		
		Build();

	}

	private void SetGeometryData(string data) {
		string[] lines = data.Split("\n".ToCharArray());
		
		for(int i = 0; i < lines.Length; i++) {
			string l = lines[i];
			
			if(l.IndexOf("#") != -1) l = l.Substring(0, l.IndexOf("#"));
			string[] p = l.Split(" ".ToCharArray());
			
			switch(p[0]) {
				case O:
					buffer.PushObject(p[1].Trim());
					break;
				case G:
					buffer.PushGroup(p[1].Trim());
					break;
				case V:
					//Unity has a left-handed coordinates system while Molecular OBJs are right-handed
					//So we have to negate the X coordinates
					buffer.PushVertex( new Vector3( -cf(p[1]), cf(p[2]), cf(p[3]) ) );
					compteurvertice++;
					break;
				case VT:
					buffer.PushUV( new Vector2( cf(p[1]), cf(p[2]) ));
					break;
				case VN:
					//Unity has a left-handed coordinates system while Molecular OBJs are right-handed
					//So we have to negate the X coordinates
					//Here it affects only light ! The winding reverse is in GeometryBuffer::PopulateMeshes
					Vector3 norm = new Vector3( -cf(p[1]), cf(p[2]), cf(p[3]) );
					norm.Normalize();
					buffer.PushNormal( norm );
					break;
				case F:
					for(int j = 1; j < p.Length; j++) {
						string[] c = p[j].Trim().Split("/".ToCharArray());
//						Debug.Log("" +p[j]);
						FaceIndices fi = new FaceIndices();
						fi.vi = ci(c[0])-1;	
						if(c.Length > 1 && c[1] != "") fi.vu = ci(c[1])-1;
						if(c.Length > 2 && c[2] != "") fi.vn = ci(c[2])-1;
//						Debug.Log("vi "+fi.vi+" vu "+fi.vu+ " vn "+fi.vn);
						buffer.PushFace(fi);
					}
					break;
				case MTL:
					if(mtl_reader != null)
						mtllib = p[1].Trim();
					break;
				case UML:
					if(mtl_reader != null)
						buffer.PushMaterialName(p[1].Trim());
					break;
			}
		}
		
		// buffer.Trace();
	}
	
	// via profiler, discovered that Convert.ToSingle and Convert.ToInt32
	// are very slow... so using float.Parse and int.Parse instead!
	//
	// memory usage during obj loading went down from 56 MB to 6.8 MB (8.2x improvement)
	// and load time went down 1742ms to 514ms (3.4x improvement)
	
	/*
	private float cf(string v) {
		return Convert.ToSingle(v.Trim(), new CultureInfo("en-US"));
	}
	
	private int ci(string v) {
		return Convert.ToInt32(v.Trim(), new CultureInfo("en-US"));
	}
	*/
	
	private float cf(string v) {
		return float.Parse(v); 
	} 
	
	private int ci(string v) {
		return int.Parse(v); 
	}
	
	
	private bool hasMaterials {
		get {
			return mtllib != null;
		}
	}
	
	/* ############## MATERIALS */
	private List<MaterialData> materialData;
	private class MaterialData {
		public string name;
		public Color ambient;
   		public Color diffuse;
   		public Color specular;
   		public float shininess;
   		public float alpha;
   		public int illumType;
   		public string diffuseTexPath;
   		public Texture2D diffuseTex;
	}
	
	private void SetMaterialData(string data) {
		string[] lines = data.Split("\n".ToCharArray());
		
		materialData = new List<MaterialData>();
		MaterialData current = new MaterialData();
		
		for(int i = 0; i < lines.Length; i++) {
			string l = lines[i];
			
			if(l.IndexOf("#") != -1) l = l.Substring(0, l.IndexOf("#"));
			string[] p = l.Split(" ".ToCharArray());
			
			switch(p[0]) {
				case NML:
					current = new MaterialData();
					current.name = p[1].Trim();
					materialData.Add(current);
					break;
				case KA:
					current.ambient = gc(p);
					break;
				case KD:
					current.diffuse = gc(p);
					break;
				case KS:
					current.specular = gc(p);
					break;
				case NS:
					current.shininess = cf(p[1]) / 1000;
					break;
				case D:
				case TR:
					current.alpha = cf(p[1]);
					break;
				case MAP_KD:
					current.diffuseTexPath = p[1].Trim();
					break;
				case ILLUM:
					current.illumType = ci(p[1]);
					break;
					
			}
		}	
	}
	
	private Material GetMaterial(MaterialData md) {
		Material m;
		
		
		// commenter le 24 
		
		if(md.illumType == 2) {
			m =  new Material(Shader.Find("Bumped Specular cut"));
//			m =  new Material(Shader.Find("Diffuse"));
			m.SetColor("_Color", Color.black);
			m.SetFloat("_Shininess", md.shininess);
//			m.SetFloat("_Shininess", 10);
		} else {
			m =  new Material(Shader.Find("Diffuse"));
		}

		m.SetColor("_Color", md.diffuse);
		
		if(md.diffuseTex != null) m.SetTexture("_MainTex", md.diffuseTex);
		
		return m;
	}
	
	private Color gc(string[] p) {
		return new Color( cf(p[1]), cf(p[2]), cf(p[3]) );
	}

	private void Build() {
		Dictionary<string, Material> materials = new Dictionary<string, Material>();
		Material m;
		
		if(hasMaterials && mtl_reader != null) {
			Debug.Log("Obj import :: MATERIAL read");
			foreach(MaterialData md in materialData) {
				materials.Add(md.name, GetMaterial(md));
			}
		} else {
			Debug.Log("Obj import :: NO MATERIAL read");
			m = new Material(Shader.Find("Mat Cap Cut"));
			materials.Add("default", m);
			m.SetTexture("_MatCap", (Texture)Resources.Load("lit_spheres/divers/daphz1"));
		}
		
		GameObject[] ms = new GameObject[buffer.numObjects];
		
		if(buffer.numObjects == 1) {
			ms[0] = new GameObject("SurfaceOBJ");
			ms[0].name="SurfaceOBJ";
			ms[0].tag="SurfaceManager";
			Debug.Log("New OBJ surface generated");
			ms[0].transform.parent = GameObject.Find("SurfaceManager").transform;
			ms[0].AddComponent(typeof(MeshFilter));
			ms[0].AddComponent(typeof(MeshRenderer));
//			ms[0] = gameObject;
		} else if(buffer.numObjects > 1) {
			for(int i = 0; i < buffer.numObjects; i++) {
				GameObject go = new GameObject();
				go.transform.parent = gameObject.transform;
				go.AddComponent(typeof(MeshFilter));
				go.AddComponent(typeof(MeshRenderer));
				ms[i] = go;
			}
		}
		Debug.Log("nb vertice :" + compteurvertice);
		buffer.PopulateMeshes(ms, materials);
	}
	
	public void killCurentSurface()
	{
		GameObject Surfacemanager;
		Surfacemanager = GameObject.Find("SurfaceOBJ");
		Debug.Log ("surface trouvee");
		Destroy(Surfacemanager);
		
	}
}