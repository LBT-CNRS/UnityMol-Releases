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

//Adapted from MDAnalysis
/*

#
# MDAnalysis --- https://www.mdanalysis.org
# Copyright (c) 2006-2017 The MDAnalysis Development Team and contributors
# (see the file AUTHORS for the full list of names)
#
# Released under the GNU Public Licence, v2 or any higher version
#
# Please cite your use of MDAnalysis in published work:
#
# R. J. Gowers, M. Linke, J. Barnoud, T. J. E. Reddy, M. N. Melo, S. L. Seyler,
# D. L. Dotson, J. Domanski, S. Buchoux, I. M. Kenney, and O. Beckstein.
# MDAnalysis: A Python package for the rapid analysis of molecular dynamics
# simulations. In S. Benthall and S. Rostrup editors, Proceedings of the 15th
# Python in Science Conference, pages 102-109, Austin, TX, 2016. SciPy.
#
# N. Michaud-Agrawal, E. J. Denning, T. B. Woolf, and O. Beckstein.
# MDAnalysis: A Toolkit for the Analysis of Molecular Dynamics Simulations.
# J. Comput. Chem. 32 (2011), 2319--2327, doi:10.1002/jcc.21787
#

"""Atom selection Hierarchy --- :mod:`MDAnalysis.core.selection`
=============================================================
This module contains objects that represent selections. They are
constructed and then applied to the group.
In general, :meth:`Parser.parse` creates a :class:`MDASelection` object
from a selection string. This :class:`MDASelection` object is then passed
an :class:`~MDAnalysis.core.groups.AtomGroup` through its
:meth:`~MDAnalysis.core.groups.AtomGroup.apply` method to apply the
``MDASelection`` to the ``AtomGroup``.
This is all invisible to the user through the
:meth:`~MDAnalysis.core.groups.AtomGroup.select_atoms` method of an
:class:`~MDAnalysis.core.groups.AtomGroup`.
"""
*/
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

using Unity.Mathematics;
using Unity.Collections;
using KNN;
using KNN.Jobs;
using Unity.Jobs;

//Adapted from MDAnalysis
/*

#
# MDAnalysis --- https://www.mdanalysis.org
# Copyright (c) 2006-2017 The MDAnalysis Development Team and contributors
# (see the file AUTHORS for the full list of names)
#
# Released under the GNU Public Licence, v2 or any higher version
#
# Please cite your use of MDAnalysis in published work:
#
# R. J. Gowers, M. Linke, J. Barnoud, T. J. E. Reddy, M. N. Melo, S. L. Seyler,
# D. L. Dotson, J. Domanski, S. Buchoux, I. M. Kenney, and O. Beckstein.
# MDAnalysis: A Python package for the rapid analysis of molecular dynamics
# simulations. In S. Benthall and S. Rostrup editors, Proceedings of the 15th
# Python in Science Conference, pages 102-109, Austin, TX, 2016. SciPy.
#
# N. Michaud-Agrawal, E. J. Denning, T. B. Woolf, and O. Beckstein.
# MDAnalysis: A Toolkit for the Analysis of Molecular Dynamics Simulations.
# J. Comput. Chem. 32 (2011), 2319--2327, doi:10.1002/jcc.21787
#

"""Atom selection Hierarchy --- :mod:`MDAnalysis.core.selection`
=============================================================
This module contains objects that represent selections. They are
constructed and then applied to the group.
In general, :meth:`Parser.parse` creates a :class:`MDASelection` object
from a selection string. This :class:`MDASelection` object is then passed
an :class:`~MDAnalysis.core.groups.AtomGroup` through its
:meth:`~MDAnalysis.core.groups.AtomGroup.apply` method to apply the
``MDASelection`` to the ``AtomGroup``.
This is all invisible to the user through the
:meth:`~MDAnalysis.core.groups.AtomGroup.select_atoms` method of an
:class:`~MDAnalysis.core.groups.AtomGroup`.
"""
*/

namespace UMol {
public class MDAnalysisSelection {

    public static HashSet<string> predefinedKeywords = new HashSet<string>() {
        "all", "nothing", "empty", "not", "byres", "resid", "name",
        "type", "atomid", "resname", "chain", "model", "ss", "protein", "nucleic", "backbone",
        "water", "ligand", "nucleicbackbone", "nucleicbase", "nucleicsugar",
        "prop", "around", "within", "insphere", "and", "or"
    };
    public Dictionary<string, MDASelection> selectionKeywords = new Dictionary<string, MDASelection>();
    public Dictionary<string, Operation> operatorKeywords = new Dictionary<string, Operation>();
    // List<string> reservedKeywords = new List<string> {"updating"};

    public string selectionStr;
    public HashSet<UnityMolAtom> input;
    public SelectionParser slP;

    public MDAnalysisSelection(string selStr, List<UnityMolAtom> atoms) {

        input = new HashSet<UnityMolAtom>(atoms);

        selectionStr = selStr;
        slP = new SelectionParser(this);


        selectionKeywords["all"] = new AllSelection(this, "all", 0);
        selectionKeywords["nothing"] = new NothingSelection(this, "nothing", 0);
        selectionKeywords["empty"] = new NothingSelection(this, "empty", 0);

        selectionKeywords["not"] = new NotSelection(this, "not", 5);
        selectionKeywords["byres"] = new ByResSelection(this, "byres", 1);
        selectionKeywords["resid"] = new RangeResidSelection(this, "resid", 0);
        selectionKeywords["name"] = new AtomNameSelection(this, "name", 0);
        selectionKeywords["type"] = new AtomTypeNameSelection(this, "type", 0);
        selectionKeywords["atomid"] = new RangeAtomIdSelection(this, "atomid", 0);
        selectionKeywords["resname"] = new ResidueNameSelection(this, "resname", 0);
        selectionKeywords["chain"] = new ChainNameSelection(this, "chain", 0);
        selectionKeywords["model"] = new ModelNameSelection(this, "model", 0);

        selectionKeywords["ss"] = new SecondaryStructureSelection(this, "ss", 0);


        selectionKeywords["protein"] = new ProteinSelection(this, "protein", 0);
        selectionKeywords["nucleic"] = new NucleicSelection(this, "nucleic", 0);
        selectionKeywords["backbone"] = new BackboneSelection(this, "backbone", 0);
        selectionKeywords["water"] = new WaterSelection(this, "water", 0);
        selectionKeywords["ligand"] = new LigandSelection(this, "ligand", 0);
        selectionKeywords["ions"] = new IonsSelection(this, "ions", 0);
        selectionKeywords["nucleicbackbone"] = new NucleicBackboneSelection(this, "nucleicbackbone", 0);
        selectionKeywords["nucleicbase"] = new NucleicBaseSelection(this, "nucleicbase", 0);
        selectionKeywords["nucleicsugar"] = new NucleicSugarSelection(this, "nucleicsugar", 0);

        selectionKeywords["prop"] = new PropertySelection(this, "prop", 0);

        selectionKeywords["around"] = new AroundSelection(this, "around", 1);
        selectionKeywords["within"] = new AroundSelection(this, "within", 1);

        selectionKeywords["insphere"] = new InSphereSelection(this, "insphere", 0);


        operatorKeywords["and"] = new AndOperation(this, "and", 3);
        operatorKeywords["or"] = new OrOperation(this, "or", 3);


        UnityMolSelectionManager selM = UnityMolMain.getSelectionManager();
        foreach (string keyw in selM.selectionMDAKeywords.Keys) {
            selectionKeywords[keyw] = new SelectionSelection(this, selM.selectionMDAKeywords[keyw], 0);
        }

        //This should be after selection because if a selection has the name of a structure, this overwrites it
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        foreach (UnityMolStructure s in sm.loadedStructures) {
            string name = s.uniqueName;
            selectionKeywords[name] = new StructureNameSelection(this, name, 0);
        }


    }

    public UnityMolSelection process() {
        try {
            MetaClass tmp = slP.Parse(selectionStr, input);
            List<UnityMolAtom> tmpRes = tmp.apply(input).ToList();
            UnityMolSelection result = new UnityMolSelection(tmpRes, "selection", selectionStr);
            return result;
        }
        catch (System.Exception e) {
            Debug.LogError("Could not parse selection expression\n" + e);
            return null;
        }
    }

    public bool isKeyword(string val) {
        if (selectionKeywords.ContainsKey(val))
            return true;
        if (operatorKeywords.ContainsKey(val))
            return true;
        if (val == "(" || val == ")" || val == " ")
            return true;
        return false;
    }
    public bool isOperator(string s) {
        return operatorKeywords.ContainsKey(s);
    }

    public List<string> getNotKeywords(List<string> tokens, ref int curId ) {
        // getNotKeywords(['H', 'Ca', 'N', 'and','resname', 'MET'])
        // => ['H', 'Ca' ,'N']
        List<string> result = new List<string>();
        for (; curId < tokens.Count; curId++) {
            string t = tokens[curId];
            if (!isKeyword(t)) {
                result.Add(t);
            }
            else {
                break;
            }
        }
        return result;
    }

    public static bool isProtein(UnityMolAtom a) {
        return ProteinSelection.knownResidues.Contains(a.residue.name);
    }
    public static bool isProtein(UnityMolResidue r) {
        return ProteinSelection.knownResidues.Contains(r.name);
    }
    public static bool isNucleic(UnityMolAtom a) {
        return NucleicSelection.knownNucleic.Contains(a.residue.name);
    }
    public static bool isNucleic(UnityMolResidue r) {
        return NucleicSelection.knownNucleic.Contains(r.name);
    }

    public static bool isBackBone(UnityMolAtom a, UnityMolBonds bonds) {
        if (ProteinSelection.knownResidues.Contains(a.residue.name)) {
            if (a.type != "H" && a.type != "CA") {
                return BackboneSelection.knownBases.Contains(a.name);
            }
            if (bonds != null) {
                return isBondedToBackbone(a, bonds);
            }
        }
        if (NucleicSelection.knownNucleic.Contains(a.residue.name)) {
            if (a.type != "H") {
                return NucleicBackboneSelection.knownNucleicBBBases.Contains(a.name);
            }
            if (bonds != null) {
                return isBondedToBackbone(a, bonds);
            }
        }
        return false;

        // return (ProteinSelection.knownResidues.Contains(a.residue.name) && BackboneSelection.knownBases.Contains(a.name)) ||
        // (NucleicSelection.knownNucleic.Contains(a.residue.name) && NucleicBackboneSelection.knownNucleicBBBases.Contains(a.name));
    }

    public static bool isBondedToBackbone(UnityMolAtom a, UnityMolBonds bonds) {
        bool isBondedToBB = false;

        //Check if hydrogen linked to a backbone atom
        if (bonds.bondsDual.ContainsKey(a)) {
            UnityMolAtom[] bonded = bonds.bondsDual[a];
            for (int i = 0; i < bonded.Length; i++) {
                if (bonded[i] != null) {
                    if (BackboneSelection.knownBases.Contains(bonded[i].name)) {
                        isBondedToBB = true;
                        break;
                    }
                    if (NucleicBackboneSelection.knownNucleicBBBases.Contains(bonded[i].name)) {
                        isBondedToBB = true;
                        break;
                    }
                }
            }
        }
        return isBondedToBB;
    }
    public static bool isSideChain(UnityMolAtom a, UnityMolBonds bonds) {
        if (ProteinSelection.knownResidues.Contains(a.residue.name)) {
            if (a.type != "H" && a.type != "CA") {
                return !BackboneSelection.knownBases.Contains(a.name);
            }

            return !isBondedToBackbone(a, bonds);
        }
        return false;


        // return (ProteinSelection.knownResidues.Contains(a.residue.name) && !BackboneSelection.knownBases.Contains(a.name)) ||
        // (NucleicSelection.knownNucleic.Contains(a.residue.name) && !NucleicBackboneSelection.knownNucleicBBBases.Contains(a.name));
    }
}

public class SelectionParser {
    //      For reference, the grammar that we parse is ::
    //     E(xpression)--> Exp(0)
    //     Exp(p) -->      P {B Exp(q)}
    //     P -->           U Exp(q) | "(" E ")" | v
    //     B(inary) -->    "and" | "or"
    //     U(nary) -->     "not"
    //     T(erms) -->     segid [value]
    //                     | resname [value]
    //                     | resid [value]
    //                     | name [value]
    //                     | type [value]
    // """
    //  # Borg pattern: http://aspn.activestate.com/ASPN/Cookbook/Python/Recipe/66531

    private MDAnalysisSelection mdaSel;

    public SelectionParser(MDAnalysisSelection mda) {
        mdaSel = mda;
    }


    private List<string> tokens;
    public int curToken = 0;

    private void expect(string s) {
        if (s == tokens[curToken]) {
            curToken++;
        }
        else {
            throw new System.Exception("Unexpected token in selection : '" + s + "'");
        }
    }

    public MetaClass Parse(string selStr, HashSet<UnityMolAtom> group) {
        selStr = selStr.Replace("(", " ( ").Replace(")", " ) ");
        tokens = selStr.Split(new [] {' '}, System.StringSplitOptions.RemoveEmptyEntries).ToList();
        // tokens.Add(" ");
        MetaClass parseTree = parseExpression(0);
        // if (curToken < tokens.Count ) {
        // throw new System.Exception("Unexpected token at end of selection : '" + tokens[curToken] + "'");
        // }
        return parseTree;
    }

    public MetaClass parseExpression(int p) {

        MetaClass exp1 = parseSubExpression();

        while (curToken < tokens.Count && mdaSel.isOperator(tokens[curToken]) && mdaSel.operatorKeywords[tokens[curToken]].precedence >= p) {
            Operation op = (Operation)mdaSel.operatorKeywords[tokens[curToken++]];


            int q = 1 + op.precedence;
            MetaClass exp2 = parseExpression(q);
            op.leftSel = exp1;
            op.rightSel = exp2;

            exp1 = op.Clone();
        }
        return exp1;
    }

    private MetaClass parseSubExpression() {
        MetaClass exp;
        string op = tokens[curToken++];
        if (op == "(") {
            exp = parseExpression(0);
            expect("(");
            return exp;
        }
        try {
            MDASelection res = mdaSel.selectionKeywords[op];
            res.init(tokens, ref curToken);
            return res.Clone();
        }
        catch (System.Exception e) {
            throw new System.Exception("MDASelection failed " + e);
        }
    }
}



//-------------------------------------------------------------------------
public abstract class MetaClass {

    public MDAnalysisSelection mdaSel;
    public string token;
    public int precedence;

    public abstract HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group);
    public MetaClass() {

    }
    public MetaClass(MetaClass cpy) : this() {
        this.mdaSel = cpy.mdaSel;
        this.token = cpy.token;
        this.precedence = cpy.precedence;
    }
    public abstract MetaClass Clone();
}


//-------------------------------------------------------------------------

public abstract class Operation : MetaClass {

    public MetaClass leftSel;
    public MetaClass rightSel;


    public Operation(MDAnalysisSelection mda, string t, int p) {
        mdaSel = mda;
        token = t;
        precedence = p;
        // mda.addToOperation(t);
    }
    public Operation (Operation cpy) : base(cpy) {
    }
}

public class AndOperation : Operation {

    public AndOperation(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
    }
    public AndOperation (AndOperation cpy) : base(cpy) {
        this.leftSel = cpy.leftSel.Clone();
        this.rightSel = cpy.rightSel.Clone();
    }

    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {

        HashSet<UnityMolAtom> lsel = leftSel.apply(group);
        HashSet<UnityMolAtom> rsel = rightSel.apply(group);

        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();

        foreach (UnityMolAtom a in rsel) {
            if (lsel.Contains(a)) {
                result.Add(a);
            }
        }
        return result;
    }
    public override MetaClass Clone() { return new AndOperation(this);}

}


public class OrOperation : Operation {

    public OrOperation(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {

    }
    public OrOperation (OrOperation cpy) : base(cpy) {
        this.leftSel = cpy.leftSel.Clone();
        this.rightSel = cpy.rightSel.Clone();
    }
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> rsel = rightSel.apply(group);
        HashSet<UnityMolAtom> lsel = leftSel.apply(group);

        HashSet<UnityMolAtom> result = rsel;
        result.UnionWith(lsel);
        return result;
    }
    public override MetaClass Clone() { return new OrOperation(this);}

}


//-------------------------------------------------------------------------


public abstract class MDASelection : MetaClass {


    public MDASelection(MDAnalysisSelection mda, string t, int p) {
        mdaSel = mda;
        token = t;
        precedence = p;
        // mdaSel.selectionKeywords[token] = this;
    }
    public abstract void init(List<string> tokens, ref int curId);

    public MDASelection(MDASelection cpy) : base(cpy) {

    }



}


public class AllSelection : MDASelection {

    public AllSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {

    }
    public AllSelection(AllSelection cpy) : base(cpy) {}

    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        return group;
    }
    public override void init(List<string> tokens, ref int curId) {
    }
    public override MetaClass Clone() { return new AllSelection(this);}
}
public class NothingSelection : MDASelection {

    public NothingSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {

    }
    public NothingSelection(NothingSelection cpy) : base(cpy) {}

    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        return new HashSet<UnityMolAtom>();
    }
    public override void init(List<string> tokens, ref int curId) {
    }
    public override MetaClass Clone() { return new NothingSelection(this);}
}



public class UnarySelection : MDASelection {
    public MetaClass sel;
    public UnarySelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
    }
    public override void init(List<string> tokens, ref int curId) {
        sel = mdaSel.slP.parseExpression(precedence);
    }
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        //To redefine
        return group;
    }

    public UnarySelection (UnarySelection cpy) : base(cpy) {
        this.sel = cpy.sel.Clone();
    }
    public override MetaClass Clone() { return new UnarySelection(this);}

}

public class NotSelection : UnarySelection {
    public NotSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
    }

    public NotSelection(NotSelection cpy) : base(cpy) {}

    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();

        HashSet<UnityMolAtom> notSel = sel.apply(group);

        foreach (UnityMolAtom a in group) {
            if (!notSel.Contains(a)) {
                result.Add(a);
            }
        }
        return result;
    }
    public override MetaClass Clone() { return new NotSelection(this);}


}


public class ByResSelection : UnarySelection {

    public ByResSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
    }
    public ByResSelection(ByResSelection cpy) : base(cpy) {}

    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {

        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();
        HashSet<UnityMolResidue> doneRes = new HashSet<UnityMolResidue>();
        HashSet<UnityMolAtom> atoms = sel.apply(group);
        foreach (UnityMolAtom a in atoms) {
            if (!doneRes.Contains(a.residue)) {
                foreach (UnityMolAtom ares in a.residue.atoms.Values) {
                    result.Add(ares);
                }
                doneRes.Add(a.residue);
            }
        }
        return result;
    }

    public override MetaClass Clone() { return new ByResSelection(this);}

}

public class RangeResidSelection : MDASelection {
    public int valueOffset = 0;
    public List<int> uppers;
    public List<int> lowers;

    public RangeResidSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
        uppers = new List<int>();
        lowers = new List<int>();
    }
    public override void init(List<string> tokens, ref int curId) {
        List<string> values = mdaSel.getNotKeywords(tokens, ref curId);

        if (values.Count == 0) {
            throw new System.Exception("Unexpected token in selection : '" + tokens[curId] + "'");
        }

        foreach (string val in values) {
            int lower = -1;
            int upper = int.MaxValue;
            try {
                lower = int.Parse(val);
            }
            catch (System.Exception e) {
                if (val.Contains(":")) {
                    string[] splitted = val.Split(new [] {':'}, System.StringSplitOptions.RemoveEmptyEntries);
                    if (splitted.Length == 2) {
                        lower = int.Parse(splitted[0]);
                        upper = int.Parse(splitted[1]);
                    }
                    else {
                        throw new System.Exception("Unexpected token in selection : '" + val + "'\n" + e);
                    }
                }
                else {
                    throw new System.Exception("Unexpected token in selection : '" + val + "'\n" + e);
                }
            }
            lowers.Add(lower);
            uppers.Add(upper);
        }
    }
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();

        foreach (UnityMolAtom a in group) {
            for (int i = 0; i < lowers.Count; i++) {
                if (uppers[i] == int.MaxValue) {
                    if ( a.residue.id == lowers[i]) {
                        result.Add(a);
                    }
                    continue;
                }
                if ( a.residue.id >= lowers[i] && a.residue.id <= uppers[i]) {
                    result.Add(a);
                }
            }
        }
        return result;
    }

    public RangeResidSelection (RangeResidSelection cpy) : base(cpy) {
        this.valueOffset = cpy.valueOffset;
        this.uppers = cpy.uppers;
        this.lowers = cpy.lowers;
    }
    public override MetaClass Clone() { return new RangeResidSelection(this);}

}

public class RangeAtomIdSelection : MDASelection {
    public int valueOffset = 0;
    public List<int> uppers;
    public List<int> lowers;

    public RangeAtomIdSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
        uppers = new List<int>();
        lowers = new List<int>();
    }
    public override void init(List<string> tokens, ref int curId) {
        List<string> values = mdaSel.getNotKeywords(tokens, ref curId);

        if (values.Count == 0) {
            throw new System.Exception("Unexpected token in selection : '" + tokens[curId] + "'");
        }

        foreach (string val in values) {
            int lower = -1;
            int upper = int.MaxValue;
            try {
                lower = int.Parse(val);
            }
            catch (System.Exception e) {
                if (val.Contains(":")) {
                    string[] splitted = val.Split(new [] {':'}, System.StringSplitOptions.RemoveEmptyEntries);
                    if (splitted.Length == 2) {
                        lower = int.Parse(splitted[0]);
                        upper = int.Parse(splitted[1]);
                    }
                    else {
                        throw new System.Exception("Unexpected token in selection : '" + val + "'\n" + e);
                    }
                }
                else {
                    throw new System.Exception("Unexpected token in selection : '" + val + "'\n" + e);
                }
            }
            lowers.Add(lower);
            uppers.Add(upper);
        }
    }
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();

        foreach (UnityMolAtom a in group) {
            for (int i = 0; i < lowers.Count; i++) {
                if (uppers[i] == int.MaxValue) {
                    if ( a.number == lowers[i]) {
                        result.Add(a);
                    }
                    continue;
                }
                if ( a.number >= lowers[i] && a.number <= uppers[i]) {
                    result.Add(a);
                }
            }
        }
        return result;
    }

    public RangeAtomIdSelection (RangeAtomIdSelection cpy) : base(cpy) {
        this.valueOffset = cpy.valueOffset;
        this.uppers = cpy.uppers;
        this.lowers = cpy.lowers;
    }
    public override MetaClass Clone() { return new RangeAtomIdSelection(this);}

}


public class StringSelection : MDASelection {

    public List<string> vals;
    public string field;
    public bool suite = true;

    public StringSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
    }

    public override void init(List<string> tokens, ref int curId) {
        if (!suite) {
            vals = new List<string>();
            vals.Add(token);
            return;
        }
        List<string> values = mdaSel.getNotKeywords(tokens, ref curId);
        if (values.Count == 0) {
            throw new System.Exception("Unexpected token in selection : '" + tokens[curId] + "'");
        }
        vals = values;

    }

    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();

        foreach (string val in vals) {
            int wildCardPos = val.IndexOf("*");
            if (wildCardPos == -1) {
                switch (field) {
                case "names":
                    foreach (UnityMolAtom a in group) {
                        if (a.name == val) {
                            result.Add(a);
                        }
                    }
                    break;
                case "types":
                    foreach (UnityMolAtom a in group) {
                        if (a.type == val) {
                            result.Add(a);
                        }
                    }
                    break;
                case "resnames":
                    foreach (UnityMolAtom a in group) {
                        if (a.residue.name == val) {
                            result.Add(a);
                        }
                    }
                    break;
                case "chainnames":
                    foreach (UnityMolAtom a in group) {
                        if (a.residue.chain.name == val) {
                            result.Add(a);
                        }
                    }
                    break;
                case "modelname":
                    foreach (UnityMolAtom a in group) {
                        if (a.residue.chain.model.name == val) {
                            result.Add(a);
                        }
                    }
                    break;
                case "structnames":
                    foreach (UnityMolAtom a in group) {
                        if (a.residue.chain.model.structure.uniqueName == val) {
                            result.Add(a);
                        }

                    }
                    // UnityMolStructureManager sm = UnityMolMain.getStructureManager();
                    // foreach(UnityMolStructure s in sm.loadedStructures){

                    // }
                    break;
                case "selectionname":
                    UnityMolSelection sel = UnityMolMain.getSelectionManager().selections[val];

                    HashSet<UnityMolAtom> selAtoms = new HashSet<UnityMolAtom>( sel.atoms );
                    foreach (UnityMolAtom a in group) {
                        if (selAtoms.Contains(a)) {
                            result.Add(a);
                        }
                    }
                    break;
                default:
                    break;
                }
            }
            else { //Wildcard
                string valwc = val.Substring(0, wildCardPos);
                switch (field) {
                case "names":
                    foreach (UnityMolAtom a in group) {
                        if (a.name.StartsWith(valwc)) {
                            result.Add(a);
                        }
                    }
                    break;
                case "types":
                    foreach (UnityMolAtom a in group) {
                        if (a.type.StartsWith(valwc)) {
                            result.Add(a);
                        }
                    }
                    break;
                case "resnames":
                    foreach (UnityMolAtom a in group) {
                        if (a.residue.name.StartsWith(valwc)) {
                            result.Add(a);
                        }
                    }
                    break;
                case "chainnames":
                    foreach (UnityMolAtom a in group) {
                        if (a.residue.chain.name.StartsWith(valwc)) {
                            result.Add(a);
                        }
                    }
                    break;
                case "modelname":
                    foreach (UnityMolAtom a in group) {
                        if (a.residue.chain.model.name.StartsWith(valwc)) {
                            result.Add(a);
                        }
                    }
                    break;
                case "structnames":
                    foreach (UnityMolAtom a in group) {
                        if (a.residue.chain.model.structure.uniqueName.StartsWith(val)) {
                            result.Add(a);
                        }
                    }
                    break;
                default:
                    break;
                }
            }
        }
        return result;
    }


    public StringSelection (StringSelection cpy) : base(cpy) {
        this.vals = cpy.vals;
        this.field = cpy.field;
        this.suite = cpy.suite;
    }
    public override MetaClass Clone() { return new StringSelection(this);}

}

public class AtomNameSelection : StringSelection {
    public AtomNameSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
        field = "names";
    }
}
public class StructureNameSelection : StringSelection {
    public StructureNameSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
        suite = false;
        field = "structnames";
    }
}

public class AtomTypeNameSelection : StringSelection {
    public AtomTypeNameSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
        field = "types";
    }
}

public class ResidueNameSelection : StringSelection {
    public ResidueNameSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
        field = "resnames";
    }
}
public class ChainNameSelection : StringSelection {
    public ChainNameSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
        field = "chainnames";
    }
}
public class ModelNameSelection : StringSelection {
    public ModelNameSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
        field = "modelname";
    }
}

public class SelectionSelection : StringSelection {
    public SelectionSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
        suite = false;
        field = "selectionname";
    }
}

public class SecondaryStructureSelection : MDASelection {

    public string ssType;

    static HashSet<string> ssTypes = new HashSet<string> {
        "helix", "coil", "sheet", "S", "H", "C"
    };

    public SecondaryStructureSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {}

    public SecondaryStructureSelection(SecondaryStructureSelection cpy) : base(cpy) {
        this.ssType = cpy.ssType;
    }

    public override void init(List<string> tokens, ref int curId) {
        List<string> values = mdaSel.getNotKeywords(tokens, ref curId);
        if (values.Count != 1) {
            throw new System.Exception("Unexpected token in selection : '" + tokens[curId] + "'");
        }
        if (!ssTypes.Contains(values[0])) {
            throw new System.Exception("Unexpected secondary structure type");
        }
        ssType = values[0];
    }

    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();
        foreach (UnityMolAtom a in group) {
            UnityMolResidue.secondaryStructureType ss = a.residue.secondaryStructure;
            switch (ss) {
            case UnityMolResidue.secondaryStructureType.Helix:
            case UnityMolResidue.secondaryStructureType.HelixRightOmega:
            case UnityMolResidue.secondaryStructureType.HelixRightPi:
            case UnityMolResidue.secondaryStructureType.HelixRightGamma:
            case UnityMolResidue.secondaryStructureType.Helix310:
            case UnityMolResidue.secondaryStructureType.HelixLeftAlpha:
            case UnityMolResidue.secondaryStructureType.HelixLeftOmega:
            case UnityMolResidue.secondaryStructureType.HelixLeftGamma:
            case UnityMolResidue.secondaryStructureType.Helix27:
                if (ssType == "helix" || ssType == "H") {
                    result.Add(a);
                }
                break;

            case UnityMolResidue.secondaryStructureType.Strand:
            case UnityMolResidue.secondaryStructureType.StrandA:
                if (ssType == "S" || ssType == "sheet") {
                    result.Add(a);
                }
                break;

            default://Coil / turn / bridge / bend...
                if (ssType == "coil" || ssType == "C") {
                    result.Add(a);
                }
                break;
            }
        }
        return result;
    }

    public override MetaClass Clone() { return new SecondaryStructureSelection(this);}

}

//Add MoleculeTypeSelection => add type to UnityMolStructure for protein/dna/rna...


public class DistanceSelection : MDASelection {

    public bool useKDtree = true;
    //TODO MDAnalysis is managing the periodic conditions, we should too
    public DistanceSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
    }
    public override void init(List<string> tokens, ref int curId) {
    }
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        return group;
    }


    public DistanceSelection (DistanceSelection cpy) : base(cpy) {
        this.useKDtree = cpy.useKDtree;
    }
    public override MetaClass Clone() { return new DistanceSelection(this);}

}


public class AroundSelection : DistanceSelection {

    public float cutoff = 0.0f;
    public MetaClass sel;

    public AroundSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
    }

    public override void init(List<string> tokens, ref int curId) {
        cutoff = float.Parse(tokens[curId++], System.Globalization.CultureInfo.InvariantCulture);

        sel = mdaSel.slP.parseExpression(precedence);
    }

    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> tmpSel = sel.apply(group);
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();

        if (tmpSel.Count == 0 || cutoff < 0.001f) {
            return result;
        }

        Transform loadedMol = UnityMolMain.getRepresentationParent().transform;

        var allPoints = new NativeArray<float3>(group.Count, Allocator.Persistent);
        var qPoints = new NativeArray<float3>(tmpSel.Count, Allocator.Persistent);

        int ida = 0;
        foreach (UnityMolAtom a in group) {
            allPoints[ida++] = loadedMol.InverseTransformPoint(a.curWorldPosition);
        }
        ida = 0;

        foreach (UnityMolAtom a in tmpSel) {
            //Transform points in the framework of the loadedMolecules transform
            qPoints[ida++] = loadedMol.InverseTransformPoint(a.curWorldPosition);
        }

        int maxRes = (int)((Mathf.PI * 4 * cutoff * cutoff) * 0.35f);//Assuming 0.35 atom per Angstrom^3
        float start = Time.realtimeSinceStartup;
#if True
        var knnContainer = new KnnContainer(allPoints, true, Allocator.TempJob);

        // Store a list of particles in range
        var rangeResults = new NativeArray<RangeQueryResult>(qPoints.Length, Allocator.TempJob);

        // Unfortunately, for batch range queries we do need to decide upfront the maximum nr. of neighbours we allow
        // This is due to limitation on allocations within a job.
        for (int i = 0; i < rangeResults.Length; ++i) {
            rangeResults[i] = new RangeQueryResult(maxRes, Allocator.TempJob);
        }

        // Fire up job to get results for all points
        var batchRange = new QueryRangeBatchJob(knnContainer, qPoints, cutoff, rangeResults);

        // And just run immediately now. This will run on multiple threads!
        batchRange.ScheduleBatch(qPoints.Length, qPoints.Length / 32).Complete();

        HashSet<int> res = new HashSet<int>();
        for (int i = 0; i < rangeResults.Length; i++) {
            for (int j = 0; j < rangeResults[i].Length; j++) {
                res.Add(rangeResults[i][j]);
            }
        }
        // Debug.Log("Time for within : " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");

        // Now the results array contains all the neighbours!
        foreach (var r in rangeResults) {
            r.Dispose();
        }
        rangeResults.Dispose();
        knnContainer.Dispose();

        //Do not use the optimized grid based implementation for now because of a Unity bug
        // NeighborSearchGridBurst nsgb = new NeighborSearchGridBurst();
        // HashSet<int> tmp = nsgb.searchInRadius(allPoints, qPoints, cutoff);

        UnityMolAtom[] groupAtoms = group.ToArray();

        foreach (int id in res) {
            result.Add(groupAtoms[id]);
        }

#elif False

        var results = WithinBurst.getAtomsWithin(allPoints, qPoints, cutoff);
        UnityMolAtom[] groupAtoms = group.ToArray();

        for (int i = 0; i < groupAtoms.Length; i++) {
            if (results[i] == 1) {
                result.Add(groupAtoms[i]);
            }
        }
        Debug.Log("Time for withinburst: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");
        results.Dispose();

#elif False

        NeighborSearchGridBurst nsgb = new NeighborSearchGridBurst();
        List<int> tmp = nsgb.searchInRadius(allPoints, qPoints, cutoff);

        UnityMolAtom[] groupAtoms = group.ToArray();

        for (int i = 0; i < tmp.Count; i++) {
            result.Add(groupAtoms[tmp[i]]);

        }
        Debug.Log("Time for grid based within: " + (1000.0f * (Time.realtimeSinceStartup - start)).ToString("f3") + " ms");

#endif
        allPoints.Dispose();
        qPoints.Dispose();

        return result;

    }

    public HashSet<UnityMolAtom> applyold(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> tmpSel = sel.apply(group);
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();

        Transform loadedMol = UnityMolMain.getRepresentationParent().transform;

        List<UnityMolAtom> groupAtoms = group.ToList();
        Vector3[] points = new Vector3[groupAtoms.Count];
        int id = 0;
        //Transform points in the framework of the loadedMolecules transform
        foreach (UnityMolAtom a in groupAtoms) {
            points[id++] = loadedMol.InverseTransformPoint(a.curWorldPosition);
        }

        KDTree tmpKDTree = KDTree.MakeFromPoints(points);

        foreach (UnityMolAtom a in tmpSel) {
            int[] ids = tmpKDTree.FindNearestsRadius(loadedMol.InverseTransformPoint(a.curWorldPosition) , cutoff);
            for (int i = 0; i < ids.Length; i++) {
                result.Add(groupAtoms[ids[i]]);
            }
        }
        return result;
    }

    public AroundSelection (AroundSelection cpy) : base(cpy) {
        this.sel = cpy.sel.Clone();
        this.cutoff = cpy.cutoff;
    }
    public override MetaClass Clone() { return new AroundSelection(this);}
}


public class InSphereSelection : DistanceSelection {

    public float cutoff = 0.0f;
    public Vector3 center = Vector3.zero;
    // public MetaClass sel;

    public InSphereSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {
    }

    public override void init(List<string> tokens, ref int curId) {
        center.x = float.Parse(tokens[curId++], System.Globalization.CultureInfo.InvariantCulture);
        center.y = float.Parse(tokens[curId++], System.Globalization.CultureInfo.InvariantCulture);
        center.z = float.Parse(tokens[curId++], System.Globalization.CultureInfo.InvariantCulture);
        cutoff = float.Parse(tokens[curId++], System.Globalization.CultureInfo.InvariantCulture);
    }
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();

        List<UnityMolAtom> groupAtoms = group.ToList();
        KDTree tmpKDTree = KDTree.MakeFromUnityMolAtoms(groupAtoms);
        //Get all atoms inside the sphere
        int[] ids = tmpKDTree.FindNearestsRadius(center , cutoff);
        foreach (int id in ids) {
            result.Add(groupAtoms[id]);
        }
        return result;
    }

    public InSphereSelection (InSphereSelection cpy) : base(cpy) {
        // this.sel = cpy.sel.Clone();
        this.cutoff = cpy.cutoff;
        this.center = cpy.center;
    }
    public override MetaClass Clone() { return new InSphereSelection(this);}
}


public class ProteinSelection : MDASelection {

    //Based on recognized residue names
    public static HashSet<string> knownResidues = new HashSet<string> {
        // # CHARMM top_all27_prot_lipid.rtf
        "ALA", "ARG", "ASN", "ASP", "CYS", "GLN", "GLU", "GLY", "HSD",
        "HSE", "HSP", "ILE", "LEU", "LYS", "MET", "PHE", "PRO", "SER", "THR",
        "TRP", "TYR", "VAL", "ALAD",
        // ## "CHO","EAM", # -- special formyl and ethanolamine termini of gramicidin
        // # PDB
        "HIS", "MSE",
        // # from Gromacs 4.5.3 oplsaa.ff/aminoacids.rtp
        "ARGN", "ASPH", "CYS2", "CYSH", "QLN", "PGLU", "GLUH", "HIS1", "HISD",
        "HISE", "HISH", "LYSH",
        // # from Gromacs 4.5.3 gromos53a6.ff/aminoacids.rtp
        "ASN1", "CYS1", "HISA", "HISB", "HIS2",
        // # from Gromacs 4.5.3 amber03.ff/aminoacids.rtp
        "HID", "HIE", "HIP", "ORN", "DAB", "LYN", "HYP", "CYM", "CYX", "ASH",
        "GLH", "ACE", "NME",
        // # from Gromacs 2016.3 amber99sb-star-ildn.ff/aminoacids.rtp
        "NALA", "NGLY", "NSER", "NTHR", "NLEU", "NILE", "NVAL", "NASN", "NGLN",
        "NARG", "NHID", "NHIE", "NHIP", "NTRP", "NPHE", "NTYR", "NGLU", "NASP",
        "NLYS", "NPRO", "NCYS", "NCYX", "NMET", "CALA", "CGLY", "CSER", "CTHR",
        "CLEU", "CILE", "CVAL", "CASF", "CASN", "CGLN", "CARG", "CHID", "CHIE",
        "CHIP", "CTRP", "CPHE", "CTYR", "CGLU", "CASP", "CLYS", "CPRO", "CCYS",
        "CCYX", "CMET", "CME", "ASF"
    };


    public ProteinSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {}
    public ProteinSelection(ProteinSelection cpy) : base(cpy) {}

    public override void init(List<string> tokens, ref int curId) {}
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();
        foreach (UnityMolAtom a in group) {
            if (knownResidues.Contains(a.residue.name)) {
                result.Add(a);
            }
        }
        return result;
    }
    public override MetaClass Clone() { return new ProteinSelection(this);}

}


public class NucleicSelection : MDASelection {

    //Based on recognized residue names
    public static  HashSet<string> knownNucleic = new HashSet<string> {
        "ADE", "URA", "CYT", "GUA", "THY", "DA", "DC", "DG", "DT", "RA",
        "RU", "RG", "RC", "A", "T", "U", "C", "G",
        "DA5", "DC5", "DG5", "DT5",
        "DA3", "DC3", "DG3", "DT3",
        "RA5", "RU5", "RG5", "RC5",
        "RA3", "RU3", "RG3", "RC3"
    };


    public NucleicSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {}
    public NucleicSelection(NucleicSelection cpy) : base(cpy) {}

    public override void init(List<string> tokens, ref int curId) {}
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();
        foreach (UnityMolAtom a in group) {
            if (knownNucleic.Contains(a.residue.name)) {
                result.Add(a);
            }
        }
        return result;
    }
    public override MetaClass Clone() { return new NucleicSelection(this);}

}

public class BackboneSelection : MDASelection {

    //Based on recognized residue names
    public static  HashSet<string> knownBases = new HashSet<string> {
        "N", "C", "CA", "O",
    };


    public BackboneSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {}
    public BackboneSelection(BackboneSelection cpy) : base(cpy) {}

    public override void init(List<string> tokens, ref int curId) {}
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();
        foreach (UnityMolAtom a in group) {
            //Is protein and is backbone
            if (ProteinSelection.knownResidues.Contains(a.residue.name) && knownBases.Contains(a.name) && a.type != "CA") {
                result.Add(a);
            }
        }
        return result;
    }
    public override MetaClass Clone() { return new BackboneSelection(this);}


}


public class NucleicBackboneSelection : MDASelection {

    //Based on recognized residue names
    public static  HashSet<string> knownNucleicBBBases = new HashSet<string> {
        "P", "C5'", "C3'", "O3'", "O5'"
    };


    public NucleicBackboneSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {}
    public NucleicBackboneSelection(NucleicBackboneSelection cpy) : base(cpy) {}

    public override void init(List<string> tokens, ref int curId) {}
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();
        foreach (UnityMolAtom a in group) {
            //Is nucleic and is backbone
            if (NucleicSelection.knownNucleic.Contains(a.residue.name) && knownNucleicBBBases.Contains(a.name)) {
                result.Add(a);
            }
        }
        return result;
    }
    public override MetaClass Clone() { return new NucleicBackboneSelection(this);}


}

public class NucleicBaseSelection : MDASelection {

    //Based on recognized residue names
    public static  HashSet<string> knownNucleicBases = new HashSet<string> {
        "N9", "N7", "C8", "C5", "C4", "N3", "C2", "N1", "C6",
        "O6", "N2", "N6",
        "O2", "N4", "O4", "C5M"
    };


    public NucleicBaseSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {}
    public NucleicBaseSelection(NucleicBaseSelection cpy) : base(cpy) {}

    public override void init(List<string> tokens, ref int curId) {}
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();
        foreach (UnityMolAtom a in group) {
            //Is nucleic and is base
            if (NucleicSelection.knownNucleic.Contains(a.residue.name) && knownNucleicBases.Contains(a.name)) {
                result.Add(a);
            }
        }
        return result;
    }

    public override MetaClass Clone() { return new NucleicBaseSelection(this);}

}

public class NucleicSugarSelection : MDASelection {

    //Based on recognized residue names
    public static  HashSet<string> knownNucleicSugarAtoms = new HashSet<string> {
        "C1'", "C2'", "C3'", "C4'", "O4'"
    };


    public NucleicSugarSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {}
    public NucleicSugarSelection(NucleicSugarSelection cpy) : base(cpy) {}


    public override void init(List<string> tokens, ref int curId) {}
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();
        foreach (UnityMolAtom a in group) {
            //Is nucleic and is sugar
            if (NucleicSelection.knownNucleic.Contains(a.residue.name) && knownNucleicSugarAtoms.Contains(a.name)) {
                result.Add(a);
            }
        }
        return result;
    }
    public override MetaClass Clone() { return new NucleicSugarSelection(this);}

}

public class WaterSelection : MDASelection {

    //Based on recognized residue names
    public static HashSet<string> waterResidues = new HashSet<string> {
        "HOH", "WAT", "SOL", "TIP3", "TP3M", "SPC", "H2O", "TIP"
    };


    public WaterSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {}
    public WaterSelection(WaterSelection cpy) : base(cpy) {}

    public override void init(List<string> tokens, ref int curId) {}
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();
        foreach (UnityMolAtom a in group) {
            if (waterResidues.Contains(a.residue.name, StringComparer.OrdinalIgnoreCase) ) {
                result.Add(a);
            }
        }
        return result;
    }
    public override MetaClass Clone() { return new WaterSelection(this);}
}

public class IonsSelection : MDASelection {

    //Adapted from https://gist.github.com/jbarnoud/37a524330f29b5b7b096

    //Based on recognized residue names
    public static  HashSet<string> ionNames = new HashSet<string> {
        "AL", "AS", "AU", "BE", "BR", "CO", "CU", "EU", "FE", "GD", "IR",
        "MG", "MN", "MO", "NI", "PT", "RH", "RU", "SE", "TA", "ZN"
        // "AG", "AL", "AS", "AU", "BE", "BR", "CA", "CD", "CL", "Cl", "CO", "CR", "CS",
        // "CU", "EU", "F", "FE", "Fe", "GD", "HG", "HO", "I", "IR", "K", "KR", "LI",
        // "MG", "Mg", "MN", "MO", "NA", "Na", "NI", "NH4", "OS", "PB", "PO4", "PT", "RH", "RU", "SE",
        // "SOD", "SO4", "SR", "TA", "V", "YB", "ZN", "Zn"
    };

    public static  HashSet<string> ionResidueNames = new HashSet<string> {
        "PO4", "SOD", "SO4", "NH4"
    };

    public static HashSet<string> specialHG = new HashSet<string> {
        "CMH", "EMC", "MBO", "MMC", "HGB", "BE7", "PMB"
    };

    public static HashSet<string> specialCL = new HashSet<string> {
        "0QE", "CPT", "DCE", "EAA", "IMN", "OCZ", "OMY", "OMZ",
        "UN9", "1N1", "2T8", "393", "3MY", "BMU", "CLM", "CP6",
        "DB8", "DIF", "EFZ", "LUR", "RDC", "UCL", "XMM", "HLT",
        "IRE", "LCP", "PCI", "VGH"
    };

    public IonsSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {}
    public IonsSelection(IonsSelection cpy) : base(cpy) {}

    public override void init(List<string> tokens, ref int curId) {}
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();
        foreach (UnityMolAtom a in group) {
            if (a.name.Length == 1) {
                if (a.name == "I" || a.name == "F" || a.name == "K" || a.name == "V" || a.name == "S") {
                    result.Add(a);
                }
            }
            else if (a.name.Length >= 2) {
                string aname2 = a.name.Substring(0, 2).ToUpper();
                if (ionNames.Contains(aname2) || aname2 == a.residue.name ||
                        ionResidueNames.Contains(a.residue.name) ||
                        (aname2 == "HG" && specialHG.Contains(a.residue.name)) ||
                        (aname2 == "CL" && specialCL.Contains(a.residue.name)) ) {
                    result.Add(a);
                }
            }
        }
        return result;
    }
    public override MetaClass Clone() { return new IonsSelection(this);}
}


public class LigandSelection : MDASelection {


    public LigandSelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {}
    public LigandSelection(LigandSelection cpy) : base(cpy) {}

    public override void init(List<string> tokens, ref int curId) {}
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {
        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();
        foreach (UnityMolAtom a in group) {
            if (a.isLigand) {
                result.Add(a);
            }
        }
        return result;
    }
    public override MetaClass Clone() { return new LigandSelection(this);}
}

public class PropertySelection : MDASelection {

    public string proper;
    public float thevalue;
    public string theoperator;


    static HashSet<string> acceptedProp = new HashSet<string>() {"x", "y", "z"};
    static HashSet<string>  opSymbols = new HashSet<string> () {"<=", ">=", "==", "!=", "<", ">"};
    static Dictionary<string, string>  oppositeSymbols = new Dictionary<string, string>() {
        {"==", "=="},
        {"!=", "!="},
        {"<", ">="},
        {">=", "<"},
        {">", "<="},
        {"<=", ">"}
    };
    static HashSet<string> properties = new HashSet<string>() {"x", "y", "z"};//{'mass', 'charge', 'x', 'y', 'z'};


    //TODO implement this !
    public PropertySelection(MDAnalysisSelection mda, string t, int p) : base(mda, t, p) {}
    public PropertySelection(PropertySelection cpy) : base(cpy) {
        this.proper = cpy.proper;
        this.theoperator = cpy.theoperator;
        this.thevalue = cpy.thevalue;
    }


    public override void init(List<string> tokens, ref int curId) {
        string pr = tokens[curId++];
        string oper = null;
        string val = null;

        foreach (string possible in opSymbols) {
            if (pr.Contains(possible)) {
                string[] xy = pr.Split(new [] {possible}, System.StringSplitOptions.RemoveEmptyEntries);
                pr = xy[0];
                oper = possible;
                if (xy.Length == 2) {
                    oper += xy[1];
                }
                break;
            }
        }

        if (oper == null) {
            oper = tokens[curId++];
        }


        foreach (string possible in opSymbols) {
            if (oper.Contains(possible)) {
                string[] xy = oper.Split(new [] {possible}, System.StringSplitOptions.RemoveEmptyEntries);
                if (xy.Length == 1 && xy[0] != "") {
                    oper = possible;
                    val = xy[0];
                }
                break;
            }
        }

        if (val == null) {
            val = tokens[curId++];
        }

        if (properties.Contains(val)) {
            string tmpprop = pr;
            pr = val;
            val = tmpprop;
            oper = oppositeSymbols[oper];
        }
        proper = pr;

        try {
            theoperator = oper;
        }
        catch {
            throw new System.Exception("Invalid operator");
        }
        thevalue = float.Parse(val, System.Globalization.CultureInfo.InvariantCulture);

    }
    public override HashSet<UnityMolAtom> apply(HashSet<UnityMolAtom> group) {

        HashSet<UnityMolAtom> result = new HashSet<UnityMolAtom>();

        if (acceptedProp.Contains(proper)) {
            if (proper == "x") {
                foreach (UnityMolAtom a in group) {
                    if (applyOp(theoperator, a.position.x, thevalue)) {
                        result.Add(a);
                    }
                }
            }
            else if (proper == "y") {
                foreach (UnityMolAtom a in group) {
                    if (applyOp(theoperator, a.position.y, thevalue)) {
                        result.Add(a);
                    }
                }
            }
            else if (proper == "z") {
                foreach (UnityMolAtom a in group) {
                    if (applyOp(theoperator, a.position.z, thevalue)) {
                        result.Add(a);
                    }
                }
            }
        }
        else {
            throw new System.Exception("Invalid property " + proper);
        }




        return result;
    }

    public override MetaClass Clone() { return new PropertySelection(this); }

    public static bool applyOp(string logic, float x, float y)
    {
        switch (logic)
        {
        case ">": return x > y;
        case "<": return x < y;
        case "==": return x == y;
        case "=!": return x != y;
        case ">=": return x >= y;
        case "<=": return x <= y;

        default: throw new System.Exception("Invalid logic");
        }
    }
}
}