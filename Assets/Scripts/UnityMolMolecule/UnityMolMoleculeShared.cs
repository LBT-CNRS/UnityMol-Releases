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
using System.Collections.Generic;


namespace UMol {
	
	public enum AtomType {
		sphere = 1,
		optihb = 4,
		cartoon = 5,
		surface = 6,
		DXSurface = 7,
		fieldlines = 8,
		trace = 9,
		sugarribbons = 10,
		ellipsoid = 12,
		bondorder = 13,
		point = 14,
		noatom = -1
	}

	public enum BondType {
		line = 1,
		optihs = 2,
		hbond = 3,
		hbondtube = 4,
		bondorder = 5,
		nobond = -1
	}

	public enum FFType {
		atomic = 0,
		HiRERNA = 1
	}
	public struct RepType {
		public AtomType atomType;
		public BondType bondType;
	
		public static bool operator ==(RepType c1, RepType c2) 
		{
		    return c1.atomType.Equals(c2.atomType) && c1.bondType.Equals(c2.bondType);
		}

		public static bool operator !=(RepType c1, RepType c2) 
		{
		   return !(c1 == c2);
		}
		public override bool Equals(object obj){
			if (!(obj is RepType))
				return false;
			return ((RepType)obj) == this;
		}
		public override int GetHashCode(){
            int h = (int)atomType;
   			unchecked{
  
	   			const int factor = 9176;
	   			h = h * factor + (int)bondType;
   			}
            return h;
		}
	}



    public enum SurfMethod {
        EDTSurf = 0,
        MSMS = 1,
        QUICKSES = 2
    }


}