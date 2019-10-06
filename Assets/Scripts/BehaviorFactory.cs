using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BehaviorFactory {
	public static Behavior GetCreateModuleBehavior(string moduleName) {
		return new Behavior(isReDo => ModuleUtil.CreateModuleBehavior(moduleName,
																	  null,
																	  true,
																	  isReDo),
							isReUndo => ModuleUtil.RemoveModuleBehavior(moduleName),
							Behavior.BehaviorType.CreateModule);
	}

	public static Behavior GetRemoveModuleBehavior(string moduleName) {
		return new Behavior(isReDo => ModuleUtil.RemoveModuleBehavior(moduleName),
							isReUndo => ModuleUtil.CreateModuleBehavior(moduleName),
							Behavior.BehaviorType.RemoveModule);
	}

	public static Behavior GetRemoveAllModuleBehavior(List<string> modules,
													  bool combineWithNextBehavior =
															  false) {
		return new Behavior(isRedo => {
								int count = modules.Count;
								for(int idx = 0; idx < count; ++ idx) ModuleUtil.RemoveModuleBehavior(modules[idx]);
							},
							isReUndo => {
								int count = modules.Count;
								for(int idx = 0; idx < count; ++ idx) ModuleUtil.CreateModuleBehavior(modules[idx], null, false);
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

	public static Behavior GetOpenModuleBehavior(string moduleName) {
		string previousModuleName = GlobalData.CurrentModule;
		return new Behavior(isRedo => GlobalData.CurrentModule = moduleName,
							isReUndo => GlobalData.CurrentModule = previousModuleName,
							Behavior.BehaviorType.OpenModule);
	}

	public static Behavior GetAddDisplayObjectBehavior(string moduleName,
													   string elementName,
													   string imageUrl,
													   Vector2 pos,
													   Vector2 size) {
		Element element = new Element {
			Name = elementName,
			X = Element.ConvertX(pos.x),
			Y = Element.ConvertY(pos.y),
			Width = size.x,
			Height = size.y,
			Visible = true
		};
		return new Behavior(isRedo =>
									DisplayObjectUtil.AddDisplayObjectBehavior(moduleName,
																			   element,
																			   imageUrl),
							isReUndo =>
									DisplayObjectUtil.RemoveDisplayObjectBehavior(moduleName,
																				  element.Name),
							Behavior.BehaviorType.AddDisplayObject);
	}

	public static Behavior GetLoadImageToDisplayObjectBehavior(string moduleName,
															   string elementName,
															   string imageUrl,
															   bool isModify = true) {
		return new Behavior(isReDo =>
									DisplayObjectUtil.LoadImageBehavior(moduleName,
																		elementName,
																		imageUrl),
							isReUndo =>
									DisplayObjectUtil.RemoveImageBehavior(moduleName, elementName),
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
		return new Behavior(isRedo => DisplayObjectUtil.RemoveDisplayObjectsBehavior(moduleName, elementNames),
							isReUndo => {
								DisplayObjectUtil.AddDisplayObjectsBehavior(moduleName, elements);
								foreach(string elementName in elementNames) {
									Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
									GlobalData.CurrentSelectDisplayObjectDic.Add(elementName, displayObject);
								}
								MessageBroker.Send(MessageBroker.Code.UpdateSelectDisplayObjectDic, elementNames);
							},
							Behavior.BehaviorType.RemoveSelectedDisplayObject);
	}

	public static Behavior GetUpdateDisplayObjectsPosBehavior(string moduleName, IReadOnlyList<string> elementNames, Vector2 originPos, Vector2 targetPos) {
		return new Behavior(isReDo => DisplayObjectUtil.UpdateDisplayObjectsPosition(moduleName, elementNames, targetPos),
							isReUndo => DisplayObjectUtil.UpdateDisplayObjectsPosition(moduleName, elementNames, originPos),
							Behavior.BehaviorType.UpdateDisplayObjectsPos);
	}

	public static Behavior GetUpdateSwapImageBehavior(string moduleName, string elementName, bool isSwap) {
		return new Behavior(isRedo => MessageBroker.Send(MessageBroker.Code.UpdateSwapImage, moduleName, elementName, isSwap),
							isReUndo => MessageBroker.Send(MessageBroker.Code.UpdateSwapImage, moduleName, elementName, ! isSwap),
							Behavior.BehaviorType.UpdateSwapImage);
	}

	public static Behavior GetChangeNameBehavior(string moduleName, string originName, string newName) {
		return new Behavior(isRedo => DisplayObjectUtil.ChangeNameBehavior(moduleName, originName, newName),
							isReUndo => DisplayObjectUtil.ChangeNameBehavior(moduleName, newName, originName),
							Behavior.BehaviorType.ChangeName);
	}

	public static Behavior GetChangeXBehavior(string moduleName, List<string> elementNames, float originX, float newX, bool isAdd = false) {
		return new Behavior(isRedo => DisplayObjectUtil.ChangeXBehavior(moduleName, elementNames, newX, isAdd),
							isReUndo => DisplayObjectUtil.ChangeXBehavior(moduleName, elementNames, originX, isAdd),
							Behavior.BehaviorType.ChangeX);
	}

	public static Behavior GetChangeYBehavior(string moduleName, List<string> elementNames, float originY, float newY, bool isAdd = false) {
		return new Behavior(isRedo => DisplayObjectUtil.ChangeYBehavior(moduleName, elementNames, newY, isAdd),
							isReUndo => DisplayObjectUtil.ChangeYBehavior(moduleName, elementNames, originY, isAdd),
							Behavior.BehaviorType.ChangeY);
	}

	public static Behavior GetChangeWidthBehavior(string moduleName, List<string> elementNames, float originWidth, float newWidth, bool isAdd = false) {
		return new Behavior(isRedo => DisplayObjectUtil.ChangeWidthBehavior(moduleName, elementNames, newWidth, isAdd),
							isReUndo => DisplayObjectUtil.ChangeWidthBehavior(moduleName, elementNames, originWidth, isAdd),
							Behavior.BehaviorType.ChangeWidth);
	}

	public static Behavior GetChangeHeightBehavior(string moduleName, List<string> elementNames, float originHeight, float newHeight, bool isAdd = false) {
		return new Behavior(isRedo => DisplayObjectUtil.ChangeHeightBehavior(moduleName, elementNames, newHeight, isAdd),
							isReUndo => DisplayObjectUtil.ChangeHeightBehavior(moduleName, elementNames, originHeight, isAdd),
							Behavior.BehaviorType.ChangeHeight);
	}
}
