using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageHoverManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image HoverImage;
    public Color OriginColor;

    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverImage.color = OriginColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverImage.color = Color.clear;
    }

    public void SimulatePointerExit() {
        HoverImage.color = Color.clear;
    }

    private void Start() {
        HoverImage.color = Color.clear;
    }
}
