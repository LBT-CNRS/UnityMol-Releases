//========= Copyright 2016-2020, HTC Corporation. All rights reserved. ===========

using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HTC.UnityPlugin.Pointer3D;


namespace UMol
{
[DisallowMultipleComponent]
public class CustomRaycastMethod : BaseRaycastMethod
{
    private static readonly IndexedSet<ICanvasRaycastTarget> canvases = new IndexedSet<ICanvasRaycastTarget>();

    public static bool AddTarget(ICanvasRaycastTarget obj) { return obj == null ? false : canvases.AddUnique(obj); }

    public static bool RemoveTarget(ICanvasRaycastTarget obj) { return obj == null ? false : canvases.Remove(obj); }

    private CustomRaycastBurst crb;
    private GameObject goAtom;

    public override void Raycast(Ray ray, float distance, List<RaycastResult> raycastResults)
    {
        if (crb == null)
            crb = UnityMolMain.getCustomRaycast();
        if(goAtom == null)
            goAtom = new GameObject("AtomGo");

        if (!ViveInput.GetPress(HandRole.RightHand, ControllerButton.PadTouch) && !ViveInput.GetPress(HandRole.LeftHand, ControllerButton.PadTouch)) {
            return;
        }
        bool isExtrAtom = false;
        Vector3 p = Vector3.zero;
        UnityMolAtom a = crb.customRaycastAtomBurst(
                             ray.origin,
                             ray.direction,
                             ref p, ref isExtrAtom, true);

        if (a != null) {
            UnityMolStructure s = a.residue.chain.model.structure;

            UnityMolMain.getAnnotationManager().setGOPos(a, goAtom);
            
            raycastResults.Add(new RaycastResult
            {
                gameObject = goAtom,
                module = raycaster,
                distance = Vector3.Distance(p, ray.origin),
                worldPosition = p,
                worldNormal = -ray.direction,
                screenPosition = Pointer3DInputModule.ScreenCenterPoint,
                index = 0,
                sortingLayer = 0,
                sortingOrder = 0
            });
        }
    }
}
}