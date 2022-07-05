using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System;

namespace UMol {
public class MSMSWrapper {

    public static int idMSMS = 0;
    public static string BinaryPath
    {
        get {
            var basePath = Application.streamingAssetsPath + "/MSMS";

            if (Application.platform == RuntimePlatform.OSXPlayer ||
                    Application.platform == RuntimePlatform.OSXEditor)
                return basePath + "/OSX/msms";

            if (Application.platform == RuntimePlatform.LinuxPlayer ||
                    Application.platform == RuntimePlatform.LinuxEditor)
                return basePath + "/Linux/msms";

            return basePath + "/Windows/msms.exe";
        }
    }

    static string toXYZR(UnityMolSelection sel, bool withHET = false) {
        StringBuilder sb = new StringBuilder();
        foreach (UnityMolAtom a in sel.atoms) {
            bool isWater = WaterSelection.waterResidues.Contains(a.residue.name, StringComparer.OrdinalIgnoreCase);
            if ((!a.isHET && !isWater) || withHET) {
                sb.Append((-a.position.x).ToString(CultureInfo.InvariantCulture));
                sb.Append(" ");
                sb.Append(a.position.y.ToString(CultureInfo.InvariantCulture));
                sb.Append(" ");
                sb.Append(a.position.z.ToString(CultureInfo.InvariantCulture));
                sb.Append(" ");
                sb.Append(a.radius.ToString("f2", CultureInfo.InvariantCulture));
                sb.Append("\n");
            }
        }
        return sb.ToString();
    }

    //Calls MSMS executable to create meshes
    public static MeshData callMSMS(UnityMolSelection sel, float density, float probeRad, string tempPath, ref int[] vertIds) {
        string binPath = BinaryPath;

        long time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        string tmpFileName = "tmpmsms_" + idMSMS.ToString() + "_" + time + "_" + sel.name;
        idMSMS++;
        string path = tempPath + "/" + tmpFileName + ".xyzr";
        string pdbLines = toXYZR(sel);
        if (pdbLines.Length == 0) {
            pdbLines = toXYZR(sel, true);
        }

        if (pdbLines.Length == 0) {
            return new MeshData();
        }
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(pdbLines);
        writer.Close();

        string opt = "-probe_radius " + probeRad.ToString("f2", CultureInfo.InvariantCulture)
                     + " -no_area -density " + density.ToString("f2", CultureInfo.InvariantCulture) + " -if " + path + " -of " + tempPath + "/" + tmpFileName;
        ProcessStartInfo info = new ProcessStartInfo(binPath, opt);
        info.UseShellExecute = false;
        info.CreateNoWindow = true;
        info.RedirectStandardInput = true;
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;

        Process _subprocess = Process.Start(info);

        // string Error = _subprocess.StandardError.ReadToEnd();
        string log = _subprocess.StandardOutput.ReadToEnd();
        // UnityEngine.Debug.Log(log);
        // UnityEngine.Debug.LogError(Error);
        // int ExitCode = _subprocess.ExitCode;

        // _subprocess.StandardInput.Close();
        _subprocess.WaitForExit();


        if (log == null || !log.Contains("MSMS terminated normally")) {
            UnityEngine.Debug.LogError("MSMS failed : " + log);
            return new MeshData();
        }


        MeshData m = new MeshData();

        string pathV = tempPath + "/" + tmpFileName + ".vert";
        string pathF = tempPath + "/" + tmpFileName + ".face";

        List<Vector3> verts = parseVertices(pathV, ref vertIds);
        List<int> triangles = parseTriangles(pathF);

        m.vertices = verts.ToArray();
        m.triangles = triangles.ToArray();
        m.colors = new Color32[m.vertices.Length];

        for (int i = 0; i < m.colors.Length; i++) {
            m.colors[i] = Color.white;
        }

        // MeshSmoother.smoothMeshLaplacian(m.vertices, m.triangles, 1);

        File.Delete(pathV);
        File.Delete(pathF);
        File.Delete(path);


        return m;
    }

    static List<Vector3> parseVertices(string path, ref int[] vertIds) {
        StreamReader sr = new StreamReader(path);
        List<Vector3> verts = new List<Vector3>();
        List<int> tmpIds = new List<int>();

        Vector3 cur = Vector3.zero;
        using (sr) { // Don't use garbage collection but free temp memory after reading the pdb file

            string line;
            while ((line = sr.ReadLine()) != null) {
                string[] sline = line.Split(new [] { ' '}, System.StringSplitOptions.RemoveEmptyEntries);
                if (sline.Length == 9) {
                    cur.x = -float.Parse(sline[0], System.Globalization.CultureInfo.InvariantCulture);
                    cur.y = float.Parse(sline[1], System.Globalization.CultureInfo.InvariantCulture);
                    cur.z = float.Parse(sline[2], System.Globalization.CultureInfo.InvariantCulture);
                    int idV = int.Parse(sline[7]) - 1;
                    tmpIds.Add(idV);
                    verts.Add(cur);
                }
            }
        }
        vertIds = tmpIds.ToArray();
        return verts;
    }

    static List<int> parseTriangles(string path) {
        StreamReader sr = new StreamReader(path);
        List<int> tris = new List<int>();

        using (sr) { // Don't use garbage collection but free temp memory after reading the pdb file

            string line;
            while ((line = sr.ReadLine()) != null) {
                string[] sline = line.Split(new [] { ' '}, System.StringSplitOptions.RemoveEmptyEntries);
                if (sline.Length == 5) {
                    tris.Add(int.Parse(sline[0]) - 1);
                    tris.Add(int.Parse(sline[2]) - 1);
                    tris.Add(int.Parse(sline[1]) - 1);
                }
            }
        }
        return tris;
    }

    public static GameObject createMSMSSurface(Transform meshPar, string name, UnityMolSelection select,
            ref MeshData meshD, ref int[] vertIds, float density = 15.0f, float probeRad = 1.4f) {

        meshD = callMSMS(select, density, probeRad, Application.temporaryCachePath, ref vertIds);

        if (meshD != null) {

            GameObject newMeshGo = new GameObject(name + "+msms");
            newMeshGo.transform.parent = meshPar;
            newMeshGo.transform.localPosition = Vector3.zero;
            newMeshGo.transform.localRotation = Quaternion.identity;
            newMeshGo.transform.localScale = Vector3.one;

            Mesh newMesh = new Mesh();
            newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            newMesh.vertices = meshD.vertices;
            newMesh.triangles = meshD.triangles;
            newMesh.colors32 = meshD.colors;

            newMesh = mattatz.MeshSmoothingSystem.MeshSmoothing.LaplacianFilter(newMesh, 3);

            newMesh.RecalculateNormals();

            MeshFilter mf = newMeshGo.AddComponent<MeshFilter>();
            mf.mesh = newMesh;

            MeshRenderer mr = newMeshGo.AddComponent<MeshRenderer>();

            Material mat = new Material(Shader.Find("Custom/SurfaceVertexColor"));
            mat.SetFloat("_Glossiness", 0.0f);
            mat.SetFloat("_Metallic", 0.0f);
            mat.SetFloat("_AOIntensity", 1.03f);
            mat.SetFloat("_AOPower", 8.0f);

            mr.material = mat;

            return newMeshGo;
        }
        UnityEngine.Debug.LogError("Failed to create MSMS mesh");

        return null;
    }
}
}
