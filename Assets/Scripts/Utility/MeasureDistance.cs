/// @file MeasureDistance.cs
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
/// $Id: MeasureDistance.cs 225 2013-04-07 14:21:34Z baaden $
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

public class MeasureDistance : MonoBehaviour 
{
LineRenderer mLine;
public GameObject obj1;
public GameObject obj2;
public GameObject lineObject;

private LineRenderer line;
private float objDistance;
public bool isChange = false;
private float coordinate_x=-100;
private float coordinate_y=-100;
//private float coordinate_x2=0;
//private float coordinate_y2=0;
//private string atominfo="";

private GameObject halo1;
private GameObject halo2;


    void initLine()
    {
        lineObject = new GameObject("measureline");
        line = lineObject.AddComponent<LineRenderer>();
        line.material = new Material (Shader.Find("Particles/Alpha Blended"));
        line.SetColors(Color.red, Color.red);
        line.SetWidth(0.3f, 0.3f);
//        lineObject.active = false;
        lineObject.SetActive(false);
        objDistance = -1f;
    }


    void OnEnable()
    {
        isChange = false;
        obj1 = null;
        obj2 = null;
        initLine();
    }

    void  Update ()
    {     	
	    if (Input.GetButtonDown ("Fire1")) 
        {
		    Ray sRay= GetComponent<Camera>().ScreenPointToRay (Input.mousePosition);
		    RaycastHit sHit;
            if (Physics.Raycast(sRay, out sHit))
            {
                if (isChange)
                {
                    obj2 = sHit.collider.gameObject;
                    
                    line.SetPosition(1, obj2.transform.position);
                    line.name="measureline";
//                    lineObject.active = true;
                    lineObject.SetActive(true);
                    
                    isChange = false;
                    objDistance = Vector3.Distance(obj1.transform.position, obj2.transform.position);
                    
                    halo2=(GameObject)Instantiate(Resources.Load("transparentsphere"),
                                      obj2.transform.localPosition,
                                      new Quaternion(0f,0f,0f,0f));
                    float rad = obj2.GetComponent<BallUpdate>().GetRealRadius();  
                    halo2.transform.localScale = new Vector3(rad+1,rad+1,rad+1);
                }
                else
                {
                    Destroy(halo1);
                    Destroy(halo2);
                    
                    obj1 = sHit.collider.gameObject;
                    
                    line.SetPosition(0, obj1.transform.position);
//                    lineObject.active = false;
                    lineObject.SetActive(false);

                    isChange = true;
                    objDistance=-1;

                    coordinate_x= Input.mousePosition.x + 5;
					coordinate_y= Screen.height-Input.mousePosition.y - 20;
//                    coordinate_x2= Input.mousePosition.x ;
//					coordinate_y2= Screen.height-Input.mousePosition.y ;

                    halo1=(GameObject)Instantiate(Resources.Load("transparentsphere"),
                                      obj1.transform.localPosition,
                                      new Quaternion(0f,0f,0f,0f));
                    float rad = obj1.GetComponent<BallUpdate>().GetRealRadius();  
                    halo1.transform.localScale = new Vector3(rad+1,rad+1,rad+1);
				}
            }

	    }


        if (Input.GetMouseButtonDown(1))
        {
//            lineObject.active = false;
            lineObject.SetActive(false);
            objDistance = -1;
            Destroy(halo1);
            Destroy(halo2);
        }
    }

    void OnGUI()
    {
        if(objDistance > -1)
            GUI.Box(new Rect (coordinate_x,coordinate_y, 100, 20),objDistance.ToString());
    }
    
    void OnDisable()
    {
        Destroy(halo1);
        Destroy(halo2);
		Destroy(lineObject);
    }

}