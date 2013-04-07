/// @file SplashScreen.cs
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
/// $Id: SplashScreen.cs 225 2013-04-07 14:21:34Z baaden $
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

using UnityEngine;
using System.Collections;

//
// SplashScreen Script
//
// Version 0.1 by Martijn Dekker
// martijn.pixelstudio@gmail.com
//
// Version 0.2 by Ferdinand Joseph Fernandez, 2010Sep7 16:45 GMT + 8
// Changes:
//  * changed levelToLoad to a string, for easier usage
//  * added waitTime, which adds a pause after fade in, and before fade
//    out (during fade waiting)
//  * added option to either automatically fade out after waitTime
//    seconds (default), or wait for user input (press any key to continue)
//  * added option to wait until fade out is complete before loading next
//    level, instead of the default, which is to load the next level
//    before fade out
//
// Version 0.3 by Ferdinand Joseph Fernandez, 2010Sep8 01:13 GMT + 8
// Changes:
//  * splash screen itself is now fading without the need for a solid
//    background color
//  * optimized some code
//
// Version 0.4 by Ferdinand Joseph Fernandez, 2010Sep14 14:09 GMT + 8
// Changes:
//  * splash screen picture can now be either centered (default) or
//    stretched on the screen
//
// Version 0.5 by Ferdinand Joseph Fernandez, 2010Sep15 18:27 GMT + 8
// Changes:
//  * now has option to start automatically or not. if not started
//    automatically, the splash screen can be started by calling
//    the StartSplash function
//  * code acknowledges if the levelToLoad is blank, in that case,
//    the code simply does not attempt to load a level
//
// Version 0.6 by Ferdinand Joseph Fernandez, 2010Sep29 13:43 GMT + 8
// Changes:
//  * added the property "gui depth" so you can control at which depth the
//    splash screen shows in
//

public class SplashScreen : MonoBehaviour
{
    public int guiDepth = 0;
    public string levelToLoad = ""; // this has to correspond to a level (file>build settings)
    public Texture2D splashLogo; // the logo to splash;
    public float fadeSpeed = 0.3f;
    public float waitTime = 0.5f; // seconds to wait before fading out
    public bool waitForInput = false; // if true, this acts as a "press any key to continue"
    public bool startAutomatically = true;
//    private float timeFadingInFinished = 0.0f;

    public enum SplashType
    {
        LoadNextLevelThenFadeOut,
        FadeOutThenLoadNextLevel
    }
    public SplashType splashType;

//    private float alpha = 0.0f;

    private enum FadeStatus
    {
        Paused,
        FadeIn,
        FadeWaiting,
        FadeOut
    }
//    private FadeStatus status = FadeStatus.FadeIn;

    private Camera oldCam;
//    private GameObject oldCamGO;

    private Rect splashLogoPos = new Rect();
    public enum LogoPositioning
    {
        Centered,
        Stretched
    }
    public LogoPositioning logoPositioning;

    private bool loadingNextLevel = false;

    void Start()
    {
//        if (startAutomatically)
//        {
//            status = FadeStatus.FadeIn;
//        }
//        else
//        {
//            status = FadeStatus.Paused;
//        }
        oldCam = Camera.main;
//        oldCamGO = Camera.main.gameObject;

        if (logoPositioning == LogoPositioning.Centered)
        {
            splashLogoPos.x = (Screen.width * 0.5f) - (splashLogo.width * 0.5f);
            splashLogoPos.y = (Screen.height * 0.5f) - (splashLogo.height * 0.5f);

            splashLogoPos.width = splashLogo.width;
            splashLogoPos.height = splashLogo.height;
        }
        else
        {
            splashLogoPos.x = 0;
            splashLogoPos.y = 0;

            splashLogoPos.width = Screen.width;
            splashLogoPos.height = Screen.height;
        }



        if (splashType == SplashType.LoadNextLevelThenFadeOut)
        {
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(Camera.main);
        }
        if ((Application.levelCount <= 1) || (levelToLoad == ""))
        {
            //Debug.LogWarning("Invalid levelToLoad value.");
        }
    }

    public void StartSplash()
    {
//        status = FadeStatus.FadeIn;
    }

    // void Update()
    // {
    //     switch(status)
    //     {
    //         case FadeStatus.FadeIn:
    //             alpha += fadeSpeed * Time.deltaTime;
    //         break;
    //         case FadeStatus.FadeWaiting:
    //             if ((!waitForInput && Time.time >= timeFadingInFinished + waitTime) || (waitForInput && Input.anyKey))
    //             {
    //                 status = FadeStatus.FadeOut;
    //             }
    //         break;
    //         case FadeStatus.FadeOut:
    //             alpha += -fadeSpeed * Time.deltaTime;
    //         break;
    //     }
    // }

    // void OnGUI()
    // {
    //     GUI.depth = guiDepth;
    //     if (splashLogo != null)
    //     {
    //         GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, Mathf.Clamp01(alpha));
    //         GUI.DrawTexture(splashLogoPos, splashLogo);
    //         if (alpha > 1.0f)
    //         {
    //             status = FadeStatus.FadeWaiting;
    //             timeFadingInFinished = Time.time;
    //             alpha = 1.0f;
    //             if (splashType == SplashType.LoadNextLevelThenFadeOut)
    //             {
    //                 oldCam.depth = -1000;
    //                 loadingNextLevel = true;
    //                 if ((Application.levelCount >= 1) && (levelToLoad != ""))
    //                 {
    //                     Application.LoadLevel(levelToLoad);
    //                 }
    //             }
    //         }
    //         if (alpha < 0.0f)
    //         {
    //             if (splashType == SplashType.FadeOutThenLoadNextLevel)
    //             {
    //                 if ((Application.levelCount >= 1) && (levelToLoad != ""))
    //                 {
    //                     Application.LoadLevel(levelToLoad);
    //                 }
    //             }
    //             else
    //             {
    //                 Destroy(oldCamGO); // somehow this doesn't work
    //                 Destroy(this);
    //             }
    //         }
    //     }
    // }

    void OnLevelWasLoaded(int lvlIdx)
    {
        if (loadingNextLevel)
        {
            Destroy(oldCam.GetComponent<AudioListener>());
            Destroy(oldCam.GetComponent<GUILayer>());
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, .5f);
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
}