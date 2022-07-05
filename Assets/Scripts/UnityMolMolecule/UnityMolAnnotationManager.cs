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
    public Dictionary<UnityMolAtom, HashSet<UnityMolAnnotation>> annotationsDict = new Dictionary<UnityMolAtom, HashSet<UnityMolAnnotation>>();

    /// <summary>
    /// List of all the UnityMolAnnotations created
    /// </summary>
    public HashSet<UnityMolAnnotation> allAnnotations = new HashSet<UnityMolAnnotation>(new AnnotationComparer());


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
        }
        allAnnotations.Clear();
        annotationsDict.Clear();
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

        var allAtoms = annotationsDict.Keys;
        foreach (UnityMolAtom a in allAtoms) {
            if (annotationsDict[a].Contains(an)) {
                annotationsDict[a].Remove(an);
            }
        }
    }
    public void RemoveAnnotations(UnityMolAtom a) {

        HashSet<UnityMolAnnotation> toRemove = new HashSet<UnityMolAnnotation>();

        if (annotationsDict.ContainsKey(a)) {
            foreach (UnityMolAnnotation anno in annotationsDict[a]) {
                toRemove.Add(anno);
            }
            annotationsDict.Remove(a);
        }
        AnnotationComparer compa = new AnnotationComparer();

        foreach (UnityMolAnnotation anno in toRemove) {
            if (allAnnotations.Contains(anno)) {
                foreach (UnityMolAnnotation ano in allAnnotations) {
                    if (compa.Equals(anno, ano)) {
                        ano.Delete();
                    }
                }
                allAnnotations.Remove(anno);
            }
        }
    }

    public void AddAnnotation(UnityMolAnnotation an) {
        allAnnotations.Add(an);

        foreach (UnityMolAtom a in an.atoms) {
            if (!annotationsDict.ContainsKey(a)) {
                annotationsDict[a] = new HashSet<UnityMolAnnotation>(new AnnotationComparer());
            }
            annotationsDict[a].Add(an);
        }
    }

    public bool AnnotationExists(UnityMolAnnotation annoType) {

        // foreach (UnityMolAtom a in annoType.atoms) {
        //     if (!annotationsDict.ContainsKey(a)) {
        //         return false;
        //     }
        // }

        return allAnnotations.Contains(annoType);

    }


    public void Annotate(UnityMolAtom a) {

        SphereAnnotation anno = new SphereAnnotation();
        anno.atoms.Add(a);

        if (AnnotationExists(anno)) {
            Debug.LogWarning("This atom is already annotated with a sphere");
            return;
        }

        anno.annoParent = a.correspondingGo.transform;
        anno.Create();
        AddAnnotation(anno);
    }

    public void AnnotateText(UnityMolAtom a, string text) {

        TextAnnotation anno = new TextAnnotation();
        anno.atoms.Add(a);

        if (AnnotationExists(anno)) {
            Debug.LogWarning("This atom is already annotated with a text, overwritting");
            RemoveAnnotation(anno);
        }

        anno.annoParent = a.correspondingGo.transform;
        anno.content = text;
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

        anno.annoParent = a2.correspondingGo.transform;
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


        anno.annoParent = a2.correspondingGo.transform;
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

        anno.annoParent = a3.correspondingGo.transform;
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

        anno.annoParent = a3.correspondingGo.transform;
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

        anno.annoParent = a4.correspondingGo.transform;
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

        anno.annoParent = a2.correspondingGo.transform;
        anno.Create();
        AddAnnotation(anno);
    }

    public int AnnotateDrawing(UnityMolStructure s, List<Vector3> pos, Color col) {
        DrawAnnotation anno = new DrawAnnotation();
        anno.positions = pos;
        anno.colorLine = col;
        anno.id = UnityMolAnnotationManager.idDraw++;
        anno.atoms.Add(s.currentModel.allAtoms[0]);

        anno.annoParent = UnityMolMain.getStructureManager().GetStructureGameObject(s.uniqueName).transform;
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
        if (a1 == null | a2 == null) { return false;}
        if (a1.GetType() == a2.GetType() && UnityMolAnnotationManager.sameAtoms(a1.atoms, a2.atoms)) {
            if (a1.GetType().ToString() == "UMol.DrawAnnotation") {
                return ((DrawAnnotation)a1).id == ((DrawAnnotation)a2).id;
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
