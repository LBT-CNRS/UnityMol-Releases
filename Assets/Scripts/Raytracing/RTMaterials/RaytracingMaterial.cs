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
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using System.Globalization;

namespace UMol {
    public class RaytracingMaterial {

        public bool propertyChanged = false;

        public static Dictionary<string, RaytracingMaterial> materialsBank = new Dictionary<string, RaytracingMaterial>();

        public bool setRTMatProperty(string name, object val) {
            Type t = this.GetType();
            var propertyValues = t.GetProperties();
            if (val.GetType() == typeof(Double)) {
                val = Convert.ToSingle(val);
            }

            foreach (var p in propertyValues) {
                if (val.GetType() == p.PropertyType && name == p.Name) {
                    p.SetValue(this, val);
                    return true;
                }
            }
            return false; //Didn't find it
        }

        public static void recordPresetMaterial() {
            RaytracingMaterial newmat;

            string rtMatName = "default";
            newmat = new RaytracingPrincipledMaterial();
            RaytracingMaterial.materialsBank[rtMatName] = newmat;

            rtMatName = "aluminium";
            newmat = new RaytracingMetalMaterial();
            ((RaytracingMetalMaterial) newmat).eta = new Vector3(1.5f, 0.98f, 0.041f);
            ((RaytracingMetalMaterial) newmat).k = new Vector3(7.6f, 6.6f, 5.4f);
            ((RaytracingMetalMaterial) newmat).roughness = 0.6f;
            RaytracingMaterial.materialsBank[rtMatName] = newmat;

            rtMatName = "gold";
            newmat = new RaytracingMetalMaterial();
            ((RaytracingMetalMaterial) newmat).eta = new Vector3(0.07f, 0.37f, 1.5f);
            ((RaytracingMetalMaterial) newmat).k = new Vector3(3.7f, 2.3f, 1.7f);
            ((RaytracingMetalMaterial) newmat).roughness = 0.2f;
            RaytracingMaterial.materialsBank[rtMatName] = newmat;

            rtMatName = "chrome";
            newmat = new RaytracingMetalMaterial();
            ((RaytracingMetalMaterial) newmat).eta = new Vector3(3.2f, 3.1f, 2.3f);
            ((RaytracingMetalMaterial) newmat).k = new Vector3(3.3f, 3.3f, 3.1f);
            ((RaytracingMetalMaterial) newmat).roughness = 0.2f;
            RaytracingMaterial.materialsBank[rtMatName] = newmat;

            rtMatName = "mirror";
            newmat = new RaytracingMetalMaterial();
            ((RaytracingMetalMaterial) newmat).eta = Vector3.zero;
            ((RaytracingMetalMaterial) newmat).k = Vector3.zero;
            ((RaytracingMetalMaterial) newmat).roughness = 0.2f;
            RaytracingMaterial.materialsBank[rtMatName] = newmat;

            rtMatName = "plastic";
            newmat = new RaytracingPrincipledMaterial();
            ((RaytracingPrincipledMaterial) newmat).roughness = 0.2f;
            ((RaytracingPrincipledMaterial) newmat).metallic = 0.3f;
            RaytracingMaterial.materialsBank[rtMatName] = newmat;

            rtMatName = "translucent";
            newmat = new RaytracingPrincipledMaterial();
            ((RaytracingPrincipledMaterial) newmat).opacity = 0.2f;
            RaytracingMaterial.materialsBank[rtMatName] = newmat;

            rtMatName = "glass1";
            newmat = new RaytracingThinGlassMaterial();
            ((RaytracingThinGlassMaterial) newmat).thickness = 0.2f;
            RaytracingMaterial.materialsBank[rtMatName] = newmat;

            rtMatName = "glass2";
            newmat = new RaytracingThinGlassMaterial();
            ((RaytracingThinGlassMaterial) newmat).thickness = 1.0f;
            ((RaytracingThinGlassMaterial) newmat).eta = 0.9f;
            RaytracingMaterial.materialsBank[rtMatName] = newmat;

            rtMatName = "glass3";
            newmat = new RaytracingThinGlassMaterial();
            ((RaytracingThinGlassMaterial) newmat).attenuationColor = new Vector3(0.85f, 0.95f, 1.0f);
            RaytracingMaterial.materialsBank[rtMatName] = newmat;

            rtMatName = "emissive";
            newmat = new RaytracingLuminousMaterial();
            ((RaytracingLuminousMaterial) newmat).intensity = 2.0f;
            RaytracingMaterial.materialsBank[rtMatName] = newmat;

            if (RaytracedObject.onNewRTMaterial != null) {
                RaytracedObject.onNewRTMaterial(new NewRTMatEventArgs(newmat));
            }
        }

        public string ToJSON(string nameMat = "UMolRTMat") {
            StringBuilder sb = new StringBuilder();

            sb.Append("{\n\t\"family\" : \"OSPRay\",\n\t\"version\" : \"0.0\",\n");
            sb.Append("\t\"notes\" : \"Generated by UnityMol\",\n");
            sb.Append("\t\"materials\" : {\n");

            sb.Append("\t\t\"");
            sb.Append(nameMat);
            sb.Append("\" : {\n");

            sb.Append("\t\t\t\"type\" : \"");
            Type t = this.GetType();
            string rttype = t.ToString().Replace("UMol.Raytracing", "").Replace("Material", "");
            sb.Append(rttype);
            sb.Append("\",\n");
            sb.Append("\t\t\t\"doubles\" : {\n");

            var propertyValues = t.GetProperties();
            foreach (var p in propertyValues) {

                if (p.PropertyType == typeof(Vector3)) {
                    sb.Append("\t\t\t\t\"");
                    sb.Append(p.Name);
                    sb.Append("\" : [");
                    Vector3 v = (Vector3) p.GetValue(this);
                    sb.Append(v.x.ToString("f3", CultureInfo.InvariantCulture));
                    sb.Append(", ");
                    sb.Append(v.y.ToString("f3", CultureInfo.InvariantCulture));
                    sb.Append(", ");
                    sb.Append(v.z.ToString("f3", CultureInfo.InvariantCulture));
                    sb.Append("],\n");
                } else if (p.PropertyType == typeof(float)) {
                    sb.Append("\t\t\t\t\"");
                    sb.Append(p.Name);
                    sb.Append("\" : [");
                    float v = (float) p.GetValue(this);
                    sb.Append(v.ToString("f3", CultureInfo.InvariantCulture));
                    sb.Append("],\n");
                }
            }
            sb.Append("\t\t\t}\n");
            sb.Append("\t\t}\n");
            sb.Append("\t}\n");
            sb.Append("}");

            return sb.ToString();
        }
    }

    public class NewRTMatEventArgs : System.EventArgs {
        public NewRTMatEventArgs(RaytracingMaterial m) {
            this.mat = m;
        }
        public RaytracingMaterial mat { get; private set; }
    }

}