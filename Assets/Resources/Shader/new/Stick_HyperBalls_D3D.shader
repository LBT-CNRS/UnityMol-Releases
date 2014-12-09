/// @file Stick_HyperBalls_D3D.shader
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
/// $Id: Stick_HyperBalls_D3D.shader 378 2013-09-10 17:18:27Z kouyoumdjian $
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

// $Id: Stick_HyperBalls_D3D.shader 378 2013-09-10 17:18:27Z kouyoumdjian $
// (c) 2010 by Marc Baaden & Matthieu Chavent, <baaden@smplinux.de> <matthieu.chavent@free.fr>
// Unity3D FvNano shaders coming from FVNano project
//
// On a un cube (mesh) dans la scene -> vertex
// la spherepos du cube est 0, 0, 0

Shader "FvNano/Stick HyperBalls D3D" {

// Proprietes exposees a l'interface graphique
Properties {
	_Rayon1 ("Rayon de la Sphere1", float) = 0.1
	_Rayon2 ("Rayon de la Sphere2", float) = 0.1
    _Color ("Couleur du lien", Color) = (1,0.1,0.1,1.0)
    _Color2 ("Couleur du lien2", Color) = (1,0.1,0.1,1.0)
    _TexPos1 ("Position de la Sphere 1", Vector) = (0.0,0.0,0.0,1.0)     // important que w != 0 !!
    _TexPos2 ("Position de la Sphere 2", Vector) = (3.0,0.0,0.0,1.0)     // important que w != 0 !!
    _Shrink ("Shrink Factor", float) = 0.1
    _Visibilite ("Visibilite de l'hyperbole", float) = 1.0
    _Light ("Light vector", Vector) = (1,0,0,0)
    _Scale("Link Scale", float) = 1.0
    _EllipseFactor("Ellipse Factor", float) = 1.0
	_MatCap ("MatCap (RGB)", 2D) = "white" {}
	_MatCap2 ("MatCap (RGB)", 2D) = "white" {}
	_Attenuation ("Attenuation", float) = 0
	_Brightness ("Brightness", float) = 1.0
}


// ==========================================================
// L'actuel Shader Cg =======================================
// ==========================================================

SubShader {
    Pass {

Program "vp" {
// Vertex combos: 1
//   opengl - ALU: 274 to 274
//   d3d9 - ALU: 304 to 304, FLOW: 6 to 6
//   d3d11 - ALU: 104 to 104, TEX: 0 to 0, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
Bind "vertex" Vertex
Float 5 [_Rayon1]
Float 6 [_Rayon2]
Vector 7 [_Color]
Vector 8 [_Color2]
Vector 9 [_TexPos1]
Vector 10 [_TexPos2]
Float 11 [_Shrink]
Float 12 [_Scale]
"3.0-!!ARBvp1.0
# 274 ALU
PARAM c[14] = { { 0, 0.001, 0.5, 9.9999998e-14 },
		state.matrix.mvp,
		program.local[5..12],
		{ 2, 1, -1 } };
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
TEMP R14;
MOV R0, c[9];
ADD R8, -R0, c[10];
DP4 R0.x, -R8, -R8;
RSQ R0.w, R0.x;
MUL R4.xyz, R0.w, -R8;
ABS R0.x, R4.z;
SGE R0.y, c[0].x, R0.x;
ADD R0.x, -R4.z, c[0].w;
MAD R3.w, R0.x, R0.y, R4.z;
MOV R0.x, c[12];
MUL R1.w, R0.x, c[5].x;
MUL R2.w, R0.x, c[6].x;
MUL R6.w, R1, R1;
ABS R0.x, -R8.z;
SGE R1.x, c[0], R0;
MUL R9.z, R1.x, c[0].y;
ABS R1.y, -R8.x;
SGE R1.y, c[0].x, R1;
ABS R1.x, -R8.y;
SGE R1.x, c[0], R1;
MOV R7.z, R3.w;
MOV R7.xy, R4;
MAD R0.y, R2.w, R2.w, -R6.w;
MUL R2.xyz, R7, R0.y;
MUL R0.xyz, R7, R2;
MUL R9.x, R1.y, c[0].y;
MUL R9.y, R1.x, c[0];
ADD R1.xyz, R9, -R8;
MUL R1.xyz, R1, c[13].x;
MUL R2.xyz, R8, R8;
RCP R7.w, c[11].x;
MAD R2.xyz, R9, R9, -R2;
MAD R0.xyz, R0, R7.w, R2;
RCP R1.x, R1.x;
RCP R1.z, R1.z;
RCP R1.y, R1.y;
MUL R6.xyz, R0, R1;
MUL R0.xyz, R7, R6;
ADD R0.x, R0, R0.y;
ADD R0.x, R0, R0.z;
ADD R0.x, R0, -R4;
ADD R0.z, R0.x, -R4.y;
RCP R1.x, R3.w;
MUL R0.z, R0, R1.x;
MOV R0.xy, c[13].y;
ADD R0.xyz, R0, -R6;
DP3 R1.x, R0, R0;
RSQ R1.x, R1.x;
MUL R2.xyz, R1.x, R0;
MUL R0.xyz, R2.zxyw, R7.yzxw;
MAD R0.xyz, R2.yzxw, R7.zxyw, -R0;
DP3 R1.x, R0, R0;
RSQ R1.x, R1.x;
MUL R3.xyz, R1.x, R0;
RCP R0.y, R0.w;
SLT R0.x, R2.w, R1.w;
MOV R1.x, R2.z;
ABS R0.w, R0.x;
MUL R0.z, vertex.position, R0.y;
MUL R0.xy, R1.w, vertex.position;
MOV R1.w, c[13].y;
MOV R1.y, R3.z;
MOV R1.z, R3.w;
SGE R0.w, c[0].x, R0;
MUL R0.xy, R0, c[13].x;
MUL R2.zw, R2.w, vertex.position.xyxy;
MAD R2.zw, R2, c[13].x, -R0.xyxy;
MAD R0.xy, R2.zwzw, R0.w, R0;
DP3 R2.z, R1, R0;
MOV R1.y, R3;
MOV R1.z, R4.y;
MOV R1.x, R2.y;
DP3 R2.y, R0, R1;
MOV R1.y, R3.x;
MOV R1.z, R4.x;
MOV R1.x, R2;
DP3 R2.x, R0, R1;
ADD R0.xyz, R9, R8;
MAD R1.xyz, R0, c[0].z, R2;
DP4 R0.z, R1, c[4];
DP4 R0.x, R1, c[1];
DP4 R0.y, R1, c[2];
DP4 R5.z, R1, c[3];
MOV R1, c[4];
MUL R2, R1.wxyz, c[3].yzwx;
MAD R2, R1.yzwx, c[3].wxyz, -R2;
MUL R4, R2, c[2].zwxy;
MUL R3, R1.zwxy, c[3].wxyz;
MAD R3, R1.wxyz, c[3].zwxy, -R3;
MUL R2, R1.yzwx, c[3].zwxy;
MAD R1, R1.zwxy, c[3].yzwx, -R2;
MOV R2, c[2];
MAD R3, R3, c[2].yzwx, R4;
MAD R1, R1, c[2].wxyz, R3;
MUL R1, R1, c[1];
MUL R3.w, R2.x, c[1].y;
DP4 R0.w, R1, c[13].yzyz;
MUL R10.w, R2.z, c[1].y;
RCP R0.w, R0.w;
MOV R5.xy, R0;
MOV R5.w, R0.z;
MUL R13.z, R2.x, c[1];
MUL R13.y, R2.z, c[1].x;
MUL R1.y, R3.w, c[4].z;
MUL R3.z, R2.y, c[1].x;
MUL R1.x, R10.w, c[3];
MAD R1.x, R3.z, c[3].z, R1;
MAD R1.x, R13.z, c[3].y, R1;
MAD R1.x, -R13.y, c[3].y, R1;
MUL R11.x, R2.y, c[1].z;
MAD R1.x, -R3.w, c[3].z, R1;
MAD R1.y, R13, c[4], R1;
MAD R1.x, -R11, c[3], R1;
MAD R1.y, R11.x, c[4].x, R1;
MAD R1.y, -R3.z, c[4].z, R1;
MAD R3.x, -R10.w, c[4], R1.y;
MUL R4.w, R0, R1.x;
MOV R1, c[3];
MAD R3.x, -R13.z, c[4].y, R3;
MUL R13.w, R2, c[1].x;
MUL R10.z, R3.w, c[3].w;
MAD R11.w, R13, c[3].y, R10.z;
MUL R12.x, R2.y, c[1].w;
MAD R2.y, R12.x, c[3].x, R11.w;
MUL R10.z, R2.w, c[1].y;
MUL R14.y, R2.x, c[1].w;
MAD R2.y, -R3.z, c[3].w, R2;
MUL R11.w, R10.z, c[4].x;
MAD R3.z, R3, c[4].w, R11.w;
MAD R2.x, -R10.z, c[3], R2.y;
MAD R3.z, R14.y, c[4].y, R3;
MAD R2.y, -R13.w, c[4], R3.z;
MUL R11.z, R1.x, c[1].y;
MUL R4.z, R0.w, R3.x;
MUL R9.w, R1.z, c[1].y;
MUL R11.y, R1, c[1].x;
MUL R3.x, R9.w, c[4];
MUL R12.y, R1.z, c[1].x;
MUL R8.w, R1.y, c[1].z;
MUL R12.z, R1, c[2].x;
MUL R10.y, R1, c[2].z;
MUL R10.x, R1.z, c[2].y;
MUL R12.w, R1.x, c[2].z;
MUL R13.x, R1, c[1].z;
MAD R3.x, R11.y, c[4].z, R3;
MAD R3.x, R13, c[4].y, R3;
MAD R3.x, -R12.y, c[4].y, R3;
MAD R3.y, -R11.z, c[4].z, R3.x;
MUL R3.z, R11, c[4].w;
MAD R4.x, -R8.w, c[4], R3.y;
MUL R3.x, R1, c[2].y;
MUL R3.y, R3.x, c[4].z;
MAD R3.y, R12.z, c[4], R3;
MAD R4.y, R10, c[4].x, R3;
MUL R3.y, R1, c[2].x;
MAD R4.y, -R3, c[4].z, R4;
MAD R4.y, -R10.x, c[4].x, R4;
MOV result.position, R5;
MAD R5.z, -R12.w, c[4].y, R4.y;
MUL R4.y, R0.w, R4.x;
MUL R4.x, R0.w, R5.z;
MOV R5.z, c[0].x;
MAD R2.y, -R3.w, c[4].w, R2;
MAD R2.x, -R14.y, c[3].y, R2;
MUL R3.w, R0, R2.x;
MAD R2.x, -R12, c[4], R2.y;
MUL R2.y, R1.w, c[1].x;
MAD R3.z, R2.y, c[4].y, R3;
MUL R11.z, R1.y, c[1].w;
MAD R11.w, R11.z, c[4].x, R3.z;
MUL R3.z, R0.w, R2.x;
MAD R2.x, -R11.y, c[4].w, R11.w;
MUL R11.w, R1, c[2].y;
MUL R11.y, R1.w, c[1];
MAD R14.x, -R11.y, c[4], R2;
MUL R2.x, R1, c[1].w;
MUL R14.z, R11.w, c[4].x;
MAD R3.y, R3, c[4].w, R14.z;
MUL R14.w, R1.x, c[2];
MAD R14.x, -R2, c[4].y, R14;
MAD R1.x, R14.w, c[4].y, R3.y;
MUL R3.y, R0.w, R14.x;
MUL R14.x, R2.w, c[1].z;
MUL R14.z, R1.w, c[2].x;
MAD R1.x, -R14.z, c[4].y, R1;
MAD R3.x, -R3, c[4].w, R1;
MUL R1.x, R1.y, c[2].w;
MAD R1.y, -R1.x, c[4].x, R3.x;
MUL R3.x, R0.w, R1.y;
MUL R2.w, R14.x, c[3].x;
MAD R1.y, R13, c[3].w, R2.w;
MAD R2.w, R14.y, c[3].z, R1.y;
MUL R1.y, R13.z, c[4].w;
MAD R2.w, -R13, c[3].z, R2;
MAD R13.w, R13, c[4].z, R1.y;
MUL R1.y, R2.z, c[1].w;
MAD R2.z, -R13, c[3].w, R2.w;
MAD R13.w, R1.y, c[4].x, R13;
MAD R2.w, -R13.y, c[4], R13;
MAD R2.z, -R1.y, c[3].x, R2;
MAD R13.y, -R14.x, c[4].x, R2.w;
MUL R2.w, R0, R2.z;
MAD R2.z, -R14.y, c[4], R13.y;
MUL R13.y, R1.w, c[1].z;
MUL R2.z, R0.w, R2;
DP4 result.texcoord[2].w, R4, R5;
DP4 result.texcoord[2].z, R5, R3;
MUL R13.z, R13.y, c[4].x;
MUL R13.w, R12, c[4];
MAD R12.w, R12.y, c[4], R13.z;
MAD R2.x, R2, c[4].z, R12.w;
MUL R12.y, R1.z, c[2].w;
MAD R13.z, R14, c[4], R13.w;
MAD R13.z, R12.y, c[4].x, R13;
MAD R12.w, -R12.z, c[4], R13.z;
MUL R12.z, R1.w, c[2];
MAD R1.w, -R2.y, c[4].z, R2.x;
MAD R12.w, -R12.z, c[4].x, R12;
MAD R2.x, -R14.w, c[4].z, R12.w;
MUL R12.w, R1.z, c[1];
MAD R1.w, -R13.x, c[4], R1;
MAD R1.z, -R12.w, c[4].x, R1.w;
MUL R2.y, R0.w, R1.z;
MUL R2.x, R0.w, R2;
MUL R1.z, R11.x, c[3].w;
MAD R1.z, R10, c[3], R1;
MAD R1.z, R1.y, c[3].y, R1;
MUL R1.w, R14.x, c[4].y;
MAD R1.w, R10, c[4], R1;
MAD R1.w, R12.x, c[4].z, R1;
MAD R1.w, -R10.z, c[4].z, R1;
MAD R1.w, -R11.x, c[4], R1;
MAD R1.z, -R10.w, c[3].w, R1;
MAD R1.z, -R14.x, c[3].y, R1;
MAD R1.y, -R1, c[4], R1.w;
MAD R1.z, -R12.x, c[3], R1;
MUL R1.w, R0, R1.z;
MUL R1.z, R0.w, R1.y;
MUL R1.y, R8.w, c[4].w;
MUL R10.z, R12, c[4].y;
MAD R8.w, R10.x, c[4], R10.z;
MAD R8.w, R1.x, c[4].z, R8;
MAD R1.y, R11, c[4].z, R1;
MAD R1.x, R12.w, c[4].y, R1.y;
MAD R1.y, -R11.w, c[4].z, R8.w;
MAD R1.y, -R10, c[4].w, R1;
MAD R1.x, -R9.w, c[4].w, R1;
MAD R1.x, -R13.y, c[4].y, R1;
MAD R8.w, -R12.y, c[4].y, R1.y;
MAD R1.y, -R11.z, c[4].z, R1.x;
MUL R1.x, R0.w, R8.w;
MUL R1.y, R0.w, R1;
MOV R0.zw, R0.z;
DP4 result.texcoord[3].w, R4, R0;
DP4 result.texcoord[3].z, R3, R0;
DP4 result.texcoord[2].y, R5, R2;
DP4 result.texcoord[2].x, R5, R1;
ADD R5.xyz, R9, -R6;
MUL R10.xyz, R5, R5;
ADD R4.x, R10, R10.y;
ADD R3.x, R4, R10.z;
MAD R6.w, R6, R7, -R3.x;
MOV R7.w, c[8].x;
DP4 result.texcoord[3].y, R2, R0;
DP4 result.texcoord[3].x, R1, R0;
ADD R0.xyz, R8, -R6;
MOV result.texcoord[4], R6;
MAD result.texcoord[6].xyz, -R5, c[11].x, R9;
MAD result.texcoord[7].xyz, -R0, c[11].x, R8;
MOV result.texcoord[5], R7;
MOV result.texcoord[0], c[7];
MOV result.texcoord[6].w, c[8].y;
MOV result.texcoord[7].w, c[8].z;
MOV result.texcoord[1].x, c[11];
END
# 274 instructions, 15 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Bind "vertex" Vertex
Matrix 0 [glstate_matrix_mvp]
Float 4 [_Rayon1]
Float 5 [_Rayon2]
Vector 6 [_Color]
Vector 7 [_Color2]
Vector 8 [_TexPos1]
Vector 9 [_TexPos2]
Float 10 [_Shrink]
Float 11 [_Scale]
"vs_3_0
; 304 ALU, 6 FLOW
dcl_position o0
dcl_texcoord2 o1
dcl_texcoord3 o2
dcl_texcoord4 o3
dcl_texcoord5 o4
dcl_texcoord6 o5
dcl_texcoord7 o6
dcl_texcoord0 o7
dcl_texcoord1 o8
def c12, 1.00000000, -1.00000000, 0.00000000, 2.00000000
def c13, 0.00000000, 0.00100000, 0.00000000, 0.50000000
dcl_position0 v0
mov r1, c2
mul r2, c3.wxyz, r1.yzwx
mov r1, c2
mad r1, c3.yzwx, r1.wxyz, -r2
mov r0, c2
mul r2, c3.zwxy, r0.wxyz
mov r0, c2
mad r2, c3.wxyz, r0.zwxy, -r2
mul r1, r1, c1.zwxy
mov r3.w, c1.y
mad r1, r2, c1.yzwx, r1
mov r0, c2
mul r2, c3.yzwx, r0.zwxy
mov r0, c2
mad r0, c3.zwxy, r0.yzwx, -r2
mad r0, r0, c1.wxyz, r1
mul r0, r0, c0
dp4 r0.x, r0, c12.xyxy
rcp r3.x, r0.x
mov r0.x, c0.z
mul r4.x, c1.y, r0
mov r0.y, c0.w
mul r1.z, c1, r0.y
mov r0.x, c0.y
mov r0.y, c0.z
mul r1.w, c1, r0.y
mul r2.w, c1, r0.x
mul r0.z, r4.x, c2.w
mad r0.x, r2.w, c2.z, r0.z
mad r0.z, r1, c2.y, r0.x
mov r0.x, c0.y
mul r3.z, c1, r0.x
mad r0.y, -r3.z, c2.w, r0.z
mul r0.z, r1.w, c3.y
mov r0.x, c0.w
mul r2.z, c1.y, r0.x
mad r0.x, -r1.w, c2.y, r0.y
mad r0.x, -r2.z, c2.z, r0
mad r0.z, r3, c3.w, r0
mad r0.z, r2, c3, r0
mad r0.y, -r2.w, c3.z, r0.z
mad r0.y, -r4.x, c3.w, r0
mad r0.y, -r1.z, c3, r0
mul r0.w, r3.x, r0.x
mov r0.x, c0.z
mul r3.y, c2, r0.x
mul r0.z, r3.x, r0.y
mov r0.x, c0.y
mul r2.y, c2.w, r0.x
mul r0.y, r3, c3.w
mad r1.x, r2.y, c3.z, r0.y
mov r0.x, c0.w
mul r1.y, c2.z, r0.x
mov r0.y, c0
mul r4.y, c2.z, r0
mad r0.x, r1.y, c3.y, r1
mad r1.x, -r4.y, c3.w, r0
mov r0.x, c0.z
mul r5.y, c2.w, r0.x
mad r0.x, -r5.y, c3.y, r1
mov r0.y, c0.w
mul r7.x, c2.y, r0.y
mad r0.y, -r7.x, c3.z, r0.x
mov r0.x, c1.z
mul r1.x, c2.w, r0
mov r0.x, c1.y
mul r5.x, c2.z, r0
mul r2.x, r1, c3.y
mad r4.z, r5.x, c3.w, r2.x
mov r0.x, c1.w
mul r2.x, c2.y, r0
mad r0.x, r2, c3.z, r4.z
mul r3.w, c2, r3
mad r4.w, -r3, c3.z, r0.x
mov r0.x, c1.z
mul r5.z, c2.y, r0.x
mov r4.z, c1.w
mul r5.w, c2.z, r4.z
mad r0.x, -r5.z, c3.w, r4.w
mad r0.x, -r5.w, c3.y, r0
mov r4.z, c0
mul r4.w, c1.x, r4.z
mov r4.z, c0.x
mul r7.y, c1.w, r4.z
mul r6.x, r4.w, c3.w
mad r6.z, r7.y, c3, r6.x
mov r6.x, c0.w
mov r4.z, c0.x
mul r7.z, c1.x, r6.x
mul r4.z, c1, r4
mul r6.y, r1.w, c2.x
mad r6.y, r4.z, c2.w, r6
mad r6.x, r7.z, c2.z, r6.y
mad r6.y, r1.z, c3.x, r6.z
mad r6.y, -r4.z, c3.w, r6
mad r1.w, -r1, c3.x, r6.y
mad r6.x, -r7.y, c2.z, r6
mad r6.x, -r4.w, c2.w, r6
mad r1.z, -r1, c2.x, r6.x
mad r6.x, -r7.z, c3.z, r1.w
mul r1.w, r3.x, r1.z
mul r1.z, r3.x, r6.x
mov r6.y, c0.x
mul r8.x, c2.w, r6.y
mov r6.x, c0
mul r0.x, r3, r0
mul r5.y, r5, c3.x
mul r6.x, c2.z, r6
mad r6.z, r6.x, c3.w, r5.y
mov r5.y, c0.w
mul r7.w, c2.x, r5.y
mad r5.y, r7.w, c3.z, r6.z
mad r6.w, -r8.x, c3.z, r5.y
mov r6.y, c0
mul r6.z, c1.x, r6.y
mov r5.y, c0.z
mul r6.y, c2.x, r5
mad r5.y, -r6, c3.w, r6.w
mad r1.y, -r1, c3.x, r5
mul r6.w, r6.z, c2
mad r5.y, r7, c2, r6.w
mad r8.z, r2, c2.x, r5.y
mov r5.y, c0.x
mul r6.w, c1.y, r5.y
mad r8.w, -r6, c2, r8.z
mov r8.y, c1.z
mul r5.y, c2.x, r8
mad r8.w, -r2, c2.x, r8
mov r8.y, c1.x
mul r8.z, r5.y, c3.w
mul r8.y, c2.w, r8
mad r9.x, r8.y, c3.z, r8.z
mad r8.w, -r7.z, c2.y, r8
mov r8.z, c1.x
mad r9.x, r5.w, c3, r9
mul r5.w, c2.z, r8.z
mad r9.x, -r5.w, c3.w, r9
mad r1.x, -r1, c3, r9
mul r9.x, r2.w, c3
mul r2.w, r3.x, r8
mad r9.x, r6.w, c3.w, r9
mad r9.x, r7.z, c3.y, r9
mov r8.w, c0.y
mov r8.z, c1.w
mul r8.z, c2.x, r8
mad r1.x, -r8.z, c3.z, r1
mad r7.y, -r7, c3, r9.x
mul r7.z, c2.x, r8.w
mad r8.w, -r6.z, c3, r7.y
mad r2.z, -r2, c3.x, r8.w
mul r9.x, r7.z, c3.w
mad r8.x, r8, c3.y, r9
mov r7.y, c0.x
mad r7.x, r7, c3, r8
mul r7.y, c2, r7
mad r7.x, -r7.y, c3.w, r7
mad r2.y, -r2, c3.x, r7.x
mad r2.y, -r7.w, c3, r2
mul r7.x, r3.z, c2
mad r7.w, r6, c2.z, r7.x
mov r7.x, c1
mul r4.y, r4, c3.x
mad r4.y, r7, c3.z, r4
mad r8.w, r4, c2.y, r7
mul r7.x, c2.y, r7
mul r3.w, r3, c3.x
mad r3.w, r7.x, c3, r3
mad r7.w, r8.z, c3.y, r3
mad r8.x, -r8.y, c3.y, r7.w
mov r3.w, c1.y
mul r7.w, c2.x, r3
mad r3.w, -r7, c3, r8.x
mad r2.x, -r2, c3, r3.w
mad r8.x, -r4.z, c2.y, r8.w
mad r3.w, -r6.z, c2.z, r8.x
mad r3.w, -r4.x, c2.x, r3
mul r6.z, r6, c3
mad r4.z, r4, c3.y, r6
mad r4.x, r4, c3, r4.z
mad r4.x, -r6.w, c3.z, r4
mad r3.z, -r3, c3.x, r4.x
mad r4.y, r6, c3, r4
mad r4.x, -r6, c3.y, r4.y
mad r3.z, -r4.w, c3.y, r3
mad r4.x, -r7.z, c3.z, r4
mad r3.y, -r3, c3.x, r4.x
mul r4.x, r7.w, c3.z
mad r5.w, r5, c3.y, r4.x
mad r5.z, r5, c3.x, r5.w
mad r5.z, -r7.x, c3, r5
mad r5.x, -r5, c3, r5.z
mov r4, c9
add r4, -c8, r4
dp4 r5.w, r4, r4
rsq r5.z, r5.w
mad r5.x, -r5.y, c3.y, r5
rcp r5.y, r5.z
mul r5.z, v0, r5.y
mov r5.y, c5.x
mul r0.y, r3.x, r0
mul r1.y, r3.x, r1
mul r1.x, r3, r1
mul r2.z, r3.x, r2
mul r2.y, r3.x, r2
mul r2.x, r3, r2
mul r3.w, r3.x, r3
mul r3.z, r3.x, r3
mul r3.y, r3.x, r3
mul r3.x, r3, r5
mov r5.x, c4
mul r5.w, c11.x, r5.y
mul r6.w, c11.x, r5.x
mov r9.xyz, c12.z
if_gt r6.w, r5.w
mul r5.xy, v0, r6.w
mul r5.xy, r5, c12.w
else
mul r5.xy, v0, r5.w
mul r5.xy, r5, c12.w
endif
dp4 r4.w, -r4, -r4
rsq r4.w, r4.w
mul r6.xyz, r4.w, -r4
mov r12.xy, r6
mov r10.xyz, r6
if_eq r6.z, c12.z
mov r10.z, c13.x
endif
if_eq -r4.x, c12.z
mov r9.xyz, c13.yzzw
endif
if_eq -r4.y, c12.z
mov r9.y, c13
endif
if_eq -r4.z, c12.z
mov r9.z, c13.y
endif
mul r4.w, r6, r6
add r6.xyz, r9, -r4
mul r7.xyz, r4, r4
mad r7.xyz, r9, r9, -r7
mul r8.xyz, r6, c12.w
mad r5.w, r5, r5, -r4
mul r6.xyz, r10, r5.w
mov r6.w, c12.x
rcp r8.w, c10.x
mul r6.xyz, r10, r6
mad r6.xyz, r6, r8.w, r7
rcp r7.x, r8.x
rcp r7.z, r8.z
rcp r7.y, r8.y
mul r8.xyz, r6, r7
mul r6.xyz, r10, r8
add r5.w, r6.x, r6.y
add r5.w, r5, r6.z
add r5.w, r5, -r12.x
add r5.w, r5, -r12.y
rcp r6.z, r10.z
mul r6.z, r5.w, r6
mov r6.xy, c12.x
add r6.xyz, r6, -r8
dp3 r5.w, r6, r6
rsq r5.w, r5.w
mul r6.xyz, r5.w, r6
mul r7.xyz, r6.zxyw, r10.yzxw
mad r7.xyz, r6.yzxw, r10.zxyw, -r7
dp3 r5.w, r7, r7
rsq r5.w, r5.w
mul r7.xyz, r5.w, r7
mov r11.y, r7.z
mov r11.x, r6.z
mov r11.z, r10
dp3 r6.z, r11, r5
mov r11.y, r7
mov r7.y, r7.x
mov r7.x, r6
mov r7.z, r12.x
dp3 r6.x, r5, r7
mov r11.z, r12.y
mov r11.x, r6.y
dp3 r6.y, r5, r11
add r5.xyz, r9, r4
mad r6.xyz, r5, c13.w, r6
dp4 r7.z, r6, c3
mov r5.w, r7.z
dp4 r7.x, r6, c0
dp4 r7.y, r6, c1
mov r7.zw, r7.z
mov r5.xy, r7
dp4 r5.z, r6, c2
mov o0, r5
mov r5.z, c12
dp4 o1.w, r3, r5
dp4 o1.z, r5, r2
dp4 o1.x, r5, r0
dp4 o1.y, r5, r1
add r5.xyz, r9, -r8
dp4 o2.x, r0, r7
add r0.xyz, r4, -r8
mul r6.xyz, r5, r5
dp4 o2.w, r3, r7
add r3.x, r6, r6.y
dp4 o2.z, r2, r7
add r2.x, r3, r6.z
mad r8.w, r4, r8, -r2.x
dp4 o2.y, r1, r7
mov o3, r8
mad o5.xyz, -r5, c10.x, r9
mad o6.xyz, -r0, c10.x, r4
mov o7, c6
mov o4.xyz, r10
mov o4.w, c7.x
mov o5.w, c7.y
mov o6.w, c7.z
mov o8.x, c10
"
}

SubProgram "d3d11 " {
Keywords { }
Bind "vertex" Vertex
ConstBuffer "$Globals" 144 // 120 used size, 13 vars
Float 0 [_Rayon1]
Float 4 [_Rayon2]
Vector 16 [_Color] 4
Vector 32 [_Color2] 4
Vector 48 [_TexPos1] 4
Vector 64 [_TexPos2] 4
Float 112 [_Shrink]
Float 116 [_Scale]
ConstBuffer "UnityPerDraw" 336 // 64 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
BindCB "$Globals" 0
BindCB "UnityPerDraw" 1
// 228 instructions, 16 temp regs, 0 temp arrays:
// ALU 103 float, 0 int, 1 uint
// TEX 0 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"vs_4_0
eefiecedfgbhpfahgfhabmcfbcjbmedgilpnglenabaaaaaabicbaaaaadaaaaaa
cmaaaaaagaaaaaaajaabaaaaejfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapahaaaafaepfdejfeejepeoaaklklkl
epfdeheociabaaaaalaaaaaaaiaaaaaabaabaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaabmabaaaaacaaaaaaaaaaaaaaadaaaaaaabaaaaaaapaaaaaa
bmabaaaaadaaaaaaaaaaaaaaadaaaaaaacaaaaaaapaaaaaabmabaaaaaeaaaaaa
aaaaaaaaadaaaaaaadaaaaaaapaaaaaabmabaaaaafaaaaaaaaaaaaaaadaaaaaa
aeaaaaaaapaaaaaabmabaaaaagaaaaaaaaaaaaaaadaaaaaaafaaaaaaadamaaaa
bmabaaaaabaaaaaaaaaaaaaaadaaaaaaafaaaaaaaealaaaabmabaaaaahaaaaaa
aaaaaaaaadaaaaaaagaaaaaaadamaaaabmabaaaaaiaaaaaaaaaaaaaaadaaaaaa
ahaaaaaaadamaaaabmabaaaaajaaaaaaaaaaaaaaadaaaaaaaiaaaaaaadamaaaa
bmabaaaaaaaaaaaaaaaaaaaaadaaaaaaajaaaaaaapaaaaaafdfgfpfagphdgjhe
gjgpgoaafeeffiedepepfceeaaklklklfdeieefciabpaaaaeaaaabaaoaahaaaa
fjaaaaaeegiocaaaaaaaaaaaaiaaaaaafjaaaaaeegiocaaaabaaaaaaaeaaaaaa
fpaaaaadhcbabaaaaaaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaad
pccabaaaabaaaaaagfaaaaadpccabaaaacaaaaaagfaaaaadpccabaaaadaaaaaa
gfaaaaadpccabaaaaeaaaaaagfaaaaaddccabaaaafaaaaaagfaaaaadeccabaaa
afaaaaaagfaaaaaddccabaaaagaaaaaagfaaaaaddccabaaaahaaaaaagfaaaaad
dccabaaaaiaaaaaagfaaaaadpccabaaaajaaaaaagiaaaaacbaaaaaaadgaaaaai
gcaabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaiadpaaaaiadpaaaaaaaadiaaaaaj
dcaabaaaabaaaaaabgifcaaaaaaaaaaaaaaaaaaafgifcaaaaaaaaaaaahaaaaaa
diaaaaahicaabaaaaaaaaaaabkaabaaaabaaaaaabkaabaaaabaaaaaadcaaaaak
ecaabaaaabaaaaaaakaabaaaabaaaaaaakaabaaaabaaaaaadkaabaiaebaaaaaa
aaaaaaaaaoaaaaaiicaabaaaaaaaaaaadkaabaaaaaaaaaaaakiacaaaaaaaaaaa
ahaaaaaaaaaaaaakpcaabaaaacaaaaaaegiocaiaebaaaaaaaaaaaaaaadaaaaaa
egiocaaaaaaaaaaaaeaaaaaabbaaaaajicaabaaaabaaaaaaegaobaiaebaaaaaa
acaaaaaaegaobaiaebaaaaaaacaaaaaaeeaaaaaficaabaaaacaaaaaadkaabaaa
abaaaaaaelaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahecaabaaa
adaaaaaadkaabaaaabaaaaaackbabaaaaaaaaaaadiaaaaailcaabaaaaeaaaaaa
pgapbaaaacaaaaaaegaibaiaebaaaaaaacaaaaaabiaaaaahicaabaaaabaaaaaa
dkaabaaaaeaaaaaaabeaaaaaaaaaaaaadhaaaaajecaabaaaaeaaaaaadkaabaaa
abaaaaaaabeaaaaabdcoobcjdkaabaaaaeaaaaaadiaaaaahhcaabaaaafaaaaaa
egacbaaaaeaaaaaaegacbaaaaeaaaaaadiaaaaahhcaabaaaafaaaaaakgakbaaa
abaaaaaaegacbaaaafaaaaaaaoaaaaaihcaabaaaafaaaaaaegacbaaaafaaaaaa
agiacaaaaaaaaaaaahaaaaaadiaaaaahhcaabaaaagaaaaaaegacbaaaacaaaaaa
egacbaaaacaaaaaabiaaaaalhcaabaaaahaaaaaaegacbaiaebaaaaaaacaaaaaa
aceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabaaaaakhcaabaaaahaaaaaa
egacbaaaahaaaaaaaceaaaaagpbciddkgpbciddkgpbciddkaaaaaaaadcaaaaak
hcaabaaaagaaaaaaegacbaaaahaaaaaaegacbaaaahaaaaaaegacbaiaebaaaaaa
agaaaaaaaaaaaaahhcaabaaaafaaaaaaegacbaaaafaaaaaaegacbaaaagaaaaaa
aaaaaaaihcaabaaaagaaaaaaegacbaiaebaaaaaaacaaaaaaegacbaaaahaaaaaa
aaaaaaahhcaabaaaagaaaaaaegacbaaaagaaaaaaegacbaaaagaaaaaaaoaaaaah
hcaabaaaafaaaaaaegacbaaaafaaaaaaegacbaaaagaaaaaadiaaaaahmcaabaaa
abaaaaaaagaebaaaaeaaaaaaagaebaaaafaaaaaaaaaaaaahecaabaaaabaaaaaa
dkaabaaaabaaaaaackaabaaaabaaaaaadcaaaaajecaabaaaabaaaaaackaabaaa
aeaaaaaackaabaaaafaaaaaackaabaaaabaaaaaadcaaaaakecaabaaaabaaaaaa
akaabaiaabaaaaaaacaaaaaadkaabaaaacaaaaaackaabaaaabaaaaaadcaaaaak
ecaabaaaabaaaaaabkaabaiaabaaaaaaacaaaaaadkaabaaaacaaaaaackaabaaa
abaaaaaaaoaaaaahbcaabaaaaaaaaaaackaabaaaabaaaaaackaabaaaaeaaaaaa
aaaaaaaihcaabaaaaaaaaaaaegacbaaaaaaaaaaacgajbaiaebaaaaaaafaaaaaa
baaaaaahecaabaaaabaaaaaaegacbaaaaaaaaaaaegacbaaaaaaaaaaaeeaaaaaf
ecaabaaaabaaaaaackaabaaaabaaaaaadiaaaaahhcaabaaaaaaaaaaacgajbaaa
aaaaaaaakgakbaaaabaaaaaadiaaaaahhcaabaaaagaaaaaajgaebaaaaeaaaaaa
jgaebaaaaaaaaaaadcaaaaakhcaabaaaagaaaaaaegacbaaaaaaaaaaacgajbaaa
aeaaaaaaegacbaiaebaaaaaaagaaaaaabaaaaaahecaabaaaabaaaaaaegacbaaa
agaaaaaaegacbaaaagaaaaaaeeaaaaafecaabaaaabaaaaaackaabaaaabaaaaaa
diaaaaahhcaabaaaagaaaaaakgakbaaaabaaaaaaegacbaaaagaaaaaadgaaaaaf
ccaabaaaaiaaaaaaakaabaaaagaaaaaadgaaaaafbcaabaaaaiaaaaaackaabaaa
aaaaaaaaaaaaaaahpcaabaaaajaaaaaaegbebaaaaaaaaaaaegbebaaaaaaaaaaa
diaaaaahpcaabaaaajaaaaaafgaabaaaabaaaaaaegaobaaaajaaaaaadbaaaaah
bcaabaaaabaaaaaaakaabaaaabaaaaaabkaabaaaabaaaaaadhaaaaajdcaabaaa
adaaaaaaagaabaaaabaaaaaaegaabaaaajaaaaaaogakbaaaajaaaaaadgaaaaaf
ecaabaaaaiaaaaaaakaabaaaaeaaaaaadgaaaaafecaabaaaaaaaaaaabkaabaaa
aeaaaaaabaaaaaahbcaabaaaabaaaaaaegacbaaaaiaaaaaaegacbaaaadaaaaaa
dgaaaaaficcabaaaabaaaaaackaabaaaaiaaaaaadgaaaaafbcaabaaaaeaaaaaa
bkaabaaaaaaaaaaadgaaaaafccaabaaaaaaaaaaabkaabaaaagaaaaaadgaaaaaf
ccaabaaaaeaaaaaackaabaaaagaaaaaabaaaaaahecaabaaaabaaaaaaegacbaaa
aeaaaaaaegacbaaaadaaaaaadgaaaaaficcabaaaadaaaaaackaabaaaaeaaaaaa
baaaaaahccaabaaaabaaaaaaegacbaaaaaaaaaaaegacbaaaadaaaaaadgaaaaaf
iccabaaaacaaaaaackaabaaaaaaaaaaaaaaaaaahhcaabaaaaaaaaaaaegacbaaa
acaaaaaaegacbaaaahaaaaaadcaaaaamhcaabaaaaaaaaaaaegacbaaaaaaaaaaa
aceaaaaaaaaaaadpaaaaaadpaaaaaadpaaaaaaaaegacbaaaabaaaaaadiaaaaai
pcaabaaaabaaaaaafgafbaaaaaaaaaaaegiocaaaabaaaaaaabaaaaaadcaaaaak
pcaabaaaabaaaaaaegiocaaaabaaaaaaaaaaaaaaagaabaaaaaaaaaaaegaobaaa
abaaaaaadcaaaaakpcaabaaaabaaaaaaegiocaaaabaaaaaaacaaaaaakgakbaaa
aaaaaaaaegaobaaaabaaaaaaaaaaaaaipcaabaaaabaaaaaaegaobaaaabaaaaaa
egiocaaaabaaaaaaadaaaaaadgaaaaafpccabaaaaaaaaaaaegaobaaaabaaaaaa
dgaaaaafeccabaaaabaaaaaaakaabaaaafaaaaaadiaaaaajhcaabaaaaaaaaaaa
hgiocaaaabaaaaaaacaaaaaaogijcaaaabaaaaaaadaaaaaadiaaaaajecaabaaa
abaaaaaabkiacaaaabaaaaaaacaaaaaackiacaaaabaaaaaaadaaaaaadiaaaaaj
pcaabaaaadaaaaaaggiicaaaabaaaaaaabaaaaaajgiccaaaabaaaaaaacaaaaaa
diaaaaaidcaabaaaaeaaaaaangafbaaaadaaaaaapgipcaaaabaaaaaaadaaaaaa
dcaaaaakicaabaaaacaaaaaackaabaaaabaaaaaadkiacaaaabaaaaaaabaaaaaa
akaabaaaaeaaaaaadiaaaaajpcaabaaaagaaaaaajgiccaaaabaaaaaaabaaaaaa
ggiicaaaabaaaaaaadaaaaaadcaaaaakicaabaaaacaaaaaabkaabaaaagaaaaaa
dkiacaaaabaaaaaaacaaaaaadkaabaaaacaaaaaadcaaaaalicaabaaaacaaaaaa
akaabaiaebaaaaaaagaaaaaadkiacaaaabaaaaaaacaaaaaadkaabaaaacaaaaaa
dcaaaaalicaabaaaacaaaaaaakaabaiaebaaaaaaadaaaaaadkiacaaaabaaaaaa
adaaaaaadkaabaaaacaaaaaadcaaaaalicaabaaaacaaaaaackaabaiaebaaaaaa
aaaaaaaadkiacaaaabaaaaaaabaaaaaadkaabaaaacaaaaaadcaaaaamncaabaaa
aeaaaaaakgihcaaaabaaaaaaacaaaaaapgijcaaaabaaaaaaadaaaaaaagajbaia
ebaaaaaaaaaaaaaabaaaaaaibcaabaaaaiaaaaaaigadbaaaaeaaaaaajgihcaaa
abaaaaaaabaaaaaadiaaaaajncaabaaaaeaaaaaapgiecaaaabaaaaaaacaaaaaa
fgidcaaaabaaaaaaadaaaaaadcaaaaamncaabaaaaeaaaaaafgidcaaaabaaaaaa
acaaaaaapgiecaaaabaaaaaaadaaaaaaagaobaiaebaaaaaaaeaaaaaabaaaaaai
ecaabaaaaiaaaaaaigadbaaaaeaaaaaaegidcaaaabaaaaaaabaaaaaadiaaaaaj
ncaabaaaaeaaaaaapgiicaaaabaaaaaaacaaaaaakgidcaaaabaaaaaaadaaaaaa
dcaaaaamhcaabaaaajaaaaaaogiicaiaebaaaaaaabaaaaaaacaaaaaadgiocaaa
abaaaaaaadaaaaaaigadbaaaaeaaaaaabaaaaaaiccaabaaaaiaaaaaaegacbaaa
ajaaaaaaigidcaaaabaaaaaaabaaaaaadiaaaaajhcaabaaaajaaaaaajgiecaaa
abaaaaaaacaaaaaacgijcaaaabaaaaaaadaaaaaadcaaaaamlcaabaaaajaaaaaa
cgigcaaaabaaaaaaacaaaaaajgibcaaaabaaaaaaadaaaaaaegaibaiaebaaaaaa
ajaaaaaabaaaaaaiicaabaaaaiaaaaaaegadbaaaajaaaaaaegiccaaaabaaaaaa
abaaaaaabbaaaaaibcaabaaaaaaaaaaaegaobaaaaiaaaaaaegiocaaaabaaaaaa
aaaaaaaaaoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpakaabaaaaaaaaaaadiaaaaahbcaabaaaaiaaaaaadkaabaaaacaaaaaa
akaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaackaabaaaajaaaaaadkiacaaa
abaaaaaaabaaaaaadiaaaaajpcaabaaaakaaaaaabgiecaaaabaaaaaaabaaaaaa
egibcaaaabaaaaaaacaaaaaadcaaaaakccaabaaaaaaaaaaackaabaaaakaaaaaa
dkiacaaaabaaaaaaadaaaaaabkaabaaaaaaaaaaadiaaaaajpcaabaaaalaaaaaa
egibcaaaabaaaaaaabaaaaaabgiecaaaabaaaaaaadaaaaaadcaaaaakccaabaaa
aaaaaaaackaabaaaalaaaaaadkiacaaaabaaaaaaacaaaaaabkaabaaaaaaaaaaa
dcaaaaalccaabaaaaaaaaaaadkaabaiaebaaaaaaalaaaaaadkiacaaaabaaaaaa
acaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaadkaabaiaebaaaaaa
akaaaaaadkiacaaaabaaaaaaadaaaaaabkaabaaaaaaaaaaadiaaaaajfcaabaaa
aeaaaaaaagibcaaaabaaaaaaacaaaaaakgiicaaaabaaaaaaadaaaaaadcaaaaal
ccaabaaaaaaaaaaackaabaiaebaaaaaaaeaaaaaadkiacaaaabaaaaaaabaaaaaa
bkaabaaaaaaaaaaadiaaaaahecaabaaaaiaaaaaabkaabaaaaaaaaaaaakaabaaa
aaaaaaaadcaaaaakccaabaaaaaaaaaaadkaabaaaagaaaaaadkiacaaaabaaaaaa
acaaaaaabkaabaaaaeaaaaaadcaaaaakccaabaaaaaaaaaaadkaabaaaaeaaaaaa
dkiacaaaabaaaaaaabaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaa
ckaabaiaebaaaaaaadaaaaaadkiacaaaabaaaaaaadaaaaaabkaabaaaaaaaaaaa
dcaaaaalccaabaaaaaaaaaaaakaabaiaebaaaaaaaeaaaaaadkiacaaaabaaaaaa
abaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaackaabaiaebaaaaaa
agaaaaaadkiacaaaabaaaaaaacaaaaaabkaabaaaaaaaaaaadiaaaaahccaabaaa
aiaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaa
dkaabaaaakaaaaaackiacaaaabaaaaaaadaaaaaadcaaaaakccaabaaaaaaaaaaa
dkaabaaaalaaaaaackiacaaaabaaaaaaacaaaaaabkaabaaaaaaaaaaadcaaaaak
ccaabaaaaaaaaaaackaabaaaaeaaaaaackiacaaaabaaaaaaabaaaaaabkaabaaa
aaaaaaaadcaaaaalccaabaaaaaaaaaaackaabaiaebaaaaaaakaaaaaackiacaaa
abaaaaaaadaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaackaabaia
ebaaaaaaajaaaaaackiacaaaabaaaaaaabaaaaaabkaabaaaaaaaaaaadcaaaaal
ccaabaaaaaaaaaaackaabaiaebaaaaaaalaaaaaackiacaaaabaaaaaaacaaaaaa
bkaabaaaaaaaaaaadiaaaaahicaabaaaaiaaaaaabkaabaaaaaaaaaaaakaabaaa
aaaaaaaabbaaaaahcccabaaaabaaaaaaegaobaaaaiaaaaaaegapbaaaabaaaaaa
baaaaaahbccabaaaabaaaaaaegadbaaaaiaaaaaaegadbaaaabaaaaaadiaaaaaj
pcaabaaaaiaaaaaajgiccaaaabaaaaaaaaaaaaaaggiicaaaabaaaaaaacaaaaaa
diaaaaaiccaabaaaaaaaaaaabkaabaaaaiaaaaaadkiacaaaabaaaaaaadaaaaaa
diaaaaajpcaabaaaamaaaaaaggiicaaaabaaaaaaaaaaaaaajgiccaaaabaaaaaa
adaaaaaadcaaaaakccaabaaaaaaaaaaabkaabaaaamaaaaaadkiacaaaabaaaaaa
acaaaaaabkaabaaaaaaaaaaadcaaaaakccaabaaaaaaaaaaackaabaaaaaaaaaaa
dkiacaaaabaaaaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaa
akaabaiaebaaaaaaaiaaaaaadkiacaaaabaaaaaaadaaaaaabkaabaaaaaaaaaaa
dcaaaaalccaabaaaaaaaaaaackaabaiaebaaaaaaabaaaaaadkiacaaaabaaaaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaaakaabaiaebaaaaaa
amaaaaaadkiacaaaabaaaaaaacaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaa
anaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaajpcaabaaaaoaaaaaa
egibcaaaabaaaaaaaaaaaaaabgiecaaaabaaaaaaacaaaaaadiaaaaaigcaabaaa
aaaaaaaakgalbaaaaoaaaaaapgiocaaaabaaaaaaadaaaaaadiaaaaajpcaabaaa
apaaaaaabgiecaaaabaaaaaaaaaaaaaaegibcaaaabaaaaaaadaaaaaadcaaaaak
ccaabaaaaaaaaaaackaabaaaapaaaaaadkiacaaaabaaaaaaacaaaaaabkaabaaa
aaaaaaaadcaaaaakecaabaaaaaaaaaaackaabaaaajaaaaaackiacaaaabaaaaaa
aaaaaaaackaabaaaaaaaaaaadcaaaaakecaabaaaaaaaaaaadkaabaaaapaaaaaa
ckiacaaaabaaaaaaacaaaaaackaabaaaaaaaaaaadcaaaaalecaabaaaaaaaaaaa
ckaabaiaebaaaaaaapaaaaaackiacaaaabaaaaaaacaaaaaackaabaaaaaaaaaaa
dcaaaaalecaabaaaaaaaaaaackaabaiaebaaaaaaaoaaaaaackiacaaaabaaaaaa
adaaaaaackaabaaaaaaaaaaadcaaaaalecaabaaaaaaaaaaackaabaiaebaaaaaa
aeaaaaaackiacaaaabaaaaaaaaaaaaaackaabaaaaaaaaaaadiaaaaahicaabaaa
anaaaaaackaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaakccaabaaaaaaaaaaa
ckaabaaaaeaaaaaadkiacaaaabaaaaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaai
ecaabaaaaaaaaaaaakaabaaaaeaaaaaadkiacaaaabaaaaaaaaaaaaaadcaaaaak
ecaabaaaaaaaaaaadkaabaaaaiaaaaaadkiacaaaabaaaaaaadaaaaaackaabaaa
aaaaaaaadcaaaaakecaabaaaaaaaaaaadkaabaaaamaaaaaadkiacaaaabaaaaaa
acaaaaaackaabaaaaaaaaaaadcaaaaalecaabaaaaaaaaaaackaabaiaebaaaaaa
amaaaaaadkiacaaaabaaaaaaacaaaaaackaabaaaaaaaaaaadcaaaaalecaabaaa
aaaaaaaackaabaiaebaaaaaaaiaaaaaadkiacaaaabaaaaaaadaaaaaackaabaaa
aaaaaaaadcaaaaalecaabaaaaaaaaaaadkaabaiaebaaaaaaaeaaaaaadkiacaaa
abaaaaaaaaaaaaaackaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaadkaabaia
ebaaaaaaaoaaaaaadkiacaaaabaaaaaaadaaaaaabkaabaaaaaaaaaaadcaaaaal
ccaabaaaaaaaaaaackaabaiaebaaaaaaajaaaaaadkiacaaaabaaaaaaaaaaaaaa
bkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaadkaabaiaebaaaaaaapaaaaaa
dkiacaaaabaaaaaaacaaaaaabkaabaaaaaaaaaaadiaaaaahgcaabaaaanaaaaaa
kgajbaaaaaaaaaaaagaabaaaaaaaaaaabbaaaaahcccabaaaacaaaaaaegaobaaa
anaaaaaaegapbaaaabaaaaaabaaaaaahbccabaaaacaaaaaaegadbaaaanaaaaaa
egadbaaaabaaaaaadgaaaaafeccabaaaacaaaaaabkaabaaaafaaaaaadiaaaaai
ccaabaaaaaaaaaaaakaabaaaalaaaaaadkiacaaaabaaaaaaaaaaaaaadiaaaaaj
pcaabaaaaeaaaaaabgiecaaaabaaaaaaaaaaaaaaegibcaaaabaaaaaaabaaaaaa
dcaaaaakccaabaaaaaaaaaaackaabaaaaeaaaaaadkiacaaaabaaaaaaadaaaaaa
bkaabaaaaaaaaaaadcaaaaakccaabaaaaaaaaaaaakaabaaaapaaaaaadkiacaaa
abaaaaaaabaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaabkaabaia
ebaaaaaaapaaaaaadkiacaaaabaaaaaaabaaaaaabkaabaaaaaaaaaaadcaaaaal
ccaabaaaaaaaaaaadkaabaiaebaaaaaaaeaaaaaadkiacaaaabaaaaaaadaaaaaa
bkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaabkaabaiaebaaaaaaalaaaaaa
dkiacaaaabaaaaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahecaabaaaajaaaaaa
bkaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaaakaabaaa
agaaaaaadkiacaaaabaaaaaaaaaaaaaadiaaaaajpcaabaaaanaaaaaaggiicaaa
abaaaaaaaaaaaaaajgiccaaaabaaaaaaabaaaaaadcaaaaakccaabaaaaaaaaaaa
bkaabaaaanaaaaaadkiacaaaabaaaaaaadaaaaaabkaabaaaaaaaaaaadcaaaaak
ccaabaaaaaaaaaaaakaabaaaamaaaaaadkiacaaaabaaaaaaabaaaaaabkaabaaa
aaaaaaaadcaaaaalccaabaaaaaaaaaaabkaabaiaebaaaaaaamaaaaaadkiacaaa
abaaaaaaabaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaaakaabaia
ebaaaaaaanaaaaaadkiacaaaabaaaaaaadaaaaaabkaabaaaaaaaaaaadcaaaaal
ccaabaaaaaaaaaaabkaabaiaebaaaaaaagaaaaaadkiacaaaabaaaaaaaaaaaaaa
bkaabaaaaaaaaaaadiaaaaahbcaabaaaajaaaaaabkaabaaaaaaaaaaaakaabaaa
aaaaaaaadiaaaaaiccaabaaaaaaaaaaadkaabaaaanaaaaaadkiacaaaabaaaaaa
adaaaaaadcaaaaakccaabaaaaaaaaaaackaabaaaamaaaaaadkiacaaaabaaaaaa
abaaaaaabkaabaaaaaaaaaaadcaaaaakccaabaaaaaaaaaaackaabaaaagaaaaaa
dkiacaaaabaaaaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaa
ckaabaiaebaaaaaaanaaaaaadkiacaaaabaaaaaaadaaaaaabkaabaaaaaaaaaaa
dcaaaaalccaabaaaaaaaaaaadkaabaiaebaaaaaaagaaaaaadkiacaaaabaaaaaa
aaaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaadkaabaiaebaaaaaa
amaaaaaadkiacaaaabaaaaaaabaaaaaabkaabaaaaaaaaaaadiaaaaahccaabaaa
ajaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaa
dkaabaaaaeaaaaaackiacaaaabaaaaaaadaaaaaadcaaaaakccaabaaaaaaaaaaa
ckaabaaaapaaaaaackiacaaaabaaaaaaabaaaaaabkaabaaaaaaaaaaadcaaaaak
ccaabaaaaaaaaaaackaabaaaalaaaaaackiacaaaabaaaaaaaaaaaaaabkaabaaa
aaaaaaaadcaaaaalccaabaaaaaaaaaaackaabaiaebaaaaaaaeaaaaaackiacaaa
abaaaaaaadaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaadkaabaia
ebaaaaaaalaaaaaackiacaaaabaaaaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaal
ccaabaaaaaaaaaaadkaabaiaebaaaaaaapaaaaaackiacaaaabaaaaaaabaaaaaa
bkaabaaaaaaaaaaadiaaaaahicaabaaaajaaaaaabkaabaaaaaaaaaaaakaabaaa
aaaaaaaabbaaaaahcccabaaaadaaaaaaegaobaaaajaaaaaaegapbaaaabaaaaaa
baaaaaahbccabaaaadaaaaaaegadbaaaajaaaaaaegadbaaaabaaaaaadgaaaaaf
eccabaaaadaaaaaackaabaaaafaaaaaadiaaaaaiccaabaaaaaaaaaaaakaabaaa
aeaaaaaadkiacaaaabaaaaaaacaaaaaadcaaaaakccaabaaaaaaaaaaaakaabaaa
aoaaaaaadkiacaaaabaaaaaaabaaaaaabkaabaaaaaaaaaaadcaaaaakccaabaaa
aaaaaaaaakaabaaaakaaaaaadkiacaaaabaaaaaaaaaaaaaabkaabaaaaaaaaaaa
dcaaaaalccaabaaaaaaaaaaabkaabaiaebaaaaaaaeaaaaaadkiacaaaabaaaaaa
acaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaabkaabaiaebaaaaaa
akaaaaaadkiacaaaabaaaaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaa
aaaaaaaabkaabaiaebaaaaaaaoaaaaaadkiacaaaabaaaaaaabaaaaaabkaabaaa
aaaaaaaadiaaaaahecaabaaaagaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaa
diaaaaaiccaabaaaaaaaaaaaakaabaaaanaaaaaadkiacaaaabaaaaaaacaaaaaa
dcaaaaakccaabaaaaaaaaaaaakaabaaaaiaaaaaadkiacaaaabaaaaaaabaaaaaa
bkaabaaaaaaaaaaadcaaaaakccaabaaaaaaaaaaaakaabaaaadaaaaaadkiacaaa
abaaaaaaaaaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaabkaabaia
ebaaaaaaanaaaaaadkiacaaaabaaaaaaacaaaaaabkaabaaaaaaaaaaadcaaaaal
ccaabaaaaaaaaaaabkaabaiaebaaaaaaadaaaaaadkiacaaaabaaaaaaaaaaaaaa
bkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaabkaabaiaebaaaaaaaiaaaaaa
dkiacaaaabaaaaaaabaaaaaabkaabaaaaaaaaaaadiaaaaahbcaabaaaagaaaaaa
bkaabaaaaaaaaaaaakaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaackaabaaa
adaaaaaadkiacaaaabaaaaaaaaaaaaaadcaaaaakccaabaaaaaaaaaaackaabaaa
anaaaaaadkiacaaaabaaaaaaacaaaaaabkaabaaaaaaaaaaadcaaaaakccaabaaa
aaaaaaaackaabaaaaiaaaaaadkiacaaaabaaaaaaabaaaaaabkaabaaaaaaaaaaa
dcaaaaalccaabaaaaaaaaaaadkaabaiaebaaaaaaaiaaaaaadkiacaaaabaaaaaa
abaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaadkaabaiaebaaaaaa
anaaaaaadkiacaaaabaaaaaaacaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaa
aaaaaaaadkaabaiaebaaaaaaadaaaaaadkiacaaaabaaaaaaaaaaaaaabkaabaaa
aaaaaaaadiaaaaahccaabaaaagaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaa
diaaaaaiccaabaaaaaaaaaaackaabaaaakaaaaaackiacaaaabaaaaaaaaaaaaaa
dcaaaaakccaabaaaaaaaaaaackaabaaaaeaaaaaackiacaaaabaaaaaaacaaaaaa
bkaabaaaaaaaaaaadcaaaaakccaabaaaaaaaaaaackaabaaaaoaaaaaackiacaaa
abaaaaaaabaaaaaabkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaadkaabaia
ebaaaaaaaoaaaaaackiacaaaabaaaaaaabaaaaaabkaabaaaaaaaaaaadcaaaaal
ccaabaaaaaaaaaaadkaabaiaebaaaaaaaeaaaaaackiacaaaabaaaaaaacaaaaaa
bkaabaaaaaaaaaaadcaaaaalccaabaaaaaaaaaaadkaabaiaebaaaaaaakaaaaaa
ckiacaaaabaaaaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaahicaabaaaagaaaaaa
bkaabaaaaaaaaaaaakaabaaaaaaaaaaabbaaaaahcccabaaaaeaaaaaaegaobaaa
agaaaaaaegapbaaaabaaaaaabaaaaaahbccabaaaaeaaaaaaegadbaaaagaaaaaa
egadbaaaabaaaaaaaaaaaaaihcaabaaaaaaaaaaaegacbaiaebaaaaaaafaaaaaa
egacbaaaahaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaaaacaaaaaaegacbaia
ebaaaaaaafaaaaaadcaaaaalhcaabaaaabaaaaaaegacbaiaebaaaaaaabaaaaaa
agiacaaaaaaaaaaaahaaaaaaegacbaaaacaaaaaadcaaaaalhcaabaaaacaaaaaa
egacbaiaebaaaaaaaaaaaaaaagiacaaaaaaaaaaaahaaaaaaegacbaaaahaaaaaa
diaaaaahdcaabaaaaaaaaaaaegaabaaaaaaaaaaaegaabaaaaaaaaaaaaaaaaaah
bcaabaaaaaaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaajbcaabaaa
aaaaaaaackaabaaaaaaaaaaackaabaaaaaaaaaaaakaabaaaaaaaaaaaaaaaaaai
eccabaaaaeaaaaaaakaabaiaebaaaaaaaaaaaaaadkaabaaaaaaaaaaadgaaaaag
iccabaaaaeaaaaaaakiacaaaaaaaaaaaacaaaaaadgaaaaafbccabaaaafaaaaaa
akaabaaaacaaaaaadgaaaaafcccabaaaafaaaaaaakaabaaaabaaaaaadgaaaaag
eccabaaaafaaaaaaakiacaaaaaaaaaaaahaaaaaadgaaaaaficaabaaaacaaaaaa
bkaabaaaabaaaaaadgaaaaafdccabaaaagaaaaaangafbaaaacaaaaaadgaaaaaf
icaabaaaabaaaaaackaabaaaacaaaaaadgaaaaafdccabaaaahaaaaaalgapbaaa
abaaaaaadgaaaaagdccabaaaaiaaaaaajgifcaaaaaaaaaaaacaaaaaadgaaaaag
pccabaaaajaaaaaaegiocaaaaaaaaaaaabaaaaaadoaaaaab"
}

}
Program "fp" {
// Fragment combos: 1
//   opengl - ALU: 165 to 165, TEX: 2 to 2
//   d3d9 - ALU: 157 to 157, TEX: 5 to 5
//   d3d11 - ALU: 91 to 91, TEX: 2 to 2, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
Float 8 [_EllipseFactor]
Float 9 [_Attenuation]
Float 10 [_Brightness]
SetTexture 0 [_MatCap] 2D
SetTexture 1 [_MatCap2] 2D
"3.0-!!ARBfp1.0
# 165 ALU, 2 TEX
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
TEMP R5;
TEMP R6;
TEMP R7;
TEMP R8;
MOV R0.xyz, fragment.texcoord[4];
MUL R0.xyz, fragment.texcoord[5], R0;
ADD R0.x, R0, R0.y;
ADD R3.w, R0.x, R0.z;
ADD R0.x, R3.w, -fragment.texcoord[5];
ADD R0.z, R0.x, -fragment.texcoord[5].y;
RCP R0.w, fragment.texcoord[5].z;
MOV R6.w, c[11].x;
MOV R4.z, -R3.w;
MUL R0.z, R0, R0.w;
MOV R0.xy, c[11].x;
ADD R0.xyz, R0, -fragment.texcoord[4];
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R2.xyz, R0.w, R0;
MUL R0.xyz, R2.zxyw, fragment.texcoord[5].yzxw;
MAD R0.xyz, R2.yzxw, fragment.texcoord[5].zxyw, -R0;
DP3 R0.w, R0, R0;
RSQ R0.w, R0.w;
MUL R0.xyz, R0.w, R0;
MUL R3.xyz, fragment.texcoord[4], -R0;
ADD R0.w, R3.x, R3.y;
ADD R1.w, R0, R3.z;
MUL R1.xyz, -R2, fragment.texcoord[4];
RCP R3.x, fragment.texcoord[1].x;
ADD R0.w, R1.x, R1.y;
ADD R4.x, R0.w, R1.z;
ADD R0.w, -fragment.texcoord[1].x, c[11].x;
RCP R0.w, R0.w;
MOV R3.z, -R0.w;
MOV R2.w, R4.x;
MOV R4.y, R1.w;
MUL R3.y, R3.x, c[8].x;
MUL R1.xyz, R3, R4;
MUL R1.xyz, R1, R4;
ADD R0.w, R1.x, R1.y;
ADD R0.w, R0, R1.z;
ADD R5.w, -fragment.texcoord[4], R0;
MOV R0.w, R1;
MOV R1.y, R0.x;
MOV R1.x, R2;
MOV R1.z, fragment.texcoord[5].x;
MUL R5.xyz, R3, R1;
MUL R1, R5.y, R0;
MAD R4, R5.x, R2, R1;
MOV R1.w, -R3;
MOV R1.xyz, fragment.texcoord[5];
MAD R7, R5.z, R1, R4;
MOV R4.w, c[11].y;
MOV R5.x, R7.w;
MOV R6.y, R0.z;
MOV R6.x, R2.z;
MOV R4.y, R0;
MOV R4.x, R2.y;
MOV R4.z, fragment.texcoord[5].y;
MUL R4.xyz, R3, R4;
MOV R6.z, fragment.texcoord[5];
MUL R3.xyz, R3, R6;
MUL R8.xyz, R0.yzww, R4.y;
MAD R6.xyz, R2.yzww, R4.x, R8;
MUL R2.xy, R0.zwzw, R3.y;
MAD R0.xyz, R1.yzww, R4.z, R6;
MAD R1.xy, R2.zwzw, R3.x, R2;
MAD R1.xy, R1.zwzw, R3.z, R1;
RCP R0.w, fragment.texcoord[2].w;
MUL R6.xyz, fragment.texcoord[2], R0.w;
MOV R2.yzw, R0.xxyz;
RCP R0.w, fragment.texcoord[3].w;
MAD R4.xyz, fragment.texcoord[3], R0.w, -R6;
MOV R3.zw, R1.xyxy;
MOV R5.y, R0.z;
MOV R5.z, R1.y;
MOV R2.x, R7.y;
MOV R3.y, R0;
MOV R3.x, R7.z;
DP4 R1.y, R4, R2;
DP4 R1.w, R5, R4;
DP4 R1.z, R4, R3;
DP4 R1.x, R7, R4;
DP4 R8.x, R6, R1;
DP4 R0.y, R6, R2;
DP4 R0.w, R6, R5;
DP4 R0.z, R6, R3;
DP4 R0.x, R6, R7;
DP4 R0.y, R6, R0;
DP4 R0.x, R1, R4;
MOV R0.w, c[11].x;
MUL R6.w, R0.x, R0.y;
MUL R4.w, R8.x, R8.x;
ADD R0.y, R4.w, -R6.w;
RSQ R0.y, R0.y;
RCP R0.z, R0.x;
RCP R0.y, R0.y;
ADD R0.x, -R8, -R0.y;
MUL R0.x, R0, R0.z;
MAD R0.xyz, R0.x, R4, R6;
DP4 R1.y, R0, R2;
DP4 R1.w, R0, R5;
DP4 R1.z, R0, R3;
DP4 R1.x, R0, R7;
DP4 R2.z, R1, c[6];
DP4 R2.x, R1, c[4];
DP4 R2.y, R1, c[5];
DP3 R1.x, R2, R2;
RSQ R1.x, R1.x;
MUL R5.xyz, R1.x, R2;
ADD R2.xyz, R0, -fragment.texcoord[7];
MUL R4.xyz, fragment.texcoord[5], R2;
MAD R1.xy, R5, c[11].w, c[11].w;
ADD R4.x, R4, R4.y;
TEX R3, R1, texture[0], 2D;
TEX R1, R1, texture[1], 2D;
MOV R2.xyz, fragment.texcoord[7];
ADD R2.xyz, fragment.texcoord[6], -R2;
DP3 R5.x, R2, R2;
MOV R2, fragment.texcoord[3];
ADD R2, fragment.texcoord[2], -R2;
DP4 R2.w, R2, R2;
RSQ R2.w, R2.w;
ADD R3, R3, -R1;
MOV R2.x, fragment.texcoord[5].w;
MOV R2.z, fragment.texcoord[7].w;
MOV R2.y, fragment.texcoord[6].w;
RCP R2.w, R2.w;
ADD R4.y, R4.x, R4.z;
RSQ R5.x, R5.x;
MUL R4.y, R5.x, R4;
MAD R1, R4.y, R3, R1;
ADD R3.xyz, fragment.texcoord[0], -R2;
MAD R2.xyz, R4.y, R3, R2;
MAD R3.x, R2.w, c[12].y, c[12].z;
MOV R3.w, c[11].x;
MUL R2.xyz, R5.z, R2;
MOV R2.w, fragment.texcoord[0];
MUL R1, R2, R1;
RCP R2.x, R3.x;
ADD R3.xyz, R0, -fragment.texcoord[6];
MUL R3.xyz, -fragment.texcoord[5], R3;
MUL R1, R1, c[10].x;
MUL R1, R1, c[12].x;
MIN R2.x, R2, c[11];
MUL R2, R1, R2.x;
ADD R3.y, R3.x, R3;
ADD R3.w, -R3, c[9].x;
ABS R3.x, R3.w;
CMP result.color, -R3.x, R1, R2;
SLT R1.x, R3.y, -R3.z;
SLT R1.y, R4.x, -R4.z;
ABS R1.y, R1;
CMP R1.z, -R1.y, c[11].y, c[11].x;
ABS R1.x, R1;
CMP R1.x, -R1, c[11].y, c[11];
CMP R1.y, -R1.x, c[11], c[11].x;
DP4 R1.x, R0, c[3];
DP4 R0.x, R0, c[2];
CMP R1.z, -R1, c[11].y, c[11].x;
ADD_SAT R1.y, R1, R1.z;
MUL R1.x, R1, c[11].z;
RCP R1.x, R1.x;
MAD result.depth.z, R0.x, R1.x, c[11].w; // hack for D3D: depth may need to be unmasked
SLT R0.y, R4.w, R6.w;
SLT R0.x, fragment.texcoord[1], c[11].y;
KIL -R1.y;
KIL -R0.y;
KIL -R0.x;
END
# 165 instructions, 9 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Matrix 0 [glstate_matrix_mvp]
Matrix 4 [glstate_matrix_invtrans_modelview0]
Float 8 [_EllipseFactor]
Float 9 [_Attenuation]
Float 10 [_Brightness]
SetTexture 0 [_MatCap] 2D
SetTexture 1 [_MatCap2] 2D
"ps_3_0
; 157 ALU, 5 TEX
dcl_2d s0
dcl_2d s1
def c11, 1.00000000, 0.00000000, -1.00000000, 0.50000000
def c12, 1.25000000, 0.00010000, 0.00001000, 2.00000000
dcl_texcoord2 v0
dcl_texcoord3 v1
dcl_texcoord4 v2
dcl_texcoord5 v3
dcl_texcoord6 v4
dcl_texcoord7 v5
dcl_texcoord0 v6
dcl_texcoord1 v7.x
mov r0.xyz, v2
mul r0.xyz, v3, r0
add r0.x, r0, r0.y
add r3.w, r0.x, r0.z
add r0.x, r3.w, -v3
add r0.z, r0.x, -v3.y
rcp r0.w, v3.z
mov r5.z, -r3.w
mul r0.z, r0, r0.w
mov r0.xy, c11.x
add r0.xyz, r0, -v2
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r2.xyz, r0.w, r0
mul r0.xyz, r2.zxyw, v3.yzxw
mad r0.xyz, r2.yzxw, v3.zxyw, -r0
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r1.xyz, r0.w, r0
mul r3.xyz, v2, -r1
add r0.w, r3.x, r3.y
add r0.w, r0, r3.z
mul r0.xyz, -r2, v2
rcp r3.x, v7.x
add r0.x, r0, r0.y
add r5.x, r0, r0.z
add r0.x, -v7, c11
rcp r0.x, r0.x
mov r3.z, -r0.x
mov r1.w, r0
mov r2.w, r5.x
mov r5.y, r0.w
mul r3.y, r3.x, c8.x
mul r0.xyz, r3, r5
mul r0.xyz, r0, r5
add r0.x, r0, r0.y
add r0.x, r0, r0.z
add r4.w, -v2, r0.x
mov r0.y, r1.x
mov r0.x, r2
mov r0.z, v3.x
mul r4.xyz, r3, r0
mul r0, r4.y, r1
mad r5, r4.x, r2, r0
mov r0.w, -r3
mov r0.xyz, v3
mad r6, r4.z, r0, r5
mov r5.w, c11.x
mov r3.w, c11.y
mov r4.y, r1
mov r4.x, r2.y
mov r4.z, v3.y
mul r5.xyz, r3, r4
mov r4.x, r6.w
mul r8.xyz, r1.yzww, r5.y
mov r7.y, r1.z
mov r7.x, r2.z
mov r7.z, v3
mul r3.xyz, r3, r7
mad r7.xyz, r2.yzww, r5.x, r8
mul r2.xy, r1.zwzw, r3.y
mad r1.xyz, r0.yzww, r5.z, r7
mad r0.xy, r2.zwzw, r3.x, r2
mad r0.xy, r0.zwzw, r3.z, r0
rcp r0.z, v0.w
mul r5.xyz, v0, r0.z
mov r2.zw, r0.xyxy
mov r4.y, r1.z
mov r4.z, r0.y
mov r2.y, r1
mov r1.yzw, r1.xxyz
mov r2.x, r6.z
mov r1.x, r6.y
dp4 r0.y, r5, r1
dp4 r0.z, r5, r2
dp4 r0.w, r5, r4
dp4 r0.x, r5, r6
dp4 r7.x, r5, r0
rcp r0.x, v1.w
mad r3.xyz, v1, r0.x, -r5
dp4 r0.y, r3, r1
dp4 r0.z, r3, r2
dp4 r0.w, r4, r3
dp4 r0.x, r6, r3
dp4 r3.w, r0, r3
dp4 r0.x, r5, r0
mul r7.x, r3.w, r7
mad r5.w, r0.x, r0.x, -r7.x
rsq r0.y, r5.w
rcp r0.y, r0.y
mov r0.w, c11.x
rcp r0.z, r3.w
add r0.x, -r0, -r0.y
mul r0.x, r0, r0.z
mad r0.xyz, r3, r0.x, r5
dp4 r3.z, r2, r0
dp4 r3.y, r1, r0
dp4 r3.w, r4, r0
dp4 r3.x, r6, r0
add r2.xyz, r0, -v5
mul r4.xyz, v3, r2
add r2.w, r4.x, r4.y
mov r2.xyz, v5
add r2.xyz, v4, -r2
dp3 r4.y, r2, r2
add r4.x, r2.w, r4.z
mov r2, v1
add r2, v0, -r2
dp4 r2.w, r2, r2
rsq r4.y, r4.y
rsq r2.w, r2.w
dp4 r1.z, r3, c6
dp4 r1.x, r3, c4
dp4 r1.y, r3, c5
dp3 r1.w, r1, r1
rsq r1.w, r1.w
mul r5.xyz, r1.w, r1
mad_pp r1.xy, r5, c11.w, c11.w
texld r3, r1, s0
texld r1, r1, s1
add r3, r3, -r1
mul r4.y, r4, r4.x
mad r1, r4.y, r3, r1
mov r3.w, c9.x
mov r2.x, v3.w
mov r2.z, v5.w
mov r2.y, v4.w
add r3.xyz, v6, -r2
mad r2.xyz, r4.y, r3, r2
rcp r2.w, r2.w
mad r3.x, r2.w, c12.y, c12.z
mul r2.xyz, r5.z, r2
mov r2.w, v6
mul r1, r2, r1
rcp r2.x, r3.x
add r3.xyz, r0, -v4
mul r3.xyz, -v3, r3
mul r1, r1, c10.x
mul r1, r1, c12.x
min r2.x, r2, c11
mul r2, r1, r2.x
add r3.x, r3, r3.y
add r3.w, c11.z, r3
abs r3.y, r3.w
cmp oC0, -r3.y, r2, r1
dp4 r2.x, r0, c3
add r1.x, r3, r3.z
dp4 r0.x, r0, c2
cmp_pp r1.y, r4.x, c11, c11.x
cmp_pp r1.x, r1, c11.y, c11
add_pp_sat r1.x, r1, r1.y
mov_pp r1, -r1.x
texkill r1.xyzw
mul r2.x, r2, c12.w
rcp r1.x, r2.x
mad oDepth, r0.x, r1.x, c11.w
cmp r0.x, r5.w, c11.y, c11
cmp r1.x, v7, c11.y, c11
mov_pp r0, -r0.x
mov_pp r1, -r1.x
texkill r0.xyzw
texkill r1.xyzw
"
}

SubProgram "d3d11 " {
Keywords { }
ConstBuffer "$Globals" 144 // 132 used size, 13 vars
Float 120 [_EllipseFactor]
Float 124 [_Attenuation]
Float 128 [_Brightness]
ConstBuffer "UnityPerDraw" 336 // 192 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
Matrix 128 [glstate_matrix_invtrans_modelview0] 4
BindCB "$Globals" 0
BindCB "UnityPerDraw" 1
SetTexture 0 [_MatCap] 2D 0
SetTexture 1 [_MatCap2] 2D 1
// 168 instructions, 11 temp regs, 0 temp arrays:
// ALU 90 float, 0 int, 1 uint
// TEX 2 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"ps_4_0
eefiecedbcbmkgmicnmcomgbomlgpdoigaglmcncabaaaaaaoibeaaaaadaaaaaa
cmaaaaaafmabaaaalaabaaaaejfdeheociabaaaaalaaaaaaaiaaaaaabaabaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaabmabaaaaacaaaaaaaaaaaaaa
adaaaaaaabaaaaaaapapaaaabmabaaaaadaaaaaaaaaaaaaaadaaaaaaacaaaaaa
apapaaaabmabaaaaaeaaaaaaaaaaaaaaadaaaaaaadaaaaaaapapaaaabmabaaaa
afaaaaaaaaaaaaaaadaaaaaaaeaaaaaaapapaaaabmabaaaaagaaaaaaaaaaaaaa
adaaaaaaafaaaaaaadadaaaabmabaaaaabaaaaaaaaaaaaaaadaaaaaaafaaaaaa
aeaeaaaabmabaaaaahaaaaaaaaaaaaaaadaaaaaaagaaaaaaadadaaaabmabaaaa
aiaaaaaaaaaaaaaaadaaaaaaahaaaaaaadadaaaabmabaaaaajaaaaaaaaaaaaaa
adaaaaaaaiaaaaaaadadaaaabmabaaaaaaaaaaaaaaaaaaaaadaaaaaaajaaaaaa
apapaaaafdfgfpfagphdgjhegjgpgoaafeeffiedepepfceeaaklklklepfdeheo
emaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaaecaaaaaaaaaaaaaaaaaaaaaaadaaaaaappppppppabaoaaaafdfgfpfe
gbhcghgfheaafdfgfpeegfhahegiaaklfdeieefcdabdaaaaeaaaaaaammaeaaaa
fjaaaaaeegiocaaaaaaaaaaaajaaaaaafjaaaaaeegiocaaaabaaaaaaamaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafibiaaaeaahabaaa
aaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaagcbaaaadpcbabaaa
abaaaaaagcbaaaadpcbabaaaacaaaaaagcbaaaadpcbabaaaadaaaaaagcbaaaad
pcbabaaaaeaaaaaagcbaaaaddcbabaaaafaaaaaagcbaaaadecbabaaaafaaaaaa
gcbaaaaddcbabaaaagaaaaaagcbaaaaddcbabaaaahaaaaaagcbaaaaddcbabaaa
aiaaaaaagcbaaaadpcbabaaaajaaaaaagfaaaaadpccabaaaaaaaaaaagfaaaaac
abmaaaaagiaaaaacalaaaaaadgaaaaafbcaabaaaaaaaaaaadkbabaaaabaaaaaa
dgaaaaafccaabaaaaaaaaaaadkbabaaaacaaaaaadgaaaaafecaabaaaaaaaaaaa
dkbabaaaadaaaaaadgaaaaafbcaabaaaabaaaaaackbabaaaabaaaaaadgaaaaaf
ecaabaaaabaaaaaackbabaaaacaaaaaadgaaaaaficaabaaaabaaaaaackbabaaa
adaaaaaadiaaaaahdcaabaaaacaaaaaaegaabaaaaaaaaaaaigaabaaaabaaaaaa
aaaaaaahbcaabaaaacaaaaaabkaabaaaacaaaaaaakaabaaaacaaaaaadcaaaaaj
bcaabaaaacaaaaaackaabaaaaaaaaaaadkaabaaaabaaaaaaakaabaaaacaaaaaa
dgaaaaafdcaabaaaabaaaaaalgbpbaaaabaaaaaaaaaaaaaibcaabaaaabaaaaaa
akaabaiaebaaaaaaabaaaaaaakaabaaaacaaaaaaaaaaaaaibcaabaaaabaaaaaa
akaabaaaabaaaaaadkbabaiaebaaaaaaacaaaaaaaoaaaaahecaabaaaacaaaaaa
akaabaaaabaaaaaadkbabaaaadaaaaaadgaaaaaidcaabaaaacaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaaaaaaaaaaaaaaaaaaaaihcaabaaaacaaaaaajgahbaia
ebaaaaaaabaaaaaaegacbaaaacaaaaaabaaaaaahbcaabaaaabaaaaaaegacbaaa
acaaaaaaegacbaaaacaaaaaaeeaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaa
diaaaaahhcaabaaaacaaaaaaagaabaaaabaaaaaaegacbaaaacaaaaaadiaaaaah
hcaabaaaadaaaaaajgaebaaaaaaaaaaacgajbaaaacaaaaaadcaaaaakhcaabaaa
adaaaaaajgaebaaaacaaaaaacgajbaaaaaaaaaaaegacbaiaebaaaaaaadaaaaaa
baaaaaahbcaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaaadaaaaaaeeaaaaaf
bcaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaaagaabaaa
abaaaaaaegacbaaaadaaaaaaaaaaaaaibcaabaaaabaaaaaackbabaiaebaaaaaa
afaaaaaaabeaaaaaaaaaiadpaoaaaaahecaabaaaaeaaaaaaabeaaaaaaaaaialp
akaabaaaabaaaaaaaoaaaaakbcaabaaaaeaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpckbabaaaafaaaaaadiaaaaaiccaabaaaaeaaaaaaakaabaaa
aeaaaaaackiacaaaaaaaaaaaahaaaaaadiaaaaaidcaabaaaafaaaaaajgafbaaa
abaaaaaaegaabaiaebaaaaaaacaaaaaaaaaaaaahbcaabaaaabaaaaaabkaabaaa
afaaaaaaakaabaaaafaaaaaadcaaaaakbcaabaaaafaaaaaackaabaiaebaaaaaa
acaaaaaadkaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaaidcaabaaaagaaaaaa
jgafbaaaabaaaaaaegaabaiaebaaaaaaadaaaaaaaaaaaaahbcaabaaaabaaaaaa
bkaabaaaagaaaaaaakaabaaaagaaaaaadcaaaaakccaabaaaafaaaaaackaabaia
ebaaaaaaadaaaaaadkaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaaidcaabaaa
abaaaaaaegaabaiaebaaaaaaaaaaaaaajgafbaaaabaaaaaaaaaaaaahbcaabaaa
abaaaaaabkaabaaaabaaaaaaakaabaaaabaaaaaadcaaaaakecaabaaaafaaaaaa
ckaabaiaebaaaaaaaaaaaaaadkaabaaaabaaaaaaakaabaaaabaaaaaadgaaaaaf
bcaabaaaabaaaaaaakaabaaaacaaaaaadgaaaaafccaabaaaabaaaaaaakaabaaa
adaaaaaadgaaaaafecaabaaaabaaaaaadkbabaaaabaaaaaadiaaaaahhcaabaaa
abaaaaaaegacbaaaabaaaaaaegacbaaaaeaaaaaadgaaaaafbcaabaaaagaaaaaa
bkaabaaaacaaaaaadgaaaaafccaabaaaagaaaaaabkaabaaaadaaaaaadgaaaaaf
ecaabaaaagaaaaaadkbabaaaacaaaaaadiaaaaahhcaabaaaagaaaaaaegacbaaa
aeaaaaaaegacbaaaagaaaaaadgaaaaafbcaabaaaahaaaaaackaabaaaacaaaaaa
dgaaaaafccaabaaaahaaaaaackaabaaaadaaaaaadgaaaaafecaabaaaahaaaaaa
dkbabaaaadaaaaaadiaaaaahhcaabaaaahaaaaaaegacbaaaaeaaaaaaegacbaaa
ahaaaaaadiaaaaahhcaabaaaaiaaaaaaegacbaaaafaaaaaaegacbaaaafaaaaaa
diaaaaahdcaabaaaaeaaaaaaegaabaaaaeaaaaaaegaabaaaaiaaaaaadgaaaaaf
icaabaaaacaaaaaaakaabaaaafaaaaaadgaaaaaficaabaaaadaaaaaabkaabaaa
afaaaaaadiaaaaahpcaabaaaajaaaaaafgafbaaaabaaaaaaegaobaaaadaaaaaa
dcaaaaajpcaabaaaajaaaaaaagaabaaaabaaaaaaegaobaaaacaaaaaaegaobaaa
ajaaaaaadgaaaaaficaabaaaaaaaaaaackaabaaaafaaaaaadcaaaaajpcaabaaa
abaaaaaakgakbaaaabaaaaaaegaobaaaaaaaaaaaegaobaaaajaaaaaadiaaaaah
hcaabaaaafaaaaaajgahbaaaadaaaaaafgafbaaaagaaaaaadcaaaaajhcaabaaa
afaaaaaaagaabaaaagaaaaaajgahbaaaacaaaaaaegacbaaaafaaaaaadcaaaaaj
ocaabaaaafaaaaaakgakbaaaagaaaaaafgaobaaaaaaaaaaaagajbaaaafaaaaaa
diaaaaahdcaabaaaacaaaaaaogakbaaaadaaaaaafgafbaaaahaaaaaadcaaaaaj
dcaabaaaacaaaaaaagaabaaaahaaaaaaogakbaaaacaaaaaaegaabaaaacaaaaaa
dcaaaaajmcaabaaaacaaaaaakgakbaaaahaaaaaakgaobaaaaaaaaaaaagaebaaa
acaaaaaaaaaaaaahicaabaaaaaaaaaaabkaabaaaaeaaaaaaakaabaaaaeaaaaaa
dcaaaaajicaabaaaaaaaaaaackaabaaaaiaaaaaackaabaaaaeaaaaaadkaabaaa
aaaaaaaaaaaaaaaiicaabaaaadaaaaaadkaabaaaaaaaaaaackbabaiaebaaaaaa
aeaaaaaadgaaaaafbcaabaaaaeaaaaaaakbabaaaabaaaaaadgaaaaafccaabaaa
aeaaaaaaakbabaaaacaaaaaadgaaaaafecaabaaaaeaaaaaaakbabaaaadaaaaaa
aoaaaaahhcaabaaaagaaaaaaegacbaaaaeaaaaaaagbabaaaaeaaaaaadgaaaaaf
bcaabaaaahaaaaaabkbabaaaabaaaaaadgaaaaafccaabaaaahaaaaaabkbabaaa
acaaaaaadgaaaaafecaabaaaahaaaaaabkbabaaaadaaaaaaaoaaaaahhcaabaaa
aiaaaaaaegacbaaaahaaaaaafgbfbaaaaeaaaaaaaaaaaaaihcaabaaaaiaaaaaa
egacbaiaebaaaaaaagaaaaaaegacbaaaaiaaaaaabaaaaaahbcaabaaaajaaaaaa
egacbaaaabaaaaaaegacbaaaaiaaaaaadgaaaaafbcaabaaaafaaaaaabkaabaaa
abaaaaaabaaaaaahccaabaaaajaaaaaaegacbaaaafaaaaaaegacbaaaaiaaaaaa
dgaaaaafbcaabaaaacaaaaaackaabaaaabaaaaaadgaaaaafccaabaaaacaaaaaa
ckaabaaaafaaaaaabaaaaaahecaabaaaajaaaaaaegacbaaaacaaaaaaegacbaaa
aiaaaaaadgaaaaafbcaabaaaadaaaaaadkaabaaaabaaaaaadgaaaaafccaabaaa
adaaaaaadkaabaaaafaaaaaadgaaaaafecaabaaaadaaaaaadkaabaaaacaaaaaa
baaaaaahicaabaaaajaaaaaaegacbaaaadaaaaaaegacbaaaaiaaaaaabaaaaaah
icaabaaaaaaaaaaaegacbaaaaiaaaaaaegacbaaaajaaaaaadgaaaaaficaabaaa
agaaaaaaabeaaaaaaaaaiadpbbaaaaahicaabaaaaiaaaaaaegaobaaaagaaaaaa
egaobaaaajaaaaaabbaaaaahbcaabaaaajaaaaaaegaobaaaabaaaaaaegaobaaa
agaaaaaabbaaaaahccaabaaaajaaaaaaegaobaaaafaaaaaaegaobaaaagaaaaaa
bbaaaaahecaabaaaajaaaaaaegaobaaaacaaaaaaegaobaaaagaaaaaabbaaaaah
icaabaaaajaaaaaaegaobaaaadaaaaaaegaobaaaagaaaaaabbaaaaahicaabaaa
agaaaaaaegaobaaaagaaaaaaegaobaaaajaaaaaadiaaaaahicaabaaaagaaaaaa
dkaabaaaaaaaaaaadkaabaaaagaaaaaadcaaaaakicaabaaaagaaaaaadkaabaaa
aiaaaaaadkaabaaaaiaaaaaadkaabaiaebaaaaaaagaaaaaadbaaaaahbcaabaaa
ajaaaaaadkaabaaaagaaaaaaabeaaaaaaaaaaaaaanaaaeadakaabaaaajaaaaaa
dbaaaaahbcaabaaaajaaaaaackbabaaaafaaaaaaabeaaaaaaaaaaaaaanaaaead
akaabaaaajaaaaaaelaaaaaficaabaaaagaaaaaadkaabaaaagaaaaaaaaaaaaaj
icaabaaaagaaaaaadkaabaiaebaaaaaaagaaaaaadkaabaiaebaaaaaaaiaaaaaa
aoaaaaahicaabaaaaaaaaaaadkaabaaaagaaaaaadkaabaaaaaaaaaaadcaaaaaj
hcaabaaaagaaaaaapgapbaaaaaaaaaaaegacbaaaaiaaaaaaegacbaaaagaaaaaa
dgaaaaafbcaabaaaaiaaaaaaakbabaaaafaaaaaadgaaaaafccaabaaaaiaaaaaa
akbabaaaagaaaaaadgaaaaafecaabaaaaiaaaaaaakbabaaaahaaaaaaaaaaaaai
hcaabaaaajaaaaaaegacbaaaagaaaaaaegacbaiaebaaaaaaaiaaaaaadiaaaaai
dcaabaaaajaaaaaaegaabaiaebaaaaaaaaaaaaaaegaabaaaajaaaaaaaaaaaaah
icaabaaaaaaaaaaabkaabaaaajaaaaaaakaabaaaajaaaaaadcaaaaakicaabaaa
aaaaaaaackaabaiaebaaaaaaaaaaaaaackaabaaaajaaaaaadkaabaaaaaaaaaaa
dgaaaaafbcaabaaaajaaaaaabkbabaaaafaaaaaadgaaaaafccaabaaaajaaaaaa
bkbabaaaagaaaaaadgaaaaafecaabaaaajaaaaaabkbabaaaahaaaaaaaaaaaaai
hcaabaaaakaaaaaaegacbaaaagaaaaaaegacbaiaebaaaaaaajaaaaaadiaaaaah
dcaabaaaaaaaaaaaegaabaaaaaaaaaaaegaabaaaakaaaaaaaaaaaaahbcaabaaa
aaaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaadcaaaaajbcaabaaaaaaaaaaa
ckaabaaaaaaaaaaackaabaaaakaaaaaaakaabaaaaaaaaaaadbaaaaakkcaabaaa
aaaaaaaaagambaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
dmaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaadkaabaaaaaaaaaaaanaaaead
bkaabaaaaaaaaaaadiaaaaaigcaabaaaaaaaaaaafgafbaaaagaaaaaakgilcaaa
abaaaaaaabaaaaaadcaaaaakgcaabaaaaaaaaaaakgilcaaaabaaaaaaaaaaaaaa
agaabaaaagaaaaaafgagbaaaaaaaaaaadcaaaaakgcaabaaaaaaaaaaakgilcaaa
abaaaaaaacaaaaaakgakbaaaagaaaaaafgagbaaaaaaaaaaaaaaaaaaigcaabaaa
aaaaaaaafgagbaaaaaaaaaaakgilcaaaabaaaaaaadaaaaaaaaaaaaahecaabaaa
aaaaaaaackaabaaaaaaaaaaackaabaaaaaaaaaaaaoaaaaahccaabaaaaaaaaaaa
bkaabaaaaaaaaaaackaabaaaaaaaaaaaaaaaaaagabmaaaaabkaabaaaaaaaaaaa
abeaaaaaaaaaaadpdgaaaaaficaabaaaagaaaaaaabeaaaaaaaaaiadpbbaaaaah
ccaabaaaaaaaaaaaegaobaaaabaaaaaaegaobaaaagaaaaaabbaaaaahecaabaaa
aaaaaaaaegaobaaaafaaaaaaegaobaaaagaaaaaabbaaaaahicaabaaaaaaaaaaa
egaobaaaacaaaaaaegaobaaaagaaaaaabbaaaaahbcaabaaaabaaaaaaegaobaaa
adaaaaaaegaobaaaagaaaaaadiaaaaaiocaabaaaabaaaaaakgakbaaaaaaaaaaa
agijcaaaabaaaaaaajaaaaaadcaaaaakocaabaaaabaaaaaaagijcaaaabaaaaaa
aiaaaaaafgafbaaaaaaaaaaafgaobaaaabaaaaaadcaaaaakocaabaaaaaaaaaaa
agijcaaaabaaaaaaakaaaaaapgapbaaaaaaaaaaafgaobaaaabaaaaaadcaaaaak
ocaabaaaaaaaaaaaagijcaaaabaaaaaaalaaaaaaagaabaaaabaaaaaafgaobaaa
aaaaaaaabaaaaaahbcaabaaaabaaaaaajgahbaaaaaaaaaaajgahbaaaaaaaaaaa
eeaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaadiaaaaahocaabaaaaaaaaaaa
fgaobaaaaaaaaaaaagaabaaaabaaaaaaaaaaaaaihcaabaaaabaaaaaaegacbaia
ebaaaaaaaiaaaaaaegacbaaaajaaaaaabaaaaaahbcaabaaaabaaaaaaegacbaaa
abaaaaaaegacbaaaabaaaaaaelaaaaafbcaabaaaabaaaaaaakaabaaaabaaaaaa
aoaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaaakaabaaaabaaaaaadgaaaaaf
bcaabaaaabaaaaaadkbabaaaaeaaaaaadgaaaaafgcaabaaaabaaaaaaagbbbaaa
aiaaaaaaaaaaaaaihcaabaaaacaaaaaaegacbaiaebaaaaaaabaaaaaaegbcbaaa
ajaaaaaadcaaaaajhcaabaaaabaaaaaaagaabaaaaaaaaaaaegacbaaaacaaaaaa
egacbaaaabaaaaaadiaaaaahhcaabaaaabaaaaaapgapbaaaaaaaaaaaegacbaaa
abaaaaaadcaaaaapgcaabaaaaaaaaaaafgagbaaaaaaaaaaaaceaaaaaaaaaaaaa
aaaaaadpaaaaaadpaaaaaaaaaceaaaaaaaaaaaaaaaaaaadpaaaaaadpaaaaaaaa
efaaaaajpcaabaaaacaaaaaajgafbaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaaefaaaaajpcaabaaaadaaaaaajgafbaaaaaaaaaaaeghobaaaabaaaaaa
aagabaaaabaaaaaaaaaaaaaipcaabaaaacaaaaaaegaobaaaacaaaaaaegaobaia
ebaaaaaaadaaaaaadcaaaaajpcaabaaaaaaaaaaaagaabaaaaaaaaaaaegaobaaa
acaaaaaaegaobaaaadaaaaaadgaaaaaficaabaaaabaaaaaadkbabaaaajaaaaaa
diaaaaahpcaabaaaaaaaaaaaegaobaaaaaaaaaaaegaobaaaabaaaaaadiaaaaai
pcaabaaaaaaaaaaaegaobaaaaaaaaaaaagiacaaaaaaaaaaaaiaaaaaadiaaaaak
pcaabaaaaaaaaaaaegaobaaaaaaaaaaaaceaaaaaaaaakadpaaaakadpaaaakadp
aaaakadpbiaaaaaibcaabaaaabaaaaaadkiacaaaaaaaaaaaahaaaaaaabeaaaaa
aaaaiadpdgaaaaaficaabaaaaeaaaaaaakbabaaaaeaaaaaadgaaaaaficaabaaa
ahaaaaaabkbabaaaaeaaaaaaaaaaaaaipcaabaaaacaaaaaaegaobaiaebaaaaaa
aeaaaaaaegaobaaaahaaaaaabbaaaaahccaabaaaabaaaaaaegaobaaaacaaaaaa
egaobaaaacaaaaaaelaaaaafccaabaaaabaaaaaabkaabaaaabaaaaaadcaaaaaj
ccaabaaaabaaaaaabkaabaaaabaaaaaaabeaaaaabhlhnbdiabeaaaaakmmfchdh
aoaaaaakccaabaaaabaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
bkaabaaaabaaaaaaddaaaaahccaabaaaabaaaaaabkaabaaaabaaaaaaabeaaaaa
aaaaiadpdiaaaaahpcaabaaaacaaaaaaegaobaaaaaaaaaaafgafbaaaabaaaaaa
dhaaaaajpccabaaaaaaaaaaaagaabaaaabaaaaaaegaobaaaacaaaaaaegaobaaa
aaaaaaaadoaaaaab"
}

}

#LINE 508

    }

} 
// Pour les cartes graphiques ne supportant pas nos Shaders
//Fallback "VertexLit"
}
