/// @file GenerateMesh.cs
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
/// $Id: GenerateMesh.cs 227 2013-04-07 15:21:09Z baaden $
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
using System.Collections.Generic;
using System;
using Molecule.Model;

public class GenerateMesh {

//		public static void GM (Vector3[] Mnormals, Vector3[] Mvertices, int[] Mtriangles, int centrage) {
	
	
		public void GM (Vector3[] Mvertices, int[] Mtriangles, Vector3 center, int surfaceNb, Color[] Colors, string tag="SurfaceManager") 
		{
				Debug.Log("Entering :: GM: "+tag);		
				GameObject GOSurface = new GameObject("SurfaceOBJ");
				GOSurface.tag = tag;
				GOSurface.transform.parent = GameObject.Find("SurfaceManager").transform;
			    Mesh mesh = new Mesh();
				GOSurface.AddComponent<MeshFilter>();
				GOSurface.AddComponent<MeshRenderer>();
				GOSurface.GetComponent<MeshFilter>().mesh = mesh;
	//			Material SurfaceShader = Resources.Load("SurfaceShader", typeof(Material)) as Material;
	//			GOSurface.renderer.material = SurfaceShader;
//				GOSurface.renderer.material = new Material(Shader.Find("Mat Cap Cut"));
				GOSurface.renderer.material = new Material(Shader.Find("Vertex Colored"));
//				GOSurface.renderer.material.SetTexture("_MatCap",(Texture)Resources.Load("graypic/bruckner"));
				GOSurface.transform.localPosition = center;
				mesh.Clear();
			
				// //Unity has a left-handed coordinates system while Molecular obj are right-handed
				// //So we have to negate the x axis and change the winding order
				// for(int i=0; i < Mvertices.Length; i++)
				// {
				// 	Mvertices[i].x = -Mvertices[i].x;
				// }
				
				// for(int tri=0; tri<Mtriangles.Length; tri=tri+3)
			 //    {
			 //        int tmp = Mtriangles[tri];
			 //        Mtriangles[tri] = Mtriangles[tri+2];
			 //        Mtriangles[tri+2] = tmp;
			 //    }
				mesh.vertices = Mvertices;
				MoleculeModel.vertices= Mvertices;
				Debug.Log("Exiting :: vertices filled");
				mesh.triangles = Mtriangles;
				mesh.colors = Colors;
				mesh.RecalculateBounds();
//				mesh.normals = Mnormals;
				mesh.RecalculateNormals();
//				mesh.Optimize();

		}
	

}
