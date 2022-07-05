/*
    ================================================================================
    Copyright Centre National de la Recherche Scientifique (CNRS)
        Contributors and copyright holders :

        Xavier Martinez, 2017-2021
        Marc Baaden, 2010-2021
        baaden@smplinux.de
        http://www.baaden.ibpc.fr

        This software is a computer program based on the Unity3D game engine.
        It is part of UnityMol, a general framework whose purpose is to provide
        a prototype for developing molecular graphics and scientific
        visualisation applications. More details about UnityMol are provided at
        the following URL: "http://unitymol.sourceforge.net". Parts of this
        source code are heavily inspired from the advice provided on the Unity3D
        forums and the Internet.

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

        References : 
        If you use this code, please cite the following reference :         
        Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
        "Game on, Science - how video game technology may help biologists tackle
        visualization challenges" (2013), PLoS ONE 8(3):e57990.
        doi:10.1371/journal.pone.0057990
       
        If you use the HyperBalls visualization metaphor, please also cite the
        following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
        B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
        using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
        J. Comput. Chem., 2011, 32, 2924

    Please contact unitymol@gmail.com
    ================================================================================
*/


// Straight Pointer Renderer|PointerRenderers|10020
namespace VRTK
{
    using UnityEngine;

    /// <summary>
    /// A visual pointer representation of a straight beam with an optional cursor at the end.
    /// </summary>
    /// <remarks>
    /// **Optional Components:**
    ///  * `VRTK_PlayAreaCursor` - A Play Area Cursor that will track the position of the pointer cursor.
    ///  * `VRTK_PointerDirectionIndicator` - A Pointer Direction Indicator that will track the position of the pointer cursor.
    /// 
    /// **Script Usage:**
    ///  * Place the `VRTK_StraightPointerRenderer` script on the same GameObject as the Pointer script it is linked to.
    ///  * Link this Pointer Renderer script to the `Pointer Renderer` parameter on the required Pointer script.
    ///
    /// **Script Dependencies:**
    ///  * A Pointer script to control the activation of this Pointer Renderer script.
    /// </remarks>
    /// <example>
    /// `VRTK/Examples/003_Controller_SimplePointer` shows the simple pointer in action and code examples of how the events are utilised and listened to can be viewed in the script `VRTK/Examples/ExampleResources/Scripts/VRTK_ControllerPointerEvents_ListenerExample.cs`
    /// </example>

    [AddComponentMenu("VRTK/Scripts/Pointers/Pointer Renderers/VRTK_StraightPointerRendererWithoutRigidBody")]
    public class VRTK_StraightPointerRendererNoRB : VRTK_BasePointerRenderer
    {
        [Header("Straight Pointer Appearance Settings")]

        [Tooltip("The maximum length the pointer tracer can reach.")]
        public float maximumLength = 100f;
        [Tooltip("The scale factor to scale the pointer tracer object by.")]
        public float scaleFactor = 0.002f;
        [Tooltip("The scale multiplier to scale the pointer cursor object by in relation to the `Scale Factor`.")]
        public float cursorScaleMultiplier = 25f;
        [Tooltip("The cursor will be rotated to match the angle of the target surface if this is true, if it is false then the pointer cursor will always be horizontal.")]
        public bool cursorMatchTargetRotation = false;
        [Tooltip("Rescale the cursor proportionally to the distance from the tracer origin.")]
        public bool cursorDistanceRescale = false;
        [Tooltip("The maximum scale the cursor is allowed to reach. This is only used when rescaling the cursor proportionally to the distance from the tracer origin.")]
        public Vector3 maximumCursorScale = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

        [Header("Straight Pointer Custom Appearance Settings")]

        [Tooltip("A custom game object to use as the appearance for the pointer tracer. If this is empty then a Box primitive will be created and used.")]
        public GameObject customTracer;
        [Tooltip("A custom game object to use as the appearance for the pointer cursor. If this is empty then a Sphere primitive will be created and used.")]
        public GameObject customCursor;

        public GameObject actualContainer;
        public GameObject actualTracer;
        protected GameObject actualCursor;

        protected Vector3 cursorOriginalScale = Vector3.one;

        /// <summary>
        /// The UpdateRenderer method is used to run an Update routine on the pointer.
        /// </summary>
        public override void UpdateRenderer()
        {
            if ((controllingPointer != null && controllingPointer.IsPointerActive()) || IsVisible())
            {
                float tracerLength = CastRayForward();
                SetPointerAppearance(tracerLength);
                MakeRenderersVisible();
            }
            base.UpdateRenderer();
        }

        /// <summary>
        /// The GetPointerObjects returns an array of the auto generated GameObjects associated with the pointer.
        /// </summary>
        /// <returns>An array of pointer auto generated GameObjects.</returns>
        public override GameObject[] GetPointerObjects()
        {
            return new GameObject[] { actualContainer, actualCursor, actualTracer };
        }

        protected override void ToggleRenderer(bool pointerState, bool actualState)
        {
            ToggleElement(actualTracer, pointerState, actualState, tracerVisibility, ref tracerVisible);
            ToggleElement(actualCursor, pointerState, actualState, cursorVisibility, ref cursorVisible);
        }

        protected override void CreatePointerObjects()
        {
            actualContainer = new GameObject(VRTK_SharedMethods.GenerateVRTKObjectName(true, gameObject.name, "StraightPointerRenderer_Container"));
            actualContainer.transform.SetParent(pointerOriginTransformFollowGameObject.transform);
            actualContainer.transform.localPosition = Vector3.zero;
            actualContainer.transform.localRotation = Quaternion.identity;
            actualContainer.transform.localScale = Vector3.one;
            VRTK_PlayerObject.SetPlayerObject(actualContainer, VRTK_PlayerObject.ObjectTypes.Pointer);

            CreateTracer();
            CreateCursor();
            Toggle(false, false);
            if (controllingPointer != null)
            {
                controllingPointer.ResetActivationTimer(true);
                controllingPointer.ResetSelectionTimer(true);
            }
        }

        protected override void DestroyPointerObjects()
        {
            if (actualContainer != null)
            {
                Destroy(actualContainer);
            }
        }

        protected override void ChangeMaterial(Color givenColor)
        {
            base.ChangeMaterial(givenColor);
            ChangeMaterialColor(actualTracer, givenColor);
            ChangeMaterialColor(actualCursor, givenColor);
        }

        protected override void UpdateObjectInteractor()
        {
            base.UpdateObjectInteractor();
            //if the object interactor is too far from the pointer tip then set it to the pointer tip position to prevent glitching.
            if (objectInteractor != null && actualCursor != null && Vector3.Distance(objectInteractor.transform.position, actualCursor.transform.position) > 0f)
            {
                objectInteractor.transform.position = actualCursor.transform.position;
            }
        }

        protected virtual void CreateTracer()
        {
            if (customTracer != null)
            {
                actualTracer = Instantiate(customTracer);
            }
            else
            {
                actualTracer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                actualTracer.GetComponent<BoxCollider>().isTrigger = true;
                // actualTracer.AddComponent<Rigidbody>().isKinematic = true;
                actualTracer.layer = LayerMask.NameToLayer("Ignore Raycast");

                SetupMaterialRenderer(actualTracer);
            }

            actualTracer.transform.name = VRTK_SharedMethods.GenerateVRTKObjectName(true, gameObject.name, "StraightPointerRenderer_Tracer");
            actualTracer.transform.SetParent(actualContainer.transform);

            VRTK_PlayerObject.SetPlayerObject(actualTracer, VRTK_PlayerObject.ObjectTypes.Pointer);
        }

        protected virtual void CreateCursor()
        {
            if (customCursor != null)
            {
                actualCursor = Instantiate(customCursor);
            }
            else
            {
                actualCursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                actualCursor.transform.localScale = Vector3.one * (scaleFactor * cursorScaleMultiplier);
                actualCursor.GetComponent<Collider>().isTrigger = true;
                // actualCursor.AddComponent<Rigidbody>().isKinematic = true;
                actualCursor.layer = LayerMask.NameToLayer("Ignore Raycast");

                SetupMaterialRenderer(actualCursor);
            }

            cursorOriginalScale = actualCursor.transform.localScale;
            actualCursor.transform.name = VRTK_SharedMethods.GenerateVRTKObjectName(true, gameObject.name, "StraightPointerRenderer_Cursor");
            actualCursor.transform.SetParent(actualContainer.transform);
            VRTK_PlayerObject.SetPlayerObject(actualCursor, VRTK_PlayerObject.ObjectTypes.Pointer);
        }

        protected virtual void CheckRayMiss(bool rayHit, RaycastHit pointerCollidedWith)
        {
            if (!rayHit || (destinationHit.collider != null && destinationHit.collider != pointerCollidedWith.collider))
            {
                if (destinationHit.collider != null)
                {
                    PointerExit(destinationHit);
                }

                destinationHit = new RaycastHit();
                ChangeColor(invalidCollisionColor);
            }
        }

        protected virtual void CheckRayHit(bool rayHit, RaycastHit pointerCollidedWith)
        {
            if (rayHit)
            {
                PointerEnter(pointerCollidedWith);

                destinationHit = pointerCollidedWith;
                ChangeColor(validCollisionColor);
            }
        }

        public void SetValidColor(){
            ChangeColor(validCollisionColor);
        }

        protected virtual float CastRayForward()
        {
            Transform origin = GetOrigin();
            Ray pointerRaycast = new Ray(origin.position, origin.forward);
            RaycastHit pointerCollidedWith;
            bool rayHit = VRTK_CustomRaycast.Raycast(customRaycast, pointerRaycast, out pointerCollidedWith, defaultIgnoreLayer, maximumLength);

            CheckRayMiss(rayHit, pointerCollidedWith);
            CheckRayHit(rayHit, pointerCollidedWith);

            float actualLength = maximumLength;
            if (rayHit && pointerCollidedWith.distance < maximumLength)
            {
                actualLength = pointerCollidedWith.distance;
            }

            return OverrideBeamLength(actualLength);
        }

        protected virtual void SetPointerAppearance(float tracerLength)
        {
            if (actualContainer != null)
            {
                //if the additional decimal isn't added then the beam position glitches
                float beamPosition = tracerLength / (2f + BEAM_ADJUST_OFFSET);

                actualTracer.transform.localScale = new Vector3(scaleFactor, scaleFactor, tracerLength);
                actualTracer.transform.localPosition = Vector3.forward * beamPosition;
                actualCursor.transform.localScale = Vector3.one * (scaleFactor * cursorScaleMultiplier);
                actualCursor.transform.localPosition = new Vector3(0f, 0f, tracerLength);

                Transform origin = GetOrigin();

                float objectInteractorScaleIncrease = 1.05f;
                ScaleObjectInteractor(actualCursor.transform.lossyScale * objectInteractorScaleIncrease);

                if (destinationHit.transform != null)
                {
                    if (cursorMatchTargetRotation)
                    {
                        actualCursor.transform.forward = -destinationHit.normal;
                    }
                    if (cursorDistanceRescale)
                    {
                        float collisionDistance = Vector3.Distance(destinationHit.point, origin.position);
                        actualCursor.transform.localScale = Vector3.Min(cursorOriginalScale * collisionDistance, maximumCursorScale);
                    }
                }
                else
                {
                    if (cursorMatchTargetRotation)
                    {
                        actualCursor.transform.forward = origin.forward;
                    }
                    if (cursorDistanceRescale)
                    {
                        actualCursor.transform.localScale = Vector3.Min(cursorOriginalScale * tracerLength, maximumCursorScale);
                    }
                }

                ToggleRenderer(controllingPointer.IsPointerActive(), false);
                UpdateDependencies(actualCursor.transform.position);
            }
        }
    }
}