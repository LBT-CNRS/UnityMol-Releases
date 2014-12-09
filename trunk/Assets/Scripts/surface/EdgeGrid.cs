using UnityEngine;
using System.Collections;

public class EdgeGrid {
	
	private EdgeArray[,,] _grid;

	public EdgeGrid(int x, int y, int z) {
		Debug.Log("Creating the EdgeGrid: " + Time.realtimeSinceStartup.ToString());
		
		_grid = new EdgeArray[x,y,z];
		
		for(int i=0; i<x; i++) {
			for (int j=0; j<y; j++) {
				for (int k=0; k<z; k++) {
					_grid[i,j,k] = new EdgeArray();
				}
			}
		}
		
		Debug.Log("EdgeGrid created: " + Time.realtimeSinceStartup.ToString());
	} // End of constructor
	
	/// <summary>
	/// Adds the index of the vertex to the edge grid.
	/// </summary>
	/// <returns>
	/// Either the index of the vertex that was already there if
	/// if existed, or the index of the new one.
	/// </returns>
	/// <param name='x'>
	/// X.
	/// </param>
	/// <param name='y'>
	/// Y.
	/// </param>
	/// <param name='z'>
	/// Z.
	/// </param>
	/// <param name='index'>
	/// Index.
	/// </param>
	public int AddVertex(int x, int y, int z, int t, int index) {
		if(_grid[x,y,z].GetEdge(t) == -1)
			_grid[x,y,z].SetEdge(t, index);
		return _grid[x,y,z].GetEdge(t);
	}
	
}
