/// @file DMatrix.cs
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
/// $Id: DMatrix.cs 298 2013-06-14 10:10:28Z kouyoumdjian $
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

namespace DisplayControl
{
	using UnityEngine;
	using System.Collections;
	
	public class DMatrix {
		//float [,]r=new float[4,4];
		public  DMatrix() {}
		
		public static float[,] Multiply(float [,]r1,float [,]r2) {
			float[,] matrix=new float[4,4];
			float[,] tR2=Transpose(r2);
			for(int i=0;i<4;i++)
				//matrix[i/4,i%4]=0;
				for(int j=0;j<4;j++)
					for(int k=0;k<4;k++)
						matrix[i,j]+=r1[i,k]*tR2[j,k];
			
			return matrix;
		}
		
		public static float[,] CreateMatrix( Vector3 v) {
			float[,] matrix=new float[4,4];
			for(int i=0;i<4;i++)
				for(int j=0;j<4;j++)
					matrix[i,j]=0;

			matrix[0,0]=v.x;
			matrix[0,1]=v.y;
			matrix[0,2]=v.z;
			matrix[0,3]=1;
			return matrix;
		}
		
		public static float[,] initMatrixZ(float a,float b) {
			float[,] matrix=new float[4,4];
			matrix[0,0]=b;
			matrix[0,1]=a;
			matrix[1,0]=-a;
			matrix[0,1]=b;
			matrix[2,2]=1;
			matrix[3,3]=1;
			return matrix;
		}
		
		public static float[,] initMatrixX(float a,float b) {
			float[,] matrix=new float[4,4];
			matrix[0,0]=1;
			matrix[1,1]=a;
			matrix[1,2]=b;
			matrix[2,1]=-b;
			matrix[2,2]=a;
			matrix[3,3]=1;
			return matrix;
		}
		
		public static void RotationMatrix(Vector3 v, Vector3 v1, Vector3 v2,float angle) {
			//float [,]mV=CreateMatrix( v);
			float [,]mV1=CreateMatrix(v1);
			float [,]mV2=CreateMatrix( v2);
			float length=Mathf.Sqrt(v.x*v.x+v.y*v.y);
			
			float [,]tz=initMatrixZ(v.x/length,v.y/length);
			float [,]tx=initMatrixZ(v.z,length);
			float [,]tzAngle=initMatrixZ(Mathf.Sin(angle),Mathf.Cos(angle));
			float [,] rTx=initMatrixZ(v.z,-length); 
			float [,] rTz=initMatrixZ(-v.x/length,v.y/length);
			//mV=Matrix.Multiply(mV,Matrix.Multiply(Matrix.Multiply(Matrix.Multiply(Matrix.Multiply(tz,tx),tzAngle),rTx),rTz));
			
			mV1=Multiply(mV1,Multiply(Multiply(Multiply(Multiply(tz,tx),tzAngle),rTx),rTz));
			mV2=Multiply(mV2,Multiply(Multiply(Multiply(Multiply(tz,tx),tzAngle),rTx),rTz));

			v1.x=mV1[0,0];
			v1.y=mV1[0,1];
			v1.z=mV1[0,2];
			
//			v.x=mV[0,0];
//			v.y=mV[0,1];
//			v.z=mV[0,2];
			
			v2.x=mV2[0,0];
			v2.y=mV2[0,1];
			v2.z=mV2[0,2];
		}
		
		public static float[,] Transpose(float [,] m) {
			float[,] matrix=new float[4,4];
			for(int i=0;i<4;i++)
				for(int j=0;j<4;j++)
					matrix[i,j]=m[j,i];

			return matrix;
		}
	}
}

