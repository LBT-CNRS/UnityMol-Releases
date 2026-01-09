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


/// Test Class for the MDAnalysisSelection
public class MDASelectionTest {


    private string fileName = "2luf.pdb";
    private string fileType = "PDB";
    private UnityMolStructure s;

    [OneTimeSetUp]
    public void Init()
    {
        UnityMolMain.disableSurfaceThread = true;
        UnityMolMain.getStructureManager().DeleteAll();
        string pathTestData = "Assets/Editor/TestData";

        Reader r = Reader.GuessReaderFrom(pathTestData + "/" + fileName, fileType);
        r.ModelsAsTraj = false;

        if (r == null) {
            Assert.Inconclusive("None Parser find for this type of file. Aborting tests.");
        }

        s = r.Read();
    }

    [Test]
    public void TestProtein() {
        MDAnalysisSelection selec = new MDAnalysisSelection("protein", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 304);
        Assert.AreEqual(ret.bonds.Count, 310);
    }


    [Test]
    public void TestBackbone() {
        MDAnalysisSelection selec = new MDAnalysisSelection("backbone", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 80);
        Assert.AreEqual(ret.bonds.Count, 79);
    }

    [Test]
    public void TestSingleResid() {
        MDAnalysisSelection selec = new MDAnalysisSelection("resid 4", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 14);
        Assert.AreEqual(ret.bonds.Count, 14);
        Assert.AreEqual(ret.atoms[0].residue.name, "PRO");

    }

    [Test]
    public void TestResidRange() {
        MDAnalysisSelection selec = new MDAnalysisSelection("resid 11:15", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 84);
        Assert.AreEqual(ret.bonds.Count, 85);

        //Retrieve all resnames from atomlist and discard repetition
        // This wont work with following residues which have same name.
        List<string> resnames = ret.atoms.Select(f => f.residue.name).Distinct().ToList();
        CollectionAssert.AreEqual(resnames, new List<string> {"GLY", "ASP", "LYS", "LEU", "TRP" });
    }


    [Test]
    public void TestResidueName() {
        MDAnalysisSelection selec = new MDAnalysisSelection("resname GLY", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 21);
        Assert.AreEqual(ret.bonds.Count, 19);
        Assert.AreEqual(ret.atoms[0].residue.id, 6);
        Assert.AreEqual(ret.atoms[ret.Count - 1].residue.id, 11);
    }

    [Test]
    public void TestAtomName() {
        MDAnalysisSelection selec = new MDAnalysisSelection("name CB", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 17);
        Assert.AreEqual(ret.bonds.Count, 0);
    }

    [Test]
    public void TestSingleAtom() {
        MDAnalysisSelection selec = new MDAnalysisSelection("resid 1 and name CB", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 1);
        Assert.AreEqual(ret.atoms[0].name, "CB");
        Assert.AreEqual(ret.atoms[0].number, 5);
        Assert.AreEqual(ret.atoms[0].residue.name, "SER");
    }

    [Test]
    public void TestOr() {
        MDAnalysisSelection selec = new MDAnalysisSelection("resname GLY or resname SER", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 56);
        Assert.AreEqual(ret.bonds.Count, 53);
    }

    [Test]
    public void TestNot() {
        MDAnalysisSelection selec = new MDAnalysisSelection("not resname GLY", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 283);
        Assert.AreEqual(ret.bonds.Count, 287);
    }

    [Test]
    public void TestComplexSelection() {
        MDAnalysisSelection selec = new MDAnalysisSelection("protein and not (resname GLY or resname SER) and not name CB",
                s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 234);
        Assert.AreEqual(ret.bonds.Count, 196);
    }

    [Test]
    public void TestSecondaryStructure() {
        MDAnalysisSelection selec = new MDAnalysisSelection("ss helix", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 152);
        Assert.AreEqual(ret.bonds.Count, 154);
    }

    [Test]
    public void TestModel() {

        //Retrieve all atoms from all models
        List<UnityMolAtom> allAtoms = new List<UnityMolAtom>();
        foreach (UnityMolModel m in s.models) {
            allAtoms.AddRange(m.allAtoms);
        }

        MDAnalysisSelection selec = new MDAnalysisSelection("model 2 and resid 1 and name CA", allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 1);
        Assert.AreEqual(ret.atoms[0].position.x, -0.006f);
        Assert.AreEqual(ret.atoms[0].position.y, -1.648f);
        Assert.AreEqual(ret.atoms[0].position.z, -6.132f);
    }


    [Test]
    public void TestEmpty() {
        MDAnalysisSelection selec = new MDAnalysisSelection("name XE", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 0);
        Assert.AreEqual(ret.bonds.Count, 0);
    }

    [Test]
    public void TestProp() {
        MDAnalysisSelection selec = new MDAnalysisSelection("prop y > 7", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();
        Assert.AreEqual(ret.Count, 23);

        MDAnalysisSelection selec2 = new MDAnalysisSelection("prop x < 3", s.currentModel.allAtoms);
        UnityMolSelection ret2 = selec2.process();
        Assert.AreEqual(ret2.Count, 213);

        MDAnalysisSelection selec3 = new MDAnalysisSelection("prop z >= 5", s.currentModel.allAtoms);
        UnityMolSelection ret3 = selec3.process();
        Assert.AreEqual(ret3.Count, 12);

        MDAnalysisSelection selec4 = new MDAnalysisSelection("prop z <= 7 and prop x > 3", s.currentModel.allAtoms);
        UnityMolSelection ret4 = selec4.process();
        Assert.AreEqual(ret4.Count, 91);

    }

    [Test]
    public void TestAroundWithin() {
        MDAnalysisSelection selec = new MDAnalysisSelection("around 3.0 resid 13", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 43);

        MDAnalysisSelection selec2 = new MDAnalysisSelection("around 3.0 resid 13", s.currentModel.allAtoms);
        UnityMolSelection ret2 = selec2.process();

        Assert.AreEqual(ret2.Count, 43);
    }

    [Test]
    public void TestAroundWithin2() {
        MDAnalysisSelection selec = new MDAnalysisSelection("not resid 10 and around 4.5 resid 10", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 52);

        List<int> atomList = new List<int>(52);
        foreach (UnityMolAtom a in ret.atoms) {
            atomList.Add(a.idInAllAtoms);
        }

        atomList.Sort();

        List<int> referenceAtomList = new List<int>() {98, 99, 100, 101, 104, 106, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 129, 130, 131, 132, 133, 134, 135, 136, 138, 144, 148, 149, 150, 152, 153, 157, 159, 160, 161, 170, 171, 174, 175, 177, 178, 180, 181, 182, 186, 187, 188, 203};
        for (int i = 0; i < atomList.Count; i++) {
            Assert.AreEqual(atomList[i], referenceAtomList[i]);
        }
    }



    [Test]
    // The structure is not centered when doing this test.
    public void TestInsphere() {
        MDAnalysisSelection selec = new MDAnalysisSelection("insphere 1.0 2.0 1.0 3.0 ", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 14);
    }

    [Test]
    // The structure is not centered when doing this test.
    public void TestInrect() {
        MDAnalysisSelection selec = new MDAnalysisSelection("inrect 1.0 1.0 1.0 5.0 0.0 0.0 0.0 5.0 0.0 0.0 0.0 5.0",
                s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        Assert.AreEqual(ret.Count, 19);
    }

    [Test]
    public void TestWildcard() {
        MDAnalysisSelection selec = new MDAnalysisSelection("resname L*", s.currentModel.allAtoms);
        UnityMolSelection ret = selec.process();

        MDAnalysisSelection selec_ref = new MDAnalysisSelection("resname LEU LYS", s.currentModel.allAtoms);
        UnityMolSelection ret_ref = selec_ref.process();

        Assert.AreEqual(ret.Count, ret_ref.Count);
    }

}

}
