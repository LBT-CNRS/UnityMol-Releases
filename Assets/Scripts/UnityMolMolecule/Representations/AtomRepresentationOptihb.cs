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
using System.Text;
using System.Collections.Generic;

namespace UMol {

public class AtomRepresentationOptihb : AtomRepresentation {

    public List<GameObject> meshesGO;
    public Dictionary<int, KeyValuePair<int, int> > coordAtomTexture;
    public Texture2D[] paramTextures;
    public Dictionary<UnityMolAtom, int> atomToId;
    public List<Color32> atomColors;
    List<Material> materials;
    public Vector3 offsetPos = Vector3.zero;

    public AtomRepresentationOptihb(int idF, string structName, UnityMolSelection sel) {
        colorationType = colorType.atom;

        representationParent = UnityMolMain.getRepStructureParent(structName).transform;

        GameObject newRep = new GameObject("AtomOptiHBRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;

        selection = sel;
        idFrame = idF;

        atomToId = new Dictionary<UnityMolAtom, int>();
        materials = new List<Material>();

        DisplayHBMesh(newRep.transform);
        // newRep.transform.position -= offset;
        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        if (offsetPos != Vector3.zero) {
            foreach (GameObject go in meshesGO)
                go.transform.localPosition = -offsetPos;
        }
        nbAtoms = selection.atoms.Count;
    }

    private void DisplayHBMesh(Transform repParent) {

        meshesGO = new List<GameObject>();

        coordAtomTexture = new Dictionary<int, KeyValuePair<int, int> >();
        atomColors = new List<Color32>(selection.atoms.Count);
        int idmesh = 0;
        int cptAtoms = 0;
        int currentVertex = 0;
        int NBPARAM = 11; //Number of parameters in the texture

        GameObject currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentGO.transform.name = "AtomMesh0";
        currentGO.transform.parent = repParent;
        GameObject.Destroy(currentGO.GetComponent<Collider>());

        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh tmpMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
        GameObject.Destroy(tmp);

        offsetPos = computeHBOffsetCentroid(selection);

        Vector3[] verticesBase = tmpMesh.vertices;
        int[] trianglesBase = tmpMesh.triangles;

        int VERTICES_IN_CUBE = verticesBase.Length; // 24 vertices by cube
        int maxAtomsinMesh = 4096 * 4;//Texture2D cannot be too big
        int LIMIT_VERTICES_IN_MESH = maxAtomsinMesh * VERTICES_IN_CUBE;
        long totalVertices = selection.atoms.Count * VERTICES_IN_CUBE;
        int currentAtomsinMesh = (int) Mathf.Min(maxAtomsinMesh, selection.atoms.Count - (idmesh * maxAtomsinMesh));


        // Compute the number of meshes needed to store all the atoms
        int nb_meshes_needed = Mathf.CeilToInt(totalVertices / (float) LIMIT_VERTICES_IN_MESH);

        if (nb_meshes_needed == 0) {
            paramTextures = new Texture2D[0];
            GameObject.Destroy(currentGO);
            return;
        }

        List<Mesh> combinedMeshes = new List<Mesh>(nb_meshes_needed);
        combinedMeshes.Add(new Mesh());
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();
        List<int> newTriangles = new List<int>();

        // Create a texture to store parameters of spheres (8 parameters to store)
        // It is used to fetch parameters for each atom in the shader
        List<Texture2D> tmpparamTextures = new List<Texture2D>();
        tmpparamTextures.Add( new Texture2D(currentAtomsinMesh, NBPARAM, TextureFormat.RGBAFloat, false));


        float brightness = 0.75f;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < selection.atoms.Count; i++) {

            if (cptAtoms == currentAtomsinMesh) {
                RecordMeshAndParameters(combinedMeshes[idmesh], tmpparamTextures[idmesh],
                                        newVertices, newTriangles, newUV, cptAtoms);

                currentGO.GetComponent<MeshFilter>().sharedMesh = combinedMeshes[idmesh];
                meshesGO.Add(currentGO);

                AssignMaterial(currentGO, tmpparamTextures[idmesh], brightness, NBPARAM, cptAtoms);
                combinedMeshes[idmesh].RecalculateBounds();



                // Create a new gameObject for the next mesh
                currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                currentGO.transform.name = "AtomMesh" + (idmesh + 1).ToString();
                currentGO.transform.parent = repParent;
                GameObject.Destroy(currentGO.GetComponent<Collider>());


                newVertices.Clear();
                newUV.Clear();
                newTriangles.Clear();
                idmesh++;

                currentAtomsinMesh = (int) Mathf.Min(maxAtomsinMesh, selection.atoms.Count - (idmesh * maxAtomsinMesh));

                tmpparamTextures.Add(new Texture2D(currentAtomsinMesh, NBPARAM, TextureFormat.RGBAFloat, false));
                combinedMeshes.Add(new Mesh());
                currentVertex = 0;
                cptAtoms = 0;

            }

            Vector3 posAtom = selection.atoms[i].position;
            if (idFrame != -1) {
                posAtom = selection.extractTrajFramePositions[idFrame][i];
            }
            posAtom += offsetPos;

            // Store the vertices
            for (int v = 0; v < verticesBase.Length; v++) {
                Vector3 vert = verticesBase[v];
                newVertices.Add(vert + posAtom);
                // IMPORTANT : Add the id of the atom in each vertex (used to fetch data in the texture)
                newUV.Add(new Vector2(cptAtoms, 0f));
            }

            //Store the triangles
            for (int t = 0; t < trianglesBase.Length; t++) {
                newTriangles.Add(trianglesBase[t] + currentVertex);
            }


            StoreParametersInTexture(tmpparamTextures[idmesh], cptAtoms, selection.atoms[i], i);

            coordAtomTexture.Add(i, new KeyValuePair<int, int>(idmesh, cptAtoms));
            if (atomToId.ContainsKey(selection.atoms[i])) {
#if UNITY_EDITOR
                sb.Append("Atom already in representation : ");
                sb.Append(selection.atoms[i]);
                sb.Append("\n");

#endif
            }
            else {
                atomToId.Add(selection.atoms[i], i);
            }


            // Create Collider for each atom
            // GameObject colGO = new GameObject("Collider_Atom_"+i);
            // SphereCollider sc = colGO.AddComponent<SphereCollider>();
            // sc.radius = radius;
            // colGO.transform.parent = collidersParent.transform;
            // collidersGO.Add(colGO);
            // colGO.transform.position = posAtom;

            currentVertex += verticesBase.Length;

            cptAtoms++;

        }
        if (cptAtoms != 0) {

            RecordMeshAndParameters(combinedMeshes[idmesh], tmpparamTextures[idmesh],
                                    newVertices, newTriangles, newUV, cptAtoms);

            combinedMeshes[idmesh].RecalculateBounds();
            currentGO.GetComponent<MeshFilter>().sharedMesh = combinedMeshes[idmesh];
            meshesGO.Add(currentGO);

            AssignMaterial(currentGO, tmpparamTextures[idmesh], brightness, NBPARAM, cptAtoms);



            //Debug the texture and the mesh
            // UnityEditor.AssetDatabase.CreateAsset(combinedMeshes[idmesh],"Assets/Resources/tmptest/testMesh"+idmesh.ToString()+".asset");
            // UnityEditor.AssetDatabase.CreateAsset(paramTextures[idmesh],"Assets/Resources/tmptest/testTex"+idmesh.ToString()+".asset");
            // UnityEditor.AssetDatabase.SaveAssets();
        }
        paramTextures = tmpparamTextures.ToArray();
#if UNITY_EDITOR
        if (sb.Length != 0) {
            Debug.LogError(sb.ToString());
        }
#endif

    }

    void StoreParametersInTexture(Texture2D tex, int atomID, UnityMolAtom umolAtom, int idInSel) {

        Vector4 posAtomvec4 = Vector4.zero;

        if (idFrame != -1) {
            Vector3 tmp = selection.extractTrajFramePositions[idFrame][idInSel];
            posAtomvec4.x = tmp.x; posAtomvec4.y = tmp.y; posAtomvec4.z = tmp.z;
        }
        else {
            posAtomvec4 = umolAtom.PositionVec4;
        }
        posAtomvec4 += new Vector4(offsetPos.x, offsetPos.y, offsetPos.z, 0.0f);
        Vector4 radVec4 = Vector4.zero;
        radVec4.x = umolAtom.radius / 3;

        tex.SetPixel(atomID, 0, posAtomvec4); // Position

        tex.SetPixel(atomID, 1, radVec4); // Radius

        tex.SetPixel(atomID, 2, umolAtom.color); // Color
        atomColors.Add(umolAtom.color32);

        tex.SetPixel(atomID, 3, Vector4.one); // Visibility (not used!)

        tex.SetPixel(atomID, 4, posAtomvec4); // Base sphere position (doesn't change)

        tex.SetPixel(atomID, 5, Vector4.one); // Id texture matcap

        tex.SetPixel(atomID, 6, Vector4.one); // Equation

        tex.SetPixel(atomID, 7, Vector4.one); // visibility (used!)

        tex.SetPixel(atomID, 8, Vector4.one); // Scale

        tex.SetPixel(atomID, 9, Vector4.zero); // Selected atom

        tex.SetPixel(atomID, 10, Vector4.zero); // AO info


    }

    void RecordMeshAndParameters(Mesh mesh, Texture2D tex, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int cptAtoms) {

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.Clear();
        // Fill the mesh with the arrays
        mesh.SetVertices(vertices);

        for (int i = 0; i < uv.Count; i++) {
            Vector2 aa = uv[i];
            aa.y = aa.x / (float)(cptAtoms - 1);
            uv[i] = aa;
        }

        mesh.SetUVs(0, uv);
        mesh.SetIndices(triangles, MeshTopology.Triangles, 0, true, 0);

        // Upload the texture to the GPU
        tex.Apply(false, false); // DO NOT REMOVE !
        tex.wrapMode = TextureWrapMode.Clamp; // Mandatory to access the data in the shader
        tex.filterMode = FilterMode.Point; // Texture will be used reading pixels
        tex.anisoLevel = 0;

    }

    void AssignMaterial(GameObject curGO, Texture2D tex, float brightness, int NBPARAM, int atomsInMesh) {

        Material HBoptiMat = null;

        if (HBoptiMat == null)
            HBoptiMat = new Material(Shader.Find("UMol/Ball HyperBalls Shadow Merged"));
        
        materials.Add(HBoptiMat);

        //Set fixed parameters for the shader
        HBoptiMat.SetTexture("_MainTex", tex);
        // HBoptiMat.SetTexture("_MatCap", (Texture) Resources.Load("Images/MatCap/daphz05"));
        HBoptiMat.SetFloat("_Brightness", brightness);
        HBoptiMat.SetFloat("_NBParam", (float) NBPARAM);
        HBoptiMat.SetFloat("_NBAtoms", (float) atomsInMesh);
        curGO.GetComponent<Renderer>().sharedMaterial = HBoptiMat;

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


        if (coordAtomTexture != null)
            coordAtomTexture.Clear();
        coordAtomTexture = null;

        if (paramTextures != null)
            for (int i = 0; i < paramTextures.Length; i++) {
                GameObject.Destroy(paramTextures[i]);
            }
        paramTextures = null;

        if (atomToId != null)
            atomToId.Clear();
        atomToId = null;
        if (atomColors != null)
            atomColors.Clear();
        atomColors = null;
    }

    ///Compute an offset to avoid float precision issue in Hyperball shaders
    public static Vector3 computeHBOffsetCentroid(UnityMolSelection sel) {
        Vector3 offset = Vector3.zero;
        Vector3 centroid = sel.structures[0].currentModel.centroid;
        if (Mathf.Abs(centroid.x) > 100.0f) {
            offset.x = -centroid.x;
        }
        if (Mathf.Abs(centroid.y) > 100.0f) {
            offset.y = -centroid.y;
        }
        if (Mathf.Abs(centroid.z) > 100.0f) {
            offset.z = -centroid.z;
        }
        return offset;
    }
}
}