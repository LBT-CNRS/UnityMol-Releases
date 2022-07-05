using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

[ScriptedImporter(1, "py")]
public class PythonScriptsImporter : ScriptedImporter{
	public override void OnImportAsset(AssetImportContext ctx){
		var existedPythonScriptAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath (ctx.assetPath) as PythonScript;
		//script text
		var text = File.ReadAllText (ctx.assetPath);
		int updateCount = existedPythonScriptAsset == null ? 0 : existedPythonScriptAsset.updateCount;
		var pythonScriptAsset = PythonScript.CreateFromString (text, updateCount);

		//script name
		var fileName = Path.GetFileNameWithoutExtension (ctx.assetPath);
		pythonScriptAsset.name = fileName;

		//script asset
		#if UNITY_2017_3_OR_NEWER
		ctx.AddObjectToAsset ("script", pythonScriptAsset);
		ctx.SetMainObject (pythonScriptAsset);
		#else
		ctx.SetMainAsset("script", pythonScriptAsset);
		#endif

	}
}