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
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine.XR;

using VRTK;

using ArtemisClientPointer = System.IntPtr;

namespace UMol {

public class ArtemisThreadedClient {

    public ArtemisManager artemisM;
    // GameObject artemisGo;

    private float[] pos2;

    private bool run = false;
    private bool kill_simulation = false;

    private int skip = 30;

    public void stop_simulation(ArtemisClientPointer client)
    {
        kill_simulation = true;
    }

    public void disconnect(ArtemisClientPointer client)
    {
        ArtemisWrapper.artemis_client_disconnect(client);
    }

    public void mainProc(ArtemisClientPointer client, int capacity)
    {
        ArtemisWrapper.ArtemisHeader header = new ArtemisWrapper.ArtemisHeader();
        ArtemisWrapper.ArtemisImdEnergies e = new ArtemisWrapper.ArtemisImdEnergies();

        kill_simulation = false;

        pos2 = new float[capacity * 3];
        run = true;
        while (run)
        {
            int ready = ArtemisWrapper.artemis_client_receive(client);
            if (ready == 1)
            {
                // Get the IMD header
                ArtemisWrapper.artemis_client_read_header(client, ref header);

                switch (header.type)
                {
                case ArtemisWrapper.artemis_imd_type_e.IMD_ENERGIES:
                    ArtemisWrapper.artemis_client_read_energies(client, ref e);

                    if (e.tstep % skip == 0) {
                        // PlotManager.Instance.PlotAdd("Total Energy (kcal/mol)", e.Etot);
                        // PlotManager.Instance.PlotAdd("Hydrogen Bonds Energy (kcal/mol)", e.Evdw);
                        // PlotManager.Instance.PlotAdd("Stacking Energy (kcal/mol)", e.Eelec);

                        artemisM.Etot = e.Etot;
                        artemisM.Evdw = e.Evdw;
                        artemisM.Eelec = e.Eelec;
                    }
                    break;
                case ArtemisWrapper.artemis_imd_type_e.IMD_FCOORDS:

                    if (ArtemisWrapper.artemis_client_read_coords(client, pos2, capacity) == 1)
                    {

                        for (int i = 0; i < capacity; i++) {
                            if (float.IsNaN(pos2[i * 3]) || float.IsInfinity(pos2[i * 3]) || Mathf.Abs(pos2[i * 3]) > 10000.0f ) {
                                artemisM.positions[i] = Vector3.one * -100.0f;
                                continue;
                            }
                            if (float.IsNaN(pos2[i * 3 + 1]) || float.IsInfinity(pos2[i * 3 + 1]) || Mathf.Abs(pos2[i * 3 + 1]) > 10000.0f ) {
                                artemisM.positions[i] = Vector3.one * -100.0f;
                                continue;
                            }
                            if (float.IsNaN(pos2[i * 3 + 2]) || float.IsInfinity(pos2[i * 3 + 2]) || Mathf.Abs(pos2[i * 3 + 2]) > 10000.0f ) {
                                artemisM.positions[i] = Vector3.one * -100.0f;
                                continue;
                            }

                            artemisM.positions[i].x = -pos2[i * 3] - artemisM.translation.x;
                            artemisM.positions[i].y = pos2[i * 3 + 1] - artemisM.translation.y;
                            artemisM.positions[i].z = pos2[i * 3 + 2] - artemisM.translation.z;
                        }

                        // artemisM.structure.trajAtomPositions.Clear();
                        // artemisM.structure.trajAtomPositions.AddRange(lst);
                        // artemisM.structure.trajAtomPositions = lst;
                        artemisM.needsUpdate = true;
                        // artemisM.structure.updateRepresentations(trajectory: true);
                        // //trajectory set to false to force updating collider position
                        // artemisM.structure.updateMeshCollider(trajectory: false);
                    }

                    break;
                // case ArtemisWrapper.artemis_imd_type_e.EIMD_SAXS:
                //     float[] curve = new float[header.length];
                //     ArtemisWrapper.artemis_client_read_saxs_curve(client, curve);
                //     List<Vector2> curve_vectors = new List<Vector2>();

                //     for (int i = 0; i < curve.Length; i++) {
                //         curve_vectors.Add(new Vector2(i*0.005f, Mathf.Log10(curve[i])));
                //     }

                //     if (ArtemisClientLoop.saxsPanel != null)
                //         ArtemisClientLoop.saxsPanel.SetInteractiveValues(curve_vectors.ToArray());
                //     break;
                default:
                    break;
                }
            } else if (ready == -1) { // Server connection was shut.
                stop();
            }
        }


        // Either we send a KILL signal to the server, either we disconnect
        // We can't disconnect then send a kill signal, otherwise the kill signal will never be handled.
        if (kill_simulation) {
            ArtemisWrapper.artemis_client_send_kill (client);
        } else {
            disconnect (client);
        }
    }

    public void send_forces(ArtemisClientPointer client, int nb_forces, int[] indices, float[] coordinates)
    {
        ArtemisWrapper.artemis_client_send_forces(client, nb_forces, indices, coordinates);
    }

    public int send_restraint(ArtemisClientPointer client, ArtemisWrapper.ArtemisEimdRestraint restraint)
    {
        return ArtemisWrapper.artemis_client_send_restraint(client, restraint);
    }

    public int send_restraint_delete(ArtemisClientPointer client, int restraint_id)
    {
        return ArtemisWrapper.artemis_client_send_restraint_delete(client, restraint_id);
    }

    public int send_position_restraint(ArtemisClientPointer client, ArtemisWrapper.ArtemisEimdPositionRestraint position_restraint)
    {
        return ArtemisWrapper.artemis_client_send_position_restraint(client, position_restraint);
    }

    public int send_position_restraint_delete(ArtemisClientPointer client, int restraint_id)
    {
        return ArtemisWrapper.artemis_client_send_position_restraint_delete(client, restraint_id);
    }

    public void stop()
    {
        run = false;
    }
}

public class ArtemisManager : MonoBehaviour {

    ArtemisThreadedClient threadedClient;

    ArtemisClientPointer client;

    Thread oThread;
    // GameObject artemisGo;

    bool initialized = false;
    private bool connected = false;

    public float Etot = 0.0f;
    public float Evdw = 0.0f;
    public float Eelec = 0.0f;
    public int protocol_version = 0;

    public Vector3[] positions;
    public UnityMolStructure structure;
    public bool needsUpdate = false;

    private Transform clref;
    private Transform crref;

    private List<float> coordinatesForces = new List<float>();
    private List<int> indicesForces = new List<int>();
    private List<int> savedIndices = new List<int>();


    private Vector3 initCoG = Vector3.zero;
    private Vector3 initCoGIMD = Vector3.zero;
    public Vector3 translation = Vector3.zero;
    bool first = true;


    public bool isConnected()
    {
        return connected;
    }

    public bool connect(string host, int port)
    {
        indicesForces.Clear();
        coordinatesForces.Clear();
        first = true;

        if (structure.trajectoryLoaded) {
            Debug.LogError("A trajectory is loaded for this structure");
            return false;
        }

        if (isConnected()) {
            disconnect();
            return false;
        }
        threadedClient = new ArtemisThreadedClient();
        threadedClient.artemisM = this;


        // ArtemisOldGUI.artemisNotificationMessage = "Connecting...";

        int capacity = structure.Count;
        positions = new Vector3[capacity];

        structure.trajAtomPositions = new Vector3[capacity];

        if (!initialized)
        {
            client = ArtemisWrapper.artemis_client_create(capacity);
            initialized = true;
        }


        Debug.Log ("Artemis Threaded Client connect");
        int res = ArtemisWrapper.artemis_client_connect(client, host, port);


        if (res == 0)
        {
            protocol_version = ArtemisWrapper.artemis_client_get_protocol_version(client);

            if (protocol_version != ArtemisWrapper.ARTEMIS_IMD_VERSION && protocol_version != ArtemisWrapper.ARTEMIS_EIMD_VERSION) {
                Debug.LogError("Wrong protocol version");
                // ArtemisOldGUI.artemisNotificationMessage = "Wrong protocol number";
                ArtemisWrapper.artemis_client_disconnect(client);
                return false;
            }

            initCoG = structure.currentModel.centerOfGravity;

            // ArtemisOldGUI.artemisNotificationMessage = "Connected to " + ArtemisOldGUI.host + ":" + ArtemisOldGUI.portString;

            // artemisGo = new GameObject("Artemis");
            // artemisGo.AddComponent<ArtemisClientLoop>();

            // GenericAtomManager atomManager = (GenericAtomManager) UnityMolMain.getCurrentAtomManager();
            // atomManager.CreateMouseOversIMDSimulation();
            // GUIMoleculeController.toggle_IMD = true;
            // GUIMoleculeController.toggle_IMD_already = true;

            oThread = new Thread(() => threadedClient.mainProc(client, capacity));
            oThread.Start();
            while (!oThread.IsAlive);

            connected = true;
            switchControllerMode(true);

        }
        else
        {
            Debug.LogError("Can't connect to " + host + ":" + port);
            connected = false;
            Clear();
            return false;
        }
        return true;
    }

    // What to do when the connection is closed/lost
    // (either from disconnect, stop, or something else)
    public void clearConnection()
    {
        if (connected) {
            connected = false;
            protocol_version = 0;

            // GameObject.DestroyImmediate(artemisGo);

            threadedClient.stop();

            oThread.Join();

            ArtemisWrapper.artemis_client_destroy(client);

            initialized = false;

            Debug.Log ("Thread joined");

        }
        first = true;
        coordinatesForces.Clear();
        indicesForces.Clear();
    }

    public void stop_simulation()
    {
        if (connected)
        {
            threadedClient.stop_simulation(client);
            clearConnection();
        }
        switchControllerMode(false);
    }

    public void disconnect()
    {
        Debug.Log("ArtemisManager::disconnect");
        if (connected)
        {
            clearConnection();
        }

        switchControllerMode(false);

    }

    void switchControllerMode(bool IMDOn) {

        if (UnityMolMain.inVR()) {
            if (clref == null)
                clref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
            if (crref == null)
                crref = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);

            if (clref != null) {
                clref.gameObject.GetComponent<PointerAtomSelection>().enabled = !IMDOn;
                clref.gameObject.GetComponent<PointerIMD>().enabled = IMDOn;
            }
            if (crref != null) {
                crref.gameObject.GetComponent<PointerAtomSelection>().enabled = !IMDOn;
                crref.gameObject.GetComponent<PointerIMD>().enabled = IMDOn;
            }

        }

    }

    void Update() {

        if (needsUpdate) {
            if (first) {
                initCoGIMD = UnityMolModel.ComputeCenterOfGravity(positions);
                translation = initCoGIMD - initCoG;
                first = false;
            }

            // for (int i = 0; i < positions.Length; i++) {
            //     positions[i] = positions[i] - translation;
            // }


            structure.trajAtomPositions = positions;
            structure.trajUpdateAtomPositions();
            structure.updateRepresentations(trajectory: true);

            needsUpdate = false;
        }
        if (indicesForces.Count != 0) {
            send_forces(indicesForces.Count, indicesForces.ToArray(), coordinatesForces.ToArray());
            coordinatesForces.Clear();
            indicesForces.Clear();
        }
    }

    public void clearForces(){
        clearPrevForces(savedIndices);
        savedIndices.Clear();
    }
    public void addForce(int id, float[] coords) {
        indicesForces.Add(id);
        coordinatesForces.Add(coords[0]);
        coordinatesForces.Add(coords[1]);
        coordinatesForces.Add(coords[2]);
        savedIndices.Add(id);
    }

    private void clearPrevForces(List<int> prevIds) {
        if(prevIds.Count == 0){
            return;
        }
        float[] coordszero = new float[3 * prevIds.Count];
        send_forces(prevIds.Count, prevIds.ToArray(), coordszero);
    }
    private void send_forces(int nb_forces, int[] indices, float[] coordinates)
    {
        threadedClient.send_forces(client, nb_forces, indices, coordinates);
    }

    public int send_restraint(ArtemisWrapper.ArtemisEimdRestraint restraint)
    {
        return threadedClient.send_restraint(client, restraint);
    }

    public int send_restraint_delete(int restraint_id)
    {
        return threadedClient.send_restraint_delete(client, restraint_id);
    }

    public int send_position_restraint(ArtemisWrapper.ArtemisEimdPositionRestraint position_restraint)
    {
        return threadedClient.send_position_restraint(client, position_restraint);
    }

    public int send_position_restraint_delete(int restraint_id)
    {
        return threadedClient.send_position_restraint_delete(client, restraint_id);
    }
    public void Clear() {

        clearConnection();
        switchControllerMode(false);
    }
}
}