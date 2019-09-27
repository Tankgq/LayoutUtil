using System.Collections.Generic;
using UnityEngine;

public static class HistoryManager {
	private static readonly List<Behavior> Behaviors = new List<Behavior>();
	private static int _currentIndex;

	public static void Do(Behavior behavior) {
		if(behavior == null) return;
		Add(behavior);
		Do();
	}

	private static void Add(Behavior behavior) {
		if(behavior == null) return;
		if(Behaviors.Count > _currentIndex) return;
		if(Behaviors.Count == _currentIndex) {
			Behaviors.Add(behavior);
			++ _currentIndex;
			return;
		}
		Behaviors[_currentIndex] = behavior;
	}

	public static void Do() {
		while(true) {
			if(_currentIndex >= Behaviors.Count) return;
			Behavior behavior = Behaviors[_currentIndex];
			if(behavior == null || behavior.Type == Behavior.BehaviorType.Null) {
				Debug.Log($"[HistoryManager] Do() - behavior: {behavior}, behavior.Type: {behavior.Type}");
				break;
			}
			behavior.Type = Behavior.BehaviorType.OpenModule;
			string key = $"{behavior.Type}_{behavior.CreateFrameCount}";
			GlobalData.ModifyDic[key] = true;
			behavior.Do(behavior.DoCount > 0);
			++ behavior.DoCount;
			++ _currentIndex;
			if(behavior.IsCombineWithNextBehavior) continue;
			break;
		}
	}

	public static void Undo() {
		while(true) {
			if(_currentIndex < 1) return;
			Behavior behavior = Behaviors[_currentIndex - 1];
			if(behavior == null || behavior.Type == Behavior.BehaviorType.Null) {
				Debug.Log($"[HistoryManager] Do() - behavior: {behavior}, behavior.Type: {behavior.Type}");
				break;
			}
			behavior.Type = Behavior.BehaviorType.OpenModule;
			string key = $"{behavior.Type}_{behavior.CreateFrameCount}";
			behavior.Undo(behavior.UndoCount > 0);
			GlobalData.ModifyDic[key] = false;
			++ behavior.UndoCount;
			-- _currentIndex;
			if(_currentIndex < 1) return;
			behavior = Behaviors[_currentIndex - 1];
			if(behavior.IsCombineWithNextBehavior) continue;
			break;
		}
	}
}
