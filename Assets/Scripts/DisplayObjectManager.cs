using UnityEngine;
using UnityEngine.EventSystems;

public class DisplayObjectManager : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private Vector2 _offset;
    
    public RectTransform SelfRect;
    public GameObject FrameWider;

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
        bool isSelect = GlobalData.CurrentSelectDisplayObjects.ContainsKey(this.GetInstanceID());
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
    
    private void Start()
    {
        if (FrameWider) FrameWider.SetActive(false);
    }
    
    private void UpdateSelectState(bool bSelect, bool needUpdate = true)
    {
        if (FrameWider) FrameWider.SetActive(bSelect);
        int instanceId = this.transform.GetInstanceID();
        bool isSelect = GlobalData.CurrentSelectDisplayObjects.ContainsKey(instanceId);
        if (needUpdate)
        {
            if (bSelect && !isSelect) {
                GlobalData.CurrentSelectDisplayObjects[instanceId] = this.transform;
                Debug.Log($"Select DisplayObject: {instanceId}, name: {this.transform.name}");
            }
            if (!bSelect && isSelect) {
                GlobalData.CurrentSelectDisplayObjects.Remove(instanceId);
            }
        }
    }

    private static bool UpdateSelectDisplayObjectSelectState(Transform displayObject, bool bSelect) {
        if (!displayObject) return false;
        var displayObjectManager = displayObject.GetComponent<DisplayObjectManager>();
        if (!displayObjectManager) return false;
        displayObjectManager.UpdateSelectState(false, false);
        return false;
    }

    public static bool DeSelectDisplayObject(Transform displayObject)
    {
        if (!displayObject) return false;
        var instanceId = displayObject.GetInstanceID();
        if (!GlobalData.CurrentSelectDisplayObjects.ContainsKey(instanceId)) return false;
        var result = UpdateSelectDisplayObjectSelectState(displayObject, false);
        if (!result) return false;
        GlobalData.CurrentSelectDisplayObjects.Remove(instanceId);
        return true;
    }
    
    public static void DeselectAllDisplayObject()
    {
        foreach (var pair in GlobalData.CurrentSelectDisplayObjects)
            UpdateSelectDisplayObjectSelectState(pair.Value, false);

        GlobalData.CurrentSelectDisplayObjects.Clear();
    }
}