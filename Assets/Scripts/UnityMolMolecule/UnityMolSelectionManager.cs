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
using System;

namespace UMol {
public class UnityMolSelectionManager {

    public delegate void OnNewClickSelection(NewSelEventArgs args);
    public OnNewClickSelection onNewClickSelection;


    public delegate void NewSelection();
    public static event NewSelection OnNewSelection;
    public delegate void DelSelection();
    public static event DelSelection OnSelectionDeleted;
    public delegate void ModifSelection();
    public static event ModifSelection OnSelectionModified;

    public enum SelectionMode {Atom, Residue, Chain, Molecule};

    public SelectionMode selectionMode = SelectionMode.Residue;



    private string curSelName = "";
    /// <summary>
    /// UnityMolSelection containing all the atoms of the currentSelections list
    /// </summary>
    public UnityMolSelection currentSelection {
        get {
            if (selections.ContainsKey(curSelName)) {
                return selections[curSelName];
            }
            return null;
        }
    }

    /// <summary>
    /// Dictionary of all selections created
    /// </summary>
    public Dictionary<string, UnityMolSelection> selections = new Dictionary<string, UnityMolSelection>();

    public Dictionary<string, string> selectionMDAKeywords = new Dictionary<string, string>();

    // private UnityMolSelection clickSelection;

    public string clickSelectionName = "selection";

    public void Add(UnityMolSelection sel) {
        if (selections.ContainsKey(sel.name)) {
            Delete(sel.name);
        }
        else if (OnNewSelection != null)
            OnNewSelection();
        selections[sel.name] = sel;
    }

    public void Delete(string selName) {
        if (!selections.ContainsKey(selName)) {
            Debug.LogWarning("Selection '" + selName + "' not found");
            return;
        }

        if (curSelName == selName) {
            ClearCurrentSelection();
        }

        DeleteRepresentations(selections[selName]);

        selections.Remove(selName);

        if (OnSelectionDeleted != null)
            OnSelectionDeleted();
    }

    public void DeleteRepresentations(UnityMolSelection sel) {
        UnityMolRepresentationManager repM = UnityMolMain.getRepresentationManager();

        foreach (RepType repType in sel.representations.Keys) {
            foreach (UnityMolRepresentation r in sel.representations[repType]) {
                repM.Delete(r);
            }
        }
        sel.representations.Clear();
    }
    public void DeleteRepresentation(UnityMolSelection sel, RepType repType) {
        UnityMolRepresentationManager repM = UnityMolMain.getRepresentationManager();

        if (sel.representations.ContainsKey(repType)) {
            foreach (UnityMolRepresentation r in sel.representations[repType]) {
                repM.Delete(r);
            }

            sel.representations.Remove(repType);
        }
    }

    public void Delete(UnityMolStructure s) {

        List<UnityMolSelection> allSel = selections.Values.ToList();
        foreach (UnityMolSelection sel in allSel) {
            if (sel.structures.Contains(s) || sel.structures.Count == 0) {
                Delete(sel.name);
            }
        }
#if !DISABLE_HIGHLIGHT
        if (currentSelection == null || currentSelection.Count == 0) {
            UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();
            hM.Clean();
        }
#endif
    }


    /// <summary>
    /// Get the "(selection)" selection. Creates it if it does not exist.
    /// </summary>
    public UnityMolSelection getClickSelection() {
        UnityMolSelection result = currentSelection;

        if (selections.ContainsKey(clickSelectionName)) { //Selection has already been created

            if (result == null) { //Selection is not current selection --> set it
                SetCurrentSelection(selections[clickSelectionName]);
            }
            return selections[clickSelectionName];
        }

        //New "(selection)"
        result = new UnityMolSelection(new List<UnityMolAtom>(), null, clickSelectionName, "nothing");
        SetCurrentSelection(result);

        return result;
    }

    /// <summary>
    /// Get the current selection if it is set otherwise create a new one with getClickSelection
    /// </summary>
    public UnityMolSelection getCurrentSelection() {
        if (currentSelection == null) {
            getClickSelection();
        }
        return currentSelection;
    }

    public void Clean() {
        ClearCurrentSelection();

        selections.Clear();
        // clickSelection = null;
    }

    public void updateSelectionsWithNewModel(UnityMolStructure s) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();


        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();

        foreach (UnityMolStructure ss in sm.loadedStructures) {
            allAtoms.AddRange(ss.currentModel.allAtoms);
        }

        foreach (UnityMolSelection sel in selections.Values) {

            if (sel.structures.Contains(s)) {
                bool saveIsAlterable = sel.isAlterable;
                sel.isAlterable = true;

                if (sel.fromSelectionLanguage) {

                    MDAnalysisSelection selec = new MDAnalysisSelection(sel.MDASelString, allAtoms);
                    UnityMolSelection ret = selec.process();

                    sel.atoms = ret.atoms;
                    sel.bonds = ret.bonds;
                    sel.structures = ret.structures;
                    if (OnSelectionModified != null)
                        OnSelectionModified();
                }
                else {
                    List<UnityMolAtom> newAtomList = new List<UnityMolAtom>(sel.atoms.Count);
                    foreach (UnityMolAtom a in sel.atoms) {
                        UnityMolAtom correspondingAtom = s.findAtomInModel(s.currentModel, a);
                        if (correspondingAtom == null) {
                            Debug.LogError("Error updating the selection: Could't find the same atom in the new model");
                            continue;
                        }
                        else {
                            newAtomList.Add(correspondingAtom);
                        }
                    }

                    newAtomList = newAtomList.Distinct().ToList();
                    sel.atoms = newAtomList;
                    sel.bonds = null;
                    if (OnSelectionModified != null)
                        OnSelectionModified();
                }
                sel.isAlterable = saveIsAlterable;

            }
        }
    }

    public void updateSelectionContentTrajectory(UnityMolStructure s) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        bool updateNecessary = false;
        //Test if update is needed
        foreach (UnityMolSelection sel in selections.Values) {
            if (sel.Count == 0 || sel.structures.Contains(s)) {
                if (sel.updateContentWithTraj && sel.fromSelectionLanguage) {
                    updateNecessary = true;
                    break;
                }
            }
        }
        if (!updateNecessary) {
            return;
        }

        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();

        if (sm.loadedStructures.Count == 1) {
            allAtoms = sm.loadedStructures[0].currentModel.allAtoms;
        }
        else {
            foreach (UnityMolStructure ss in sm.loadedStructures) {
                allAtoms.AddRange(ss.currentModel.allAtoms);
            }
        }

        foreach (UnityMolSelection sel in selections.Values) {
            if (sel.Count == 0 || sel.structures.Contains(s)) {
                if (sel.updateContentWithTraj) {

                    bool saveIsAlterable = sel.isAlterable;
                    sel.isAlterable = true;
                    if (sel.fromSelectionLanguage) {

                        MDAnalysisSelection selec = new MDAnalysisSelection(sel.MDASelString, allAtoms);
                        UnityMolSelection result = selec.process();

                        sel.atoms = result.atoms;
                        if (!sel.bondsNull) { //Need to update bonds ?
                            sel.bonds = result.bonds;
                        }
                        sel.structures = result.structures;

                        if (OnSelectionModified != null)
                            OnSelectionModified();
                    }
                    else {
                        //Cannot update a selection without MDASelection string
                    }
                    sel.isAlterable = saveIsAlterable;

                    foreach (List<UnityMolRepresentation> reps in sel.representations.Values) {
                        foreach (UnityMolRepresentation r in reps) {
                            r.updateWithNewSelection(sel);
                        }
                    }
                }
            }
        }
    }


    public static void launchSelectionModified() {
        if (OnSelectionModified != null)
            OnSelectionModified();
    }
    public void updateSelectionsWithMDA(UnityMolStructure s, bool forceModif = false) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();

        foreach (UnityMolStructure ss in sm.loadedStructures) {
            allAtoms.AddRange(ss.currentModel.allAtoms);
        }

        foreach (UnityMolSelection sel in selections.Values) {
            if (sel.structures.Contains(s)) {
                bool saveIsAlterable = sel.isAlterable;
                if (forceModif) {
                    sel.isAlterable = true;
                }
                if (sel.fromSelectionLanguage) {

                    MDAnalysisSelection selec = new MDAnalysisSelection(sel.MDASelString, allAtoms);
                    UnityMolSelection ret = selec.process();

                    sel.atoms = ret.atoms;
                    sel.bonds = ret.bonds;
                    sel.structures = ret.structures;

                    if (OnSelectionModified != null)
                        OnSelectionModified();

                }
                else {
                    sel.bonds = null;//Update bonds at first access
                }
                if (forceModif) {
                    sel.isAlterable = saveIsAlterable;
                }

            }
        }
    }


    public void ClearCurrentSelection() {
        if (currentSelection != null) {

#if !DISABLE_HIGHLIGHT
            UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();
            hM.Clean();
#endif

            curSelName = "";
        }
    }

    public void SetCurrentSelection(UnityMolSelection sel) {
        ClearCurrentSelection();

        if (!selections.ContainsKey(sel.name)) {
            Add(sel);
        }

        curSelName = sel.name;

#if !DISABLE_HIGHLIGHT
        UnityMolHighlightManager hM = UnityMolMain.getHighlightManager();
        hM.HighlightAtoms(currentSelection);
#endif
    }

    public bool AddSelectionKeyword(string keyword, string selectionName) {

        checkStructureNameAlreadyInKeywords();

        if (MDAnalysisSelection.predefinedKeywords.Contains(keyword)) {
            Debug.LogError("Cannot add the keyword '" + keyword + "' because it is a predefined keyword");
            return false;
        }
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.nameToStructure.ContainsKey(keyword)) {
            Debug.LogError("Cannot add the keyword '" + keyword + "' because it is the name of a loaded molecule");
            return false;
        }
        if (!selections.ContainsKey(selectionName)) {
            Debug.LogError("Selection '" + selectionName + "' does not exist");
            return false;
        }
        if (keyword.Contains(")") || keyword.Contains("(") || keyword.Contains(" ") || keyword.Contains("\t") || keyword.Contains("\n") || string.IsNullOrEmpty(keyword)) {
            Debug.LogError("Cannot add the keyword '" + keyword + "' because it contains forbidden characters");
            return false;
        }
        selectionMDAKeywords[keyword] = selectionName;

        return true;
    }

    void checkStructureNameAlreadyInKeywords() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        foreach (string n in sm.nameToStructure.Keys) {
            if (selectionMDAKeywords.ContainsKey(n)) {
                RemoveSelectionKeyword(n);
            }
        }
    }

    public void RemoveSelectionKeyword(string keyword) {
        if (selectionMDAKeywords.ContainsKey(keyword)) {
            selectionMDAKeywords.Remove(keyword);
        }
    }

    public string findNewSelectionName(string selName) {
        int toAdd = 2;
        string result = selName + "_" + toAdd.ToString();
        while (selections.ContainsKey(result)) {
            toAdd++;
            result = selName + "_" + toAdd.ToString();
        }
        return result;
    }
}
}