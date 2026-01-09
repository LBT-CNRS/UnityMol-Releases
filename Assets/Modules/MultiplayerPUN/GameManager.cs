using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using Photon.Voice.PUN;

using UMol.API;

/**
 * Authors: Mariano Spivak, Rajkumar Darbar
 **/

namespace UMol {

/// <summary>
/// Global class to handle Photon Multiplayer mode
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks
{
    #region Public Fields

    /// <summary>
    /// Prefab of a player
    /// </summary>
    [Tooltip("The prefab to use for representing the player")]
    public GameObject PlayerPrefab;

    /// <summary>
    /// References to UI buttons (both VR and Desktop ones) named "Request Ownership"
    /// Active when the player is not owner
    /// </summary>
    [SerializeField]
    private List<GameObject> UIRequestOwnership;

    /// <summary>
    /// References to UI buttons (both VR and Desktop ones) named "Ownership granted"
    /// Active when the player is the owner
    /// </summary>
    [SerializeField]
    private List<GameObject> UIOwnershipGranted;


    /// <summary>
    /// Is the player will receive Unity API commands from the owner?
    /// </summary>
    public bool IsFollowingPunCommands = true;

    #endregion

    /// <summary>
    /// Instance of the player prefab
    /// </summary>
    private GameObject playerInstance;
    /// <summary>
    /// Instance of the player prefab
    /// </summary>
    private GameObject localPlayerInstance;

    /// <summary>
    /// Reference to the player controllers
    /// Activated when the player is the owner. Deactivated otherwise
    /// </summary>
    private InteractionToggleManager interactionToggleManager;

    /// <summary>
    /// PhotonView component of the Game Object "Photon Network"
    /// </summary>
    [SerializeField]
    private new PhotonView photonView;

    /// <summary>
    /// PhotonView component of the Game Object "LoadedMolecules"
    /// </summary>
    [SerializeField]
    private PhotonView loadedMoleculesPhotonView;

    /// <summary>
    /// The transform of the XR Origin in VR.
    /// Needed to make the local player a child of the XR origin for locomotion purposes.
    /// </summary>
    [SerializeField]
    [Tooltip("[Only in VR] The Transform of the XR Origin.")]
    private Transform XROriginTransform;

    /// <summary>
    /// PhotonView components for each structure loaded
    /// </summary>
    private Dictionary<string, PhotonView> molViewPairs = new();

    /// <summary>
    /// True when the viewID is ready to add PhotonView components in the <see cref="molViewPairs"/>
    /// </summary>
    [SerializeField]
    private bool viewIDReady;

    /// <summary>
    /// Use to communicate the viewID and store it until molecule is ready
    /// </summary>
    private Dictionary<string, int> molViewIDPairs = new();


    private void Awake()
    {
        // Player Prefab initialization for later instantiation
        // Create a PhotonPrefabPool with the Player Prefab
        if (PlayerPrefab != null)
        {
            Dictionary<string, GameObject> prefabDictionary = new() {
                { "playerAvatar", PlayerPrefab }
            };

            PhotonNetwork.PrefabPool = new CustomPrefabPool(prefabDictionary);
        }

        interactionToggleManager = GetComponent<InteractionToggleManager>();

    }


    #region Public Methods

    /// <summary>
    /// When the local player is disconnected or leave the room
    /// </summary>
    public override void OnLeftRoom()
    {
        //Reset the UI elements
        foreach (GameObject go in UIRequestOwnership) { go.SetActive(true); }
        foreach (GameObject go in UIOwnershipGranted) { go.SetActive(false); }
    }

    /// <summary>
    /// When a player is disconnected
    /// </summary>
    public override void OnDisconnected(DisconnectCause cause)
    {
        UnityMolMain.pythonCommands.Changed -= OnPythonCommands_Changed;
        UnityMolStructureManager.OnMoleculeLoaded -= OnNewMolecule;
        Destroy(localPlayerInstance);
        //Should photon views on molecules be destroyed also?
    }

    /// <summary>
    /// When a player join the room
    /// </summary>
    public void InstantiatePlayer()
    {
        // Store the mode (VR or not) of the local player in CustomProperties
        // It will be sync trough 'InstantiatePlayer()' to others players
        ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        playerProperties["inVR"] = UnityMolMain.inVR();
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            playerInstance = PhotonNetwork.Instantiate("playerAvatar", new Vector3(0f,0f,0f), Quaternion.identity);
        }

        if (playerInstance == null) {
            return;
        }

        // need to search for its PhotonView and check it is mine, then deactivate the head/hands models
        if (!playerInstance.GetComponent<PhotonView>().IsMine) {
            return;
        }

        // In VR, make the instance of the local player a child of the XR Origin.
        // This will allow to track the joystick locomotion.
        if ((UnityMolMain.inVR())) {
            if (XROriginTransform) {
                playerInstance.transform.SetParent(XROriginTransform);
            }
    }

        // hide local player models
        playerInstance.GetComponent<NetworkPlayer>().HideLocalModels();
        // also, enable the trackedposedriver
        playerInstance.GetComponent<NetworkPlayer>().ActivateTrackedPoseDriver();
        // initialize the local player recorder
        GetComponent<PunVoiceClient>().PrimaryRecorder = playerInstance.GetComponent<Recorder>();

        // subscribe to commands
        UnityMolMain.pythonCommands.Changed += OnPythonCommands_Changed;
        UnityMolStructureManager.OnMoleculeLoaded += OnNewMolecule;
        // finally save local player instance
        localPlayerInstance = playerInstance;
    }

    /// <summary>
    /// Should the other players receive the UnityMol API commands sent by the owner?
    /// </summary>
    /// <param name="toggle">True to Activate, False to deactivate</param>
    public void ToggleFollowCommands(bool toggle)
    {
        IsFollowingPunCommands = toggle;
    }

    /// <summary>
    /// Activate/Desactivate Voice Chat
    /// </summary>
    /// <param name="toggle">True to Activate, False to deactivate</param>
    public void ToggleUsePunVoice(bool toggle)
    {
        if (localPlayerInstance != null) {
            localPlayerInstance.GetComponent<Recorder>().TransmitEnabled = toggle;
        }
    }

    /// <summary>
    /// Activate/Desactivate the Debug Voice Chat.
    /// If activated, player will hear himself
    /// </summary>
    /// <param name="toggle">True to Activate, False to deactivate</param>
    public void ToggleUseDebugVoice(bool toggle)
    {
        if (localPlayerInstance != null) {
            localPlayerInstance.GetComponent<Recorder>().DebugEchoMode = toggle;
        }
    }

    /// <summary>
    /// Wrapper When the player want to control the objects.
    /// </summary>
    public void PunRequestOwnership()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (photonView == null)
            {
                photonView = GetComponent<PhotonView>();
            }
            // Call the RPC method to transfer ownership
            photonView.RPC("RPC_RequestOwnership", RpcTarget.All, PhotonNetwork.LocalPlayer);
        }
        else
        {
            Debug.LogWarning("Not connected to Photon Network!");
        }
    }

    /// <summary>
    /// Photon callback to transfer the ownership. Sent to all players
    /// Called by <see cref="PunRequestOwnership"/>
    /// </summary>
    /// <param name="owner">Player requesting the ownership</param>
    [PunRPC]
    public void RPC_RequestOwnership(Photon.Realtime.Player owner)
    {

        // redefine the photonView
        if (loadedMoleculesPhotonView != null)
        {
            // If this client is the owner, request ownership
            if (owner == PhotonNetwork.LocalPlayer)
            {
                Debug.Log("Requesting Ownership");

                loadedMoleculesPhotonView.TransferOwnership(owner);
                // change the UI buttons to identify the change
                foreach (GameObject go in UIRequestOwnership) { go.SetActive(false); }
                foreach (GameObject go in UIOwnershipGranted) { go.SetActive(true); }
                // enable the controller interactions
                interactionToggleManager.ToggleInteraction(true);

                // transfer the ownership for all molecules loaded
                foreach (PhotonView pv in molViewPairs.Values)
                {
                    if (pv != null && !pv.IsMine)
                    {
                        pv.TransferOwnership(owner);
                    }
                }
            }
            // else modify UI to allow the other users to request ownership in return
            else
            {
                Debug.Log("Other user requesting Ownership");

                // change the UI buttons
                foreach (GameObject go in UIRequestOwnership) { go.SetActive(true); }
                foreach (GameObject go in UIOwnershipGranted) { go.SetActive(false); }
                // disable interactions
                interactionToggleManager.ToggleInteraction(false);
            }
        }
    }

    /// <summary>
    /// Send UnityMol API commands to the network
    /// </summary>
    /// <param name="command">UnityMol command</param>
    [PunRPC]
    public void RPC_RunCommand(string command)
    {
        Debug.Log(IsFollowingPunCommands + " Receiving command: " + command);
        if (IsFollowingPunCommands)
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
            {
                InterfaceAPIPython.ExecuteCommand(command);
            } else {
                APIPython.ExecuteCommand(command);
            }
        }

    }

    /// <summary>
    /// Add to the dictionary of (UnityMolStructure name, viewid) a new element
    /// Sync across all players
    /// </summary>
    /// <param name="structName"></param>
    /// <param name="viewID"></param>
    [PunRPC]
    public void RPC_AddPhotonView(string structName, int viewID)
    {
        // the non-control users receive this method, saving the name/viewID pair
        molViewIDPairs.Add(structName, viewID);
        Debug.Log("Added molViewIDPair = ("+structName+","+viewID+")");
        viewIDReady = true;
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Callback when a new python command is received
    /// </summary>
    /// <param name="index"></param>
    private void OnPythonCommands_Changed(int index)
    {
        if (!loadedMoleculesPhotonView.GetComponent<PhotonView>().IsMine) {
            return;
        }

        if (UnityMolMain.pythonCommands.Count > 0)
        {
            string lastCommand = UnityMolMain.pythonCommands[UnityMolMain.pythonCommands.Count - 1];

            // Ignore connectMultiplayer() & disconnectMultiplayer() python commands for obvious reasons
            if (lastCommand.Contains("disconnectMultiplayer()") || lastCommand.Contains("connectMultiplayer")) {
                return;
            }


            Debug.Log($"List changed at index {index}. Last item: {lastCommand}");
            // send the command through a PunRPC
            if (IsFollowingPunCommands)
            {
                photonView.RPC("RPC_RunCommand", RpcTarget.OthersBuffered, lastCommand);
            }
        }
        else
        {
            Debug.Log($"List is empty after change at index {index}");
        }
    }

    /// <summary>
    /// When a new molecule is loaded in the scene by the owner
    /// Need to store & share the viewID associated to it for all players
    /// </summary>
    private void OnNewMolecule()
    {
        // Retrieve the structure.
        // Structure has been added by the python command sent
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        UnityMolStructure s = sm.loadedStructures[^1];
        Debug.Log("Print the name: " + s.name);

        // For non-owners, wait for the owner to send the viewID and exit
        if (!loadedMoleculesPhotonView.GetComponent<PhotonView>().IsMine)
        {
            Debug.Log("On non-control user, running coroutine");
            StartCoroutine(waitForViewID());
            return;
        }

        // Owner POV only:

        // Find the GO associated to the structure
        GameObject go = sm.GetStructureGameObject(s.name); // identified the gameobject
        // add the photon view component
        PhotonView view = go.AddComponent<PhotonView>();
        // define a new viewid
        PhotonNetwork.AllocateViewID(view);
        // successfully added a viewid, add the rest of the components
        addPhotonComponents(go, s.name, view.ViewID);
        // send the viewid to the other users
        photonView.RPC("RPC_AddPhotonView", RpcTarget.OthersBuffered, s.name, view.ViewID);

    }

    /// <summary>
    /// Add other Photon components to a new molecule structure.
    /// </summary>
    /// <param name="go">GO of the new molecule</param>
    /// <param name="molName">name of the molecule</param>
    /// <param name="viewID">viewID of the PhotonView component</param>
    private void addPhotonComponents(GameObject go, string molName, int viewID)
    {
        // In theory, should always be true;
        if (!go.TryGetComponent(out PhotonView view))
        {
            view = go.AddComponent<PhotonView>();
            // assign the viewID given by owner
            view.ViewID = viewID;
        }

        view.OwnershipTransfer = OwnershipOption.Takeover;
        // add PhotonTransformView component
        PhotonTransformView ptv = go.AddComponent<PhotonTransformView>();
        ptv.m_SynchronizePosition = true;
        ptv.m_SynchronizeRotation = true;
        // update observables
        view.FindObservables(true);
        // store the new molecule with their respective PhotonView ID
        molViewPairs.Add(molName, view);
    }

    /// <summary>
    /// Coroutine for the non owners player to wait to receive a viewID.
    /// </summary>
    /// <returns></returns>
    private IEnumerator waitForViewID()
    {
        // Wait until the condition returns true
        UnityMolStructureManager sm = UnityMolMain.getStructureManager();
        string currentStructureName = sm.GetCurrentStructure().name;
        while (!viewIDReady)
        {
            Debug.Log("Waiting for viewID, current name =" + currentStructureName);
            yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds before checking again
        }
        // Once received in the dict, add the components
        GameObject go = sm.GetStructureGameObject(currentStructureName); // identified the gameobject
        int viewID = molViewIDPairs[currentStructureName];
        addPhotonComponents(go, currentStructureName, viewID);
        viewIDReady = false; // reset flag
    }

    #endregion

}

}

/// <summary>
/// Instantiate the player prefab Avatar
/// </summary>
public class CustomPrefabPool : IPunPrefabPool
{
    private readonly Dictionary<string, GameObject> prefabDictionary;
    public CustomPrefabPool(Dictionary<string, GameObject> prefabs)
    {
        prefabDictionary = prefabs;
    }
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        if (!prefabDictionary.TryGetValue(prefabId, out GameObject prefab))
        {
            Debug.LogError($"Prefab with ID {prefabId} not found in dictionary!");
            return null;
        }
        // Instantiate and disable the object
        GameObject instance = Object.Instantiate(prefab, position, rotation);
        instance.SetActive(false); // Ensure the object is inactive
        return instance;
    }

    public void Destroy(GameObject gameObject)
    {
        Object.Destroy(gameObject);
    }


}
