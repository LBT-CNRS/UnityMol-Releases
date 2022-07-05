using UnityEngine;
using System.IO;
using Microsoft.Scripting.Hosting;

public class PythonScript:ScriptableObject{
	[SerializeField]
	[HideInInspector]
	string m_text;
	public string text {
		get {
			return m_text;
		}
	}

	[SerializeField]
	[HideInInspector]
	int m_updateCount;	
	bool m_dirty = true;
	CompiledCode m_compiled;
	public bool compiledWithError {
		get;
		private set;
	}
		
	public void Execute(ScriptScope scope){
		if (compiledWithError) {
			throw new UnityException ("PythonScript(" + name + ") was compiled with error");
		}
		if (m_compiled == null || m_dirty) {
			Compile ();
		}
		m_compiled.Execute (scope);
	}

	public int updateCount {
		get {
			return m_updateCount + (m_dirty ? 1 : 0);
		}
		private set {
			m_updateCount = value;
		}
	}

	void Compile(){
		var engine = PythonUtils.GetEngine ();
		var source = engine.CreateScriptSourceFromString (PythonUtils.defaultPythonBehaviourHeader + text);
		try {			
			m_compiled = source.Compile ();
		} catch (System.Exception ex) {
			compiledWithError = true;
			m_updateCount++;
			m_dirty = false;
			throw ex;
		}
		m_updateCount++;
		m_dirty = false;
	}

	public static PythonScript CreateFromString(string str, int updateCount = 0){
		var script = ScriptableObject.CreateInstance<PythonScript> ();
		script.updateCount = updateCount;
		script.m_text = str;
		return script;
	}

	public static PythonScript CreateFromFile (string filePath){
		var text = File.ReadAllText (filePath);
		return CreateFromString (text);
	}
}