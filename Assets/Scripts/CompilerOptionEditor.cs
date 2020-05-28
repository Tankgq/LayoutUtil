#if UNITY_EDITOR || UNITY_EDITOR_64

using System;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[InitializeOnLoad]
public class CompilerOptionEditor {
	private const string CompileStartTimeKey = "CompileStartTime";
	private const string CompilingKey = "Compiling";
	private static bool _waitingForCompile;
	private static bool _compiling;

	static CompilerOptionEditor() {
		_compiling = EditorPrefs.GetBool(CompilingKey, false);
		EditorApplication.update += OnEditorUpdate;
		// 在 update 没能拦截到编译的情况下强行将编译拦截掉
		CompilationPipeline.compilationStarted += OnStartCompilation;
		CompilationPipeline.compilationFinished += OnFinishCompilation;
	}

	private static void OnEditorUpdate() {
		if(_waitingForCompile) return;
		bool isCompiling = EditorApplication.isCompiling;
		if(isCompiling && EditorApplication.isPlaying) {
			StopCompile();
			return;
		}
		if(! isCompiling || _compiling) return;
		StopCompile();
	}

	private static void OnStartCompilation(object obj) {
		if(_compiling) return;
		StopCompile(true);
	}

	private static void OnFinishCompilation(object obj) {
		if(_compiling) return;
		Debug.Log("强行拦截编译结束");
	}

	private static void StopCompile(bool forceStop = false) {
		EditorApplication.LockReloadAssemblies();
		_waitingForCompile = true;
		Debug.Log(forceStop ? "强行拦截编译中" : "等待编译");
	}

	[UnityEditor.Callbacks.DidReloadScripts]
	private static void OnFinishCompilation() {
		string compileStartTime = EditorPrefs.GetString(CompileStartTimeKey, null);
		if(string.IsNullOrWhiteSpace(compileStartTime)) return;
		EditorPrefs.SetString(CompileStartTimeKey, null);
		DateTime startCompilingTime = new DateTime(long.Parse(compileStartTime));
		TimeSpan ts = DateTime.Now - startCompilingTime;
		Debug.Log($"编译结束, 耗时 {ts.TotalMilliseconds:0.000} ms");
		_compiling = false;
		EditorPrefs.SetBool(CompilingKey, _compiling);
	}

	[MenuItem("Util/StartCompile #F10", true)]
	public static bool NeedShowStartCompile() {
		return _waitingForCompile;
	}

	[MenuItem("Util/StartCompile #F10", false)]
	public static void StartCompile() {
		if(! _waitingForCompile) return;
		EditorApplication.UnlockReloadAssemblies();
		_waitingForCompile = false;
		_compiling = true;
		EditorPrefs.SetBool(CompilingKey, true);
		EditorPrefs.SetString(CompileStartTimeKey, DateTime.Now.Ticks.ToString());
		Debug.Log("开始编译");
	}
}

#endif
