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
using System.Collections.Concurrent;
using System.Threading;
using System;
using System.Collections.Generic;

namespace UMol {

    /// <summary>
    /// XDR/XTC trajectory reader. Threaded version
    /// </summary>
    public class ThreadedXDRReader {

        /// <summary>
        /// Number of frames to pre-fetch from the file
        /// Bigger value means we need to access the xtc file less often but when we do it takes more time.
        /// </summary>
        private const int TRAJBUFFERSIZE = 128;

        /// <summary>
        /// Pointer to the trajectory file
        /// </summary>
        private readonly IntPtr filePointer;

        /// <summary>
        /// Thread safe collection of frames
        /// </summary>
        private readonly BlockingCollection<FrameRequest> framesToRead = new();

        /// <summary>
        /// Number of atoms in the trajectory
        /// </summary>
        private readonly int atomCount;
        /// <summary>
        /// Number of frames in the trajectory.
        /// </summary>
        private readonly int frameCount;
        /// <summary>
        /// Thread to handle the read
        /// </summary>
        private readonly Thread ioThread;

        /// <summary>
        /// Trajectory in the .trr format?
        /// </summary>
        private readonly bool isTrr;

        /// <summary>
        /// Current index in trajectory
        /// </summary>
        private int currentBankIndex;
        /// <summary>
        /// Part of the trajectory in the buffer
        /// </summary>
        private Vector3[][] trajectoryBuffer;
        /// <summary>
        /// Mapping between the position of the frame globally and in the buffer
        /// </summary>
        private ConcurrentDictionary<int, int> frameToTrajBuffer = new();
        /// <summary>
        /// Offsets between the global trajectory and the buffer
        /// </summary>
        private long[] offsets;

        /// <summary>
        /// Positions of the atoms in a flat array
        /// </summary>
        private float[] tempPositions;

        /// <summary>
        /// Box dimensions in the trajectory file
        /// <remarks>Currently unused</remarks>
        /// </summary>
        private float[,] box = new float[3, 3];

        /// <summary>
        /// Thread-safe struct to handle a frame
        /// </summary>
        private struct FrameRequest {
            public readonly int FrameIndex;
            public readonly ManualResetEventSlim WaitHandle;

            public FrameRequest(int frameIndex) {
                FrameIndex = frameIndex;
                WaitHandle = new ManualResetEventSlim(false);
            }
        }

        public ThreadedXDRReader(int nAtoms, int frameCount, IntPtr fpointer, bool trr, long[] offsetsArray) {
            filePointer = fpointer;
            isTrr = trr;
            offsets = offsetsArray;
            trajectoryBuffer = new Vector3[TRAJBUFFERSIZE][];
            atomCount = nAtoms;
            this.frameCount = frameCount;
            tempPositions = new float[nAtoms * 3];
            for (int i = 0; i < TRAJBUFFERSIZE; i++) {
                trajectoryBuffer[i] = new Vector3[nAtoms];
            }
            ioThread = new Thread(readFramesThreaded);
            ioThread.Start();
        }

        /// <summary>
        /// Return the atoms positions of the frame number requested
        /// </summary>
        /// <param name="frameNumber"> Global Frame number</param>
        /// <returns>vector of atom positions</returns>
        public Vector3[] GetFrame(int frameNumber) {
            FrameRequest request = new(frameNumber);
            framesToRead.Add(request);

            request.WaitHandle.Wait();

            int indexInBuffer;
            while(!frameToTrajBuffer.TryGetValue(frameNumber, out indexInBuffer)) {
                continue;
            }
            return trajectoryBuffer[indexInBuffer];
        }


        private void readFramesThreaded() {
            foreach (FrameRequest request in framesToRead.GetConsumingEnumerable()) {
                readBufferOfFrames(request.FrameIndex);
                request.WaitHandle.Set();
            }
        }

        /// <summary>
        /// Read a certain number of frames and save the atom positions
        /// </summary>
        /// <param name="middleFrame"></param>
        private void readBufferOfFrames(int middleFrame) {
            for (int i = middleFrame - TRAJBUFFERSIZE / 2; i < middleFrame + TRAJBUFFERSIZE / 2; i++) {
                if (i < 0 || i >= frameCount) {
                    continue;
                }
                if (frameToTrajBuffer.ContainsKey(i)) {
                    continue;
                }

                int res = readOneFrame(i);

                if (res != (int)XDRStatus.exdrOK) {
                    Debug.LogErrorFormat("Error when reading frame {0} from the trajectory file", i);
                    return;
                }


                processFrame(tempPositions, trajectoryBuffer[currentBankIndex]);

                //Remove previous value
                int frameToDelete = -1;
                foreach (KeyValuePair<int, int> f in frameToTrajBuffer) {
                    if (f.Value == currentBankIndex) {
                        frameToDelete = f.Key;
                        break;
                    }
                }
                if (frameToDelete != -1) {
                    frameToTrajBuffer.TryRemove(frameToDelete, out _);
                }

                frameToTrajBuffer[i] = currentBankIndex;

                currentBankIndex++;
                if (currentBankIndex == TRAJBUFFERSIZE) {
                    currentBankIndex = 0;
                }
            }
        }

        /// <summary>
        /// Read one frame in the trajectory file
        /// </summary>
        /// <param name="frameNumber">Global frame number</param>
        /// <returns></returns>
        private int readOneFrame(int frameNumber) {
            int res = (int)XDRFileWrapper.xdr_seek(filePointer, offsets[frameNumber], (int) SEEK.SEEK_SET);
            if (res != (int)XDRStatus.exdrOK) {
                Debug.LogWarning("Error when seeking into the trajectory file");
                return res;
            }

            int step = 0;
            float time = 0;
            float precision = 0;
            if (filePointer == IntPtr.Zero) {
                Debug.LogWarning("Trajectory was not previously opened.");
                return (int) XDRFileReaderStatus.FILENOTFOUND;
            }

            if (!isTrr) {
                return (int)XDRFileWrapper.read_xtc(filePointer, atomCount, ref step, ref time, box, tempPositions,
                    ref precision);
            }

            // TRR file
            float lambda = 0.0f;
            return (int) XDRFileWrapper.read_trr(filePointer, atomCount, ref step, ref time, ref lambda, box, tempPositions, null, null);;

        }

        /// <summary>
        /// Transform a flat array of coordinates into a Vector3 array of coordinates
        /// </summary>
        /// <param name="positionBuffer">Flat array of the coordinates</param>
        /// <param name="outPositions">Vector3 array of the coordinates</param>
        private void processFrame(float[] positionBuffer, Vector3[] outPositions) {

            for (int i = 0; i < positionBuffer.Length / 3; i++) {
                float x = -positionBuffer[i * 3];
                float y = positionBuffer[i * 3 + 1];
                float z = positionBuffer[i * 3 + 2];
                outPositions[i] = new Vector3(x, y, z) * 10;
            }
        }

        /// <summary>
        /// Clear attributes and close the thread.
        /// </summary>
        public void Clear() {
            framesToRead.CompleteAdding();
            ioThread.Join();
            trajectoryBuffer = null;
            frameToTrajBuffer.Clear();
            tempPositions = null;
        }

    }
}
