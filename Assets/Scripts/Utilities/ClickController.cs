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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Events;

namespace UMol {
public class ClickController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{

    [SerializeField]
    [Tooltip("How long to trigger a long press")]
    private float holdTime = 0.5f;

    public float timeDoubleLimit = 0.25f;
    public PointerEventData.InputButton button;

    [System.Serializable]
    public class OnSingleClick : UnityEvent {};
    public OnSingleClick onSingleClick;

    [System.Serializable]
    public class OnDoubleClick : UnityEvent {};
    public OnDoubleClick onDoubleClick;

    [System.Serializable]
    public class OnLongClick : UnityEvent {};
    public OnLongClick onLongClick;

    private int clickCount;
    private float firstClickTime;
    private float currentTime;

    bool longPressDelayInvoked = false;
    bool ignoreClickWhenLong = false;
    private Coroutine clickCo;

    private ClickController () {
        clickCount = 0;
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (!longPressDelayInvoked) {
            Invoke("OnLongPress", holdTime);
            longPressDelayInvoked = true;
        }
    }
    public void OnPointerUp(PointerEventData eventData) {
        CancelInvoke("OnLongPress");
        longPressDelayInvoked = false;
    }

    public void OnPointerExit(PointerEventData eventData) {
        CancelInvoke("OnLongPress");
        longPressDelayInvoked = false;
    }
    private void OnLongPress() {
        if (clickCo != null) {
            StopCoroutine(clickCo);
            clickCo = null;
        }
        ignoreClickWhenLong = true;
        onLongClick.Invoke();
        longPressDelayInvoked = false;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (ignoreClickWhenLong) {
            ignoreClickWhenLong = false;
            return;
        }

        if (this.button != eventData.button)
            return;

        this.clickCount++;

        if (this.clickCount == 1) {
            firstClickTime = eventData.clickTime;
            currentTime = firstClickTime;
            clickCo = StartCoroutine(ClickRoutine());
        }
    }

    private IEnumerator ClickRoutine () {

        while (clickCount != 0)
        {
            yield return new WaitForEndOfFrame();

            currentTime += Time.deltaTime;

            if (currentTime >= firstClickTime + timeDoubleLimit) {
                if (clickCount == 1) {
                    onSingleClick.Invoke();
                } else {
                    onDoubleClick.Invoke();
                }
                clickCount = 0;
            }
        }
    }
}
}