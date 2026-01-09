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
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace UMol {
///From https://github.com/BMMG-Curtin/Molecular-Dynamics-Visualization-MDV/blob/master/Assets/CurtinUniversity/Scripts/CurtinUniversity/MolecularDynamics/Model/FileParser/XTCTrajectoryParser.cs

/// <summary>
/// Some of the code below has been derived from the XTC file parsing routines in VMD.
/// To save time I have not rewritten all the C code from VMD but migrated some of the C code into C# unsafe code.
/// Due to the migration and the need to map managed to unmanaged memory the 'readData' method likely has a performance hit since it
/// copies managed memory into unamanged memory. A full rewrite of the C methods into managed code would likely improve file read performance.
/// </summary>
public class XTCTrajectoryParserCSharp {
#if UNITY_WEBGL

    private const int DEFAULT_FRAME_READ = 1000; // maximum amount of frames that will be read into a trajectory file by this class
    // roughly, 12 bytes per atom per frame. Assume 500k max atoms = 6mb per frame. 1000 frames = 6gb data in memory max

    /// <summary>
    /// Returns number of frames in trajectory file.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static int GetFrameCount(string filename) {

        BinaryReader reader = null;
        int count = 0;

        try {

            reader = new BinaryReader(new FileStream(filename, FileMode.Open));

            while (reader.BaseStream.Position != reader.BaseStream.Length) {
                discardFrame(reader);
                count++;
            }
        }
        catch (Exception e) {

            // handle end of file corruption gracefully.
            if (count > 0) {
                count--; // if exception will be reading frame so need to discard last frame count
                return count;
            }
            else {
                throw new Exception(e.Message);
            }
        }
        finally {

            if (reader != null) {
                reader.Close();
            }
        }

        return count;
    }

    /// <summary>
    /// Returns the number of atoms in the first frame of the trajectory file.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static int GetAtomCount(string filename) {

        BinaryReader reader = null;
        int count = 0;

        try {

            reader = new BinaryReader(new FileStream(filename, FileMode.Open));
            Vector3[] frame = getFrame(reader);
            count = frame.Length;
        }
        catch (Exception e) {

            throw new Exception(e.Message);
        }
        finally {

            if (reader != null) {
                reader.Close();
            }
        }

        return count;
    }

    public static List<Vector3[]> GetTrajectory(string filename) {
        return GetTrajectory(filename, 0, DEFAULT_FRAME_READ, 1);
    }

    public static List<Vector3[]> GetTrajectory(string filename, int startFrame, int numFrames, int frameFrequency) {

        List<Vector3[]> trajectory = new List<Vector3[]>();

        BinaryReader reader = null;

        try {

            reader = new BinaryReader(new FileStream(filename, FileMode.Open));

            for (int i = 0; i < startFrame; i++) {
                discardFrame(reader);
            }

            int framesAdded = 0;
            int currentFrame = 0;

            while (reader.BaseStream.Position != reader.BaseStream.Length && framesAdded < numFrames) {

                currentFrame++;

                if (currentFrame % frameFrequency == 0) {
                    trajectory.Add(getFrame(reader));
                    framesAdded++;
                }
                else {
                    discardFrame(reader);
                }
            }
        }
        catch (Exception e) {

            // handle end of file corruption gracefully.
            if (trajectory.Count > 0) {
                return trajectory;
            }
            else {
                throw new Exception(e.Message);
            }
        }
        finally {

            if (reader != null) {
                reader.Close();
            }
        }

        return trajectory;
    }
    /// <summary>
    /// Reads a timestep from an .xtc file.
    /// </summary>
    private static Vector3[] getFrame(BinaryReader reader) {


        // Check magic number
        if (getInt32(reader) != magicNumber) {
            throw new Exception("XTC file not well formed, no magic number in frame header");
        }

        int N = getInt32(reader);
        Vector3[] frame = new Vector3[N];

        // get timestep header info
        int step = getInt32(reader);
        float time  = getFloat32(reader);

        // read and discard the box
        for (int i = 0; i < 9; i++) {
            getFloat32(reader);
        }

        // get the coordinates
        xtc_3dfcoord(reader, ref frame);

        return frame;
    }

    private static void discardFrame(BinaryReader reader) {


        // Check magic number
        if (getInt32(reader) != magicNumber) {
            throw new Exception("XTC file not well formed, no magic number in frame header");
        }

        // discard 12 frame metadata fields
        reader.BaseStream.Seek(12 * 4, SeekOrigin.Current);

        // discard the coordinates metadata & data

        int lsize = getInt32(reader);
        if (lsize <= 9) {

            reader.BaseStream.Seek(lsize * 3 * 4, SeekOrigin.Current);
            return;
        }
        else {

            // discard 7 coord metadata fields
            reader.BaseStream.Seek(8 * 4, SeekOrigin.Current);

            // get length of coord data
            int len = getInt32(reader);
            if (len < 1) {
                throw new Exception("Error reading coordinates");
            }

            // discard coordinate data
            reader.BaseStream.Seek(len, SeekOrigin.Current);

            // round out byte read to nearest 4 bytes
            if (len % 4 != 0) {
                reader.BaseStream.Seek(4 - (len % 4), SeekOrigin.Current);
            }
        }
    }

    /// <summary>
    /// All code below is derived from the VMD C code to read XTC files
    /// Original code has been modified to interface with a C# binary file reader and appropriate C# types to match byte lengths in original code.
    /// Note that most methods are declared 'unsafe' for pointer management
    /// </summary>

    private static int magicNumber = 1995; // first number in each frame of an xtc binary file

    // integer table used in decompression
    private static int[] xtc_magicints = new int[] {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 10, 12, 16, 20, 25, 32, 40, 50, 64,
        80, 101, 128, 161, 203, 256, 322, 406, 512, 645, 812, 1024, 1290,
        1625, 2048, 2580, 3250, 4096, 5060, 6501, 8192, 10321, 13003, 16384,
        20642, 26007, 32768, 41285, 52015, 65536, 82570, 104031, 131072,
        165140, 208063, 262144, 330280, 416127, 524287, 660561, 832255,
        1048576, 1321122, 1664510, 2097152, 2642245, 3329021, 4194304,
        5284491, 6658042, 8388607, 10568983, 13316085, 16777216
    };

    private const int FIRSTIDX = 9;

    private static int getInt32(BinaryReader reader) {

        try {
            byte[] bytes = reader.ReadBytes(4);
            return (bytes[3]) | (bytes[2] << 8) | (bytes[1] << 16) | (bytes[0] << 24);
        }
        catch (Exception) {
            throw new Exception("Error reading int32");
        }
    }

    private static float getFloat32(BinaryReader reader) {

        try {

            byte[] bytes = new byte[4];
            int bytesRead = reader.Read(bytes, 0, bytes.Length);
            int i = (bytes[3] + (bytes[2] << 8) + (bytes[1] << 16) + (bytes[0] << 24));
            return BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
        }
        catch (Exception) {
            throw new Exception("Error reading float32");
        }
    }

    private static unsafe void getData(BinaryReader reader, byte* buf, int len) {

        if (len < 1) {
            throw new Exception("Error readering xtc file data");
        }

        try {

            // read bytes
            byte[] bytes = reader.ReadBytes(len);

            // round out byte read to nearest 4 bytes
            if (len % 4 != 0) {
                reader.BaseStream.Seek(4 - (len % 4), SeekOrigin.Current);
            }

            // copy bytes to buffer. This is necessary to overcome the limitations of 'fixed' and unmanaged memory in the calling code.
            for (int i = 0; i < len; i++) {
                buf[i] = bytes[i];
            }
        }
        catch (Exception) {
            throw new Exception("Error readering xtc file data");
        }
    }

    // returns the number of bits in the binary expansion of the given integer.
    private static int xtc_sizeofint(int size) {
        uint num = 1;
        uint ssize = (uint)size;
        int nbits = 0;

        while (ssize >= num && nbits < 32) {
            nbits++;
            num <<= 1;
        }
        return nbits;
    }

    // calculates the number of bits a set of integers, when compressed will take up.
    private static int xtc_sizeofints(int nints, uint[] sizes) {
        int i;
        uint num;
        uint nbytes;
        uint nbits;
        uint[] bytes = new uint[32];
        uint bytecnt;
        uint tmp;
        nbytes = 1;
        bytes[0] = 1;
        nbits = 0;
        for (i = 0; i < nints; i++) {
            tmp = 0;
            for (bytecnt = 0; bytecnt < nbytes; bytecnt++) {
                tmp = bytes[bytecnt] * sizes[i] + tmp;
                bytes[bytecnt] = tmp & 0xff;
                tmp >>= 8;
            }
            while (tmp != 0) {
                bytes[bytecnt++] = tmp & 0xff;
                tmp >>= 8;
            }
            nbytes = bytecnt;
        }
        num = 1;
        nbytes--;
        while (bytes[nbytes] >= num) {
            nbits++;
            num *= 2;
        }
        return (int)nbits + (int)nbytes * 8;
    }

    // reads bits from a buffer.
    static unsafe int xtc_receivebits(int* buf, int nbits) {

        int cnt, num;
        uint lastbits, lastbyte;
        byte* cbuf;
        int mask = (1 << nbits) - 1;

        cbuf = ((byte*)buf) + 3 * sizeof(int);
        cnt = buf[0];
        lastbits = (uint) buf[1];
        lastbyte = (uint) buf[2];

        num = 0;
        while (nbits >= 8) {
            lastbyte = (lastbyte << 8) | cbuf[cnt++];
            num |= ((int)lastbyte >> (int)lastbits) << (nbits - 8);
            nbits -= 8;
        }
        if (nbits > 0) {
            if (lastbits < (uint)nbits) {
                lastbits += 8;
                lastbyte = (lastbyte << 8) | cbuf[cnt++];
            }
            lastbits -= (uint)nbits;
            num |= (((int)lastbyte >> (int)lastbits) & ((1 << nbits) - 1));
        }
        num &= mask;
        buf[0] = cnt;
        buf[1] = (int)lastbits;
        buf[2] = (int)lastbyte;

        return (short)num;
    }

    // decompresses small integers from the buffer sizes parameter has to be non-zero to prevent divide-by-zero
    static unsafe void xtc_receiveints(int* buf, int nints, int nbits, uint* sizes, int* nums) {

        int[] bytes = new int[32];
        int i, j, nbytes, num, p;

        bytes[1] = bytes[2] = bytes[3] = 0;
        nbytes = 0;

        while (nbits > 8) {
            bytes[nbytes++] = xtc_receivebits(buf, 8);
            nbits -= 8;
        }

        if (nbits > 0) {
            bytes[nbytes++] = xtc_receivebits(buf, nbits);
        }

        for (i = nints - 1; i > 0; i--) {
            num = 0;
            for (j = nbytes - 1; j >= 0; j--) {
                num = (num << 8) | bytes[j];
                p = (num / (int)sizes[i]);
                bytes[j] = p;
                num = num - p * (int)sizes[i];
            }
            nums[i] = num;
        }

        nums[0] = bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
    }

    // function that reads compressed coordinates
    static unsafe void xtc_3dfcoord(BinaryReader reader, ref Vector3[] coords) {

        int s = 0;
        int* size = &s;
        float* precision;
        int oldsize;

        int[] minint = new int[3];
        int[] maxint = new int[3];
        int *lip;
        int smallidx;
        uint[] sizeint = new uint[3];
        uint[] sizesmall = new uint[3];
        uint[] bitsizeint = new uint[3];
        uint size3;
        int flag, k;
        int small, smaller, i, is_smaller, run;
        float* lfp;
        int tmp;
        int* thiscoord;
        int[] prevcoord = new int[3];

        int bufsize, lsize;
        uint bitsize;
        float inv_precision;

        /* avoid uninitialized data compiler warnings */
        bitsizeint[0] = 0;
        bitsizeint[1] = 0;
        bitsizeint[2] = 0;

        lsize = getInt32(reader);

        if (*size != 0 && lsize != *size) {
            throw new Exception("Error reading coordinates");
        };

        *size = lsize;
        size3 = (uint) * size * 3;
        if (*size <= 9) {
            for (i = 0; i < *size; i++) {
                coords[i].x = -getFloat32(reader) * 10;
                coords[i].y = getFloat32(reader) * 10;
                coords[i].z = getFloat32(reader) * 10;
            }
            return;
        }
        float[] buffer = new float[3 * coords.Length];

        float p = getFloat32(reader);
        precision = &p;

        int ipsize = (int)size3 * sizeof(int);
        fixed (int* ip = new int[ipsize]) {

            bufsize = (int)(size3 * 1.2f) * sizeof(int);

            fixed (int* buf = new int[bufsize]) {

                oldsize = *size;

                buf[0] = buf[1] = buf[2] = 0;

                minint[0] = getInt32(reader);
                minint[1] = getInt32(reader);
                minint[2] = getInt32(reader);

                maxint[0] = getInt32(reader);
                maxint[1] = getInt32(reader);
                maxint[2] = getInt32(reader);

                sizeint[0] = (uint)(maxint[0] - minint[0] + 1);
                sizeint[1] = (uint)(maxint[1] - minint[1] + 1);
                sizeint[2] = (uint)(maxint[2] - minint[2] + 1);

                /* check if one of the sizes is to big to be multiplied */
                if ((sizeint[0] | sizeint[1] | sizeint[2]) > 0xffffff) {
                    bitsizeint[0] = (uint)xtc_sizeofint((int)sizeint[0]);
                    bitsizeint[1] = (uint)xtc_sizeofint((int)sizeint[1]);
                    bitsizeint[2] = (uint)xtc_sizeofint((int)sizeint[2]);
                    bitsize = 0; /* flag the use of large sizes */
                }
                else {
                    bitsize = (uint)xtc_sizeofints(3, sizeint);
                }

                smallidx = getInt32(reader);
                smaller = xtc_magicints[FIRSTIDX > smallidx - 1 ? FIRSTIDX : smallidx - 1] / 2;
                small = xtc_magicints[smallidx] / 2;
                sizesmall[0] = sizesmall[1] = sizesmall[2] = (uint)xtc_magicints[smallidx];

                /* check for zero values that would yield corrupted data */
                if (sizesmall[0] == 0 || sizesmall[1] == 0 || sizesmall[2] == 0) {
                    throw new Exception("Error reading coordinates");
                }

                /* buf[0] holds the length in bytes */
                buf[0] = getInt32(reader);
                if (buf[0] < 1) {
                    throw new Exception("Error reading coordinates");
                }

                getData(reader, (byte*)&buf[3], buf[0]);

                buf[0] = buf[1] = buf[2] = 0;

                // start fixed fp
                fixed (float* fp = buffer)
                {
                    lfp = fp;
                    inv_precision = 1.0f / (*precision);
                    run = 0;
                    i = 0;
                    lip = ip;
                    while (i < lsize) {

                        thiscoord = (int*)(lip) + i * 3;

                        if (bitsize == 0) {
                            thiscoord[0] = xtc_receivebits(buf, (int)bitsizeint[0]);
                            thiscoord[1] = xtc_receivebits(buf, (int)bitsizeint[1]);
                            thiscoord[2] = xtc_receivebits(buf, (int)bitsizeint[2]);
                        }
                        else {
                            fixed (uint* sip = sizeint)
                            {
                                xtc_receiveints(buf, 3, (int)bitsize, sip, thiscoord);
                            }
                        }

                        i++;

                        thiscoord[0] += minint[0];
                        thiscoord[1] += minint[1];
                        thiscoord[2] += minint[2];

                        prevcoord[0] = thiscoord[0];
                        prevcoord[1] = thiscoord[1];
                        prevcoord[2] = thiscoord[2];


                        flag = xtc_receivebits(buf, 1);
                        is_smaller = 0;
                        if (flag == 1) {
                            run = xtc_receivebits(buf, 5);
                            is_smaller = run % 3;
                            run -= is_smaller;
                            is_smaller--;
                        }
                        if (run > 0) {
                            thiscoord += 3;
                            for (k = 0; k < run; k += 3) {

                                fixed (uint* ssp = sizesmall)
                                {
                                    xtc_receiveints(buf, 3, smallidx, ssp, thiscoord);
                                }
                                i++;
                                thiscoord[0] += prevcoord[0] - small;
                                thiscoord[1] += prevcoord[1] - small;
                                thiscoord[2] += prevcoord[2] - small;
                                if (k == 0) {
                                    /* interchange first with second atom for better
                                        * compression of water molecules
                                        */
                                    tmp = thiscoord[0];
                                    thiscoord[0] = prevcoord[0];
                                    prevcoord[0] = tmp;
                                    tmp = thiscoord[1];
                                    thiscoord[1] = prevcoord[1];
                                    prevcoord[1] = tmp;
                                    tmp = thiscoord[2];
                                    thiscoord[2] = prevcoord[2];
                                    prevcoord[2] = tmp;
                                    *lfp++ = prevcoord[0] * inv_precision;
                                    *lfp++ = prevcoord[1] * inv_precision;
                                    *lfp++ = prevcoord[2] * inv_precision;

                                    if (sizesmall[0] == 0 || sizesmall[1] == 0 || sizesmall[2] == 0) {
                                        throw new Exception("Error reading coordinates");
                                    }

                                }
                                else {
                                    prevcoord[0] = thiscoord[0];
                                    prevcoord[1] = thiscoord[1];
                                    prevcoord[2] = thiscoord[2];
                                }
                                *lfp++ = thiscoord[0] * inv_precision;
                                *lfp++ = thiscoord[1] * inv_precision;
                                *lfp++ = thiscoord[2] * inv_precision;
                            }
                        }
                        else {
                            *lfp++ = thiscoord[0] * inv_precision;
                            *lfp++ = thiscoord[1] * inv_precision;
                            *lfp++ = thiscoord[2] * inv_precision;
                        }
                        smallidx += is_smaller;
                        if (is_smaller < 0) {
                            small = smaller;
                            if (smallidx > FIRSTIDX) {
                                smaller = xtc_magicints[smallidx - 1] / 2;
                            }
                            else {
                                smaller = 0;
                            }
                        }
                        else if (is_smaller > 0) {
                            smaller = small;
                            small = xtc_magicints[smallidx] / 2;
                        }
                        sizesmall[0] = sizesmall[1] = sizesmall[2] = (uint)xtc_magicints[smallidx];
                    }

                } // end fixed fp
            } // end fixed buf
        } // end fixed ip
        for (int id = 0; id < coords.Length; id++) {
            coords[id].x = -buffer[id * 3]*10;
            coords[id].y = buffer[id * 3 + 1]*10;
            coords[id].z = buffer[id * 3 + 2]*10;
        }
    }
#endif
}
}
