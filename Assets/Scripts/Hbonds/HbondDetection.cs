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


using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Unity.Collections;
using Unity.Mathematics;
using KNN;
using KNN.Jobs;
using Unity.Jobs;

namespace UMol {
public class HbondDetection {

    public static float neighboorDistance = 4.0f;//4 Angstrom
    public static float limitDistanceHA = 2.8f;
    public static float limitAngleDHA = 120.0f;
    public static float limitAngleHAB = 90.0f;

    public static List<string> listAcceptors = new List<string> {"O", "N"};
    public static Dictionary<string, int> maxBonded = new Dictionary<string, int> { {"O", 2}, {"N", 3}};

    public static UnityMolBonds DetectHydrogenBonds(UnityMolSelection selec) {

        UnityMolBonds customChemBonds = getCustomChemBonds(selec);
        if (customChemBonds.Count != 0) {
            return customChemBonds;
        }
        // return DetectHydrogenBonds_Avogadro(selec);
        return DetectHydrogenBonds_Shrodinger(selec);
    }

    public static UnityMolBonds getCustomChemBonds(UnityMolSelection selec) {
        UnityMolBonds bonds = new UnityMolBonds();

        HashSet<UnityMolModel> doneModel = new HashSet<UnityMolModel>();

        foreach (UnityMolAtom a in selec.atoms) {
            UnityMolModel curModel = a.residue.chain.model;
            if (curModel.customChemBonds.Count != 0) {
                foreach (Int2 pair in curModel.customChemBonds) {
                    UnityMolAtom a1 = curModel.getAtomWithID(pair.x);
                    UnityMolAtom a2 = curModel.getAtomWithID(pair.y);
                    if (a1 != null && a2 != null) {
                        bonds.Add(a1, a2);
                    }
                    else if (a1 == null) {
                        Debug.LogWarning("Could not find atom with id " + pair.x + " in " + curModel.structure);
                    }
                    else if (a2 == null) {
                        Debug.LogWarning("Could not find atom with id " + pair.y + " in " + curModel.structure);
                    }
                }
                doneModel.Add(curModel);
            }
        }
        return bonds;
    }

    public static UnityMolBonds DetectHydrogenBonds_Avogadro(UnityMolSelection selec) {
        //From https://github.com/cryos/avogadro/blob/2a98712f24506d023aa4b84c984136a913017e81/libavogadro/src/protein.cpp
        UnityMolBonds bonds = new UnityMolBonds();

        if(selec.Count == 0){
            return bonds;
        }

        Transform loadedMol = UnityMolMain.getRepresentationParent().transform;

        var allAtoms = new NativeArray<float3>(selec.atoms.Count, Allocator.Persistent);

        int id = 0;
        //Transform points in the framework of the loadedMolecules transform
        foreach (UnityMolAtom a in selec.atoms) {
            allAtoms[id++] = loadedMol.InverseTransformPoint(a.curWorldPosition);
        }

        //Arbitrary number that should be enough to find all atoms in the neighboorDistance
        int kNeighbours = 15;

        //Create KDTree
        KnnContainer knnContainer = new KnnContainer(allAtoms, false, Allocator.TempJob);
        NativeArray<float3> queryPositions = new NativeArray<float3>(allAtoms.Length, Allocator.TempJob);
        NativeArray<int> results = new NativeArray<int>(selec.atoms.Count * kNeighbours, Allocator.TempJob);

        var rebuildJob = new KnnRebuildJob(knnContainer);
        rebuildJob.Schedule().Complete();

        var batchQueryJob = new QueryKNearestBatchJob(knnContainer, queryPositions, results);
        batchQueryJob.ScheduleBatch(queryPositions.Length, queryPositions.Length / 32).Complete();

        int idA = 0;
        foreach (UnityMolAtom a1 in selec.atoms) {

            for (int i = 0; i < kNeighbours; i++) { //For all neighboors
                int idAtom = results[idA * kNeighbours + i];

                if (idAtom < 0 || idAtom >= selec.atoms.Count) {
                    continue;
                }
                
                UnityMolAtom a2 = selec.atoms[idAtom];

                UnityMolResidue res1 = a1.residue;
                UnityMolResidue res2 = a2.residue;

                if (res1 == res2) {
                    continue;
                }
                if (Mathf.Abs(res2.id - res1.id) <= 2) {
                    continue;
                }

                // residue 1 has the N-H
                // residue 2 has the C=O
                if (a1.type != "O") {
                    if (a2.type != "O") {
                        continue;
                    }
                }
                else {
                    res1 = a2.residue;
                    res2 = a1.residue;
                }

                if (!res1.atoms.ContainsKey("N")) {
                    continue;
                }
                if (!res2.atoms.ContainsKey("C") || !res2.atoms.ContainsKey("O")) {
                    continue;
                }

                UnityMolAtom N = res1.atoms["N"];
                UnityMolAtom H = null;
                UnityMolAtom C = res2.atoms["C"];
                UnityMolAtom O = res2.atoms["O"];

                if (!selec.bonds.bondsDual.ContainsKey(N)) {
                    Debug.Log("Nothing bonded to " + N);
                    if (selec.bonds.bonds.ContainsKey(N)) {
                        for (int d = 0; d < selec.bonds.bonds[N].Length; d++) {
                            if (selec.bonds.bonds[N][d] != null) {
                                Debug.Log(selec.bonds.bonds[N][d]);
                            }
                        }
                    }
                    continue;
                }
                UnityMolAtom[] bondedN = selec.bonds.bondsDual[N];

                Vector3 posH = Vector3.zero;
                foreach (UnityMolAtom bN in bondedN) { //For all neighboors of N
                    if (bN != null) {
                        if (bN.type == "H") {
                            posH = bN.position;
                            H = bN;
                            break;
                        }
                        else { //Average of bonded atoms to compute H position
                            posH += N.position - bN.position;
                        }
                    }
                }
                if (H == null) { //H was not find => compute position
                    posH = N.position + (1.1f * posH);
                }


                //  C=O ~ H-N
                //
                //  C +0.42e   O -0.42e
                //  H +0.20e   N -0.20e
                float rON = (O.position - N.position).magnitude;
                float rCH = (C.position - posH).magnitude;
                float rOH = (O.position - posH).magnitude;
                float rCN = (C.position - N.position).magnitude;

                float eON = 332 * (-0.42f * -0.20f) / rON;
                float eCH = 332 * ( 0.42f *  0.20f) / rCH;
                float eOH = 332 * (-0.42f *  0.20f) / rOH;
                float eCN = 332 * ( 0.42f * -0.20f) / rCN;
                float E = eON + eCH + eOH + eCN;

                if (E >= -0.5f)
                    continue;

                if (H == null) {
                    bonds.Add(O, N);
                }
                else {
                    bonds.Add(O, H);
                }
            }
            idA++;
        }

        knnContainer.Dispose();
        queryPositions.Dispose();
        results.Dispose();
        allAtoms.Dispose();

        // Vector3[] points = new Vector3[selec.atoms.Count];
        // int id = 0;
        // //Transform points in the framework of the loadedMolecules transform
        // foreach (UnityMolAtom a in selec.atoms) {
        //     points[id++] = loadedMol.InverseTransformPoint(a.curWorldPosition);
        // }

        // KDTree tmpKDTree = KDTree.MakeFromPoints(points);


        // foreach (UnityMolAtom a1 in selec.atoms) {

        //     int[] ids = tmpKDTree.FindNearestsRadius(loadedMol.InverseTransformPoint(a1.curWorldPosition) , neighboorDistance);

        //     for (int i = 0; i < ids.Length; i++) { //For all neighboors
        //         UnityMolAtom a2 = selec.atoms[ids[i]];

        //         UnityMolResidue res1 = a1.residue;
        //         UnityMolResidue res2 = a2.residue;

        //         if (res1 == res2) {
        //             continue;
        //         }
        //         if (Mathf.Abs(res2.id - res1.id) <= 2) {
        //             continue;
        //         }

        //         // residue 1 has the N-H
        //         // residue 2 has the C=O
        //         if (a1.type != "O") {
        //             if (a2.type != "O") {
        //                 continue;
        //             }
        //         }
        //         else {
        //             res1 = a2.residue;
        //             res2 = a1.residue;
        //         }

        //         if (!res1.atoms.ContainsKey("N")) {
        //             continue;
        //         }
        //         if (!res2.atoms.ContainsKey("C") || !res2.atoms.ContainsKey("O")) {
        //             continue;
        //         }

        //         UnityMolAtom N = res1.atoms["N"];
        //         UnityMolAtom H = null;
        //         UnityMolAtom C = res2.atoms["C"];
        //         UnityMolAtom O = res2.atoms["O"];

        //         if (!selec.bonds.bondsDual.ContainsKey(N)) {
        //             Debug.Log("Nothing bonded to " + N);
        //             if (selec.bonds.bonds.ContainsKey(N)) {
        //                 for (int d = 0; d < selec.bonds.bonds[N].Length; d++) {
        //                     if (selec.bonds.bonds[N][d] != null) {
        //                         Debug.Log(selec.bonds.bonds[N][d]);
        //                     }
        //                 }
        //             }
        //             continue;
        //         }
        //         UnityMolAtom[] bondedN = selec.bonds.bondsDual[N];

        //         Vector3 posH = Vector3.zero;
        //         foreach (UnityMolAtom bN in bondedN) { //For all neighboors of N
        //             if (bN != null) {
        //                 if (bN.type == "H") {
        //                     posH = bN.position;
        //                     H = bN;
        //                     break;
        //                 }
        //                 else { //Average of bonded atoms to compute H position
        //                     posH += N.position - bN.position;
        //                 }
        //             }
        //         }
        //         if (H == null) { //H was not find => compute position
        //             posH = N.position + (1.1f * posH);
        //         }


        //         //  C=O ~ H-N
        //         //
        //         //  C +0.42e   O -0.42e
        //         //  H +0.20e   N -0.20e
        //         float rON = (O.position - N.position).magnitude;
        //         float rCH = (C.position - posH).magnitude;
        //         float rOH = (O.position - posH).magnitude;
        //         float rCN = (C.position - N.position).magnitude;

        //         float eON = 332 * (-0.42f * -0.20f) / rON;
        //         float eCH = 332 * ( 0.42f *  0.20f) / rCH;
        //         float eOH = 332 * (-0.42f *  0.20f) / rOH;
        //         float eCN = 332 * ( 0.42f * -0.20f) / rCN;
        //         float E = eON + eCH + eOH + eCN;

        //         if (E >= -0.5f)
        //             continue;

        //         if (H == null) {
        //             bonds.Add(O, N);
        //         }
        //         else {
        //             bonds.Add(O, H);
        //         }
        //     }
        // }


        return bonds;
    }

    public static UnityMolBonds DetectHydrogenBonds_Shrodinger(UnityMolSelection selec) {
        UnityMolBonds bonds = new UnityMolBonds();
        if(selec.Count == 0){
            return bonds;
        }

        Transform loadedMol = UnityMolMain.getRepresentationParent().transform;

        List<UnityMolAtom> acceptors = getAcceptors(selec);

        var allAtoms = new NativeArray<float3>(selec.atoms.Count, Allocator.Persistent);
        var accAtoms = new NativeArray<float3>(acceptors.Count, Allocator.Persistent);

        int id = 0;
        //Transform points in the framework of the loadedMolecules transform
        foreach (UnityMolAtom a in selec.atoms) {
            allAtoms[id++] = loadedMol.InverseTransformPoint(a.curWorldPosition);
        }
        id = 0;
        foreach (UnityMolAtom a in acceptors) {
            accAtoms[id++] = loadedMol.InverseTransformPoint(a.curWorldPosition);
        }

        //Arbitrary number that should be enough to find all atoms in the neighboorDistance
        int kNeighbours = 15;

        //Create KDTree
        var knnContainer = new KnnContainer(allAtoms, true, Allocator.TempJob);
        var queryPositions = new NativeArray<float3>(accAtoms, Allocator.TempJob);
        var results = new NativeArray<int>(acceptors.Count * kNeighbours, Allocator.TempJob);

        var batchQueryJob = new QueryKNearestBatchJob(knnContainer, queryPositions, results);
        batchQueryJob.ScheduleBatch(queryPositions.Length, queryPositions.Length / 32).Complete();

        int idA = 0;
        foreach (UnityMolAtom acc in acceptors) {

            for (int i = 0; i < kNeighbours; i++) { //For all neighboors

                UnityMolAtom n = selec.atoms[results[idA * kNeighbours + i]];
                if (n.type == "H" &&
                        n.residue.chain.model.structure.uniqueName == acc.residue.chain.model.structure.uniqueName) {

                    float distance = Vector3.Distance(acc.position, n.position);
                    if (distance < limitDistanceHA) {
                        UnityMolAtom donor = getBonded(n, selec.bonds);

                        if (donor == null) {
                            continue;
                        }
                        float DHA_angle = Vector3.Angle(donor.position - n.position, acc.position - n.position);

                        if (DHA_angle > limitAngleDHA) {
                            UnityMolAtom bondedToAcceptor = getBonded(acc, selec.bonds);

                            if (bondedToAcceptor == null) {
                                continue;
                            }

                            float HAB_angle = Vector3.Angle(n.position - acc.position, bondedToAcceptor.position -  acc.position);
                            if (HAB_angle > limitAngleHAB) {
                                bonds.Add(n, acc);
                            }
                        }
                    }
                }
            }
            idA++;
        }
        
        knnContainer.Dispose();
        queryPositions.Dispose();
        results.Dispose();
        accAtoms.Dispose();
        allAtoms.Dispose();


        // Vector3[] points = new Vector3[selec.atoms.Count];
        // int id = 0;
        // //Transform points in the framework of the loadedMolecules transform
        // foreach (UnityMolAtom a in selec.atoms) {
        //     points[id++] = loadedMol.InverseTransformPoint(a.curWorldPosition);
        // }

        // KDTree tmpKDTree = KDTree.MakeFromPoints(points);

        // List<UnityMolAtom> acceptors = getAcceptors(selec);

        // foreach (UnityMolAtom acc in acceptors) {
        //     List<UnityMolAtom> neighboors = findNeighboors(acc, selec, tmpKDTree, loadedMol);

        //     foreach (UnityMolAtom n in neighboors) {
        //         if (n.type == "H") {
        //             float distance = Vector3.Distance(acc.position, n.position);
        //             if (distance < limitDistanceHA) {
        //                 UnityMolAtom donor = getBonded(n, selec.bonds);

        //                 if (donor == null) {
        //                     continue;
        //                 }
        //                 float DHA_angle = Vector3.Angle(donor.position - n.position, acc.position - n.position);

        //                 if (DHA_angle > limitAngleDHA) {
        //                     UnityMolAtom bondedToAcceptor = getBonded(acc, selec.bonds);

        //                     if (bondedToAcceptor == null) {
        //                         continue;
        //                     }

        //                     float HAB_angle = Vector3.Angle(n.position - acc.position, bondedToAcceptor.position -  acc.position);
        //                     if (HAB_angle > limitAngleHAB) {
        //                         bonds.Add(n, acc);
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }

        return bonds;
    }

    static List<UnityMolAtom> getAcceptors(UnityMolSelection selec) {
        List<UnityMolAtom> result = new List<UnityMolAtom>();

        foreach (UnityMolAtom a in selec.atoms) {
            if (isAcceptor(a)) {
                int nbBonded = selec.bonds.countBondedAtoms(a);

                if (nbBonded > 0 && nbBonded < maxBonded[a.type]) {
                    if (a.type == "O" && !linkedToH(a, selec)) {
                        result.Add(a);
                    }
                    else if (a.type == "N") {
                        result.Add(a);
                    }
                }
            }
        }
        return result;
    }
    static bool linkedToH(UnityMolAtom a, UnityMolSelection selec) {
        UnityMolAtom[] bonded = selec.bonds.bondsDual[a];
        if (bonded == null)
            return false;
        for (int i = 0; i < bonded.Length; i++) {
            if (bonded[i] != null) {
                if (bonded[i].type == "H") {
                    return true;
                }
            }
        }
        return false;
    }
    static bool isAcceptor(UnityMolAtom atom) {
        return listAcceptors.Contains(atom.type);
    }

//Returns first atom bonded to atom
    static UnityMolAtom getBonded(UnityMolAtom a, UnityMolBonds bonds) {
        try {
            return bonds.bondsDual[a][0];
        }
        catch {
            // Debug.LogWarning("Did not find atom "+a+" in bonds");
            return null;
        }
    }

}
}