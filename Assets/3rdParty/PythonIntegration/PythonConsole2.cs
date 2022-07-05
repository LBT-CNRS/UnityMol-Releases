using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Scripting.Hosting;
using UnityEngine.UI;
using System;
using System.Text;
using System.Reflection;
using TMPro;


using UMol.API;

/// From https://github.com/AlexLemminG/PythonToUnity_Integration

public class PythonConsole2 : MonoBehaviour {
    public Color defaultColor = Color.white;
    public bool visibleByDefault = true;

    public int lineLimit = 500;

    public TMP_InputField input;
    public RectTransform textsRoot;
    public TMP_Text textTemplate;
    public ScrollRect scroll;
    public RectTransform panel;

    const float appearSpeed = 6f;

    private RectTransform lastTextTransform;


    List<string> m_previousCommands = new List<string>();
    int m_previousCommandSelected;
    ScriptScope m_scope;
    bool m_visible = true;
    string m_prevFrameInputText = "";
    bool m_commandExecutionInProgress;
    string m_log;
    bool m_suspendNextMessage;
    bool m_listeningToDevelopmentConsole = true;
    Coroutine m_toggleVisibilityCoroutine;
    LayoutElement layoutE;
    Button showConsoleButton;
    Button hideConsoleButton;

    public void Select(UnityEngine.Object o) {
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = o;
#else
        m_scope.SetVariable ("selection", o);
#endif
    }

    public void Clear() {
        var textTemplateTransform = textTemplate.transform;
        for (int i = textsRoot.childCount - 1; i >= 0; i--) {
            var textTransform = textsRoot.GetChild(i);
            if (textTransform != textTemplateTransform) {
                Destroy(textTransform.gameObject);
            }
        }
        lastTextTransform = null;
        m_suspendNextMessage = true;
    }


    public void ShowLog() {
        m_listeningToDevelopmentConsole = true;
        Application.logMessageReceived -= PrintLogMessageToConsole;
        Application.logMessageReceived += PrintLogMessageToConsole;
    }

    public void HideLog() {
        m_listeningToDevelopmentConsole = false;
        Application.logMessageReceived -= PrintLogMessageToConsole;
    }

    //used to write by python (definition is important)
    public void write(string s) {
        if (string.IsNullOrEmpty (s) || s == "\n")
            return;
        var message = (string.IsNullOrEmpty(m_log) ? "" : "\n") + "<i>" + ">>>" + s + "</i> ";
        m_log += message;
    }


    bool ShouldToggleConsole() {
        return Input.GetButtonDown ("Console");
    }

    // void Awake() {
    //     inputf.lineType = InputField.LineType.MultiLineNewline;
    // }

    // void OnDisable() {
    //     inputf.onValidateInput -= CheckForEnter;
    //     // inputf.OnSubmit.RemoveListener (value => ExecuteCommand(inputf.text));
    //     HideLog ();
    // }

    void OnDisable() {
        // input.onSubmit.RemoveListener (CheckForEnter);
        input.onSubmit.RemoveListener (ExecuteCommand);
        HideLog ();
    }


    void Start() {
#if !DISABLE_CONSOLE
        try {
            showConsoleButton = transform.Find("Canvas/ShowUI").gameObject.GetComponent<Button>();
            hideConsoleButton = transform.Find("Canvas/CloseUI").gameObject.GetComponent<Button>();
        }
        catch {}

        SetVisible (visibleByDefault, true);
        layoutE = input.GetComponent<LayoutElement> ();

        restoreUserPrefsCommands();

#else
        gameObject.SetActive (false);
#endif

    }

    void restoreUserPrefsCommands(){
        int Ncomm = PlayerPrefs.GetInt("NRestoreCommands", 0);
        List<string> prevComm = new List<string>(Ncomm);
        for(int i = Ncomm-1; i == 0; i--){
            string c = PlayerPrefs.GetString("lastcommand"+i, "");
            if(!string.IsNullOrEmpty(c)){
                m_previousCommands.Add(c);
            }
        }
    }

    void OnEnable() {
#if !DISABLE_CONSOLE
        // input.onSubmit.AddListener (CheckForEnter);
        input.onSubmit.AddListener (ExecuteCommand);

        // inputf.onValidateInput += CheckForEnter;
        // inputf.OnSubmit.AddListener(delegate {ExecuteCommand(inputf.text);});


        if (m_scope == null) {
            RecreateScope ();
        }
        if (m_listeningToDevelopmentConsole)
            ShowLog ();
#endif
    }

    void RecreateScope() {
        m_scope = PythonUtils.GetEngine ().CreateScope ();
        m_scope.SetVariable ("console", this);

        var fullScript = PythonUtils.defaultPythonConsoleHeader + getAPIPythonMethods() + GlobalAssemblyImport ();
        PythonUtils.GetEngine ().Execute (fullScript, m_scope);
    }

    string getAPIPythonMethods() {
        StringBuilder res = new StringBuilder();
        MethodInfo[] methodInfos = typeof(APIPython).GetMethods(BindingFlags.Public | BindingFlags.Static);
        foreach (MethodInfo m in methodInfos) {
            res.Append(m.Name);
            res.Append(" = APIPython.");
            res.Append(m.Name);
            res.Append("\n");
        }
        return res.ToString();
    }

    static string GlobalAssemblyImport() {
        var import = new StringBuilder();
        import.Append ("\nimport ");
        bool importedOne = false;
        var globalTypes = Assembly.GetAssembly (typeof(PythonConsole2)).GetTypes ();
        foreach (var type in globalTypes) {
            if (type.IsPublic && type.Namespace == null) {
                if (importedOne) {
                    import.Append (',');
                } else {
                    importedOne = true;
                }
                string name = type.Name;
                if (name.Contains("`")) {
                    name = name.Split(new [] { '`'}, System.StringSplitOptions.RemoveEmptyEntries)[0];
                }
                import.Append (name);
            }
        }
        return import.ToString ();
    }

    void UpdateSubmitButtonReaction ()
    {
        TMP_InputField.LineType preferedLineType;
        if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
            preferedLineType = TMP_InputField.LineType.MultiLineNewline;
        }
        else {
            preferedLineType = TMP_InputField.LineType.MultiLineSubmit;
        }
        //setting lineType every frame makes garbage
        if (input.lineType != preferedLineType) {
            input.lineType = preferedLineType;
        }
    }

    void UpdateSelection ()
    {
#if UNITY_EDITOR
        if (Application.isEditor) {
            m_scope.SetVariable ("selection", UnityEditor.Selection.activeObject);
        }
#endif
    }

    void Update() {
#if !DISABLE_CONSOLE
        UpdateSelection ();

        if (ShouldToggleConsole()) {
            input.text = m_prevFrameInputText;
            ToggleVisibility ();
        }

        if (!input.isFocused)
            return;

        HandleSelectPreviousCommand ();

        UpdateSubmitButtonReaction ();

        layoutE.preferredHeight = input.textComponent.preferredHeight + 8;

        m_prevFrameInputText = input.text;
#endif
    }

    public void ToggleVisibility (bool immediately = false) {
        SetVisible (!m_visible, immediately);
    }

    void SetVisible(bool value, bool immediately) {
        m_visible = value;
        if (m_toggleVisibilityCoroutine != null) {
            StopCoroutine (m_toggleVisibilityCoroutine);
        }
        m_toggleVisibilityCoroutine = StartCoroutine (ToggleVisibilityCoroutine(m_visible, immediately));
    }

    IEnumerator ToggleVisibilityCoroutine(bool makeVisible, bool immediately) {

        if (makeVisible) {
            if (showConsoleButton != null) {
                showConsoleButton.gameObject.SetActive(false);
                hideConsoleButton.gameObject.SetActive(true);
            }
        }
        else {
            if (showConsoleButton != null) {
                showConsoleButton.gameObject.SetActive(true);
                hideConsoleButton.gameObject.SetActive(false);
            }
            input.interactable = false;
        }
        float t = immediately ? 1f : 0f;
        while (t <= 1f) {
            t += Time.unscaledDeltaTime * appearSpeed;
            panel.GetComponent<Animator>().SetFloat("Appeared", makeVisible ? t : 1f - t);
            if (t <= 1f) {
                yield return null;
            }
        }
        if (!makeVisible) {
            panel.gameObject.SetActive(false);
        } else {
            panel.gameObject.SetActive(true);

            input.interactable = true;
            input.ActivateInputField ();
        }

        yield break;
    }
    void HandleSelectPreviousCommand() {
        if (m_previousCommands.Count == 0 || m_previousCommandSelected == -1 && input.textComponent.textInfo.lineCount > 1)
            return;
        bool commandSet = false;
        if (Input.GetKeyDown (KeyCode.UpArrow)) {
            m_previousCommandSelected++;
            commandSet = true;
        }
        if (Input.GetKeyDown (KeyCode.DownArrow)) {
            m_previousCommandSelected--;
            commandSet = true;
        }
        if (commandSet) {
            bool erase = m_previousCommandSelected < 0;
            m_previousCommandSelected = Mathf.Clamp(m_previousCommandSelected, 0, m_previousCommands.Count - 1);
            var previousCommand = m_previousCommands [m_previousCommandSelected];
            if (erase)
                m_previousCommandSelected = -1;

            input.text = erase ? "" : previousCommand;
            input.textComponent.ForceMeshUpdate ();
            input.caretPosition = input.text.Length;
        }
    }

    public void ExecuteCommand (string command) {
        if (input.wasCanceled) {
            input.ActivateInputField ();
            return;
        }
        input.text = "";
        input.ActivateInputField ();
        if (!string.IsNullOrEmpty(command.Trim())) {
            int ncommands = m_previousCommands.Count;
            if (ncommands == 0 || (m_previousCommands[0] != command)) {
                m_previousCommands.Insert (0, command);
            }
        }
        m_previousCommandSelected = -1;

        m_commandExecutionInProgress = true;
        bool exception = false;
        try {
            PythonUtils.GetEngine ().Execute (command, m_scope);
        } catch (Exception e) {
            exception = true;

#if UNITY_EDITOR
            Debug.LogError(e);
#endif
            string message = e.Message.Split(new [] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)[0];
            write (message);
            throw new System.Exception(message);
        }

        m_commandExecutionInProgress = false;

        var commandLog = "<b>" + (exception ? "<color=#d22>" : "") + command + (exception ? "</color>" : "") + "</b>";
        if (string.IsNullOrEmpty(m_log)) {
            m_log = commandLog;
        } else {
            m_log =  commandLog + "\n" + m_log;
        }

        FlushLog ();
        scroll.verticalNormalizedPosition = 0f;
    }


    void FlushLog() {
        if (!m_suspendNextMessage) {
            var text = Instantiate(textTemplate, textsRoot);
            text.text = m_log + " ";
            Vector2 pos;
            if (lastTextTransform != null) {
                pos = lastTextTransform.anchoredPosition - Vector2.up * text.preferredHeight;
            } else {
                pos = textTemplate.rectTransform.anchoredPosition - Vector2.up * text.preferredHeight;
            }
            lastTextTransform = text.rectTransform;
            lastTextTransform.anchoredPosition = pos;
            var rootSize = textsRoot.sizeDelta;
            rootSize.y = pos.y - 6;
            textsRoot.sizeDelta = rootSize;

            //Remove some lines if too much message
            if (textsRoot.childCount >= lineLimit) {
                int nDelete = textsRoot.childCount - lineLimit;
                for (int i = 1 ; i <= nDelete; i++) {
                    var textTransform = textsRoot.GetChild(i);
                    Destroy(textTransform.gameObject);
                }
            }

        }
        m_suspendNextMessage = false;
        m_log = "";

        UpdateScrollPositionAfterMove ();
    }

    void UpdateScrollPositionAfterMove() {
        var viewportHeight = scroll.GetComponent<RectTransform> ().sizeDelta.y;
        var oldHeight = scroll.content.sizeDelta.y - viewportHeight;
        float pos = scroll.verticalNormalizedPosition;

        Canvas.ForceUpdateCanvases ();

        var newHeight = scroll.content.sizeDelta.y - viewportHeight;
        if (pos * oldHeight < 20f)
            scroll.verticalNormalizedPosition = 0f;
        else
            scroll.verticalNormalizedPosition = (pos * oldHeight + (newHeight - oldHeight)) / newHeight;
    }

    void PrintLogMessageToConsole (string condition, string stackTrace, LogType type) {
        Color color = defaultColor;
        bool printStackTrace = false;
        switch (type) {
        case LogType.Assert:
        case LogType.Error:
        case LogType.Exception:
            color = Color.red;
            printStackTrace = true;
            if (!string.IsNullOrEmpty(stackTrace)) {
                stackTrace = stackTrace.Split(new [] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)[0];
            }
            break;
        case LogType.Warning:
            color = Color.yellow;
            break;
        }
        var colorHex = "#" + ColorUtility.ToHtmlStringRGBA(color);
        var message = "[" + type + "] " + condition + (printStackTrace ? "\n" + stackTrace : "");
        message = "<color=" + colorHex + ">" + message + "</color>";
        if (string.IsNullOrEmpty(m_log)) {
            m_log = message + " ";
        } else {
            m_log += "\n" + message + " ";
        }

        if (!m_commandExecutionInProgress) {
            FlushLog ();
        }
    }

    private char CheckForEnter(string text, int charIndex, char addedChar)
    {
        if (addedChar == '\n' )
        {
            // onSubmit.Invoke(text);
            ExecuteCommand(text);
            return '\0';
        }
        else
            return addedChar;
    }


}