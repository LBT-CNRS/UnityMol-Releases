//using UnityEngine;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Reflection;

public static class PythonUtils {
	public static bool IsPythonFile(string path){
		return System.IO.Path.GetExtension (path) == ".py";
	}

	//This types assembies will be loaded to python runtime engine
	//we need to mention a lot of UnityEngine classes because
	//starting from 2017.2 is now separated to many smaller assemblies
	static System.Type[] c_mustHaveTypes = {
		typeof(PythonUtils),

		typeof(UnityEngine.GameObject),
		typeof(UnityEngine.Rigidbody),
		typeof(UnityEngine.AI.NavMeshAgent),
		//...
	};

	static ScriptEngine m_engine;
	public static ScriptEngine GetEngine(){
		if (m_engine == null) {
			m_engine = Python.CreateEngine ();
			foreach (var type in c_mustHaveTypes) {
				m_engine.Runtime.LoadAssembly (type.Assembly);
			}
		}
		return m_engine;
	}


	public const string defaultPythonConsoleHeader =
@"import clr
clr.AddReference('UnityEngine','System', 'Assembly-CSharp')
from UnityEngine import *
from UMol import *
from UMol.API import *
import System.Single
from System.Collections.Generic import *
def float(x):
	return clr.Convert(x, System.Single)

import UnityEngine
Destroy = UnityEngine.Object.Destroy
FindObjectOfType = UnityEngine.Object.FindObjectOfType
FindObjectsOfType = UnityEngine.Object.FindObjectsOfType
DontDestroyOnLoad = UnityEngine.Object.DontDestroyOnLoad
DestroyImmediate = UnityEngine.Object.DestroyImmediate
Instantiate = UnityEngine.Object.Instantiate
import sys
sys.stdout=console
Select = console.Select
Clear = console.Clear

selM = UnityMolMain.getSelectionManager()
sm = UnityMolMain.getStructureManager()

RGBA = Color

";

	public const string defaultPythonBehaviourHeader =
@"
import clr
clr.AddReference('UnityEngine','System', 'Assembly-CSharp')
from UnityEngine import *
from UMol import *
from UMol.API import *
import System.Single
def float(x):
	return clr.Convert(x, System.Single)

import UnityEngine
Destroy = UnityEngine.Object.Destroy
FindObjectOfType = UnityEngine.Object.FindObjectOfType
FindObjectsOfType = UnityEngine.Object.FindObjectsOfType
DontDestroyOnLoad = UnityEngine.Object.DontDestroyOnLoad
DestroyImmediate = UnityEngine.Object.DestroyImmediate
Instantiate = UnityEngine.Object.Instantiate

Update = None
Awake = None
Start = None
OnEnable = None
OnDisable = None
OnDestroy = None
OnCollisionEnter = None
OnCollisionStay = None
OnCollisionExit = None
";
	
}
