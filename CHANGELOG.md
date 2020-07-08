# UnityMol Changelog

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
