/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2022
        Hubert Santuz, 2022-2026
        Marc Baaden, 2010-2026
        unitymol@gmail.com
        https://unity.mol3d.tech/

        This file is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications based on the Unity3D game engine.
        More details about UnityMol are provided at the following URL: https://unity.mol3d.tech/

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

        To help us with UnityMol development, we ask that you cite
        the research papers listed at https://unity.mol3d.tech/cite-us/.
    ================================================================================
*/
using UnityEngine;
using System.Collections.Generic;


namespace UMol {
	
	public enum AtomType {
		noatom,
		sphere,
		optihb,
		cartoon,
		surface,
		DXSurface,
		fieldlines,
		trace,
		sugarribbons,
		sheherasade,
		ellipsoid,
		bondorder,
		point,
		explosurf
	}

	public enum BondType {
		nobond,
		line,
		optihs,
		hbond,
		hbondtube,
		bondorder
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
        QUICKSES = 2,
        QUICKSURF = 3
    }


}