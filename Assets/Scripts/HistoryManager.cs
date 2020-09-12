using System;
using System.Collections.Generic;
using FarPlane;
using UnityEngine;

public static class HistoryManager {
	private static readonly Stack<Behavior> Behaviors = new Stack<Behavior>();
	private static readonly Stack<Behavior> UnDoneBehaviors = new Stack<Behavior>();

	public static void Do(Behavior behavior, bool justAdd = false) {
		if(behavior == null) return;
		Add(behavior);
		Do(justAdd);
	}

	private static void Add(Behavior behavior) {
		if(behavior == null) return;
		if(UnDoneBehaviors.Count > 0) UnDoneBehaviors.Clear();
		UnDoneBehaviors.Push(behavior);
	}

	public static void Do(bool justAdd = false) {
		while(UnDoneBehaviors.Count > 0) {
			Behavior behavior = UnDoneBehaviors.Pop();
			if(behavior == null || behavior.Type == BehaviorType.Null) {
				Debug.Log($"[WARN] [HistoryManager] Do() - behavior: {behavior}, behavior.Type: {behavior?.Type}");
				break;
			}
			Debug.Log($"[INFO] [HistoryManager] {(behavior.IsDone ? "ReDo" : "Do")}() - behavior: {behavior}, behavior.Type: {behavior.Type}, behavior.CombineType: {behavior.CombineType}");
			string key = $"{behavior.Type}_{behavior.CreateFrameCount}";
			if(GlobalData.ModifyDic.ContainsKey(key) && GlobalData.ModifyDic[key]) {
				Debug.Log($"[WARN] [HistoryManager] {(behavior.IsDone ? "ReDo" : "Do")}() - key: {key}, behavior.Type: {behavior.Type}");
			}
			GlobalData.ModifyDic[key] = behavior.IsModify;
			if(behavior.IsModify) UlEventSystem.DispatchTrigger<UIEventType>(UIEventType.UpdateTitle);
			if(! justAdd) {
				try {
					behavior.Do(behavior.IsDone);
				} catch(Exception e) {
					Debug.Log(e);
					throw;
				}
			}
			behavior.IsDone = true;
			Behaviors.Push(behavior);
			if(behavior.CombineType == CombineType.Next) continue;
			if(UnDoneBehaviors.Count == 0) break;
			behavior = UnDoneBehaviors.Peek();
			if(behavior == null || behavior.Type == BehaviorType.Null) break;
			if(behavior.CombineType == CombineType.Previous) continue;
			break;
		}
	}

	public static void Undo() {
		while(Behaviors.Count > 0) {
			Behavior behavior = Behaviors.Pop();
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
			UnDoneBehaviors.Push(behavior);
			if(behavior.CombineType == CombineType.Previous) continue;
			if(Behaviors.Count == 0) break;
			behavior = Behaviors.Peek();
			if(behavior == null || behavior.Type == BehaviorType.Null) break;
			if(behavior.CombineType == CombineType.Next) continue;
			break;
		}
	}
}
