using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DisplayObjectListManager : MonoBehaviour
{
    public Transform DisplayObjectListContainer;
    public Transform DisplayObjectItem;

    private static readonly List<Transform> DisplayObjectItemPool = new List<Transform>();

    private void Start()
    {
        GlobalData.DisplayObjects.ObserveEveryValueChanged(displayObjects => displayObjects.Count)
            .Subscribe(list =>
            {
                try
                {
                    RecycleAll();
                    foreach (Transform displayObject in GlobalData.DisplayObjects) {
                        Transform displayObjectItem = GetDisplayObjectItem();
                        displayObjectItem.SetParent(DisplayObjectListContainer);
                        displayObjectItem.GetComponentInChildren<Text>().text = displayObject.name;
                    }
                }
                catch (Exception e)
                {
                    MessageBoxUtil.Show($"{e}");
                }
            });
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
        int length = DisplayObjectListContainer.childCount;
        for (int idx = length - 1; idx >= 0; --idx)
            RecycleDisplayObject(DisplayObjectListContainer.GetChild(idx));
    }
}
