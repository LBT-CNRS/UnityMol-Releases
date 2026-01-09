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
using System.Runtime.InteropServices;

namespace UMol {
	public enum XDRStatus
		{ exdrOK, exdrHEADER, exdrSTRING, exdrDOUBLE,
		exdrINT, exdrFLOAT, exdrUINT, exdr3DX, exdrCLOSE, exdrMAGIC,
		exdrNOMEM, exdrENDOFFILE, exdrFILENOTFOUND, exdrNR };

	// Provides function prototypes to use the xdrfile library.
	public static class XDRFileWrapper {
		// Opens a trajectory file located at "path" using the provided mode.
		// mode = "r" for read, mode = "w" for write.
		// Returns a file pointer to an xdr file datatype, or NULL if an error occurs.
		[DllImport ("xdrfile", CallingConvention=CallingConvention.Cdecl)]
		public static extern System.IntPtr xdrfile_open([In] string path, [In] string mode);

		// Closes a previously opened trajectory file passed in argument.
		// Returns 0 on success (XDRStatus.endrOK), non-zero on error.
		[DllImport ("xdrfile", CallingConvention=CallingConvention.Cdecl)]
		public static extern XDRStatus xdrfile_close([In] System.IntPtr xfp);

		// Returns the number of atoms in the xtc file into *natoms.
		// Returns 0 on success (XDRStatus.endrOK), non-zero on error.
		[DllImport ("xdrfile", CallingConvention=CallingConvention.Cdecl)]
		public static extern XDRStatus read_xtc_natoms([In] string filename, ref int natoms);

		// Reads one frame of an opened xtc file.
		// Returns 0 on success (XDRStatus.endrOK), non-zero on error.
		[DllImport ("xdrfile", CallingConvention=CallingConvention.Cdecl)]
		public static extern XDRStatus read_xtc(System.IntPtr xd, int natoms, ref int step, ref float time, float[,] box, [In, Out] float[] x, ref float prec);

		// Returns the number of atoms in the xtc file into *natoms.
		// Returns 0 on success (XDRStatus.endrOK), non-zero on error.
		[DllImport ("xdrfile", CallingConvention=CallingConvention.Cdecl)]
		public static extern XDRStatus read_trr_natoms([In] string filename, ref int natoms);

		// Reads one frame of an opened trr file.
		// Returns 0 on success (XDRStatus.endrOK), non-zero on error.
		[DllImport ("xdrfile", CallingConvention=CallingConvention.Cdecl)]
		public static extern XDRStatus read_trr(System.IntPtr xd, int natoms, ref int step, ref float time, ref float lambda, float[,] box, [In, Out] float[] x, [In, Out] float[] v, [In, Out] float[] f);

		[DllImport ("xdrfile", CallingConvention=CallingConvention.Cdecl)]
		public static extern int read_xtc_numframes([In] string filename, ref int natoms, ref System.IntPtr offsets);

		[DllImport ("xdrfile", CallingConvention=CallingConvention.Cdecl)]
		public static extern int read_trr_numframes([In] string filename, ref int natoms, ref System.IntPtr offsets);

		[DllImport ("xdrfile", CallingConvention=CallingConvention.Cdecl)]
		public static extern XDRStatus xdr_seek(System.IntPtr xd, long pos, int whence);

		[DllImport ("xdrfile", CallingConvention=CallingConvention.Cdecl)]
		public static extern long xdr_tell(System.IntPtr xd);
	}
}
