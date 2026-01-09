using UnityEngine;
using UnityEngine.EventSystems;

/**
 * Author: Mariano Spivak
 **/

/// <summary>
/// Control the camera through keyboard and mouse
/// <remarks>Used in Multiplayer mode for a non-VR user.</remarks>
/// </summary>
public class CameraController : MonoBehaviour
{
    /// <summary>
    /// Speed when moving the camera
    /// </summary>
    public float MoveSpeed = 2f;
    /// <summary>
    /// Speed when rotating the camera
    /// </summary>
    public float LookSpeed = 1f;

    private float rotationX;

    /// <summary>
    /// Minimum boundary of the room
    /// </summary>
    public Vector3 minBounds = new(-5f, 0f, -5f);

    /// <summary>
    /// Maximum boundary of the room
    /// </summary>
    public Vector3 maxBounds = new(5f, 3f, 5f);

    private void Update()
    {
        // Prevent movement if interacting with UI
        if (isInteractingWithUI()) {
            return;
        }

        // Keyboard movement (WASD + Q/E for vertical)
        float moveX = Input.GetAxis("Horizontal") + Input.GetAxis("Horizontal1"); // A, D / Left, Right
        float moveZ = Input.GetAxis("Vertical") + Input.GetAxis("Vertical1");   // W, S / Forward, Backward
        float moveY = 0f;

        if (Input.GetKey(KeyCode.Q) || Input.GetButton("Joystick5")) {
            moveY = -1f; // Move down
        }

        if (Input.GetKey(KeyCode.E) || Input.GetButton("Joystick6")) {
            moveY = 1f;  // Move up
        }

        Vector3 move = transform.right * moveX + transform.up * moveY + transform.forward * moveZ;
        Vector3 newPosition = transform.position + move * MoveSpeed * Time.deltaTime;

        // Clamp the new position to the defined boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minBounds.y, maxBounds.y);
        newPosition.z = Mathf.Clamp(newPosition.z, minBounds.z, maxBounds.z);

        transform.position = newPosition;

        // Mouse Look (hold right mouse button)
        if (Input.GetMouseButton(1) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            float mouseX = Input.GetAxis("Mouse X") * LookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * LookSpeed;

            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            transform.localRotation = Quaternion.Euler(rotationX, transform.localRotation.eulerAngles.y + mouseX, 0f);
        }

        // using right joystick pad to look around
        if (Mathf.Abs(Input.GetAxis("HTC_VIU_UnityAxis3")) > 0.1f || Mathf.Abs(Input.GetAxis("HTC_VIU_UnityAxis4")) > 0.1f)
        {
            float mouseX = Input.GetAxis("HTC_VIU_UnityAxis3") * LookSpeed;
            float mouseY = Input.GetAxis("HTC_VIU_UnityAxis4") * -LookSpeed;

            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            transform.localRotation = Quaternion.Euler(rotationX, transform.localRotation.eulerAngles.y + mouseX, 0f);
        }
    }

    private static bool isInteractingWithUI()
    {
        // Check if mouse is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
            return true;
        }

        // Check if a UI element (like an InputField) is selected
        return EventSystem.current.currentSelectedGameObject != null;
    }
}
