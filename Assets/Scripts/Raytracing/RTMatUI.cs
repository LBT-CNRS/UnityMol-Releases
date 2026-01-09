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
using UMol.API;
using UnityEngine;
using UnityEngine.UI;

namespace UMol {
    public class RTMatUI : MonoBehaviour {

        public GameObject mainPanel;

        public Dropdown selectionDropD;
        public Dropdown repDropD;
        public Dropdown matTypeDropD;

        public GameObject principledGo;
        public GameObject metalGo;
        public GameObject carPaintGo;
        public GameObject alloyGo;
        public GameObject glassGo;
        public GameObject thinGlassGo;
        public GameObject metallicPaintGo;
        public GameObject luminousGo;

        public bool shouldEnable = false;

        UnityMolSelectionManager selM;
        int curSelCount = 0;
        bool updateSelList = false;
        bool updateRepList = false;

        void Start() {
            selM = UnityMolMain.getSelectionManager();
            SetListeners();
        }
        void OnEnable() {
            UnityMolStructureManager.OnMoleculeLoaded += shouldUpdateSelList;
            UnityMolStructureManager.OnMoleculeDeleted += shouldUpdateSelList;
            UnityMolRepresentationManager.OnRepresentationVisibility += shouldUpdateRepList;
        }

        void OnDisable() {
            UnityMolStructureManager.OnMoleculeLoaded -= shouldUpdateSelList;
            UnityMolStructureManager.OnMoleculeDeleted -= shouldUpdateSelList;
            UnityMolRepresentationManager.OnRepresentationVisibility -= shouldUpdateRepList;
        }
        void shouldUpdateSelList() {
            updateSelList = true;
        }
        void shouldUpdateRepList() {
            updateRepList = true;
        }

        public void setActivePanel(bool a){
            shouldEnable = a;
        }
        void Update() {

            if(UnityMolMain.raytracingMode){
                if(shouldEnable && !mainPanel.activeInHierarchy){
                    mainPanel.SetActive(true);
                }
                else if(!shouldEnable && mainPanel.activeInHierarchy){
                    mainPanel.SetActive(false);
                }
            }
            else{
                if(mainPanel.activeInHierarchy)
                    mainPanel.SetActive(false);
            }
            if (curSelCount != selM.selections.Count) {
                updateSelList = true;
                curSelCount = selM.selections.Count;
            }
            if (updateSelList) {
                updateSelList = false;
                fillSelList();
                curSelCount = selM.selections.Count;
            }
            if (updateRepList) {
                updateRepList = false;
                fillReplistForSelection();
            }
        }

        void SetListeners() {
            List<GameObject> objs = new List<GameObject> {
                principledGo,
                metalGo,
                carPaintGo,
                alloyGo,
                glassGo,
                thinGlassGo,
                metallicPaintGo,
                luminousGo
            };
            foreach (GameObject go in objs) {
                foreach (Transform t in go.transform) {
                    if (t.name.StartsWith("Vec3_")) {
                        Slider sx = t.Find("Sliders/SliderR").gameObject.GetComponent<Slider>();
                        Slider sy = t.Find("Sliders/SliderG").gameObject.GetComponent<Slider>();
                        Slider sz = t.Find("Sliders/SliderB").gameObject.GetComponent<Slider>();

                        InputField ifx = t.Find("Texts/InputField1").gameObject.GetComponent<InputField>();
                        InputField ify = t.Find("Texts/InputField2").gameObject.GetComponent<InputField>();
                        InputField ifz = t.Find("Texts/InputField3").gameObject.GetComponent<InputField>();

                        string propName = t.name.Replace("Vec3_", "");
                        sx.onValueChanged.AddListener(
                            delegate {
                                changeRTMatProperty(propName, sx.value, sy.value, sz.value);
                                ifx.SetValue(sx.value.ToString("f3"));
                            });
                        sy.onValueChanged.AddListener(
                            delegate {
                                changeRTMatProperty(propName, sx.value, sy.value, sz.value);
                                ify.SetValue(sy.value.ToString("f3"));
                            });
                        sz.onValueChanged.AddListener(
                            delegate {
                                changeRTMatProperty(propName, sx.value, sy.value, sz.value);
                                ifz.SetValue(sz.value.ToString("f3"));
                            });

                        ifx.onValueChanged.AddListener(
                            delegate {
                                sx.SetValue(float.Parse(ifx.text));
                                changeRTMatProperty(propName, sx.value, sy.value, sz.value);
                            });

                        ify.onValueChanged.AddListener(
                            delegate {
                                sy.SetValue(float.Parse(ify.text));
                                changeRTMatProperty(propName, sx.value, sy.value, sz.value);
                            });
                        ifz.onValueChanged.AddListener(
                            delegate {
                                sz.SetValue(float.Parse(ifz.text));
                                changeRTMatProperty(propName, sx.value, sy.value, sz.value);
                            });

                    } else if (t.name.StartsWith("Slider_")) {
                        Slider s = t.Find("Slider").gameObject.GetComponent<Slider>();
                        InputField if1 = t.Find("InputField").gameObject.GetComponent<InputField>();
                        string propName = t.name.Replace("Slider_", "");
                        s.onValueChanged.AddListener(
                            delegate {
                                if1.SetValue(s.value.ToString("f3"));
                                changeRTMatProperty(propName, s.value);
                            });
                        if1.onValueChanged.AddListener(
                            delegate {
                                s.SetValue(float.Parse(if1.text));
                                changeRTMatProperty(propName, s.value);
                            });

                    } else if (t.name.StartsWith("Toggle_")) {
                        Toggle tog = t.Find("Toggle").gameObject.GetComponent<Toggle>();
                        string propName = t.name.Replace("Toggle_", "");
                        tog.onValueChanged.AddListener(
                            delegate {
                                changeRTMatProperty(propName, tog.isOn);
                            });
                    }
                }
            }
        }


        //Get the list of selections
        public void fillSelList() {
            selectionDropD.ClearOptions();
            List<string> opts = new List<string>(selM.selections.Count + 1);
            opts.Add("Selections");
            foreach (var seln in selM.selections.Keys) {
                opts.Add(seln);
            }
            selectionDropD.AddOptions(opts);
        }

        //Get the list of representations for the choosed selection
        public void fillReplistForSelection() {

            if (selectionDropD.value != 0) { //Check if selection is choosed

                string selName = selectionDropD.options[selectionDropD.value].text;
                UnityMolSelection sel = selM.selections[selName];
                repDropD.ClearOptions();
                List<string> opts = new List<string>(sel.representations.Keys.Count + 1);
                opts.Add("Representations");
                foreach (RepType rt in sel.representations.Keys) {
                    opts.Add(APIPython.getTypeFromRepType(rt));
                }
                repDropD.AddOptions(opts);
            }
            getCurrentRTMaterialType(); //Clear the RT property panel
        }
        //Get the RaytracingMaterial type from the RaytracedObject and update the panel
        public void getCurrentRTMaterialType() {
            if (selectionDropD.value == 0 || repDropD.value == 0) {
                matTypeDropD.SetValue(0);
                changeRTMaterialType(false);
                return;
            }

            UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
            string selName = selectionDropD.options[selectionDropD.value].text;
            string displayedRepType = repDropD.options[repDropD.value].text;
            RepType curRepType = APIPython.getRepType(displayedRepType);
            int rtmattype = -1;
            bool found = false;
            if (selM.selections.ContainsKey(selName)) {
                if (curRepType.atomType != AtomType.noatom || curRepType.bondType != BondType.nobond) {
                    List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, curRepType);
                    if (existingReps != null) {
                        foreach (UnityMolRepresentation existingRep in existingReps) {
                            foreach (SubRepresentation sr in existingRep.subReps) {
                                if (sr.atomRepManager != null) {
                                    rtmattype = sr.atomRepManager.GetRTMaterialType();
                                    if (rtmattype != -1) {
                                        found = true;
                                        break;
                                    }
                                }
                                if (sr.bondRepManager != null) {
                                    rtmattype = sr.bondRepManager.GetRTMaterialType();
                                    if (rtmattype != -1) {
                                        found = true;
                                        break;
                                    }
                                }
                            }
                            if (found)
                                break;
                        }
                    }
                }
            }
            if (rtmattype == -1) {
                matTypeDropD.SetValue(0);
                changeRTMaterialType(false);
            } else if (rtmattype + 1 < matTypeDropD.options.Count) {
                matTypeDropD.SetValue(rtmattype + 1);
                changeRTMaterialType(false);
            }
        }
        //Show the corresponding UI panel for the selected RT material
        public void changeRTMaterialType(bool apply = true) {
            principledGo.SetActive(false);
            metalGo.SetActive(false);
            carPaintGo.SetActive(false);
            alloyGo.SetActive(false);
            glassGo.SetActive(false);
            thinGlassGo.SetActive(false);
            metallicPaintGo.SetActive(false);
            luminousGo.SetActive(false);

            if (selectionDropD.value != 0 && repDropD.value != 0) {

                string choice = matTypeDropD.options[matTypeDropD.value].text;
                int rtmatType = 0; //Principled
                switch (choice) {
                    case "Principled":
                        principledGo.SetActive(true);
                        break;
                    case "Metal":
                        rtmatType = 2;
                        metalGo.SetActive(true);
                        break;
                    case "CarPaint":
                        rtmatType = 1;
                        carPaintGo.SetActive(true);
                        break;
                    case "Alloy":
                        rtmatType = 3;
                        alloyGo.SetActive(true);
                        break;
                    case "Glass":
                        rtmatType = 4;
                        glassGo.SetActive(true);
                        break;
                    case "ThinGlass":
                        rtmatType = 5;
                        thinGlassGo.SetActive(true);
                        break;
                    case "MetallicPaint":
                        rtmatType = 6;
                        metallicPaintGo.SetActive(true);
                        break;
                    case "Luminous":
                        rtmatType = 7;
                        luminousGo.SetActive(true);
                        break;
                }
                if (apply) {
                    string selName = selectionDropD.options[selectionDropD.value].text;
                    string displayedRepType = repDropD.options[repDropD.value].text;
                    APIPython.setRTMaterialType(selName, displayedRepType, rtmatType);
                }

            } else {
                matTypeDropD.SetValue(0);
            }
        }


        RaytracedObject getRTO() {
            if (selectionDropD.value == 0 || repDropD.value == 0) {
                return null;
            }

            UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
            string selName = selectionDropD.options[selectionDropD.value].text;
            string displayedRepType = repDropD.options[repDropD.value].text;
            RepType curRepType = APIPython.getRepType(displayedRepType);
            if (selM.selections.ContainsKey(selName)) {
                if (curRepType.atomType != AtomType.noatom || curRepType.bondType != BondType.nobond) {
                    List<UnityMolRepresentation> existingReps = repManager.representationExists(selName, curRepType);
                    if (existingReps != null) {
                        foreach (UnityMolRepresentation existingRep in existingReps) {
                            foreach (SubRepresentation sr in existingRep.subReps) {
                                if (sr.atomRepManager != null &&
                                    sr.atomRepManager.rtos != null &&
                                    sr.atomRepManager.rtos.Count != 0) {
                                    return sr.atomRepManager.rtos[0];
                                }
                                if (sr.bondRepManager != null &&
                                    sr.bondRepManager.rtos != null &&
                                    sr.bondRepManager.rtos.Count != 0) {
                                    return sr.bondRepManager.rtos[0];
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        //Get all the parameters from the RaytracingMaterial and update slider values
        public void updateMaterialProperties() {
            if (matTypeDropD.value == 0)
                return;
            RaytracedObject rto = getRTO();
            if (rto != null) {
                Type t = rto.rtMat.GetType();
                var propertyValues = t.GetProperties();

                GameObject rtmatPar = getCurRTMatGo();
                if (rtmatPar == null)
                    return;
                foreach (var p in propertyValues) {
                    if (p.PropertyType == typeof(bool) && p.Name == "propertyChanged") {
                        continue;
                    }
                    updateDisplayedProperty(rtmatPar, p.Name, p.GetValue(rto.rtMat));
                }
            }
        }
        GameObject getCurRTMatGo() {
            string choice = matTypeDropD.options[matTypeDropD.value].text;
            switch (choice) {
                case "Principled":
                    return principledGo;
                case "Metal":
                    return metalGo;
                case "CarPaint":
                    return carPaintGo;
                case "Alloy":
                    return alloyGo;
                case "Glass":
                    return glassGo;
                case "ThinGlass":
                    return thinGlassGo;
                case "MetallicPaint":
                    return metallicPaintGo;
                case "Luminous":
                    return luminousGo;
            }
            return null;
        }

        void updateDisplayedProperty(GameObject rtmatPar, string propName, object val) {

            if (val.GetType() == typeof(Vector3)) {
                Transform t = rtmatPar.transform.Find("Vec3_" + propName + "/Sliders");
                float x, y, z;
                x = ((Vector3) val).x;
                y = ((Vector3) val).y;
                z = ((Vector3) val).z;

                t.Find("SliderR").gameObject.GetComponent<Slider>().SetValue(x);
                t.Find("SliderG").gameObject.GetComponent<Slider>().SetValue(y);
                t.Find("SliderB").gameObject.GetComponent<Slider>().SetValue(z);
            } else if (val.GetType() == typeof(bool)) {
                Transform t = rtmatPar.transform.Find("Toggle_" + propName + "/Toggle");
                t.gameObject.GetComponent<Toggle>().SetValue((bool) val);

            } else if (val.GetType() == typeof(float)) {
                Transform t = rtmatPar.transform.Find("Slider_" + propName + "/Slider");
                t.gameObject.GetComponent<Slider>().SetValue((float) val);
            }
        }

        void changeRTMatProperty(string propName, float v) {
            string selName = selectionDropD.options[selectionDropD.value].text;
            string displayedRepType = repDropD.options[repDropD.value].text;
            RepType curRepType = APIPython.getRepType(displayedRepType);

            string rtype = APIPython.getTypeFromRepType(curRepType);
            APIPython.setRTMaterialProperty(selName, rtype, propName, v);
        }

        void changeRTMatProperty(string propName, float vx, float vy, float vz) {
            string selName = selectionDropD.options[selectionDropD.value].text;
            string displayedRepType = repDropD.options[repDropD.value].text;
            RepType curRepType = APIPython.getRepType(displayedRepType);

            Vector3 v = new Vector3(vx, vy, vz);
            string rtype = APIPython.getTypeFromRepType(curRepType);
            APIPython.setRTMaterialProperty(selName, rtype, propName, v);
        }

        void changeRTMatProperty(string propName, bool v) {
            string selName = selectionDropD.options[selectionDropD.value].text;
            string displayedRepType = repDropD.options[repDropD.value].text;
            RepType curRepType = APIPython.getRepType(displayedRepType);

            string rtype = APIPython.getTypeFromRepType(curRepType);
            APIPython.setRTMaterialProperty(selName, rtype, propName, v);
        }
        public void saveRTMatToFile() {
            if (selectionDropD.value == 0 || repDropD.value == 0) {
                return;
            }
            RaytracedObject rto = getRTO();
            if (rto != null) {
                var mat = rto.rtMat;
                string content = mat.ToJSON();
                var rsf = (ReadSaveFilesWithBrowser) FindObjectOfType(typeof(ReadSaveFilesWithBrowser));
                if(rsf == null){
                    rsf = gameObject.AddComponent<ReadSaveFilesWithBrowser>();
                }
                rsf.SaveJson(content);
            }
        }
    }
}
