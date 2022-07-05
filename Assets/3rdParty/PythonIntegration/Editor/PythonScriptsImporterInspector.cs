using UnityEditor.Experimental.AssetImporters;
using System.IO;
using UnityEditor;
using System;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[CustomEditor(typeof(PythonScriptsImporter))]
internal class PythonScriptsImporterInspector:AssetImporterEditor{
	public override void OnInspectorGUI (){
		//do nothing
	}
}