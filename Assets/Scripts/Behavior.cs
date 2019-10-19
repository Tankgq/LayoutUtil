using System;
using UnityEngine;

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

public class Behavior {
	
	// 参数为 isRedo
	public readonly Action<bool> Do;
	public bool IsDone;

	// 参数为 isReUndo
	public readonly Action<bool> Undo;
	public bool IsUndone;

	// 是否要和下一个 behavior 合并
	public readonly bool IsCombineWithNextBehavior;

	public readonly int CreateFrameCount;
	public readonly BehaviorType Type;

	// 当前的行为是否会修改数据
	public readonly bool IsModify;

	public Behavior(Action<bool> doBehavior, Action<bool> undoBehavior, BehaviorType type, bool isModify = true, bool combineWithNextBehavior = false) {
		Do = doBehavior;
		Undo = undoBehavior;
		IsDone = IsUndone = false;
		Type = type;
		IsModify = isModify;
		IsCombineWithNextBehavior = combineWithNextBehavior;
		CreateFrameCount = Time.frameCount;
	}
}
