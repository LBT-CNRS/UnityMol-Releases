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
/// $Id: ColorPicker.cs 648 2014-08-08 13:35:12Z tubiana $
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
using Molecule.Model;



public class ColorPicker{
	
	private Texture2D m_aTexture;
	private Rect m_text_area = new Rect(5, 45, 228, 228);
	private Rect m_close_area = new Rect(5, 20, 50, 20);
	private Rect m_r_area = new Rect(60, 20, 50, 20);
	private Rect m_g_area = new Rect(115, 20, 50, 20);
	private Rect m_b_area = new Rect(170, 20, 50, 20);
	private int m_r_value = 255;
	private int m_g_value = 255;
	private int m_b_value = 255;
	private Rect m_activeArea;
	private Rect m_rect;
	private string m_title;
	private bool m_enabled;
	private int callId;
	
	private List<string> m_atoms = new List<string>();
	private string m_residue = "All";
	private string m_chain = "All";

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
	
	public ColorPicker(int xstart,int ystart,int width, int height, 
		ColorObject colorObj, List<string> atoms, string residue = "All", string chain = "All", 
		string title="Color Picker", int caller=-1)
	{
		m_colorObj = colorObj;
		m_atoms = atoms;
		m_residue = residue;
		m_chain = chain;
		m_r_value = (int)(colorObj.color.r * 255f);
		m_g_value = (int)(colorObj.color.g * 255f);
		m_b_value = (int)(colorObj.color.b * 255f);
		m_rect = new Rect(xstart,ystart,width,height);
	    m_aTexture = (Texture2D)Resources.Load("ImprovedColorPicker");
		m_title=title;
		this.enabled = true;
		m_activeArea = new Rect(m_rect.x + m_text_area.x, m_rect.y + m_text_area.y, m_text_area.width, m_text_area.height);
		callId = caller;
	}	

	public ColorPicker(Rect r, 
		ColorObject colorObj, List<string> atoms, string residue = "All", string chain = "All", 
		string title="Color Picker", int caller=-1)
	{
		m_colorObj = colorObj;
		m_atoms = atoms;
		m_residue = residue;
		m_chain = chain;
		m_r_value = (int)(colorObj.color.r * 255f);
		m_g_value = (int)(colorObj.color.g * 255f);
		m_b_value = (int)(colorObj.color.b * 255f);
		m_rect = r;
	    m_aTexture = (Texture2D)Resources.Load("ImprovedColorPicker");
		m_title=title;
		this.enabled = true;
		m_activeArea = new Rect(m_rect.x + m_text_area.x, m_rect.y + m_text_area.y, m_text_area.width, m_text_area.height);
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
			GUI.DragWindow();
			Vector3 mousePos = Input.mousePosition;
			mousePos.y = Screen.height - mousePos.y;
			if(m_activeArea.Contains(mousePos) && Input.GetMouseButton(0) && GUIUtility.hotControl == 0)
			{		
				int X = (int)((mousePos.x - m_activeArea.x) / m_activeArea.width * m_aTexture.width);
				int Y = (int)((mousePos.y - m_activeArea.y) / m_activeArea.height * m_aTexture.height);
//				Debug.Log(X + " " + Y + " " + mousePos.x + " " + mousePos.y);
				m_r_value = (int)(m_aTexture.GetPixel(X,-Y).r * 255);
				m_g_value = (int)(m_aTexture.GetPixel(X,-Y).g * 255);
				m_b_value = (int)(m_aTexture.GetPixel(X,-Y).b * 255);
//				Debug.Log("Color: "+m_color);
			}
			if(m_r_value < 0)
				m_r_value = 0;
			if(m_r_value > 255)
				m_r_value = 255;
			if(m_g_value < 0)
				m_g_value = 0;
			if(m_g_value > 255)
				m_g_value = 255;
			if(m_b_value < 0)
				m_b_value = 0;
			if(m_b_value > 255)
				m_b_value = 255;
			m_colorObj.color = new Color((float)m_r_value/255f, (float)m_g_value/255f, (float)m_b_value/255f, 1f);
			
			if(m_atoms != null){
				GenericManager manager = Molecule.View.DisplayMolecule.GetManagers()[0];
				if(!UI.GUIMoleculeController.toggle_NA_CLICK){
					manager.SetColor(m_colorObj.color, m_atoms, m_residue, m_chain);
				}
				else{ 
					foreach(GameObject obj in Camera.main.GetComponent<ClickAtom>().objList){
							manager.SetColor(m_colorObj.color, (int)obj.GetComponent<BallUpdate>().number);
					}
				}
			}
			
		}
	}
	
	public void loadColor(int a){
		
		if(GUI.Button(m_close_area, "Close"))
			UI.GUIMoleculeController.m_colorPicker = null;
		string temp;
		temp = GUI.TextField(m_r_area, m_r_value.ToString());
		int.TryParse(temp, out m_r_value);
		temp = GUI.TextField(m_g_area, m_g_value.ToString());
		int.TryParse(temp, out m_g_value);
		temp = GUI.TextField(m_b_area, m_b_value.ToString());
		int.TryParse(temp, out m_b_value);
		GUI.DrawTexture(m_text_area, m_aTexture, ScaleMode.ScaleToFit, true, 0F);
		if (Event.current.type == EventType.Repaint)
        	MoleculeModel.newtooltip = GUI.tooltip;
		
	}

}
