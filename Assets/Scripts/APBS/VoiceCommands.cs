/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Joseph Laurenti, 2019-2020
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


using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UMol.API;

namespace UMol
{
    public class VoiceCommands : MonoBehaviour
    {
        // Voice command vars
        private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
        private KeywordRecognizer recognizer;

        private UnityMolSelectionManager selM;

        public GameObject CanvasMainUI_VR;
        UIManager vrUIManager;


        // Start is called before the first frame update
        void Start()
        {
            selM = UnityMolMain.getSelectionManager();
            keyActs.Add("load new", loadNewFile);

            keyActs.Add("toggle side chains", showSC);
            keyActs.Add("toggle hydrogens", showHydrogens);
            keyActs.Add("toggle backbone", showBackbone);

            keyActs.Add("center selection", centerSelection);
            keyActs.Add("center structure", centerStructure);

            keyActs.Add("show cartoon", showCartoon);
            keyActs.Add("hide cartoon", hideCartoon);

            keyActs.Add("show hyperball", showHyperball);
            keyActs.Add("hide hyperball", hideHyperball);

            keyActs.Add("show surface", showSurface);
            keyActs.Add("hide surface", hideSurface);

            keyActs.Add("rotate x", switchRotateX);
            keyActs.Add("rotate y", switchRotateY);
            keyActs.Add("rotate z", switchRotateZ);

            keyActs.Add("switch room", switchRoom);
            keyActs.Add("switch mode", switchToDesktop);
            keyActs.Add("take screenshot", takeScreenShot);

            recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
            recognizer.OnPhraseRecognized += OnKeywordsRecognized;
            recognizer.Start();

            if(CanvasMainUI_VR != null){
                vrUIManager = CanvasMainUI_VR.GetComponent<UIManager>();
            }
        }


        void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
        {
            Debug.Log("Command: " + args.text);
            keyActs[args.text].Invoke();
        }

        void loadNewFile()
        {
            vrUIManager.loadBrowser();
        }

        void showSC()
        {
            vrUIManager.showHideSC();
        }
        void showHydrogens()
        {
            vrUIManager.showHideH();
        }
        void showBackbone()
        {
            vrUIManager.showHideBB();
        }

        void centerSelection()
        {
            if (selM == null) {
                selM = UnityMolMain.getSelectionManager();
            }

            UnityMolSelection sel = selM.currentSelection;
            if (sel == null)
            {
                Debug.LogWarning("Using the last loaded molecule as input");
                sel = APIPython.last().ToSelection();
            }
            string CurSel = selM.currentSelection.name;

            APIPython.centerOnSelection(CurSel, lerp: true);
        }
        void centerStructure()
        {
            if (selM == null) {
                selM = UnityMolMain.getSelectionManager();
            }

            UnityMolSelection sel = selM.currentSelection;
            if (sel == null)
            {
                Debug.LogWarning("Using the last loaded molecule as input");
                sel = APIPython.last().ToSelection();
            }
            string CurSel = selM.currentSelection.structures[0].uniqueName;

            APIPython.centerOnStructure(CurSel, lerp: true);
        }

        void showCartoon()
        {
            if (selM == null) {
                selM = UnityMolMain.getSelectionManager();
            }

            UnityMolSelection sel = selM.currentSelection;
            if (sel == null)
            {
                Debug.LogWarning("Using the last loaded molecule as input");
                sel = APIPython.last().ToSelection();
            }
            string CurSel = selM.currentSelection.name;

            APIPython.showSelection(CurSel, "c");
        }
        void hideCartoon()
        {
            if (selM == null) {
                selM = UnityMolMain.getSelectionManager();
            }

            UnityMolSelection sel = selM.currentSelection;
            if (sel == null)
            {
                Debug.LogWarning("Using the last loaded molecule as input");
                sel = APIPython.last().ToSelection();
            }
            string CurSel = selM.currentSelection.name;

            APIPython.hideSelection(CurSel, "c");
        }

        void showHyperball()
        {
            if (selM == null) {
                selM = UnityMolMain.getSelectionManager();
            }

            UnityMolSelection sel = selM.currentSelection;
            if (sel == null)
            {
                Debug.LogWarning("Using the last loaded molecule as input");
                sel = APIPython.last().ToSelection();
            }
            string CurSel = selM.currentSelection.name;

            APIPython.showSelection(CurSel, "hb");
        }
        void hideHyperball()
        {
            if (selM == null) {
                selM = UnityMolMain.getSelectionManager();
            }

            UnityMolSelection sel = selM.currentSelection;
            if (sel == null)
            {
                Debug.LogWarning("Using the last loaded molecule as input");
                sel = APIPython.last().ToSelection();
            }
            string CurSel = selM.currentSelection.name;

            APIPython.hideSelection(CurSel, "hb");
        }


        void showSurface()
        {
            if (selM == null) {
                selM = UnityMolMain.getSelectionManager();
            }

            UnityMolSelection sel = selM.currentSelection;
            if (sel == null)
            {
                Debug.LogWarning("Using the last loaded molecule as input");
                sel = APIPython.last().ToSelection();
            }
            string CurSel = selM.currentSelection.name;

            APIPython.showSelection(CurSel, "s");
        }
        void hideSurface()
        {
            if (selM == null) {
                selM = UnityMolMain.getSelectionManager();
            }

            UnityMolSelection sel = selM.currentSelection;
            if (sel == null)
            {
                Debug.LogWarning("Using the last loaded molecule as input");
                sel = APIPython.last().ToSelection();
            }
            string CurSel = selM.currentSelection.name;

            APIPython.hideSelection(CurSel, "s");
        }

        void switchRoom()
        {
            vrUIManager.switchRoomMode();
        }

        void switchToDesktop()
        {
            Transform HMD = VRTK.VRTK_DeviceFinder.HeadsetCamera();
            HMD.gameObject.GetComponent<SwitchVROnOff>().switchVR();
        }

        void takeScreenShot()
        {
            vrUIManager.saveScreenshot();
        }

        void switchRotateX()
        {
            APIPython.switchRotateAxisX();
        }
        void switchRotateY()
        {
            APIPython.switchRotateAxisY();
        }
        void switchRotateZ()
        {
            APIPython.switchRotateAxisZ();
        }
    }
}
