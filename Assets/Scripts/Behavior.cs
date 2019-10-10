using System;
using UnityEngine;

public class Behavior {
	public enum BehaviorType {
		Null,
		CreateModule,
		RemoveModule,
		RemoveAllModule,
		ImportModules,
		OpenModule,
		AddDisplayObject,
		LoadImageToDisplayObject,
		RemoveSelectedDisplayObject,
		UpdateSelectedDisplayObjectDic,
		UpdateDisplayObjectsPos,
		UpdateSwapImage,
		ChangeName,
		ChangeX,
		ChangeY,
		ChangeWidth,
		ChangeHeight,
		MoveModuleUp,
		MoveModuleDown,
		MoveSelectDisplayObjectsUp,
		MoveSelectDisplayObjectsDown,
		CopyDisplayObjects
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

	// 当前的行为是否会修改数据
	public readonly bool IsModify;

	public Behavior(Action<bool> doBehavior, Action<bool> undoBehavior, BehaviorType type, bool isModify = true, bool combineWithNextBehavior = false) {
		Do = doBehavior;
		Undo = undoBehavior;
		DoCount = UndoCount = 0;
		Type = type;
		IsModify = isModify;
		IsCombineWithNextBehavior = combineWithNextBehavior;
		CreateFrameCount = Time.frameCount;
	}
}
