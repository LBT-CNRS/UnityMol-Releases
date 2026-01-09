using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;

using Photon.Pun;
using Photon.Voice.PUN;
using UMol;

/**
 * Authors: Mariano Spivak, Rajkumar Darbar
 **/

/// <summary>
/// Class managing a player.
/// Attached to the player prefab
/// </summary>
public class NetworkPlayer : MonoBehaviour
{

    /// <summary>
    /// Link to the photon view
    /// </summary>
    private PhotonView photonView;

    /// <summary>
    /// Link to the PhotonVoice view
    /// </summary>
    private PhotonVoiceView photonVoiceView;

    /// <summary>
    /// Head avatar GameObject of the player
    /// Will hold the Tracked Pose Driver and the Photon Transform view component
    /// </summary>
    [SerializeField] private GameObject playerAvatar;

    /// <summary>
    /// GameObject of the head model of a player
    /// </summary>
    [SerializeField]
    private GameObject headAvatarModel;

    /// <summary>
    /// GameObject of the left controller
    /// </summary>
    [SerializeField] private GameObject leftHand;

    /// <summary>
    /// GameObject of the right controller
    /// </summary>
    [SerializeField] private GameObject rightHand;

    /// <summary>
    /// GameObject of the left controller model of a player
    /// </summary>
    [SerializeField]
    private GameObject leftHandModel;

    /// <summary>
    /// GameObject of the right controller model of a player
    /// </summary>
    [SerializeField]
    private GameObject rightHandModel;

    /// <summary>
    /// Mesh of the name of the player
    /// </summary>
    [SerializeField]
    private TextMesh nameMesh;

    /// <summary>
    /// Sprite to activate if a player is recording
    /// </summary>
    [SerializeField]
    private GameObject recorderSprite;

    /// <summary>
    /// Sprite to activate if a player is speaking
    /// </summary>
    [SerializeField]
    private GameObject speakerSprite;

    /// <summary>
    /// List of Tracking components for the controllers
    /// </summary>
    [SerializeField]
    private List<TrackedPoseDriver> trackedPoseDriversHands = new();

    /// <summary>
    /// Name of the player
    /// </summary>
    private string playerName;

    /// <summary>
    /// Camera object which will be controller by a non-VR player to move.
    /// </summary>
    private Camera cameraParent;

    /// <summary>
    /// If the player is local and in non-VR mode
    /// </summary>
    private bool isMineAndInDesktop;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        photonVoiceView = GetComponentInParent<PhotonVoiceView>();
        cameraParent = Camera.main;

    }

    /// <summary>
    /// Hide models for local player
    /// </summary>
    public void HideLocalModels() {
        headAvatarModel.SetActive(false);
        leftHandModel.SetActive(false);
        rightHandModel.SetActive(false);
    }


    /// <summary>
    /// Activate the tracking objects for the local player
    /// </summary>
    public void ActivateTrackedPoseDriver()
    {

        //Always activate the tracking of the head avatar
        playerAvatar.GetComponent<TrackedPoseDriver>().enabled = true;

        // In desktop mode, link the control of the camera to the player head avatar
        // and deactivate controllers
        if(!UnityMolMain.inVR())
        {
            isMineAndInDesktop = true;
            cameraParent.GetComponent<CameraController>().enabled = true;
            leftHand.SetActive(false);
            rightHand.SetActive(false);

            //Put the head avatar above the ground
            Vector3 vector3 = cameraParent.transform.position;
            vector3.y = 1;
            cameraParent.transform.position = vector3;
            return;
        }

        // In VR mode, activate the tracking of the controllers
        foreach (TrackedPoseDriver poseDriverHand in trackedPoseDriversHands)
        {
            poseDriverHand.enabled = true;
        }
    }

    /// <summary>
    /// Called for every new instance of the class (local & distant players)
    /// </summary>
    private void OnEnable()
    {
        if (nameMesh != null && photonView.Owner != null) {
            playerName = photonView.Owner.NickName;
            nameMesh.text = playerName;

            //Retrieve VR mode of this player
            if (photonView.Owner.CustomProperties.ContainsKey("inVR")) {
                bool inVR = (bool)photonView.Owner.CustomProperties["inVR"];
                // Don't activate controllers for non-VR players
                if (!inVR) {
                    leftHand.SetActive(false);
                    rightHand.SetActive(false);
                }
            }
        }

    }

    private void Update()
    {
        recorderSprite.SetActive(photonVoiceView.IsRecording);
        speakerSprite.SetActive(photonVoiceView.IsSpeaking);

        // In non VR mode, link the control of the camera to the player head avatar
        if (isMineAndInDesktop)
        {
            // if in desktop or stereo display
            playerAvatar.transform.position = cameraParent.transform.position;
            playerAvatar.transform.rotation = cameraParent.transform.rotation;
        }

    }

}
