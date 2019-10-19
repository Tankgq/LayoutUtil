using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageHoverManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public Image hoverImage;
	public Color originColor;

	public void OnPointerEnter(PointerEventData eventData) {
		hoverImage.color = originColor;
	}

	public void OnPointerExit(PointerEventData eventData) {
		hoverImage.color = Color.clear;
	}

	public void SimulatePointerExit() {
		hoverImage.color = Color.clear;
	}

	private void Start() {
		hoverImage.color = Color.clear;
	}
}
