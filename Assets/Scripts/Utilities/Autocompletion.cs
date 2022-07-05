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
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using UMol.API;
using UMol;
using TMPro;


[RequireComponent (typeof (TMP_Text))]
public class Autocompletion : MonoBehaviour {

    private string[] APIFunctionNames;
    private string[] loadedMolAndSelections;
    public TMP_InputField inf;
    public RawImage img;
    bool tabPressed = false;
    int choice = 0;
    List<int> completion;
    List<int> completionMolSel;
    int idQuote = -1;

    void Awake() {


        MethodInfo[] methodInfos = typeof(APIPython).GetMethods(BindingFlags.Public |
                                   BindingFlags.Static);

        APIFunctionNames = new string[methodInfos.Length];
        int i = 0;
        foreach (MethodInfo mi in methodInfos.Distinct()) {
            APIFunctionNames[i++] = mi.Name;
        }
    }
    void Start() {
        // List<int> test = AutoCompleteText("showD", APIFunctionNames);
        // foreach(int id in test){
        //     Debug.Log("Suggestion: "+APIFunctionNames[id]);
        // }

        if (inf != null) {
            inf.onValueChanged.AddListener(delegate { RefreshSuggestions(inf);} );
        }
    }


    void RefreshSuggestions(TMP_InputField inputF) {
        TMP_Text txt = GetComponent<TMP_Text>();
        int inlen = inputF.text.Length;

        if (inputF.text.Contains("(")) {//Complete molecule names and selection names

            if (inputF.text.Contains("\"") || inputF.text.Contains("'") ) {

                loadedMolAndSelections = getMolAndSelNames();

                bool curTabPress = false;
                if (inlen > 0 && inputF.text[inlen - 1] == '\t') {
                    curTabPress = true;

                    if (tabPressed) { //Change choice
                        choice++;
                    }
                    inputF.SetValue(inputF.text.Remove(inlen - 1));
                }
                else {
                    tabPressed = false;
                    choice = 0;
                }

                if (!curTabPress || completionMolSel == null) {//Update suggestions
                    txt.text = "";
                    string molselToComplete = extractMolSelString(inputF.text, out idQuote);

                    if (idQuote != -1) {

                        completionMolSel = AutoCompleteText(molselToComplete, loadedMolAndSelections);
                        foreach (int id in completionMolSel) {
                            txt.text += "<color=red>-</color>" + loadedMolAndSelections[id] + "\n";
                        }

                    }
                    else {
                        completionMolSel = new List<int>();
                    }
                    
                    if (img != null) {
                        img.enabled = completionMolSel.Count > 0;
                    }
                }

                if (completionMolSel.Count > 0) {
                    if (curTabPress) {

                        if (choice >= completionMolSel.Count) {
                            choice = 0;
                        }
                        string newS = inputF.text.Substring(0, idQuote) + loadedMolAndSelections[completionMolSel[choice]];
                        inputF.SetValue(newS);
                        inputF.stringPosition = inputF.text.Length;
                        updateTextCompletionWithChoice(txt, completionMolSel, loadedMolAndSelections);
                        tabPressed = true;
                    }
                }
            }
        }
        else {

            bool curTabPress = false;
            if (inlen > 0 && inputF.text[inlen - 1] == '\t') {
                curTabPress = true;

                if (tabPressed) { //Change choice
                    choice++;
                }
                inputF.SetValue(inputF.text.Remove(inlen - 1));
            }
            else {
                tabPressed = false;
                choice = 0;
            }

            if (!curTabPress || completion == null) {//Update suggestions
                txt.text = "";

                completion = AutoCompleteText(inputF.text, APIFunctionNames);
                foreach (int id in completion) {
                    txt.text += "<color=red>-</color>" + APIFunctionNames[id] + "\n";
                }

                if (img != null) {
                    img.enabled = completion.Count > 0;
                }
            }

            if (completion.Count > 0) {
                if (curTabPress) {

                    if (choice >= completion.Count) {
                        choice = 0;
                    }
                    inputF.SetValue(APIFunctionNames[completion[choice]]);
                    inputF.stringPosition = inputF.text.Length;
                    updateTextCompletionWithChoice(txt, completion, APIFunctionNames);
                    tabPressed = true;
                }
            }
        }
    }

    string extractMolSelString(string s, out int idQuote) {
        int iddq = s.LastIndexOf("\"");
        int idsq = s.LastIndexOf("'");
        int id = iddq + 1;

        idQuote = -1;

        if (iddq < 0) {
            id = idsq + 1;
        }
        if (id == 0 || id >= s.Length - 1) {
            return "";
        }
        idQuote = id;
        return s.Substring(id);

    }
    string[] getMolAndSelNames() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        List<string> res = new List<string>();

        foreach (UnityMolStructure s in sm.loadedStructures) {
            res.Add(s.uniqueName);
        }
        foreach (string selname in selM.selections.Keys) {
            res.Add(selname);
        }

        return res.ToArray();
    }

    void updateTextCompletionWithChoice(TMP_Text txt, List<int> complList, string[] compleText) {
        txt.text = "";

        int i = 0;
        foreach (int id in complList) {
            if (i == choice) {
                txt.text += "<u><color=red>-</color>" + compleText[id] + "</u>\n";
            }
            else {
                txt.text += "<color=red>-</color>" + compleText[id] + "\n";
            }
            i++;
        }
    }



    /// <summary>A textField to popup a matching popup, based on developers input values.</summary>
    /// <param name="input">string input.</param>
    /// <param name="source">the data of all possible values (string).</param>
    /// <param name="maxShownCount">the amount to display result.</param>
    /// <param name="levenshteinDistance">
    /// value between 0f ~ 1f,
    /// - more then 0f will enable the fuzzy matching
    /// - 1f = anything thing is okay.
    /// - 0f = require full match to the reference
    /// - recommend 0.4f ~ 0.7f
    /// </param>
    /// <returns>A list of compatible string ids in source.</returns>
    public static List<int> AutoCompleteText(string input, string[] source, int maxShownCount = 5, float levenshteinDistance = 0.5f)
    {
        List<int> result = new List<int>();
        HashSet<string> uniqueSrc = new HashSet<string>(source);

        if (!string.IsNullOrEmpty(input) && input.Length >= 2 && input.Length < 25) {

            string keywords = input;

            for (int i = 0; i < source.Length; i++) {
                if (result.Count == maxShownCount) {
                    return result;
                }
                if (source[i].StartsWith(keywords)) {
                    result.Add(i);
                    uniqueSrc.Remove(source[i]);
                }
            }
            if (result.Count == maxShownCount) {
                return result;
            }

            List<string> toRM = new List<string>();

            foreach (string s in uniqueSrc) {
                if (result.Count == maxShownCount) {
                    return result;
                }
                if (s.Contains(keywords)) {
                    result.Add(Array.IndexOf(source, s));
                    toRM.Add(s);
                }
            }
            if (result.Count == maxShownCount) {
                return result;
            }
            foreach (string s in toRM) {
                uniqueSrc.Remove(s);
            }


            levenshteinDistance = Mathf.Clamp01(levenshteinDistance);
            foreach (string s in uniqueSrc) {
                if (result.Count == maxShownCount) {
                    return result;
                }
                int distance = LevenshteinDistance(s, keywords);
                bool closeEnough = (int)(levenshteinDistance * s.Length) > distance;

                if (closeEnough) {
                    result.Add(Array.IndexOf(source, s));
                }
            }

        }

        return result;
    }

/// <summary>Computes the Levenshtein Edit Distance between two enumerables.</summary>
/// <typeparam name="T">The type of the items in the enumerables.</typeparam>
/// <param name="lhs">The first enumerable.</param>
/// <param name="rhs">The second enumerable.</param>
/// <returns>The edit distance.</returns>
/// <see cref="https://blogs.msdn.microsoft.com/toub/2006/05/05/generic-levenshtein-edit-distance-with-c/"/>
    public static int LevenshteinDistance<T>(IEnumerable<T> lhs, IEnumerable<T> rhs) where T : System.IEquatable<T>
    {
        // Validate parameters
        if (lhs == null) throw new System.ArgumentNullException("lhs");
        if (rhs == null) throw new System.ArgumentNullException("rhs");

        // Convert the parameters into IList instances
        // in order to obtain indexing capabilities
        IList<T> first = lhs as IList<T> ?? new List<T>(lhs);
        IList<T> second = rhs as IList<T> ?? new List<T>(rhs);

        // Get the length of both.  If either is 0, return
        // the length of the other, since that number of insertions
        // would be required.
        int n = first.Count, m = second.Count;
        if (n == 0) return m;
        if (m == 0) return n;

        // Rather than maintain an entire matrix (which would require O(n*m) space),
        // just store the current row and the next row, each of which has a length m+1,
        // so just O(m) space. Initialize the current row.
        int curRow = 0, nextRow = 1;

        int[][] rows = new int[][] { new int[m + 1], new int[m + 1] };
        for (int j = 0; j <= m; ++j)
            rows[curRow][j] = j;

        // For each virtual row (since we only have physical storage for two)
        for (int i = 1; i <= n; ++i)
        {
            // Fill in the values in the row
            rows[nextRow][0] = i;

            for (int j = 1; j <= m; ++j)
            {
                int dist1 = rows[curRow][j] + 1;
                int dist2 = rows[nextRow][j - 1] + 1;
                int dist3 = rows[curRow][j - 1] +
                            (first[i - 1].Equals(second[j - 1]) ? 0 : 1);

                rows[nextRow][j] = System.Math.Min(dist1, System.Math.Min(dist2, dist3));
            }

            // Swap the current and next rows
            if (curRow == 0)
            {
                curRow = 1;
                nextRow = 0;
            }
            else
            {
                curRow = 0;
                nextRow = 1;
            }
        }

        // Return the computed edit distance
        return rows[curRow][m];
    }

}
