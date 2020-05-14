#if UNITY_EDITOR || UNITY_EDITOR_64

using System;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[InitializeOnLoad]
public class CompilerOptionEditor {
	
	private const string CompileStartTimeKey = "CompileStartTime";
	private const string CompilingKey = "Compiling";
	private static bool _compiling;

	static CompilerOptionEditor() {
		_compiling = EditorPrefs.GetBool(CompilingKey, false);
		Observable.EveryUpdate().Subscribe(_ => OnEditorUpdate());
	}

	private static bool _waitingForCompile;

	private static void OnEditorUpdate() {
		if(_waitingForCompile) return;
		bool isCompiling = EditorApplication.isCompiling;
		if(! isCompiling && _compiling) {
			DateTime startCompilingTime = new DateTime(long.Parse(EditorPrefs.GetString(CompileStartTimeKey, DateTime.Now.Ticks.ToString())));
			TimeSpan ts = DateTime.Now - startCompilingTime;
			Debug.Log($"编译结束, 耗时 {ts.TotalMilliseconds:0.000} ms");
			_compiling = false;
			EditorPrefs.SetBool(CompilingKey, _compiling);
		}
		if(! isCompiling || _compiling) return;
		EditorApplication.LockReloadAssemblies();
		_waitingForCompile = true;
		Debug.Log("等待编译");
	}

	[MenuItem("Custom/StartCompile #F10", true)]
	public static bool NeedShowStartCompile() {
		return _waitingForCompile;
	}
	
	[MenuItem("Custom/StartCompile #F10", false)]
	public static void StartCompile() {
		if(! _waitingForCompile) return;
		EditorApplication.UnlockReloadAssemblies();
		_waitingForCompile = false;
		_compiling = true;
		EditorPrefs.SetBool(CompilingKey, true);
		EditorPrefs.SetString(CompileStartTimeKey, DateTime.Now.Ticks.ToString());
		Assert.IsTrue(EditorApplication.isCompiling);
		Debug.Log("开始编译");
	}
}

#endif
