using System;
using UnityEngine;

public class Behavior {
	public enum BehaviorType {
		Null,
		CreateModule,
		OpenModule,
		ImportModule,
		AddDisplayObject,
		LoadImageToDisplayObject,
		SelectDisplayObject,
	}
	
	// 参数为 isRedo
	public readonly Action<bool> Do;
	public int DoCount;

	// 参数为 isReUndo
	public readonly Action<bool> Undo;
	public int UndoCount;

	// 是否要和下一个 behavior 合并
	public readonly bool IsCombineWithNextBehavior;

	public readonly int CreateFrameCount;
	public BehaviorType Type;

	public Behavior(Action<bool> doBehavior, Action<bool> undoBehavior, bool combineWithNextBehavior = false) {
		Do = doBehavior;
		Undo = undoBehavior;
		DoCount = UndoCount = 0;
		IsCombineWithNextBehavior = combineWithNextBehavior;
		CreateFrameCount = Time.frameCount;
		Type = BehaviorType.Null;
	}
}
