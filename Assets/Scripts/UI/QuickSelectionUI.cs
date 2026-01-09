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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Linq;

namespace UMol {
public class QuickSelectionUI : MonoBehaviour {

    public Dropdown dpdStructure;
    public Dropdown dpdChain;
    public Dropdown dpdResidue;

    private string currentStructureName = null;
    private string currentChainName = null;
    private int currentResidueId = -1;

    private string resName = "";

    private UnityMolStructureManager sm;
    private UnityMolSelectionManager selM;
    private int curUIS = 0;


    void Start() {
        sm = UnityMolMain.getStructureManager();
        selM = UnityMolMain.getSelectionManager();


        if (dpdStructure == null) {
            return;
        }
        if (dpdChain == null) {
            return;
        }
        if (dpdResidue == null) {
            return;
        }

        dpdStructure.onValueChanged.AddListener(delegate {
            selectCurrentStructure(dpdStructure);
            updateAvailableChains(dpdChain);
            updateAvailableResidues(dpdResidue);
        });

        dpdChain.onValueChanged.AddListener(delegate {
            selectCurrentChain(dpdChain);
            updateAvailableResidues(dpdResidue);
        });

        dpdResidue.onValueChanged.AddListener(delegate {
            selectCurrentResidue(dpdResidue);
        });


    }

    List<string> getChainNames(UnityMolStructure s) {
        return s.currentModel.chains.Keys.ToList();
    }

    void updateLoadedStrucUI() {

        List<string> newOptions = new List<string>() {"Structure"};
        foreach (UnityMolStructure s in sm.loadedStructures) {
            newOptions.Add(s.name);
        }

        dpdStructure.ClearOptions();
        dpdStructure.AddOptions(newOptions);

        updateAvailableChains(dpdChain);
        dpdResidue.ClearOptions();
        dpdResidue.AddOptions(new List<string>() {"Residue"});
    }
    void selectCurrentStructure(Dropdown dpds) {
        currentStructureName = null;

        if (dpds.value > 0) {
            currentStructureName = dpds.options[dpds.value].text;
        }
    }

    void updateAvailableChains(Dropdown dpdc) {
        dpdc.SetValue(0);
        currentChainName = null;

        dpdc.ClearOptions();

        HashSet<string> availableChains = new HashSet<string>();
        availableChains.Add("Chain");

        if (currentStructureName == null) {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                availableChains.UnionWith(s.currentModel.chains.Keys.ToList());
            }
        }
        else {
            availableChains.UnionWith(sm.nameToStructure[currentStructureName].currentModel.chains.Keys.ToList());
        }

        dpdc.AddOptions(availableChains.ToList());

    }
    void selectCurrentChain(Dropdown dpdc) {
        currentChainName = null;

        if (dpdc.value > 0) {
            currentChainName = dpdc.options[dpdc.value].text;
        }
    }

    void updateAvailableResidues(Dropdown dpdr) {
        dpdr.SetValue(0);
        currentResidueId = -1;
        dpdr.ClearOptions();

        HashSet<string> allResName = new HashSet<string>();
        List<string> availableResi = new List<string>() {"Residue"};

        if (currentStructureName == null && currentChainName == null) {
            // Debug.LogWarning("Please choose at least a structure or a chain");
            dpdr.AddOptions(availableResi);
            return;
        }
        if (currentStructureName == null) {
            foreach (UnityMolStructure s in sm.loadedStructures) {
                if (currentChainName == null) {
                    foreach (UnityMolChain c in s.currentModel.chains.Values) {
                        foreach (UnityMolResidue r in c.residues) {
                            availableResi.Add(r.name + "°" + r.id);
                            allResName.Add(r.name);
                        }
                    }
                }
                else if (s.currentModel.chains.ContainsKey(currentChainName)) {

                    UnityMolChain c = s.currentModel.chains[currentChainName];
                    foreach (UnityMolResidue r in c.residues) {
                        availableResi.Add(r.name + "°" + r.id);
                        allResName.Add(r.name);
                    }
                }
            }
        }
        else {
            UnityMolStructure s = sm.nameToStructure[currentStructureName];
            if (currentChainName == null) {
                foreach (UnityMolChain c in s.currentModel.chains.Values) {
                    foreach (UnityMolResidue r in c.residues) {
                        availableResi.Add(r.name + "°" + r.id);
                        allResName.Add(r.name);
                    }
                }
            }
            else if (s.currentModel.chains.ContainsKey(currentChainName)) {

                UnityMolChain c = s.currentModel.chains[currentChainName];
                foreach (UnityMolResidue r in c.residues) {
                    availableResi.Add(r.name + "°" + r.id);
                    allResName.Add(r.name);
                }
            }
        }

        foreach (string s in allResName) {
            availableResi.Add(s + "*");
        }

        dpdr.AddOptions(availableResi);
    }

    public void selectCurrentResidue(Dropdown dpdr) {
        currentResidueId = -1;

        if (dpdr.value > 0) {
            try {
                string fullR = dpdr.options[dpdr.value].text;
                if (fullR.EndsWith("*")) {
                    currentResidueId = -2;
                    resName = fullR.Remove(fullR.Length - 1);
                    return;
                }
                string[] sp = fullR.Split(new [] { '°'}, System.StringSplitOptions.RemoveEmptyEntries);
                currentResidueId = int.Parse(sp.Last());
            }
            catch {

            }
        }
    }

    public void doQuickSelection() {
        if (currentStructureName == null && currentChainName == null) {
            Debug.LogWarning("Please choose at least a structure or a chain");
            return;
        }

        string selMDAQuery = "";
        if (!string.IsNullOrEmpty(currentStructureName)) {
            selMDAQuery = currentStructureName;
            if (!string.IsNullOrEmpty(currentChainName)) {
                selMDAQuery += " and chain " + currentChainName;
                if (currentResidueId >= 0) {
                    selMDAQuery += " and resid " + currentResidueId;
                }
                else if (currentResidueId == -2) {
                    selMDAQuery += " and resname " + resName;
                }
            }
            else {
                if (currentResidueId >= 0) {
                    selMDAQuery += " and resid " + currentResidueId;
                }
                else if (currentResidueId == -2) {
                    selMDAQuery += " and resname " + resName;
                }
            }
        }
        else {
            selMDAQuery = "chain " + currentChainName;
            if (currentResidueId >= 0) {
                selMDAQuery += " and resid " + currentResidueId;
            }
            else if (currentResidueId == -2) {
                selMDAQuery += " and resname " + resName;
            }
        }

        if (selM.currentSelection == null || !selM.currentSelection.isAlterable) {
            selM.getClickSelection();
        }

        UnityMolSelection curSel = selM.currentSelection;

        API.APIPython.select(selMDAQuery, curSel.name, createSelection: true,
                             addToExisting: false, silent: false);

        Debug.Log("Selection: " + selMDAQuery);
        // currentStructureName = null;
        // currentChainName = null;
        // currentResidueId = -1;
        // dpdStructure.value = 0;
        // dpdChain.value = 0;
        // dpdResidue.value = 0;
    }

    void Update() {
        if (dpdStructure == null) {
            return;
        }
        if (dpdChain == null) {
            return;
        }
        if (dpdResidue == null) {
            return;
        }
        if ( curUIS != sm.loadedStructures.Count) {
            updateLoadedStrucUI();
            curUIS = sm.loadedStructures.Count;
        }
    }

}
}