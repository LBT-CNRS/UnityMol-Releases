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
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

namespace UMol
{


public class ReadOSPRayMaterialJson {

    ///Store the materials in the material bank
    public static void readRTMatJson(string path){

		IDictionary deserializedData = null;

        StreamReader sr;

        if (Application.platform == RuntimePlatform.Android) {
            var textStream = new StringReaderStream(AndroidUtils.GetFileText(path));
            sr = new StreamReader(textStream);
        }
        else
            sr = new StreamReader(path);

		using(sr) {
			string jsonString = sr.ReadToEnd();
			deserializedData = (IDictionary) Json.Deserialize(jsonString);
		}

        try{
            IDictionary mats = (IDictionary)deserializedData["materials"];
            foreach(string m in mats.Keys){
                string name = m;
                string type = "principled";
                bool ignore = false;
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                IDictionary matParams = (IDictionary)mats[m];
                foreach(string p in matParams.Keys){
                    if(p == "doubles" || p == "float" || p == "floats"){
                        IDictionary fParams = (IDictionary)matParams[p];
                        foreach(string fp in fParams.Keys){

                            IList tmp = (IList) fParams[fp];
                            List<float> resParams = new List<float>();
                            foreach(var t in tmp){
                                resParams.Add(float.Parse(t.ToString(), System.Globalization.CultureInfo.InvariantCulture));
                            }
                            if(resParams.Count == 1){
                                parameters[fp] = resParams[0];
                            }
                            if(resParams.Count == 3){
                                parameters[fp] = new Vector3(resParams[0], resParams[1], resParams[2]);
                            }
                        }
                    }
                    else if (p != "textures") {//Ignoring textures for now

                        if(p == "type"){
                            type = matParams[p] as string;
                            if(type == "OBJMaterial"){//Ignoring OBJ material for scivis
                                ignore = true;
                                break;
                            }
                        }
                        else{

                            List<float> otherParams = new List<float>();
                            foreach(var t in matParams[p] as IList){
                                otherParams.Add(float.Parse(t.ToString(), System.Globalization.CultureInfo.InvariantCulture));
                            }
                            if(otherParams.Count == 1){
                                parameters[p] = otherParams[0];
                            }
                            if(otherParams.Count == 3){
                                parameters[p] = new Vector3(otherParams[0], otherParams[1], otherParams[2]);
                            }
                        }
                    }
                }
                if(ignore){
                    continue;
                }
                RaytracingMaterial newmat = getRTMat(parameters, type.ToLower());
                RaytracingMaterial.materialsBank[name] = newmat;
                Debug.Log("Adding RT material " + name);
            }
        }
        catch (System.Exception e){
            Debug.LogError("Wrong json format "+e);
            return;
        }
    }

    static RaytracingMaterial getRTMat(Dictionary<string, object> parameters, string lowtype){
        RaytracingMaterial res = null;
        if(lowtype == "principled"){
            res = new RaytracingPrincipledMaterial();
        }
        else if(lowtype == "carpaint"){
            res = new RaytracingCarPaintMaterial();
        }
        else if(lowtype == "metal"){
            res = new RaytracingMetalMaterial();
        }
        else if(lowtype == "alloy"){
            res = new RaytracingAlloyMaterial();
        }
        else if(lowtype == "glass"){
            res = new RaytracingGlassMaterial();
        }
        else if(lowtype == "thinglass"){
            res = new RaytracingThinGlassMaterial();
        }
        else if(lowtype == "metallicpaint"){
            res = new RaytracingMetallicPaintMaterial();
        }
        else if(lowtype == "luminous"){
            res = new RaytracingLuminousMaterial();
        }
        else {
            return res;
        }
        foreach(string k in parameters.Keys){
            res.setRTMatProperty(k, parameters[k]);
        }
        return res;
    }
}

}