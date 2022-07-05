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

public class TrajectoryPlayer : MonoBehaviour {

    public UnityMolStructure s;

    public bool play = false;
    public bool forward = true;
    public bool looping = true;
    public bool forwardAndBack = false;
    public float trajFramerate = 3.0f;

    public bool smoothing = false;

    public bool createUI = true;
    public GameObject trajUI;

    private float timeperiod = 0.0f;

    private bool prevLoop = true;
    private bool prevForward = true;
    private float prevFrameRate = 3.0f;
    private bool sliderChanged = false;


    private Text frameText;
    private Slider frameSlider;
    private Text frameCountText;
    private Toggle loopToggle;
    private Toggle forwardToggle;
    private Slider frameRateSlider;
    private Text framerateText;

    void OnDestroy() {
        if (trajUI) {
            trajUI.SetActive(false);
            Canvas.ForceUpdateCanvases();
            //Force update Canvas of LoadedMoleculesUI parent
            Transform t = trajUI.transform.parent.parent.parent;
            if (t != null && t.GetComponent<RectTransform>() != null) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(t.GetComponent<RectTransform>());
            }
        }
    }

    void Start() {

#if !DISABLE_MAINUI
        //Delay the creation of the reader to let time for the UI to be created
        this.Invoke(doCreateTrajP, 0.1f);
#endif
    }
    void doCreateTrajP() {
        var uiMan = GameObject.FindObjectsOfType<UIManager>();
        if (uiMan.Length == 0) {
            return;
        }
        trajUI = uiMan[0].structureNameToUIObject[s.uniqueName].transform.Find("Trajectory Menu").gameObject;
        trajUI.SetActive(true);
        frameText = trajUI.transform.Find("Row 1/Current Frame").GetComponent<Text>();
        frameCountText = trajUI.transform.Find("Row 1/Frame Count").GetComponent<Text>();
        frameSlider = trajUI.transform.Find("Row 2/Timeline").GetComponent<Slider>();
        loopToggle = trajUI.transform.Find("Row 5/Loop").GetComponent<Toggle>();
        forwardToggle = trajUI.transform.Find("Row 5/ForwardSwitch").GetComponent<Toggle>();
        frameRateSlider = trajUI.transform.Find("Row 4/FrameRate").GetComponent<Slider>();
        framerateText = trajUI.transform.Find("Row 4/Current FrameRate").GetComponent<Text>();


        trajUI.transform.Find("Row 3/Play").GetComponent<Button>().onClick.AddListener(switchPlay);
        loopToggle.onValueChanged.AddListener((value) => {switchLoop(value);});
        trajUI.transform.Find("Row 5/Smooth").GetComponent<Toggle>().onValueChanged.AddListener((value) => {switchSmooth(value);});

        forwardToggle.onValueChanged.AddListener((value) => {switchForward(value);});

        trajUI.transform.Find("Row 5/BackForth").GetComponent<Toggle>().onValueChanged.AddListener((value) => {switchBackForth(value);});

        trajUI.transform.Find("Row 3/Backward").GetComponent<Button>().onClick.AddListener(forcePrevFrame);
        trajUI.transform.Find("Row 3/Forward").GetComponent<Button>().onClick.AddListener(forceNextFrame);

        trajUI.transform.Find("Row 3/Unload").GetComponent<Button>().onClick.AddListener(unloadTrajectory);

        frameSlider.onValueChanged.AddListener(setFrame);

        trajUI.transform.Find("Row 4/FrameRate").GetComponent<Slider>().onValueChanged.AddListener(changeFrameRate);

        updateFrameCount();
        updateFrameNumber();
        updateFramerateValue();
    }

    void switchPlay() {
        play = !play;
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
    void switchSmooth(bool newSmooth) {
        smoothing = newSmooth;
    }

    void Update() {

        if (s.xdr != null && play) {
            sliderChanged = false;
            float invFramerate = 1.0f / trajFramerate;
            if (!smoothing) {
                if (timeperiod > invFramerate) {
                    timeperiod = 0.0f;

                    if (forwardAndBack) {
                        if ((forward && s.xdr.currentFrame + 1 >= s.xdr.numberFrames) || (!forward && s.xdr.currentFrame - 1 < 0)) {
                            forward = !forward;
                        }
                    }

                    s.trajNext(forward, looping);
                }
            }
            else {
                bool newFrame = false;
                if (timeperiod > invFramerate) {
                    timeperiod = 0.0f;
                    newFrame = true;
                }

                if (newFrame && forwardAndBack) {
                    if ((forward && s.xdr.currentFrame + 2 >= s.xdr.numberFrames) || (!forward && s.xdr.currentFrame - 2 < 0)) {
                        forward = !forward;
                    }
                }
                float t = trajFramerate * timeperiod;
                s.trajNextSmooth(t, forward, looping, newFrame);
            }

            //Update UI Part
            if (trajUI != null) {
                updateFrameNumber();

                if (prevLoop != looping) {
                    updateLoopToggle();
                }
                if (prevFrameRate != trajFramerate) {
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

    void updateFrameCount() {
        if (frameCountText == null || frameSlider == null) {
            return;
        }
        frameCountText.text = s.xdr.numberFrames + " frames";
        frameSlider.maxValue = s.xdr.numberFrames;
    }

    void updateFrameNumber() {
        if (frameText == null || frameSlider == null) {
            return;
        }
        frameText.text = "Frame " + s.xdr.currentFrame;
        frameSlider.value = s.xdr.currentFrame;
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
        frameRateSlider.value = trajFramerate;
        framerateText.text = "Speed : " + trajFramerate.ToString("F1");

        prevFrameRate = trajFramerate;
    }
    void forceNextFrame() {
        s.trajNext(true, looping);
        play = false;
        updateFrameNumber();
    }
    void forcePrevFrame() {
        s.trajNext(false, looping);
        play = false;
        updateFrameNumber();
    }
    void changeFrameRate(float newF) {
        trajFramerate = newF;
    }
    void setFrame(float val) {
        if (sliderChanged) {
            float frameNumber = val;
            int idF = (int) frameNumber;
            play = false;
            s.trajSetFrame(idF);
            updateFrameNumber();
        }
    }

    void unloadTrajectory() {
        API.APIPython.unloadTraj(s.uniqueName);
        GameObject.Destroy(this);
    }
}
}