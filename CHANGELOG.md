# UnityMol Changelog

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
- Ambient Occlusion for hyperballs in Van der Waals representation
- Add a new representation "Sheherasade"
- Parse mmCIF connectivity
- Fix some UI + sound issues
- Performance improvements (fieldlines/trajectory smoother/raycast/center of gravity -> compiled with Burst)
- Command line arguments (example 'UnityMol.exe -i 1kx2 pdbs/1crn.pdb')
- Fix recording videos in VR
- IMD should work on Mac and Linux now
- Fix PDB writer when residue name is too large
