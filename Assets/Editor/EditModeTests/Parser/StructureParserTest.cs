using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UMol;
using UMol.ForceFields;


namespace UMolTest {

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

/// Generic class Test for the parsing of a molecular structure.
/// The class will test the same structure in different formats.
[TestFixture("2luf.cif.gz", "CIF")]
[TestFixture("2luf.pdb", "PDB")]
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
    public void Init() {

        UnityMolMain.disableSurfaceThread = true;
        UnityMolMain.getStructureManager().DeleteAll();
        string pathTestData = "Assets/Editor/TestData";

        Reader r = Reader.GuessReaderFrom(pathTestData + "/" + fileName, fileType);
        r.ModelsAsTraj = false;

        if (r == null) {
            Assert.Inconclusive("None Parser find for this type of file. Aborting tests.");
        }

        s = r.Read();

        //Get a few random atoms from the readed structure.
        atoms = new List<UnityMolAtom>();
        atoms.Add(s.models[0].allAtoms[0]);
        atoms.Add(s.models[1].allAtoms[2]);
        atoms.Add(s.models[9].allAtoms[303]);

        residues = s.models[0].chains["A"].residues;
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
            // Assert.AreEqual(ref_coords[i], atoms[i].oriPosition);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[i].x, atoms[i].oriPosition.x, 0.0001f);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[i].y, atoms[i].oriPosition.y, 0.0001f);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[i].z, atoms[i].oriPosition.z, 0.0001f);

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

    [Test]
    public void ForcefieldParsingTest() {
        ForceFieldsManager ffm = UnityMolMain.getForceFieldsManager();
        ForceField activeFF = ffm.ActiveForceField;
        FFResidue ffres = activeFF.GetResidue(d.name);
        FFAtom ffatm = ffres.GetAtom("CA");
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ffatm.charge, -0.0249f, 0.00001f);
        Assert.AreEqual(ffatm.type, "CX");
    }

    [Test]
    public void ForcefieldParsing2Test() {
        ForceFieldsManager ffm = UnityMolMain.getForceFieldsManager();
        ForceField activeFF = ffm.ActiveForceField;
        FFResidue ffres = activeFF.GetResidue(d.name);
        FFAtom ffatm = ffres.GetAtom("C");
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ffatm.charge, 0.5973f, 0.00001f);
        Assert.AreEqual(ffatm.type, "C");
    }

}

/// Same tests but with parsing models as a trajectory
[TestFixture("2luf.cif.gz", "CIF")]
[TestFixture("2luf.pdb", "PDB")]
public class StructureModelParserTest {


    //Variables for the TestFixture cases
    private string fileName;
    private string fileType; //Can be: PDB, CIF


    private UnityMolStructure s;
    private List<UnityMolResidue> residues;
    private UnityMolResidue d;


    public StructureModelParserTest(string fileName, string fileType)
    {
        this.fileName = fileName;
        this.fileType = fileType;
    }

    // Run once before all tests
    [OneTimeSetUp]
    public void Init()
    {

        UnityMolMain.disableSurfaceThread = true;
        UnityMolMain.getStructureManager().DeleteAll();
        string pathTestData = "Assets/Editor/TestData";

        Reader r = Reader.GuessReaderFrom(pathTestData + "/" + fileName, fileType);
        r.ModelsAsTraj = true;

        if (r == null) {
            Assert.Inconclusive("None Parser find for this type of file. Aborting tests.");
        }

        s = r.Read();

        residues = s.models[0].chains["A"].residues;
        d = residues[0];
    }

    [Test]
    public void NameTest() {
        Assert.AreEqual("2luf", s.name);
    }

    [Test]
    public void NumberModelsTest() {
        Assert.AreEqual(1, s.models.Count);
    }

    [Test]
    public void NumberModelFramesTest() {
        Assert.AreEqual(10, s.modelFrames.Count);
    }


    [Test]
    public void NumberResiduesTest() {
        Assert.AreEqual(20, residues.Count);
    }

    [Test]
    public void NumberAtomsTest() {
        foreach (Vector3[] frame in s.modelFrames)
            Assert.AreEqual(304, frame.Length);
    }


    [Test]
    public void NumberBondsTest() {
        Assert.AreEqual(310, s.models[0].bonds.Count);
    }

    [Test]
    public void AtomCoordinatesTest() {
        s.setModel(0);

        List<Vector3> ref_coords = new List<Vector3>();
        ref_coords.Add(new Vector3(-0.440f, -2.558f, -5.583f));
        ref_coords.Add(new Vector3(-2.259f, -2.587f, -4.026f));
        ref_coords.Add(new Vector3(8.040f, 1.920f, 4.866f));

        int id = 0;
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[0].x, s.currentModel.allAtoms[id].position.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[0].y, s.currentModel.allAtoms[id].position.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[0].z, s.currentModel.allAtoms[id].position.z, 0.0001f);

        id = 2;
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[1].x, s.currentModel.allAtoms[id].position.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[1].y, s.currentModel.allAtoms[id].position.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[1].z, s.currentModel.allAtoms[id].position.z, 0.0001f);

        id = 303;
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[2].x, s.currentModel.allAtoms[id].position.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[2].y, s.currentModel.allAtoms[id].position.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref_coords[2].z, s.currentModel.allAtoms[id].position.z, 0.0001f);

    }

    [Test]
    public void AtomCoordinatesModelsTest() {
        s.setModel(1);
        Vector3 ref1Atom0 = new Vector3(-1.085f, -2.691f, -6.418f);
        Vector3 ref1Atom2 = new Vector3(-2.268f, -3.438f, -4.507f);
        Vector3 ref1Atom303 = new Vector3(7.916f, 1.825f, 4.789f);


        int id = 0;
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref1Atom0.x, s.currentModel.allAtoms[id].position.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref1Atom0.y, s.currentModel.allAtoms[id].position.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref1Atom0.z, s.currentModel.allAtoms[id].position.z, 0.0001f);

        id = 2;
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref1Atom2.x, s.currentModel.allAtoms[id].position.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref1Atom2.y, s.currentModel.allAtoms[id].position.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref1Atom2.z, s.currentModel.allAtoms[id].position.z, 0.0001f);

        id = 303;
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref1Atom303.x, s.currentModel.allAtoms[id].position.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref1Atom303.y, s.currentModel.allAtoms[id].position.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref1Atom303.z, s.currentModel.allAtoms[id].position.z, 0.0001f);


        s.setModel(9);
        Vector3 ref9Atom0 = new Vector3(-2.932f, -2.659f, -9.715f);
        Vector3 ref9Atom2 = new Vector3(-2.995f, -3.348f, -7.400f);
        Vector3 ref9Atom303 = new Vector3(9.150f, 3.972f, 1.984f);

        id = 0;
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref9Atom0.x, s.currentModel.allAtoms[id].position.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref9Atom0.y, s.currentModel.allAtoms[id].position.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref9Atom0.z, s.currentModel.allAtoms[id].position.z, 0.0001f);

        id = 2;
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref9Atom2.x, s.currentModel.allAtoms[id].position.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref9Atom2.y, s.currentModel.allAtoms[id].position.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref9Atom2.z, s.currentModel.allAtoms[id].position.z, 0.0001f);

        id = 303;
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref9Atom303.x, s.currentModel.allAtoms[id].position.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref9Atom303.y, s.currentModel.allAtoms[id].position.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(ref9Atom303.z, s.currentModel.allAtoms[id].position.z, 0.0001f);

    }

    [Test]
    public void AtomNamesTest() {

        List<string> names = new List<string>();
        names.Add("N");
        names.Add("C");
        names.Add("HD22");

        Assert.AreEqual(names[0], s.currentModel.allAtoms[0].name);
        Assert.AreEqual(names[1], s.currentModel.allAtoms[2].name);
        Assert.AreEqual(names[2], s.currentModel.allAtoms[303].name);
    }

    [Test]
    public void AtomTypesTest() {

        List<string> types = new List<string>();
        types.Add("N");
        types.Add("C");
        types.Add("H");

        Assert.AreEqual(types[0], s.currentModel.allAtoms[0].type);
        Assert.AreEqual(types[1], s.currentModel.allAtoms[2].type);
        Assert.AreEqual(types[2], s.currentModel.allAtoms[303].type);

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


/// Test Class for the PDBParser
public class SimplePDBParserTest {


    private UnityMolStructure s;
    private UnityMolAtom a;



    // Run once before all tests
    [OneTimeSetUp]
    public void Init()
    {

        UnityMolMain.disableSurfaceThread = true;
        UnityMolMain.getStructureManager().DeleteAll();
        string pathTestData = "Assets/Editor/TestData";
        string fileName = "helix_2luf.pdb";

        PDBReader r = new PDBReader(pathTestData + "/" + fileName);
        r.ModelsAsTraj = false;
        s =  r.Read();
        a = s.models[0].allAtoms[20];

    }

    [Test]
    public void ChainNameTest() {
        Assert.AreEqual("_", s.models[0].chains["_"].name);
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
        // Assert.AreEqual(new Vector3(-6.449f, 8.090f, 0.385f), a.oriPosition);
        Vector3 p = new Vector3(-6.449f, 8.090f, 0.385f);

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(p.x, a.oriPosition.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(p.y, a.oriPosition.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(p.z, a.oriPosition.z, 0.0001f);
    }

}


/// Test Class for the GroParser
public class SimpleGroParserTest {


    private UnityMolStructure s;
    private UnityMolAtom a;


    // Run once before all tests
    [OneTimeSetUp]
    public void Init()
    {

        UnityMolMain.disableSurfaceThread = true;
        UnityMolMain.getStructureManager().DeleteAll();
        string pathTestData = "Assets/Editor/TestData";
        string fileName = "helix_2luf.gro";

        GROReader r = new GROReader(pathTestData + "/" + fileName);
        r.ModelsAsTraj = false;
        s = r.Read();
        a = s.models[0].allAtoms[10];

    }

    [Test]
    public void ResidueNameTest() {
        Assert.AreEqual("SER", a.residue.name);
    }

    [Test]
    public void ResidueNameTest2() {
        Assert.AreEqual("TRP", s.currentModel.allAtoms[s.Length - 1].residue.name);
    }

    [Test]
    public void ChainNameTest() {
        Assert.AreEqual("A", s.models[0].chains["A"].name);
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
        Vector3 expected = new Vector3(-9.43f, 1.25f, -1.96f);


        UnityEngine.Assertions.Assert.AreApproximatelyEqual(expected.x, a.position.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(expected.y, a.position.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(expected.z, a.position.z, 0.0001f);

    }

    [Test]
    public void BoxTest() {
        Vector3 expected = new Vector3(1.0f, 2.0f, 3.0f);

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(expected.x, s.periodic.x, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(expected.y, s.periodic.y, 0.0001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(expected.z, s.periodic.z, 0.0001f);

    }
}

public class MultipleFramesGroParserTest {

    private UnityMolStructure s;
    private UnityMolAtom a;

    // Run once before all tests
    [OneTimeSetUp]
    public void Init()
    {

        UnityMolMain.disableSurfaceThread = true;
        UnityMolMain.getStructureManager().DeleteAll();
        string pathTestData = "Assets/Editor/TestData";
        string fileName = "2frames.gro";

        GROReader r = new GROReader(pathTestData + "/" + fileName);
        r.ModelsAsTraj = false;
        s = r.Read();
        a = s.models[0].allAtoms[0];

    }

    [Test]
    public void ResidueNameTest() {
        Assert.AreEqual("DA", a.residue.name);
    }

    [Test]
    public void ChainNameTest() {
        Assert.AreEqual("A", s.models[0].chains["A"].name);
    }

    [Test]
    public void NumberAtomsTest() {
        Assert.AreEqual(2, s.Length);
    }

    [Test]
    public void modelFramesTest() {

    Vector3 frame1 = new Vector3(-32.46f ,61.91f, 08.74f);
    Vector3 frame2 = new Vector3(-32.67f, 60.73f, 08.76f);

    UnityEngine.Assertions.Assert.AreApproximatelyEqual(frame1.x, s.modelFrames[0][0].x, 0.0001f);
    UnityEngine.Assertions.Assert.AreApproximatelyEqual(frame1.y, s.modelFrames[0][0].y, 0.0001f);
    UnityEngine.Assertions.Assert.AreApproximatelyEqual(frame1.z, s.modelFrames[0][0].z, 0.0001f);
    UnityEngine.Assertions.Assert.AreApproximatelyEqual(frame2.x, s.modelFrames[1][0].x, 0.0001f);
    UnityEngine.Assertions.Assert.AreApproximatelyEqual(frame2.y, s.modelFrames[1][0].y, 0.0001f);
    UnityEngine.Assertions.Assert.AreApproximatelyEqual(frame2.z, s.modelFrames[1][0].z, 0.0001f);
    }

}


/// Test Class for the XTCParser
public class XTCParserTest {


    private UnityMolStructure s;
    private UnityMolAtom a0;
    private UnityMolAtom ahalf;
    private UnityMolAtom alast;


    // Run once before all tests
    [OneTimeSetUp]
    public void Init()
    {

        UnityMolMain.disableSurfaceThread = true;
        UnityMolMain.getStructureManager().DeleteAll();
        string pathTestData = "Assets/Editor/TestData";
        string fileName = "protein_md.pdb";
        string xtcFileName = "protein_md_clean.xtc";

        PDBReader r = new PDBReader(pathTestData + "/" + fileName);
        r.ModelsAsTraj = false;
        s = r.Read();
        int N = s.Count;
        a0 = s.models[0].allAtoms[0];
        ahalf = s.models[0].allAtoms[N / 2];
        alast = s.models[0].allAtoms[N - 1];

        s.readTrajectoryXDR(pathTestData + "/" + xtcFileName);
    }


    [Test]
    public void TrajectoryLoadedTest() {
        Assert.AreNotEqual(null, s.xdr);
        Assert.AreEqual(true, s.trajectoryLoaded);
    }

    [Test]
    public void StartingFrameIdTest() {
        s.trajSetFrame(0);
        Assert.AreEqual(0, s.xdr.CurrentFrame);
    }

    [Test]
    public void FrameCountTest() {
        Assert.AreEqual(5001, s.xdr.NumberFrames);
    }

    [Test]
    public void FirstFrameAtomPositionTest() {
        s.trajSetFrame(0);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-35.390f, a0.position.x, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(41.390f, a0.position.y, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(12.320f, a0.position.z, 0.001f);

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-30.520f, ahalf.position.x, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(31.840f, ahalf.position.y, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(13.630f,  ahalf.position.z, 0.001f);

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-38.250f, alast.position.x, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(21.639f, alast.position.y, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(14.150f,  alast.position.z, 0.001f);

    }

    [Test]
    public void HalfFrameAtomPositionTest() {
        s.trajSetFrame(s.xdr.NumberFrames / 2);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-43.300f, a0.position.x, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(29.050f, a0.position.y, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(7.510f, a0.position.z, 0.001f);

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-34.500f, ahalf.position.x, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(34.659f, ahalf.position.y, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(18.210f,  ahalf.position.z, 0.001f);

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-22.590f, alast.position.x, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(29.110f, alast.position.y, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(19.330f,  alast.position.z, 0.001f);

    }

    [Test]
    public void LastFrameAtomPositionTest() {
        s.trajSetFrame(s.xdr.NumberFrames - 1);

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-36.510f, a0.position.x, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(39.500f, a0.position.y, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(7.530f, a0.position.z, 0.001f);

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-36.959f, ahalf.position.x, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(34.080f, ahalf.position.y, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(15.819f,  ahalf.position.z, 0.001f);

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(-22.550f, alast.position.x, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(25.040f, alast.position.y, 0.001f);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(20.110f,  alast.position.z, 0.001f);

    }

}

}
