/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Sebastien Doutreligne, 2017
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


// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine.UI;

// namespace UMol{
// public class ArtemisClientLoop : MonoBehaviour {

//     private static bool toggleTotalEnergyPlot = false;
//     private static bool toggleHBondsEnergyPlot = false;
//     private static bool toggleStackingEnergyPlot = false;
//     private static bool toggleSAXSCurvePlot = false;

//     public static Texture2D totalEnergyPlotTexture;
//     public static Texture2D hbondsEnergyPlotTexture;
//     public static Texture2D stackingEnergyPlotTexture;
//     public static Texture2D saxsCurvePlotTexture;
//     public static Texture2D saxsEmptyGrid;

//     public static GameObject totalEnergyGameObject;
//     public static GameObject hbondsEnergyGameObject;
//     public static GameObject stackingEnergyGameObject;
//     public static GameObject saxsCurveGameObject;

//     // public static SAXSParticlePlotPanel saxsPanel;

//     public static Text totalEnergyValue;
//     public static Text hbondsEnergyValue;
//     public static Text stackingEnergyValue;

//     private static GameObject instantiatePlot(string name, Texture2D plot_texture, int min, int max, Color c) {
//         Debug.Assert(min < max);

//         PlotManager.Instance.PlotCreate(name, min, max, c, ref plot_texture);

//         GameObject go;

//         go = GameObject.Instantiate(Resources.Load("Plotter/PlotPanel")) as GameObject;
//         go.transform.SetParent(GameObject.Find("Temporary Canvas").transform);
//         go.transform.position = new Vector2(300, 200);
//         Transform image = go.transform.Find("Image");
//         image.GetComponent<RawImage>().texture = plot_texture;

//         Transform name_label = go.transform.Find("Name");
//         name_label.GetComponent<Text>().text = name;

//         Transform min_label = go.transform.Find("Min");
//         min_label.GetComponent<Text>().text = min.ToString("0.00");

//         Transform max_label = go.transform.Find("Max");
//         max_label.GetComponent<Text>().text = max.ToString("0.00");

//         return go;
//     }

//     public static void instantiateSAXSPlot()
//     {
//         saxsCurveGameObject = GameObject.Instantiate(Resources.Load("Plotter/SAXSPlotPanel")) as GameObject;
//         saxsCurveGameObject.transform.SetParent(GameObject.Find("Temporary Canvas").transform);
//         saxsCurveGameObject.SetActive(true);
//         saxsCurveGameObject.transform.position = new Vector2(300f, 200f);
//         saxsCurveGameObject.GetComponent<RectTransform>().localScale = Vector3.one;
//         saxsPanel = saxsCurveGameObject.GetComponent<SAXSParticlePlotPanel>();

//         toggleSAXSCurvePlot = true;
//     }

//     public static void activateSAXSPlot()
//     {
//         toggleSAXSCurvePlot = true;
//         saxsCurveGameObject.SetActive(true);
//     }

//     public static void deactivateSAXSPlot()
//     {
//         saxsCurveGameObject.SetActive(false);
//         toggleSAXSCurvePlot = false;
//     }

//     public static void destroySAXSPlot()
//     {
//         GameObject.Destroy(saxsCurveGameObject);

//         toggleSAXSCurvePlot = false;
//         LoadTypeGUI.toggleIMDSAXS = false;
//     }

//     public static void instanciateTotalEnergyPlot()
//     {
//         totalEnergyGameObject = instantiatePlot("Total Energy (kcal/mol)", totalEnergyPlotTexture, -200, 300, GUIDisplay.artemisPlotWhiteStyle.normal.textColor);

//         totalEnergyValue = totalEnergyGameObject.transform.Find("Value").GetComponent<Text>();

//         toggleTotalEnergyPlot = true;
//     }

//     public static void activateTotalEnergyPlot()
//     {
//         toggleTotalEnergyPlot = true;
//         totalEnergyGameObject.SetActive(true);
//     }

//     public static void deactivateTotalEnergyPlot()
//     {
//         totalEnergyGameObject.SetActive(false);

//         toggleTotalEnergyPlot = false;
//     }

//     public static void destroyTotalEnergyPlot()
//     {
//         PlotManager.Instance.PlotDestroy("Total Energy (kcal/mol)");

//         GameObject.Destroy(totalEnergyGameObject);

//         toggleTotalEnergyPlot = false;
//         LoadTypeGUI.toggleIMDTotalEnergy = false;
//     }

//     public static void instanciateHBondsEnergyPlot()
//     {
//         hbondsEnergyGameObject = instantiatePlot("Hydrogen Bonds Energy (kcal/mol)", hbondsEnergyPlotTexture, -100, 10, GUIDisplay.artemisPlotGreenStyle.normal.textColor);

//         hbondsEnergyValue = hbondsEnergyGameObject.transform.Find("Value").GetComponent<Text>();

//         toggleHBondsEnergyPlot = true;
//     }

//     public static void activateHBondsEnergyPlot()
//     {
//         toggleHBondsEnergyPlot = true;
//         hbondsEnergyGameObject.SetActive(true);
//     }

//     public static void deactivateHBondsEnergyPlot()
//     {
//         hbondsEnergyGameObject.SetActive(false);

//         toggleHBondsEnergyPlot = false;
//     }

//     public static void destroyHBondsEnergyPlot()
//     {
//         PlotManager.Instance.PlotDestroy("Hydrogen Bonds Energy (kcal/mol)");

//         GameObject.Destroy(hbondsEnergyGameObject);

//         toggleHBondsEnergyPlot = false;
//         LoadTypeGUI.toggleIMDHBondsEnergy = false;
//     }

//     public static void instantiateStackingEnergyPlot()
//     {
//         stackingEnergyGameObject = instantiatePlot("Stacking Energy (kcal/mol)", stackingEnergyPlotTexture, -100, 10, GUIDisplay.artemisPlotRedStyle.normal.textColor);

//         stackingEnergyValue = stackingEnergyGameObject.transform.Find("Value").GetComponent<Text>();

//         toggleStackingEnergyPlot = true;
//     }

//     public static void activateStackingEnergyPlot()
//     {
//         toggleStackingEnergyPlot = true;
//         stackingEnergyGameObject.SetActive(true);
//     }

//     public static void deactivateStackingEnergyPlot()
//     {
//         stackingEnergyGameObject.SetActive(false);

//         toggleStackingEnergyPlot = false;
//     }

//     public static void destroyStackingEnergyPlot()
//     {
//         PlotManager.Instance.PlotDestroy("Stacking Energy (kcal/mol)");

//         GameObject.Destroy(stackingEnergyGameObject);

//         toggleStackingEnergyPlot = false;
//         LoadTypeGUI.toggleIMDStackingEnergy = false;
//     }

//     void OnDestroy() {
//         destroyTotalEnergyPlot();
//         destroyStackingEnergyPlot();
//         destroyHBondsEnergyPlot();
//         destroySAXSPlot();
//     }

//     void Start () {
//         instanciateTotalEnergyPlot();
//         instanciateHBondsEnergyPlot();
//         instantiateStackingEnergyPlot();
//         instantiateSAXSPlot();
//     }

//     void Update () {

//         if (LoadTypeGUI.toggleIMDTotalEnergy && !toggleTotalEnergyPlot)
//         {
//             activateTotalEnergyPlot();
//         }
//         if (!LoadTypeGUI.toggleIMDTotalEnergy && toggleTotalEnergyPlot)
//         {
//             deactivateTotalEnergyPlot();
//         }

//         if (LoadTypeGUI.toggleIMDHBondsEnergy && !toggleHBondsEnergyPlot)
//         {
//             activateHBondsEnergyPlot();
//         }
//         if (!LoadTypeGUI.toggleIMDHBondsEnergy && toggleHBondsEnergyPlot)
//         {
//             deactivateHBondsEnergyPlot();
//         }

//         if (LoadTypeGUI.toggleIMDStackingEnergy && !toggleStackingEnergyPlot)
//         {
//             activateStackingEnergyPlot();
//         }
//         if (!LoadTypeGUI.toggleIMDStackingEnergy && toggleStackingEnergyPlot)
//         {
//             deactivateStackingEnergyPlot();
//         }

//         if (LoadTypeGUI.toggleIMDSAXS && !toggleSAXSCurvePlot)
//         {
//             activateSAXSPlot();
//         }
//         if (!LoadTypeGUI.toggleIMDSAXS && toggleSAXSCurvePlot)
//         {
//             deactivateSAXSPlot();
//         }

//         ArtemisClientLoop.totalEnergyValue.text = PlotManager.Instance.getLatestValue("Total Energy (kcal/mol)").ToString("0.00");
//         ArtemisClientLoop.hbondsEnergyValue.text = PlotManager.Instance.getLatestValue("Hydrogen Bonds Energy (kcal/mol)").ToString("0.00");
//         ArtemisClientLoop.stackingEnergyValue.text = PlotManager.Instance.getLatestValue("Stacking Energy (kcal/mol)").ToString("0.00");

//     }
// }