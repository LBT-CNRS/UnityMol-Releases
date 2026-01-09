using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

using UMol;

namespace UMolTest {


/// Test Class for the UnityMolResidue
public class ResidueTest {


	private List<UnityMolAtom> list_atoms;
	private UnityMolResidue res;

	// Run once before running Atom test cases
	[OneTimeSetUp]
	public void Init() {

		list_atoms = new List<UnityMolAtom>();
		list_atoms.Add(new UnityMolAtom("CA", "C", new Vector3(0, 0, 0), 0.0f, 1));
		list_atoms.Add(new UnityMolAtom("C1", "C", new Vector3(1, 1, 1), 0.0f, 2));
		list_atoms.Add(new UnityMolAtom("O1", "O", new Vector3(2, 2, 2), 0.0f, 3));
	}

	[Test]
	public void SizeResidueTest() {
		res = new UnityMolResidue(0, 1, list_atoms, "DUMMY");

		Assert.AreEqual(res.Length, 3);
	}

	[Test]
	public void AvoidAtomDuplicationTest() {
		// Add a duplicate atom
		list_atoms.Add(new UnityMolAtom("CA", "X", new Vector3(0, 0, 0), 0.0f, 1));

		res = new UnityMolResidue(0, 1, list_atoms, "DUMMY");

		Assert.AreEqual(res.Length, 3);
		Assert.AreEqual(res.atoms["CA"].type, "C");

	}
}
}
