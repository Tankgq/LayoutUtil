﻿using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class HierarchyManager : MonoBehaviour
    {
        public Transform HierarchyListContainer;

        public InputField NameInputField;
        public Button UpButton;
        public Button DownButton;

        public InputField SearchInputField;

        private static string SearchText = null;

        private static readonly List<Transform> DisplayObjectItemPool = new List<Transform>();
        private static readonly List<Transform> DisplayObjectItems = new List<Transform>();

        private static readonly List<Transform> ModuleItemPool = new List<Transform>();
        private static readonly List<Transform> ModuleItems = new List<Transform>();

        private void Start()
        {
            GlobalData.CurrentDisplayObjects.ObserveEveryValueChanged(displayObjects => displayObjects.Count)
                .Subscribe(_ => RefreshDisplayObjectItem());
            GlobalData.CurrentSelectDisplayObjectDic.ObserveEveryValueChanged(dic => dic.Count)
                .Subscribe(_ => RefreshDisplayObjectItem());
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
            if (length == 0) return Instantiate(GlobalData.DisplayObjectItemPrefab.transform, GlobalData.HierarchyContainer.transform);
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
            swapImage.StartObserveImageChange();
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
            int count = GlobalData.Modules.Count;
            for (int idx = 0; idx < count; ++idx)
            {
                if (ModuleItems[idx].name.IndexOf(SearchText) != -1)
                    ModuleItems[idx].GetComponentInChildren<Text>().text = Utils.GetHighlight(ModuleItems[idx].name, SearchText);
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
                    swapImage.ForceUpdate();
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
            for (var idx = 0; idx < count; ++idx)
            {
                Transform displayObjectItem = GetDisplayObjectItem();
                DisplayObjectItems.Add(displayObjectItem);
                displayObjectItem.SetParent(GlobalData.HierarchyContainer.transform);
                displayObjectItem.SetSiblingIndex(currentModuleIdx + idx + 1);
                displayObjectItem.name = GlobalData.CurrentDisplayObjects[idx].name;
                displayObjectItem.GetComponentInChildren<Text>().text = GlobalData.CurrentDisplayObjects[idx].name;
            }
            if (count == 0 || GlobalData.CurrentSelectDisplayObjectDic.Count == 0) return;
            foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
            {
                int idx = DisplayObjectItems.FindIndex(element => element.name.Equals(pair.Value.name));
                if (idx < 0 || idx >= count) continue;
                DisplayObjectItems[idx].GetChild(0).gameObject.SetActive(true);
            }
        }

        private void RefreshModuleItem()
        {
            if (!string.IsNullOrWhiteSpace(GlobalData.CurrentModule))
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
}