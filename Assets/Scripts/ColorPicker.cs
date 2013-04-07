/// @file ColorPicker.cs
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
/// $Id: ColorPicker.cs 223 2013-04-06 22:32:11Z baaden $
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
using Molecule.Model;



public class ColorPicker{
	
	private Texture2D m_aTexture;
	private Rect m_text_area = new Rect(5, 45, 228, 256);
	private Rect m_close_area = new Rect(5, 20, 50, 20);
	private Rect m_activeArea;
	private Rect m_rect;
	private string m_title;
	private bool m_enabled;
	private int callId;

	public bool enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			m_enabled = value;
		}
	}
	//We use a ColorObject to reference a variable from outside
	//Impossible to do with UnityEngine.Color as it is a Struct
	private ColorObject m_colorObj;
	public ColorObject color
	{
		get 
		{
			return m_colorObj;
		}
		set 
		{
			m_colorObj = value;
		}
	}	
	
	public ColorPicker(int xstart,int ystart,int width, int height, ColorObject colorObj, string title="Color Picker", int caller=-1)
	{
		m_colorObj = colorObj;
		m_rect = new Rect(xstart,ystart,width,height);
	    m_aTexture = (Texture2D)Resources.Load("EnergyGrayColor2");
		m_title=title;
		this.enabled = true;
		m_activeArea = new Rect(m_rect.x + m_text_area.y, m_rect.y + m_text_area.y, m_text_area.width, m_text_area.height);
		callId = caller;
	}	

	public ColorPicker(Rect r, ColorObject colorObj, string title="Color Picker", int caller=-1)
	{
		m_colorObj = colorObj;
		m_rect = r;
	    m_aTexture = (Texture2D)Resources.Load("EnergyGrayColor2");
		m_title=title;
		this.enabled = true;
		m_activeArea = new Rect(m_rect.x + m_text_area.y, m_rect.y + m_text_area.y, m_text_area.width, m_text_area.height);
		callId = caller;
	}

	public void SwitchEnable(int caller)
	{
		if(caller == callId)
			enabled = !enabled;
	}

	public void OnGUI()
	{
		if(enabled)
		{
			GUI.Window(60, m_rect, loadColor, m_title);
			Vector3 mousePos = Input.mousePosition;
			mousePos.y = Screen.height - mousePos.y;
			if(m_activeArea.Contains(mousePos)&& Input.GetMouseButton(0))
			{		
				//Debug.Log("test4");
				int X = (int)(mousePos.x-m_rect.x)-5;
				int Y = (int)(m_text_area.height)-((int)(mousePos.y-m_rect.y)-(int)(m_text_area.y));
				
				m_colorObj.color = m_aTexture.GetPixel(X,Y);
				// Debug.Log("Color: "+m_color);
			}
		}
	}
	
	public void loadColor(int a){
		
//		Debug.Log("test3");
		if(GUI.Button(m_close_area, "Close"))
			enabled = false;
		GUI.DrawTexture(m_text_area, m_aTexture, ScaleMode.ScaleToFit, true, 0F);
		if (Event.current.type == EventType.Repaint)
        	MoleculeModel.newtooltip = GUI.tooltip;
		
	}

}
