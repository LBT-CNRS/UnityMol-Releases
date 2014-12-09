/// @file ReadJson.cs
/// @brief Details to be specified
/// @author FvNano/LBT team
/// @author Marc Baaden <baaden@smplinux.de>
/// @date   2013-4
///
/// Copyright Centre National de la Recherche Scientifique (CNRS)
///
/// contributors :
/// FvNano/LBT team, 2010-13
/// Marc Baaden, 2010-13
///
/// baaden@smplinux.de
/// http://www.baaden.ibpc.fr
///
/// This software is a computer program based on the Unity3D game engine.
/// It is part of UnityMol, a general framework whose purpose is to provide
/// a prototype for developing molecular graphics and scientific
/// visualisation applications. More details about UnityMol are provided at
/// the following URL: "http://unitymol.sourceforge.net". Parts of this
/// source code are heavily inspired from the advice provided on the Unity3D
/// forums and the Internet.
///
/// This software is governed by the CeCILL-C license under French law and
/// abiding by the rules of distribution of free software. You can use,
/// modify and/or redistribute the software under the terms of the CeCILL-C
/// license as circulated by CEA, CNRS and INRIA at the following URL:
/// "http://www.cecill.info".
/// 
/// As a counterpart to the access to the source code and rights to copy, 
/// modify and redistribute granted by the license, users are provided only 
/// with a limited warranty and the software's author, the holder of the 
/// economic rights, and the successive licensors have only limited 
/// liability.
///
/// In this respect, the user's attention is drawn to the risks associated 
/// with loading, using, modifying and/or developing or reproducing the 
/// software by the user in light of its specific status of free software, 
/// that may mean that it is complicated to manipulate, and that also 
/// therefore means that it is reserved for developers and experienced 
/// professionals having in-depth computer knowledge. Users are therefore 
/// encouraged to load and test the software's suitability as regards their 
/// requirements in conditions enabling the security of their systems and/or 
/// data to be ensured and, more generally, to use and operate it in the 
/// same conditions as regards security.
///
/// The fact that you are presently reading this means that you have had 
/// knowledge of the CeCILL-C license and that you accept its terms.
///
/// $Id: ReadJson.cs 266 2013-05-06 10:41:42Z kouyoumdjian $
///
/// References : 
/// If you use this code, please cite the following reference : 	
/// Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
/// "Game on, Science - how video game technology may help biologists tackle
/// visualization challenges" (2013), PLoS ONE 8(3):e57990.
/// doi:10.1371/journal.pone.0057990
///
/// If you use the HyperBalls visualization metaphor, please also cite the
/// following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
/// B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
/// using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
/// J. Comput. Chem., 2011, 32, 2924
///

namespace  ParseData.ParsePDB
{
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ReadJson 
{
	
	private List<List<Vector3>> m_fieldlines;
	
	public List<List<Vector3>> GetFieldLines()
	{
		return m_fieldlines;
	}
			
			
	public List<List<Vector3>> ReadFile(string fieldlines_file_content, Vector3 Offset) 
	{
		List<List<Vector3>> linelist=new List<List<Vector3>>();
		StringReader sr = new StringReader(fieldlines_file_content);
		string line = null;
		line = sr.ReadLine();
		string[] tok = line.Split(' ');

		if(tok[0] == "n")
		{
			// We have an .apf file (animated potential fieldlines)
			List<Vector3> particlelist = new List<Vector3>();
			while(line != null)
			{
				tok = line.Split(' ');
				if(tok[0] == "n" && particlelist.Count > 0)
				{
					linelist.Add(particlelist);
					particlelist = new List<Vector3>();
				}
				if(tok[0] == "v")
				{
					//Unity has a left-handed coordinates system while PDBs are right-handed
					//So we have to inverse the X coordinates
					Vector3 values = new Vector3(-float.Parse(tok[1]) + Offset.x,
												  float.Parse(tok[2]) + Offset.y,
												  float.Parse(tok[3]) + Offset.z
												);
					particlelist.Add(values);
				}
				line = sr.ReadLine();
			}
			if(particlelist.Count > 0)
				linelist.Add(particlelist);
			else
				Debug.Log("Reading Fieldlines apf ERROR");
		}
		else
		{
			// We have a JSON file
			object value=(object)MiniJSON.JsonDecode(fieldlines_file_content);
			
			Debug.Log(((Hashtable)value)["lines"]);
			ArrayList FieldLinesarray=(ArrayList)(((Hashtable)value)["lines"]);
			
	//		Hashtable FieldLines = (Hashtable)value;
			
			Debug.Log(FieldLinesarray.Count);

			for (int i=0; i<FieldLinesarray.Count; i++) 
			{
				List<Vector3> particlelist=new List<Vector3>();
				for(int j=0;j<((ArrayList)FieldLinesarray[i]).Count;j+=3)
				{
	//				Debug.Log("FieldLine: " +((ArrayList)FieldLinesarray[i])[j]+", "+((ArrayList)FieldLinesarray[i])[j+1]+", "+((ArrayList)FieldLinesarray[i])[j+2]);

					//Unity has a left-handed coordinates system while PDBs are right-handed
					//So we have to inverse the X coordinates
					double x=-(double)(((ArrayList)FieldLinesarray[i])[j]);
					double y=(double)(((ArrayList)FieldLinesarray[i])[j+1]);
					double z=(double)(((ArrayList)FieldLinesarray[i])[j+2]);
					particlelist.Add(new Vector3(float.Parse(System.Convert.ToString(x))+Offset.x,float.Parse(System.Convert.ToString(y))+Offset.y,float.Parse(System.Convert.ToString(z))+Offset.z));

	//				Debug.Log("FieldLine: " +((Vector3)particlelist[particlelist.Count-1]).z);

	//				Debug.Log("FieldLine: " +((ArrayList)FieldLinesarray[i])[j]);
				}
				linelist.Add(particlelist);
			}
		}

		m_fieldlines = linelist;
		return GetFieldLines();
		
	}
}
}
