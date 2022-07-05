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



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMol;
using UMol.API;

namespace UMol
{
    public class TourManager : MonoBehaviour
    {
        private UnityMolSelectionManager selM;
        public static UnityMolAtom AtomFromSel;
        public int CountTourAtoms = 0;
        public static Dictionary<int, string> TourList = new Dictionary<int, string>();
        public float realTourTime = 5.0f;
        int tourStepsCheck = 1;

        void OnEnable(){
            if (selM == null) {
                selM = UnityMolMain.getSelectionManager();
            }
            selM.onNewClickSelection += newClickSelection;
        }
        void OnDisable(){
            if (selM == null) {
                selM = UnityMolMain.getSelectionManager();
            }
            selM.onNewClickSelection -= newClickSelection;
        }

        void newClickSelection(NewSelEventArgs selEv){
            if(selEv.sel != null && selEv.sel.Count >= 1){
                AtomFromSel = selEv.sel.atoms[0];
            }
        }

        public void addAtomToTour()
        {
            if (selM == null) {
                selM = UnityMolMain.getSelectionManager();
            }
            if(AtomFromSel == null){
                return;
            }

            UnityMolSelection hoveredSelection = null;
            hoveredSelection = AtomFromSel.ToSelection();

            string MDA_Atom = hoveredSelection.MDASelString;

            string tourSelName = "TourPos_" + CountTourAtoms;
            Debug.Log("Current tour atom place is: " + CountTourAtoms);

            TourList.Add(CountTourAtoms, tourSelName); //add position and selection name

            APIPython.select(MDA_Atom, tourSelName);

            CountTourAtoms++; //increase place in dictionary
            APIPython.deleteSelection(selM.clickSelectionName);
            APIPython.clearSelections();
        }


        public void startNewTour()
        {
            if(TourList == null || TourList.Count < 2){
                return;
            }
            string tourTime = GameObject.Find("TourPauseTimeText").GetComponent<Text>().text;
            realTourTime = float.Parse(tourTime);

            string selName = TourList[0];

            APIPython.setCurrentSelection(selName);
            APIPython.centerOnSelection(selName, lerp: true);

            StartCoroutine(MoveTourPos(realTourTime));
        }

        public IEnumerator MoveTourPos(float waitTime)
        {
            for (int i = 1; i < TourList.Count; i++)
            {
                string selName = TourList[i];
                yield return new WaitForSeconds(waitTime);
                setViewTour(selName);
            }
        }

        public void setViewTour(string selName)
        {
            APIPython.setCurrentSelection(selName);
            APIPython.centerOnSelection(selName, lerp: true);
            Debug.Log("Current selection: " + selName);
        }

        public void resetTour()
        {
            CountTourAtoms = 0;
            TourList.Clear();
        }
    }
}
