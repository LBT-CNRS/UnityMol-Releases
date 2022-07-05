# List of commands available through the Python console

```python
### Load a local molecular file (pdb/mmcif/gro/mol2/sdf/xyz formats)
load(filePath: str, readHetm: bool = True, forceDSSP: bool = False, showDefaultRep: bool = True, center: bool = True)
```

```python
### Load a molecular file (pdb/mmcif/gro/mol2/sdf/xyz formats) from a string
loadFromString(fileName: str, fileContent: str, readHetm: bool = True, forceDSSP: bool = False, showDefaultRep: bool = True, center: bool = True)
```

```python
### Fetch a remote molecular file (pdb or mmcif zipped)
fetch(PDBId, usemmCIF: bool = True, readHetm: bool = True, forceDSSP: bool = False, showDefaultRep: bool = True, center = True)
```

```python
loadMartiniITP(structureName: str, filePath: str)
```


```python
### Delete all the loaded molecules
reset()
```

```python
### Switch between parsed secondary structure information and DSSP computation
switchSSAssignmentMethod(structureName: str, forceDSSP: bool = False)
```

```python
### Show/Hide hydrogens in representations of the provided selection
### This only works for lines, hyperball and sphere representations
showHideHydrogensInSelection(selName: str)
```

```python
### Show/Hide side chains in representations of the current selection
### This only works for lines, hyperball and sphere representations only
showHideSideChainsInSelection(selName: str)
```

```python
### Show/Hide backbone in representations of the current selection
### This only works for lines, hyperball and sphere representations only
showHideBackboneInSelection(selName: str)
```

```python
### Set the current model of the structure
### This function is used by ModelPlayers.cs to read the models of a structure like a trajectory
setModel(structureName: str, modelId: int)
```

```python
### Load a trajectory for a loaded structure
### It creates a XDRFileReader in the corresponding UnityMolStructure and a TrajectoryPlayer
loadTraj(structureName: str, path: str)
```

```python
### Unload a trajectory for a specific structure
unloadTraj(structureName: str)
```

```python
### Load a density map for a specific structure
### This function creates a DXReader instance in the UnityMolStructure
loadDXmap(structureName: str, path: str)
```

```python
### Unload the density map for the structure
unloadDXmap(structureName: str)
```

```python
### Read a json file and display fieldLines for the specified structure
readJSONFieldlines(structureName: str, path: str)
```

```python
### Remove the json file for fieldlines stored in the currentModel of the specified structure
unloadJSONFieldlines(structureName: str)
```

```python
### Change fieldline computation gradient threshold
setFieldlineGradientThreshold(selName: str, val: float)
```

```python
### Utility function to be able to get the group of the structure
### This group is used to be able to move all the loaded molecules in the same group
### Groups can be between 0 and 9 included
getStructureGroup(structureName: str)
```

```python
### Utility function to be able to get the structures of the group
### This group is used to move all the loaded molecules in the same group
### Groups can be between 0 and 9 included
getStructuresOfGroup(group: int)
```

```python
### Utility function to be set the group of a structure
### This group is used to be able to move all the loaded molecules in the same group
### Groups can be between 0 and 9 included
setStructureGroup(structureName: str, newGroup: int)
```

```python
### Delete a molecule and all its UnityMolSelection and UnityMolRepresentation
delete(structureName: str)
```

```python
### Show as 'type' all loaded molecules
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
show(type: str)
```

```python
### Show all loaded molecules only as the 'type' representation
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
showAs(type: str)

### Create selections and default representations: all in cartoon, not protein in hyperballs
### Also create a selection containing "not protein and not water and not ligand and not ions"
defaultRep(selName: str)
```

```python
### Create default representations (cartoon for protein + HB for not protein atoms)
showDefault(selName: str)
```

```python
### Unhide all representations already created for a specified structure
showStructureAllRepresentations(structureName: str)
```

```python
### Show the selection as 'type'
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
### If the representation is already there, update it if the selection content changed and show it
### Surface example: showSelection("all(1kx2)", "s", True, True, True, SurfMethod.MSMS)
### Iso-surface example: showSelection("all(1kx2)", "dxiso", last().dxr, 0.0f)
showSelection(selName: str, type: str, args)
```

```python
### Hide every representations of the specified selection
hideSelection(selName: str)
```

```python
### Hide every representation of type 'type' of the specified selection
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
hideSelection(selName: str, type: str)
```

```python
### Delete every representations of type 'type' of the specified selection
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
deleteRepresentationInSelection(selName: str, type: str)
```

```python
### Delete every representations of the specified selection
deleteRepresentationsInSelection(selName: str)
```

```python
### Hide every representations of the specified structure
hideStructureAllRepresentations(structureName: str)
```

```python
### Utility function to test if a representation is shown for a specified structure
areRepresentationsOn(structureName: str)
```

```python
### Utility function to test if a representation of type 'type' is shown for a specified selection
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
areRepresentationsOn(selName: str, type: str)
```

```python
### Hide all representations of type 'type'
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
hide(type: str)
```

```python
### Switch between the 2 types of surface computation methods: EDTSurf and MSMS
switchSurfaceComputeMethod(selName: str)
```

```python
### Switch between cut surface mode and no-cut surface mode
switchCutSurface(selName: str, isCut: bool)
```

```python
### Switch all surface representation in selection to a solid surface material
setSolidSurface(selName: str)
```

```python
### Switch all surface representation in selection to a wireframe surface material when available
setWireframeSurface(selName: str)
```

```python
### Switch all surface representation in selection to a transparent surface material
setTransparentSurface(selName: str, alpha: float = 0.8)
```

```python
### Recompute the DX surface with a new iso value
updateDXIso(selName: str, newVal: float)
```

```python
### Change hyperball representation parameters in the specified selection to a preset
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
setSmoothness(selName: str, type: str, val: float)
```

```python
### Change hyperball representation parameters in the specified selection to a preset
### Metaphores can be "Smooth", "Balls&Sticks", "VdW", "Licorice"
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
setMetal(selName: str, type: str, val: float)
```

```python
### Change surface wireframe size
setSurfaceWireframe(selName: str, type: str, val: float)
```

```python
### Change hyperball representation parameters in all selections that contains a hb representation
### Metaphores can be "Smooth", "Balls&Sticks", "VdW", "Licorice"
setHyperBallMetaphore(metaphore: str)
```

```python
### Change hyperball representation parameters in the specified selection to a preset
### Metaphores can be "Smooth", "Balls&Sticks", "VdW", "Licorice"
setHyperBallMetaphore(selName: str, metaphore: str)
```

```python
### Set shininess for the hyperball representations of the specified selection
setHyperBallShininess(selName: str, shin: float)
```

```python
### Set the shrink factor for the hyperball representations of the specified selection
setHyperballShrink(selName: str, shrink: float)
```

```python
### Change all hyperball representation in the selection with a new texture mapped
### idTex of the texture is the index in UnityMolMain.atomColors.textures
setHyperballTexture(selName: str, idTex: int)
```

```python
clearHyperballAO(selName: str)
```

```python
### Set the color of the cartoon representation of the specified selection based on the nature of secondary structure assigned
### ssType can be "helix", "sheet" or "coil"
setCartoonColorSS(selName: str, ssType: str, col: Color)
```

```python
### Change the size of the representation of type 'type' in the selection
### Mainly used for hyperball representation
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
setRepSize(selName: str, type: str, size: float)
```

```python
### Change the color of all representation of type 'type' in selection
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
colorSelection(selName: str, type: str, col: Color)
```

```python
### Change the color of all representation of type 'type' in selection
### colorS can be "black", "white", "yellow", "green", "red", "blue", "pink", "gray"
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
colorSelection(selName: str, type: str, colorS: str)
```

```python
### Change the color of all representation of type 'type' in selection
### colors is a list of colors the length of the selection named selName
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
colorSelection(selName: str, type: str, colors: List<Color>)
```

```python
### Reset the color of all representation of type 'type' in selection to the default value
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
resetColorSelection(selName: str, type: str)
```

```python
### In the representation of type repType, color all atoms of type atomType in the selection selName with
colorAtomType(selName: str, repType: str, atomType: str, col: Color)
```

```python
### Use the color palette to color representations of type 'type' in the selection 'selName' by chain
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
colorByChain(selName: str, type: str)
```

```python
### Use the color palette to color representations of type 'type' in the selection 'selName' by residue
### type can be "cartoon", "c", "surface", "s", "hb", "line", "l", "hbond"
colorByResidue(selName: str, type: str)
```

```python
### Color representations of type 'type' in the selection 'selName' by atom
colorByAtom(selName: str, type: str)
```

```python
### Color representations of type 'type' in the selection 'selName' by hydrophobicity
colorByHydrophobicity(selName: str, type: str)
```

```python
### Color representations of type 'type' in the selection 'selName' by sequence (rainbow effect)
colorBySequence(selName: str, type: str)
```

```python
### Use the dx map to color by charge around atoms
### Only works for surface for now
### If normalizeDensity is set to True, the density values will be normalized
### if it is set to True, the default -10|10 range is used
colorByCharge(selName: str, normalizeDensity: bool = False, minDens: float = -10.0, maxDens: float = 10.0)
```

```python
### Color residues by "restype": negatively charge = red, positively charged = blue, nonpolar = light yellow,
### polar = green, cys = orange
colorByResidueType(selName: str, type: str)
```

```python
### Color residues by Bfactor
colorByBfactor(selName: str, type: str, startColor: Color, endColor: Color)
```

```python
### Color residues by Bfactor: low to high = blue to red
colorByBfactor(selName: str, type: str)
```

```python
setLineSize(selName: str, val: float)
```

```python
setTraceSize(selName: str, val: float)
```

```python
switchSheherasadeMethod(selName: str)
```

```python
setSheherasadeTexture(selName: str, idTex: int)
```

```python
### Offsets all representations to center the structure 'structureName'
### Instead of moving the camera, move the loaded molecules to center them on the center of the camera
centerOnStructure(structureName: str, lerp: bool = False, recordCommand: bool = True)
```

```python
getManipulationManager()
```

```python
### Offsets all representations to center the selection 'selName'
centerOnSelection(selName: str, lerp: bool = False)
```

```python
### Create a UnityMolSelection based on MDAnalysis selection language (https://www.mdanalysis.org/docs/documentation_pages/selections.html)
### Returns a UnityMolSelection object, adding it to the selection manager if createSelection is True
### If a selection with the same name already exists and addToExisting is True, add atoms to the already existing selection
### Set forceCreate to True if the selection is empty but still need to generate the selection
select(selMDA: str, name: str = "selection", createSelection: bool = True, addToExisting: bool = False, silent: bool = False, setAsCurrentSelection: bool = True, forceCreate: bool = False, allModels: bool = False)
```

```python
### Add a keyword to the selection language
addSelectionKeyword(keyword: str, selName: str)
```

```python
### Remove a keyword from the selection language
removeSelectionKeyword(keyword: str, selName: str)
```

```python
### Set the selection as currentSelection in the UnityMolSelectionManager
setCurrentSelection(selName: str)
```

```python
### Look for an existing selection named 'name' and add atoms to it based on MDAnalysis selection language
addToSelection(selMDA: str, name: str = "selection", silent: bool = False, allModels: bool = False)
```

```python
### Look for an existing selection named 'name' and remove atoms from it based on MDAnalysis selection language
removeFromSelection(selMDA: str, name: str = "selection", silent: bool = False, allModels: bool = False)
```

```python
### Delete selection 'selName' and all its representations
deleteSelection(selName: str)
```

```python
### Duplicate selection 'selName' and without the representations
duplicateSelection(selName: str)
```

```python
### Change the 'oldSelName' selection name into 'newSelName'
renameSelection(oldSelName: str, newSelName: str)
```

```python
### Update the atoms of the selection based on a new MDAnalysis language selection
### The selection only applies to the structures of the selection
updateSelectionWithMDA(selName: str, selectionString: str, forceAlteration: bool, silent: bool = False, recordCommand: bool = True, allModels: bool = False)

cleanHighlight()
```

```python
### Select atoms of all loaded molecules inside a sphere defined by a world space position and a radius in Anstrom
selectInSphere(position: Vector3, radius: float)
```

```python
### Update representations of the specified selection
updateRepresentations(selName: str)
```

```python
### Clear the currentSelection in UnityMolSelectionManager
clearSelections()
```

```python
### Utility function to test if a trajectory is playing for any loaded molecule
isATrajectoryPlaying()
```

```python
setUpdateSelectionTraj(selName: str, v: bool)
```

```python
### Utility function to change the material of highlighted selection
changeHighlightMaterial(Material newMat)
```

```python
### Take a screenshot of the current viewpowith: int a specific resolution
screenshot(path: str, resolutionWidth: int = 1280, resolutionHeight: int = 720, transparentBG: bool = False)
```

```python
### Start to record a video with FFMPEG at a specific resolution and framerate
startVideo(filePath: str, resolutionWidth: int = 1280, resolutionHeight: int = 720, frameRate: int = 30)
```

```python
### Stop recording
stopVideo()
```

```python
### Play the opposite function of the lastly called APIPython function recorded in UnityMolMain.pythonUndoCommands
undo()
```

```python
### Set the local position and rotation (euler angles) of the given structure
setStructurePositionRotation(structureName: str, pos: Vector3, rot: Vector3)
```

```python
getStructurePositionRotation(structureName: str, ref pos: Vector3, ref rot: Vector3)
```

```python
### Save the history of commands executed in a file
saveHistoryScript(path: str)
```

```python
setRotationCenter(newPos: Vector3)
```

```python
### Load a python script of commands (possibly the output of the saveHistoryScript function)
loadHistoryScript(path: str)
```

```python
### Set the position, scale and rotation of the parent of all loaded molecules
### Linear interpolation between the current state of the camera to the specified values
setMolParentTransform(pos: Vector3, scale: Vector3, rot: Vector3, centerOfRotation: Vector3)
```

```python
### Change the scale of the parent of the representations of each molecules
### Try to not move the center of mass
changeGeneralScale_cog(newVal: float)
```

```python
### Change the scale of the parent of the representations of each molecules
### Keep relative positions of molecules, use the first loaded molecule center of gravity to compensate the translation due to scaling
changeGeneralScale(newVal: float)
```

```python
### Use Reduce method to add hydrogens
addHydrogensReduce(structureName: str)
```

```python
### Use HAAD method to add hydrogens
addHydrogensHaad(structureName: str)
```

```python
### Set the atoms of the selection named 'selName' to ligand
setAsLigand(selName: str, isLig: bool = True, updateAllSelections: bool = True)
```

```python
### Merge UnityMolStructure structureName2 in structureName using a different chain name to avoid conflict
mergeStructure(structureName: str, structureName2: str, chainName: str = "Z")
```

```python
### Connect to a running simulation using the IMD protocol implemented in Artemis
### The running simulation is binded to a UnityMolStructure
connectIMD(structureName: str, adress: str, port: int)
```

```python
### Disconnect from the IMD simulation for the specified structure
disconnectIMD(structureName: str)
```

```python
getSurfaceType(selName: str)
```

```python
getHyperBallMetaphore(selName: str)
```

```python
setCameraNearPlane(newV: float)
```

```python
setCameraFarPlane(newV: float)
```

```python
### Return the lastly loaded UnityMolStructure
last()
```

```python
### Change the background color of the camera
bg_color(colorS: str)
```

```python
### Switch on or off the rotation around the X axis of all loaded molecules
switchRotateAxisX()
```

```python
### Switch on or off the rotation around the Y axis of all loaded molecules
switchRotateAxisY()
```

```python
### Switch on or off the rotation around the Z axis of all loaded molecules
switchRotateAxisZ()
```

```python
### Change the rotation speed around the X axis
changeRotationSpeedX(val: float)
```

```python
### Change the rotation speed around the Y axis
changeRotationSpeedY(val: float)
```

```python
### Change the rotation speed around the Z axis
changeRotationSpeedZ(val: float)
```

```python
### Change the mouse scroll speed
setMouseScrollSpeed(val: float)
```

```python
### Change the speed of mouse rotations and translations
setMouseMoveSpeed(val: float)
```

```python
### Stop rotation around all axis
stopRotations()
```


```python
### Transform a of: str representation type to a RepType object
getRepType(type: str)
```

```python
### Transform a representation type into a string
getTypeFromRepType(rept: RepType)
```

```python
annotateAtom(structureName: str, atomId: int)
```

```python
removeAnnotationAtom(structureName: str, atomId: int)
```

```python
annotateAtomText(structureName: str, atomId: int, text: str)
```

```python
removeAnnotationAtomText(structureName: str, atomId: int, text: str)
```

```python
annotateLine(structureName: str, atomId: int, structureName2: str, atomId2: int)
```

```python
removeAnnotationLine(structureName: str, atomId: int, structureName2: str, atomId2: int)
```

```python
annotateDistance(structureName: str, atomId: int, structureName2: str, atomId2: int)
```

```python
removeAnnotationDistance(structureName: str, atomId: int, structureName2: str, atomId2: int)
```

```python
annotateAngle(structureName: str, atomId: int, structureName2: str, atomId2: int, structureName3: str, atomId3: int)
```

```python
removeAnnotationAngle(structureName: str, atomId: int, structureName2: str, atomId2: int, structureName3: str, atomId3: int)
```

```python
annotateDihedralAngle(structureName: str, atomId: int, structureName2: str, atomId2: int, structureName3: str, atomId3: int, structureName4: str, atomId4: int)
```

```python
removeAnnotationDihedralAngle(structureName: str, atomId: int, structureName2: str, atomId2: int, structureName3: str, atomId3: int, structureName4: str, atomId4: int)
```

```python
annotateRotatingArrow(structureName: str, atomId: int, structureName2: str, atomId2: int)
```

```python
removeAnnotationRotatingArrow(structureName: str, atomId: int, structureName2: str, atomId2: int)
```

```python
annotateArcLine(structureName: str, atomId: int, structureName2: str, atomId2: int, structureName3: str, atomId3: int)
```

```python
removeAnnotationArcLine(structureName: str, atomId: int, structureName2: str, atomId2: int, structureName3: str, atomId3: int)
```

```python
annotateDrawLine(structureName: str, line: List<Vector3>, col: Color)
```

```python
removeLastDrawLine(structureName: str, id: int)
```

```python
clearDrawings()
```

```python
clearAnnotations()
```
