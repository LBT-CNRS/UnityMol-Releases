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
using UnityEngine;
using UnityEngine.UI;

namespace UMol {

/// <summary>
/// Global class to handle a trajectory file :
///   - parse the file & set the objects
///   - Manage the UI elements
/// </summary>
public class TrajectoryPlayer : MonoBehaviour {

    /// <summary>
    /// Structure linked to the trajectory
    /// Assumed a trajectory is already set for this structure.
    /// </summary>
    public UnityMolStructure s;

    // List of booleans linked to the UI elements to interact with the trajectory
    public bool play = false;
    public bool forward = true;
    public bool looping = true;
    public bool forwardAndBack = false;
    public float trajFramerate = 10.0f;
    public bool average = false;
    public int windowSize = 5;
    public bool smoothing = false;

    /// <summary>
    /// List of UI trajectory Game objects
    /// </summary>
    public List<GameObject> trajUIs;



    private float timeperiod;
    private bool prevLoop = true;
    private bool prevForward = true;
    private float prevFrameRate = 3.0f;
    private bool sliderChanged = true;
    private int prevWindowSize = 5;


    //List of UI elements
    private List<Text> frameTexts;
    private List<Slider> frameSliders;
    private List<Text> frameCountTexts;
    private List<Toggle> loopToggles;
    private List<Toggle> forwardToggles;
    private List<Toggle> forwardAndBackToggles;
    private List<Slider> frameRateSliders;
    private List<Text> framerateTexts;
    private List<Toggle> smoothToggles;
    private List<Toggle> averageToggles;
    private List<Text> windowSizeTexts;


    private void Start() {
        trajUIs = new List<GameObject>();

        frameTexts = new List<Text>();
        frameSliders = new List<Slider>();
        frameCountTexts = new List<Text>();
        loopToggles = new List<Toggle>();
        forwardToggles = new List<Toggle>();
        forwardAndBackToggles = new List<Toggle>();
        frameRateSliders = new List<Slider>();
        framerateTexts = new List<Text>();
        smoothToggles = new List<Toggle>();
        averageToggles = new List<Toggle>();
        windowSizeTexts = new List<Text>();

#if !DISABLE_MAINUI
        //Delay the creation of the reader to let time for the UI to be created
        this.Invoke(doCreateTrajP, 0.1f);
#endif
    }

    /// <summary>
    /// Activate the UI Menu for trajectories & initialize the private attributes of the UI elements
    /// Set also the callbacks when interacting with the UI.
    /// </summary>
    private void doCreateTrajP() {
        UIManager[] uiMans = FindObjectsOfType<UIManager>();
        if (uiMans.Length == 0) {
            return;
        }
        foreach (UIManager uiMan in uiMans) {
            GameObject curTrajUI = uiMan.structureNameToUIObject[s.name].transform.Find("Trajectory Menu").gameObject;
            trajUIs.Add(curTrajUI);
            curTrajUI.SetActive(true);
            Text frameText = curTrajUI.transform.Find("Row 1/Current Frame").GetComponent<Text>();
            frameTexts.Add(frameText);
            Text frameCountText = curTrajUI.transform.Find("Row 1/Frame Count").GetComponent<Text>();
            frameCountTexts.Add(frameCountText);
            Slider frameSlider = curTrajUI.transform.Find("Row 2/Timeline").GetComponent<Slider>();
            frameSliders.Add(frameSlider);
            Toggle loopToggle = curTrajUI.transform.Find("Row 5/Loop").GetComponent<Toggle>();
            loopToggles.Add(loopToggle);
            Toggle forwardToggle = curTrajUI.transform.Find("Row 5/ForwardSwitch").GetComponent<Toggle>();
            forwardToggles.Add(forwardToggle);
            Toggle forwardAndBackToggle = curTrajUI.transform.Find("Row 5/BackForth").GetComponent<Toggle>();
            forwardAndBackToggles.Add(forwardAndBackToggle);
            Slider frameRateSlider = curTrajUI.transform.Find("Row 4/FrameRate").GetComponent<Slider>();
            frameRateSliders.Add(frameRateSlider);
            Text framerateText = curTrajUI.transform.Find("Row 4/Current FrameRate").GetComponent<Text>();
            framerateTexts.Add(framerateText);
            Toggle smoothToggle = curTrajUI.transform.Find("Row 5/Smooth").GetComponent<Toggle>();
            smoothToggles.Add(smoothToggle);

            Toggle averageToggle = curTrajUI.transform.Find("Row 6/DoAverage").GetComponent<Toggle>();
            averageToggles.Add(averageToggle);

            curTrajUI.transform.Find("Row 3/Play").GetComponent<Button>().onClick.AddListener(switchPlay);
            loopToggle.onValueChanged.AddListener(switchLoop);
            curTrajUI.transform.Find("Row 5/Smooth").GetComponent<Toggle>().onValueChanged.AddListener(switchSmooth);

            forwardToggle.onValueChanged.AddListener(switchForward);

            curTrajUI.transform.Find("Row 5/BackForth").GetComponent<Toggle>().onValueChanged.AddListener(switchBackForth);

            curTrajUI.transform.Find("Row 3/Backward").GetComponent<Button>().onClick.AddListener(forcePrevFrame);
            curTrajUI.transform.Find("Row 3/Forward").GetComponent<Button>().onClick.AddListener(forceNextFrame);

            curTrajUI.transform.Find("Row 3/Unload").GetComponent<Button>().onClick.AddListener(unloadTrajectory);

            frameSlider.onValueChanged.AddListener(setFrame);

            curTrajUI.transform.Find("Row 4/FrameRate").GetComponent<Slider>().onValueChanged.AddListener(changeFrameRate);

            curTrajUI.transform.Find("Row 6/DoAverage").GetComponent<Toggle>().onValueChanged.AddListener(switchAverage);

            Text windowSizeText = curTrajUI.transform.Find("Row 6/WindowSize/Image/Size").GetComponent<Text>();
            windowSizeTexts.Add(windowSizeText);
            curTrajUI.transform.Find("Row 6/WindowSize/ButtonMinus").GetComponent<Button>().onClick.AddListener(() => {changeWindowSize(windowSize - 1);});
            curTrajUI.transform.Find("Row 6/WindowSize/ButtonPlus").GetComponent<Button>().onClick.AddListener(() => {changeWindowSize(windowSize + 1);});


            updateFrameCount();
            updateFrameNumber();
            updateFramerateValue();
            updateWindowSizeValue();


            LayoutRebuilder.ForceRebuildLayoutImmediate(curTrajUI.transform.parent.parent.gameObject.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(curTrajUI.transform.parent.parent.parent.gameObject.GetComponent<RectTransform>());
        }
    }

    /// <summary>
    /// Callback when hit the Play button
    /// </summary>
    private void switchPlay() {
        play = !play;
    }

    /// <summary>
    /// Callback when hit the Loop button
    /// </summary>
    /// <param name="newL">New value</param>
    private void switchLoop(bool newL) {
        looping = newL;
        foreach (Toggle l in loopToggles) {
            l.SetValue(newL);
        }
    }

    /// <summary>
    /// Callback when hit the Forward button
    /// </summary>
    /// <param name="newF">New value</param>
    private void switchForward(bool newF) {
        forward = newF;
        foreach (Toggle l in forwardToggles) {
            l.SetValue(newF);
        }
    }

    /// <summary>
    /// Callback when hit the Back&Forth button
    /// </summary>
    /// <param name="newS">New value</param>
    private void switchBackForth(bool newS) {
        forwardAndBack = newS;
        foreach (Toggle l in forwardAndBackToggles) {
            l.SetValue(newS);
        }
    }

    /// <summary>
    /// Callback when hit the Smooth button
    /// </summary>
    /// <param name="newSmooth">New value</param>
    private void switchSmooth(bool newSmooth) {
        if (newSmooth) {
            switchAverage(false);
        }

        smoothing = newSmooth;
        foreach (Toggle l in smoothToggles) {
            l.SetValue(newSmooth);
        }
    }

    /// <summary>
    /// Callback when hit the Average window button
    /// </summary>
    /// <param name="newAv">New value</param>
    private void switchAverage(bool newAv) {
        if (newAv) {
            switchSmooth(false);
        }

        average = newAv;
        foreach (Toggle l in averageToggles) {
            l.SetValue(newAv);
        }
    }

    /// <summary>
    /// Callback when hit the 'NextFrame' button
    /// </summary>
    private void forceNextFrame() {
        s.trajNext(true, looping);
        play = false;
        updateFrameNumber();
    }

    /// <summary>
    /// Callback when hit the 'PreviousFrame' button
    /// </summary>
    private void forcePrevFrame() {
        s.trajNext(false, looping);
        play = false;
        updateFrameNumber();
    }

    /// <summary>
    /// Callback when the value of the UI Framerate is changed
    /// </summary>
    private void changeFrameRate(float newF) {
        trajFramerate = newF;
    }

    /// <summary>
    /// Callback when the value of the Windows Size is changed
    /// </summary>
    private void changeWindowSize(int newS) {
        newS = Mathf.Max(0, newS);
        windowSize = newS;
    }

    /// <summary>
    /// Callback when the slider of the frames is changed
    /// </summary>
    private void setFrame(float val) {
        if (sliderChanged) {
            int idF = (int) val;
            play = false;
            s.trajSetFrame(idF);
            updateFrameNumber();
        }
    }

    /// <summary>
    /// Callback when hit the 'Unload' UI button
    /// </summary>
    private void unloadTrajectory() {
        API.APIPython.unloadTraj(s.name);
        Destroy(this);
    }

    /// <summary>
    /// Main loop
    /// </summary>
    private void Update() {

        if (s.xdr != null && play) {
            sliderChanged = false;
            float invFramerate = 1.0f / trajFramerate;

            if (!forwardAndBack && !looping) {
                if ((forward && s.xdr.CurrentFrame + 1 >= s.xdr.NumberFrames) || (!forward && s.xdr.CurrentFrame - 1 < 0)) {
                    play = false;
                }
            }

            if (!smoothing) {
                if (timeperiod > invFramerate) {
                    timeperiod = 0.0f;

                    if (forwardAndBack) {
                        if ((forward && s.xdr.CurrentFrame + 1 >= s.xdr.NumberFrames) || (!forward && s.xdr.CurrentFrame - 1 < 0)) {
                            forward = !forward;
                        }
                    }

                    s.trajNext(forward, looping, average, windowSize);
                }
            }
            else {
                bool newFrame = false;
                if (timeperiod > invFramerate) {
                    timeperiod = 0.0f;
                    newFrame = true;
                }

                if (newFrame && forwardAndBack) {
                    if ((forward && s.xdr.CurrentFrame + 2 >= s.xdr.NumberFrames) || (!forward && s.xdr.CurrentFrame - 2 < 0)) {
                        forward = !forward;
                    }
                }
                float t = trajFramerate * timeperiod;
                s.trajNextSmooth(t, forward, looping, newFrame);
            }

            //Update UI Part
            if (trajUIs != null && trajUIs.Count > 0) {
                updateFrameNumber();

                if (prevLoop != looping) {
                    updateLoopToggle();
                }
                if (!Mathf.Approximately(prevFrameRate, trajFramerate)) {
                    updateFramerateValue();
                }
                if (prevForward != forward) {
                    updateForwardToggle();
                }
                if (prevWindowSize != windowSize) {
                    updateWindowSizeValue();
                }

                sliderChanged = true;
            }
            timeperiod += Time.deltaTime;

        }
    }

    /// <summary>
    /// Update the UI element FrameCount based on the trajectory number of frames.
    /// </summary>
    private void updateFrameCount() {
        if (frameCountTexts == null || frameSliders == null) {
            return;
        }
        foreach (var fct in frameCountTexts) {
            fct.text = s.xdr.NumberFrames + " frames";
        }
        foreach (var fs in frameSliders) {
            fs.maxValue = s.xdr.NumberFrames - 1 ; //Frames start at 0
        }
    }

    /// <summary>
    /// Update the UI element FrameNumber based on the trajectory current frame.
    /// </summary>
    private void updateFrameNumber() {
        if (frameTexts == null || frameSliders == null) {
            return;
        }
        foreach (var ft in frameTexts) {
            ft.text = String.Format("Frame {0}", s.xdr.CurrentFrame);
        }
        foreach (var fs in frameSliders) {
            fs.value = s.xdr.CurrentFrame;
        }
    }

    /// <summary>
    /// Update all UI elements Loop based on its value.
    /// </summary>
    private void updateLoopToggle() {
        if (loopToggles == null) {
            return;
        }
        foreach (var lt in loopToggles) {
            lt.isOn = looping;
        }
        prevLoop = looping;
    }

    /// <summary>
    /// Update the UI element Forward based on its value.
    /// </summary>
    private void updateForwardToggle() {
        if (forwardToggles == null) {
            return;
        }
        foreach (var ft in forwardToggles) {
            ft.isOn = forward;
        }
        prevForward = forward;
    }

    /// <summary>
    /// Update the UI element Framerate based on its value.
    /// </summary>
    private void updateFramerateValue() {
        if (frameRateSliders == null) {
            return;
        }
        foreach (Slider frs in frameRateSliders) {
            frs.value = trajFramerate;
        }

        foreach (Text ft in framerateTexts) {
            ft.text = "Speed : " + trajFramerate.ToString("F1");
        }

        prevFrameRate = trajFramerate;
    }

    /// <summary>
    /// Update the UI element Window Size based on its value.
    /// </summary>
    private void updateWindowSizeValue() {
        foreach (Text wst in windowSizeTexts) {
            wst.text = windowSize.ToString();
        }
        prevWindowSize = windowSize;
    }

    private void OnDestroy() {
        foreach (GameObject trajui in trajUIs) {
            trajui.SetActive(false);
            Canvas.ForceUpdateCanvases();
            //Force update Canvas of LoadedMoleculesUI parent
            Transform t = trajui.transform.parent.parent.parent;
            if (t != null && t.GetComponent<RectTransform>() != null) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(t.GetComponent<RectTransform>());
            }
        }
        trajUIs.Clear();
        frameTexts.Clear();
        frameSliders.Clear();
        frameCountTexts.Clear();
        loopToggles.Clear();
        forwardToggles.Clear();
        forwardAndBackToggles.Clear();
        frameRateSliders.Clear();
        framerateTexts.Clear();
        smoothToggles.Clear();
        averageToggles.Clear();
        windowSizeTexts.Clear();
    }
}
}
