﻿using System;
using System.Collections.Generic;
using System.Linq;
using FarPlane;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HierarchyManager : MonoBehaviour {
	public Button upButton;
	public Button downButton;

	public InputField searchInputField;

	private static string _searchText;
	private static bool _isGlobalSearchFlag = true;

	private static readonly List<Transform> DisplayObjectItemPool = new List<Transform>();
	private static readonly List<Transform> DisplayObjectItems = new List<Transform>();

	private static readonly List<Transform> ModuleItemPool = new List<Transform>();
	private static readonly List<Transform> ModuleItems = new List<Transform>();

	private void Start() {
		GlobalData.CurrentDisplayObjects
				  .ObserveEveryValueChanged(displayObjects => displayObjects.Count)
				  .Subscribe(_ => RefreshDisplayObjectItem());
		UlEventSystem.GetSubject<DataEventType, SelectedChangeData>(DataEventType.SelectedChange)
					 .SampleFrame(1)
					 .Subscribe(_ => RefreshModuleItem());
		UlEventSystem.GetTriggerSubject<UIEventType>(UIEventType.UpdateHierarchy)
					 .Sample(TimeSpan.FromMilliseconds(100))
					 .Subscribe(_ => RefreshDisplayObjectItem());

		searchInputField.OnValueChangedAsObservable()
						.Where(txt => ! txt.Equals(_searchText))
						.Sample(TimeSpan.FromMilliseconds(500))
						.Subscribe(txt => {
							 if(_isGlobalSearchFlag)
								 UlEventSystem.Dispatch<DataEventType, ChangeModuleEventData>(DataEventType.ChangeModule, new ChangeModuleEventData(null));
							 _searchText = txt;
							 RefreshModuleItem();
						 });

		UlEventSystem.GetTriggerSubject<DataEventType>(DataEventType.ChangeModule)
					 .Sample(TimeSpan.FromMilliseconds(100))
					 .Subscribe(_ => RefreshModuleItem());
		UlEventSystem.GetTriggerSubject<UIEventType>(UIEventType.RefreshModuleItem)
					 .Subscribe(_ => RefreshModuleItem());
		StartObserveSwapImage();
	}

	private static Transform GetDisplayObjectItem() {
		int length = DisplayObjectItemPool.Count;
		if(length == 0) {
			DisplayObjectItemPool.Add(Instantiate(GlobalData.DisplayObjectItemPrefab.transform, GlobalData.HierarchyContainer.transform));
			length = 1;
		}

		Transform result = DisplayObjectItemPool[length - 1];
		DisplayObjectItemPool.RemoveAt(length - 1);
		return result;
	}

	private static void RecycleDisplayObject(Transform displayObjectItem) {
		if(! displayObjectItem) return;
		displayObjectItem.SetParent(null);
		displayObjectItem.GetChild(0).gameObject.SetActive(false);
		displayObjectItem.GetComponentInChildren<ImageHoverManager>().SimulatePointerExit();
		DisplayObjectItemPool.Add(displayObjectItem);
	}

	private static Transform GetModuleItem() {
		int length = ModuleItemPool.Count;
		if(length == 0) return Instantiate(GlobalData.ModuleItemPrefab.transform, GlobalData.HierarchyContainer.transform);
		Transform result = ModuleItemPool[length - 1];
		SwapImageManager swapImage = result.GetComponentInChildren<SwapImageManager>();
		swapImage.UpdateSwapImage(false);
		result.GetChild(0).gameObject.SetActive(false);
		ModuleItemPool.RemoveAt(length - 1);
		return result;
	}

	private static void RecycleModuleItem(Transform moduleItem) {
		if(! moduleItem) return;
		moduleItem.SetParent(null);
		moduleItem.GetChild(0).gameObject.SetActive(false);
		SwapImageManager swapImage = moduleItem.GetComponentInChildren<SwapImageManager>();
		swapImage.UpdateSwapImage(false);
		moduleItem.GetComponentInChildren<ImageHoverManager>().SimulatePointerExit();
		ModuleItemPool.Add(moduleItem);
	}

	private static void RecycleAllDisplayObjectItem() {
		foreach(Transform displayObjectItem in DisplayObjectItems) RecycleDisplayObject(displayObjectItem);
		DisplayObjectItems.Clear();
	}

	private static void RecycleAllModuleItem() {
		foreach(Transform moduleItem in ModuleItems) RecycleModuleItem(moduleItem);
		ModuleItems.Clear();
	}

	private static void ShowAllSearchedDisplayObjectItem() {
		if(string.IsNullOrWhiteSpace(_searchText)) return;
		if(! _isGlobalSearchFlag && string.IsNullOrWhiteSpace(GlobalData.CurrentModule)) return;
		int count = GlobalData.Modules.Count;
		for(int idx = 0; idx < count; ++ idx) {
			if(_isGlobalSearchFlag && ModuleItems[idx].name.IndexOf(_searchText, StringComparison.Ordinal) != -1)
				ModuleItems[idx].GetComponentInChildren<Text>().text = ModuleItems[idx].name.HighlightText(_searchText);
			if(! _isGlobalSearchFlag && ! GlobalData.Modules[idx].Equals(GlobalData.CurrentModule)) continue;
			int siblingIndex = ModuleItems[idx].GetSiblingIndex();
			List<Element> displayObjects = GlobalData.ModuleDic[GlobalData.Modules[idx]];
			bool hasFind = false;
			int count2 = displayObjects.Count;
			for(int idx2 = 0; idx2 < count2; ++ idx2) {
				Element displayObject = displayObjects[idx2];
				if(displayObject.Name.IndexOf(_searchText, StringComparison.Ordinal) == -1) continue;
				hasFind = true;
				Transform displayObjectItem = GetDisplayObjectItem();
				DisplayObjectItems.Add(displayObjectItem);
				displayObjectItem.SetParent(GlobalData.HierarchyContainer.transform);
				displayObjectItem.SetSiblingIndex(++ siblingIndex);
				displayObjectItem.name = displayObject.Name.HighlightText(_searchText);
				displayObjectItem.GetComponentInChildren<Text>().text = displayObjectItem.name;
			}

			if(! hasFind) continue;
			SwapImageManager swapImage = ModuleItems[idx].GetComponentInChildren<SwapImageManager>();
			swapImage.UpdateSwapImage(true);
		}
	}

	private static void RefreshDisplayObjectItem() {
		RecycleAllDisplayObjectItem();
		if(! string.IsNullOrWhiteSpace(_searchText)) {
			ShowAllSearchedDisplayObjectItem();
			return;
		}

		if(string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
		int currentModuleIdx = ModuleItems.FindIndex(module => module.name.Equals(GlobalData.CurrentModule));
		if(currentModuleIdx == -1) return;
		SwapImageManager swapImage = ModuleItems[currentModuleIdx].GetComponentInChildren<SwapImageManager>();
		swapImage.UpdateSwapImage(true);
		ModuleItems[currentModuleIdx].GetChild(0).gameObject.SetActive(true);
		int count = GlobalData.CurrentDisplayObjects.Count;
		for(int idx = 0; idx < count; ++ idx) {
			Transform displayObjectItem = GetDisplayObjectItem();
			DisplayObjectItems.Add(displayObjectItem);
			displayObjectItem.SetParent(GlobalData.HierarchyContainer.transform);
			displayObjectItem.SetSiblingIndex(currentModuleIdx + idx + 1);
			displayObjectItem.name = GlobalData.CurrentDisplayObjects[idx].name;
			displayObjectItem.GetComponentInChildren<Text>().text = GlobalData.CurrentDisplayObjects[idx].name;
			SwapImageManager sim = displayObjectItem.GetComponentInChildren<SwapImageManager>();
			if(! sim) continue;
			Element element = GlobalData.GetElement(displayObjectItem.name);
			if(element != null) sim.UpdateSwapImage(! element.Visible);
		}

		if(count == 0 || GlobalData.CurrentSelectDisplayObjectDic.Count == 0) return;
		foreach(Transform displayObjectItem in GlobalData.CurrentSelectDisplayObjectDic.Select(pair => DisplayObjectItems.Find(element => element.name.Equals(pair.Key)))
														 .Where(displayObjectItem => displayObjectItem != null)) {
			displayObjectItem.GetChild(0).gameObject.SetActive(true);
		}
	}

	private void RefreshModuleItem() {
		if(! string.IsNullOrWhiteSpace(GlobalData.CurrentModule) && _isGlobalSearchFlag) {
			_searchText = string.Empty;
			searchInputField.text = string.Empty;
		}

		RecycleAllDisplayObjectItem();
		RecycleAllModuleItem();
		int length = GlobalData.Modules.Count;
		for(int idx = 0; idx < length; ++ idx) {
			Transform moduleItem = GetModuleItem();
			ModuleItems.Add(moduleItem);
			moduleItem.SetParent(GlobalData.HierarchyContainer.transform);
			moduleItem.name = GlobalData.Modules[idx];
			moduleItem.position = Vector3.zero;
			moduleItem.GetComponentInChildren<Text>().text = GlobalData.Modules[idx];
		}

		RefreshDisplayObjectItem();
	}

	public static bool InSearchMode() {
		return ! string.IsNullOrEmpty(_searchText);
	}

	public void SwitchSearchMode(Text searchModeText) {
		if(searchModeText == null) return;
		_isGlobalSearchFlag = ! _isGlobalSearchFlag;
		searchModeText.text = _isGlobalSearchFlag ? "G" : "L";
		RefreshModuleItem();
	}

	public void MoveModuleUpBehavior(string moduleName) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		int idx = GlobalData.Modules.FindIndex(0, moduleName.Equals);
		if(idx == -1 || idx == 0) return;
		List<string> moduleNames = GlobalData.Modules;
		string tmp = moduleNames[idx - 1];
		moduleNames[idx - 1] = moduleNames[idx];
		moduleNames[idx] = tmp;
		RefreshModuleItem();
	}

	public void MoveModuleDownBehavior(string moduleName) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		int idx = GlobalData.Modules.FindIndex(0, moduleName.Equals);
		if(idx == -1 || idx == GlobalData.Modules.Count - 1) return;
		List<string> moduleNames = GlobalData.Modules;
		string tmp = moduleNames[idx + 1];
		moduleNames[idx + 1] = moduleNames[idx];
		moduleNames[idx] = tmp;
		RefreshModuleItem();
	}

	public static string GetModuleName(int displayObjectSiblingIndex) {
		if(displayObjectSiblingIndex < 1) return string.Empty;
		int count = ModuleItems.Count;
		if(count != GlobalData.Modules.Count) return string.Empty;
		for(int idx = 1; idx < count; ++ idx) {
			int siblingIndex = ModuleItems[idx].GetSiblingIndex();
			if(siblingIndex > displayObjectSiblingIndex) return GlobalData.Modules[idx - 1];
		}

		return GlobalData.Modules[count - 1];
	}

	private static Transform GetDisplayObjectItem(string moduleName, string elementName) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! GlobalData.CurrentModule.Equals(moduleName)) return null;
		return DisplayObjectItems.Find(item => item.name.Equals(elementName));
	}

	private static void StartObserveSwapImage() {
		Subject<SwapImageEventData> subject = UlEventSystem.TryToGetSubject<UIEventType, SwapImageEventData>(UIEventType.SwapImage);
		if(subject != null) return;
		UlEventSystem.GetSubject<UIEventType, SwapImageEventData>(UIEventType.SwapImage)
					 .Subscribe(eventData => {
						  if(eventData == null) return;
						  if(string.IsNullOrWhiteSpace(eventData.ModuleName) || ! eventData.ModuleName.Equals(GlobalData.CurrentModule)) return;
						  if(string.IsNullOrWhiteSpace(eventData.ElementName)) return;
						  Transform displayObject = GlobalData.CurrentDisplayObjectDic[eventData.ElementName];
						  if(displayObject) displayObject.gameObject.SetActive(! eventData.IsSwap);
						  Transform item = GetDisplayObjectItem(eventData.ModuleName, eventData.ElementName);
						  if(item) {
							  SwapImageManager sim = item.GetComponentInChildren<SwapImageManager>();
							  if(sim) sim.UpdateSwapImage(eventData.IsSwap);
						  }
						  Element element = GlobalData.GetElement(eventData.ElementName);
						  if(element != null) element.Visible = ! eventData.IsSwap;
						});
	}
}
