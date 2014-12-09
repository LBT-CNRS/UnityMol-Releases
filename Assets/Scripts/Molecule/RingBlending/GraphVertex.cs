using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is made to convert an atom (postion X,Y,Z) to a object with
/// a list of neighbor. 
/// Each neighbor is a "GraphVertex" object and it's made for a quick cycle search.
/// </summary>
public class GraphVertex
{

	public List<GraphVertex> neighbor = new List<GraphVertex>(); //list of neighboor
	public int idRing = -1 ; //-1 if this atom is not in a ring (only for C or O).
	public char type;
	public int id;
	public string resname;
	public Vector3 coordinate;
	public bool flag=false;
		

	/// <summary>
	/// This function will search recursivly a cycle : 
	/// We "walk" from atom to atom, and if we find the atom which we start of, 
	/// (between 3 and 7 step, for "triangles" to hexagone), we return TRUE to say that
	/// we found a cycle and to add the atom to a list of atoms.
	/// </summary>
	/// <returns><c>true</c>, if cycle was searched, <c>false</c> otherwise.</returns>
	/// <param name="vertex"> list of atoms</param>
	/// <param name="first">Starting atom</param>
	/// <param name="size"> Number of step</param>
	public bool SearchCycle(List<int> vertex,int first, int size=0){
		//we don't want to go to far in checking the size of the cycle.
		if (size>20){
			return false;
		}

		//we search for every neighbor.
		for (int i=0; i<neighbor.Count; i++){
			if (neighbor[i].id==first){
				if (size>3){
					vertex.Add(id);
					return true;
				}

			}

			//if we found the first.
			if(neighbor[i].flag==false){
				flag=true;
				if(neighbor[i].SearchCycle(vertex,first, size+1)==true){
					vertex.Add(id);
					return true;			
				}
			}
		}
		return false;
	}
		


	public bool AlreadyAdded(Dictionary <int,List<int>>  connectivityList, int key, int atom){
		bool added = false;;
		if (connectivityList.ContainsKey(key)){

			for (int j=0; j<connectivityList[key].Count / 3; j++){
				if (connectivityList[key][0+(3*j)] == atom)
					added=true;
			}

			if (added)
				return true;
			else
				return false;
		}
		return false;
	}
		
	public void addInDict(Dictionary <int,List<int>>  connectivityList, int ring1, int atom1, int ring2, int atom2){

		if (! AlreadyAdded(connectivityList, ring1, atom1)){
			if (connectivityList.ContainsKey(ring1))
				connectivityList[ring1].AddMany(atom1, ring2, atom2);
			else
				connectivityList[ring1] = new List<int>( new int [] {atom1, ring2, atom2});
		}
	}
		

	public bool SearchConnection(Dictionary <int,List<int>>  connectivityList, List<int> trashlist, int r1=-1, int size=0){

		//we don't want to go to far in checking the size of the cycle.
		if (size>3)
			return false;

		if (size == 0 ){
			r1 = this.idRing;
			if (this.idRing==-1)
				return false;
		}

		if ((size == 1) && (this.idRing != -1))
			return false;



		//we search for every neighbor.
		for (int i=0; i<neighbor.Count; i++){
			if (this.idRing != -1){
				if ((size==2) || (size == 3)){
					if (this.idRing != r1){
						trashlist.Add(this.idRing);
						trashlist.Add(this.id);
						return true;
					}
				}
			}




			
			//if we found the first.
			if(neighbor[i].flag==false){
				flag=true;
				if(neighbor[i].SearchConnection(connectivityList,trashlist, r1, size+1)==true){
					if (size==0){
						int a1=this.id;
						int r2 = trashlist[0];
						int a2 = trashlist[1];
						if (r1 < r2)
							addInDict(connectivityList, r1, a1, r2, a2);
						else
							addInDict(connectivityList, r2, a2, r1, a1);
						return true;
					}else
						return true;
				}				
			}
		}
		return false;
	}

	
}


