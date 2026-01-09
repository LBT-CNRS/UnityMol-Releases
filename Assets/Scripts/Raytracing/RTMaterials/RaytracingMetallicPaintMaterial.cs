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
public class RaytracingMetallicPaintMaterial : RaytracingMaterial{

    private Vector3 _baseColor = new Vector3(0.8f, 0.8f, 0.8f);// white 0.8	color of base coat
    public Vector3 baseColor {
        get { return _baseColor; }
        set {
            propertyChanged = true;
            _baseColor = value;
        }
    }
    private float _flakeAmount = 0.3f;// 0.3	amount of flakes, in [0–1]
    public float flakeAmount {
        get { return _flakeAmount; }
        set {
            propertyChanged = true;
            _flakeAmount = value;
        }
    }
    private Vector3 _flakeColor;// Aluminium	color of metallic flakes
    public Vector3 flakeColor {
        get { return _flakeColor; }
        set {
            propertyChanged = true;
            _flakeColor = value;
        }
    }
    private float _flakeSpread = 0.5f;// 0.5	spread of flakes, in [0–1]
    public float flakeSpread {
        get { return _flakeSpread; }
        set {
            propertyChanged = true;
            _flakeSpread = value;
        }
    }
    private float _eta = 1.5f;// 1.5	index of refraction of clear coat
    public float eta {
        get { return _eta;}
        set {
            propertyChanged = true;
            _eta = value;
        }
    }
}
}