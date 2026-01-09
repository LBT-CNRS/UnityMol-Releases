# UnityMol Changelog

## [current dev master]

## [1.2.0]

- Improve errors handling with external libraries/binaries
- Drop Meshlab & MeshSmoothing support
- Regroup Wrappers for external libraries/binaries in a single folder
- Add missing libraries/binaries for various platforms
- Fix bugs in dropdown Representations menu (now always on front + scrollable) & Reorganization
- Command line option "-r" to start ZMQ server after launch
- ZMQ commands server sends back detailed results and stdout
- Fix application crash when failing to parse floats in input files (coordinates for examples)
- Add XML Documentation for all API functions
- annotations are now updated when changing manually (button or slider) frames of a trajectory
- Correct  the last frame number in the trajectory slider to avoid warning
- Added a threaded XDR trajectory reader
- Added a setRepSizes function to the API
- Fix keyboard issue with control keys on Mac
- Add Multiplayer feature with Photon library.
- Bump to Unity 2021
- Update VIU to 1.19.0
- Switch to new XR management
- Remove custom SMAA (since Unity handle it now)
- Remove VR packages from dependencies
- Refactor PDB & PDBx parser
- Add a fetch_URL() API function which can fetch any structure file from any URL.
- Update Point Representation for Metal
- New binaries for external plugins compatible with ARM architecture (for Mac Silicon)
- Define common python functions available from the Python console (can be loaded through the API or the CLI)
- Improve version management (based on git tag/describe) and use semantic version.
- Fix missing one triangle for surface mesh with cutSurface option
- Rewrite of GRO parser (now supports correctly multiples frames)
- Fix color by atom for the surface after adding atoms to a structure
- Improving adding atoms to a structure (now can be reorder)
- Fix the parsing of script file when blocks were defined inside it
- Fix mismatch width between tube and sphere for line representation
- Change RCSB server URL for retrieving PDB
- Update MDDriver plugin library to v1.0 (issue #60)
- Fix MDDriver related bugs and improve error handling
- Fix crash related to hydrogens and surfaces (issue #61)
- Serialization of the scene, available from API
- Speed up colors changes for selection
- Fixed several minor errors such as memory leaks and on surface mesh, chains, raytracing, parsers
- update to NSGB version 1.0.6 which fixed bugs
- Fix insphere search bug by replacing KNN with NSGB
- Replace KNN by NSGB for hbond detection
- Entirely remove KNN from the project
- Fix error potentially occurring when adding hydrogens
- Fix bug with coordinate update when loading a trajectory
- Changelog history complete in the main repository
- Add CUDA Quicksurf DLL
- Allow larger hyberball meshes
- Improve lightning for HB/HS shaders
- Remove having one GameObject per atom
- Implement spatial search
- Add depth cueing update when moving molecules


## [1.1.3] - 2021-02-10

- [Feature] Add <ins>**orthographic camera mode**</ins>
- [Feature] <ins>**Serialization**</ins>: export the current UnityMol state to a JSON file (selections, representations, annotations) !
- [VR|Feature] LeapMotion support re-added
- [Feature] <ins>**Tour feature**</ins>: long click on a selection to add it to the tour animation
- [Feature] <ins>**Call python functions each frame of a trajectory**</ins>: ```last().NewFrameDeleg += myPythonfunc```
- [Feature] Energy plots for IMD
- [Feature] Each line of the console can be clicked to copy to clipboard
- [Feature] LeftAlt + click to display information about atom (like double right click)
- [Feature] Add color by resid and resnum + Update UI
- [Feature] Outline color can be changed
- [Feature] Limited view for cartoon
- [Feature] Center on selection now has a customizable time to zoom
- [Feature|Experimental] Grid view (pymol like)
- [Feature] Remote command from python using ZMQ module (call ```activateExternalCommands()``` to enable)
- [Feature] UnityMolMain.iversion returns 113 for 1.1.3
- [Feature] Export Ambient Occlusion when exporting FBX and OBJ files
- [Feature] Follow a selection during a trajectory (desktop only), ```getManipulationManager().followSelection = "selName"```
- [VR|Feature] <ins>**New VR scenes added**</ins>
- [VR|Fix] <ins>**Performance**</ins> issues caused by rendering at 2 times the resolution
- [Fix] Improve cartoon normals
- [Fix] Autocompletion works for larger paths
- [Fix] Annotations, sphere annotation size + world annotation warnings
- [Fix] Adding hydrogens with Reduce or Haad caused incorrect atom ids
- [Fix] MDDriver disconnect fixes
- [Fix] Double click to zoom should behave better
- [Fix] Empty lines are ignored when loading python scripts
- [Fix] Line representation trajectory + updating selection caused errors
- [Fix] Trajectory performance issue at the end of the file
- [Fix] Unloading dx map
- [Fix] Simplify AO computation
- Disable surface threads for large molecules (>50k)
- Depth of Field effect now follows the selected atom during the trajectory


## [1.1.2] - 2020-10-14

- [⚠️] Change the default selection "all(1kx2)" to "all_1kx2"
- [Feature] Average trajectory with a sliding window
- [Feature] Add python standard modules (random/urllib/zlib...) still no numpy :(
- [Feature] Measurements in desktop mode by pressing "M" and cliking on atoms
- [Feature] Double click selection name to center the view
- [Feature] UI for annotations
- [Feature] Set bounding box line size
- [Feature] Use **MDDriver** instead of Artemis for IMD
- [Feature] Add a function to set trajectory frame: setTrajFrame()
- [Fix] centerOnStructure now fills the screen with the selection
- [Fix] Reduce / Haad did not work when UnityMol was in a folder with spaces
- [Fix] Inverted IMD coordinates !
- [Fix] Transparent surfaces are now using a dither shader
- [Fix] UI now shows if all representations of a molecule are shown/hidden
- [Fix] Saving/restoring the history of commands at launch/quit
- [Docking] Fix parsing forcefield
- UI now uses various events to be updated
- Improve manipulation behaviour when stopping an animation

## [1.1.1] - 2020-09-09

- [Feature] Biological assembly can now be fetched by doing ```fetch("1BTA", bioAssembly= True)```
- [Feature] Native notifications: ```NativeNotifications.Notify("Message")``` / ```NativeNotifications.AskYesNo("Question")``` / ```NativeNotifications.AskContinue("Question")```
- [Feature] Add 2D text annotation: ```annotate2DText(Vector2(0.5, 0.5), scale= 1.0, "Text", Color.white)```
- [Feature] Cubic interpolation for trajectory
- [Feature] Add a function to unhide selection representations
- [Feature] Hyperball parameter switch with interpolation: ```setHyperBallMetaphore("licorice", lerp= True)```
- [Feature] Get current parent positions/rotations with ```loadedMolParentToString(addToHistory= True)```
- [Feature] API functions to pause/unpause a video recording
- [VR] Drop obsolete VRTK plugin, we now move to VIU (https://github.com/ViveSoftware/ViveInputUtility-Unity)
- [VR] Range selection with long press
- [VR] Vibrations when hovering UI buttons
- [Docking] Enable/disable custom colliders based on atom names
- [Docking] Fix recording docking state to PDB
- [Fix] exporting hyperball without cubes
- [Fix] updating empty selections during trajectory when a UnityMolSelection has ```updateContentWithTraj = True```
- [Fix] mouse interaction when dragging from UI to scene
- [Parser] Improve atom type recognition
- [Parser] Fix a GRO parser bug for atom serial
- [Parser] Fix a PDB parser bug for negative residue ids
- [Parser] Fix a PDB parser bug when using modelAsTraj and alternative positions
- [Parser] Fix PDB/PSF/TOP/XML parsers when more than 27 bonds per atom
- [IMD] Artemis plugin is now compiled without spamming the log file
- [IMD] Fix distance pull with mouse
- Various improvements when recording videos/screenshot (antialiasing + post-process effects)
- Outline now affects transparent surfaces
- Improved logo: https://drive.google.com/file/d/1XEGGiQK20CN1WwIm02lX9TTrabeAcWDi/preview
- Faster line update during trajectory reading/IMD
- Use SharpZipLib to avoid .Net gzip issues
- .ent extension added to file browser
- Fix AO for surfaces when ```UnityMolMain.disableSurfaceThread = True```
- Text annotation orientation fix
- Stop camera movements when interacting with the scene before the animation is done
- Various other bug fixes




## [1.1.0] - 2020-06-29

- Wireframe shader for all platforms
- Transparent cartoon
- Improve line representations visual
- Add a PLY mesh exporter
- Export hyperballs to meshes (WIP)
- Read/Write large PDB/mmCIF/GRO files
- Add a way to translate or rotate only the clicked molecule in desktop mode
- Parse Tuffery rotamer library
- Set light parameters from API
- Add function to play a sonar sound at world position
- Add functions to wait for N frames or N seconds from scripts (yield waitSeconds / yield waitFrames)
- Autocomplete paths
- XTC trajectory reader for WebGL
- Add functions to change cartoon tube size / draw as tube + bfactor
- Add function to select inside a box
- Add functions to get and set chi torsion angles
- Add option to not record every action to console
- Add input fields to sliders
- Disable AO for VDW by default + Add a button to compute AO

- Fix trajectory UI for VR scene
- Improve PDB/mmCIF/GRO parsers -> faster + less memory
- UnityMolBonds now uses integers instead of UnityMolAtom -> faster + less memory
- Fix UI scaling + allocate way less memory per selection + faster creation
- Fix a lot of memory leaks (Unity meshes and materials)
- Improve PDB parser: resnum is used instead of the parsed residue id + use hybrid36 parsing norm
- Fix precision hyperboloid shrink factor precision

## [1.0.39] - 2020-04-27

- Read PSF or TOP files to import a topology (only bonding info is read)
- Change default selections & representations
- Fix parenthesis in selection language
- Import multi-line python script fix
- Fix PDB writer sometimes causing issues with EDTSurf/Reduce/haad
- Change UnityMolStructure.uniqueName to UnityMolStructure.name
- A lot of "quality of life" improvements, shortcuts, UI...
- Add shortcuts to show/hide the console and the UI: CTRL+Space / CTRL+Backspace
- Add power saving mode (can cause black screen on Mac)
- Add customUnityMolColors.txt file in StreamingAssets/ folder to easily add new atom colors/radius
- Add UI for post-processing effects (DepthOfField/Outline/DepthCueing)
- Remove + and - operators for selections
- Can now recompute secondary structure information during a trajectory (structure.updateSSWithTraj)
- Export OBJ files with exportRepresentationsToOBJFile function
- Depth cueing works for all the representations/shaders
- Improve atom type recognition
- Improve autocompletion
- Improve automatic selection query content
- Renamed centerOfGravity to centroid

## [1.0.37] - 2020-04-07

- Trajectory picker
- Support for non-orthogonal DX maps
- Partial support for MARTINI files
- Support for large XTC trajectories, not fully loaded in memory
- Limited view for iso-surfaces (enableLimitedView/setLimitedViewCenter/setLimitedViewRadius functions)
- Load bonds from XML file (loadBondsXML function)
- Add bond order representation based on hyperballs
- New representations (Point/HireRNA ellipsoids/Alpha trace/Sheherasade/bond order)
- PDB/GRO/mmCIF multi-model files are read as a trajectory by default
- Save representation state when changing models or selection content
- Drag&Drop support for molecular and history script files
- Pymol sessions to UnityMol commands script [here](https://github.com/LBT-CNRS/PymolToUnityMol)
- A LOT of bug fixes and performance improvements

## [1.0.36]

- Bond order representation (WIP)
- Parse XML bonds for covalent and non-covalent bonds (WIP)
- Fast implementation of "within" selection
- CEAlign
- Fix bounding boxes for hyperball during trajectory
- Export SS info to PDB
- Save/Restore representation parameters (WIP)
- Fix IMD force clear
- Drag and Drop now supports different file types
- Bug fixes

## [1.0.34] - 2019-10-14

- Separated threads to compute molecular surfaces at load time
- Ultra-fast cartoon representation
- Ambient Occlusion for hyperballs in Van der Waals representation
- Add a new representation "Sheherasade"
- Parse mmCIF connectivity
- Fix some UI + sound issues
- Performance improvements (fieldlines/trajectory smoother/raycast/center of gravity -> compiled with Burst)
- Command line arguments (example 'UnityMol.exe -i 1kx2 pdbs/1crn.pdb')
- Fix recording videos in VR
- IMD should work on Mac and Linux now
- Fix PDB writer when residue name is too large
