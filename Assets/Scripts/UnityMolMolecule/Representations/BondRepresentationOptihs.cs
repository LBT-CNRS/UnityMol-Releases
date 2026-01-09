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
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UMol {
public class BondRepresentationOptihs : BondRepresentation {

    public List<GameObject> meshesGO;
    public Dictionary<AtomDuo, int3> coordStickTexture;
    public Dictionary<UnityMolAtom, List<AtomDuo>> atomToDuo;

    public Texture2D[] paramTextures;
    public Dictionary<UnityMolAtom, Color32> colorPerAtom;
    List<Material> materials;

    public Vector3 offsetPos = Vector3.zero;

    public float shrink = 0.4f;
    public float scale = 1 / 3.0f;

    public BondRepresentationOptihs(int idF, string structName, UnityMolSelection sel) {
        colorationType = colorType.atom;

        representationParent = UnityMolMain.getRepStructureParent(structName).transform;

        GameObject newRep = new GameObject("BondOptiHSRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        selection = sel;
        idFrame = idF;

        if (sel.structures[0].structureType == UnityMolStructure.MolecularType.Martini) {
            shrink = 0.05f;
        }

        materials = new List<Material>();


        DisplayHSMesh(newRep.transform);

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbBonds = selection.bonds.Count;

        if (offsetPos != Vector3.zero) {
            foreach (GameObject go in meshesGO)
                go.transform.localPosition = -offsetPos;
        }

    }

    private void DisplayHSMesh(Transform repParent) {

        meshesGO = new List<GameObject>();

        int idmesh = 0;
        int cptSticks = 0;
        int currentVertex = 0;
        const int NBPARAM = 14; // Number of parameters in the texture

        // List<KeyValuePair<UnityMolAtom,UnityMolAtom>> allBonds = linearizeBonds();
        int nbSticks = selection.bonds.Length;
        if (nbSticks == 0) {
            return;
        }

        UnityMolModel m = selection.atoms[0].residue.chain.model;

        GameObject currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentGO.transform.name = "BondMesh0";
        currentGO.transform.parent = repParent;
        GameObject.Destroy(currentGO.GetComponent<Collider>());

        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh tmpMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
        GameObject.Destroy(tmp);

        offsetPos = AtomRepresentationOptihb.computeHBOffsetCentroid(selection);

        Vector3[] verticesBase = tmpMesh.vertices;
        int[] trianglesBase = tmpMesh.triangles;

        int VERTICES_IN_CUBE = verticesBase.Length; // 24 vertices by cube
        int maxSticksinMesh = 4096 * 4; //Texture2D cannot be too big
        int LIMIT_VERTICES_IN_MESH = maxSticksinMesh * VERTICES_IN_CUBE;
        long totalVertices = nbSticks * VERTICES_IN_CUBE;
        int currentSticksinMesh = (int) Mathf.Min(maxSticksinMesh, nbSticks - (idmesh * maxSticksinMesh));

        // Compute the number of meshes needed to store all the sticks
        int nbMeshesNeeded = Mathf.CeilToInt(totalVertices / (float) LIMIT_VERTICES_IN_MESH) + 1;

        Mesh[] combinedMeshes = new Mesh[nbMeshesNeeded];
        combinedMeshes[idmesh] = new Mesh();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();
        List<int> newTriangles = new List<int>();

        // Create a texture to store parameters of sticks (11 parameters to store)
        // It is used to fetch parameters for each stick in the shader
        paramTextures = new Texture2D[nbMeshesNeeded];
        paramTextures[idmesh] = new Texture2D(currentSticksinMesh, NBPARAM, TextureFormat.RGBAFloat, false);

        float brightness = 0.75f;
        float attenuation = 0.0f;


        coordStickTexture = new Dictionary<AtomDuo, int3 >();
        atomToDuo = new Dictionary<UnityMolAtom, List<AtomDuo>>();
        colorPerAtom = new Dictionary<UnityMolAtom, Color32>();

        // foreach (AtomDuo key in flattenBonds){
        foreach (int ida in selection.bonds.bonds.Keys) {
            UnityMolAtom atom1 = m.allAtoms[ida];

            foreach (int idb in selection.bonds.bonds[ida]) {
                if (idb != -1) {
                    UnityMolAtom atom2 = m.allAtoms[idb];
                    AtomDuo key = new AtomDuo(atom1, atom2);
                    AtomDuo key2 = new AtomDuo(atom2, atom1);
                    if (!coordStickTexture.ContainsKey(key) && !coordStickTexture.ContainsKey(key2)) { // Not already done

                        if (cptSticks == currentSticksinMesh) {

                            RecordMeshAndParameters(combinedMeshes[idmesh], paramTextures[idmesh],
                                                    newVertices, newTriangles, newUV, cptSticks);

                            combinedMeshes[idmesh].RecalculateBounds();
                            currentGO.GetComponent<MeshFilter>().sharedMesh = combinedMeshes[idmesh];
                            meshesGO.Add(currentGO);

                            AssignMaterial(currentGO, paramTextures[idmesh], brightness,
                                           attenuation, NBPARAM, cptSticks);


                            //Create a new gameObject for the next mesh
                            currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            currentGO.transform.name = "BondMesh" + (idmesh + 1).ToString();
                            currentGO.transform.parent = repParent;
                            GameObject.Destroy(currentGO.GetComponent<Collider>());


                            newVertices.Clear();
                            newUV.Clear();
                            newTriangles.Clear();
                            idmesh++;

                            currentSticksinMesh = (int) Mathf.Max(0, Mathf.Min(maxSticksinMesh, nbSticks - (idmesh * maxSticksinMesh)));
                            paramTextures[idmesh] = new Texture2D(currentSticksinMesh < 1 ? 1 : currentSticksinMesh, NBPARAM, TextureFormat.RGBAFloat, false);
                            combinedMeshes[idmesh] = new Mesh();
                            currentVertex = 0;
                            cptSticks = 0;

                        }

                        Vector3 posAtom1 = atom1.position;
                        if (idFrame != -1) {
                            posAtom1 = selection.extractTrajFramePositions[idFrame][selection.atomToIdInSel[atom1]];
                        }

                        posAtom1 += offsetPos;

                        // Store the vertices
                        for (int v = 0; v < verticesBase.Length; v++) {
                            Vector3 vert = verticesBase[v];
                            newVertices.Add(vert + posAtom1);// Vector3.Scale(vert,new Vector3(aModel.scale/100f,aModel.scale/100f,aModel.scale/100f))+posAtom);
                            // IMPORTANT : Add the id of the atom in each vertex (used to fetch data in the texture)
                            newUV.Add(new Vector2(cptSticks, 0f));
                        }

                        //Store the triangles
                        for (int t = 0; t < trianglesBase.Length; t++)
                            newTriangles.Add(trianglesBase[t] + (cptSticks * VERTICES_IN_CUBE));

                        float visi = 1.0f;



                        StoreParametersInTexture(paramTextures[idmesh], cptSticks, atom1, atom2, offsetPos, visi);

                        int3 infoTex; infoTex.x = idmesh; infoTex.y = cptSticks; infoTex.z = 0;
                        coordStickTexture[key] = infoTex;
                        if (!atomToDuo.ContainsKey(atom1))
                            atomToDuo[atom1] = new List<AtomDuo>();
                        if (!atomToDuo.ContainsKey(atom2))
                            atomToDuo[atom2] = new List<AtomDuo>();
                        atomToDuo[atom1].Add(key);
                        atomToDuo[atom2].Add(key);

                        // Add the opposite bond
                        // infoTex.z = 1;
                        // key = new KeyValuePair<UnityMolAtom, UnityMolAtom>(atom2, atom1);
                        // coordStickTexture[key] = infoTex;

                        cptSticks++;
                        currentVertex += verticesBase.Length;

                    }
                }
            }
        }
        if (cptSticks != 0) {

            RecordMeshAndParameters(combinedMeshes[idmesh], paramTextures[idmesh], newVertices,
                                    newTriangles, newUV, cptSticks);

            combinedMeshes[idmesh].RecalculateBounds();
            currentGO.GetComponent<MeshFilter>().sharedMesh = combinedMeshes[idmesh];
            meshesGO.Add(currentGO);

            AssignMaterial(currentGO, paramTextures[idmesh], brightness, attenuation, NBPARAM, cptSticks);

            //  //Debug the texture and the mesh
            //  // UnityEditor.AssetDatabase.CreateAsset(combinedMeshes[idmesh],"Assets/Resources/tmptest/testMesh"+idmesh.ToString()+".asset");
            //  // UnityEditor.AssetDatabase.CreateAsset(paramTextures[idmesh],"Assets/Resources/tmptest/testTex"+idmesh.ToString()+".asset");
            //  // UnityEditor.AssetDatabase.SaveAssets();

        }
    }

    void StoreParametersInTexture(Texture2D tex, int bondId, UnityMolAtom umolAtom1, UnityMolAtom umolAtom2, Vector4 offset, float visible = 1.0f) {

        Vector4 posAtom1vec4 = umolAtom1.PositionVec4 + offset;
        Vector4 posAtom2vec4 = umolAtom2.PositionVec4 + offset;

        if (idFrame != -1) {
            Vector3 tmp1 = selection.extractTrajFramePositions[idFrame][selection.atomToIdInSel[umolAtom1]];
            posAtom1vec4 = new Vector4(tmp1.x, tmp1.y, tmp1.z, 0.0f) + offset;
            Vector3 tmp2 = selection.extractTrajFramePositions[idFrame][selection.atomToIdInSel[umolAtom2]];
            posAtom2vec4 = new Vector4(tmp2.x, tmp2.y, tmp2.z, 0.0f) + offset;
        }

        Vector4 radiusv4 = Vector4.zero;
        radiusv4.x = umolAtom1.radius;
        tex.SetPixel(bondId, 0, radiusv4); // Radius sphere 1
        radiusv4.x = umolAtom2.radius;
        tex.SetPixel(bondId, 1, radiusv4); // Radius sphere 2

        tex.SetPixel(bondId, 2, umolAtom1.color);
        colorPerAtom[umolAtom1] = umolAtom1.color;
        tex.SetPixel(bondId, 3, umolAtom2.color);
        colorPerAtom[umolAtom2] = umolAtom2.color;
        tex.SetPixel(bondId, 4, posAtom1vec4); // Changing position
        tex.SetPixel(bondId, 5, posAtom2vec4); // Changing position
        tex.SetPixel(bondId, 6, posAtom1vec4); // Base position
        tex.SetPixel(bondId, 7, posAtom2vec4); // Base position
        tex.SetPixel(bondId, 8, Vector4.one); // Atom type matcap id
        tex.SetPixel(bondId, 9, Vector4.one); // Atom type matcap id 2
        tex.SetPixel(bondId, 10, Vector4.one * visible); // Visibility
        tex.SetPixel(bondId, 11, Vector4.one); // Scale for atom 1 in x and atom 2 in y
        tex.SetPixel(bondId, 12, Vector4.zero); // Atom selected
        tex.SetPixel(bondId, 13, Vector4.zero); // Atom selected

    }

    void RecordMeshAndParameters(Mesh mesh, Texture2D tex, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int cptSticks) {
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.Clear();

        for (int i = 0; i < uv.Count; i++) {
            Vector2 aa = uv[i];
            aa.y = aa.x / (float)(cptSticks - 1);
            uv[i] = aa;
        }

        // Fill the mesh with the arrays
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uv);
        mesh.SetIndices(triangles, MeshTopology.Triangles, 0, true, 0);

        // Upload the texture to the GPU
        tex.Apply(false); // DO NOT REMOVE !!!!!!!!!!!!!!!!!!
        tex.wrapMode = TextureWrapMode.Clamp; // Mandatory to access the data in the shader
        tex.filterMode = FilterMode.Point; // Texture will be used reading pixels
        tex.anisoLevel = 0;
    }

    void AssignMaterial(GameObject curGO, Texture2D tex, float brightness,
                        float attenuation, int NBPARAM, int sticksInMesh) {


        Material HSoptiMat = null;

        HSoptiMat = new Material(Shader.Find("UMol/Sticks HyperBalls Shadow Merged"));

        materials.Add(HSoptiMat);

        //Set fixed parameters for the shader
        HSoptiMat.SetTexture("_MainTex", tex);
        // HSoptiMat.SetTexture("_MatCap", (Texture) Resources.Load("Images/MatCap/daphz05"));
        HSoptiMat.SetFloat("_Brightness", brightness);
        HSoptiMat.SetFloat("_Attenuation", attenuation);
        HSoptiMat.SetFloat("_NBParam", (float) NBPARAM);
        HSoptiMat.SetFloat("_NBSticks", (float) sticksInMesh);
        HSoptiMat.SetFloat("_Shrink", shrink);
        HSoptiMat.SetFloat("_Visibility", 1f);
        HSoptiMat.SetFloat("_Scale", scale);
        HSoptiMat.SetFloat("_EllipseFactor", 1f);
        curGO.GetComponent<Renderer>().sharedMaterial = HSoptiMat;

        curGO.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
        curGO.GetComponent<Renderer>().receiveShadows = true;
    }


    public override void Clean() {

        if (meshesGO != null) {
            for (int i = 0; i < meshesGO.Count; i++) {
                GameObject.Destroy(meshesGO[i].GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(meshesGO[i]);
            }
        }

        if (materials != null) {
            for (int i = 0; i < materials.Count; i++) {
                GameObject.Destroy(materials[i]);
            }
            materials.Clear();
            materials = null;
        }

        if (coordStickTexture != null)
            coordStickTexture.Clear();
        coordStickTexture = null;

        if (atomToDuo != null)
            atomToDuo.Clear();
        atomToDuo = null;
        if (colorPerAtom != null)
            colorPerAtom.Clear();
        colorPerAtom = null;

        if (paramTextures != null) {
            for (int i = 0; i < paramTextures.Length; i++) {
                GameObject.Destroy(paramTextures[i]);
            }
        }
    }
}


public class AtomDuo {

    public UnityMolAtom a1;
    public UnityMolAtom a2;

    public AtomDuo(UnityMolAtom atom1, UnityMolAtom atom2) {
        a1 = atom1;
        a2 = atom2;
    }

    public static bool operator ==(AtomDuo lhs, AtomDuo rhs) {
        if (ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs)) { return false;}
        if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs)) { return true;}
        return lhs.a1.Equals(rhs.a1) && lhs.a2.Equals(rhs.a2);
    }
    public static bool operator !=(AtomDuo lhs, AtomDuo rhs) {
        if (ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs)) { return true;}
        if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs)) { return false;}
        return !(lhs == rhs);
    }
    public override bool Equals(object o) {
        if (o is AtomDuo)
        {
            AtomDuo duo2 = (AtomDuo)o;
            return a1.Equals(duo2.a1) && a2.Equals(duo2.a2);
        }
        return false;
    }


    // public override int GetHashCode() {
    //  return lightHashCode;
    // }

    public override int GetHashCode() {
        int lhash = a1.serial;
        unchecked
        {
            const int factor = 9176;

            lhash = lhash * factor + a2.serial;
        }
        return lhash;
    }
}

public class AtomTrio {

    public UnityMolAtom a1;
    public UnityMolAtom a2;
    public UnityMolAtom a3;

    public AtomTrio(UnityMolAtom atom1, UnityMolAtom atom2, UnityMolAtom atom3) {
        a1 = atom1;
        a2 = atom2;
        a3 = atom3;
    }

    public static bool operator ==(AtomTrio lhs, AtomTrio rhs) {
        if (ReferenceEquals(null, lhs) && ReferenceEquals(null, rhs)) { return false;}
        if (ReferenceEquals(null, lhs) || ReferenceEquals(null, rhs)) { return true;}
        return lhs.a1.Equals(rhs.a1) && lhs.a2.Equals(rhs.a2) && lhs.a3.Equals(rhs.a3);
    }
    public static bool operator !=(AtomTrio lhs, AtomTrio rhs) {
        return !(lhs == rhs);
    }

    public override bool Equals(object o) {
        if (o is AtomTrio)
        {
            AtomTrio trio2 = (AtomTrio)o;
            return a1 == trio2.a1 && a2 == trio2.a2 && a3 == trio2.a3;
        }
        return false;
    }



    public override int GetHashCode() {
        int lhash = a1.serial;
        unchecked
        {
            const int factor = 9176;

            lhash = lhash * factor + a2.serial;
            lhash = lhash * factor + a3.serial;
        }
        return lhash;
    }
}

}