using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class HierarchyManager : MonoBehaviour
{
    public Transform DisplayObjectListContainer;

    public InputField NameInputField;
    public Button UpButton;
    public Button DownButton;

    private static readonly List<Transform> DisplayObjectItemPool = new List<Transform>();
    private readonly List<Transform> DisplayObjectItems = new List<Transform>();

    private void Start()
    {
        GlobalData.DisplayObjects.ObserveEveryValueChanged(displayObjects => displayObjects.Count)
            .Subscribe(_ => Refresh());
        GlobalData.CurrentSelectDisplayObjects.ObserveEveryValueChanged(dic => dic.Count)
            .Subscribe(_ => Refresh());
        UpButton.OnClickAsObservable()
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(_ => Refresh());
        DownButton.OnClickAsObservable()
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(_ => Refresh());

        NameInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocused => ! isFocused && !string.IsNullOrEmpty(NameInputField.text))
            .Subscribe(_ =>
            {
                if (GlobalData.CurrentSelectDisplayObjects.Count != 1) return;
                int instanceId = GlobalData.CurrentSelectDisplayObjects.Keys.First();
                int idx = GlobalData.DisplayObjects.FindIndex(element => element.GetInstanceID() == instanceId);
                if (idx < 0 || idx >= DisplayObjectItems.Count) return;
                DisplayObjectItems[idx].GetComponentInChildren<Text>().text = NameInputField.text;
            });
    }

    private Transform GetDisplayObjectItem()
    {
        int length = DisplayObjectItemPool.Count;
        if(length == 0) return Instantiate(GlobalData.DisplayObjectItemPrefab.transform, DisplayObjectListContainer);
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

    private void RecycleAll()
    {
        foreach(Transform displayObjectItem in DisplayObjectItems)
            RecycleDisplayObject(displayObjectItem);
        DisplayObjectItems.Clear();
    }

    private void Refresh() {
        RecycleAll();
        int count = GlobalData.DisplayObjects.Count;
        for (var idx = 0; idx < count; ++idx) {
            Transform displayObjectItem = GetDisplayObjectItem();
            DisplayObjectItems.Add(displayObjectItem);
            displayObjectItem.SetParent(DisplayObjectListContainer);
            displayObjectItem.GetComponentInChildren<Text>().text = GlobalData.DisplayObjects[idx].name;
        }
        int length = DisplayObjectItems.Count;
        if (length == 0 || GlobalData.CurrentSelectDisplayObjects.Count == 0) return;
        foreach (var pair in GlobalData.CurrentSelectDisplayObjects) {
            int idx = GlobalData.DisplayObjects.FindIndex(element => element.GetInstanceID() == pair.Key);
            if (idx < 0 || idx >= length) continue;
            DisplayObjectItems[idx].GetChild(0).gameObject.SetActive(true);
        }
    }
}
