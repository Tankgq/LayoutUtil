using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisplayObjectItemManager : MonoBehaviour, IPointerDownHandler {
    
    public void OnPointerDown(PointerEventData eventData)
    {
        int idx = this.transform.GetSiblingIndex();
        if (idx == -1) return;
        Transform displayObject = GlobalData.DisplayObjects[idx];
        int instanceId = displayObject.GetInstanceID();
        bool isSelect = GlobalData.CurrentSelectDisplayObjects.ContainsKey(instanceId);
        Debug.Log($"isSelect: {isSelect}");
        if (isSelect) {
            if (KeyboardEventManager.IsControlDown())
                GlobalData.CurrentSelectDisplayObjects.Remove(instanceId);
        } else {
            if (!KeyboardEventManager.IsShiftDown())
                DeselectAllDisplayObjectItem();
            GlobalData.AddCurrentSelectObject(displayObject);
        }
    }

    private static int GetDisplayObjectInstanceId(Transform displayObjectItem)
    {
        if (!displayObjectItem) return 0;
        int idx = displayObjectItem.GetSiblingIndex();
        if (idx == -1) return 0;
        return GlobalData.DisplayObjects[idx].GetInstanceID();
    }

    public static bool DeSelectDisplayObject(Transform displayObjectItem) {
        if (!displayObjectItem) return false;
        int instanceId = GetDisplayObjectInstanceId(displayObjectItem);
        if (instanceId == 0) return false;
        if (!GlobalData.CurrentSelectDisplayObjects.ContainsKey(instanceId)) return false;
        GlobalData.CurrentSelectDisplayObjects.Remove(instanceId);
        return true;
    }

    public static void DeselectAllDisplayObjectItem() {
        GlobalData.CurrentSelectDisplayObjects.Clear();
    }
}
