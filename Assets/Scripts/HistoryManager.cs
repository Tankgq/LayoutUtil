using System.Collections.Generic;
using FarPlane;
using UnityEngine;

public static class HistoryManager {
	private static readonly List<Behavior> Behaviors = new List<Behavior>();
	private static int _currentIndex;
	private static int _currentCount;

	public static void Do(Behavior behavior, bool justAdd = false) {
		if(behavior == null) return;
		Add(behavior);
		Do(justAdd);
	}

	private static void Add(Behavior behavior) {
		if(behavior == null) return;
		if(Behaviors.Count == _currentIndex) {
			Behaviors.Add(behavior);
			++ _currentCount;
			return;
		}

		Behaviors[_currentIndex] = behavior;
	}

	public static void Do(bool justAdd = false) {
		while(true) {
			if(_currentIndex >= _currentCount) return;
			Behavior behavior = Behaviors[_currentIndex];
			if(behavior == null || behavior.Type == BehaviorType.Null) {
				Debug.Log($"[WARN] [HistoryManager] Do() - behavior: {behavior}, behavior.Type: {behavior?.Type}");
				break;
			}

			Debug.Log($"[INFO] [HistoryManager] {(behavior.IsDone ? "ReDo" : "Do")}() - behavior: {behavior}, behavior.Type: {behavior.Type}");
			string key = $"{behavior.Type}_{behavior.CreateFrameCount}";
			if(GlobalData.ModifyDic.ContainsKey(key) && GlobalData.ModifyDic[key]) {
				Debug.Log($"[WARN] [HistoryManager] {(behavior.IsDone ? "ReDo" : "Do")}() - key: {key}, behavior.Type: {behavior.Type}");
			}

			GlobalData.ModifyDic[key] = behavior.IsModify;
			if(behavior.IsModify) UlEventSystem.DispatchTrigger<UIEventType>(UIEventType.UpdateTitle);
			if(! justAdd) behavior.Do(behavior.IsDone);
			behavior.IsDone = true;
			++ _currentIndex;
			if(behavior.IsCombineWithNextBehavior) continue;
			break;
		}
	}

	public static void Undo() {
		while(true) {
			if(_currentIndex < 1) return;
			Behavior behavior = Behaviors[_currentIndex - 1];
			if(behavior == null || behavior.Type == BehaviorType.Null) {
				Debug.Log($"[WARN] [HistoryManager] Undo() - behavior: {behavior}, behavior.Type: {behavior?.Type}");
				break;
			}

			Debug.Log($"[INFO] [HistoryManager] {(behavior.IsUndone ? "ReUndo" : "Undo")}() - behavior: {behavior}, behavior.Type: {behavior.Type}");
			string key = $"{behavior.Type}_{behavior.CreateFrameCount}";
			if(! GlobalData.ModifyDic.ContainsKey(key)) {
				Debug.Log($"[ERROR] [HistoryManager] {(behavior.IsUndone ? "ReUndo" : "Undo")}() - key: {key}, behavior.Type: {behavior.Type}");
				return;
			}

			behavior.Undo(behavior.IsUndone);
			if(GlobalData.ModifyDic[key]) UlEventSystem.DispatchTrigger<UIEventType>(UIEventType.UpdateTitle);
			GlobalData.ModifyDic[key] = false;
			behavior.IsUndone = true;
			-- _currentIndex;
			if(_currentIndex < 1) return;
			behavior = Behaviors[_currentIndex - 1];
			if(behavior.IsCombineWithNextBehavior) continue;
			break;
		}
	}
}
