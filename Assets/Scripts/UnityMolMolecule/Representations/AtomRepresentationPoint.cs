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
using System.Collections.Generic;


namespace UMol {

/// <summary>
/// Handles the Point representation (flat 2D circle) for atoms
/// Uses 3rd Party library 'PCX' to rendering
/// <remarks>Metal doesn't support Disk Shader so use the Point one instead</remarks>
/// </summary>
public class AtomRepresentationPoint : AtomRepresentation {

    /// <summary>
    /// The GameObject of the Mesh holding the representation
    /// </summary>
    public GameObject meshGO;

    /// <summary>
    /// General Scaling factor for the Point size.
    /// </summary>
    public const float ScalingFactor = 0.625f;

    public Dictionary<UnityMolAtom, int> atomToId;

    public AtomRepresentationPoint(int idF, string structName, UnityMolSelection sel) {
        colorationType = colorType.atom;

        representationParent = UnityMolMain.getRepStructureParent(structName).transform;

        GameObject newRep = new("AtomRepresentationPoint") {
            transform = {
                parent = representationParent
            }
        };
        representationTransform = newRep.transform;

        selection = sel;
        idFrame = idF;

        atomToId = new Dictionary<UnityMolAtom, int>();

        displayPoints(newRep.transform);

        newRep.transform.localPosition = Vector3.zero;
        newRep.transform.localRotation = Quaternion.identity;
        newRep.transform.localScale = Vector3.one;

        nbAtoms = selection.atoms.Count;
    }

    /// <summary>
    /// Display the atoms as points user the shader.
    /// </summary>
    /// <param name="repParent">The Transform component of the Umol Representation GO</param>
    private void displayPoints(Transform repParent) {

        meshGO = new GameObject("AtomRepPoints") {
            transform = {
                parent = repParent
            }
        };

        Mesh mesh = createMeshForPoints();
        MeshFilter meshFilter = meshGO.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        MeshRenderer meshRenderer = meshGO.AddComponent<MeshRenderer>();

        // On Metal, Disk Shader is not supported.
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal) {

            meshRenderer.sharedMaterial = new Material(Shader.Find("Point Cloud/Point"));
            meshRenderer.sharedMaterial.EnableKeyword("_DISTANCE_ON");
        } else {
            meshRenderer.sharedMaterial = new Material(Shader.Find("Point Cloud/Disk"));

        }
        //Adjust the size with a arbitrary factor
        float scaleX = GameObject.Find("LoadedMolecules").transform.localScale.x;
        meshRenderer.sharedMaterial.SetFloat("_PointSize", ScalingFactor * scaleX);

    }

    /// <summary>
    /// Create the Mesh from the list of atoms selected
    /// </summary>
    /// <returns>the Mesh created</returns>
    private Mesh createMeshForPoints() {
        Mesh mesh = new();
        int N = selection.atoms.Count;
        mesh.indexFormat = selection.atoms.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;

        Vector3[] vertices = new Vector3[N];
        Color32[] colors = new Color32[N];
        int[] indices = new int[N];

        if (idFrame != -1) {
            for (int j = 0; j < N; j++) {
                vertices[j] = selection.extractTrajFramePositions[idFrame][j];
                colors[j] = selection.atoms[j].color;
                indices[j] = j;
                atomToId[selection.atoms[j]] = j;
            }
        }

        else {
            for (int i = 0; i < N; i++) {
                vertices[i] = selection.atoms[i].position;
                colors[i] = selection.atoms[i].color;
                indices[i] = i;
                atomToId[selection.atoms[i]] = i;
            }
        }
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetIndices(indices, MeshTopology.Points, 0);

        return mesh;
    }

    /// <summary>
    /// Destroy the representation
    /// </summary>
    public override void Clean() {
        if(meshGO != null){
            GameObject.Destroy(meshGO.GetComponent<MeshFilter>().sharedMesh);
            GameObject.Destroy(meshGO.GetComponent<MeshRenderer>().sharedMaterial);
        }
        if(atomToId != null) {
            atomToId.Clear();
        }
        atomToId = null;
    }
}
}
