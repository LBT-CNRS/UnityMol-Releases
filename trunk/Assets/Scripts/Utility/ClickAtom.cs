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
/// $Id: ClickAtom.cs 225 2013-04-07 14:21:34Z baaden $
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
using UI;
using Molecule.Model;

public class ClickAtom : MonoBehaviour 
{
LineRenderer mLine;
public GameObject obj;


private LineRenderer line;
public bool clicked;
//private float coordinate_x=-100;
//private float coordinate_y=-100;
//private float coordinate_x2=0;
//private float coordinate_y2=0;

private float width=0;
private float height=0;
private GameObject halo;

private string atominfo="";

    void  Update ()
    {     	
	    if (Input.GetButtonDown ("Fire1")) 
        {
		    Ray sRay= camera.ScreenPointToRay (Input.mousePosition);
		    RaycastHit sHit;
		    long atomnumber=0;
            if (Physics.Raycast(sRay, out sHit))
            {
            	obj = sHit.collider.gameObject;
            	if(obj.GetComponent<BallUpdate>() == null)
            		return;

		    	atominfo="";
              
            	Vector3 vl=new Vector3();
            	
            	vl=obj.renderer.transform.localPosition;
            	
                Destroy(halo);
            	halo=Instantiate(Resources.Load("transparentsphere"),vl,new Quaternion(0f,0f,0f,0f)) as GameObject;
                float rad = obj.GetComponent<BallUpdate>().GetRealRadius();  
                halo.transform.localScale = new Vector3(rad+1,rad+1,rad+1);
				
                atomnumber = obj.GetComponent<BallUpdate>().number;		
	

            	ArrayList alist=MoleculeModel.atomsLocationlist;
		
					
				float [] a=alist[(int)atomnumber] as float[];
					
				if(UI.GUIDisplay.file_extension=="xgmml")
				{
					
					atominfo+="ID :  "+(MoleculeModel.CSidList[(int)atomnumber] as int[])[0];
					atominfo+="  ||  Label :  "+ (MoleculeModel.CSLabelList[(int)atomnumber] as string[])[0];
					atominfo+="  ||  X :  "+(a[0]-MoleculeModel.Offset.x)+" , Y :  "+(a[1]-MoleculeModel.Offset.y)+" , Z :  "+(a[2]-MoleculeModel.Offset.z);
					atominfo+="  ||  Radius :  "+((MoleculeModel.CSRadiusList[(int)atomnumber]) as float[])[0];
					if(MoleculeModel.CSSGDList.Count>1)
					{
						atominfo+="  ||  SGD symbol :  "+((MoleculeModel.CSSGDList[(int)atomnumber]) as string[])[0];
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
					atominfo+="X :  "+(a[0]-MoleculeModel.Offset.x)+" , Y :  "+(a[1]-MoleculeModel.Offset.y)+" , Z :  "+(a[2]-MoleculeModel.Offset.z);
					atominfo+="  ||  Type :  "+(MoleculeModel.atomsTypelist[(int)atomnumber] as AtomModel).type;;
					atominfo+="  ||  RES :  "+MoleculeModel.atomsResnamelist[(int)atomnumber];
					
					width=400;
					height=25;
//					Debug.Log(atominfo);
					MoleculeModel.target = new Vector3(a[0],a[1],a[2]);
				}
   
//                coordinate_x= Input.mousePosition.x + 5;
//				coordinate_y= Screen.height-Input.mousePosition.y + 20;
//                coordinate_x2= Input.mousePosition.x ;
//				coordinate_y2= Screen.height-Input.mousePosition.y ;

                clicked = true;
				
            }

	    }


        if (Input.GetMouseButtonDown(1))
        {
            Destroy(halo);
            clicked = false;
        }
    }

    void OnGUI()
    {
        if(clicked)
        {
        	Vector3 pos = Camera.main.WorldToScreenPoint(obj.transform.position);
            GUI.Box(new Rect (pos.x + 5, Screen.height - pos.y + 20, width, height),atominfo);
        }

    }
    
    void OnDisable()
    {
    	clicked = false;	
  		Destroy(halo);	
    }
}
