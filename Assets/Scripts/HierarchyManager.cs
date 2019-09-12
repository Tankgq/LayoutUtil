using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HierarchyManager : MonoBehaviour
{
	public Transform HierarchyListContainer;

	public InputField NameInputField;
	public Button UpButton;
	public Button DownButton;

	public InputField SearchInputField;

	private static string SearchText = null;
	private static bool IsGlobalSearchFlag = true;

	private static readonly List<Transform> DisplayObjectItemPool = new List<Transform>();
	private static readonly List<Transform> DisplayObjectItems = new List<Transform>();

	private static readonly List<Transform> ModuleItemPool = new List<Transform>();
	private static readonly List<Transform> ModuleItems = new List<Transform>();

	private void Start()
	{
		GlobalData.CurrentDisplayObjects.ObserveEveryValueChanged(displayObjects => displayObjects.Count)
				  .Subscribe(_ => RefreshDisplayObjectItem());
		Subject<object[]> updateSelectDisplayObjectSubject = MessageBroker.GetSubject(MessageBroker.UpdateSelectDisplayObject);
		updateSelectDisplayObjectSubject.SampleFrame(1)
										.Subscribe(_ => RefreshDisplayObjectItem());
		GlobalData.CurrentSelectDisplayObjectDic.ObserveEveryValueChanged(dic => dic.Count)
				  // .Subscribe(_ => RefreshDisplayObjectItem());
				  .Subscribe(_ => MessageBroker.Send(MessageBroker.UpdateSelectDisplayObject));
		UpButton.OnClickAsObservable()
				.Sample(TimeSpan.FromMilliseconds(100))
				.Subscribe(_ => RefreshDisplayObjectItem());
		DownButton.OnClickAsObservable()
				  .Sample(TimeSpan.FromMilliseconds(100))
				  .Subscribe(_ => RefreshDisplayObjectItem());

		SearchInputField.OnValueChangedAsObservable()
						.Where(txt => !txt.Equals(SearchText))
						.Sample(TimeSpan.FromMilliseconds(500))
						.Subscribe(txt =>
						{
							if (IsGlobalSearchFlag)
								GlobalData.CurrentModule = null;
							SearchText = txt;
							RefreshModuleItem();
						});

		GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.CurrentModule)
				  .Sample(TimeSpan.FromMilliseconds(100))
				  .Subscribe(_ => RefreshModuleItem());
		GlobalData.Modules.ObserveEveryValueChanged(moduleNames => moduleNames.Count)
				  .Sample(TimeSpan.FromMilliseconds(100))
				  .Subscribe(_ => RefreshModuleItem());
	}

	private static Transform GetDisplayObjectItem()
	{
		int length = DisplayObjectItemPool.Count;
		if (length == 0)
		{
			DisplayObjectItemPool.Add(Instantiate(GlobalData.DisplayObjectItemPrefab.transform,
												  GlobalData.HierarchyContainer.transform));
			length = 1;
		}
		Transform result = DisplayObjectItemPool[length - 1];
		DisplayObjectItemPool.RemoveAt(length - 1);
		return result;
	}

	private static void RecycleDisplayObject(Transform displayObjectItem)
	{
		if (!displayObjectItem) return;
		displayObjectItem.SetParent(null);
		displayObjectItem.GetChild(0).gameObject.SetActive(false);
		displayObjectItem.GetComponentInChildren<ImageHoverManager>().SimulatePointerExit();
		DisplayObjectItemPool.Add(displayObjectItem);
	}

	private static Transform GetModuleItem()
	{
		int length = ModuleItemPool.Count;
		if (length == 0) return Instantiate(GlobalData.ModuleItemPrefab.transform, GlobalData.HierarchyContainer.transform);
		Transform result = ModuleItemPool[length - 1];
		SwapImageManager swapImage = result.GetComponentInChildren<SwapImageManager>();
		swapImage.IsSwap = false;
		swapImage.StartObserveImageChange();
		result.GetChild(0).gameObject.SetActive(false);
		ModuleItemPool.RemoveAt(length - 1);
		return result;
	}

	private static void RecycleModuleItem(Transform moduleItem)
	{
		if (!moduleItem) return;
		moduleItem.SetParent(null);
		moduleItem.GetChild(0).gameObject.SetActive(false);
		SwapImageManager swapImage = moduleItem.GetComponentInChildren<SwapImageManager>();
		swapImage.StopObserveImageChange();
		swapImage.IsSwap = false;
		moduleItem.GetComponentInChildren<ImageHoverManager>().SimulatePointerExit();
		ModuleItemPool.Add(moduleItem);
	}

	private static void RecycleAllDisplayObjectItem()
	{
		foreach (Transform displayObjectItem in DisplayObjectItems)
			RecycleDisplayObject(displayObjectItem);
		DisplayObjectItems.Clear();
	}

	private static void RecycleAllModuleItem()
	{
		foreach (Transform moduleItem in ModuleItems)
			RecycleModuleItem(moduleItem);
		ModuleItems.Clear();
	}

	private static void ShowAllSeachedDisplayObjectItem()
	{
		if (string.IsNullOrWhiteSpace(SearchText)) return;
		if (!IsGlobalSearchFlag && string.IsNullOrWhiteSpace(GlobalData.CurrentModule))
			return;
		int count = GlobalData.Modules.Count;
		for (int idx = 0; idx < count; ++idx)
		{
			if (IsGlobalSearchFlag && ModuleItems[idx].name.IndexOf(SearchText) != -1)
				ModuleItems[idx].GetComponentInChildren<Text>().text = Utils.GetHighlight(ModuleItems[idx].name, SearchText);
			if (!IsGlobalSearchFlag && !GlobalData.Modules[idx].Equals(GlobalData.CurrentModule))
				continue;
			int silbingIndex = ModuleItems[idx].GetSiblingIndex();
			List<Element> displayObjects = GlobalData.ModuleDic[GlobalData.Modules[idx]];
			bool hasFind = false;
			int count2 = displayObjects.Count;
			for (int idx2 = 0; idx2 < count2; ++idx2)
			{
				Element displayObject = displayObjects[idx2];
				if (displayObject.Name.IndexOf(SearchText) == -1) continue;
				hasFind = true;
				Transform displayObjectItem = GetDisplayObjectItem();
				DisplayObjectItems.Add(displayObjectItem);
				displayObjectItem.SetParent(GlobalData.HierarchyContainer.transform);
				displayObjectItem.SetSiblingIndex(++silbingIndex);
				displayObjectItem.name = Utils.GetHighlight(displayObject.Name, SearchText);
				displayObjectItem.GetComponentInChildren<Text>().text = displayObjectItem.name;
			}
			if (hasFind)
			{
				SwapImageManager swapImage = ModuleItems[idx].GetComponentInChildren<SwapImageManager>();
				swapImage.IsSwap = true;
			}
		}
	}

	private static void RefreshDisplayObjectItem()
	{
		RecycleAllDisplayObjectItem();
		if (!string.IsNullOrWhiteSpace(SearchText))
		{
			ShowAllSeachedDisplayObjectItem();
			return;
		}
		if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
		int currentModuleIdx = ModuleItems.FindIndex(module => module.name.Equals(GlobalData.CurrentModule));
		if (currentModuleIdx == -1) return;
		SwapImageManager swapImage = ModuleItems[currentModuleIdx].GetComponentInChildren<SwapImageManager>();
		swapImage.IsSwap = true;
		swapImage.ForceUpdate();
		ModuleItems[currentModuleIdx].GetChild(0).gameObject.SetActive(true);
		int count = GlobalData.CurrentDisplayObjects.Count;
		for (int idx = 0; idx < count; ++idx)
		{
			Transform displayObjectItem = GetDisplayObjectItem();
			DisplayObjectItems.Add(displayObjectItem);
			displayObjectItem.SetParent(GlobalData.HierarchyContainer.transform);
			displayObjectItem.SetSiblingIndex(currentModuleIdx + idx + 1);
			displayObjectItem.name = GlobalData.CurrentDisplayObjects[idx].name;
			displayObjectItem.GetComponentInChildren<Text>().text = GlobalData.CurrentDisplayObjects[idx].name;
			SwapImageManager si = displayObjectItem.GetComponentInChildren<SwapImageManager>();
			if (si)
			{
				Element element = GlobalData.GetElement(displayObjectItem.name);
				if (element != null) si.IsSwap = !element.Visible;
				new Action<string, string>((module, name) =>
				{
					si.StartObserveImageChange(isSwap =>
					{
						if (string.IsNullOrEmpty(module) || !module.Equals(GlobalData.CurrentModule))
							return;
						Transform displayObject = GlobalData.CurrentDisplayObjectDic[name];
						displayObject.gameObject.SetActive(!isSwap);
						Element element2 = GlobalData.GetElement(name);
						element2.Visible = !isSwap;
					});
				})(GlobalData.CurrentModule, displayObjectItem.name);
			}
		}
		if (count == 0 || GlobalData.CurrentSelectDisplayObjectDic.Count == 0) return;
		foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
		{
			Transform displayObjectItem = DisplayObjectItems.Find(element => element.name.Equals(pair.Key));
			if (displayObjectItem == null) continue;
			displayObjectItem.GetChild(0).gameObject.SetActive(true);
		}
	}

	private void RefreshModuleItem()
	{
		if (!string.IsNullOrWhiteSpace(GlobalData.CurrentModule) && IsGlobalSearchFlag)
		{
			SearchText = string.Empty;
			SearchInputField.text = string.Empty;
		}
		RecycleAllDisplayObjectItem();
		RecycleAllModuleItem();
		int length = GlobalData.Modules.Count;
		for (var idx = 0; idx < length; ++idx)
		{
			Transform moduleItem = GetModuleItem();
			ModuleItems.Add(moduleItem);
			moduleItem.SetParent(GlobalData.HierarchyContainer.transform);
			moduleItem.name = GlobalData.Modules[idx];
			moduleItem.GetComponentInChildren<Text>().text = GlobalData.Modules[idx];
		}
		RefreshDisplayObjectItem();
	}

	public static bool InSearchMode()
	{
		return !string.IsNullOrEmpty(SearchText);
	}

	public void SwitchSearchMode(Text searchModeText)
	{
		if (searchModeText == null) return;
		IsGlobalSearchFlag = !IsGlobalSearchFlag;
		searchModeText.text = IsGlobalSearchFlag ? "G" : "L";
		RefreshModuleItem();
	}

	public static void UpdateDisplayObjectName(string originName, string newName)
	{
		if (string.IsNullOrWhiteSpace(originName) || string.IsNullOrWhiteSpace(newName)) return;
		Transform displayObjectItem = DisplayObjectItems.Find(item => item.name.Equals(originName));
		if (displayObjectItem == null) return;
		displayObjectItem.name = newName;
		displayObjectItem.GetComponentInChildren<Text>().text = newName;
	}

	public void MoveCurrentModuleUp()
	{
		if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
		int idx = GlobalData.Modules.FindIndex(0, name => GlobalData.CurrentModule.Equals(name));
		if (idx == -1 || idx == 0) return;
		List<string> moduleNames = GlobalData.Modules;
		string tmp = moduleNames[idx - 1];
		moduleNames[idx - 1] = moduleNames[idx];
		moduleNames[idx] = tmp;
		RefreshModuleItem();
	}

	public void MoveCurrentModuleDown()
	{
		if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
		int idx = GlobalData.Modules.FindIndex(0, name => GlobalData.CurrentModule.Equals(name));
		if (idx == -1 || idx == GlobalData.Modules.Count - 1) return;
		List<string> moduleNames = GlobalData.Modules;
		string tmp = moduleNames[idx + 1];
		moduleNames[idx + 1] = moduleNames[idx];
		moduleNames[idx] = tmp;
		RefreshModuleItem();
	}

	public static string GetModuleName(int displayObjectSiblingIndex)
	{
		if (displayObjectSiblingIndex < 1) return string.Empty;
		int count = ModuleItems.Count;
		if (count != GlobalData.Modules.Count) return string.Empty;
		for (int idx = 1; idx < count; ++idx)
		{
			int siblingIndex = ModuleItems[idx].GetSiblingIndex();
			if (siblingIndex > displayObjectSiblingIndex)
				return GlobalData.Modules[idx - 1];
		}
		return GlobalData.Modules[count - 1];
	}
}