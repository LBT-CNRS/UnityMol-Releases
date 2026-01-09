using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UMolPlayTests
{

/// Utility class used in Play Mode Tests
public class TestUtils
{

    /// <summary>
    /// Assert all Game Objects are find and not null.
    /// </summary>
    /// <param name="GameObjects"> array of the Game Object's names</param>
    public static void AssertGO(string[] GameObjects) {
        foreach (var name in GameObjects) {
            Assert.IsNotNull(GameObject.Find(name), "Missing Game Object " + name);
        }
    }

    /// <summary>
    /// Assert all Components <c>components</c> are activated in the Game Object go.
    /// </summary>
    /// <param name="go">Game Object wanted</param>
    /// <param name="components"> array of Component's names</param>
    public static void AssertComponents(GameObject go, string[] components) {
        foreach (var name in components) {
            Assert.IsNotNull(go.GetComponent(name), "Missing component " + name + " in Game Object " + go.ToString());
        }
    }
}

}
