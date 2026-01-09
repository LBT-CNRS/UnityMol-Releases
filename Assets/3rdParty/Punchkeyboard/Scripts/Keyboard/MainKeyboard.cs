using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

public class MainKeyboard : MonoBehaviour
{

    public InputField activeIF;
    public Button toTrigger;

    private GameObject sphereKeyL;
    private GameObject sphereKeyR;

    GameObject _cL;
    GameObject _cR;

    //Empirical "good" initial position of the keyboard relative to the camera
    Vector3 relativePos = new Vector3(-0.01821253f, -0.03484856f, 0.6358277f);
    Vector3 relativeQuatEuler = new Vector3(-22.306f, -4.671f, 0.0f);

    public GameObject controllerL {
        get {
            if (_cL == null) {
                _cL = GameObject.Find("LeftHand");
            }
            return _cL;
        }
    }
    public GameObject controllerR {
        get {
            if (_cR == null) {
                _cR = GameObject.Find("RightHand");
            }
            return _cR;
        }
    }

    public void restoreKeyboardPos() {
        Transform savedPar = transform.parent;
        transform.parent = Camera.main.transform;
        transform.localPosition = relativePos;
        transform.localRotation = Quaternion.Euler(relativeQuatEuler);
        transform.parent = savedPar;
    }
    void OnEnable() {
        restoreKeyboardPos();
        activateSphereInteractors();
    }
    void OnDisable() {
        hideSphereInteractors();
    }

    void activateSphereInteractors() {
        if (sphereKeyL == null && controllerL != null) {
            try {
                sphereKeyL = controllerL.transform.Find("SphereKeyboard").gameObject;
            } catch {}
        }

        if (sphereKeyR == null && controllerR != null) {
            try {
                sphereKeyR = controllerR.transform.Find("SphereKeyboard").gameObject;
            } catch {}
        }

        if (sphereKeyL != null)
            sphereKeyL.SetActive(true);
        if (sphereKeyR != null)
            sphereKeyR.SetActive(true);
    }

    void hideSphereInteractors() {
        if (sphereKeyL != null)
            sphereKeyL.SetActive(false);
        if (sphereKeyR != null)
            sphereKeyR.SetActive(false);
    }
}

