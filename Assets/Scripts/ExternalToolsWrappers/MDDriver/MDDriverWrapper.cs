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
using System.Runtime.InteropServices;

namespace UMol {
/// <summary>
/// Class containing only static methods linked to the MDDriver library
/// </summary>
public static class MDDriverWrapper {

    [DllImport ("Unity_MDDriver")]
    public static extern IntPtr createMDDriverInstance();

    [DllImport ("Unity_MDDriver")]
    public static extern void deleteMDDriverInstance(IntPtr instance);

    [DllImport ("Unity_MDDriver")]
    public static extern void MDDriver_init(IntPtr instance, string hostname, int port);

    [DllImport ("Unity_MDDriver")]
    public static extern int MDDriver_start(IntPtr instance);

    [DllImport ("Unity_MDDriver")]
    public static extern int MDDriver_stop(IntPtr instance);

    [DllImport ("Unity_MDDriver")]
    public static extern bool MDDriver_isConnected(IntPtr instance);

    [DllImport ("Unity_MDDriver")]
    public static extern int MDDriver_getNbParticles(IntPtr instance);

    [DllImport ("Unity_MDDriver")]
    public static extern int MDDriver_getPositions(IntPtr instance, [In, Out] float[] verts, int nbParticles);

    [DllImport ("Unity_MDDriver")]
    public static extern void MDDriver_pause(IntPtr instance);

    [DllImport ("Unity_MDDriver")]
    public static extern void MDDriver_play(IntPtr instance);

    [DllImport ("Unity_MDDriver")]
    public static extern void MDDriver_setForces(IntPtr instance, int nbforces, int[] atomslist, float[] forceslist);

    [DllImport ("Unity_MDDriver")]
    public static extern void MDDriver_getEnergies(IntPtr instance, ref IMDEnergies energies);

    [DllImport ("Unity_MDDriver")]
    public static extern int MDDriver_loop(IntPtr instance);

    [DllImport ("Unity_MDDriver")]
    public static extern void MDDriver_disconnect(IntPtr instance);


    public struct IMDEnergies
    {
        public int tstep;  //!< integer timestep index
        public float T;          //!< Temperature in degrees Kelvin
        public float Etot;       //!< Total energy, in Kcal/mol
        public float Epot;       //!< Potential energy, in Kcal/mol
        public float Evdw;       //!< Van der Waals energy, in Kcal/mol
        public float Eelec;      //!< Electrostatic energy, in Kcal/mol
        public float Ebond;      //!< Bond energy, Kcal/mol
        public float Eangle;     //!< Angle energy, Kcal/mol
        public float Edihe;      //!< Dihedral energy, Kcal/mol
        public float Eimpr;
    };

}
}
