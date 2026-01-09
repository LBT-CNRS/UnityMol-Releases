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
using System.Collections.Generic;
using System.IO;
using UMol.API;

namespace UMol {

/// <summary>
/// Handle the command line arguments for the UnityMol executable
/// </summary>
public class ReadCommandLineArguments: MonoBehaviour {

	/// <summary>
	/// Return the value of the argument passed in 'name' as a list of strings.
	/// For example, return 'myfile.pdb toto.pdb' for an argument like '-i myfile.pdb toto.pdb'
	/// </summary>
	/// <param name="name">name of the argument</param>
	/// <returns>list of values of the argument</returns>
	private static List<string> getArgs(string name) {

		string[] args = System.Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length; i++) {
			if (args[i] == name && args.Length > i + 1) {
				List<string> res = new List<string>();
				for (int j = i + 1; j < args.Length; j++) {
					if (args[j].StartsWith("-")) {
						break;
					}
					res.Add(args[j]);
				}
				return res;
			}
		}
		return new List<string>();
	}

	/// <summary>
	/// Return the value of the argument passed in 'name' as a string.
	/// For example, return 'myfile.pdb' for an argument like '-i myfile.pdb'
	/// </summary>
	/// <param name="name">name of the argument</param>
	/// <returns>value of the argument</returns>
	private static string getArg(string name) {

		string[] args = System.Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length; i++) {
			if (args[i] == name && args.Length > i + 1) {
				return args[i + 1];
			}
		}
		return "";
	}

	/// <summary>
	/// Check if the argument supplied in 'name' is found in the command line
	/// </summary>
	/// <param name="name">name of the argument</param>
	/// <returns>True if found. False otherwise</returns>
	private static bool isArgPresent(string name) {

		string[] args = System.Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length; i++) {
			if (args[i] == name) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Start method
	/// </summary>
	public void Start() {
		List<string> filePaths = getArgs("-i");
		string directory = getArg("-d");

		if (isArgPresent("-v") || isArgPresent("--version")) {
			APIPython.getVersion();
		}

        if (isArgPresent("-r") || isArgPresent("--remoteControl")) {
            APIPython.activateExternalCommands();
        }

        if (isArgPresent("-c") || isArgPresent("--commons")) {
            APIPython.loadPythonCommonsModule();
        }

		// Change directory
		if (!string.IsNullOrEmpty(directory)) {
			Debug.Log("Setting directory to '" + directory + "'");
			APIPython.cd(Path.GetFullPath(directory));
		}

		//Parse the different files supplied
		foreach (string p in filePaths) {
			if (PythonUtils.IsPythonFile(p)) {
				APIPython.loadHistoryScript(p);
			}
			else if (p.Length == 4 && !p.Contains(".")) {
				APIPython.fetch(p);
			}
			else {
				if (p.EndsWith(".xtc") && APIPython.last() != null) {
					string lastStructureName = APIPython.last().name;
					if (lastStructureName != null) {
						APIPython.loadTraj(lastStructureName, p);
					}
				}
				else if (p.EndsWith(".dx") && APIPython.last() != null) {
					string lastStructureName = APIPython.last().name;
					if (lastStructureName != null) {
						APIPython.loadDXmap(lastStructureName, p);
					}
				}
				else if (p.EndsWith(".itp") && APIPython.last() != null) {
					string lastStructureName = APIPython.last().name;
					if (lastStructureName != null) {
						APIPython.loadMartiniITP(lastStructureName, p);
					}
				}
				else if (p.EndsWith(".psf") && APIPython.last() != null) {
					string lastStructureName = APIPython.last().name;
					if (lastStructureName != null) {
						APIPython.loadPSFTopology(lastStructureName, p);
					}
				}
				else if (p.EndsWith(".top") && APIPython.last() != null) {
					string lastStructureName = APIPython.last().name;
					if (lastStructureName != null) {
						APIPython.loadTOPTopology(lastStructureName, p);
					}
				}
				else {
					APIPython.load(p);
				}

			}
		}


	}
}
}
