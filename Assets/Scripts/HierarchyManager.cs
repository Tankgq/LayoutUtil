using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HierarchyManager : MonoBehaviour
{
    public Transform HierarchyListContainer;

    public InputField NameInputField;
    public Button UpButton;
    public Button DownButton;

    private static readonly List<Transform> DisplayObjectItemPool = new List<Transform>();
    private readonly List<Transform> DisplayObjectItems = new List<Transform>();

    private static readonly List<Transform> ModuleItemPool = new List<Transform>();
    private readonly List<Transform> ModuleItems = new List<Transform>();

    private void Start()
    {
        GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.CurrentDisplayObjects == null ? 0 : GlobalData.CurrentDisplayObjects.Count)
            .Concat(GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.CurrentDisplayObjects).Select(_ => 1),
                    GlobalData.CurrentSelectDisplayObjectDic.ObserveEveryValueChanged(dic => dic.Count),
                    UpButton.OnClickAsObservable().Select(_ => 0),
                    DownButton.OnClickAsObservable().Select(_ => 0))
            .Sample(TimeSpan.FromMilliseconds(100))
            .Subscribe(_ => RefreshDisplayObjectItem());
        //        GlobalData.CurrentDisplayObjects.ObserveEveryValueChanged(displayObjects => displayObjects.Count)
        //            .Subscribe(_ => RefreshDisplayObjectItem());
        //        GlobalData.CurrentSelectDisplayObjectDic.ObserveEveryValueChanged(dic => dic.Count)
        //            .Subscribe(_ => RefreshDisplayObjectItem());
        //        UpButton.OnClickAsObservable()
        //            .Sample(TimeSpan.FromSeconds(1))
        //            .Subscribe(_ => RefreshDisplayObjectItem());
        //        DownButton.OnClickAsObservable()
        //            .Sample(TimeSpan.FromSeconds(1))
        //            .Subscribe(_ => RefreshDisplayObjectItem());

        NameInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocused => ! isFocused && !string.IsNullOrEmpty(NameInputField.text))
            .Subscribe(_ =>
            {
                if (GlobalData.CurrentSelectDisplayObjectDic.Count != 1) return;
                Transform displayObject = GlobalData.CurrentSelectDisplayObjectDic.First().Value;
                int idx = DisplayObjectItems.FindIndex(element => element.name.Equals(displayObject.name));
                if (idx < 0 || idx >= DisplayObjectItems.Count) return;
                DisplayObjectItems[idx].GetComponentInChildren<Text>().text = NameInputField.text;
            });
        GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.CurrentModule)
            .Concat(GlobalData.ModuleNames.ObserveEveryValueChanged(moduleNames => moduleNames.Count).Select(_ => string.Empty))
            .Sample(TimeSpan.FromMilliseconds(100))
            .Subscribe(_ => RefreshModuleItem());

//        GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.CurrentModule)
//                                   .Subscribe(_ => RefreshModuleItem());
//        GlobalData.ModuleNames.ObserveEveryValueChanged(moduleNames => moduleNames.Count)
//            .Subscribe(_ => RefreshModuleItem());
    }

    private Transform GetDisplayObjectItem()
    {
        int length = DisplayObjectItemPool.Count;
        if(length == 0) return Instantiate(GlobalData.DisplayObjectItemPrefab.transform, HierarchyListContainer);
        Transform result = DisplayObjectItemPool[length - 1];
        DisplayObjectItemPool.RemoveAt(length - 1);
        return result;
    }

    private static void RecycleDisplayObject(Transform displayObjectItem)
    {
        if (!displayObjectItem) return;
        displayObjectItem.SetParent(null);
        displayObjectItem.GetChild(0).gameObject.SetActive(false);
        DisplayObjectItemPool.Add(displayObjectItem);
    }

    private Transform GetModuleItem()
    {
        int length = ModuleItemPool.Count;
        if(length == 0) return Instantiate(GlobalData.ModuleItemPrefab.transform, HierarchyListContainer);
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
        ModuleItemPool.Add(moduleItem);
    }

    private void RecycleAllDisplayObjectItem()
    {
        foreach(Transform displayObjectItem in DisplayObjectItems)
            RecycleDisplayObject(displayObjectItem);
        DisplayObjectItems.Clear();
    }

    private void RecycleAllModuleItem()
    {
        foreach (Transform moduleItem in ModuleItems)
            RecycleModuleItem(moduleItem);
        ModuleItems.Clear();
    }

    private void RefreshDisplayObjectItem() {
        RecycleAllDisplayObjectItem();
        if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
        int currentModuleIdx = ModuleItems.FindIndex(module => module.name.Equals(GlobalData.CurrentModule));
        if (currentModuleIdx == -1) return;
        ModuleItems[currentModuleIdx].GetComponentInChildren<SwapImageManager>().IsSwap = true;
        ModuleItems[currentModuleIdx].GetChild(0).gameObject.SetActive(true);
        int count = GlobalData.CurrentDisplayObjects.Count;
        for (var idx = 0; idx < count; ++idx) {
            Transform displayObjectItem = GetDisplayObjectItem();
            DisplayObjectItems.Add(displayObjectItem);
            displayObjectItem.SetParent(HierarchyListContainer);
            displayObjectItem.SetSiblingIndex(currentModuleIdx + idx + 1);
            displayObjectItem.name = GlobalData.CurrentDisplayObjects[idx].name;
            displayObjectItem.GetComponentInChildren<Text>().text = GlobalData.CurrentDisplayObjects[idx].name;
        }
        int length = DisplayObjectItems.Count;
        if (length == 0 || GlobalData.CurrentSelectDisplayObjectDic.Count == 0) return;
        foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic) {
            int idx = DisplayObjectItems.FindIndex(element => element.name == pair.Value.name);
            if (idx < 0 || idx >= length) continue;
            DisplayObjectItems[idx].GetChild(0).gameObject.SetActive(true);
        }
    }

    private void RefreshModuleItem()
    {
        RecycleAllDisplayObjectItem();
        RecycleAllModuleItem();
        int length = GlobalData.ModuleNames.Count;
        for (var idx = 0; idx < length; ++idx)
        {
            Transform moduleItem = GetModuleItem();
            ModuleItems.Add(moduleItem);
            moduleItem.SetParent(HierarchyListContainer);
            moduleItem.name = GlobalData.ModuleNames[idx];
            moduleItem.GetComponentInChildren<Text>().text = GlobalData.ModuleNames[idx];
        }
        RefreshDisplayObjectItem();
    }

//    private static void OnModuleStateChanged(Transform moduleItem, bool isOn)
//    {
//        Debug.Log($"moduleItem: {moduleItem}, isOn: {isOn}");
//        if(! moduleItem) {
//            GlobalData.CurrentModule = null;
//            return;
//        }
//
//        GlobalData.CurrentModule = isOn ? moduleItem.name : null;
//    }
}
