/// @file ScreenShot.cs
/// @brief Details to be specified
/// @author FvNano/LBT team
/// @author Marc Baaden <baaden@smplinux.de>
/// @date   2013-4
///
/// Copyright Centre National de la Recherche Scientifique (CNRS)
///
/// contributors :
/// FvNano/LBT team, 2010-13
/// Marc Baaden, 2010-13
///
/// baaden@smplinux.de
/// http://www.baaden.ibpc.fr
///
/// This software is a computer program based on the Unity3D game engine.
/// It is part of UnityMol, a general framework whose purpose is to provide
/// a prototype for developing molecular graphics and scientific
/// visualisation applications. More details about UnityMol are provided at
/// the following URL: "http://unitymol.sourceforge.net". Parts of this
/// source code are heavily inspired from the advice provided on the Unity3D
/// forums and the Internet.
///
/// This software is governed by the CeCILL-C license under French law and
/// abiding by the rules of distribution of free software. You can use,
/// modify and/or redistribute the software under the terms of the CeCILL-C
/// license as circulated by CEA, CNRS and INRIA at the following URL:
/// "http://www.cecill.info".
/// 
/// As a counterpart to the access to the source code and rights to copy, 
/// modify and redistribute granted by the license, users are provided only 
/// with a limited warranty and the software's author, the holder of the 
/// economic rights, and the successive licensors have only limited 
/// liability.
///
/// In this respect, the user's attention is drawn to the risks associated 
/// with loading, using, modifying and/or developing or reproducing the 
/// software by the user in light of its specific status of free software, 
/// that may mean that it is complicated to manipulate, and that also 
/// therefore means that it is reserved for developers and experienced 
/// professionals having in-depth computer knowledge. Users are therefore 
/// encouraged to load and test the software's suitability as regards their 
/// requirements in conditions enabling the security of their systems and/or 
/// data to be ensured and, more generally, to use and operate it in the 
/// same conditions as regards security.
///
/// The fact that you are presently reading this means that you have had 
/// knowledge of the CeCILL-C license and that you accept its terms.
///
/// $Id: ScreenShot.cs 213 2013-04-06 21:13:42Z baaden $
///
/// References : 
/// If you use this code, please cite the following reference : 	
/// Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
/// "Game on, Science - how video game technology may help biologists tackle
/// visualization challenges" (2013), PLoS ONE 8(3):e57990.
/// doi:10.1371/journal.pone.0057990
///
/// If you use the HyperBalls visualization metaphor, please also cite the
/// following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
/// B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
/// using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
/// J. Comput. Chem., 2011, 32, 2924
///

// ******  Notice : It doesn't works in Wep Player environment.  ******
// ******    It works in PC environment.                         ******
// Default method have some problem, when you take a Screen shot for your game. 
// So add this script.
// CF Page : http://technology.blurst.com/unity-jpg-encoding-javascript/
// made by Jerry ( sdragoon@nate.com )

using UnityEngine;
using System.Collections;
using System.IO;

public class ScreenShot : MonoBehaviour
{
    private int count = 0;
    public bool open=false;
    public bool sequence=false;
    //public string directorypath="";
	//public int frameRate= 25;

    void Update()
    {
        if (open)
            StartCoroutine(ScreenshotEncode());
            if(!sequence)open=false;
    }

    public IEnumerator ScreenshotEncode()
    {
        #if !UNITY_WEBPLAYER
            // wait for graphics to render
            yield return new WaitForEndOfFrame();

            // create a texture to pass to encoding
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);

            // put buffer into texture
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            // split the process up--ReadPixels() and the GetPixels() call inside of the encoder are both pretty heavy
            yield return 0;

            byte[] bytes = texture.EncodeToPNG();

            // save our test image (could also upload to WWW)
    	    if(UI.GUIDisplay.directorypath=="/Users/lvzhihan/temp/pdb/")
    	    	File.WriteAllBytes(Application.dataPath + "/../testscreen-" + count + ".png", bytes);
    	    else
            	File.WriteAllBytes(UI.GUIDisplay.directorypath + "/testscreen-" + count + ".png", bytes);

            count++;

            //Debug.Log( Application.dataPath + "/../testscreen-" + count + ".png" );
        #else
            yield return new WaitForEndOfFrame();
        #endif
    }
}
