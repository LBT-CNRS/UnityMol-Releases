using UnityEngine;
using UnityEngine.UI;

/**
 * Author: Mariano Spivak
 **/

namespace UMol {

/// <summary>
/// Component of Room name InputField.
/// Save the name in the Player Preferences for future sessions.
/// </summary>
[RequireComponent(typeof(InputField))]
public class RoomNameInputField : MonoBehaviour
{
    #region Private Constants
    // Store the PlayerPref Key to avoid typos
    public const string RoomNamePrefKey = "RoomName";
    #endregion

    #region MonoBehaviour CallBacks

    void Start()
    {
        InputField inputField = GetComponent<InputField>();
        if (inputField != null)
        {
            if (PlayerPrefs.HasKey(RoomNamePrefKey)) {
                inputField.text = PlayerPrefs.GetString(RoomNamePrefKey);;
            }
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the name of the room, and save it in the PlayerPrefs for future sessions.
    /// </summary>
    /// <param name="value">The name of the Player</param>
    public void SetRoomName(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            PlayerPrefs.SetString(RoomNamePrefKey,value);
        }
    }
    #endregion
}

}
