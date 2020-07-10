<img src="Assets/Resources/Logo/LogoUnityMol1.0_outline.png" width="300">


# <ins>Install **git lfs** before cloning ! </ins>

## UnityMol 1.0.36 public version

This is the public version of UnityMol 1.0.36 released (july 2020) with ABPS-UnityMol paper: <i>Visualizing biomolecular electrostatics in virtual reality with UnityMol-APBS </i> (10.1002/pro.3773)

APBS integration was done by Joseph Laurenti.

Working on Unity 2019.4.x LTS, tested on Windows/Mac/Linux.

----------

## License

<b> This is UnityMol open-source version licensed under the LGPL-3.0 (see under) </b>

<b> For commercial use, UnityMol proposes a commercial license that can be negociated by contacting us at unitymol@gmail.com</b>

<b> For more details please read [LICENCE file](LICENSE.md).

[![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](https://www.gnu.org/licenses/lgpl-3.0)

----------

## Binaries

Recent UnityMol versions can be found on [sourceforge](https://sourceforge.net/projects/unitymol/)

----------

## UnityMol 1.0 Features (vs 0.9.x versions)

- clean data structure / code base with a high level API, unit tests, doxygen documentation
- VR capabilities with VRTK (Oculus/Vive/Windows Mixed Reality HMDs/...)
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

<img src="https://owncloud.galaxy.ibpc.fr/owncloud/index.php/s/ER0EbdQd9rTthUb/download" width="300">

-----------

## How to use

- Clone the repo with git lfs installed and open the project using latest version of Unity 2019.4.x
- MainUIScene.unity is the main scene for desktop / MainScene_VR_Desktop.unity is the VR scene that has a switch to turn VR on/off
- To load and display a molecule from a script, use UMol.API.APIPython functions:

```csharp
using UnityEngine;
using UMol;
using UMol.API;

public class loadMolecule : MonoBehaviour {
	void Start() {
		UnityMolStructure s = APIPython.fetch("1KX2");//This fetches the file from the PDB
		UnityMolStructure s2 = APIPython.fetch("PDBs/3eam.cif");//This loads a local file

		UnityMolSelection sel = APIPython.select("3eam and resid 1:10 and chain A", "chainA");//Create a selection
		APIPython.showSelection("chainA", "hyperball");//Show the selection as hyperball

		APIPython.delete(s.name);//Remove a loaded molecule

		//Use more APIPython functions here
	}
}
```
- When loading molecules the "LoadedMolecules" object will be created if it doesn't exist and all selections of the molecules will be created under it. The global scale is set by changing this LoadedMolecules object.
- In desktop mode, the camera does not move, the mocules are moving !
- For VR: There are a lot of scripts for VR molecular interactions, check "[VRTK_Scripts]" child objects in the VR scene. Grabbing molecules is based on a custom raycasting implementation (CustomRaycast). The VR camera is different for each HMD type, check "[VRTK_SDKManager]" object based on the VRTK framework.
- Anti-aliasing is done using SMAA script because hyperball shaders do not support it. You can disable MSAA.
- Check documentation for more information on the data structure and details on the implementation

-----------

## Platforms

- UnityMol is mainly developped on Windows and Mac but should work on Linux
- Partially works on WebGL and Android ergo Oculus Quest.
- A collaborative prototype was done using 2 Hololens v1
- VR on Windows only, tested on HTC Vive / Vive Pro / Oculus DK2 & CV1 & Rift S / Oculus Quest (linked) / Microsoft Mixed Reality HMDs
- All HMDs supporting SteamVR should work

-------------------------------------
### the UnityMol team
