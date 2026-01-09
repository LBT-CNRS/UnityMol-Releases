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
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UMol.API;
using UMol;
using Unity.Mathematics;
using TMPro;


[RequireComponent (typeof (TMP_Text))]
public class Autocompletion : MonoBehaviour {

    private List<string> APIFunctionNames;
    private List<string> loadedMolAndSelections;
    private List<string> possiblePaths;
    public TMP_InputField inf;
    public RawImage img;
    bool tabPressed = false;
    int choice = 0;
    List<int> completion;
    List<int> completionMolSel;
    int idQuote = -1;

    bool escapePressed = false;

    void Awake() {

        MethodInfo[] methodInfos = typeof(APIPython).GetMethods(BindingFlags.Public |
                                   BindingFlags.Static);

        APIFunctionNames = new List<string>(methodInfos.Length);
        foreach (MethodInfo mi in methodInfos.Distinct()) {
            APIFunctionNames.Add(mi.Name);
        }
    }
    void Start() {

        if (inf != null) {
            inf.onValueChanged.AddListener(delegate { RefreshSuggestions(inf);} );
        }
    }

    void Update() {
        if (inf != null && inf.text.Length == 0 && escapePressed) {
            escapePressed = false;
        }
        if (Input.GetKey(KeyCode.Tab)) {
            escapePressed = false;
        }
        if (Input.GetKey(KeyCode.Escape)) {
            escapePressed = true;
            if (img != null)
                img.enabled = false;
            TMP_Text txt = GetComponent<TMP_Text>();
            txt.text = "";
        }
    }


    void RefreshSuggestions(TMP_InputField inputF) {
        TMP_Text txt = GetComponent<TMP_Text>();
        int inlen = inputF.text.Length;

        if (escapePressed)
            return;
        if (inputF.text.Contains("(") && !inputF.text.EndsWith(")")) {//Complete molecule names and selection names and paths

            if (inputF.text.Contains("\"") || inputF.text.Contains("'") ) {

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
                    string molselpathToComplete = extractMolSelString(inputF.text, out idQuote);

                    if (idQuote != -1) {
                        loadedMolAndSelections = getMolAndSelNames();

                        if (inputF.text.StartsWith("load") || inputF.text.StartsWith("export") || inputF.text.StartsWith("read")) {
                            possiblePaths = getPossiblePath(molselpathToComplete);
                            if (possiblePaths != null && possiblePaths.Count > 0) {
                                loadedMolAndSelections.AddRange(possiblePaths);
                            }
                        }
                        completionMolSel = AutoCompleteText(molselpathToComplete, loadedMolAndSelections);

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

                        if (choice >= completionMolSel.Count ) {
                            choice = 0;
                        }
                        string newS = inputF.text.Substring(0, idQuote);
                        string toAdd = loadedMolAndSelections[completionMolSel[choice]];
                        newS += toAdd;

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
    List<string> getMolAndSelNames() {
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        List<string> res = new List<string>();

        foreach (UnityMolStructure s in sm.loadedStructures) {
            res.Add(s.name);
        }
        foreach (string selname in selM.selections.Keys) {
            res.Add(selname);
        }

        return res;
    }
    List<string> getPossiblePath(string startp, int limit = 500) {
        if (startp.Length < 1)
            return null;
        if (startp.Contains("~"))//TODO fix that
            return null;
        List<string> res = new List<string>();
        string dirName = null;
        string absStartingpath = null;
        try {
            absStartingpath = Path.GetFullPath(startp);
            dirName = Path.GetDirectoryName(absStartingpath);
        }
        catch {}
        if (dirName != null) {
            try {
                string[] dirs = Directory.GetDirectories(dirName);
                for (int i = 0; i < dirs.Length; i++) {
                    res.Add(dirs[i].Replace("\\", "/"));
                    if (res.Count >= limit) {
                        return res;
                    }
                }
            }
            catch {}
            try {
                string[] files = Directory.GetFiles(dirName);
                for (int i = 0; i < files.Length; i++) {
                    res.Add(files[i].Replace("\\", "/"));
                    if (res.Count >= limit) {
                        return res;
                    }
                }
            }
            catch {}
        }

        return res;
    }

    void updateTextCompletionWithChoice(TMP_Text txt, List<int> complList, List<string> compleText) {
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
    public static List<int> AutoCompleteText(string input, List<string> source, int maxShownCount = 8, float levenshteinDistance = 0.5f)
    {
        List<int> result = new List<int>();
        List<int2> sortedRes = new List<int2>();
        HashSet<string> uniqueSrc = new HashSet<string>(source);

        if (!string.IsNullOrEmpty(input) && input.Length >= 2 && input.Length < 200) {

            string keywords = input;

            for (int i = 0; i < source.Count; i++) {
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
                    int id = source.IndexOf(s);
                    result.Add(id);
                    toRM.Add(s);
                }
            }
            if (result.Count == maxShownCount) {
                return result;
            }
            foreach (string s in toRM) {
                uniqueSrc.Remove(s);
            }


            int ids = 0;
            foreach (string s in uniqueSrc) {
                int2 r; r.x = ids;
                r.y = FuzzyMatch(s, keywords);
                sortedRes.Add(r);
                ids++;
            }

            sortedRes.Sort(delegate(int2 c1, int2 c2) { return c2.y.CompareTo(c1.y); });
            for (int i = 0; i < sortedRes.Count; i++) {
                if (result.Count == maxShownCount) {
                    break;
                }
                if (sortedRes[i].y > 1.5 * input.Length)
                    result.Add(sortedRes[i].x);
            }

            // levenshteinDistance = Mathf.Clamp01(levenshteinDistance);
            // foreach (string s in uniqueSrc) {
            //     if (result.Count == maxShownCount) {
            //         return result;
            //     }
            //     int distance = LevenshteinDistance(s, keywords);
            //     bool closeEnough = (int)(levenshteinDistance * s.Length) > distance;

            //     if (closeEnough) {
            //         result.Add(Array.IndexOf(source, s));
            //     }
            // }

        }

        return result;
    }


    //From https://gist.github.com/CDillinger/2aa02128f840bdca90340ce08ee71bc2
    /// <summary>
    /// Does a fuzzy search for a pattern within a string, and gives the search a score on how well it matched.
    /// </summary>
    /// <param name="stringToSearch">The string to search for the pattern in.</param>
    /// <param name="pattern">The pattern to search for in the string.</param>
    /// <returns>The score which this search received, if a match was found.</param>
    public static int FuzzyMatch(string stringToSearch, string pattern)
    {
        // Score consts
        const int adjacencyBonus = 5;               // bonus for adjacent matches
        const int separatorBonus = 10;              // bonus if match occurs after a separator
        const int camelBonus = 10;                  // bonus if match is uppercase and prev is lower

        const int leadingLetterPenalty = -3;        // penalty applied for every letter in stringToSearch before the first match
        const int maxLeadingLetterPenalty = -9;     // maximum penalty for leading letters
        const int unmatchedLetterPenalty = -1;      // penalty for every letter that doesn't matter


        // Loop variables
        var score = 0;
        var patternIdx = 0;
        var patternLength = pattern.Length;
        var strIdx = 0;
        var strLength = stringToSearch.Length;
        var prevMatched = false;
        var prevLower = false;
        var prevSeparator = true;                   // true if first letter match gets separator bonus

        // Use "best" matched letter if multiple string letters match the pattern
        char? bestLetter = null;
        char? bestLower = null;
        int? bestLetterIdx = null;
        var bestLetterScore = 0;

        var matchedIndices = new List<int>();

        // Loop over strings
        while (strIdx != strLength)
        {
            var patternChar = patternIdx != patternLength ? pattern[patternIdx] as char ? : null;
            var strChar = stringToSearch[strIdx];

            var patternLower = patternChar != null ? char.ToLower((char)patternChar) as char ? : null;
            var strLower = char.ToLower(strChar);
            var strUpper = char.ToUpper(strChar);

            var nextMatch = patternChar != null && patternLower == strLower;
            var rematch = bestLetter != null && bestLower == strLower;

            var advanced = nextMatch && bestLetter != null;
            var patternRepeat = bestLetter != null && patternChar != null && bestLower == patternLower;
            if (advanced || patternRepeat)
            {
                score += bestLetterScore;
                matchedIndices.Add((int)bestLetterIdx);
                bestLetter = null;
                bestLower = null;
                bestLetterIdx = null;
                bestLetterScore = 0;
            }

            if (nextMatch || rematch)
            {
                var newScore = 0;

                // Apply penalty for each letter before the first pattern match
                // Note: Math.Max because penalties are negative values. So max is smallest penalty.
                if (patternIdx == 0)
                {
                    var penalty = Math.Max(strIdx * leadingLetterPenalty, maxLeadingLetterPenalty);
                    score += penalty;
                }

                // Apply bonus for consecutive bonuses
                if (prevMatched)
                    newScore += adjacencyBonus;

                // Apply bonus for matches after a separator
                if (prevSeparator)
                    newScore += separatorBonus;

                // Apply bonus across camel case boundaries. Includes "clever" isLetter check.
                if (prevLower && strChar == strUpper && strLower != strUpper)
                    newScore += camelBonus;

                // Update pattern index IF the next pattern letter was matched
                if (nextMatch)
                    ++patternIdx;

                // Update best letter in stringToSearch which may be for a "next" letter or a "rematch"
                if (newScore >= bestLetterScore)
                {
                    // Apply penalty for now skipped letter
                    if (bestLetter != null)
                        score += unmatchedLetterPenalty;

                    bestLetter = strChar;
                    bestLower = char.ToLower((char)bestLetter);
                    bestLetterIdx = strIdx;
                    bestLetterScore = newScore;
                }

                prevMatched = true;
            }
            else
            {
                score += unmatchedLetterPenalty;
                prevMatched = false;
            }

            // Includes "clever" isLetter check.
            prevLower = strChar == strLower && strLower != strUpper;
            prevSeparator = strChar == '_' || strChar == ' ';

            ++strIdx;
        }

        // Apply score for last match
        if (bestLetter != null)
        {
            score += bestLetterScore;
            matchedIndices.Add((int)bestLetterIdx);
        }

        return score;
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
