$Id: README 23 2014-12-09 21:37:11Z baaden $

This is UnityMol/SVN revision $Rev: 676 aka 0.9.3 $.
This document is still work in progress.

(Sweet) UnityMol - Readme
=========================

Sweet UnityMol is a molecular viewer and prototyping platform for the Unity3D game engine with specific extensions for sugar molecules as described in [a recent publication (Perez et al., Glycobiology 2014)][sweetumolpaper]. It is developped by Marc Baaden's team in Paris based on previous work ([Lv et al., 2013][umolpaper]) and includes [HyperBalls][hbpaper]
designed to visualize molecular structures using GPU graphics card capabilities based on shaders (GLSL or Cg).
It can read Protein Data Bank (PDB) files, Cytoscape networks, OpenDX maps and Wavefront OBJ meshes.

All Sweet UnityMol functionality is integrated in more recent UnityMol releases.

More information on <http://unitymol.sourceforge.net>

Features
--------

* SugarRibbons, RingBlending and more sugar-related visual representations
* Lit sphere artistic texturing of surfaces and HyperBalls
* Screen space effects from Unity (Blur, Glow, ..)
* Animated electric field lines
* (Simple) surfaces with cut planes
* specific representations for carbohydrates and polysaccharides
* description to be completed soon

Scene organization
-------------------
* Molecule.unity: Full UnityMol application. Only works as standalone or in the editor.
* MainMenu.unity: Main Menu with buttons to load pre-defined scenes in a webplayer context.
* 1KX2.unity: pre-defined scene loading 1KX2.pdb from Scenes/1KX2/
* Fieldines.unity: pre-defined scene loading fieldline.pdb/.json/.obj from Scenes/fieldlines/
* Network.unity: pre-defined scene loading a CytoScape network
* FromPDB.unity: pre-defined scene fetching a PDB file corresponding to the PDB id entered in the Main Menu (no validation for the moment)

If a scene is not loaded correctly, no error message is displayed for the moment. Try to go back to the Main Menu (Open->Main menu) and load another scene.

Scene creation
--------------
- Let's create a scene called Toto:

	Copy Molecule.unity into Toto.unity
	Create a C# script named ScenePreload_Toto.cs in Assets/Scripts/. The class must inherit from MonoBehaviour.
	You need a method with the following signature:
	```
		IEnumerator InitScene(RequestPDB requestPDB)
		{

		}
	```
	Inspire yourself from ScenePreload_Fieldlines or ScenePreload_1KX2 to initialize the scene with the content you want.
	Be sure to be in the scene Toto in Unity and select the LoadBox object in the hierarchy.
	Then just add ScenePreload_Toto.cs as a component of LoadBox.

The method InitScene will be called after Molecule3D and the GUI are initialized.
Normally you can initialize everything and change any flag from here.
UIData.server_url will be www.shaman.ibpc.fr/umolweb in the Editor and "." otherwise.


About the HyperBalls shaders
----------------------------
The shaders are working great for OpenGL versions of Unity (MacOS, Windows with the -force-opengl flag).
If you want to modify these shaders and have them work in Direct3D you have to follow this procedure:

	Make your changes in the OpenGL version of the shaders: BallImproved.shader and StickImproved.shader.
	Let Unity compile them. It will throw some errors. Select the shader in the project tree and click on "Open compiled shader" in the inspector.
	Copy the content of this file and paste it into Ball_HyperBalls_D3D.shader or Stick_HyperBalls_D3D.shader respectively.
	Then replace the occurences of "oDepth.z" by "oDepth" . There is only one in the original shader.
	Change the name of the shader at the top of the file to "FvNano/Ball HyperBalls D3D" or "FvNano/Stick HyperBalls D3D".

UnityMol is able to detect the platform it runs on and choose the appropriate shaders.


Citations
---------
* Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden: [Game on, Science - how video game technology may help biologists tackle visualization challenges][umolpaper], PLoS ONE (2013) 8(3):e57990.

* S. Perez, T. Tubiana, A. Imberty and M. Baaden: [Three-Dimensional Representations of Complex Carbohydrates and Polysaccharides. SweetUnityMol: A Video Game Based Computer Graphic Software][sweetumolpaper], Glycobiology (2014).

Related work
------------
* M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert, B. Raffin, M. Baaden: [GPU-accelerated atom and dynamic bond visualization using HyperBalls, a unified algorithm for balls, sticks and hyperboloids][hbpaper]; J Comput Chem (2011) 32, 2924.
* M. Chavent, B. Levy, M. Krone, K. Bidmon, J. P. Nomine, T. Ertl and M. Baaden: [GPU-powered tools boost molecular visualization][gpupaper]; Brief Bioinformatics (2011)


Copyright and License
================================================
This program is under the [CeCill-C licence][cecill], which is compatible with the LGPL licence.


<!-- REFERENCES -->
[umol]: http://unitymol.sourceforge.net "http://unitymol.sourceforge.net"
[umolweb]: http://www.shaman.ibpc.fr/umolweb "http://www.shaman.ibpc.fr/umolweb/"
[hb]: http://hyperballs.sourceforge.net "http://hyperballs.sourceforge.net"
[cecill]: http://www.cecill.info/index.en.html "CeCill licence"
[umolpaper]: http://dx.doi.org/10.1371/journal.pone.0057990 "http://dx.doi.org/10.1371/journal.pone.0057990"
[sweetumolpaper]: http://dx.doi.org/10.1093/glycob/cwu133 "http://dx.doi.org/10.1093/glycob/cwu133"
[hbpaper]: http://dx.doi.org/10.1002/jcc.21861 "http://dx.doi.org/10.1002/jcc.21861"
[gpupaper]: http://dx.doi.org/10.1093/bib/bbq089 "http://dx.doi.org/10.1093/bib/bbq089"
[markdown]: http://daringfireball.net/projects/markdown/ "http://daringfireball.net/projects/markdown/"
[cpkwikip]: http://en.wikipedia.org/wiki/CPK_coloring "http://en.wikipedia.org/wiki/CPK_coloring"
[gdbweb]: http://about.gremedy.com "http://about.gremedy.com"
