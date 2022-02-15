using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using UnityEditor;

[CustomEditor(typeof(PythonScript))]
public class PythonScriptAssetInspector:Editor{
	private const int kMaxChars = 7000;
	private GUIStyle m_TextStyle;
	public override void OnInspectorGUI ()
	{
		if (this.m_TextStyle == null) {
			this.m_TextStyle = "ScriptText";
		}
		bool enabled = GUI.enabled;
		GUI.enabled = true;
		PythonScript textAsset = base.target as PythonScript;
		if (textAsset != null) {
			string text;
			if (base.targets.Length > 1) {
				text = "";
			} else {
				text = textAsset.text;
				if (text.Length > 7000) {
					text = text.Substring (0, 7000) + @"...

						<...etc...>";
				}
			}
			Rect rect = GUILayoutUtility.GetRect (new GUIContent (text), this.m_TextStyle);
			rect.x = 0;
			rect.y -= 3;
			rect.width += 17;
			GUI.Box (rect, text, this.m_TextStyle);
		}
		GUI.enabled = enabled;
	}
}