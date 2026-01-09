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
public class RaytracingPrincipledMaterial : RaytracingMaterial{

    private Vector3 _baseColor = new Vector3(0.8f, 0.8f, 0.8f);//   white 0.8   base reflectivity (diffuse and/or metallic)
    public Vector3 baseColor {
        get { return _baseColor;}
        set {
            propertyChanged = true;
            _baseColor = value;
        }
    }
    private Vector3 _edgeColor = Vector3.one;//   white   edge tint (metallic only)
    public Vector3 edgeColor {
        get { return _edgeColor;}
        set {
            propertyChanged = true;
            _edgeColor = value;
        }
    }
    private float _metallic = 0.0f;//    0   mix between dielectric (diffuse and/or specular) and metallic (specular only with complex IOR) in [0–1]
    public float metallic {
        get { return _metallic;}
        set {
            propertyChanged = true;
            _metallic = value;
        }
    }
    private float _diffuse = 1.0f;// 1   diffuse reflection weight in [0–1]
    public float diffuse {
        get { return _diffuse;}
        set {
            propertyChanged = true;
            _diffuse = value;
        }
    }
    private float _specular = 1.0f;//    1   specular reflection/transmission weight in [0–1]
    public float specular {
        get { return _specular;}
        set {
            propertyChanged = true;
            _specular = value;
        }
    }
    private float _ior = 1.0f;// 1   dielectric index of refraction
    public float ior {
        get { return _ior; }
        set {
            propertyChanged = true;
            _ior = value;
        }
    }
    private float _transmission = 0.0f;//    0   specular transmission weight in [0–1]
    public float transmission {
        get { return _transmission;}
        set {
            propertyChanged = true;
            _transmission = value;
        }
    }
    private Vector3 _transmissionColor = Vector3.one;//   white   attenuated color due to transmission (Beer’s law)
    public Vector3 transmissionColor {
        get { return _transmissionColor;}
        set {
            propertyChanged = true;
            _transmissionColor = value;
        }
    }
    private float _transmissionDepth = 1.0f;//   1   distance at which color attenuation is equal to transmissionColor
    public float transmissionDepth {
        get { return _transmissionDepth;}
        set {
            propertyChanged = true;
            _transmissionDepth = value;
        }
    }
    private float _roughness = 0.0f;//   0   diffuse and specular roughness in [0–1], 0 is perfectly smooth
    public float roughness {
        get { return _roughness;}
        set {
            propertyChanged = true;
            _roughness = value;
        }
    }
    private float _anisotropy = 0.0f;//  0   amount of specular anisotropy in [0–1]
    public float anisotropy {
        get { return _anisotropy;}
        set {
            propertyChanged = true;
            _anisotropy = value;
        }
    }
    private float _rotation = 0.0f;//    0   rotation of the direction of anisotropy in [0–1], 1 is going full circle
    public float rotation {
        get { return _rotation;}
        set {
            propertyChanged = true;
            _rotation = value;
        }
    }
    private float _normal = 1.0f;//  1   default normal map/scale for all layers
    public float normal {
        get { return _normal;}
        set {
            propertyChanged = true;
            _normal = value;
        }
    }
    private float _baseNormal = 1.0f;//  1   base normal map/scale (overrides default normal)
    public float baseNormal {
        get { return _baseNormal;}
        set {
            propertyChanged = true;
            _baseNormal = value;
        }
    }
    private bool _thin = false;//    false   flag specifying whether the material is thin or solid
    public bool thin {
        get { return _thin; }
        set {
            propertyChanged = true;
            _thin = value;
        }
    }
    private float _thickness = 1.0f;//   1   thickness of the material (thin only), affects the amount of color attenuation due to specular transmission
    public float thickness {
        get { return _thickness;}
        set {
            propertyChanged = true;
            _thickness = value;
        }
    }
    private float _backlight = 0.0f;//   0   amount of diffuse transmission (thin only) in [0–2], 1 is 50% reflection and 50% transmission, 2 is transmission only
    public float backlight {
        get { return _backlight;}
        set {
            propertyChanged = true;
            _backlight = value;
        }
    }
    private float _coat = 0.0f;//    0   clear coat layer weight in [0–1]
    public float coat {
        get { return _coat;}
        set {
            propertyChanged = true;
            _coat = value;
        }
    }
    private float _coatIor = 1.5f;// 1.5 clear coat index of refraction
    public float coatIor {
        get { return _coatIor;}
        set {
            propertyChanged = true;
            _coatIor = value;
        }
    }
    private Vector3 _coatColor = Vector3.one;//   white   clear coat color tint
    public Vector3 coatColor {
        get { return _coatColor;}
        set {
            propertyChanged = true;
            _coatColor = value;
        }
    }
    private float _coatThickness = 1.0f;//   1   clear coat thickness, affects the amount of color attenuation
    public float coatThickness {
        get { return _coatThickness;}
        set {
            propertyChanged = true;
            _coatThickness = value;
        }
    }
    private float _coatRoughness = 0.0f;//   0   clear coat roughness in [0–1], 0 is perfectly smooth
    public float coatRoughness {
        get { return _coatRoughness;}
        set {
            propertyChanged = true;
            _coatRoughness = value;
        }
    }
    private float _coatNormal = 1.0f;//  1   clear coat normal map/scale (overrides default normal)
    public float coatNormal {
        get { return _coatNormal;}
        set {
            propertyChanged = true;
            _coatNormal = value;
        }
    }
    private float _sheen = 0.0f;//   0   sheen layer weight in [0–1]
    public float sheen {
        get { return _sheen;}
        set {
            propertyChanged = true;
            _sheen = value;
        }
    }
    private Vector3 _sheenColor = Vector3.one;//  white   sheen color tint
    public Vector3 sheenColor {
        get { return _sheenColor;}
        set {
            propertyChanged = true;
            _sheenColor = value;
        }
    }
    private float _sheenTint = 0.0f;//   0   how much sheen is tinted from sheenColor toward baseColor
    public float sheenTint {
        get { return _sheenTint;}
        set {
            propertyChanged = true;
            _sheenTint = value;
        }
    }
    private float _sheenRoughness = 0.2f;//  0.2 sheen roughness in [0–1], 0 is perfectly smooth
    public float sheenRoughness {
        get { return _sheenRoughness;}
        set {
            propertyChanged = true;
            _sheenRoughness = value;
        }
    }
    private float _opacity = 1.0f;// 1   cut-out opacity/transparency, 1 is fully opaque
    public float opacity {
        get { return _opacity;}
        set {
            propertyChanged = true;
            _opacity = value;
        }
    }
}
}