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
    private Vector2 _offset = new Vector2();

    private RectTransform _rect = null;

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

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mousePos = eventData.position;
        mousePos -= _offset;
        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_rect, mousePos, eventData.enterEventCamera, out pos);
        _rect.position = pos;
    }

    private void UpdateState()
    {
        if (frame) frame.SetActive(_isHover);
        if (FrameWider) FrameWider.SetActive(_isDown);
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
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDown = false;
        Debug.Log("OnPointerUp");
        UpdateState();
    }
}
