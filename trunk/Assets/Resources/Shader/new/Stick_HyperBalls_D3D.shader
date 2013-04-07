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
/// $Id: Stick_HyperBalls_D3D.shader 210 2013-04-06 20:52:41Z baaden $
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

// $Id: Stick_HyperBalls_D3D.shader 210 2013-04-06 20:52:41Z baaden $
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
}


// ==========================================================
// L'actuel Shader Cg =======================================
// ==========================================================

SubShader {
    Pass {

Program "vp" {
// Vertex combos: 1
//   opengl - ALU: 341 to 341
//   d3d9 - ALU: 362 to 362, FLOW: 6 to 6
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
# 341 ALU
PARAM c[14] = { { 0, 0.001, 0.5, 9.9999998e-014 },
		state.matrix.mvp,
		program.local[5..12],
		{ 2, 1 } };
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
TEMP R15;
MOV R0, c[9];
ADD R8, -R0, c[10];
MOV R0.z, c[12].x;
DP4 R0.x, -R8, -R8;
MUL R0.w, R0.z, c[5].x;
MUL R2.x, R8.y, R8.y;
RSQ R0.x, R0.x;
MUL R4.xyz, R0.x, -R8;
ABS R0.y, R4.z;
MUL R1.w, R0.z, c[6].x;
MUL R7.w, R0, R0;
MAD R0.z, R1.w, R1.w, -R7.w;
RCP R8.w, c[11].x;
MOV R6.xy, R4;
ADD R0.x, -R4.z, c[0].w;
SGE R0.y, c[0].x, R0;
MAD R2.w, R0.x, R0.y, R4.z;
MUL R0.y, R2.w, R0.z;
ABS R0.x, -R8.z;
SGE R0.x, c[0], R0;
MUL R6.w, R0.x, c[0].y;
MUL R1.x, R8.z, R8.z;
MAD R0.x, R6.w, R6.w, -R1;
MUL R0.y, R2.w, R0;
MAD R0.x, R0.y, R8.w, R0;
MUL R0.y, R4.x, R0.z;
ABS R1.x, -R8;
SGE R1.x, c[0], R1;
MUL R9.x, R1, c[0].y;
ADD R1.y, -R8.x, R9.x;
MUL R1.x, R8, R8;
MUL R1.y, R1, c[13].x;
MOV R6.z, R2.w;
MAD R3.x, -R8, -R8, R2;
MOV R3.z, R2.w;
MUL R0.y, R4.x, R0;
MAD R1.x, R9, R9, -R1;
MAD R0.y, R8.w, R0, R1.x;
RCP R1.y, R1.y;
MUL R7.x, R0.y, R1.y;
MUL R0.y, R4, R0.z;
ABS R1.x, -R8.y;
SGE R0.z, c[0].x, R1.x;
MUL R9.y, R0.z, c[0];
ADD R1.x, -R8.y, R9.y;
MUL R1.x, R1, c[13];
MUL R0.y, R4, R0;
MAD R0.z, R9.y, R9.y, -R2.x;
MAD R0.z, R8.w, R0.y, R0;
RCP R1.x, R1.x;
MUL R9.w, R0.z, R1.x;
ADD R0.y, R6.w, -R8.z;
MUL R0.z, R4.y, R9.w;
MUL R0.y, R0, c[13].x;
RCP R0.y, R0.y;
MUL R9.z, R0.x, R0.y;
MAD R0.z, R4.x, R7.x, R0;
MAD R0.x, R2.w, R9.z, R0.z;
ADD R0.x, R0, -R4;
MOV R7.z, R9;
ADD R0.x, R0, -R4.y;
RCP R0.y, R2.w;
MUL R0.z, R0.x, R0.y;
MOV R7.y, R9.w;
MOV R0.xy, c[13].y;
ADD R0.xyz, R0, -R7;
DP3 R1.x, R0, R0;
RSQ R1.x, R1.x;
MUL R0.xyz, R1.x, R0;
MUL R1.xyz, R0.zxyw, R6.yzxw;
MAD R1.xyz, R0.yzxw, R6.zxyw, -R1;
DP3 R2.y, R1, R1;
RSQ R2.y, R2.y;
MUL R2.xyz, R2.y, R1;
MAD R1.x, -R8.z, -R8.z, R3;
MOV R3.y, R2.z;
MOV R3.x, R0.z;
RSQ R1.x, R1.x;
RCP R0.z, R1.x;
MUL R1.z, vertex.position, R0;
MUL R1.x, R0.w, vertex.position.y;
MUL R2.z, R1.x, c[13].x;
MUL R1.x, R1.w, vertex.position.y;
SLT R0.z, R1.w, R0.w;
MAD R2.w, R1.x, c[13].x, -R2.z;
ABS R1.x, R0.z;
MUL R0.z, R0.w, vertex.position.x;
MUL R0.w, R0.z, c[13].x;
MUL R0.z, R1.w, vertex.position.x;
MAD R0.z, R0, c[13].x, -R0.w;
SGE R1.y, c[0].x, R1.x;
MAD R1.x, R0.z, R1.y, R0.w;
MAD R1.y, R1, R2.w, R2.z;
DP3 R0.w, R3, R1;
MOV R3.y, R2;
MOV R3.x, R0.y;
MOV R2.y, R2.x;
MOV R2.x, R0;
ADD R0.z, R6.w, R8;
MAD R0.z, R0, c[0], R0.w;
MOV R3.z, R4.y;
DP3 R0.w, R1, R3;
MOV R2.z, R4.x;
ADD R0.y, R8, R9;
MAD R0.y, R0, c[0].z, R0.w;
DP3 R1.x, R1, R2;
ADD R0.x, R8, R9;
MAD R0.x, R0, c[0].z, R1;
MOV R0.w, c[13].y;
DP4 R1.z, R0, c[4];
DP4 R1.x, R0, c[1];
DP4 R1.y, R0, c[2];
DP4 R2.z, R0, c[3];
MOV R0, c[2];
MUL R10.y, R0, c[1].z;
MUL R10.z, R0.w, c[1].y;
MUL R3.w, R0.y, c[1].x;
MUL R13.w, R10.z, c[4].x;
MUL R10.x, R0.z, c[1].y;
MUL R13.x, R0.y, c[1].w;
MUL R13.z, R0, c[1].w;
MUL R3.z, R0.x, c[1].y;
MUL R11.y, R0.w, c[1].z;
MOV R2.xy, R1;
MOV R2.w, R1.z;
MOV result.position, R2;
MOV R4.xyw, R2;
MUL R2.z, R10.x, c[3].x;
MUL R2.y, R0.x, c[1].z;
MAD R1.w, R3, c[3].z, R2.z;
MUL R2.x, R0.z, c[1];
MAD R1.w, R2.y, c[3].y, R1;
MAD R1.w, -R2.x, c[3].y, R1;
MAD R3.y, -R3.z, c[3].z, R1.w;
MUL R2.w, R3.z, c[4].z;
MAD R2.w, R2.x, c[4].y, R2;
MAD R2.w, R10.y, c[4].x, R2;
MAD R3.x, -R3.w, c[4].z, R2.w;
MUL R2.w, R2.x, c[3];
MUL R4.z, R2.w, c[4].y;
MUL R2.w, R3, c[3].z;
MAD R3.x, -R10, c[4], R3;
MUL R1.w, R0, c[1].x;
MAD R4.z, R2.w, c[4].w, R4;
MUL R2.w, R1, c[3].y;
MAD R2.w, R2, c[4].z, R4.z;
MUL R4.z, R3, c[3].w;
MAD R2.w, R4.z, c[4].z, R2;
MAD R2.w, R2.z, c[4], R2;
MUL R2.z, R10, c[3];
MAD R2.w, R2.z, c[4].x, R2;
MUL R2.z, R2.y, c[3].y;
MAD R2.z, R2, c[4].w, R2.w;
MUL R10.w, R10.y, c[3];
MAD R5.x, R10.w, c[4], R2.z;
MUL R2.z, R0.x, c[1].w;
MUL R2.w, R11.y, c[3].x;
MAD R0.w, R2, c[4].y, R5.x;
MUL R0.x, R2.z, c[3].z;
MAD R0.w, R0.x, c[4].y, R0;
MUL R0.x, R13, c[3];
MAD R0.y, R0.x, c[4].z, R0.w;
MUL R0.x, R13.z, c[3].y;
MAD R0.y, R0.x, c[4].x, R0;
MUL R0.x, R3.w, c[3].w;
MAD R0.y, -R0.x, c[4].z, R0;
MUL R0.x, R2, c[3].y;
MAD R0.y, -R0.x, c[4].w, R0;
MUL R0.x, R1.w, c[3].z;
MAD R0.y, -R0.x, c[4], R0;
MUL R0.x, R3.z, c[3].z;
MAD R0.y, -R0.x, c[4].w, R0;
MUL R0.x, R10, c[3].w;
MAD R0.y, -R0.x, c[4].x, R0;
MUL R0.x, R10.z, c[3];
MAD R0.y, -R0.x, c[4].z, R0;
MUL R0.x, R2.y, c[3].w;
MAD R0.y, -R0.x, c[4], R0;
MUL R0.x, R10.y, c[3];
MAD R0.y, -R0.x, c[4].w, R0;
MUL R0.x, R11.y, c[3].y;
MAD R0.y, -R0.x, c[4].x, R0;
MUL R0.x, R2.z, c[3].y;
MAD R0.y, -R0.x, c[4].z, R0;
MUL R0.x, R13, c[3].z;
MAD R0.y, -R0.x, c[4].x, R0;
MUL R0.x, R13.z, c[3];
MAD R0.x, -R0, c[4].y, R0.y;
RCP R11.x, R0.x;
MAD R0.y, -R10, c[3].x, R3;
MUL R5.w, R11.x, R0.y;
MOV R0, c[3];
MAD R3.x, -R2.y, c[4].y, R3;
MUL R12.y, R0.z, c[1];
MUL R5.z, R11.x, R3.x;
MUL R14.y, R0.z, c[1].x;
MUL R14.x, R0.w, c[2];
MUL R3.x, R0.y, c[1];
MUL R3.y, R12, c[4].x;
MUL R14.w, R0.x, c[1].z;
MAD R3.y, R3.x, c[4].z, R3;
MAD R3.y, R14.w, c[4], R3;
MAD R3.y, -R14, c[4], R3;
MUL R11.z, R0.x, c[1].y;
MAD R5.x, -R11.z, c[4].z, R3.y;
MUL R3.y, R0.x, c[2];
MUL R11.w, R0.y, c[1].z;
MUL R15.x, R0.z, c[2];
MUL R5.y, R3, c[4].z;
MUL R12.x, R0.y, c[2].z;
MAD R5.y, R15.x, c[4], R5;
MUL R13.y, R0, c[2].x;
MAD R5.y, R12.x, c[4].x, R5;
MUL R12.z, R0, c[2].y;
MAD R5.y, -R13, c[4].z, R5;
MUL R14.z, R0.x, c[2];
MAD R5.y, -R12.z, c[4].x, R5;
MAD R12.w, -R14.z, c[4].y, R5.y;
MAD R5.x, -R11.w, c[4], R5;
MUL R5.y, R11.x, R5.x;
MUL R5.x, R11, R12.w;
MAD R12.w, R1, c[3].y, R4.z;
MOV R4.z, c[0].x;
MAD R12.w, R13.x, c[3].x, R12;
MAD R12.w, -R3, c[3], R12;
MAD R13.w, R3, c[4], R13;
MAD R3.w, -R10.z, c[3].x, R12;
MAD R12.w, R2.z, c[4].y, R13;
MAD R12.w, -R1, c[4].y, R12;
MAD R3.z, -R3, c[4].w, R12.w;
MUL R12.w, R0, c[2].y;
MAD R3.w, -R2.z, c[3].y, R3;
MUL R13.w, R12, c[4].x;
MAD R13.y, R13, c[4].w, R13.w;
MUL R15.y, R0.x, c[2].w;
MAD R3.z, -R13.x, c[4].x, R3;
MAD R13.y, R15, c[4], R13;
MAD R13.y, -R14.x, c[4], R13;
MAD R3.y, -R3, c[4].w, R13;
MUL R13.y, R0, c[2].w;
MUL R13.w, R0, c[1].x;
MUL R11.z, R11, c[4].w;
MAD R15.z, R13.w, c[4].y, R11;
MUL R11.z, R0.y, c[1].w;
MAD R15.z, R11, c[4].x, R15;
MAD R3.x, -R3, c[4].w, R15.z;
MUL R0.y, R0.w, c[1];
MAD R3.x, -R0.y, c[4], R3;
MUL R15.z, R0.x, c[1].w;
MAD R0.x, -R15.z, c[4].y, R3;
MAD R3.y, -R13, c[4].x, R3;
MUL R3.x, R11, R3.y;
MUL R3.y, R11.x, R0.x;
MAD R0.x, R2, c[3].w, R2.w;
MUL R2.w, R2.y, c[4];
MAD R0.x, R2.z, c[3].z, R0;
MAD R0.x, -R1.w, c[3].z, R0;
MAD R2.w, R1, c[4].z, R2;
MAD R1.w, R13.z, c[4].x, R2;
MAD R1.w, -R2.x, c[4], R1;
MAD R0.x, -R2.y, c[3].w, R0;
MAD R0.x, -R13.z, c[3], R0;
MUL R2.w, R11.x, R0.x;
MAD R1.w, -R11.y, c[4].x, R1;
MAD R1.w, -R2.z, c[4].z, R1;
MUL R2.x, R14.z, c[4].w;
MAD R2.x, R14, c[4].z, R2;
MUL R14.x, R0.z, c[2].w;
MAD R2.x, R14, c[4], R2;
MUL R0.x, R0.w, c[1].z;
MUL R2.z, R11.x, R1.w;
MUL R1.w, R0.x, c[4].x;
MAD R1.w, R14.y, c[4], R1;
MUL R14.y, R0.w, c[2].z;
MAD R1.w, R15.z, c[4].z, R1;
MAD R0.w, -R13, c[4].z, R1;
MAD R2.x, -R15, c[4].w, R2;
MAD R2.x, -R14.y, c[4], R2;
MAD R1.w, -R15.y, c[4].z, R2.x;
MUL R2.x, R11, R1.w;
MOV R1.zw, R1.z;
MUL R13.w, R0.z, c[1];
MAD R0.w, -R14, c[4], R0;
MAD R0.z, -R13.w, c[4].x, R0.w;
MUL R2.y, R11.x, R0.z;
MUL R0.w, R11.y, c[4].y;
MAD R0.w, R10.x, c[4], R0;
MAD R0.z, R10, c[3], R10.w;
MAD R0.z, R13, c[3].y, R0;
MAD R0.z, -R10.x, c[3].w, R0;
MAD R0.w, R13.x, c[4].z, R0;
MAD R0.w, -R10.z, c[4].z, R0;
MAD R0.w, -R10.y, c[4], R0;
MAD R0.z, -R11.y, c[3].y, R0;
MUL R10.y, R14, c[4];
MAD R10.x, -R13.z, c[4].y, R0.w;
MAD R0.z, -R13.x, c[3], R0;
MUL R0.w, R11.x, R0.z;
MUL R0.z, R11.x, R10.x;
MUL R10.x, R11.w, c[4].w;
MAD R0.y, R0, c[4].z, R10.x;
MAD R10.y, R12.z, c[4].w, R10;
MAD R10.x, R13.y, c[4].z, R10.y;
MAD R0.y, R13.w, c[4], R0;
MAD R0.y, -R12, c[4].w, R0;
MAD R0.x, -R0, c[4].y, R0.y;
MAD R0.y, -R11.z, c[4].z, R0.x;
MAD R10.x, -R12.w, c[4].z, R10;
MAD R10.x, -R12, c[4].w, R10;
MAD R10.x, -R14, c[4].y, R10;
MUL R0.x, R11, R10;
MUL R0.y, R11.x, R0;
MUL R3.w, R11.x, R3;
MUL R3.z, R11.x, R3;
DP4 result.texcoord[2].y, R4, R2;
DP4 result.texcoord[3].y, R2, R1;
ADD R2.x, -R7, R9;
DP4 result.texcoord[2].z, R4, R3;
DP4 result.texcoord[3].z, R3, R1;
ADD R3.x, -R9.w, R9.y;
MUL R2.y, R3.x, R3.x;
MAD R2.y, R2.x, R2.x, R2;
ADD R2.x, R6.w, -R9.z;
MOV R9.z, R6.w;
MAD R2.x, R2, R2, R2.y;
MAD R7.w, R7, R8, -R2.x;
MOV R6.w, c[8].x;
DP4 result.texcoord[2].x, R4, R0;
DP4 result.texcoord[3].x, R0, R1;
ADD R0.xyz, -R7, R9;
MAD result.texcoord[6].xyz, -R0, c[11].x, R9;
ADD R0.xyz, R8, -R7;
DP4 result.texcoord[2].w, R5, R4;
DP4 result.texcoord[3].w, R5, R1;
MOV result.texcoord[4], R7;
MAD result.texcoord[7].xyz, -R0, c[11].x, R8;
MOV result.texcoord[5], R6;
MOV result.texcoord[0], c[7];
MOV result.texcoord[6].w, c[8].y;
MOV result.texcoord[7].w, c[8].z;
MOV result.texcoord[1].x, c[11];
END
# 341 instructions, 16 R-regs
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
; 362 ALU, 6 FLOW
dcl_position o0
dcl_texcoord2 o1
dcl_texcoord3 o2
dcl_texcoord4 o3
dcl_texcoord5 o4
dcl_texcoord6 o5
dcl_texcoord7 o6
dcl_texcoord0 o7
dcl_texcoord1 o8
def c12, 0.00000000, 2.00000000, 0.00000000, 0.00100000
def c13, 1.00000000, 0.50000000, 0, 0
dcl_position0 v0
mov r0, c9
add r4, -c8, r0
mov r0.y, c0.z
mul r1.y, c1.w, r0
mov r0.y, c0
mul r7.y, c1.z, r0
mul r0.z, r1.y, c3.y
mul r0.x, -r4.y, -r4.y
mad r0.x, -r4, -r4, r0
mad r0.x, -r4.z, -r4.z, r0
rsq r0.x, r0.x
rcp r0.x, r0.x
mul r5.z, v0, r0.x
mov r0.x, c0.w
mul r2.z, c1.y, r0.x
mad r0.y, r7, c3.w, r0.z
mad r0.z, r2, c3, r0.y
mov r0.x, c0.z
mul r3.z, c1.y, r0.x
mov r0.y, c0
mul r10.x, c1.w, r0.y
mul r1.x, r3.z, c2.w
mov r0.x, c0.w
mul r1.z, c1, r0.x
mad r0.y, r10.x, c2.z, r1.x
mad r0.x, r1.z, c2.y, r0.y
mad r0.z, -r10.x, c3, r0
mad r0.y, -r3.z, c3.w, r0.z
mad r0.z, -r7.y, c2.w, r0.x
mov r0.x, c0
mul r7.w, c1.z, r0.x
mad r0.z, -r1.y, c2.y, r0
mov r0.x, c0
mul r8.x, c1.y, r0
mul r0.w, r7, c2
mul r0.x, r8, c2.z
mul r0.w, r0, c3.y
mad r1.w, r0.x, c3, r0
mov r0.w, c0.x
mov r0.x, c0.y
mul r7.z, c1.x, r0.x
mul r7.x, c1.w, r0.w
mul r6.w, r7.z, c2
mad r10.y, r7.x, c2, r6.w
mul r0.x, r7, c2.y
mad r0.x, r0, c3.z, r1.w
mad r0.w, r6, c3.z, r0.x
mul r8.y, r7, c2.x
mov r0.x, c0.z
mul r3.w, c1.x, r0.x
mad r0.w, r8.y, c3, r0
mul r0.x, r10, c2.z
mad r0.w, r0.x, c3.x, r0
mul r0.x, r3.w, c2.y
mad r0.w, r0.x, c3, r0
mov r0.x, c0.w
mul r2.w, c1.x, r0.x
mad r10.y, r2.z, c2.x, r10
mad r8.y, r8.x, c2.z, r8
mul r0.x, r1.y, c2
mad r0.w, r1.x, c3.x, r0
mad r1.x, r0, c3.y, r0.w
mul r0.w, r2, c2.z
mad r1.x, r0.w, c3.y, r1
mul r0.w, r2.z, c2.x
mad r1.x, r0.w, c3.z, r1
mul r0.w, r1.z, c2.y
mad r1.x, r0.w, c3, r1
mul r0.w, r8.x, c2
mad r1.x, -r0.w, c3.z, r1
mul r0.w, r7, c2.y
mad r1.x, -r0.w, c3.w, r1
mul r0.w, r7.x, c2.z
mad r1.x, -r0.w, c3.y, r1
mul r0.w, r7.z, c2.z
mad r1.x, -r0.w, c3.w, r1
mul r0.w, r7.y, c2
mad r1.x, -r0.w, c3, r1
mul r0.w, r10.x, c2.x
mad r1.x, -r0.w, c3.z, r1
mul r0.w, r3, c2
mad r1.x, -r0.w, c3.y, r1
mul r0.w, r3.z, c2.x
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
mul r5.x, c2.y, r0.y
mov r0.y, c0
mul r8.w, c2, r0.y
mul r1.x, r5, c3.w
mad r1.w, r8, c3.z, r1.x
mov r1.x, c0.w
mul r9.z, c2, r1.x
mov r0.y, c0
mul r3.y, c2.z, r0
mad r1.x, r9.z, c3.y, r1.w
mad r1.w, -r3.y, c3, r1.x
mov r1.x, c0.z
mul r6.x, c2.w, r1
mad r1.x, -r6, c3.y, r1.w
mov r0.y, c0.w
mul r8.z, c2.y, r0.y
mad r0.y, -r8.z, c3.z, r1.x
mov r1.w, c1.y
mul r5.w, c2.z, r1
mov r1.x, c1.z
mul r1.x, c2.w, r1
mul r2.x, r1, c3.y
mad r5.y, r5.w, c3.w, r2.x
mov r2.x, c1.w
mul r2.y, c2, r2.x
mov r1.w, c1.y
mul r2.x, c2.w, r1.w
mad r5.y, r2, c3.z, r5
mad r6.y, -r2.x, c3.z, r5
mov r1.w, c1
mov r5.y, c1.z
mul r5.y, c2, r5
mul r6.z, c2, r1.w
mad r6.y, -r5, c3.w, r6
mad r1.w, -r6.z, c3.y, r6.y
mad r6.y, r7.w, c2.w, r0.x
mul r0.x, r3, r1.w
mad r1.w, r2, c2.z, r6.y
mul r6.y, r3.w, c3.w
mad r1.w, -r7.x, c2.z, r1
mad r6.y, r7.x, c3.z, r6
mad r6.y, r1.z, c3.x, r6
mad r1.w, -r3, c2, r1
mad r1.z, -r1, c2.x, r1.w
mad r6.y, -r7.w, c3.w, r6
mad r1.y, -r1, c3.x, r6
mul r1.w, r3.x, r1.z
mov r1.z, c0.w
mul r9.y, c2.x, r1.z
mov r6.y, c0.x
mad r1.y, -r2.w, c3.z, r1
mad r10.y, -r8.x, c2.w, r10
mul r6.y, c2.z, r6
mul r6.x, r6, c3
mad r6.x, r6.y, c3.w, r6
mad r9.w, r9.y, c3.z, r6.x
mov r6.x, c0
mul r9.x, c2.w, r6
mov r1.z, c0
mad r9.w, -r9.x, c3.z, r9
mul r6.x, c2, r1.z
mad r1.z, -r6.x, c3.w, r9.w
mad r9.z, -r9, c3.x, r1
mul r1.z, r3.x, r1.y
mul r1.y, r3.x, r9.z
mov r9.z, c1
mul r6.w, c2.x, r9.z
mov r9.z, c1.x
mul r9.w, r6, c3
mul r9.z, c2.w, r9
mad r10.z, r9, c3, r9.w
mov r9.w, c1.x
mad r10.z, r6, c3.x, r10
mul r6.z, c2, r9.w
mad r10.z, -r6, c3.w, r10
mad r1.x, -r1, c3, r10.z
mul r10.z, r10.x, c3.x
mad r10.x, -r10, c2, r10.y
mad r10.y, r8.x, c3.w, r10.z
mov r9.w, c1
mul r9.w, c2.x, r9
mad r1.x, -r9.w, c3.z, r1
mad r10.x, -r2.w, c2.y, r10
mad r10.y, r2.w, c3, r10
mul r2.w, r3.x, r10.x
mad r10.x, -r7, c3.y, r10.y
mad r10.x, -r7.z, c3.w, r10
mad r2.z, -r2, c3.x, r10.x
mov r7.x, c0.y
mul r7.x, c2, r7
mul r10.x, r7, c3.w
mad r10.x, r9, c3.y, r10
mov r9.x, c0
mad r10.x, r8.z, c3, r10
mul r8.z, c2.y, r9.x
mov r9.x, c1
mul r3.y, r3, c3.x
mad r3.y, r8.z, c3.z, r3
mad r3.y, r6.x, c3, r3
mad r3.y, -r6, c3, r3
mad r3.y, -r7.x, c3.z, r3
mad r3.y, -r5.x, c3.x, r3
mul r9.x, c2.y, r9
mul r2.x, r2, c3
mad r10.y, r9.x, c3.w, r2.x
mad r10.x, -r8.z, c3.w, r10
mad r2.x, -r8.w, c3, r10
mad r9.w, r9, c3.y, r10.y
mov r8.w, c1.y
mad r2.x, -r9.y, c3.y, r2
mul r8.w, c2.x, r8
mad r9.z, -r9, c3.y, r9.w
mad r9.z, -r8.w, c3.w, r9
mad r9.y, -r2, c3.x, r9.z
mul r2.y, r3.x, r2.x
mul r2.x, r3, r9.y
mul r9.y, r7.z, c3.z
mad r9.y, r7.w, c3, r9
mad r9.y, r3.z, c3.x, r9
mad r8.y, r3.w, c2, r8
mad r8.x, -r8, c3.z, r9.y
mad r7.w, -r7, c2.y, r8.y
mad r7.z, -r7, c2, r7.w
mad r7.y, -r7, c3.x, r8.x
mad r7.y, -r3.w, c3, r7
mad r3.z, -r3, c2.x, r7
mul r3.w, r3.x, r3.z
mul r3.z, r3.x, r7.y
mul r7.y, r8.w, c3.z
mad r6.z, r6, c3.y, r7.y
mad r5.y, r5, c3.x, r6.z
mad r5.y, -r9.x, c3.z, r5
mad r5.y, -r5.w, c3.x, r5
mad r5.x, -r6.w, c3.y, r5.y
mov r5.y, c5.x
mul r0.z, r3.x, r0
mul r0.y, r3.x, r0
mul r1.x, r3, r1
mul r2.z, r3.x, r2
mul r3.y, r3.x, r3
mul r3.x, r3, r5
mov r5.x, c4
mul r5.w, c11.x, r5.y
mul r6.w, c11.x, r5.x
mov r9.xyz, c12.x
if_gt r6.w, r5.w
mul r5.x, v0, r6.w
mul r5.y, v0, r6.w
mul r5.x, r5, c12.y
mul r5.y, r5, c12
else
mul r5.x, v0, r5.w
mul r5.y, v0, r5.w
mul r5.x, r5, c12.y
mul r5.y, r5, c12
endif
dp4 r4.w, -r4, -r4
rsq r4.w, r4.w
mul r6.xyz, r4.w, -r4
mov r12.xy, r6
mov r10.xyz, r6
if_eq r6.z, c12.x
mov r10.z, c12
endif
if_eq -r4.x, c12.x
mov r9.xyz, c12.wxxw
endif
if_eq -r4.y, c12.x
mov r9.y, c12.w
endif
if_eq -r4.z, c12.x
mov r9.z, c12.w
endif
add r4.w, r9.x, -r4.x
mul r6.y, r4.w, c12
mul r4.w, r6, r6
add r6.x, r9.y, -r4.y
mul r6.z, r4.x, r4.x
rcp r8.w, c10.x
rcp r6.w, r6.y
mad r5.w, r5, r5, -r4
mul r6.y, r5.w, r12.x
mul r6.x, r6, c12.y
mad r6.z, r9.x, r9.x, -r6
mul r6.y, r12.x, r6
mad r6.y, r8.w, r6, r6.z
mul r8.x, r6.y, r6.w
rcp r6.z, r6.x
mul r6.x, r5.w, r12.y
mul r6.y, r4, r4
mul r5.w, r10.z, r5
mov r6.w, c13.x
mad r6.y, r9, r9, -r6
mul r6.x, r12.y, r6
mad r6.x, r8.w, r6, r6.y
mul r10.w, r6.x, r6.z
mul r6.y, r12, r10.w
add r6.x, r9.z, -r4.z
mov r8.y, r10.w
mad r6.z, r12.x, r8.x, r6.y
mul r6.x, r6, c12.y
rcp r6.y, r6.x
mul r6.x, r4.z, r4.z
mad r6.x, r9.z, r9.z, -r6
mul r5.w, r10.z, r5
mad r5.w, r5, r8, r6.x
mul r9.w, r5, r6.y
mad r5.w, r10.z, r9, r6.z
add r5.w, r5, -r12.x
mov r8.z, r9.w
rcp r6.x, r10.z
add r5.w, r5, -r12.y
mul r6.z, r5.w, r6.x
mov r6.xy, c13.x
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
add r5.w, r9.z, r4.z
mad r6.z, r5.w, c13.y, r6
mov r11.y, r7
mov r7.y, r7.x
mov r7.x, r6
add r5.w, r9.y, r4.y
mov r7.z, r12.x
mov r11.z, r12.y
mov r11.x, r6.y
dp3 r6.y, r5, r11
dp3 r5.y, r5, r7
add r5.x, r9, r4
mad r6.y, r5.w, c13, r6
mad r6.x, r5, c13.y, r5.y
dp4 r7.z, r6, c3
mov r5.w, r7.z
dp4 r7.x, r6, c0
dp4 r7.y, r6, c1
mov r7.zw, r7.z
mov r5.xy, r7
dp4 r5.z, r6, c2
mov o0, r5
mov r5.z, c12.x
dp4 o1.z, r5, r2
dp4 o2.z, r2, r7
add r2.x, r9, -r8
dp4 o1.w, r3, r5
dp4 o2.w, r3, r7
add r3.x, r9.y, -r10.w
mul r2.y, r3.x, r3.x
mad r2.y, r2.x, r2.x, r2
add r2.x, r9.z, -r9.w
mad r2.x, r2, r2, r2.y
mad r8.w, r4, r8, -r2.x
dp4 o1.y, r5, r1
dp4 o2.y, r1, r7
add r1.xyz, r4, -r8
dp4 o1.x, r5, r0
dp4 o2.x, r0, r7
add r0.xyz, r9, -r8
mov o3, r8
mad o5.xyz, -r0, c10.x, r9
mad o6.xyz, -r1, c10.x, r4
mov o7, c6
mov o4.xyz, r10
mov o4.w, c7.x
mov o5.w, c7.y
mov o6.w, c7.z
mov o8.x, c10
"
}

}
Program "fp" {
// Fragment combos: 1
//   opengl - ALU: 173 to 173, TEX: 1 to 1
//   d3d9 - ALU: 175 to 175, TEX: 4 to 4
SubProgram "opengl " {
Keywords { }
Float 8 [_EllipseFactor]
SetTexture 0 [_MatCap] 2D
"3.0-!!ARBfp1.0
# 173 ALU, 1 TEX
PARAM c[11] = { state.matrix.mvp,
		state.matrix.modelview[0].invtrans,
		program.local[8],
		{ 1, 0, 0.5, 200 },
		{ 1.5 } };
TEMP R0;
TEMP R1;
TEMP R2;
TEMP R3;
TEMP R4;
TEMP R5;
TEMP R6;
TEMP R7;
TEMP R8;
MOV R2.xyz, fragment.texcoord[4];
MUL R0.w, fragment.texcoord[5].y, R2.y;
MAD R0.x, fragment.texcoord[5], R2, R0.w;
MAD R2.x, -fragment.texcoord[5], R2, -R0.w;
MAD R0.x, fragment.texcoord[5].z, R2.z, R0;
ADD R0.x, -fragment.texcoord[5], R0;
ADD R0.z, -fragment.texcoord[5].y, R0.x;
RCP R1.x, fragment.texcoord[5].z;
ADD R0.w, -fragment.texcoord[1].x, c[9].x;
MAD R4.w, -fragment.texcoord[5].z, R2.z, R2.x;
RCP R4.y, R0.w;
MOV R5.w, c[9].y;
MUL R0.z, R0, R1.x;
MOV R0.xy, c[9].x;
ADD R0.xyz, -fragment.texcoord[4], R0;
DP3 R1.x, R0, R0;
RSQ R1.x, R1.x;
MUL R0.xyz, R1.x, R0;
MUL R1.xyz, fragment.texcoord[5].yzxw, R0.zxyw;
MAD R1.xyz, fragment.texcoord[5].zxyw, R0.yzxw, -R1;
DP3 R1.w, R1, R1;
RSQ R1.w, R1.w;
MUL R1.xyz, R1.w, R1;
MUL R1.w, fragment.texcoord[4].y, R1.y;
MAD R1.w, fragment.texcoord[4].x, -R1.x, -R1;
MAD R3.w, fragment.texcoord[4].z, -R1.z, R1;
RCP R1.w, fragment.texcoord[1].x;
MUL R3.z, R1.w, c[8].x;
MUL R2.w, R3.z, R3;
MUL R2.y, fragment.texcoord[4], R0;
MAD R2.y, fragment.texcoord[4].x, -R0.x, -R2;
MAD R4.x, fragment.texcoord[4].z, -R0.z, R2.y;
MUL R2.z, R1.w, R0.x;
MUL R3.y, R3.z, R1;
MUL R3.x, R1.w, R0.y;
MUL R2.w, R3, R2;
MUL R2.y, R1.w, R4.x;
MAD R2.y, R4.x, R2, R2.w;
MUL R2.w, R3.z, R1.x;
MUL R7.w, R2, R1.x;
MUL R3.z, R3, R1;
MAD R0.x, R2.z, R0, R7.w;
MUL R0.w, -R4.y, R4;
MAD R0.w, R4, R0, R2.y;
ADD R6.w, -fragment.texcoord[4], R0;
MUL R0.w, R2, R3;
MAD R2.x, R2.z, R4, R0.w;
MUL R0.w, -R4.y, fragment.texcoord[5].x;
MAD R6.x, R0.w, R4.w, R2;
MUL R2.x, R3.w, R3.y;
MUL R4.z, R3.w, R3;
MUL R3.w, R1, R0.z;
MAD R1.w, R4.x, R3, R4.z;
MAD R2.x, R4, R3, R2;
MUL R2.y, -R4, fragment.texcoord[5];
MAD R2.x, R4.w, R2.y, R2;
MUL R4.z, -R4.y, fragment.texcoord[5];
MAD R4.w, R4, R4.z, R1;
MUL R4.x, R1.z, R3.y;
MUL R1.x, R2.w, R1.z;
RCP R1.w, fragment.texcoord[2].w;
MUL R7.xyz, fragment.texcoord[2], R1.w;
RCP R1.w, fragment.texcoord[3].w;
MAD R5.xyz, fragment.texcoord[3], R1.w, -R7;
MOV R7.w, c[9].x;
MAD R4.y, R0.z, R3.x, R4.x;
MAD R1.x, R2.z, R0.z, R1;
MAD R4.x, R0.w, fragment.texcoord[5].z, R1;
MAD R1.x, fragment.texcoord[5].z, R2.y, R4.y;
MUL R1.z, R1, R3;
MAD R0.z, R0, R3.w, R1;
MAD R4.z, fragment.texcoord[5], R4, R0;
MUL R0.z, R2.w, R1.y;
MOV R4.y, R1.x;
MAD R0.z, R2, R0.y, R0;
MUL R1.y, R1, R3;
MAD R0.y, R0, R3.x, R1;
MAD R3.x, R0.w, fragment.texcoord[5].y, R0.z;
MAD R3.y, fragment.texcoord[5], R2, R0;
DP4 R1.z, R5, R4;
MOV R3.w, R2.x;
MOV R3.z, R1.x;
MOV R6.y, R2.x;
MOV R6.z, R4.w;
MAD R2.x, R0.w, fragment.texcoord[5], R0;
DP4 R1.y, R5, R3;
MOV R2.y, R3.x;
MOV R2.z, R4.x;
MOV R2.w, R6.x;
DP4 R1.x, R5, R2;
DP4 R1.w, R6, R5;
DP4 R8.x, R7, R1;
DP4 R0.x, R7, R2;
DP4 R0.z, R7, R4;
DP4 R0.y, R7, R3;
DP4 R0.w, R7, R6;
DP4 R0.y, R7, R0;
DP4 R0.x, R1, R5;
MOV R0.w, c[9].x;
MUL R7.w, R0.x, R0.y;
MUL R5.w, R8.x, R8.x;
ADD R0.y, R5.w, -R7.w;
RSQ R0.y, R0.y;
RCP R0.z, R0.x;
RCP R0.y, R0.y;
ADD R0.x, -R8, -R0.y;
MUL R0.x, R0, R0.z;
MAD R0.xyz, R0.x, R5, R7;
DP4 R1.y, R0, R3;
DP4 R1.x, R0, R2;
DP4 R1.z, R0, R4;
DP4 R1.w, R0, R6;
DP4 R2.z, R1, c[6];
DP4 R2.x, R1, c[4];
DP4 R2.y, R1, c[5];
DP3 R1.x, R2, R2;
RSQ R1.x, R1.x;
MUL R4.xyz, R1.x, R2;
MAX R1.y, R4.z, c[9];
MOV R3.xyz, fragment.texcoord[7];
ADD R1.x, R0.y, -fragment.texcoord[7].y;
POW R3.w, R1.y, c[9].w;
MUL R1.y, fragment.texcoord[5], R1.x;
ADD R1.x, R0, -fragment.texcoord[7];
MAD R2.x, fragment.texcoord[5], R1, R1.y;
ADD R1.xyz, fragment.texcoord[6], -R3;
DP3 R2.w, R1, R1;
ADD R1.w, R0.z, -fragment.texcoord[7].z;
MAD R1.w, fragment.texcoord[5].z, R1, R2.x;
RSQ R2.w, R2.w;
MUL R1.w, R2, R1;
MOV R1.z, fragment.texcoord[7].w;
MOV R1.x, fragment.texcoord[5].w;
MOV R1.y, fragment.texcoord[6].w;
ADD R2.xyz, fragment.texcoord[0], -R1;
MAD R1.xyz, R1.w, R2, R1;
MAD R2.xyz, R4.z, R1, R3.w;
MAD R4.xy, R4, c[9].z, c[9].z;
TEX R1, R4, texture[0], 2D;
MOV R2.w, fragment.texcoord[0];
MUL R1, R2, R1;
MUL R2.x, R0.y, -fragment.texcoord[5].y;
MUL result.color, R1, c[10].x;
MAD R1.x, R0, -fragment.texcoord[5], R2;
MAD R1.y, R0.x, fragment.texcoord[5].x, -R2.x;
MAD R2.x, R0.z, fragment.texcoord[5].z, R1.y;
MAD R1.w, R0.z, -fragment.texcoord[5].z, R1.x;
MOV R1.xyz, fragment.texcoord[6];
MAD R1.x, fragment.texcoord[5], R1, R1.w;
MAD R1.x, fragment.texcoord[5].y, R1.y, R1;
MUL R1.z, -fragment.texcoord[5], R1;
SLT R1.x, R1, R1.z;
MAD R1.y, -fragment.texcoord[5].x, R3.x, R2.x;
ABS R1.x, R1;
MUL R1.z, fragment.texcoord[5], R3;
MAD R1.y, -fragment.texcoord[5], R3, R1;
SLT R1.y, R1, R1.z;
ABS R1.y, R1;
CMP R1.z, -R1.y, c[9].y, c[9].x;
CMP R1.x, -R1, c[9].y, c[9];
CMP R1.y, -R1.x, c[9], c[9].x;
DP4 R1.x, R0, c[3];
DP4 R0.x, R0, c[2];
CMP R1.z, -R1, c[9].y, c[9].x;
ADD_SAT R1.y, R1, R1.z;
RCP R1.x, R1.x;
MAD R0.x, R0, R1, c[9];
MUL result.depth.z, R0.x, c[9];
SLT R0.y, R5.w, R7.w;
SLT R0.x, fragment.texcoord[1], c[9].y;
KIL -R1.y;
KIL -R0.y;
KIL -R0.x;
END
# 173 instructions, 9 R-regs
"
}

SubProgram "d3d9 " {
Keywords { }
Matrix 0 [glstate_matrix_mvp]
Matrix 4 [glstate_matrix_invtrans_modelview0]
Float 8 [_EllipseFactor]
SetTexture 0 [_MatCap] 2D
"ps_3_0
; 175 ALU, 4 TEX
dcl_2d s0
def c9, 1.00000000, 0.00000000, 200.00000000, 0.50000000
def c10, 1.50000000, 0, 0, 0
dcl_texcoord2 v0
dcl_texcoord3 v1
dcl_texcoord4 v2
dcl_texcoord5 v3
dcl_texcoord6 v4
dcl_texcoord7 v5
dcl_texcoord0 v6
dcl_texcoord1 v7.x
rcp r3.x, v7.x
mov r0.x, v2.y
mul r0.w, v3.y, r0.x
mov r0.x, v2
mad r0.y, v3.x, r0.x, r0.w
mov r0.x, v2.z
mad r0.z, v3, r0.x, r0.y
add r0.z, -v3.x, r0
mov r6.w, c9.x
mul r3.y, r3.x, c8.x
rcp r1.x, v3.z
add r0.z, -v3.y, r0
mul r0.z, r0, r1.x
mov r0.xy, c9.x
add r0.xyz, -v2, r0
dp3 r1.x, r0, r0
rsq r1.x, r1.x
mul r0.xyz, r1.x, r0
mul r1.xyz, v3.yzxw, r0.zxyw
mad r1.xyz, v3.zxyw, r0.yzxw, -r1
dp3 r1.w, r1, r1
rsq r1.w, r1.w
mul r1.xyz, r1.w, r1
mul r2.z, r3.y, r1.y
mul r1.w, v2.y, r1.y
mad r1.w, v2.x, -r1.x, -r1
mad r3.z, v2, -r1, r1.w
mul r4.x, r3.y, r1
mul r2.x, r3.y, r3.z
mul r3.y, r3, r1.z
mul r1.w, v2.y, r0.y
mad r1.w, v2.x, -r0.x, -r1
mad r3.w, v2.z, -r0.z, r1
mul r2.y, r3.z, r2.x
mul r2.x, r3, r3.w
mul r4.y, r3.x, r0
mul r7.x, r1.z, r2.z
mad r2.y, r3.w, r2.x, r2
mov r1.w, v2.x
mad r2.x, -v3, r1.w, -r0.w
add r1.w, -v7.x, c9.x
mov r0.w, v2.z
mad r0.w, -v3.z, r0, r2.x
rcp r4.z, r1.w
mul r1.w, -r4.z, r0
mad r1.w, r0, r1, r2.y
add r5.w, -v2, r1
mul r2.x, r3, r0
mul r1.w, r4.x, r3.z
mad r2.y, r2.x, r3.w, r1.w
mul r1.w, -r4.z, v3.x
mad r5.x, r1.w, r0.w, r2.y
mul r2.y, r3.z, r2.z
mad r2.w, r3, r4.y, r2.y
mul r2.y, -r4.z, v3
mad r2.w, r0, r2.y, r2
mul r4.w, r3.z, r3.y
mul r3.z, r3.x, r0
mad r3.x, r3.w, r3.z, r4.w
mul r4.w, -r4.z, v3.z
mad r3.w, r0, r4, r3.x
mul r4.z, r1.y, r2
mul r3.x, r4, r1.z
mad r2.z, r2.x, r0, r3.x
mad r7.x, r0.z, r4.y, r7
mad r3.x, r1.w, v3.z, r2.z
mul r1.z, r1, r3.y
mad r0.z, r0, r3, r1
mad r2.z, v3, r2.y, r7.x
mad r3.z, v3, r4.w, r0
mad r4.y, r0, r4, r4.z
mul r1.y, r4.x, r1
mul r1.x, r4, r1
mad r0.x, r2, r0, r1
mad r0.y, r2.x, r0, r1
mad r2.x, r1.w, v3.y, r0.y
mad r1.x, r1.w, v3, r0
rcp r0.w, v0.w
mul r6.xyz, v0, r0.w
mad r2.y, v3, r2, r4
mov r3.y, r2.z
mov r1.y, r2.x
mov r1.z, r3.x
mov r1.w, r5.x
dp4 r0.x, r6, r1
dp4 r0.y, r6, r2
mov r4.w, c9.y
mov r5.y, r2.w
mov r5.z, r3.w
dp4 r0.z, r6, r3
dp4 r0.w, r6, r5
dp4 r7.x, r6, r0
rcp r0.x, v1.w
mad r4.xyz, v1, r0.x, -r6
dp4 r0.x, r4, r1
dp4 r0.y, r4, r2
dp4 r0.z, r4, r3
dp4 r0.w, r5, r4
dp4 r4.w, r0, r4
dp4 r0.x, r6, r0
mul r7.x, r4.w, r7
mad r6.w, r0.x, r0.x, -r7.x
rsq r0.y, r6.w
rcp r0.y, r0.y
mov r0.w, c9.x
rcp r0.z, r4.w
add r0.x, -r0, -r0.y
mul r0.x, r0, r0.z
mad r0.xyz, r4, r0.x, r6
dp4 r4.x, r1, r0
dp4 r4.y, r2, r0
dp4 r4.z, r3, r0
dp4 r4.w, r5, r0
dp4 r1.z, r4, c6
dp4 r1.x, r4, c4
dp4 r1.y, r4, c5
dp3 r1.w, r1, r1
rsq r1.w, r1.w
mul r2.xyz, r1.w, r1
max r2.w, r2.z, c9.y
pow r1, r2.w, c9.z
mad_pp r3.xy, r2, c9.w, c9.w
mov r1.w, r1.x
add r1.y, r0, -v5
add r1.x, r0, -v5
mul r1.y, v3, r1
mad r2.y, v3.x, r1.x, r1
add r2.x, r0.z, -v5.z
mov r1.xyz, v5
add r1.xyz, v4, -r1
mad r2.y, v3.z, r2.x, r2
dp3 r2.x, r1, r1
rsq r2.x, r2.x
mov r1.z, v5.w
mov r1.x, v3.w
mov r1.y, v4.w
mul r2.x, r2, r2.y
add r4.xyz, v6, -r1
mad r1.xyz, r2.x, r4, r1
mad r2.xyz, r2.z, r1, r1.w
texld r1, r3, s0
mov r2.w, v6
mul r2, r2, r1
mul r1.x, -v3.y, r0.y
mad r1.y, v3.x, r0.x, -r1.x
mad r1.z, v3, r0, r1.y
mov r1.y, v5.x
mad r1.z, -v3.x, r1.y, r1
mov r1.y, v5
mad r1.z, -v3.y, r1.y, r1
mov r1.y, v5.z
mul oC0, r2, c10.x
dp4 r2.x, r0, c3
mad r1.x, -v3, r0, r1
mad r1.z, -v3, r1.y, r1
mad r1.y, -v3.z, r0.z, r1.x
mov r1.x, v4
mad r1.y, v3.x, r1.x, r1
mov r1.x, v4.y
mad r1.y, v3, r1.x, r1
mov r1.x, v4.z
mad r1.x, v3.z, r1, r1.y
dp4 r0.x, r0, c2
rcp r2.x, r2.x
mad r0.x, r0, r2, c9
mul oDepth, r0.x, c9.w  //Hack for Direct3D. Original line : mul oDepth.z, r0.x, c9.w
cmp r0.x, r6.w, c9.y, c9
mov_pp r0, -r0.x
cmp_pp r1.y, r1.z, c9, c9.x
cmp_pp r1.x, r1, c9.y, c9
add_pp_sat r1.x, r1, r1.y
mov_pp r1, -r1.x
texkill r1.xyzw
cmp r1.x, v7, c9.y, c9
mov_pp r1, -r1.x
texkill r0.xyzw
texkill r1.xyzw
"
}

}

#LINE 416

    }

} 
// Pour les cartes graphiques ne supportant pas nos Shaders
Fallback "VertexLit"
}