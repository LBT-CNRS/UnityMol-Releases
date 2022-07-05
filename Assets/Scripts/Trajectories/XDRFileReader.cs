/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2021
        Marc Baaden, 2010-2021
        baaden@smplinux.de
        http://www.baaden.ibpc.fr

        This software is a computer program based on the Unity3D game engine.
        It is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications. More details about UnityMol are provided at
        the following URL: "http://unitymol.sourceforge.net". Parts of this
        source code are heavily inspired from the advice provided on the Unity3D
        forums and the Internet.

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

        References : 
        If you use this code, please cite the following reference :         
        Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
        "Game on, Science - how video game technology may help biologists tackle
        visualization challenges" (2013), PLoS ONE 8(3):e57990.
        doi:10.1371/journal.pone.0057990
       
        If you use the HyperBalls visualization metaphor, please also cite the
        following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
        B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
        using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
        J. Comput. Chem., 2011, 32, 2924

    Please contact unitymol@gmail.com
    ================================================================================
*/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System;

namespace UMol {

// namespace Trajectories {
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

enum SEEK {
	SEEK_SET = 0,   /* set file offset to offset */
	SEEK_CUR = 1,   /* set file offset to current plus offset */
	SEEK_END = 2   /* set file offset to EOF plus offset */
}

//
// Performs I/O on an XTC file.
//
public class XDRFileReader {
	private int TRAJBUFFERSIZE = 40;

	public UnityMolStructure structure;
	public int currentFrame = 0;
	public int numberAtoms = 0;
	public int numberFrames = 0;

	public System.IntPtr file_pointer = System.IntPtr.Zero;
	public string offsetFileName;
	public long[] offsets;
	bool is_trr = false;

	float[,] box = new float[3, 3];

	public struct FrameInfo {
		public int step;
		public float time;
	}

	List<FrameInfo> frames_info;

	/// Buffer of frames containing TRAJBUFFERSIZE/2 frames before the current frame and TRAJBUFFERSIZE/2 after
	private Vector3[][] trajectoryBuffer;
	/// Get position of the frame in the buffer
	private Dictionary<int, int> frameToTrajBuffer = new Dictionary<int, int>();
	int idB = 0;
	private float[] trajectoryBufferF;
	private TrajectorySmoother trajSmoother;

	/// Initiate at frame 0 and returns the number of frames
	public int load_trajectory() {

		trajSmoother = new TrajectorySmoother();
		sync_scene_with_frame(0);

		return numberFrames;
	}

	/// Opens a trajectory file.
	public int open_trajectory(UnityMolStructure stru, string filename, bool is_trr = false) {
		if (file_pointer != System.IntPtr.Zero) {
			Debug.LogError("This instance has a trajectory already opened.");
			return (int) XDRFileReaderStatus.TRAJECTORYPRESENT;
		}
		if (stru.trajectoryLoaded) {
			Debug.LogError("This structure has a trajectory already opened.");
			return (int) XDRFileReaderStatus.TRAJECTORYPRESENT;
		}

		structure = stru;

		XDRStatus result;
		if (is_trr) {
			result = XDRFileWrapper.read_trr_natoms (filename, ref numberAtoms);
		} else {
			result = XDRFileWrapper.read_xtc_natoms (filename, ref numberAtoms);
		}

		if (result != XDRStatus.exdrOK) {
			Debug.LogError("Could not get number of atoms from file " + filename);
			return (int) XDRFileReaderStatus.FILENOTFOUND;
		}

		if (numberAtoms > structure.Count) {
			Debug.LogWarning("Trajectory has not the same number of atoms than the first model of the structure." + numberAtoms + " vs " + structure.Count);
//				numberAtoms = (int)MoleculeModel.atomsnumber;
		}

		file_pointer = XDRFileWrapper.xdrfile_open (filename, "r");
		if (file_pointer == System.IntPtr.Zero) {
			Debug.LogError("Could not open file " + filename);
			return (int) XDRFileReaderStatus.FILENOTFOUND;
		}

		int res = updateOffsetFile(filename);
		if (res != (int) XDRFileReaderStatus.SUCCESS) {
			if (res == (int) XDRFileReaderStatus.OFFSETFILETREAD) {
				Debug.LogError("Could not read offset file " + offsetFileName);
				return (int) XDRFileReaderStatus.OFFSETFILETREAD;
			}

			Debug.LogError("Could not create offset file " + offsetFileName);
			return (int) XDRFileReaderStatus.OFFSETFILECREATION;
		}


		this.is_trr = is_trr;
		structure.trajectoryLoaded = true;
		return numberAtoms;
	}

	bool diffFrames(Vector3[] f1, Vector3[] f2) {
		if (f1.Length != f2.Length) {
			return false;
		}
		for (int i = 0; i < f1.Length; i++) {
			if (!Mathf.Approximately(f1[i].x, f2[i].x) ||
			        !Mathf.Approximately(f1[i].y, f2[i].y) ||
			        !Mathf.Approximately(f1[i].z, f2[i].z)) {
				return false;
			}
		}

		return true;
	}

	public Vector3[] getFrame(int frame_number) {
		if (trajectoryBuffer == null || trajectoryBuffer[0] == null) {
			if (numberFrames < TRAJBUFFERSIZE) {
				TRAJBUFFERSIZE = numberFrames;
			}
			trajectoryBuffer = new Vector3[TRAJBUFFERSIZE][];
			for (int i = 0; i < TRAJBUFFERSIZE; i++) {
				trajectoryBuffer[i] = new Vector3[numberAtoms];
			}
			trajectoryBufferF = new float[numberAtoms * 3];
		}

		if (frameToTrajBuffer.ContainsKey(frame_number)) { //Already in memory
			return trajectoryBuffer[frameToTrajBuffer[frame_number]];
		}

		//Not in memory => load TRAJBUFFERSIZE/4 before and after = fills half of the buffer
		loadBufferFrames(frame_number);

		return trajectoryBuffer[frameToTrajBuffer[frame_number]];
	}

	/// Load TRAJBUFFERSIZE/4 frames before and after frame_number
	private void loadBufferFrames(int frame_number) {
		int i = 0;

		int startF = frame_number - TRAJBUFFERSIZE / 4;
		while (i < TRAJBUFFERSIZE / 2) {//While not filled half of the array
			if (startF + i < 0) {
				startF++;
				continue;
			}
			if (startF + i >= numberFrames) {
				startF = -i;
				continue;
			}
			int idF = startF + i;
			loadOneFrame(idF);
			i++;
		}
	}

	// Load one frame in the buffer and manage the associated dictionary
	private Vector3[] loadOneFrame(int frame_number) {
		int res = (int)XDRFileWrapper.xdr_seek(file_pointer, offsets[frame_number], (int) SEEK.SEEK_SET);

		int step = 0;
		float time = 0f;
		float precision = 0f;
		int status = next_frame(ref step, ref time, trajectoryBufferF, ref precision);

		for (int i = 0; i < numberAtoms; i++) {
			//Should be a minus sign here
			trajectoryBuffer[idB][i] = new Vector3(-trajectoryBufferF[i * 3], trajectoryBufferF[i * 3 + 1], trajectoryBufferF[i * 3 + 2]) * 10.0f;
		}

		//Remove previous value
		int fToDel = -1;
		foreach(var f in frameToTrajBuffer){
			if(f.Value == idB){
				fToDel = f.Key;
			}
		}
		if(fToDel != -1)
			frameToTrajBuffer.Remove(fToDel);

		Vector3[] frame = trajectoryBuffer[idB];

		frameToTrajBuffer[frame_number] = idB;

		idB++;
		if (idB == TRAJBUFFERSIZE) {
			idB = 0;
		}
		return frame;
	}


	public int updateOffsetFile(string trajFile, bool forceCreate = false) {
		offsetFileName = trajFile + ".offset";
		bool offsetExists = File.Exists(offsetFileName);

		bool generateOffsetFile = false;
		if (forceCreate) {
			return createOffsetFile(trajFile);
		}
		if (offsetExists) {
			DateTime lastModif = File.GetLastWriteTime(offsetFileName);
			DateTime creationTraj = File.GetLastWriteTime(trajFile);
			if (lastModif > creationTraj) { //Offset file is posterior to creation of traj file
				try {
					return readOffsetFile();
				}
				catch {//Failed to read offset file => create one
				}
			}
		}

		return createOffsetFile(trajFile);

	}

	int createOffsetFile(string fileName) {
		try {
			if (!is_trr) {//XTC

				IntPtr outOffsets = IntPtr.Zero;
				int res = 0;
				res = (int)XDRFileWrapper.read_xtc_numframes(fileName, ref numberFrames, ref outOffsets);
				if (res != (int) XDRStatus.exdrOK) {
					return res;
				}
				offsets = new long[numberFrames];
				Marshal.Copy(outOffsets, offsets, 0, numberFrames);
				// Marshal.FreeCoTaskMem(outOffsets);
			}
			else {
				IntPtr outOffsets = IntPtr.Zero;
				int res = 0;
				res = (int)XDRFileWrapper.read_trr_numframes(fileName, ref numberFrames, ref outOffsets);
				if (res != (int) XDRStatus.exdrOK) {
					return res;
				}
				offsets = new long[numberFrames];
				Marshal.Copy(outOffsets, offsets, 0, numberFrames);
				// Marshal.FreeCoTaskMem(outOffsets);
			}

			BinaryWriter bw = new BinaryWriter(new FileStream(offsetFileName, FileMode.Create));
			bw.Write((Int32)numberFrames);
			for (int i = 0; i < numberFrames; i++) {
				bw.Write(offsets[i]);
			}
			bw.Close();
		}
		catch (System.Exception e) {
			Debug.LogError(e);
			return (int) XDRFileReaderStatus.OFFSETFILECREATION;
		}

		return (int) XDRFileReaderStatus.SUCCESS;
	}
	int readOffsetFile() {
		try {
			BinaryReader br = new BinaryReader(new FileStream(offsetFileName, FileMode.Open));
			numberFrames = br.ReadInt32();
			if (numberFrames > 0 && numberFrames < 2e9) { //2 billion seems like a fair upper limit for trajectories
				offsets = new long[numberFrames];
				for (int i = 0; i < numberFrames; i++) {
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

// Reads the next frame from the trajectory.
// Feeds parameters passed by reference.
// The array of positions is required to have a size == sizeof(float) * numberAtoms * 3 for this to work properly.
	public int next_frame(ref int step, ref float time, [In, Out] float[] positions, ref float precision) {
		if (file_pointer == System.IntPtr.Zero) {
			Debug.LogWarning("Trajectory was not previously opened.");
			return (int) XDRFileReaderStatus.FILENOTFOUND;
		}

		if (is_trr) {
			float lambda = 0.0f;
			int res = 0;
			res = (int) XDRFileWrapper.read_trr (file_pointer, numberAtoms, ref step, ref time, ref lambda, box, positions, null, null);
			return res;
		} else {
			return (int) XDRFileWrapper.read_xtc (file_pointer, numberAtoms, ref step, ref time, box, positions, ref precision);
		}
	}

	public int sync_scene_with_frame(int frame_number) {
		if (frame_number >= numberFrames) {
			Debug.LogError("Frame number " + frame_number + " does not exist.");
			return (int) XDRFileReaderStatus.FRAMEDOESNOTEXIST;
		}

		Vector3[] f = getFrame(frame_number);

		structure.trajAtomPositions = f;

		structure.trajUpdateAtomPositions();

		currentFrame = frame_number;
		return (int) XDRFileReaderStatus.SUCCESS;
	}

	public int close_trajectory() {

		trajSmoother.clear();

		if (file_pointer == System.IntPtr.Zero) {
			//Debug.LogWarning("Trajectory was not previously opened.");
			return (int) XDRFileReaderStatus.FILENOTFOUND;
		}

		XDRStatus result = XDRFileWrapper.xdrfile_close(file_pointer);

		offsets = null;
		file_pointer = System.IntPtr.Zero;
		numberAtoms = 0;
		numberFrames = 0;
		frameToTrajBuffer.Clear();
		structure.trajectoryLoaded = false;

		return (int) result;
	}
	public void Clear() {
		close_trajectory();
		//Clear atomsIMDSimulationLocationlist
		if (structure.trajAtomPositions != null) {
			structure.trajAtomPositions = null;
		}
	}
	public int sync_scene_with_frame_smooth(int frame1, int frame2, float t, bool new_frame = false) {
		if (frame1 >= numberFrames || frame1 < 0) {
			Debug.LogError("Frame number " + frame1 + " does not exist.");
			return (int) XDRFileReaderStatus.FRAMEDOESNOTEXIST;
		}
		if (frame2 >= numberFrames || frame2 < 0) {
			Debug.LogError("Frame number " + frame2 + " does not exist.");
			return (int) XDRFileReaderStatus.FRAMEDOESNOTEXIST;
		}

		t = Mathf.Clamp(t, 0.0f, 1.0f);

		Vector3[] f1 = getFrame(frame1);
		Vector3[] f2 = getFrame(frame2);


		trajSmoother.init(f1, f2);
		trajSmoother.process(structure.trajAtomPositions, t);

		structure.trajUpdateAtomPositions();

		//Not always updating currentFrame
		if (new_frame)
			currentFrame = frame1;

		return (int) XDRFileReaderStatus.SUCCESS;


	}
}
}
