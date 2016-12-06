// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissioßns and
// limitations under the License.

// The controller is not available for versions of Unity without the
// // GVR native integration.
#if UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)

using UnityEngine;

using Gvr;

/// @cond
namespace Gvr.Internal {
  /// Internal representation of the controller's current state.
  /// This representation is used by controller providers to represent the controller's state.
  ///
  /// The fields in this class have identical meanings to their correspondents in the GVR C API,
  /// so they are not redundantly documented here.
  class ControllerState {
    internal GvrConnectionState connectionState = GvrConnectionState.Disconnected;
    internal Quaternion orientation = Quaternion.identity;
    internal Vector3 gyro = Vector3.zero;
    internal Vector3 accel = Vector3.zero;
    internal bool isTouching = false;
    internal Vector2 touchPos = Vector2.zero;
    internal bool touchDown = false;
    internal bool touchUp = false;
    internal bool recentering = false;
    internal bool recentered = false;

    internal bool clickButtonState = false;
    internal bool clickButtonDown = false;
    internal bool clickButtonUp = false;

    internal bool appButtonState = false;
    internal bool appButtonDown = false;
    internal bool appButtonUp = false;

    internal string errorDetails = "";

    public void CopyFrom(ControllerState other) {
      connectionState = other.connectionState;
      orientation = other.orientation;
      gyro = other.gyro;
      accel = other.accel;
      isTouching = other.isTouching;
      touchPos = other.touchPos;
      touchDown = other.touchDown;
      touchUp = other.touchUp;
      recentering = other.recentering;
      recentered = other.recentered;
      clickButtonState = other.clickButtonState;
      clickButtonDown = other.clickButtonDown;
      clickButtonUp = other.clickButtonUp;
      appButtonState = other.appButtonState;
      appButtonDown = other.appButtonDown;
      appButtonUp = other.appButtonUp;
      errorDetails = other.errorDetails;
    }

    /// Resets the transient state (the state variables that represent events, and which are true
    /// for only one frame).
    public void ClearTransientState() {
      touchDown = false;
      touchUp = false;
      recentered = false;
      clickButtonDown = false;
      clickButtonUp = false;
      appButtonDown = false;
      appButtonUp = false;
    }
  }
}
/// @endcond

#endif  // UNITY_HAS_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
