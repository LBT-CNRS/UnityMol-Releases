﻿// Copyright 2015 Google Inc. All rights reserved.
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
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;

/// This script provides an interface for gaze based input pointers used with
/// the GazeInputModule script.
///
/// It provides methods called on gaze interaction with in-game objects and UI,
/// trigger events, and 'BaseInputModule' class state changes.
///
/// To have the methods called, an instance of this (implemented) class must be
/// registered with the **GazeInputModule** script on 'OnEnable' by assigning
/// itself to the **GazeInputModule.gazePointer** static member variable.
/// A registered instance should also un-register itself at 'OnDisable' calls
/// by setting the **GazeInputModule.gazePointer** static member variable
/// to null.
///
/// This class is expected to be inherited by pointers responding to the user's
/// looking at objects in the scene by the movement of their head. For example,
/// see the GvrReticle class.
public interface IGvrGazePointer {
  /// This is called when the 'BaseInputModule' system should be enabled.
  void OnGazeEnabled();
  /// This is called when the 'BaseInputModule' system should be disabled.
  void OnGazeDisabled();

  /// Called when the user is looking on a valid GameObject. This can be a 3D
  /// or UI element.
  ///
  /// The camera is the event camera, the target is the object
  /// the user is looking at, and the intersectionPosition is the intersection
  /// point of the ray sent from the camera on the object.
  void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                   bool isInteractive);

  /// Called every frame the user is still looking at a valid GameObject. This
  /// can be a 3D or UI element.
  ///
  /// The camera is the event camera, the target is the object the user is
  /// looking at, and the intersectionPosition is the intersection point of the
  /// ray sent from the camera on the object.
  void OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                  bool isInteractive);

  /// Called when the user's look no longer intersects an object previously
  /// intersected with a ray projected from the camera.
  /// This is also called just before **OnGazeDisabled** and may have have any of
  /// the values set as **null**.
  ///
  /// The camera is the event camera and the target is the object the user
  /// previously looked at.
  void OnGazeExit(Camera camera, GameObject targetObject);

  /// Called when a trigger event is initiated. This is practically when
  /// the user begins pressing the trigger.
  void OnGazeTriggerStart(Camera camera);

  /// Called when trigger event is finished. This is practically when
  /// the user releases the trigger.
  void OnGazeTriggerEnd(Camera camera);

  /// Return the radius of the gaze pointer. This is used when searching for
  /// valid gaze targets. If a radius is 0, the GvrGaze will use a ray
  /// to find a valid gaze target. Otherwise it will use a SphereCast.
  /// The *innerRadius* is used for finding new targets while the *outerRadius*
  /// is used to see if you are still nearby the object currently looked at
  /// to avoid a flickering effect when just at the border of the intersection.
  void GetPointerRadius(out float innerRadius, out float outerRadius);
}
