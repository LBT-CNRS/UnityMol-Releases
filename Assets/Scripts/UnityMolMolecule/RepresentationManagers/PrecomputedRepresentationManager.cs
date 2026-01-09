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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace UMol {

    public class PrecomputedRepresentationManager {
        public Dictionary<string, MeshData> precomputedRep = new Dictionary<string, MeshData>();
        public Dictionary<string, Dictionary<UnityMolResidue, List<int>>> precomputedCartoonAsso = new Dictionary<string, Dictionary<UnityMolResidue, List<int>>>();
        public Dictionary<string, int[]> precomputedSurfAsso = new Dictionary<string, int[]>();

        public void Clear(string sName) {
            var keys = precomputedRep.Keys.ToArray();

            foreach (string k in keys) {
                if (k.StartsWith(sName + "_")) {
                    precomputedRep.Remove(k);
                    precomputedCartoonAsso.Remove(k);
                    precomputedSurfAsso.Remove(k);
                }
            }
        }

        public void Clear() {
            var keys = precomputedRep.Keys.ToArray();

            foreach (string k in keys) {
                precomputedRep.Remove(k);
                precomputedCartoonAsso.Remove(k);
                precomputedSurfAsso.Remove(k);
            }
            precomputedRep.Clear();
            precomputedCartoonAsso.Clear();
            precomputedSurfAsso.Clear();
        }
        public void Clear(string sName, string repName) {
            var keys = precomputedRep.Keys.ToArray();
            foreach (string k in keys) {
                if (k.StartsWith(sName + "_") && k.EndsWith(repName)) {
                    precomputedRep.Remove(k);
                    precomputedCartoonAsso.Remove(k);
                    precomputedSurfAsso.Remove(k);
                }
            }
        }

        public bool ContainsRep(string key) {
            return precomputedRep.ContainsKey(key);
        }
    }

}