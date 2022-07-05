using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//Adapted from https://gist.github.com/mminer/975374

//TODO: Pool objects

public class PopUpSystem : MonoBehaviour {

    public GameObject notificationButtonPrefab;

    public Transform notificationPanelTransform;

    [Tooltip("Adjust this value to delay the fade animation")]
    public float timeBeforeAnimationStarts = 3.0f;

    public Queue<GameObject> notificationQueue = new Queue<GameObject>();
    private GameObject currentGo;

    public Color errorColor = Color.red;
    public Color warningColor = Color.yellow;
    public Color logColor = Color.black;

    Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>()
    {
        { LogType.Assert, Color.white },
        { LogType.Error, Color.red },
        { LogType.Exception, Color.red },
        { LogType.Log, Color.white },
        { LogType.Warning, Color.yellow },
    };

    struct Log
    {
        public string message;
        public string stackTrace;
        public LogType type;
    }
    List<Log> logs = new List<Log>();

    void OnEnable ()
    {
#if !DISABLE_NOTIFICATIONS
        Application.RegisterLogCallback(HandleLog);
        notificationQueue.Clear();
#endif
    }

    void OnDisable ()
    {
        Application.RegisterLogCallback(null);
    }



    /// <summary>
    /// Records a log from the log callback.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="stackTrace">Trace of where the message came from.</param>
    /// <param name="type">Type of message (error, exception, warning, assert).</param>
    void HandleLog (string message, string stackTrace, LogType type)
    {
        logs.Add(new Log() {
            message = message,
            stackTrace = stackTrace,
            type = type,
        });

        if (notificationPanelTransform != null && notificationButtonPrefab != null) {
            GameObject notif = GameObject.Instantiate(notificationButtonPrefab, notificationPanelTransform);
            // notif.transform.SetParent(notificationPanelTransform, false);
            notif.transform.Find("Text").gameObject.GetComponent<Text>().text = message;
            Image logo = notif.transform.Find("Image").gameObject.GetComponent<Image>();
            switch (type) {
            case LogType.Error:
            case LogType.Exception:
                logo.color = errorColor;
                break;
            case LogType.Warning:
                logo.color = warningColor;
                break;
            case LogType.Log:
                logo.color = logColor;
                break;
            default:
                logo.color = Color.black;
                break;
            }
            notificationQueue.Enqueue(notif);
        }

    }


    void Update() {
#if !DISABLE_NOTIFICATIONS
        //Play a delayed animation once the previous gameObject is destroyed
        if (currentGo == null && notificationQueue.Count != 0) {
            currentGo = notificationQueue.Dequeue();
            if (currentGo != null) {
                currentGo.GetComponent<SlideAnimationButton>().delayedAnimation(timeBeforeAnimationStarts);
            }
        }
#endif
    }

    public void ClearAllNotifications() {
        foreach (GameObject n in notificationQueue) {
            n.GetComponent<SlideAnimationButton>().playAnimation();
        }
        notificationQueue.Clear();
    }
}
