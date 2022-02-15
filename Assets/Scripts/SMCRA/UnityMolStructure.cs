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
using UnityEngine.XR;
using System.Collections.Generic;
using System.Linq;

using VRTK;
namespace UMol {

/// <summary>
/// Part of the SMCRA data structure, UnityMolStructure stores the models of the structure
/// </summary>
public class UnityMolStructure {

	/// <summary>
	/// Store all the models of the structure
	/// </summary>
	public List<UnityMolModel> models;

	/// <summary>
	/// Parsed structure name
	/// </summary>
	public string name;

	/// <summary>
	/// Unique structure name
	/// </summary>
	public string uniqueName;

	/// <summary>
	/// Identified type of molecular structure (all-atoms/Martini/Hire-RNA/OPEP)
	/// </summary>
	public MolecularType structureType = MolecularType.standard;

	/// <summary>
	/// List of representations for this structure
	/// </summary>
	public List<UnityMolRepresentation> representations;

	/// <summary>
	/// Trajectory loaded for this structure
	/// </summary>
	public bool trajectoryLoaded = false;

	/// <summary>
	/// Recompute secondary structure assignation for each step of the trajectory
	/// </summary>
	public bool trajComputeSS = false;

	/// <summary>
	/// Atom positions used when reading a trajectory file
	/// </summary>
	public Vector3[] trajAtomPositions;

	/// <summary>
	/// Trajectory reader using xdrfile library
	/// </summary>
	public XDRFileReader xdr;

	/// <summary>
	/// Trajectory reader using xdrfile library
	/// </summary>
	public ArtemisManager artemisM;

	/// <summary>
	/// Trajectory player calling trajNext from a monobehaviour Update loop
	/// </summary>
	public TrajectoryPlayer trajPlayer;

	/// <summary>
	/// Multiple models should be read as a trajectory
	/// </summary>
	public bool trajectoryMode = false;

	/// <summary>
	/// If trajectoryMode is true, for each frame/model, store the positions of each atom
	/// </summary>
	public List<Vector3[]> modelFrames;

	/// <summary>
	/// Dx file reader
	/// </summary>
	public DXReader dxr;

	/// <summary>
	/// Secondary structures were parsed
	/// </summary>
	public bool ssInfoFromFile = false;

	/// <summary>
	/// Secondary structures parsed from the file
	/// </summary>
	public List<Reader.secStruct> parsedSSInfo;

	/// <summary>
	/// Connectivity parsed from the file
	/// </summary>
	public List<Int2> parsedConnectivity;

	/// <summary>
	/// Class holding threads to compute surfaces started when creating the structure
	/// </summary>
	public SurfaceThread surfThread;

	/// <summary>
	/// Current model used
	/// </summary>
	public UnityMolModel currentModel {
		get
		{
			return models[currentModelId];
		}
	}

	/// <summary>
	/// Current model id used
	/// </summary>
	public int currentModelId = 0;

	/// <summary>
	/// Current frame id used, when trajectoryMode is true
	/// </summary>
	public int currentFrameId = 0;

	/// <summary>
	/// Monobehaviour script to read models like trajectories
	/// </summary>
	public ModelsPlayer modelsPlayer;

	/// <summary>
	/// Id of the group used to move the molecules
	/// </summary>
	public int groupID = 0;

	/// <summary>
	/// Associates a GameObject with an UnityMolAtom
	/// </summary>
	public Dictionary<UnityMolAtom, GameObject> atomToGo;

	/// <summary>
	/// Array of GameObjects, one for each atom, created in Reader.cs
	/// </summary>
	private GameObject[] atomGos;

	public enum MolecularType {
		standard,//All-atoms
		// Martini,//WARNING Not published yet
		OPEP,
		HIRERNA
	}


	/// <summary>
	/// UnityMolStructure constructor taking a list of models as arg
	/// </summary>
	public UnityMolStructure(List<UnityMolModel> listModels, string nameStructure) {
		models = listModels;
		name = nameStructure;
		uniqueName = nameStructure;

		if (UnityMolMain.getStructureManager().isNameUsed(uniqueName)) {
			uniqueName = UnityMolMain.getStructureManager().findNewStructureName(uniqueName);
		}

		representations = new List<UnityMolRepresentation>();
		currentModelId = 0;
		currentFrameId = 0;
		createModelPlayer();
	}

	/// <summary>
	/// UnityMolStructure constructor taking a model as arg
	/// </summary>
	public UnityMolStructure(UnityMolModel newModel, string nameStructure) {
		models = new List<UnityMolModel>();
		models.Add(newModel);
		name = nameStructure;
		uniqueName = nameStructure;
		representations = new List<UnityMolRepresentation>();
		currentModelId = 0;
		currentFrameId = 0;
		createModelPlayer();
	}


	/// <summary>
	/// UnityMolStructure constructor taking a list of models and a list of frames
	/// </summary>
	public UnityMolStructure(List<UnityMolModel> listModels, string nameStructure, List<Vector3[]> frames) {
		models = listModels;
		name = nameStructure;
		uniqueName = nameStructure;
		modelFrames = frames;
		trajectoryMode = true;

		if (UnityMolMain.getStructureManager().isNameUsed(uniqueName)) {
			uniqueName = UnityMolMain.getStructureManager().findNewStructureName(uniqueName);
		}

		representations = new List<UnityMolRepresentation>();
		currentModelId = 0;
		currentFrameId = 0;
		createModelPlayer();
	}

	/// <summary>
	/// Returns the number of atoms in the current model
	/// </summary>
	public int Count {
		get {return currentModel.allAtoms.Count;}
	}
	/// <summary>
	/// Returns the number of atoms in the current model
	/// </summary>
	public int Length {
		get {return Count;}
	}


	/// <summary>
	/// Update atom radii and colors of all atoms based on the detected structure molecular type (Martini/HireRNA...)
	/// </summary>
	public void updateAtomRepValues() {
		string prefix = UnityMolMain.topologies.prefixMolType[structureType];


		foreach (UnityMolModel m in models) {
			foreach (UnityMolAtom a in m.allAtoms) {
				//WARNING Not published yet
				a.SetAtomRepresentationModel(prefix);
				
			}
		}
	}

	public void readTrajectoryXDR(string trajPath) {
		if (xdr == null && !trajectoryLoaded) {
			xdr = new XDRFileReader();
		}
		int result = xdr.open_trajectory(this, trajPath);
		if (result >= 0 ) {
			xdr.load_trajectory();
		}
		else if (result == (int) XDRFileReaderStatus.TRAJECTORYPRESENT) {
			throw new System.Exception("Trajectory already exists");
		}
		else {
			unloadTrajectoryXDR();
			throw new System.Exception("Trajectory reader failure " + result);
		}
	}

	public void unloadTrajectoryXDR() {
		if (xdr != null) {
			xdr.Clear();
			xdr = null;
			for (int i = 0; i < currentModel.allAtoms.Count; i++) {
				currentModel.allAtoms[i].position = currentModel.allAtoms[i].oriPosition;
			}
			currentModel.ComputeCenterOfGravity();

			updateRepresentations(trajectory: false);

			trajAtomPositions = null;
		}

		if (trajPlayer) {
			GameObject.DestroyImmediate(trajPlayer);
		}
#if !DISABLE_HIGHLIGHT
		UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();
		hM.Clean();
#endif

	}

	public void readDX(string dxPath) {
		if (dxr != null) {
			unloadDX();
		}
		try {
			dxr = new DXReader(dxPath);
			dxr.readDxFile(this);
		}
		catch {
			throw new System.Exception("DX map reader failure");
		}

	}

	public void unloadDX() {
		if (dxr != null) {
			dxr.densityValues = null;
			dxr = null;
		}
	}

	public void createModelPlayer() {
		if (models.Count > 1 || (trajectoryMode && modelFrames != null && modelFrames.Count > 1) ) {
			UnityMolStructureManager sm = UnityMolMain.getStructureManager();
			GameObject structureParent = null;
			try {
				structureParent = sm.GetStructureGameObject(uniqueName);
			}
			catch {
				GameObject loadedMolGO = UnityMolMain.getRepresentationParent();
				string sName = ToSelectionName();
				Transform sP = loadedMolGO.transform.Find(ToSelectionName());
				if (UnityMolMain.inVR() && sP == null) {

					Transform clref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
					Transform crref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
					if (clref != null) {
						sP = clref.Find(sName);
					}
					if (sP == null && crref != null) {
						sP = crref.Find(sName);
					}
				}

				if (sP == null) {
					structureParent = new GameObject(sName);
					structureParent.transform.parent = loadedMolGO.transform;
					structureParent.transform.localPosition = Vector3.zero;
					structureParent.transform.localRotation = Quaternion.identity;
					structureParent.transform.localScale = Vector3.one;
				}
			}

			modelsPlayer = structureParent.AddComponent<ModelsPlayer>();
			modelsPlayer.play = false;
			modelsPlayer.s = this;
		}
	}

	public void modelNext(bool forward = true, bool loop = true) {
		if (trajectoryMode) {
			if (modelFrames.Count > 1) {
				if (forward) {
					if (currentFrameId + 1 >= modelFrames.Count) {
						if (loop) {
							currentFrameId = 0;
						}
						else {
							currentFrameId = modelFrames.Count - 1;
						}
					}
					else {
						currentFrameId++;
					}
				}
				else {
					if (currentFrameId - 1 < 0) {
						if (loop) {
							currentFrameId = modelFrames.Count - 1;
						}
						else {
							currentFrameId = 0;
						}
					}
					else {
						currentFrameId--;
					}
				}
				API.APIPython.setModel(uniqueName, currentFrameId);
			}
		}
		else {


			if (models.Count > 1) {
				if (forward) {
					if (currentModelId + 1 >= models.Count) {
						if (loop) {
							currentModelId = 0;
						}
						else {
							currentModelId = models.Count - 1;
						}
					}
					else {
						currentModelId++;
					}
				}
				else {
					if (currentModelId - 1 < 0) {
						if (loop) {
							currentModelId = models.Count - 1;
						}
						else {
							currentModelId = 0;
						}
					}
					else {
						currentModelId--;
					}
				}
				API.APIPython.setModel(uniqueName, currentModelId);
			}
		}
	}

	public void setModel(int modelId) {
		if (trajectoryMode && modelFrames != null && modelFrames.Count > 1) {
			if (modelId >= 0 && modelId < modelFrames.Count) {
				trajAtomPositions = modelFrames[modelId];
				trajUpdateAtomPositions();
				currentFrameId = modelId;

				updateRepresentations(trajectory: true);
			}
			else {
				Debug.LogWarning("Wrong model number");
			}
			return;
		}

		if (models.Count > 1) {
			if (modelId >= 0 && modelId < models.Count) {
				currentModelId = modelId;
				updateRepresentations(trajectory: false);
			}
			else {
				Debug.LogWarning("Wrong model number");
				return;
			}
			return;
		}
		Debug.LogError("This structure does not contain several models");
	}

	public void trajNext(bool forward = true, bool loop = true) {
		if (xdr != null && trajectoryLoaded) {
			int newFrameId = 0;
			if (forward) {
				newFrameId = xdr.currentFrame + 1;
				if (newFrameId >= xdr.numberFrames) {
					if (loop) {
						xdr.sync_scene_with_frame(0);
					}
					else {
						xdr.sync_scene_with_frame(xdr.numberFrames - 1);
					}
				}
				else {
					xdr.sync_scene_with_frame(newFrameId);
				}
			}
			else {
				newFrameId = xdr.currentFrame - 1;
				if (newFrameId < 0) {
					if (loop) {
						xdr.sync_scene_with_frame(xdr.numberFrames - 1);
					}
					else {
						xdr.sync_scene_with_frame(0);
					}
				}
				else {
					xdr.sync_scene_with_frame(newFrameId);
				}
			}

			updateRepresentations();
		}
		else {
			Debug.LogError("No trajectory loaded for this structure");
		}
	}

	public void trajNextSmooth(float t, bool forward = true, bool loop = true, bool newFrame = false) {
		if (xdr != null && trajectoryLoaded) {
			int newFrameId = xdr.currentFrame;
			if (forward) {
				if (newFrame) {
					newFrameId = xdr.currentFrame + 1;
				}
				if (newFrameId + 1 >= xdr.numberFrames) {
					if (loop) {
						xdr.sync_scene_with_frame(0);
					}
					else
						xdr.sync_scene_with_frame(xdr.numberFrames - 1);
				}
				else {
					xdr.sync_scene_with_frame_smooth(newFrameId, newFrameId + 1, t, newFrame);
				}
			}
			else {
				if (newFrame) {
					newFrameId = xdr.currentFrame - 1;
				}
				if (newFrameId - 1 < 0) {
					if (loop) {
						xdr.sync_scene_with_frame(xdr.numberFrames - 1);
					}
					else {
						xdr.sync_scene_with_frame(0);
					}
				}
				else {
					xdr.sync_scene_with_frame_smooth(newFrameId, newFrameId - 1, t, newFrame);
				}
			}

			updateRepresentations();
		}
		else {
			Debug.LogError("No trajectory loaded for this structure");
		}
	}


	public void trajSetFrame(int idF) {
		if (xdr != null && trajectoryLoaded) {
			if (idF >= 0 && idF < xdr.numberFrames) {
				xdr.sync_scene_with_frame(idF);
				updateRepresentations();
			}
			else {
				Debug.LogWarning("Wrong frame number");
			}
		}
		else {
			Debug.LogError("No trajectory loaded for this structure");
		}
	}

	/// <summary>
	/// Update positions of GameObject recorded in atomToGo and update representations with new positions
	/// </summary>
	public void updateRepresentations(bool trajectory = true) {

		UnityMolMain.getPrecompRepManager().Clear(uniqueName);

		if (trajectory) {

			//Update GameObject for each atom
			int idA = 0;
			// GameObject[] gos = atomToGo.Values.ToArray();
			foreach (UnityMolAtom a in currentModel.allAtoms) {
				if (float.IsNaN(trajAtomPositions[idA].x) || float.IsInfinity(trajAtomPositions[idA].x) ||
				        float.IsNaN(trajAtomPositions[idA].y) || float.IsInfinity(trajAtomPositions[idA].y) ||
				        float.IsNaN(trajAtomPositions[idA].z) || float.IsInfinity(trajAtomPositions[idA].z)) {
					// gos[idA].transform.localPosition = Vector3.zero;
					atomToGo[a].transform.localPosition = Vector3.zero;
				}
				else {
					// gos[idA].transform.localPosition = trajAtomPositions[idA];
					atomToGo[a].transform.localPosition = trajAtomPositions[idA];
				}
				idA++;
				//Stop if trajectory contains less atoms than the model
				if (idA >= trajAtomPositions.Length) {
					break;
				}
			}
			if (trajComputeSS) {
				DSSP.assignSS_DSSP(this);
				ssInfoFromFile = false;
			}

			UnityMolMain.getSelectionManager().updateSelectionContentTrajectory(this);

			for (int i = 0; i < representations.Count; i++) {
				UnityMolRepresentation rep = representations[i];
				rep.updateWithTrajectory();
			}

		}
		else {
			//Update GameObject for each atom
			foreach (UnityMolAtom a in currentModel.allAtoms) {
				atomToGo[a].transform.localPosition = a.position;
			}

			UnityMolMain.getSelectionManager().updateSelectionsWithNewModel(this);

			foreach (UnityMolSelection sel in UnityMolMain.getSelectionManager().selections.Values) {
				if (sel.structures.Contains(this)) {
					foreach (List<UnityMolRepresentation> reps in sel.representations.Values) {
						foreach (UnityMolRepresentation r in reps) {
							r.updateWithNewSelection(sel);
						}
					}
				}
			}
			// for (int i = 0; i < representations.Count; i++) {
			// 	UnityMolRepresentation rep = representations[i];
			// 	rep.updateWithModel();
			// }
		}

		UnityMolMain.getCustomRaycast().needsFullUpdate = true;



#if !DISABLE_HIGHLIGHT
		UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();
		hM.Clean();
#endif

	}


	/// <summary>
	/// Updates each atom position of the current model of the structure based on the trajectory
	/// Uses center of gravity from the first model of the structure !
	/// </summary>
	public void trajUpdateAtomPositions() {
		int N = Mathf.Min(currentModel.allAtoms.Count, trajAtomPositions.Length);
		UnityMolModel m = currentModel;
		for (int i = 0; i < N; i++) {
			m.allAtoms[i].position.x = trajAtomPositions[i].x;
			m.allAtoms[i].position.y = trajAtomPositions[i].y;
			m.allAtoms[i].position.z = trajAtomPositions[i].z;
		}
		currentModel.ComputeCenterOfGravity();
		UnityMolMain.getCustomRaycast().needsUpdatePos = true;

	}

	public void createTrajectoryPlayer() {
		if (trajPlayer == null) {
			UnityMolStructureManager sm = UnityMolMain.getStructureManager();
			GameObject structureParent = sm.GetStructureGameObject(uniqueName);

			if (structureParent == null) {
				GameObject loadedMolGO = UnityMolMain.getRepresentationParent();

				structureParent = loadedMolGO.transform.Find(ToSelectionName()).gameObject;
				if (structureParent == null) {
					structureParent = new GameObject(ToSelectionName());
					structureParent.transform.parent = loadedMolGO.transform;
					structureParent.transform.localPosition = Vector3.zero;
					structureParent.transform.localRotation = Quaternion.identity;
					structureParent.transform.localScale = Vector3.one;
				}
			}
			trajPlayer = structureParent.AddComponent<TrajectoryPlayer>();
			trajPlayer.play = true;
			trajPlayer.s = this;
		}
		else {
			Debug.LogWarning("Trajectory player already exists for this structure");
		}
	}

	/// <summary>
	/// Split different models read in a file into several UnityMolStructure
	/// </summary>
	public List<UnityMolStructure> splitModelsInStructures() {
		List<UnityMolStructure> result = new List<UnityMolStructure>();

		for (int i = 0; i < models.Count; i++) {
			UnityMolModel m = models[i];

			//TODO: Clone the model !
			UnityMolStructure newS = new UnityMolStructure(m, uniqueName + "_" + m.name);
			newS.models[0].structure = newS;

			newS.trajectoryLoaded = false;
			newS.ssInfoFromFile = ssInfoFromFile;

			result.Add(newS);
			UnityMolMain.getStructureManager().AddStructure(newS);
		}
		return result;
	}


	/// <summary>
	/// Find the corresponding atom from a model to a different model
	/// Returns null if not found
	/// </summary>
	public UnityMolAtom findAtomInModel(UnityMolModel newModel, UnityMolAtom prevAtom, bool strict = false) {

		if (!strict) {
			if (prevAtom.idInAllAtoms >= 0 && prevAtom.idInAllAtoms < newModel.allAtoms.Count) {
				return newModel.allAtoms[prevAtom.idInAllAtoms];
			}

			//Could not find the atom
			return null;
		}

		UnityMolResidue prevres = prevAtom.residue;
		UnityMolChain prevchain = prevres.chain;

		if (newModel.chains.ContainsKey(prevchain.name)) {
			//Look for the same residue
			int h = prevres.id;
			if (newModel.chains[prevchain.name].residues.ContainsKey(h)) {
				UnityMolResidue r = newModel.chains[prevchain.name].residues[h];
				if (r.atoms.ContainsKey(prevAtom.name)) {
					return r.atoms[prevAtom.name];
				}
			}

		}

		//Could not find the atom
		return null;
	}

	/// <summary>
	/// Outputs a UnityMolSelection of all the atoms of the structure, including the atoms of the different models
	/// </summary>
	public UnityMolSelection ToSelectionAll() {
		List<UnityMolAtom> selectedAtoms = models[0].allAtoms;
		for (int i = 1; i < models.Count; i++) {
			selectedAtoms.AddRange(models[i].allAtoms);
		}
		return new UnityMolSelection(selectedAtoms, "AllModels_" + uniqueName);
	}
	/// <summary>
	/// Outputs a UnityMolSelection of all the atoms of the current model of structure
	/// </summary>
	public UnityMolSelection ToSelection() {//Current model only
		List<UnityMolAtom> selectedAtoms = currentModel.allAtoms;
		string selString = uniqueName;
		UnityMolSelection sel = new UnityMolSelection(selectedAtoms, currentModel.bonds, ToSelectionName(), selString);
		sel.structures = new List<UnityMolStructure>();
		sel.structures.Add(this);
		sel.isAlterable = false;
		return sel;
	}

	public string ToSelectionName() {
		return "all(" + uniqueName + ")";
	}

	public string formatName(int length) {
		string nameS = uniqueName;

		if (nameS.Length > length) {
			nameS = nameS.Substring(0, Mathf.Min(length - 3, nameS.Length));
			nameS += "...";
		}
		return nameS;
	}

	/// <summary>
	/// Add an atom to the structure.
	/// Check if the model and the chain exists in the structure, check if the atom is not already existing
	/// </summary>
	public void AddAtom(UnityMolAtom toAdd, string modelName, string chainName) {
		UnityMolModel m = null;

		foreach (UnityMolModel mdl in models) {
			if (modelName == mdl.name) {
				m = mdl;
				break;
			}
		}
		if (m == null) { //Try to find the model based on its id
			try {
				int modelId = int.Parse(modelName);
				if (modelId >= 0 && modelId < models.Count) {
					m = models[modelId];
				}
			}
			catch { //Int parse failed
			}
		}

		if (m == null) {
			throw new System.Exception("Adding Atom Error, model not found (" + modelName + ")");
		}

		if (!m.chains.ContainsKey(chainName)) {
			throw new System.Exception("Adding Atom Error");
		}
		UnityMolChain c = m.chains[chainName];

		UnityMolResidue r = null;

		if (!c.residues.TryGetValue(toAdd.residue.id, out r)) {
			throw new System.Exception("Adding Atom Error");
		}

		if (r.atoms.ContainsKey(toAdd.name)) {
			throw new System.Exception("Adding Atom Already Existing Error (" + toAdd.name + ")");
		}
		toAdd.SetResidue(r);

		r.atoms[toAdd.name] = toAdd;
		toAdd.idInAllAtoms = m.allAtoms.Count;
		m.allAtoms.Add(toAdd);

		UnityMolMain.getCustomRaycast().needsFullUpdate = true;
		UnityMolMain.getPrecompRepManager().Clear(uniqueName);

		atomGos = null;
	}

	/// <summary>
	/// Merge a structure into the current one.
	/// The chain name is used to avoid conflicts between existing residues/atoms of the current structure and the merged one
	/// Delete the structure to be merged and all its representation
	/// </summary>
	public void MergeStructure(UnityMolStructure tobeMerged, string newChainName) {

		UnityMolChain[] chains = tobeMerged.currentModel.chains.Values.ToArray();
		foreach (UnityMolChain c in chains) {
			string chainName = newChainName;
			while (currentModel.chains.ContainsKey(chainName)) { //Chain already exists
				chainName = findNewChainName(chainName);
			}
			//Modify the chain to be integrated in the current UnityMolStructure
			c.name = chainName;
			c.model = currentModel;

			currentModel.chains[chainName] = c;

			foreach (UnityMolResidue r in c.residues.Values) {
				//Modify idInAllAtoms
				foreach (UnityMolAtom a in r.atoms.Values) {
					a.idInAllAtoms = currentModel.allAtoms.Count;
					currentModel.allAtoms.Add(a);
				}
			}
		}

		//Fill bonds
		foreach (UnityMolAtom a in tobeMerged.currentModel.bonds.bonds.Keys) {
			foreach (UnityMolAtom b in tobeMerged.currentModel.bonds.bonds[a]) {
				if (b != null) {
					currentModel.bonds.Add(a, b);
				}
			}
		}

		currentModel.ComputeCenterOfGravity();
		currentModel.fillIdAtoms();


		//Need to create colliders for newly added atoms
		Reader.CreateColliders(new UnityMolSelection(tobeMerged.currentModel.allAtoms, newBonds: null, ToSelectionName(), uniqueName));

		UnityMolMain.getStructureManager().Delete(tobeMerged);

		UnityMolMain.getCustomRaycast().needsFullUpdate = true;
		UnityMolMain.getPrecompRepManager().Clear(uniqueName);
	}
	private string findNewChainName(string name) {
		string result = name;
		if (name.Length == 1) {
			char tmp = name[0];
			tmp++;
			result = tmp.ToString();
		}
		else {
			int toAdd = 2;
			result = name + toAdd.ToString();
			while (currentModel.chains.ContainsKey(result)) {
				toAdd++;
				result = name + toAdd.ToString();
			}
		}

		return result;
	}

	public Vector3[] getColliderPositions() {

		if (atomGos == null) {
			atomGos = atomToGo.Values.ToArray();
		}
		Vector3[] collidersPos = new Vector3[Count];

		GameObject loadedMolGO = UnityMolMain.getRepresentationParent();
		Transform repParent = loadedMolGO.transform;

		for (int i = 0; i < Count; i++) {
			Vector3 localPos = repParent.InverseTransformPoint(atomGos[i].transform.position);

			collidersPos[i] = localPos;
		}

		return collidersPos;
	}

	public GameObject[] getAtomGos() {
		if (atomGos == null) {
			atomGos = atomToGo.Values.ToArray();
		}
		return atomGos;
	}

	public void disconnectIMD() {

		if (artemisM == null)
			return;
		artemisM.disconnect();
		GameObject.DestroyImmediate(artemisM);

		artemisM = null;

		for (int i = 0; i < currentModel.allAtoms.Count; i++) {
			currentModel.allAtoms[i].position = currentModel.allAtoms[i].oriPosition;
		}
		currentModel.ComputeCenterOfGravity();

		updateRepresentations(trajectory: false);

		if (trajAtomPositions != null) {
			trajAtomPositions = null;
		}
	}

	public void OnDestroy() {
		if (modelFrames != null) {
			modelFrames.Clear();
		}
		disconnectIMD();
		unloadTrajectoryXDR();
		if (surfThread != null) {
			surfThread.Clear();
		}
		UnityMolMain.getPrecompRepManager().Clear(uniqueName);
	}
}
}