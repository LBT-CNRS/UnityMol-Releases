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
using System.Linq;

namespace UMol {
public abstract class UnityMolAnnotation {

    public GameObject go;
    public bool isShown = true;
    public Transform annoParent;
    public List<UnityMolAtom> atoms = new List<UnityMolAtom>();

    public UnityMolAnnotation() {}

    public abstract void Create();
    public abstract void Update();
    public abstract void UnityUpdate();
    public abstract void Delete();
    public abstract void Show(bool show);
    public abstract SerializedAnnotation Serialize();
    public abstract int toAnnoType();

    public void fillSerializedAtoms(SerializedAnnotation san) {
        san.annoType = toAnnoType();
        if (atoms == null || atoms.Count == 0)
            return;
        san.structureIds = new List<int>(atoms.Count);
        san.atomIds = new List<int>(atoms.Count);
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        for (int i = 0; i < atoms.Count; i++) {
            san.structureIds.Add(-1);
            san.atomIds.Add(-1);
        }

        for (int i = 0; i < sm.loadedStructures.Count; i++) {
            for (int a = 0; a < atoms.Count; a++) {
                if (sm.loadedStructures[i] == atoms[a].residue.chain.model.structure) {
                    san.structureIds[a] = i;
                    san.atomIds[a] = atoms[a].idInAllAtoms;
                }
            }
        }
    }
}
}