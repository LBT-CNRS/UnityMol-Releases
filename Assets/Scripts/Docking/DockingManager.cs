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
using System.IO;
using System.Linq;
using UMol.Docking;
using UMol.API;

namespace UMol {
public class DockingManager : MonoBehaviour {

    public bool useSoundFeedback = false;
    public bool dockingSpecificRepresentations = false;

    public bool playSound = true;
    public float timeBetweenSounds = 1.0f;
    private bool stopSound = false;
    private AudioSource audioS;
    private AudioClip audioClip;
    const int nbEnergyValues = 10;
    Queue<float> energyValues = new Queue<float>();
    private AudioClip goodClip;
    private AudioClip badClip;

    Vector3[] posBuffer;
    int curCountAtoms = 0;

    public DockingNBEnergyThread calcNBEnergy;

    private ExplosionSpawner exploSpawner;

    public bool isRunning = false;

    private int savedState = 0;

    public float percentageAccResidue = 0.2f;

    public float VDWUIScaling = 1.0f;
    public float ElecUIScaling = 1.0f;
    public bool collidersOn = false;


    void OnEnable() {
        UnityMolStructureManager.OnMoleculeLoaded += stopDockingMode;
        UnityMolStructureManager.OnMoleculeDeleted += stopDockingMode;
    }


    void OnDisable() {
        UnityMolStructureManager.OnMoleculeLoaded -= stopDockingMode;
        UnityMolStructureManager.OnMoleculeDeleted -= stopDockingMode;
    }



    public void stopDockingMode() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (calcNBEnergy != null) {
            DisableCollidersRigidbodies();

            GameObject.DestroyImmediate(calcNBEnergy);
        }
        calcNBEnergy = null;
        stopSound = true;
        if (isRunning)
            Debug.Log("Stopping docking mode");
        isRunning = false;
    }

    public void startDockingMode() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        if (sm.loadedStructures.Count == 0) {
            Debug.LogError("Cannot load docking mode, no loaded molecule");
            return;
        }

        List<UnityMolStructure> structureList = new List<UnityMolStructure>();
        foreach (UnityMolStructure s in sm.loadedStructures) {
            if (s.trajectoryLoaded || s.mddriverM != null) {
                Debug.LogError("Cannot load docking mode, a trajectory or an IMD session is running on a structure");
                return;
            }
            if (!s.ignoreDocking) {
                structureList.Add(s);
            }
        }

        if (calcNBEnergy == null) {
            calcNBEnergy = gameObject.AddComponent<DockingNBEnergyThread>();
        }

        if (exploSpawner == null) {
            exploSpawner = gameObject.AddComponent<ExplosionSpawner>();
            exploSpawner.exploPrefab = Resources.Load<PooledObject>("Prefabs/PooledExplosion");
        }


        if (calcNBEnergy.isSetup == false) {

            calcNBEnergy.structures = structureList;

            curCountAtoms = getAtomCount(calcNBEnergy.structures);

            posBuffer = new Vector3[curCountAtoms];

            calcNBEnergy.colliderPositions = getWorldPositions(calcNBEnergy.structures, curCountAtoms);

            calcNBEnergy.Setup();

            calcNBEnergy._isPause = false;
        }

        EnableCollidersRigidbodies();

        isRunning = true;

        if (audioS == null) {
            audioS = gameObject.AddComponent<AudioSource>();
            AudioClip c = (AudioClip) Resources.Load("Sounds/beep3");
            audioS.clip = c;
        }

        energyValues.Clear();

        float elec = calcNBEnergy.nbEnergies.elec;
        float vdw = calcNBEnergy.nbEnergies.vdw;
        float total = elec + vdw;

        for (int i = 0; i < nbEnergyValues; i++) {
            energyValues.Enqueue(total);
        }

        if (useSoundFeedback) {
            StartCoroutine(playSoundLoop());
        }
    }

    public void DisableColliders() {
        if (isRunning && collidersOn) {
            DisableCollidersRigidbodies();
        }
    }

    public void EnableColliders() {
        if (isRunning && !collidersOn) {
            EnableCollidersRigidbodies();
        }
    }

    private int getAtomCount(List<UnityMolStructure> structures) {
        int count = 0;
        foreach (var s in structures) {
            count += s.Count;
        }
        return count;
    }
    private Vector3[] getWorldPositions(List<UnityMolStructure> structures, int fullSize) {
        if (posBuffer == null || posBuffer.Length != fullSize) {
            posBuffer = new Vector3[fullSize];
        }
        int cumul = 0;
        foreach (UnityMolStructure s in structures) {
            s.getWorldPositions(ref posBuffer, cumul);
            cumul += s.Count;
        }
        return posBuffer;
    }

    string manageSavingFolder() {
        string directorypath = "";

        directorypath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/UnityMolDocking/";

        if (!System.IO.Directory.Exists(directorypath)) {
            // Create the folder
            System.IO.Directory.CreateDirectory(directorypath);
        }

        return directorypath;
    }

    /// By default, docking mode activates colliders on Calpha atoms
    /// This function activates colliders for a specified atom name or atom type
    /// Setting atomName and atomType to null will enable colliders for all atoms
    public void EnableCollidersCustomAtoms(string atomName, string atomType) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (isRunning) {

            int count = 0;
            foreach (UnityMolStructure s in calcNBEnergy.structures) {
                GameObject go = sm.GetStructureGameObject(s.name);
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb == null) {
                    rb = go.AddComponent<Rigidbody>();
                    go.AddComponent<CollisionsVRDocking>().spawner = exploSpawner;

                    rb.isKinematic = false;
                    rb.useGravity = false;
                    rb.mass = 100f;
                    rb.drag = 200f;
                    rb.angularDrag = 200f;

                }
                int idA = 0;
                foreach (UnityMolAtom a in s.currentModel.allAtoms) {
                    if (string.IsNullOrEmpty(atomName) || a.name == atomName) {
                        if (string.IsNullOrEmpty(atomType) || a.type == atomType) {
                            GameObject goa = UnityMolMain.getAnnotationManager().getGO(a);
                            SphereCollider sc = goa.GetComponent<SphereCollider>();
                            if (sc == null)
                                sc = goa.AddComponent<SphereCollider>();
                            sc.radius = a.radius;
                            count++;
                        }
                    }
                    idA++;
                }
            }
            Debug.Log("Activating colliders on " + (string.IsNullOrEmpty(atomName) ? " " : "name " + atomName) +
                      (string.IsNullOrEmpty(atomType) ? "" : "type " + atomType) +
                      "  => " + count + " colliders added");
        }
        collidersOn = true;
    }

    /// This function desactivates colliders for a specified atom name or atom type
    /// To desactivate all colliders, call DisableCollidersRigidbodies
    public void DisableCollidersCustomAtoms(string atomName, string atomType) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        if (isRunning) {
            foreach (UnityMolStructure s in calcNBEnergy.structures) {
                GameObject go = sm.GetStructureGameObject(s.name);

                int idA = 0;
                foreach (UnityMolAtom a in s.currentModel.allAtoms) {
                    if (string.IsNullOrEmpty(atomName) || a.name == atomName) {
                        if (string.IsNullOrEmpty(atomType) || a.type == atomType) {
                            GameObject goa = UnityMolMain.getAnnotationManager().getGO(a);
                            SphereCollider sc = goa.GetComponent<SphereCollider>();
                            if (sc != null)
                                Destroy(sc);
                        }
                    }
                    idA++;
                }
            }
        }
    }

    private void EnableCollidersRigidbodies(string atomName = "CA", string atomType = "C") {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        foreach (UnityMolStructure s in calcNBEnergy.structures) {

            //Create rigidbodies
            GameObject go = sm.GetStructureGameObject(s.name);
            Rigidbody rb = go.AddComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.useGravity = false;
            rb.mass = 100f;
            rb.drag = 200f;
            rb.angularDrag = 200f;

            // MeshCollider mc = go.transform.Find("Colliders").GetComponent<MeshCollider>();
            // mc.convex = true;
            // mc.enabled = false;


            //Need to add colliders to detect collisions because convex MeshCollider is too simple
            int idA = 0;
            foreach (UnityMolAtom a in s.currentModel.allAtoms) {
                if (string.IsNullOrEmpty(atomName) || a.name == atomName) {
                    if (string.IsNullOrEmpty(atomType) || a.type == atomType) {
                        GameObject goa = UnityMolMain.getAnnotationManager().getGO(a);
                        SphereCollider sc = goa.AddComponent<SphereCollider>();
                        sc.radius = a.radius * 1.5f;//Bigger than CA
                    }
                }
                idA++;
            }


            go.AddComponent<CollisionsVRDocking>().spawner = exploSpawner;
        }
        collidersOn = true;
    }


    public void DisableCollidersRigidbodies() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        foreach (UnityMolStructure s in calcNBEnergy.structures) {


            GameObject go = sm.GetStructureGameObject(s.name);

            CollisionsVRDocking cvd = go.GetComponent<CollisionsVRDocking>();
            if (cvd != null)
                Destroy(cvd);

            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null)
                Destroy(rb);

            int idA = 0;
            foreach (UnityMolAtom a in s.currentModel.allAtoms) {
                GameObject g = UnityMolMain.getAnnotationManager().getGOIfExists(a);
                if (g != null) {
                    SphereCollider sc = g.GetComponent<SphereCollider>();
                    if (sc != null) {
                        Destroy(sc);
                    }
                }
                idA++;
            }
        }
        collidersOn = false;
    }

    public void FreezeDockingRigidbody(string structureName) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        GameObject go = sm.GetStructureGameObject(structureName);
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
            rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void UnfreezeDockingRigidbody(string structureName) {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        GameObject go = sm.GetStructureGameObject(structureName);
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
            rb.constraints = RigidbodyConstraints.None;
    }

    public string saveDockingState() {

        string directory = manageSavingFolder();
        string filename = "Unitymol_dock_" + savedState.ToString() + ".pdb";
        string path = Path.Combine(directory, filename);

        while (File.Exists(path)) {
            savedState++;
            filename = "Unitymol_dock_" + savedState.ToString() + ".pdb";
            path = Path.Combine(directory, filename);
        }

        saveDockingState(path);
        savedState++;
        return path;
    }



    public void saveDockingState(string path) {

        if (string.IsNullOrEmpty(path)) {
            Debug.LogError("Invalid path!");
            return;
        }

        Vector3[] positions = getWorldPositions(calcNBEnergy.structures, curCountAtoms);
        List<UnityMolAtom> structuresAtoms = calcNBEnergy.structures[0].currentModel.ToAtomList();

        for (int i = 1; i < calcNBEnergy.structures.Count; i++) {
            structuresAtoms.AddRange(calcNBEnergy.structures[i].currentModel.ToAtomList());
        }

        UnityMolSelection structuresSel = new UnityMolSelection(structuresAtoms, newBonds: null, "dockingWrite");

        string pdbData = PDBReader.Write(structuresSel, overridedPos: positions, rewriteChains: true);

        float elec = calcNBEnergy.nbEnergies.elec;
        float vdw = calcNBEnergy.nbEnergies.vdw;
        float total = elec + vdw;


        using (StreamWriter writer = new StreamWriter(path, false))
        {
            // Write energy as REMARK
            writer.WriteLine("REMARK      NON-BONDED ENERGY (kcal/mol): {0}", total.ToString("N3"));
            // Write entire PDB file
            writer.Write(pdbData);
            writer.Close();
        }

        Debug.Log("Saved docking state to: '" + path + "'");
    }

    void Update() {
        if (calcNBEnergy != null) {
            pauseEnergyComputation(false);
            if (calcNBEnergy._isDone) {
                // Get latest energy
                float elec = calcNBEnergy.nbEnergies.elec;
                float vdw = calcNBEnergy.nbEnergies.vdw;
                float total = calcNBEnergy.nbEnergies.elec + calcNBEnergy.nbEnergies.vdw;

                calcNBEnergy.colliderPositions = getWorldPositions(calcNBEnergy.structures, curCountAtoms);
                calcNBEnergy._isDone = false;
                manageSound(total);
            }
            if (dockingSpecificRepresentations) {
                manageRepresentation();
            }
        }
    }

    float meanQueueEnergies() {
        float sum = 0.0f;
        foreach (float v in energyValues) {
            sum += v;
        }
        return sum / energyValues.Count;
    }

    void manageSound(float total) {
        float meanEnergyBefore = meanQueueEnergies();

        energyValues.Dequeue();
        energyValues.Enqueue(total);

        float meanEnergyNow = meanQueueEnergies();

        float EDiff = meanEnergyNow - meanEnergyBefore;
        int minE = -999;
        int maxE = 999;


        float totalClamped = Mathf.Clamp(total, minE, maxE);
        float eclampedTo1 = (totalClamped - minE) / (maxE + maxE) ;
        timeBetweenSounds = 1 - Mathf.Lerp(0.0f, 0.9f, eclampedTo1);

        // else{
        // if(EDiff > 5.0f){
        //  if(badClip == null){
        //      badClip = Resources.Load("Sounds/beep5Bad") as AudioClip;
        //  }
        //  AudioSource.PlayClipAtPoint(badClip, transform.position, 0.1f);
        // }
        // else if( EDiff < -5.0f) {
        //  if(goodClip == null){
        //      goodClip = Resources.Load("Sounds/successShort") as AudioClip;
        //  }
        //  AudioSource.PlayClipAtPoint(goodClip, transform.position, 0.1f);
        // }
    }

    void manageRepresentation() {
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        CustomRaycastBurst raycaster = UnityMolMain.getCustomRaycast();

        for (int i = 0; i < calcNBEnergy.structures.Count - 1; i++) {
            UnityMolStructure s1 = calcNBEnergy.structures[i];
            Vector3 cog1 = s1.currentModel.centroid;
            GameObject sgo = sm.structureToGameObject[s1.name];
            cog1 = sgo.transform.TransformPoint(cog1);

            for (int j = i + 1; j < calcNBEnergy.structures.Count; j++) {
                List<UnityMolAtom> selectedAtoms = new List<UnityMolAtom>();
                List<UnityMolAtom> selectedAtoms2 = new List<UnityMolAtom>();

                UnityMolStructure s2 = calcNBEnergy.structures[j];
                Vector3 cog2 = s2.currentModel.centroid;
                GameObject sgo2 = sm.structureToGameObject[s2.name];
                cog2 = sgo2.transform.TransformPoint(cog2);
                Vector3 normal = (cog2 - cog1).normalized;

                // Debug.DrawLine(cog1, cog2, Color.yellow, 0.1f);

                float scaleFactor = sgo.transform.lossyScale.x;
                foreach (UnityMolAtom a1 in s1.currentModel.allAtoms) {
                    Vector3 ori = a1.curWorldPosition + (normal * a1.radius * scaleFactor);
                    Vector3 p = Vector3.zero;
                    bool isExtrAtom = false;
                    UnityMolAtom a = raycaster.customRaycastAtomBurst(ori, normal, ref p, ref isExtrAtom, false);
                    if (a != null && a.residue.chain.model.structure == s2) {
                        selectedAtoms.Add(a1);
                        selectedAtoms2.Add(a);
                        // foreach(UnityMolAtom ainR in a1.residue.atoms.Values){
                        // selectedAtoms.Add(ainR);
                        // }
                        // foreach(UnityMolAtom ainR in a.residue.atoms.Values){
                        // selectedAtoms2.Add(ainR);
                        // }
                    }
                }

                foreach (UnityMolAtom a2 in s2.currentModel.allAtoms) {
                    Vector3 ori = a2.curWorldPosition - (normal * a2.radius * scaleFactor);
                    Vector3 p = Vector3.zero;
                    bool isExtrAtom = false;
                    UnityMolAtom a = raycaster.customRaycastAtomBurst(ori, -normal, ref p, ref isExtrAtom, false);
                    if (a != null && a.residue.chain.model.structure == s1) {
                        selectedAtoms2.Add(a2);
                        selectedAtoms.Add(a);
                        // foreach(UnityMolAtom ainR in a2.residue.atoms.Values){
                        // selectedAtoms2.Add(ainR);
                        // }
                        // foreach(UnityMolAtom ainR in a.residue.atoms.Values){
                        // selectedAtoms.Add(ainR);
                        // }
                    }
                }
                // selectedAtoms = selectedAtoms.Distinct().ToList();
                // selectedAtoms2 = selectedAtoms2.Distinct().ToList();

                //If x% of the residue atoms are in the selection, add the full residue
                //Otherwise remove the atoms from the selection

                HashSet<UnityMolAtom> selset1 = new HashSet<UnityMolAtom>(selectedAtoms);
                HashSet<UnityMolAtom> selset2 = new HashSet<UnityMolAtom>(selectedAtoms2);

                HashSet<UnityMolAtom> toRemove1 = new HashSet<UnityMolAtom>();
                HashSet<UnityMolAtom> toRemove2 = new HashSet<UnityMolAtom>();

                foreach (UnityMolAtom a in selset1) {
                    if (!toRemove1.Contains(a)) {
                        UnityMolResidue r = a.residue;
                        int cptRAtoms = r.atoms.Count;
                        int cptAtomFromResInSel = 0;
                        foreach (UnityMolAtom ar in r.atoms.Values) {
                            if (selset1.Contains(ar)) {
                                cptAtomFromResInSel++;
                            }
                        }
                        if (cptAtomFromResInSel / cptRAtoms < percentageAccResidue) {
                            foreach (UnityMolAtom ar in r.atoms.Values) {
                                toRemove1.Add(ar);
                            }
                        }
                    }
                }
                foreach (UnityMolAtom a in selset2) {
                    if (!toRemove2.Contains(a)) {
                        UnityMolResidue r = a.residue;
                        int cptRAtoms = r.atoms.Count;
                        int cptAtomFromResInSel = 0;
                        foreach (UnityMolAtom ar in r.atoms.Values) {
                            if (selset2.Contains(ar)) {
                                cptAtomFromResInSel++;
                            }
                        }
                        if (cptAtomFromResInSel / cptRAtoms < percentageAccResidue) {
                            foreach (UnityMolAtom ar in r.atoms.Values) {
                                toRemove2.Add(ar);
                            }
                        }
                    }
                }
                foreach (UnityMolAtom a in toRemove1) {
                    if (selset1.Contains(a)) {
                        selset1.Remove(a);
                    }
                }
                foreach (UnityMolAtom a in toRemove2) {
                    if (selset2.Contains(a)) {
                        selset2.Remove(a);
                    }
                }

                selectedAtoms = selset1.ToList();
                selectedAtoms2 = selset2.ToList();

                string selName = "dynamicDockingSelection_" + s1.name + "_" + i;
                string selName2 = "dynamicDockingSelection_" + s2.name + "_" + j;

                if (!selM.selections.ContainsKey(selName)) {
                    UnityMolSelection sel = new UnityMolSelection(selectedAtoms, selName);
                    selM.Add(sel);
                }
                else {
                    UnityMolSelection sel = selM.selections[selName];
                    sel.atoms = selectedAtoms;
                }

                if (!selM.selections.ContainsKey(selName2)) {
                    UnityMolSelection sel = new UnityMolSelection(selectedAtoms2, selName2);
                    selM.Add(sel);
                }
                else {
                    UnityMolSelection sel = selM.selections[selName2];
                    sel.atoms = selectedAtoms2;
                    selM.Add(sel);
                }

                //DEBUG
                // foreach (UnityMolAtom a1 in s1.currentModel.allAtoms) {
                //     Vector3 ori = a1.curWorldPosition + (normal * a1.radius * scaleFactor);
                //     if(a1.type == "C"){
                //         UnityMolAtom afound = raycaster.customRaycastAtomBurst(ori, normal);
                //         if(afound == null){
                //             Debug.DrawRay(ori, normal, Color.red, 0.1f);
                //         }
                //         else if(afound.residue.chain.model.structure == s2){
                //             Debug.DrawLine(ori, afound.curWorldPosition, Color.green, 0.1f);
                //         }
                //         else{
                //             Debug.DrawLine(ori, afound.curWorldPosition, Color.yellow, 0.1f);
                //         }
                //     }
                //     // UnityMolAtom a = raycaster.customRaycastAtomBurst(a1.curWorldPosition, normal);
                //     // if (a != null && a.residue.chain.model.structure == s2) {
                //     // }
                // }

                APIPython.showSelection(selName, "hb");
                APIPython.showSelection(selName2, "hb");
            }
        }

        //Do selections that updates every frame to show the interface between molecules => slow bc of the KDTree implementation
        // foreach (UnityMolStructure s in calcNBEnergy.structures){
        //  string selName = "dynamicDockingSelection_"+s.name;
        //  string selMDA = "byres not "+s.name+" and around 4.0 "+s.name;
        //  if(!selM.selections.ContainsKey(selName)){
        //      UnityMolSelection sel = APIPython.select(selMDA, selName, silent: true, forceCreate: true);
        //      sel.forceGlobalSelection = true;
        //      APIPython.showSelection(sel.name, "hb");
        //  }
        //  else{
        //      APIPython.updateSelectionWithMDA(selName, selMDA, silent: true);
        //  }
        // }
    }

    public void pauseEnergyComputation(bool pause) {
        if (!pause) {
            calcNBEnergy._isPause = false;
            isRunning = true;
        }
        else {
            calcNBEnergy._isPause = true;
            isRunning = false;
        }
    }

    IEnumerator playSoundLoop() {
        while (true) {
            if (stopSound) {
                break;
            }
            if (isRunning) {
                yield return new WaitForSeconds(timeBetweenSounds);
                audioS.Play();
            }
        }
    }

}
}