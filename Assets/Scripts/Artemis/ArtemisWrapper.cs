/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Sebastien Doutreligne, 2017
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

using ArtemisClientPointer = System.IntPtr;

namespace UMol{
public class ArtemisWrapper {

    // IMD protocol version
    public static int ARTEMIS_IMD_VERSION = 2;
    // Extended IMD protocol version = max value of a 32-bit signed integer - 1 = 2^31-1
    public static int ARTEMIS_EIMD_VERSION = 2147483647;

    // Header types
    public enum artemis_imd_type_e {
        IMD_DISCONNECT = 0, //Client disconnect from server
        IMD_ENERGIES = 1, // Energies structures
        IMD_FCOORDS = 2, // Atoms coordinates as floating point numbers
        IMD_GO = 3, // Client wants the simulation to run
        IMD_HANDSHAKE = 4, // Handshake for endian consistency
        IMD_KILL = 5, // Client wants to shutdown the simulation
        IMD_MDCOMM = 6, // Client sends forces to the simulation
        IMD_PAUSE = 7, // Client wants to pause the simulation for a while
        IMD_TRATE = 8, // Client sets the IMD transmission rate
        IMD_IOERROR = 9, // An I/O error occured
        EIMD_SAXS = 10,
        EIMD_RESTRAINT = 11,
        EIMD_POSITION_RESTRAINT = 12,
        EIMD_RESTRAINT_DELETE = 13,
        EIMD_POSITION_RESTRAINT_DELETE = 14,
    };

    public struct ArtemisHeader
    {
        public artemis_imd_type_e type;
        public int length;
    };

    public struct ArtemisImdEnergies
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

    // Structure of a spring restraint on atoms i and j.
    public struct ArtemisEimdRestraint {
        public int atom_i;
        public int atom_j;
        public int trest;
        public double constant;
        public double equilibrium_distance;
        public double drestl;
    }

    // Structure of a position restraint on atom i.
    public struct ArtemisEimdPositionRestraint {
        public int atom_i; // Atom id
        public double k;   // Restraint constant
        public double equilibrium_position_x;
        public double equilibrium_position_y;
        public double equilibrium_position_z;
        public double equilibrium_position_delta_x;
        public double equilibrium_position_delta_y;
        public double equilibrium_position_delta_z;
    }

    [DllImport ("artemis")]
    public static extern ArtemisClientPointer artemis_client_create(int nb_atoms);
    
    [DllImport ("artemis")]
    public static extern int artemis_client_destroy(ArtemisClientPointer client);
    
    [DllImport ("artemis")]
    public static extern int artemis_client_connect(ArtemisClientPointer client, [In] string hostname, [In] int port);
    
    [DllImport ("artemis")]
    public static extern int artemis_client_disconnect(ArtemisClientPointer client);

    [DllImport ("artemis")]
    public static extern int artemis_client_get_protocol_version(ArtemisClientPointer client);

    [DllImport ("artemis")]
    public static extern int artemis_client_receive(ArtemisClientPointer client);

    [DllImport ("artemis")]
    public static extern int artemis_client_read_header(ArtemisClientPointer client, ref ArtemisHeader header);

    [DllImport ("artemis")]
    public static extern int artemis_client_read_energies(ArtemisClientPointer client, ref ArtemisImdEnergies energies);

    [DllImport ("artemis")]
    public static extern int artemis_client_read_coords(ArtemisClientPointer client, [In, Out] float[] coords, int nb_atoms);

    [DllImport ("artemis")]
    public static extern int artemis_client_read_saxs_curve(ArtemisClientPointer client, [In, Out] float[] curve);
    
    [DllImport ("artemis")]
    public static extern int artemis_client_send_forces(ArtemisClientPointer client, int nb_forces, [In] int[] indexes, [In] float[] forces);

    [DllImport ("artemis")]
    public static extern int artemis_client_send_go(ArtemisClientPointer client);

    [DllImport ("artemis")]
    public static extern int artemis_client_send_kill(ArtemisClientPointer client);

    [DllImport ("artemis")]
    public static extern int artemis_client_send_pause(ArtemisClientPointer client);

    [DllImport ("artemis")]
    public static extern int artemis_client_send_trate(ArtemisClientPointer client, int rate);

    [DllImport ("artemis")]
    public static extern int artemis_client_send_restraint(ArtemisClientPointer client, ArtemisEimdRestraint restraint);

    [DllImport ("artemis")]
    public static extern int artemis_client_send_position_restraint(ArtemisClientPointer client, ArtemisEimdPositionRestraint restraint);

    [DllImport ("artemis")]
    public static extern int artemis_client_send_restraint_delete(ArtemisClientPointer client, int restraint_id);

    [DllImport ("artemis")]
    public static extern int artemis_client_send_position_restraint_delete(ArtemisClientPointer client, int restraint_id);
}
}