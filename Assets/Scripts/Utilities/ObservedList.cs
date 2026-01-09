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
using System;
using System.Collections;
using System.Collections.Generic;

namespace UMol {
[Serializable]
public class ObservedList<T> : List<T>
{
    public event Action<int> Changed;

    public new void Add(T item)
    {
        base.Add(item);
        InvokeChanged();
    }

    public new void Remove(T item)
    {
        base.Remove(item);
        InvokeChanged();
    }
    public new void AddRange(IEnumerable<T> collection)
    {
        base.AddRange(collection);
        InvokeChanged();
    }
    public new void RemoveRange(int index, int count)
    {
        base.RemoveRange(index, count);
        InvokeChanged();
    }
    public new void Clear()
    {
        base.Clear();
        InvokeChanged();
    }
    public new void Insert(int index, T item)
    {
        base.Insert(index, item);
        InvokeChanged();
    }
    public new void InsertRange(int index, IEnumerable<T> collection)
    {
        base.InsertRange(index, collection);
        InvokeChanged();
    }
    public new void RemoveAll(Predicate<T> match)
    {
        base.RemoveAll(match);
        InvokeChanged();
    }

    public new T this[int index]
    {
        get
        {
            return base[index];
        }
        set
        {
            base[index] = value;
            Changed(index);
        }
    }

    void InvokeChanged()
    {
        if (Changed != null)
            Changed(this.Count);
    }
}
}