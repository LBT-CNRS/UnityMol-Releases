using UnityEngine;

/**
 * Author: Mariano Spivak
 **/


namespace UMol {

/// <summary>
/// Class holding references to the controllers of a player
/// Controllers are the controllers in VR and the manipulation manager in non-VR
/// Used during multiplayer mode : if the player is the owner of the molecules,
/// controllers are activated. If not, they are deactivated
/// </summary>
public class InteractionToggleManager : MonoBehaviour
{
    /// <summary>
    /// Left controller for VR
    /// </summary>
    [SerializeField]
    private ControllerGrabAndScale leftControllerGrabAndScale;

    /// <summary>
    /// Right controller for VR
    /// </summary>
    [SerializeField]
    private ControllerGrabAndScale rightControllerGrabAndScale;

    /// <summary>
    /// Manipulation manager for mouse manipulation in non-VR
    /// </summary>
    [SerializeField]
    private ManipulationManager manipulationManager;

    /// <summary>
    /// Activate/Deactivate the controllers.
    /// </summary>
    /// <param name="mode">True to activate. False to deactivate</param>
    public void ToggleInteraction(bool mode) {
        if (leftControllerGrabAndScale != null) {
            leftControllerGrabAndScale.enabled = mode;
        }

        if (rightControllerGrabAndScale != null) {
            rightControllerGrabAndScale.enabled = mode;
        }

        if (manipulationManager != null) {
            manipulationManager.ActivateMouse = mode;
        }
    }

}

}
