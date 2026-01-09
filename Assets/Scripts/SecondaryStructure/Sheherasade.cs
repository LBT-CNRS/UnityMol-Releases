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
using System.Linq;
using System;
using System.Threading;

namespace UMol
{
/// <summary>
/// Class used for generating the mesh for Sheherasade representation
/// </summary>
public class Sheherasade
{
	public static Texture arrowTexture;
	/// <summary>
	/// number of point in a tile (need to be odd)
	/// </summary>
	public static int resolution_tile = 9;

	/// <summary>
	/// default color for the sheet
	/// </summary>
	public static Color32 sheetColor = Color.red;

	/// <summary>
	/// Semaphore 1 for the first part of the parallel calcul
	/// </summary>
	public static System.Object lock_1 = new System.Object();

	/// <summary>
	/// Semaphore 2 for the second part of the parallel calcul
	/// </summary>
	public static System.Object lock_2 = new System.Object();

	/// <summary>
	/// creates and returns the mesh of the SheHeRASADe representation
	/// parametter : a list of residu, the UV list, list of color for the residu, list for color by physic
	/// and bool for the straight/smooth surface
	/// </summary>
	public static MeshData CreateChainMesh(UnityMolSelection sel, int idFrame,
	                                       List<UnityMolResidue> residues, List<Vector2> uv_Map,
	                                       ref Dictionary<UnityMolResidue, List<int>> residueToVert,
	                                       bool bezier = true)
	{

		if (arrowTexture == null) {
			arrowTexture = (Texture) Resources.Load("Images/Sheherasade_Beta_Sheet");
		}

		// sav of the corner, to compute the normal
		Dictionary<UnityMolResidue, List<float>[]> corner = new Dictionary<UnityMolResidue, List<float>[]>();
		// List of the vertice for the Mesh
		List<Vector3> vertices = new List<Vector3>();
		// List of thriangle for the Mesh
		List<int> triangles = new List<int>();
		// List of color for the Uni Color of the Mesh
		List<Color32> colors = new List<Color32>();
		//List of normal of the Mesh
		List<Vector3> normals = new List<Vector3>();
		// List of all the strand of the molecule
		List<Strand> strands = new List<Strand>();
		//List of all the tile find in the molecule
		List<BezierTile> bezierTiles = new List<BezierTile>();
		//Dicionary that's use to find the edge that are shared by multiple tiles and fix the normal
		Dictionary<UnityMolResidue, Dictionary<UnityMolResidue, Sav_Vert>> vertices_Sav =
		    new Dictionary<UnityMolResidue, Dictionary<UnityMolResidue, Sav_Vert>>();

		//Find all the Beta strand in the protein
		CreateStrands(strands, residues);

		// Find the Hydrogen between the Beta strand
		Dictionary<UnityMolResidue, List<UnityMolResidue>> hydrogenBonds = FindHydrogenBonds(strands, sel, idFrame);

		//Make all the tile frome the beta strand
		CreateBezierTiles(bezierTiles, strands, hydrogenBonds, sel, idFrame);

		//creates smooth surfaces for each Bezier tile
		if (bezier)
		{
			//Calcul all normal of each tile.
			MakeSheets(bezierTiles);

			Thread[] tab_thread = new Thread[bezierTiles.Count()];
			for (int i = 0; i < bezierTiles.Count(); i++)
			{
				/*Thread*/
				/**/
				ThreadWork thr = new ThreadWork(bezierTiles[i], vertices, colors, triangles, uv_Map,
				                                normals, vertices_Sav, residueToVert, corner);
				tab_thread[i] = new Thread(new ThreadStart(thr.TileToMesh));
				tab_thread[i].Start();
				/**/

				/*No thread*/
				/*
				TileToMesh(bezierTiles[i], vertices, colors, triangles, uv_Map,
					colorbyR, ColorPhy, normals, vertices_Sav, corner);
				/**/
			}
			/**/
			for (int i = 0; i < bezierTiles.Count(); i++)
				tab_thread[i].Join();
			/**/
		}

		//creates Straight surfaces for each Bezier tile ( no bezier )
		else
		{
			Thread[] tab_thread = new Thread[bezierTiles.Count()];
			for (int i = 0; i < bezierTiles.Count(); i++)
			{
				ThreadWork thr = new ThreadWork(bezierTiles[i], vertices, colors, residueToVert, triangles, uv_Map);
				tab_thread[i] = new Thread(new ThreadStart(thr.TileToMesh_2));
				tab_thread[i].Start();
			}
			for (int i = 0; i < bezierTiles.Count(); i++)
				tab_thread[i].Join();

		}

		// make the mesh
		MeshData mesh = new MeshData
		{
			triangles = triangles.ToArray(),
			vertices = vertices.ToArray(),
			colors = colors.ToArray(),
			normals = normals.ToArray()
		};

		return mesh;
	}

	/// <summary>
	/// Fill the list of beta-strands by going through
	/// the list of residues and determining if they
	/// belongs to a strand secondary structure type
	/// </summary>
	private static void CreateStrands( List<Strand> strands, List<UnityMolResidue> residues)
	{
		Strand tmp = new Strand();

		foreach (UnityMolResidue r in residues)
		{

			if (r.secondaryStructure == UnityMolResidue.secondaryStructureType.Strand)
			{
				tmp.Add(r);
			}
			else if (tmp.residues.Count() != 0)
			{
				strands.Add(tmp);
				tmp = new Strand();
			}
		}
		if (tmp.residues.Count() != 0)
			strands.Add(tmp);
	}

	/// <summary>
	/// Find all Hydrogen bound that compose the Beta strand
	/// </summary>
	private static Dictionary<UnityMolResidue, List<UnityMolResidue>> FindHydrogenBonds(List<Strand> strands,
	        UnityMolSelection sel, int idFrame)
	{

		List<UnityMolAtom> atoms = new List<UnityMolAtom>();
		Dictionary<UnityMolResidue, List<UnityMolResidue>> Hbonds =
		    new Dictionary<UnityMolResidue, List<UnityMolResidue>>();


		// make select with all the atom from B strand
		foreach (Strand st in strands)
		{
			foreach (UnityMolResidue r in st.residues)
			{
				foreach (UnityMolAtom atom in r.allAtoms)
				{
					atoms.Add(atom);
				}
			}
		}

		// Find all Hydrogen bound in the selected atom
		UnityMolSelection selec = new UnityMolSelection(atoms, "residues", "");
		UnityMolBonds bonds = HbondDetection.DetectHydrogenBonds_Shrodinger(selec, idFrame, sel.atomToIdInSel);

		UnityMolModel curM = atoms[0].residue.chain.model;

		// convert and filled the list of residue link by Hydrogen bound
		foreach(int idA in bonds.bonds.Keys){

			UnityMolAtom a = curM.allAtoms[idA];
			UnityMolResidue r1 = a.residue;

			foreach (int idb in bonds.bonds[idA])
			{
				if (idb != -1)
				{
					UnityMolAtom b = curM.allAtoms[idb];
					UnityMolResidue r2 = b.residue;

					if (r1 != r2)
					{
						List<UnityMolResidue> newlist = null;

						if (Hbonds.TryGetValue(r1, out newlist))
						{
							if (!newlist.Exists(x => x == r2))
								Hbonds[r1].Add(r2);
						}
						else
							Hbonds[r1] = new List<UnityMolResidue>
						{
							r2
						};
					}
				}
			}
		}

		// If Avogardo dosen't works try with Shrodinger
		if (Hbonds.Count() == 0)
		{
			// Debug.Log("Hydrogen Bound not Found, use 2nd function");
			bonds = HbondDetection.DetectHydrogenBonds_Avogadro(selec, idFrame, sel.atomToIdInSel);

			foreach(int idA in bonds.bonds.Keys){

				UnityMolAtom a = curM.allAtoms[idA];
				UnityMolResidue r1 = a.residue;

				foreach (int idb in bonds.bonds[idA])
				{
					if (idb != -1)
					{
						UnityMolAtom b = curM.allAtoms[idb];
						UnityMolResidue r2 = b.residue;

						if (r1 != r2)
						{
							List<UnityMolResidue> newlist = null;

							if (Hbonds.TryGetValue(r1, out newlist))
							{
								if (!newlist.Exists(x => x == r2))
									Hbonds[r1].Add(r2);
							}
							else
								Hbonds[r1] = new List<UnityMolResidue>
							{
								r2
							};
						}
					}
				}
			}
		}

		return Hbonds;
	}

	/// <summary>
	/// Generates the list of Bezier Tiles where each tile is composed of 4 different residues
	/// </summary>
	private static void CreateBezierTiles(List<BezierTile> bezierTiles,
	                                      List<Strand> strands, Dictionary<UnityMolResidue, List<UnityMolResidue>> Hbonds,
	                                      UnityMolSelection sel, int idFrame)
	{
		for (int i = 0; i < strands.Count - 1; i++)
		{
			Strand s1 = strands[i];
			for (int j = i + 1; j < strands.Count; j++)
			{
				Strand s2 = strands[j];

				// Orientation of Strand : TRUE = parallel / FALSE = Anti-Parallel
				// with the dir we can parcour the main strand in 0 to max
				// and the other in max to 0 or 0 to max to parcour the
				// strand in the same "way"
				bool dir = s1.FindOrientation(s2, sel, idFrame);

				UnityMolResidue r1 = s1.residues.First();
				UnityMolResidue r2;

				int mini = dir ? 0 : s2.residues.Count() - 1;
				int pos1 = 0;

				// while strand are not finish
				while (r1 != null && mini < s2.residues.Count() && 0 <= mini)
				{
					// if the residue containe a Hydrogen Bond with an other strand
					// ( maybe not this one )
					if (Hbonds.ContainsKey(r1))
					{
						int pos2 = mini;
						r2 = s2.residues[pos2];

						// search a bound between the actual residiu and the 2nd strand
						while (r2 != null && !Hbonds[r1].Contains(r2))
						{
							if (dir)
							{
								pos2++;
								r2 = pos2 < s2.residues.Count() ? s2.residues[pos2] : null;
							}
							else
							{
								pos2--;
								r2 = 0 <= pos2 ? s2.residues[pos2] : null;
							}
						}

						// Hydrogene bound found
						if (r2 != null)
						{
							bool first = true;
							UnityMolResidue p1;
							UnityMolResidue p2 = r1;
							UnityMolResidue p3 = r2;
							UnityMolResidue p4;

							if (dir)
							{
								pos2++;
								r2 = pos2 < s2.residues.Count() ? s2.residues[pos2] : null;
							}
							else
							{
								pos2--;
								r2 = 0 <= pos2 ? s2.residues[pos2] : null;
							}

							pos1++;
							r1 = pos1 < s1.residues.Count() ? s1.residues[pos1] : null;

							// if a bound is found between both strand, strat to creat Tile
							while (r1 != null && r2 != null
							        && (first || (Hbonds.ContainsKey(r1) && Hbonds[r1].Contains(r2))))
							{

								if (first)
								{
									if (Hbonds.ContainsKey(r1) && Hbonds[r1].Contains(r2))
									{
										p1 = p2;
										p2 = r1;
										p4 = p3;
										p3 = r2;
										bezierTiles.Add(new BezierTile(p1, p2, p3, p4, dir,
										                               sel, idFrame));
									}
									else
										first = false;
								}
								else
								{
									p1 = p2;
									p2 = r1;
									p4 = p3;
									p3 = r2;
									first = true;
									UnityMolResidue p5 = s1.residues[pos1 - 1];
									UnityMolResidue p6 = dir ? s2.residues[pos2 - 1] : s2.residues[pos2 + 1];
									bezierTiles.Add(new BezierTile(p1, p5, p6, p4, dir,
									                               sel, idFrame));
									bezierTiles.Add(new BezierTile(p5, p2, p3, p6, dir,
									                               sel, idFrame));
								}

								if (dir)
								{
									pos2++;
									r2 = pos2 < s2.residues.Count() ? s2.residues[pos2] : null;
								}
								else
								{
									pos2--;
									r2 = 0 <= pos2 ? s2.residues[pos2] : null;
								}

								pos1++;
								r1 = pos1 < s1.residues.Count() ? s1.residues[pos1] : null;

							}

							// update the minimum residue on Strand 2 that can be link
							// in the next research.
							mini = pos2;
						}

						// Hydrogene bound not found, next residue on Strand 1
						else
						{
							pos1++;
							r1 = pos1 < s1.residues.Count() ? s1.residues[pos1] : null;
						}
					}

					// if the residue doesn't contain Hydrogen bound pass to next.
					else
					{
						pos1++;
						r1 = pos1 < s1.residues.Count() ? s1.residues[pos1] : null;
					}
				}
			}
		}
	}

	/// <summary>
	/// Calcul all corner of each tile with the other tile around it and then generate
	/// all the point of the matrix of the tile
	/// </summary>
	private static void MakeSheets( List<BezierTile> tiles)
	{
		for (int i = 0; i < tiles.Count(); i++)
		{
			BezierTile tile_actu = tiles[i];

			for (int j = i + 1; j < tiles.Count(); j++)
			{
				BezierTile tile_search = tiles[j];
				int[,] edge = new int[2, 2];
				Vector3 a_1 = new Vector3(0, 0, 0), a_2 = new Vector3(0, 0, 0);
				int found = 0;
				bool rect = true;
				for (int k = 0; k < 4; k++)
					for (int h = 0; h < 4; h++)
					{
						if (tile_actu.residues[k] == tile_search.residues[h])
						{
							edge[found, 0] = k;
							edge[found, 1] = h;
							found++;
						}
					}

				switch (found)
				{
				case 0:
					break;
				case 1:
					switch (edge[0, 0])
					{
					case 0:
						if (edge[0, 1] == 0 || edge[0, 1] == 2)
							rect = true;
						else
							rect = false;
						break;
					case 1:
						if (edge[0, 1] == 1 || edge[0, 1] == 3)
							rect = true;
						else
							rect = false;
						break;
					case 2:
						if (edge[0, 1] == 0 || edge[0, 1] == 2)
							rect = true;
						else
							rect = false;
						break;
					case 3:
						if (edge[0, 1] == 3 || edge[0, 1] == 1)
							rect = true;
						else
							rect = false;
						break;
					default:
						break;
					}
					if (rect)
					{
						tile_actu.normals[edge[0, 0]] += tile_search.normals_original[edge[0, 1]];
						tile_search.normals[edge[0, 1]] += tile_actu.normals_original[edge[0, 0]];
					}
					else
					{
						tile_actu.normals[edge[0, 0]] -= tile_search.normals_original[edge[0, 1]];
						tile_search.normals[edge[0, 1]] -= tile_actu.normals_original[edge[0, 0]];
					}
					break;

				case 2:

					switch (edge[0, 0])
					{
					case 0:
						if (edge[0, 1] == 0 || edge[0, 1] == 2)
							rect = true;
						else
							rect = false;
						break;
					case 1:
						if (edge[0, 1] == 1 || edge[0, 1] == 3)
							rect = true;
						else
							rect = false;
						break;
					case 2:
						if (edge[0, 1] == 2 || edge[0, 1] == 2)
							rect = true;
						else
							rect = false;
						break;
					case 3:
						if (edge[0, 1] == 3 || edge[0, 1] == 1)
							rect = true;
						else
							rect = false;
						break;
					default:
						break;
					}

					switch (edge[0, 0])
					{
					case 0:
						if (edge[1, 0] == 1)
						{
							if (edge[0, 1] == 1 || edge[0, 1] == 3)
								rect = true;
							else
								rect = false;
						}
						else
						{
							if (edge[0, 1] == 1 || edge[0, 1] == 3)
								rect = true;
							else
								rect = false;
						}
						break;
					case 1:
						if (edge[1, 0] == 2)
						{
							if (edge[0, 1] == 0 || edge[0, 1] == 2)
								rect = true;
							else
								rect = false;
						}
						else
						{
							if (edge[0, 1] == 0 || edge[0, 1] == 2)
								rect = true;
							else
								rect = false;
						}
						break;
					case 2:
						if (edge[1, 0] == 1)
						{
							if (edge[0, 1] == 1 || edge[0, 1] == 3)
								rect = true;
							else
								rect = false;
						}
						else
						{
							if (edge[0, 1] == 1 || edge[0, 1] == 3)
								rect = true;
							else
								rect = false;
						}
						break;
					case 3:
						if (edge[1, 0] == 2)
						{
							if (edge[0, 1] == 0 || edge[0, 1] == 2)
								rect = true;
							else
								rect = false;
						}
						else
						{
							if (edge[0, 1] == 0 || edge[0, 1] == 2)
								rect = true;
							else
								rect = false;
						}
						break;
					default:
						break;
					}
					if (rect)
					{
						tile_actu.normals[edge[0, 0]] += tile_search.normals_original[edge[0, 1]];
						tile_search.normals[edge[0, 1]] += tile_actu.normals_original[edge[0, 0]];

						tile_actu.normals[edge[1, 0]] += tile_search.normals_original[edge[1, 1]];
						tile_search.normals[edge[1, 1]] += tile_actu.normals_original[edge[1, 0]];
					}
					else
					{
						tile_actu.normals[edge[0, 0]] -= tile_search.normals_original[edge[0, 1]];
						tile_search.normals[edge[0, 1]] -= tile_actu.normals_original[edge[0, 0]];

						tile_actu.normals[edge[1, 0]] -= tile_search.normals_original[edge[1, 1]];
						tile_search.normals[edge[1, 1]] -= tile_actu.normals_original[edge[1, 0]];
					}
					break;

				default:
					break;
				}
			}

			for (int k = 0; k < 4; k++)
				tile_actu.normals[k].Normalize();

			tile_actu.Generatfullpoint();
		}
	}

	/// <summary>
	/// Create Mesh with sharp edge without Bezier
	/// </summary>
	private static void TileToMesh(BezierTile tile,  List<Vector3> verticesList,
	                               List<Color32> colorsList,  List<int> trianglesList,
	                               List<Vector2> uv_Map,
	                               Dictionary<UnityMolResidue, List<int>> residueToVert)
	{
		// matrix containe all point of the tile convert with bezier fonction
		Vector3[,] tab_point = new Vector3[resolution_tile + 1, resolution_tile + 1];
		//matrix of bezier surface + normal
		Vector3[,] tab_point_Recto = new Vector3[resolution_tile + 1, resolution_tile + 1];
		//matrix of bezier surface - normal
		Vector3[,] tab_point_Verso = new Vector3[resolution_tile + 1, resolution_tile + 1];

		//matrix that containe all the normal of the surface
		Vector3[,] normal = new Vector3[resolution_tile + 1, resolution_tile + 1];


		//matrix that containe all position of the point of the surfoce head
		int[,] pos_recto = new int[resolution_tile + 1, resolution_tile + 1];

		//matrix that containe all position of the point of the surfoce tail
		int[,] pos_verso = new int[resolution_tile + 1, resolution_tile + 1];

		// Create the straight surface
		tab_point[0, 0] = tile.allpoint[0];
		tab_point[0, resolution_tile] = tile.allpoint[1];
		tab_point[resolution_tile, resolution_tile] = tile.allpoint[2];
		tab_point[resolution_tile, 0] = tile.allpoint[3];

		Vector3 v_edge0 = tab_point[0, resolution_tile] - tab_point[0, 0];
		Vector3 v_edge1 = tab_point[resolution_tile, resolution_tile] - tab_point[0, resolution_tile];
		Vector3 v_edge2 = tab_point[resolution_tile, resolution_tile] - tab_point[resolution_tile, 0];
		Vector3 v_edge3 = tab_point[resolution_tile, 0] - tab_point[0, 0];

		for (int i = 1; i < resolution_tile; i++)
		{
			tab_point[0, i] = tab_point[0, 0] + v_edge0 * i / resolution_tile;
			tab_point[i, resolution_tile] = tab_point[0, resolution_tile] + v_edge1 * i / resolution_tile;
			tab_point[resolution_tile, i] = tab_point[resolution_tile, 0] + v_edge2 * i / resolution_tile;
			tab_point[i, 0] = tab_point[0, 0] + v_edge3 * i / resolution_tile;
		}

		for (int i = 1; i < resolution_tile; i++ )
			for (int j = 1; j < resolution_tile; j++ )
				tab_point[i, j] = tab_point[i, 0] +
				                  (tab_point[i, resolution_tile] - tab_point[i, 0]) * j / resolution_tile;

		//=========== Seperate The two Surface head and tale by the Normal =============//
		for (int i = 0; i < resolution_tile; i++)
			for (int j = 0; j < resolution_tile; j++)
			{
				Vector3 tmp_normal = Vector3.Normalize(Vector3.Cross(tab_point[i, j + 1] - tab_point[i, j],
				                                       tab_point[i + 1, j] - tab_point[i, j]));
				normal[i, j] += tmp_normal;
				normal[i, j + 1] += tmp_normal;
				normal[i + 1, j] += tmp_normal;

				tmp_normal = Vector3.Normalize(Vector3.Cross(tab_point[i + 1, j] - tab_point[i + 1, j + 1],
				                               tab_point[i, j + 1] - tab_point[i + 1, j + 1]));
				normal[i + 1, j + 1] += tmp_normal;
				normal[i, j + 1] += tmp_normal;
				normal[i + 1, j] += tmp_normal;
			}

		for (int i = 0; i <= resolution_tile; i++)
			for (int j = 0; j <= resolution_tile; j++)
			{
				normal[i, j].Normalize();
				tab_point_Recto[i, j] = tab_point[i, j] + normal[i, j] * 0.0005f;
				tab_point_Verso[i, j] = tab_point[i, j] - normal[i, j] * 0.0005f;
			}

		lock (lock_1)
		{
			//Add Vertex and UV and color
			for (int i = 0; i <= resolution_tile; i++)
				for (int j = 0; j <= resolution_tile; j++)
				{

					UnityMolResidue res_tmp = tile.residues[0];
					if (i <= resolution_tile / 2 && j > resolution_tile / 2)
						res_tmp = tile.residues[1];
					else if (i > resolution_tile / 2 && j <= resolution_tile / 2)
						res_tmp = tile.residues[3];
					else if (i > resolution_tile / 2 && j > resolution_tile / 2)
						res_tmp = tile.residues[2];

					if (!residueToVert.ContainsKey(res_tmp)) {
						residueToVert[res_tmp] = new List<int>();
					}
					residueToVert[res_tmp].Add(verticesList.Count);
					residueToVert[res_tmp].Add(verticesList.Count + 1);

					// add Vertex Recto
					pos_recto[i, j] = verticesList.Count;
					verticesList.Add(tab_point_Recto[i, j]);

					// add Vertex Verso
					pos_verso[i, j] = verticesList.Count;
					verticesList.Add(tab_point_Verso[i, j]);

					//Add UV Map
					float uv_i = (float)i / resolution_tile / 2;
					float uv_j = (float)j / resolution_tile;
					Vector2 v = new Vector2(uv_i, uv_j);
					if (!tile.orientation)
						v = new Vector2(uv_i + 0.5f, uv_j);
					uv_Map.Add(v);
					uv_Map.Add(v);




					// Possible to put a switch that add color by residue of the corner
					// here by default white for all
					colorsList.Add(sheetColor);
					colorsList.Add(sheetColor);

				}
		}

		lock (lock_2)
		{
			// Add Triangle
			for (int i = 0; i < resolution_tile; i++)
				for (int j = 0; j < resolution_tile; j++)
				{
					// Add Recto face
					trianglesList.Add(pos_recto[i, j]);
					trianglesList.Add(pos_recto[i, j + 1]);
					trianglesList.Add(pos_recto[i + 1, j]);

					trianglesList.Add(pos_recto[i, j + 1]);
					trianglesList.Add(pos_recto[i + 1, j + 1]);
					trianglesList.Add(pos_recto[i + 1, j]);

					// AddVerso face
					trianglesList.Add(pos_verso[i, j + 1]);
					trianglesList.Add(pos_verso[i, j]);
					trianglesList.Add(pos_verso[i + 1, j]);

					trianglesList.Add(pos_verso[i + 1, j + 1]);
					trianglesList.Add(pos_verso[i, j + 1]);
					trianglesList.Add(pos_verso[i + 1, j]);
				}
		}
	}

	/// <summary>
	/// Create Mesh with smooth edge with Bezier
	/// </summary>
	private static void TileToMesh(BezierTile tile, List<Vector3> verticesList, List<Color32> colorsList,
	                               List<int> trianglesList, List<Vector2> uv_Map,
	                               List<Vector3> normals, Dictionary<UnityMolResidue, Dictionary<UnityMolResidue, Sav_Vert>> vertices_Sav,
	                               Dictionary<UnityMolResidue, List<float>[]> corner,
	                               Dictionary<UnityMolResidue, List<int>> residueToVert)
	{
		//=========== Variable for the Mesh =============//
		// matrix containe all point of the tile convert with bezier fonction
		Vector3[,] tab_point = new Vector3[resolution_tile + 1, resolution_tile + 1];
		//matrix of bezier surface + normal
		Vector3[,] tab_point_Recto = new Vector3[resolution_tile + 1, resolution_tile + 1];
		//matrix of bezier surface - normal
		Vector3[,] tab_point_Verso = new Vector3[resolution_tile + 1, resolution_tile + 1];
		Vector3[,] normal = new Vector3[resolution_tile + 1, resolution_tile + 1];

		//matrix that containe all position of the point of the surfoce head
		int[,] pos_recto = new int[resolution_tile + 1, resolution_tile + 1];
		//matrix that containe all position of the point of the surfoce tail
		int[,] pos_verso = new int[resolution_tile + 1, resolution_tile + 1];

		//=========== Variable for the custom Normal =============//
		Sav_Vert edge_0, edge_0_r, edge_1, edge_1_r, edge_2, edge_2_r, edge_3, edge_3_r;
		Dictionary<UnityMolResidue, Sav_Vert> tmp;
		int[] edge_0_recto = { 0 }, edge_1_recto = { 0 }, edge_2_recto = { 0 }, edge_3_recto = { 0 };
		int[] edge_0_verso = { 0 }, edge_1_verso = { 0 }, edge_2_verso = { 0 }, edge_3_verso = { 0 };
		// bool that used to tell if an edge is allready calculate
		bool[] edge_calculate = { false, false, false, false };
		// Variable for corner
		List<float>[] corner_0, corner_1, corner_2, corner_3;
		int[,] dir = new int[4, 2];
		int count;

		//=========== Calcul of the Bezier Surface =============//
		for (int i = 0; i <= resolution_tile; i++)
			for (int j = 0; j <= resolution_tile; j++)
				tab_point[i, j] = PointBezier(tile.matrix, ((float)i / (float)resolution_tile),
				                              ((float)j / (float)resolution_tile));

		//=========== Seperate The two Surface head and tale by the Normal =============//
		for (int i = 0; i < resolution_tile; i++)
			for (int j = 0; j < resolution_tile; j++)
			{
				Vector3 tmp_normal = Vector3.Normalize(Vector3.Cross(tab_point[i, j + 1] - tab_point[i, j],
				                                       tab_point[i + 1, j] - tab_point[i, j]));
				normal[i, j] += tmp_normal;
				normal[i, j + 1] += tmp_normal;
				normal[i + 1, j] += tmp_normal;

				tmp_normal = Vector3.Normalize(Vector3.Cross(tab_point[i + 1, j] - tab_point[i + 1, j + 1],
				                               tab_point[i, j + 1] - tab_point[i + 1, j + 1]));
				normal[i + 1, j + 1] += tmp_normal;
				normal[i, j + 1] += tmp_normal;
				normal[i + 1, j] += tmp_normal;
			}

		for (int i = 0; i <= resolution_tile; i++)
			for (int j = 0; j <= resolution_tile; j++)
			{
				normal[i, j].Normalize();
				tab_point_Recto[i, j] = tab_point[i, j] + normal[i, j] * 0.0005f;
				tab_point_Verso[i, j] = tab_point[i, j] - normal[i, j] * 0.0005f;
			}

		//=========== Generate and Add : Vertice/Normal/UV/Color =============//
		lock (lock_1)
		{
			Vector3 tile_normal;
			//=========== Search if the edge allready Exist =============//
			// Edge 0 with corner 0 to 1
			tile_normal = Vector3.Normalize(normal[0, 0] + normal[0, resolution_tile]);
			if (vertices_Sav.TryGetValue(tile.residues[0], out tmp))
			{
				if (tmp.TryGetValue(tile.residues[1], out edge_0))
				{
					edge_calculate[0] = true;
					if (edge_0.GetDir(tile_normal))
					{
						edge_0_recto = edge_0.edge_recto;
						edge_0_verso = edge_0.edge_verso;
					}
					else
					{
						edge_0_recto = edge_0.edge_verso;
						edge_0_verso = edge_0.edge_recto;
					}
				}
				else
				{
					tmp[tile.residues[1]] = new Sav_Vert(tile_normal);

					if (vertices_Sav.TryGetValue(tile.residues[1], out tmp))
						tmp[tile.residues[0]] = new Sav_Vert(tile_normal);
					else
					{
						vertices_Sav[tile.residues[1]] = new Dictionary<UnityMolResidue, Sav_Vert>();
						vertices_Sav[tile.residues[1]][tile.residues[0]] = new Sav_Vert(tile_normal);
					}
				}
			}
			else
			{
				vertices_Sav[tile.residues[0]] = new Dictionary<UnityMolResidue, Sav_Vert>();
				vertices_Sav[tile.residues[0]][tile.residues[1]] = new Sav_Vert(tile_normal);

				if (vertices_Sav.TryGetValue(tile.residues[1], out tmp))
					tmp[tile.residues[0]] = new Sav_Vert(tile_normal);
				else
				{
					vertices_Sav[tile.residues[1]] = new Dictionary<UnityMolResidue, Sav_Vert>();
					vertices_Sav[tile.residues[1]][tile.residues[0]] = new Sav_Vert(tile_normal);
				}
			}
			edge_0 = vertices_Sav[tile.residues[0]][tile.residues[1]];
			edge_0_r = vertices_Sav[tile.residues[1]][tile.residues[0]];

			// Edge 3 with corner 0 to 3
			tile_normal = Vector3.Normalize(normal[0, 0] + normal[resolution_tile, 0]);
			tmp = vertices_Sav[tile.residues[0]];
			if (tmp.TryGetValue(tile.residues[3], out edge_3))
			{
				edge_calculate[3] = true;
				if (edge_3.GetDir(tile_normal))
				{
					edge_3_recto = edge_3.edge_recto;
					edge_3_verso = edge_3.edge_verso;
				}
				else
				{
					edge_3_recto = edge_3.edge_verso;
					edge_3_verso = edge_3.edge_recto;
				}
			}
			else
			{
				tmp[tile.residues[3]] = new Sav_Vert(tile_normal);
				edge_3 = tmp[tile.residues[3]];

				if (vertices_Sav.TryGetValue(tile.residues[3], out tmp))
					tmp[tile.residues[0]] = new Sav_Vert(tile_normal);
				else
				{
					vertices_Sav[tile.residues[3]] = new Dictionary<UnityMolResidue, Sav_Vert>();
					vertices_Sav[tile.residues[3]][tile.residues[0]] = new Sav_Vert(tile_normal);
				}
			}
			edge_3 = vertices_Sav[tile.residues[0]][tile.residues[3]];
			edge_3_r = vertices_Sav[tile.residues[3]][tile.residues[0]];

			// Edge 1 with corner 1 to 2
			tile_normal = Vector3.Normalize(normal[0, resolution_tile] + normal[resolution_tile, resolution_tile]);
			tmp = vertices_Sav[tile.residues[1]];
			if (tmp.TryGetValue(tile.residues[2], out edge_1))
			{
				edge_calculate[1] = true;
				if (edge_1.GetDir(tile_normal))
				{
					edge_1_recto = edge_1.edge_recto;
					edge_1_verso = edge_1.edge_verso;
				}
				else
				{
					edge_1_recto = edge_1.edge_verso;
					edge_1_verso = edge_1.edge_recto;
				}
			}
			else
			{
				tmp[tile.residues[2]] = new Sav_Vert(tile_normal);
				edge_1 = tmp[tile.residues[2]];

				if (vertices_Sav.TryGetValue(tile.residues[2], out tmp))
					tmp[tile.residues[1]] = new Sav_Vert(tile_normal);
				else
				{
					vertices_Sav[tile.residues[2]] = new Dictionary<UnityMolResidue, Sav_Vert>();
					vertices_Sav[tile.residues[2]][tile.residues[1]] = new Sav_Vert(tile_normal);
				}
			}
			edge_1 = vertices_Sav[tile.residues[1]][tile.residues[2]];
			edge_1_r = vertices_Sav[tile.residues[2]][tile.residues[1]];

			// Edge 2 with corner 3 to 2
			tile_normal = Vector3.Normalize(normal[resolution_tile, 0] + normal[resolution_tile, resolution_tile]);
			tmp = vertices_Sav[tile.residues[3]];
			if (tmp.TryGetValue(tile.residues[2], out edge_2))
			{
				edge_calculate[2] = true;
				if (edge_2.GetDir(tile_normal))
				{
					edge_2_recto = edge_2.edge_recto;
					edge_2_verso = edge_2.edge_verso;
				}
				else
				{
					edge_2_recto = edge_2.edge_verso;
					edge_2_verso = edge_2.edge_recto;
				}
			}
			else
			{
				tmp[tile.residues[2]] = new Sav_Vert(tile_normal);
				edge_2 = tmp[tile.residues[2]];

				if (vertices_Sav.TryGetValue(tile.residues[2], out tmp))
					tmp[tile.residues[3]] = new Sav_Vert(tile_normal);
				else
				{
					vertices_Sav[tile.residues[2]] = new Dictionary<UnityMolResidue, Sav_Vert>();
					vertices_Sav[tile.residues[2]][tile.residues[3]] = new Sav_Vert(tile_normal);
				}
			}
			edge_2 = vertices_Sav[tile.residues[3]][tile.residues[2]];
			edge_2_r = vertices_Sav[tile.residues[2]][tile.residues[3]];

			// Corner 0
			if (corner.TryGetValue(tile.residues[0], out corner_0))
			{
				if ( 0 < Vector3.Dot(normals[(int)corner_0[0][0]], normal[0, 0]))
				{
					dir[0, 0] = 0;
					dir[0, 1] = 1;
				}
				else
				{
					dir[0, 0] = 1;
					dir[0, 1] = 0;
				}
			}
			else
			{
				dir[0, 0] = 0;
				dir[0, 1] = 1;
				corner[tile.residues[0]] = new List<float>[2];
				corner[tile.residues[0]][0] = new List<float>();
				corner[tile.residues[0]][1] = new List<float>();
				corner_0 = corner[tile.residues[0]];
			}

			// Corner 1
			if (corner.TryGetValue(tile.residues[1], out corner_1))
			{
				if (0 < Vector3.Dot(normals[(int)corner_1[0][0]], normal[0, resolution_tile]))
				{
					dir[1, 0] = 0;
					dir[1, 1] = 1;
				}
				else
				{
					dir[1, 0] = 1;
					dir[1, 1] = 0;
				}
			}
			else
			{
				dir[1, 0] = 0;
				dir[1, 1] = 1;
				corner[tile.residues[1]] = new List<float>[2];
				corner[tile.residues[1]][0] = new List<float>();
				corner[tile.residues[1]][1] = new List<float>();
				corner_1 = corner[tile.residues[1]];
			}

			// Corner 2
			if (corner.TryGetValue(tile.residues[2], out corner_2))
			{
				if (0 < Vector3.Dot(normals[(int)corner_2[0][0]], normal[resolution_tile, resolution_tile]))
				{
					dir[2, 0] = 0;
					dir[2, 1] = 1;
				}
				else
				{
					dir[2, 0] = 1;
					dir[2, 1] = 0;
				}
			}
			else
			{
				dir[2, 0] = 0;
				dir[2, 1] = 1;
				corner[tile.residues[2]] = new List<float>[2];
				corner[tile.residues[2]][0] = new List<float>();
				corner[tile.residues[2]][1] = new List<float>();
				corner_2 = corner[tile.residues[2]];
			}

			// Corner 3
			if (corner.TryGetValue(tile.residues[3], out corner_3))
			{
				if (0 < Vector3.Dot(normals[(int)corner_3[0][0]], normal[resolution_tile, 0]))
				{
					dir[3, 0] = 0;
					dir[3, 1] = 1;
				}
				else
				{
					dir[3, 0] = 1;
					dir[3, 1] = 0;
				}
			}
			else
			{
				dir[3, 0] = 0;
				dir[3, 1] = 1;
				corner[tile.residues[3]] = new List<float>[2];
				corner[tile.residues[3]][0] = new List<float>();
				corner[tile.residues[3]][1] = new List<float>();
				corner_3 = corner[tile.residues[3]];
			}

			//=========== Add to the Mesh : Vertice/color/UV =============//
			for (int i = 0; i <= resolution_tile; i++)
				for (int j = 0; j <= resolution_tile; j++)
				{
					pos_recto[i, j] = verticesList.Count();
					pos_verso[i, j] = verticesList.Count() + 1;

					// Edge 0 and corner 0 and 1
					if (i == 0)
					{
						//Corner 0
						if (j == 0)
						{
							count = corner_0[dir[0, 0]].Count();
							if (0 < count)
							{
								normal[i, j] += normals[(int)corner_0[dir[0, 0]][0]] * count;
								normal[i, j].Normalize();

								for (int y = 0; y < count; y++)
								{
									normals[(int)corner_0[dir[0, 0]][y]] = normal[i, j];
									normals[(int)corner_0[dir[0, 1]][y]] = -normal[i, j];
								}
							}
							corner_0[dir[0, 0]].Add(pos_recto[i, j]);
							corner_0[dir[0, 1]].Add(pos_verso[i, j]);
						}
						//Corner 1
						if (j == resolution_tile)
						{
							count = corner_1[dir[1, 0]].Count();
							if (0 < count)
							{
								normal[i, j] += normals[(int)corner_1[dir[1, 0]][0]] * count;
								normal[i, j].Normalize();

								for (int y = 0; y < count; y++)
								{
									normals[(int)corner_1[dir[1, 0]][y]] = normal[i, j];
									normals[(int)corner_1[dir[1, 1]][y]] = -normal[i, j];
								}
							}
							corner_1[dir[1, 0]].Add(pos_recto[i, j]);
							corner_1[dir[1, 1]].Add(pos_verso[i, j]);
						}

						if (edge_calculate[0])
						{
							if (j != 0 && j != resolution_tile)
							{
								verticesList[edge_0_recto[j]] += normal[i, j] * 0.0005f;
								verticesList[edge_0_verso[j]] -= normal[i, j] * 0.0005f;

								tab_point_Recto[i, j] += normals[edge_0_recto[j]] * 0.0005f;
								tab_point_Verso[i, j] += normals[edge_0_verso[j]] * 0.0005f;

								normal[i, j] += normals[edge_0_recto[j]];
								normal[i, j].Normalize();

								normals[edge_0_recto[j]] = normal[i, j];
								normals[edge_0_verso[j]] = -normal[i, j];
							}
						}

						else
						{
							edge_0.edge_recto[j] = pos_recto[i, j];
							edge_0.edge_verso[j] = pos_verso[i, j];

							edge_0_r.edge_recto[resolution_tile - j] = pos_recto[i, j];
							edge_0_r.edge_verso[resolution_tile - j] = pos_verso[i, j];
						}
					}

					// Edge 1
					if (j == resolution_tile)
					{
						if (edge_calculate[1])
						{
							if (i != 0 && i != resolution_tile)
							{
								verticesList[edge_1_recto[i]] += normal[i, j] * 0.0005f;
								verticesList[edge_1_verso[i]] -= normal[i, j] * 0.0005f;

								tab_point_Recto[i, j] += normals[edge_1_recto[i]] * 0.0005f;
								tab_point_Verso[i, j] += normals[edge_1_verso[i]] * 0.0005f;

								normal[i, j] += normals[edge_1_recto[i]];
								normal[i, j].Normalize();

								normals[edge_1_recto[i]] = normal[i, j];
								normals[edge_1_verso[i]] = -normal[i, j];
							}
						}
						else
						{
							edge_1.edge_recto[i] = pos_recto[i, j];
							edge_1.edge_verso[i] = pos_verso[i, j];

							edge_1_r.edge_recto[resolution_tile - i] = pos_recto[i, j];
							edge_1_r.edge_verso[resolution_tile - i] = pos_verso[i, j];
						}
					}

					// Edge 2 and corner 2 and 3
					if (i == resolution_tile)
					{
						//Corner 2
						if (j == resolution_tile)
						{
							count = corner_2[dir[2, 0]].Count();
							if (count > 0)
							{
								normal[i, j] += normals[(int)corner_2[dir[2, 0]][0]] * count;
								normal[i, j].Normalize();

								for (int y = 0; y < count; y++)
								{
									normals[(int)corner_2[dir[2, 0]][y]] = normal[i, j];
									normals[(int)corner_2[dir[2, 1]][y]] = -normal[i, j];
								}
							}

							corner_2[dir[2, 0]].Add(pos_recto[i, j]);
							corner_2[dir[2, 1]].Add(pos_verso[i, j]);

						}
						//Corner 3
						if (j == 0)
						{
							count = corner_3[dir[3, 0]].Count();
							if (count > 0)
							{
								normal[i, j] += normals[(int)corner_3[dir[3, 0]][0]] * count;
								normal[i, j].Normalize();

								for (int y = 0; y < count; y++)
								{
									normals[(int)corner_3[dir[3, 0]][y]] = normal[i, j];
									normals[(int)corner_3[dir[3, 1]][y]] = -normal[i, j];
								}
							}
							corner_3[dir[3, 0]].Add(pos_recto[i, j]);
							corner_3[dir[3, 1]].Add(pos_verso[i, j]);
						}

						if (edge_calculate[2])
						{
							if (j != 0 && j != resolution_tile)
							{
								verticesList[edge_2_recto[j]] += normal[i, j] * 0.0005f;
								verticesList[edge_2_verso[j]] -= normal[i, j] * 0.0005f;

								tab_point_Recto[i, j] += normals[edge_2_recto[j]] * 0.0005f;
								tab_point_Verso[i, j] += normals[edge_2_verso[j]] * 0.0005f;

								normal[i, j] += normals[edge_2_recto[j]];
								normal[i, j].Normalize();

								normals[edge_2_recto[j]] = normal[i, j];
								normals[edge_2_verso[j]] = -normal[i, j];
							}
						}
						else
						{
							edge_2.edge_recto[j] = pos_recto[i, j];
							edge_2.edge_verso[j] = pos_verso[i, j];

							edge_2_r.edge_recto[resolution_tile - j] = pos_recto[i, j];
							edge_2_r.edge_verso[resolution_tile - j] = pos_verso[i, j];
						}
					}

					// Edge 3
					if (j == 0)
					{
						if (edge_calculate[3])
						{
							if (i != 0 && i != resolution_tile)
							{
								verticesList[edge_3_recto[i]] += normal[i, j] * 0.0005f;
								verticesList[edge_3_verso[i]] -= normal[i, j] * 0.0005f;

								tab_point_Recto[i, j] += normals[edge_3_recto[i]] * 0.0005f;
								tab_point_Verso[i, j] += normals[edge_3_verso[i]] * 0.0005f;

								normal[i, j] += normals[edge_3_recto[i]];
								normal[i, j].Normalize();

								normals[edge_3_recto[i]] = normal[i, j];
								normals[edge_3_verso[i]] = -normal[i, j];
							}
						}
						else
						{
							edge_3.edge_recto[i] = pos_recto[i, j];
							edge_3.edge_verso[i] = pos_verso[i, j];

							edge_3_r.edge_recto[resolution_tile - i] = pos_recto[i, j];
							edge_3_r.edge_verso[resolution_tile - i] = pos_verso[i, j];
						}
					}

					UnityMolResidue res_tmp = tile.residues[0];
					if (i <= resolution_tile / 2 && j > resolution_tile / 2)
						res_tmp = tile.residues[1];
					else if (i > resolution_tile / 2 && j <= resolution_tile / 2)
						res_tmp = tile.residues[3];
					else if (i > resolution_tile / 2 && j > resolution_tile / 2)
						res_tmp = tile.residues[2];

					if (!residueToVert.ContainsKey(res_tmp)) {
						residueToVert[res_tmp] = new List<int>();
					}
					residueToVert[res_tmp].Add(verticesList.Count);
					residueToVert[res_tmp].Add(verticesList.Count + 1);

					// add Vertex Recto
					verticesList.Add(tab_point_Recto[i, j]);
					normals.Add(normal[i, j]);

					// add Vertex Verso
					verticesList.Add(tab_point_Verso[i, j]);
					normals.Add(-normal[i, j]);

					// UVs calcul
					float uv_i = (float)i / resolution_tile / 2;
					float uv_j = (float)j / resolution_tile;

					Vector2 v = new Vector2(uv_i, uv_j);
					if (!tile.orientation)
						v = new Vector2(uv_i + 0.5f, uv_j);
					uv_Map.Add(v);
					uv_Map.Add(v);


					colorsList.Add(sheetColor);
					colorsList.Add(sheetColor);
				}
		}

		//=========== Add Triangle to the Mesh =============//
		lock (lock_2)
		{
			for (int i = 0; i < resolution_tile; i++)
				for (int j = 0; j < resolution_tile; j++)
				{
					// Add Recto face
					trianglesList.Add(pos_recto[i, j]);
					trianglesList.Add(pos_recto[i, j + 1]);
					trianglesList.Add(pos_recto[i + 1, j]);

					trianglesList.Add(pos_recto[i, j + 1]);
					trianglesList.Add(pos_recto[i + 1, j + 1]);
					trianglesList.Add(pos_recto[i + 1, j]);

					// AddVerso face
					trianglesList.Add(pos_verso[i, j + 1]);
					trianglesList.Add(pos_verso[i, j]);
					trianglesList.Add(pos_verso[i + 1, j]);

					trianglesList.Add(pos_verso[i + 1, j + 1]);
					trianglesList.Add(pos_verso[i, j + 1]);
					trianglesList.Add(pos_verso[i + 1, j]);
				}
		}
	}

	/// <summary>
	/// Calculate the position of the point with the matrix of Bezier
	/// </summary>
	private static Vector3 PointBezier(Vector3[,] P, float u, float v)
	{
		float[,] B = { { -1, 3, -3, 1 }, { 3, -6, 3, 0 }, { -3, 3, 0, 0 }, { 1, 0, 0, 0 } };
		float[] vect_u = { (float)Math.Pow(u, 3), (float)Math.Pow(u, 2), u, 1 };
		float[] vect_v = { (float)Math.Pow(v, 3), (float)Math.Pow(v, 2), v, 1 };
		Vector3[,] vect_B = new Vector3[4, 4];
		Vector3[,] vect_P = new Vector3[4, 4];
		Vector3[] vect_w = new Vector3[4];
		Vector3 point = new Vector3(0, 0, 0);

		// Calcul of Vector B * P
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				vect_B[i, j] = new Vector3(0, 0, 0);

				for (int k = 0; k < 4; k++)
					vect_B[i, j] += B[i, k] * P[k, j];
			}
		}

		// Calcul of Vector (B*P)*B = v_P
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				vect_P[i, j] = new Vector3(0, 0, 0);

				for (int k = 0; k < 4; k++)
					vect_P[i, j] += vect_B[i, k] * B[k, j];
			}
		}

		// Calcul of Vector U * v_P = W
		for (int j = 0; j < 4; j++)
		{
			vect_w[j] = new Vector3(0, 0, 0);

			for (int k = 0; k < 4; k++)
				vect_w[j] += vect_u[k] * vect_P[k, j];
		}

		// Calcul of Vector W * V = point
		for (int k = 0; k < 4; k++)
			point += vect_w[k] * vect_v[k];

		return point;
	}


	/// <summary>
	/// Calss that store all the information of the edge that have been calculate
	/// use to generate the normal of the egde for the smooth sheet
	/// </summary>
	public class Sav_Vert
	{
		/// <summary>
		/// Store the normal of the tile that have calculate the edge
		/// </summary>
		public Vector3 normal;
		/// <summary>
		/// Store the ID used in the vertice list of the recto edge of the mesh.
		/// </summary>
		public int[] edge_recto;
		/// <summary>
		/// Store the ID used in the vertice list of the verso edge of the mesh
		/// </summary>
		public int[] edge_verso;

		/// <summary>
		/// Constructor of the Class
		/// </summary>
		public Sav_Vert()
		{
			normal = new Vector3(0, 0, 0);
			edge_recto = new int[resolution_tile + 1];
			edge_verso = new int[resolution_tile + 1];
		}

		/// <summary>
		/// Constructor of the class, with the normal of the tile, that creat the edge
		/// </summary>
		public Sav_Vert(Vector3 n)
		{
			normal = n;
			edge_recto = new int[resolution_tile + 1];
			edge_verso = new int[resolution_tile + 1];
		}

		/// <summary>
		/// return if the tile that calculate the edge and the actual tile are in the same direction
		/// </summary>
		public bool GetDir(Vector3 n)
		{
			return 0 <= Vector3.Dot(normal, n);
		}

		/// <summary>
		/// return the ID of the point of the recto edge
		/// </summary>
		public int GetRecto(int i)
		{
			if (0 <= i && i < resolution_tile + 1)
				return edge_recto[i];
			else
				return -1;
		}

		/// <summary>
		/// return the id of the point of the verso edge
		/// </summary>
		public int GetVerso(int i)
		{
			if (0 <= i && i < resolution_tile + 1)
				return edge_verso[i];
			else
				return -1;
		}
	}

	/// <summary>
	/// Class that use to give parameter of the function in the thread.
	/// </summary>
	public class ThreadWork
	{
		/// <summary>
		/// The tile that will be calculate
		/// </summary>
		public BezierTile tile;
		/// <summary>
		/// the vertice list of the mesh
		/// </summary>
		public List<Vector3> verticesList;

		public Dictionary<UnityMolResidue, List<int>> residueToVert;
		/// <summary>
		/// the list of the color uni
		/// </summary>
		public List<Color32> colorsList;
		/// <summary>
		/// list of the triangles of the mesh
		/// </summary>
		public List<int> trianglesList;
		/// <summary>
		/// the UV's list
		/// </summary>
		public List<Vector2> uv_Map;
		/// <summary>

		/// <summary>
		/// the normal of the mesh
		/// </summary>
		public List<Vector3> normals;
		/// <summary>
		/// Use to save the edge of each tile to calculate the normal of it
		/// </summary>
		public Dictionary<UnityMolResidue, Dictionary<UnityMolResidue, Sav_Vert>> vertices_Sav;
		/// <summary>
		/// use to save the corner and calculate the normal of it
		/// </summary>
		public Dictionary<UnityMolResidue, List<float>[]> corner;

		/// <summary>
		/// Constructor of the thread for the smooth version
		/// </summary>
		public ThreadWork(BezierTile tile, List<Vector3> verticesList, List<Color32> colorsList,
		                  List<int> trianglesList, List<Vector2> uv_Map,
		                  List<Vector3> normals, Dictionary<UnityMolResidue, Dictionary<UnityMolResidue, Sav_Vert>> vertices_Sav,
		                  Dictionary<UnityMolResidue, List<int>> resToV,
		                  Dictionary<UnityMolResidue, List<float>[]> corner)
		{
			this.tile = tile;
			this.verticesList = verticesList;
			this.residueToVert = resToV;
			this.colorsList = colorsList;
			this.trianglesList = trianglesList;
			this.uv_Map = uv_Map;
			this.normals = normals;
			this.vertices_Sav = vertices_Sav;
			this.corner = corner;
		}

		/// <summary>
		/// Constructor of the thread for the straight version
		/// </summary>
		public ThreadWork(BezierTile tile, List<Vector3> verticesList, List<Color32> colorsList,
		                  Dictionary<UnityMolResidue, List<int>> resToV,
		                  List<int> trianglesList, List<Vector2> uv_Map)
		{
			this.tile = tile;
			this.verticesList = verticesList;
			this.residueToVert = resToV;
			this.colorsList = colorsList;
			this.trianglesList = trianglesList;
			this.uv_Map = uv_Map;
		}

		/// <summary>
		/// </summary>
		public void TileToMesh()
		{
			Sheherasade.TileToMesh(tile, verticesList, colorsList, trianglesList, uv_Map, normals, vertices_Sav, corner, residueToVert);
		}

		/// <summary>
		/// </summary>
		public void TileToMesh_2()
		{
			Sheherasade.TileToMesh(tile, verticesList, colorsList, trianglesList, uv_Map, residueToVert);
		}
	}
}

/// <summary>
/// class representing a beta strand
/// </summary>
public class Strand
{
	/// <summary>
	/// list of residues composing the strand
	/// </summary>
	public List<UnityMolResidue> residues;

	/// <summary>
	/// construct a Strand with a list of UnityMolResidue
	/// </summary>
	public Strand(List<UnityMolResidue> rList = null)
	{
		if (rList != null)
		{
			residues = rList;
		}
		else
		{
			residues = new List<UnityMolResidue>();
		}
	}

	/// <summary>
	/// Adds a new residue to the list of residues
	/// </summary>
	public void Add(UnityMolResidue r)
	{
		residues.Add(r);
	}

	/// <summary>
	/// Find the orientation between two beta-strands by testing the direction
	/// of the vetor of the strand and compare it
	/// to find if they are in the same direction TRUE parallel , FALSE anti-parallel
	/// </summary>
	public bool FindOrientation(Strand s2, UnityMolSelection sel, int idFrame)
	{
		Vector3 dirS1 = residues.Last().atoms["CA"].position
		                - residues.First().atoms["CA"].position;
		Vector3 dirS2 = s2.residues.Last().atoms["CA"].position
		                - s2.residues.First().atoms["CA"].position;

		if (idFrame != -1) {
			int idA = sel.atomToIdInSel[residues.Last().atoms["CA"]];
			int idB = sel.atomToIdInSel[residues.First().atoms["CA"]];
			int idC = sel.atomToIdInSel[s2.residues.Last().atoms["CA"]];
			int idD = sel.atomToIdInSel[s2.residues.First().atoms["CA"]];


			dirS1 = sel.extractTrajFramePositions[idFrame][idA] - sel.extractTrajFramePositions[idFrame][idB];
			dirS2 = sel.extractTrajFramePositions[idFrame][idC] - sel.extractTrajFramePositions[idFrame][idD];
		}

		if (0 <= Vector3.Dot(dirS1, dirS2))
			return true;
		return false;
	}
}

/// <summary>
/// class gathering the datas used for generating the Bezier tiles
/// </summary>
public class BezierTile
{
	/// <summary>
	/// List of the 4 corner of the tiles and normal
	/// </summary>
	public UnityMolResidue[] residues;

	/// <summary>
	/// Normal after unification of the adjacent tile
	/// </summary>
	public Vector3[] normals;

	/// <summary>
	/// original normal of the tile
	/// </summary>
	public Vector3[] normals_original;

	/// <summary>
	/// orientation of the tile true for paralell and false for anti-paralell
	/// </summary>
	public bool orientation;

	/// <summary>
	/// the 16 point of the matrix use to make the matrix of bezier
	///  00 - 04 - 08 - 01
	///  11 - 12 - 14 - 05
	///  07 - 15 - 13 - 09
	///  03 - 10 - 06 - 02
	/// </summary>
	public Vector3[] allpoint;

	/// <summary>
	/// the 16 normal of the matrix use to make the other point of bezier's matrix
	///  00 - 04 - 08 - 01
	///  11 - 12 - 14 - 05
	///  07 - 15 - 13 - 09
	///  03 - 10 - 06 - 02
	/// </summary>
	public Vector3[] allnormals;

	/// <summary>
	/// well aranged matrix of the tile
	/// </summary>
	public Vector3[,] matrix;

	/// <summary>
	/// construct the bezier tile with the 4 residues
	///  needed for the tile. It should be constructed as below:
	///  - - -res1 - res2- - -
	///        |	    |
	///  - - -res4 - res3- - -
	/// 4 residu in order and a bool for the orientation parallel/ Anti-parallel
	/// </summary>
	public BezierTile(UnityMolResidue res1, UnityMolResidue res2,
	                  UnityMolResidue res3, UnityMolResidue res4, bool ori,
	                  UnityMolSelection sel, int idFrame)
	{
		orientation = ori;

		allpoint = new Vector3[16];
		allnormals = new Vector3[16];

		residues = new UnityMolResidue[4];
		normals = new Vector3[4];
		normals_original = new Vector3[4];

		residues[0] = res1;
		allpoint[0] = res1.atoms["CA"].position;
		residues[1] = res2;
		allpoint[1] = res2.atoms["CA"].position;
		residues[2] = res3;
		allpoint[2] = res3.atoms["CA"].position;
		residues[3] = res4;
		allpoint[3] = res4.atoms["CA"].position;

		if (idFrame != -1) {
			allpoint[0] = sel.extractTrajFramePositions[idFrame][sel.atomToIdInSel[res1.atoms["CA"]]];
			allpoint[1] = sel.extractTrajFramePositions[idFrame][sel.atomToIdInSel[res2.atoms["CA"]]];
			allpoint[2] = sel.extractTrajFramePositions[idFrame][sel.atomToIdInSel[res3.atoms["CA"]]];
			allpoint[3] = sel.extractTrajFramePositions[idFrame][sel.atomToIdInSel[res4.atoms["CA"]]];
		}

		GenerateNormal();
	}

	/// <summary>
	/// Generate normal of the tile's corner
	/// </summary>
	public void GenerateNormal()
	{
		//Normal of Corner of the tile
		for (int i = 0; i < 4; i++)
		{
			Vector3 s1 = allpoint[i];
			Vector3 s2 = allpoint[(i + 1) % 4];
			Vector3 s3 = allpoint[(i + 2) % 4];

			normals[i] = Vector3.Cross(s2 - s1, s3 - s1);

			s3 = allpoint[(i + 3) % 4];

			normals[i] += Vector3.Cross(s2 - s1, s3 - s1);

			s2 = allpoint[(i + 2) % 4];

			normals[i] += Vector3.Cross(s2 - s1, s3 - s1);

			normals[i].Normalize();
			normals_original[i] = normals[i];
		}
	}

	/// <summary>
	/// Generate all the point of the matrix for bezier
	/// </summary>
	public void Generatfullpoint()
	{
		Vector3 vect;
		Vector3 tmp;
		float dist;
		int pos = 12;
		for (int i = 0; i < 4; i++)
			allnormals[i] = normals[i];

		for (int i = 0; i < 4; i++)
		{
			tmp = (allpoint[(i + 1) % 4] - allpoint[i]);
			vect = Vector3.Cross(Vector3.Cross(allnormals[i], tmp), allnormals[i]);
			vect.Normalize();
			dist = Vector3.Distance(allpoint[(i + 1) % 4], allpoint[i]) * 1 / 3;
			allpoint[4 + i] = allpoint[i] + (vect * dist);
			allnormals[4 + i] = allnormals[i] * 2 + allnormals[(i + 1) % 4];
			allnormals[4 + i].Normalize();
		}

		for (int i = 0; i < 4; i++)
		{
			tmp = (allpoint[i] - allpoint[(i + 1) % 4]);
			vect = Vector3.Cross(Vector3.Cross(allnormals[(i + 1) % 4], tmp), allnormals[(i + 1) % 4]);
			vect.Normalize();
			dist = Vector3.Distance(allpoint[(i + 1) % 4], allpoint[i]) * 1 / 3;
			allpoint[8 + i] = allpoint[(i + 1) % 4] + (vect * dist);
			allnormals[8 + i] = allnormals[(i + 1) % 4] * 2 + allnormals[i];
			allnormals[8 + i].Normalize();
		}

		for (int i = 4; i < 12; i += 2)
		{
			tmp = (allpoint[14 - i] - allpoint[i]);
			vect = Vector3.Cross(Vector3.Cross(allnormals[i], tmp), allnormals[i]);
			vect.Normalize();
			dist = Vector3.Distance(allpoint[14 - i], allpoint[i]) * 1 / 3;
			allpoint[pos] = allpoint[i] + (vect * dist);
			allnormals[pos] = allnormals[i] * 2 + allnormals[14 - i];
			allnormals[pos].Normalize();
			pos++;
		}

		matrix = new Vector3[4, 4];
		matrix[0, 0] = allpoint[0];
		matrix[0, 1] = allpoint[4];
		matrix[0, 2] = allpoint[8];
		matrix[0, 3] = allpoint[1];
		matrix[1, 0] = allpoint[11];
		matrix[1, 1] = allpoint[12];
		matrix[1, 2] = allpoint[14];
		matrix[1, 3] = allpoint[5];
		matrix[2, 0] = allpoint[7];
		matrix[2, 1] = allpoint[15];
		matrix[2, 2] = allpoint[13];
		matrix[2, 3] = allpoint[9];
		matrix[3, 0] = allpoint[3];
		matrix[3, 1] = allpoint[10];
		matrix[3, 2] = allpoint[6];
		matrix[3, 3] = allpoint[2];
	}

	/// <summary>
	/// Give the string representation of the tile
	/// </summary>
	override public String ToString()
	{
		String str = residues[0].ToString() + " || " + residues[1].ToString() + " || "
		             + residues[2].ToString() + " || " + residues[3].ToString() + " || ";
		return str;
	}


}

}
