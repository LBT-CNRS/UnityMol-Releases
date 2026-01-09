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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using MiniJSON;

namespace UMol {

/// <summary>
/// Parse a JSON fieldLines.
/// File Format: https://manual.gromacs.org/current/reference-manual/file-formats.html#gro
/// </summary>
public class FieldLinesReader {
    public Dictionary <string, List<Vector3>> LinesPositions;


	public FieldLinesReader () {}
	public FieldLinesReader (string filePath) {
        LinesPositions = new Dictionary <string, List<Vector3>>();
		IDictionary deserializedData;
        StreamReader sr;

        if (Application.platform == RuntimePlatform.Android) {
            StringReaderStream textStream = new(AndroidUtils.GetFileText(filePath));
            sr = new StreamReader(textStream);
        }
        else {
            sr = new StreamReader(filePath);
        }

        using(sr) {
			string jsonString = sr.ReadToEnd();
			deserializedData = (IDictionary) Json.Deserialize(jsonString);
		}


		foreach (string v in deserializedData.Keys) {
			List<Vector3> listP = new();
			IList d = (IList)deserializedData[v];

			foreach (IList p in d) {
				float x = -Convert.ToSingle( p[0] );
				float y = Convert.ToSingle( p[1] );
				float z = Convert.ToSingle( p[2] );

				Vector3 pos = new(x, y, z);
				listP.Add(pos);

			}

			LinesPositions[v] = listP;

		}
	}

    /// <summary>
    /// Compute Field lines from a DX object
    /// </summary>
    /// <param name="r">DX object parsed from a file</param>
    /// <param name="nbIter">Number of iterations</param>
    /// <param name="gradThreshold">Threshold for fieldlines</param>
    /// <returns>FieldLinesReader object</returns>
    public static FieldLinesReader ComputeFieldlinesToFlReader(DXReader r, int nbIter, float gradThreshold) {

        Vector3[] grad = r.gradient;

        List<Vector3>[] fl = FieldLinesBurst.computeFL(grad, r.deltaS, r.origin, r.gridSize, nbIter, gradThreshold, r.xl, r.yl, r.zl);

        if (fl == null) {
            return null;
        }

        FieldLinesReader fakeFLR = new();
        Dictionary<string, List<Vector3>> linesPos = new();
        int id = 0;
        for (int i = 0; i < fl.Length; i++) {
            if (fl[i].Count != 0) {
                linesPos[id.ToString()] = fl[i];
                id++;
            }
        }
        fakeFLR.LinesPositions = linesPos;
        return fakeFLR;
    }
}
}
