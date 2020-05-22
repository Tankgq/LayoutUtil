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
	CopyDisplayObjects,
	UpdateFrameVisible,
	ChangeModuleName,
}

public enum CombineType {
	/// <summary>
	/// 与其它 behavior 相互独立
	/// </summary>
	Independent,
	/// <summary>
	/// 与上一个 behavior 合并
	/// </summary>
	Previous,
	/// <summary>
	/// 与下一个 behavior 合并
	/// </summary>
	Next
}

public class Behavior {
	
	// 参数为 isRedo
	public readonly Action<bool> Do;
	public bool IsDone;

	// 参数为 isReUndo
	public readonly Action<bool> Undo;
	public bool IsUndone;
	
	public readonly CombineType CombineType;
	
	public readonly int CreateFrameCount;
	public readonly BehaviorType Type;
	
	/// <summary>
	/// 当前的行为是否会修改数据
	/// </summary>
	public readonly bool IsModify;

	public Behavior(Action<bool> doBehavior, Action<bool> undoBehavior, BehaviorType type, bool isModify = true, CombineType combineType = CombineType.Independent) {
		Do = doBehavior;
		Undo = undoBehavior;
		IsDone = IsUndone = false;
		Type = type;
		IsModify = isModify;
		CombineType = combineType;
		CreateFrameCount = Time.frameCount;
	}
}
