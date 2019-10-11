using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BehaviorFactory {
	public static Behavior GetCreateModuleBehavior(string moduleName) {
		return new Behavior(isReDo => ModuleUtil.CreateModuleBehavior(moduleName),
							isReUndo => ModuleUtil.RemoveModuleBehavior(moduleName),
							Behavior.BehaviorType.CreateModule);
	}

	public static Behavior GetRemoveModuleBehavior(string moduleName) {
		return new Behavior(isReDo => ModuleUtil.RemoveModuleBehavior(moduleName),
							isReUndo => ModuleUtil.CreateModuleBehavior(moduleName, null, true, true),
							Behavior.BehaviorType.RemoveModule);
	}

	public static Behavior GetRemoveAllModuleBehavior(List<string> modules, bool combineWithNextBehavior = false) {
		return new Behavior(isRedo => {
								int count = modules.Count;
								for(int idx = 0; idx < count; ++ idx) ModuleUtil.RemoveModuleBehavior(modules[idx]);
							},
							isReUndo => {
								int count = modules.Count;
								for(int idx = 0; idx < count; ++ idx) ModuleUtil.CreateModuleBehavior(modules[idx], null, false, true);
							},
							Behavior.BehaviorType.RemoveAllModule,
							combineWithNextBehavior);
	}

	public static Behavior GetImportModulesBehavior(string filePath, List<Module> modules) {
		string previousFilePath = GlobalData.CurrentFilePath;
		return new Behavior(isRedo => {
								int count = modules.Count;
								for(int idx = 0; idx < count; ++ idx) {
									Module module = modules[idx];
									ModuleUtil.CreateModuleBehavior(module.Name,
																	module.Elements,
																	false,
																	isRedo);
								}

								GlobalData.CurrentFilePath = filePath;
							},
							isReUndo => {
								int count = modules.Count;
								for(int idx = 0; idx < count; ++ idx) {
									Module module = modules[idx];
									ModuleUtil.RemoveModuleBehavior(module.Name);
								}

								GlobalData.CurrentFilePath = previousFilePath;
							},
							Behavior.BehaviorType.ImportModules);
	}

	public static Behavior GetOpenModuleBehavior(string moduleName, bool combineWithNextBehavior = false) {
		string previousModuleName = GlobalData.CurrentModule;
		return new Behavior(isRedo => GlobalData.CurrentModule = moduleName,
							isReUndo => GlobalData.CurrentModule = previousModuleName,
							Behavior.BehaviorType.OpenModule,
							combineWithNextBehavior);
	}

	public static Behavior GetAddDisplayObjectBehavior(string  moduleName, string  elementName, string  imageUrl, Vector2 pos, Vector2 size) {
		Element element = new Element {
			Name = elementName,
			X = Element.ConvertX(pos.x),
			Y = Element.ConvertY(pos.y),
			Width = size.x,
			Height = size.y,
			Visible = true
		};
		return new Behavior(isRedo => DisplayObjectUtil.AddDisplayObjectBehavior(moduleName, element, imageUrl),
							isReUndo => DisplayObjectUtil.RemoveDisplayObjectBehavior(moduleName, element.Name),
							Behavior.BehaviorType.AddDisplayObject);
	}

	public static Behavior GetLoadImageToDisplayObjectBehavior(string moduleName,
															   string elementName,
															   string imageUrl,
															   bool   isModify = true) {
		return new Behavior(isReDo => DisplayObjectUtil.LoadImageBehavior(moduleName, elementName, imageUrl),
							isReUndo => DisplayObjectUtil.RemoveImageBehavior(moduleName, elementName),
							Behavior.BehaviorType.LoadImageToDisplayObject,
							isModify);
	}

	public static Behavior GetRemoveSelectedDisplayObjectBehavior(string moduleName) {
		List<Element> elements = GlobalData
								.CurrentSelectDisplayObjectDic
								.Select(pair => GlobalData.GetElement(pair.Key))
								.ToList();
		int length = elements.Count;
		List<string> elementNames = new List<string>();
		for(int idx = 0; idx < length; ++ idx) elementNames.Add(elements[idx].Name);
		return new Behavior(isReDo => DisplayObjectUtil.RemoveDisplayObjectsBehavior(moduleName, elementNames),
							isReUndo => {
								DisplayObjectUtil.AddDisplayObjectsBehavior(moduleName, elements);
								foreach(string elementName in elementNames) {
									Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
									GlobalData.CurrentSelectDisplayObjectDic.Add(elementName, displayObject);
								}

								MessageBroker.SendUpdateSelectDisplayObjectDic(elementNames);
							},
							Behavior.BehaviorType.RemoveSelectedDisplayObject);
	}

	public static Behavior GetUpdateSelectDisplayObjectBehavior(string moduleName, List<string> addElements = null, List<string> removeElements = null) {
		return new Behavior(isReDo => DisplayObjectUtil.UpdateSelectDisplayObjectDicBehavior(moduleName, addElements, removeElements),
							isReUndo => DisplayObjectUtil.UpdateSelectDisplayObjectDicBehavior(moduleName, removeElements, addElements),
							Behavior.BehaviorType.UpdateSelectedDisplayObjectDic,
							false);
	}

	public static Behavior GetUpdateDisplayObjectsPosBehavior(string moduleName, IReadOnlyList<string> elementNames, Vector2 originPos, Vector2 targetPos) {
		return new Behavior(isReDo => DisplayObjectUtil.UpdateDisplayObjectsPosition(moduleName, elementNames, targetPos),
							isReUndo => DisplayObjectUtil.UpdateDisplayObjectsPosition(moduleName, elementNames, originPos),
							Behavior.BehaviorType.UpdateDisplayObjectsPos);
	}

	public static Behavior GetUpdateSwapImageBehavior(string moduleName, string elementName, bool isSwap) {
		return new Behavior(isReDo => MessageBroker.SendUpdateSwapImage(moduleName, elementName, isSwap),
							isReUndo => MessageBroker.SendUpdateSwapImage(moduleName, elementName, ! isSwap),
							Behavior.BehaviorType.UpdateSwapImage);
	}

	public static Behavior GetChangeNameBehavior(string moduleName, string originName, string newName) {
		return new Behavior(isReDo => DisplayObjectUtil.ChangeNameBehavior(moduleName, originName, newName),
							isReUndo => DisplayObjectUtil.ChangeNameBehavior(moduleName, newName, originName),
							Behavior.BehaviorType.ChangeName);
	}

	public static Behavior GetChangeXBehavior(string moduleName, List<string> elementNames, float originX, float newX, bool isAdd = false) {
		return new Behavior(isReDo => DisplayObjectUtil.ChangeXBehavior(moduleName, elementNames, newX, isAdd),
							isReUndo => DisplayObjectUtil.ChangeXBehavior(moduleName, elementNames, originX, isAdd),
							Behavior.BehaviorType.ChangeX);
	}

	public static Behavior GetChangeYBehavior(string moduleName, List<string> elementNames, float originY, float newY, bool isAdd = false) {
		return new Behavior(isReDo => DisplayObjectUtil.ChangeYBehavior(moduleName, elementNames, newY, isAdd),
							isReUndo => DisplayObjectUtil.ChangeYBehavior(moduleName, elementNames, originY, isAdd),
							Behavior.BehaviorType.ChangeY);
	}

	public static Behavior GetChangeWidthBehavior(string moduleName, List<string> elementNames, float originWidth, float newWidth, bool isAdd = false) {
		return new Behavior(isReDo => DisplayObjectUtil.ChangeWidthBehavior(moduleName, elementNames, newWidth, isAdd),
							isReUndo => DisplayObjectUtil.ChangeWidthBehavior(moduleName, elementNames, originWidth, isAdd),
							Behavior.BehaviorType.ChangeWidth);
	}

	public static Behavior GetChangeHeightBehavior(string moduleName, List<string> elementNames, float originHeight, float newHeight, bool isAdd = false) {
		return new Behavior(isReDo => DisplayObjectUtil.ChangeHeightBehavior(moduleName, elementNames, newHeight, isAdd),
							isReUndo => DisplayObjectUtil.ChangeHeightBehavior(moduleName, elementNames, originHeight, isAdd),
							Behavior.BehaviorType.ChangeHeight);
	}

	public static Behavior GetMoveModuleUpBehavior(string moduleName) {
		return new Behavior(isReDo => GlobalData.HierarchyManager.MoveModuleUpBehavior(moduleName),
							isReUndo => GlobalData.HierarchyManager.MoveModuleDownBehavior(moduleName),
							Behavior.BehaviorType.MoveModuleUp);
	}

	public static Behavior GetMoveModuleDownBehavior(string moduleName) {
		return new Behavior(isReDo => GlobalData.HierarchyManager.MoveModuleDownBehavior(moduleName),
							isReUndo => GlobalData.HierarchyManager.MoveModuleUpBehavior(moduleName),
							Behavior.BehaviorType.MoveModuleDown);
	}

	public static Behavior GetMoveDisplayObjectsUpBehavior(string moduleName, List<string> elementNames) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return null;
		if(elementNames == null || elementNames.Count == 0) return null;
		List<Transform> displayObjects = GlobalData.CurrentDisplayObjects;
		List<int> elementIdxList = elementNames.Select(elementName => displayObjects.FindIndex(element => elementName.Equals(element.name)))
											   .Where(idx => idx != -1)
											   .ToList();
		if(elementIdxList.Count == 0) return null;
		elementIdxList.Sort();
		if(elementIdxList[0] == 0) return null;
		return new Behavior(isReDo => DisplayObjectUtil.MoveDisplayObjectsUpBehavior(moduleName, elementIdxList),
							isReUndo => DisplayObjectUtil.MoveDisplayObjectsDownBehavior(moduleName, elementIdxList),
							Behavior.BehaviorType.MoveSelectDisplayObjectsUp);
	}

	public static Behavior GetMoveDisplayObjectsDownBehavior(string moduleName, List<string> elementNames) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return null;
		if(elementNames == null || elementNames.Count == 0) return null;
		List<Transform> displayObjects = GlobalData.CurrentDisplayObjects;
		List<int> elementIdxList = elementNames.Select(elementName => displayObjects.FindIndex(element => elementName.Equals(element.name)))
											   .Where(idx => idx != -1)
											   .ToList();
		int count = elementIdxList.Count;
		if(count == 0) return null;
		elementIdxList.Sort();
		if(elementIdxList[count - 1] == displayObjects.Count - 1) return null;
		return new Behavior(isReDo => DisplayObjectUtil.MoveDisplayObjectsDownBehavior(moduleName, elementIdxList),
							isReUndo => DisplayObjectUtil.MoveDisplayObjectsUpBehavior(moduleName, elementIdxList),
							Behavior.BehaviorType.MoveSelectDisplayObjectsDown);
	}

	public static Behavior GetCopyDisplayObjectsBehavior(string moduleName, List<string> elementNames) {
		return new Behavior(isReDo => DisplayObjectUtil.CopySelectDisplayObjectsBehavior(moduleName, elementNames),
							isReUndo => DisplayObjectUtil.RemoveDisplayObjectsBehavior(moduleName, elementNames),
							Behavior.BehaviorType.CopyDisplayObjects);
	}
}
