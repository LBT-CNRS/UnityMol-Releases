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
using System.Collections.Generic;
using System.IO;


//From VMD
/*
 * corresponding table of VDW radii.
 * van der Waals radii are taken from A. Bondi,
 * J. Phys. Chem., 68, 441 - 452, 1964,
 * except the value for H, which is taken from R.S. Rowland & R. Taylor,
 * J.Phys.Chem., 100, 7384 - 7391, 1996. Radii that are not available in
 * either of these publications have RvdW = 2.00 Å.
 * The radii for Ions (Na, K, Cl, Ca, Mg, and Cs are based on the CHARMM27
 * Rmin/2 parameters for (SOD, POT, CLA, CAL, MG, CES) by default.
 */

namespace UMol {

/// <summary>
/// Stores the colors and the VDW radii for all atom types
/// Based on text files in StreamingAssets :
///   - defaultUnityMolColors.txt for regular atoms types
///   - defaultMartiniUnityMolColors.txt for Martini CG beads
///   - colorPalette.txt for residues colors and define palette for coloring by chain or residue number
///   - customUnityMolColors.txt for Users who want to overload the default values
/// </summary>
public class UnityMolDefaultColors {

    // Default colors if text files can't be parsed
    public Color oxygenColor     = new Color(0.827f, 0.294f, 0.333f, 1f);
    public Color carbonColor     = new Color(0.282f, 0.6f, 0.498f, 1f);
    public Color nitrogenColor   = new Color(0.443f, 0.662f, 0.882f, 1f);
    public Color hydrogenColor   = Color.white;
    public Color sulphurColor    = new Color(1f, 0.839f, 0.325f, 1f);
    public Color phosphorusColor = new Color(0.960f, 0.521f, 0.313f, 1f);
    public Color unknownColor    = new Color(1f, 0.4f, 1f, 1f);
    public Color ferrousColor    = new Color(0.875f, 0.398f, 0.199f, 1f);

    public static Color32 orange = new Color(1.0f, 0.5f, 0.31f, 1.0f);
    public static Color32 lightyellow = new Color(1.0f, 240 / 255.0f, 140 / 255.0f, 1.0f);

    /// <summary>
    /// Dictionary holding the color for each atom type
    /// </summary>
    public Dictionary<string, Color32> colorByAtom;

    /// <summary>
    /// Dictionary holding the VDW radi for each atom type
    /// </summary>
    public Dictionary<string, float> radiusByAtom;

    /// <summary>
    /// List of colors defining a palette
    /// Useful when coloring a molecule by chain or residue number
    /// </summary>
    private List<Color32> colorPalette;

    /// <summary>
    /// Dictionary holding the color for each residue
    /// </summary>
    public Dictionary<string, Color32> colorByResidue;

    /// <summary>
    /// Dictionary holding the color for each residue type:
    /// acid = red, basic = blue, nonpolar = yellow, polar, green, cysteine = orange
    /// Based on https://pymolwiki.org/index.php/Resicolor
    /// </summary>
    public Dictionary<string, Color32> colorRestypeByResidue = new Dictionary<string, Color32> {
        {"ALA", lightyellow},
        {"ARG", Color.blue},
        {"ASN", Color.green},
        {"ASP", Color.red},
        {"CYS", orange},
        {"GLN", Color.green},
        {"GLU", Color.red},
        {"GLY", lightyellow},
        {"HIS", Color.green},
        {"ILE", lightyellow},
        {"LEU", lightyellow},
        {"LYS", Color.blue},
        {"MET", lightyellow},
        {"PHE", lightyellow},
        {"PRO", lightyellow},
        {"SEC", Color.green},
        {"SER", Color.green},
        {"THR", Color.green},
        {"TRP", lightyellow},
        {"TYR", Color.green},
        {"VAL", lightyellow}
    };

    /// <summary>
    /// Dictionary holding the color for each residue charge type:
    /// Negative residues in red, blue for positively charged residues, others in white
    /// </summary>
    public Dictionary<string, Color> colorReschargeByResidue = new Dictionary<string, Color> {
        {"ALA", Color.white},
        {"ARG", Color.blue},
        {"ASN", Color.white},
        {"ASP", Color.red},
        {"CYS", Color.white},
        {"GLN", Color.white},
        {"GLU", Color.red},
        {"GLY", Color.white},
        {"HIS", Color.blue},
        {"ILE", Color.white},
        {"LEU", Color.white},
        {"LYS", Color.blue},
        {"MET", Color.white},
        {"PHE", Color.white},
        {"PRO", Color.white},
        {"SEC", Color.white},
        {"SER", Color.white},
        {"THR", Color.white},
        {"TRP", Color.white},
        {"TYR", Color.white},
        {"VAL", Color.white}
    };


    /// <summary>
    /// Array of textures available
    /// </summary>
    public Object[] textures;

    /// <summary>
    /// Constructor of the class. Takes in arguments the different text files defining the colors
    /// </summary>
    /// <param name="pathColorR">path to the default UnityMol colors.
    /// default will be StreaminAssets/defaultUnityMolColors.txt</param>
    /// <param name="pathPalette">path to the default color palette.
    /// default will be StreaminAssets/colorPalette.txt</param>
    /// <param name="pathMartiniDefault">path to the default Martini UnityMol colors.
    /// default will be StreaminAssets/defaultMartiniUnityMolColors.txt</param>
    /// <param name="pathCustom">path to the custom UnityMol colors.
    /// default will be StreaminAssets/customUnityMolColors.txt</param>
    public UnityMolDefaultColors(string pathColorR = null, string pathPalette = null, string pathMartiniDefault = null, string pathCustom = null) {
        if (pathColorR == null) {
            pathColorR = Path.Combine(Application.streamingAssetsPath , "defaultUnityMolColors.txt");
        }
        if (pathPalette == null) {
            pathPalette = Path.Combine(Application.streamingAssetsPath , "colorPalette.txt");
        }
        if (pathMartiniDefault == null) {
            pathMartiniDefault = Path.Combine(Application.streamingAssetsPath , "defaultMartiniUnityMolColors.txt");
        }
        if (pathCustom == null) {
            pathCustom = Path.Combine(Application.streamingAssetsPath , "customUnityMolColors.txt");
        }

        colorByAtom = new Dictionary<string, Color32>();
        radiusByAtom = new Dictionary<string, float>();
        colorByResidue = new Dictionary<string, Color32>();
        colorPalette = new List<Color32>();

        parseColorRadiusByAtom(pathColorR);
        parseColorRadiusByAtom(pathCustom);
        parseColorRadiusByAtomMartini(pathMartiniDefault);
        parseColorPaletteAndResidue(pathPalette);

        // Don't load textures for Android
        if (Application.platform != RuntimePlatform.Android) {
            textures = Resources.LoadAll("Images/MatCap", typeof(Texture2D));
        }
    }

    /// <summary>
    /// Parse a color file to fill the color and VDW radi values for regular atom types.
    /// </summary>
    /// <param name="colorFilePath">the path to the color file</param>
    private void parseColorRadiusByAtom(string colorFilePath) {
        //Set default color even if the parsing fails
        colorByAtom["C"] = carbonColor;
        colorByAtom["O"] = oxygenColor;
        colorByAtom["N"] = nitrogenColor;
        colorByAtom["H"] = hydrogenColor;
        colorByAtom["S"] = sulphurColor;
        colorByAtom["P"] = phosphorusColor;
        colorByAtom["FE"] = ferrousColor;


        radiusByAtom["C"] =  1.70f;
        radiusByAtom["O"] =  1.52f;
        radiusByAtom["N"] =  1.55f;
        radiusByAtom["H"] =  1.20f;
        radiusByAtom["S"] =  1.80f;
        radiusByAtom["P"] =  1.80f;
        radiusByAtom["FE"] = 1.56f;

        StreamReader sr = GetStreamReaderFromPath(colorFilePath);

        using(sr) {
            string line;
            int cptColorParsed = 0;
            while ((line = sr.ReadLine()) != null) {
                if (line.StartsWith("#") || line.Trim().Length < 3) {
                    continue;
                }
                try {
                    string[] splits = line.Split(new [] { '\t', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    string atomType = splits[0].ToUpper();
                    Color atomCol = Color.white;
                    ColorUtility.TryParseHtmlString(splits[1], out atomCol);
                    float radius = float.Parse(splits[2], System.Globalization.CultureInfo.InvariantCulture);
                    colorByAtom[atomType] = (Color32) atomCol;
                    radiusByAtom[atomType] = radius;
                    cptColorParsed++;
                }
                catch {
                    Debug.LogWarning("Ignoring color/atom line " + line);
                }
            }
        }
    }

    /// <summary>
    /// Parse a color file to fill the color and VDW radi values for Martini CG beads.
    /// </summary>
    /// <param name="colorFilePath">the path to the color file</param>
    private void parseColorRadiusByAtomMartini(string colorFilePath) {

        StreamReader sr = GetStreamReaderFromPath(colorFilePath);

        using (sr) {
            string s;
            int cptline = 0;
            while ((s = sr.ReadLine()) != null) {

                cptline++;
                if (s.Length > 1 && !s.StartsWith("#")) {
                    try {

                        string[] fields = s.Split(new [] {' '});
                        if (fields.Length != 6)
                            continue;

                        Color curCol = new Color(float.Parse(fields[2], System.Globalization.CultureInfo.InvariantCulture),
                                                 float.Parse(fields[3], System.Globalization.CultureInfo.InvariantCulture),
                                                 float.Parse(fields[4], System.Globalization.CultureInfo.InvariantCulture), 1.0f);

                        string resCGname = fields[0].Trim();
                        string CGname = fields[1].Trim();
                        float radius = float.Parse(fields[5], System.Globalization.CultureInfo.InvariantCulture);

                        string atomType = "MARTINI_" + resCGname + "_" + CGname;
                        colorByAtom[atomType.ToUpper()] = curCol;
                        radiusByAtom[atomType.ToUpper()] = radius;

                    }
                    catch  {
                        Debug.LogWarning("Ignoring line " + cptline);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Parse a color file to fill the residue colors and the palette.
    /// </summary>
    /// <param name="path">the path to the color file</param>
    private void parseColorPaletteAndResidue(string path) {

        StreamReader sr = GetStreamReaderFromPath(path);
        colorPalette.Clear();

        using(sr) {
            string line;
            // int cptColorParsed = 0;
            while ((line = sr.ReadLine()) != null) {
                if (line.StartsWith("#") || line.Trim().Length < 3) {
                    continue;
                }
                if (line.StartsWith("-")) {
                    try {
                        string[] splits = line.Split(new [] { '\t', ' ', '-' }, System.StringSplitOptions.RemoveEmptyEntries);
                        Color newCol = Color.white;
                        ColorUtility.TryParseHtmlString(splits[0], out newCol);
                        colorPalette.Add(newCol);
                    }
                    catch {
                        Debug.LogWarning("Ignoring line " + line);
                    }
                }
                else {

                    try {
                        string[] splits = line.Split(new [] { '\t', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        string resType = splits[0].ToUpper();
                        Color resCol = Color.white;
                        ColorUtility.TryParseHtmlString(splits[1], out resCol);
                        colorByResidue[resType] = resCol;
                    }
                    catch {
                        Debug.LogWarning("Ignoring line " + line);
                    }
                }
            }
        }

    }

    /// <summary>
    /// Fill the parameter 'color' and 'radius' based on the atom type given in 'atomType'
    /// Except the parameter 'atomType' to be in uppercase
    /// </summary>
    /// <param name="atomType">the atom type</param>
    /// <param name="color">the color found (output)</param>
    /// <param name="radius">the VDW radius found (output)</param>
    public void getColorAtom(string atomType, out Color32 color, out float radius) {
        color = (Color32) unknownColor;
        radius = 1.0f;
        bool found = false;
        if (colorByAtom != null && colorByAtom.TryGetValue(atomType, out color)) {
            found = true;
        }
        if (radiusByAtom != null && radiusByAtom.TryGetValue(atomType, out radius)) {
            found = true;
        }
        if (!found) {
            color = (Color32) unknownColor;
            radius = 2.0f;
        }

    }

    /// <summary>
    /// Return true if the atom type given is found in the 'radiusByAtom' dictionary
    /// </summary>
    /// <param name="atomType">the atom type</param>
    /// <returns>True if found. False otherwise</returns>
    public bool isKnownAtom(string atomType) {
        return radiusByAtom.ContainsKey(atomType);
    }

    /// <summary>
    /// Return the maximal VDW radius among the atoms list
    /// </summary>
    /// <param name="atoms">the atom list to search</param>
    /// <returns>the maximum VDW radius</returns>
    public float getMaxRadius(List<UnityMolAtom> atoms) {
        if (radiusByAtom == null) {
            return -1.0f;
        }
        float maxVDW = 0.0f;

        foreach (UnityMolAtom a in atoms) {
            if (radiusByAtom.ContainsKey(a.type)) {
                maxVDW = Mathf.Max(maxVDW, radiusByAtom[a.type]);
            }
        }
        return maxVDW;
    }

    /// <summary>
    /// Return the ith Color among the color palette.
    /// Always return a Color:
    ///    - if i negative, return the first color
    ///    - if i greater than the list, return a modulo
    ///    - if the list is empty, return a random color.
    /// </summary>
    /// <param name="i">the index</param>
    /// <returns>a Color</returns>
    public Color getColorFromPalette(int i) {
        if (colorPalette.Count == 0) {
            return getRandomColor();
        }
        if (i < 0)
            i = 0;

        if (i >= colorPalette.Count) {
            i = i % colorPalette.Count;
        }
        return colorPalette[i];
    }

    /// <summary>
    /// Return the color of a residue
    /// If the residue is not found in the dictionary colorByResidue,
    /// a new random color will be created and store along the residue name.
    /// </summary>
    /// <param name="res">the UnityMolResidue</param>
    /// <returns>the color of the residue </returns>
    public Color getColorForResidue(UnityMolResidue res) {
        if (colorByResidue.ContainsKey(res.name)) {
            return colorByResidue[res.name];
        }
        Color newCol = getRandomColor();
        colorByResidue[res.name] = newCol;
        return newCol;
    }

    /// <summary>
    /// Return the color based on the type of a residue
    /// If the residue is not found in the dictionary colorRestypeByResidue,
    /// a new random color will be created and store along the residue name.
    /// </summary>
    /// <param name="res">the UnityMolResidue</param>
    /// <returns>the color of the residue </returns>
    public Color getColorRestypeForResidue(UnityMolResidue res) {
        if (colorRestypeByResidue.ContainsKey(res.name)) {
            return colorRestypeByResidue[res.name];
        }
        Color newCol = getRandomColor();
        colorRestypeByResidue[res.name] = newCol;
        return newCol;
    }

    /// <summary>
    /// Return the color based on the charge type of a residue
    /// acid = red, basic = blue, nonpolar = yellow, polar, green, cysteine = orange
    /// If the residue is not found in the dictionary colorRestypeByResidue,
    /// a new color set to black will be created and store along the residue name.
    /// </summary>
    /// <param name="res">the UnityMolResidue</param>
    /// <returns>the color of the residue </returns>
    public Color getColorReschargeForResidue(UnityMolResidue res) {
        if (colorReschargeByResidue.ContainsKey(res.name)) {
            return colorReschargeByResidue[res.name];
        }
        Color newCol = Color.black;
        colorReschargeByResidue[res.name] = newCol;
        return newCol;
    }


    /// <summary>
    /// Return a random color with Hue based on the golden ratio.
    /// </summary>
    /// <returns>the random color</returns>
    private Color getRandomColor() {
        const float goldenRatio = 0.618033988749895f;
        Color rndCol = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

        float H, S, V;
        Color.RGBToHSV(rndCol, out H, out S, out V);
        H = (H + goldenRatio) % 1.0f;
        return Color.HSVToRGB(H, S, V);
    }

    /// <summary>
    /// Get a StreamReader object from a path file.
    /// Handle Android case.
    /// </summary>
    /// <param name="path">the path to the file</param>
    /// <returns>the StreamReader of the file</returns>
    private StreamReader GetStreamReaderFromPath(string path)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            var textStream = new StringReaderStream(AndroidUtils.GetFileText(path));
            return new StreamReader(textStream);
        }
        else
        {
            FileInfo LocalFile = new FileInfo(path);
            if (!LocalFile.Exists) {
                return null;
            }
            return new StreamReader(path);
        }
    }
}
}
