/// @file ClickAtom.cs
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
/// $Id: ClickAtom.cs 486 2014-05-05 08:09:21Z sebastien $
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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI;
using Molecule.Model;

public class ClickAtom : MonoBehaviour 
{
	LineRenderer mLine;
	public List<GameObject> objList = new List<GameObject>();
	
	
	private LineRenderer line;
	//public bool clicked;
	//private float coordinate_x=-100;
	//private float coordinate_y=-100;
	//private float coordinate_x2=0;
	//private float coordinate_y2=0;
	
	private float width=0;
	private float height=0;
	private List<GameObject> haloList = new List<GameObject>();
	
	private List<string> atominfoList = new List<string>();

    void  Update ()
    {     	
	    if (Input.GetButtonDown ("Fire1") && GUIUtility.hotControl == 0) 
        {
		    Ray sRay= GetComponent<Camera>().ScreenPointToRay (Input.mousePosition);
		    RaycastHit sHit;
		    long atomnumber=0;
			GameObject obj;
			GameObject halo;
			string atominfo;
			
            if (Physics.Raycast(sRay, out sHit))
            {
				Vector3 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
            	if(GUIMoleculeController.m_colorPicker != null && GUIMoleculeController.m_colorPicker.enabled 
					&& Rectangles.colorPickerRect.Contains (mousePos))
					return;
				
				obj = sHit.collider.gameObject;
            	if(obj.GetComponent<BallUpdate>() == null || objList.IndexOf(obj) >= 0)
            		return;

		    	atominfo="";
              
            	Vector3 vl=new Vector3();
            	
            	vl=obj.GetComponent<Renderer>().transform.localPosition;
            	
                //Destroy(halo);
            	halo=Instantiate(Resources.Load("transparentsphere"),vl,new Quaternion(0f,0f,0f,0f)) as GameObject;
                float rad = obj.GetComponent<BallUpdate>().GetRealRadius();  
                halo.transform.localScale = new Vector3(rad+1,rad+1,rad+1);
                halo.transform.parent = obj.transform;
				
                atomnumber = obj.GetComponent<BallUpdate>().number;		
	
            	List<float[]> alist=MoleculeModel.atomsLocationlist;
		
				float[] a=alist[(int)atomnumber];
					
				if(UI.GUIDisplay.file_extension=="xgmml")
				{
					
					atominfo+="ID :  "+(MoleculeModel.CSidList[(int)atomnumber])[0];
					atominfo+="  ||  Label :  "+ (MoleculeModel.CSLabelList[(int)atomnumber])[0];
					atominfo+="  ||  X :  "+ (a[0]-MoleculeModel.Offset.x)+" , Y :  "+(a[1]-MoleculeModel.Offset.y)+" , Z :  " + (a[2]-MoleculeModel.Offset.z);
					atominfo+="  ||  Radius :  "+ ((MoleculeModel.CSRadiusList[(int)atomnumber]))[0];
					if(MoleculeModel.CSSGDList.Count>1)
					{
						atominfo+="  ||  SGD symbol :  "+((MoleculeModel.CSSGDList[(int)atomnumber]))[0];
						width=750;
					}
					else
					{
						width=600;
					}
					
					height=25;

				}
				else
				{
					atominfo+="X :  "+(-(a[0]-MoleculeModel.Offset.x))+" , Y :  "+(a[1]-MoleculeModel.Offset.y)+" , Z :  "+(a[2]-MoleculeModel.Offset.z);
					atominfo+=" ||  Type :  " + (MoleculeModel.atomsTypelist[(int)atomnumber]).type;
					atominfo+="("+ MoleculeModel.atomsNamelist[(int)atomnumber]+")";
					atominfo+=" nb "+ MoleculeModel.atomsNumberList[(int)atomnumber];
					atominfo+=" ||  RES "+MoleculeModel.residueIds[(int)atomnumber]+":  " + MoleculeModel.atomsResnamelist[(int)atomnumber];
					atominfo+=" ||  Chain : " + (MoleculeModel.atomsChainList[(int)atomnumber]); 
					
					width=500;
					height=25;
//					Debug.Log(atominfo);
					MoleculeModel.target = new Vector3(a[0],a[1],a[2]);
				}
   
//                coordinate_x= Input.mousePosition.x + 5;
//				coordinate_y= Screen.height-Input.mousePosition.y + 20;
//                coordinate_x2= Input.mousePosition.x ;
//				coordinate_y2= Screen.height-Input.mousePosition.y ;
				
				objList.Add(obj);
				haloList.Add(halo);
				atominfoList.Add(atominfo);

                //clicked = true;
            }
	    }


        if (Input.GetMouseButtonDown(1) && GUIUtility.hotControl == 0)
        {
            Ray sRay= GetComponent<Camera>().ScreenPointToRay (Input.mousePosition);
		    RaycastHit sHit;
			GameObject obj;
			
            if (Physics.Raycast(sRay, out sHit))
            {
				obj = sHit.collider.gameObject;
				if(obj.name != "transparentsphere(Clone)")
            		return;
				int spot = haloList.IndexOf(obj);
				Destroy(haloList[spot]);
				objList.RemoveAt(spot);
				haloList.RemoveAt(spot);
				atominfoList.RemoveAt(spot);
            	//clicked = false;
			}
        }
    }

    void OnGUI()
    {
        if(/*clicked &&*/ atominfoList.Count > 0)
        {
			for(int i=0; i < atominfoList.Count; i++){
        		Vector3 pos = Camera.main.WorldToScreenPoint(objList[i].transform.position);
				GUI.Box(new Rect (pos.x + 5, Screen.height - pos.y + 20, width, height),atominfoList[i]);}
        }

    }
    
    void OnDisable()
    {
    	//clicked = false;
		ClearSelection();
    }
	
	public void ClearSelection()
	{
		foreach(GameObject halo in haloList)
  			Destroy(halo);
		objList.Clear();
		haloList.Clear();
		atominfoList.Clear();
	}
}