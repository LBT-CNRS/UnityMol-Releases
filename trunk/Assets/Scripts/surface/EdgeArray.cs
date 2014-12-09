using UnityEngine;
using System.Collections;

public class EdgeArray {
	
	private int[] edgeOccupationArray;
	
	public EdgeArray() {
		edgeOccupationArray = new int[12] {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1};
	}
	
	public int GetEdge(int index) {
		return edgeOccupationArray[index];
	}
	
	public void SetEdge(int index, int vertexIndex) {
		edgeOccupationArray[index] = vertexIndex;
	}

}
