/// @file Ball_HyperBalls_D3D.shader
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
/// $Id: Ball_HyperBalls_D3D.shader 210 2013-04-06 20:52:41Z baaden $
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

// Upgrade NOTE: replaced 'glstate.matrix.invtrans.modelview[0]' with 'UNITY_MATRIX_IT_MV'
// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'

// $Id: Ball_HyperBalls_D3D.shader 210 2013-04-06 20:52:41Z baaden $
// (c) 2010 by Marc Baaden & Matthieu Chavent, <baaden@smplinux.de> <matthieu.chavent@free.fr>
// Unity3D FvNano shaders coming from FVNano project
//
// On a un cube (mesh) dans la scene -> vertex
// la spherepos du cube est 0, 0, 0


Shader "FvNano/Ball HyperBalls D3D" {


// Proprietes exposees a l'interface graphique
Properties {
	_Rayon ("Rayon de la Sphere", float) = 0.1
	_Color ("Couleur de la Sphere", Color) = (1,0.1,0.1,1.0)
	_TexPos ("Position de la sphere", Vector) = (0.0,0.0,0.0,1.0)     // important que w != 0 !!
	_Visibilite ("Visibilite de la Sphere", float) = 1.0
	_Light ("Light vector", Vector) = (1,0,0,0)
	_Equation("Equation", Vector) = (1,1,1,1)
	_MatCap ("MatCap (RGB)", 2D) = "white" {}
}


// ==========================================================
// L'actuel Shader Cg =======================================
// ==========================================================

SubShader {
	Pass {
		Name "BASE"
        Tags { "LightMode" = "Always" }		

Program "vp" {
// Vertex combos: 1
//   opengl - ALU: 244 to 244
//   d3d9 - ALU: 278 to 278, FLOW: 2 to 2
SubProgram "opengl " {
Keywords { }
Bind "vertex" Vertex
Float 5 [_Rayon]
Vector 6 [_TexPos]
Vector 7 [_Equation]
"3.0-!!ARBvp1.0
# 244 ALU
PARAM c[8] = { { 0, 2, 1 },
		state.matrix.mvp,
		program.local[5..7] };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
TEMP R5;
TEMP R6;
TEMP R7;
TEMP R8;
TEMP R9;
TEMP R10;
TEMP R11;
TEMP R12;
TEMP R13;
MUL R0.xyz, vertex.position, c[5].x;
MUL R0.xyz, R0, c[0].y;
MOV R0.w, c[0].z;
MUL R1.xyz, vertex.position, c[0].y;
SLT R0.w, c[5].x, R0;
ABS R0.w, R0;
ADD R1.xyz, R1, c[6];
ADD R0.xyz, R0, c[6];
ADD R0.xyz, R0, -R1;
SGE R0.w, c[0].x, R0;
MAD R6.xyz, R0, R0.w, R1;
MOV R0, c[2];
MOV R6.w, c[0].z;
MUL R8.x, R0.y, c[1].z;
MUL R8.y, R0.w, c[1];
MUL R8.z, R0, c[1].y;
MUL R3.y, R0.x, c[1];
DP4 R1.z, R6, c[4];
MUL R3.z, R0.y, c[1].x;
MUL R2.z, R8, c[3].x;
MUL R10.z, R8.y, c[4].x;
MUL R2.y, R0.x, c[1].z;
MAD R1.w, R3.z, c[3].z, R2.z;
MUL R2.x, R0.z, c[1];
MAD R1.w, R2.y, c[3].y, R1;
MAD R1.w, -R2.x, c[3].y, R1;
MAD R4.z, -R3.y, c[3], R1.w;
MUL R2.w, R3.y, c[4].z;
MAD R2.w, R2.x, c[4].y, R2;
MAD R2.w, R8.x, c[4].x, R2;
MAD R3.x, -R3.z, c[4].z, R2.w;
MUL R2.w, R2.x, c[3];
MUL R3.w, R2, c[4].y;
MUL R2.w, R3.z, c[3].z;
MAD R3.x, -R8.z, c[4], R3;
MUL R1.w, R0, c[1].x;
MAD R3.w, R2, c[4], R3;
MUL R2.w, R1, c[3].y;
MAD R2.w, R2, c[4].z, R3;
MUL R3.w, R3.y, c[3];
MAD R2.w, R3, c[4].z, R2;
MAD R2.w, R2.z, c[4], R2;
MUL R2.z, R8.y, c[3];
MAD R2.w, R2.z, c[4].x, R2;
MUL R2.z, R2.y, c[3].y;
MUL R11.x, R0.y, c[1].w;
MAD R3.w, R1, c[3].y, R3;
MAD R3.w, R11.x, c[3].x, R3;
MUL R11.y, R0.z, c[1].w;
DP4 R1.x, R6, c[1];
DP4 R1.y, R6, c[2];
MUL R9.x, R0.w, c[1].z;
MAD R2.z, R2, c[4].w, R2.w;
MUL R7.z, R8.x, c[3].w;
MAD R5.x, R7.z, c[4], R2.z;
MUL R2.z, R0.x, c[1].w;
MUL R2.w, R9.x, c[3].x;
MOV R7.xy, R1;
MOV R7.w, R1.z;
MOV R4.xyw, R7;
MAD R0.w, R2, c[4].y, R5.x;
MUL R0.x, R2.z, c[3].z;
MAD R0.w, R0.x, c[4].y, R0;
MUL R0.x, R11, c[3];
MAD R0.y, R0.x, c[4].z, R0.w;
MUL R0.x, R11.y, c[3].y;
MAD R0.y, R0.x, c[4].x, R0;
MUL R0.x, R3.z, c[3].w;
MAD R0.y, -R0.x, c[4].z, R0;
MUL R0.x, R2, c[3].y;
MAD R0.y, -R0.x, c[4].w, R0;
MUL R0.x, R1.w, c[3].z;
MAD R0.y, -R0.x, c[4], R0;
MUL R0.x, R3.y, c[3].z;
MAD R0.y, -R0.x, c[4].w, R0;
MUL R0.x, R8.z, c[3].w;
MAD R0.y, -R0.x, c[4].x, R0;
MUL R0.x, R8.y, c[3];
MAD R0.y, -R0.x, c[4].z, R0;
MUL R0.x, R2.y, c[3].w;
MAD R0.y, -R0.x, c[4], R0;
MUL R0.x, R8, c[3];
MAD R0.y, -R0.x, c[4].w, R0;
MUL R0.x, R9, c[3].y;
MAD R0.y, -R0.x, c[4].x, R0;
MUL R0.x, R2.z, c[3].y;
MAD R0.y, -R0.x, c[4].z, R0;
MUL R0.x, R11, c[3].z;
MAD R0.y, -R0.x, c[4].x, R0;
MUL R0.x, R11.y, c[3];
MAD R0.x, -R0, c[4].y, R0.y;
RCP R8.w, R0.x;
MAD R0.y, -R8.x, c[3].x, R4.z;
MUL R5.w, R8, R0.y;
MOV R0, c[3];
MUL R10.w, R0.x, c[2].y;
MAD R3.x, -R2.y, c[4].y, R3;
MUL R9.z, R0, c[1].y;
MUL R5.z, R8.w, R3.x;
MUL R12.z, R0, c[2].x;
MUL R5.x, R10.w, c[4].z;
MUL R11.z, R0.y, c[2].x;
MUL R11.w, R0.x, c[2].z;
MUL R3.x, R0.y, c[1];
MUL R4.z, R9, c[4].x;
MUL R12.x, R0, c[1].z;
MAD R4.z, R3.x, c[4], R4;
MUL R13.x, R0.z, c[1];
MAD R4.z, R12.x, c[4].y, R4;
MUL R10.y, R0.x, c[1];
MAD R4.z, -R13.x, c[4].y, R4;
MAD R4.z, -R10.y, c[4], R4;
MUL R9.y, R0, c[1].z;
MAD R4.z, -R9.y, c[4].x, R4;
MUL R5.y, R8.w, R4.z;
MOV R4.z, c[0].x;
MUL R10.x, R0.y, c[2].z;
MAD R5.x, R12.z, c[4].y, R5;
MAD R5.x, R10, c[4], R5;
MUL R9.w, R0.z, c[2].y;
MAD R5.x, -R11.z, c[4].z, R5;
MAD R5.x, -R9.w, c[4], R5;
MAD R5.x, -R11.w, c[4].y, R5;
MUL R5.x, R8.w, R5;
MAD R3.w, -R3.z, c[3], R3;
MAD R10.z, R3, c[4].w, R10;
MAD R3.z, -R8.y, c[3].x, R3.w;
MAD R3.w, R2.z, c[4].y, R10.z;
MAD R10.z, -R1.w, c[4].y, R3.w;
MAD R3.y, -R3, c[4].w, R10.z;
MAD R3.z, -R2, c[3].y, R3;
MUL R10.z, R0.w, c[2].y;
MUL R3.w, R8, R3.z;
MAD R3.y, -R11.x, c[4].x, R3;
MUL R3.z, R8.w, R3.y;
MUL R12.y, R10.z, c[4].x;
MAD R3.y, R11.z, c[4].w, R12;
MUL R12.y, R0.x, c[2].w;
MUL R11.z, R0.w, c[1].x;
MUL R10.y, R10, c[4].w;
MAD R13.y, R11.z, c[4], R10;
MUL R10.y, R0, c[1].w;
MAD R13.y, R10, c[4].x, R13;
MAD R3.x, -R3, c[4].w, R13.y;
MUL R12.w, R0, c[2].x;
MAD R3.y, R12, c[4], R3;
MAD R3.y, -R12.w, c[4], R3;
MAD R3.y, -R10.w, c[4].w, R3;
MUL R10.w, R0.y, c[2];
MUL R0.y, R0.w, c[1];
MAD R3.x, -R0.y, c[4], R3;
MUL R13.y, R0.x, c[1].w;
MAD R0.x, -R13.y, c[4].y, R3;
MAD R3.y, -R10.w, c[4].x, R3;
MUL R3.x, R8.w, R3.y;
MUL R3.y, R8.w, R0.x;
MAD R0.x, R2, c[3].w, R2.w;
MUL R2.w, R2.y, c[4];
MAD R0.x, R2.z, c[3].z, R0;
MAD R0.x, -R1.w, c[3].z, R0;
MAD R2.w, R1, c[4].z, R2;
MAD R1.w, R11.y, c[4].x, R2;
MAD R1.w, -R2.x, c[4], R1;
MUL R2.x, R11.w, c[4].w;
MAD R0.x, -R2.y, c[3].w, R0;
MAD R0.x, -R11.y, c[3], R0;
MUL R2.w, R8, R0.x;
MAD R1.w, -R9.x, c[4].x, R1;
MAD R1.w, -R2.z, c[4].z, R1;
MUL R0.x, R0.w, c[1].z;
MUL R2.z, R8.w, R1.w;
MUL R1.w, R0.x, c[4].x;
MAD R1.w, R13.x, c[4], R1;
MUL R11.w, R0.z, c[2];
MAD R2.x, R12.w, c[4].z, R2;
MAD R2.x, R11.w, c[4], R2;
MAD R2.x, -R12.z, c[4].w, R2;
MUL R12.z, R0.w, c[2];
MAD R1.w, R13.y, c[4].z, R1;
MAD R0.w, -R11.z, c[4].z, R1;
MAD R2.x, -R12.z, c[4], R2;
MAD R1.w, -R12.y, c[4].z, R2.x;
MUL R2.x, R8.w, R1.w;
MOV R1.zw, R1.z;
MUL R11.z, R0, c[1].w;
MAD R0.w, -R12.x, c[4], R0;
MAD R0.z, -R11, c[4].x, R0.w;
MUL R2.y, R8.w, R0.z;
MAD R0.z, R8.y, c[3], R7;
MUL R0.w, R9.x, c[4].y;
MAD R0.w, R8.z, c[4], R0;
MAD R0.z, R11.y, c[3].y, R0;
MAD R0.w, R11.x, c[4].z, R0;
MAD R0.z, -R8, c[3].w, R0;
MAD R0.w, -R8.y, c[4].z, R0;
MAD R0.w, -R8.x, c[4], R0;
MAD R0.z, -R9.x, c[3].y, R0;
MUL R8.x, R12.z, c[4].y;
MAD R7.z, -R11.y, c[4].y, R0.w;
MAD R0.z, -R11.x, c[3], R0;
MUL R0.w, R8, R0.z;
MUL R0.z, R8.w, R7;
MUL R7.z, R9.y, c[4].w;
MAD R0.y, R0, c[4].z, R7.z;
MAD R8.x, R9.w, c[4].w, R8;
MAD R7.z, R10.w, c[4], R8.x;
MAD R0.y, R11.z, c[4], R0;
MAD R0.y, -R9.z, c[4].w, R0;
MAD R0.x, -R0, c[4].y, R0.y;
MAD R0.y, -R10, c[4].z, R0.x;
MAD R7.z, -R10, c[4], R7;
MAD R7.z, -R10.x, c[4].w, R7;
MAD R7.z, -R11.w, c[4].y, R7;
MUL R0.x, R8.w, R7.z;
MUL R0.y, R8.w, R0;
DP4 R7.z, R6, c[3];
DP4 result.texcoord[2].x, R0, R1;
DP4 result.texcoord[1].x, R4, R0;
MOV R0.xyz, c[6];
DP4 result.texcoord[2].w, R5, R1;
DP4 result.texcoord[2].z, R3, R1;
DP4 result.texcoord[2].y, R2, R1;
MUL R1, R0.xyzz, -c[7].xyzz;
MUL R0.w, -R1.x, c[6].x;
MOV result.texcoord[3].xyz, R1;
MUL R1.xy, R0.y, -c[7].y;
MUL R0.xy, R0.zxzw, -c[7].zxzw;
MAD R0.w, -c[5].x, c[5].x, R0;
MAD R0.w, -R1.x, c[6].y, R0;
DP4 result.texcoord[1].w, R5, R4;
DP4 result.texcoord[1].z, R4, R3;
DP4 result.texcoord[1].y, R4, R2;
MOV result.position, R7;
MOV result.texcoord[4], R7;
MOV result.color.w, R1;
MAD result.texcoord[3].w, -R0.x, c[6].z, R0;
MOV result.texcoord[6].w, R0.y;
MOV result.texcoord[7].w, R1.y;
MOV result.texcoord[6].yz, c[0].x;
MOV result.texcoord[7].xz, c[0].x;
MOV result.color.xy, c[0].x;
MOV result.texcoord[6].x, c[7];
MOV result.texcoord[7].y, c[7];
MOV result.color.z, c[7];
END
# 244 instructions, 14 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Bind "vertex" Vertex
Matrix 0 [glstate_matrix_mvp]
Float 4 [_Rayon]
Vector 5 [_TexPos]
Vector 6 [_Equation]
"vs_3_0
; 278 ALU, 2 FLOW
dcl_position o0
dcl_texcoord1 o1
dcl_texcoord2 o2
dcl_texcoord6 o3
dcl_texcoord7 o4
dcl_color0 o5
dcl_texcoord3 o6
dcl_texcoord4 o7
def c7, 1.00000000, 2.00000000, 0.00000000, 0
dcl_position0 v0
mov r0.y, c0.z
mul r1.y, c1.w, r0
mov r0.x, c0.w
mul r2.z, c1.y, r0.x
mov r0.y, c0
mul r5.w, c1.z, r0.y
mul r0.z, r1.y, c3.y
mad r0.y, r5.w, c3.w, r0.z
mad r0.z, r2, c3, r0.y
mov r0.x, c0.z
mul r6.x, c1.y, r0
mov r0.y, c0
mul r8.z, c1.w, r0.y
mul r1.x, r6, c2.w
mov r0.x, c0.w
mul r1.z, c1, r0.x
mad r0.y, r8.z, c2.z, r1.x
mad r0.x, r1.z, c2.y, r0.y
mad r0.z, -r8, c3, r0
mad r0.y, -r6.x, c3.w, r0.z
mad r0.z, -r5.w, c2.w, r0.x
mov r0.x, c0
mul r3.w, c1.z, r0.x
mad r0.z, -r1.y, c2.y, r0
mov r0.x, c0
mul r6.y, c1, r0.x
mul r0.w, r3, c2
mul r0.x, r6.y, c2.z
mul r0.w, r0, c3.y
mad r1.w, r0.x, c3, r0
mov r0.w, c0.x
mov r0.x, c0.y
mul r6.z, c1.x, r0.x
mul r8.y, c1.w, r0.w
mul r2.x, r6.z, c2.w
mad r9.x, r8.y, c2.y, r2
mul r0.x, r8.y, c2.y
mad r0.x, r0, c3.z, r1.w
mad r0.w, r2.x, c3.z, r0.x
mul r6.w, r5, c2.x
mov r0.x, c0.z
mul r5.z, c1.x, r0.x
mad r0.w, r6, c3, r0
mul r0.x, r8.z, c2.z
mad r0.w, r0.x, c3.x, r0
mul r0.x, r5.z, c2.y
mad r0.w, r0.x, c3, r0
mov r0.x, c0.w
mul r2.w, c1.x, r0.x
mad r9.x, r2.z, c2, r9
mad r6.w, r6.y, c2.z, r6
mov r2.x, c1
mul r0.x, r1.y, c2
mad r0.w, r1.x, c3.x, r0
mad r1.x, r0, c3.y, r0.w
mul r0.w, r2, c2.z
mad r1.x, r0.w, c3.y, r1
mul r0.w, r2.z, c2.x
mad r1.x, r0.w, c3.z, r1
mul r0.w, r1.z, c2.y
mad r1.x, r0.w, c3, r1
mul r0.w, r6.y, c2
mad r1.x, -r0.w, c3.z, r1
mul r0.w, r3, c2.y
mad r1.x, -r0.w, c3.w, r1
mul r0.w, r8.y, c2.z
mad r1.x, -r0.w, c3.y, r1
mul r0.w, r6.z, c2.z
mad r1.x, -r0.w, c3.w, r1
mul r0.w, r5, c2
mad r1.x, -r0.w, c3, r1
mul r0.w, r8.z, c2.x
mad r1.x, -r0.w, c3.z, r1
mul r0.w, r5.z, c2
mad r1.x, -r0.w, c3.y, r1
mul r0.w, r6.x, c2.x
mad r1.x, -r0.w, c3.w, r1
mul r0.w, r1.y, c2.y
mad r1.x, -r0.w, c3, r1
mul r0.w, r2, c2.y
mad r1.x, -r0.w, c3.z, r1
mul r0.w, r2.z, c2.z
mad r1.x, -r0.w, c3, r1
mul r0.w, r1.z, c2.x
mad r0.w, -r0, c3.y, r1.x
rcp r3.x, r0.w
mad r0.z, -r2, c2, r0
mul r0.w, r3.x, r0.z
mad r0.z, -r1, c3.y, r0.y
mov r0.y, c0.z
mul r3.y, c2, r0
mov r0.y, c0
mul r7.y, c2.w, r0
mul r1.x, r3.y, c3.w
mad r1.w, r7.y, c3.z, r1.x
mov r1.x, c0.w
mul r4.x, c2.z, r1
mov r0.y, c0
mul r3.z, c2, r0.y
mad r1.x, r4, c3.y, r1.w
mad r1.w, -r3.z, c3, r1.x
mov r1.x, c0.z
mul r4.w, c2, r1.x
mad r1.x, -r4.w, c3.y, r1.w
mov r0.y, c0.w
mul r7.x, c2.y, r0.y
mad r0.y, -r7.x, c3.z, r1.x
mov r1.w, c1.y
mul r4.z, c2, r1.w
mov r1.x, c1.z
mul r1.x, c2.w, r1
mul r2.y, r1.x, c3
mad r4.y, r4.z, c3.w, r2
mov r2.y, c1.w
mul r7.z, c2.y, r2.y
mov r1.w, c1.y
mul r2.y, c2.w, r1.w
mad r4.y, r7.z, c3.z, r4
mad r5.x, -r2.y, c3.z, r4.y
mov r1.w, c1
mov r4.y, c1.z
mul r4.y, c2, r4
mul r5.y, c2.z, r1.w
mad r5.x, -r4.y, c3.w, r5
mad r1.w, -r5.y, c3.y, r5.x
mad r5.x, r3.w, c2.w, r0
mul r0.x, r3, r1.w
mad r1.w, r2, c2.z, r5.x
mul r5.x, r5.z, c3.w
mad r1.w, -r8.y, c2.z, r1
mad r5.x, r8.y, c3.z, r5
mad r5.x, r1.z, c3, r5
mad r1.w, -r5.z, c2, r1
mad r1.z, -r1, c2.x, r1.w
mad r5.x, -r3.w, c3.w, r5
mad r1.y, -r1, c3.x, r5.x
mul r1.w, r3.x, r1.z
mov r1.z, c0.w
mul r7.w, c2.x, r1.z
mov r5.x, c0
mul r5.x, c2.z, r5
mul r4.w, r4, c3.x
mad r4.w, r5.x, c3, r4
mad r8.w, r7, c3.z, r4
mov r4.w, c0.x
mul r8.x, c2.w, r4.w
mov r1.z, c0
mad r8.w, -r8.x, c3.z, r8
mul r4.w, c2.x, r1.z
mad r1.z, -r4.w, c3.w, r8.w
mad r4.x, -r4, c3, r1.z
mad r1.y, -r2.w, c3.z, r1
mul r1.z, r3.x, r1.y
mul r1.y, r3.x, r4.x
mov r4.x, c1.z
mul r4.x, c2, r4
mul r8.w, r4.x, c3
mul r2.x, c2.w, r2
mad r9.y, r2.x, c3.z, r8.w
mad r9.x, -r6.y, c2.w, r9
mad r6.w, r5.z, c2.y, r6
mov r8.w, c1.x
mad r9.y, r5, c3.x, r9
mul r5.y, c2.z, r8.w
mad r9.y, -r5, c3.w, r9
mad r1.x, -r1, c3, r9.y
mul r9.y, r8.z, c3.x
mad r8.z, -r8, c2.x, r9.x
mad r9.x, r6.y, c3.w, r9.y
mov r8.w, c1
mul r8.w, c2.x, r8
mad r1.x, -r8.w, c3.z, r1
mad r8.z, -r2.w, c2.y, r8
mad r9.x, r2.w, c3.y, r9
mul r2.w, r3.x, r8.z
mad r8.z, -r8.y, c3.y, r9.x
mad r8.z, -r6, c3.w, r8
mad r2.z, -r2, c3.x, r8
mov r8.y, c0
mul r8.y, c2.x, r8
mul r8.z, r8.y, c3.w
mad r8.z, r8.x, c3.y, r8
mov r8.x, c0
mad r8.z, r7.x, c3.x, r8
mul r7.x, c2.y, r8
mad r8.z, -r7.x, c3.w, r8
mov r8.x, c1
mul r8.x, c2.y, r8
mul r2.y, r2, c3.x
mad r2.y, r8.x, c3.w, r2
mad r8.z, -r7.y, c3.x, r8
mad r7.y, r8.w, c3, r2
mad r2.x, -r2, c3.y, r7.y
mov r2.y, c1
mul r7.y, c2.x, r2
mad r2.y, -r7, c3.w, r2.x
mad r7.z, -r7, c3.x, r2.y
mad r2.x, -r7.w, c3.y, r8.z
mul r2.y, r3.x, r2.x
mul r2.x, r3, r7.z
mul r7.z, r6, c3
mad r7.z, r3.w, c3.y, r7
mad r3.w, -r3, c2.y, r6
mad r6.w, r6.x, c3.x, r7.z
mad r3.w, -r6.z, c2.z, r3
mad r6.y, -r6, c3.z, r6.w
mad r5.w, -r5, c3.x, r6.y
mad r5.z, -r5, c3.y, r5.w
mul r5.w, r3.z, c3.x
mul r3.z, r3.x, r5
mad r5.z, r7.x, c3, r5.w
mad r3.w, -r6.x, c2.x, r3
mul r5.w, r7.y, c3.z
mad r4.w, r4, c3.y, r5.z
mad r4.w, -r5.x, c3.y, r4
mad r5.y, r5, c3, r5.w
mad r5.x, r4.y, c3, r5.y
mad r4.y, -r8, c3.z, r4.w
mad r3.y, -r3, c3.x, r4
mad r4.w, -r8.x, c3.z, r5.x
mad r4.y, -r4.z, c3.x, r4.w
mad r4.y, -r4.x, c3, r4
mov r4.x, c7
mul r0.z, r3.x, r0
mul r0.y, r3.x, r0
mul r1.x, r3, r1
mul r2.z, r3.x, r2
mul r3.w, r3.x, r3
mul r3.y, r3.x, r3
mul r3.x, r3, r4.y
if_lt c4.x, r4.x
mul r4.xyz, v0, c7.y
add r4.xyz, r4, c5
mov r4.w, c7.x
else
mul r4.xyz, v0, c4.x
mul r4.xyz, r4, c7.y
add r4.xyz, r4, c5
mov r4.w, c7.x
endif
dp4 r5.z, r4, c3
mov r7.w, r5.z
dp4 r5.x, r4, c0
dp4 r5.y, r4, c1
mov r5.zw, r5.z
mov r7.xy, r5
dp4 r7.z, r4, c2
mov r6.xyw, r7
mov r6.z, c7
dp4 o1.x, r6, r0
dp4 o2.x, r0, r5
mov r0.xy, c6.xzzw
mul r0.xyz, c5.xzzw, -r0.xyyw
mov o5.w, r0.z
mov r0.z, c6.y
mul r0.zw, c5.y, -r0.z
mul r0.x, -r0, c5
dp4 o1.y, r6, r1
dp4 o2.y, r1, r5
mad r1.x, -c4, c4, r0
mov o6.z, r0.y
mov r0.xy, c6.xzzw
mul r0.xy, c5.zxzw, -r0.yxzw
mad r0.z, -r0, c5.y, r1.x
mad o6.w, -r0.x, c5.z, r0.z
mov o3.w, r0.y
mov r0.xy, c5
dp4 o1.w, r3, r6
dp4 o1.z, r6, r2
dp4 o2.w, r3, r5
dp4 o2.z, r2, r5
mov o0, r7
mov o7, r7
mov o3.yz, c7.z
mov o4.xz, c7.z
mov o4.w, r0
mov o5.xy, c7.z
mul o6.xy, -c6, r0
mov o3.x, c6
mov o4.y, c6
mov o5.z, c6
"
}

}
Program "fp" {
// Fragment combos: 1
//   opengl - ALU: 65 to 65, TEX: 1 to 1
//   d3d9 - ALU: 65 to 65, TEX: 3 to 3
SubProgram "opengl " {
Keywords { }
Matrix 8 [_Object2World]
Vector 12 [_Color]
Float 13 [_Cut]
Vector 14 [_Cutplane]
SetTexture 0 [_MatCap] 2D
"3.0-!!ARBfp1.0
# 65 ALU, 1 TEX
PARAM c[17] = { state.matrix.mvp,
		state.matrix.modelview[0].invtrans,
		program.local[8..14],
		{ 1, 0, 0.5, 200 },
		{ 1.5 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
RCP R0.x, fragment.texcoord[1].w;
MUL R3.xyz, fragment.texcoord[1], R0.x;
MOV R3.w, c[15].x;
RCP R0.x, fragment.texcoord[2].w;
MAD R2.xyz, fragment.texcoord[2], R0.x, -R3;
MOV R2.w, c[15].y;
DP4 R1.w, fragment.texcoord[3], R2;
DP4 R1.z, R2, fragment.color.primary;
DP4 R1.y, R2, fragment.texcoord[7];
DP4 R1.x, R2, fragment.texcoord[6];
DP4 R4.y, R3, R1;
DP4 R0.w, R3, fragment.texcoord[3];
DP4 R0.z, R3, fragment.color.primary;
DP4 R0.x, R3, fragment.texcoord[6];
DP4 R0.y, R3, fragment.texcoord[7];
DP4 R0.y, R3, R0;
DP4 R0.x, R1, R2;
MOV R0.w, c[15].x;
MUL R4.x, R0, R0.y;
MUL R3.w, R4.y, R4.y;
ADD R0.y, R3.w, -R4.x;
RSQ R0.y, R0.y;
RCP R0.z, R0.x;
RCP R0.y, R0.y;
ADD R0.x, -R4.y, -R0.y;
MUL R0.x, R0, R0.z;
MAD R0.xyz, R0.x, R2, R3;
DP4 R1.w, R0, fragment.texcoord[3];
DP4 R1.z, R0, fragment.color.primary;
DP4 R1.x, R0, fragment.texcoord[6];
DP4 R1.y, R0, fragment.texcoord[7];
DP4 R2.z, R1, c[6];
DP4 R2.x, R1, c[4];
DP4 R2.y, R1, c[5];
DP3 R1.x, R2, R2;
RSQ R1.x, R1.x;
MUL R1.xyz, R1.x, R2;
MAX R1.w, R1.z, c[15].y;
POW R1.w, R1.w, c[15].w;
MAD R2.xyz, R1.z, c[12], R1.w;
MAD R1.xy, R1, c[15].z, c[15].z;
TEX R1, R1, texture[0], 2D;
MOV R2.w, c[12];
MUL R1, R2, R1;
MUL result.color, R1, c[16].x;
DP4 R2.x, R0, c[3];
DP4 R1.x, R0, c[2];
RCP R1.y, R2.x;
MAD R1.x, R1, R1.y, c[15];
DP4 R1.y, R0, c[8];
MUL result.depth.z, R1.x, c[15];
DP4 R1.x, R0, c[9];
DP4 R0.y, R0, c[10];
MAD R1.y, R1, c[14].x, c[14].w;
MAD R1.y, R1.x, c[14], R1;
MOV R1.x, c[15];
ADD R0.x, -R1, c[13];
MUL R0.y, R0, c[14].z;
ABS R0.x, R0;
SLT R0.y, R1, -R0;
CMP R0.x, -R0, c[15].y, c[15];
MUL R0.x, R0, R0.y;
SLT R0.y, R3.w, R4.x;
KIL -R0.x;
KIL -R0.y;
END
# 65 instructions, 5 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Matrix 0 [glstate_matrix_mvp]
Matrix 4 [glstate_matrix_invtrans_modelview0]
Matrix 8 [_Object2World]
Vector 12 [_Color]
Float 13 [_Cut]
Vector 14 [_Cutplane]
SetTexture 0 [_MatCap] 2D
"ps_3_0
; 65 ALU, 3 TEX
dcl_2d s0
def c15, 1.00000000, 0.00000000, -1.00000000, 200.00000000
def c16, 0.50000000, 1.50000000, 0, 0
dcl_texcoord1 v0
dcl_texcoord2 v1
dcl_texcoord6 v2
dcl_texcoord7 v3
dcl_color0 v4
dcl_texcoord3 v5
rcp r0.x, v0.w
mul r1.xyz, v0, r0.x
mov r1.w, c15.x
dp4 r0.w, r1, v5
dp4 r0.z, r1, v4
dp4 r0.x, r1, v2
dp4 r0.y, r1, v3
dp4 r3.x, r1, r0
rcp r0.x, v1.w
mad r0.xyz, v1, r0.x, -r1
mov r0.w, c15.y
dp4 r2.w, v5, r0
dp4 r2.z, r0, v4
dp4 r2.x, r0, v2
dp4 r2.y, r0, v3
dp4 r0.w, r2, r0
dp4 r1.w, r1, r2
mul r3.x, r0.w, r3
mad r3.w, r1, r1, -r3.x
rsq r2.x, r3.w
mov r2.w, c15.x
rcp r2.y, r0.w
rcp r2.x, r2.x
add r0.w, -r1, -r2.x
mul r0.w, r0, r2.y
mad r2.xyz, r0, r0.w, r1
dp4 r0.w, v5, r2
dp4 r0.z, v4, r2
dp4 r0.x, v2, r2
dp4 r0.y, v3, r2
dp4 r1.z, r0, c6
dp4 r1.x, r0, c4
dp4 r1.y, r0, c5
dp3 r0.x, r1, r1
rsq r0.x, r0.x
mul r3.xyz, r0.x, r1
max r1.x, r3.z, c15.y
pow r0, r1.x, c15.w
mad r1.xyz, r3.z, c12, r0.x
mad_pp r0.xy, r3, c16.x, c16.x
dp4 r1.w, r2, c8
mad r3.x, r1.w, c14, c14.w
dp4 r1.w, r2, c9
mad r3.y, r1.w, c14, r3.x
mov r1.w, c13.x
dp4 r3.x, r2, c10
add r1.w, c15.z, r1
mad r3.x, r3, c14.z, r3.y
abs r1.w, r1
texld r0, r0, s0
cmp r1.w, -r1, c15.x, c15.y
cmp r3.x, r3, c15.y, c15
mul_pp r3.x, r1.w, r3
mov r1.w, c12
mul r1, r1, r0
mov_pp r0, -r3.x
texkill r0.xyzw
dp4 r0.y, r2, c3
rcp r0.z, r0.y
dp4 r0.y, r2, c2
mul oC0, r1, c16.y
mad r1.x, r0.y, r0.z, c15
cmp r0.x, r3.w, c15.y, c15
mov_pp r0, -r0.x
mul oDepth, r1.x, c16.x 	//Hack for Direct3D. Original line : mul oDepth.z, r1.x, c16.x
texkill r0.xyzw
"
}

}

#LINE 376

	}

} 
// Pour les cartes graphiques ne supportant pas nos Shaders
Fallback "VertexLit"
}