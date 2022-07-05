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
using System.Runtime.InteropServices;

// namespace Trajectories {
	public enum XDRStatus 
		{ exdrOK, exdrHEADER, exdrSTRING, exdrDOUBLE, 
		exdrINT, exdrFLOAT, exdrUINT, exdr3DX, exdrCLOSE, exdrMAGIC,
		exdrNOMEM, exdrENDOFFILE, exdrFILENOTFOUND, exdrNR };

	// Provides function prototypes to use the xdrfile library.
	public class XDRFileWrapper {
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
// }