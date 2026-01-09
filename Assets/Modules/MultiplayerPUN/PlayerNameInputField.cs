using UnityEngine;
using UnityEngine.UI;

/**
 * Author: Mariano Spivak
 **/

namespace UMol {

/// <summary>
/// Component of Player name InputField.
/// Let the user input his name, will appear above the player in the game.
/// Save the name in the Player Preferences for future sessions.
/// </summary>
[RequireComponent(typeof(InputField))]
public class PlayerNameInputField : MonoBehaviour
{
    #region Private Constants
    // Store the PlayerPref Key to avoid typos
    private const string playerNamePrefKey = "PlayerName";
    #endregion

    #region MonoBehaviour CallBacks

    private void Start()
    {
        InputField inputField = GetComponent<InputField>();
        if (inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey)) {
                inputField.text = PlayerPrefs.GetString(playerNamePrefKey);;
            }
        }
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
    /// </summary>
    /// <param name="value">The name of the Player</param>
    public void SetPlayerName(string value)
    {
        // #Important
        if (!string.IsNullOrEmpty(value))
        {
            PlayerPrefs.SetString(playerNamePrefKey,value);
        }
    }
    #endregion
}

}
