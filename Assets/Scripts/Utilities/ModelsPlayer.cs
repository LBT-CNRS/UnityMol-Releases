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

    public GameObject modelUI;

    private float timeperiod = 0.0f;

    private bool prevLoop = true;
    private bool prevForward = true;
    private float prevFrameRate = 3.0f;
    private bool sliderChanged = false;

    private Text modelText;
    private Slider modelSlider;
    private Text modelCountText;
    private Toggle loopToggle;
    private Toggle forwardToggle;
    private Slider frameRateSlider;
    private Text framerateText;

    void OnDestroy() {
        if (modelUI) {
            modelUI.SetActive(false);
            Canvas.ForceUpdateCanvases();
            //Force update Canvas of LoadedMoleculesUI parent
            Transform t = modelUI.transform.parent.parent.parent;
            if (t != null && t.GetComponent<RectTransform>() != null) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(t.GetComponent<RectTransform>());
            }
        }
    }

    void Start() {
        //Makes sure the UI is created before accessing it
        Invoke("Init", 1);
    }

    void Init(){
#if !DISABLE_MAINUI
        var uiMan = GameObject.FindObjectsOfType<UIManager>();
        if (uiMan.Length == 0) {
            return;
        }
        if(!uiMan[0].structureNameToUIObject.ContainsKey(s.uniqueName)){
            return;
        }
        modelUI = uiMan[0].structureNameToUIObject[s.uniqueName].transform.Find("Model Menu").gameObject;
        modelUI.SetActive(true);
        modelText = modelUI.transform.Find("Row 1/Current Model").GetComponent<Text>();
        modelCountText = modelUI.transform.Find("Row 1/Model Count").GetComponent<Text>();
        modelSlider = modelUI.transform.Find("Row 2/Timeline").GetComponent<Slider>();
        loopToggle = modelUI.transform.Find("Row 5/Loop").GetComponent<Toggle>();
        forwardToggle = modelUI.transform.Find("Row 5/ForwardSwitch").GetComponent<Toggle>();
        frameRateSlider = modelUI.transform.Find("Row 4/FrameRate").GetComponent<Slider>();
        framerateText = modelUI.transform.Find("Row 4/Current FrameRate").GetComponent<Text>();


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
    }
    void switchForward(bool newF) {
        forward = newF;
    }
    void switchBackForth(bool newS) {
        forwardAndBack = newS;
    }

    void Update() {

        if (play) {
            sliderChanged = false;
            float invFramerate = 1.0f / modelFramerate;

            if (timeperiod > invFramerate) {
                timeperiod = 0.0f;

                if (forwardAndBack) {
                    if(s.trajectoryMode){
                        if ((forward && s.currentFrameId + 1 >= s.modelFrames.Count) || (!forward && s.currentFrameId - 1 < 0)) {
                            forward = !forward;
                        }
                    }
                    else{
                        if ((forward && s.currentModelId + 1 >= s.models.Count) || (!forward && s.currentModelId - 1 < 0)) {
                            forward = !forward;
                        }
                    }
                }

                s.modelNext(forward, looping);
            }
        

            //Update UI Part
            if (modelUI != null) {
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
        if (modelCountText == null || modelSlider == null) {
            return;
        }
        if(s.trajectoryMode && s.modelFrames != null && s.modelFrames.Count > 1){
            modelCountText.text = s.modelFrames.Count + " frames";
            modelSlider.maxValue = s.modelFrames.Count - 1;
        }
        else{
            modelCountText.text = s.models.Count + " models";
            modelSlider.maxValue = s.models.Count;            
        }
    }

    void updateModelNumber() {
        if (modelText == null || modelSlider == null) {
            return;
        }
        if(s.trajectoryMode && s.modelFrames != null && s.modelFrames.Count > 1){
            modelText.text = "Model " + s.currentFrameId;
            modelSlider.value = s.currentFrameId;
        }
        else{
            modelText.text = "Model " + s.currentModelId;
            modelSlider.value = s.currentModelId;
        }
    }
    void updateLoopToggle() {
        if (loopToggle == null) {
            return;
        }
        loopToggle.isOn = looping;
        prevLoop = looping;
    }

    void updateForwardToggle() {
        if (forwardToggle == null) {
            return;
        }
        forwardToggle.isOn = forward;
        prevForward = forward;
    }
    void updateFramerateValue() {
        if (frameRateSlider == null) {
            return;
        }
        frameRateSlider.value = modelFramerate;
        framerateText.text = "Speed : " + modelFramerate.ToString("F1");

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
            API.APIPython.setModel(s.uniqueName, idF);
        }
    }
}
}