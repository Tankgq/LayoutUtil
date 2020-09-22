using System.Collections.Generic;
using System.Linq;
using FarPlane;
using UnityEngine;

public static class BehaviorFactory {
	public static Behavior GetCreateModuleBehavior(string moduleName) {
		string currentModule = GlobalData.CurrentModule;
		return new Behavior(isReDo => ModuleUtil.CreateModuleBehavior(moduleName),
							isReUndo => ModuleUtil.RemoveModuleBehavior(moduleName, currentModule),
							BehaviorType.CreateModule);
	}

	public static Behavior GetRemoveModuleBehavior(string moduleName) {
		return new Behavior(isReDo => ModuleUtil.RemoveModuleBehavior(moduleName),
							isReUndo => ModuleUtil.CreateModuleBehavior(moduleName, null, true, true),
							BehaviorType.RemoveModule);
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
							BehaviorType.RemoveAllModule,
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
							BehaviorType.ImportModules,
							false);
	}

	public static Behavior GetOpenModuleBehavior(string moduleName, CombineType combineType = CombineType.Independent) {
		string previousModuleName = GlobalData.CurrentModule;
		return new Behavior(isRedo => ModuleUtil.OpenModule(moduleName),
							isReUndo => ModuleUtil.OpenModule(previousModuleName),
							BehaviorType.OpenModule,
							false,
							combineType);
	}

	public static Behavior GetAddDisplayObjectBehavior(string moduleName,
													   string elementName,
													   string imageUrl,
													   Vector2 pos,
													   Vector2 size,
													   CombineType combineType = CombineType.Independent) {
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
							BehaviorType.AddDisplayObject,
							true,
							combineType);
	}

	public static Behavior GetLoadImageToDisplayObjectBehavior(string moduleName,
															   string elementName,
															   string imageUrl,
															   bool isModify = true) {
		return new Behavior(isReDo => DisplayObjectUtil.LoadImageBehavior(moduleName, elementName, imageUrl),
							isReUndo => DisplayObjectUtil.RemoveImageBehavior(moduleName, elementName),
							BehaviorType.LoadImageToDisplayObject,
							isModify);
	}

	public static Behavior GetRemoveSelectedDisplayObjectBehavior(string moduleName) {
		List<Element> elements = GlobalData.CurrentSelectDisplayObjectDic.Select(pair => GlobalData.GetElement(pair.Key)).ToList();
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
								UlEventSystem.Dispatch<DataEventType, SelectedChangeData>(DataEventType.SelectedChange,
																						  new SelectedChangeData(moduleName, elementNames));
//								MessageBroker.SendUpdateSelectDisplayObjectDic(elementNames);
							},
							BehaviorType.RemoveSelectedDisplayObject);
	}

	public static Behavior GetUpdateSelectDisplayObjectBehavior(string moduleName,
																List<string> addElements = null,
																List<string> removeElements = null,
																CombineType combineType = CombineType.Independent) {
		return new Behavior(isReDo => DisplayObjectUtil.UpdateSelectDisplayObjectDicBehavior(moduleName, addElements, removeElements),
							isReUndo => DisplayObjectUtil.UpdateSelectDisplayObjectDicBehavior(moduleName, removeElements, addElements),
							BehaviorType.UpdateSelectedDisplayObjectDic,
							false,
							combineType);
	}

	public static Behavior GetUpdateDisplayObjectsPosBehavior(string moduleName, IReadOnlyList<string> elementNames, Vector2 originPos, Vector2 targetPos) {
		return new Behavior(isReDo => DisplayObjectUtil.UpdateDisplayObjectsPosition(moduleName, elementNames, targetPos),
							isReUndo => DisplayObjectUtil.UpdateDisplayObjectsPosition(moduleName, elementNames, originPos),
							BehaviorType.UpdateDisplayObjectsPos);
	}

	public static Behavior GetUpdateSwapImageBehavior(string moduleName, string elementName, bool isSwap) {
		return new Behavior(isReDo => UlEventSystem.Dispatch<UIEventType, SwapImageEventData>(UIEventType.SwapImage,
																							  new SwapImageEventData(moduleName, elementName, isSwap)),
							isReUndo => UlEventSystem.Dispatch<UIEventType, SwapImageEventData>(UIEventType.SwapImage,
																								new SwapImageEventData(moduleName, elementName, ! isSwap)),
							BehaviorType.UpdateSwapImage);
	}

	public static Behavior GetChangeNameBehavior(string moduleName, string originName, string newName) {
		return new Behavior(isReDo => DisplayObjectUtil.ChangeNameBehavior(moduleName, originName, newName),
							isReUndo => DisplayObjectUtil.ChangeNameBehavior(moduleName, newName, originName),
							BehaviorType.ChangeName);
	}

	public static Behavior GetChangeXBehavior(string moduleName, List<string> elementNames, float originX, float newX, bool isAdd = false) {
		return new Behavior(isReDo => DisplayObjectUtil.ChangeXBehavior(moduleName, elementNames, newX, isAdd),
							isReUndo => DisplayObjectUtil.ChangeXBehavior(moduleName, elementNames, originX, isAdd),
							BehaviorType.ChangeX);
	}

	public static Behavior GetChangeYBehavior(string moduleName, List<string> elementNames, float originY, float newY, bool isAdd = false) {
		return new Behavior(isReDo => DisplayObjectUtil.ChangeYBehavior(moduleName, elementNames, newY, isAdd),
							isReUndo => DisplayObjectUtil.ChangeYBehavior(moduleName, elementNames, originY, isAdd),
							BehaviorType.ChangeY);
	}

	public static Behavior GetChangeWidthBehavior(string moduleName, List<string> elementNames, float originWidth, float newWidth, bool isAdd = false) {
		return new Behavior(isReDo => DisplayObjectUtil.ChangeWidthBehavior(moduleName, elementNames, newWidth, isAdd),
							isReUndo => DisplayObjectUtil.ChangeWidthBehavior(moduleName, elementNames, originWidth, isAdd),
							BehaviorType.ChangeWidth);
	}

	public static Behavior GetChangeHeightBehavior(string moduleName, List<string> elementNames, float originHeight, float newHeight, bool isAdd = false) {
		return new Behavior(isReDo => DisplayObjectUtil.ChangeHeightBehavior(moduleName, elementNames, newHeight, isAdd),
							isReUndo => DisplayObjectUtil.ChangeHeightBehavior(moduleName, elementNames, originHeight, isAdd),
							BehaviorType.ChangeHeight);
	}

	public static Behavior GetMoveModuleUpBehavior(string moduleName) {
		return new Behavior(isReDo => GlobalData.HierarchyManager.MoveModuleUpBehavior(moduleName),
							isReUndo => GlobalData.HierarchyManager.MoveModuleDownBehavior(moduleName),
							BehaviorType.MoveModuleUp);
	}

	public static Behavior GetMoveModuleDownBehavior(string moduleName) {
		return new Behavior(isReDo => GlobalData.HierarchyManager.MoveModuleDownBehavior(moduleName),
							isReUndo => GlobalData.HierarchyManager.MoveModuleUpBehavior(moduleName),
							BehaviorType.MoveModuleDown);
	}

	public static Behavior GetMoveDisplayObjectsUpBehavior(string moduleName, List<string> elementNames) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return null;
		if(elementNames == null || elementNames.Count == 0) return null;
		string firstElementName = GlobalData.CurrentDisplayObjects[0].name;
		int idx = elementNames.FindIndex(firstElementName.Equals);
		if(idx != -1) return null;
		return new Behavior(isReDo => DisplayObjectUtil.MoveDisplayObjectsUpBehavior(moduleName, elementNames),
							isReUndo => DisplayObjectUtil.MoveDisplayObjectsDownBehavior(moduleName, elementNames),
							BehaviorType.MoveSelectDisplayObjectsUp);
	}

	public static Behavior GetMoveDisplayObjectsDownBehavior(string moduleName, List<string> elementNames) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return null;
		if(elementNames == null || elementNames.Count == 0) return null;
		string lastElementName = GlobalData.CurrentDisplayObjects[GlobalData.CurrentDisplayObjects.Count - 1].name;
		int idx = elementNames.FindIndex(lastElementName.Equals);
		if(idx != -1) return null;
		return new Behavior(isReDo => DisplayObjectUtil.MoveDisplayObjectsDownBehavior(moduleName, elementNames),
							isReUndo => DisplayObjectUtil.MoveDisplayObjectsUpBehavior(moduleName, elementNames),
							BehaviorType.MoveSelectDisplayObjectsDown);
	}

	public static Behavior GetCopyDisplayObjectsBehavior(string moduleName, List<Element> copiedElements, bool needSelect = true, CombineType combineType = CombineType.Independent) {
		return new Behavior(isReDo => DisplayObjectUtil.CopySelectDisplayObjectsBehavior(moduleName, copiedElements, needSelect),
							isReUndo => DisplayObjectUtil.RemoveDisplayObjectsBehavior(moduleName, copiedElements),
							BehaviorType.CopyDisplayObjects,
							true,
							combineType);
	}

	public static Behavior GetUpdateFrameVisibleBehavior(bool isShow) {
		return new Behavior(isReDo => DisplayObjectUtil.UpdateFrameVisible(isShow),
							isReUndo => DisplayObjectUtil.UpdateFrameVisible(! isShow),
							BehaviorType.UpdateFrameVisible,
							false);
	}

	public static Behavior GetChangeModuleNameBehavior(string moduleName, string newModuleName) {
		return new Behavior(isRedo => ModuleUtil.ChangeModuleName(moduleName, newModuleName),
							isReUndo => ModuleUtil.ChangeModuleName(newModuleName, moduleName),
							BehaviorType.ChangeModuleName);
	}
}
