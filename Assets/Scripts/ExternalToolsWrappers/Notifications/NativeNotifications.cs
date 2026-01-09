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
using System.Runtime.InteropServices;

namespace UMol {

/// <summary>
/// Wrapper to handle the Native Notifications external library
/// Used to pop native OS Notifications through UnityMol.
/// </summary>
public static class NativeNotifications {
    [DllImport("NativeNotifBrowser")]
    private static extern void ShowNotification(string t, string m, int itype);

    [DllImport("NativeNotifBrowser")]
    private static extern bool ShowDualChoice(string t, string m, int itype);

    [DllImport("NativeNotifBrowser")]
    private static extern bool ShowOKCancel(string t, string m, int itype);

    /// <summary>
    /// Pop a native OS notification.
    /// </summary>
    /// <param name="message">the message delivered</param>
    /// <param name="title">Title of the notification (default = UnityMol)</param>
    /// <param name="itype">Type of notification to use. <see cref="notifType"/> </param>
    public static void Notify(string message, string title = "UnityMol", notifType itype = notifType.info){
        ShowNotification(title, message, (int)itype);
    }

    /// <summary>
    /// Pop a native OS notification with "Yes" & "No" button
    /// </summary>
    /// <param name="question">the message delivered</param>
    /// <param name="title">Title of the notification (default = UnityMol)</param>
    /// <param name="itype">Type of notification to use. <see cref="notifType"/> </param>
    /// <returns>True if "Yes" button clicked. False otherwise</returns>
    public static bool AskYesNo(string question, string title = "UnityMol", notifType itype = notifType.question) {
        return ShowDualChoice(title, question, (int)itype);
    }

    /// <summary>
    /// Pop a native OS notification with "OK" & "Cancel" button
    /// </summary>
    /// <param name="question">the message delivered</param>
    /// <param name="title">Title of the notification (default = UnityMol)</param>
    /// <param name="itype">Type of notification to use. <see cref="notifType"/> </param>
    /// <returns></returns>
    public static bool AskContinue(string question, string title = "UnityMol", notifType itype = notifType.question) {
        return ShowOKCancel(title, question, (int)itype);
    }
}

/// <summary>
/// Type of notifications supported: info, warning, error, question
/// </summary>
public enum notifType{
    info = 0,
    warning = 1,
    error = 2,
    question = 3
}
}
