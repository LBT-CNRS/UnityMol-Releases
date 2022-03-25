# UnityMol 1.0.36 source code for APBS paper (http://www-lbt.ibpc.fr/node/2718)
--------------------

## APBS

The integration of APBS tools was mainly done by Joseph Laurenti from Nathan Baker's group at PNNL.

First, install PDB2PQR and APBS (https://sourceforge.net/projects/pdb2pqr/  &  https://sourceforge.net/projects/apbs/), for example with "C:/APBS_PDB2PQR/apbs/bin/apbs.exe" & "C:/APBS_PDB2PQR/pdb2pqr/pdb2pqr.exe". Set executable path by clicking the first button of the PDB2PQR menu, UnityMol will look in sub directories for apbs and pdb2pqr binaries.

Note that some steps will load another molecular file and hide the previous one.

APBS tools are not binded to APIPython functions so you cannot script APBS calls for now.

## Python console

This is implemented with IronPython, an interpreter for Python 2.7 in C#. You can use most of standard library python modules but not numpy of cython.

Wiki page: Python-Console.md

## MDAnalysis selection language

Wiki page: MDAnalysisSelectionLanguage.md

Please refer to MDAnalysis documentation ([here](https://www.mdanalysis.org/docs/documentation_pages/selections.html)) for a detailed explanation about the language and some examples.

The language is not fully implemented, also some keywords have been added.

## Things you need to know

- This version is a snapshot, the developpement continued, a lot of bug fixes, perf improvements, features were developed in the meantime.
- Dev was done on Windows and Mac.
- Some shaders do not work correctly on Metal so make sure you use OpenGL on mac
- There is a bug on Unity for Mac (before 2020.2.0a12) that makes an empty Unity project consume a lot of CPU resources even when the focus is lost. This is not UnityMol code causing it.
- Raycasting is done using a custom engine without using PhysX that caused a lot of slow downs for medium to large molecules.
- Do NOT activate adaptive rendering for Oculus... Rendering bugs


## Unpublished features removed from this release

- Fast cartoon parallel Burst code
- Support for coarse grain models
