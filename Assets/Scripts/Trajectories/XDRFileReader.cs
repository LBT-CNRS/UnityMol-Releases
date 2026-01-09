/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2022
        Hubert Santuz, 2022-2026
        Marc Baaden, 2010-2026
        unitymol@gmail.com
        https://unity.mol3d.tech/

        This file is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications based on the Unity3D game engine.
        More details about UnityMol are provided at the following URL: https://unity.mol3d.tech/

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

        To help us with UnityMol development, we ask that you cite
        the research papers listed at https://unity.mol3d.tech/cite-us/.
    ================================================================================
*/
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System;

namespace UMol {


    /// <summary>
    /// XDR File status
    /// </summary>
    enum XDRFileReaderStatus {
        OFFSETFILECREATION = -7,
        OFFSETFILETREAD = -6,
        TRAJECTORYPRESENT = -5,
        FRAMEDOESNOTEXIST = -4,
        ENDOFFILE = -3,
        NUMBEROFATOMSMISMATCH = -2,
        FILENOTFOUND = -1,
        SUCCESS = 0
    }

    /// <summary>
    /// Seek enum
    /// </summary>
    enum SEEK {
        SEEK_SET = 0,   /* set file offset to offset */
        SEEK_CUR = 1,   /* set file offset to current plus offset */
        SEEK_END = 2   /* set file offset to EOF plus offset */
    }

/// <summary>
///  XTC/TRR Trajectory Reader
/// </summary>
public class XDRFileReader {


    /// <summary>
    /// Current Frame played
    /// </summary>
    public int CurrentFrame;

    /// <summary>
    /// Number of frames in the trajectory
    /// </summary>
    public int NumberFrames;

    /// <summary>
    /// Path of the trajectory file
    /// </summary>
    public string TrajectoryFilePath;

    /// <summary>
    /// Structure associated to the trajectory
    /// </summary>
    private UnityMolStructure structure;



    /// <summary>
    /// Pointer to the trajectory file
    /// </summary>
    private IntPtr filePointer = IntPtr.Zero;


    /// <summary>
    /// Number of Atoms in the trajectory
    /// </summary>
    private int numberAtoms;




    /// <summary>
    /// OffSet filename
    /// </summary>
    private string offsetFileName;
    /// <summary>
    /// List of offsets
    /// </summary>
    private long[] offsets;

    /// <summary>
    /// Trajectory in the .trr format?
    /// </summary>
    private bool isTrr;

    /// <summary>
    /// Box dimensions in the trajectory file
    /// <remarks>Currently unused</remarks>
    /// </summary>
    private float[,] box = new float[3, 3];

    /// <summary>
    /// Buffer of frames containing TRAJBUFFERSIZE/2 frames before the current frame and TRAJBUFFERSIZE/2 after
    /// </summary>
    private Vector3[][] trajectoryBuffer;


    /// <summary>
    /// Trajectory smoother
    /// UI option
    /// </summary>
    private TrajectorySmoother trajSmoother;

    /// <summary>
    /// Trajectory helper to average a certain amount of frame
    /// UI option
    /// </summary>
    private TrajectoryMean trajMean;
    /// <summary>
    /// List of frames to average
    /// </summary>
    private Vector3[] framesToMean;

    /// <summary>
    /// Object to handle the threaded version of the reader
    /// </summary>
    private ThreadedXDRReader threadedReader;

#if UNITY_WEBGL
    /// <summary>
    /// Full trajectory stored in Vector3 array
    /// </summary>
    List<Vector3[]> fullTrajectory;
#endif


    /// <summary>
    /// Initiate at frame 0 and returns the number of frames
    ///
    /// </summary>
    /// <returns></returns>
    public int LoadTrajectory() {

        trajSmoother = new TrajectorySmoother();
        trajMean = new TrajectoryMean();
        sync_scene_with_frame(0);

        return NumberFrames;
    }


    /// <summary>
    /// Opens a trajectory file linked to a structure.
    /// </summary>
    /// <param name="stru">UnityMol structure reference for the trajectory</param>
    /// <param name="filePath">Path of the trajectory file</param>
    /// <param name="is_trr">Type of trajectory file: TRR or XTC</param>
    /// <returns>the number of frames in the trajectory</returns>
    public int open_trajectory(UnityMolStructure stru, string filePath, bool is_trr = false) {
        if (filePointer != IntPtr.Zero) {
            Debug.LogError("This instance has a trajectory already opened.");
            return (int) XDRFileReaderStatus.TRAJECTORYPRESENT;
        }
        if (stru.trajectoryLoaded) {
            Debug.LogError("This structure has a trajectory already opened.");
            return (int) XDRFileReaderStatus.TRAJECTORYPRESENT;
        }

        structure = stru;
        TrajectoryFilePath = filePath;
        isTrr = is_trr;

#if UNITY_WEBGL
        fullTrajectory = XTCTrajectoryParserCSharp.GetTrajectory(filePath);
        numberAtoms = fullTrajectory[0].Length;
        numberFrames = fullTrajectory.Count;

        if (numberAtoms > structure.Count) {
            Debug.LogWarning("Trajectory has not the same number of atoms than the first model of the structure." + numberAtoms + " vs " + structure.Count);
        }
        this.is_trr = is_trr;

        structure.trajectoryLoaded = true;

        return numberAtoms;
#endif
        try{
            filePointer = XDRFileWrapper.xdrfile_open (filePath, "r");
        } catch (DllNotFoundException) {
            Debug.LogError("XDR Reader failed: Missing external xdr library.");
            return -1;
        }

        if (filePointer == IntPtr.Zero) {
            Debug.LogError("Could not open file " + filePath);
            return (int) XDRFileReaderStatus.FILENOTFOUND;
        }

        XDRStatus result;
        if (is_trr) {
            result = XDRFileWrapper.read_trr_natoms (filePath, ref numberAtoms);
        } else {
            result = XDRFileWrapper.read_xtc_natoms (filePath, ref numberAtoms);
        }

        if (result != XDRStatus.exdrOK) {
            Debug.LogError("Could not get number of atoms from file " + filePath);
            return (int) XDRFileReaderStatus.FILENOTFOUND;
        }

        if (numberAtoms > structure.Count) {
            Debug.LogWarning("Trajectory has not the same number of atoms than the first model of the structure." + numberAtoms + " vs " + structure.Count);
        }


        int res = updateOffsetFile(filePath);
        if (res != (int) XDRFileReaderStatus.SUCCESS) {
            if (res == (int) XDRFileReaderStatus.OFFSETFILETREAD) {
                Debug.LogError("Could not read offset file " + offsetFileName);
                return (int) XDRFileReaderStatus.OFFSETFILETREAD;
            }

            Debug.LogError("Could not create offset file " + offsetFileName);
            return (int) XDRFileReaderStatus.OFFSETFILECREATION;
        }

        structure.trajectoryLoaded = true;
        threadedReader = new ThreadedXDRReader(numberAtoms, NumberFrames, filePointer, is_trr, offsets);
        return numberAtoms;
    }


    /// <summary>
    /// Return  atoms coordinates of the frame number
    /// </summary>
    /// <param name="frameNumber">The frame number</param>
    /// <returns>Array of coordinates of all atoms</returns>
    public Vector3[] GetFrame(int frameNumber) {
#if UNITY_WEBGL
        if (fullTrajectory != null) {
            if (frameNumber >= 0 && frameNumber < numberFrames) {
                return fullTrajectory[frameNumber];
            }
        }
#endif
        return threadedReader.GetFrame(frameNumber);
    }


    /// <summary>
    /// Update the offset file for the trajectory.
    /// If the file is not found or the boolean is set to True, offset file will be created.
    /// </summary>
    /// <param name="trajFile">Full Trajectory file</param>
    /// <param name="forceCreate">Force the creation of the offset file.</param>
    /// <returns>Exit code. 0 means success.</returns>
    private int updateOffsetFile(string trajFile, bool forceCreate = false) {
        offsetFileName = trajFile + ".offset";
        bool offsetExists = File.Exists(offsetFileName);

        if (forceCreate) {
            return createOffsetFile(trajFile);
        }

        if (!offsetExists) {
            return createOffsetFile(trajFile);
        }

        DateTime lastModif = File.GetLastWriteTime(offsetFileName);
        DateTime creationTraj = File.GetLastWriteTime(trajFile);
        if (lastModif > creationTraj) { //Offset file is posterior to creation of traj file
            try {
                return readOffsetFile();
            }
            catch {
                //Failed to read offset file => create one
            }
        }
        return createOffsetFile(trajFile);
    }

    /// <summary>
    /// Create the offset file for the trajectory
    /// </summary>
    /// <param name="fileName">Trajectory file</param>
    /// <returns>Exit code. 0 means success.</returns>
    private int createOffsetFile(string fileName) {
        try {
            if (!isTrr) {//XTC

                IntPtr outOffsets = IntPtr.Zero;
                int res = XDRFileWrapper.read_xtc_numframes(fileName, ref NumberFrames, ref outOffsets);
                if (res != (int) XDRStatus.exdrOK) {
                    return res;
                }
                offsets = new long[NumberFrames];
                Marshal.Copy(outOffsets, offsets, 0, NumberFrames);
            }
            else {
                IntPtr outOffsets = IntPtr.Zero;
                int res = XDRFileWrapper.read_trr_numframes(fileName, ref NumberFrames, ref outOffsets);
                if (res != (int) XDRStatus.exdrOK) {
                    return res;
                }
                offsets = new long[NumberFrames];
                Marshal.Copy(outOffsets, offsets, 0, NumberFrames);
            }

            BinaryWriter bw = new(new FileStream(offsetFileName, FileMode.Create));
            bw.Write(NumberFrames);
            for (int i = 0; i < NumberFrames; i++) {
                bw.Write(offsets[i]);
            }
            bw.Close();
        }
        catch (Exception e) {
            Debug.LogError(e);
            return (int) XDRFileReaderStatus.OFFSETFILECREATION;
        }

        return (int) XDRFileReaderStatus.SUCCESS;
    }

    /// <summary>
    /// Read the offset file
    /// </summary>
    /// <returns>Exit code. 0 means success.</returns>
    private int readOffsetFile() {
        try {
            BinaryReader br = new(new FileStream(offsetFileName, FileMode.Open));
            NumberFrames = br.ReadInt32();
            if (NumberFrames > 0 && NumberFrames < 2e9) { //2 billion seems like a fair upper limit for trajectories
                offsets = new long[NumberFrames];
                for (int i = 0; i < NumberFrames; i++) {
                    offsets[i] = br.ReadInt64();
                }
            }
            else {
                return (int) XDRFileReaderStatus.OFFSETFILETREAD;
            }
        }
        catch {
            return (int) XDRFileReaderStatus.OFFSETFILETREAD;
        }

        return (int) XDRFileReaderStatus.SUCCESS;
    }

    /// <summary>
    /// Reads the next frame from the trajectory.
    ///
    /// </summary>
    /// <param name="frameNumber"></param>
    /// <param name="windowMean"></param>
    /// <param name="windowSize"></param>
    /// <param name="windowForward"></param>
    /// <returns>Exit code. 0 means success.</returns>
    public int sync_scene_with_frame(int frameNumber, bool windowMean = false, int windowSize = 5, bool windowForward = true) {
        if (frameNumber >= NumberFrames) {
            Debug.LogError("Frame number " + frameNumber + " does not exist.");
            return (int) XDRFileReaderStatus.FRAMEDOESNOTEXIST;
        }


        if (windowMean) {
            windowSize = Mathf.Max(1, windowSize);
            getFramesToMean(frameNumber, windowSize, windowForward);
            trajMean.Init(framesToMean, numberAtoms, windowSize);
            trajMean.Process(structure.trajAtomPositions);
        }

        else {
            Vector3[] f = GetFrame(frameNumber);
            structure.trajAtomPositions = f;
        }

        structure.trajUpdateAtomPositions();

        CurrentFrame = frameNumber;
        return (int) XDRFileReaderStatus.SUCCESS;
    }

    /// <summary>
    /// Reads the next frame from the trajectory with smoothing feature
    /// </summary>
    /// <param name="frame1"></param>
    /// <param name="frame2"></param>
    /// <param name="t"></param>
    /// <param name="newFrame"></param>
    /// <returns>Exit code. 0 means success.</returns>
    public int sync_scene_with_frame_smooth(int frame1, int frame2, float t, bool newFrame = false) {
        if (frame1 >= NumberFrames || frame1 < 0) {
            Debug.LogError("Frame number " + frame1 + " does not exist.");
            return (int) XDRFileReaderStatus.FRAMEDOESNOTEXIST;
        }
        if (frame2 >= NumberFrames || frame2 < 0) {
            Debug.LogError("Frame number " + frame2 + " does not exist.");
            return (int) XDRFileReaderStatus.FRAMEDOESNOTEXIST;
        }

        t = Mathf.Clamp(t, 0.0f, 1.0f);

        Vector3[] f1 = GetFrame(frame1);
        Vector3[] f2 = GetFrame(frame2);
        trajSmoother.Init(null, f1, f2, null);
        trajSmoother.Process(structure.trajAtomPositions, t);

        structure.trajUpdateAtomPositions();

        //Not always updating currentFrame
        if (newFrame) {
            CurrentFrame = frame1;
        }

        return (int) XDRFileReaderStatus.SUCCESS;
    }

    /// <summary>
    /// Fills framesToMean array of arrays of positions
    /// </summary>
    /// <param name="start"></param>
    /// <param name="windowSize"></param>
    /// <param name="forward"></param>
    private void getFramesToMean(int start, int windowSize, bool forward = true) {
        if (framesToMean == null || framesToMean.Length != windowSize * numberAtoms) {
            framesToMean = new Vector3[windowSize * numberAtoms];
        }
        int count = 0;

        if (forward) {
            for (int i = start; i < NumberFrames; i++) {
                Vector3[] f = GetFrame(i);
                Array.Copy(f, 0, framesToMean, count * numberAtoms, numberAtoms);
                count++;
                if (count == windowSize) {
                    break;
                }
            }
            //Fill with last frame when not enough frames
            while (count != windowSize) {
                Vector3[] f = GetFrame(NumberFrames - 1);
                Array.Copy(f, 0, framesToMean, count * numberAtoms, numberAtoms);
                count++;
            }
        }
        else {
            for (int i = start; i >= 0; i--) {
                Vector3[] f = GetFrame(i);
                Array.Copy(f, 0, framesToMean, count * numberAtoms, numberAtoms);
                count++;
                if (count == windowSize) {
                    break;
                }
            }
            //Fill with first frame when not enough frames
            while (count != windowSize) {
                Vector3[] f = GetFrame(0);
                Array.Copy(f, 0, framesToMean, count * numberAtoms, numberAtoms);
                count++;
            }
        }
    }

    /// <summary>
    /// Close the trajectory file & reset the attributes.
    /// </summary>
    /// <returns>Exit code. 0 means success.</returns>
    private void closeTrajectory() {

        trajSmoother.Clear();
        trajMean.Clear();
        threadedReader.Clear();

        if (filePointer == IntPtr.Zero) {
            return;
        }

        XDRFileWrapper.xdrfile_close(filePointer);

        offsets = null;
        filePointer = IntPtr.Zero;
        numberAtoms = 0;
        NumberFrames = 0;
        structure.trajectoryLoaded = false;
    }

    /// <summary>
    /// Clear the object & reset the trajectory status of the UnitymolStructure.
    /// </summary>
    public void Clear() {
        closeTrajectory();
        if (structure != null && structure.trajAtomPositions != null) {
            structure.trajAtomPositions = null;
            structure.trajectoryLoaded = false;
        }
    }
}
}
