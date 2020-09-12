#if UNITY_EDITOR || UNITY_EDITOR_64

using System;
using UnityEditor;
using UnityEditorInternal;

[InitializeOnLoad]
public class EditorWindowFocusUtility {

	public static event Action<bool> OnUnityEditorFocus = hasFocus => {};
	private static bool _appFocused;

	static EditorWindowFocusUtility() {
		EditorApplication.update += Update;
	}

	private static void Update() {
		if(! _appFocused && InternalEditorUtility.isApplicationActive) {
			_appFocused = InternalEditorUtility.isApplicationActive;
			OnUnityEditorFocus(true);
		} else if(_appFocused && ! InternalEditorUtility.isApplicationActive) {
			_appFocused = InternalEditorUtility.isApplicationActive;
			OnUnityEditorFocus(false);
		}
	}
}

#endif
