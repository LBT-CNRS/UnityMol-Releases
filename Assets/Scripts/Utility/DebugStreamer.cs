/// @file DebugStreamer.cs
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
/// $Id: DebugStreamer.cs 248 2013-04-07 20:52:21Z baaden $
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

/*	DebugStreamer in c#
	Ankit Goel
	ankit.goel@apra-infotech.com
	· displays on screen a history of 'numberOfLines' of whatever text is sent to 'message'
	· 'showLineMovement' adds a rotating mark at the end of the lines of text, so repetitive message can be seen to be moving

	· to use, add this script to a game object, then from another script simply add "DebugStreamer.message = "text to display";
	This script is the c# version of the debug streamer provided by Jamie McCarter.
*/
using UnityEngine;
using System.Collections;

public class DebugStreamer : MonoBehaviour {

public static string message;
public bool showLineMovement;
public TextAnchor anchorAt = TextAnchor.LowerLeft;
public int numberOfLines = 5;
public int pixelOffset = 5;
	
private GameObject guiObj;
private GUIText guiTxt;
private TextAnchor _anchorAt;
private float _pixelOffset;
private bool _showLineMovement;
private string	_message;
private ArrayList messageHistory = new ArrayList ();
private int messageHistoryLength;
private string	displayText;
private int	patternIndex = 0;
private string[] pattern = new string[] {"-", "\\", "|", "/"};
	// Use this for initialization
	void Awake ()
	{
		guiObj = new GameObject("Debug Streamer");
		guiObj.AddComponent<GUIText>();
		guiObj.transform.position = Vector3.zero;
		guiObj.transform.localScale = new Vector3(0, 0, 1);
		guiObj.name = "Debug Streamer";
		guiTxt = guiObj.GetComponent<GUIText>();
		_anchorAt = anchorAt;
		_message = message;
		SetPosition();
	}
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
			//	if anchorAt or pixelOffset has changed while running, update the text position
		if(_anchorAt!=anchorAt || _pixelOffset!=pixelOffset)
			{
				_anchorAt = anchorAt;
				_pixelOffset = pixelOffset;
				SetPosition();
			}
			
		//	if the message has changed, update the display
		if(_message!=message)
			{
			_message = message;
			if(showLineMovement)
			{
				messageHistory.Insert(0,message + "\t" + pattern[patternIndex]);
				messageHistoryLength = messageHistory.Count;
				//messageHistoryLength = messageHistory.Unshift(message + "\t" + pattern[patternIndex]);
			}
			else
				messageHistory.Insert(0, message);
				messageHistoryLength = messageHistory.Count;
				//messageHistoryLength = messageHistory.Unshift(message);
			
			patternIndex = (patternIndex + 1) % 4;
			while(messageHistoryLength>numberOfLines)
				{
					//messageHistory.Pop();
					messageHistory.RemoveAt(messageHistory.Count - 1);
					messageHistoryLength = messageHistory.Count;
				}
		
			//	create the multi-line text to display
			displayText = "";
			for(int i = 0; i<messageHistory.Count; i++)
				{
				if(i==0)
					displayText = messageHistory[i] as string;
				else
					displayText = (messageHistory[i] as string) + "\n" + displayText;
				}
			
			guiTxt.text = displayText;
			}
	}
	
	public void OnDisable()
	{
		if(guiObj!=null)
			GameObject.DestroyImmediate(guiObj.gameObject);
	}
	
	public void SetPosition()
	{
	switch(anchorAt)
		{
		case TextAnchor.UpperLeft:
			guiObj.transform.position = new Vector3(0.0f, 1.0f, 0.0f);
			guiTxt.anchor = anchorAt;
			guiTxt.alignment = TextAlignment.Left;
			guiTxt.pixelOffset = new Vector2(pixelOffset, -pixelOffset);
			break;
		case TextAnchor.UpperCenter:
			guiObj.transform.position = new Vector3(0.5f, 1.0f, 0.0f);
			guiTxt.anchor = anchorAt;
			guiTxt.alignment = TextAlignment.Center;
			guiTxt.pixelOffset = new Vector2(0, -pixelOffset);
			break;
		case TextAnchor.UpperRight:
			guiObj.transform.position = new Vector3(1.0f, 1.0f, 0.0f);
			guiTxt.anchor = anchorAt;
			guiTxt.alignment = TextAlignment.Right;
			guiTxt.pixelOffset = new Vector2(-pixelOffset, -pixelOffset);
			break;
		case TextAnchor.MiddleLeft:
			guiObj.transform.position = new Vector3(0.0f, 0.5f, 0.0f);
			guiTxt.anchor = anchorAt;
			guiTxt.alignment = TextAlignment.Left;
			guiTxt.pixelOffset = new Vector2(pixelOffset, 0.0f);
			break;
		case TextAnchor.MiddleCenter:
			guiObj.transform.position = new Vector3(0.5f, 0.5f, 0.0f);
			guiTxt.anchor = anchorAt;
			guiTxt.alignment = TextAlignment.Center;
			guiTxt.pixelOffset = new Vector2(0, 0);
			break;
		case TextAnchor.MiddleRight:
			guiObj.transform.position = new Vector3(1.0f, 0.5f, 0.0f);
			guiTxt.anchor = anchorAt;
			guiTxt.alignment = TextAlignment.Right;
			guiTxt.pixelOffset = new Vector2(-pixelOffset, 0.0f);
			break;
		case TextAnchor.LowerLeft:
			guiObj.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
			guiTxt.anchor = anchorAt;
			guiTxt.alignment = TextAlignment.Left;
			guiTxt.pixelOffset = new Vector2(pixelOffset, pixelOffset);
			break;
		case TextAnchor.LowerCenter:
			guiObj.transform.position = new Vector3(0.5f, 0.0f, 0.0f);
			guiTxt.anchor = anchorAt;
			guiTxt.alignment = TextAlignment.Center;
			guiTxt.pixelOffset = new Vector2(0, pixelOffset);
			break;
		case TextAnchor.LowerRight:
			guiObj.transform.position = new Vector3(1.0f, 0.0f, 0.0f);
			guiTxt.anchor = anchorAt;
			guiTxt.alignment = TextAlignment.Right;
			guiTxt.pixelOffset = new Vector2(-pixelOffset, pixelOffset);
			break;
		}
	}
}
