/// @file LinkedSpriteManager.cs
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
/// $Id: LinkedSpriteManager.cs 213 2013-04-06 21:13:42Z baaden $
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

//-----------------------------------------------------------------
//  LinkedSpriteManager v0.632 (7-24-2009)
//  Copyright 2009 Brady Wright and Above and Beyond Software
//  All rights reserved
//-----------------------------------------------------------------
// A class to allow the drawing of multiple "quads" as part of a
// single aggregated mesh so as to achieve multiple, independently
// moving objects using a single draw call.
//-----------------------------------------------------------------


using UnityEngine;
using System.Collections;

// A variation on the SpriteManager that automatically links all
// translations and rotations of the client GameObjects to the
// associated sprite - meaning the client need not worry about
// micromanaging all transformations:
public class LinkedSpriteManager : SpriteManager 
{
    Transform t;
    Vector3 pos;
    Sprite s;


    // Use this for initialization
    void Start () 
    {
    
    }

    // Transforms all sprites by their associated GameObject's
    // transforms:
    void TransformSprites()
    {
        for(int i=0; i<activeBlocks.Count; ++i)
        {
            ((Sprite)activeBlocks[i]).Transform();
        }

        // Handle any billboarded sprites:
        if(activeBillboards.Count > 0)
        {
            t = Camera.main.transform;

            for(int i=0; i<activeBillboards.Count; ++i)
            {
                s = (Sprite)activeBillboards[i];
                pos = s.clientTransform.position;

                vertices[s.mv1] = pos + t.TransformDirection(s.v1);
                vertices[s.mv2] = pos + t.TransformDirection(s.v2);
                vertices[s.mv3] = pos + t.TransformDirection(s.v3);
                vertices[s.mv4] = pos + t.TransformDirection(s.v4);
            }
        }
    }

    // LateUpdate is called once per frame
    new void LateUpdate() 
    {
        // Transform all sprites according to their
        // client GameObject's transforms:
        TransformSprites();

        // Copy over the changes:
        mesh.vertices = vertices;

        // See if we have any active animations:
        if (playingAnimations.Count > 0)
        {
            animTimeElapsed = Time.deltaTime;

            for (i = 0; i < playingAnimations.Count; ++i)
            {
                tempSprite = (Sprite)playingAnimations[i];

                // Step the animation, and if it has finished
                // playing, remove it from the playing list:
                if (!tempSprite.StepAnim(animTimeElapsed))
                    playingAnimations.Remove(tempSprite);
            }

            uvsChanged = true;
        }

        if (vertCountChanged)
        {
            mesh.uv = UVs;
            mesh.colors = colors;
            mesh.triangles = triIndices;

            vertCountChanged = false;
            uvsChanged = false;
            colorsChanged = false;
        }
        else
        {
            if (uvsChanged)
            {
                mesh.uv = UVs;
                uvsChanged = false;
            }

            if (colorsChanged)
            {
                colorsChanged = false;

                mesh.colors = colors;
            }

            // Explicitly recalculate bounds since
            // we didn't assign new triangles (which
            // implicitly recalculates bounds)
            if (updateBounds || autoUpdateBounds)
            {
                mesh.RecalculateBounds();
                updateBounds = false;
            }
        }
    }
}
 