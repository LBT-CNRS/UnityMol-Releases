/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2021
        Marc Baaden, 2010-2021
        baaden@smplinux.de
        http://www.baaden.ibpc.fr

        This software is a computer program based on the Unity3D game engine.
        It is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications. More details about UnityMol are provided at
        the following URL: "http://unitymol.sourceforge.net". Parts of this
        source code are heavily inspired from the advice provided on the Unity3D
        forums and the Internet.

        This program is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        This program is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with this program. If not, see <https://www.gnu.org/licenses/>.

        References : 
        If you use this code, please cite the following reference :         
        Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
        "Game on, Science - how video game technology may help biologists tackle
        visualization challenges" (2013), PLoS ONE 8(3):e57990.
        doi:10.1371/journal.pone.0057990
       
        If you use the HyperBalls visualization metaphor, please also cite the
        following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
        B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
        using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
        J. Comput. Chem., 2011, 32, 2924

    Please contact unitymol@gmail.com
    ================================================================================
*/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineSmoother : MonoBehaviour 
{
    public static Vector3[] SmoothLine( Vector3[] inputPoints, float segmentSize )
    {
        //create curves
        AnimationCurve curveX = new AnimationCurve();
        AnimationCurve curveY = new AnimationCurve();
        AnimationCurve curveZ = new AnimationCurve();

        //create keyframe sets
        Keyframe[] keysX = new Keyframe[inputPoints.Length];
        Keyframe[] keysY = new Keyframe[inputPoints.Length];
        Keyframe[] keysZ = new Keyframe[inputPoints.Length];

        //set keyframes
        for( int i = 0; i < inputPoints.Length; i++ )
        {
            keysX[i] = new Keyframe( i, inputPoints[i].x );
            keysY[i] = new Keyframe( i, inputPoints[i].y );
            keysZ[i] = new Keyframe( i, inputPoints[i].z );
        }

        //apply keyframes to curves
        curveX.keys = keysX;
        curveY.keys = keysY;
        curveZ.keys = keysZ;

        //smooth curve tangents
        for( int i = 0; i < inputPoints.Length; i++ )
        {
            curveX.SmoothTangents( i, 0 );
            curveY.SmoothTangents( i, 0 );
            curveZ.SmoothTangents( i, 0 );
        }

        //list to write smoothed values to
        List<Vector3> lineSegments = new List<Vector3>();

        //find segments in each section
        for( int i = 0; i < inputPoints.Length; i++ )
        {
            //add first point
            lineSegments.Add( inputPoints[i] );

            //make sure within range of array
            if( i+1 < inputPoints.Length )
            {
                //find distance to next point
                float distanceToNext = Vector3.Distance(inputPoints[i], inputPoints[i+1]);

                //number of segments
                int segments = (int)(distanceToNext / segmentSize);

                //add segments
                for( int s = 1; s < segments; s++ )
                {
                    //interpolated time on curve
                    float time = ((float)s/(float)segments) + (float)i;

                    //sample curves to find smoothed position
                    Vector3 newSegment = new Vector3( curveX.Evaluate(time), curveY.Evaluate(time), curveZ.Evaluate(time) );

                    //add to list
                    lineSegments.Add( newSegment );
                }
            }
        }

        return lineSegments.ToArray();
    }

}
