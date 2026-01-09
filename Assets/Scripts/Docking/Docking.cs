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
// Functions used during real-time interactive docking

namespace UMol {
namespace Docking {

// Unity imports
using UnityEngine;

// C# imports
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Text;

using ForceFields;

// Energy terms
// Define a struct to hold terms calculated by this module.
// Make sure names match those in the structure if you want transferrability
public struct Energy {
    public float elec;
    public float vdw;

    public Energy(float _e, float _v) {
        elec = _e;
        vdw = _v;
    }
}

//
// Threaded Class to avoid blocking UI
//
public class DockingNBEnergyThread: MonoBehaviour {

    // Modify on every frame/update
    public Vector3[] colliderPositions;

    // Pass *once* before Start()
    public List<UnityMolStructure> structures;

    private List<string> atomsFFTypeList;
    private List<float> atomFFChargeList;
    private List<float> atomEpsList;
    private List<float> atomRminList;
    //

    // Return values
    public Energy nbEnergies;
    //

    public bool isSetup = false;
    public bool _isRunning;
    public bool _isDone = true;
    public bool _isPause = true;
    private Thread _thread;
    private NBEnergetics energyHandler;

    private System.Diagnostics.Stopwatch watch;

    // Create new Thread
    public void Start() {
        _thread = new Thread(CalculateNBEnergy);
        _thread.Start();
    }


    void CalculateNBEnergy() {

        _isRunning = true;
        while (_isRunning ) {
            if (!_isPause && !_isDone) {
                if (colliderPositions == null) {
                    continue;
                }

                // watch = System.Diagnostics.Stopwatch.StartNew();

                energyHandler.atomCoords = colliderPositions;

                energyHandler.Calculate();
                nbEnergies = energyHandler.nbEnergies;
                _isDone = true;

                // watch.Stop();
                // double elapsedUs = 1000000.0 * (double)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
                // Debug.LogFormat("NBEnergy: LJ(6-12) = {0} ; Coul = {1} ({2:N2} us)", nbEnergies.vdw, nbEnergies.elec, elapsedUs.ToString("F2"));
            }
        }
    }

    // Used to setup calculations.
    // Call *after* passing data but *before* Start()
    public void Setup() {

        //
        int[] atomsPerChain;
        int[] cumulAtomsPerChain;
        dividePerChain(structures,
                       out cumulAtomsPerChain, out atomsPerChain);

        getFFValuesForStructures(structures);

        int[] uniqueTypeIndices;
        Vector2[] paramArray;
        int numUTypes;
        BuildLJParameterTable(atomsFFTypeList, atomEpsList, atomRminList,
                              out uniqueTypeIndices, out paramArray, out numUTypes);

        // Initiate energyHandler
        energyHandler = new NBEnergetics();

        energyHandler.nbEnergies = nbEnergies;

        energyHandler.sizeOfPartition = cumulAtomsPerChain;
        energyHandler.atomPartitions = atomsPerChain;

        energyHandler.atomFFCharge = atomFFChargeList.ToArray();
        energyHandler.uniqueTypeIndices = uniqueTypeIndices;
        energyHandler.paramArray = paramArray;
        energyHandler.numUTypes = numUTypes;

        energyHandler.atomCoords = colliderPositions;

        energyHandler.SetImplementation();

        energyHandler.Calculate();
        nbEnergies = energyHandler.nbEnergies;
        _isDone = true;
        Debug.LogFormat("Initial NBEnergy: LJ(6-12) = {0} ; Coul = {1}", nbEnergies.vdw, nbEnergies.elec);

        isSetup = true;
    }

    // Helper method to divide a list of atoms into a [] array
    // with atoms per chain.
    private void dividePerChain(List<UnityMolStructure> structures, out int[] cumulAtomsPerChain, out int[] atomsPerChain) {
        HashSet<UnityMolChain> uniqueChains = new HashSet<UnityMolChain>();

        foreach (UnityMolStructure s in structures) {
            foreach (UnityMolChain c in s.currentModel.chains.Values) {
                uniqueChains.Add(c);
            }
        }
        int numChains = uniqueChains.Count;
        cumulAtomsPerChain = new int[numChains + 1];

        List<int> tempAtomList = new List<int>();

        int tempCountAtoms = 0;
        cumulAtomsPerChain[0] = 0;
        int cpt = 0;
        int chainIndex = 0;
        foreach (UnityMolChain c in uniqueChains) {

            int countChain = c.Count;
            tempCountAtoms += countChain;

            cumulAtomsPerChain[chainIndex + 1] = tempCountAtoms;
            for (int j = 0; j < countChain; j++) {
                tempAtomList.Add(cpt);
                cpt++;
            }
            chainIndex++;
        }

        atomsPerChain = tempAtomList.ToArray();
    }


    private void getFFValuesForStructures(List<UnityMolStructure> structures) {
        atomsFFTypeList = new List<string>();
        atomFFChargeList = new List<float>();
        atomEpsList = new List<float>();
        atomRminList = new List<float>();

        ForceFieldsManager ffm = UnityMolMain.getForceFieldsManager();
        ForceField activeFF = ffm.ActiveForceField;

        string curType = "undefined";
        float curCharge = 0.0f;
        float curEps = 0.0f;
        float curRmin = 0.0f;

        int countUndefined = 0;
        StringBuilder sb = new StringBuilder();
        foreach (UnityMolStructure s in structures) {
            foreach (UnityMolChain c in s.currentModel.chains.Values) {
                foreach (UnityMolResidue r in c.residues) {
                    //Reside is known in the force field
                    if (activeFF.ContainsResidue(r.name)) {
                        FFResidue ffres = activeFF.GetResidue(r.name);
                        foreach (UnityMolAtom a in r.atoms.Values) {
                            if (ffres.ContainsAtom(a.name)) {
                                FFAtom ffatm = ffres.GetAtom(a.name);
                                FFAtomType ffaty = activeFF.GetAtomType(ffatm.type);
                                curType = ffatm.type;
                                curCharge = ffatm.charge;
                                curEps = ffaty.eps;
                                curRmin = ffaty.rmin;
                            }
                            else {
                                sb.Append("Atom ");
                                sb.Append(a);
                                sb.Append(" not recognized in the forcefield\n");

                                curType = "undefined";
                                curCharge = 0.0f;
                                curEps = 0.0f;
                                curRmin = 0.0f;
                                countUndefined++;
                            }
                            atomsFFTypeList.Add(curType);
                            atomFFChargeList.Add(curCharge);
                            atomEpsList.Add(curEps);
                            atomRminList.Add(curRmin);
                        }
                    }
                    //Reside is NOT known in the force field
                    else {
                        sb.Append(s.name);
                        sb.Append(" Residue ");
                        sb.Append(r.name);
                        sb.Append("_");
                        sb.Append(r.id);
                        sb.Append(" not recognized in the forcefield\n");
                        foreach (UnityMolAtom a in r.atoms.Values) {

                            curType = "undefined";
                            curCharge = 0.0f;
                            curEps = 0.0f;
                            curRmin = 0.0f;
                            countUndefined++;

                            atomsFFTypeList.Add(curType);
                            atomFFChargeList.Add(curCharge);
                            atomEpsList.Add(curEps);
                            atomRminList.Add(curRmin);
                        }
                    }
                }
            }
        }

        Debug.LogWarning(sb.ToString());
        Debug.LogWarning(countUndefined + " atoms not defined in the forcefield and ignored during the docking !");
        if (countUndefined > atomsFFTypeList.Count / 2.0f)
            Debug.LogWarning("At least half the atoms of your system are undefined");
    }

    // Helper method to build combination table for LJ parameters
    private void BuildLJParameterTable(List<string> atomTypeList,
                                       List<float> atomEpsList,
                                       List<float> atomRminList,
                                       out int[] uniqueTypeIndices,
                                       out Vector2[] paramArray,
                                       out int numUTypes) {

        // Condense Data to unique types
        // Use an array to map every atom type to unique array position
        // Use these unique indices in the nested array for A/B parameters
        uniqueTypeIndices = new int[atomTypeList.Count]; // return value
        HashSet<string> typeHashSet = new HashSet<string>(atomTypeList);
        string[] uniqueTypes = new string[typeHashSet.Count];
        typeHashSet.CopyTo(uniqueTypes);

        numUTypes = uniqueTypes.Length;
        paramArray = new Vector2[numUTypes * numUTypes];

        int typeIndex;
        for (int i = 0; i < atomTypeList.Count; i++) {
            typeIndex = Array.IndexOf(uniqueTypes, atomTypeList[i]);
            uniqueTypeIndices[i] = typeIndex;
        }
        // Debug.LogFormat("Unique atom types in system: {0}", uniqueTypes.Length);

        // Calculate LJ A/B parameters
        // Full matrix (ij and ji access)
        for (int i = 0; i < atomTypeList.Count; i++) {
            float eps_i = atomEpsList[i];
            float rmin_i = atomRminList[i];

            for (int j = 0; j < atomTypeList.Count; j++) {
                float eps_j = atomEpsList[j];
                float rmin_j = atomRminList[j];

                // Eps(ij) = sqrt(eps(i) * eps(j))
                float eps_ij = Mathf.Sqrt(eps_i * eps_j);
                // Rmin(i,j) = Rmin(i) + Rmin(j)
                float rmin_ij = rmin_i + rmin_j;

                // A(ij) = eps(ij)*Rmin(ij)^12
                float rmin_ij_6 = Mathf.Pow(rmin_ij, 6);
                float A_ij = eps_ij * rmin_ij_6 * rmin_ij_6;
                // B(ij) = 2*eps(ij)*Rmin(ij)^6
                float B_ij = 2 * eps_ij * rmin_ij_6;

                Vector2 pairParam = new Vector2(A_ij, B_ij);
                int id = uniqueTypeIndices[i] * numUTypes + uniqueTypeIndices[j];
                paramArray[id] = pairParam;
            }
        }
        Debug.LogFormat("LJ Parameter Array Size: {0}", paramArray.Length);
    }

    public void OnDisable() {
        if (_isRunning) {

            // Force thread to quit
            _isRunning = false;

            // wait for thread to finish and clean up
            _thread.Join();
        }
        colliderPositions = null;
        structures.Clear();
        atomsFFTypeList.Clear();
        atomFFChargeList.Clear();
        atomEpsList.Clear();
        atomRminList.Clear();
        isSetup = false;
    }
} // end of DockingNBEnergyThread

// Class to calculate energetics of a molecular system at at given frame.
// Written for the AMBER force field
class NBEnergetics {

    // private float sq_threshold = 12.0f * 12.0f; // 12A cutoff, somewhat arbitrary
    public float sq_threshold = 999.0f * 999.0f; // infinite cutoff
    public float elec_scaling = 332.0522f;

    // Input data structures
    public int[] sizeOfPartition; // cumulative number of atoms in each chain
    public int[] atomPartitions; // array of arrays (atoms in each chain)

    public float[] atomFFCharge; // array with charges for Coulomb potential
    public int[] uniqueTypeIndices; // array with positions for LJ Table
    public Vector2[] paramArray; // LJ Table (flatten)
    public int numUTypes; // row size of LJ Table

    public Vector3[] atomCoords; // Actual positions (xyz)

    // Energies
    public Energy nbEnergies;

    private float elcEnergy;
    private float vdwEnergy;

    // Assign method to delegate depending on availability of C++
    private CPPWrapper cpp;
    public Action Calculate;

    public void SetImplementation() {
        this.Calculate = CSharpCalculate;
        try {
            cpp = new CPPWrapper();
            cpp.passParameters(atomPartitions, sizeOfPartition, uniqueTypeIndices,
                               paramArray, atomFFCharge, sizeOfPartition.Length - 1,
                               sq_threshold, elec_scaling, numUTypes);

            this.Calculate = CPPCalculate;

            // Dummy method to test if DLL is actually there and working.
            float[] dummyEnergy;
            Vector3[] dummyCoords = new Vector3[atomFFCharge.Length];
            cpp.callComputeNBEnergies(dummyCoords, out dummyEnergy);
            Debug.Log("Using optimized C++ implementation of energy calculation routine");
        }
        catch (DllNotFoundException) {
            this.Calculate = CSharpCalculate;
            Debug.Log("Using native C# implementation of energy calculation routine");
        }
    }


    // Optimized CPP implementation
    private void CPPCalculate() {

        float[] energies; // for return of C++ code
        cpp.callComputeNBEnergies(atomCoords, out energies);
        nbEnergies.vdw = energies[0];
        nbEnergies.elec = energies[1];
    }

    // Fallback C# implementation
    private void CSharpCalculate() {
        elcEnergy = 0.0f;
        vdwEnergy = 0.0f;

        int nbChains = sizeOfPartition.Length - 1;

        // iterate over chains
        for (int ch_i = 0; ch_i < nbChains - 1; ch_i++) {
            for (int ch_j = ch_i + 1; ch_j < nbChains; ch_j++) {

                // iterate over atoms
                for (int at_i = sizeOfPartition[ch_i]; at_i < sizeOfPartition[ch_i + 1]; at_i++) {
                    int i_idx = atomPartitions[at_i];

                    for (int at_j = sizeOfPartition[ch_j]; at_j < sizeOfPartition[ch_j + 1]; at_j++) {
                        int j_idx = atomPartitions[at_j];
                        float sq_dij = (atomCoords[i_idx] - atomCoords[j_idx]).sqrMagnitude;

                        if (sq_dij <= sq_threshold) {
                            int type_i_idx = uniqueTypeIndices[i_idx];
                            int type_j_idx = uniqueTypeIndices[j_idx];

                            float dij_6 = sq_dij * sq_dij * sq_dij;

                            elcEnergy += (elec_scaling * atomFFCharge[i_idx] * atomFFCharge[j_idx]) / Mathf.Sqrt(sq_dij);
                            vdwEnergy += (paramArray[type_i_idx * numUTypes + type_j_idx].x / (dij_6 * dij_6)) - (paramArray[type_i_idx * numUTypes  + type_j_idx].y / dij_6);
                        }
                    }
                }
            }
        }
        nbEnergies.vdw = vdwEnergy;
        nbEnergies.elec = elcEnergy;
    }
}

// Wrapper class for optimized C++ methods contained in DLL/.so
class CPPWrapper {

    // C++ Calls
    [DllImport("OptimizedNBEnergyCalculation")]
    private static extern void setupEnergyComputation(int[] atomPartition, int[] sizePartition, int[] uniqueTypeIndices, Vector2[] paramArray,
            float[] atomFFCharge, int nbChains, float threshold, float elecScaling, int numUTypes);


    [DllImport("OptimizedNBEnergyCalculation")]
    private static extern void ComputeNBEnergies(Vector3[] coords, out float elecEnergy, out float vdwEnergy);

    // C# interface calls
    public void passParameters(int[] atomPartition, int[] sizePartition, int[] uniqueTypeIndices, Vector2[] paramArray,
                               float[] atomFFCharge, int nbChains, float threshold, float elecScaling, int numUTypes) {

        setupEnergyComputation(atomPartition, sizePartition, uniqueTypeIndices, paramArray, atomFFCharge, nbChains, threshold, elecScaling, numUTypes);

    }

    public void callComputeNBEnergies(Vector3[] coords, out float[] result) {

        result = new float[2] {0.0f, 0.0f};

        float elec = 0.0f;
        float vdw = 0.0f;

        ComputeNBEnergies(coords, out elec, out vdw);

        result[1] = elec;
        result[0] = vdw;
    }
}
}
}