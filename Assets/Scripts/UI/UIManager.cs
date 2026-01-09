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
using UnityEngine.UI;
using UnityEngine.UI.Extensions.ColorPicker;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;


using HTC.UnityPlugin.Pointer3D;
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

    public InputField inFRoom;
    public InputField inFUser;

	public Dictionary<string, GameObject> structureNameToUIObject = new Dictionary<string, GameObject>();
	public Dictionary<string, GameObject> selectionToUIObject = new Dictionary<string, GameObject>();
	public Dictionary<UnityMolAnnotation, GameObject> annoToUIObject = new Dictionary<UnityMolAnnotation, GameObject>();

	public Dictionary<string, List<GameObject>> graphUIPerStruc = new Dictionary<string, List<GameObject>>();

	private UnityMolStructureManager sm;
	private UnityMolSelectionManager selM;
	private UnityMolRepresentationManager repM;

	public Transform loadedMolUITransform;
	public Transform globalSelectionsUITransform;
	public Transform annotationUITransform;
	public Transform tourUITransform;

	public Text loadedMolLabel;

	public Transform selectionsUITransform;

	public GameObject lightDir;

	private bool curRTMode = false;

	SequenceViewerUI seqUI;
	public GameObject seqUIGo;

	//Number of selected atoms (used to know when to update UI elements)
	// private int UIcurSelectedAtoms = 0;
	//Number of selections in currentSelections (used to know when to update UI elements)
	private int UIcurSelections = 0;
	// //Number of active representations (used to know when to update UI elements)
	// private int UIcurActiveRep = 0;
	//Number of total selected atoms (used to know when to update UI elements)
	private int UItotalAtomSelected = 0;

	bool shouldUpdateRepVisiUI = true;
	bool shouldUpdateSelUI = true;
	bool shouldUpdateCountLabel = true;


	public float IPD = 0.063f;//IPD in mm
	public float offsetIPD = 0.024f;

	GameObject rightCam;

	GameObject scrollView;
	GameObject closeUIButton;
	GameObject showUIButton;

	void Awake() {

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
		if (annotationUITransform == null) {
			annotationUITransform = GameObject.Find(mainUIName + "/Selection Scroll View/Viewport/Content/Annotations").transform;
		}
		if (tourUITransform == null) {
            GameObject tour = GameObject.Find(mainUIName + "/Selection Scroll View/Viewport/Content/TourSelections");
            if (tour) {
                tourUITransform = GameObject.Find(mainUIName + "/Selection Scroll View/Viewport/Content/TourSelections")
                    .transform;
            }
        }


		scrollView = GameObject.Find(mainUIName + "/Selection Scroll View");
		closeUIButton = GameObject.Find(mainUIName + "/CloseUI");
		showUIButton = GameObject.Find(mainUIName + "/ShowUI");

		if (showUIButton != null)
			showUIButton.SetActive(false);

		if (lightDir == null) {
			lightDir = GameObject.Find("lightDir");
		}
		if (lightDir != null) {
			lightDir.SetActive(false);
		}

#else
		gameObject.SetActive(false);
#endif
	}

	void Start() {
#if !DISABLE_MAINUI
		UnityMolStructureManager.OnMoleculeLoaded += updateLoadedMoleculesUI;
		UnityMolStructureManager.OnMoleculeDeleted += updateLoadedMoleculesUI;
		UnityMolRepresentationManager.OnRepresentationVisibility += planUpdateRepVisiUI;
		UnityMolRepresentationManager.OnNewRepresentation += planUpdateRepVisiUI;
		UnityMolRepresentationManager.OnRepresentationDeleted += planUpdateRepVisiUI;
		UnityMolAnnotationManager.OnNewAnnotation += newAnnotation;
		UnityMolAnnotationManager.OnRemoveAnnotation += removeAnnotation;

		UnityMolSelectionManager.OnNewSelection += planUpdateSelectionUI;
		UnityMolSelectionManager.OnSelectionDeleted += planUpdateSelectionUI;
		UnityMolSelectionManager.OnSelectionModified += planUpdateCountLabel;

		RaytracedObject.onNewRTMaterial -= updateRTMat;

		UnityMolStructureManager.OnIMDConnected += initIMDGraphUI;
		UnityMolStructureManager.OnIMDDisconnected += destroyIMDGraphUI;

		ManipulationManager.OnTourModified += updateTourUI;

#endif
	}

	void OnDestroy() {
#if !DISABLE_MAINUI
		UnityMolStructureManager.OnMoleculeLoaded -= updateLoadedMoleculesUI;
		UnityMolStructureManager.OnMoleculeDeleted -= updateLoadedMoleculesUI;
		UnityMolRepresentationManager.OnRepresentationVisibility -= planUpdateRepVisiUI;
		UnityMolRepresentationManager.OnNewRepresentation -= planUpdateRepVisiUI;
		UnityMolRepresentationManager.OnRepresentationDeleted -= planUpdateRepVisiUI;
		UnityMolAnnotationManager.OnNewAnnotation -= newAnnotation;
		UnityMolAnnotationManager.OnRemoveAnnotation -= removeAnnotation;

		UnityMolSelectionManager.OnNewSelection -= planUpdateSelectionUI;
		UnityMolSelectionManager.OnSelectionDeleted -= planUpdateSelectionUI;
		UnityMolSelectionManager.OnSelectionModified -= planUpdateCountLabel;

		UnityMolStructureManager.OnIMDConnected -= initIMDGraphUI;
		UnityMolStructureManager.OnIMDDisconnected -= destroyIMDGraphUI;

		ManipulationManager.OnTourModified -= updateTourUI;

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
		rsfwb.SaveState();

		// APIPython.saveHistoryScript(path);
	}
	public void loadStateFromPythonScript(bool forceDesktop = false) {
		// string path = Path.Combine(Application.streamingAssetsPath , "UmolState.py");
		// APIPython.loadHistoryScript(path);
		ReadSaveFilesWithBrowser rsfwb = GetComponent<ReadSaveFilesWithBrowser>();
		if (rsfwb == null) {
			rsfwb = gameObject.AddComponent<ReadSaveFilesWithBrowser>();
		}
		rsfwb.ReadState(forceDesktop);
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
		rsfwb.ReadFiles(readHTM, forceDesktop);
	}


	public void wrapperFetch(Text t) {
		StartCoroutine(fetch(t.text));
		EventSystem.current.SetSelectedGameObject(null);
	}

	public IEnumerator fetch(string t) {
		yield return 0;

		if (string.IsNullOrEmpty(t)) {
            if (inFPDB == null) {
                inFPDB = GameObject.Find("InputFieldPDB/InputField").GetComponent<InputField>();
                if (inFPDB != null && string.IsNullOrEmpty(inFPDB.text))
                {
                    t = inFPDB.text;
                } else
                {
                    Debug.LogError("Empty InputField 'PDBID'.");
                    yield break;
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
		bool bioAssembly = false;


		try {
			readHTM = transform.Find("Selection Scroll View/Viewport/Content/FilesButtons/Toggles/Toggle heteroAtm/Toggle").GetComponent<Toggle>().isOn;
			bioAssembly = transform.Find("Selection Scroll View/Viewport/Content/FilesButtons/Toggles/Toggle bioassembly/Toggle").GetComponent<Toggle>().isOn;
		}
		catch {}

		try {
            if (!string.IsNullOrEmpty(t))
            {
			    APIPython.fetch(t, mmCIF, readHTM, bioAssembly: bioAssembly);
			}
		}
		catch (Exception e)
        {
			string errM = e.ToString();
			if (errM.Contains("404")) {
				Debug.LogError("Wrong PDB Id '" + t + "'");
			}
			else if (errM.Contains("ConnectFailure")) {
				Debug.LogError("No internet connection or blocked access to the PDB");
			}
			else {
				Debug.LogError("Could not fetch PDB file " + e);
			}
        }
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
			if (!structureNameToUIObject.ContainsKey(s.name)) {
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
			if (!structureNameToUIObject.ContainsKey(s.name)) {
				if (seqViewer) {
					seqUI.CreateSequenceViewer(s);
				}
				GameObject newB = createNewMolButton(s);
				structureNameToUIObject[s.name] = newB;
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

		try {
			LayoutRebuilder.ForceRebuildLayoutImmediate(loadedMolUITransform.GetComponent<RectTransform>());
			LayoutRebuilder.ForceRebuildLayoutImmediate(globalSelectionsUITransform.GetComponent<RectTransform>());
		}
		catch {

		}
	}
	private void newAnnotation(AnnoEventArgs arg) {
		createAnnotationUI(arg.anno);
	}

	private void removeAnnotation(AnnoEventArgs arg) {
		if (annoToUIObject.ContainsKey(arg.anno)) {
			Destroy(annoToUIObject[arg.anno]);
			annoToUIObject.Remove(arg.anno);
		}
	}

	//TODO improve this ! We just delete and recreate all buttons for now
	private void updateTourUI() {

		ManipulationManager mm = APIPython.getManipulationManager();

		int childs = tourUITransform.childCount;
		for (int i = childs - 1; i >= 0; i--) {
			GameObject.DestroyImmediate( tourUITransform.GetChild(i).gameObject );
		}


		int id = 0;
		foreach (string seln in mm.tourSelections) {
			//Create tour selection ui button
			GameObject newButtonGo = Instantiate((GameObject) Resources.Load("Prefabs/AnnotationButton"));//Save prefab
			newButtonGo.transform.SetParent(tourUITransform);
			newButtonGo.transform.localScale = Vector3.one;
			newButtonGo.transform.localPosition = Vector3.zero;
			newButtonGo.transform.localRotation = Quaternion.identity;
			newButtonGo.transform.Find("Main/MaskImage/Text").GetComponent<Text>().text = seln;
			newButtonGo.transform.name = "TourSel_" + id.ToString();

			newButtonGo.transform.Find("Main/Delete/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				int sid = int.Parse(newButtonGo.transform.name.Substring(8));
				mm.removeFromTour(sid);
			});

			GameObject showhideselTour = newButtonGo.transform.Find("Main/ShowHide").gameObject;
			showhideselTour.SetActive(false);
			id++;
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(loadedMolUITransform.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(globalSelectionsUITransform.GetComponent<RectTransform>());

	}

	private void planUpdateRepVisiUI() {
		shouldUpdateRepVisiUI = true;
	}

	private void updateRepVisiUI() {
		shouldUpdateRepVisiUI = false;
		RepType surfRepType = APIPython.getRepType("s");
		foreach (string selS in selectionToUIObject.Keys) {
			GameObject selUIgo = selectionToUIObject[selS];
			if (selM.selections.ContainsKey(selS)) {
				foreach (RepType rt in selM.selections[selS].representations.Keys) {
					GameObject toggleRepType = repTypeToGo(selUIgo, rt);
					if (toggleRepType == null) {
						setupRepresentationButtons(selUIgo, selM.selections[selS], APIPython.getTypeFromRepType(rt));
						continue;
					}
					toggleRepType.SetActive(true);
					bool active = selM.selections[selS].representations[rt][0].isEnabled;
					toggleRepType.transform.Find("OptionsLabel/ShowHideButton/Button Layer/Button Icon").GetComponent<Image>().enabled = active ;
					toggleRepType.transform.Find("OptionsLabel/ShowHideButton/Button Layer/Button Icon Hidden").GetComponent<Image>().enabled = !active;

					if (rt == surfRepType) { //Update surface type label
						Text labelSurfMethod = toggleRepType.transform.Find("Options/SurfaceComputeMethod/LabelSurfMethod/Text").gameObject.GetComponent<Text>();
						SurfMethod surfMet = getSurfMethodInSelection(selS);
						labelSurfMethod.text = surfMet.ToString();
					}

				}
			}
		}
		foreach (string struName in structureNameToUIObject.Keys) {
			if (APIPython.areRepresentationsOn(struName)) {
				structureNameToUIObject[struName].transform.Find("Text/ActionButtons/ShowHideButton/Button Layer/Button Icon").GetComponent<Image>().enabled = true ;
				structureNameToUIObject[struName].transform.Find("Text/ActionButtons/ShowHideButton/Button Layer/Button Icon Hidden").GetComponent<Image>().enabled = false;
			}
			else {
				structureNameToUIObject[struName].transform.Find("Text/ActionButtons/ShowHideButton/Button Layer/Button Icon").GetComponent<Image>().enabled = false;
				structureNameToUIObject[struName].transform.Find("Text/ActionButtons/ShowHideButton/Button Layer/Button Icon Hidden").GetComponent<Image>().enabled = true;
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(loadedMolUITransform.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(globalSelectionsUITransform.GetComponent<RectTransform>());
	}

	private GameObject createNewMolButton(UnityMolStructure s) {


		GameObject newButton = Instantiate((GameObject) Resources.Load("Prefabs/ButtonLoadedMol"));
		newButton.transform.SetParent(loadedMolUITransform, false);
		newButton.transform.Find("Text").GetComponent<Text>().text = s.FormatName(20);
		newButton.transform.name = "Button_" + s.name;
		newButton.transform.localScale = Vector3.one;
		// LoadedMolButtonFunctions butFunc = newButton.GetComponent<LoadedMolButtonFunctions>();
		// butFunc.structure = s;
		// butFunc.switchSelectedStructure();
		// newButton.GetComponent<Button>().onClick.AddListener(butFunc.switchSelectedStructure);

		newButton.transform.Find("Text/CollapseButton").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			bool first = true;
			bool shouldShow = true;

			foreach (string selS in selectionToUIObject.Keys) {
				if (selM.selections[selS].structures.Count == 1 && selM.selections[selS].structures.Contains(s)) {
					GameObject selUIgo = selectionToUIObject[selS];
					if (first) {
						shouldShow = !selUIgo.activeInHierarchy;
						first = false;
					}
					selUIgo.SetActive(shouldShow);
				}
			}
			if (shouldShow) {
				newButton.transform.Find("Text/CollapseButton/Expand").gameObject.GetComponent<Image>().enabled = false;
				newButton.transform.Find("Text/CollapseButton/Expanded").gameObject.GetComponent<Image>().enabled = true;
			}
			else {
				newButton.transform.Find("Text/CollapseButton/Expand").gameObject.GetComponent<Image>().enabled = true;
				newButton.transform.Find("Text/CollapseButton/Expanded").gameObject.GetComponent<Image>().enabled = false;
			}
		});

		newButton.transform.Find("Text/SelectButton").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (!selM.selections.ContainsKey(s.ToSelectionName())) {
				UnityMolSelection newAllSel = s.ToSelection();
				selM.Add(newAllSel);
			}
			APIPython.setCurrentSelection(s.ToSelectionName());

		});

		newButton.transform.Find("Text/ActionButtons/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			if (APIPython.areRepresentationsOn(s.name)) {
				APIPython.hideStructureAllRepresentations(s.name);
			}
			else {
				APIPython.showStructureAllRepresentations(s.name);
			}
		});

		newButton.transform.Find("Text/ActionButtons/DeleteButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.delete(s.name);
		});

		newButton.transform.Find("Text/ActionButtons/ShowHideHydrogen/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.addHydrogensReduce(s.name);
			// APIPython.addHydrogensHaad(s.name);
		});

		Text curGroupText = newButton.transform.Find("Group/Text").gameObject.GetComponent<Text>();

		newButton.transform.Find("Group/GroupMinus").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			int curGroup = APIPython.getStructureGroup(s.name);
			curGroup = Mathf.Max(0, curGroup - 1);
			APIPython.setStructureGroup(s.name, curGroup);
			curGroupText.text = "Group " + curGroup;
		});

		newButton.transform.Find("Group/GroupPlus").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			int curGroup = APIPython.getStructureGroup(s.name);
			curGroup = curGroup + 1;
			APIPython.setStructureGroup(s.name, curGroup);
			curGroupText.text = "Group " + curGroup;
		});

		if (s.models.Count > 1) {
			newButton.transform.Find("Model Menu").gameObject.SetActive(true);
		}


		return newButton;
	}

	void planUpdateSelectionUI() {
		shouldUpdateSelUI = true;
	}
	public void updateSelectionUI() {
		shouldUpdateSelUI = false;

		if (!isLoadedStructureUIUpdated()) { //Need to create molecule buttons before selections
			updateLoadedMoleculesUI();
			shouldUpdateSelUI = true;
			return;
		}

		try {
			foreach (string sname in selM.selections.Keys) {
				if (!selectionToUIObject.ContainsKey(sname)) {
					createSelectionToggle(selM.selections[sname]);
				}
			}
		}
		catch (System.Exception e) {
			Debug.LogError("Failed to create selection toggle : " + e);
		}

		//Remove obsolete selection buttons
		List<string> selNames = selectionToUIObject.Keys.ToList();
		foreach (string selS in selNames) {
			if (!selM.selections.ContainsKey(selS)) {
				GameObject.DestroyImmediate(selectionToUIObject[selS]);
				selectionToUIObject.Remove(selS);
			}
		}

		//Update representation rows
		foreach (string selS in selectionToUIObject.Keys) {
			GameObject selUIgo = selectionToUIObject[selS];
			if (selM.selections.ContainsKey(selS)) {
				foreach (RepType rt in selM.selections[selS].representations.Keys) {
					setupRepresentationButtons(selUIgo, selM.selections[selS], APIPython.getTypeFromRepType(rt));
				}
			}
		}

		updateCountLabel();
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
	private void planUpdateCountLabel() {
		shouldUpdateCountLabel = true;
	}
	private void updateCountLabel() {
		shouldUpdateCountLabel = false;
		//Update count labels
		foreach (string selS in selectionToUIObject.Keys) {
			int cpt = selM.selections[selS].Count;
			Text labelCountText = selectionToUIObject[selS].transform.Find("Main/LabelCount").GetComponent<Text>();

			//Update state of update with traj toggle
			Toggle updateToggle = selectionToUIObject[selS].transform.Find("EditSelection/ToggleUpdateWithTraj/Toggle").gameObject.GetComponent<Toggle>();
			updateToggle.SetValue(selM.selections[selS].updateContentWithTraj);

			int curLabelCount = int.Parse(labelCountText.text.Split(new char[] {'(', ')'}, StringSplitOptions.RemoveEmptyEntries)[0]);
			InputField selInF = selectionToUIObject[selS].transform.Find("EditSelection/InputFieldSelectionString").GetComponent<InputField>();
			string selCommand = selM.selections[selS].ToSelectionCommand();
			if (selInF.text != selCommand)
				selInF.SetValue(selCommand);//Change the text without calling events
			if (cpt != curLabelCount) {
				labelCountText.text = "(" + cpt + ")";
			}
			selectionToUIObject[selS].transform.Find("Main/Toggle").GetComponent<Toggle>().SetValue(false);
			if (selM.currentSelection != null && selM.currentSelection.name == selS) {
				selectionToUIObject[selS].transform.Find("Main/Toggle").GetComponent<Toggle>().SetValue(true);
			}
		}
	}

	private void createSelectionToggle(UnityMolSelection sel) {

		if (loadedMolUITransform == null) {
			loadedMolUITransform = GameObject.Find(mainUIName + "/Selection Scroll View/Viewport/Content/LoadedMoleculesUI").transform;
		}

		string nameStruct = APIPython.last().name;
		if (sel.structures.Count > 0)
			nameStruct = sel.structures[0].name;

		Transform molT = loadedMolUITransform.Find("Button_" + nameStruct);

		//This should not happen because the event OnMoleculeLoaded should trigger the function call before
		if (molT == null) {
			createNewMolButton(sel.structures[0]);
			return;
		}

		bool isGlobalSel = (sel.structures.Count > 1);

		GameObject newButtonGo = Instantiate((GameObject) Resources.Load("Prefabs/SelectionButtonDropdown"));
		if (isGlobalSel || sel.forceGlobalSelection) {
			newButtonGo.transform.SetParent(globalSelectionsUITransform);
		}
		else if (sel.structures.Count == 0) {//Should not happen
			Transform molPar = molT.parent;
			newButtonGo.transform.SetParent(molPar);
			insertButtonInHierarchy(newButtonGo, nameStruct);
			sel.structures.Add(APIPython.last());
		}
		else {
			Transform molPar = molT.parent;
			newButtonGo.transform.SetParent(molPar);
			insertButtonInHierarchy(newButtonGo, nameStruct);
		}
		newButtonGo.transform.Find("EditSelection/InputFieldName").GetComponent<InputField>().text = sel.name;
		newButtonGo.transform.Find("Main/MaskImage/Text").GetComponent<Text>().text = sel.name;
		newButtonGo.transform.Find("Main/LabelCount").GetComponent<Text>().text = "(" + sel.Count + ")";
		newButtonGo.transform.name = "Selection_" + sel.name;
		newButtonGo.transform.localScale = Vector3.one;
		newButtonGo.transform.localPosition = Vector3.zero;
		newButtonGo.transform.localRotation = Quaternion.identity;

		if (!mainUIName.Contains("VR")) {
			Transform tmpcont = newButtonGo.transform.Find("Main/AddRepDropdown/Template/Viewport/Content");
			Destroy(tmpcont.GetComponent<CanvasRaycastTarget>());
			Destroy(tmpcont.GetComponent<Canvas>());
		}

		selectionToUIObject[sel.name] = newButtonGo;

		setupRepresentationButtons(newButtonGo, sel);

		setupSelectionNamesButtons(newButtonGo, sel);
	}

	private void createAnnotationUI(UnityMolAnnotation a) {
		GameObject newButtonGo = Instantiate((GameObject) Resources.Load("Prefabs/AnnotationButton"));
		newButtonGo.transform.SetParent(annotationUITransform);
		newButtonGo.transform.localScale = Vector3.one;
		newButtonGo.transform.localPosition = Vector3.zero;
		newButtonGo.transform.localRotation = Quaternion.identity;
		newButtonGo.transform.Find("Main/MaskImage/Text").GetComponent<Text>().text = a.GetType().ToString();
		newButtonGo.transform.name = "Annotation_";

		newButtonGo.transform.Find("Main/Delete/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			//TODO: Should implement a better function in APIPython !
			UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();
			anM.RemoveAnnotation(a);
		});

		GameObject showhideanno = newButtonGo.transform.Find("Main/ShowHide/Button Layer").gameObject;
		showhideanno.GetComponent<Button>().onClick.AddListener(
		delegate {
			bool nowShown = !a.isShown;
			showhideanno.transform.Find("Button Icon").GetComponent<Image>().enabled = nowShown ;
			showhideanno.transform.Find("Button Icon Hidden").GetComponent<Image>().enabled = !nowShown;

			a.Show(nowShown);
		});

		annoToUIObject[a] = newButtonGo;

	}

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


	private void setupRepresentationButtons(GameObject newButtonGo, UnityMolSelection sel, string repT = "") {

		string selName = sel.name;

		if (repT == "") {
			Dropdown mainDropdownRep = newButtonGo.transform.Find("Main/AddRepDropdown").gameObject.GetComponent<Dropdown>();
			mainDropdownRep.onValueChanged.AddListener(
			delegate {

				try {
					switch (mainDropdownRep.options[mainDropdownRep.value].text) {
					case "Cartoon":
						APIPython.showSelection(selName, "c");
						repT = "c";
						break;
					case "Hyperball":
						APIPython.showSelection(selName, "hb");
						repT = "hb";
						break;
					case "Line":
						APIPython.showSelection(selName, "l");
						repT = "l";
						break;
					case "BondOrder":
						APIPython.showSelection(selName, "bondorder");
						repT = "bondorder";
						break;
					case "Surface":
						APIPython.showSelection(selName, "s");
						repT = "s";
						break;
					case "Surface (MSMS)":
						APIPython.showSelection(selName, "s", SurfMethod.MSMS);
						repT = "s";
						break;
					case "H-bond":
						APIPython.showSelection(selName, "hbond");
						repT = "hbond";
						break;
					case "H-bond tube":
						APIPython.showSelection(selName, "hbondtube");
						repT = "hbondtube";
						break;
					case "Fieldlines":
						APIPython.showSelection(selName, "fl");
						repT = "fl";
						break;
					case "Trace":
						APIPython.showSelection(selName, "trace");
						repT = "trace";
						break;
					case "SugarRibbons":
						APIPython.showSelection(selName, "sugarribbons");
						repT = "sugarribbons";
						break;
					case "Iso-surface":
						UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
						UnityMolSelection cursel = selM.selections[selName];
						UnityMolStructure s = cursel.structures[0];
						APIPython.showSelection(selName, "dxiso", s.name);
						repT = "dxiso";
						break;
					case "Sheherasade":
						APIPython.showSelection(selName, "sheherasade");
						repT = "sheherasade";
						break;
					case "Ellipsoid":
						APIPython.showSelection(selName, "ellipsoid");
						repT = "ellipsoid";
						break;
					case "Point":
						APIPython.showSelection(selName, "point");
						repT = "point";
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

				mainDropdownRep.value = 0;
			});
		}

		if (repT == "") {
			return;
		}
		RepType curRepType = APIPython.getRepType(repT);

		GameObject newRow = repTypeToGo(newButtonGo, curRepType);

		if (newRow != null) {
			newRow.gameObject.SetActive(true);
			//Force update UI layout
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
			newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

			Canvas.ForceUpdateCanvases();
			return;
		}
		Transform repOp = newButtonGo.transform.Find("RepOptions");
		string nameRow = "Row" + APIPython.getTypeFromRepType(curRepType).ToUpper();

		GameObject toInstan = (GameObject) Resources.Load("UI/RepresentationRows/" + nameRow);
		if (toInstan == null) {
			Debug.LogWarning("Cannot instantiate the UI for " + nameRow);
			return;
		}
		newRow = (GameObject) Instantiate(toInstan);
		newRow.transform.SetParent(repOp, false);
		newRow.name = nameRow;
		newRow.SetActive(true);


		switch (repT) {
		case "c":
		case "cartoon":
			//Cartoon ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {

				if (APIPython.areRepresentationsOn(selName, "c")) {
					APIPython.hideSelection(selName, "c");
				}
				else {
					APIPython.showSelection(selName, "c");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "c");
				APIPython.deleteRepresentationInSelection(selName, "c");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});


			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "c");
			});

			newRow.transform.Find("Options/ColorButtons2/ShadowsOn/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setShadows(selName, "c", true);
			});

			newRow.transform.Find("Options/ColorButtons2/ShadowsOff/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setShadows(selName, "c", false);
			});

			Dropdown colorByDD = newRow.transform.Find("Options/ColorButtons/DropdownColorby").gameObject.GetComponent<Dropdown>();

			if (!mainUIName.Contains("VR")) {
				Transform tmpcont = colorByDD.transform.Find("Template/Viewport/Content");
				Destroy(tmpcont.GetComponent<CanvasRaycastTarget>());
				Destroy(tmpcont.GetComponent<Canvas>());
			}

			colorByDD.onValueChanged.AddListener(delegate {
				if (colorByDD.value != 0) {
					string choice = colorByDD.options[colorByDD.value].text;

					switch (choice) {
					case "Atom":
						APIPython.colorByAtom(selName, "c");
						break;
					case "Residue":
						APIPython.colorByResidue(selName, "c");
						break;
					case "Chain":
						APIPython.colorByChain(selName, "c");
						break;
					// case "Model":
					// 	APIPython.colorByModel(selName, "c");
					// 	break;
					case "Hydrophobicity":
						APIPython.colorByHydrophobicity(selName, "c");
						break;
					case "ResId":
						APIPython.colorByResid(selName, "c");
						break;
					case "ResNum":
						APIPython.colorByResnum(selName, "c");
						break;
					case "Sequence":
						APIPython.colorBySequence(selName, "c");
						break;
					// case "Charge":
					// 	APIPython.colorByCharge(selName, "c");
					// 	break;
					case "ResType":
						APIPython.colorByResidueType(selName, "c");
						break;
					case "ResCharge":
						APIPython.colorByResidueCharge(selName, "c");
						break;
					case "BFactor":
						APIPython.colorByBfactor(selName, "c");
						break;
					}
				}
			});

			ColorPickerControl cartoonColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			cartoonColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "c", cartoonColorPicker.CurrentColor);
			});

			Slider cartoonSliderSmooth = newRow.transform.Find("Options/SliderSmooth/Slider").gameObject.GetComponent<Slider>();
			cartoonSliderSmooth.onValueChanged.AddListener(
			delegate {
				APIPython.setSmoothness(selName, "c", cartoonSliderSmooth.value);
			});

			Slider cartoonSliderMetal = newRow.transform.Find("Options/SliderMetal/Slider").gameObject.GetComponent<Slider>();
			cartoonSliderMetal.onValueChanged.AddListener(
			delegate {
				APIPython.setMetal(selName, "c", cartoonSliderMetal.value);
			});


			ColorPickerControl cartoonHelixColorPicker = newRow.transform.Find("Options/ColorPickerHelix").gameObject.GetComponent<ColorPickerControl>();

			cartoonHelixColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.setCartoonColorSS(selName, "helix" , cartoonHelixColorPicker.CurrentColor);
			});


			ColorPickerControl cartoonSheetColorPicker = newRow.transform.Find("Options/ColorPickerSheet").gameObject.GetComponent<ColorPickerControl>();

			cartoonSheetColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.setCartoonColorSS(selName, "sheet" , cartoonSheetColorPicker.CurrentColor);
			});



			ColorPickerControl cartoonCoilColorPicker = newRow.transform.Find("Options/ColorPickerCoil").gameObject.GetComponent<ColorPickerControl>();

			cartoonCoilColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.setCartoonColorSS(selName, "coil" , cartoonCoilColorPicker.CurrentColor);
			});


			newRow.transform.Find("Options/ColorButtons3/Solid/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setSolidCartoon(selName);
			});

			newRow.transform.Find("Options/ColorButtons3/Transparent/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setTransparentCartoon(selName);
			});

			Slider cartoonSliderAlpha = newRow.transform.Find("Options/SliderAlpha/Slider").gameObject.GetComponent<Slider>();
			cartoonSliderAlpha.onValueChanged.AddListener(
			delegate {
				APIPython.setTransparentCartoon(selName, cartoonSliderAlpha.value);
			});

			Slider cartoonSliderTS = newRow.transform.Find("Options/SliderTubeSize/Slider").gameObject.GetComponent<Slider>();
			cartoonSliderTS.onValueChanged.AddListener(
			delegate {
				APIPython.setTubeSizeCartoon(selName, cartoonSliderTS.value);
			});

			Dropdown RTMatdropDown = newRow.transform.Find("Options/RaytracingMaterials/Dropdown").gameObject.GetComponent<Dropdown>();

			if (UnityMolMain.raytracingMode) {

				List<string> opts = new List<string>(RaytracingMaterial.materialsBank.Count + 1);
				RTMatdropDown.ClearOptions();

				opts.Add("Raytracing Materials");
				foreach (string k in RaytracingMaterial.materialsBank.Keys) {
					opts.Add(k);
				}

				RTMatdropDown.AddOptions(opts);
			}

			RTMatdropDown.onValueChanged.AddListener(
			delegate {
				string matName = RTMatdropDown.options[RTMatdropDown.value].text;
				if (RaytracingMaterial.materialsBank.ContainsKey(matName)) {
					APIPython.setRTMaterial(selName, "c", matName);
				}
			}
			);

			RTMatdropDown.interactable = UnityMolMain.raytracingMode;

			break;

		case "hb":
		case "hyperball":
		case "hyperballs":

			//Hyperball ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.areRepresentationsOn(selName, "hb")) {
					APIPython.hideSelection(selName, "hb");
				}
				else {
					APIPython.showSelection(selName, "hb");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "hb");
				APIPython.deleteRepresentationInSelection(selName, "hb");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});


			newRow.transform.Find("Options/HBMetaphoreButtons/Smooth/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setHyperBallMetaphore(selName, "Smooth");
			});
			newRow.transform.Find("Options/HBMetaphoreButtons/Balls&Sticks/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setHyperBallMetaphore(selName, "Balls&Sticks");
			});
			newRow.transform.Find("Options/HBMetaphoreButtons/Licorice/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setHyperBallMetaphore(selName, "Licorice");
			});
			newRow.transform.Find("Options/HBMetaphoreButtons/VdW/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setHyperBallMetaphore(selName, "VdW");
			});



			Dropdown colorByDDhb = newRow.transform.Find("Options/ColorButtons/DropdownColorby").gameObject.GetComponent<Dropdown>();

			if (!mainUIName.Contains("VR")) {
				Transform tmpcont = colorByDDhb.transform.Find("Template/Viewport/Content");
				Destroy(tmpcont.GetComponent<CanvasRaycastTarget>());
				Destroy(tmpcont.GetComponent<Canvas>());
			}

			colorByDDhb.onValueChanged.AddListener(delegate {
				if (colorByDDhb.value != 0) {
					string choice = colorByDDhb.options[colorByDDhb.value].text;

					switch (choice) {
					case "Atom":
						APIPython.colorByAtom(selName, "hb");
						break;
					case "Residue":
						APIPython.colorByResidue(selName, "hb");
						break;
					case "Chain":
						APIPython.colorByChain(selName, "hb");
						break;
					// case "Model":
					// 	APIPython.colorByModel(selName, "hb");
					// 	break;
					case "Hydrophobicity":
						APIPython.colorByHydrophobicity(selName, "hb");
						break;
					case "ResId":
						APIPython.colorByResid(selName, "hb");
						break;
					case "ResNum":
						APIPython.colorByResnum(selName, "hb");
						break;
					case "Sequence":
						APIPython.colorBySequence(selName, "hb");
						break;
					// case "Charge":
					// 	APIPython.colorByCharge(selName, "hb");
					// 	break;
					case "ResType":
						APIPython.colorByResidueType(selName, "hb");
						break;
					case "ResCharge":
						APIPython.colorByResidueCharge(selName, "hb");
						break;
					case "BFactor":
						APIPython.colorByBfactor(selName, "hb");
						break;
					}
				}
			});

			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "hb");
			});

			newRow.transform.Find("Options/ColorButtons2/doAO/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setHyperBallMetaphore(selName, "vdw", false);
			});

			newRow.transform.Find("Options/ColorButtons2/RemoveAO/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.clearHyperballAO(selName);
			});
			newRow.transform.Find("Options/ColorButtons2/ShadowsOn/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setShadows(selName, "hb", true);
			});
			newRow.transform.Find("Options/ColorButtons2/ShadowsOff/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setShadows(selName, "hb", false);
			});

			ColorPickerControl hbColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			hbColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "hb", hbColorPicker.CurrentColor);
			});


			Slider sliderShrink = newRow.transform.Find("Options/SliderShrink/Slider").gameObject.GetComponent<Slider>();
			sliderShrink.onValueChanged.AddListener(
			delegate {
				APIPython.setHyperballShrink(selName, sliderShrink.value);
			});

			Slider sliderScale = newRow.transform.Find("Options/SliderScale/Slider").gameObject.GetComponent<Slider>();
			sliderScale.onValueChanged.AddListener(
			delegate {
				APIPython.setRepSize(selName, "hb", sliderScale.value);
			});

			Slider sliderShine = newRow.transform.Find("Options/SliderShininess/Slider").gameObject.GetComponent<Slider>();
			sliderShine.onValueChanged.AddListener(
			delegate {
				APIPython.setHyperBallShininess(selName, sliderShine.value);
			});

			createTextureMenu(newRow.transform.Find("Options/Texture Scroll View/Viewport/Content"), selName);

			Dropdown RTMatHBdropDown = newRow.transform.Find("Options/RaytracingMaterials/Dropdown").gameObject.GetComponent<Dropdown>();

			if (UnityMolMain.raytracingMode) {

				List<string> opts = new List<string>(RaytracingMaterial.materialsBank.Count + 1);
				RTMatHBdropDown.ClearOptions();

				opts.Add("Raytracing Materials");
				foreach (string k in RaytracingMaterial.materialsBank.Keys) {
					opts.Add(k);
				}

				RTMatHBdropDown.AddOptions(opts);
			}

			RTMatHBdropDown.onValueChanged.AddListener(
			delegate {
				string matName = RTMatHBdropDown.options[RTMatHBdropDown.value].text;
				if (RaytracingMaterial.materialsBank.ContainsKey(matName)) {
					APIPython.setRTMaterial(selName, "hb", matName);
				}
			}
			);

			break;

		case "l":
		case "lines":
		case "line":
			//Lines ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.areRepresentationsOn(selName, "l")) {
					APIPython.hideSelection(selName, "l");
				}
				else {
					APIPython.showSelection(selName, "l");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "l");
				APIPython.deleteRepresentationInSelection(selName, "l");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});


			Slider lineSliderSize = newRow.transform.Find("Options/SliderSize/SliderWidth").gameObject.GetComponent<Slider>();
			lineSliderSize.onValueChanged.AddListener(
			delegate {
				APIPython.setLineSize(selName, lineSliderSize.value);
			});


			Dropdown colorByDDLine = newRow.transform.Find("Options/ColorButtons/DropdownColorby").gameObject.GetComponent<Dropdown>();

			if (!mainUIName.Contains("VR")) {
				Transform tmpcont = colorByDDLine.transform.Find("Template/Viewport/Content");
				Destroy(tmpcont.GetComponent<CanvasRaycastTarget>());
				Destroy(tmpcont.GetComponent<Canvas>());
			}

			colorByDDLine.onValueChanged.AddListener(delegate {
				if (colorByDDLine.value != 0) {
					string choice = colorByDDLine.options[colorByDDLine.value].text;

					switch (choice) {
					case "Atom":
						APIPython.colorByAtom(selName, "l");
						break;
					case "Residue":
						APIPython.colorByResidue(selName, "l");
						break;
					case "Chain":
						APIPython.colorByChain(selName, "l");
						break;
					// case "Model":
					// 	APIPython.colorByModel(selName, "l");
					// 	break;
					case "Hydrophobicity":
						APIPython.colorByHydrophobicity(selName, "l");
						break;
					case "ResId":
						APIPython.colorByResid(selName, "l");
						break;
					case "ResNum":
						APIPython.colorByResnum(selName, "l");
						break;
					case "Sequence":
						APIPython.colorBySequence(selName, "l");
						break;

					case "ResType":
						APIPython.colorByResidueType(selName, "l");
						break;
					case "ResCharge":
						APIPython.colorByResidueCharge(selName, "l");
						break;
					case "BFactor":
						APIPython.colorByBfactor(selName, "l");
						break;
					}
				}
			});



			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "l");
			});


			ColorPickerControl lineColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			lineColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "l", lineColorPicker.CurrentColor);
			});


			Dropdown RTMatLdropDown = newRow.transform.Find("Options/RaytracingMaterials/Dropdown").gameObject.GetComponent<Dropdown>();

			if (UnityMolMain.raytracingMode) {

				List<string> opts = new List<string>(RaytracingMaterial.materialsBank.Count + 1);
				RTMatLdropDown.ClearOptions();

				opts.Add("Raytracing Materials");
				foreach (string k in RaytracingMaterial.materialsBank.Keys) {
					opts.Add(k);
				}

				RTMatLdropDown.AddOptions(opts);
			}

			RTMatLdropDown.onValueChanged.AddListener(
			delegate {
				string matName = RTMatLdropDown.options[RTMatLdropDown.value].text;
				if (RaytracingMaterial.materialsBank.ContainsKey(matName)) {
					APIPython.setRTMaterial(selName, "l", matName);
				}
			}
			);


			break;

		case "bondorder":
		case "bondorders":
			//Bond Order ------------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.areRepresentationsOn(selName, "bondorder")) {
					APIPython.hideSelection(selName, "bondorder");
				}
				else {
					APIPython.showSelection(selName, "bondorder");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "bondorder");
				APIPython.deleteRepresentationInSelection(selName, "bondorder");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});




			Dropdown colorByDDBO = newRow.transform.Find("Options/ColorButtons/DropdownColorby").gameObject.GetComponent<Dropdown>();

			if (!mainUIName.Contains("VR")) {
				Transform tmpcont = colorByDDBO.transform.Find("Template/Viewport/Content");
				Destroy(tmpcont.GetComponent<CanvasRaycastTarget>());
				Destroy(tmpcont.GetComponent<Canvas>());
			}

			colorByDDBO.onValueChanged.AddListener(delegate {
				if (colorByDDBO.value != 0) {
					string choice = colorByDDBO.options[colorByDDBO.value].text;

					switch (choice) {
					case "Atom":
						APIPython.colorByAtom(selName, "bondorder");
						break;
					case "Residue":
						APIPython.colorByResidue(selName, "bondorder");
						break;
					case "Chain":
						APIPython.colorByChain(selName, "bondorder");
						break;
					// case "Model":
					// 	APIPython.colorByModel(selName, "bondorder");
					// 	break;
					case "Hydrophobicity":
						APIPython.colorByHydrophobicity(selName, "bondorder");
						break;
					case "ResId":
						APIPython.colorByResid(selName, "bondorder");
						break;
					case "ResNum":
						APIPython.colorByResnum(selName, "bondorder");
						break;
					case "Sequence":
						APIPython.colorBySequence(selName, "bondorder");
						break;

					case "ResType":
						APIPython.colorByResidueType(selName, "bondorder");
						break;
					case "ResCharge":
						APIPython.colorByResidueCharge(selName, "bondorder");
						break;
					case "BFactor":
						APIPython.colorByBfactor(selName, "bondorder");
						break;
					}
				}
			});


			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "bondorder");
			});

			newRow.transform.Find("Options/ColorButtons2/ShadowsOn/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setShadows(selName, "bondorder", true);
			});
			newRow.transform.Find("Options/ColorButtons2/ShadowsOff/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setShadows(selName, "bondorder", false);
			});


			ColorPickerControl hbColorPickerBO = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			hbColorPickerBO.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "bondorder", hbColorPickerBO.CurrentColor);
			});


			// Slider sliderShrink = newRow.transform.Find("Options/SliderShrink/Slider").gameObject.GetComponent<Slider>();
			// sliderShrink.onValueChanged.AddListener(
			// delegate {
			// 	APIPython.setHyperballShrink(selName, sliderShrink.value);
			// });

			// Slider sliderScale = newRow.transform.Find("Options/SliderScale/Slider").gameObject.GetComponent<Slider>();
			// sliderScale.onValueChanged.AddListener(
			// delegate {
			// 	APIPython.setRepSize(selName, "bondorder", sliderScale.value);
			// });


			createTextureMenu(newRow.transform.Find("Options/Texture Scroll View/Viewport/Content"), selName, true);
			break;

		case "s":
		case "surface":
			//Surface ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.areRepresentationsOn(selName, "s")) {
					APIPython.hideSelection(selName, "s");
				}
				else {
					APIPython.showSelection(selName, "s");
				}
			});



			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "s");
				APIPython.deleteRepresentationInSelection(selName, "s");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});

			Text labelSurfMethod = newRow.transform.Find("Options/SurfaceComputeMethod/LabelSurfMethod/Text").gameObject.GetComponent<Text>();
			SurfMethod surfMet = getSurfMethodInSelection(selName);
			labelSurfMethod.text = surfMet.ToString();

			newRow.transform.Find("Options/SurfaceComputeMethod/SwitchSurfMethod/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.switchSurfaceComputeMethod(selName);
				SurfMethod surfMethod = getSurfMethodInSelection(selName);
				labelSurfMethod.text = surfMethod.ToString();
			});

			Toggle cutSurf = newRow.transform.Find("Options/ToggleIsCut/Toggle").gameObject.GetComponent<Toggle>();
			cutSurf.onValueChanged.AddListener(
			delegate {
				APIPython.switchCutSurface(selName, cutSurf.isOn);
			});


			newRow.transform.Find("Options/SurfMatButtons/Normal/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setSolidSurface(selName);
			});
			newRow.transform.Find("Options/SurfMatButtons/Wireframe/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setWireframeSurface(selName);
			});
			newRow.transform.Find("Options/SurfMatButtons/Transparent/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setTransparentSurface(selName);
			});


			Dropdown colorByDDSurf = newRow.transform.Find("Options/ColorButtons/DropdownColorby").gameObject.GetComponent<Dropdown>();

			if (!mainUIName.Contains("VR")) {
				Transform tmpcont = colorByDDSurf.transform.Find("Template/Viewport/Content");
				Destroy(tmpcont.GetComponent<CanvasRaycastTarget>());
				Destroy(tmpcont.GetComponent<Canvas>());
			}

			colorByDDSurf.onValueChanged.AddListener(delegate {
				if (colorByDDSurf.value != 0) {
					string choice = colorByDDSurf.options[colorByDDSurf.value].text;

					switch (choice) {
					case "Atom":
						APIPython.colorByAtom(selName, "s");
						break;
					case "Residue":
						APIPython.colorByResidue(selName, "s");
						break;
					case "Chain":
						APIPython.colorByChain(selName, "s");
						break;
					// case "Model":
					// 	APIPython.colorByModel(selName, "s");
					// 	break;
					case "Hydrophobicity":
						APIPython.colorByHydrophobicity(selName, "s");
						break;
					case "ResId":
						APIPython.colorByResid(selName, "s");
						break;
					case "ResNum":
						APIPython.colorByResnum(selName, "s");
						break;
					case "Sequence":
						APIPython.colorBySequence(selName, "s");
						break;

					case "ResType":
						APIPython.colorByResidueType(selName, "s");
						break;
					case "ResCharge":
						APIPython.colorByResidueCharge(selName, "s");
						break;
					case "BFactor":
						APIPython.colorByBfactor(selName, "s");
						break;
					case "Charge":
						APIPython.colorByCharge(selName);
						break;
					}
				}
			});


			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "s");
			});
			newRow.transform.Find("Options/ColorButtons3/ShadowsOn/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setShadows(selName, "s", true);
			});

			newRow.transform.Find("Options/ColorButtons3/ShadowsOff/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setShadows(selName, "s", false);
			});

			newRow.transform.Find("Options/ColorButtons3/SwitchAO/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.isSurfaceAOOn(selName)) {
					APIPython.clearSurfaceAO(selName);
				}
				else {
					APIPython.computeSurfaceAO(selName);
				}
			});

			ColorPickerControl surfColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			surfColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "s", surfColorPicker.CurrentColor);
			});

			Slider surfSliderSmooth = newRow.transform.Find("Options/SliderSmooth/Slider").gameObject.GetComponent<Slider>();
			surfSliderSmooth.onValueChanged.AddListener(
			delegate {
				APIPython.setSmoothness(selName, "s", surfSliderSmooth.value);
			});

			Slider surfSliderMetal = newRow.transform.Find("Options/SliderMetal/Slider").gameObject.GetComponent<Slider>();
			surfSliderMetal.onValueChanged.AddListener(
			delegate {
				APIPython.setMetal(selName, "s", surfSliderMetal.value);
			});

			Slider surfSliderAlpha = newRow.transform.Find("Options/SliderAlpha/Slider").gameObject.GetComponent<Slider>();
			surfSliderAlpha.onValueChanged.AddListener(
			delegate {
				APIPython.setTransparentSurface(selName, surfSliderAlpha.value);
			});
			Slider surfSliderWire = newRow.transform.Find("Options/SliderWire/Slider").gameObject.GetComponent<Slider>();
			surfSliderWire.onValueChanged.AddListener(
			delegate {
				APIPython.setSurfaceWireframe(selName, "s", surfSliderWire.value);
			});

			Dropdown RTMatSdropDown = newRow.transform.Find("Options/RaytracingMaterials/Dropdown").gameObject.GetComponent<Dropdown>();

			if (UnityMolMain.raytracingMode) {

				List<string> opts = new List<string>(RaytracingMaterial.materialsBank.Count + 1);
				RTMatSdropDown.ClearOptions();

				opts.Add("Raytracing Materials");
				foreach (string k in RaytracingMaterial.materialsBank.Keys) {
					opts.Add(k);
				}

				RTMatSdropDown.AddOptions(opts);
			}

			RTMatSdropDown.onValueChanged.AddListener(
			delegate {
				string matName = RTMatSdropDown.options[RTMatSdropDown.value].text;
				if (RaytracingMaterial.materialsBank.ContainsKey(matName)) {
					APIPython.setRTMaterial(selName, "s", matName);
				}
			}
			);

			break;

		case "hbond":
		case "hbonds":
			//Hbond ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.areRepresentationsOn(selName, "hbond")) {
					APIPython.hideSelection(selName, "hbond");
				}
				else {
					APIPython.showSelection(selName, "hbond");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "hbond");
				APIPython.deleteRepresentationInSelection(selName, "hbond");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});

			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "hbond");
			});


			newRow.transform.Find("Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.colorByChain(selName, "hbond");
			});


			ColorPickerControl hbondColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			hbondColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "hbond", hbondColorPicker.CurrentColor);
			});
			break;

		case "hbondtube":
		case "hbondtubes":
			//Hbondtube ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.areRepresentationsOn(selName, "hbondtube")) {
					APIPython.hideSelection(selName, "hbondtube");
				}
				else {
					APIPython.showSelection(selName, "hbondtube");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "hbondtube");
				APIPython.deleteRepresentationInSelection(selName, "hbondtube");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});

			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "hbondtube");
			});


			newRow.transform.Find("Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.colorByChain(selName, "hbondtube");
			});


			ColorPickerControl hbondtubeColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			hbondtubeColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "hbondtube", hbondtubeColorPicker.CurrentColor);
			});

			Dropdown RTMathbtdropDown = newRow.transform.Find("Options/RaytracingMaterials/Dropdown").gameObject.GetComponent<Dropdown>();

			if (UnityMolMain.raytracingMode) {

				List<string> opts = new List<string>(RaytracingMaterial.materialsBank.Count + 1);
				RTMathbtdropDown.ClearOptions();

				opts.Add("Raytracing Materials");
				foreach (string k in RaytracingMaterial.materialsBank.Keys) {
					opts.Add(k);
				}

				RTMathbtdropDown.AddOptions(opts);
			}

			RTMathbtdropDown.onValueChanged.AddListener(
			delegate {
				string matName = RTMathbtdropDown.options[RTMathbtdropDown.value].text;
				if (RaytracingMaterial.materialsBank.ContainsKey(matName)) {
					APIPython.setRTMaterial(selName, "hbondtube", matName);
				}
			}
			);

			break;

		case "fl":
		case "fieldlines":
		case "fieldline":
			//Fieldlines ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.areRepresentationsOn(selName, "fl")) {
					APIPython.hideSelection(selName, "fl");
				}
				else {
					APIPython.showSelection(selName, "fl");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "hbond");
				APIPython.deleteRepresentationInSelection(selName, "fl");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});

			//Slider
			Text flSliderText = newRow.transform.Find("Options/SliderFL/MagnLabel/Text").gameObject.GetComponent<Text>();
			Slider flSliderMagn = newRow.transform.Find("Options/SliderFL/Slider").gameObject.GetComponent<Slider>();
			SliderDrag flSliderMagnDrag = flSliderMagn.gameObject.GetComponent<SliderDrag>();

			// flSliderMagn.onValueChanged.AddListener(
			// delegate {
			// 	flSliderText.text = "Gradient magnitude : " + flSliderMagn.value.ToString("F1");
			// });

			flSliderMagnDrag.EndDrag +=  delegate {
				APIPython.setFieldlineGradientThreshold(selName, flSliderMagn.value);
			};

			Text flSliderSizeText = newRow.transform.Find("Options/SliderSizeFL/SizeLabel/Text").gameObject.GetComponent<Text>();
			Slider flSliderSize = newRow.transform.Find("Options/SliderSizeFL/Slider").gameObject.GetComponent<Slider>();
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

			Text flSliderLengthText = newRow.transform.Find("Options/SliderLengthFL/LengthLabel/Text").gameObject.GetComponent<Text>();
			Slider flSliderLength = newRow.transform.Find("Options/SliderLengthFL/Slider").gameObject.GetComponent<Slider>();

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


			Text flSliderSpeedText = newRow.transform.Find("Options/SliderSpeedFL/SpeedLabel/Text").gameObject.GetComponent<Text>();
			Slider flSliderSpeed = newRow.transform.Find("Options/SliderSpeedFL/Slider").gameObject.GetComponent<Slider>();

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


			ColorPickerControl startFLColorPicker = newRow.transform.Find("Options/ColorPickerStart").gameObject.GetComponent<ColorPickerControl>();
			ColorPickerControl endFLColorPicker = newRow.transform.Find("Options/ColorPickerEnd").gameObject.GetComponent<ColorPickerControl>();

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
			break;

		case "trace":
			//Trace ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.areRepresentationsOn(selName, "trace")) {
					APIPython.hideSelection(selName, "trace");
				}
				else {
					APIPython.showSelection(selName, "trace");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.deleteRepresentationInSelection(selName, "trace");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});


			Slider traceSize = newRow.transform.Find("Options/SliderSize/SliderWidth").gameObject.GetComponent<Slider>();
			traceSize.onValueChanged.AddListener(
			delegate {
				APIPython.setTraceSize(selName, traceSize.value);
			});


			Dropdown colorByDDTrace = newRow.transform.Find("Options/ColorButtons/DropdownColorby").gameObject.GetComponent<Dropdown>();

			if (!mainUIName.Contains("VR")) {
				Transform tmpcont = colorByDDTrace.transform.Find("Template/Viewport/Content");
				Destroy(tmpcont.GetComponent<CanvasRaycastTarget>());
				Destroy(tmpcont.GetComponent<Canvas>());
			}

			colorByDDTrace.onValueChanged.AddListener(delegate {
				if (colorByDDTrace.value != 0) {
					string choice = colorByDDTrace.options[colorByDDTrace.value].text;

					switch (choice) {
					case "Atom":
						APIPython.colorByAtom(selName, "trace");
						break;
					case "Residue":
						APIPython.colorByResidue(selName, "trace");
						break;
					case "Chain":
						APIPython.colorByChain(selName, "trace");
						break;
					// case "Model":
					// 	APIPython.colorByModel(selName, "trace");
					// 	break;
					case "Hydrophobicity":
						APIPython.colorByHydrophobicity(selName, "trace");
						break;
					case "ResId":
						APIPython.colorByResid(selName, "trace");
						break;
					case "ResNum":
						APIPython.colorByResnum(selName, "trace");
						break;
					case "Sequence":
						APIPython.colorBySequence(selName, "trace");
						break;

					case "ResType":
						APIPython.colorByResidueType(selName, "trace");
						break;
					case "ResCharge":
						APIPython.colorByResidueCharge(selName, "trace");
						break;
					case "BFactor":
						APIPython.colorByBfactor(selName, "trace");
						break;
					}
				}
			});


			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "trace");
			});



			ColorPickerControl traceColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			traceColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "trace", traceColorPicker.CurrentColor);
			});

			Dropdown RTMattracedropDown = newRow.transform.Find("Options/RaytracingMaterials/Dropdown").gameObject.GetComponent<Dropdown>();

			if (UnityMolMain.raytracingMode) {

				List<string> opts = new List<string>(RaytracingMaterial.materialsBank.Count + 1);
				RTMattracedropDown.ClearOptions();

				opts.Add("Raytracing Materials");
				foreach (string k in RaytracingMaterial.materialsBank.Keys) {
					opts.Add(k);
				}

				RTMattracedropDown.AddOptions(opts);
			}

			RTMattracedropDown.onValueChanged.AddListener(
			delegate {
				string matName = RTMattracedropDown.options[RTMattracedropDown.value].text;
				if (RaytracingMaterial.materialsBank.ContainsKey(matName)) {
					APIPython.setRTMaterial(selName, "trace", matName);
				}
			}
			);

			break;

		case "sugarribbons":
			//SugarRibbons ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.areRepresentationsOn(selName, "sugarribbons")) {
					APIPython.hideSelection(selName, "sugarribbons");
				}
				else {
					APIPython.showSelection(selName, "sugarribbons");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.deleteRepresentationInSelection(selName, "sugarribbons");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});


			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "sugarribbons");
			});

			newRow.transform.Find("Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.colorByChain(selName, "sugarribbons");
			});
			newRow.transform.Find("Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.colorByResidue(selName, "sugarribbons");
			});

			ColorPickerControl sugarColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			sugarColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "sugarribbons", sugarColorPicker.CurrentColor);
			});
			break;

		case "dxiso":
			//DXSurface ------------------------------------
			Slider dxSliderIso = newRow.transform.Find("Options/SliderIso/Slider").gameObject.GetComponent<Slider>();
			SliderDrag dxSliderIsoDrag = dxSliderIso.gameObject.GetComponent<SliderDrag>();

			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
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



			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "dxiso");
				APIPython.deleteRepresentationInSelection(selName, "dxiso");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});

			newRow.transform.Find("Options/LimitedViewButtons/ActivateLimitedView/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.getLimitedView(selName, "dxiso")) {
					APIPython.disableLimitedView(selName, "dxiso");
				}
				else {
					APIPython.enableLimitedView(selName, "dxiso");
				}
			});

			Slider dxSliderLimitedVRad = newRow.transform.Find("Options/LimitedViewButtons/SliderLimitedRad/Slider").gameObject.GetComponent<Slider>();

			dxSliderLimitedVRad.onValueChanged.AddListener(
			delegate {
				APIPython.setLimitedViewRadius(selName, "dxiso", dxSliderLimitedVRad.value);
			}
			);

			Slider dxSliderLimitedVPosX = newRow.transform.Find("Options/SliderLimitedPosX/Slider").gameObject.GetComponent<Slider>();

			dxSliderLimitedVPosX.onValueChanged.AddListener(
			delegate {
				Vector3 curCenter = APIPython.getLimitedViewCenter(selName, "dxiso");
				curCenter.x = dxSliderLimitedVPosX.value;
				APIPython.setLimitedViewCenter(selName, "dxiso", curCenter);
			}
			);

			Slider dxSliderLimitedVPosY = newRow.transform.Find("Options/SliderLimitedPosY/Slider").gameObject.GetComponent<Slider>();

			dxSliderLimitedVPosY.onValueChanged.AddListener(
			delegate {
				Vector3 curCenter = APIPython.getLimitedViewCenter(selName, "dxiso");
				curCenter.y = dxSliderLimitedVPosY.value;
				APIPython.setLimitedViewCenter(selName, "dxiso", curCenter);
			}
			);

			Slider dxSliderLimitedVPosZ = newRow.transform.Find("Options/SliderLimitedPosZ/Slider").gameObject.GetComponent<Slider>();

			dxSliderLimitedVPosZ.onValueChanged.AddListener(
			delegate {
				Vector3 curCenter = APIPython.getLimitedViewCenter(selName, "dxiso");
				curCenter.z = dxSliderLimitedVPosZ.value;
				APIPython.setLimitedViewCenter(selName, "dxiso", curCenter);
			}
			);

			newRow.transform.Find("Options/SurfMatButtons/Normal/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setSolidSurface(selName);
			});
			newRow.transform.Find("Options/SurfMatButtons/Wireframe/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setWireframeSurface(selName);
			});
			newRow.transform.Find("Options/SurfMatButtons/Transparent/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setTransparentSurface(selName);
			});


			Dropdown colorByDDdiso = newRow.transform.Find("Options/ColorButtons/DropdownColorby").gameObject.GetComponent<Dropdown>();

			if (!mainUIName.Contains("VR")) {
				Transform tmpcont = colorByDDdiso.transform.Find("Template/Viewport/Content");
				Destroy(tmpcont.GetComponent<CanvasRaycastTarget>());
				Destroy(tmpcont.GetComponent<Canvas>());
			}

			colorByDDdiso.onValueChanged.AddListener(delegate {
				if (colorByDDdiso.value != 0) {
					string choice = colorByDDdiso.options[colorByDDdiso.value].text;

					switch (choice) {
					case "Atom":
						APIPython.colorByAtom(selName, "dxiso");
						break;
					case "Residue":
						APIPython.colorByResidue(selName, "dxiso");
						break;
					case "Chain":
						APIPython.colorByChain(selName, "dxiso");
						break;
					// case "Model":
					// 	APIPython.colorByModel(selName, "dxiso");
					// 	break;
					case "Hydrophobicity":
						APIPython.colorByHydrophobicity(selName, "dxiso");
						break;
					case "ResId":
						APIPython.colorByResid(selName, "dxiso");
						break;
					case "ResNum":
						APIPython.colorByResnum(selName, "dxiso");
						break;
					case "Sequence":
						APIPython.colorBySequence(selName, "dxiso");
						break;
					case "ResType":
						APIPython.colorByResidueType(selName, "dxiso");
						break;
					case "ResCharge":
						APIPython.colorByResidueCharge(selName, "dxiso");
						break;
					case "BFactor":
						APIPython.colorByBfactor(selName, "dxiso");
						break;
					case "Charge":
						APIPython.colorByCharge(selName);
						break;
					}
				}
			});


			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "dxiso");
			});

			newRow.transform.Find("Options/ColorButtons3/ShadowsOn/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setShadows(selName, "dxiso", true);
			});

			newRow.transform.Find("Options/ColorButtons3/ShadowsOff/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setShadows(selName, "dxiso", false);
			});

			ColorPickerControl dxColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

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


			Slider dxSliderSmooth = newRow.transform.Find("Options/SliderSmooth/Slider").gameObject.GetComponent<Slider>();
			dxSliderSmooth.onValueChanged.AddListener(
			delegate {
				APIPython.setSmoothness(selName, "dxiso", dxSliderSmooth.value);
			});

			Slider dxSliderMetal = newRow.transform.Find("Options/SliderMetal/Slider").gameObject.GetComponent<Slider>();
			dxSliderMetal.onValueChanged.AddListener(
			delegate {
				APIPython.setMetal(selName, "dxiso", dxSliderMetal.value);
			});

			Slider dxSliderAlpha = newRow.transform.Find("Options/SliderAlpha/Slider").gameObject.GetComponent<Slider>();
			dxSliderAlpha.onValueChanged.AddListener(
			delegate {
				APIPython.setTransparentSurface(selName, dxSliderAlpha.value);
			});
			Slider dxSliderWire = newRow.transform.Find("Options/SliderWire/Slider").gameObject.GetComponent<Slider>();
			dxSliderWire.onValueChanged.AddListener(
			delegate {
				APIPython.setSurfaceWireframe(selName, "dxiso", dxSliderWire.value);
			});

			Dropdown RTMatdxisotdropDown = newRow.transform.Find("Options/RaytracingMaterials/Dropdown").gameObject.GetComponent<Dropdown>();

			if (UnityMolMain.raytracingMode) {

				List<string> opts = new List<string>(RaytracingMaterial.materialsBank.Count + 1);
				RTMatdxisotdropDown.ClearOptions();

				opts.Add("Raytracing Materials");
				foreach (string k in RaytracingMaterial.materialsBank.Keys) {
					opts.Add(k);
				}

				RTMatdxisotdropDown.AddOptions(opts);
			}

			RTMatdxisotdropDown.onValueChanged.AddListener(
			delegate {
				string matName = RTMatdxisotdropDown.options[RTMatdxisotdropDown.value].text;
				if (RaytracingMaterial.materialsBank.ContainsKey(matName)) {
					APIPython.setRTMaterial(selName, "dxiso", matName);
				}
			}
			);

			break;

		case "sheherasade":
			//Sheherasade ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {

				if (APIPython.areRepresentationsOn(selName, "sheherasade")) {
					APIPython.hideSelection(selName, "sheherasade");
				}
				else {
					APIPython.showSelection(selName, "sheherasade");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "sheherasade");
				APIPython.deleteRepresentationInSelection(selName, "sheherasade");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});


			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "sheherasade");
			});

			newRow.transform.Find("Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.colorByChain(selName, "sheherasade");
			});
			newRow.transform.Find("Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.colorByResidue(selName, "sheherasade");
			});

			ColorPickerControl sheheColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			sheheColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "sheherasade", sheheColorPicker.CurrentColor);
			});

			Slider sheheSliderSmooth = newRow.transform.Find("Options/SliderSmooth/Slider").gameObject.GetComponent<Slider>();
			sheheSliderSmooth.onValueChanged.AddListener(
			delegate {
				APIPython.setSmoothness(selName, "sheherasade", sheheSliderSmooth.value);
			});

			Slider sheheSliderMetal = newRow.transform.Find("Options/SliderMetal/Slider").gameObject.GetComponent<Slider>();
			sheheSliderMetal.onValueChanged.AddListener(
			delegate {
				APIPython.setMetal(selName, "sheherasade", sheheSliderMetal.value);
			});


			newRow.transform.Find("Options/SwitchSmooth/ToggleSmooth/Toggle").gameObject.GetComponent<Toggle>().onValueChanged.AddListener(
			delegate {
				APIPython.switchSheherasadeMethod(selName);
			});

			newRow.transform.Find("Options/TextureButtons/NoTexture/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setSheherasadeTexture(selName, -1);
			});

			newRow.transform.Find("Options/TextureButtons/ArrowTexture/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.setSheherasadeTexture(selName, 0);
			});
			break;

		case "ellipsoid":
			//Ellipsoid ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.areRepresentationsOn(selName, "ellipsoid")) {
					APIPython.hideSelection(selName, "ellipsoid");
				}
				else {
					APIPython.showSelection(selName, "ellipsoid");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "ellipsoid");
				APIPython.deleteRepresentationInSelection(selName, "ellipsoid");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});


			Slider ellipsoidSliderSize = newRow.transform.Find("Options/SliderSize/SliderWidth").gameObject.GetComponent<Slider>();
			ellipsoidSliderSize.onValueChanged.AddListener(
			delegate {
				APIPython.setRepSize(selName, "ellipsoid", ellipsoidSliderSize.value);
			});


			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "ellipsoid");
			});

			newRow.transform.Find("Options/ColorButtons/ColorByChain/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.colorByChain(selName, "ellipsoid");
			});
			newRow.transform.Find("Options/ColorButtons/ColorByResidue/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.colorByResidue(selName, "ellipsoid");
			});

			ColorPickerControl ellipsoidColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			ellipsoidColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "ellipsoid", ellipsoidColorPicker.CurrentColor);
			});
			break;

		case "point":
		case "points":
		case "p":
			//Point ------------------------------------
			newRow.transform.Find("OptionsLabel/ShowHideButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				if (APIPython.areRepresentationsOn(selName, "p")) {
					APIPython.hideSelection(selName, "p");
				}
				else {
					APIPython.showSelection(selName, "p");
				}
			});

			newRow.transform.Find("OptionsLabel/DeleteRepButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				// APIPython.hideSelection(selName, "p");
				APIPython.deleteRepresentationInSelection(selName, "p");
				newRow.transform.Find("Options").gameObject.SetActive(false);
				newRow.SetActive(false);
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
				newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

				Canvas.ForceUpdateCanvases();
			});


			Slider pointSliderSize = newRow.transform.Find("Options/SliderSize/SliderWidth").gameObject.GetComponent<Slider>();
			pointSliderSize.onValueChanged.AddListener(
			delegate {
				APIPython.setRepSize(selName, "p", pointSliderSize.value);
			});


			Dropdown colorByDDP = newRow.transform.Find("Options/ColorButtons/DropdownColorby").gameObject.GetComponent<Dropdown>();

			if (!mainUIName.Contains("VR")) {
				Transform tmpcont = colorByDDP.transform.Find("Template/Viewport/Content");
				Destroy(tmpcont.GetComponent<CanvasRaycastTarget>());
				Destroy(tmpcont.GetComponent<Canvas>());
			}

			colorByDDP.onValueChanged.AddListener(delegate {
				if (colorByDDP.value != 0) {
					string choice = colorByDDP.options[colorByDDP.value].text;

					switch (choice) {
					case "Atom":
						APIPython.colorByAtom(selName, "p");
						break;
					case "Residue":
						APIPython.colorByResidue(selName, "p");
						break;
					case "Chain":
						APIPython.colorByChain(selName, "p");
						break;
					// case "Model":
					// 	APIPython.colorByModel(selName, "p");
					// 	break;
					case "Hydrophobicity":
						APIPython.colorByHydrophobicity(selName, "p");
						break;
					case "ResId":
						APIPython.colorByResid(selName, "p");
						break;
					case "ResNum":
						APIPython.colorByResnum(selName, "p");
						break;
					case "Sequence":
						APIPython.colorBySequence(selName, "p");
						break;

					case "ResType":
						APIPython.colorByResidueType(selName, "p");
						break;
					case "ResCharge":
						APIPython.colorByResidueCharge(selName, "p");
						break;
					case "BFactor":
						APIPython.colorByBfactor(selName, "p");
						break;
					}
				}
			});


			newRow.transform.Find("Options/ColorButtons/ResetColorButton/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
			delegate {
				APIPython.resetColorSelection(selName, "p");
			});



			ColorPickerControl pointColorPicker = newRow.transform.Find("Options/ColorPicker").gameObject.GetComponent<ColorPickerControl>();

			pointColorPicker.onValueChanged.AddListener(
			delegate {
				APIPython.colorSelection(selName, "p", pointColorPicker.CurrentColor);
			});
			break;
		}
		//Force update UI layout
		newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
		newButtonGo.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

		Canvas.ForceUpdateCanvases();
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
			// updateRepresentationUI();
			updateRepVisiUI();
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


		//Delete Button -------------------------------
		newButtonGo.transform.Find("Main/Delete/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.deleteSelection(sel.name);
		});

		newButtonGo.transform.Find("Main/Duplicate/Button Layer").gameObject.GetComponent<Button>().onClick.AddListener(
		delegate {
			APIPython.duplicateSelection(sel.name);
		});


		newButtonGo.transform.Find("Main/Toggle").gameObject.GetComponent<ClickController>().onSingleClick.AddListener(
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


		newButtonGo.transform.Find("Main/Toggle").gameObject.GetComponent<ClickController>().onDoubleClick.AddListener(
		delegate {
			APIPython.centerOnSelection(sel.name, lerp: true);
		});

		newButtonGo.transform.Find("Main/Toggle").gameObject.GetComponent<ClickController>().onLongClick.AddListener(
		delegate {
			Debug.Log("Adding selection to tour : " + sel);
			APIPython.addSelectionToTour(sel.name);
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

	void updateRTMat(NewRTMatEventArgs rtmatargs) {

		try {
			foreach (string selS in selectionToUIObject.Keys) {
				GameObject selUIgo = selectionToUIObject[selS];
				if (selM.selections.ContainsKey(selS)) {
					foreach (RepType rept in selM.selections[selS].representations.Keys) {
						GameObject toggleRepType = repTypeToGo(selUIgo, rept);
						if (toggleRepType != null) {
							Transform dpgo = toggleRepType.transform.Find("Options/RaytracingMaterials/Dropdown");
							if (dpgo != null) {
								Dropdown rtmatDropdown = dpgo.gameObject.GetComponent<Dropdown>();

								//Update dropdown enabled
								rtmatDropdown.interactable = UnityMolMain.raytracingMode;
								//Update list of mat
								rtmatDropdown.ClearOptions();
								List<string> opts = new List<string>(RaytracingMaterial.materialsBank.Count + 1);

								opts.Add("Raytracing Materials");
								foreach (string k in RaytracingMaterial.materialsBank.Keys) {
									opts.Add(k);
								}
								rtmatDropdown.AddOptions(opts);

								//Update current mat for this rep
							}
						}
					}
				}
			}
		}
		catch (System.Exception e) {
			Debug.LogError(e);
		}
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
					res = surfRep.SurfMethod;
				}
			}
		}
		return res;
	}

	public void switchDockingMode() {
		APIPython.switchDockingMode();
	}
	public void saveDockingState() {
		DockingManager dm = UnityMolMain.getDockingManager();
		dm.saveDockingState();
	}
	public void enableDockingColliders() {
		DockingManager dm = UnityMolMain.getDockingManager();
		dm.EnableColliders();
	}
	public void disableDockingColliders() {
		DockingManager dm = UnityMolMain.getDockingManager();
		dm.DisableColliders();
	}

	public void enableDockingCollidersCustom(InputField ipfn) {
		if (ipfn != null) {
			string ipftt = getOtherInputFieldText(ipfn);
			DockingManager dm = UnityMolMain.getDockingManager();
			dm.EnableCollidersCustomAtoms(ipfn.text, ipftt);
		}
	}

	public void disableDockingCollidersCustom(InputField ipfn) {
		if (ipfn != null) {
			string ipftt = getOtherInputFieldText(ipfn);
			DockingManager dm = UnityMolMain.getDockingManager();
			dm.DisableCollidersCustomAtoms(ipfn.text, ipftt);
		}
	}
	string getOtherInputFieldText(InputField ipf) {
		Transform p = ipf.transform.parent.parent;
		if (p != null) {
			var ii = p.GetComponentsInChildren<InputField>();
			foreach (InputField ip in ii) {
				if (ip != ipf) {
					return ip.text;
				}
			}
		}
		return null;
	}

	public void switchRaytracingMode(Text t) {
		UnityMolMain.raytracingMode = !UnityMolMain.raytracingMode;
		if (t != null) {
			t.text = "Raytracing\n<b><color=#007AC1>";

			if (UnityMolMain.raytracingMode) {
				t.text += "On";
			}
			else {
				t.text += "Off";
			}
			t.text += "</color></b>";
		}
	}
	public void switchRTDenoiser(Text t) {
		if (!RaytracerManager.Instance)
			return;
		RaytracerManager.Instance.forceDenoiserOff(RaytracerManager.Instance.rtDenoiser);
		if (t != null) {
			t.text = "Denoiser\n<b><color=#007AC1>";

			if (RaytracerManager.Instance.rtDenoiser) {
				t.text += "On";
			}
			else {
				t.text += "Off";
			}
			t.text += "</color></b>";
		}
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
	public void switchIDLEMode(Text t) {
		UnityMolMain.allowIDLE = !UnityMolMain.allowIDLE;

		t.text = "IDLE Mode\n <b><color=#007AC1>";

		if (UnityMolMain.allowIDLE) {
			t.text += "On";
		}
		else {
			t.text += "Off";
		}
		t.text += "</color></b>";
	}

	public void switchMeasureMode(Text t) {
		t.text = "Measure Mode\n <b><color=#007AC1>";

		int newMode = (int) UnityMolMain.measureMode + 1;
		if (newMode > 2)
			newMode = 0;
		APIPython.setMeasureMode(newMode);
		switch (newMode) {
		case 0:
			t.text += "Distance";
			break;
		case 1:
			t.text += "Angle";
			break;
		case 2:
			t.text += "TorsionAngle";
			break;
		default:
			break;
		}
		t.text += "</color></b>";
	}

	public void setFogStart(Slider s) {
		APIPython.setDepthCueingStart(s.value);
	}
	public void setFogDensity(Slider s) {
		APIPython.setDepthCueingDensity(s.value);
	}
	public void setOutlineThickness(Slider s) {
		APIPython.setOutlineThickness(s.value);
	}
	public void setOutlineColor(ColorPickerControl cpc) {
		APIPython.setOutlineColor(cpc.CurrentColor);
	}

	public void switchDOF(Text t) {
		if (UnityMolMain.isDOFOn) {
			APIPython.disableDOF();
		}
		else {
			APIPython.enableDOF();
		}

		t.text = "Depth of Field\n <b><color=#007AC1>";

		if (UnityMolMain.isDOFOn) {
			t.text += "On";
		}
		else {
			t.text += "Off";
		}
		t.text += "</color></b>";
	}

	public void setDOFAperture(Slider s) {
		APIPython.setDOFAperture(s.value);
	}
	public void setDOFFocalLength(Slider s) {
		APIPython.setDOFFocalLength(s.value);
	}

	public void freezeLoadedMol() {
		DockingManager dm = UnityMolMain.getDockingManager();
		foreach (UnityMolStructure s in sm.loadedStructures) {
			dm.FreezeDockingRigidbody(s.name);
		}
	}
	public void unfreezeLoadedMol() {
		DockingManager dm = UnityMolMain.getDockingManager();
		foreach (UnityMolStructure s in sm.loadedStructures) {
			dm.UnfreezeDockingRigidbody(s.name);
		}
	}
	public void setDockingScaleVDW(InputField ifvdw) {
		float newV = 1.0f;
		try {
			newV = float.Parse(ifvdw.text);
		}
		catch {
			Debug.LogWarning("Wrong float value");
			ifvdw.text = "1.0";
		}
		UnityMolMain.getDockingManager().VDWUIScaling = newV;
	}
	public void setDockingScaleElec(InputField ifelec) {
		float newV = 1.0f;
		try {
			newV = float.Parse(ifelec.text);
		}
		catch {
			Debug.LogWarning("Wrong float value");
			ifelec.text = "1.0";
		}
		UnityMolMain.getDockingManager().ElecUIScaling = newV;
	}
	public void switchRoomMode() {
		if (room != null) {
			room.SetActive(!room.activeInHierarchy);
			if (floor != null) {
				floor.SetActive(!room.activeInHierarchy);
			}
		}
	}

	public void switchRoom() {
		Scene scene = SceneManager.GetActiveScene();
		if (scene.name.Contains("MainScene") || scene.name.Contains("living"))
			SceneManager.LoadScene("RoomVR_hexa");
		else if (scene.name.Contains("hexa"))
			SceneManager.LoadScene("RoomVR_floor");
		else if (scene.name.Contains("floor"))
			SceneManager.LoadScene("RoomVR_living");
		room = GameObject.Find("RoomVR");
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
	public void setScale(Slider s) {
		APIPython.changeGeneralScale(s.value);
	}

	public void switchSeqViewer() {
		if (seqUIGo != null) {
			seqUIGo.SetActive(!seqUIGo.activeInHierarchy);
		}
	}

	public void centerViewStructure() {
		if (selM.currentSelection != null && selM.currentSelection.Count != 0) {
			APIPython.centerOnStructure(selM.currentSelection.structures[0].name, lerp: true);
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

	public void switchOutline(Text t) {
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

	public void startTour() {
		APIPython.getManipulationManager().startTour();
	}
	public void clearTourSelections() {
		APIPython.clearTour();
	}
	public void tourPrev() {
		APIPython.getManipulationManager().tourPrevious();
	}
	public void tourNext() {
		APIPython.getManipulationManager().tourNext();
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
				brep.Hbonds = filterHbondsNotInterface(ligandSelection, brep.Hbonds);
				brep.Recompute(true);
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

		UnityMolModel m = ligand.atoms[0].residue.chain.model;
		foreach (int ida in bonds.bonds.Keys) {
			UnityMolAtom atom1 = m.allAtoms[ida];

			bool atom1Lig = ligand.atoms.Contains(atom1);
			foreach (int idb in bonds.bonds[ida]) {
				if (idb != -1) {
					UnityMolAtom atom2 = m.allAtoms[idb];
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
		int port = 8888;
		string address = "localhost";
		if (!string.IsNullOrEmpty(inFIMDIP.text))
			address = inFIMDIP.text;
		if (!string.IsNullOrEmpty(inFIMDPort.text))
			port = int.Parse(inFIMDPort.text);

		if (APIPython.last() != null) {
			APIPython.connectIMD(APIPython.last().name, address, port);
		}
		else {
			Debug.LogWarning("Load a molecule first");
		}

	}
	public void disconnectIMD() {
		APIPython.disconnectIMD(APIPython.last().name);
	}

    public void connectMultiplayer() {
        if (inFRoom == null || inFUser == null) {
            inFRoom = GameObject.Find("InputFieldRoomName/InputField").GetComponent<InputField>();
            inFUser = GameObject.Find("InputFieldUserName/InputField").GetComponent<InputField>();
        }

        if (string.IsNullOrEmpty(inFRoom.text)) {
            Debug.LogError("Missing Room Name. Please specify one and click connect again");
            return;
        }

        if (string.IsNullOrEmpty(inFUser.text)) {
            Debug.LogError("Missing Player Name. Please specify one and click connect again");
            return;
        }

        APIPython.connectMultiplayer(inFRoom.text, inFUser.text);
    }

    public void disconnectMultiplayer() {
        APIPython.disconnectMultiplayer();
    }


	void initIMDGraphUI(StructureEventArgs args) {
		UnityMolStructure s = args.structure;
		List<GameObject> graphUI = new List<GameObject>(3);
		GraphManager gm = UnityMolMain.getGraphManager();

		Texture2D t1 = gm.GetPlotTexture(s.mddriverM.curNamePlot1);
		Texture2D t2 = gm.GetPlotTexture(s.mddriverM.curNamePlot2);
		Texture2D t3 = gm.GetPlotTexture(s.mddriverM.curNamePlot3);

		if (!transform.name.Contains("VR")) {
			Transform graphPar = transform.Find("CanvasGraphs/Panel");
			if (graphPar == null) {
				Debug.LogError("Cannot create graph, UI is not correctly setup");
				return;
			}
			Transform graphCanv = transform.Find("CanvasGraphs");
			graphCanv.Find("ButtonClose").gameObject.SetActive(true);

			// if (graphPar == null) {
			// 	GameObject canv = new GameObject("CanvasGraphs");
			// 	canv.transform.parent = transform;
			// 	canv.AddComponent<Canvas>();
			// 	GameObject pan = new GameObject("CanvasGraphs");
			// 	pan.transform.parent = canv.transform;
			// 	Image pani = pan.AddComponent<Image>();
			// 	pani.color = new Color(0.0f, 0.0f, 0.0f, 0.4f);

			// 	GridLayoutGroup glg = pan.AddComponent<GridLayoutGroup>();
			// 	glg.cellSize = Vector2.one * 300.0f;
			// 	graphPar = pan.transform;
			// }

			graphPar.gameObject.GetComponent<Image>().enabled = true;

			GameObject graph1 = new GameObject(s.mddriverM.curNamePlot1);
			GameObject graph2 = new GameObject(s.mddriverM.curNamePlot2);
			GameObject graph3 = new GameObject(s.mddriverM.curNamePlot3);

			graph1.transform.parent = graphPar; graph1.transform.localScale = Vector3.one;
			graph2.transform.parent = graphPar; graph2.transform.localScale = Vector3.one;
			graph3.transform.parent = graphPar; graph3.transform.localScale = Vector3.one;

			RawImage im1 = graph1.AddComponent<RawImage>();
			RawImage im2 = graph2.AddComponent<RawImage>();
			RawImage im3 = graph3.AddComponent<RawImage>();

			im1.texture = t1; im1.raycastTarget = false;
			im2.texture = t2; im2.raycastTarget = false;
			im3.texture = t3; im3.raycastTarget = false;

			graphUI.Add(graph1);
			graphUI.Add(graph2);
			graphUI.Add(graph3);
		}
		else {
			GameObject graphcanvgo1 = Instantiate((GameObject) Resources.Load("Prefabs/CanvasGraphVR"));
			GameObject graphcanvgo2 = Instantiate((GameObject) Resources.Load("Prefabs/CanvasGraphVR"));
			GameObject graphcanvgo3 = Instantiate((GameObject) Resources.Load("Prefabs/CanvasGraphVR"));

			DontDestroyOnLoad(graphcanvgo1);
			DontDestroyOnLoad(graphcanvgo2);
			DontDestroyOnLoad(graphcanvgo3);

			graphcanvgo1.transform.position = new Vector3(0.0f, 1.0f, 0.5f);
			graphcanvgo2.transform.position = new Vector3(0.5f, 1.0f, 0.5f);
			graphcanvgo3.transform.position = new Vector3(1.0f, 1.0f, 0.5f);

			Transform graphPar1 = graphcanvgo1.transform.Find("Panel");
			Transform graphPar2 = graphcanvgo2.transform.Find("Panel");
			Transform graphPar3 = graphcanvgo3.transform.Find("Panel");
			if (graphPar1 == null) {
				Debug.LogError("Cannot create graph, VR prefab UI is not correctly setup");
				return;
			}

			GameObject graph1 = new GameObject(s.mddriverM.curNamePlot1, typeof(RectTransform));
			GameObject graph2 = new GameObject(s.mddriverM.curNamePlot2, typeof(RectTransform));
			GameObject graph3 = new GameObject(s.mddriverM.curNamePlot3, typeof(RectTransform));

			graph1.transform.parent = graphPar1; graph1.transform.localScale = Vector3.one; graph1.transform.localPosition = Vector3.zero;
			graph2.transform.parent = graphPar2; graph2.transform.localScale = Vector3.one; graph2.transform.localPosition = Vector3.zero;
			graph3.transform.parent = graphPar3; graph3.transform.localScale = Vector3.one; graph3.transform.localPosition = Vector3.zero;

			RectTransform rt1 = graph1.GetComponent (typeof (RectTransform)) as RectTransform;
			RectTransform rt2 = graph2.GetComponent (typeof (RectTransform)) as RectTransform;
			RectTransform rt3 = graph3.GetComponent (typeof (RectTransform)) as RectTransform;
			rt1.sizeDelta = Vector2.one * 300;
			rt2.sizeDelta = Vector2.one * 300;
			rt3.sizeDelta = Vector2.one * 300;

			RawImage im1 = graph1.AddComponent<RawImage>();
			RawImage im2 = graph2.AddComponent<RawImage>();
			RawImage im3 = graph3.AddComponent<RawImage>();

			im1.texture = t1; //im1.raycastTarget = false;
			im2.texture = t2; //im2.raycastTarget = false;
			im3.texture = t3; //im3.raycastTarget = false;

			graphUI.Add(graphcanvgo1);
			graphUI.Add(graphcanvgo2);
			graphUI.Add(graphcanvgo3);
		}


		graphUIPerStruc[s.name] = graphUI;

	}
	void destroyIMDGraphUI(StructureEventArgs args) {
		UnityMolStructure s = args.structure;
		if (graphUIPerStruc.ContainsKey(s.name)) {
			for (int i = 0; i < graphUIPerStruc[s.name].Count; i++) {
				GameObject.Destroy(graphUIPerStruc[s.name][i]);
			}
			graphUIPerStruc.Remove(s.name);
		}
		if (!transform.name.Contains("VR")) {
			if (graphUIPerStruc.Count == 0) {
				Transform graphPar = transform.Find("CanvasGraphs/Panel");
				if (graphPar != null)
					graphPar.gameObject.GetComponent<Image>().enabled = false;
				Transform graphCanv = transform.Find("CanvasGraphs");
				graphCanv.Find("ButtonClose").gameObject.SetActive(false);
				graphCanv.Find("ButtonShow").gameObject.SetActive(false);

			}
		}
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


	public void setLightIntensity(Slider s) {
		APIPython.setDirLightIntensity(s.value);
	}

	public void setAmbientLightIntensity(Slider s) {
		APIPython.setAmbientLightIntensity(s.value);
	}

	public void setLightShadow(Slider s) {
		APIPython.setDirLightShadow(s.value);
	}

	float lightDirX = 35.0f;
	float lightDirY = 0.0f;
	float lightDirZ = 0.0f;

	public void setLightDirX(Slider s) {
		lightDirX = s.value;
		Vector3 curEuler = new Vector3(lightDirX, lightDirY, lightDirZ);
		APIPython.setDirLightDirection(curEuler);
		if (lightDir != null) {
			lightDir.SetActive(true);
			lightDir.transform.rotation = Quaternion.Euler(curEuler);
			Invoke("hideLightDir", 2.0f);
		}
	}

	public void setLightDirY(Slider s) {
		lightDirY = s.value;
		Vector3 curEuler = new Vector3(lightDirX, lightDirY, lightDirZ);
		APIPython.setDirLightDirection(curEuler);
		if (lightDir != null) {
			lightDir.SetActive(true);
			lightDir.transform.rotation = Quaternion.Euler(curEuler);
			Invoke("hideLightDir", 2.0f);
		}
	}
	public void setLightDirZ(Slider s) {
		lightDirZ = s.value;
		Vector3 curEuler = new Vector3(lightDirX, lightDirY, lightDirZ);
		APIPython.setDirLightDirection(curEuler);
		if (lightDir != null) {
			lightDir.SetActive(true);
			lightDir.transform.rotation = Quaternion.Euler(curEuler);
			Invoke("hideLightDir", 2.0f);
		}
	}
	void hideLightDir() {
		if (lightDir != null) {
			lightDir.SetActive(false);
		}
	}

	public void CopyToClipboard(TMPro.TMP_Text t)
	{
		TextEditor te = new TextEditor();
		te.text = t.text;
		te.SelectAll();
		te.Copy();
	}

	void LateUpdate() {
#if DISABLE_MAINUI
		return;
#endif

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

		if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Backspace)) {
			if (scrollView.activeInHierarchy) {
				if (closeUIButton != null)
					closeUIButton.GetComponent<Button>().onClick.Invoke();
			}
			else {
				if (showUIButton != null)
					showUIButton.GetComponent<Button>().onClick.Invoke();
			}
		}

		if (shouldUpdateRepVisiUI)
			updateRepVisiUI();
		if (shouldUpdateSelUI)
			updateSelectionUI();
		if (shouldUpdateCountLabel)
			updateCountLabel();

		if (UnityMolMain.raytracingMode != curRTMode) {
			curRTMode = UnityMolMain.raytracingMode;
			updateRTMat(null);
		}
	}
}
}
