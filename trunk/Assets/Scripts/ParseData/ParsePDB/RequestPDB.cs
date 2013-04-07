/// @file RequestPDB.cs
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
/// $Id: RequestPDB.cs 228 2013-04-07 16:20:01Z baaden $
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
	using System.IO;
	using ParseData.IParsePDB;
	using System.Net;
	using System.Linq;
	using System;
	using Molecule.Model;
	using Molecule.Control;
	using System.Xml;
	using System.Text;	
	using System.Text.RegularExpressions;
	using UI;
	
	public class RequestPDB
	{
//		private string url="http://www.pdb.org/pdb/downLoad/downLoadFile.do?fileFormat=pdb&compression=NO&structureId=3KX3";
//		private string data="";
		public float progress=0.0f;
		public bool isDone=false;
		
    	public bool Loading = true;
		//public GUISkin mySkin;
		  
			// choice open file
//		bool wantpdb=true;
//		bool wantxgmml=false;
		bool wantobj=false;
		bool wantdx=false;
		bool wantjson =false;
		    
		void Start()
		{
			//mySkin?Resources.Load("MyGUISkin") as GUISkin;
		}
		
		public RequestPDB()
		{
		}
		
		
	
		void Update()
		{
			
			if (isDone)
            {
                Loading = false;
                           }
            else
            {
                Loading = true;
            }

			//progress=;
		}
		
		void OnGUI()
    	{
			//start
       		//if (Loading)
        	//{

            //	GUI.Label(new Rect(100, 15, 200, 30), "", "bj");
            //	GUI.Label(new Rect(100,15, progress * 200, 30), "", "qj");
        	//}
    	}

		//Move this method to the related class
		public IEnumerator LoadJsonWWW(string file_name, Vector3 Offset)
		{
			WWW www = new WWW(file_name);
			
			progress = 0;
			isDone = false;
			while(!www.isDone)
			{	
//				Debug.Log("*** PDB: "+www.progress);
				progress = www.progress;
				yield return new WaitForEndOfFrame();
			}
			
			ReadJson readjson=new ReadJson();
			MoleculeModel.FieldLineList = readjson.ReadFile(www.text,Offset);
			isDone = true;
		}

		public void LoadJsonRequest(string file_name, Vector3 Offset)
		{
			WWW www = new WWW(file_name);
			
			progress = 0;
			isDone = false;
			while(!www.isDone)
			{	
//				Debug.Log("*** PDB: "+www.progress);
				progress = www.progress;
			}
			
			ReadJson readjson=new ReadJson();
			MoleculeModel.FieldLineList = readjson.ReadFile(www.text,Offset);
			isDone = true;
		} 
		
		//Move this method to the related class
		public void LoadDxRequest(string file_name, Vector3 Offset)
		{  // function Loading DX file
		
//			ReadDX readdx = new ReadDX();
//			Debug.Log("time avant: "+ DateTime.Now);
//			DateTime temp = DateTime.Now;
//			readdx.getDX(file_name);
//			Debug.Log("time : apres"+ (DateTime.Now-temp));
//			temp = DateTime.Now;
//
////			readdx.calGradient();
////			Debug.Log("time gradient: "+ (DateTime.Now-temp));
//			
//			// Calcul et affichage des ligne de champs7
////			Loader.CalcFieldline(readdx.X,readdx.Y,readdx.Z,readdx.GetDelta(),readdx.GetOrigin());
//			
//			
////			 lancement iso surface
//			readdx.isoSurface(10f);
//			Debug.Log("time : surface "+ (DateTime.Now-temp));
			// lancement de la transformation du pdb en density
//			temp = DateTime.Now;

		}

		//TODO put molecule offset here
		public IEnumerator LoadOBJWWW(string file_name)
		{
			WWW www = new WWW(file_name);
			
			progress = 0;
			isDone = false;
			while(!www.isDone)
			{	
//				Debug.Log("*** PDB: "+www.progress);
				progress = www.progress;
				yield return new WaitForEndOfFrame();
			}
			
			OBJ obj = new OBJ(new StringReader(www.text));
			obj.Load();
			isDone = true;
		}
		
		//TODO put molecule offset here
		public void LoadOBJRequest(string file_base_name){
			
			Debug.Log("name:"+file_base_name+".obj");
			
						
			string path = file_base_name+".obj";
//			path = "file:/"+path;
			Debug.Log("file complet :"+ path);
//			string path = "http://www.everyday3d/unity3d/obj/monkey.obj";
			
			
			FileInfo file=new FileInfo(file_base_name+".obj");

			if ( file.Exists){
	//			ReadOBJ readObj =new ReadOBJ();
	//			readObj.GetOBJ(url,id);
	//			OBJ obj = Camera.main.GetComponent<OBJ>(path);
//				var go = new GameObject("Your Script Container");
//				var obj = go.AddComponent<OBJ>();
				// THIS PRODUCES A WARNING:: You are trying to create a MonoBehaviour using the 'new' keyword.  This is not allowed.  MonoBehaviours can only be added using AddComponent().  Alternatively, your script can inherit from ScriptableObject or no base class at all
				OBJ obj = new OBJ(path);
				Debug.Log("Generate new OBJ from "+path);
				obj.Load();
	//			Debug.Log("Coroutine Started");$
			} 
			else {
				for (int i =0;i< 6;i++){
					file=new FileInfo(file_base_name+i+".obj");
					if (file.Exists){
						path = file_base_name+i+".obj";
						OBJ obj = new OBJ(path);
						Debug.Log("new OBJ");
						obj.Load();
					}
				}
			}
		}
		
		public  void LoadXMLRequest(string file_name)
		{

			XmlReader reader=new XmlTextReader(file_name);
	
//			int i=0;
			while(reader.Read())
			{
				if(reader.ReadToFollowing("PDBx:atom_site"))
				{
					Vector3 v=new Vector3();
				
					reader.ReadToDescendant("PDBx:Cartn_x");
					v.x=reader.ReadElementContentAsFloat();
					reader.ReadToFollowing("PDBx:Cartn_y");
					v.y=reader.ReadElementContentAsFloat();
					reader.ReadToFollowing("PDBx:Cartn_z");
					v.z=reader.ReadElementContentAsFloat();
					
	//				print(i%150);
	//				print(i/150);
	//				location1[i%150,i/150]=v;
	//				i++;
					
				}
			}	
		
		}
	
		public IEnumerator LoadPDBWWW(string file_name)
		{
			WWW www = new WWW(file_name);
			
			progress = 0;
			isDone = false;
			while(!www.isDone)
			{	
//				Debug.Log("*** PDB: "+www.progress);
				progress = www.progress;
				yield return new WaitForEndOfFrame();
			}
			Debug.Log("read");
			ReadPDB(new StringReader(www.text));
			isDone = true;
		}

		public void LoadPDBResource(string resource_name)
		{
			TextAsset text_data = Resources.Load(resource_name) as TextAsset;
			StringReader sr = new StringReader(text_data.text);

			ReadPDB(sr);
		}

		public void FetchPDB(string url,string id, string proxyserver = "", int proxyport = 0)
		{
			StreamReader sr ;
			Stream dataStream=null;
			HttpWebResponse response=null;

			HttpWebRequest request =(HttpWebRequest) WebRequest.Create (url+id+".pdb");
            
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;

            //Set proxy information if needed
            Debug.Log("LoadPDB: " + proxyserver + " " + proxyport);
            if(proxyserver != "")
            	request.Proxy = new WebProxy(proxyserver,proxyport);

            // Get the response.
            response = (HttpWebResponse)request.GetResponse ();
            // Display the status.
			Debug.Log("LoadPDB Status :: " + response.StatusDescription);
            
            // Get the stream containing content returned by the server.
     	    dataStream = response.GetResponseStream ();
            // Open the stream using a StreamReader for easy access.
            sr = new StreamReader (dataStream);

            ReadPDB(sr);

			if(dataStream!=null&& response!=null)
			{
				dataStream.Close ();
       			response.Close ();
			}
		}

	
		public void LoadPDBRequest(string file_base_name, bool withData = true)
		{
			StreamReader sr ;
				
//			FileInfo file=new FileInfo(file_base_name+".pdb");
			sr=new StreamReader(file_base_name+".pdb");
			
			ReadPDB(sr);

			if(withData)
			{
				FileInfo fieldlinefile=new FileInfo(file_base_name+".json");
				FileInfo apffile=new FileInfo(file_base_name+".apf");
				if(fieldlinefile.Exists)
	//			if(wantjson)
				{
					LoadJsonRequest("file://"+file_base_name+".json",MoleculeModel.Offset);
					MoleculeModel.FieldLineFileExist=true;
				}
				else if(apffile.Exists)
				{
					LoadJsonRequest("file://"+file_base_name+".apf",MoleculeModel.Offset);
					MoleculeModel.FieldLineFileExist=true;
				}
				else
				{
					MoleculeModel.FieldLineFileExist=false;
					MoleculeModel.FieldLineList=null;
				}
					
				FileInfo Surfacefile=new FileInfo(file_base_name+".obj");
				FileInfo Surfacefile0=new FileInfo(file_base_name+"0.obj");

				if(Surfacefile.Exists || Surfacefile0.Exists)
	//			if(wantobj)
				{
					LoadOBJRequest(file_base_name);
					MoleculeModel.SurfaceFileExist=true;
					GUIMoleculeController.modif=true;
				}
				else
				{
					MoleculeModel.SurfaceFileExist=false;
				}
					
				FileInfo dxfile=new FileInfo(file_base_name+".dx");
				if(dxfile.Exists)
	//			if(wantdx)
				{
					LoadDxRequest(file_base_name+".dx",MoleculeModel.Offset);
				
				}
			}
 
 
		}           
         	  // 	Regex RE = new Regex("\n", RegexOptions.Multiline);
         	 	// MatchCollection theMatches = RE.Matches(text);
 
         		// int matchescount=theMatches.Count;       
         private void ReadPDB(TextReader sr)
         {
         	ArrayList alist=new ArrayList();
//         	ArrayList CSidList=new ArrayList();
         	ArrayList typelist=new ArrayList();
//         	ArrayList edgelist=new ArrayList();
         	ArrayList resnamelist=new ArrayList();
         	ArrayList atomsNameList = new ArrayList();
         	

         	ArrayList BFactorLsit=new ArrayList();
         	
         	
//         	ArrayList linelist=new ArrayList();
         	
//         	ArrayList CSSGDList=new ArrayList();
//         	ArrayList CSRadiusList=new ArrayList();
//         	ArrayList CSColorList=new ArrayList();
//         	ArrayList CSLabelList=new ArrayList();
         	
         	ArrayList calist=new ArrayList();
         	ArrayList caChainlist=new ArrayList();

         	string[] dnaBackboneAtoms = new string[] {"C5'"};
         	
        	int nowline=0;
        
			string s;
			
			while((s=sr.ReadLine())!=null)
			{
				if(s.StartsWith("ENDMDL"))
					break;

				if(s.Length>4)
				{
					bool isAtomLine = s.StartsWith("ATOM");
					if(UIData.readHetAtom)
						isAtomLine = isAtomLine || s.StartsWith("HETATM");
					if(isAtomLine)
					{
						// Debug.Log(s);
						//print("true");
						float[] vect=new float[3];
						string sx=s.Substring(30,8);
						string sy=s.Substring(38,8);
						string sz=s.Substring(46,8);
						string sbfactor=s.Substring(60,6);
						string typestring=s.Substring(12,4).Trim();
						atomsNameList.Add(typestring);
						int bout;
						bool b = int.TryParse(typestring[0].ToString(), out bout);
						string type;
						if(b)
						{
							type=typestring[1].ToString();
						}
						else
						{
							type=typestring[0].ToString();
							
						}
						string resname=s.Substring(17,3);

						//Unity has a left-handed coordinates system while PDBs are right-handed
						//So we have to inverse the X coordinates
						float x=-float.Parse(sx);
						float y=float.Parse(sy);
						float z=float.Parse(sz);
						float bfactor= float.Parse(sbfactor);
						vect[0]=x;
						vect[1]=y;
						vect[2]=z;
						//Debug.Log(type + " " + x+"   "+y+"   "+z);
						// Debug.Log("vect0="+vect[0]+"vect1="+vect[1]+"vect2="+vect[2]);
						if(typestring[0].ToString()=="C"&&typestring.Length>1)
						{
							if(typestring[1].ToString()=="A")
							{
								string chaintype="chain"+s.Substring(21,1);
								calist.Add(vect);
								caChainlist.Add(chaintype);
								// Debug.Log("chaintype:"+chaintype);
							}
						}
						if(dnaBackboneAtoms.Contains(typestring))
						{
							string chaintype="chain"+s.Substring(21,1);
							calist.Add(vect);
							caChainlist.Add(chaintype);
						}

						BFactorLsit.Add(bfactor);
						alist.Add(vect);
						AtomModel aModel = AtomModel.GetModel(type);
						if(aModel == null) aModel = AtomModel.GetModel("X");
						typelist.Add(aModel);
						resnamelist.Add(resname);
						
					}
					nowline++;
					// progress=(float)nowline/(float)matchescount;
					//Debug.Log(progress);
				}
			}
			isDone=true;
			
		   	// 	for(int i=0;i<alist.Count;i++)
		   	// 	{
						// Debug.Log(alist[i] as float[]);
						// Debug.Log("alist:"+"x:"+(alist[i] as float[])[0]+"|y:"+(alist[i] as float[])[1]+"|z:"+(alist[i] as float[])[2]);
		   	// 	}
		
						//sw.Flush();
			//sw.Close();
			Debug.Log("typelist:"+typelist.Count);
			sr.Close ();

        	Debug.Log("caChainlist:"+caChainlist.Count);

			// }
			
		
			Vector3 minPoint= new Vector3(float.MaxValue,float.MaxValue,float.MaxValue);
    		Vector3 maxPoint= new Vector3(float.MinValue,float.MinValue,float.MinValue);
			
			//float [] a0=alist[0] as float[];
			Vector3 bary = Vector3.zero;			
			for(int i=0; i<alist.Count; i++)
    		{
    			float[] position= alist[i] as float[];
    			minPoint = Vector3.Min(minPoint, new Vector3(position[0],position[1],position[2]));
	        	maxPoint = Vector3.Max(maxPoint, new Vector3(position[0],position[1],position[2]));
				bary = bary+(new Vector3(position[0],position[1],position[2]));
    		}
    		//Vector3 centerPoint = minPoint + ((maxPoint - minPoint) / 2);
			Vector3 centerPoint = bary/alist.Count;
			MoleculeModel.target = Vector3.zero;
			Debug.Log("centerPoint:"+centerPoint + " min/max " + minPoint + "/" + maxPoint);
			
			MoleculeModel.Offset = -centerPoint;
			//Test figure
			// MoleculeModel.Offset = Vector3.zero;

			//Vector3 minPointnew=Vector3.zero;
			//Vector3 maxPointnew=Vector3.zero;
			bary = Vector3.zero;
			Debug.Log("alist.Count:"+alist.Count);
			for(int i=0; i<alist.Count; i++)
			{
				float[] position= alist[i] as float[];
				float[] vect=new float[3];
				vect[0]=position[0]+MoleculeModel.Offset.x;
				vect[1]=position[1]+MoleculeModel.Offset.y;
				vect[2]=position[2]+MoleculeModel.Offset.z;
				// Debug.Log("alist[i]:"+((float[])alist[i])[0]+","+((float[])alist[i])[1]+","+((float[])alist[i])[2]+"||vect:"+vect[0]+","+vect[1]+","+vect[2]);
				alist[i]=vect;
				// minPointnew = Vector3.Min(minPointnew, new Vector3(vect[0],vect[1],vect[2]));
	        	// maxPointnew = Vector3.Max(maxPointnew, new Vector3(vect[0],vect[1],vect[2]));
				bary = bary+(new Vector3(vect[0],vect[1],vect[2]));
			}
			bary = bary/alist.Count;
			Debug.Log("Bary center :" + bary);
			
			MoleculeModel.MinValue = minPoint+MoleculeModel.Offset;
			MoleculeModel.MaxValue = maxPoint+MoleculeModel.Offset;
			
			// Vector3 centerPointnew = minPointnew + ((maxPointnew - minPointnew) / 2);
			MoleculeModel.Center = bary;	
			
			for(int i=0; i<calist.Count; i++)
			{
				float[] position= calist[i] as float[];
				float[] vect=new float[4];
				vect[0] = position[0]+MoleculeModel.Offset.x;
				vect[1] = position[1]+MoleculeModel.Offset.y;
				vect[2] = position[2]+MoleculeModel.Offset.z;
				vect[3] = 0;
				// Debug.Log("alist[i]:"+((float[])alist[i])[0]+","+((float[])alist[i])[1]+","+((float[])alist[i])[2]+"||vect:"+vect[0]+","+vect[1]+","+vect[2]);
				calist[i]=vect;
			}

			
			// Debug.Log("MoleculeModel.target "+MoleculeModel.target);
			// MoleculeModel.cameraLocation.x=MoleculeModel.target.x;
			// MoleculeModel.cameraLocation.y=MoleculeModel.target.y;
			
			MoleculeModel.cameraLocation.x=0;
			MoleculeModel.cameraLocation.y=0;//			MoleculeModel.cameraLocation.z=MoleculeModel.target.z-((maxPoint - minPoint) ).z;
			
			MoleculeModel.cameraLocation.z=MoleculeModel.target.z-(Vector3.Distance(maxPoint,minPoint));
			// MoleculeModel.cameraLocation.z=0;

			MoleculeModel.atomsLocationlist=alist;
			MoleculeModel.CatomsLocationlist=calist;
			MoleculeModel.atomsTypelist=typelist;
			MoleculeModel.BFactorList=BFactorLsit;
			MoleculeModel.atomsNamelist = atomsNameList;
			MoleculeModel.atomsResnamelist=resnamelist;
			
			// Trace interpolation from C-alpha positions
			// Only if there are more than 2 C-alpha
			if(calist.Count > 2)
			{
//				var go = new GameObject("Your Script Container");
//				var geninterpolationarray = go.AddComponent("GenInterpolationArray");
				GenInterpolationArray geninterpolationarray = new GenInterpolationArray();
				geninterpolationarray.InputKeyNodes=calist;
				geninterpolationarray.InputTypeArray=caChainlist;
				geninterpolationarray.CalculateSplineArray();
				calist=null;
				caChainlist=null;
				calist=geninterpolationarray.OutputKeyNodes;
				caChainlist=geninterpolationarray.OutputTypeArray;
			}
			MoleculeModel.CaSplineList=calist;
			MoleculeModel.CaSplineTypeList = new ArrayList();
			for(int k=0; k<calist.Count; k++)
				MoleculeModel.CaSplineTypeList.Add(AtomModel.GetModel(caChainlist[k] as string));
			MoleculeModel.CaSplineChainList=caChainlist;
			
			if(UIData.ffType == UIData.FFType.HiRERNA)
			{
				MoleculeModel.bondEPList=ControlMolecule.CreateBondsList_HiRERNA(atomsNameList);
			}
			else
			{
				// MoleculeModel.bondList=ControlMolecule.CreateBondsList(alist,typelist);
				MoleculeModel.bondEPList=ControlMolecule.CreateBondsEPList(alist,typelist);
				MoleculeModel.bondCAList=ControlMolecule.CreateBondsCAList(caChainlist);	
			}
			
			MoleculeModel.atomsnumber = alist.Count;
			MoleculeModel.bondsnumber = MoleculeModel.bondEPList.Count;

		}

		public  IEnumerator LoadXGMML(string file_name)
		{
			WWW www = new WWW(file_name);
			
			progress = 0;
			isDone = false;
			while(!www.isDone)
			{	
				progress = www.progress;
				yield return new WaitForEndOfFrame();
			}
			Debug.Log("read");
			ReadXGMML(www.text);
			isDone = true;
	 	}

	 	//TODO: avoid reading the file 3 times
	 	public void ReadXGMML(String xml_content)	
		{
			ArrayList alist=new ArrayList();
			ArrayList typelist=new ArrayList();
			ArrayList edgelist=new ArrayList();
			ArrayList resnamelist=new ArrayList();

			ArrayList CSidList=new ArrayList();
			ArrayList CSSGDList=new ArrayList();
			ArrayList CSRadiusList=new ArrayList();
			ArrayList CSColorList=new ArrayList();
			ArrayList CSLabelList=new ArrayList();

			
			XmlReader reader=new XmlTextReader(new StringReader(xml_content));
			while(reader.Read())
			{
				if(reader.ReadToFollowing("node"))
				{
					float[] vect=new float[3];
//					Vector3 v=new Vector3();
					float [] intarrayw= new float[1];
					while (reader.MoveToNextAttribute()) // Read the attributes.
					{
						if(reader.Name=="label")
						{
							string [] intarray= new string[1];
							intarray[0]=reader.Value;
							CSLabelList.Add(intarray);

						}
						else if(reader.Name=="id")
						{
							int [] intarray= new int[1];
							intarray[0]=int.Parse(reader.Value);
							CSidList.Add(intarray);
						}
	//						Debug.Log(" " + reader.Name + "='" + reader.Value + "'");
					}
	//				if(reader.NodeType==XmlNodeType.Element)
	//                {
	//						Debug.Log(" " + reader.Name + "='" + reader.Value + "'");
	//                	string [] intarray= new string[1];
	//						intarray[0]=reader.Value;
	//                	if(reader.LocalName=="SGD symbol")CSSGDList.Add(intarray);
	//                	
	//                }
					reader.ReadToFollowing("graphics");
					while (reader.MoveToNextAttribute()) // Read the attributes.
					{
						if(reader.Name=="type")
						{
							
						}
						else if(reader.Name=="h")
						{

						}
						else if(reader.Name=="w")
						{
	//						Debug.Log(" " + reader.Name + "='" + reader.Value + "'");
							intarrayw[0]=float.Parse(reader.Value)/60;

							CSRadiusList.Add(intarrayw);
						}
						else if(reader.Name=="x")
						{
	//						Debug.Log(" " + reader.Name + "='" + reader.Value + "'");
							//Take the opposite of y beceause Unity is left-handed
							vect[0]=float.Parse(reader.Value)/60;
								
						}
						else if(reader.Name=="y")
						{
	//						Debug.Log(" " + reader.Name + "='" + reader.Value + "'");
							//Take the opposite of y beceause screen is directed in -y
							vect[1]=-float.Parse(reader.Value)/60;
						}
						else if(reader.Name=="fill")
						{
							string [] intarray= new string[1];
							intarray[0]=reader.Value;
							CSColorList.Add(intarray);
							
							
						}
						else if(reader.Name=="width")
						{
							
						}
						else if(reader.Name=="outline")
						{
							
						}
	//						Debug.Log(" " + reader.Name + "='" + reader.Value + "'");

					}
					//Take the opposite for z to make the node pop out toward the user
					vect[2]=-GUIMoleculeController.depthfactor*intarrayw[0];
					alist.Add(vect);
					typelist.Add(AtomModel.GetModel("S"));
	//				modellist.Add(model);
				}
				
												
			}
			reader.Close();
			XmlReader reader2=new XmlTextReader(new StringReader(xml_content));
			while(reader2.Read())
			{
			 	if(reader2.ReadToFollowing("edge"))
				{
					int[] vectint=new int[2];
					while (reader2.MoveToNextAttribute()) // Read the attributes.
					{
						if(reader2.Name=="label")
						{
							
						}
						else if(reader2.Name=="source")
						{
	//						Debug.Log(" " + reader2.Name + "='" + reader2.Value + "'");
							vectint[0]=int.Parse(reader2.Value);
						}
						else if(reader2.Name=="target")
						{
	//						Debug.Log(" " + reader2.Name + "='" + reader2.Value + "'");
							vectint[1]=int.Parse(reader2.Value);
						}
	//						Debug.Log(" " + reader.Name + "='" + reader.Value + "'");
					}
	//						Debug.Log(" vectint[0]:" + vectint[0]+", vectint[1]:" + vectint[1]);
					edgelist.Add(vectint);

				}
		
			}
			reader2.Close();
			
			XmlReader reader3=new XmlTextReader(new StringReader(xml_content));
			while(reader3.Read())
			{
				if(reader3.NodeType==XmlNodeType.Element)
	            {
	            	while (reader3.MoveToNextAttribute()) // Read the attributes.
					{
						if(reader3.Name=="name"&& reader3.Value=="SGD symbol")
						{
							if(reader3.MoveToNextAttribute())
							{
	//							Debug.Log(" " + reader3.Name + "=" + reader3.Value + " ");
	                			string [] intarray= new string[1];
								intarray[0]=reader3.Value;
	                			CSSGDList.Add(intarray);
	//							Debug.Log(" " + reader3.Name + "='" + reader3.Value + "'");
							}
	                		

						}
					 }
				 }

			}
			reader3.Close();
		
			
			MoleculeModel.atomsLocationlist=alist;
			MoleculeModel.atomsTypelist=typelist;
			MoleculeModel.atomsResnamelist=resnamelist;
			MoleculeModel.CSidList=CSidList;
			
			MoleculeModel.CSLabelList=CSLabelList;
			MoleculeModel.CSRadiusList=CSRadiusList;
			MoleculeModel.CSColorList=CSColorList;
			MoleculeModel.CSSGDList=CSSGDList;
			//float [] a0=alist[0] as float[];
			
			Vector3 minPoint= new Vector3(float.MaxValue,float.MaxValue,float.MaxValue);
    		Vector3 maxPoint= new Vector3(float.MinValue,float.MinValue,float.MinValue);
			
			for(int i=0; i<alist.Count; i++)
    		{
    			float[] position= alist[i] as float[];
//    			Debug.Log(position[0]+","+position[1]+","+position[2]);
    			minPoint = Vector3.Min(minPoint, new Vector3(position[0],position[1],position[2]));
	        	maxPoint = Vector3.Max(maxPoint, new Vector3(position[0],position[1],position[2]));
    		}
    		Vector3 centerPoint = minPoint + ((maxPoint - minPoint) / 2);
			//MoleculeModel.target = centerPoint;
			
			Camera.main.transform.position=new Vector3(0,0,0);
			
			MoleculeModel.Offset = -centerPoint;
			MoleculeModel.MinValue = minPoint;
			MoleculeModel.MaxValue = maxPoint;
			Debug.Log("centerPoint="+centerPoint );

		
			
			for(int i=0; i<alist.Count; i++)
			{
				float[] position= alist[i] as float[];
				float[] vectarray=new float[3];
				vectarray[0]=position[0]+MoleculeModel.Offset.x;
				vectarray[1]=position[1]+MoleculeModel.Offset.y;
				vectarray[2]=position[2]+MoleculeModel.Offset.z;
				
				alist[i]=vectarray;
			}
			
			
//			Debug.Log("MoleculeModel.target "+MoleculeModel.target);
			MoleculeModel.cameraLocation.x=MoleculeModel.target.x;
			MoleculeModel.cameraLocation.y=MoleculeModel.target.y;
//			MoleculeModel.cameraLocation.z=MoleculeModel.target.z-((maxPoint - minPoint) ).z;
			MoleculeModel.cameraLocation.z=-80;
//			MoleculeModel.cameraLocation.z=MoleculeModel.target.z;
			
			
//			MoleculeModel.bondList=ControlMolecule.CreateBondsList(alist,typelist);
//			MoleculeModel.bondEPList=ControlMolecule.CreateBondsEPList(alist,typelist);
			MoleculeModel.bondEPList=ControlMolecule.CreateBondsCSList(edgelist);
			MoleculeModel.atomsnumber = alist.Count;
			MoleculeModel.bondsnumber = MoleculeModel.bondEPList.Count;
			
//			return alist;			
		}
		
						////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	private void LoadChoice(int a)
	{
		FileInfo fieldlinefile=new FileInfo(UI.GUIDisplay.file_base_name+".json");
		if(fieldlinefile.Exists)
			wantjson = GUILayout.Toggle (wantjson, new GUIContent ( "JSON" , "Open the Json file"));
		
		FileInfo Surfacefile=new FileInfo(UI.GUIDisplay.file_base_name+".obj");
		FileInfo Surfacefile0=new FileInfo(UI.GUIDisplay.file_base_name+"0.obj");

		if(Surfacefile.Exists || Surfacefile0.Exists)
			wantobj = GUILayout.Toggle (wantobj, new GUIContent ( "OBJ" , "Open the Obj file"));
			
		FileInfo dxfile=new FileInfo(UI.GUIDisplay.file_base_name+".dx");
		if(dxfile.Exists)
				wantdx = GUILayout.Toggle (wantdx, new GUIContent ( "DX" , "Open the Dx file"));
		if (GUILayout.Button("Confirm")){
//				wantpdb=false;
			}

	}
	
	}
	
}
