/// @file MeshMerger.cs
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
/// $Id: MeshMerger.cs 213 2013-04-06 21:13:42Z baaden $
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

// Mesh Merger Script
// Copyright 2009, Russ Menapace
// http://humanpoweredgames.com

// Summary:
//  This script allows you to draw a large number of meshes with a single 
//  draw call.  This is particularly useful for iPhone games.

// License:
//  Free to use as you see fit, but I would appreciate one of the following:
//  * A credit for Human Powered Games, or even a link to humanpoweredgames.com
//    in whatever you make with this
//  * Hire me to make games or simulations
//  * A donation to the PayPal account at russ@databar.com.  I'm very poor, so 
//    even a small donation would be greatly appreciated!
//  * A thank you note to russ@databar.com
//  * Suggestions on how the script could be improved mailed to russ@databar.com

// Warranty:
//  This software carries no warranty, and I don't guarantee anything about it.
//  If it burns down your house or gets your cat pregnant, don't look at me. 

// Acnowledgements: 
//  This was pieced together out of code I found onthe UnifyCommunity wiki, and 
//  the Unity forum.  I did not keep track of names, but I do recall gaining
//  a lot of insight from the posts of PirateNinjaAlliance.  
//  Thanks to anybody that may have been involved.

// Requirements:  
//  All the meshes you want to use must use the same material.  
//  This material may be a texture atlas and the meshes UV to portions of the atlas.
//  The texture atlas technique works particularly well for GUI stuff.

// Usage:
//  There are two ways to use this script:

//  Implicit:  
//    Simply drop the script into a GameObject that has a number of
//    child objects containing mesh filters.

//  Explicit:
//    Populate the meshFilter array with the meshes you want merged
//    Optionally, set the material to be used.  If no material is selected,
//    The script will apply the first material it encounters to all subsequent
//    meshes

// To see if it's working:
//  Move the camera so you can see several of your objects in the Game pane
//  Note the number of draw calls
//  Hit play. You should see the number of draw calls for those meshes reduced to one
 
using UnityEngine;
using System;

//==============================================================================
public class MeshMerger : MonoBehaviour 
{ 
  public MeshFilter[] meshFilters;
  public Material material;
  
  //----------------------------------------------------------------------------
  void Start () 
  { 
    // if not specified, go find meshes
    if(meshFilters.Length == 0)
    {
      // find all the mesh filters
      Component[] comps = GetComponentsInChildren(typeof(MeshFilter));
      meshFilters = new MeshFilter[comps.Length];
  
      int mfi = 0;
      foreach(Component comp in comps)
        meshFilters[mfi++] = (MeshFilter) comp;
    }
    
    // figure out array sizes
    int vertCount = 0;
    int normCount = 0;
    int triCount = 0;
    int uvCount = 0;

    foreach(MeshFilter mf in meshFilters)
    {
      vertCount += mf.mesh.vertices.Length; 
      normCount += mf.mesh.normals.Length;
      triCount += mf.mesh.triangles.Length; 
      uvCount += mf.mesh.uv.Length;
      if(material == null)
        material = mf.gameObject.GetComponent<Renderer>().material;       
    }
    
    // allocate arrays
    Vector3[] verts = new Vector3[vertCount];
    Vector3[] norms = new Vector3[normCount];
    Transform[] aBones = new Transform[meshFilters.Length];
    Matrix4x4[] bindPoses = new Matrix4x4[meshFilters.Length];
    BoneWeight[] weights = new BoneWeight[vertCount];
    int[] tris  = new int[triCount];
    Vector2[] uvs = new Vector2[uvCount];
    
    int vertOffset = 0;
    int normOffset = 0;
    int triOffset = 0;
    int uvOffset = 0;
    int meshOffset = 0;
    
    // merge the meshes and set up bones
    foreach(MeshFilter mf in meshFilters)
    {     
      foreach(int i in mf.mesh.triangles)
        tris[triOffset++] = i + vertOffset;
    
      aBones[meshOffset] = mf.transform;
      bindPoses[meshOffset] = Matrix4x4.identity;
      
      foreach(Vector3 v in mf.mesh.vertices)
      {
        weights[vertOffset].weight0 = 1.0f;
        weights[vertOffset].boneIndex0 = meshOffset;
        verts[vertOffset++] = v;
      }

      foreach(Vector3 n in mf.mesh.normals)
        norms[normOffset++] = n;
              
      foreach(Vector2 uv in mf.mesh.uv)
        uvs[uvOffset++] = uv;
  
      meshOffset++;
      
      MeshRenderer mr = 
        mf.gameObject.GetComponent(typeof(MeshRenderer)) 
        as MeshRenderer;

      if(mr)
        mr.enabled = false;
    }

    // hook up the mesh
    Mesh me = new Mesh();       
    me.name = gameObject.name;
    me.vertices = verts;
    me.normals = norms;
    me.boneWeights = weights;
    me.uv = uvs;
    me.triangles = tris;
    me.bindposes = bindPoses;

    // hook up the mesh renderer        
    SkinnedMeshRenderer smr = 
      gameObject.AddComponent(typeof(SkinnedMeshRenderer)) 
      as SkinnedMeshRenderer;
  
    smr.sharedMesh = me;
    smr.bones = aBones;
    GetComponent<Renderer>().material = material;

  }
}

