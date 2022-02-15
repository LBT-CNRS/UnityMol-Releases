using UnityEngine;
using Microsoft.Scripting.Hosting;

public class PythonBehaviour : MonoBehaviour {
	[SerializeField]
	protected PythonScript m_script;
	public PythonScript script{
		get{
			return m_script;
		}
		set{
			if (m_script != value) {
				scopeInitializedWithScript = false;
				m_script = value;
			}
		}
	}
	

	private ScriptScope m_scope;
	public ScriptScope scope {
		get{
			CheckConsistency ();
			return m_scope;
		}
		protected set{
			m_scope = value;
		}
	}

	public System.Action onScopeUpdated;

	System.Action awake;
	System.Action onEnable;
	System.Action start;
	System.Action update;
	System.Action onDisable;
	System.Action onDestroy;

	System.Action<Collision> onCollisionEnter;
	System.Action<Collision> onCollisionStay;
	System.Action<Collision> onCollisionExit;
	

	int scriptUpdateCount = -1;
	bool scopeInitializedWithScript;
	void CheckConsistency(){
		FixBrokenReferenceBug ();
		if (m_scope == null) {
			InitScope ();
		}
		if (script == null)
			return;
		if(script.updateCount != scriptUpdateCount || !scopeInitializedWithScript){
			script.Execute (m_scope);

			InitMethod ("Update", ref update);
			InitMethod ("Awake", ref awake);
			InitMethod ("Start", ref start);
			InitMethod ("OnEnable", ref onEnable);
			InitMethod ("OnDisable", ref onDisable);
			InitMethod ("OnDestroy", ref onDestroy);
			InitMethod ("OnCollisionEnter", ref onCollisionEnter);
			InitMethod ("OnCollisionStay", ref onCollisionStay);
			InitMethod ("OnCollisionExit", ref onCollisionExit);

			scriptUpdateCount = script.updateCount;
			scopeInitializedWithScript = true;

			CallOnScopeUpdated ();
		}
	}

	void FixBrokenReferenceBug(){
		#if UNITY_EDITOR
		if (script == null && !Object.ReferenceEquals (script, null)) {
			var so = new UnityEditor.SerializedObject (this);
			var id = script.GetInstanceID ();
			so.FindProperty ("script").objectReferenceInstanceIDValue = 0;
			so.FindProperty ("script").objectReferenceInstanceIDValue = id;
			so.ApplyModifiedPropertiesWithoutUndo ();
		}
		#endif
	}

	void CallOnScopeUpdated(){
		if (onScopeUpdated != null) {
			onScopeUpdated ();
		}
	}

	void InitScope(){
		if (m_scope == null) {
			m_scope = PythonUtils.GetEngine ().CreateScope ();
		}
		m_scope.SetVariable ("owner", this);
		m_scope.SetVariable ("gameObject", gameObject);
		m_scope.SetVariable ("transform", transform);
		scopeInitializedWithScript = false;
	}

	void InitMethod<T>(string methodName, ref T action){
		action = default(T);
		if (m_scope.ContainsVariable (methodName)) {
			action = m_scope.GetVariable<T> (methodName);
		}
	}

	void CallAction(ref System.Action action){
		CheckConsistency ();
		if (script == null || script.compiledWithError)
			return;
		if (action != null) {
			try{
				action ();
			}catch(System.Exception e){
				Debug.LogException (e);
			}
			CallOnScopeUpdated ();
		}
	}
	void CallCollisionAction(ref System.Action<Collision> action, Collision collision){
		CheckConsistency ();
		if (action != null) {
			try{
				action (collision);
			}catch(System.Exception e){
				Debug.LogException (e);
			}
			CallOnScopeUpdated ();
		}
	}

	protected void OnEnable(){
		CallAction (ref onEnable);
	}
	protected void Update(){
		CallAction (ref update);
	}
	protected void OnDisable(){
		CallAction (ref onDisable);
	}
	protected void Start(){
		CallAction (ref start);
	}
	protected virtual void Awake(){
		CallAction (ref awake);
	}
	protected void OnDestroy(){
		CallAction (ref onDestroy);
	}
	protected void OnCollisionEnter(Collision collision){
		CallCollisionAction (ref onCollisionEnter, collision);
	}
	protected void OnCollisionStay(Collision collision){
		CallCollisionAction (ref onCollisionStay, collision);
	}
	protected void OnCollisionExit(Collision collision){
		CallCollisionAction (ref onCollisionExit, collision);
	}
}