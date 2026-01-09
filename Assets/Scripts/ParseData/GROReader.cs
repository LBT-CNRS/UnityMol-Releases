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
// New classes to handle GRO format
// Xavier Martinez
// Hubert Santuz
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UMol {

/// <summary>
/// Creates a UnityMolStructure object from a local GRO file
/// File Format: https://manual.gromacs.org/current/reference-manual/file-formats.html#gro
/// </summary>
public class GROReader: Reader {

    /// <summary>
    /// List of possible GRO extensions
    /// </summary>
    public static readonly string[] GROextensions = { "gro" };

    /// <summary>
    /// Create a GROReader with an optional filename
    /// </summary>
    public GROReader(string fileName = "") : base(fileName) { }

    /// <summary>
    /// Parse a GRO file to a UnityMolStructure object
    /// </summary>
    protected override UnityMolStructure ReadData(StreamReader sr, bool readHET = true, bool readWater = true,
                                                  bool simplyParse = false,
                                                  UnityMolStructure.MolecularType? forceStructureType = null) {


        float start = Time.realtimeSinceStartup;

        List<UnityMolModel> models = new List<UnityMolModel>();
        List<UnityMolChain> chains = new List<UnityMolChain>();
        List<UnityMolResidue> residues = new List<UnityMolResidue>();
        List<UnityMolAtom> atomsinResidue = new List<UnityMolAtom>();
        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();
        List<Vector3[]> frames = new List<Vector3[]>();

        // Read GRO frames as a trajectory no matter what
        ModelsAsTraj = true;

        // Temporary variables
        StringBuilder sbtrim = new StringBuilder(10);
        string atomLine = "";
        int lastResidueid = -1;
        string lastResidueName = "";
        int privateResidueNumber = 0;

        int lastAtomSerial = -1;
        int atomSerialOffset = 0; // Modified when atom serial > 99999

        Vector3 boxFirstFrame = Vector3.zero;
        Vector3 box = Vector3.zero;
        // Atoms coordinates of one frame
        //use a list the first time as the number of atoms is not known
        List<Vector3> firstFrameAtomsPositions = new List<Vector3>();


        bool endFile = false; //For reading multiple frames

        using (sr) {
            // Read the first frame to parse the atoms

            //Check header and exit if failing
            int numberOfAtoms = readHeader(sr);
            if (numberOfAtoms == -1) {
                return null;
            }

            for (int i = 0; i < numberOfAtoms; i++) {
                try {
                    atomLine = sr.ReadLine();

                    int resId = ParseInt(atomLine, 0, 5);//int.Parse(line.Substring(0, 5).Trim());
                    string resName = SubstringWithTrim(sbtrim, atomLine, 5, 5);//line.Substring(5, 5).Trim();
                    string atomName = SubstringWithTrim(sbtrim, atomLine, 10, 5);//line.Substring(10, 5).Trim();
                    int atomSerial = ParseInt(atomLine, 15, 15 + 5); //int.Parse(line.Substring(15, 5).Trim());

                    // Ignore water if requested
                    if (!readWater && WaterSelection.waterResidues.Contains(resName, StringComparer.OrdinalIgnoreCase)) {
                        continue;
                    }

                    string atomType = PDBReader.GuessElementFromAtomName(atomName, resName, false);

                    // Coordinates
                    TryParseFloatFast(atomLine, 20, 20 + 8, out float posx);
                    TryParseFloatFast(atomLine, 28, 28 + 8, out float posy);
                    TryParseFloatFast(atomLine, 36, 36 + 8, out float posz);
                    // Unity has X inverted
                    posx = -posx;

                    //GRO file format unit is nm not Angstrom
                    Vector3 coord = new Vector3(posx, posy, posz) * 10.0f;
                    firstFrameAtomsPositions.Add(coord);

                    // New residue
                    if (atomsinResidue.Count != 0 && lastResidueid != resId) {
                        residues.Add(new UnityMolResidue(privateResidueNumber, lastResidueid, atomsinResidue, lastResidueName));
                        privateResidueNumber++;
                        // Add a UnityMolResidue reference to atoms of the residue
                        foreach (UnityMolAtom t in atomsinResidue) {
                            t.SetResidue(residues.Last());
                        }
                        atomsinResidue.Clear();
                    }

                    // GRO file supports atoms serial up to 99999
                    if (lastAtomSerial == 99999) {
                        atomSerialOffset += 100000;
                    }
                    int modifAtomSerial = atomSerialOffset + atomSerial;

                    float bfactor = 0.0f;
                    UnityMolAtom newAtom = new UnityMolAtom(atomName, atomType, coord, bfactor, modifAtomSerial);
                    atomsinResidue.Add(newAtom);
                    allAtoms.Add(newAtom);

                    lastResidueid = resId;
                    lastResidueName = resName;
                    lastAtomSerial = atomSerial;
                }
                catch {
                    Debug.LogWarning("Fail to parse line:\n" + atomLine);
                    Debug.LogWarning("Exiting...");
                    return null;
                }
            }

            // Record last residue
            if (atomsinResidue.Count > 0) {
                residues.Add(new UnityMolResidue(privateResidueNumber, lastResidueid, atomsinResidue, lastResidueName));
                privateResidueNumber++;
                foreach (UnityMolAtom a in atomsinResidue) {
                    a.SetResidue(residues.Last());
                }
                atomsinResidue.Clear();
            }

            //GRO file does not handle chain. Everything belong to one chain
            chains.Add(new UnityMolChain(residues, "A"));

            //Check we read the correct number of atoms
            if (numberOfAtoms != allAtoms.Count()) {
                Debug.LogWarning("Mismatch between the number of atoms stated in the file (" + numberOfAtoms + ")" +
                                 "and the number of atoms parsed (" + allAtoms.Count() + ").");
                return null;
            }

            //Record the model
            UnityMolModel model = new UnityMolModel(chains, "Model");
            model.allAtoms.AddRange(allAtoms);
            models.Add(model);
            frames.Add(firstFrameAtomsPositions.ToArray());

            allAtoms.Clear();
            chains.Clear();

            // try to Read box line
            string newline = sr.ReadLine();
            if (newline != null) {
                boxFirstFrame = parseBoxLine(newline);
            }
            else {
                Debug.Log("No box line found.");
                endFile = true;
            }
            //End of the first frame

            // Loop until the end of file to parse frames if present
            int numberOfFrames = 1; //The first frame was parsed above
            while (!endFile) {
                endFile = true;

                //Try to read header of next frame
                string titleLine = sr.ReadLine();

                if (titleLine != null) //stream still has lines
                {
                    // new frame detected.
                    numberOfFrames++;

                    //Try to parse the atom line of the next line
                    int numberOfAtomsNextFrame;
                    try {
                        numberOfAtomsNextFrame = int.Parse(sr.ReadLine());
                    }
                    catch {
                        Debug.LogWarning("Could not parse the line containing the number of atoms " +
                                        "of frame " + numberOfFrames + ". Exiting..");
                        return null;
                    }

                    if (numberOfAtomsNextFrame != numberOfAtoms) {
                        Debug.LogWarning("The frame number " + numberOfFrames + "does not have the same number " +
                                            "of atoms (" + numberOfAtomsNextFrame + ") than the first one (" +
                                            numberOfAtoms + "). Exiting..");
                        return null;
                    }

                    // Parse only the atom coordinates
                    Vector3[] currentFrame = new Vector3[numberOfAtoms];
                    try {
                        parseAtomsCoordinates(sr, ref currentFrame);
                    } catch (Exception e)
                    {
                        Debug.LogWarning("Parser failed while parsing coordinates at line" + newline + "\n (" + e.Message + ")");
                        return null;
                    }

                    frames.Add(currentFrame);
                    endFile = false;
                    // Read box line of the frame
                    box = parseBoxLine(sr.ReadLine());
                    if (box == Vector3.zero) {
                        Debug.LogWarning("Could not parse the line containing the box " +
                                        "of frame " + numberOfFrames + ". Exiting..");
                        return null;
                    }
                }

            } //end of while

        } // end of parsing

        // Create the UnityMolStructure
        UnityMolStructure newStruct;
        if (frames.Count > 1) {
            newStruct = new UnityMolStructure(models, FileNameWithoutExtension, frames);
        }
        else {
            newStruct = new UnityMolStructure(models, FileNameWithoutExtension);
        }

        //Defines its molecular type
        if (forceStructureType.HasValue) {
            newStruct.structureType = forceStructureType.Value;
        }
        else {
            newStruct.SetStructureMolecularType();
        }

        if (newStruct.structureType != UnityMolStructure.MolecularType.standard) {
            newStruct.updateAtomRepValues();
        }

        // Add a reference to the structure for each model
        for (int i = 0; i < models.Count; i++) {
            newStruct.models[i].structure = newStruct;
        }

        newStruct.periodic = boxFirstFrame;


        if (!simplyParse) {
            for (int i = 0; i < models.Count; i++) {
                newStruct.models[i].fillIdAtoms();
                newStruct.models[i].bonds = ComputeUnityMolBonds.ComputeBondsByResidue(models[i].allAtoms);
                newStruct.models[i].ComputeCentroid();
            }

            UnityMolSelection sel = newStruct.ToSelection();

            if (newStruct.models.Count != 1) {
                for (int i = 1; i < newStruct.models.Count; i++) {
                    CreateUnityObjects(newStruct.ToSelectionName(), new UnityMolSelection(newStruct.models[i].allAtoms, newBonds: null, sel.name, newStruct.name));
                }
            }
            CreateUnityObjects(newStruct.ToSelectionName(), sel);
            newStruct.surfThread = StartSurfaceThread(sel);

            UnityMolMain.getStructureManager().AddStructure(newStruct);
            UnityMolMain.getSelectionManager().Add(sel);
        }

#if UNITY_EDITOR
        Debug.Log("Time for parsing: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");
#endif
        return newStruct;

    }

    /// <summary>
    /// Read the header of a GRO file and return the number of atoms (-1 if failed)
    /// The header is composed of 2 lines : one title line and one with the number of atoms
    /// The header is present before each frame in cas of a trajectory GRO file.
    /// </summary>
    /// <param name="sr">the StreamReader holding the file</param>
    /// <returns>the number of atoms read. -1 if failed</returns>
    private static int readHeader(StreamReader sr) {
        int nAtoms = -1;
        // Read title line and ignore it
        _ = sr.ReadLine();

        //Read number of atoms line
        try {
            nAtoms = int.Parse(sr.ReadLine() ?? string.Empty);
        }
        catch {
            Debug.LogWarning("Could not parse the line containing the number of atoms. Exiting..");
        }

        return nAtoms;
    }


    /// <summary>
    /// Parse a box line and return its vector
    /// unit cell line (from http://manual.gromacs.org/current/online/gro.html)
    /// v1(x) v2(y) v3(z) v1(y) v1(z) v2(x) v2(z) v3(x) v3(y)
    /// Last 6 vectors are not mandatory
    /// </summary>
    /// <remarks>
    /// Only the first 3 values are stored (even if 9 values are read)
    /// TODO: Improve the case for 9 values
    /// </remarks>
    /// <param name="line">line to parse</param>
    /// <returns>the box as a Vector3</returns>
    private static Vector3 parseBoxLine(string line) {
        Vector3 box = Vector3.zero;

        try {
            string[] array = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            float[] floatData = Array.ConvertAll(array, s => float.Parse(s, CultureInfo.InvariantCulture));
            if (floatData.Count() == 3 || floatData.Count() == 9) {
                box.Set(floatData[0], floatData[1], floatData[2]);
            }
        } catch {
            // ignored
        }

        return box;
    }

    /// <summary>
    /// Parse only the atom coordinates of one GRO frame contained in the StreamReader 'sr'
    /// Fill the 'coordinates' array parameter with the new values.
    /// Assumes than the number of lines read and the size of array matches.
    /// </summary>
    /// <param name="sr">the streamreader holding the content</param>
    /// <param name="coordinates">the array of coordinates to save the new coordinates</param>
    private static void parseAtomsCoordinates(StreamReader sr, ref Vector3[] coordinates) {
        if (coordinates == null) {
            return;
        }

        for (int i = 0; i < coordinates.Count(); i++) {
            string atomLine = sr.ReadLine();
            // Coordinates
            TryParseFloatFast(atomLine, 20, 20 + 8, out float posx);
            TryParseFloatFast(atomLine, 28, 28 + 8, out float posy);
            TryParseFloatFast(atomLine, 36, 36 + 8, out float posz);
            // Unity has X inverted
            posx = -posx;
            //GRO file format unit is nm not Angstrom
            Vector3 coord = new Vector3(posx, posy, posz) * 10.0f;

            coordinates[i] = coord;
        }

    }

    /// <summary>
    /// GRO writer
    /// Uses a structure and outputs a string containing the first model
    /// </summary>
    public static string Write(UnityMolStructure structure) {
        UnityMolModel m = structure.models[0];
        return Write(m.ToSelection());
    }

    /// <summary>
    /// GRO writer
    /// Uses a selection to output a string containing the first model
    /// </summary>
    public static string Write(UnityMolSelection select, bool writeHET = true, Vector3[] overridedPos = null) {
        if (overridedPos != null && select.atoms.Count != overridedPos.Length) {
            Debug.LogError("Size of the overridedPos list does not match the number of atoms in the selections");
            return "";
        }

        if (select.structures.Count > 1) {
            Debug.LogError("Only supports selections with one structure");
            return "";
        }

        int count = select.atoms.Count;

        if (!writeHET) {
            count += select.atoms.Count(t => !t.isHET);
        }

        StringBuilder sw = new StringBuilder();
        sw.Append(select.name.Replace("t=", "").ToUpper());
        sw.Append("\n");
        sw.AppendFormat("{0,5}", count);
        sw.Append("\n");

        int atomSerial = 0;

        for (int i = 0; i < select.atoms.Count; i++) {
            UnityMolAtom a = select.atoms[i];
            if (a.isHET && !writeHET) {
                continue;
            }
            int serial = atomSerial;
            if (atomSerial > 99999) {
                serial = atomSerial % 99999;
            }
            int resid = a.residue.id;

            if (resid > 9999) {
                resid = resid % 9999;
            }

            float x = -1 * a.oriPosition.x * 0.1f; // Revert to right-handed
            float y = a.oriPosition.y * 0.1f;
            float z = a.oriPosition.z * 0.1f;

            if (overridedPos != null) {
                x = -1 * overridedPos[i].x * 0.1f;
                y = overridedPos[i].y * 0.1f;
                z = overridedPos[i].z * 0.1f;
            }

            sw.AppendFormat(CultureInfo.InvariantCulture, "{0,5}", resid);
            sw.AppendFormat(CultureInfo.InvariantCulture, "{0,-5}", a.residue.name);
            sw.AppendFormat(CultureInfo.InvariantCulture, "{0,5}", a.name);
            sw.AppendFormat(CultureInfo.InvariantCulture, "{0,5}", serial + 1);

            int decX = (int)(1000 * (x - (int)x));
            int decY = (int)(1000 * (y - (int)y));
            int decZ = (int)(1000 * (z - (int)z));

            sw.AppendFormat(CultureInfo.InvariantCulture, "{0,4}.{1,-3}", (int)x, decX);
            sw.AppendFormat(CultureInfo.InvariantCulture, "{0,4}.{1,-3}", (int)y, decY);
            sw.AppendFormat(CultureInfo.InvariantCulture, "{0,4}.{1,-3}", (int)z, decZ);


            sw.Append("\n");

            atomSerial++;
        }
        sw.Append("   1.00000   1.00000   1.00000\n\n");
        return sw.ToString();
    }

}

}
