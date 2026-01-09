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
//
// Manager to load/select force fields from UI
// Joao Rodrigues (j.p.g.l.m.rodrigues@gmail.com)
//

// Unity imports
using UnityEngine;

// C# imports
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using MiniJSON;

namespace UMol {
namespace ForceFields {

/// <summary>
/// Manage Forcefields for Docking mode
/// </summary>
public class ForceFieldsManager  {

    /// <summary>
    /// Forcefield currently activated
    /// </summary>
    public ForceField ActiveForceField;

    /// <summary>
    /// List of ForceField objets
    /// </summary>
    private static readonly List<ForceField> ffList = new();

    /// <summary>
    /// List of forcefield names
    /// </summary>
    private static readonly List<string> ffNameList = new();

    /// <summary>
    /// Construct the ForceFieldManager by parsing Forcefield files in ForceFieldfolder folder
    /// in StreamingAssets
    /// </summary>
    /// <param name="forcefieldfolder">name of the forcefield folder in StreamingAssets,
    /// by default "ForceFields".</param>
    public ForceFieldsManager(string forcefieldfolder="ForceFields") {

        // Scan FF folder for .json files and try parsing them
        string forceFieldsPath = Application.streamingAssetsPath + "/" + forcefieldfolder;

        if (Application.platform == RuntimePlatform.Android)
        {
            string fileName = Path.Combine(forceFieldsPath, "ff14SB.json");

            ForceField ff = loadForceField(fileName);
            ffList.Add(ff);
            ffNameList.Add(ff.name);
        }
        else
        {

            DirectoryInfo dir = new(forceFieldsPath);
            FileInfo[] info = dir.GetFiles("*.json");
            foreach (FileInfo f in info) {
                string fileName = f.FullName;
                try {
                    ForceField ff = loadForceField(fileName);
                    ffList.Add(ff);
                    ffNameList.Add(ff.name);
                    Debug.LogFormat("Loaded ForceField file : {0} ", fileName);
                }
                catch {
                    Debug.LogFormat("Could not parse force field JSON file: {0}", fileName);
                }
            }
        }
        activateForceField("ff14SB");
    }

    /// <summary>
    /// Parse a forcefield file and return an ForceField object
    /// </summary>
    /// <param name="fileName">ForceField filename</param>
    /// <returns>ForceField object</returns>
    private static ForceField loadForceField(string fileName) {

        IDictionary deserializedData;

        StreamReader sr;

        if (Application.platform == RuntimePlatform.Android) {
            StringReaderStream textStream = new(AndroidUtils.GetFileText(fileName));
            sr = new StreamReader(textStream);
        }
        else {
            sr = new StreamReader(fileName);
        }

        using (sr) {
            string jsonString = sr.ReadToEnd();
            deserializedData = (IDictionary) Json.Deserialize(jsonString);
        }

        string name = Path.GetFileNameWithoutExtension(fileName);
        ForceField ff = new(name);

        // Read atom types first
        IDictionary atomTypeLibraryJSON = (IDictionary)deserializedData["atom_types"];
        foreach (string atomTypeName in atomTypeLibraryJSON.Keys) {
            FFAtomType newAtomType = new(atomTypeName);
            IDictionary typeParams = (IDictionary)atomTypeLibraryJSON[atomTypeName];

            newAtomType.eps = float.Parse(typeParams["epsilon"].ToString(), System.Globalization.CultureInfo.InvariantCulture);
            newAtomType.rmin = float.Parse(typeParams["rmin"].ToString(), System.Globalization.CultureInfo.InvariantCulture);

            ff.AddAtomType(newAtomType);
        }

        // Read residue library
        IDictionary residueLibraryJSON = (IDictionary)deserializedData["residues"];

        foreach (string residueName in residueLibraryJSON.Keys) {
            FFResidue newResidue = new(residueName);
            IDictionary atoms = (IDictionary)residueLibraryJSON[residueName];

            foreach (string atomName in atoms.Keys) {
                IDictionary thisAtom = (IDictionary)atoms[atomName];

                FFAtom newAtom = new(residueName, atomName, (string)thisAtom["type"]) {
                    charge = float.Parse(thisAtom["charge"].ToString(), System.Globalization.CultureInfo.InvariantCulture)
                };

                newResidue.AddAtom(newAtom);
            }
            ff.AddResidue(newResidue);
        }
        return ff;
    }

    /// <summary>
    /// Activate the forcefield whose name is the parameter
    /// </summary>
    /// <param name="ffName">name of the forcefield to activate</param>
    /// <exception cref="Exception">if the forcefield name is not found</exception>
    private void activateForceField(string ffName) {

        if (!ffNameList.Contains(ffName)) {
            string message = "Force field '{0}' is not available. Pick from {1}";
            message = string.Format(message, ffName, String.Join(", ", ffNameList.ToArray()));

            Debug.LogFormat(message);
            throw new Exception(message);
        }

        int idx = ffNameList.IndexOf(ffName);
        ActiveForceField = ffList[idx];
    }

}
}
}
