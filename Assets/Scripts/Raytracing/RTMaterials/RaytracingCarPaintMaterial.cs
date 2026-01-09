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


namespace UMol
{
    public class RaytracingCarPaintMaterial : RaytracingMaterial
    {

        private Vector3 _baseColor = new Vector3(0.8f, 0.8f, 0.8f);//   white 0.8   base reflectivity (diffuse and/or metallic)
        public Vector3 baseColor
        {
            get { return _baseColor; }
            set
            {
                propertyChanged = true;
                _baseColor = value;
            }
        }
        private float _roughness = 0.0f;//   0   diffuse roughness in [0–1], 0 is perfectly smooth
        public float roughness
        {
            get { return _roughness; }
            set
            {
                propertyChanged = true;
                _roughness = value;
            }
        }
        float _normal = 1.0f;//	1	normal map/scale
        public float normal
        {
            get { return _normal; }
            set
            {
                propertyChanged = true;
                _normal = value;
            }
        }
        float _flakeDensity = 0.0f;//	0	density of metallic flakes in [0–1], 0 disables flakes, 1 fully covers the surface with flakes
        public float flakeDensity
        {
            get { return _flakeDensity; }
            set
            {
                propertyChanged = true;
                _flakeDensity = value;
            }
        }
        float _flakeScale = 100.0f;//	100	scale of the flake structure, higher values increase the amount of flakes
        public float flakeScale
        {
            get { return _flakeScale; }
            set
            {
                propertyChanged = true;
                _flakeScale = value;
            }
        }
        float _flakeSpread = 0.3f;//	0.3	flake spread in [0–1]
        public float flakeSpread
        {
            get { return _flakeSpread; }
            set
            {
                propertyChanged = true;
                _flakeSpread = value;
            }
        }
        float _flakeJitter = 0.75f;//	0.75	flake randomness in [0–1]
        public float flakeJitter
        {
            get { return _flakeJitter; }
            set
            {
                propertyChanged = true;
                _flakeJitter = value;
            }
        }
        float _flakeRoughness = 0.3f;//	0.3	flake roughness in [0–1], 0 is perfectly smooth
        public float flakeRoughness
        {
            get { return _flakeRoughness; }
            set
            {
                propertyChanged = true;
                _flakeRoughness = value;
            }
        }
        float _coat = 1.0f;//	1	clear coat layer weight in [0–1]
        public float coat
        {
            get { return _coat; }
            set
            {
                propertyChanged = true;
                _coat = value;
            }
        }
        float _coatIor = 1.5f;//	1.5	clear coat index of refraction
        public float coatIor
        {
            get { return _coatIor; }
            set
            {
                propertyChanged = true;
                _coatIor = value;
            }
        }
        Vector3 _coatColor = Vector3.one;//	white	clear coat color tint
        public Vector3 coatColor
        {
            get { return _coatColor; }
            set
            {
                propertyChanged = true;
                _coatColor = value;
            }
        }
        float _coatThickness = 1.0f;//	1	clear coat thickness, affects the amount of color attenuation
        public float coatThickness
        {
            get { return _coatThickness; }
            set
            {
                propertyChanged = true;
                _coatThickness = value;
            }
        }
        float _coatRoughness = 0.0f;//	0	clear coat roughness in [0–1], 0 is perfectly smooth
        public float coatRoughness
        {
            get { return _coatRoughness; }
            set
            {
                propertyChanged = true;
                _coatRoughness = value;
            }
        }
        float _coatNormal = 1.0f;//	1	clear coat normal map/scale
        public float coatNormal
        {
            get { return _coatNormal; }
            set
            {
                propertyChanged = true;
                _coatNormal = value;
            }
        }
        Vector3 _flipflopColor = Vector3.one;//	white	reflectivity of coated flakes at grazing angle, used together with coatColor produces a pearlescent paint
        public Vector3 flipflopColor
        {
            get { return _flipflopColor; }
            set
            {
                propertyChanged = true;
                _flipflopColor = value;
            }
        }
        float _flipflopFalloff = 1.0f;//	1	flip flop color falloff, 1 disables the flip flop effect
        public float flipflopFalloff
        {
            get { return _flipflopFalloff; }
            set
            {
                propertyChanged = true;
                _flipflopFalloff = value;
            }
        }


    }
}