# UnityMol 1.0.36 public version

<img src="Assets/Resources/Logo/LogoUnityMol1.0_outline.png" width="300">


This is the public version of UnityMol 1.0.36 source files released (July 2020) with ABPS-UnityMol paper: **Visualizing biomolecular electrostatics in virtual reality with UnityMol-APBS** ([10.1002/pro.3773](https://doi.org/10.1002/pro.3773))

APBS integration was done by Joseph Laurenti.


## UnityMol 1.0 Features

- clean data structure / code base with a high level API, unit tests, doxygen documentation
- Virtual Reality (VR) capabilities with VRTK (Oculus/Vive/Windows Mixed Reality HMDs/...)
- __a python console__ to script all actions in UnityMol
- save/load commands to save/restore the current scene
- advanced selections using MDAnalysis language
- fast hyperball representation
- classical molecular representations cartoon/surface (EDTSurf/MSMS)/line/fieldlines/
- covalent bonds based on OpenMM topology definition [here](https://raw.githubusercontent.com/pandegroup/openmm/master/wrappers/python/simtk/openmm/app/data/residues.xml)
- molecular file parsers: PDB/mmCIF/GRO/MOL2/SDF/XYZ
- trajectory reader: XTC | Trajectory smoother built-in
- MDAnalysis version of libxdrfile -> low memory usage even for large trajectories
- object space ambient occlusion (AO) for surfaces and VDW hyperballs
- new Unity UI system (faster than 0.9.x versions)

## Binaries

Recent UnityMol versions can be found on [sourceforge](https://sourceforge.net/projects/unitymol/)

## Requirements

UnityMol is based on the game engine [Unity](https://unity.com).

This version is working on **Unity 2019.4.21f1 LTS**, tested on Windows/Mac/Linux. Main developpement is done on Windows.

### Known Bugs
 - Some shaders do not work correctly on Metal so make sure you use OpenGL on Mac.
 - There is a bug on Unity for Mac (before 2020.2.0a12) that makes an empty Unity project consume a lot of CPU resources even when the focus is lost. This is not UnityMol code causing it.
 - Do not activate adaptive rendering for Oculus... Rendering bugs.


## How to use

- Clone the repo with **git lfs installed** and open the project using the 2019.4.21f1 version of Unity.
- `MainUIScene.unity` is the main scene for desktop.
- `MainScene_VR_Desktop.unity` is the Virtual Reality (VR) scene that has a switch to turn VR on/off.

### APBS

The integration of APBS tools was mainly done by Joseph Laurenti from Nathan Baker's group at PNNL.

First, install PDB2PQR and APBS (https://sourceforge.net/projects/pdb2pqr/  &  https://sourceforge.net/projects/apbs/), for example with "C:/APBS_PDB2PQR/apbs/bin/apbs.exe" & "C:/APBS_PDB2PQR/pdb2pqr/pdb2pqr.exe".
Set executable path by clicking the first button of the PDB2PQR menu, UnityMol will look in sub directories for apbs and pdb2pqr binaries.

Note that some steps will load another molecular file and hide the previous one.

APBS tools are not binded to APIPython functions so you cannot script APBS calls for now.


### Additional configuration for the VR scene

For the VR scene to work, the VR headset must be first configured and functional with SteamVR.

UnityMol uses VRTK 3.x that is limited to the SteamVR plugin version 1.2.3, you must stick to this version (already imported in the project).

The scene lack of a skybox due to the large size of the image file needed. You can find nice examples on this website https://polyhaven.com/. For instance, we usually use this image https://polyhaven.com/a/noon_grass (version 4K).

To add it to the scene:
  - Save the chosen image in the Assets folder.
  - In the Editor, edit the following parameters in the Inspector view:
    - `Shape` -> `cube`
    - `Max Size` -> `4096`
    - Tick `Use Crunch Compression`
  - Choose the texture in the `Cubemap` option of the `skyhdr` material (under `Assets/Ressources/Materiels`)
  - Generate LightMaps (Menu `Windows`->`Rendering`->`Lightning Settings`)


## Ressources

 - The website of UnityMol is here: http://unitymol.sourceforge.net
 - a tutorial can be found here: https://nezix.github.io/
 - UnityMol provides a Python API to access functions from the console. The documentation is here: [UMolAPI](UMolAPI.md)


## Features

### Python console

This is implemented with IronPython, an interpreter for Python 2.7 in C#.
You can use most of standard library python modules but not numpy of cython.


### MDAnalysis selection language

UnityMol implements an atom selection language,  based on the [MDAnalysis](https://www.mdanalysis.org/) one.
Please refer to MDAnalysis documentation ([here](https://www.mdanalysis.org/docs/documentation_pages/selections.html)) for a detailed explanation about the language and some examples.

The language is not fully implemented, also some keywords have been added.

### Scripting

To load and display a molecule from a script, use `UMol.API.APIPython` functions:

```csharp
using UnityEngine;
using UMol;
using UMol.API;

public class loadMolecule : MonoBehaviour {
	void Start() {
		UnityMolStructure s = APIPython.fetch("1KX2"); //This fetches the file from the PDB
		UnityMolStructure s2 = APIPython.fetch("PDBs/3eam.cif"); //This loads a local file

		UnityMolSelection sel = APIPython.select("3eam and resid 1:10 and chain A", "chainA"); //Create a selection
		APIPython.showSelection("chainA", "hyperball"); //Show the selection as hyperball

		APIPython.delete(s.name); //Remove a loaded molecule

		//Use more APIPython functions here
	}
}
```

Same commands from the python console:

```python
s = fetch("1KX2")
s2 = load("PDBs/3eam.cif")
sel = select("3eam and resid 1:10 and chain A", "chainA")
showSelection("chainA", "hb")
delete(s.name)
```

### Notes

- This version is a snapshot, UnityMol is still in continuous development.
- When loading molecules the `LoadedMolecules` object will be created if it doesn't exist and all selections of the molecules will be created under it. The global scale is set by changing this `LoadedMolecules` object.
- In desktop mode, the camera does not move, the mocules are moving!
- For VR: There are a lot of scripts for VR molecular interactions, check `[VRTK_Scripts]` child objects in the VR scene. Grabbing molecules is based on a custom raycasting implementation (`CustomRaycast`). The VR camera is different for each HMD type, check `[VRTK_SDKManager]` object based on the VRTK framework.
- Anti-aliasing is done using SMAA script because hyperball shaders do not support it. You can disable MSAA.
- Raycasting is done using a custom engine without using PhysX that caused a lot of slow downs for medium to large molecules.

## License

UnityMol is **dual-licensed** under the LGPL-3.0 (see under). However, all external assets used in UnityMol are under a permissive open-source license: MIT/Apache-2.0/BSD-3.0

For commercial use, UnityMol can be licensed under a custom license. Please contact unitymol@gmail.com

[![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](https://www.gnu.org/licenses/lgpl-3.0)


## the UnityMol team






