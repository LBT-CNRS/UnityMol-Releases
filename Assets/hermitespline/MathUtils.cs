/// @file MathUtils.cs
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
/// $Id: MathUtils.cs 212 2013-04-06 20:59:44Z baaden $
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

public class MathUtils{

    public static float GetQuatLength(Quaternion q){
        return Mathf.Sqrt(q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w);   
    }
    
    //
    public static Quaternion GetQuatConjugate(Quaternion q){
        return new Quaternion(-q.x, -q.y, -q.z, q.w);
    }
    
    //
    // Logarithm of a unit quaternion. The result is not necessary a unit quaternion.
    // 
    public static Quaternion GetQuatLog(Quaternion q ){
        Quaternion res = q;
        res.w = 0;
 
        if (Mathf.Abs(q.w) < 1.0){      
            float theta = Mathf.Acos(q.w);
            float sin_theta = Mathf.Sin(theta);

            if (Mathf.Abs(sin_theta) > 0.0001){
                float coef = theta/sin_theta;
	            res.x = q.x*coef;
	            res.y = q.y*coef;
	            res.z = q.z*coef;
      		}
        }
     	return res;
    }

    //
    // Exp
    //
    public static Quaternion GetQuatExp(Quaternion q){
		Quaternion res = q;
        float fAngle = Mathf.Sqrt(q.x*q.x + q.y*q.y + q.z*q.z);    
        float fSin = Mathf.Sin(fAngle);
        
        res.w = Mathf.Cos(fAngle);

        if (Mathf.Abs(fSin) > 0.0001){
            float coef = fSin/fAngle;
            res.x = coef*q.x;
            res.y = coef*q.y;
            res.z = coef*q.z;
        }        
        return res;
    } 

    //
    // SQUAD Spherical Quadrangle interpolation [Shoe87]
    //
    public static Quaternion GetQuatSquad (float t, Quaternion q0, Quaternion q1, Quaternion a0,Quaternion a1){

        float slerpT = 2.0f*t*(1.0f-t);
    	Quaternion slerpP = Slerp(q0, q1, t);
		Quaternion slerpQ = Slerp(a0, a1, t);
  
        return Slerp(slerpP, slerpQ, slerpT); 

    }

    public static Quaternion GetSquadIntermediate (Quaternion q0uaternion,Quaternion q1uaternion,Quaternion q2uaternion){
        Quaternion q1Inv = GetQuatConjugate(q1uaternion);
		Quaternion p0 = GetQuatLog(q1Inv*q0uaternion);

        Quaternion p2 = GetQuatLog(q1Inv*q2uaternion);
        Quaternion sum=new Quaternion(-0.25f*(p0.x+p2.x), -0.25f*(p0.y+p2.y), -0.25f*(p0.z+p2.z), -0.25f*(p0.w+p2.w));

        return q1uaternion*GetQuatExp(sum);
    }
    
    //
    // Smooths the input parameter t. If less than k1 ir greater than k2, it uses a sin. Between k1 and k2 it uses 
    // linear interp.
    //
    public static float Ease(float t,float k1,float k2){ 
      	float f;
		float s; 
		f = k1*2/Mathf.PI + k2 - k1 + (1.0f-k2)*2/Mathf.PI;
	  
		if (t < k1) 
		{ 
		    s = k1*(2/Mathf.PI)*(Mathf.Sin((t/k1)*Mathf.PI/2-Mathf.PI/2)+1); 
		} 
		else 
		if (t < k2) 
		{ 
		    s = (2*k1/Mathf.PI + t-k1); 
		} 
		else 
		{ 
		    s= 2*k1/Mathf.PI + k2-k1 + ((1-k2)*(2/Mathf.PI))*Mathf.Sin(((t-k2)/(1.0f-k2))*Mathf.PI/2); 
		} 
		f = k1*2/Mathf.PI + k2 - k1 + (1.0f-k2)*2/Mathf.PI;
		
		if (t < k1) 
		{ 
		    s = k1*(2/Mathf.PI)*(Mathf.Sin((t/k1)*Mathf.PI/2-Mathf.PI/2)+1); 
		} 
		else 
		if (t < k2) 
		{ 
		    s = (2*k1/Mathf.PI + t-k1); 
		} 
		else 
		{ 
		    s= 2*k1/Mathf.PI + k2-k1 + ((1-k2)*(2/Mathf.PI))*Mathf.Sin(((t-k2)/(1.0f-k2))*Mathf.PI/2); 
		} 	
		return (s/f); 
    }
    
    //
    // We need this because Quaternion.Slerp always does it using the shortest arc
    //
    public static Quaternion Slerp (Quaternion p ,Quaternion q,float t){
        Quaternion ret;
        float omega;
        float invSin;
        float fCoeff0;
        float fCoeff1;

		float fCos = Quaternion.Dot(p, q);
        
        if ((1.0f + fCos) > 0.00001f){
            if ((1.0f - fCos) > 0.00001f){
                omega = Mathf.Acos(fCos);
                invSin = 1.0f/Mathf.Sin(omega);
                fCoeff0 = Mathf.Sin((1.0f-t)*omega)*invSin;
                fCoeff1 = Mathf.Sin(t*omega)*invSin;
            }
            else{
                fCoeff0 = 1.0f-t;
                fCoeff1 = t;
            }
            
            ret.x = fCoeff0*p.x + fCoeff1*q.x;
			ret.y = fCoeff0*p.y + fCoeff1*q.y;
			ret.z = fCoeff0*p.z + fCoeff1*q.z;
			ret.w = fCoeff0*p.w + fCoeff1*q.w;          
        }
        else{
            fCoeff0 = Mathf.Sin((1.0f-t)*Mathf.PI*0.5f);
            fCoeff1 = Mathf.Sin(t*Mathf.PI*0.5f);
            
            ret.x = fCoeff0*p.x - fCoeff1*p.y;
			ret.y = fCoeff0*p.y + fCoeff1*p.x;
			ret.z = fCoeff0*p.z - fCoeff1*p.w;
			ret.w =  p.z;
        }
        return ret;
    }
}


