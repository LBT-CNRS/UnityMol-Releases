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
using UMol.API;


namespace UMol {
public class UnityMolAnnotationManager : MonoBehaviour {

    public bool drawMode = false;

    public static int idDraw = 0;

    /// <summary>
    /// Dictionary of UnityMolAnnotations that can be accessed with UnityMolAtoms
    /// </summary>


    /// <summary>
    /// Hashset of all the UnityMolAnnotations created
    /// </summary>
    public HashSet<UnityMolAnnotation> allAnnotations = new HashSet<UnityMolAnnotation>(new AnnotationComparer());

    /// Keep track of annotations by structure, useful when removing a structure
    private Dictionary<UnityMolStructure, List<UnityMolAnnotation>> structureToAnno = new Dictionary<UnityMolStructure, List<UnityMolAnnotation>>();

    public delegate void AnnotationNew(AnnoEventArgs args);
    public static event AnnotationNew OnNewAnnotation;

    public delegate void AnnotationRemoved(AnnoEventArgs args);
    public static event AnnotationRemoved OnRemoveAnnotation;


    private Dictionary<UnityMolAtom, GameObject> atomToGo = new Dictionary<UnityMolAtom, GameObject>();

    public GameObject getGO(UnityMolAtom a) {
        GameObject go = null;
        if (atomToGo.TryGetValue(a, out go))
            return go;
        var s = a.residue.chain.model.structure;
        go = new GameObject("Atom");
        go.transform.SetParent(s.annotationParent);
        go.transform.localPosition = a.position;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        atomToGo[a] = go;
        return go;
    }
    public GameObject getGOIfExists(UnityMolAtom a) {
        GameObject go = null;
        if (atomToGo.TryGetValue(a, out go))
            return go;
        return null;
    }
    public void setGOPos(UnityMolAtom a, GameObject go) {
        var s = a.residue.chain.model.structure;
        go.transform.SetParent(s.annotationParent);
        go.transform.localPosition = a.position;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
    }


    public void UpdateAtomPositions() {
        foreach (UnityMolAtom a in atomToGo.Keys) {
            if (atomToGo[a]) {
                atomToGo[a].transform.localPosition = a.position;
            }
        }
        // If atom positions have changed, one   need to update the annotations
        foreach (UnityMolAnnotation anno in allAnnotations) {
            anno.Update();
        }
    }

    void Awake() {
        UnityMolStructureManager.OnMoleculeDeleted += updateDeletedAtoms;
    }
    void OnDestroy() {
        UnityMolStructureManager.OnMoleculeDeleted -= updateDeletedAtoms;
    }

    public void updateDeletedAtoms() {
        List<UnityMolAtom> toRM = new List<UnityMolAtom>();
        foreach (UnityMolAtom a in atomToGo.Keys) {
            if (atomToGo[a] == null)
                toRM.Add(a);
        }
        foreach (UnityMolAtom a in toRM) {
            atomToGo.Remove(a);
        }
    }


    //Do the update only if a trajectory is playing
    void Update() {
        if (APIPython.isATrajectoryPlaying()) {
            foreach (UnityMolAnnotation anno in allAnnotations) {
                anno.Update();
            }
        }
        foreach (UnityMolAnnotation anno in allAnnotations) {
            anno.UnityUpdate();
        }
    }

    public void Clean() {
        foreach (UnityMolAnnotation an in allAnnotations) {
            an.Delete();
            if (OnRemoveAnnotation != null) {
                OnRemoveAnnotation(new AnnoEventArgs(an));
            }
        }
        allAnnotations.Clear();
        structureToAnno.Clear();
        atomToGo.Clear();
    }


    public void CleanDrawings() {
        List<UnityMolAnnotation> annos = allAnnotations.ToList();
        foreach (UnityMolAnnotation an in annos) {
            if (an.GetType() == new DrawAnnotation().GetType()) {
                RemoveAnnotation(an);
            }
        }
    }


    public void Show(bool show = true) {
        foreach (UnityMolAnnotation an in allAnnotations) {
            an.Show(show);
        }
    }

    public void RemoveAnnotation(UnityMolAnnotation an) {

        if (allAnnotations.Contains(an)) {
            AnnotationComparer compa = new AnnotationComparer();
            foreach (UnityMolAnnotation ano in allAnnotations) {
                if (compa.Equals(ano, an)) {
                    ano.Delete();
                }
            }
            allAnnotations.Remove(an);
        }


        if (OnRemoveAnnotation != null) {
            OnRemoveAnnotation(new AnnoEventArgs(an));
        }
    }
    public void RemoveAnnotations(UnityMolAtom a) {

        HashSet<UnityMolAnnotation> toRemove = new HashSet<UnityMolAnnotation>();

        AnnotationComparer compa = new AnnotationComparer();

        UnityMolStructure s = a.residue.chain.model.structure;
        foreach (UnityMolAnnotation anno in toRemove) {
            if (allAnnotations.Contains(anno)) {
                foreach (UnityMolAnnotation ano in allAnnotations) {
                    if (compa.Equals(anno, ano)) {
                        ano.Delete();
                        if (structureToAnno.ContainsKey(s))
                            structureToAnno[s].Remove(ano);
                    }
                }
                allAnnotations.Remove(anno);
            }
        }
    }

    public void RemoveAnnotations(UnityMolStructure s) {
        if (structureToAnno.ContainsKey(s)) {
            foreach (UnityMolAnnotation a in structureToAnno[s]) {
                RemoveAnnotation(a);
            }
            if (structureToAnno.ContainsKey(s))
                structureToAnno.Remove(s);
        }
    }

    public void AddAnnotation(UnityMolAnnotation an) {
        allAnnotations.Add(an);

        foreach (UnityMolAtom a in an.atoms) {
            UnityMolStructure s = a.residue.chain.model.structure;
            if (!structureToAnno.ContainsKey(s))
                structureToAnno[s] = new List<UnityMolAnnotation>();
            structureToAnno[s].Add(an);
        }
        if (OnNewAnnotation != null) {
            OnNewAnnotation(new AnnoEventArgs(an));
        }
    }

    public bool AnnotationExists(UnityMolAnnotation annoType) {
        return allAnnotations.Contains(annoType);
    }


    public void Annotate(UnityMolAtom a) {

        SphereAnnotation anno = new SphereAnnotation();
        anno.atoms.Add(a);

        if (AnnotationExists(anno)) {
            Debug.LogWarning("This atom is already annotated with a sphere");
            return;
        }

        anno.annoParent = getGO(a).transform;
        anno.Create();
        AddAnnotation(anno);
    }

    public void AnnotateSphere(Transform par, float sScale) {
        CustomSphereAnnotation anno = new CustomSphereAnnotation();

        if (AnnotationExists(anno)) {
            Debug.LogWarning("This parent is already annotated with a sphere");
            return;
        }

        anno.annoParent = par;
        anno.scale = sScale;
        anno.Create();
        AddAnnotation(anno);
    }

    public void AnnotateText(UnityMolAtom a, string text, Color col, bool withLine) {

        TextAnnotation anno = new TextAnnotation();
        anno.atoms.Add(a);

        if (AnnotationExists(anno)) {
            Debug.LogWarning("This atom is already annotated with a text at the same position, overwritting");
            RemoveAnnotation(anno);
        }

        anno.annoParent = getGO(a).transform;
        anno.content = text;
        anno.showLine = withLine;
        anno.colorText = col;
        anno.Create();
        AddAnnotation(anno);
    }

    public void AnnotateWorldText(Transform par, float scale, string text, Color textCol) {

        CustomTextAnnotation anno = new CustomTextAnnotation();
        anno.content = text;
        anno.annoParent = par;

        if (AnnotationExists(anno)) {
            Debug.LogWarning("There is already an annotation with the same text, overwritting");
            RemoveAnnotation(anno);
        }

        anno.scale = scale;
        anno.colorText = textCol;
        anno.Create();
        AddAnnotation(anno);
    }

    public void Annotate2DText(string text, float scale, Color textCol, Vector2 screenpos) {

        Annotate2D anno = new Annotate2D();

        //Don't test if this already exists

        anno.content = text;
        anno.scale = scale;
        anno.colorText = textCol;
        anno.posPercent = screenpos;
        anno.Create();
        AddAnnotation(anno);
    }

    public void AnnotateDistance(UnityMolAtom a1, UnityMolAtom a2) {
        DistanceAnnotation anno = new DistanceAnnotation();
        anno.atoms.Add(a1);
        anno.atoms.Add(a2);

        if (AnnotationExists(anno)) {
            Debug.LogWarning("These atoms are already annotated with a distance");
            return;
        }

        anno.annoParent = getGO(a2).transform;
        anno.Create();
        AddAnnotation(anno);
    }
    public void AnnotateLine(UnityMolAtom a1, UnityMolAtom a2) {

        LineAtomAnnotation anno = new LineAtomAnnotation();
        anno.atoms.Add(a1);
        anno.atoms.Add(a2);

        if (AnnotationExists(anno)) {
            Debug.LogWarning("These atoms are already annotated with a line");
            return;
        }


        anno.annoParent = getGO(a2).transform;
        anno.Create();
        AddAnnotation(anno);
    }
    public void AnnotateWorldLine(Vector3 p1, Vector3 p2, Transform par, float sizeLine, Color lineCol) {

        CustomLineAnnotation anno = new CustomLineAnnotation();
        anno.start = p1;
        anno.end = p2;
        anno.annoParent = par;

        if (AnnotationExists(anno)) {
            Debug.LogWarning("These positions are already annotated with a line");
            return;
        }

        anno.colorLine = lineCol;
        anno.sizeLine = sizeLine;
        anno.Create();
        AddAnnotation(anno);
    }

    public void AnnotateAngle(UnityMolAtom a1, UnityMolAtom a2, UnityMolAtom a3) {

        AngleAnnotation anno = new AngleAnnotation();
        anno.atoms.Add(a1);
        anno.atoms.Add(a2);
        anno.atoms.Add(a3);

        if (AnnotationExists(anno)) {
            Debug.LogWarning("These atoms are already annotated with an angle");
            return;
        }

        anno.annoParent = getGO(a3).transform;
        anno.Create();
        AddAnnotation(anno);

    }

    public void AnnotateCurvedLine(UnityMolAtom a1, UnityMolAtom a2, UnityMolAtom a3) {

        ArcLineAnnotation anno = new ArcLineAnnotation();
        anno.atoms.Add(a1);
        anno.atoms.Add(a2);
        anno.atoms.Add(a3);

        if (AnnotationExists(anno)) {
            Debug.LogWarning("These atoms are already annotated with an arc");
            return;
        }

        anno.annoParent = getGO(a3).transform;
        anno.Create();
        AddAnnotation(anno);

    }

    public void AnnotateDihedralAngle(UnityMolAtom a1, UnityMolAtom a2, UnityMolAtom a3, UnityMolAtom a4) {

        TorsionAngleAnnotation anno = new TorsionAngleAnnotation();
        anno.atoms.Add(a1);
        anno.atoms.Add(a2);
        anno.atoms.Add(a3);
        anno.atoms.Add(a4);

        if (AnnotationExists(anno)) {
            Debug.LogWarning("These atoms are already annotated with a torsion angle");
            return;
        }

        anno.annoParent = getGO(a4).transform;
        anno.Create();
        AddAnnotation(anno);
    }

    public void AnnotateDihedralArrow(UnityMolAtom a1, UnityMolAtom a2) {

        ArrowAnnotation anno = new ArrowAnnotation();
        anno.atoms.Add(a1);
        anno.atoms.Add(a2);


        if (AnnotationExists(anno)) {
            Debug.LogWarning("These atoms are already annotated with a torsion angle");
            return;
        }

        anno.annoParent = getGO(a2).transform;
        anno.Create();
        AddAnnotation(anno);
    }

    public int AnnotateDrawing(UnityMolStructure s, List<Vector3> pos, Color col) {
        DrawAnnotation anno = new DrawAnnotation();
        anno.positions = pos;
        anno.colorLine = col;
        anno.id = UnityMolAnnotationManager.idDraw++;
        anno.atoms.Add(s.currentModel.allAtoms[0]);

        anno.annoParent = s.annotationParent.transform;
        anno.Create();
        AddAnnotation(anno);
        return anno.id;
    }



    public static float dihedral(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
        Vector3 ab = v1 - v2;
        Vector3 cb = v3 - v2;
        Vector3 db = v4 - v3;

        Vector3 u = Vector3.Cross(ab, cb);
        Vector3 v = Vector3.Cross(db, cb);
        Vector3 w = Vector3.Cross(v, u);

        float angle = getAngle2(u, v); //Vector3.Angle(u,v);
        if (getAngle2(cb, w) > 0.001f)
            angle = -angle;
        return angle * Mathf.Rad2Deg;

    }
    public static float getAngle2(Vector3 v1, Vector3 v2) {
        float s = Vector3.Cross(v1, v2).magnitude;
        float c = Vector3.Dot(v1, v2);
        float angle = Mathf.Atan2(s, c);
        return angle;
    }

    public static bool sameAtoms(List<UnityMolAtom> atoms1, List<UnityMolAtom> atoms2) {

        return atoms1.All(item => atoms2.Contains(item)) &&
               atoms1.Distinct().Count() == atoms2.Distinct().Count();
    }
}

public class AnnotationComparer : IEqualityComparer<UnityMolAnnotation>
{
    public bool Equals(UnityMolAnnotation a1, UnityMolAnnotation a2)
    {
        if (a1 == null && a2 == null) { return true;}
        if (a1 == null || a2 == null) { return false;}
        if (a1.GetType() == a2.GetType() && UnityMolAnnotationManager.sameAtoms(a1.atoms, a2.atoms)) {
            if (a1.GetType().ToString() == "UMol.DrawAnnotation") {
                return ((DrawAnnotation)a1).id == ((DrawAnnotation)a2).id;
            }
            if (a1.GetType().ToString() == "UMol.CustomSphereAnnotation") {
                return ((CustomSphereAnnotation)a1).annoParent.name == ((CustomSphereAnnotation)a2).annoParent.name;
            }
            if (a1.GetType().ToString() == "UMol.CustomLineAnnotation") {
                return ((CustomLineAnnotation)a1).start == ((CustomLineAnnotation)a2).start &&
                       ((CustomLineAnnotation)a1).end == ((CustomLineAnnotation)a2).end;
            }
            if (a1.GetType().ToString() == "UMol.CustomTextAnnotation") {
                return ((CustomTextAnnotation)a1).content == ((CustomTextAnnotation)a2).content &&
                       ((CustomTextAnnotation)a1).annoParent.name == ((CustomTextAnnotation)a2).annoParent.name;
            }
            if (a1.GetType().ToString() == "UMol.Annotate2D") {
                return ((Annotate2D)a1).content == ((Annotate2D)a2).content &&
                       ((Annotate2D)a1).posPercent == ((Annotate2D)a2).posPercent;
            }
            return true;
        }
        return false;
    }
    public int GetHashCode(UnityMolAnnotation a)
    {
        string h = a.GetType() + "_" + a.atoms.Count + "_";
        foreach (UnityMolAtom at in a.atoms) {
            h += at.ToString() + "_";
        }
        return h.GetHashCode();
        // return a.GetHashCode();
    }
}

}
