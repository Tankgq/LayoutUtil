using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DisplayObjectListManager : MonoBehaviour
{
    public Transform DisplayObjectListContainer;
    public Transform DisplayObjectItem;

    public InputField NameInputField;
    public Button UpButton;
    public Button DownButton;

    private static readonly List<Transform> DisplayObjectItemPool = new List<Transform>();
    private readonly List<Transform> DisplayObjectItems = new List<Transform>();

    private void Start()
    {

        GlobalData.DisplayObjects.ObserveEveryValueChanged(displayObjects => displayObjects.Count)
            .Subscribe(list => Refresh());
//        GlobalData.CurrentSelectDisplayObjects.ObserveEveryValueChanged(dic => dic.Count)

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

        UpButton.OnClickAsObservable()
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(_ => Refresh());

        DownButton.OnClickAsObservable()
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(_ => Refresh());
    }

    private Transform GetDisplayObjectItem()
    {
        int length = DisplayObjectItemPool.Count;
        if(length == 0) return Instantiate(DisplayObjectItem, DisplayObjectListContainer);
        Transform result = DisplayObjectItemPool[length - 1];
        DisplayObjectItemPool.RemoveAt(length - 1);
        return result;
    }

    private void RecycleDisplayObject(Transform displayObjectItem)
    {
        if (!displayObjectItem) return;
        displayObjectItem.SetParent(null);
        DisplayObjectItemPool.Add(displayObjectItem);
    }

    private void RecycleAll()
    {
        foreach(Transform displayObjectItem in DisplayObjectItems)
        {
            RecycleDisplayObject(displayObjectItem);
        }
        DisplayObjectItems.Clear();
    }

    private void Refresh()
    {
        RecycleAll();
        foreach (Transform displayObject in GlobalData.DisplayObjects) {
            Transform displayObjectItem = GetDisplayObjectItem();
            DisplayObjectItems.Add(displayObjectItem);
            displayObjectItem.SetParent(DisplayObjectListContainer);
            displayObjectItem.GetComponentInChildren<Text>().text = displayObject.name;
        }
    }
}
