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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UMol {
public class ModelsPlayer : MonoBehaviour {

    public UnityMolStructure s;

    public bool play = false;
    public bool forward = true;
    public bool looping = true;
    public bool forwardAndBack = false;
    public float modelFramerate = 3.0f;

    public List<GameObject> modelUIs;

    private float timeperiod = 0.0f;

    private bool prevLoop = true;
    private bool prevForward = true;
    private float prevFrameRate = 3.0f;
    private bool sliderChanged = false;

    private List<Text> modelTexts;
    private List<Slider> modelSliders;
    private List<Text> modelCountTexts;
    private List<Toggle> loopToggles;
    private List<Toggle> forwardToggles;
    private List<Slider> frameRateSliders;
    private List<Text> framerateTexts;
    private List<Toggle> forwardAndBackToggles;

    void OnDestroy() {
        foreach (GameObject modelUI in modelUIs) {

            modelUI.SetActive(false);
            Canvas.ForceUpdateCanvases();
            //Force update Canvas of LoadedMoleculesUI parent
            Transform t = modelUI.transform.parent.parent.parent;
            if (t != null && t.GetComponent<RectTransform>() != null) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(t.GetComponent<RectTransform>());
            }
        }
        modelUIs.Clear();
        modelTexts.Clear();
        modelSliders.Clear();
        modelCountTexts.Clear();
        loopToggles.Clear();
        forwardToggles.Clear();
        frameRateSliders.Clear();
        framerateTexts.Clear();
        forwardAndBackToggles.Clear();
    }

    void Start() {
        modelUIs = new List<GameObject>();
        modelTexts = new List<Text>();
        modelSliders = new List<Slider>();
        modelCountTexts = new List<Text>();
        loopToggles = new List<Toggle>();
        forwardToggles = new List<Toggle>();
        frameRateSliders = new List<Slider>();
        framerateTexts = new List<Text>();
        forwardAndBackToggles = new List<Toggle>();

        //Makes sure the UI is created before accessing it
        Invoke("Init", 1);
    }

    void Init() {
#if !DISABLE_MAINUI
        var uiMans = GameObject.FindObjectsOfType<UIManager>();
        if (uiMans.Length == 0) {
            return;
        }
        foreach (var uiMan in uiMans) {
            if (!uiMan.structureNameToUIObject.ContainsKey(s.name)) {
                continue;
            }
            GameObject modelUI = uiMan.structureNameToUIObject[s.name].transform.Find("Model Menu").gameObject;
            modelUIs.Add(modelUI);
            modelUI.SetActive(true);
            Text modelText = modelUI.transform.Find("Row 1/Current Model").GetComponent<Text>();
            modelTexts.Add(modelText);
            Text modelCountText = modelUI.transform.Find("Row 1/Model Count").GetComponent<Text>();
            modelCountTexts.Add(modelCountText);
            Slider modelSlider = modelUI.transform.Find("Row 2/Timeline").GetComponent<Slider>();
            modelSliders.Add(modelSlider);
            Toggle loopToggle = modelUI.transform.Find("Row 5/Loop").GetComponent<Toggle>();
            loopToggles.Add(loopToggle);
            Toggle forwardToggle = modelUI.transform.Find("Row 5/ForwardSwitch").GetComponent<Toggle>();
            forwardToggles.Add(forwardToggle);
            Toggle forwardAndBackToggle = modelUI.transform.Find("Row 5/BackForth").GetComponent<Toggle>();
            forwardAndBackToggles.Add(forwardAndBackToggle);
            Slider frameRateSlider = modelUI.transform.Find("Row 4/FrameRate").GetComponent<Slider>();
            frameRateSliders.Add(frameRateSlider);
            Text framerateText = modelUI.transform.Find("Row 4/Current FrameRate").GetComponent<Text>();
            framerateTexts.Add(framerateText);


            modelUI.transform.Find("Row 3/Play").GetComponent<Button>().onClick.AddListener(switchPlay);
            loopToggle.onValueChanged.AddListener((value) => {switchLoop(value);});

            forwardToggle.onValueChanged.AddListener((value) => {switchForward(value);});

            modelUI.transform.Find("Row 5/BackForth").GetComponent<Toggle>().onValueChanged.AddListener((value) => {switchBackForth(value);});

            modelUI.transform.Find("Row 3/Backward").GetComponent<Button>().onClick.AddListener(forcePrevFrame);
            modelUI.transform.Find("Row 3/Forward").GetComponent<Button>().onClick.AddListener(forceNextFrame);

            modelSlider.onValueChanged.AddListener(setModel);

            modelUI.transform.Find("Row 4/FrameRate").GetComponent<Slider>().onValueChanged.AddListener(changeFrameRate);

            updateModelCount();
            updateModelNumber();
            updateFramerateValue();

            LayoutRebuilder.ForceRebuildLayoutImmediate(modelUI.transform.parent.parent.gameObject.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(modelUI.transform.parent.parent.parent.gameObject.GetComponent<RectTransform>());
        }
#endif

    }

    void switchPlay() {
        play = !play;
// #if !DISABLE_HIGHLIGHT
//         UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();
//         if(!play){
//             hM.updateHighlightedAtomPositions();
//             hM.show();
//         }
//         else{
//             hM.show(false);
//         }
// #endif

    }
    void switchLoop(bool newL) {
        looping = newL;
        foreach (var l in loopToggles) {
            l.SetValue(newL);
        }
    }
    void switchForward(bool newF) {
        forward = newF;
        foreach (var l in forwardToggles) {
            l.SetValue(newF);
        }
    }
    void switchBackForth(bool newS) {
        forwardAndBack = newS;
        foreach (var l in forwardAndBackToggles) {
            l.SetValue(newS);
        }
    }

    void Update() {

        if (play) {
            sliderChanged = false;
            float invFramerate = 1.0f / modelFramerate;

            if (timeperiod > invFramerate) {
                timeperiod = 0.0f;

                if (forwardAndBack) {
                    if (s.trajectoryMode) {
                        if ((forward && s.currentFrameId + 1 >= s.modelFrames.Count) || (!forward && s.currentFrameId - 1 < 0)) {
                            forward = !forward;
                        }
                    }
                    else {
                        if ((forward && s.currentModelId + 1 >= s.models.Count) || (!forward && s.currentModelId - 1 < 0)) {
                            forward = !forward;
                        }
                    }
                }

                s.modelNext(forward, looping);
            }


            //Update UI Part
            if (modelUIs != null && modelUIs.Count > 0) {
                updateModelNumber();

                if (prevLoop != looping) {
                    updateLoopToggle();
                }
                if (prevFrameRate != modelFramerate) {
                    updateFramerateValue();
                }
                if (prevForward != forward) {
                    updateForwardToggle();
                }
                sliderChanged = true;
            }
            timeperiod += UnityEngine.Time.deltaTime;

        }
    }

    void updateModelCount() {
        if (modelCountTexts == null || modelSliders == null) {
            return;
        }

        if (s.trajectoryMode && s.modelFrames != null && s.modelFrames.Count > 1) {

            foreach (var mct in modelCountTexts) {
                mct.text = s.modelFrames.Count + " frames";
            }
            foreach (var ms in modelSliders) {
                ms.maxValue = s.modelFrames.Count - 1;
            }
        }
        else {
            foreach (var mct in modelCountTexts) {
                mct.text = s.models.Count + " frames";
            }
            foreach (var ms in modelSliders) {
                ms.maxValue = s.models.Count - 1;
            }
        }
    }

    void updateModelNumber() {
        if (modelTexts == null || modelSliders == null) {
            return;
        }
        if (s.trajectoryMode && s.modelFrames != null && s.modelFrames.Count > 1) {

            foreach (var mt in modelTexts) {
                mt.text = "Model " + s.currentFrameId;
            }
            foreach (var ms in modelSliders) {
                ms.value = s.currentFrameId;
            }
        }
        else {

            foreach (var mt in modelTexts) {
                mt.text = "Model " + s.currentModelId;
            }
            foreach (var ms in modelSliders) {
                ms.value = s.currentModelId;
            }
        }
    }
    void updateLoopToggle() {
        if (loopToggles == null) {
            return;
        }
        foreach (var lt in loopToggles) {
            lt.isOn = looping;
        }
        prevLoop = looping;
    }

    void updateForwardToggle() {
        if (forwardToggles == null) {
            return;
        }
        foreach (var ft in forwardToggles) {
            ft.isOn = forward;
        }
        prevForward = forward;
    }
    void updateFramerateValue() {
        if (frameRateSliders == null) {
            return;
        }

        foreach (var frs in frameRateSliders)
            frs.value = modelFramerate;

        foreach (var ft in framerateTexts)
            ft.text = "Speed : " + modelFramerate.ToString("F1");

        prevFrameRate = modelFramerate;
    }
    void forceNextFrame() {
        s.modelNext(true, looping);
        play = false;
        updateModelNumber();
    }
    void forcePrevFrame() {
        s.modelNext(false, looping);
        play = false;
        updateModelNumber();
    }
    void changeFrameRate(float newF) {
        modelFramerate = newF;
    }
    void setModel(float val) {
        if (sliderChanged) {
            float frameNumber = val;
            int idF = (int) frameNumber;
            play = false;
            API.APIPython.setModel(s.name, idF);
        }
    }
}
}