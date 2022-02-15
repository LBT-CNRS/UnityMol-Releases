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
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UMol {

namespace Tests
{


public class ReaderTest {

    private Reader r;


    [Test]
    public void GuessedPDBParser() {
        r = Reader.GuessReaderFrom("2luf.pdb.gz");

        Assert.That(r, Is.InstanceOf<PDBReader>());
    }

    [Test]
    public void GuessedPDBxParser() {
        r = Reader.GuessReaderFrom("2luf.mmcif");

        Assert.That(r, Is.InstanceOf<PDBxReader>());
    }

    [Test]
    public void GuessedUnknownParser() {
        r = Reader.GuessReaderFrom("2luf.azererze");

        Assert.IsNull(r);
    }
}

// Generic class Test for the parsing of a molecular structure.
// The class will test the same structure in different formats.
// It uses the TestFixture attribute to parameterized the tests.
[TestFixture("2luf.cif.gz","CIF")]
[TestFixture("2luf.pdb","PDB")]
public class StructureParserTest {


    //Variables for the TestFixture cases
    private string fileName;
    private string fileType; //Can be: PDB, CIF


    private UnityMolStructure s;
    private List<UnityMolAtom> atoms;
    private List<UnityMolResidue> residues;
    private UnityMolResidue d;


    public StructureParserTest(string fileName, string fileType)
    {
        this.fileName = fileName;
        this.fileType = fileType;
    }

    // Run once before all tests
    [OneTimeSetUp]
    public void Init()
    {
        UnityMolMain.getStructureManager().DeleteAll();
        string pathTestData = "Assets/Editor/TestData";

        Reader r = Reader.GuessReaderFrom(pathTestData + "/"+ fileName, fileType);
        r.modelsAsTraj = false;

        if(r == null) {
            Assert.Inconclusive("None Parser find for this type of file. Aborting tests.");
        }

        s = r.Read();

        //Get a few random atoms from the readed structure.
        atoms = new List<UnityMolAtom>();
        atoms.Add(s.models[0].allAtoms[0]);
        atoms.Add(s.models[1].allAtoms[2]);
        atoms.Add(s.models[9].allAtoms[303]);

        residues = s.models[0].chains["A"].residues.Values.ToList();
        d = residues[0];
    }

    [Test]
    public void NameTest() {
        Assert.AreEqual("2luf", s.name);
    }

    [Test]
    public void NumberModelsTest() {
        Assert.AreEqual(10, s.models.Count);
    }

    [Test]
    public void NumberResiduesTest() {
        Assert.AreEqual(20, residues.Count);
    }

    [Test]
    public void NumberAtomsTest() {
        Assert.AreEqual(304, s.Length);
    }

    [Test]
    public void NumberBondsTest() {
        Assert.AreEqual(310, s.models[0].bonds.Count);
    }


    [Test]
    public void AtomCoordinatesTest() {

        List<Vector3> ref_coords = new List<Vector3>();
        ref_coords.Add(new Vector3(-0.440f, -2.558f, -5.583f));
        ref_coords.Add(new Vector3(-2.268f, -3.438f, -4.507f));
        ref_coords.Add(new Vector3(9.150f, 3.972f, 1.984f));

        for (int i = 0; i < atoms.Count; i++)
        {
            Assert.AreEqual(ref_coords[i], atoms[i].oriPosition);
        }

    }

    [Test]
    public void AtomNamesTest() {

        List<string> names = new List<string>();
        names.Add("N");
        names.Add("C");
        names.Add("HD22");

        for (int i = 0; i < atoms.Count; i++)
        {
            Assert.AreEqual(names[i], atoms[i].name);
        }
    }

    [Test]
    public void AtomTypesTest() {

        List<string> types = new List<string>();
        types.Add("N");
        types.Add("C");
        types.Add("H");

        for (int i = 0; i < atoms.Count; i++)
        {
            Assert.AreEqual(types[i], atoms[i].type);
        }
    }

    [Test]
    public void ResidueNBAtomsTest() {
        Assert.AreEqual(13, d.allAtoms.Count);

    }

    [Test]
    public void ResidueNameTest() {
        Assert.AreEqual("SER", d.name);
    }

    [Test]
    public void SecondaryStructure1Test() {
        Assert.AreEqual(UnityMolResidue.secondaryStructureType.Helix, residues[7].secondaryStructure);
    }
    [Test]
    public void SecondaryStructure2Test() {
        Assert.AreEqual(UnityMolResidue.secondaryStructureType.Helix, residues[16].secondaryStructure);
    }
    [Test]
    public void SecondaryStructure3Test() {
        Assert.AreEqual(UnityMolResidue.secondaryStructureType.Helix, residues[10].secondaryStructure);
    }
    [Test]
    public void SecondaryStructure4Test() {
        Assert.AreEqual(UnityMolResidue.secondaryStructureType.Coil, residues[6].secondaryStructure);
    }
    [Test]
    public void SecondaryStructure5Test() {
        Assert.AreEqual(UnityMolResidue.secondaryStructureType.Coil, residues[17].secondaryStructure);
    }
}

[TestFixture]
public class SimplePDBParserTest {


    private UnityMolStructure s;
    private UnityMolAtom a;



    // Run once before all tests
    [OneTimeSetUp]
    public void Init()
    {
        UnityMolMain.getStructureManager().DeleteAll();
        string pathTestData = "Assets/Editor/TestData";
        string fileName = "helix_2luf.pdb";

        PDBReader r = new PDBReader(pathTestData + "/"+ fileName);
        r.modelsAsTraj = false;
        s =  r.Read();
        a = s.models[0].allAtoms[20];

    }

    [Test]
    public void ChainNameTest() {
        Assert.AreEqual("_",s.models[0].chains["_"].name);
    }

    [Test]
    public void NumberAtomsTest() {
        Assert.AreEqual(116, s.Length);
    }

    [Test]
    public void AtomNamesTest() {
        Assert.AreEqual("HB3", a.name);
    }

    [Test]
    public void AtomCoordinatesTest() {
        Assert.AreEqual(new Vector3(-6.449f, 8.090f, 0.385f), a.oriPosition);
    }

}

[TestFixture]
public class SimpleGroParserTest {


    private UnityMolStructure s;
    private UnityMolAtom a;



    // Run once before all tests
    [OneTimeSetUp]
    public void Init()
    {
        UnityMolMain.getStructureManager().DeleteAll();
        string pathTestData = "Assets/Editor/TestData";
        string fileName = "helix_2luf.gro";

        GROReader r = new GROReader(pathTestData + "/"+ fileName);
        r.modelsAsTraj = false;
        s =  r.Read();
        a = s.models[0].allAtoms[10];

    }

    [Test]
    public void ChainNameTest() {
        Assert.AreEqual("A",s.models[0].chains["A"].name);
    }

    [Test]
    public void NumberAtomsTest() {
        Assert.AreEqual(116, s.Length);
    }

    [Test]
    public void AtomNamesTest() {
        Assert.AreEqual("HG", a.name);
    }

    [Test]
    public void AtomCoordinatesTest() {
        Vector3 expected = new Vector3(-9.43f, 1.25f,-1.96f);

        float distance = Vector3.Distance(expected, a.position);
        Assert.That(distance, Is.LessThanOrEqualTo(0.001f));
    }

}
}
}