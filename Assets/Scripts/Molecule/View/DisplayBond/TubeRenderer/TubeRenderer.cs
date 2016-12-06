/// @file TubeRenderer.cs
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
/// $Id: TubeRenderer.cs 225 2013-04-07 14:21:34Z baaden $
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

// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;



public class TubeRenderer : MonoBehaviour 
{


public TubeVertex[] vertices;
public Material material;

public int crossSegments = 3;
private Vector3[] crossPoints;
private int lastCrossSegments;
public float flatAtDistance=-1;

private Vector3 lastCameraPosition1;
private Vector3 lastCameraPosition2;
public int movePixelsForRebuild= 6;
public float maxRebuildTime= 0.1f;
private float lastRebuildTime= 0.00f;

void  Reset ()
{
	
    vertices = new TubeVertex[2];
    vertices[0]=new TubeVertex(Vector3.zero, 1.0f, Color.white);
    vertices[1]=new TubeVertex(new Vector3(1,0,0), 1.0f, Color.white);
    
}
void  Start ()
{
    gameObject.AddComponent<MeshFilter>();
    MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
    mr.material = material;
}

void  LateUpdate ()
{
    if (vertices==null || vertices.Length <= 1)
     {
        GetComponent<Renderer>().enabled=false;
        return;
    }
    GetComponent<Renderer>().enabled=true;
    
    //rebuild the mesh?
    bool  re=false;
//    float distFromMainCam;
    if(vertices.Length > 1)
    {
        Vector3 cur1 = Camera.main.WorldToScreenPoint(vertices[0].point);
//        distFromMainCam = lastCameraPosition1.z;
        lastCameraPosition1.z = 0;
        Vector3 cur2 = Camera.main.WorldToScreenPoint(vertices[vertices.Length - 1].point);
        lastCameraPosition2.z = 0;

        float distance = (lastCameraPosition1 - cur1).magnitude;
        distance += (lastCameraPosition2 - cur2).magnitude;

        if(distance > movePixelsForRebuild || Time.time - lastRebuildTime > maxRebuildTime)
        {
            re = true;
            lastCameraPosition1 = cur1;
            lastCameraPosition2 = cur2;
        }
    }

    if (re) {
        //draw tube
        
        if (crossSegments != lastCrossSegments) {
            crossPoints = new Vector3[crossSegments];
            float theta = 2.0f*Mathf.PI/crossSegments;
            int c;
            for (c=0;c<crossSegments;c++) {
                crossPoints[c] = new Vector3(Mathf.Cos(theta*c), Mathf.Sin(theta*c), 0);
            }
            lastCrossSegments = crossSegments;
        }
        
        Vector3[] meshVertices = new Vector3[vertices.Length*crossSegments];
        Vector2[] uvs = new Vector2[vertices.Length*crossSegments];
        Color[] colors = new Color[vertices.Length*crossSegments];
        int[] tris = new int[vertices.Length*crossSegments*6];
        int[] lastVertices = new int[crossSegments];
        int[] theseVertices = new int[crossSegments];
        Quaternion rotation;
        int p;
        for (p=0;p<vertices.Length;p++) {
            if (p<vertices.Length-1)
            {
            	 rotation = Quaternion.FromToRotation(Vector3.forward, vertices[p+1].point-vertices[p].point);
            }
            else 
            {
            	 rotation = Quaternion.FromToRotation(Vector3.forward, vertices[p].point-vertices[p-1].point);
            }
            int c;
            for (c=0;c<crossSegments;c++) {
                int vertexIndex = p*crossSegments+c;
                meshVertices[vertexIndex] = vertices[p].point + rotation * crossPoints[c] * vertices[p].radius;
                uvs[vertexIndex] = new Vector2((0.0f+c)/crossSegments,(0.0f+p)/vertices.Length);
                colors[vertexIndex] = vertices[p].color;
                
//        print(c+" - vertex index "+(p*crossSegments+c) + " is " + meshVertices[p*crossSegments+c]);
                lastVertices[c]=theseVertices[c];
                theseVertices[c] = p*crossSegments+c;
            }
            //make triangles
            if (p>0) {
                for (c=0;c<crossSegments;c++) {
                    int start= (p*crossSegments+c)*6;
                    tris[start] = lastVertices[c];
                    tris[start+1] = lastVertices[(c+1)%crossSegments];
                    tris[start+2] = theseVertices[c];
                    tris[start+3] = tris[start+2];
                    tris[start+4] = tris[start+1];
                    tris[start+5] = theseVertices[(c+1)%crossSegments];
//          print("Triangle: indexes("+tris[start]+", "+tris[start+1]+", "+tris[start+2]+"), ("+tris[start+3]+", "+tris[start+4]+", "+tris[start+5]+")");
                }
            }
        }
        
        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.uv = uvs;
        GetComponent<MeshFilter>().mesh = mesh;
    }
}

//sets all the points to points of a Vector3 array, as well as capping the ends.
public void  SetPoints ( Vector3[] points ,   float radius ,   Color col  ){
    if (points.Length < 2) return;
    vertices = new TubeVertex[points.Length+2];
    
    Vector3 v0offset = (points[0]-points[1])*0.01f;
    vertices[0] = new TubeVertex(v0offset+points[0], 0.0f, col);
    Vector3 v1offset = (points[points.Length-1] - points[points.Length-2])*0.01f;
    vertices[vertices.Length-1] = new TubeVertex(v1offset+points[points.Length-1], 0.0f, col);
    int p;
    for (p=0;p<points.Length;p++) {
        vertices[p+1] = new TubeVertex(points[p], radius, col);
    }
}
}