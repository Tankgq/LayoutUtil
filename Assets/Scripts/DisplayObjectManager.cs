using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DisplayObjectManager : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject frame;
    public GameObject FrameWider;

    private bool _isDown = false;
    private bool _isHover = false;
    private bool _inDragging = false;
    private RectTransform _rect = null;

    private Vector2 _offset = new Vector2();

    private void Start()
    {
        if(frame) frame.SetActive(false);
        if(FrameWider) FrameWider.SetActive(false);
        _rect = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        if (!_rect) _rect = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData) {
        Vector2 mousePos = eventData.position;
        mousePos -= _offset;
        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_rect, mousePos, eventData.enterEventCamera, out pos);
        _rect.position = pos;
    }

    private void UpdateState(bool force = false)
    {
        if (frame) frame.SetActive(_isHover);
        if (FrameWider && (_isDown || force)) FrameWider.SetActive(_isDown);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHover = true;
        Debug.Log("OnPointerEnter");
        UpdateState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHover = false;
        Debug.Log("OnPointerEnter");
        UpdateState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDown = true;
        Debug.Log("OnPointerDown");
        UpdateState();
        Vector2 mousePos = eventData.position;
        Vector2 mousePos2;
        bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, mousePos, eventData.enterEventCamera, out mousePos2);
        if (_rect) _offset = mousePos2;
        if(! Input.GetKey(KeyCode.LeftShift) || ! Input.GetKey(KeyCode.RightShift))
            DeselectAllDisplayObject();
        GlobalData.curSelectDisplayObjects.Add(this.GetInstanceID(), this.transform);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDown = false;
        Debug.Log("OnPointerUp");
        UpdateState();
//        GlobalData.curSelectDisplayObject = null;
    }

    private static bool UpdateDisplayObjectState(Transform displayObject)
    {
        if (!displayObject) return false;
        DisplayObjectManager displayObjectManager = displayObject.GetComponent<DisplayObjectManager>();
        if (!displayObjectManager) return false;
        displayObjectManager.UpdateState(true);
        return false;
    }

    public static bool DeSelectDisplayObject(Transform displayObject)
    {
        if (!displayObject) return false;
        int instanceId = displayObject.GetInstanceID();
        if (!GlobalData.curSelectDisplayObjects.ContainsKey(instanceId)) return false;
        bool result = UpdateDisplayObjectState(displayObject);
        if (!result) return false;
        GlobalData.curSelectDisplayObjects.Remove(instanceId);
        return true;
    }

    public static void DeselectAllDisplayObject()
    {
        foreach (KeyValuePair<int, Transform> pair in GlobalData.curSelectDisplayObjects)
        {
            UpdateDisplayObjectState(pair.Value);
        }

        GlobalData.curSelectDisplayObjects.Clear();
    }

    public static DisplayObject ConvertToDisplayObject(Transform displayObject) {
        if(! displayObject) return null;

        RectTransform rect = displayObject.GetComponent<RectTransform>();
        Vector2 pos = rect.anchoredPosition;
        Vector2 size = rect.sizeDelta;

        DisplayObject result = new DisplayObject();
        result.name = displayObject.name;
        result.x = pos.x;
        result.y = - pos.y;
        result.width = size.x;
        result.height = size.y;
        return result;
    }
}
