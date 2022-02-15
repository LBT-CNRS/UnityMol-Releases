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


// KDTree.cs - A Stark, September 2009.

//  This class implements a data structure that stores a list of points in space.
//  A common task in game programming is to take a supplied point and discover which
//  of a stored set of points is nearest to it. For example, in path-plotting, it is often
//  useful to know which waypoint is nearest to the player's current
//  position. The kd-tree allows this "nearest neighbour" search to be carried out quickly,
//  or at least much more quickly than a simple linear search through the list.

//  At present, the class only allows for construction (using the MakeFromPoints static method)
//  and nearest-neighbour searching (using FindNearest). More exotic kd-trees are possible, and
//  this class may be extended in the future if there seems to be a need.

//  The nearest-neighbour search returns an integer index - it is assumed that the original
//  array of points is available for the lifetime of the tree, and the index refers to that
//  array.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UMol {

public class KDTree {

    public KDTree[] lr;
    public Vector3 pivot;
    public int pivotIndex;
    public int axis;

    // Change this value to 2 if you only need two-dimensional X,Y points. The search will
    // be quicker in two dimensions.
    const int numDims = 3;


    public KDTree() {
        lr = new KDTree[2];
    }


    // Make a new tree from a list of points.
    public static KDTree MakeFromPoints(params Vector3[] points) {
        if(points.Length == 0){
            return null;
        }
        int[] indices = Iota(points.Length);
        return MakeFromPointsInner(0, 0, points.Length - 1, points, indices);
    }

    ///Make sure the atoms are part of the same structure
    ///otherwise you don't get the correct local positions when getting atom.position
    public static KDTree MakeFromUnityMolAtoms(List<UnityMolAtom> atoms) {
        if(atoms.Count == 0){
            return null;
        }
        int[] indices = Iota(atoms.Count);
        return MakeFromAtomsInner(0, 0, atoms.Count - 1, atoms, indices);
    }

    // Recursively build a tree by separating points at plane boundaries.
    static KDTree MakeFromAtomsInner(int depth, int stIndex, int enIndex, List<UnityMolAtom> atoms, int[] inds) {
        KDTree root = new KDTree();
        root.axis = depth % numDims;
        int splitPoint = FindPivotIndexAtoms(atoms, inds, stIndex, enIndex, root.axis);

        root.pivotIndex = inds[splitPoint];
        root.pivot = atoms[root.pivotIndex].position;

        int leftEndIndex = splitPoint - 1;

        if (leftEndIndex >= stIndex) {
            root.lr[0] = MakeFromAtomsInner(depth + 1, stIndex, leftEndIndex, atoms, inds);
        }

        int rightStartIndex = splitPoint + 1;

        if (rightStartIndex <= enIndex) {
            root.lr[1] = MakeFromAtomsInner(depth + 1, rightStartIndex, enIndex, atoms, inds);
        }

        return root;
    }

    // Recursively build a tree by separating points at plane boundaries.
    static KDTree MakeFromPointsInner(int depth, int stIndex, int enIndex, Vector3[] points, int[] inds) {
        KDTree root = new KDTree();
        root.axis = depth % numDims;
        int splitPoint = FindPivotIndex(points, inds, stIndex, enIndex, root.axis);

        root.pivotIndex = inds[splitPoint];
        root.pivot = points[root.pivotIndex];

        int leftEndIndex = splitPoint - 1;

        if (leftEndIndex >= stIndex) {
            root.lr[0] = MakeFromPointsInner(depth + 1, stIndex, leftEndIndex, points, inds);
        }

        int rightStartIndex = splitPoint + 1;

        if (rightStartIndex <= enIndex) {
            root.lr[1] = MakeFromPointsInner(depth + 1, rightStartIndex, enIndex, points, inds);
        }

        return root;
    }


    static void SwapElements(int[] arr, int a, int b) {
        int temp = arr[a];
        arr[a] = arr[b];
        arr[b] = temp;
    }

    // Simple "median of three" heuristic to find a reasonable splitting plane.
    static int FindSplitPointAtoms(List<UnityMolAtom> atoms, int[] inds, int stIndex, int enIndex, int axis) {

        float a = atoms[inds[stIndex]].position[axis];
        float b = atoms[inds[enIndex]].position[axis];
        int midIndex = (stIndex + enIndex) / 2;
        float m = atoms[inds[midIndex]].position[axis];

        if (a > b) {
            if (m > a) {
                return stIndex;
            }

            if (b > m) {
                return enIndex;
            }

            return midIndex;
        } else {
            if (a > m) {
                return stIndex;
            }

            if (m > b) {
                return enIndex;
            }

            return midIndex;
        }
    }
    //  Simple "median of three" heuristic to find a reasonable splitting plane.
    static int FindSplitPoint(Vector3[] points, int[] inds, int stIndex, int enIndex, int axis) {
        float a = points[inds[stIndex]][axis];
        float b = points[inds[enIndex]][axis];
        int midIndex = (stIndex + enIndex) / 2;
        float m = points[inds[midIndex]][axis];

        if (a > b) {
            if (m > a) {
                return stIndex;
            }

            if (b > m) {
                return enIndex;
            }

            return midIndex;
        } else {
            if (a > m) {
                return stIndex;
            }

            if (m > b) {
                return enIndex;
            }

            return midIndex;
        }
    }

    // Find a new pivot index from the range by splitting the points that fall either side
    // of its plane.
    public static int FindPivotIndexAtoms(List<UnityMolAtom> atoms, int[] inds, int stIndex, int enIndex, int axis) {
        int splitPoint = FindSplitPointAtoms(atoms, inds, stIndex, enIndex, axis);
        // int splitPoint = Random.Range(stIndex, enIndex);

        Vector3 pivot = atoms[inds[splitPoint]].position;
        SwapElements(inds, stIndex, splitPoint);

        int currPt = stIndex + 1;
        int endPt = enIndex;

        while (currPt <= endPt) {
            Vector3 curr = atoms[inds[currPt]].position;

            if ((curr[axis] > pivot[axis])) {
                SwapElements(inds, currPt, endPt);
                endPt--;
            } else {
                SwapElements(inds, currPt - 1, currPt);
                currPt++;
            }
        }
        return currPt - 1;
    }

    // Find a new pivot index from the range by splitting the points that fall either side
    // of its plane.
    public static int FindPivotIndex(Vector3[] points, int[] inds, int stIndex, int enIndex, int axis) {
        int splitPoint = FindSplitPoint(points, inds, stIndex, enIndex, axis);
        // int splitPoint = Random.Range(stIndex, enIndex);

        Vector3 pivot = points[inds[splitPoint]];
        SwapElements(inds, stIndex, splitPoint);

        int currPt = stIndex + 1;
        int endPt = enIndex;

        while (currPt <= endPt) {
            Vector3 curr = points[inds[currPt]];

            if ((curr[axis] > pivot[axis])) {
                SwapElements(inds, currPt, endPt);
                endPt--;
            } else {
                SwapElements(inds, currPt - 1, currPt);
                currPt++;
            }
        }

        return currPt - 1;
    }

    public static int[] Iota(int num) {
        int[] result = new int[num];

        for (int i = 0; i < num; i++) {
            result[i] = i;
        }

        return result;
    }

    // Find the nearest point in the set to the supplied point.
    public int FindNearest(Vector3 pt) {
        float bestSqDist = 1000000000f;
        int bestIndex = -1;

        Search(pt, ref bestSqDist, ref bestIndex);

        return bestIndex;
    }

    public float FindNearest_R(Vector3 pt) {
        float bestSqDist = 1000000000f;
        int bestIndex = -1;

        Search(pt, ref bestSqDist, ref bestIndex);

        return (Mathf.Sqrt(bestSqDist));
    }

    // Recursively search the tree.
    void Search(Vector3 pt, ref float bestSqSoFar, ref int bestIndex) {
        float mySqDist = (pivot - pt).sqrMagnitude;

        if (mySqDist < bestSqSoFar) {
            bestSqSoFar = mySqDist;
            bestIndex = pivotIndex;
        }

        float planeDist = pt[axis] - pivot[axis]; // DistFromSplitPlane(pt, pivot, axis);

        int selector = planeDist <= 0 ? 0 : 1;

        if (lr[selector] != null) {
            lr[selector].Search(pt, ref bestSqSoFar, ref bestIndex);
        }

        selector = (selector + 1) % 2;

        float sqPlaneDist = planeDist * planeDist;

        if ((lr[selector] != null) && (bestSqSoFar > sqPlaneDist)) {
            lr[selector].Search(pt, ref bestSqSoFar, ref bestIndex);
        }
    }

    // Get a point's distance from an axis-aligned plane.
    float DistFromSplitPlane(Vector3 pt, Vector3 planePt, int axis) {
        return pt[axis] - planePt[axis];
    }

    public int FindNearestK(Vector3 pt, int k) {
        // Find and returns k-th nearest neighbour
        float bestSqDist = 1000000000f;
        float minSqDist = 0f;
        int bestIndex = -1;

        for (int i = 0; i < k - 1; i++) {
            SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);

            minSqDist = bestSqDist;
            bestSqDist = 1000000000f;
            bestIndex = -1;
        }

        SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);

        return bestIndex;
    }
    // public int FindNearestK( Vector3 pt, int k )
    //    {
    //        // Find and returns    k-th nearest neighbour
    //        float bestSqDist = 1000000000f;
    //        float minSqDist = -1.0f;
    //        int bestIndex = -1;
    //        HashSet<int> _pivotIndexSet = new HashSet<int>();

    //        for ( int i = 0; i < k - 1; i++ )
    //        {
    //            SearchK( pt, _pivotIndexSet, ref bestSqDist, ref minSqDist, ref bestIndex );
    //            _pivotIndexSet.Add( bestIndex );

    //            minSqDist = bestSqDist;
    //            bestSqDist = 1000000000f;
    //            bestIndex = -1;
    //        }

    //        SearchK( pt, _pivotIndexSet, ref bestSqDist, ref minSqDist, ref bestIndex );

    //        return bestIndex;
    //    }
    public int[] FindNearestsK(Vector3 pt, int k) {
        // Find and returns all k neighbours
        float bestSqDist = 1000000000f;
        float minSqDist = 0f;
        int bestIndex = -1;
        int[] bestIndexK = new int[k];

        for (int i = 0; i < k - 1; i++) {
            SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);
            bestIndexK[i] = bestIndex;

            minSqDist = bestSqDist;
            bestSqDist = 1000000000f;
            bestIndex = -1;
        }

        SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);
        bestIndexK[k - 1] = bestIndex;


        return bestIndexK;
    }
    public int[] FindNearestsRadius(Vector3 pt, float radius) {
        // Find and returns all neighbour in range

        float minDist = 0.0f;
        int k = 5;
        List<int> results = new List<int>();

        IdDist[] nearests = null;
        while (minDist < radius) {
            k *= 2;
            nearests = FindNearestsK_IndicesDistances(pt, k);
            minDist = nearests[k - 1].dist;
        }

        for (int i = 0; i < nearests.Length; i++) {
            if (nearests[i].dist > 0 && nearests[i].dist <= radius) {
                results.Add(nearests[i].id);
            }
        }
        return results.ToArray();
    }


    public float FindNearestK_R(Vector3 pt, int k) {
        // Find and returns k-th nearest neighbour distance
        float bestSqDist = 1000000000f;
        float minSqDist = 0f;
        int bestIndex = -1;

        for (int i = 0; i < k - 1; i++) {
            SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);

            minSqDist = bestSqDist;
            bestSqDist = 1000000000f;
            bestIndex = -1;
        }

        SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);

        return (Mathf.Sqrt(bestSqDist));
    }

    public float[] FindNearestsK_R(Vector3 pt, int k) {
        // Find and returns all k neighbours distances
        float bestSqDist = 1000000000f;
        float minSqDist = 0f;
        int bestIndex = -1;
        float[] bestDistances = new float[k];

        for (int i = 0; i < k - 1; i++) {
            SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);
            bestDistances[i] = Mathf.Sqrt(bestSqDist);

            minSqDist = bestSqDist;
            bestSqDist = 1000000000f;
            bestIndex = -1;
        }

        SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);
        bestDistances[k - 1] = Mathf.Sqrt(bestSqDist);

        return bestDistances;
    }

    public IdDist[] FindNearestsK_IndicesDistances(Vector3 pt, int k) {
        // Find and returns all k neighbours indices and distances
        float bestSqDist = 1000000000f;
        float minSqDist = 0f;
        int bestIndex = -1;
        IdDist[] bestDistances = new IdDist[k];

        for (int i = 0; i < k - 1; i++) {
            SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);
            IdDist newIdDist;
            newIdDist.id = bestIndex;
            newIdDist.dist = Mathf.Sqrt(bestSqDist);
            bestDistances[i] = newIdDist;

            minSqDist = bestSqDist;
            bestSqDist = 1000000000f;
            bestIndex = -1;
        }

        SearchK(pt, ref bestSqDist, ref minSqDist, ref bestIndex);

        IdDist iddist;
        iddist.id = bestIndex;
        iddist.dist = Mathf.Sqrt(bestSqDist);
        bestDistances[k - 1] = iddist;

        return bestDistances;
    }


    // void SearchK( Vector3 pt, HashSet<int> p_pivotIndexSet, ref float bestSqSoFar, ref float minSqDist, ref int bestIndex )
    //    {
    //        float mySqDist = ( pivot - pt ).sqrMagnitude;

    //        if ( mySqDist < bestSqSoFar )
    //        {
    //            if ( mySqDist >= minSqDist && !p_pivotIndexSet.Contains( pivotIndex ) )
    //            {
    //                bestSqSoFar = mySqDist;
    //                bestIndex = pivotIndex;
    //            }
    //        }

    void SearchK(Vector3 pt, ref float bestSqSoFar, ref float minSqDist, ref int bestIndex) {
        float mySqDist = (pivot - pt).sqrMagnitude;

        if (mySqDist < bestSqSoFar) {
            if (mySqDist > minSqDist) {
                bestSqSoFar = mySqDist;
                bestIndex = pivotIndex;
            }
        }

        float planeDist = pt[axis] - pivot[axis]; //DistFromSplitPlane(pt, pivot, axis);

        int selector = planeDist <= 0 ? 0 : 1;

        if (lr[selector] != null) {
            lr[selector].SearchK(pt, ref bestSqSoFar, ref minSqDist, ref bestIndex);
        }

        selector = (selector + 1) % 2;

        float sqPlaneDist = planeDist * planeDist;

        if ((lr[selector] != null) && (bestSqSoFar > sqPlaneDist)) {
            lr[selector].SearchK(pt, ref bestSqSoFar, ref minSqDist, ref bestIndex);
        }
    }









    //  Simple output of tree structure - mainly useful for getting a rough
    //  idea of how deep the tree is (and therefore how well the splitting
    //  heuristic is performing).
    public string Dump(int level) {
        string result = pivotIndex.ToString().PadLeft(level) + "\n";

        if (lr[0] != null) {
            result += lr[0].Dump(level + 2);
        }

        if (lr[1] != null) {
            result += lr[1].Dump(level + 2);
        }

        return result;
    }
}
public struct IdDist {
    public int id;
    public float dist;
}
}