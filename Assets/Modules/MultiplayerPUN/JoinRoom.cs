using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;

/**
 * Authors: Mariano Spivak, Rajkumar Darbar
 **/

namespace UMol {

/// <summary>
/// Class to handle connections to the Photon server through Photon Rooms
/// </summary>
public class JoinRoom : MonoBehaviourPunCallbacks
{

    #region Public Fields

    /// <summary>
    /// Name of the Room
    /// </summary>
    public string RoomName;

    [Tooltip("If activated, the RoomName set above will override the one saved in the Player Preferences.")]
    public bool overridePlayerPrefsRoomName;

    /// <summary>
    /// Name of the json file to parse the Photon App ids : PUN & Voice
    /// The location of this file is expected to be in the StreamingAssets folder
    /// </summary>
    public string PhotonIdsfile = "PhotonIds.json";

    #endregion

    #region Private Serializable Fields
    /// <summary>
    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    /// </summary>
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 20;

    /// <summary>
    /// References to UI Buttons "Join a room"
    /// It's a list because in VR, there is 2 menus.
    /// </summary>
    [SerializeField]
    private List<GameObject> UIJoinRoom;

    /// <summary>
    /// References to UI Buttons "Leave a room"
    /// It's a list because in VR, there is 2 menus.
    /// </summary>
    [SerializeField]
    private List<GameObject> UILeaveRoom;

    #endregion

    #region Private Fields
    /// <summary>
    /// Client's version number. Users are separated by version
    /// </summary>
    [SerializeField] private string gameVersion = "0.1";

    /// <summary>
    /// Reference roomVR object
    /// Only activate & set for a non-VR player
    /// </summary>
    [Tooltip("GameObject of the VR room. Only set for a non-VR player")]
    [SerializeField] private GameObject roomVR;

    /// <summary>
    /// Reference of the CameraController component
    /// Used by a non-VR player to move inside the roomVR.
    /// Only activate & set for a non-VR player
    /// </summary>
    [Tooltip("Component to control the main Camera for a non-VR player.")]
    [SerializeField] private CameraController cameraController;

    #endregion

    #region  Public Methods

    /// <summary>
    /// Use to set the Photon App id (both PUN & Voice)
    /// </summary>
    public void Awake() {
        parsePhotonIds();
        if (overridePlayerPrefsRoomName)
        {
            PlayerPrefs.SetString(RoomNameInputField.RoomNamePrefKey, RoomName);
        }
    }


    /// <summary>
    /// Start the connection process for a multiplayer session.
    /// - If already connected, we attempt joining a room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// The App id of the application instance is read from a file named "PhotonIds.json" by default.
    /// </summary>
    public void Connect(string roomName, string playerName)
    {

        PhotonNetwork.GameVersion = gameVersion;
        RoomName = roomName;
        PhotonNetwork.NickName = playerName;


        if (!PhotonNetwork.IsConnected)
        {
            if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime)) {
                Debug.LogError("Photon App ID PUN is not set. Check the " + PhotonIdsfile +
                               " in the StreamingAssets folder.");
                return;
            }
            if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice)) {
                Debug.LogError("Photon App ID Voice is not set. Check the " + PhotonIdsfile +
                               " in the StreamingAssets folder.");
                return;
            }
            PhotonNetwork.ConnectUsingSettings();
            // Callback "OnConnectedToMaster" is then called
        }
        else
        {
            joinOrCreateRoom();
        }
    }

    /// <summary>
    /// Disconnect from a multiplayer session. Leave room & disconnect from the master server.
    /// </summary>
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        PunVoiceClient.Instance.Disconnect();
    }

    /// <summary>
    /// Check if one is connected to a multiplayer session
    /// </summary>
    /// <returns>True if connected. False otherwise</returns>
    public bool IsConnected() {
        return PhotonNetwork.IsConnectedAndReady;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Read the file PhotonsIds.json (or value in <see cref="PhotonIdsfile"/>) in StreamingAssets folder
    /// and populate the App ID PUN & App ID Voice in the PhotonServerSettings asset
    /// </summary>
    private void parsePhotonIds() {
        StreamReader sr;
        string photonIdsfilePath = Path.Combine(Application.streamingAssetsPath , PhotonIdsfile);

        if (Application.platform == RuntimePlatform.Android) {
            StringReaderStream textStream = new(AndroidUtils.GetFileText(photonIdsfilePath));
            sr = new StreamReader(textStream);
        }
        else {
            sr = new StreamReader(photonIdsfilePath);
        }

        using (sr) {
            string jsonString = sr.ReadToEnd();
            IDictionary photonAppIds = (IDictionary)Json.Deserialize(jsonString);
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = (string)photonAppIds["PunID"];
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = (string)photonAppIds["VoiceID"];
        }
    }

    /// <summary>
    /// Connect to a photon Room.
    /// Join an existing room or create a new one.
    /// </summary>
    private void joinOrCreateRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(RoomName, new RoomOptions { MaxPlayers = maxPlayersPerRoom }, TypedLobby.Default);

    }
    #endregion

    #region MonoBehaviourPunCallbacks CallBacks

    /// <summary>
    /// Callback when connected to the Photon Network
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        joinOrCreateRoom();
    }

    /// <summary>
    /// Callback when the player joined the room
    /// </summary>
    public override void OnJoinedRoom()
    {
        //Instantiate the new player
        GetComponent<GameManager>().InstantiatePlayer();

        // If I am the master, aka the first in the room, then trigger the requestOwnership
        if (PhotonNetwork.IsMasterClient) {
            GetComponent<GameManager>().PunRequestOwnership();
        }

        // Toggling UI Buttons
        foreach (GameObject go in UIJoinRoom) { go.SetActive(false); }
        foreach (GameObject go in UILeaveRoom) { go.SetActive(true); }

        // Toggling roomVR and cameraController for Desktop scene
        if (!UnityMolMain.inVR()) {
            roomVR.SetActive(true);
            cameraController.enabled = true;
        }
        Debug.LogFormat("You ({0}) successfully joined room {1} ",PhotonNetwork.LocalPlayer.NickName, PhotonNetwork.CurrentRoom.Name);
    }

    /// <summary>
    /// Called only when a remote Player joined the room
    /// </summary>
    /// <param name="newPlayer">the new player</param>
    public override void OnPlayerEnteredRoom(Player newPlayer) {
        Debug.Log("Remote Player " + newPlayer.NickName + " joined the room");
    }

    /// <summary>
    /// Callback when the player leaved the room
    /// </summary>
    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
        PunVoiceClient.Instance.Disconnect();
        // Toggling UI Buttons
        foreach (GameObject go in UIJoinRoom) { go.SetActive(true); }
        foreach (GameObject go in UILeaveRoom) { go.SetActive(false); }

        // Deactivate roomVR and cameraController in Desktop scene
        if (!UnityMolMain.inVR()) {
            roomVR.SetActive(false);
            cameraController.enabled = false;
        }
        Debug.Log("Left room successfully");
    }

    /// <summary>
    /// Callback when a Player has been disconnected
    /// </summary>
    /// <param name="cause">Reason of the disconnection</param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        //Ignore if triggered by the player
        string reason = cause == DisconnectCause.DisconnectByClientLogic ? "." : " : " + cause + ".";
        Debug.Log("Disconnected from multiplayer server" + reason);
        // Perform any additional cleanup here if needed
    }

    #endregion
}

}
