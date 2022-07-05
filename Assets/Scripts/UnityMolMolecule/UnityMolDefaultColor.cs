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

public class UnityMolDefaultColors {

    public Color oxygenColor     = new Color(0.827f, 0.294f, 0.333f, 1f);
    public Color carbonColor     = new Color(0.282f, 0.6f, 0.498f, 1f);
    public Color nitrogenColor   = new Color(0.443f, 0.662f, 0.882f, 1f);
    public Color hydrogenColor   = Color.white;
    public Color sulphurColor    = new Color(1f, 0.839f, 0.325f, 1f);
    public Color phosphorusColor = new Color(0.960f, 0.521f, 0.313f, 1f);
    public Color unknownColor    = new Color(1f, 0.4f, 1f, 1f);
    public Color ferrousColor    = new Color(0.875f, 0.398f, 0.199f, 1f);

    public static Color orange = new Color(1.0f, 0.5f, 0.31f, 1.0f);
    public static Color lightyellow = new Color(1.0f, 240 / 255.0f, 140 / 255.0f, 1.0f);

    public Dictionary<string, Color> colorByAtom;
    public Dictionary<string, float> radiusByAtom;

    private List<Color> colorPalette = new List<Color>();
    public Dictionary<string, Color> colorByResidue = new Dictionary<string, Color>();

    /// Based on https://pymolwiki.org/index.php/Resicolor
    public Dictionary<string, Color> colorRestypeByResidue = new Dictionary<string, Color>  {
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
    /// Negative residues in red, blue for positively charged residues, others in white
    public Dictionary<string, Color> colorReschargeByResidue = new Dictionary<string, Color>  {
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


    public Object[] textures;

    public UnityMolDefaultColors(string pathColorR = null, string pathPalette = null) {
        if (pathColorR == null) {
            pathColorR = Path.Combine(Application.streamingAssetsPath , "defaultUnityMolColors.txt");
        }
        if (pathPalette == null) {
            pathPalette = Path.Combine(Application.streamingAssetsPath , "colorPalette.txt");
        }
        parseColorRadiusByAtom(pathColorR);
        parseColorPaletteAndResidue(pathPalette);
        if (Application.platform != RuntimePlatform.Android) {
            textures = Resources.LoadAll("Images/MatCap", typeof(Texture2D));
        }
    }

    public void parseColorRadiusByAtom(string colorFilePath) {
        if (colorByAtom == null) {
            colorByAtom = new Dictionary<string, Color>();
        }
        if (radiusByAtom == null) {
            radiusByAtom = new Dictionary<string, float>();
        }
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

        StreamReader sr;

        if (Application.platform == RuntimePlatform.Android)
        {
            var textStream = new StringReaderStream(AndroidUtils.GetFileText(colorFilePath));
            sr = new StreamReader(textStream);
        }
        else
        {

            FileInfo LocalFile = new FileInfo(colorFilePath);
            if (!LocalFile.Exists) {
                Debug.LogWarning("File not found: " + colorFilePath);
                return;
            }
            sr = new StreamReader(colorFilePath);
        }

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
                    colorByAtom[atomType] = atomCol;
                    radiusByAtom[atomType] = radius;
                    cptColorParsed++;
                }
                catch {
                    Debug.LogWarning("Ignoring color/atom line " + line);
                }
            }
        }
    }

    public void getColorAtom(string atomType, out Color color, out float radius) {
        color = unknownColor;
        radius = 1.0f;
        bool found = false;
        if (colorByAtom != null && colorByAtom.TryGetValue(atomType.ToUpper(), out color)) {
            found = true;
        }
        if (radiusByAtom != null && radiusByAtom.TryGetValue(atomType.ToUpper(), out radius)) {
            found = true;
        }
        if (!found) {
            color = unknownColor;
            radius = 2.0f;
#if UNITY_EDITOR
            // Debug.LogWarning("Unknown atom " + atomType);
#endif
        }

    }
    public bool isKnownAtom(string atomType) {
        return radiusByAtom.ContainsKey(atomType);
    }

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

    void parseColorPaletteAndResidue(string path) {
        StreamReader sr;

        if (Application.platform == RuntimePlatform.Android)
        {
            var textStream = new StringReaderStream(AndroidUtils.GetFileText(path));
            sr = new StreamReader(textStream);
        }
        else
        {
            FileInfo LocalFile = new FileInfo(path);
            if (!LocalFile.Exists)
            {
                Debug.LogWarning("File not found: " + path);
                return;
            }
            sr = new StreamReader(path);
        }
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
        // Random.State oldState = Random.state;

        // //Initialize the seed for random color to have the same color for residues all the time
        // Random.InitState(123);

        // float pos = 0.0f;
        // foreach(string resName in UnityMolMain.topologies.bondedAtomsPerResidue.Keys){
        //     colorByResidue[resName] = getRandomColor();
        // }

        // Random.state = oldState;
    }

    public Color getColorFromPalette(int id) {
        if (colorPalette.Count == 0) {
            return getRandomColor();
        }
        if (id < 0)
            id = 0;

        if (id >= colorPalette.Count) {
            id = id % colorPalette.Count;
        }
        return colorPalette[id];
    }

    public Color getColorForResidue(UnityMolResidue res) {
        if (colorByResidue.ContainsKey(res.name)) {
            return colorByResidue[res.name];
        }
        Color newCol = getRandomColor();
        colorByResidue[res.name] = newCol;
        return newCol;
    }

    public Color getColorRestypeForResidue(UnityMolResidue res) {
        if (colorRestypeByResidue.ContainsKey(res.name)) {
            return colorRestypeByResidue[res.name];
        }
        Color newCol = getRandomColor();
        colorRestypeByResidue[res.name] = newCol;
        return newCol;
    }
    public Color getColorReschargeForResidue(UnityMolResidue res) {
        if (colorReschargeByResidue.ContainsKey(res.name)) {
            return colorReschargeByResidue[res.name];
        }
        Color newCol = Color.black;
        colorReschargeByResidue[res.name] = newCol;
        return newCol;
    }


    private Color getRandomColor() {
        const float goldenRatio = 0.618033988749895f;
        Color rndCol = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

        float H, S, V;
        Color.RGBToHSV(rndCol, out H, out S, out V);
        H = (H + goldenRatio) % 1.0f;
        return Color.HSVToRGB(H, S, V);
    }
}
}