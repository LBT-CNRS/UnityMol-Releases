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


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using System.Reflection;

using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;
using UnityEngine.EventSystems;

using UMol.API;


namespace UMol {
public class UIManager : MonoBehaviour {

	public string mainUIName = "CanvasMainUI";
	public bool seqViewer = false;

	public GameObject room;
	public GameObject floor;

	public InputField inFPDB;

	public InputField inFIMDIP;
	public InputField inFIMDPort;


	public Dictionary<string, GameObject> structureNameToUIObject = new Dictionary<string, GameObject>();
	public Dictionary<string, GameObject> selectionToUIObject = new Dictionary<string, GameObject>();

	private UnityMolStructureManager sm;
	private UnityMolSelectionManager selM;
	private UnityMolRepresentationManager repM;

	public Transform loadedMolUITransform;
	public Transform globalSelectionsUITransform;

	public Text loadedMolLabel;

	public Transform selectionsUITransform;

	SequenceViewerUI seqUI;
	public GameObject seqUIGo;

	//Number of selected atoms (used to know when to update UI elements)
	// private int UIcurSelectedAtoms = 0;
	//Number of selections in currentSelections (used to know when to update UI elements)
	private int UIcurSelections = 0;
	//Number of active representations (used to know when to update UI elements)
	private int UIcurActiveRep = 0;
	//Number of total selected atoms (used to know when to update UI elements)
	private int UItotalAtomSelected = 0;

	bool showUpdateRepVisiUI = false;


	public float IPD = 0.063f;//IPD in mm
	public float offsetIPD = 0.024f;

	GameObject rightCam;

	void Awake() {
		XRSettings.eyeTextureResolutionScale = 2.0f;

#if !DISABLE_MAINUI
		sm = UnityMolMain.getStructureManager();
		selM = UnityMolMain.getSelectionManager();
		repM = UnityMolMain.getRepresentationManager();

		if (seqViewer) {
			if (seqUI == null) {
				if (seqUIGo == null) {
					seqUIGo = new GameObject("SequenceUIGO");
				}
				seqUI = seqUIGo.GetComponent<SequenceViewerUI>();
				if (seqUI == null) {
					seqUI = seqUIGo.AddComponent<SequenceViewerUI>();
				}
				seqUIGo.SetActive(false);//Hide it
			}
		}

		rightCam = GameObject.Find("Cyclop/CameraUMolXR");

		if (GameObject.Find(mainUIName) == null) {
			GameObject newPanel = Instantiate((GameObject) Resources.Load("Prefabs/CanvasMainUI"));
			newPanel.transform.name = mainUIName;
		}

		if (loadedMolUITransform == null) {
			loadedMolUITransform = GameObject.Find(mainUIName + "/Selection Scroll View/Viewport/Content/LoadedMoleculesUI").transform;
		}
		if (loadedMolLabel == null) {
			loadedMolLabel = GameObject.Find(mainUIName + "/Selection Scroll View/Viewport/Content/LoadedMoleculesLabel/PanelLayer/Title").GetComponent<Text>();
		}
		if (selectionsUITransform == null) {
			selectionsUITransform = GameObject.Find(mainUIName + "/Selection Scroll View/Viewport/Content").transform;
		}
		if (globalSelectionsUITransform == null) {
			globalSelectionsUITransform = GameObject.Find(mainUIName + "/Selection Scroll View/Viewport/Content/GlobalSelections").transform;
		}

		UnityMolStructureManager.OnMoleculeLoaded += updateLoadedMoleculesUI;
		UnityMolStructureManager.OnMoleculeDeleted += updateLoadedMoleculesUI;
		UnityMolRepresentationManager.OnRepresentationVisibility += planUpdateRepVisiUI;
#else
		gameObject.SetActive(false);
#endif
	}


	public void undo() {
		APIPython.undo();
	}

	public void saveStateAsPythonScript() {
		// string path = Path.Combine(Application.streamingAssetsPath , "UmolState.py");
		// APIPython.saveHistoryScript(path);

		ReadSaveFilesWithBrowser rsfwb = GetComponent<ReadSaveFilesWithBrowser>();
		if (rsfwb == null) {
			rsfwb = gameObject.AddComponent<ReadSaveFilesWithBrowser>();
		}
		rsfwb.saveState();

		// APIPython.saveHistoryScript(path);
	}
	public void loadStateFromPythonScript() {
		// string path = Path.Combine(Application.streamingAssetsPath , "UmolState.py");
		// APIPython.loadHistoryScript(path);
		ReadSaveFilesWithBrowser rsfwb = GetComponent<ReadSaveFilesWithBrowser>();
		if (rsfwb == null) {
			rsfwb = gameObject.AddComponent<ReadSaveFilesWithBrowser>();
		}
		rsfwb.readState();
	}

	public void createSelection() {
		int i = 0;
		string selName = "newSelection";
		while (selM.selections.ContainsKey(selName)) {
			i++;
			selName = "newSelection_" + i;
		}
		APIPython.select("nothing", selName, forceCreate: true);
	}
	public void createGlobalSelection() {
		int i = 0;
		string selName = "newGlobalSelection";
		while (selM.selections.ContainsKey(selName)) {
			i++;
			selName = "newGlobalSelection_" + i;
		}
		UnityMolSelection sel = APIPython.select("nothing", selName, forceCreate: true);
		if (sel != null) {
			sel.forceGlobalSelection = true;
		}
	}


	public void loadBrowser(bool forceDesktop = false) {

		bool readHTM = true;
		try {
			readHTM = GameObject.Find("Toggle heteroAtm").GetComponent<Toggle>().isOn;
		}
		catch {
		}

		ReadSaveFilesWithBrowser rsfwb = GetComponent<ReadSaveFilesWithBrowser>();
		if (rsfwb == null) {
			rsfwb = gameObject.AddComponent<ReadSaveFilesWithBrowser>();
		}
		rsfwb.readFiles(readHTM, forceDesktop);
	}


	public void wrapperFetch(Text t) {
		StartCoroutine(fetch(t.text));
	}

	public IEnumerator fetch(string t) {
		if (string.IsNullOrEmpty(t)) {
			if (inFPDB == null) {
				inFPDB = GameObject.Find("InputFieldPDB").GetComponent<InputField>();
				if (inFPDB != null) {
					t = inFPDB.text;
				}
			}
		}
		bool mmCIF = true;
		try {
			mmCIF = GameObject.Find("Toggle mmCIF/Toggle").GetComponent<Toggle>().isOn;
		}
		catch {
		}

		// APIPython.clear();
		yield return 0;

		bool readHTM = true;
		try {
			readHTM = GameObject.Find("Toggle heteroAtm/Toggle").GetComponent<Toggle>().isOn;
		}
		catch {
		}

		try {
			APIPython.fetch(t, mmCIF, readHTM);
			// APIPython.showDefault(APIPython.last().ToSelection().name);
		}


		catch (System.Exception e) {
			string errM = e.ToString();
			if (errM.Contains("404")) {
				Debug.LogError("Wrong PDB Id");
			}
			else if (errM.Contains("ConnectFailure")) {
				Debug.LogError("No internet connection or blocked access to the PDB");
			}
			else {
				Debug.LogError("Could not fetch PDB file");
			}

			// if(internetCanvas != null){
			//     internetCanvas.gameObject.SetActive(true);
			//     Invoke("hideInternetWarning", 3.0f);
			// }
		}

		// selM.ClearCurrentSelections();
		// selM.AddToCurrentSelections(APIPython.last().ToSelection());
	}

	public void setIPD(float v) {
		GameObject cyclop = GameObject.Find("Cyclop");

		if (cyclop != null) {
			GameObject camL = GameObject.Find("CameraUMolXL");

			float newVal = v / 1000.0f;

			newVal += offsetIPD;

			camL.transform.localPosition = new Vector3( -newVal / 2, camL.transform.localPosition.y, camL.transform.localPosition.z);
			if (rightCam != null) {
				rightCam.transform.localPosition = new Vector3( newVal / 2, rightCam.transform.localPosition.y, rightCam.transform.localPosition.z);
			}
			IPD = newVal;
		}
	}

	public void changeIPD(Slider s) {
		setIPD(s.value);
	}

	public void setIPDZero() {
		setIPD(0.0f);
	}

	public void switch3DStereo(Toggle t) {
		// XRSettings.enabled = t.isOn;
	}


	public void updateTextIPD(Text t) {
		t.text = "IPD: " + Mathf.Round(IPD * 1000.0f).ToString("F0") + " mm";
	}


	private bool isLoadedStructureUIUpdated() {
		foreach (UnityMolStructure s in sm.loadedStructures) {
			if (!structureNameToUIObject.ContainsKey(s.uniqueName)) {
				return false;
			}
		}
		foreach (string s in structureNameToUIObject.Keys) {
			if (!sm.nameToStructure.ContainsKey(s)) {
				return false;
			}
		}

		return true;
	}

	private void updateLoadedMoleculesUI() {
		foreach (UnityMolStructure s in sm.loadedStructures) {
			if (!structureNameToUIObject.ContainsKey(s.uniqueName)) {
				if (seqViewer) {
					seqUI.CreateSequenceViewer(s);
				}
				GameObject newB = createNewMolButton(s);
				structureNameToUIObject[s.uniqueName] = newB;
			}
		}
		List<string> names = structureNameToUIObject.Keys.ToList();
		foreach (string s in names) {
			//Old structure to remove
			if (!sm.nameToStructure.ContainsKey(s)) {
				//Remove loaded molecules UI buttons
				GameObject.DestroyImmediate(structureNameToUIObject[s]);
				structureNameToUIObject.Remove(s);
				//Remove sequence UI buttons
				if (seqViewer) {
					if (seqUI != null) {
						seqUI.DeleteStructure(s);
					}
				}
			}
		}
		if (loadedMolLabel != null) {
			loadedMolLabel.text = "Loaded Molecules (" + sm.loadedStructures.Count + ")";
		}

		try{
			LayoutRebuilder.ForceRebuildLayoutImmediate(loadedMolUITransform.GetComponent<RectTransform>());
			LayoutRebuilder.ForceRebuildLayoutImmediate(globalSelectionsUITransform.GetComponent<RectTransform>());
		}
		catch {

		}
	}


	private void planUpdateRepVisiUI() {
		showUpdateRepVisiUI = true;
	}

	private void updateRepVisiUI() {
		showUpdateRepVisiUI = false;
		foreach (string selS in selectionToUIObject.Keys) {
			GameObject selUIgo = selectionToUIObject[selS];
			if (selM.selections.ContainsKey(selS)) {
				foreach (RepType rt in selM.selections[selS].representations.Keys) {
					GameObject toggleRepType = repTypeToGo(selUIgo, rt);
					if (toggleRepType == null) {
						continue;
					}
					bool active = selM.selections[selS].representations[rt][0].isEnabled;
					toggleRepType.transform.Find("OptionsLabel/ShowHideButton/Button Layer/Button Icon").GetComponent<Image>().enabled = active ;
					toggleRepType.transform.Find("OptionsLabel/ShowHideButton/Button Layer/Button Icon Hidden").GetComponent<Image>().enabled = !active;
				}
			}
		}
	}

	private GameObject createNewMolButton(UnityMolStructure s) {


		GameObject newButton = Instantiate((GameObject) Resources.Load("Prefabs/ButtonLoadedMol"));
		newButton.transform.SetParent(loadedMolUITransform, false);
		newButton.transform.Find("Text").GetComponent<Text>().text = s.formatName(20);
		newButton.transform.name = "Button_" + s.uniqueName;
		newButton.transform.localScale = Vector3.one;
		// LoadedMolButtonFunctions butFunc = newButton.GetComponent<LoadedMolButtonFunctions>();
		// butFunc.structure = s;
		// butFunc.switchSelectedStructure();
		// newButton.GetComponent<Button>().onClick.AddListener(butFunc.switchSelectedStructure);
		newButton.transform.Find("Text/ActionButtons/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(s.uniqueName)) {
				APIPython.hideStructureAllRepresentations(s.uniqueName);
			}
			else {
				APIPython.showStructureAllRepresentations(s.uniqueName);
			}
		});

		newButton.transform.Find("Text/ActionButtons/DeleteButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.delete(s.uniqueName);
		});

		newButton.transform.Find("Text/ActionButtons/ShowHideHydrogen/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.addHydrogensReduce(s.uniqueName);
			// APIPython.addHydrogensHaad(s.uniqueName);
		});

		Text curGroupText = newButton.transform.Find("Group/Text").gameObject.GetComponent<Text>();

		newButton.transform.Find("Group/GroupMinus").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			int curGroup = APIPython.getStructureGroup(s.uniqueName);
			curGroup = Mathf.Max(0, curGroup - 1);
			APIPython.setStructureGroup(s.uniqueName, curGroup);
			curGroupText.text = "Group " + curGroup;
		});

		newButton.transform.Find("Group/GroupPlus").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			int curGroup = APIPython.getStructureGroup(s.uniqueName);
			curGroup = curGroup + 1;
			APIPython.setStructureGroup(s.uniqueName, curGroup);
			curGroupText.text = "Group " + curGroup;
		});

		if (s.models.Count > 1) {
			newButton.transform.Find("Model Menu").gameObject.SetActive(true);
		}


		return newButton;
	}

	public void updateSelectionUI() {
		try {
			foreach (UnityMolSelection s in selM.selections.Values) {
				if (!selectionToUIObject.ContainsKey(s.name)) {
					createSelectionToggle(s);
				}
			}
		}
		catch (System.Exception e) {
			Debug.LogError("Failed to create selection toggle : " + e);
		}
		// foreach (string ss in selectionToUIObject.Keys) {
		// 	try {
		// 		UnityMolSelection s = selM.selections[ss];
		// 		if (!selM.currentSelections.Contains(s)) {
		// 			// if (selectionToUIObject[ss].GetComponent<SelectionButtonFunctions>().isSelected) {
		// 			// selectionToUIObject[ss].GetComponent<SelectionButtonFunctions>().switchState();
		// 			// }
		// 		}
		// 		else {
		// 			// if (!selectionToUIObject[ss].GetComponent<SelectionButtonFunctions>().isSelected) {
		// 			// selectionToUIObject[ss].GetComponent<SelectionButtonFunctions>().switchState();
		// 			// }
		// 		}
		// 	}
		// 	catch {
		// 	}
		// }

		List<string> selNames = selectionToUIObject.Keys.ToList();
		foreach (string selS in selNames) {
			if (!selM.selections.ContainsKey(selS)) {
				GameObject.DestroyImmediate(selectionToUIObject[selS]);
				selectionToUIObject.Remove(selS);
			}
		}

		foreach (string selS in selectionToUIObject.Keys) {
			int cpt = selM.selections[selS].Count;
			Text labelCountText = selectionToUIObject[selS].transform.Find("Main/LabelCount").GetComponent<Text>();

			Toggle updateToggle = selectionToUIObject[selS].transform.Find("EditSelection/ToggleUpdateWithTraj/Toggle").gameObject.GetComponent<Toggle>();
			updateToggle.SetValue(selM.selections[selS].updateWithTraj);

			int curLabelCount = int.Parse(labelCountText.text.Split(new char[] {'(', ')'}, StringSplitOptions.RemoveEmptyEntries)[0]);
			if (cpt != curLabelCount) {
				selectionToUIObject[selS].transform.Find("EditSelection/InputFieldSelectionString").GetComponent<InputField>().text = selM.selections[selS].ToSelectionCommand();
				labelCountText.text = "(" + cpt + ")";
			}
			selectionToUIObject[selS].transform.Find("Main/Toggle").GetComponent<Toggle>().SetValue(false);
			if (selM.currentSelection != null && selM.currentSelection.name == selS) {
				selectionToUIObject[selS].transform.Find("Main/Toggle").GetComponent<Toggle>().SetValue(true);
			}
		}
		if (selM.currentSelection == null) {
			UIcurSelections = 0;
		}
		else {
			UIcurSelections = selM.currentSelection.Count;
		}

		//Needed to update the UI layout
		// loadedMolUITransform.GetComponent<RectTransform>().ForceUpdateRectTransforms();
		// LayoutRebuilder.MarkLayoutForRebuild(loadedMolUITransform.GetComponent<RectTransform>());
		// Canvas.ForceUpdateCanvases();
		// loadedMolUITransform.GetComponent<VerticalLayoutGroup>().enabled = false; loadedMolUITransform.GetComponent<VerticalLayoutGroup>().enabled = true;
		LayoutRebuilder.ForceRebuildLayoutImmediate(loadedMolUITransform.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(globalSelectionsUITransform.GetComponent<RectTransform>());


	}


	private void createSelectionToggle(UnityMolSelection sel) {

		bool isGlobalSel = (sel.structures.Count > 1);

		GameObject newButtonGo = Instantiate((GameObject) Resources.Load("Prefabs/SelectionButtonDropdown"));
		if (isGlobalSel || sel.forceGlobalSelection) {
			newButtonGo.transform.SetParent(globalSelectionsUITransform);
		}
		else if (sel.structures.Count == 0) {
			string nameStruct = APIPython.last().uniqueName;
			Transform molT = loadedMolUITransform.Find("Button_" + nameStruct);
			Transform molPar = molT.parent;
			newButtonGo.transform.SetParent(molPar);
			insertButtonInHierarchy(newButtonGo, nameStruct);
			sel.structures.Add(APIPython.last());
		}
		else {
			string nameStruct = sel.structures[0].uniqueName;
			Transform molT = loadedMolUITransform.Find("Button_" + nameStruct);
			Transform molPar = molT.parent;
			newButtonGo.transform.SetParent(molPar);
			insertButtonInHierarchy(newButtonGo, nameStruct);
		}
		newButtonGo.transform.Find("EditSelection/InputFieldName").GetComponent<InputField>().text = sel.name;
		newButtonGo.transform.Find("Main/Text").GetComponent<Text>().text = sel.name;
		newButtonGo.transform.Find("Main/LabelCount").GetComponent<Text>().text = "(" + sel.Count + ")";
		newButtonGo.transform.name = "Selection_" + sel.name;
		newButtonGo.transform.localScale = Vector3.one;
		newButtonGo.transform.localPosition = Vector3.zero;
		newButtonGo.transform.localRotation = Quaternion.identity;

		// saveRowButtonState(newButtonGo.transform.Find("RepOptions/RowC/Options").gameObject);

		selectionToUIObject[sel.name] = newButtonGo;

		setupRepresentationButtons(newButtonGo, sel);

		setupSelectionNamesButtons(newButtonGo, sel);


	}

	// private void saveRowButtonState(GameObject go){

	// 	var tmp = go.GetComponentsInChildren<Component>();
	// 	List<FieldInfo> infos = new List<FieldInfo>();

	// 	foreach(Component t in tmp){
	// 		if(t.GetType() == typeof(Slider) || t.GetType() == typeof(Toggle)){
	// 			foreach(var f in t.GetType().GetFields()){
	// 				if(f.IsStatic) continue;
	// 				infos.Add(f);
	// 				Debug.Log(f.Name);
	// 			}
	// 			var props = t.GetType().GetProperties();
	// 			foreach(var p in props){
	// 				Debug.Log(p);
	// 			}
	// 		}
	// 	}
	// }

	// T CopyComponent<T>(T original, GameObject destination) where T : Component
//     {
//         System.Type type = original.GetType();
//         var dst = destination.GetComponent(type) as T;
//         if (!dst) dst = destination.AddComponent(type) as T;
//         var fields = type.GetFields();
//         foreach (var field in fields)
//         {
//             if (field.IsStatic) continue;
//             field.SetValue(dst, field.GetValue(original));
//         }
//         var props = type.GetProperties();
//         foreach (var prop in props)
//         {
//             if (!prop.CanWrite || prop.Name == "name") continue;
//             prop.SetValue(dst, prop.GetValue(original, null), null);
//         }
//         return dst as T;
//     }

	private void insertButtonInHierarchy(GameObject go, string nameStruct) {
		//Changes the order so that the selection button is under the good structure label
		// int id = 0;
		bool start = false;
		int index = -1;
		foreach (Transform t in go.transform.parent) {
			if (t.name == "Button_" + nameStruct) {
				start = true;
				continue;
			}
			if (start && t.name.StartsWith("Button_")) { //New molecule
				index = t.GetSiblingIndex();
				break;
			}
		}

		//If order didn't change => button already at the right place
		if (index != -1) {
			go.transform.SetSiblingIndex(index);
		}
	}


	private void setupRepresentationButtons(GameObject newButtonGo, UnityMolSelection sel) {

		//Delete Button -------------------------------
		newButtonGo.transform.Find("Main/Delete/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.deleteSelection(sel.name);
		});

		newButtonGo.transform.Find("Main/Duplicate/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.duplicateSelection(sel.name);
		});

		newButtonGo.transform.Find("Main/Toggle").gameObject.GetComponent<Toggle>().onValueChanged.AddListener(
		delegate {
			UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
			if ( selM.currentSelection != null) {
				string curSelName = selM.currentSelection.name;
				if ( sel.name == curSelName) {
					APIPython.clearSelections();
					Debug.Log("Selection : " + sel.name + " is now inactive");
				}
				else {
					APIPython.setCurrentSelection(sel.name);
					Debug.Log("Selection : " + sel.name + " is now active");
				}
			}
			else {
				APIPython.setCurrentSelection(sel.name);
				Debug.Log("Selection : " + sel.name + " is now active");
			}


			updateSelectionUI();
		});




		string selName = sel.name;

		GameObject rowCartoon = newButtonGo.transform.Find("RepOptions/RowC").gameObject;
		GameObject rowHB = newButtonGo.transform.Find("RepOptions/RowHB").gameObject;
		GameObject rowBO = newButtonGo.transform.Find("RepOptions/RowBONDORDER").gameObject;
		GameObject rowLine = newButtonGo.transform.Find("RepOptions/RowL").gameObject;
		GameObject rowSurf = newButtonGo.transform.Find("RepOptions/RowS").gameObject;
		GameObject rowHbond = newButtonGo.transform.Find("RepOptions/RowHBOND").gameObject;
		GameObject rowHbondtube = newButtonGo.transform.Find("RepOptions/RowHBONDTUBE").gameObject;
		GameObject rowFL = newButtonGo.transform.Find("RepOptions/RowFL").gameObject;
		GameObject rowTrace = newButtonGo.transform.Find("RepOptions/RowTRACE").gameObject;
		GameObject rowSugar = newButtonGo.transform.Find("RepOptions/RowSUGARRIBBONS").gameObject;
		GameObject rowDX = newButtonGo.transform.Find("RepOptions/RowDXISO").gameObject;
		GameObject rowEllips = newButtonGo.transform.Find("RepOptions/RowELLIPSOID").gameObject;
		GameObject rowPoint = newButtonGo.transform.Find("RepOptions/RowPOINT").gameObject;

		Dropdown mainDropdownRep = newButtonGo.transform.Find("Main/AddRepDropdown").gameObject.GetComponent<Dropdown>();
		mainDropdownRep.onValueChanged.AddListener(
		delegate {

			GameObject toShow = null;
			try {
				switch (mainDropdownRep.options[mainDropdownRep.value].text) {
				case "Cartoon":
					toShow = rowCartoon;
					APIPython.showSelection(selName, "c");
					break;
				case "Hyperball":
					toShow = rowHB;
					APIPython.showSelection(selName, "hb");
					break;
				case "Line":
					toShow = rowLine;
					APIPython.showSelection(selName, "l");
					break;
				case "BondOrder":
					toShow = rowBO;
					APIPython.showSelection(selName, "bondorder");
					break;
				case "Surface":
					toShow = rowSurf;
					APIPython.showSelection(selName, "s");
					break;
				case "H-bond":
					toShow = rowHbond;
					APIPython.showSelection(selName, "hbond");
					break;
				case "H-bond tube":
					toShow = rowHbondtube;
					APIPython.showSelection(selName, "hbondtube");
					break;
				case "Fieldlines":
					toShow = rowFL;
					APIPython.showSelection(selName, "fl");
					break;
				case "Trace":
					toShow = rowTrace;
					APIPython.showSelection(selName, "trace");
					break;
				case "SugarRibbons":
					toShow = rowSugar;
					APIPython.showSelection(selName, "sugarribbons");
					break;
				case "Iso-surface":
					toShow = rowDX;
					UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
					UnityMolSelection cursel = selM.selections[selName];
					UnityMolStructure s = cursel.structures[0];
					APIPython.showSelection(selName, "dxiso", s.dxr, 0.0f);
					break;
				case "Ellipsoid":
					toShow = rowEllips;
					APIPython.showSelection(selName, "ellipsoid");
					break;
				case "Point":
					toShow = rowPoint;
					APIPython.showSelection(selName, "point");
					break;
				default:
					break;
				}
			}

			catch (System.Exception e) {
				mainDropdownRep.value = 0;
				Debug.LogError(e);
				return;
			}

			if (toShow != null) {
				toShow.SetActive(true);
			}
			mainDropdownRep.value = 0;
		});




		//Cartoon ------------------------------------
		newButtonGo.transform.Find("RepOptions/RowC/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {

			if (APIPython.areRepresentationsOn(selName, "c")) {
				APIPython.hideSelection(selName, "c");
			}
			else {
				APIPython.showSelection(selName, "c");
			}
		});

		newButtonGo.transform.Find("RepOptions/RowC/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			// APIPython.hideSelection(selName, "c");
			APIPython.deleteRepresentationInSelection(selName, "c");
			rowCartoon.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});


		newButtonGo.transform.Find("RepOptions/RowC/Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "c");
		});

		newButtonGo.transform.Find("RepOptions/RowC/Options/ColorButtons2/ShadowsOn/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setShadows(selName, "c", true);
		});

		newButtonGo.transform.Find("RepOptions/RowC/Options/ColorButtons2/ShadowsOff/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setShadows(selName, "c", false);
		});

		newButtonGo.transform.Find("RepOptions/RowC/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "c");
		});
		newButtonGo.transform.Find("RepOptions/RowC/Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByResidue(selName, "c");
		});

		ColorPickerControl cartoonColorPicker = newButtonGo.transform.Find("RepOptions/RowC/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		cartoonColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "c", cartoonColorPicker.CurrentColor);
		});

		Slider cartoonSliderSmooth = newButtonGo.transform.Find("RepOptions/RowC/Options/SliderSmooth/Slider").gameObject.GetComponent<Slider>();
		cartoonSliderSmooth.onValueChanged.AddListener(
		delegate {
			APIPython.setSmoothness(selName, "c", cartoonSliderSmooth.value);
		});

		Slider cartoonSliderMetal = newButtonGo.transform.Find("RepOptions/RowC/Options/SliderMetal/Slider").gameObject.GetComponent<Slider>();
		cartoonSliderMetal.onValueChanged.AddListener(
		delegate {
			APIPython.setMetal(selName, "c", cartoonSliderMetal.value);
		});


		ColorPickerControl cartoonHelixColorPicker = newButtonGo.transform.Find("RepOptions/RowC/Options/ColorPickerHelix").gameObject.GetComponent<ColorPickerControl>();

		cartoonHelixColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.setCartoonColorSS(selName, "helix" , cartoonHelixColorPicker.CurrentColor);
		});


		ColorPickerControl cartoonSheetColorPicker = newButtonGo.transform.Find("RepOptions/RowC/Options/ColorPickerSheet").gameObject.GetComponent<ColorPickerControl>();

		cartoonSheetColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.setCartoonColorSS(selName, "sheet" , cartoonSheetColorPicker.CurrentColor);
		});



		ColorPickerControl cartoonCoilColorPicker = newButtonGo.transform.Find("RepOptions/RowC/Options/ColorPickerCoil").gameObject.GetComponent<ColorPickerControl>();

		cartoonCoilColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.setCartoonColorSS(selName, "coil" , cartoonCoilColorPicker.CurrentColor);
		});





		//Hyperball ------------------------------------
		newButtonGo.transform.Find("RepOptions/RowHB/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "hb")) {
				APIPython.hideSelection(selName, "hb");
			}
			else {
				APIPython.showSelection(selName, "hb");
			}
		});

		newButtonGo.transform.Find("RepOptions/RowHB/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			// APIPython.hideSelection(selName, "hb");
			APIPython.deleteRepresentationInSelection(selName, "hb");
			rowHB.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});



		newButtonGo.transform.Find("RepOptions/RowHB/Options/HBMetaphoreButtons/Smooth/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setHyperBallMetaphore(selName, "Smooth");
		});
		newButtonGo.transform.Find("RepOptions/RowHB/Options/HBMetaphoreButtons/Balls&Sticks/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setHyperBallMetaphore(selName, "Balls&Sticks");
		});
		newButtonGo.transform.Find("RepOptions/RowHB/Options/HBMetaphoreButtons/Licorice/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setHyperBallMetaphore(selName, "Licorice");
		});
		newButtonGo.transform.Find("RepOptions/RowHB/Options/HBMetaphoreButtons/VdW/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setHyperBallMetaphore(selName, "VdW");
		});


		newButtonGo.transform.Find("RepOptions/RowHB/Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "hb");
		});

		newButtonGo.transform.Find("RepOptions/RowHB/Options/ColorButtons2/RemoveAO/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.clearHyperballAO(selName);
		});
		newButtonGo.transform.Find("RepOptions/RowHB/Options/ColorButtons2/ShadowsOn/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setShadows(selName, "hb", true);
		});
		newButtonGo.transform.Find("RepOptions/RowHB/Options/ColorButtons2/ShadowsOff/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setShadows(selName, "hb", false);
		});
		newButtonGo.transform.Find("RepOptions/RowHB/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "hb");
		});
		newButtonGo.transform.Find("RepOptions/RowHB/Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByResidue(selName, "hb");
		});

		ColorPickerControl hbColorPicker = newButtonGo.transform.Find("RepOptions/RowHB/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		hbColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "hb", hbColorPicker.CurrentColor);
		});


		Slider sliderShrink = newButtonGo.transform.Find("RepOptions/RowHB/Options/SliderShrink/Slider").gameObject.GetComponent<Slider>();
		sliderShrink.onValueChanged.AddListener(
		delegate {
			APIPython.setHyperballShrink(selName, sliderShrink.value);
		});

		Slider sliderScale = newButtonGo.transform.Find("RepOptions/RowHB/Options/SliderScale/Slider").gameObject.GetComponent<Slider>();
		sliderScale.onValueChanged.AddListener(
		delegate {
			APIPython.setRepSize(selName, "hb", sliderScale.value);
		});

		Slider sliderShine = newButtonGo.transform.Find("RepOptions/RowHB/Options/SliderShininess/Slider").gameObject.GetComponent<Slider>();
		sliderShine.onValueChanged.AddListener(
		delegate {
			APIPython.setHyperBallShininess(selName, sliderShine.value);
		});

		createTextureMenu(newButtonGo.transform.Find("RepOptions/RowHB/Options/Texture Scroll View/Viewport/Content"), selName);



		//Lines ------------------------------------
		newButtonGo.transform.Find("RepOptions/RowL/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "l")) {
				APIPython.hideSelection(selName, "l");
			}
			else {
				APIPython.showSelection(selName, "l");
			}
		});

		newButtonGo.transform.Find("RepOptions/RowL/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			// APIPython.hideSelection(selName, "l");
			APIPython.deleteRepresentationInSelection(selName, "l");
			rowLine.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});


		Slider lineSliderSize = newButtonGo.transform.Find("RepOptions/RowL/Options/SliderSize/SliderWidth").gameObject.GetComponent<Slider>();
		lineSliderSize.onValueChanged.AddListener(
		delegate {
			APIPython.setLineSize(selName, lineSliderSize.value);
		});


		newButtonGo.transform.Find("RepOptions/RowL/Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "l");
		});

		newButtonGo.transform.Find("RepOptions/RowL/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "l");
		});
		newButtonGo.transform.Find("RepOptions/RowL/Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByResidue(selName, "l");
		});

		ColorPickerControl lineColorPicker = newButtonGo.transform.Find("RepOptions/RowL/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		lineColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "l", lineColorPicker.CurrentColor);
		});

		//Bond Order ------------------------------------------
		newButtonGo.transform.Find("RepOptions/RowBONDORDER/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "bondorder")) {
				APIPython.hideSelection(selName, "bondorder");
			}
			else {
				APIPython.showSelection(selName, "bondorder");
			}
		});

		newButtonGo.transform.Find("RepOptions/RowBONDORDER/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			// APIPython.hideSelection(selName, "bondorder");
			APIPython.deleteRepresentationInSelection(selName, "bondorder");
			rowBO.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});


		newButtonGo.transform.Find("RepOptions/RowBONDORDER/Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "bondorder");
		});

		newButtonGo.transform.Find("RepOptions/RowBONDORDER/Options/ColorButtons2/ShadowsOn/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setShadows(selName, "bondorder", true);
		});
		newButtonGo.transform.Find("RepOptions/RowBONDORDER/Options/ColorButtons2/ShadowsOff/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setShadows(selName, "bondorder", false);
		});
		newButtonGo.transform.Find("RepOptions/RowBONDORDER/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "bondorder");
		});
		newButtonGo.transform.Find("RepOptions/RowBONDORDER/Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByResidue(selName, "bondorder");
		});

		ColorPickerControl hbColorPickerBO = newButtonGo.transform.Find("RepOptions/RowBONDORDER/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		hbColorPickerBO.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "bondorder", hbColorPickerBO.CurrentColor);
		});


		// Slider sliderShrink = newButtonGo.transform.Find("RepOptions/RowBONDORDER/Options/SliderShrink/Slider").gameObject.GetComponent<Slider>();
		// sliderShrink.onValueChanged.AddListener(
		// delegate {
		// 	APIPython.setHyperballShrink(selName, sliderShrink.value);
		// });

		// Slider sliderScale = newButtonGo.transform.Find("RepOptions/RowBONDORDER/Options/SliderScale/Slider").gameObject.GetComponent<Slider>();
		// sliderScale.onValueChanged.AddListener(
		// delegate {
		// 	APIPython.setRepSize(selName, "bondorder", sliderScale.value);
		// });


		createTextureMenu(newButtonGo.transform.Find("RepOptions/RowBONDORDER/Options/Texture Scroll View/Viewport/Content"), selName, true);


		//Surface ------------------------------------
		newButtonGo.transform.Find("RepOptions/RowS/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "s")) {
				APIPython.hideSelection(selName, "s");
			}
			else {
				APIPython.showSelection(selName, "s");
			}
		});



		newButtonGo.transform.Find("RepOptions/RowS/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			// APIPython.hideSelection(selName, "s");
			APIPython.deleteRepresentationInSelection(selName, "s");
			rowSurf.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});

		Text labelSurfMethod = newButtonGo.transform.Find("RepOptions/RowS/Options/SurfaceComputeMethod/LabelSurfMethod/Text").gameObject.GetComponent<Text>();
		SurfMethod surfMet = getSurfMethodInSelection(selName);
		labelSurfMethod.text = surfMet.ToString();

		newButtonGo.transform.Find("RepOptions/RowS/Options/SurfaceComputeMethod/SwitchSurfMethod/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.switchSurfaceComputeMethod(selName);
			SurfMethod surfMethod = getSurfMethodInSelection(selName);
			labelSurfMethod.text = surfMethod.ToString();
		});

		Toggle cutSurf = newButtonGo.transform.Find("RepOptions/RowS/Options/ToggleIsCut/Toggle").gameObject.GetComponent<Toggle>();
		cutSurf.onValueChanged.AddListener(
		delegate {
			APIPython.switchCutSurface(selName, cutSurf.isOn);
		});


		newButtonGo.transform.Find("RepOptions/RowS/Options/SurfMatButtons/Normal/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setSolidSurface(selName);
		});
		newButtonGo.transform.Find("RepOptions/RowS/Options/SurfMatButtons/Wireframe/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setWireframeSurface(selName);
		});
		newButtonGo.transform.Find("RepOptions/RowS/Options/SurfMatButtons/Transparent/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setTransparentSurface(selName);
		});


		newButtonGo.transform.Find("RepOptions/RowS/Options/ColorButtons/ColorByAtom/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByAtom(selName, "s");
		});
		newButtonGo.transform.Find("RepOptions/RowS/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "s");
		});
		newButtonGo.transform.Find("RepOptions/RowS/Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByResidue(selName, "s");
		});

		newButtonGo.transform.Find("RepOptions/RowS/Options/ColorButtons2/ColorByCharge/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByCharge(selName);
		});

		newButtonGo.transform.Find("RepOptions/RowS/Options/ColorButtons2/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "s");
		});
		newButtonGo.transform.Find("RepOptions/RowS/Options/ColorButtons3/ShadowsOn/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setShadows(selName, "s", true);
		});

		newButtonGo.transform.Find("RepOptions/RowS/Options/ColorButtons3/ShadowsOff/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setShadows(selName, "s", false);
		});

		newButtonGo.transform.Find("RepOptions/RowS/Options/ColorButtons3/RemoveAO/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.clearSurfaceAO(selName);
		});

		ColorPickerControl surfColorPicker = newButtonGo.transform.Find("RepOptions/RowS/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		surfColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "s", surfColorPicker.CurrentColor);
		});

		Slider surfSliderSmooth = newButtonGo.transform.Find("RepOptions/RowS/Options/SliderSmooth/Slider").gameObject.GetComponent<Slider>();
		surfSliderSmooth.onValueChanged.AddListener(
		delegate {
			APIPython.setSmoothness(selName, "s", surfSliderSmooth.value);
		});

		Slider surfSliderMetal = newButtonGo.transform.Find("RepOptions/RowS/Options/SliderMetal/Slider").gameObject.GetComponent<Slider>();
		surfSliderMetal.onValueChanged.AddListener(
		delegate {
			APIPython.setMetal(selName, "s", surfSliderMetal.value);
		});

		Slider surfSliderAlpha = newButtonGo.transform.Find("RepOptions/RowS/Options/SliderAlpha/Slider").gameObject.GetComponent<Slider>();
		surfSliderAlpha.onValueChanged.AddListener(
		delegate {
			APIPython.setTransparentSurface(selName, surfSliderAlpha.value);
		});
		Slider surfSliderWire = newButtonGo.transform.Find("RepOptions/RowS/Options/SliderWire/Slider").gameObject.GetComponent<Slider>();
		surfSliderWire.onValueChanged.AddListener(
		delegate {
			APIPython.setSurfaceWireframe(selName, "s", surfSliderWire.value);
		});


		//Hbond ------------------------------------
		newButtonGo.transform.Find("RepOptions/RowHBOND/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "hbond")) {
				APIPython.hideSelection(selName, "hbond");
			}
			else {
				APIPython.showSelection(selName, "hbond");
			}
		});

		newButtonGo.transform.Find("RepOptions/RowHBOND/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			// APIPython.hideSelection(selName, "hbond");
			APIPython.deleteRepresentationInSelection(selName, "hbond");
			rowHbond.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});

		newButtonGo.transform.Find("RepOptions/RowHBOND/Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "hbond");
		});


		newButtonGo.transform.Find("RepOptions/RowHBOND/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "hbond");
		});


		ColorPickerControl hbondColorPicker = newButtonGo.transform.Find("RepOptions/RowHBOND/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		hbondColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "hbond", hbondColorPicker.CurrentColor);
		});


		//Hbondtube ------------------------------------
		newButtonGo.transform.Find("RepOptions/RowHBONDTUBE/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "hbondtube")) {
				APIPython.hideSelection(selName, "hbondtube");
			}
			else {
				APIPython.showSelection(selName, "hbondtube");
			}
		});

		newButtonGo.transform.Find("RepOptions/RowHBONDTUBE/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			// APIPython.hideSelection(selName, "hbondtube");
			APIPython.deleteRepresentationInSelection(selName, "hbondtube");
			rowHbondtube.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});

		newButtonGo.transform.Find("RepOptions/RowHBONDTUBE/Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "hbondtube");
		});


		newButtonGo.transform.Find("RepOptions/RowHBONDTUBE/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "hbondtube");
		});


		ColorPickerControl hbondtubeColorPicker = newButtonGo.transform.Find("RepOptions/RowHBONDTUBE/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		hbondtubeColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "hbondtube", hbondtubeColorPicker.CurrentColor);
		});


		//Fieldlines ------------------------------------
		newButtonGo.transform.Find("RepOptions/RowFL/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "fl")) {
				APIPython.hideSelection(selName, "fl");
			}
			else {
				APIPython.showSelection(selName, "fl");
			}
		});

		newButtonGo.transform.Find("RepOptions/RowFL/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			// APIPython.hideSelection(selName, "hbond");
			APIPython.deleteRepresentationInSelection(selName, "fl");
			rowFL.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});

		//Slider
		Text flSliderText = newButtonGo.transform.Find("RepOptions/RowFL/Options/SliderFL/MagnLabel/Text").gameObject.GetComponent<Text>();
		Slider flSliderMagn = newButtonGo.transform.Find("RepOptions/RowFL/Options/SliderFL/Slider").gameObject.GetComponent<Slider>();
		SliderDrag flSliderMagnDrag = flSliderMagn.gameObject.GetComponent<SliderDrag>();

		flSliderMagn.onValueChanged.AddListener(
		delegate {
			flSliderText.text = "Gradient magnitude : " + flSliderMagn.value.ToString("F1");
		});

		flSliderMagnDrag.EndDrag +=  delegate {
			APIPython.setFieldlineGradientThreshold(selName, flSliderMagn.value);
		};

		Text flSliderSizeText = newButtonGo.transform.Find("RepOptions/RowFL/Options/SliderSizeFL/SizeLabel/Text").gameObject.GetComponent<Text>();
		Slider flSliderSize = newButtonGo.transform.Find("RepOptions/RowFL/Options/SliderSizeFL/Slider").gameObject.GetComponent<Slider>();
		SliderDrag flSliderSizeDrag = flSliderSize.gameObject.GetComponent<SliderDrag>();

		flSliderSize.onValueChanged.AddListener(
		delegate {
			flSliderSizeText.text = "Line width : " + flSliderSize.value.ToString("F2");
		});

		flSliderSizeDrag.EndDrag +=
		delegate {
			if (selM.selections.ContainsKey(selName)) {

				RepType repType = APIPython.getRepType("fl");

				List<UnityMolRepresentation> existingReps = repM.representationExists(selName, repType);
				foreach (UnityMolRepresentation rep in existingReps) {
					SubRepresentation sr = rep.subReps.First();//There shouldn't be more than one
					FieldLinesRepresentationManager flRepM = (FieldLinesRepresentationManager) sr.atomRepManager;
					flRepM.SetSizes(flSliderSize.value);
				}
			}
		};

		Text flSliderLengthText = newButtonGo.transform.Find("RepOptions/RowFL/Options/SliderLengthFL/LengthLabel/Text").gameObject.GetComponent<Text>();
		Slider flSliderLength = newButtonGo.transform.Find("RepOptions/RowFL/Options/SliderLengthFL/Slider").gameObject.GetComponent<Slider>();

		flSliderLength.onValueChanged.AddListener(
		delegate {
			flSliderLengthText.text = "Line length : " + flSliderLength.value.ToString("F2");

			if (selM.selections.ContainsKey(selName)) {

				RepType repType = APIPython.getRepType("fl");

				List<UnityMolRepresentation> existingReps = repM.representationExists(selName, repType);
				foreach (UnityMolRepresentation rep in existingReps) {
					SubRepresentation sr = rep.subReps.First();//There shouldn't be more than one
					FieldLinesRepresentationManager flRepM = (FieldLinesRepresentationManager) sr.atomRepManager;
					flRepM.SetLengthLine(flSliderLength.value);
				}
			}
		});


		Text flSliderSpeedText = newButtonGo.transform.Find("RepOptions/RowFL/Options/SliderSpeedFL/SpeedLabel/Text").gameObject.GetComponent<Text>();
		Slider flSliderSpeed = newButtonGo.transform.Find("RepOptions/RowFL/Options/SliderSpeedFL/Slider").gameObject.GetComponent<Slider>();

		flSliderSpeed.onValueChanged.AddListener(
		delegate {
			flSliderSpeedText.text = "Line speed : " + flSliderSpeed.value.ToString("F2");

			if (selM.selections.ContainsKey(selName)) {

				RepType repType = APIPython.getRepType("fl");

				List<UnityMolRepresentation> existingReps = repM.representationExists(selName, repType);
				foreach (UnityMolRepresentation rep in existingReps) {
					SubRepresentation sr = rep.subReps.First();//There shouldn't be more than one
					FieldLinesRepresentationManager flRepM = (FieldLinesRepresentationManager) sr.atomRepManager;
					flRepM.SetSpeedLine(flSliderSpeed.value);
				}
			}
		});


		ColorPickerControl startFLColorPicker = newButtonGo.transform.Find("RepOptions/RowFL/Options/ColorPickerStart").gameObject.GetComponent<ColorPickerControl>();
		ColorPickerControl endFLColorPicker = newButtonGo.transform.Find("RepOptions/RowFL/Options/ColorPickerEnd").gameObject.GetComponent<ColorPickerControl>();

		startFLColorPicker.onValueChanged.AddListener(
		delegate {
			if (selM.selections.ContainsKey(selName)) {

				RepType repType = APIPython.getRepType("fl");

				List<UnityMolRepresentation> existingReps = repM.representationExists(selName, repType);
				foreach (UnityMolRepresentation rep in existingReps) {
					SubRepresentation sr = rep.subReps.First();//There shouldn't be more than one
					FieldLinesRepresentationManager flRepM = (FieldLinesRepresentationManager) sr.atomRepManager;
					flRepM.SetStartColor(startFLColorPicker.CurrentColor);
				}
			}
		});

		endFLColorPicker.onValueChanged.AddListener(
		delegate {
			if (selM.selections.ContainsKey(selName)) {

				RepType repType = APIPython.getRepType("fl");

				List<UnityMolRepresentation> existingReps = repM.representationExists(selName, repType);
				foreach (UnityMolRepresentation rep in existingReps) {
					SubRepresentation sr = rep.subReps.First();//There shouldn't be more than one
					FieldLinesRepresentationManager flRepM = (FieldLinesRepresentationManager) sr.atomRepManager;
					flRepM.SetEndColor(endFLColorPicker.CurrentColor);
				}
			}
		});

		//Trace ------------------------------------
		newButtonGo.transform.Find("RepOptions/RowTRACE/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "trace")) {
				APIPython.hideSelection(selName, "trace");
			}
			else {
				APIPython.showSelection(selName, "trace");
			}
		});

		newButtonGo.transform.Find("RepOptions/RowTRACE/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.deleteRepresentationInSelection(selName, "trace");
			rowTrace.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});


		Slider traceSize = newButtonGo.transform.Find("RepOptions/RowTRACE/Options/SliderSize/SliderWidth").gameObject.GetComponent<Slider>();
		traceSize.onValueChanged.AddListener(
		delegate {
			APIPython.setTraceSize(selName, traceSize.value);
		});


		newButtonGo.transform.Find("RepOptions/RowTRACE/Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "trace");
		});

		newButtonGo.transform.Find("RepOptions/RowTRACE/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "trace");
		});
		newButtonGo.transform.Find("RepOptions/RowTRACE/Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByResidue(selName, "trace");
		});

		ColorPickerControl traceColorPicker = newButtonGo.transform.Find("RepOptions/RowTRACE/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		traceColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "trace", traceColorPicker.CurrentColor);
		});

		//SugarRibbons ------------------------------------
		newButtonGo.transform.Find("RepOptions/RowSUGARRIBBONS/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "sugarribbons")) {
				APIPython.hideSelection(selName, "sugarribbons");
			}
			else {
				APIPython.showSelection(selName, "sugarribbons");
			}
		});

		newButtonGo.transform.Find("RepOptions/RowSUGARRIBBONS/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.deleteRepresentationInSelection(selName, "sugarribbons");
			rowSugar.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});


		newButtonGo.transform.Find("RepOptions/RowSUGARRIBBONS/Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "sugarribbons");
		});

		newButtonGo.transform.Find("RepOptions/RowSUGARRIBBONS/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "sugarribbons");
		});
		newButtonGo.transform.Find("RepOptions/RowSUGARRIBBONS/Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByResidue(selName, "sugarribbons");
		});

		ColorPickerControl sugarColorPicker = newButtonGo.transform.Find("RepOptions/RowSUGARRIBBONS/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		sugarColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "sugarribbons", sugarColorPicker.CurrentColor);
		});

		//DXSurface ------------------------------------
		Slider dxSliderIso = newButtonGo.transform.Find("RepOptions/RowDXISO/Options/SliderIso/Slider").gameObject.GetComponent<Slider>();
		SliderDrag dxSliderIsoDrag = dxSliderIso.gameObject.GetComponent<SliderDrag>();

		newButtonGo.transform.Find("RepOptions/RowDXISO/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "dxiso")) {
				APIPython.hideSelection(selName, "dxiso");
			}
			else {
				UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
				UnityMolSelection cursel = selM.selections[selName];
				UnityMolStructure s = cursel.structures[0];
				APIPython.showSelection(selName, "dxiso", s.dxr, dxSliderIso.value);
			}
		});



		newButtonGo.transform.Find("RepOptions/RowDXISO/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			// APIPython.hideSelection(selName, "dxiso");
			APIPython.deleteRepresentationInSelection(selName, "dxiso");
			rowDX.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});


		newButtonGo.transform.Find("RepOptions/RowDXISO/Options/SurfMatButtons/Normal/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setSolidSurface(selName);
		});
		newButtonGo.transform.Find("RepOptions/RowDXISO/Options/SurfMatButtons/Wireframe/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setWireframeSurface(selName);
		});
		newButtonGo.transform.Find("RepOptions/RowDXISO/Options/SurfMatButtons/Transparent/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setTransparentSurface(selName);
		});


		newButtonGo.transform.Find("RepOptions/RowDXISO/Options/ColorButtons/ColorByAtom/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByAtom(selName, "dxiso");
		});
		newButtonGo.transform.Find("RepOptions/RowDXISO/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "dxiso");
		});
		newButtonGo.transform.Find("RepOptions/RowDXISO/Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByResidue(selName, "dxiso");
		});

		newButtonGo.transform.Find("RepOptions/RowDXISO/Options/ColorButtons2/ColorByCharge/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByCharge(selName);
		});

		newButtonGo.transform.Find("RepOptions/RowDXISO/Options/ColorButtons2/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "dxiso");
		});

		ColorPickerControl dxColorPicker = newButtonGo.transform.Find("RepOptions/RowDXISO/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		dxColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "dxiso", dxColorPicker.CurrentColor);
		});


		// dxSliderIsoDrag.EndDrag +=  delegate {
		// APIPython.updateDXIso(selName, dxSliderIso.value);
		// };

		dxSliderIso.onValueChanged.AddListener(
		delegate {
			APIPython.updateDXIso(selName, dxSliderIso.value);
		}
		);


		Slider dxSliderSmooth = newButtonGo.transform.Find("RepOptions/RowDXISO/Options/SliderSmooth/Slider").gameObject.GetComponent<Slider>();
		dxSliderSmooth.onValueChanged.AddListener(
		delegate {
			APIPython.setSmoothness(selName, "dxiso", dxSliderSmooth.value);
		});

		Slider dxSliderMetal = newButtonGo.transform.Find("RepOptions/RowDXISO/Options/SliderMetal/Slider").gameObject.GetComponent<Slider>();
		dxSliderMetal.onValueChanged.AddListener(
		delegate {
			APIPython.setMetal(selName, "dxiso", dxSliderMetal.value);
		});

		Slider dxSliderAlpha = newButtonGo.transform.Find("RepOptions/RowDXISO/Options/SliderAlpha/Slider").gameObject.GetComponent<Slider>();
		dxSliderAlpha.onValueChanged.AddListener(
		delegate {
			APIPython.setTransparentSurface(selName, dxSliderAlpha.value);
		});
		Slider dxSliderWire = newButtonGo.transform.Find("RepOptions/RowDXISO/Options/SliderWire/Slider").gameObject.GetComponent<Slider>();
		dxSliderWire.onValueChanged.AddListener(
		delegate {
			APIPython.setSurfaceWireframe(selName, "dxiso", dxSliderWire.value);
		});

		//Ellipsoid ------------------------------------
		newButtonGo.transform.Find("RepOptions/RowELLIPSOID/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "ellipsoid")) {
				APIPython.hideSelection(selName, "ellipsoid");
			}
			else {
				APIPython.showSelection(selName, "ellipsoid");
			}
		});

		newButtonGo.transform.Find("RepOptions/RowELLIPSOID/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			// APIPython.hideSelection(selName, "ellipsoid");
			APIPython.deleteRepresentationInSelection(selName, "ellipsoid");
			rowEllips.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});


		Slider ellipsoidSliderSize = newButtonGo.transform.Find("RepOptions/RowELLIPSOID/Options/SliderSize/SliderWidth").gameObject.GetComponent<Slider>();
		ellipsoidSliderSize.onValueChanged.AddListener(
		delegate {
			APIPython.setRepSize(selName, "ellipsoid", ellipsoidSliderSize.value);
		});


		newButtonGo.transform.Find("RepOptions/RowELLIPSOID/Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "ellipsoid");
		});

		newButtonGo.transform.Find("RepOptions/RowELLIPSOID/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "ellipsoid");
		});
		newButtonGo.transform.Find("RepOptions/RowELLIPSOID/Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByResidue(selName, "ellipsoid");
		});

		ColorPickerControl ellipsoidColorPicker = newButtonGo.transform.Find("RepOptions/RowELLIPSOID/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		ellipsoidColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "ellipsoid", ellipsoidColorPicker.CurrentColor);
		});

		//Point ------------------------------------
		newButtonGo.transform.Find("RepOptions/RowPOINT/OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(selName, "p")) {
				APIPython.hideSelection(selName, "p");
			}
			else {
				APIPython.showSelection(selName, "p");
			}
		});

		newButtonGo.transform.Find("RepOptions/RowPOINT/OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			// APIPython.hideSelection(selName, "p");
			APIPython.deleteRepresentationInSelection(selName, "p");
			rowPoint.SetActive(false);
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
		});


		Slider pointSliderSize = newButtonGo.transform.Find("RepOptions/RowPOINT/Options/SliderSize/SliderWidth").gameObject.GetComponent<Slider>();
		pointSliderSize.onValueChanged.AddListener(
		delegate {
			APIPython.setRepSize(selName, "p", pointSliderSize.value);
		});


		newButtonGo.transform.Find("RepOptions/RowPOINT/Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.resetColorSelection(selName, "p");
		});

		newButtonGo.transform.Find("RepOptions/RowPOINT/Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByChain(selName, "p");
		});
		newButtonGo.transform.Find("RepOptions/RowPOINT/Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.colorByResidue(selName, "p");
		});

		ColorPickerControl pointColorPicker = newButtonGo.transform.Find("RepOptions/RowPOINT/Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

		pointColorPicker.onValueChanged.AddListener(
		delegate {
			APIPython.colorSelection(selName, "p", pointColorPicker.CurrentColor);
		});



	}

	private void createTextureMenu(Transform textureMenuParent, string selName, bool isBondOrder = false) {
		UnityEngine.Object[] allMatCapTextures = UnityMolMain.atomColors.textures;


		GameObject emptytexButton = Instantiate((GameObject) Resources.Load("Prefabs/TextureButton"));
		emptytexButton.transform.SetParent(textureMenuParent, false);
		RawImage emptyimg = emptytexButton.GetComponent<RawImage>();
		emptyimg.texture = null;

		emptytexButton.gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.setHyperballTexture(selName, -1);
		});

		for (int idTex = 0; idTex < allMatCapTextures.Length; idTex++) {
			GameObject texButton = Instantiate((GameObject) Resources.Load("Prefabs/TextureButton"));
			texButton.transform.SetParent(textureMenuParent, false);
			RawImage img = texButton.GetComponent<RawImage>();
			img.texture = (Texture) allMatCapTextures[idTex];

			int tmpTexId = idTex;

			if (!isBondOrder) {
				texButton.gameObject.GetComponent<Button>().onClick.AddListener(
				delegate {
					APIPython.setHyperballTexture(selName, tmpTexId);
				});
			}
			else {
				texButton.gameObject.GetComponent<Button>().onClick.AddListener(
				delegate {
					APIPython.setBondOrderTexture(selName, tmpTexId);
				});
			}
			idTex++;
		}

	}

	private void setupSelectionNamesButtons(GameObject newButtonGo, UnityMolSelection sel) {
		InputField nameIF = newButtonGo.transform.Find("EditSelection/InputFieldName").gameObject.GetComponent<InputField>();
		nameIF.onEndEdit.AddListener(
		delegate {
			APIPython.renameSelection(sel.name, nameIF.text);
			updateSelectionUI();
			updateRepresentationUI();
			expandEditOptions(sel);
		});



		InputField selIF = newButtonGo.transform.Find("EditSelection/InputFieldSelectionString").gameObject.GetComponent<InputField>();
		// selIF.text = Truncate(sel.ToSelectionCommand(), 100);
		selIF.text = sel.ToSelectionCommand();

		selIF.onEndEdit.AddListener(
		delegate {
			bool success = APIPython.updateSelectionWithMDA(sel.name, selIF.text, true);

			updateSelectionUI();
		});

		Toggle updateToggle = newButtonGo.transform.Find("EditSelection/ToggleUpdateWithTraj/Toggle").gameObject.GetComponent<Toggle>();
		updateToggle.onValueChanged.AddListener(
		delegate {
			APIPython.setUpdateSelectionTraj(sel.name, updateToggle.isOn);
		});
	}

	private void expandEditOptions(UnityMolSelection sel) {
		selectionToUIObject[sel.name].transform.Find("EditSelection").gameObject.SetActive(true);
	}

	public static string Truncate(string value, int maxLength)
	{
		if (string.IsNullOrEmpty(value)) return value;
		return value.Length <= maxLength ? value : value.Substring(0, maxLength);
	}

	private bool isSelectionUIUpdated() {
		int cptAtomSel = 0;

		foreach (string s in selM.selections.Keys) {
			if (!selectionToUIObject.ContainsKey(s)) {
				return false;
			}
			cptAtomSel += selM.selections[s].Count;
		}
		foreach (string s in selectionToUIObject.Keys) {
			if (!selM.selections.ContainsKey(s)) {
				return false;
			}
		}

		if (cptAtomSel != UItotalAtomSelected) {
			UItotalAtomSelected = cptAtomSel;
			return false;
		}

		return true;
	}


	private void updateRepresentationUI() {

		RepType surfRepType = APIPython.getRepType("s");

		foreach (string selS in selectionToUIObject.Keys) {
			GameObject selUIgo = selectionToUIObject[selS];

			foreach (RepType rept in selM.selections[selS].representations.Keys) {

				GameObject toggleRepType = repTypeToGo(selUIgo, rept);

				if (toggleRepType != null) {
					bool isActive = false;
					if (selM.selections[selS].representations[rept].Count > 0) {
						isActive = true;
						if (rept == surfRepType) { //Update surface type label
							Text labelSurfMethod = toggleRepType.transform.Find("Options/SurfaceComputeMethod/LabelSurfMethod/Text").gameObject.GetComponent<Text>();
							SurfMethod surfMet = getSurfMethodInSelection(selS);
							labelSurfMethod.text = surfMet.ToString();
						}
					}
					toggleRepType.SetActive(isActive);
				}
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(loadedMolUITransform.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(globalSelectionsUITransform.GetComponent<RectTransform>());


		UIcurActiveRep = repM.activeRepresentations.Count;
	}

	private GameObject repTypeToGo(GameObject selUIGo, RepType rept) {
		string typeS = APIPython.getTypeFromRepType(rept).ToUpper();
		Transform t = selUIGo.transform.Find("RepOptions/Row" + typeS);
		if (t != null) {
			return t.gameObject;
		}
		return null;
	}

	public SurfMethod getSurfMethodInSelection(string selName) {
		SurfMethod res = SurfMethod.EDTSurf;
		if (selM.selections.ContainsKey(selName)) {
			UnityMolSelection sel = selM.selections[selName];

			RepType repType = APIPython.getRepType("s");

			List<UnityMolRepresentation> existingReps = repM.representationExists(selName, repType);
			if (existingReps != null) {
				UnityMolRepresentation existingRep = existingReps.First();
				SubRepresentation sr = existingRep.subReps.First();
				SurfaceRepresentation surfRep = (SurfaceRepresentation) sr.atomRep;
				if (surfRep.isStandardSurface) {
					res = surfRep.surfMethod;
				}
			}
		}
		return res;
	}


	public void switchDepthCueing(Text t) {
		if (UnityMolMain.isFogOn) {
			APIPython.disableDepthCueing();
		}
		else {
			APIPython.enableDepthCueing();
		}

		t.text = "Depth Cueing\n <b><color=#007AC1>";

		if (UnityMolMain.isFogOn) {
			t.text += "On";
		}
		else {
			t.text += "Off";
		}
		t.text += "</color></b>";

	}
	public void changeFogStart(Slider s) {
		APIPython.setDepthCueingStart(s.value);
	}
	public void changeFogDensity(Slider s) {
		APIPython.setDepthCueingDensity(s.value);
	}

	public void switchRoomMode() {
		if (room != null) {
			room.SetActive(!room.activeInHierarchy);
			if (floor != null) {
				floor.SetActive(!room.activeInHierarchy);
			}
		}
	}

	public void showHideSC() {
		if (selM.currentSelection != null && selM.currentSelection.Count != 0) {
			APIPython.showHideSideChainsInSelection(selM.currentSelection.name);
		}
		else {
			Debug.LogWarning("Empty selection");
		}
	}
	public void showHideH() {
		if (selM.currentSelection != null && selM.currentSelection.Count != 0) {
			APIPython.showHideHydrogensInSelection(selM.currentSelection.name);
		}
		else {
			Debug.LogWarning("Empty selection");
		}
	}
	public void showHideBB() {
		if (selM.currentSelection != null && selM.currentSelection.Count != 0) {
			APIPython.showHideBackboneInSelection(selM.currentSelection.name);
		}
		else {
			Debug.LogWarning("Empty selection");
		}
	}
	public void changeScale(Slider s) {
		APIPython.changeGeneralScale(s.value);
	}

	public void switchSeqViewer() {
		if (seqUIGo != null) {
			seqUIGo.SetActive(!seqUIGo.activeInHierarchy);
		}
	}

	public void centerViewStructure() {
		if (selM.currentSelection != null && selM.currentSelection.Count != 0) {
			APIPython.centerOnStructure(selM.currentSelection.structures[0].uniqueName, lerp: true);
		}
		else {
			Debug.LogWarning("Empty selection");
		}
	}
	public void centerViewSelection() {
		if (selM.currentSelection != null && selM.currentSelection.Count != 0) {
			APIPython.centerOnSelection(selM.currentSelection.name, lerp: true);
		}
		else {
			Debug.LogWarning("Empty selection");
		}
	}
	public void cealignLasts() {
		int N = sm.loadedStructures.Count;
		if (N < 2) {
			Debug.LogWarning("Not enough structures to align");
			return;
		}
		string s1 = sm.loadedStructures[N - 2].ToSelectionName();
		string s2 = sm.loadedStructures[N - 1].ToSelectionName();
		APIPython.cealign(s1, s2);
	}

	public void outlineSwitch(Text t) {
		t.text = "Outline\n<b><color=#007AC1>";

		OutlineEffectUtil outlineScript = Camera.main.gameObject.GetComponent<OutlineEffectUtil>();
		if (outlineScript == null) {
			t.text += "Off";
			t.text += "</color></b>";
			return;
		}

		if (outlineScript.isOn) {
			APIPython.disableOutline();
			t.text += "Off";
		}
		else {
			APIPython.enableOutline();
			t.text += "On";
		}
		t.text += "</color></b>";
	}

	public void clearAnnotations() {
		APIPython.clearAnnotations();

		var foundPM = FindObjectsOfType<PointerMeasurements>();
		foreach (var pm in foundPM) {
			pm.resetTouchedAtoms();
		}
	}

	public UnityMolSelection selectActiveSite(string structureName) {
		return APIPython.select(structureName + " and byres protein and within 4.0 ligand", structureName + " and byres protein and within 4.0 ligand");
	}

	public UnityMolSelection selectActiveSiteAndLigand(string structureName) {
		return APIPython.select(structureName + " and ligand or byres protein and within 4.0 ligand", structureName + " and ligand or byres protein and within 4.0 ligand");
	}

	public void showHbondInterface(string structureName) {
		UnityMolSelection sel = selectActiveSiteAndLigand(structureName);

		//Remove existing hbond representation
		APIPython.deleteRepresentationInSelection(sel.name, "hbond");

		//Show hbonds
		APIPython.showSelection(sel.name, "hbond");

		MDAnalysisSelection selec2 = new MDAnalysisSelection("ligand", sel.atoms);
		UnityMolSelection ligandSelection = selec2.process();

		RepType repType = APIPython.getRepType("hbond");

		foreach (UnityMolRepresentation rep in sel.representations[repType]) {
			foreach (SubRepresentation sr in rep.subReps) {
				BondRepresentationHbonds brep = (BondRepresentationHbonds)sr.bondRep;
				brep.hbonds = filterHbondsNotInterface(ligandSelection, brep.hbonds);
				brep.recomputeLight();
			}
		}
	}

	public void saveScreenshot() {

		string directorypath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/UnityMolScreenshots/";
		if (!System.IO.Directory.Exists(directorypath)) {
			System.IO.Directory.CreateDirectory(directorypath);
		}
		int idI = 0;
		string filePath = directorypath + "Umol_" + idI.ToString() + ".png";
		while (System.IO.File.Exists(filePath)) {
			idI++;
			filePath = directorypath + "Umol_" + idI.ToString() + ".png";
		}
		APIPython.screenshot(filePath, 1920, 1080);
	}

	/// <summary>
	/// Returns a new UnityMolBonds with only bonds that are not both ligand or both protein
	/// </summary>
	public UnityMolBonds filterHbondsNotInterface(UnityMolSelection ligand, UnityMolBonds bonds) {
		UnityMolBonds res = new UnityMolBonds();

		var keys = bonds.Dbonds.Keys;

		foreach (UnityMolAtom atom1 in keys) {
			bool atom1Lig = ligand.atoms.Contains(atom1);

			for (int at = 0; at < bonds.Dbonds[atom1].Length; at++) {
				UnityMolAtom atom2 = bonds.Dbonds[atom1][at];

				if (bonds.Dbonds[atom1][at] != null) {
					bool atom2Lig = ligand.atoms.Contains(atom2);

					if ( (!atom1Lig && atom2Lig) ||
					        (atom1Lig && !atom2Lig) ) { //Test if atoms are not both ligand or both protein
						res.Add(atom1, atom2);
					}
				}
			}
		}

		return res;
	}


	public void switchDrawMode(Text t) {
		t.text = "Draw mode\n <b><color=#007AC1>";

		UnityMolAnnotationManager am = UnityMolMain.getAnnotationManager();
		am.drawMode = !am.drawMode;

		if (am.drawMode) {
			t.text += "On";
		}
		else {
			t.text += "Off";
		}
		t.text += "</color></b>";
	}

	public void clearDrawings() {
		APIPython.clearDrawings();
	}

	public void connectIMD() {
		if (inFIMDIP == null || inFIMDPort == null) {
			inFIMDIP = GameObject.Find("InputFieldAddress/InputField").GetComponent<InputField>();
			inFIMDPort = GameObject.Find("InputFieldPort/InputField").GetComponent<InputField>();
		}
		if (!string.IsNullOrEmpty(inFIMDIP.text) && !string.IsNullOrEmpty(inFIMDPort.text)) {
			int p = int.Parse(inFIMDPort.text);
			if (APIPython.last() != null) {
				APIPython.connectIMD(APIPython.last().uniqueName, inFIMDIP.text, p);
			}
			else {
				Debug.LogWarning("Load a molecule first");
			}
		}
		else {
			Debug.LogWarning("Please enter IP address and port");
		}
	}
	public void disconnectIMD() {
		APIPython.disconnectIMD(APIPython.last().uniqueName);
	}

	public void showSurface() {
		APIPython.showAs("s");
	}
	public void showCartoon() {
		APIPython.showAs("c");
	}
	public void showHB() {
		APIPython.showAs("hb");
		APIPython.setHyperBallMetaphore("Smooth");
	}
	public void showVDW() {
		APIPython.showAs("hb");
		APIPython.setHyperBallMetaphore("VdW");
	}

	void LateUpdate() {
#if DISABLE_MAINUI
		return;
#endif
// 		if (structureNameToUIObject.Count != sm.loadedStructures.Count || !isLoadedStructureUIUpdated()) {
// 			try {
// 				updateLoadedMoleculesUI();
// 			}
// #if UNITY_EDITOR
// 			catch (System.Exception e) {
// 				Debug.LogError(e);
// 			}
// #else
// 			catch {}
// #endif
		// }

		GameObject curUIInput = EventSystem.current.currentSelectedGameObject;

		if (curUIInput == null && Input.GetButton("Hyperballs")) {
			showHB();
		}
		if (curUIInput == null && Input.GetButton("Cartoon")) {
			showCartoon();
		}
		if (curUIInput == null && Input.GetButton("Surface")) {
			showSurface();
		}
		if (curUIInput == null && Input.GetButton("VdW")) {
			showVDW();
		}
		if (curUIInput == null && Input.GetButton("Screenshot")) {
			saveScreenshot();
		}

		if(showUpdateRepVisiUI){
			updateRepVisiUI();
		}

		if ((selM.currentSelection == null && UIcurSelections != 0) ||
		        (selM.currentSelection != null && UIcurSelections != selM.currentSelection.Count) ||
		        selectionToUIObject.Count != selM.selections.Count || !isSelectionUIUpdated()) {
			try {
				updateSelectionUI();
			}
#if UNITY_EDITOR
			catch (System.Exception e) {
				Debug.LogError(e);
			}
#else
			catch {}
#endif
		}

		if (UIcurActiveRep != repM.activeRepresentations.Count) {
			try {
				updateRepresentationUI();
			}
#if UNITY_EDITOR
			catch (System.Exception e) {
				Debug.LogError(e);
			}
#else
			catch {}
#endif
		}

	}
}
}