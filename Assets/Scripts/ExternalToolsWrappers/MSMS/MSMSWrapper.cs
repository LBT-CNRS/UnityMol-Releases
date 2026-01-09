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
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System;
using WebSocketSharp;
using Debug = UnityEngine.Debug;

namespace UMol {

/// <summary>
/// Wrapper to handle the MSMS external binary
/// </summary>
public static class MSMSWrapper {

    /// <summary>
    /// static counter to MSMS calls to identify easily the temporary MSMS output file.
    /// </summary>
    private static int idMSMS = 0;

    /// <summary>
    /// Path to MSMS Binary
    /// null if the running platform is not supported
    /// </summary>
    private static string binaryPath
    {
        get {
            string basePath = Application.streamingAssetsPath + "/MSMS";

            return Application.platform switch {
                RuntimePlatform.OSXPlayer or RuntimePlatform.OSXEditor => basePath + "/OSX/msms",
                RuntimePlatform.LinuxPlayer or RuntimePlatform.LinuxEditor => basePath + "/Linux/msms",
                RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor => basePath + "/Windows/msms.exe",
                _ => null
            };
        }
    }

    /// <summary>
    /// Handle & Manage the call to the MSMS external binary
    /// </summary>
    /// <param name="idF">the Frame ID</param>
    /// <param name="sel">the selection to compute the surface on</param>
    /// <param name="mData"> the MeshData generated from the computation</param>
    /// <param name="density">the density value from the MSMS algorithm</param>
    /// <param name="probeRad">the probe radius from the MSMS algorithm</param>
    /// <param name="tempPath">a temporary path folder to write MSMS temporary result file</param>
    public static void createMSMSSurface(int idF, UnityMolSelection sel, ref MeshData mData,
        float density=15.0f, float probeRad=1.4f, string tempPath="") {

        if (tempPath.IsNullOrEmpty()) {
            tempPath = Application.temporaryCachePath;
        }

        if (binaryPath == null) {
            Debug.LogError("MSMS failed: no binary for platform " + Application.platform);
            return;
        }
        if (!File.Exists(binaryPath)) {
            Debug.LogError("MSMS failed: couldn't locate binary " + binaryPath);
            return;
        }

        long time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        string tmpFileName = "tmpmsms_" + idMSMS + "_" + time + "_" + sel.name;
        idMSMS++;
        string path = tempPath + "/" + tmpFileName + ".xyzr";
        string pdbLines = toXYZR(sel, idF);
        if (pdbLines.Length == 0) {
            pdbLines = toXYZR(sel, idF, true);
        }

        if (pdbLines.Length == 0) {
            return;
        }
        StreamWriter writer = new(path, false);
        writer.WriteLine(pdbLines);
        writer.Close();

        string opt = "-probe_radius " + probeRad.ToString("f2", CultureInfo.InvariantCulture)
                     + " -no_area -density " + density.ToString("f2", CultureInfo.InvariantCulture)
                     + " -if \"" + path + "\" -of \"" + tempPath + "/" + tmpFileName + "\"";
        ProcessStartInfo info = new(binaryPath, opt) {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Process subprocess = Process.Start(info);

        string log = subprocess.StandardOutput.ReadToEnd();

        subprocess.WaitForExit();


        if (!log.Contains("MSMS terminated normally")) {
            Debug.LogError("MSMS failed : " + log);
            return;
        }


        string pathV = tempPath + "/" + tmpFileName + ".vert";
        string pathF = tempPath + "/" + tmpFileName + ".face";

        int Nvert = getCountFromHeader(pathV);
        int Ntri = getCountFromHeader(pathF) * 3;


        if (mData == null) {
            mData = new MeshData {
                vertices = new Vector3[Nvert],
                triangles = new int[Ntri],
                colors = new Color32[Nvert],
                normals = new Vector3[Nvert],
                atomByVert = new int[Nvert]
            };
        }
        else {
            if (Nvert > mData.vertices.Length) {//We need more space
                mData.vertices = new Vector3[Nvert];
                mData.colors = new Color32[Nvert];
                mData.normals = new Vector3[Nvert];
                mData.atomByVert = new int[Nvert];
                mData.triangles = new int[Ntri];
            }
        }
        mData.nVert = Nvert;
        mData.nTri = Ntri / 3;


        parseVerticesNormals(pathV, ref mData);
        parseTriangles(pathF, ref mData.triangles);

        mData.FillWhite();

        File.Delete(pathV);
        File.Delete(pathF);
        File.Delete(path);
    }

    /// <summary>
    /// Export an UnityMolSelection to a string in XYZ format.
    /// </summary>
    /// <param name="sel">the UnityMolSelection</param>
    /// <param name="idF">the frame ID</param>
    /// <param name="withHET">whether hetero atoms are processed or not.</param>
    /// <returns></returns>
    static string toXYZR(UnityMolSelection sel, int idF, bool withHET = false) {
        StringBuilder sb = new();
        for (int i = 0; i < sel.atoms.Count; i++) {
            UnityMolAtom a = sel.atoms[i];
            bool isWater = WaterSelection.waterResidues.Contains(a.residue.name, StringComparer.OrdinalIgnoreCase);
            if ((!a.isHET && !isWater) || withHET) {
                if (idF != -1) {
                    Vector3 p = sel.extractTrajFramePositions[idF][i];
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1} {2} ", -p.x, p.y, p.z);
                }
                else {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1} {2} ", -a.position.x, a.position.y, a.position.z);
                }
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2}\n", a.radius);
            }
        }
        return sb.ToString();
    }

    private static void parseVerticesNormals(string path, ref MeshData mData) {
        if (!File.Exists(path)) {
            Debug.LogError("MSMS failed");
            return;
        }
        StreamReader sr = new(path);
        float[] buff = new float[6];

        using (sr) {
            string line;

            sr.ReadLine();
            sr.ReadLine();
            sr.ReadLine();

            int id = 0;
            while ((line = sr.ReadLine()) != null) {

                int stopped = Reader.ParseFloats(6, line, ref buff);
                int end = 0;
                int dummy = Reader.ParseInt(ref end, line, stopped);
                int idV = Reader.ParseInt(line, end) - 1;

                mData.atomByVert[id] = idV;
                mData.vertices[id] = new Vector3(-buff[0], buff[1], buff[2]);
                mData.normals[id] = new Vector3(-buff[3], buff[4], buff[5]);
                id++;
            }
        }
    }

    static void parseTriangles(string path, ref int[] tris) {
        StreamReader sr = new(path);

        using (sr) {
            string line;
            sr.ReadLine();
            sr.ReadLine();
            sr.ReadLine();

            int id = 0;
            while ((line = sr.ReadLine()) != null) {
                int newStart = 0;
                int newStart2 = 0;
                int t1 = Reader.ParseInt(ref newStart, line, 0) - 1;
                int t3 = Reader.ParseInt(ref newStart2, line, newStart) - 1;
                int t2 = Reader.ParseInt(ref newStart, line, newStart2) - 1;
                tris[id++] = t1;
                tris[id++] = t2;
                tris[id++] = t3;
            }
        }
    }


    static int getCountFromHeader(string path) {
        StreamReader sr = new(path);
        int N;
        using (sr) {
            sr.ReadLine();
            sr.ReadLine();
            string line = sr.ReadLine();

            //Should contain number of verts
            N = Reader.ParseInt(line);
        }
        return N;
    }
}
}
