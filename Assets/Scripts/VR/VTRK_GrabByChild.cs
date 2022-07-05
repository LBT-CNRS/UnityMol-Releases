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


// Child Of Controller Grab Attach|GrabAttachMechanics|50060
namespace VRTK.GrabAttachMechanics
{
    using UnityEngine;

    /// <summary>
    /// Sets the grabbed Interactable Object to be a child of the grabbing object.
    /// </summary>
    /// <remarks>
    ///   > The Interactable Object will have 1:1 tracking of the grabbing object, however it will also have reduced collision detection and will be able to pass through other colliders.
    ///
    /// **Script Usage:**
    ///  * Place the `VRTK_ChildOfControllerGrabAttach` script on either:
    ///    * The GameObject of the Interactable Object to detect interactions on.
    ///    * Any other scene GameObject and then link that GameObject to the Interactable Objects `Grab Attach Mechanic Script` parameter to denote use of the grab mechanic.
    /// </remarks>
    /// <example>
    /// `VRTK/Examples/023_Controller_ChildOfControllerOnGrab` uses this grab attach mechanic for the bow and the arrow.
    /// </example>
    [AddComponentMenu("VRTK/Scripts/Interactions/Interactables/Grab Attach Mechanics/VRTK_ChildOfControllerGrabAttach")]
    public class VRTK_GrabByChild : VRTK_BaseGrabAttach
    {
        /// <summary>
        /// The StartGrab method sets up the grab attach mechanic as soon as an Interactable Object is grabbed. It is also responsible for creating the joint on the grabbed object.
        /// </summary>
        /// <param name="grabbingObject">The GameObject that is doing the grabbing.</param>
        /// <param name="givenGrabbedObject">The GameObject that is being grabbed.</param>
        /// <param name="givenControllerAttachPoint">The point on the grabbing object that the grabbed object should be attached to after grab occurs.</param>
        /// <returns>Returns `true` if the grab is successful, `false` if the grab is unsuccessful.</returns>
        public override bool StartGrab(GameObject grabbingObject, GameObject givenGrabbedObject, Rigidbody givenControllerAttachPoint)
        {


            if (base.StartGrab(grabbingObject, givenGrabbedObject, givenControllerAttachPoint))
            {
                SnapObjectToGrabToController(grabbingObject, givenGrabbedObject);
                // grabbedObjectScript.isKinematic = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// The StopGrab method ends the grab of the current Interactable Object and cleans up the state.
        /// </summary>
        /// <param name="applyGrabbingObjectVelocity">If `true` will apply the current velocity of the grabbing object to the grabbed object on release.</param>
        public override void StopGrab(bool applyGrabbingObjectVelocity)
        {
            ReleaseObject(applyGrabbingObjectVelocity);
            base.StopGrab(applyGrabbingObjectVelocity);
        }

        protected override void Initialise()
        {
            tracked = false;
            climbable = false;
            kinematic = true;
        }


        protected virtual void SnapObjectToGrabToController(GameObject grabbingObject, GameObject obj)
        {
            obj.transform.SetParent(grabbingObject.transform.parent);
        }
    }
}