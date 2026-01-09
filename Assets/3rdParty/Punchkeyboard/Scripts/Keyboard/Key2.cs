using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

public class Key2 : MonoBehaviour
{

    public string text;
    public static int NkeyPressed = 0;//Total number of current key pressed
    public AudioSource audios;
    public MainKeyboard mainkb;

    public delegate void OnKeyPressed();
    public delegate void OnKeyReleased();
    public OnKeyPressed keyPressed;
    public OnKeyReleased keyReleased;

    private const float DistanceToBePressed = 0.01f;
    private const float KeyBounceBackMultiplier = 1500f;
    private bool beingPressed = false;

    private Vector3 initialPosition;
    private float currentDistance = 0.0f;
    private Vector3 constrainedPosition;
    private Quaternion constrainedRotation;

    private Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
        keyPressed += buttonPressed;
        // keyReleased += restoreButtonColor;

        initialPosition = transform.localPosition;
        constrainedPosition = transform.localPosition;
        constrainedRotation = transform.localRotation;
    }

    void FixedUpdate()
    {
        ConstrainPosition ();

        Vector3 PositionDelta = initialPosition - transform.localPosition;
        rb.velocity = PositionDelta * KeyBounceBackMultiplier * Time.deltaTime;
    }

    void Update() {
        currentDistance = Vector3.Distance(transform.localPosition, initialPosition);

        if (currentDistance > DistanceToBePressed)
        {
            if (!beingPressed && NkeyPressed < 10) {
                if (keyPressed != null)
                    keyPressed();
                beingPressed = true;
                NkeyPressed++;
            }
        }
        else if (beingPressed) {
            if (keyReleased != null)
                keyReleased();
            NkeyPressed--;
            beingPressed = false;
        }
    }

    void ConstrainPosition()
    {
        constrainedPosition.y = transform.localPosition.y;
        if (transform.localPosition.y > initialPosition.y)
        {
            constrainedPosition.y = initialPosition.y;
        }
        transform.localPosition = constrainedPosition;
        transform.localRotation = constrainedRotation;
    }

    void buttonPressed() {
        AudioSource.PlayClipAtPoint(audios.clip, transform.position, 2.0f);

        //Get closest controller to trigger vibration
        float distL = float.MaxValue;
        float distR = float.MaxValue;
        if (mainkb.controllerL != null)
            distL = Vector3.Distance(transform.position, mainkb.controllerL.transform.position);
        if (mainkb.controllerR != null)
            distR = Vector3.Distance(transform.position, mainkb.controllerR.transform.position);

        if (distL < distR)
            ViveInput.TriggerHapticPulse(HandRole.LeftHand, 500);
        else
            ViveInput.TriggerHapticPulse(HandRole.RightHand, 500);

        if (mainkb != null && mainkb.activeIF != null) {
            if (text == "Back") {
                if (mainkb.activeIF.text.Length > 0)
                    mainkb.activeIF.text = mainkb.activeIF.text.Remove(mainkb.activeIF.text.Length - 1);
            }
            else if (text == "Return") {
                if (mainkb.toTrigger != null)
                    mainkb.toTrigger.onClick.Invoke();
            }
            else
                mainkb.activeIF.text += text;
        }
    }



}