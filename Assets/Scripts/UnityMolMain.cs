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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UMol {
public class UnityMolMain {
	public static string version = "1.0.36";

	public static UnityMolTopologyDef topologies = new UnityMolTopologyDef();

	public static UnityMolDefaultColors atomColors = new UnityMolDefaultColors();

	private static UnityMolStructureManager structureManager = new UnityMolStructureManager();
	private static UnityMolRepresentationManager representationManager = new UnityMolRepresentationManager();
	private static UnityMolSelectionManager selectionManager = new UnityMolSelectionManager();

	private static PrecomputedRepresentationManager precompRepManager = new PrecomputedRepresentationManager();

#if !DISABLE_HIGHLIGHT
	private static UnityMolHighlightManager highlightManager = new UnityMolHighlightManager();
#endif

	private static UnityMolAnnotationManager annotationManager;

	public static MeasureMode measureMode = MeasureMode.distance;

	public static ObservedList<string> pythonCommands = new ObservedList<string>();
	public static ObservedList<string> pythonUndoCommands = new ObservedList<string>();
	public static List<string> NPrevCommands = new List<string>();
	public static int NRestoreCommands = 20;

	private static GameObject loadedMolGO;

	private static CustomRaycastBurst raycaster;

	public static string APBSInstallPath = "";

	public static bool isFogOn = false;
	public static float fogStart = 0.0f;
	public static float fogDensity = 0.5f;

	public delegate void OnNewCommand(CommandEventArgs args);
	public static event OnNewCommand onNewCommand;

	public static string getVersionString() {
		return "Release " + version;
	}
	public static bool inVR() {
		return XRSettings.enabled && (XRSettings.loadedDeviceName != null
		                              && !XRSettings.loadedDeviceName.StartsWith("stereo"))
		       && VRTK.VRTK_SDKManager.instance != null;
	}

	public static GameObject getRepresentationParent() {
		if (loadedMolGO == null) {
			loadedMolGO = GameObject.Find("LoadedMolecules");
		}
		if (loadedMolGO == null) {
			loadedMolGO = new GameObject("LoadedMolecules");
		}
		return loadedMolGO;
	}

	public static CustomRaycastBurst getCustomRaycast() {
		if (raycaster == null) {
			raycaster = new CustomRaycastBurst();
		}
		return raycaster;
	}

	public static UnityMolStructureManager getStructureManager() {
		return structureManager;
	}

	public static UnityMolRepresentationManager getRepresentationManager() {
		return representationManager;
	}

	public static UnityMolSelectionManager getSelectionManager() {
		return selectionManager;
	}

	public static PrecomputedRepresentationManager getPrecompRepManager() {
		return precompRepManager;
	}

#if !DISABLE_HIGHLIGHT
	public static UnityMolHighlightManager getHighlightManager() {
		return highlightManager;
	}
#endif

	public static UnityMolAnnotationManager getAnnotationManager() {
		if (annotationManager == null) {
			GameObject annoManaGo = new GameObject("AnnotationManager");
			annotationManager = annoManaGo.AddComponent<UnityMolAnnotationManager>();
		}
		return annotationManager;
	}

	public static void recordPythonCommand(string command) {
		if (onNewCommand != null) {
			onNewCommand(new CommandEventArgs(command));
		}
		pythonCommands.Add(command);
		addCommandToUserPref(command);
	}
	public static void recordUndoPythonCommand(string command) {
		pythonUndoCommands.Add(command);
	}

	static void addCommandToUserPref(string command) {
		NPrevCommands.Add(command);

		int Ncomm = Mathf.Min(NRestoreCommands, NPrevCommands.Count);
		PlayerPrefs.SetInt("NRestoreCommands", Ncomm);
		for (int i = 0; i < Ncomm; i++) {
			PlayerPrefs.SetString("lastcommand" + i, NPrevCommands[i]);

		}
	}
	public static string commandHistory() {
		StringBuilder sb = new StringBuilder();
		foreach (string s in pythonCommands) {
			sb.Append(s);
			sb.Append("\n");
		}
		return sb.ToString();
	}
}

public class CommandEventArgs : System.EventArgs {
	public CommandEventArgs(string com) {
		this.command = com;
	}
	public string command {get; private set;}
}

public class NewSelEventArgs : System.EventArgs {
	public NewSelEventArgs(UnityMolSelection s) {
		this.sel = s;
	}
	public UnityMolSelection sel {get; private set;}
}

}
