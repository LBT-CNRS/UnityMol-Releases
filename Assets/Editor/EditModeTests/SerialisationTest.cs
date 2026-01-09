using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using UMol;

namespace UMolTest
{


/// Test Class for the Serialisation
/// Each test must be in the correct order, from the simplest to the most complicated.
/// It's due to the fact that the managers cannot be cleared (UnityError) between tests during the EditMode tests
/// and those tests keep modifying the different managers.
/// Make sure the SerialisationTestClear is at the end for clearing the managers and to make sure next tests will not fail.
public class SerialisationTest {


    private UnityMolStructure s;

    private string pathTestData = "Assets/Editor/TestData";


    [OneTimeSetUp]
    public void Init()
    {
        UnityMolMain.disableSurfaceThread = true;
        UnityMolMain.getRepresentationManager().Clean();
        UnityMolMain.getSelectionManager().Clean();
        UnityMolMain.getStructureManager().DeleteAll();

        PDBxReader rx = new PDBxReader();
        rx.ModelsAsTraj = true;

        s = rx.Fetch("2LUF");

        s.readHET = true;
        s.modelsAsTraj = true;
        s.fetchedmmCIF = true;
        s.pdbID = "2LUF";
        s.bioAssembly = false;
    }

    /// Test with a simple JSON file.
    /// Just the fecth command has been used.
    [Test]
    public void writeStateFile1() {

        string sessionJson = UnityMolMain.sessionToJSON();

        string pathJSON = pathTestData + "/" + "state1.json" ;
        string refSimpleJSON = File.ReadAllText(pathJSON, Encoding.UTF8);

        Assert.AreEqual(sessionJson, refSimpleJSON);

    }

    /// Test with a more complicated case.
    /// Fetch used and one selection and one representation have been added.
    [Test]
    public void writeStateFile2() {

        // Add one selection containing ALA residus
        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        MDAnalysisSelection selec = new MDAnalysisSelection("resname GLY", s.currentModel.allAtoms);
        UnityMolSelection newContent = selec.process();
        Debug.Log("number of atoms : " + newContent.ToString());
        newContent.name = "select_ala";
        selM.Add(newContent);

        //Add one representation of the new "select_ala" selection.
        UnityMolRepresentationManager repManager = UnityMolMain.getRepresentationManager();
        repManager.AddRepresentation(newContent, AtomType.sphere, BondType.nobond);


        string sessionJson = UnityMolMain.sessionToJSON();
        string pathJSON = pathTestData + "/" + "state2.json" ;
        string refSimpleJSON = File.ReadAllText(pathJSON, Encoding.UTF8);

        Assert.AreEqual(sessionJson, refSimpleJSON);

    }

    /// Test with one annotation.
    [Test]
    public void writeStateFile3() {

        UnityMolAnnotationManager anM = UnityMolMain.getAnnotationManager();

        UnityMolAtom a = s.currentModel.getAtomWithID(1);
        UnityMolAtom a2 = s.currentModel.getAtomWithID(6);
        anM.AnnotateDistance(a, a2);

        string sessionJson = UnityMolMain.sessionToJSON();
        string pathJSON = pathTestData + "/" + "state3.json" ;
        string refSimpleJSON = File.ReadAllText(pathJSON, Encoding.UTF8);

        Assert.AreEqual(sessionJson, refSimpleJSON);
    }

}


/// Dummy Test class to clear the different managers and the different inners calls to Destroy().
/// Destroy() calls made Unity Editor Test to fail due to the error message.
/// Here we call the `UnityMolMain.getStructureManager().DeleteAll()` just after ignoring the error messages on a dummy test.
/// This will clear the managers and make sure the next tests are not failing.

public class SerialisationTestClear {

    [Test]
    public void clearGO()
    {
        LogAssert.ignoreFailingMessages = true;
        UnityMolMain.getStructureManager().DeleteAll();
    }
}

}
