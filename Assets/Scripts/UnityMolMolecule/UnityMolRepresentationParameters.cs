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

namespace UMol {

/// This class is used to save representations parameters
public class UnityMolRepresentationParameters {

	public RepType repT;

	// public Material mat;

	public colorType colorationType;
	public Color32 fullColor;

	//Shared
	public Dictionary<UnityMolAtom, Color32> colorPerAtom;

	public float smoothness;
	public float metal;

	public bool useHET;
	public bool useWat;
	public bool shadow;

	public int textureId;

	public bool sideChainsOn;
	public bool hydrogensOn;
	public bool backboneOn;

	public Color bfactorStartColor;
	public Color bfactorEndColor;

	//Surface
	public SurfMethod surfMethod;
	public float surfAlpha;
	public float surfProbeRad;
	public bool surfCutSurface;
	public bool surfCutByChain;
	public bool surfAO;
	public bool surfIsTransparent;
	public bool surfIsWireframe;
	public float surfWireframeSize;

	//DXSurface
	public float DXsurfIso;

	//Fieldlines
	public int FLnbIter;
	public float FLmagThresh;
	public float FLwidth;
	public float FLlength;
	public float FLspeed;
	public Color32 FLstartCol;
	public Color32 FLendCol;

	//OptiHS
	public float HSShrink;
	public float HSScale;

	//OptiHB
	public float HBScale;

	//Line
	public float LineWidth;

	//HbondsTube
	public float HBTHeight;
	public float HBTSpace;
	public float HBTRadius;
	public bool HBCustomBonds;//Both Hbond & Hbondtube

	//Tube
	public float TubeWidth;

	//SugarRibbons
	public bool SRplanes;

}

public enum colorType{
	atom,
	res,
	chain,
	hydro,
	seq,
	charge,
	restype,
	rescharge,
	bfactor,
	full,//one color
	defaultCartoon,
	custom
}

}