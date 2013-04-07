/// @file PDBtoDEN.cs
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
/// $Id: PDBtoDEN.cs 227 2013-04-07 15:21:09Z baaden $
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
using System;
using Molecule.Model;
using UI;

public class PDBtoDEN : MonoBehaviour
{
	static float[,,] GridS;
	static Color[,,] VertColor;
	public static Color[] colors;

	public static int X;
	public static int Y;
	public static int Z;
	public static Vector3 delta = new Vector3(2f,2f,2f);
	public static Vector3 origin = new Vector3(MoleculeModel.MinValue.x,
											   MoleculeModel.MinValue.y,
		 									   MoleculeModel.MinValue.z);
	// Use this for initialization
	void Start ()
	{
	
	}

	public void TranPDBtoDEN(){
		
		
//		if (MoleculeModel.MinValue.x >0)
//			MoleculeModel.MinValue.x = 0;
//		if (MoleculeModel.MinValue.y >0)
//			MoleculeModel.MinValue.y = 0;
//		if (MoleculeModel.MinValue.z >0)
//			MoleculeModel.MinValue.z = 0;
		Debug.Log("Entering :: Generation of density from PDB");
		X = (int)(((MoleculeModel.MaxValue.x-MoleculeModel.MinValue.x)*2)+40);
		Y = (int)(((MoleculeModel.MaxValue.y-MoleculeModel.MinValue.y)*2)+40);
		Z = (int)(((MoleculeModel.MaxValue.z-MoleculeModel.MinValue.z)*2)+40);

		
		Debug.Log("Density point X,Y,Z :: "+ X+","+Y+","+Z);
		Debug.Log("Density minValue :: " + MoleculeModel.MinValue);
		GridS = new float[X,Y,Z];
		VertColor = new Color[X,Y,Z];
		
		int i;
		int j;
		int k;
		float Dist;
		float bfactor;
		int atomnumber=0;
		Color atomColor;
		string type;
		float density;
		
		float maxValue =(float)MoleculeModel.BFactorList[0];
		foreach (float f in MoleculeModel.BFactorList)
   			if (f>maxValue)
     			maxValue =f;
		
		for (i=0;i<X;i++)
			for(j=0;j<Y;j++)
				for(k=0;k<Z;k++)
					VertColor[i,j,k].b=1f;
		
		foreach (float[] coord in MoleculeModel.atomsLocationlist){
//		foreach (float[] coord in MoleculeModel.CatomsLocationlist){
			// i = Mathf.RoundToInt((coord[0]-MoleculeModel.MinValue.x-MoleculeModel.Offset.x)*2)+18;
			// j = Mathf.RoundToInt((coord[1]-MoleculeModel.MinValue.y-MoleculeModel.Offset.y)*2)+18;
			// k = Mathf.RoundToInt((coord[2]-MoleculeModel.MinValue.z-MoleculeModel.Offset.z)*2)+18;
			i = Mathf.RoundToInt((coord[0]-MoleculeModel.MinValue.x)*2)+18;
			j = Mathf.RoundToInt((coord[1]-MoleculeModel.MinValue.y)*2)+18;
			k = Mathf.RoundToInt((coord[2]-MoleculeModel.MinValue.z)*2)+18;
//			Debug.Log("i,j,k : " + i +","+j+","+k);
//			GridS[i,j,k]=2;
//		}
			type = (MoleculeModel.atomsTypelist[atomnumber] as AtomModel).type;
//			type = "C";
//			Debug.Log("i j k : "+i+","+j+","+k);
			// Vector3 v1 = new Vector3((coord[0]-MoleculeModel.MinValue.x-MoleculeModel.Offset.x)*2+18,
			// 						 (coord[1]-MoleculeModel.MinValue.y-MoleculeModel.Offset.y)*2+18,
			// 						 (coord[2]-MoleculeModel.MinValue.z-MoleculeModel.Offset.z)*2+18);
			Vector3 v1 = new Vector3((coord[0]-MoleculeModel.MinValue.x)*2+18,
									 (coord[1]-MoleculeModel.MinValue.y)*2+18,
									 (coord[2]-MoleculeModel.MinValue.z)*2+18);
			float AtomRadius = 1f;
			
			 // possibilité de créer une liste a la lecture du pdb et de la reprendre ici. Comme cela on peut lire d'autre propriété biologique
			switch(type)
				{
				case "C": 
					AtomRadius =3.4f;
					atomColor = MoleculeModel.carbonColor;
					break;
				case "N": 
					AtomRadius =3.1f;
					atomColor =MoleculeModel.nitrogenColor;
					break;	
				case "O": 
					AtomRadius =3.04f;
					atomColor = MoleculeModel.oxygenColor;
					break;
				case "S": 
					AtomRadius =4.54f;
					atomColor = MoleculeModel.sulphurColor;
					break;
				case "P": 
					AtomRadius =3.6f;
					atomColor =		MoleculeModel.phosphorusColor;

					break;
				case "H": 
					AtomRadius =2.4f;
					atomColor = MoleculeModel.hydrogenColor;

					break;
				default: 
					AtomRadius =2f;
					atomColor = MoleculeModel.unknownColor;
					break;
				
					}
			
			if (UIData.toggleSurf){
			for (int l = i-8 ;l < i+9 ; l++)
					for ( int m = j-8 ; m < j+9 ; m++)
						for ( int n = k-8 ; n < k+9 ; n++){
							Vector3 v2 = new Vector3(l,m,n);
							Dist = Vector3.Distance(v1,v2);
							density = (float)Math.Exp(-((Dist/AtomRadius)*(Dist/AtomRadius)));
							if (density > GridS[l,m,n])
//							if (VertColor[l,m,n].b!=0){
//								if (density > 0.5)
									VertColor[l,m,n] = atomColor;
							GridS[l,m,n] += density;
							}
			
			}else if (UIData.toggleBfac){			
			for (int l = i-8 ;l < i+9 ; l++)
				for ( int m = j-8 ; m < j+9 ; m++)
					for ( int n = k-8 ; n < k+9 ; n++){
						Vector3 v2 = new Vector3(l,m,n);
						Dist = Vector3.Distance(v1,v2);

						bfactor = ((float)MoleculeModel.BFactorList[atomnumber]/maxValue*5);
						if (bfactor>0)	
						GridS[l,m,n] += (float)Math.Exp(-((Dist/bfactor)*(Dist/bfactor)));
						else
						GridS[l,m,n] -= (float)Math.Exp(-((Dist/bfactor)*(Dist/bfactor)));
						
						if (VertColor[l,m,n].b == 1f && VertColor[l,m,n].r==0f){
							VertColor[l,m,n].r +=((float)MoleculeModel.BFactorList[atomnumber]/(maxValue));
							VertColor[l,m,n].b -=((float)MoleculeModel.BFactorList[atomnumber]/(2*maxValue));
//						}
//						if ( (VertColor[l,m,n].r + ((float)MoleculeModel.BFactorList[atomnumber]/(maxValue))) > 1){
//							VertColor[l,m,n].r=1f;
//							VertColor[l,m,n].b=0f;
						}
						else 
						{
							VertColor[l,m,n].r +=((float)MoleculeModel.BFactorList[atomnumber]/(20*maxValue));
							VertColor[l,m,n].b -=((float)MoleculeModel.BFactorList[atomnumber]/(20*maxValue));
						}
					
						}
			}
			atomnumber++;
			}
						
//				}
//			}
//		}
		
		// export the density in a .dx file readable by pymol or vmd
//		StreamWriter test;
//		test = new StreamWriter("grille.dx");
//		test.Write("# Data from APBS 1.3\n#\n# POTENTIAL (kT/e)\n#\nobject 1 class gridpositions counts "+X+" "+Y+" "+Z+"\norigin -2.330000e+01 -2.34000e+01 -2.550000e+01\ndelta 5.000000e-01 0.000000e+00 0.000000e+00\ndelta 0.000000e+00 5.000000e-01 0.000000e+00\ndelta 0.000000e+00 0.000000e+00 5.000000e-01\nobject 2 class gridconnections counts "+X+" "+Y+" "+Z+"\nobject 3 class array type double rank 0 items "+X*Y*Z+" data follows\n");
//		for (i=0 ; i< X ; i++){
//			for (j=0 ; j<Y ; j++){
//				for (k=0 ; k<Z ; k++){
//					test.WriteLine(GridS[i,j,k]);
//					}
//				}
//			}
//			test.Write("attribute \"dep\" string \"positions\"\nobject \"regular positions regular connections\" class field\ncomponent \"positions\" value 1\ncomponent \"connections\" value 2\ncomponent \"data\" value 3");
//			test.Close();
	}
	
	public static void ProSurface(float seuil){
		
		// to creat the structure since the pdb

		
		Vector4[] points;
		points = new Vector4[ (X) * (Y) * (Z)];
		colors = new Color[(X) * (Y) * (Z)];
		// convert grid
		for (int j = 0; j < Y; j++) {
			for (int i = 0; i < Z; i++) {
				for (int k = 0; k < X; k++) {
						points[j*(Z)*(X) + i*(X) + k] = new Vector4 (k, j, i , GridS[k,j,i]);
						colors[j*(Z)*(X) + i*(X) + k] = VertColor[k,j,i];
				}
			}
		}
		
		
		Debug.Log("Entering :: Marching Cubes");
		MarchingCubesRec MCInstance;
		MCInstance = new MarchingCubesRec();
		DestroySurface();
		MCInstance.MCRecMain(X, Y, Z, seuil, points, 0f,false, delta, origin, colors);
		points = null;
		colors = null;
//		long bytebefore = GC.GetTotalMemory(false);
//		long byteafter = GC.GetTotalMemory(true);
		GC.GetTotalMemory(true);
		GC.Collect();
//		long byteafter2 = GC.GetTotalMemory(false);
//		Debug.Log ("before: "+(bytebefore/1000000)+"+ " afterCollet: " +(byteafter2/1000000));
		
	}
	
	public static void initColors(int X,int Y,int Z, Color col){
		colors = new Color[(X) * (Y) * (Z)];

		for (int j = 0; j < Y; j++) {
			for (int i = 0; i < Z; i++) {
				for (int k = 0; k < X; k++) {
					colors[j*(Z)*(X) + i*(X) + k] = col;
				}
			}
		}
	}
	public static void DestroySurface() { // to destroy previously displayed isosurface at GUI change
		GameObject[] surfaceOBJ = GameObject.FindGameObjectsWithTag("SurfaceManager");
		for (int l = 0; l < surfaceOBJ.Length; l++) {
			
			Destroy(surfaceOBJ[l]);
		}

	}
}

