/// @file GeometryBuffer.cs
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
/// $Id: GeometryBuffer.cs 227 2013-04-07 15:21:09Z baaden $
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
using System.Collections.Generic;
using Molecule.Model;

public class GeometryBuffer {
	
	int vertextot= 0;
	private List<ObjectData> objects;
	public List<Vector3> vertices;
	public List<Vector2> uvs;
	public List<Vector3> normals;
	
	private ObjectData current;
	private class ObjectData {
		public string name;
		public List<GroupData> groups;
		public List<FaceIndices> allFaces;
		public ObjectData() {
			groups = new List<GroupData>();
			allFaces = new List<FaceIndices>();
		}
	}
	

	
	
	private GroupData curgr;
	private class GroupData {
		public string name;
		public string materialName;
		public List<FaceIndices> faces;
		public GroupData() {
			faces = new List<FaceIndices>();
		}
		public bool isEmpty { get { return faces.Count == 0; } }
	}
	
	public GeometryBuffer() {
		objects = new List<ObjectData>();
		ObjectData d = new ObjectData();
		d.name = "default";
		objects.Add(d);
		current = d;
		
		GroupData g = new GroupData();
		g.name = "default";
		g.materialName = "default";
		d.groups.Add(g);
		curgr = g;
		
		vertices = new List<Vector3>();
		uvs = new List<Vector2>();
		normals = new List<Vector3>();
	}
	
	public void PushObject(string name) {
		//Debug.Log("Adding new object " + name + ". Current is empty: " + isEmpty);
		if(isEmpty) objects.Remove(current);
		
		ObjectData n = new ObjectData();
		n.name = name;
		objects.Add(n);
		
		GroupData g = new GroupData();
		g.name = "default";
		g.materialName = "default";
		n.groups.Add(g);
		
		curgr = g;
		current = n;
	}
	
	public void PushGroup(string name) {
		if(curgr.isEmpty) current.groups.Remove(curgr);
		GroupData g = new GroupData();
		g.name = name;
		g.materialName = "default";
		current.groups.Add(g);
		curgr = g;
	}
	
	public void PushMaterialName(string name) {
		Debug.Log("Pushing new material " + name + " with curgr.empty=" + curgr.isEmpty);
		if(!curgr.isEmpty) PushGroup(name);
		if(curgr.name == "default") curgr.name = name;
		curgr.materialName = name;
	}
	
	public void PushVertex(Vector3 v) {
		vertices.Add(v);
		vertextot++;
	}
	
	public void PushUV(Vector2 v) {
		uvs.Add(v);
	}
	
	public void PushNormal(Vector3 v) {
		normals.Add(v);
	}
	
	public void PushFace(FaceIndices f) {
		curgr.faces.Add(f);
		current.allFaces.Add(f);

	}
	
	public void Trace() {
		Debug.Log("OBJ has " + objects.Count + " object(s)");
		Debug.Log("OBJ has " + vertices.Count + " vertice(s)");
		Debug.Log("OBJ has " + uvs.Count + " uv(s)");
		Debug.Log("OBJ has " + normals.Count + " normal(s)");
		foreach(ObjectData od in objects) {
			Debug.Log(od.name + " has " + od.groups.Count + " group(s)");
			foreach(GroupData gd in od.groups) {
				Debug.Log(od.name + "/" + gd.name + " has " + gd.faces.Count + " faces(s)");
			}
		}
		
	}
	
	public int numObjects { get { return objects.Count; } }	
	public bool isEmpty { get { return vertices.Count == 0; } }
//	public bool hasUVs { get { return uvs.Count == 0; } }
	public bool hasUVs = true;
	public bool hasNormals { get { return normals.Count > 0; } }
	
	public void PopulateMeshes(GameObject[] gs, Dictionary<string, Material> mats) {
		if(gs.Length != numObjects) return; // Should not happen unless obj file is corrupt...
		
		for(int i = 0; i < gs.Length; i++) {

			ObjectData od = objects[i];
			
			if(od.name != "default") gs[i].name = od.name;
			
//			Debug.Log("triangle tot:"+ triangletot);
			Vector3[] tvertices = new Vector3[vertextot];
			Vector2[] tuvs = new Vector2[vertextot];
			Vector3[] tnormals = new Vector3[vertextot];
			
			int[] triangles = new int[od.allFaces.Count];
			
			//Calcul of baricenter of the surface
			float barix=0;
			float bariy=0;
			float bariz=0;
//
			for (int bari =0; bari< vertextot;bari++){
				barix = barix + vertices[bari].x;
//				Debug.Log("t " + vertices[bari]);
				bariy = bariy + vertices[bari].y;
				bariz = bariz + vertices[bari].z;
			}
//			Debug.Log(" baricentre: "+ barix +" "+ bariy+ " "+ bariz);
//
			barix = barix/vertextot;
			bariy = bariy/vertextot;
			bariz = bariz/vertextot;
			Debug.Log("Surface baricentre coords :: "+ barix +" "+ bariy+ " "+ bariz);
			
			int k=0;
//			float pas = 1f/214f;
//			float temp1=pas;
//			float temp2=pas;
//			int compteur=1;
			for (int l=0; l<vertextot;l++){
				tvertices[l] = vertices[l];
				tvertices[l].x = vertices[l].x;//-barix;
				tvertices[l].y = vertices[l].y;//-bariy;
				tvertices[l].z = vertices[l].z;//-bariz;
				tnormals[l]= normals[l];
				
				//Calcul de l'uv map non fonctionnel
				
//				tuvs[k]=new Vector2(temp1,temp2);
//				Debug.Log("Vector : "+tuvs[k] +" pas : " +pas + "compteur :" +compteur);
//				compteur++;
//				temp1 += pas;
//				if (temp1 >0.99){
//					temp2 += pas;
//					temp1= pas;
//				}
				
			}
			foreach(FaceIndices fi in od.allFaces) {
				triangles[k] = fi.vi;
//				tvertices[k] = vertices[fi.vi];
//				if(hasUVs) tuvs[k] = uvs[fi.vu];
//				if(hasNormals) tnormals[k] = normals[fi.vn];
				k++;
			}
		
			Vector3 baricenter = new Vector3(barix,bariy,bariz);
			Debug.Log("Obj center = " + baricenter);
			gs[i].transform.localPosition = gs[i].transform.localPosition + MoleculeModel.Offset;
			gs[i].transform.localPosition = gs[i].transform.localPosition + MoleculeModel.Center;
//			gs[i].transform.localPosition = gs[i].transform.localPosition + MoleculeModel.Offset;
//			Debug.Log(" baricentre: "+ barix +" "+ bariy+ " "+ bariz);
			Mesh m = (gs[i].GetComponent(typeof(MeshFilter)) as MeshFilter).mesh;
			m.vertices = tvertices;
			MoleculeModel.vertices = tvertices;
			if(hasUVs) m.uv = tuvs;
			if(hasNormals) m.normals = tnormals;
		
			//Unity has a left-handed coordinates system while Molecular obj are right-handed
			//So we have to change the winding order
			for(int tri=0; tri<triangles.Length; tri=tri+3)
		    {
		        int tmp = triangles[tri];
		        triangles[tri] = triangles[tri+2];
		        triangles[tri+2] = tmp;
		    }

			if(od.groups.Count == 1) {
				GroupData gd = od.groups[0];
				Debug.Log("Mesh material used :: "+gd.materialName);
				gs[i].GetComponent<Renderer>().material = mats[gd.materialName];
				
//				int[] triangles = new int[gd.faces.Count];
//				for(int j = 0; j < triangles.Length; j++) triangles[j] = j;
				
				m.triangles = triangles;
				
			} //else {
//				int gl = od.groups.Count;
//				Material[] sml = new Material[gl];
//				m.subMeshCount = gl;
//				int c = 0;
//				
//				for(int j = 0; j < gl; j++) {
//					sml[j] = mats[od.groups[j].materialName]; 
//					int[] triangles = new int[od.groups[j].faces.Count];
//					int l = od.groups[j].faces.Count + c;
//					int s = 0;
//					for(; c < l; c++, s++) triangles[s] = c;
//					m.SetTriangles(triangles, j);
//				}
//				
//				gs[i].renderer.materials = sml;
//			}
		}
	}
}


