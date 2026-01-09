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
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {

	public PooledObject prefab;
	public int activeCount = 0;

	List<PooledObject> availableObjects = new List<PooledObject>();

	// public static ObjectPool GetPool (PooledObject prefab) {
	// 	GameObject obj;
	// 	ObjectPool pool;
	// 	if (Application.isEditor) {
	// 		obj = GameObject.Find(prefab.name + " Pool");
	// 		if (obj) {
	// 			pool = obj.GetComponent<ObjectPool>();
	// 			if (pool) {
	// 				return pool;
	// 			}
	// 		}
	// 	}
	// 	obj = new GameObject(prefab.name + " Pool");
	// 	pool = obj.AddComponent<ObjectPool>();
	// 	pool.prefab = prefab;
	// 	return pool;
	// }

	public PooledObject GetObject () {
		PooledObject obj;
		int lastAvailableIndex = availableObjects.Count - 1;
		if (lastAvailableIndex >= 0) {
			obj = availableObjects[lastAvailableIndex];
			availableObjects.RemoveAt(lastAvailableIndex);
			obj.gameObject.SetActive(true);
		}
		else {
			obj = Instantiate<PooledObject>(prefab);
			// obj.transform.SetParent(transform, false);
			obj.Pool = this;
			obj.transform.position = new Vector3(0.0f,0.0f,10.0f);
		}
		activeCount++;
		return obj;
	}

	public void AddObject (PooledObject obj) {
		obj.gameObject.SetActive(false);
		availableObjects.Add(obj);
		activeCount--;
	}
}