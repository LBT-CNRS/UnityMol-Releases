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
/// $Id: Ball_HyperBalls_D3D.shader 378 2013-09-10 17:18:27Z kouyoumdjian $
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

// $Id: Ball_HyperBalls_D3D.shader 378 2013-09-10 17:18:27Z kouyoumdjian $
// (c) 2010 by Marc Baaden & Matthieu Chavent, <baaden@smplinux.de> <matthieu.chavent@free.fr>
// Unity3D FvNano shaders coming from FVNano project
//
// On a un cube (mesh) dans la scene -> vertex
// la spherepos du cube est 0, 0, 0


Shader "FvNano/Ball HyperBalls D3D" {

// Properties exposed to the interface
Properties {
	_Rayon ("Rayon de la Sphere", float) = 0.1
	_Color ("Couleur de la Sphere", Color) = (1,0.1,0.1,1.0)
	_TexPos ("Position de la sphere", Vector) = (0.0,0.0,0.0,1.0)     // important que w != 0 !!
	_Visibilite ("Visibilite de la Sphere", float) = 1.0
	_Light ("Light vector", Vector) = (1,0,0,0)
	_Equation("Equation", Vector) = (1,1,1,1)
	_MatCap ("MatCap (RGB)", 2D) = "white" {}
	_Attenuation ("Attenuation", float) = 0
	_Brightness ("Brightness", float) = 1.0
}


// ==========================================================
// L'actuel Shader Cg =======================================
// ==========================================================

SubShader {
	ZWrite On // doesn't seem to help
	Pass {
		//Name "BASE"
        //Tags { "LightMode" = "Always" }		

Program "vp" {
// Vertex combos: 1
//   opengl - ALU: 210 to 210
//   d3d9 - ALU: 245 to 245, FLOW: 2 to 2
//   d3d11 - ALU: 65 to 65, TEX: 0 to 0, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
Bind "vertex" Vertex
Float 5 [_Rayon]
Vector 6 [_TexPos]
Vector 7 [_Equation]
"3.0-!!ARBvp1.0
# 210 ALU
PARAM c[8] = { { 0, 2, 1, -1 },
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
MOV R0, c[4];
MUL R2, R0.wxyz, c[3].yzwx;
MAD R2, R0.yzwx, c[3].wxyz, -R2;
MUL R3, R2, c[2].zwxy;
MUL R1, R0.zwxy, c[3].wxyz;
MAD R1, R0.wxyz, c[3].zwxy, -R1;
MUL R2, R0.yzwx, c[3].zwxy;
MAD R0, R0.zwxy, c[3].yzwx, -R2;
MOV R2, c[2];
MAD R1, R1, c[2].yzwx, R3;
MAD R0, R0, c[2].wxyz, R1;
MOV R1, c[3];
MUL R0, R0, c[1];
DP4 R0.x, R0, c[0].zwzw;
MUL R3.w, R2.x, c[1].y;
MUL R10.z, R2.x, c[1];
MOV R4.x, c[0].z;
SLT R4.x, c[5], R4;
ABS R4.x, R4;
RCP R0.w, R0.x;
MUL R9.x, R2.z, c[1].y;
MUL R11.w, R2.y, c[1].x;
MUL R0.x, R9, c[3];
MAD R0.x, R11.w, c[3].z, R0;
MUL R10.y, R2.z, c[1].x;
MUL R0.y, R3.w, c[4].z;
MAD R0.x, R10.z, c[3].y, R0;
MAD R0.x, -R10.y, c[3].y, R0;
MUL R8.w, R2.y, c[1].z;
MAD R0.x, -R3.w, c[3].z, R0;
MAD R0.y, R10, c[4], R0;
MAD R0.x, -R8.w, c[3], R0;
MAD R0.y, R8.w, c[4].x, R0;
MUL R12.z, R2.w, c[1].x;
MUL R12.x, R2.w, c[1].y;
MUL R12.y, R2, c[1].w;
MUL R5.w, R0, R0.x;
MAD R0.y, -R11.w, c[4].z, R0;
MAD R0.x, -R9, c[4], R0.y;
MAD R0.x, -R10.z, c[4].y, R0;
MUL R5.z, R0.w, R0.x;
MUL R8.x, R1.z, c[1].y;
MUL R11.x, R1.y, c[1];
MUL R0.x, R8, c[4];
MUL R9.z, R1.x, c[1];
MAD R0.x, R11, c[4].z, R0;
MUL R9.y, R1.z, c[1].x;
MAD R0.x, R9.z, c[4].y, R0;
MUL R11.z, R1.x, c[1].y;
MAD R0.x, -R9.y, c[4].y, R0;
MUL R7.z, R1.y, c[1];
MAD R0.x, -R11.z, c[4].z, R0;
MAD R0.x, -R7.z, c[4], R0;
MUL R10.w, R1.x, c[2].y;
MUL R5.y, R0.w, R0.x;
MUL R11.y, R1, c[2].x;
MOV R6.w, c[0].z;
MOV R4.z, c[0].x;
MUL R2.x, R2, c[1].w;
MUL R10.x, R1.z, c[2];
MUL R0.x, R10.w, c[4].z;
MUL R8.z, R1.y, c[2];
MAD R0.x, R10, c[4].y, R0;
MAD R0.x, R8.z, c[4], R0;
MAD R0.x, -R11.y, c[4].z, R0;
MUL R8.y, R1.z, c[2];
MAD R3.x, -R8.y, c[4], R0;
MUL R9.w, R1.x, c[2].z;
MAD R3.x, -R9.w, c[4].y, R3;
MUL R0.xyz, vertex.position, c[5].x;
MUL R5.x, R0.w, R3;
MUL R0.xyz, R0, c[0].y;
ADD R3.xyz, R0, c[6];
MUL R0.xyz, vertex.position, c[0].y;
ADD R0.xyz, R0, c[6];
ADD R3.xyz, R3, -R0;
SGE R4.x, c[0], R4;
MAD R6.xyz, R3, R4.x, R0;
DP4 R0.z, R6, c[4];
MUL R3.x, R3.w, c[3].w;
MAD R3.x, R12.z, c[3].y, R3;
MAD R2.y, R12, c[3].x, R3.x;
MAD R2.y, -R11.w, c[3].w, R2;
MUL R3.y, R11.z, c[4].w;
MUL R3.x, R12, c[4];
MAD R3.x, R11.w, c[4].w, R3;
MAD R3.x, R2, c[4].y, R3;
MAD R3.x, -R12.z, c[4].y, R3;
MAD R3.x, -R3.w, c[4].w, R3;
MAD R3.x, -R12.y, c[4], R3;
MAD R2.y, -R12.x, c[3].x, R2;
MAD R2.y, -R2.x, c[3], R2;
MUL R3.w, R0, R2.y;
MUL R2.y, R1.w, c[1].x;
DP4 R0.x, R6, c[1];
DP4 R0.y, R6, c[2];
MUL R3.z, R0.w, R3.x;
MOV R7.xy, R0;
MOV R7.w, R0.z;
MOV R4.xyw, R7;
MUL R11.z, R1.y, c[1].w;
MAD R3.y, R2, c[4], R3;
MAD R3.y, R11.z, c[4].x, R3;
MAD R3.x, -R11, c[4].w, R3.y;
MUL R11.w, R1, c[2].y;
MUL R11.x, R1.w, c[1].y;
MUL R3.y, R11.w, c[4].x;
MAD R3.y, R11, c[4].w, R3;
MUL R12.w, R1.x, c[1];
MUL R11.y, R1.x, c[2].w;
MAD R3.x, -R11, c[4], R3;
MUL R13.x, R1.w, c[2];
MAD R1.x, R11.y, c[4].y, R3.y;
MAD R3.x, -R12.w, c[4].y, R3;
MUL R9.w, R9, c[4];
MUL R3.y, R0.w, R3.x;
MAD R1.x, -R13, c[4].y, R1;
MAD R3.x, -R10.w, c[4].w, R1;
MUL R10.w, R2, c[1].z;
MUL R1.x, R1.y, c[2].w;
MAD R1.y, -R1.x, c[4].x, R3.x;
MUL R3.x, R0.w, R1.y;
MUL R2.w, R10, c[3].x;
MAD R1.y, R10, c[3].w, R2.w;
MAD R1.y, R2.x, c[3].z, R1;
MAD R2.w, -R12.z, c[3].z, R1.y;
MUL R1.y, R2.z, c[1].w;
MAD R2.z, -R10, c[3].w, R2.w;
MUL R13.y, R10.z, c[4].w;
MAD R12.z, R12, c[4], R13.y;
MAD R12.z, R1.y, c[4].x, R12;
MAD R2.w, -R10.y, c[4], R12.z;
MAD R10.y, -R10.w, c[4].x, R2.w;
MAD R2.x, -R2, c[4].z, R10.y;
MAD R2.z, -R1.y, c[3].x, R2;
MUL R2.w, R0, R2.z;
MUL R2.z, R0.w, R2.x;
MUL R10.y, R1.w, c[1].z;
MUL R2.x, R10.y, c[4];
MAD R2.x, R9.y, c[4].w, R2;
MUL R9.y, R1.z, c[2].w;
MAD R9.w, R13.x, c[4].z, R9;
MAD R9.w, R9.y, c[4].x, R9;
MAD R10.x, -R10, c[4].w, R9.w;
MUL R9.w, R1, c[2].z;
MAD R2.x, R12.w, c[4].z, R2;
MAD R1.w, -R2.y, c[4].z, R2.x;
MAD R1.w, -R9.z, c[4], R1;
MUL R9.z, R1, c[1].w;
MAD R1.z, -R9, c[4].x, R1.w;
MUL R2.y, R0.w, R1.z;
MAD R10.x, -R9.w, c[4], R10;
MAD R2.x, -R11.y, c[4].z, R10;
MUL R2.x, R0.w, R2;
MUL R1.z, R8.w, c[3].w;
MUL R1.w, R10, c[4].y;
MAD R1.z, R12.x, c[3], R1;
MAD R1.z, R1.y, c[3].y, R1;
MAD R1.w, R9.x, c[4], R1;
MAD R1.w, R12.y, c[4].z, R1;
MAD R1.z, -R9.x, c[3].w, R1;
MAD R1.w, -R12.x, c[4].z, R1;
MAD R1.w, -R8, c[4], R1;
MAD R1.z, -R10.w, c[3].y, R1;
MAD R1.y, -R1, c[4], R1.w;
MAD R1.z, -R12.y, c[3], R1;
MUL R1.w, R0, R1.z;
MUL R1.z, R0.w, R1.y;
MUL R1.y, R7.z, c[4].w;
MUL R7.z, R9.w, c[4].y;
MAD R1.y, R11.x, c[4].z, R1;
MAD R7.z, R8.y, c[4].w, R7;
MAD R7.z, R1.x, c[4], R7;
MAD R1.y, R9.z, c[4], R1;
MAD R1.x, -R8, c[4].w, R1.y;
MAD R1.y, -R11.w, c[4].z, R7.z;
MAD R1.y, -R8.z, c[4].w, R1;
MAD R1.x, -R10.y, c[4].y, R1;
MAD R7.z, -R9.y, c[4].y, R1.y;
MAD R1.x, -R11.z, c[4].z, R1;
MUL R1.y, R0.w, R1.x;
MUL R1.x, R0.w, R7.z;
MOV R0.zw, R0.z;
DP4 R7.z, R6, c[3];
DP4 result.texcoord[2].z, R3, R0;
DP4 result.texcoord[1].z, R4, R3;
DP4 result.texcoord[2].y, R2, R0;
DP4 result.texcoord[1].y, R4, R2;
MOV R3.xyz, c[6];
MUL R2.xyz, R3, c[7];
DP4 result.texcoord[2].w, R5, R0;
DP4 result.texcoord[2].x, R1, R0;
MUL R0.xyz, R2, c[6];
MAD R0.x, -c[5], c[5], R0;
ADD R0.x, R0, R0.y;
DP4 result.texcoord[1].w, R5, R4;
DP4 result.texcoord[1].x, R4, R1;
MOV result.position, R7;
MOV result.texcoord[4], R7;
ADD result.texcoord[3].w, R0.x, R0.z;
MOV result.texcoord[6].w, -R2.x;
MOV result.texcoord[7].w, -R2.y;
MOV result.color.w, -R2.z;
MOV result.texcoord[3].xyz, -R2;
MOV result.texcoord[6].yz, c[0].x;
MOV result.texcoord[7].xz, c[0].x;
MOV result.color.xy, c[0].x;
MOV result.texcoord[6].x, c[7];
MOV result.texcoord[7].y, c[7];
MOV result.color.z, c[7];
END
# 210 instructions, 14 R-regs
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
; 245 ALU, 2 FLOW
dcl_position o0
dcl_texcoord1 o1
dcl_texcoord2 o2
dcl_texcoord6 o3
dcl_texcoord7 o4
dcl_color0 o5
dcl_texcoord3 o6
dcl_texcoord4 o7
def c7, 1.00000000, -1.00000000, 2.00000000, 0.00000000
dcl_position0 v0
mov r1, c2
mul r2, c3.wxyz, r1.yzwx
mov r7.x, c1.w
mul r8.x, c2, r7
mov r1, c2
mad r1, c3.yzwx, r1.wxyz, -r2
mov r0, c2
mul r2, c3.zwxy, r0.wxyz
mov r0, c2
mad r2, c3.wxyz, r0.zwxy, -r2
mul r1, r1, c1.zwxy
mov r7.x, c0
mad r1, r2, c1.yzwx, r1
mov r0, c2
mul r2, c3.yzwx, r0.zwxy
mov r0, c2
mad r0, c3.zwxy, r0.yzwx, -r2
mad r0, r0, c1.wxyz, r1
mul r0, r0, c0
dp4 r0.x, r0, c7.xyxy
rcp r3.x, r0.x
mov r0.x, c0.z
mul r6.y, c1, r0.x
mov r0.y, c0.w
mul r1.z, c1, r0.y
mov r0.x, c0.y
mov r0.y, c0.w
mul r5.w, c1.y, r0.y
mul r8.w, c1, r0.x
mul r0.z, r6.y, c2.w
mad r0.x, r8.w, c2.z, r0.z
mad r0.z, r1, c2.y, r0.x
mov r0.x, c0.z
mul r1.w, c1, r0.x
mov r0.x, c0.y
mul r3.w, c1.z, r0.x
mul r0.w, r1, c3.y
mad r0.x, r3.w, c3.w, r0.w
mad r0.y, r5.w, c3.z, r0.x
mad r0.x, -r3.w, c2.w, r0.z
mad r0.y, -r8.w, c3.z, r0
mad r0.x, -r1.w, c2.y, r0
mad r0.x, -r5.w, c2.z, r0
mad r0.y, -r6, c3.w, r0
mad r0.y, -r1.z, c3, r0
mul r0.w, r3.x, r0.x
mul r0.z, r3.x, r0.y
mov r0.x, c0.z
mul r4.y, c2, r0.x
mov r0.y, c0.w
mul r1.y, c2.z, r0
mov r0.x, c0.y
mov r0.y, c0.z
mul r4.x, c2.w, r0.y
mul r7.x, c1.y, r7
mul r1.x, r4.y, c3.w
mul r2.y, c2.w, r0.x
mad r0.x, r2.y, c3.z, r1
mad r1.x, r1.y, c3.y, r0
mov r0.x, c0.y
mul r3.z, c2, r0.x
mad r0.x, -r3.z, c3.w, r1
mad r1.x, -r4, c3.y, r0
mov r0.x, c0.w
mul r2.z, c2.y, r0.x
mov r0.x, c1.y
mul r3.y, c2.z, r0.x
mov r0.x, c1.w
mad r4.w, -r2.z, c3.z, r1.x
mov r0.y, c1.z
mul r1.x, c2.w, r0.y
mul r0.y, r1.x, c3
mad r2.w, r3.y, c3, r0.y
mul r2.x, c2.y, r0
mov r0.y, c1
mul r7.y, c2.w, r0
mad r0.x, r2, c3.z, r2.w
mad r2.w, -r7.y, c3.z, r0.x
mov r0.x, c1.z
mul r4.z, c2.y, r0.x
mad r0.x, -r4.z, c3.w, r2.w
mov r0.y, c1.w
mul r2.w, c2.z, r0.y
mad r0.x, -r2.w, c3.y, r0
mul r0.y, r3.x, r4.w
mov r4.w, c0.x
mul r6.z, c1, r4.w
mul r5.x, r1.w, c2
mad r5.y, r6.z, c2.w, r5.x
mov r4.w, c0
mul r8.z, c1.x, r4.w
mov r5.x, c0.z
mul r6.x, c1, r5
mov r4.w, c0.x
mul r5.z, c1.w, r4.w
mad r5.y, r8.z, c2.z, r5
mad r4.w, -r5.z, c2.z, r5.y
mul r5.x, r6, c3.w
mad r5.x, r5.z, c3.z, r5
mad r5.x, r1.z, c3, r5
mad r4.w, -r6.x, c2, r4
mad r1.z, -r1, c2.x, r4.w
mad r5.x, -r6.z, c3.w, r5
mad r4.w, -r1, c3.x, r5.x
mul r1.w, r3.x, r1.z
mad r1.z, -r8, c3, r4.w
mov r5.x, c0.w
mul r7.z, c2.x, r5.x
mov r4.w, c0.x
mul r0.x, r3, r0
mul r4.w, c2.z, r4
mul r4.x, r4, c3
mad r4.x, r4.w, c3.w, r4
mad r5.y, r7.z, c3.z, r4.x
mov r4.x, c0
mul r8.y, c2.w, r4.x
mad r4.x, -r8.y, c3.z, r5.y
mov r5.x, c0.z
mul r5.y, c2.x, r5.x
mad r4.x, -r5.y, c3.w, r4
mad r1.y, -r1, c3.x, r4.x
mov r5.x, c1
mul r7.w, c2, r5.x
mov r4.x, c1.z
mul r4.x, c2, r4
mul r6.w, r4.x, c3
mad r6.w, r7, c3.z, r6
mov r5.x, c1
mad r2.w, r2, c3.x, r6
mul r5.x, c2.z, r5
mad r6.w, -r5.x, c3, r2
mad r1.x, -r1, c3, r6.w
mov r2.w, c0.y
mul r6.w, c1.x, r2
mul r2.w, r6, c2
mad r1.x, -r8, c3.z, r1
mad r2.w, r5.z, c2.y, r2
mad r2.w, r5, c2.x, r2
mad r2.w, -r7.x, c2, r2
mad r2.w, -r8, c2.x, r2
mad r2.w, -r8.z, c2.y, r2
mul r8.w, r8, c3.x
mad r8.w, r7.x, c3, r8
mad r8.w, r8.z, c3.y, r8
mad r8.w, -r5.z, c3.y, r8
mov r8.z, c0.y
mul r5.z, c2.x, r8
mad r8.z, -r6.w, c3.w, r8.w
mad r8.z, -r5.w, c3.x, r8
mul r8.w, r5.z, c3
mad r8.y, r8, c3, r8.w
mov r5.w, c0.x
mad r2.z, r2, c3.x, r8.y
mul r5.w, c2.y, r5
mad r8.y, -r5.w, c3.w, r2.z
mad r2.y, -r2, c3.x, r8
mad r2.y, -r7.z, c3, r2
mul r2.z, r3.x, r8
mov r7.z, c1.y
mov r8.y, c1.x
mul r8.z, r7.y, c3.x
mul r7.y, c2, r8
mad r8.y, r7, c3.w, r8.z
mad r8.x, r8, c3.y, r8.y
mad r7.w, -r7, c3.y, r8.x
mul r7.z, c2.x, r7
mad r7.w, -r7.z, c3, r7
mad r2.x, -r2, c3, r7.w
mul r7.w, r3, c2.x
mul r8.x, r6.w, c3.z
mad r7.w, r7.x, c2.z, r7
mad r7.w, r6.x, c2.y, r7
mad r8.x, r6.z, c3.y, r8
mad r6.z, -r6, c2.y, r7.w
mad r6.z, -r6.w, c2, r6
mad r7.w, r6.y, c3.x, r8.x
mad r6.y, -r6, c2.x, r6.z
mad r6.w, -r7.x, c3.z, r7
mad r6.z, -r3.w, c3.x, r6.w
mul r3.w, r3.x, r6.y
mul r6.y, r3.z, c3.x
mad r6.x, -r6, c3.y, r6.z
mul r3.z, r3.x, r6.x
mad r5.w, r5, c3.z, r6.y
mul r6.x, r7.z, c3.z
mad r5.y, r5, c3, r5.w
mad r5.x, r5, c3.y, r6
mad r4.w, -r4, c3.y, r5.y
mad r5.x, r4.z, c3, r5
mad r4.z, -r5, c3, r4.w
mad r4.y, -r4, c3.x, r4.z
mad r4.w, -r7.y, c3.z, r5.x
mad r4.z, -r3.y, c3.x, r4.w
mul r3.y, r3.x, r4
mad r4.y, -r4.x, c3, r4.z
mov r4.x, c7
mul r1.z, r3.x, r1
mul r1.y, r3.x, r1
mul r1.x, r3, r1
mul r2.w, r3.x, r2
mul r2.y, r3.x, r2
mul r2.x, r3, r2
mul r3.x, r3, r4.y
if_lt c4.x, r4.x
mul r4.xyz, v0, c7.z
add r4.xyz, r4, c5
mov r4.w, c7.x
else
mul r4.xyz, v0, c4.x
mul r4.xyz, r4, c7.z
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
mov r6.z, c7.w
dp4 o1.z, r6, r2
dp4 o2.z, r2, r5
dp4 o1.y, r6, r1
dp4 o1.x, r6, r0
mov r2.xyz, c6
dp4 o2.y, r1, r5
mul r1.xyz, c5, r2
dp4 o2.x, r0, r5
mul r0.xyz, r1, c5
mad r0.x, -c4, c4, r0
add r0.x, r0, r0.y
dp4 o1.w, r3, r6
dp4 o2.w, r3, r5
mov o0, r7
mov o7, r7
add o6.w, r0.x, r0.z
mov o3.w, -r1.x
mov o4.w, -r1.y
mov o5.w, -r1.z
mov o6.xyz, -r1
mov o3.yz, c7.w
mov o4.xz, c7.w
mov o5.xy, c7.w
mov o3.x, c6
mov o4.y, c6
mov o5.z, c6
"
}

SubProgram "d3d11 " {
Keywords { }
Bind "vertex" Vertex
ConstBuffer "$Globals" 160 // 112 used size, 11 vars
Float 16 [_Rayon]
Vector 48 [_TexPos] 4
Vector 96 [_Equation] 4
ConstBuffer "UnityPerDraw" 336 // 64 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
BindCB "$Globals" 0
BindCB "UnityPerDraw" 1
// 170 instructions, 16 temp regs, 0 temp arrays:
// ALU 65 float, 0 int, 0 uint
// TEX 0 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"vs_4_0
eefiecedofiplilgngcgkkionaioflojghiblnfcabaaaaaaiebkaaaaadaaaaaa
cmaaaaaagaaaaaaaemabaaaaejfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapahaaaafaepfdejfeejepeoaaklklkl
epfdeheooeaaaaaaaiaaaaaaaiaaaaaamiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaneaaaaaaabaaaaaaaaaaaaaaadaaaaaaabaaaaaaapaaaaaa
neaaaaaaacaaaaaaaaaaaaaaadaaaaaaacaaaaaaapaaaaaaneaaaaaaagaaaaaa
aaaaaaaaadaaaaaaadaaaaaaapaaaaaaneaaaaaaahaaaaaaaaaaaaaaadaaaaaa
aeaaaaaaapaaaaaannaaaaaaaaaaaaaaaaaaaaaaadaaaaaaafaaaaaaapaaaaaa
neaaaaaaadaaaaaaaaaaaaaaadaaaaaaagaaaaaaapaaaaaaneaaaaaaaeaaaaaa
aaaaaaaaadaaaaaaahaaaaaaapaaaaaafdfgfpfagphdgjhegjgpgoaafeeffied
epepfceeaaedepemepfcaaklfdeieefcdabjaaaaeaaaabaaemagaaaafjaaaaae
egiocaaaaaaaaaaaahaaaaaafjaaaaaeegiocaaaabaaaaaaaeaaaaaafpaaaaad
hcbabaaaaaaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaadpccabaaa
abaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaagfaaaaad
pccabaaaaeaaaaaagfaaaaadpccabaaaafaaaaaagfaaaaadpccabaaaagaaaaaa
gfaaaaadpccabaaaahaaaaaagiaaaaacbaaaaaaaaaaaaaajbcaabaaaaaaaaaaa
akiacaaaaaaaaaaaabaaaaaaakiacaaaaaaaaaaaabaaaaaadcaaaaakhcaabaaa
aaaaaaaaegbcbaaaaaaaaaaaagaabaaaaaaaaaaaegiccaaaaaaaaaaaadaaaaaa
dbaaaaaiicaabaaaaaaaaaaaakiacaaaaaaaaaaaabaaaaaaabeaaaaaaaaaiadp
dcaaaaanhcaabaaaabaaaaaaegbcbaaaaaaaaaaaaceaaaaaaaaaaaeaaaaaaaea
aaaaaaeaaaaaaaaaegiccaaaaaaaaaaaadaaaaaadhaaaaajhcaabaaaaaaaaaaa
pgapbaaaaaaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaaipcaabaaa
abaaaaaafgafbaaaaaaaaaaaegiocaaaabaaaaaaabaaaaaadcaaaaakpcaabaaa
abaaaaaaegiocaaaabaaaaaaaaaaaaaaagaabaaaaaaaaaaaegaobaaaabaaaaaa
dcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaacaaaaaakgakbaaaaaaaaaaa
egaobaaaabaaaaaaaaaaaaaipcaabaaaaaaaaaaaegaobaaaaaaaaaaaegiocaaa
abaaaaaaadaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaaaaaaaaadiaaaaaj
pcaabaaaabaaaaaangijcaaaabaaaaaaacaaaaaakgihcaaaabaaaaaaadaaaaaa
diaaaaajpcaabaaaacaaaaaajgiccaaaabaaaaaaaaaaaaaaggiicaaaabaaaaaa
acaaaaaadiaaaaaibcaabaaaadaaaaaabkaabaaaacaaaaaadkiacaaaabaaaaaa
adaaaaaadiaaaaajpcaabaaaaeaaaaaaggiicaaaabaaaaaaaaaaaaaajgiccaaa
abaaaaaaadaaaaaadcaaaaakbcaabaaaadaaaaaabkaabaaaaeaaaaaadkiacaaa
abaaaaaaacaaaaaaakaabaaaadaaaaaadcaaaaakbcaabaaaadaaaaaadkaabaaa
abaaaaaadkiacaaaabaaaaaaaaaaaaaaakaabaaaadaaaaaadcaaaaalbcaabaaa
adaaaaaaakaabaiaebaaaaaaacaaaaaadkiacaaaabaaaaaaadaaaaaaakaabaaa
adaaaaaadcaaaaalbcaabaaaadaaaaaaakaabaiaebaaaaaaabaaaaaadkiacaaa
abaaaaaaaaaaaaaaakaabaaaadaaaaaadcaaaaalbcaabaaaadaaaaaaakaabaia
ebaaaaaaaeaaaaaadkiacaaaabaaaaaaacaaaaaaakaabaaaadaaaaaadcaaaaam
ocaabaaaadaaaaaakgihcaaaabaaaaaaacaaaaaapgijcaaaabaaaaaaadaaaaaa
fgaobaiaebaaaaaaabaaaaaabaaaaaaibcaabaaaafaaaaaajgahbaaaadaaaaaa
jgihcaaaabaaaaaaabaaaaaadiaaaaajocaabaaaadaaaaaapgiecaaaabaaaaaa
acaaaaaafgidcaaaabaaaaaaadaaaaaadcaaaaamocaabaaaadaaaaaafgidcaaa
abaaaaaaacaaaaaapgiecaaaabaaaaaaadaaaaaafgaobaiaebaaaaaaadaaaaaa
baaaaaaiecaabaaaafaaaaaajgahbaaaadaaaaaaegidcaaaabaaaaaaabaaaaaa
diaaaaajocaabaaaadaaaaaapgiicaaaabaaaaaaacaaaaaakgidcaaaabaaaaaa
adaaaaaadcaaaaamhcaabaaaagaaaaaaogiicaiaebaaaaaaabaaaaaaacaaaaaa
dgiocaaaabaaaaaaadaaaaaajgahbaaaadaaaaaabaaaaaaiccaabaaaafaaaaaa
egacbaaaagaaaaaaigidcaaaabaaaaaaabaaaaaadiaaaaajhcaabaaaagaaaaaa
jgiecaaaabaaaaaaacaaaaaacgijcaaaabaaaaaaadaaaaaadcaaaaamlcaabaaa
agaaaaaacgigcaaaabaaaaaaacaaaaaajgibcaaaabaaaaaaadaaaaaaegaibaia
ebaaaaaaagaaaaaabaaaaaaiicaabaaaafaaaaaaegadbaaaagaaaaaaegiccaaa
abaaaaaaabaaaaaabbaaaaaiccaabaaaabaaaaaaegaobaaaafaaaaaaegiocaaa
abaaaaaaaaaaaaaaaoaaaaakccaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpbkaabaaaabaaaaaadiaaaaahbcaabaaaafaaaaaaakaabaaa
adaaaaaabkaabaaaabaaaaaadiaaaaajdcaabaaaadaaaaaaegiacaaaabaaaaaa
acaaaaaacgikcaaaabaaaaaaadaaaaaadiaaaaaiecaabaaaabaaaaaaakaabaaa
adaaaaaadkiacaaaabaaaaaaaaaaaaaadcaaaaakecaabaaaabaaaaaadkaabaaa
acaaaaaadkiacaaaabaaaaaaadaaaaaackaabaaaabaaaaaadcaaaaakecaabaaa
abaaaaaadkaabaaaaeaaaaaadkiacaaaabaaaaaaacaaaaaackaabaaaabaaaaaa
dcaaaaalecaabaaaabaaaaaackaabaiaebaaaaaaaeaaaaaadkiacaaaabaaaaaa
acaaaaaackaabaaaabaaaaaadcaaaaalecaabaaaabaaaaaackaabaiaebaaaaaa
acaaaaaadkiacaaaabaaaaaaadaaaaaackaabaaaabaaaaaadcaaaaalecaabaaa
abaaaaaadkaabaiaebaaaaaaadaaaaaadkiacaaaabaaaaaaaaaaaaaackaabaaa
abaaaaaadiaaaaahccaabaaaafaaaaaackaabaaaabaaaaaabkaabaaaabaaaaaa
diaaaaajpcaabaaaahaaaaaaegibcaaaabaaaaaaaaaaaaaabgiecaaaabaaaaaa
acaaaaaadiaaaaaidcaabaaaagaaaaaaogakbaaaahaaaaaalgipcaaaabaaaaaa
adaaaaaadcaaaaakecaabaaaabaaaaaackaabaaaagaaaaaackiacaaaabaaaaaa
aaaaaaaabkaabaaaagaaaaaadiaaaaajpcaabaaaaiaaaaaabgiecaaaabaaaaaa
aaaaaaaaegibcaaaabaaaaaaadaaaaaadcaaaaakecaabaaaabaaaaaadkaabaaa
aiaaaaaackiacaaaabaaaaaaacaaaaaackaabaaaabaaaaaadcaaaaalecaabaaa
abaaaaaackaabaiaebaaaaaaaiaaaaaackiacaaaabaaaaaaacaaaaaackaabaaa
abaaaaaadcaaaaalecaabaaaabaaaaaackaabaiaebaaaaaaahaaaaaackiacaaa
abaaaaaaadaaaaaackaabaaaabaaaaaadcaaaaalecaabaaaabaaaaaabkaabaia
ebaaaaaaadaaaaaackiacaaaabaaaaaaaaaaaaaackaabaaaabaaaaaadiaaaaah
icaabaaaafaaaaaackaabaaaabaaaaaabkaabaaaabaaaaaabaaaaaahcccabaaa
abaaaaaaegadbaaaafaaaaaaegadbaaaaaaaaaaadiaaaaajpcaabaaaajaaaaaa
ggiicaaaabaaaaaaabaaaaaajgiccaaaabaaaaaaacaaaaaadiaaaaaikcaabaaa
agaaaaaafganbaaaajaaaaaapgipcaaaabaaaaaaadaaaaaadcaaaaakbcaabaaa
abaaaaaaakaabaaaabaaaaaadkiacaaaabaaaaaaabaaaaaabkaabaaaagaaaaaa
diaaaaajpcaabaaaakaaaaaajgiccaaaabaaaaaaabaaaaaaggiicaaaabaaaaaa
adaaaaaadcaaaaakbcaabaaaabaaaaaabkaabaaaakaaaaaadkiacaaaabaaaaaa
acaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaaakaabaiaebaaaaaa
akaaaaaadkiacaaaabaaaaaaacaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaa
abaaaaaaakaabaiaebaaaaaaajaaaaaadkiacaaaabaaaaaaadaaaaaaakaabaaa
abaaaaaadcaaaaalbcaabaaaabaaaaaadkaabaiaebaaaaaaabaaaaaadkiacaaa
abaaaaaaabaaaaaaakaabaaaabaaaaaadiaaaaahbcaabaaaalaaaaaaakaabaaa
abaaaaaabkaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaadkaabaaaakaaaaaa
dkiacaaaabaaaaaaacaaaaaadkaabaaaagaaaaaadcaaaaakbcaabaaaabaaaaaa
dkaabaaaadaaaaaadkiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaadcaaaaal
bcaabaaaabaaaaaackaabaiaebaaaaaaajaaaaaadkiacaaaabaaaaaaadaaaaaa
akaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaaakaabaiaebaaaaaaadaaaaaa
dkiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaa
ckaabaiaebaaaaaaakaaaaaadkiacaaaabaaaaaaacaaaaaaakaabaaaabaaaaaa
diaaaaahccaabaaaalaaaaaaakaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaaj
pcaabaaaamaaaaaabgiecaaaabaaaaaaabaaaaaaegibcaaaabaaaaaaacaaaaaa
diaaaaaibcaabaaaabaaaaaadkaabaaaamaaaaaackiacaaaabaaaaaaadaaaaaa
diaaaaajpcaabaaaanaaaaaaegibcaaaabaaaaaaabaaaaaabgiecaaaabaaaaaa
adaaaaaadcaaaaakbcaabaaaabaaaaaadkaabaaaanaaaaaackiacaaaabaaaaaa
acaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaabkaabaaaadaaaaaa
ckiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaa
ckaabaiaebaaaaaaamaaaaaackiacaaaabaaaaaaadaaaaaaakaabaaaabaaaaaa
dcaaaaalbcaabaaaabaaaaaackaabaiaebaaaaaaagaaaaaackiacaaaabaaaaaa
abaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaackaabaiaebaaaaaa
anaaaaaackiacaaaabaaaaaaacaaaaaaakaabaaaabaaaaaadiaaaaahicaabaaa
alaaaaaaakaabaaaabaaaaaabkaabaaaabaaaaaabaaaaaahbccabaaaabaaaaaa
egadbaaaalaaaaaaegadbaaaaaaaaaaadiaaaaajpcaabaaaaoaaaaaaggiicaaa
abaaaaaaaaaaaaaajgiccaaaabaaaaaaabaaaaaadiaaaaaibcaabaaaabaaaaaa
akaabaaaaoaaaaaadkiacaaaabaaaaaaacaaaaaadcaaaaakbcaabaaaabaaaaaa
akaabaaaacaaaaaadkiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaadcaaaaak
bcaabaaaabaaaaaaakaabaaaajaaaaaadkiacaaaabaaaaaaaaaaaaaaakaabaaa
abaaaaaadcaaaaalbcaabaaaabaaaaaabkaabaiaebaaaaaaaoaaaaaadkiacaaa
abaaaaaaacaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaabkaabaia
ebaaaaaaajaaaaaadkiacaaaabaaaaaaaaaaaaaaakaabaaaabaaaaaadcaaaaal
bcaabaaaabaaaaaabkaabaiaebaaaaaaacaaaaaadkiacaaaabaaaaaaabaaaaaa
akaabaaaabaaaaaadiaaaaahbcaabaaaapaaaaaaakaabaaaabaaaaaabkaabaaa
abaaaaaadiaaaaaibcaabaaaabaaaaaackaabaaaajaaaaaadkiacaaaabaaaaaa
aaaaaaaadcaaaaakbcaabaaaabaaaaaackaabaaaaoaaaaaadkiacaaaabaaaaaa
acaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackaabaaaacaaaaaa
dkiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaa
dkaabaiaebaaaaaaacaaaaaadkiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaa
dcaaaaalbcaabaaaabaaaaaadkaabaiaebaaaaaaaoaaaaaadkiacaaaabaaaaaa
acaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaadkaabaiaebaaaaaa
ajaaaaaadkiacaaaabaaaaaaaaaaaaaaakaabaaaabaaaaaadiaaaaahccaabaaa
apaaaaaaakaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaaibcaabaaaabaaaaaa
ckaabaaaamaaaaaackiacaaaabaaaaaaaaaaaaaadiaaaaajpcaabaaaacaaaaaa
bgiecaaaabaaaaaaaaaaaaaaegibcaaaabaaaaaaabaaaaaadcaaaaakbcaabaaa
abaaaaaackaabaaaacaaaaaackiacaaaabaaaaaaacaaaaaaakaabaaaabaaaaaa
dcaaaaakbcaabaaaabaaaaaackaabaaaahaaaaaackiacaaaabaaaaaaabaaaaaa
akaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaadkaabaiaebaaaaaaahaaaaaa
ckiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaa
dkaabaiaebaaaaaaacaaaaaackiacaaaabaaaaaaacaaaaaaakaabaaaabaaaaaa
dcaaaaalbcaabaaaabaaaaaadkaabaiaebaaaaaaamaaaaaackiacaaaabaaaaaa
aaaaaaaaakaabaaaabaaaaaadiaaaaahicaabaaaapaaaaaaakaabaaaabaaaaaa
bkaabaaaabaaaaaabaaaaaahiccabaaaabaaaaaaegadbaaaapaaaaaaegadbaaa
aaaaaaaadiaaaaaibcaabaaaabaaaaaaakaabaaaakaaaaaadkiacaaaabaaaaaa
aaaaaaaadcaaaaakbcaabaaaabaaaaaabkaabaaaaoaaaaaadkiacaaaabaaaaaa
adaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaaakaabaaaaeaaaaaa
dkiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaa
bkaabaiaebaaaaaaaeaaaaaadkiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaa
dcaaaaalbcaabaaaabaaaaaaakaabaiaebaaaaaaaoaaaaaadkiacaaaabaaaaaa
adaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaabkaabaiaebaaaaaa
akaaaaaadkiacaaaabaaaaaaaaaaaaaaakaabaaaabaaaaaadiaaaaahbcaabaaa
ajaaaaaaakaabaaaabaaaaaabkaabaaaabaaaaaadiaaaaaibcaabaaaabaaaaaa
dkaabaaaaoaaaaaadkiacaaaabaaaaaaadaaaaaadcaaaaakbcaabaaaabaaaaaa
ckaabaaaaeaaaaaadkiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaadcaaaaak
bcaabaaaabaaaaaackaabaaaakaaaaaadkiacaaaabaaaaaaaaaaaaaaakaabaaa
abaaaaaadcaaaaalbcaabaaaabaaaaaackaabaiaebaaaaaaaoaaaaaadkiacaaa
abaaaaaaadaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaadkaabaia
ebaaaaaaakaaaaaadkiacaaaabaaaaaaaaaaaaaaakaabaaaabaaaaaadcaaaaal
bcaabaaaabaaaaaadkaabaiaebaaaaaaaeaaaaaadkiacaaaabaaaaaaabaaaaaa
akaabaaaabaaaaaadiaaaaahccaabaaaajaaaaaaakaabaaaabaaaaaabkaabaaa
abaaaaaadiaaaaaibcaabaaaabaaaaaadkaabaaaacaaaaaackiacaaaabaaaaaa
adaaaaaadcaaaaakbcaabaaaabaaaaaackaabaaaaiaaaaaackiacaaaabaaaaaa
abaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackaabaaaanaaaaaa
ckiacaaaabaaaaaaaaaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaa
ckaabaiaebaaaaaaacaaaaaackiacaaaabaaaaaaadaaaaaaakaabaaaabaaaaaa
dcaaaaalbcaabaaaabaaaaaadkaabaiaebaaaaaaanaaaaaackiacaaaabaaaaaa
aaaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaadkaabaiaebaaaaaa
aiaaaaaackiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaadiaaaaahicaabaaa
ajaaaaaaakaabaaaabaaaaaabkaabaaaabaaaaaabaaaaaaheccabaaaabaaaaaa
egadbaaaajaaaaaaegadbaaaaaaaaaaadiaaaaaibcaabaaaabaaaaaackaabaaa
agaaaaaadkiacaaaabaaaaaaabaaaaaadcaaaaakbcaabaaaabaaaaaackaabaaa
amaaaaaadkiacaaaabaaaaaaadaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaa
abaaaaaackaabaaaanaaaaaadkiacaaaabaaaaaaacaaaaaaakaabaaaabaaaaaa
dcaaaaalbcaabaaaabaaaaaadkaabaiaebaaaaaaanaaaaaadkiacaaaabaaaaaa
acaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaadkaabaiaebaaaaaa
amaaaaaadkiacaaaabaaaaaaadaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaa
abaaaaaabkaabaiaebaaaaaaadaaaaaadkiacaaaabaaaaaaabaaaaaaakaabaaa
abaaaaaadiaaaaahecaabaaaalaaaaaaakaabaaaabaaaaaabkaabaaaabaaaaaa
bbaaaaahbccabaaaacaaaaaaegaobaaaalaaaaaaegapbaaaaaaaaaaadcaaaaak
bcaabaaaabaaaaaackaabaaaaiaaaaaadkiacaaaabaaaaaaacaaaaaaakaabaaa
agaaaaaadcaaaaakbcaabaaaabaaaaaabkaabaaaadaaaaaadkiacaaaabaaaaaa
aaaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaadkaabaiaebaaaaaa
ahaaaaaadkiacaaaabaaaaaaadaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaa
abaaaaaackaabaiaebaaaaaaagaaaaaadkiacaaaabaaaaaaaaaaaaaaakaabaaa
abaaaaaadcaaaaalbcaabaaaabaaaaaadkaabaiaebaaaaaaaiaaaaaadkiacaaa
abaaaaaaacaaaaaaakaabaaaabaaaaaadiaaaaahecaabaaaafaaaaaaakaabaaa
abaaaaaabkaabaaaabaaaaaabbaaaaahcccabaaaacaaaaaaegaobaaaafaaaaaa
egapbaaaaaaaaaaadiaaaaaibcaabaaaabaaaaaaakaabaaaacaaaaaadkiacaaa
abaaaaaaacaaaaaadcaaaaakbcaabaaaabaaaaaaakaabaaaahaaaaaadkiacaaa
abaaaaaaabaaaaaaakaabaaaabaaaaaadcaaaaakbcaabaaaabaaaaaaakaabaaa
amaaaaaadkiacaaaabaaaaaaaaaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaa
abaaaaaabkaabaiaebaaaaaaacaaaaaadkiacaaaabaaaaaaacaaaaaaakaabaaa
abaaaaaadcaaaaalbcaabaaaabaaaaaabkaabaiaebaaaaaaamaaaaaadkiacaaa
abaaaaaaaaaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaabkaabaia
ebaaaaaaahaaaaaadkiacaaaabaaaaaaabaaaaaaakaabaaaabaaaaaadiaaaaah
ecaabaaaapaaaaaaakaabaaaabaaaaaabkaabaaaabaaaaaabbaaaaahiccabaaa
acaaaaaaegaobaaaapaaaaaaegapbaaaaaaaaaaadiaaaaaibcaabaaaabaaaaaa
akaabaaaanaaaaaadkiacaaaabaaaaaaaaaaaaaadcaaaaakbcaabaaaabaaaaaa
ckaabaaaacaaaaaadkiacaaaabaaaaaaadaaaaaaakaabaaaabaaaaaadcaaaaak
bcaabaaaabaaaaaaakaabaaaaiaaaaaadkiacaaaabaaaaaaabaaaaaaakaabaaa
abaaaaaadcaaaaalbcaabaaaabaaaaaabkaabaiaebaaaaaaaiaaaaaadkiacaaa
abaaaaaaabaaaaaaakaabaaaabaaaaaadcaaaaalbcaabaaaabaaaaaadkaabaia
ebaaaaaaacaaaaaadkiacaaaabaaaaaaadaaaaaaakaabaaaabaaaaaadcaaaaal
bcaabaaaabaaaaaabkaabaiaebaaaaaaanaaaaaadkiacaaaabaaaaaaaaaaaaaa
akaabaaaabaaaaaadiaaaaahecaabaaaajaaaaaaakaabaaaabaaaaaabkaabaaa
abaaaaaabbaaaaaheccabaaaacaaaaaaegaobaaaajaaaaaaegapbaaaaaaaaaaa
dgaaaaafpccabaaaahaaaaaaegaobaaaaaaaaaaadgaaaaagbccabaaaadaaaaaa
akiacaaaaaaaaaaaagaaaaaadgaaaaaigccabaaaadaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaadiaaaaajhcaabaaaaaaaaaaaegiccaaaaaaaaaaa
adaaaaaaegiccaaaaaaaaaaaagaaaaaadgaaaaagiccabaaaadaaaaaaakaabaia
ebaaaaaaaaaaaaaadgaaaaaifccabaaaaeaaaaaaaceaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaadgaaaaagcccabaaaaeaaaaaabkiacaaaaaaaaaaaagaaaaaa
dgaaaaagiccabaaaaeaaaaaabkaabaiaebaaaaaaaaaaaaaadgaaaaaidccabaaa
afaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadgaaaaageccabaaa
afaaaaaackiacaaaaaaaaaaaagaaaaaadgaaaaagiccabaaaafaaaaaackaabaia
ebaaaaaaaaaaaaaadiaaaaaiicaabaaaaaaaaaaaakaabaaaaaaaaaaaakiacaaa
aaaaaaaaadaaaaaadcaaaaamicaabaaaaaaaaaaaakiacaiaebaaaaaaaaaaaaaa
abaaaaaaakiacaaaaaaaaaaaabaaaaaadkaabaaaaaaaaaaadcaaaaakicaabaaa
aaaaaaaabkaabaaaaaaaaaaabkiacaaaaaaaaaaaadaaaaaadkaabaaaaaaaaaaa
dcaaaaakiccabaaaagaaaaaackaabaaaaaaaaaaackiacaaaaaaaaaaaadaaaaaa
dkaabaaaaaaaaaaadgaaaaaghccabaaaagaaaaaaegacbaiaebaaaaaaaaaaaaaa
doaaaaab"
}

SubProgram "gles3 " {
Keywords { }
"!!GLES3#version 300 es


#ifdef VERTEX

#define gl_Vertex _glesVertex
in vec4 _glesVertex;
float xll_determinant_mf2x2( mat2 m) {
    return m[0][0]*m[1][1] - m[0][1]*m[1][0];
}
float xll_determinant_mf3x3( mat3 m) {
    vec3 temp;
    temp.x = m[1][1]*m[2][2] - m[1][2]*m[2][1];
    temp.y = - (m[0][1]*m[2][2] - m[0][2]*m[2][1]);
    temp.z = m[0][1]*m[1][2] - m[0][2]*m[1][1];
    return dot( m[0], temp);
}
float xll_determinant_mf4x4( mat4 m) {
    vec4 temp;
    temp.x = xll_determinant_mf3x3( mat3( m[1].yzw, m[2].yzw, m[3].yzw));
    temp.y = -xll_determinant_mf3x3( mat3( m[0].yzw, m[2].yzw, m[3].yzw));
    temp.z = xll_determinant_mf3x3( mat3( m[0].yzw, m[1].yzw, m[3].yzw));
    temp.w = -xll_determinant_mf3x3( mat3( m[0].yzw, m[1].yzw, m[2].yzw));
    return dot( m[0], temp);
}
#line 150
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 186
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 180
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 383
struct Quadric {
    highp vec3 s1;
    highp vec3 s2;
};
#line 340
struct Ray {
    highp vec3 origin;
    highp vec3 direction;
};
#line 322
struct v2p {
    highp vec4 p;
    highp vec4 i_near;
    highp vec4 i_far;
    highp vec4 colonne1;
    highp vec4 colonne2;
    highp vec4 colonne3;
    highp vec4 colonne4;
    highp vec4 worldpos;
};
#line 317
struct appdata {
    highp vec4 vertex;
};
#line 334
struct fragment_out {
    highp vec4 Color;
    highp float depth;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_LightPosition[4];
uniform highp vec4 unity_LightAtten[4];
#line 19
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_SHBr;
#line 23
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
#line 27
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
uniform highp vec4 _LightSplitsNear;
#line 31
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
uniform highp vec4 unity_ShadowFadeCenterAndType;
#line 35
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
uniform highp mat4 _Object2World;
#line 39
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
uniform highp mat4 glstate_matrix_texture0;
#line 43
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
uniform highp mat4 glstate_matrix_projection;
#line 47
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
uniform lowp vec4 unity_ColorSpaceGrey;
#line 76
#line 81
#line 86
#line 90
#line 95
#line 119
#line 136
#line 157
#line 165
#line 192
#line 205
#line 214
#line 219
#line 228
#line 233
#line 242
#line 259
#line 264
#line 290
#line 298
#line 302
#line 306
uniform highp float _Rayon;
uniform highp vec4 _Color;
uniform highp vec4 _TexPos;
uniform highp float _Visibilite;
#line 310
uniform highp vec4 _Light;
uniform highp vec4 _Equation;
uniform sampler2D _MatCap;
uniform highp float _Cut;
#line 314
uniform highp vec4 _Cutplane;
uniform highp int _Attenuation;
uniform highp float _Brightness;
#line 346
#line 350
#line 354
#line 389
#line 415
#line 424
#line 354
highp mat4 mat_inverse( in highp mat4 my_mat ) {
    highp mat4 inv;
    highp float det = xll_determinant_mf4x4(my_mat);
    #line 358
    highp float invdet = (1.0 / det);
    inv[0][0] = (invdet * (((((((my_mat[1][1] * my_mat[2][2]) * my_mat[3][3]) + ((my_mat[2][1] * my_mat[3][2]) * my_mat[1][3])) + ((my_mat[3][1] * my_mat[1][2]) * my_mat[2][3])) - ((my_mat[1][1] * my_mat[3][2]) * my_mat[2][3])) - ((my_mat[2][1] * my_mat[1][2]) * my_mat[3][3])) - ((my_mat[3][1] * my_mat[2][2]) * my_mat[1][3])));
    inv[1][0] = (invdet * (((((((my_mat[1][0] * my_mat[3][2]) * my_mat[2][3]) + ((my_mat[2][0] * my_mat[1][2]) * my_mat[3][3])) + ((my_mat[3][0] * my_mat[2][2]) * my_mat[1][3])) - ((my_mat[1][0] * my_mat[2][2]) * my_mat[3][3])) - ((my_mat[2][0] * my_mat[3][2]) * my_mat[1][3])) - ((my_mat[3][0] * my_mat[1][2]) * my_mat[2][3])));
    inv[2][0] = (invdet * (((((((my_mat[1][0] * my_mat[2][1]) * my_mat[3][3]) + ((my_mat[2][0] * my_mat[3][1]) * my_mat[1][3])) + ((my_mat[3][0] * my_mat[1][1]) * my_mat[2][3])) - ((my_mat[1][0] * my_mat[3][1]) * my_mat[2][3])) - ((my_mat[2][0] * my_mat[1][1]) * my_mat[3][3])) - ((my_mat[3][0] * my_mat[2][1]) * my_mat[1][3])));
    #line 362
    inv[3][0] = (invdet * (((((((my_mat[1][0] * my_mat[3][1]) * my_mat[2][2]) + ((my_mat[2][0] * my_mat[1][1]) * my_mat[3][2])) + ((my_mat[3][0] * my_mat[2][1]) * my_mat[1][2])) - ((my_mat[1][0] * my_mat[2][1]) * my_mat[3][2])) - ((my_mat[2][0] * my_mat[3][1]) * my_mat[1][2])) - ((my_mat[3][0] * my_mat[1][1]) * my_mat[2][2])));
    inv[0][1] = (invdet * (((((((my_mat[0][1] * my_mat[3][2]) * my_mat[2][3]) + ((my_mat[2][1] * my_mat[0][2]) * my_mat[3][3])) + ((my_mat[3][1] * my_mat[2][2]) * my_mat[0][3])) - ((my_mat[0][1] * my_mat[2][2]) * my_mat[3][3])) - ((my_mat[2][1] * my_mat[3][2]) * my_mat[0][3])) - ((my_mat[3][1] * my_mat[0][2]) * my_mat[2][3])));
    inv[1][1] = (invdet * (((((((my_mat[0][0] * my_mat[2][2]) * my_mat[3][3]) + ((my_mat[2][0] * my_mat[3][2]) * my_mat[0][3])) + ((my_mat[3][0] * my_mat[0][2]) * my_mat[2][3])) - ((my_mat[0][0] * my_mat[3][2]) * my_mat[2][3])) - ((my_mat[2][0] * my_mat[0][2]) * my_mat[3][3])) - ((my_mat[3][0] * my_mat[2][2]) * my_mat[0][3])));
    inv[2][1] = (invdet * (((((((my_mat[0][0] * my_mat[3][1]) * my_mat[2][3]) + ((my_mat[2][0] * my_mat[0][1]) * my_mat[3][3])) + ((my_mat[3][0] * my_mat[2][1]) * my_mat[0][3])) - ((my_mat[0][0] * my_mat[2][1]) * my_mat[3][3])) - ((my_mat[2][0] * my_mat[3][1]) * my_mat[0][3])) - ((my_mat[3][0] * my_mat[0][1]) * my_mat[2][3])));
    #line 366
    inv[3][1] = (invdet * (((((((my_mat[0][0] * my_mat[2][1]) * my_mat[3][2]) + ((my_mat[2][0] * my_mat[3][1]) * my_mat[0][2])) + ((my_mat[3][0] * my_mat[0][1]) * my_mat[2][2])) - ((my_mat[0][0] * my_mat[3][1]) * my_mat[2][2])) - ((my_mat[2][0] * my_mat[0][1]) * my_mat[3][2])) - ((my_mat[3][0] * my_mat[2][1]) * my_mat[0][2])));
    inv[0][2] = (invdet * (((((((my_mat[0][1] * my_mat[1][2]) * my_mat[3][3]) + ((my_mat[1][1] * my_mat[3][2]) * my_mat[0][3])) + ((my_mat[3][1] * my_mat[0][2]) * my_mat[1][3])) - ((my_mat[0][1] * my_mat[3][2]) * my_mat[1][3])) - ((my_mat[1][1] * my_mat[0][2]) * my_mat[3][3])) - ((my_mat[3][1] * my_mat[1][2]) * my_mat[0][3])));
    inv[1][2] = (invdet * (((((((my_mat[0][0] * my_mat[3][2]) * my_mat[1][3]) + ((my_mat[1][0] * my_mat[0][2]) * my_mat[3][3])) + ((my_mat[3][0] * my_mat[1][2]) * my_mat[0][3])) - ((my_mat[0][0] * my_mat[1][2]) * my_mat[3][3])) - ((my_mat[1][0] * my_mat[3][2]) * my_mat[0][3])) - ((my_mat[3][0] * my_mat[0][2]) * my_mat[1][3])));
    inv[2][2] = (invdet * (((((((my_mat[0][0] * my_mat[1][1]) * my_mat[3][3]) + ((my_mat[1][0] * my_mat[3][1]) * my_mat[0][3])) + ((my_mat[3][0] * my_mat[0][1]) * my_mat[1][3])) - ((my_mat[0][0] * my_mat[3][1]) * my_mat[1][3])) - ((my_mat[1][0] * my_mat[0][1]) * my_mat[3][3])) - ((my_mat[3][0] * my_mat[1][1]) * my_mat[0][3])));
    #line 370
    inv[3][2] = (invdet * (((((((my_mat[0][0] * my_mat[3][1]) * my_mat[1][2]) + ((my_mat[1][0] * my_mat[0][1]) * my_mat[3][2])) + ((my_mat[3][0] * my_mat[1][1]) * my_mat[0][2])) - ((my_mat[0][0] * my_mat[1][1]) * my_mat[3][2])) - ((my_mat[1][0] * my_mat[3][1]) * my_mat[0][2])) - ((my_mat[3][0] * my_mat[0][1]) * my_mat[1][2])));
    inv[0][3] = (invdet * (((((((my_mat[0][1] * my_mat[2][2]) * my_mat[1][3]) + ((my_mat[1][1] * my_mat[0][2]) * my_mat[2][3])) + ((my_mat[2][1] * my_mat[1][2]) * my_mat[0][3])) - ((my_mat[0][1] * my_mat[1][2]) * my_mat[2][3])) - ((my_mat[1][1] * my_mat[2][2]) * my_mat[0][3])) - ((my_mat[2][1] * my_mat[0][2]) * my_mat[1][3])));
    inv[1][3] = (invdet * (((((((my_mat[0][0] * my_mat[1][2]) * my_mat[2][3]) + ((my_mat[1][0] * my_mat[2][2]) * my_mat[0][3])) + ((my_mat[2][0] * my_mat[0][2]) * my_mat[1][3])) - ((my_mat[0][0] * my_mat[2][2]) * my_mat[1][3])) - ((my_mat[1][0] * my_mat[0][2]) * my_mat[2][3])) - ((my_mat[2][0] * my_mat[1][2]) * my_mat[0][3])));
    inv[2][3] = (invdet * (((((((my_mat[0][0] * my_mat[2][1]) * my_mat[1][3]) + ((my_mat[1][0] * my_mat[0][1]) * my_mat[2][3])) + ((my_mat[2][0] * my_mat[1][1]) * my_mat[0][3])) - ((my_mat[0][0] * my_mat[1][1]) * my_mat[2][3])) - ((my_mat[1][0] * my_mat[2][1]) * my_mat[0][3])) - ((my_mat[2][0] * my_mat[0][1]) * my_mat[1][3])));
    #line 374
    inv[3][3] = (invdet * (((((((my_mat[0][0] * my_mat[1][1]) * my_mat[2][2]) + ((my_mat[1][0] * my_mat[2][1]) * my_mat[0][2])) + ((my_mat[2][0] * my_mat[0][1]) * my_mat[1][2])) - ((my_mat[0][0] * my_mat[2][1]) * my_mat[1][2])) - ((my_mat[1][0] * my_mat[0][1]) * my_mat[2][2])) - ((my_mat[2][0] * my_mat[1][1]) * my_mat[0][2])));
    return inv;
}
#line 430
v2p ballimproved_v( in appdata v ) {
    #line 432
    highp mat4 ModelViewProj = glstate_matrix_mvp;
    highp mat4 ModelViewProjI = mat_inverse( ModelViewProj);
    v2p o;
    highp vec4 spaceposition;
    #line 436
    spaceposition.xyz = _TexPos.xyz;
    spaceposition.w = 1.0;
    if ((_Rayon < 1.0)){
        spaceposition.xyz += (v.vertex.xyz * 2.0);
    }
    else{
        spaceposition.xyz += (v.vertex.xyz * (2.0 * _Rayon));
    }
    #line 440
    o.p = (ModelViewProj * spaceposition);
    v.vertex = o.p;
    o.worldpos = o.p;
    highp vec4 near = o.p;
    #line 444
    near.z = 0.0;
    near = (ModelViewProjI * near);
    highp vec4 far = o.p;
    far.z = far.w;
    #line 448
    o.i_far = (ModelViewProjI * far);
    o.i_near = near;
    highp vec4 equation1 = vec4( _Equation.xyz, _Rayon);
    highp vec4 eq1TexPos = (equation1 * _TexPos);
    #line 452
    highp vec4 eq1TexSq = (eq1TexPos * _TexPos);
    o.colonne1 = vec4( equation1.x, 0.0, 0.0, (-eq1TexPos.x));
    o.colonne2 = vec4( 0.0, equation1.y, 0.0, (-eq1TexPos.y));
    o.colonne3 = vec4( 0.0, 0.0, equation1.z, (-eq1TexPos.z));
    #line 456
    o.colonne4 = vec4( (-eq1TexPos.x), (-eq1TexPos.y), (-eq1TexPos.z), (((((-equation1.w) * equation1.w) + eq1TexSq.x) + eq1TexSq.y) + eq1TexSq.z));
    return o;
}
out highp vec4 xlv_TEXCOORD1;
out highp vec4 xlv_TEXCOORD2;
out highp vec4 xlv_TEXCOORD6;
out highp vec4 xlv_TEXCOORD7;
out highp vec4 xlv_COLOR0;
out highp vec4 xlv_TEXCOORD3;
out highp vec4 xlv_TEXCOORD4;
void main() {
    v2p xl_retval;
    appdata xlt_v;
    xlt_v.vertex = vec4(gl_Vertex);
    xl_retval = ballimproved_v( xlt_v);
    gl_Position = vec4(xl_retval.p);
    xlv_TEXCOORD1 = vec4(xl_retval.i_near);
    xlv_TEXCOORD2 = vec4(xl_retval.i_far);
    xlv_TEXCOORD6 = vec4(xl_retval.colonne1);
    xlv_TEXCOORD7 = vec4(xl_retval.colonne2);
    xlv_COLOR0 = vec4(xl_retval.colonne3);
    xlv_TEXCOORD3 = vec4(xl_retval.colonne4);
    xlv_TEXCOORD4 = vec4(xl_retval.worldpos);
}


#endif
#ifdef FRAGMENT

#define gl_FragData _glesFragData
layout(location = 0) out mediump vec4 _glesFragData[4];
void xll_clip_f(float x) {
  if ( x<0.0 ) discard;
}
mat2 xll_transpose_mf2x2(mat2 m) {
  return mat2( m[0][0], m[1][0], m[0][1], m[1][1]);
}
mat3 xll_transpose_mf3x3(mat3 m) {
  return mat3( m[0][0], m[1][0], m[2][0],
               m[0][1], m[1][1], m[2][1],
               m[0][2], m[1][2], m[2][2]);
}
mat4 xll_transpose_mf4x4(mat4 m) {
  return mat4( m[0][0], m[1][0], m[2][0], m[3][0],
               m[0][1], m[1][1], m[2][1], m[3][1],
               m[0][2], m[1][2], m[2][2], m[3][2],
               m[0][3], m[1][3], m[2][3], m[3][3]);
}
#line 150
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 186
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 180
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 383
struct Quadric {
    highp vec3 s1;
    highp vec3 s2;
};
#line 340
struct Ray {
    highp vec3 origin;
    highp vec3 direction;
};
#line 322
struct v2p {
    highp vec4 p;
    highp vec4 i_near;
    highp vec4 i_far;
    highp vec4 colonne1;
    highp vec4 colonne2;
    highp vec4 colonne3;
    highp vec4 colonne4;
    highp vec4 worldpos;
};
#line 317
struct appdata {
    highp vec4 vertex;
};
#line 334
struct fragment_out {
    highp vec4 Color;
    highp float depth;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[4];
uniform highp vec4 unity_LightPosition[4];
uniform highp vec4 unity_LightAtten[4];
#line 19
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
uniform highp vec4 unity_SHBr;
#line 23
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
#line 27
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
uniform highp vec4 _LightSplitsNear;
#line 31
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
uniform highp vec4 unity_ShadowFadeCenterAndType;
#line 35
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
uniform highp mat4 _Object2World;
#line 39
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
uniform highp mat4 glstate_matrix_texture0;
#line 43
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
uniform highp mat4 glstate_matrix_projection;
#line 47
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
uniform lowp vec4 unity_ColorSpaceGrey;
#line 76
#line 81
#line 86
#line 90
#line 95
#line 119
#line 136
#line 157
#line 165
#line 192
#line 205
#line 214
#line 219
#line 228
#line 233
#line 242
#line 259
#line 264
#line 290
#line 298
#line 302
#line 306
uniform highp float _Rayon;
uniform highp vec4 _Color;
uniform highp vec4 _TexPos;
uniform highp float _Visibilite;
#line 310
uniform highp vec4 _Light;
uniform highp vec4 _Equation;
uniform sampler2D _MatCap;
uniform highp float _Cut;
#line 314
uniform highp vec4 _Cutplane;
uniform highp int _Attenuation;
uniform highp float _Brightness;
#line 346
#line 350
#line 354
#line 389
#line 415
#line 424
#line 377
highp float distance_attenuation( in highp int attenuate, in highp vec4 eye, in highp vec4 pos ) {
    #line 379
    highp float dist = distance( pos, eye);
    highp float attenuation = (1.0 / (1e-05 + (0.0001 * dist)));
    return min( attenuation, 1.0);
}
#line 389
Quadric isect_surf( in Ray r, in highp mat4 matrix_coef ) {
    Quadric q;
    highp vec4 direction = vec4( r.direction, 0.0);
    #line 393
    highp vec4 origin = vec4( r.origin, 1.0);
    highp vec4 mcoef_dir = (matrix_coef * direction);
    highp float a = dot( direction, mcoef_dir);
    highp float b = dot( origin, mcoef_dir);
    #line 397
    highp float c = dot( origin, (matrix_coef * origin));
    highp float delta = ((b * b) - (a * c));
    if ((delta < 0.0)){
        xll_clip_f(-1.0);
    }
    highp float sqDelta = sqrt(delta);
    #line 401
    highp float t1 = (((-b) - sqDelta) / a);
    highp float t2 = (((-b) + sqDelta) / a);
    highp float t = (( (t1 < t2) ) ? ( t1 ) : ( t2 ));
    q.s1 = (r.origin + (t * r.direction));
    #line 405
    q.s2 = q.s1;
    return q;
}
#line 415
Ray primary_ray( in highp vec4 near1, in highp vec4 far1 ) {
    highp vec3 near = (near1.xyz / near1.w);
    highp vec3 far = (far1.xyz / far1.w);
    #line 419
    Ray ray;
    ray.origin = near;
    ray.direction = (far - near);
    return ray;
}
#line 424
highp float update_z_buffer( in highp vec3 M, in highp mat4 ModelViewP ) {
    highp vec4 Ms = (ModelViewP * vec4( M, 1.0));
    highp float depth1 = (0.5 + (Ms.z / (2.0 * Ms.w)));
    #line 428
    return depth1;
}
#line 459
fragment_out ballimproved_p( in v2p i ) {
    #line 461
    highp vec3 light = _Light.xyz;
    highp mat4 ModelViewProj = glstate_matrix_mvp;
    highp mat4 ModelViewIT = glstate_matrix_invtrans_modelview0;
    highp vec4 pcolor2;
    #line 465
    fragment_out OUT;
    highp mat4 mat = xll_transpose_mf4x4(mat4( i.colonne1, i.colonne2, i.colonne3, i.colonne4));
    Ray ray = primary_ray( i.i_near, i.i_far);
    Quadric q = isect_surf( ray, mat);
    #line 469
    highp vec3 M = q.s1;
    OUT.depth = update_z_buffer( M, ModelViewProj);
    highp vec4 M1 = vec4( M, 1.0);
    highp vec4 M2 = (mat * M1);
    #line 473
    highp vec3 normal = normalize((ModelViewIT * M2).xyz);
    highp vec3 lightvec = vec3( 0.0, 0.0, 1.0);
    highp vec3 eyepos = vec3( 0.0, 0.0, 1.0);
    highp float diffuse = dot( normal, lightvec);
    #line 477
    highp vec3 diffusecolor;
    diffusecolor = _Color.xyz;
    mediump vec2 vn = normal.xy;
    highp vec4 matcapLookup = texture( _MatCap, ((vn * 0.5) + 0.5));
    #line 481
    OUT.Color.xyz = (diffuse * diffusecolor);
    OUT.Color.w = _Color.w;
    OUT.Color = (((OUT.Color * matcapLookup) * 1.25) * _Brightness);
    if ((_Attenuation == 1)){
        #line 486
        highp float attenuation = distance_attenuation( 1, i.i_near, i.i_far);
        OUT.Color = (OUT.Color * attenuation);
    }
    return OUT;
}
in highp vec4 xlv_TEXCOORD1;
in highp vec4 xlv_TEXCOORD2;
in highp vec4 xlv_TEXCOORD6;
in highp vec4 xlv_TEXCOORD7;
in highp vec4 xlv_COLOR0;
in highp vec4 xlv_TEXCOORD3;
in highp vec4 xlv_TEXCOORD4;
void main() {
    fragment_out xl_retval;
    v2p xlt_i;
    xlt_i.p = vec4(0.0);
    xlt_i.i_near = vec4(xlv_TEXCOORD1);
    xlt_i.i_far = vec4(xlv_TEXCOORD2);
    xlt_i.colonne1 = vec4(xlv_TEXCOORD6);
    xlt_i.colonne2 = vec4(xlv_TEXCOORD7);
    xlt_i.colonne3 = vec4(xlv_COLOR0);
    xlt_i.colonne4 = vec4(xlv_TEXCOORD3);
    xlt_i.worldpos = vec4(xlv_TEXCOORD4);
    xl_retval = ballimproved_p( xlt_i);
    gl_FragData[0] = vec4(xl_retval.Color);
    gl_FragDepth = float(xl_retval.depth);
}


#endif"
}

}
Program "fp" {
// Fragment combos: 1
//   opengl - ALU: 68 to 68, TEX: 1 to 1
//   d3d9 - ALU: 66 to 66, TEX: 2 to 2
//   d3d11 - ALU: 47 to 47, TEX: 1 to 1, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
Vector 8 [_Color]
Float 9 [_Attenuation]
Float 10 [_Brightness]
SetTexture 0 [_MatCap] 2D
"3.0-!!ARBfp1.0
# 68 ALU, 1 TEX
PARAM c[13] = { state.matrix.mvp,
		state.matrix.modelview[0].invtrans,
		program.local[8..10],
		{ 1, 0, 2, 0.5 },
		{ 1.25, 9.9999997e-05, 9.9999997e-06 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
RCP R0.x, fragment.texcoord[1].w;
MUL R3.xyz, fragment.texcoord[1], R0.x;
MOV R3.w, c[11].x;
RCP R0.x, fragment.texcoord[2].w;
MAD R2.xyz, fragment.texcoord[2], R0.x, -R3;
MOV R2.w, c[11].y;
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
MUL R4.x, R0, R0.y;
MUL R3.w, R4.y, R4.y;
ADD R0.y, R3.w, -R4.x;
RSQ R0.y, R0.y;
RCP R0.y, R0.y;
ADD R0.z, -R4.y, -R0.y;
MOV R0.w, c[11].x;
RCP R0.x, R0.x;
ADD R0.y, -R4, R0;
MUL R0.y, R0.x, R0;
MUL R0.x, R0.z, R0;
ADD R0.z, R0.x, -R0.y;
CMP R0.x, R0.z, R0, R0.y;
MAD R0.xyz, R0.x, R2, R3;
DP4 R1.w, R0, fragment.texcoord[3];
DP4 R1.z, R0, fragment.color.primary;
DP4 R1.x, R0, fragment.texcoord[6];
DP4 R1.y, R0, fragment.texcoord[7];
DP4 R2.z, R1, c[6];
DP4 R2.x, R1, c[4];
DP4 R2.y, R1, c[5];
DP3 R1.x, R2, R2;
RSQ R2.w, R1.x;
MUL R3.xyz, R2.w, R2;
MOV R1, fragment.texcoord[2];
ADD R1, fragment.texcoord[1], -R1;
DP4 R1.z, R1, R1;
MUL R2.xyz, R3.z, c[8];
MAD R1.xy, R3, c[11].w, c[11].w;
RSQ R2.w, R1.z;
RCP R3.x, R2.w;
MOV R2.w, c[8];
TEX R1, R1, texture[0], 2D;
MUL R1, R2, R1;
MAD R2.x, R3, c[12].y, c[12].z;
MOV R3.x, c[11];
ADD R3.y, -R3.x, c[9].x;
DP4 R3.x, R0, c[3];
DP4 R0.x, R0, c[2];
MUL R1, R1, c[10].x;
RCP R2.x, R2.x;
SLT R0.z, R3.w, R4.x;
MUL R1, R1, c[12].x;
MIN R2.x, R2, c[11];
MUL R2, R1, R2.x;
ABS R3.y, R3;
CMP result.color, -R3.y, R1, R2;
MUL R1.x, R3, c[11].z;
RCP R0.y, R1.x;
MAD result.depth.z, R0.x, R0.y, c[11].w; // hack for D3D: depth may need to be unmasked
KIL -R0.z;
END
# 68 instructions, 5 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Matrix 0 [glstate_matrix_mvp]
Matrix 4 [glstate_matrix_invtrans_modelview0]
Vector 8 [_Color]
Float 9 [_Attenuation]
Float 10 [_Brightness]
SetTexture 0 [_MatCap] 2D
"ps_3_0
; 66 ALU, 2 TEX
dcl_2d s0
def c11, 1.00000000, 0.00000000, -1.00000000, 0.50000000
def c12, 1.25000000, 0.00010000, 0.00001000, 2.00000000
dcl_texcoord1 v0
dcl_texcoord2 v1
dcl_texcoord6 v2
dcl_texcoord7 v3
dcl_color0 v4
dcl_texcoord3 v5
rcp r0.x, v0.w
mul r1.xyz, v0, r0.x
mov r1.w, c11.x
dp4 r0.w, r1, v5
dp4 r0.z, r1, v4
dp4 r0.x, r1, v2
dp4 r0.y, r1, v3
dp4 r3.x, r1, r0
rcp r0.x, v1.w
mad r0.xyz, v1, r0.x, -r1
mov r0.w, c11.y
dp4 r2.w, v5, r0
dp4 r2.z, r0, v4
dp4 r2.x, r0, v2
dp4 r2.y, r0, v3
dp4 r0.w, r2, r0
dp4 r1.w, r1, r2
mul r3.x, r0.w, r3
mad r3.w, r1, r1, -r3.x
rsq r2.x, r3.w
rcp r2.x, r2.x
add r2.y, -r1.w, -r2.x
mov r2.w, c11.x
add r1.w, -r1, r2.x
rcp r0.w, r0.w
mul r1.w, r0, r1
mul r0.w, r2.y, r0
add r2.x, r0.w, -r1.w
cmp r0.w, r2.x, r1, r0
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
mov r0, v1
add r0, v0, -r0
mul r1.xyz, r3.z, c8
dp4 r1.w, r0, r0
mad_pp r3.xy, r3, c11.w, c11.w
texld r0, r3, s0
rsq r3.x, r1.w
mov r1.w, c8
mul r0, r1, r0
rcp r1.x, r3.x
mad r1.x, r1, c12.y, c12.z
rcp r1.y, r1.x
mov r1.x, c9
add r3.x, c11.z, r1
mul r0, r0, c10.x
mul r0, r0, c12.x
min r1.y, r1, c11.x
mul r1, r0, r1.y
abs r3.x, r3
cmp oC0, -r3.x, r1, r0
dp4 r0.x, r2, c3
mul r0.y, r0.x, c12.w
rcp r1.y, r0.y
dp4 r1.x, r2, c2
cmp r0.x, r3.w, c11.y, c11
mov_pp r0, -r0.x
mad oDepth, r1.x, r1.y, c11.w
texkill r0.xyzw
"
}

SubProgram "d3d11 " {
Keywords { }
ConstBuffer "$Globals" 160 // 152 used size, 11 vars
Vector 32 [_Color] 4
// unknown _Attenuation Class=0 Type=2 Cols=1 Rows=1 Size=4
Float 148 [_Brightness]
ConstBuffer "UnityPerDraw" 336 // 192 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
Matrix 128 [glstate_matrix_invtrans_modelview0] 4
BindCB "$Globals" 0
BindCB "UnityPerDraw" 1
SetTexture 0 [_MatCap] 2D 0
// 63 instructions, 4 temp regs, 0 temp arrays:
// ALU 46 float, 1 int, 0 uint
// TEX 1 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"ps_4_0
eefieceddconoknclpollgmklgobjogfbpblpmikabaaaaaaieajaaaaadaaaaaa
cmaaaaaabiabaaaagmabaaaaejfdeheooeaaaaaaaiaaaaaaaiaaaaaamiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaneaaaaaaabaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaaneaaaaaaacaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apapaaaaneaaaaaaagaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaaneaaaaaa
ahaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapapaaaannaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaafaaaaaaapapaaaaneaaaaaaadaaaaaaaaaaaaaaadaaaaaaagaaaaaa
apapaaaaneaaaaaaaeaaaaaaaaaaaaaaadaaaaaaahaaaaaaapaaaaaafdfgfpfa
gphdgjhegjgpgoaafeeffiedepepfceeaaedepemepfcaaklepfdeheoemaaaaaa
acaaaaaaaiaaaaaadiaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaa
ecaaaaaaaaaaaaaaaaaaaaaaadaaaaaappppppppabaoaaaafdfgfpfegbhcghgf
heaafdfgfpeegfhahegiaaklfdeieefcbaaiaaaaeaaaaaaaaeacaaaafjaaaaae
egiocaaaaaaaaaaaakaaaaaafjaaaaaeegiocaaaabaaaaaaamaaaaaafkaaaaad
aagabaaaaaaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaagcbaaaadpcbabaaa
abaaaaaagcbaaaadpcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaad
pcbabaaaaeaaaaaagcbaaaadpcbabaaaafaaaaaagcbaaaadpcbabaaaagaaaaaa
gfaaaaadpccabaaaaaaaaaaagfaaaaacabmaaaaagiaaaaacaeaaaaaadgaaaaaf
icaabaaaaaaaaaaaabeaaaaaaaaaiadpaoaaaaahhcaabaaaaaaaaaaaegbcbaaa
abaaaaaapgbpbaaaabaaaaaabbaaaaahbcaabaaaabaaaaaaegbobaaaadaaaaaa
egaobaaaaaaaaaaabbaaaaahccaabaaaabaaaaaaegbobaaaaeaaaaaaegaobaaa
aaaaaaaabbaaaaahecaabaaaabaaaaaaegbobaaaafaaaaaaegaobaaaaaaaaaaa
bbaaaaahicaabaaaabaaaaaaegbobaaaagaaaaaaegaobaaaaaaaaaaabbaaaaah
bcaabaaaabaaaaaaegaobaaaaaaaaaaaegaobaaaabaaaaaaaoaaaaahocaabaaa
abaaaaaaagbjbaaaacaaaaaapgbpbaaaacaaaaaaaaaaaaaiocaabaaaabaaaaaa
agajbaiaebaaaaaaaaaaaaaafgaobaaaabaaaaaabaaaaaahbcaabaaaacaaaaaa
egbcbaaaadaaaaaajgahbaaaabaaaaaabaaaaaahccaabaaaacaaaaaaegbcbaaa
aeaaaaaajgahbaaaabaaaaaabaaaaaahecaabaaaacaaaaaaegbcbaaaafaaaaaa
jgahbaaaabaaaaaabaaaaaahbcaabaaaadaaaaaajgahbaaaabaaaaaaegacbaaa
acaaaaaadiaaaaahbcaabaaaabaaaaaaakaabaaaabaaaaaaakaabaaaadaaaaaa
baaaaaahicaabaaaacaaaaaaegbcbaaaagaaaaaajgahbaaaabaaaaaabbaaaaah
icaabaaaaaaaaaaaegaobaaaaaaaaaaaegaobaaaacaaaaaadcaaaaakbcaabaaa
abaaaaaadkaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaa
dbaaaaahbcaabaaaacaaaaaaakaabaaaabaaaaaaabeaaaaaaaaaaaaaelaaaaaf
bcaabaaaabaaaaaaakaabaaaabaaaaaaanaaaeadakaabaaaacaaaaaaaaaaaaaj
bcaabaaaacaaaaaadkaabaiaebaaaaaaaaaaaaaaakaabaiaebaaaaaaabaaaaaa
aaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaaaaaaaaaakaabaaaabaaaaaa
aoaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaakaabaaaadaaaaaaaoaaaaah
bcaabaaaabaaaaaaakaabaaaacaaaaaaakaabaaaadaaaaaadbaaaaahbcaabaaa
acaaaaaaakaabaaaabaaaaaadkaabaaaaaaaaaaadhaaaaajicaabaaaaaaaaaaa
akaabaaaacaaaaaaakaabaaaabaaaaaadkaabaaaaaaaaaaadcaaaaajhcaabaaa
aaaaaaaapgapbaaaaaaaaaaajgahbaaaabaaaaaaegacbaaaaaaaaaaadgaaaaaf
icaabaaaaaaaaaaaabeaaaaaaaaaiadpbbaaaaahbcaabaaaabaaaaaaegbobaaa
aeaaaaaaegaobaaaaaaaaaaadiaaaaaihcaabaaaabaaaaaaagaabaaaabaaaaaa
egiccaaaabaaaaaaajaaaaaabbaaaaahicaabaaaabaaaaaaegbobaaaadaaaaaa
egaobaaaaaaaaaaadcaaaaakhcaabaaaabaaaaaaegiccaaaabaaaaaaaiaaaaaa
pgapbaaaabaaaaaaegacbaaaabaaaaaabbaaaaahicaabaaaabaaaaaaegbobaaa
afaaaaaaegaobaaaaaaaaaaabbaaaaahicaabaaaaaaaaaaaegbobaaaagaaaaaa
egaobaaaaaaaaaaadcaaaaakhcaabaaaabaaaaaaegiccaaaabaaaaaaakaaaaaa
pgapbaaaabaaaaaaegacbaaaabaaaaaadcaaaaakhcaabaaaabaaaaaaegiccaaa
abaaaaaaalaaaaaapgapbaaaaaaaaaaaegacbaaaabaaaaaabaaaaaahicaabaaa
aaaaaaaaegacbaaaabaaaaaaegacbaaaabaaaaaaeeaaaaaficaabaaaaaaaaaaa
dkaabaaaaaaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaadiaaaaaihcaabaaaacaaaaaakgakbaaaabaaaaaaegiccaaaaaaaaaaa
acaaaaaadcaaaaapdcaabaaaabaaaaaaegaabaaaabaaaaaaaceaaaaaaaaaaadp
aaaaaadpaaaaaaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaaaaaaaaaaaa
efaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaadiaaaaahhcaabaaaacaaaaaaegacbaaaabaaaaaaegacbaaaacaaaaaa
diaaaaaiicaabaaaacaaaaaadkaabaaaabaaaaaadkiacaaaaaaaaaaaacaaaaaa
diaaaaaipcaabaaaabaaaaaaegaobaaaacaaaaaafgifcaaaaaaaaaaaajaaaaaa
diaaaaakpcaabaaaabaaaaaaegaobaaaabaaaaaaaceaaaaaaaaakadpaaaakadp
aaaakadpaaaakadpaaaaaaaipcaabaaaacaaaaaaegbobaiaebaaaaaaabaaaaaa
egbobaaaacaaaaaabbaaaaahicaabaaaaaaaaaaaegaobaaaacaaaaaaegaobaaa
acaaaaaaelaaaaaficaabaaaaaaaaaaadkaabaaaaaaaaaaadcaaaaajicaabaaa
aaaaaaaadkaabaaaaaaaaaaaabeaaaaabhlhnbdiabeaaaaakmmfchdhaoaaaaak
icaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
aaaaaaaaddaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaaaaaaiadp
diaaaaahpcaabaaaacaaaaaapgapbaaaaaaaaaaaegaobaaaabaaaaaacaaaaaai
icaabaaaaaaaaaaaakiacaaaaaaaaaaaajaaaaaaabeaaaaaabaaaaaadhaaaaaj
pccabaaaaaaaaaaapgapbaaaaaaaaaaaegaobaaaacaaaaaaegaobaaaabaaaaaa
diaaaaaikcaabaaaaaaaaaaafgafbaaaaaaaaaaakgiocaaaabaaaaaaabaaaaaa
dcaaaaakdcaabaaaaaaaaaaaogikcaaaabaaaaaaaaaaaaaaagaabaaaaaaaaaaa
ngafbaaaaaaaaaaadcaaaaakdcaabaaaaaaaaaaaogikcaaaabaaaaaaacaaaaaa
kgakbaaaaaaaaaaaegaabaaaaaaaaaaaaaaaaaaidcaabaaaaaaaaaaaegaabaaa
aaaaaaaaogikcaaaabaaaaaaadaaaaaaaaaaaaahccaabaaaaaaaaaaabkaabaaa
aaaaaaaabkaabaaaaaaaaaaaaoaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaa
bkaabaaaaaaaaaaaaaaaaaagabmaaaaaakaabaaaaaaaaaaaabeaaaaaaaaaaadp
doaaaaab"
}

SubProgram "gles3 " {
Keywords { }
"!!GLES3"
}

}

#LINE 384

	}

} 
// Pour les cartes graphiques ne supportant pas nos Shaders
//Fallback "VertexLit" // automatically enables shadow casting, which probably eats resources and does't work anyway
}
