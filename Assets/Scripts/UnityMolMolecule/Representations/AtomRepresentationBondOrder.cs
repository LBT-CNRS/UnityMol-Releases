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
using UnityEngine.Rendering;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine.XR;

using VRTK;

namespace UMol {

public class AtomRepresentationBondOrder : AtomRepresentation {

    public List<GameObject> meshesGO;
    public Dictionary<int, KeyValuePair<int, int> > coordAtomTexture;
    public Texture2D[] paramTextures;
    public Dictionary<UnityMolAtom, int> atomToId;
    public List<Color> atomColors;
    public bool withShadow = true;


    public AtomRepresentationBondOrder(string structName, UnityMolSelection sel) {

        GameObject loadedMolGO = UnityMolMain.getRepresentationParent();

        representationParent = loadedMolGO.transform.Find(structName);
        if (UnityMolMain.inVR() && representationParent == null) {

            Transform clref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
            Transform crref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);
            if (clref != null) {
                representationParent = clref.Find(structName);
            }
            if (representationParent == null && crref != null) {
                representationParent = crref.Find(structName);
            }
        }
        if (representationParent == null) {
            representationParent = (new GameObject(structName).transform);
            representationParent.parent = loadedMolGO.transform;
            representationParent.localPosition = Vector3.zero;
            representationParent.localRotation = Quaternion.identity;
            representationParent.localScale = Vector3.one;
        }
        GameObject newRep = new GameObject("AtomBondOrderRepresentation");
        newRep.transform.parent = representationParent;
        representationTransform = newRep.transform;


        selection = sel;

        atomToId = new Dictionary<UnityMolAtom, int>();
        DisplayHBMesh(newRep.transform);
        // newRep.transform.position -= offset;
        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.Count;
    }

    private void DisplayHBMesh(Transform repParent) {

        meshesGO = new List<GameObject>();

        coordAtomTexture = new Dictionary<int, KeyValuePair<int, int> >();
        atomColors = new List<Color>(selection.Count);
        int idmesh = 0;
        int cptAtoms = 0;
        int currentVertex = 0;
        int NBPARAM = 11; //Number of parameters in the texture


        // collidersParent = new GameObject("Colliders");
        // collidersParent.transform.parent = AtomMeshParent.transform;

        GameObject currentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        currentGO.transform.name = "AtomMesh0";
        currentGO.transform.parent = repParent;
        GameObject.Destroy(currentGO.GetComponent<Collider>());

        GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh tmpMesh = tmp.GetComponent<MeshFilter>().sharedMesh;
        GameObject.Destroy(tmp);

        Vector3[] verticesBase = tmpMesh.vertices;
        int[] trianglesBase = tmpMesh.triangles;

        int VERTICES_IN_CUBE = verticesBase.Length; // 2 4 vertices by cube
        int LIMIT_VERTICES_IN_MESH = 65534 / 4; // Unity limit : number of vertices in a mesh => needs to be smaller because of float precision in shader
        long totalVertices = selection.Count * VERTICES_IN_CUBE;
        int maxAtomsinMesh = LIMIT_VERTICES_IN_MESH / VERTICES_IN_CUBE;
        int currentAtomsinMesh = (int) Mathf.Min(maxAtomsinMesh, selection.Count - (idmesh * maxAtomsinMesh));


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


        float brightness = 1.0f;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < selection.Count; i++) {

            if (cptAtoms == currentAtomsinMesh) {
                RecordMeshAndParameters(combinedMeshes[idmesh], tmpparamTextures[idmesh],
                                        newVertices.ToArray(), newTriangles.ToArray(), newUV.ToArray());

                currentGO.GetComponent<MeshFilter>().mesh = combinedMeshes[idmesh];
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

                currentAtomsinMesh = (int) Mathf.Min(maxAtomsinMesh, selection.Count - (idmesh * maxAtomsinMesh));

                tmpparamTextures.Add(new Texture2D(currentAtomsinMesh, NBPARAM, TextureFormat.RGBAFloat, false));
                combinedMeshes.Add(new Mesh());
                currentVertex = 0;
                cptAtoms = 0;

            }


            Vector3 posAtom = selection.atoms[i].position;

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


            StoreParametersInTexture(tmpparamTextures[idmesh], cptAtoms, selection.atoms[i]);

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
                                    newVertices.ToArray(), newTriangles.ToArray(), newUV.ToArray());

            combinedMeshes[idmesh].RecalculateBounds();
            currentGO.GetComponent<MeshFilter>().mesh = combinedMeshes[idmesh];
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

    void StoreParametersInTexture(Texture2D tex, int atomID, UnityMolAtom umolAtom) {

        Vector4 posAtomvec4 = umolAtom.PositionVec4;

        Vector4 radVec4 = Vector4.zero;
        radVec4.x = umolAtom.radius / 3;

        tex.SetPixel(atomID, 0, posAtomvec4); // Position

        tex.SetPixel(atomID, 1, radVec4); // Radius

        tex.SetPixel(atomID, 2, umolAtom.color); // Color
        atomColors.Add(umolAtom.color);

        tex.SetPixel(atomID, 3, Vector4.one); // Visibility (not used!)

        tex.SetPixel(atomID, 4, posAtomvec4); // Base sphere position (doesn't change)

        tex.SetPixel(atomID, 5, Vector4.one); // Id texture matcap

        tex.SetPixel(atomID, 6, Vector4.one); // Equation

        tex.SetPixel(atomID, 7, Vector4.one); // visibility (used!)

        tex.SetPixel(atomID, 8, Vector4.one * 0.5f); // Scale

        tex.SetPixel(atomID, 9, Vector4.zero); // Selected atom

        tex.SetPixel(atomID, 10, Vector4.zero); // AO info


    }

    void RecordMeshAndParameters(Mesh mesh, Texture2D tex, Vector3[] vertices, int[] triangles, Vector2[] uv) {

        mesh.Clear();
        // Fill the mesh with the arrays
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        // Upload the texture to the GPU
        tex.Apply(false, false); // DO NOT REMOVE !!!!!!!!!!!!!!!!!!
        tex.wrapMode = TextureWrapMode.Clamp; // Mandatory to access the data in the shader
        tex.filterMode = FilterMode.Point; // Texture will be used reading pixels
        tex.anisoLevel = 0;


    }

    void AssignMaterial(GameObject curGO, Texture2D tex, float brightness, int NBPARAM, int atomsInMesh) {


        Material HBoptiMat = null;

        if (!withShadow) {
            HBoptiMat = new Material(Shader.Find("UMol/Ball HyperBalls Merged"));
        }
        else {
            HBoptiMat = new Material(Shader.Find("UMol/Ball HyperBalls Shadow Merged"));
        }

        //Set fixed parameters for the shader
        HBoptiMat.SetTexture("_MainTex", tex);
        HBoptiMat.SetTexture("_MatCap", (Texture) Resources.Load("Images/MatCap/daphz05"));
        HBoptiMat.SetFloat("_Brightness", brightness);
        HBoptiMat.SetFloat("_NBParam", (float) NBPARAM);
        HBoptiMat.SetFloat("_NBAtoms", (float) atomsInMesh);
        curGO.GetComponent<Renderer>().sharedMaterial = HBoptiMat;

        if (withShadow) {
            curGO.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
            curGO.GetComponent<Renderer>().receiveShadows = true;
        }
        else {
            curGO.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            curGO.GetComponent<Renderer>().receiveShadows = false;
        }
    }
    public override void Clean(){}
}
}