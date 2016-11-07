using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Molecule.Model;

public class QuadAtoms : MonoBehaviour {
	private List<float[]> atoms; // could be static at this point, though not for long
	private List<AtomModel> typeList;
	private static float radius = 0.2f;
	public Material mat;
	public Camera mCamera;
//	private bool init = false;
//	private GameObject[] planes;
	private GameObject[] spheres;
	Vector3 cX, cY;

	// Use this for initialization
	public void Init () {
		Debug.Log("Hi! I'm a Quad Renderer!");
		atoms = MoleculeModel.atomsLocationlist;
		typeList = MoleculeModel.atomsTypelist;
		mat = new Material(Shader.Find("Transparent/Cutout/Diffuse"));
		mat.color = Color.blue;
		mCamera = Camera.main;
		cX = radius * (mCamera.transform.rotation * Vector3.right);
		cY = radius * (mCamera.transform.rotation * Vector3.up);
//		transform.LookAt(transform.position + mCamera.transform.rotation * Vector3.back, mCamera.transform.rotation * Vector3.up);
//		planes = new GameObject[atoms.Count];
		spheres = new GameObject[atoms.Count];
		for(int i=0; i<atoms.Count; i++)
		{
//			DrawSphere(i);
			DrawPlane(i);
		}
//		init = true;
	}
	
	private void DrawPlane(int i)
	{
		float[] atom = atoms[i];
		Color col = typeList[i].baseColor;
		float x = atom[0];
		float y = atom[1];
		float z = atom[2];
		
		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		
		Vector3 pos = new Vector3(x,y,z);
		Vector3[] vertices = new Vector3[4];
		
		
		vertices[0] = pos - cX + cY;
		vertices[1] = pos + cX + cY;
		vertices[2] = pos - cX - cY;
		vertices[3] = pos + cX - cY;
		
		mesh.vertices = vertices;
		
		int[] triangles = new int[6];
		
		// Top left
		triangles[0] = 0;
		triangles[1] = 1;
		triangles[2] = 2;
		
		// Bottom right
		triangles[3] = 1;
		triangles[4] = 2;
		triangles[5] = 3;
		
		mesh.triangles = triangles;
		
		Vector3[] normals = new Vector3[4];

		normals[0] = -Vector3.forward;
		normals[1] = -Vector3.forward;
		normals[2] = -Vector3.forward;
		normals[3] = -Vector3.forward;
		
		mesh.normals = normals;
		
		Vector2[] uv = new Vector2[4];

		uv[0] = new Vector2(0, 0);
		uv[1] = new Vector2(1, 0);
		uv[2] = new Vector2(0, 1);
		uv[3] = new Vector2(1, 1);
		
		mesh.uv = uv;
		
		Color[] colors = new Color[4];
		for(int j=0; i<4; i++)
			colors[j] = col;
		
		mesh.colors = colors;
			
/*
		GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.transform.Translate(new Vector3(x,y,z));
		plane.transform.localScale = new Vector3(radius, radius, radius);
		//plane.transform.Rotate(new Vector3(-90,0,0));
		plane.transform.parent = this.transform;
		plane.renderer.material.color = col;
		planes[i] = plane; 
*/
	}
	
	
	private void DrawSphere(int i)
	{
		float[] atom = atoms[i];
		Color col = typeList[i].baseColor;
		float x = atom[0];
		float y = atom[1];
		float z = atom[2];
		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.Translate(new Vector3(x,y,z));
		sphere.transform.parent = this.transform;
		sphere.GetComponent<Renderer>().material.color = col;
		spheres[i] = sphere;
	}
	
}
