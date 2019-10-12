using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;

public static class ModuleUtil {
	public static bool CreateModuleBehavior(string moduleName, List<Element> elements = null, bool selectModule = true, bool needCheckCache = false) {
		if(string.IsNullOrWhiteSpace(moduleName)) return false;
		if(GlobalData.ModuleDic.ContainsKey(moduleName)) return false;
		// 还原 module 时尝试取回删除时保存的数据
		if(needCheckCache && elements == null) GlobalData.CacheModuleDic.TryGetValue(moduleName, out elements);
		if(elements == null) elements = new List<Element>();
		GlobalData.ModuleDic[moduleName] = elements;
		GlobalData.Modules.Add(moduleName);
		if(selectModule) GlobalData.CurrentModule = moduleName;
		return true;
	}

	public static bool RemoveModuleBehavior(string moduleName) {
		if(string.IsNullOrWhiteSpace(moduleName)) return false;
		if(! GlobalData.ModuleDic.ContainsKey(moduleName)) return false;
		// 删除时将数据保存起来, 避免还原时没有数据
		GlobalData.CacheModuleDic[moduleName] = GlobalData.ModuleDic[moduleName];
		GlobalData.Modules.Remove(moduleName);
		GlobalData.ModuleDic.Remove(moduleName);
		if(moduleName.Equals(GlobalData.CurrentModule)) GlobalData.CurrentModule = null;
		return true;
	}

	private static void RemoveCurrentModule() {
		HistoryManager.Do(BehaviorFactory.GetRemoveModuleBehavior(GlobalData.CurrentModule));
	}

	public static void CreateModule() {
		DialogManager.ShowGetValue("请输入 module 名:",
								   "module",
								   txt => {
									   if(string.IsNullOrWhiteSpace(txt)) {
										   QuickTipManager.ShowQuickTip("请输入正确的 module");
										   // DialogManager.ShowError("请输入正确的 module", KeyCode.Return, 0, 0);
										   return;
									   }

									   if(GlobalData.ModuleDic.ContainsKey(txt)) {
										   QuickTipManager.ShowQuickTip("module 已存在");
										   // DialogManager.ShowError("module 已存在", KeyCode.Return, 0, 0);
										   return;l
									   }

									   HistoryManager.Do(BehaviorFactory.GetCreateModuleBehavior(txt));
								   });
	}

	public static void ExportCurrentModule() {
		if(string.IsNullOrWhiteSpace(GlobalData.CurrentModule)) {
			DialogManager.ShowWarn("请先打开一个 module");
			return;
		}

		ContainerManager.UpdateCurrentDisplayObjectData();
		Module module = new Module {Name = GlobalData.CurrentModule};
		module.Elements = GlobalData.ModuleDic[module.Name];
		string jsonString = JsonConvert.SerializeObject(module, Formatting.Indented);
		GUIUtility.systemCopyBuffer = jsonString;
		QuickTipManager.ShowQuickTip("已导出到剪切板");
	}

	public static void CheckExportModules() {
		if(GlobalData.Modules.Count == 0) {
			QuickTipManager.ShowQuickTip("当前没有任何 module, 导出失败");
			return;
		}

		string filePath = SaveFileUtil.SaveFile("json 文件(*.json)\0*.json");
		ExportModules(filePath);
	}

	private static void RemoveAllModules(bool combineWithNextBehavior = false) {
		if(GlobalData.Modules.Count == 0) return;
		List<string> modules = new List<string>();
		modules.AddRange(GlobalData.Modules);
		HistoryManager.Do(BehaviorFactory.GetRemoveAllModuleBehavior(modules, combineWithNextBehavior));
	}

	public static void CheckRemoveCurrentModule() {
		if(string.IsNullOrEmpty(GlobalData.CurrentModule)) {
			CheckRemoveAllModules();
			return;
		}

		DialogManager.ShowQuestion($"是否删除当前打开的 module: {GlobalData.CurrentModule}",
								   RemoveCurrentModule,
								   null);
	}

	private static void CheckRemoveAllModules() {
		if(GlobalData.Modules.Count == 0) {
			DialogManager.ShowInfo("当前没有任何 module 可以删除");
			return;
		}

		DialogManager.ShowQuestion("是否删除所有 module.", () => RemoveAllModules(), null);
	}

	public static void ExportModules(string filePath, bool showQuickTip = false) {
		if(string.IsNullOrWhiteSpace(filePath)) return;
		ContainerManager.UpdateCurrentDisplayObjectData();
		List<Module> modules = new List<Module>();
		int count = GlobalData.Modules.Count;
		for(int idx = 0; idx < count; ++ idx) {
			Module module = new Module {Name = GlobalData.Modules[idx]};
			module.Elements = GlobalData.ModuleDic[module.Name];
			Rectangle rect = DisplayObjectUtil.GetMinRectangleContainsDisplayObjects(module.Elements);
			if(rect != null) {
				module.X = rect.X;
				module.Y = rect.Y;
				module.Width = rect.Width;
				module.Height = rect.Height;
			}

			modules.Add(module);
		}

		string jsonString = JsonConvert.SerializeObject(modules, Formatting.Indented);
		bool result = Utils.WriteFile(filePath, Encoding.UTF8.GetBytes(jsonString));
		if(result) {
			string message = $"成功导出到 {filePath}";
			if(showQuickTip)
				QuickTipManager.ShowQuickTip(message);
			else
				DialogManager.ShowInfo(message);
			GlobalData.CurrentFilePath = filePath;
			GlobalData.ModifyCount = 0;
		} else {
			const string message = "导出失败";
			if(showQuickTip)
				QuickTipManager.ShowQuickTip(message);
			else
				DialogManager.ShowError(message, 0, 0);
		}
	}

	public static void CheckImportModules() {
		string filePath = OpenFileUtil.OpenFile("json 文件(*.json)\0*.json");
		if(string.IsNullOrEmpty(filePath)) return;
		if(GlobalData.Modules.Count == 0) {
			ImportModules(filePath);
			return;
		}

		DialogManager.ShowQuestion("导入时会先将所有 modules 都删除, 是否继续导入?",
								   () => ImportModules(filePath),
								   null,
								   "确定",
								   "取消",
								   KeyCode.Return,
								   KeyCode.Escape,
								   0,
								   165);
	}

	private static void ImportModules(string filePath) {
		byte[] bytes = Utils.ReadFile(filePath);
		string jsonStr = Encoding.UTF8.GetString(bytes);
		RemoveAllModules(true);
		Observable.Timer(TimeSpan.Zero)
				  .Subscribe(_ => {
					   try {
						   List<Module> modules = JsonConvert.DeserializeObject<List<Module>>(jsonStr);
						   HistoryManager.Do(BehaviorFactory.GetImportModulesBehavior(filePath, modules));
					   } catch(Exception e) {
						   DialogManager.ShowError($"导入失败({e})");
					   }
				   });
	}

	public static void OpenModule(string moduleName) {
		HistoryManager.Do(BehaviorFactory.GetOpenModuleBehavior(moduleName));
	}
}
