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
using System.Linq;
using Unity.Mathematics;


namespace UMol {
public class BondRepresentationBondOrder : BondRepresentation {

    public List<GameObject> meshesGO;
    public Dictionary<AtomDuo, int3> coordStickTexture;
    public Dictionary<UnityMolAtom, List<AtomDuo>> atomToDuo;
    public Texture2D[] paramTextures;
    public Dictionary<UnityMolAtom, Color32> colorPerAtom;

    public List<GameObject> multiBondGO;
    public Texture2D[] multiBondParamTextures;
    public Dictionary<AtomDuo, int3> multiBondCoordStickTexture;
    public Dictionary<AtomDuo, AtomTrio> doubleBonded;
    public Dictionary<AtomDuo, AtomTrio> tripleBonded;

    public Vector3 offsetPos = Vector3.zero;

    List<Material> materials;

    //Specific link shrink
    public float shrink = 0.001f;
    public float scale = 0.035f;
    public bool withShadow = true;


    public BondRepresentationBondOrder(int idF, string structName, UnityMolSelection sel) {

        representationParent = UnityMolMain.getRepStructureParent(structName).transform;

        GameObject newRep = new GameObject("BondOrderRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        selection = sel;
        idFrame = idF;

        if (sel.structures[0].structureType == UnityMolStructure.MolecularType.Martini) {
            shrink = 0.05f;
        }


        doubleBonded = getDoubleBonded(selection);
        tripleBonded = getTripleBonded(selection);

        materials = new List<Material>();

        offsetPos = AtomRepresentationOptihb.computeHBOffsetCentroid(selection);

        DisplayHSMesh(newRep.transform);
        DisplayHSMeshMultiBond(newRep.transform);

        // newRep.transform.position -= offset;

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbBonds = selection.bonds.Count;

        if (offsetPos != Vector3.zero) {
            foreach (GameObject go in meshesGO)
                go.transform.localPosition = -offsetPos;
            foreach (GameObject go in multiBondGO)
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
        long nbSticks = selection.bonds.Length;
        if (nbSticks == 0)
            return;

        UnityMolModel m = selection.atoms[0].residue.chain.model;
        // flattenBonds = selection.bonds.ToAtomDuoList(m);

        GameObject currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentGO.transform.name = "BondMesh0";
        currentGO.transform.parent = repParent;
        GameObject.Destroy(currentGO.GetComponent<Collider>());

        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh tmpMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
        GameObject.Destroy(tmp);

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
        paramTextures[idmesh] = new Texture2D(currentSticksinMesh < 1 ? 1 : currentSticksinMesh, NBPARAM, TextureFormat.RGBAFloat, false);

        float brightness = 1.0f;
        float attenuation = 0.0f;


        coordStickTexture = new Dictionary<AtomDuo, int3 >();
        atomToDuo = new Dictionary<UnityMolAtom, List<AtomDuo>>();
        colorPerAtom = new Dictionary<UnityMolAtom, Color32>();


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

                        if (doubleBonded.ContainsKey(key) || doubleBonded.ContainsKey(key2)) { //Still show the centered bond for triple bonds
                            visi = 0.0f;
                        }
                        StoreParametersInTexture(paramTextures[idmesh], cptSticks, atom1, atom2, offsetPos, visi);


                        // //Create Collider for each atom
                        // GameObject colGO = new GameObject("Collider_Atom_"+i);
                        // SphereCollider sc = colGO.AddComponent<SphereCollider>();
                        // sc.radius = radius;
                        // colGO.transform.parent = collidersParent.transform;
                        // collidersGO.Add(colGO);
                        // colGO.transform.position = posAtom;



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

    private void DisplayHSMeshMultiBond(Transform repParent) {

        multiBondGO = new List<GameObject>();

        int idmesh = 0;
        int cptSticks = 0;
        int currentVertex = 0;
        const int NBPARAM = 14; // Number of parameters in the texture

        //for triple bonds, the centered one is shown by the previous function
        long nbSticks = doubleBonded.Count * 2 + tripleBonded.Count * 2;
        if (nbSticks == 0)
            return;

        GameObject currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentGO.transform.name = "BondMesh_multi0";
        currentGO.transform.parent = repParent;
        GameObject.Destroy(currentGO.GetComponent<Collider>());

        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh tmpMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
        GameObject.Destroy(tmp);

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
        multiBondParamTextures = new Texture2D[nbMeshesNeeded];
        multiBondParamTextures[idmesh] = new Texture2D(currentSticksinMesh < 1 ? 1 : currentSticksinMesh, NBPARAM, TextureFormat.RGBAFloat, false);

        float brightness = 1.0f;
        float attenuation = 0.0f;


        multiBondCoordStickTexture = new Dictionary<AtomDuo, int3 >();

        var multiBondedKeys = doubleBonded.Keys.ToList();
        multiBondedKeys.AddRange(tripleBonded.Keys.ToList());

        foreach (AtomDuo d in multiBondedKeys) {
            bool isDouble = true;
            AtomTrio key = null;
            if (doubleBonded.ContainsKey(d)) {
                key = doubleBonded[d];
            }
            else {
                key = tripleBonded[d];
                isDouble = false;
            }
            UnityMolAtom atom1 = key.a1;
            UnityMolAtom atom2 = key.a2;
            AtomDuo key2 = new AtomDuo(d.a2, d.a1);

            if (!multiBondCoordStickTexture.ContainsKey(d) && !multiBondCoordStickTexture.ContainsKey(key2)) { // Not already done

                if (cptSticks == currentSticksinMesh) {

                    RecordMeshAndParameters(combinedMeshes[idmesh], multiBondParamTextures[idmesh],
                                            newVertices, newTriangles, newUV, cptSticks);

                    combinedMeshes[idmesh].RecalculateBounds();
                    currentGO.GetComponent<MeshFilter>().sharedMesh = combinedMeshes[idmesh];
                    multiBondGO.Add(currentGO);

                    AssignMaterial(currentGO, multiBondParamTextures[idmesh], brightness,
                                   attenuation, NBPARAM, cptSticks);


                    //Create a new gameObject for the next mesh
                    currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    currentGO.transform.name = "BondMesh_multi" + (idmesh + 1).ToString();
                    currentGO.transform.parent = repParent;
                    GameObject.Destroy(currentGO.GetComponent<Collider>());


                    newVertices.Clear();
                    newUV.Clear();
                    newTriangles.Clear();
                    idmesh++;

                    currentSticksinMesh = (int) Mathf.Max(0, Mathf.Min(maxSticksinMesh, nbSticks - (idmesh * maxSticksinMesh)));
                    multiBondParamTextures[idmesh] = new Texture2D(currentSticksinMesh < 1 ? 1 : currentSticksinMesh, NBPARAM, TextureFormat.RGBAFloat, false);
                    combinedMeshes[idmesh] = new Mesh();
                    currentVertex = 0;
                    cptSticks = 0;

                }


                Vector3 posAtom1 = atom1.position;
                if (idFrame != -1) {
                    posAtom1 = selection.extractTrajFramePositions[idFrame][selection.atomToIdInSel[atom1]];
                }

                posAtom1 += offsetPos;

                Vector3 offset = computeOffset(key);
                if (!isDouble) {
                    offset *= 1.2f;
                }

                // Store the vertices
                for (int v = 0; v < verticesBase.Length; v++) {
                    Vector3 vert = verticesBase[v];
                    newVertices.Add(vert + posAtom1 - offset);// Vector3.Scale(vert,new Vector3(aModel.scale/100f,aModel.scale/100f,aModel.scale/100f))+posAtom);
                    // IMPORTANT : Add the id of the atom in each vertex (used to fetch data in the texture)
                    newUV.Add(new Vector2(cptSticks, 0f));
                }

                //Store the triangles
                for (int t = 0; t < trianglesBase.Length; t++)
                    newTriangles.Add(trianglesBase[t] + (cptSticks * VERTICES_IN_CUBE));

                StoreParametersInTexture(multiBondParamTextures[idmesh], cptSticks, atom1, atom2, offsetPos - offset);


                int3 infoTex; infoTex.x = idmesh; infoTex.y = cptSticks; infoTex.z = 0;
                multiBondCoordStickTexture[d] = infoTex;

                // ----------------------
                cptSticks++;
                currentVertex += verticesBase.Length;

                // Store the vertices (bis)
                for (int v = 0; v < verticesBase.Length; v++) {
                    Vector3 vert = verticesBase[v];
                    newVertices.Add(vert + posAtom1 + offset);// Vector3.Scale(vert,new Vector3(aModel.scale/100f,aModel.scale/100f,aModel.scale/100f))+posAtom);
                    // IMPORTANT : Add the id of the atom in each vertex (used to fetch data in the texture)
                    newUV.Add(new Vector2(cptSticks, 0f));
                }

                //Store the triangles (bis)
                for (int t = 0; t < trianglesBase.Length; t++)
                    newTriangles.Add(trianglesBase[t] + (cptSticks * VERTICES_IN_CUBE));

                StoreParametersInTexture(multiBondParamTextures[idmesh], cptSticks, atom1, atom2, offset + offsetPos);

                cptSticks++;
                currentVertex += verticesBase.Length;
            }
        }
        if (cptSticks != 0) {

            RecordMeshAndParameters(combinedMeshes[idmesh], multiBondParamTextures[idmesh], newVertices,
                                    newTriangles, newUV, cptSticks);

            combinedMeshes[idmesh].RecalculateBounds();
            currentGO.GetComponent<MeshFilter>().sharedMesh = combinedMeshes[idmesh];
            multiBondGO.Add(currentGO);

            AssignMaterial(currentGO, multiBondParamTextures[idmesh], brightness, attenuation, NBPARAM, cptSticks);
        }


    }


    void StoreParametersInTexture(Texture2D tex, int bondId, UnityMolAtom umolAtom1, UnityMolAtom umolAtom2, Vector3 offset, float visible = 1.0f) {

        Vector4 offset4 = new Vector4(offset.x, offset.y, offset.z, 0.0f);
        Vector4 posAtom1vec4 = umolAtom1.PositionVec4 + offset4;
        Vector4 posAtom2vec4 = umolAtom2.PositionVec4 + offset4;
        if (idFrame != -1) {
            Vector3 tmp1 = selection.extractTrajFramePositions[idFrame][selection.atomToIdInSel[umolAtom1]];
            posAtom1vec4 = new Vector4(tmp1.x, tmp1.y, tmp1.z, 0.0f) + offset4;
            Vector3 tmp2 = selection.extractTrajFramePositions[idFrame][selection.atomToIdInSel[umolAtom2]];
            posAtom2vec4 = new Vector4(tmp2.x, tmp2.y, tmp2.z, 0.0f) + offset4;
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

        if (!withShadow) {
            HSoptiMat = new Material(Shader.Find("UMol/Sticks HyperBalls Merged"));
        }
        else {
            HSoptiMat = new Material(Shader.Find("UMol/Sticks HyperBalls Shadow Merged"));
        }

        materials.Add(HSoptiMat);

        //Set fixed parameters for the shader
        HSoptiMat.SetTexture("_MainTex", tex);
        HSoptiMat.SetTexture("_MatCap", (Texture) Resources.Load("Images/MatCap/daphz05"));
        HSoptiMat.SetFloat("_Brightness", brightness);
        HSoptiMat.SetFloat("_Attenuation", attenuation);
        HSoptiMat.SetFloat("_NBParam", (float) NBPARAM);
        HSoptiMat.SetFloat("_NBSticks", (float) sticksInMesh);
        HSoptiMat.SetFloat("_Shrink", shrink);
        HSoptiMat.SetFloat("_Visibility", 1f);
        HSoptiMat.SetFloat("_Scale", scale);
        HSoptiMat.SetFloat("_EllipseFactor", 1f);
        curGO.GetComponent<Renderer>().sharedMaterial = HSoptiMat;

        if (withShadow) {
            curGO.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
            curGO.GetComponent<Renderer>().receiveShadows = true;
        }
        else {
            curGO.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            curGO.GetComponent<Renderer>().receiveShadows = false;
        }
    }
    public Vector3 computeOffset(AtomTrio t) {
        Vector3 posA1 = t.a1.position;
        Vector3 posA2 = t.a2.position;
        Vector3 posA3 = posA2;
        if (t.a3 != null)
            posA3 = t.a3.position;

        if (selection.extractTrajFrame) {
            posA1 = selection.extractTrajFramePositions[idFrame][selection.atomToIdInSel[t.a1]];
            posA2 = selection.extractTrajFramePositions[idFrame][selection.atomToIdInSel[t.a2]];
            posA3 = posA2;
            if (t.a3 != null)
                posA3 = selection.extractTrajFramePositions[idFrame][selection.atomToIdInSel[t.a3]];
        }

        Vector3 v12 = (posA2 - posA1).normalized;
        Vector3 v13 = Vector3.zero;
        if (t.a3 == null) {
            v13 = posA1.normalized;
        }
        else {
            v13 = (posA3 - posA1).normalized;
        }

        float dp = Vector3.Dot(v12, v13);
        if (1 - Mathf.Abs(dp) < 1e-6) {
            v13 = new Vector3(1, 0, 0);
            dp = Vector3.Dot(v12, v13);
            if (1 - Mathf.Abs(dp) < 1e-6) {
                v13 = new Vector3(1, 0, 0);
                dp = Vector3.Dot(v12, v13);
            }
        }

        Vector3 res = (v12 * dp) - v13;
        return res.normalized * 0.1f;
    }

    //TODO move this
    public static Dictionary<AtomDuo, AtomTrio> getDoubleBonded(UnityMolSelection sel) {
        Dictionary<AtomDuo, AtomTrio> doubleBonded = new Dictionary<AtomDuo, AtomTrio>();

        UnityMolModel m = sel.atoms[0].residue.chain.model;
        foreach (int ida in sel.bonds.bonds.Keys) {
            foreach (int idb in sel.bonds.bonds[ida]) {
                if (idb != -1) {
                    UnityMolAtom atom1 = m.allAtoms[ida];
                    UnityMolAtom atom2 = m.allAtoms[idb];

                    bondOrderType bondO = sel.bonds.getBondOrder(atom1, atom2, true);
                    if (bondO.order > 1.0f && bondO.btype == bondType.covalent && bondO.order < 3.0f) {
                        UnityMolAtom atom3 = getFirstBondedAtom(sel.bonds, atom1, atom2);
                        doubleBonded[new AtomDuo(atom1, atom2)] = new AtomTrio(atom1, atom2, atom3);
                    }
                }
            }
        }
        return doubleBonded;
    }

    public static Dictionary<AtomDuo, AtomTrio> getTripleBonded(UnityMolSelection sel) {
        Dictionary<AtomDuo, AtomTrio> tripleBonded = new Dictionary<AtomDuo, AtomTrio>();
        UnityMolModel m = sel.atoms[0].residue.chain.model;
        foreach (int ida in sel.bonds.bonds.Keys) {
            foreach (int idb in sel.bonds.bonds[ida]) {
                if (idb != -1) {
                    UnityMolAtom atom1 = m.allAtoms[ida];
                    UnityMolAtom atom2 = m.allAtoms[idb];
                    bondOrderType bondO = sel.bonds.getBondOrder(atom1, atom2, true);
                    if (bondO.order > 2.0f && bondO.btype == bondType.covalent && bondO.order < 4.0f) {
                        UnityMolAtom atom3 = getFirstBondedAtom(sel.bonds, atom1, atom2);
                        tripleBonded[new AtomDuo(atom1, atom2)] = new AtomTrio(atom1, atom2, atom3);
                    }
                }
            }
        }
        return tripleBonded;
    }

    public static UnityMolAtom getFirstBondedAtom(UnityMolBonds bonds, UnityMolAtom a, UnityMolAtom a2) {

        UnityMolModel m = a.residue.chain.model;
        int[] res;
        if (bonds.bonds.TryGetValue(a.idInAllAtoms, out res)) {
            foreach (int idb in res) {
                if (idb != -1 && idb != a2.idInAllAtoms) {
                    return m.allAtoms[idb];
                }
            }
        }

        return null;
    }
    public override void Clean() {

        if (meshesGO != null) {
            for (int i = 0; i < meshesGO.Count; i++) {
                GameObject.Destroy(meshesGO[i].GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(meshesGO[i]);
            }
        }
        if (multiBondGO != null) {
            for (int i = 0; i < multiBondGO.Count; i++) {
                GameObject.Destroy(multiBondGO[i].GetComponent<MeshFilter>().sharedMesh);
                GameObject.Destroy(multiBondGO[i]);
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

        if (multiBondParamTextures != null) {
            for (int i = 0; i < multiBondParamTextures.Length; i++) {
                GameObject.Destroy(multiBondParamTextures[i]);
            }
        }
        if (multiBondCoordStickTexture != null)
            multiBondCoordStickTexture.Clear();
        multiBondCoordStickTexture = null;
        if (doubleBonded != null)
            doubleBonded.Clear();
        doubleBonded = null;
        if (tripleBonded != null)
            tripleBonded.Clear();
        tripleBonded = null;

    }
}
}