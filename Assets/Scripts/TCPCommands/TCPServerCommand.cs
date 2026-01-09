/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2022
        Hubert Santuz, 2022-2026
        Marc Baaden, 2010-2026
        unitymol@gmail.com
        https://unity.mol3d.tech/

        This file is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications based on the Unity3D game engine.
        More details about UnityMol are provided at the following URL: https://unity.mol3d.tech/

        This program is free software: you can redistribute it and/or modify
        it under the terms of the GNU General Public License as published by
        the Free Software Foundation, either version 3 of the License, or
        (at your option) any later version.

        This program is distributed in the hope that it will be useful,
        but WITHOUT ANY WARRANTY; without even the implied warranty of
        MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        GNU General Public License for more details.

        You should have received a copy of the GNU General Public License
        along with this program. If not, see <https://www.gnu.org/licenses/>.

        To help us with UnityMol development, we ask that you cite
        the research papers listed at https://unity.mol3d.tech/cite-us/.
    ================================================================================
*/
using System;
using System.Diagnostics;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

using UnityEngine;
using System.Collections;

namespace UMol {


public class NetMqCommandManager {
    private readonly Thread _listenerWorker;

    public bool _listenerCancelled;

    public delegate string MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly Stopwatch _contactWatch;

    private const long ContactThreshold = 1000;

    public bool Connected;

    public bool waitingForCommandRes = false;


    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var server = new ResponseSocket())
        {
            server.Bind("tcp://*:5555");

            while (!_listenerCancelled)
            {
                Connected = _contactWatch.ElapsedMilliseconds < ContactThreshold;
                string message;
                if (!server.TryReceiveFrameString(out message)) continue;
                _contactWatch.Restart();
                waitingForCommandRes = true;
                var response = _messageDelegate(message);
                server.SendFrame(response);
            }
        }
        NetMQConfig.Cleanup();
    }

    public NetMqCommandManager(MessageDelegate messageDelegate)
    {
        _messageDelegate = messageDelegate;
        _contactWatch = new Stopwatch();
        _contactWatch.Start();
        _listenerWorker = new Thread(ListenerWork);
    }

    public void Start()
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
    }

    public void Stop()
    {
        NetMQConfig.Cleanup();
        _listenerCancelled = true;
        _listenerWorker.Abort();
    }
}

public class TCPServerCommand : MonoBehaviour {
    private NetMqCommandManager _commandManager;
    private string commandReceived = "";
    private string commandResult = "";
    private bool wasRunningBackground = false;
    private System.Text.StringBuilder capturedLogs = new System.Text.StringBuilder();
    /// Wait X seconds before restoring the run in background state if no command is called
    public float timeoutRunInBG = 5.0f;
    Coroutine restoreRunInBG;

    void Start() {
        wasRunningBackground = Application.runInBackground;
        _commandManager = new NetMqCommandManager(HandleMessage);
        _commandManager.Start();
    }

    void HandleUnityLog(string logString, string stackTrace, LogType type) {
        capturedLogs.AppendLine($"[{type}] {logString}");
        if (type == LogType.Exception || type == LogType.Error)
            capturedLogs.AppendLine(stackTrace);
    }

    void Update() {
        if (_commandManager.waitingForCommandRes) {
            if (restoreRunInBG != null)
                StopCoroutine(restoreRunInBG);
            Application.runInBackground = true;

            bool success = false;
            object res = null;

            // Clean the buffer before each command
            capturedLogs.Clear();

            try {
                Application.logMessageReceived += HandleUnityLog;
                res = UMol.API.APIPython.ExecuteCommandWithFeedback(commandReceived, ref success);
                Application.logMessageReceived -= HandleUnityLog;
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("Exception while executing command: " + e.Message);
            }

            if (!success) {
                UnityEngine.Debug.LogError("The command did not execute successfully");
            }

            string resultOnly = commandResToString(res);
            string logs = capturedLogs.ToString();
            // Create the CommandResponse object
            CommandResponse response = new CommandResponse
            {
                success = success,  // Update this based on actual success
                result = resultOnly, // Command result from execution
                stdout = logs // Replace this with actual stdout if available
            };

            // Return the response as a JSON string
            commandResult = JsonUtility.ToJson(response);

            _commandManager.waitingForCommandRes = false;
            restoreRunInBG = StartCoroutine(waitAndSetRunInBG());
        }
    }

    private string commandResToString(object res) {
        string comRes;
        if (res != null && res.GetType() == typeof(System.String)) {
            comRes = res as string;
        }
        else if (res != null && res.GetType() == typeof(System.Boolean)) {
            comRes = UMol.API.APIPython.cBoolToPy((bool)res);
        }
        else if (res != null && res.GetType() == typeof(UnityMolStructure) ) {
            comRes = (res as UnityMolStructure).name;
        }
        else if (res != null && res.GetType() == typeof(UnityMolSelection) ) {
            comRes = (res as UnityMolSelection).name;
        }
        else {
            comRes = " ";
        }
        return comRes;
    }

    private string HandleMessage(string message)
    {
        //Warning : This is not running on the main thread !
        UnityEngine.Debug.Log("Received command '" + message + "'");
        commandReceived = message;

        // Wait for the command to execute and return something
        while (_commandManager.waitingForCommandRes && !_commandManager._listenerCancelled) {
        }

        // Return the response as a JSON string
        return commandResult;
    }

    // Public method to add messages to captured logs from external routines
    public void AddLogMessage(string message)
    {
        if (_commandManager.waitingForCommandRes) {
            capturedLogs.AppendLine(message.Trim());
        }
    }

    private void OnDestroy()
    {
        _commandManager._listenerCancelled = true;
        _commandManager.Stop();
        Application.runInBackground = wasRunningBackground;
    }
    IEnumerator waitAndSetRunInBG(){
        yield return new WaitForSeconds(timeoutRunInBG);
        Application.runInBackground = wasRunningBackground;
    }

    // CommandResponse class
    [System.Serializable]
    public class CommandResponse
    {
        public bool success;
        public string result;
        public string stdout;
    }

}
}
