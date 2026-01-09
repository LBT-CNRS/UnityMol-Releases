using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;


namespace UMolPlayTests
{

/// <summary>
/// Ensure the scene MainUIScene is correctly loaded.
/// </summary>
public class LoadMainUISceneTest
{

    /// Path to the tested scene
    public string ScenePath = "Assets/Scenes/MainUIScene.unity";

    /// <summary>
    /// Array of Game Objects loaded in the tested scene.
    /// </summary>
    public string[] GOInstances = { "CameraUMolX", "LoadedMolecules", "Directional Light",
                                    "EventSystem", "CanvasMainUI", "ConsolePython_Autocomplete",
                                    "ArgumentReader", "FileDrop", "UMolQuit", "PowerSaving2"};

    [OneTimeSetUp]
    public void OneTimeSetup() {
         EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
    }


    /// <summary>
    /// Check if all Game Objects are loaded when the scene is loaded.
    /// </summary>
    [UnityTest]
    public IEnumerator  GameObjectsTest()
    {
        TestUtils.AssertGO(GOInstances);
        yield return null;
    }

    /// <summary>
    /// Test if some components of the camera are activated.
    /// </summary>
    [UnityTest]
    public IEnumerator CameraComponentsTest() {
        yield return new WaitForSeconds(5);
        var camera = GameObject.Find("CameraUMolX");
        var components = new string[] { "Camera", "CameraManager", "ManipulationManager", "PostProcessVolume" };
        TestUtils.AssertComponents(camera, components);
        yield return null;
    }

}
}
