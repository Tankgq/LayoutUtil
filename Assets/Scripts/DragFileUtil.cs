using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DragFileUtil : MonoBehaviour {
	private UnityDragAndDropHook _hook;

	private void Start() {
		_hook = new UnityDragAndDropHook();
		_hook.InstallHook();
		_hook.OnDroppedFiles += OnFiles;
	}

	private void OnDisable() {
		_hook?.UninstallHook();
	}

	private void OnDestroy() {
		if(null == _hook) return;
		_hook.UninstallHook();
		GC.Collect();
	}

	private static readonly Regex RegImageSuffix = new Regex(@"\.(jpe?g|png)$");

	private static void OnFiles(IEnumerable<string> aFiles, POINT aPos) {
		try {
			foreach(string path in aFiles) {
				if(! RegImageSuffix.IsMatch(path)) continue;
				Vector2 pos = Utils.GetRealPositionInContainer(new Vector2(aPos.x, aPos.y), 1);
				DisplayObjectUtil.AddDisplayObject(path, pos, Vector2.zero);
			}
		} catch(Exception e) {
			DialogManager.ShowError(e.ToString());
		}
	}
}
