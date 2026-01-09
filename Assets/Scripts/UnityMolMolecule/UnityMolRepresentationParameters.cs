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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System;
using System.Globalization;

namespace UMol {

/// This class is used to save representations parameters
public class UnityMolRepresentationParameters {

    public RepType repT;

    // public Material mat;

    public colorType colorationType;
    public Color32 fullColor;

    //Shared
    public Dictionary<UnityMolAtom, Color32> colorPerAtom;

    public float smoothness;
    public float metal;

    public bool useHET;
    public bool useWat;
    public bool shadow;

    public int textureId;

    public bool sideChainsOn;
    public bool hydrogensOn;
    public bool backboneOn;

    public Color bfactorStartColor;
    public Color bfactorEndColor;
    public Color bfactorMidColor;

    //Surface
    public SurfMethod surfMethod;
    public float surfAlpha;
    public float surfProbeRad;
    public bool surfCutSurface;
    public bool surfCutByChain;
    public bool surfAO;
    public bool surfIsTransparent;
    public bool surfIsWireframe;
    public float surfWireframeSize;

    public bool surfLimitedView;
    public Vector3 surfLimitedViewCenter;
    public float surfLimitedViewRadius;

    //DXSurface
    public float DXsurfIso;

    //Sheherasade
    public bool sheheBezier;

    //Fieldlines
    public int FLnbIter;
    public float FLmagThresh;
    public float FLwidth;
    public float FLlength;
    public float FLspeed;
    public Color32 FLstartCol;
    public Color32 FLendCol;

    //OptiHS
    public int HSIdTex;
    public float HSShrink;
    public float HSScale;

    //OptiHB
    public float HBScale;
    public bool HB_AO;
    public int HBIdTex;

    //Line
    public float LineWidth;

    //Point
    public float PointSize;

    //Sphere
    // public float SphereSize;

    //HbondsTube
    public float HBTHeight;
    public float HBTSpace;
    public float HBTRadius;
    public bool HBCustomBonds;//Both Hbond & Hbondtube

    //Cartoon
    public float CartoonTubeSize;
    public bool CartoonAsTube;
    public bool CartoonAsBFactorTube;
    public bool CartoonTransparent;
    public float CartoonAlpha;
    public bool CartoonLimitedView;
    public Vector3 CartoonLimitedViewCenter;
    public float CartoonLimitedViewRadius;

    //Tube
    public float TubeWidth;

    //SugarRibbons
    public bool SRplanes;


    public SerializedRepresentation Serialize(UnityMolSelection sel) {
        SerializedRepresentation rep = new SerializedRepresentation();
        rep.selName = sel.name;
        rep.atomtype = (int)repT.atomType;
        rep.bondtype = (int)repT.bondType;
        rep.colorationType = (int)colorationType;
        rep.fullColor = fullColor;
        rep.smoothness = smoothness;
        rep.metal = metal;
        rep.useHET = useHET;
        rep.useWat = useWat;
        rep.shadow = shadow;
        rep.textureId = textureId;
        rep.sideChainsOn = sideChainsOn;
        rep.hydrogensOn = hydrogensOn;
        rep.backboneOn = backboneOn;
        rep.bfactorStartColor = bfactorStartColor;
        rep.bfactorEndColor = bfactorEndColor;
        rep.bfactorMidColor = bfactorMidColor;
        rep.surfMethod = (int)surfMethod;
        rep.surfAlpha = surfAlpha;
        rep.surfProbeRad = surfProbeRad;
        rep.surfCutSurface = surfCutSurface;
        rep.surfCutByChain = surfCutByChain;
        rep.surfAO = surfAO;
        rep.surfIsTransparent = surfIsTransparent;
        rep.surfIsWireframe = surfIsWireframe;
        rep.surfWireframeSize = surfWireframeSize;
        rep.surfLimitedView = surfLimitedView;
        rep.surfLimitedViewCenter = surfLimitedViewCenter;
        rep.surfLimitedViewRadius = surfLimitedViewRadius;
        rep.DXsurfIso = DXsurfIso;
        rep.sheheBezier = sheheBezier;
        rep.FLnbIter = FLnbIter;
        rep.FLmagThresh = FLmagThresh;
        rep.FLwidth = FLwidth;
        rep.FLlength = FLlength;
        rep.FLspeed = FLspeed;
        rep.FLstartCol = FLstartCol;
        rep.FLendCol = FLendCol;
        rep.HSIdTex = HSIdTex;
        rep.HSShrink = HSShrink;
        rep.HSScale = HSScale;
        rep.HBScale = HBScale;
        rep.HB_AO = HB_AO;
        rep.HBIdTex = HBIdTex;
        rep.LineWidth = LineWidth;
        rep.PointSize = PointSize;
        rep.HBTHeight = HBTHeight;
        rep.HBTSpace = HBTSpace;
        rep.HBTRadius = HBTRadius;
        rep.HBCustomBonds = HBCustomBonds;
        rep.CartoonTubeSize = CartoonTubeSize;
        rep.CartoonAsTube = CartoonAsTube;
        rep.CartoonAsBFactorTube = CartoonAsBFactorTube;
        rep.CartoonTransparent = CartoonTransparent;
        rep.CartoonAlpha = CartoonAlpha;
        rep.CartoonLimitedView = CartoonLimitedView;
        rep.CartoonLimitedViewCenter = CartoonLimitedViewCenter;
        rep.CartoonLimitedViewRadius = CartoonLimitedViewRadius;
        rep.TubeWidth = TubeWidth;
        rep.SRplanes = SRplanes;

        if (colorPerAtom != null && colorPerAtom.Count != 0) {
            rep.colorPerAtom = new SerializedColorPerAtom();
            rep.colorPerAtom.idInSel = new List<int>(sel.Count);
            rep.colorPerAtom.colors = new List<int>(4 * sel.Count);
            for (int i = 0; i < sel.Count; i++) {
                rep.colorPerAtom.idInSel.Add(i);
                Color32 col = colorPerAtom[sel.atoms[i]];
                rep.colorPerAtom.colors.Add(col.r);
                rep.colorPerAtom.colors.Add(col.g);
                rep.colorPerAtom.colors.Add(col.b);
                rep.colorPerAtom.colors.Add(col.a);
            }
        }
        return rep;
    }
}

public enum colorType {
    atom,
    res,
    chain,
    hydro,
    seq,
    charge,
    restype,
    rescharge,
    resid,
    resnum,
    bfactor,
    full,//one color
    defaultCartoon,
    custom
}

[Serializable]
public class SerializedStructure {
    public string name;
    public string path;
    public string pdbID;
    public string trajPath;
    public string dxPath;
    public bool fromPath;
    public bool fetchmmCif;
    public bool bioAssembly;
    public bool modelsAsTraj;
    public bool trajLoaded;
    public bool dxLoaded;
    public bool ssFromFile;
    public bool ignoreDocking;
    public bool readHET;
    public int currentModel;
    public int currentFrameId;
    public int currentFrameTraj;
    public int groupId;
    public int structureType;
    //MDDriver ?
}

[Serializable]
public class SerializedSelection {
    public string name;
    public string query;
    public int count;
    public List<string> structureNames;
    public List<int> structureIds;
    public List<int> atomIds;
}
[Serializable]
public class SerializedColorPerAtom {
    public List<int> idInSel;
    public List<int> colors;

    public Dictionary<UnityMolAtom, Color32> ToDictionary(UnityMolSelection sel) {
        Dictionary<UnityMolAtom, Color32> ret = new Dictionary<UnityMolAtom, Color32>(sel.Count);

        int i = 0;
        foreach (int id in idInSel) {
            if (id >= 0 && id < sel.Count) {
                Color32 col = new Color32((byte)colors[i], (byte)colors[i + 1], (byte)colors[i + 2], (byte)colors[i + 3]);
                ret[sel.atoms[id]] = col;
            }
            i += 4;
        }

        return ret;
    }
}
[Serializable]
public class SerializedRepresentation {
    public string selName;
    public int atomtype;
    public int bondtype;
    public int colorationType;
    public Color32 fullColor;
    public float smoothness;
    public float metal;
    public bool useHET;
    public bool useWat;
    public bool shadow;
    public int textureId;
    public bool sideChainsOn;
    public bool hydrogensOn;
    public bool backboneOn;
    public Color32 bfactorStartColor;
    public Color32 bfactorEndColor;
    public Color32 bfactorMidColor;
    public int surfMethod;
    public float surfAlpha;
    public float surfProbeRad;
    public bool surfCutSurface;
    public bool surfCutByChain;
    public bool surfAO;
    public bool surfIsTransparent;
    public bool surfIsWireframe;
    public float surfWireframeSize;
    public bool surfLimitedView;
    public Vector3 surfLimitedViewCenter;
    public float surfLimitedViewRadius;
    public float DXsurfIso;
    public bool sheheBezier;
    public int FLnbIter;
    public float FLmagThresh;
    public float FLwidth;
    public float FLlength;
    public float FLspeed;
    public Color32 FLstartCol;
    public Color32 FLendCol;
    public int HSIdTex;
    public float HSShrink;
    public float HSScale;
    public float HBScale;
    public bool HB_AO;
    public int HBIdTex;
    public float LineWidth;
    public float PointSize;
    public float HBTHeight;
    public float HBTSpace;
    public float HBTRadius;
    public bool HBCustomBonds;
    public float CartoonTubeSize;
    public bool CartoonAsTube;
    public bool CartoonAsBFactorTube;
    public bool CartoonTransparent;
    public float CartoonAlpha;
    public bool CartoonLimitedView;
    public Vector3 CartoonLimitedViewCenter;
    public float CartoonLimitedViewRadius;
    public float TubeWidth;
    public bool SRplanes;
    public SerializedColorPerAtom colorPerAtom;

    public void FuseHB(SerializedRepresentation hsrep) {
        bondtype = hsrep.bondtype;
        HSIdTex = hsrep.HSIdTex;
        HSShrink = hsrep.HSShrink;
        HSScale = hsrep.HSScale;
    }

    public UnityMolRepresentationParameters toRepParameters(UnityMolSelection sel) {
        UnityMolRepresentationParameters ret = new UnityMolRepresentationParameters();
        ret.repT.atomType = (AtomType)atomtype;
        ret.repT.bondType = (BondType)bondtype;
        ret.colorationType = (colorType)colorationType;
        ret.fullColor = fullColor;
        ret.colorPerAtom = colorPerAtom.ToDictionary(sel);

        ret.smoothness = smoothness;
        ret.metal = metal;
        ret.useHET = useHET;
        ret.useWat = useWat;
        ret.shadow = shadow;
        ret.textureId = textureId;
        ret.sideChainsOn = sideChainsOn;
        ret.hydrogensOn = hydrogensOn;
        ret.backboneOn = backboneOn;
        ret.bfactorStartColor = bfactorStartColor;
        ret.bfactorEndColor = bfactorEndColor;
        ret.bfactorMidColor = bfactorMidColor;
        ret.surfMethod = (SurfMethod)surfMethod;
        ret.surfAlpha = surfAlpha;
        ret.surfProbeRad = surfProbeRad;
        ret.surfCutSurface = surfCutSurface;
        ret.surfCutByChain = surfCutByChain;
        ret.surfAO = surfAO;
        ret.surfIsTransparent = surfIsTransparent;
        ret.surfIsWireframe = surfIsWireframe;
        ret.surfWireframeSize = surfWireframeSize;
        ret.surfLimitedView = surfLimitedView;
        ret.surfLimitedViewCenter = surfLimitedViewCenter;
        ret.surfLimitedViewRadius = surfLimitedViewRadius;
        ret.DXsurfIso = DXsurfIso;
        ret.sheheBezier = sheheBezier;
        ret.FLnbIter = FLnbIter;
        ret.FLmagThresh = FLmagThresh;
        ret.FLwidth = FLwidth;
        ret.FLlength = FLlength;
        ret.FLspeed = FLspeed;
        ret.FLstartCol = FLstartCol;
        ret.FLendCol = FLendCol;
        ret.HSIdTex = HSIdTex;
        ret.HSShrink = HSShrink;
        ret.HSScale = HSScale;
        ret.HBScale = HBScale;
        ret.HB_AO = HB_AO;
        ret.HBIdTex = HBIdTex;
        ret.LineWidth = LineWidth;
        ret.PointSize = PointSize;
        ret.HBTHeight = HBTHeight;
        ret.HBTSpace = HBTSpace;
        ret.HBTRadius = HBTRadius;
        ret.HBCustomBonds = HBCustomBonds;
        ret.CartoonTubeSize = CartoonTubeSize;
        ret.CartoonAsTube = CartoonAsTube;
        ret.CartoonAsBFactorTube = CartoonAsBFactorTube;
        ret.CartoonTransparent = CartoonTransparent;
        ret.CartoonAlpha = CartoonAlpha;
        ret.CartoonLimitedView = CartoonLimitedView;
        ret.CartoonLimitedViewCenter = CartoonLimitedViewCenter;
        ret.CartoonLimitedViewRadius = CartoonLimitedViewRadius;
        ret.TubeWidth = TubeWidth;
        ret.SRplanes = SRplanes;

        return ret;
    }
}

[Serializable]
public class SerializedAnnotation {
    public List<int> structureIds;
    public List<int> atomIds;

    public bool showLine;
    public int annoType;
    public int id;
    public float size;
    public float size2;
    public Color color;
    public string content;
    public Vector2 posPercent;
    public List<Vector3> positions;
}

[Serializable]
public class SerializedUMolSession {
    public List<SerializedStructure> structures;
    public List<SerializedSelection> selections;
    public List<SerializedRepresentation> representations;
    public List<SerializedAnnotation> annotations;
}
[Serializable]
public class SerializedRoot {
    public SerializedUMolSession UMolSession;

    public void restoreStructures() {
        if (UMolSession == null || UMolSession.structures == null || UMolSession.structures.Count == 0) {
            Debug.LogWarning("No structure to restore");
            return;
        }

        UnityMolStructureManager sM = UnityMolMain.getStructureManager();

        foreach (var sstruc in UMolSession.structures) {
            bool doRename = true;
            if (sM.nameToStructure.ContainsKey(sstruc.name)) {
                Debug.LogWarning("A structure named '" + sstruc.name + "' is already loaded, name will differ");
                doRename = false;
            }

            UnityMolStructure newStruct = null;
            if (!sstruc.fromPath) {
                if (sstruc.fetchmmCif) {
                    PDBxReader rx = new PDBxReader();
                    rx.ModelsAsTraj = sstruc.modelsAsTraj;

                    newStruct = rx.Fetch(sstruc.pdbID, readHet : sstruc.readHET, forceType : sstruc.structureType, bioAssembly : sstruc.bioAssembly);
                } else {
                    if (sstruc.bioAssembly) {
                        Debug.LogWarning("Biological Assembly data are available only for mmCIF");
                    }
                    PDBReader r = new PDBReader();
                    r.ModelsAsTraj = sstruc.modelsAsTraj;
                    newStruct = r.Fetch(sstruc.pdbID, readHet : sstruc.readHET, forceType : sstruc.structureType);
                }

                if (!sstruc.ssFromFile) {
                    DSSP.assignSS_DSSP(newStruct);
                }

                newStruct.readHET = sstruc.readHET;
                newStruct.modelsAsTraj = sstruc.modelsAsTraj;
                newStruct.fetchedmmCIF = sstruc.fetchmmCif;
                newStruct.pdbID = sstruc.pdbID;
                newStruct.bioAssembly = sstruc.bioAssembly;

            }
            else {//From path

                Reader r = Reader.GuessReaderFrom(sstruc.path);
                if (r != null) {
                    r.ModelsAsTraj = sstruc.modelsAsTraj;
                    newStruct = r.Read(readHet: sstruc.readHET, forceType: sstruc.structureType);

                    if (newStruct != null) {

                        if (!sstruc.ssFromFile) {
                            DSSP.assignSS_DSSP(newStruct);
                        }

                    } else {
                        Debug.LogError("Could not load file " + sstruc.path);
                    }
                }
            }

            if (newStruct.name != sstruc.name && doRename) {
                GameObject go = sM.structureToGameObject[newStruct.name];

                sM.nameToStructure.Remove(newStruct.name);
                newStruct.name = sstruc.name;
                sM.nameToStructure[sstruc.name] = newStruct;
                go.name = newStruct.ToSelectionName();
            }

            newStruct.groupID = sstruc.groupId;
            newStruct.currentModelId = sstruc.currentModel;
            if (sstruc.currentFrameId != 0) {
                newStruct.setModel(sstruc.currentFrameId);
            }

            if (sstruc.dxLoaded) {
                newStruct.readDX(sstruc.dxPath);
            }

            if (sstruc.trajLoaded) {
                newStruct.readTrajectoryXDR(sstruc.trajPath);
                newStruct.createTrajectoryPlayer();
                newStruct.trajPlayer.play = false;
                newStruct.trajSetFrame(sstruc.currentFrameTraj);
            }
        }
    }
    public void restoreSelections(bool useSelectionQuery = false) {
        if (UMolSession == null || UMolSession.selections == null || UMolSession.selections.Count == 0) {
            Debug.LogWarning("No selection to restore");
            return;
        }

        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolStructureManager sM = UnityMolMain.getStructureManager();

        List<UnityMolAtom> tempAtomList = new List<UnityMolAtom>();
        List<string> structureNames = new List<string>();

        StringBuilder sbwarning = new StringBuilder();
        foreach (var ssel in UMolSession.selections) {
            bool allStrucLoaded = true;
            string missingStrucName = "";

            //Check if all structure needed for this selection are loaded
            foreach (string sname in ssel.structureNames) {
                if (!sM.nameToStructure.ContainsKey(sname)) {
                    missingStrucName = sname;
                    allStrucLoaded = false;
                    break;
                }
            }
            if (!allStrucLoaded) {
                Debug.LogError("Cannot restore selection named '" + ssel.name + "'. The structure '" + missingStrucName + "' is not loaded.");
                continue;
            }

            UnityMolSelection ret = null;

            if (useSelectionQuery) {//Use the selection query
                if (ssel.structureNames.Count == 1) {
                    UnityMolStructure s = sM.nameToStructure[ssel.structureNames[0]];
                    MDAnalysisSelection selec = new MDAnalysisSelection(ssel.query, s.currentModel.allAtoms);
                    ret = selec.process();
                    ret.name = ssel.name;

                }
                else {
                    //Fill the list of atoms we need to compute the selection query
                    if (!sameStructureNames(ssel.structureNames, structureNames)) {
                        tempAtomList.Clear();
                        foreach (string sname in ssel.structureNames) {
                            tempAtomList.AddRange(sM.nameToStructure[sname].currentModel.allAtoms);
                        }
                        structureNames = ssel.structureNames;
                    }
                    MDAnalysisSelection selec = new MDAnalysisSelection(ssel.query, tempAtomList);
                    ret = selec.process();
                    ret.name = ssel.name;
                }

            }
            else {//Use atom ids => default behavior
                if (ssel.structureNames.Count == 1) {
                    UnityMolStructure s = sM.nameToStructure[ssel.structureNames[0]];
                    List<UnityMolAtom> atoms = new List<UnityMolAtom>(s.Count);
                    foreach (int id in ssel.atomIds) {
                        if (id >= 0 && id < s.currentModel.Count) {
                            atoms.Add(s.currentModel.allAtoms[id]);
                        }
                        else {
                            sbwarning.Append("Couldn't get the atom " + id + " restoring selection " + ssel.name + "\n");
                        }
                    }
                    ret = new UnityMolSelection(atoms, newBonds: null, ssel.name);
                }
                else {
                    List<UnityMolAtom> atoms = new List<UnityMolAtom>();
                    UnityMolStructure s = null;
                    for (int i = 0; i < ssel.structureIds.Count; i++) {
                        if (s == null || ssel.structureIds[i - 1] != ssel.structureIds[i]) {
                            string sname = ssel.structureNames[ssel.structureIds[i]];
                            s = sM.nameToStructure[sname];
                        }
                        atoms.Add(s.currentModel.allAtoms[ssel.atomIds[i]]);
                    }

                    ret = new UnityMolSelection(atoms, newBonds: null, ssel.name);
                }
            }

            if (ret.Count != ssel.count)
                Debug.LogWarning("Atom count in the selection is different from the JSON file information");

            //Already contains this selection = overwrite
            if (selM.selections.ContainsKey(ret.name)) {
                bool save = selM.selections[ret.name].isAlterable;
                selM.selections[ret.name].isAlterable = true;
                
                selM.selections[ret.name].atoms = ret.atoms;
                if (!selM.selections[ret.name].bondsNull) {
                    selM.selections[ret.name].fillBonds();
                }
                selM.selections[ret.name].fillStructures();

                selM.selections[ret.name].isAlterable = save;
            }
            else
                selM.Add(ret);
        }
        if (sbwarning.Length != 0)
            Debug.LogWarning(sbwarning.ToString());
    }
    public void restoreRepresentations() {
        if (UMolSession == null || UMolSession.representations == null || UMolSession.representations.Count == 0) {
            Debug.LogWarning("No representation to restore");
            return;
        }
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();

        foreach (var srep in UMolSession.representations) {
            if (!selM.selections.ContainsKey(srep.selName)) {
                Debug.LogError("Could not restore representation on selection '" + srep.selName + "' because the selection does not exist.");
                continue;
            }

            UnityMolSelection sel = selM.selections[srep.selName];
            AtomType atype = (AtomType)srep.atomtype;
            BondType btype = (BondType)srep.bondtype;

            UnityMolRepresentationParameters reppar = srep.toRepParameters(sel);

            UnityMolRepresentation rep = repManager.AddRepresentation(sel, atype, btype);
            foreach (SubRepresentation sr in rep.subReps) {
                if (sr.atomRepManager != null)
                    sr.atomRepManager.Restore(reppar);
                if (sr.bondRepManager != null)
                    sr.bondRepManager.Restore(reppar);
            }
        }
    }

    List<UnityMolAtom> getAtoms(SerializedAnnotation anno) {
        List<UnityMolAtom> result = new List<UnityMolAtom>(4);
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();

        for (int i = 0; i < anno.atomIds.Count; i++) {
            int sid = anno.structureIds[i];
            int aid = anno.atomIds[i];
            int N = sm.loadedStructures.Count;
            int offset = N - UMolSession.structures.Count;
            if (sid < 0 || offset + sid >= N )
                return null;
            UnityMolStructure s = sm.loadedStructures[offset + sid];
            if (aid < 0 || aid > s.Count)
                return null;
            UnityMolAtom a = s.currentModel.allAtoms[aid];

            result.Add(a);
        }

        return result;
    }

    public void restoreAnnotations() {
        if (UMolSession == null || UMolSession.annotations == null || UMolSession.annotations.Count == 0) {
            Debug.LogWarning("No annotation to restore");
            return;
        }

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();

        foreach (var sanno in UMolSession.annotations) {
            List<UnityMolAtom> atoms = getAtoms(sanno);
            switch (sanno.annoType) {
            case 0:
                anM.AnnotateAngle(atoms[0], atoms[1], atoms[2]);
                break;
            case 1:
                anM.Annotate2DText(sanno.content, sanno.size, sanno.color, sanno.posPercent);
                break;
            case 2:
                anM.AnnotateCurvedLine(atoms[0], atoms[1], atoms[2]);
                break;
            case 3:
                anM.AnnotateDihedralArrow(atoms[0], atoms[1]);
                break;
            case 4:
                anM.AnnotateWorldLine(sanno.positions[0], sanno.positions[1], UnityMolMain.getRepresentationParent().transform.parent, sanno.size, sanno.color);
                break;
            case 5:
                Vector3 worldP = sanno.positions[0];
                GameObject tmpSpherePar = new GameObject("WorldSphereAnnotation_" + worldP.x.ToString("F3", CultureInfo.InvariantCulture) + "_" +
                        worldP.y.ToString("F3", CultureInfo.InvariantCulture) + "_" + worldP.z.ToString("F3", CultureInfo.InvariantCulture));

                tmpSpherePar.transform.position = worldP;
                anM.AnnotateSphere(tmpSpherePar.transform, sanno.size);

                break;
            case 6:

                Vector3 worldP1 = sanno.positions[0];
                GameObject tmpTextPar = new GameObject("WorldTextAnnotation_" + worldP1.x.ToString("F3", CultureInfo.InvariantCulture) + "_" +
                                                       worldP1.y.ToString("F3", CultureInfo.InvariantCulture) + "_" + worldP1.z.ToString("F3", CultureInfo.InvariantCulture));

                tmpTextPar.transform.parent = UnityMolMain.getRepresentationParent().transform.parent;

                tmpTextPar.transform.localPosition = worldP1;
                tmpTextPar.transform.localRotation = Quaternion.identity;
                tmpTextPar.transform.localScale = Vector3.one;

                anM.AnnotateWorldText(tmpTextPar.transform, sanno.size, sanno.content, sanno.color);
                break;
            case 7:
                anM.AnnotateDistance(atoms[0], atoms[1]);
                break;
            case 8:
                UnityMolStructure s = atoms[0].residue.chain.model.structure;
                int id = anM.AnnotateDrawing(s, sanno.positions, sanno.color);
                break;
            case 9:
                anM.AnnotateLine(atoms[0], atoms[1]);
                break;
            case 10:
                //Not implemented yet
                //anM.AnnotateSound();
                break;
            case 11:
                anM.Annotate(atoms[0]);
                break;
            case 12:
                anM.AnnotateText(atoms[0], sanno.content, sanno.color, sanno.showLine);
                break;
            case 13:
                anM.AnnotateDihedralAngle(atoms[0], atoms[1], atoms[2], atoms[3]);
                break;
            default:
                Debug.LogWarning("Ignoring undefined annotation type");
                break;
            }
        }
    }

    private bool sameStructureNames(List<string> s1, List<string> s2) {
        if (s1.Count != s2.Count)
            return false;
        for (int i = 0; i < s1.Count; i++) {
            if (s1[i] != s2[i])
                return false;
        }
        return true;
    }
}
}

