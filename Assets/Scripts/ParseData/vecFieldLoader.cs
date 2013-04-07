/// @file vecFieldLoader.cs
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
/// $Id: vecFieldLoader.cs 225 2013-04-07 14:21:34Z baaden $
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

using System.IO;
using System.Net;
using UnityEngine;
using System.Collections;
using Molecule.Model;

public class vecFieldLoader : MonoBehaviour {
	
	public Grid grid;
	
	public class Grid {
		private Vector3[,,] _grid;
		private GameObject[] _lines;
		
		public Grid(int x, int y, int z){
			_grid = new Vector3[x,y,z];
			_lines = new GameObject[y*x/2+1];
		}
		
		public void init(string vecFieldName)
		{
			StreamReader sr = null;
			#if UNITY_WEBPLAYER
				HttpWebRequest request =(HttpWebRequest) WebRequest.Create(vecFieldName);
				// If required by the server, set the credentials.
				request.Credentials = CredentialCache.DefaultCredentials;
				// Get the response.
			    HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
			    // Get the stream containing content returned by the server.
	     	    Stream dataStream = response.GetResponseStream ();
	            // Open the stream using a StreamReader for easy access.
	            sr = new StreamReader (dataStream);
			#else
				FileInfo file = new FileInfo(vecFieldName);
				sr = file.OpenText();
			#endif

			string line = " ";
//			int ind = 0;
			for(int i=0; i<_grid.GetLength(0); i++){
				for(int j=0; j<_grid.GetLength(1); j++){
					for(int k=0; k<_grid.GetLength(2); k++){
						line = sr.ReadLine();
						string[] vals = line.Split(' ');
						_grid[i,j,k] = new Vector3(float.Parse(vals[0]),
												   float.Parse(vals[1]),
												   float.Parse(vals[2]));
						
//						_lines[ind] = new GameObject();
						//Debug.Log(_grid[i,j,k]);
//						ind ++;
					}
				}
			}
			
		}
		public void init2()
		{

//			int ind = 0;
			ReadDX dx = GameObject.Find("LoadBox").GetComponent<ReadDX>();
			for(int i=0; i<_grid.GetLength(0)-2; i++){
				for(int j=0; j<_grid.GetLength(1)-2; j++){
					for(int k=0; k<_grid.GetLength(2)-2; k++){
						_grid[i,j,k] = dx.getGradient(i,j,k);
						
//						if (i== 50)
//							("gradient : "+ReadDX.getGradient(i,j,k)+" grid : " + _grid[i,j,k]);
						
//						ind ++;
					}
				}
			}
			
		}
		
		public void displayVec(){
			int ind = 0;
			Color c1 = Color.white;
			Color c2 = Color.red;
			
			for(int i=0; i<_grid.GetLength(0); i++){
				for(int j=0; j<_grid.GetLength(1); j++){
					for(int k=0; k<_grid.GetLength(2); k++){
						Vector3 pos = new Vector3(i*0.5f,j*0.5f,k*0.5f);
						LineRenderer lr = _lines[ind].AddComponent<LineRenderer>();
						lr.SetVertexCount(2);
						lr.SetPosition(0,pos);
						lr.SetPosition(1,pos+_grid[i,j,k]);
						lr.SetWidth(0.05F, 0.05F);
						lr.material = new Material(Shader.Find("Particles/Additive"));
						lr.SetColors(c1,c2);
						ind ++;
					}
				}
			}
		}
		
		public bool inside(Vector3 pos){
			int X = Mathf.CeilToInt(pos.x);
			int Y = Mathf.CeilToInt(pos.y);
			int Z = Mathf.CeilToInt(pos.z);
			if(X > _grid.GetLength(0) || X < 0)
				return false;
			if(Y > _grid.GetLength(1) || Y < 0)
				return false;
			if(Z > _grid.GetLength(2) || Z < 0)
				return false;
			return true;
		}
		
		
		public void displayStreamline(float x, float y, float z,Vector3 delta, Vector3 origin)
		{
			Color c1 = Color.green;
			Color c2 = Color.blue;
			GameObject stream = new GameObject();
//			Vector3 pos = new Vector3(x,y,z);
			LineRenderer lr = stream.AddComponent<LineRenderer>();
			lr.SetWidth(0.05F, 0.05F);
			lr.material = new Material(Shader.Find("Particles/Additive"));
			lr.SetColors(c1,c2);
			
			Vector3 x0, vel;
//			ArrayList xi = new ArrayList();
			float minVel = 0.05f;
			
			Vector3[] xj;
			xj = new Vector3[300];
			int taille = 0;
			
			x0 = new Vector3(x,y,z);
			do{
//				xi.Add(x0);
				xj[taille] = x0;
				
				vel = _grid[(int)x0.x,(int)x0.y,(int)x0.z];
//				("vel : "+vel.x); 									// print here
				x0 = x0 + (vel/1.0f);
				taille++;
			}while(inside(x0) && vel.magnitude > minVel && taille < 299); 
			
			lr.SetVertexCount(taille);
//			lr.SetVertexCount(xi.Count);
//			for(int i=0; i<xi.Count; i++){
//				Vector3 pos2 = (Vector3)xi[i];
////				("position" + pos2);
//				Vector3 posi = new Vector3((origin.x + (pos2.x* delta.x)) , (origin.y+(pos2.y* delta.y)) , (origin.z+(pos2.z* delta.z))) + MoleculeModel.Offset;
////				("position" + posi);
//				lr.SetPosition(i , posi);
////				lr.SetPosition(i, (Vector3)xi[i]);
//			}	
			for(int i=0; i<taille; i++){
				Vector3 pos2 = xj[i];
				Vector3 posi = new Vector3((origin.x + (pos2.x* delta.x)+MoleculeModel.Offset.x) , (origin.y+(pos2.y* delta.y)+MoleculeModel.Offset.y) , (origin.z+(pos2.z* delta.z))+MoleculeModel.Offset.z);
				lr.SetPosition(i,posi);				
			}
		}
		
	}
	
	// Use this for initialization
	 public void CalcFieldline(int X,int Y, int Z,Vector3 delta, Vector3 origin) {
		System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
		grid = new Grid(X-2,Y-2,Z-2);
//		grid.init("/ibpc/sonic/alex/dxGradient/test");
		grid.init2();
//		grid.displayVec();
		
		// generation de ligne a partir de plan dans la grille
//		for(int i=0; i< X-2; i+=2)
//			for(int j=0; j< Z-2; j+=2){
//				grid.displayStreamline(i,i,j,delta,origin);
//				grid.displayStreamline(j,i,j,delta,origin);
//		}
		
		
		// generation de ligne a partir d'un points sur 10 de la surface moleculaire
		for (int i=0; i<MoleculeModel.vertices.Length; i+=10){
			grid.displayStreamline((int)(MoleculeModel.vertices[i].x-origin.x),(int)(MoleculeModel.vertices[i].y-origin.y),(int)(MoleculeModel.vertices[i].z-origin.z),delta,origin);
			}
	}

}
