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


namespace UMol {
public class RaytracingAlloyMaterial : RaytracingMaterial{

    private Vector3 _color = new Vector3(0.9f, 0.9f, 0.9f);// white 0.9 reflectivity at normal incidence (0 degree)
    public Vector3 color {
        get { return _color;}
        set {
            propertyChanged = true;
            _color = value;
        }
    }

    private Vector3 _edgeColor = Vector3.one;// white reflectivity at grazing angle (90 degree)
    public Vector3 edgeColor {
        get { return _edgeColor;}
        set {
            propertyChanged = true;
            _edgeColor = value;
        }
    }
    
    private float _roughness = 0.1f;// 0.1	roughness, in [0–1], 0 is perfect mirror
    public float roughness {
        get { return _roughness;}
        set {
            propertyChanged = true;
            _roughness = value;
        }
    }

}
}