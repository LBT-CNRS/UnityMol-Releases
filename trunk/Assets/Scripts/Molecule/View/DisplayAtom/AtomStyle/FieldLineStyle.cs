/// @file FieldLineStyle.cs
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
/// $Id: FieldLineStyle.cs 329 2013-08-06 13:47:40Z erwan $
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

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Molecule.Model;
using Config;
using UI;
using Molecule.Control;
using System.Globalization;
	
public class FieldLineStyle
{
	public FieldLineStyle()
	{
	}
	
	public static void DisplayFieldLine()
	{
		
		if (MoleculeModel.fieldLineFileExists) {
			Color c1 = Color.white;
			Debug.Log("Entering :: DisplayFieldLine");
			
			float disttot = 0.0f;
			float xdif = 0.0f;
			float ydif = 0.0f;
			float zdif = 0.0f;
			Vector3 locationbegin = new Vector3(0,0,0);
			Vector3 curent = new Vector3(0,0,0);
		
			for(int i=0;i<MoleculeModel.FieldLineList.Count;i++)
//			for(int i=7;i<8;i++)
			{
				GameObject FieldLineF = new GameObject();
				FieldLineF.name="FieldLineF"+i;
				FieldLineF.tag="FieldLineManager";
				FieldLineF.transform.parent = GameObject.Find("FieldLineManager").transform;
		    	LineRenderer lineRenderer = FieldLineF.AddComponent<LineRenderer>();
				lineRenderer.useWorldSpace = true;
		   		lineRenderer.material = new Material (Shader.Find("Custom/FieldLineCg"));
		    	lineRenderer.SetColors(c1, c1);
		    	lineRenderer.SetWidth(0.2f,0.2f);
//			    lineRenderer.SetVertexCount(((ArrayList)MoleculeModel.FieldLineList[i]).Count);
				lineRenderer.SetVertexCount(5);

				//distance
//				float[] leng= new float[((ArrayList)MoleculeModel.FieldLineList[i]).Count];
//				leng[0] = UnityEngine.Random.value * 4.0f;

				//For the static cut we need to store the real world position of each vertex.
				//luMin will store the min values in x,y,z for the line's vertices.
				//luWidth will store the max - min
				Vector3 luMin = new Vector3(float.MaxValue,float.MaxValue,float.MaxValue);
				Vector3 luWidth = new Vector3(float.MinValue,float.MinValue,float.MinValue);
				ArrayList posLookup = new ArrayList();


				int j;
				int nbPoint =0;
				int nbsegment=0;
				for(j=0;j<((List<Vector3>)MoleculeModel.FieldLineList[i]).Count;j++)
//				for(j=0;j<1;j++)
				{		
//					Vector3 location=new Vector3(((Vector3)((ArrayList)MoleculeModel.FieldLineList[i])[j]).x,((Vector3)((ArrayList)MoleculeModel.FieldLineList[i])[j]).y,((Vector3)((ArrayList)MoleculeModel.FieldLineList[i])[j]).z);
					Vector3 location=new Vector3(	((MoleculeModel.FieldLineList[i])[j]).x,
													((MoleculeModel.FieldLineList[i])[j]).y,
													((MoleculeModel.FieldLineList[i])[j]).z);
				
					if (j==0){
				        lineRenderer.SetPosition(j, location);
						nbPoint +=1;
						lineRenderer.SetVertexCount(nbPoint+1);
						locationbegin = location;

						posLookup.Add(location);
						luMin = location;	
						//let store the max values in luWidth for the moment
						luWidth = location;

					}
				
					if(j>0){
//						float dist = Vector3.Distance (((Vector3)((ArrayList)MoleculeModel.FieldLineList[i])[j]), ((Vector3)((ArrayList)MoleculeModel.FieldLineList[i])[j-1]));
						float dist = Vector3.Distance (	((MoleculeModel.FieldLineList[i])[j]),
														((MoleculeModel.FieldLineList[i])[j-1])	);
				 		disttot += dist;
//						Debug.Log("dist: " + dist + " disttot:" + disttot);
//						leng[j] = leng[j-1] + dist;
						if(disttot>1){	
							xdif = location[0] - locationbegin[0];						//Debug.Log("xdiff: " + xdif);
							ydif = location[1] - locationbegin[1];						//Debug.Log("ydiff: " + ydif);
							zdif = location[2] - locationbegin[2];						//Debug.Log("zdiff: " + zdif);
//							Debug.Log("longueur "+disttot +" " + (int)disttot);
	
							if(disttot-(int)disttot <0.5)
								nbsegment = 2*(int)disttot;
							else 
								nbsegment = 2	*(int)disttot+1;
							
							int k;	
//							Debug.Log("Begin "+locationbegin);
							curent = locationbegin;
							for (k=0; k<nbsegment;k++){
//								Debug.Log("curent" + curent);
								curent[0]+=(xdif/nbsegment);
								curent[1]+=(ydif/nbsegment);
								curent[2]+=(zdif/nbsegment);
//								Debug.Log("curent" + curent);
								lineRenderer.SetPosition(nbPoint, curent);
//								Debug.Log(" "+new Vector3((locationbegin[0]+xdif)/nbsegment,(locationbegin[1]+ydif)/nbsegment,(locationbegin[2]+ydif)/nbsegment));
								nbPoint +=1;
								lineRenderer.SetVertexCount(nbPoint+1);

								posLookup.Add(curent);
								for(int ind=0; ind<3; ind++)
								{
									luMin[ind] = Mathf.Min(luMin[ind],curent[ind]);	
									//let store the max values in luWidth for the moment
									luWidth[ind] = Mathf.Max(luWidth[ind],curent[ind]);
								}
								
							}
							disttot = 0;					
							locationbegin = curent;
						}
													
					}
				}

				//The real positions are passed to the shader in a texture
				//The values have to be converted to color, hence between 0 and 1. 0 matches the minVal, 1 matches the maxVal
				//luMin and luWidth are used to compute the color value and then retrieve the real value in the shader
				//colVal = (realVal - minVal) / widthVal
				//realVal = (colVal * widthVal) + minVal
				//And thus for x, y and z
				Texture2D tex_wPos = new Texture2D(nbPoint,1,TextureFormat.ARGB32, false);
				tex_wPos.filterMode = FilterMode.Point;	//Automatic interpolation
				tex_wPos.wrapMode = TextureWrapMode.Clamp;	//Clamp to avoid periodic interpolation
				luWidth = luWidth - luMin;
				//Computing and storing the colValues in the texture.
				for(j=0; j<nbPoint; ++j)
				{
					Vector3 p = ((Vector3)posLookup[j])-luMin;
					tex_wPos.SetPixel(j,0,new Color(p.x/luWidth.x, p.y/luWidth.y, p.z/luWidth.z, 1.0f));
				}
				tex_wPos.Apply(); //Apply the new colors to the texture
				//Binding the variables to the shader
				lineRenderer.material.SetTexture("_PosLookup",tex_wPos);
				lineRenderer.material.SetVector("_MinPosLookup",luMin);
				lineRenderer.material.SetVector("_WidthPosLookup",luWidth);

				lineRenderer.SetVertexCount(nbPoint);
//				MoleculeModel.FieldLineDist.Add(leng);
				lineRenderer.material.SetFloat("_Unsynchronize",UnityEngine.Random.value * 10.0f);
				lineRenderer.material.SetFloat("_timeOff",Time.time);
				lineRenderer.material.SetColor("_Color",Color.white);
				

			}
		}

	}
}
