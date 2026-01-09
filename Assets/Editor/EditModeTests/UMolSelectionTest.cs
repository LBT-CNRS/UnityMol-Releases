using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UMol;

namespace UMolTest
{


/// Test Class for the UnityMolSelection
// TODO: Test Serialize function
public class UnityMolSelectionTest {

    private string fileName = "2luf.pdb";
    private string fileType = "PDB";
    private UnityMolStructure s, s1;
    private UnityMolSelection selBonds, selGlobal;

    [OneTimeSetUp]
    public void Init()
    {

        UnityMolMain.disableSurfaceThread = true;
        UnityMolMain.getStructureManager().DeleteAll();
        // Load Structure
        string pathTestData = "Assets/Editor/TestData";

        Reader r = Reader.GuessReaderFrom(pathTestData + "/" + fileName, fileType);
        r.ModelsAsTraj = false;

        if (r == null) {
            Assert.Inconclusive("None Parser find for this type of file. Aborting tests.");
        }

        s = r.Read();

        // Create reference selections
        selBonds = new UnityMolSelection(s.currentModel.allAtoms, s.currentModel.bonds,"test_sel");

        // Creat a global selection from 2 structures
        UnityMolStructure s1 = Reader.GuessReaderFrom(pathTestData + "/helix_2luf.pdb", "PDB").Read();
        List<UnityMolAtom> atoms = s.currentModel.chains["A"].residues[15].ToAtomList().Concat(s1.currentModel.chains["_"].residues[0].ToAtomList()).ToList();
        selGlobal = new UnityMolSelection(atoms,"sel_global");
    }

    // Test different constructors, fillStructures() and bondsNull
    [Test]
    public void OthersConstructorsTest()
    {
        UnityMolSelection sel1 = new UnityMolSelection(s.currentModel.allAtoms,"test_sel1");
        Assert.AreEqual(s.Count, sel1.Count);
        Assert.AreEqual(false, sel1.fromSelectionLanguage);
        Assert.AreEqual(s, sel1.structures[0]);
        Assert.AreEqual(true, sel1.bondsNull);

        UnityMolSelection sel2 = new UnityMolSelection(s.currentModel.allAtoms,"test_sel2_mda", "select all atoms");
        Assert.AreEqual(true, sel2.fromSelectionLanguage);
        Assert.AreEqual("select all atoms", sel2.MDASelString);

        UnityMolSelection sel3 = new UnityMolSelection(s.currentModel.allAtoms[4], "test_sel3");
        Assert.AreEqual(1, sel3.Count);
        Assert.AreEqual(false, sel3.fromSelectionLanguage);
        Assert.AreEqual(s, sel3.structures[0]);
        Assert.AreEqual(true, sel3.bondsNull);
        Assert.AreEqual(0, sel3.bonds.Count);
    }

    [Test]
    public void fillBondsTest()
    {
        UnityMolSelection sel = new UnityMolSelection(s.currentModel.ToAtomList(), "test_sel");
        Assert.AreEqual(true, sel.bondsNull);
        sel.fillBonds();
        Assert.AreEqual(310, sel.bonds.Count);
    }

    [Test]
    public void isGlobalSelectionTest()
    {
        Assert.AreEqual(false, selBonds.isGlobalSelection());
        Assert.AreEqual(true, selGlobal.isGlobalSelection());
    }

    [Test]
    public void ToSelectionCommandTest()
    {

        Assert.AreEqual("2luf and (atomid 1:304) ",selBonds.ToSelectionCommand());
        Assert.AreEqual("2luf and (atomid 214:230 or helix_2luf and (atomid 1:11) ",selGlobal.ToSelectionCommand());

    }

    [Test]
    public void sameAtomsTest()
    {
        Assert.AreEqual(true, selBonds.sameAtoms(selBonds));
        Assert.AreEqual(false,selBonds.sameAtoms(selGlobal));
    }

    [Test]
    public void sameModelTest()
    {
        Assert.AreEqual(true, selBonds.sameModel());
        Assert.AreEqual(false, selGlobal.sameModel());

        //Construct selection of atoms from the same structure but from different models
        List<UnityMolAtom> atomsModels = s.currentModel.chains["A"].residues[0].ToAtomList().Concat(s.models[1].chains["A"].residues[0].ToAtomList()).ToList();
        UnityMolSelection selModels = new UnityMolSelection(atomsModels,"selModels");
        Assert.AreEqual(false,selModels.sameModel());
    }

    // computeMinMaxPos() is called when accessing maxPos and minPos
    [Test]
    public void computeMinMaxPos()
    {
        // MaxPos
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(11.436f, selBonds.maxPos.x, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(11.300f, selBonds.maxPos.y, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(7.889f, selBonds.maxPos.z, 0.001f);
        // MinPos
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-11.836f, selBonds.minPos.x, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-9.118f, selBonds.minPos.y, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-7.482f, selBonds.minPos.z, 0.001f);
    }

}
}
