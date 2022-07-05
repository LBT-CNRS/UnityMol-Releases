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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Linq;

namespace UMol {
public class SequenceViewerUI : MonoBehaviour {


	public static string[] allColors = new string[] {"navy", "lime", "blue", "magenta", "brown", "cyan", "green",
	        "lightblue", "black", "maroon", "grey", "darkblue", "olive",
	        "orange", "purple", "red", "silver", "teal", "yellow"};

	public Dictionary<string, Dictionary<UnityMolResidue, GameObject>> residueToUIGo = new Dictionary<string, Dictionary<UnityMolResidue, GameObject>>();
	public List<Toggle> allUIToggle = new List<Toggle>();

	public GameObject CreateSequenceViewer(UnityMolStructure newS) {

		Dictionary<UnityMolResidue, GameObject> resToGo = new Dictionary<UnityMolResidue, GameObject>();

		string seqVName = "SequenceViewer";
		if (XRSettings.enabled) {
			seqVName += "VR";
		}

		
		Transform seqViewerT = transform.Find(seqVName);
		GameObject seqViewer = null;

		if (seqViewerT == null) {
			if (UnityMolMain.inVR()) {
				seqViewer = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/CanvasSequenceViewerVR"));
			}
			else{
				seqViewer = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/CanvasSequenceViewer"));
			}
			seqViewer.name = seqVName;
			seqViewer.transform.SetParent(transform);
		}
		else{
			seqViewer = seqViewerT.gameObject;
		}
		Transform seqList = seqViewer.transform.Find("Scroll View/Content");


		GameObject newSeqView = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/MoleculeSeqView"));
		newSeqView.transform.SetParent(seqList, false);


		// if (XRSettings.enabled) {

		// 	newSeqView.transform.localPosition = Vector3.zero;
		// 	newSeqView.transform.localScale = Vector3.one;
		// 	newSeqView.transform.localRotation = Quaternion.identity;
		// }


		newSeqView.transform.Find("MoleculeLabel/Text").GetComponent<Text>().text = newS.uniqueName;
		Transform contentParent = newSeqView.transform.Find("Scroll View/Content");

		int idColorChain = 0;

		foreach (UnityMolChain c in newS.currentModel.chains.Values) {
			string nameChain = "<color=" + allColors[idColorChain] + "><b><size=8>" + c.name + "</size></b>";
			foreach (UnityMolResidue r in c.residues.Values) {
				int id = r.id;
				string name = r.name;
				GameObject newResUI = GameObject.Instantiate((GameObject) Resources.Load("Prefabs/ResidueToggle"));
				newResUI.transform.SetParent(contentParent, false);

				string resLabel = nameChain + "\n" + name + "\n" +id.ToString() + "</color>";
				newResUI.transform.Find("Label").GetComponent<Text>().text = resLabel;

				Toggle resToggle = newResUI.GetComponent<Toggle>();
				resToggle.onValueChanged.AddListener((value) => { ResidueButtonSelectionSwitch(r, value); });

				resToGo[r] = newResUI;
				allUIToggle.Add(resToggle);

			}
			idColorChain++;
			if (idColorChain == allColors.Length) {
				idColorChain = 0;
			}
		}

		residueToUIGo[newS.uniqueName] = resToGo;


		return newSeqView;
	}

	public void ResidueButtonSelectionSwitch(UnityMolResidue r, bool val) {
		UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();

		UnityMolSelection sel = r.ToSelection();
		UnityMolSelection curSel = selM.getCurrentSelection();

		if (val) {
			API.APIPython.addToSelection(sel.MDASelString, curSel.name);
		}
		else {
			API.APIPython.removeFromSelection(sel.MDASelString, curSel.name);
		}
	}


	public void DeleteStructure(string s){

		if(residueToUIGo.ContainsKey(s)){
			GameObject toRM = null;
			bool first = true;
			foreach(GameObject go in residueToUIGo[s].Values) {
				if(first){
					toRM = go.transform.parent.parent.parent.gameObject;
				}
				first = false;
				allUIToggle.Remove(go.GetComponent<Toggle>());
			}
			residueToUIGo.Remove(s);
			if(toRM != null){
				GameObject.Destroy(toRM);
			}
		}
		
	}
}
}