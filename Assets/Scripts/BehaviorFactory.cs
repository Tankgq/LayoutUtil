using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
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
								for(int idx = 0; idx < count; ++ idx)
									ModuleUtil.RemoveModuleBehavior(modules[idx]);
							},
							isReUndo => {
								int count = modules.Count;
								for(int idx = 0; idx < count; ++ idx)
									ModuleUtil.CreateModuleBehavior(modules[idx], null, false);
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

	public static Behavior GetAddDisplayObjectBehavior(string  moduleName,
													   string  elementName,
													   string  imageUrl,
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

	public static Behavior GetLoadImageToDisplayObjectBehavior(
		string moduleName,
		string elementName,
		string imageUrl,
		bool   isModify = true) {
		return new Behavior(isReDo =>
									DisplayObjectUtil.LoadImageBehavior(moduleName,
																		elementName,
																		imageUrl),
							isReUndo =>
									DisplayObjectUtil.RemoveImageBehavior(moduleName, elementName),
							Behavior.BehaviorType.LoadImageToDisplayObject,
							isModify);
	}

	public static Behavior GetUpdateSelectDisplayObjectDicBehavior(
		string       moduleName,
		List<string> addElements,
		List<string> removeElements) {
		return new Behavior();
	}

	public static Behavior GetRemoveSelectedDisplayObjectBehavior(string moduleName) {
		List<Element> elements = GlobalData
								.CurrentSelectDisplayObjectDic
								.Select(pair => GlobalData.GetElement(pair.Key))
								.ToList();
		return new Behavior(isRedo => {
								foreach(Element element in elements)
									DisplayObjectUtil.RemoveDisplayObjectBehavior(moduleName,
																				  element.Name);
							},
							isReUndo => {
								foreach(Element element in elements) {
									DisplayObjectUtil.AddDisplayObjectBehavior(moduleName, element);
									Transform displayObject = GlobalData.CurrentDisplayObjectDic[element.Name];
									GlobalData.CurrentSelectDisplayObjectDic.Add(element.Name,
																				 displayObject);
								}
							}, Behavior.BehaviorType.RemoveSelectedDisplayObject);                                                                                                                                                                                                                     
	}
}
