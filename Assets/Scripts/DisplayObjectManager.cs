using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisplayObjectManager : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private Vector2 _offset;
    
    public RectTransform SelfRect;
//    public Toggle SelfToggle;

    public void OnDrag(PointerEventData eventData)
    {
        var mousePos = eventData.position;
        mousePos -= _offset;
        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(SelfRect, mousePos, eventData.enterEventCamera, out pos);
        SelfRect.position = pos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(this.transform.GetInstanceID());
        if (isSelect)
        {
            if (KeyboardEventManager.IsControlDown())
                UpdateSelectState(false);
        }
        else
        {
            if (!KeyboardEventManager.IsShiftDown())
                DeselectAllDisplayObject();
            UpdateSelectState(true);
        }
        var mousePos = eventData.position;
        Vector2 offset;
        var isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(SelfRect, mousePos, eventData.enterEventCamera, out offset);
        if (isRect) _offset = offset;
    }
    
    private void UpdateSelectState(bool bSelect)
    {
        int instanceId = this.transform.GetInstanceID();
        bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(instanceId);
        if (bSelect && !isSelect) {
            GlobalData.AddCurrentSelectObject(this.transform);
        }
        if (!bSelect && isSelect) {
            GlobalData.CurrentSelectDisplayObjectDic.Remove(instanceId);
        }
    }

    public static bool DeSelectDisplayObject(Transform displayObject)
    {
        if (!displayObject) return false;
        var instanceId = displayObject.GetInstanceID();
        if (!GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(instanceId)) return false;
        GlobalData.CurrentSelectDisplayObjectDic.Remove(instanceId);
        return true;
    }
    
    public static void DeselectAllDisplayObject()
    {
        GlobalData.CurrentSelectDisplayObjectDic.Clear();
    }
}