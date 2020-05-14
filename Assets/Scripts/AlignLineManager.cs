using UnityEngine;
using UnityEngine.UI;

public class AlignLineManager : MonoBehaviour {

	public RectTransform selfRect;

	public Image UpImage;
	public Image DownImage;
	public Image LeftImage;
	public Image RightImage;
	
	public RectTransform UpImageRect;
	public RectTransform DownImageRect;
	public RectTransform LeftImageRect;
	public RectTransform RightImageRect;
	
	public void UpdateHorizontal(AlignType leftType, AlignType rightType, Vector2 position, Vector2 sizeDelta) {
		UpImage.color = Color.clear;
		DownImage.color = Color.clear;
		switch(leftType) {
			case AlignType.Top:
				LeftImage.color = Color.red;
				break;
			case AlignType.HorizontalCenter:
				LeftImage.color = Color.green;
				break;
			case AlignType.Bottom:
				LeftImage.color = Color.blue;
				break;
			default:
				LeftImage.color = Color.clear;
				break;
		}
		switch(rightType) {
			case AlignType.Top:
				RightImage.color = Color.red;
				break;
			case AlignType.HorizontalCenter:
				RightImage.color = Color.green;
				break;
			case AlignType.Bottom:
				RightImage.color = Color.blue;
				break;
			default:
				RightImage.color = Color.clear;
				break;
		}

		selfRect.anchoredPosition = position;
		selfRect.sizeDelta = sizeDelta;

		Vector2 halfSize = new Vector2(sizeDelta.x * 0.5f, sizeDelta.y);
		LeftImageRect.anchoredPosition = Vector2.zero;
		LeftImageRect.sizeDelta = halfSize;
		RightImageRect.anchoredPosition = new Vector2(sizeDelta.x * 0.5f, 0);
		RightImageRect.sizeDelta = halfSize;
	}

	public void UpdateVertical(AlignType upType, AlignType downType, Vector2 position, Vector2 sizeDelta) {
		LeftImage.color = Color.clear;
		RightImage.color = Color.clear;
		switch(upType) {
			case AlignType.Left:
				UpImage.color = Color.magenta;
				break;
			case AlignType.VerticalCenter:
				UpImage.color = Color.yellow;
				break;
			case AlignType.Right:
				UpImage.color = Color.cyan;
				break;
			default:
				UpImage.color = Color.clear;
				break;
		}
		switch(downType) {
			case AlignType.Left:
				DownImage.color = Color.magenta;
				break;
			case AlignType.VerticalCenter:
				DownImage.color = Color.yellow;
				break;
			case AlignType.Right:
				DownImage.color = Color.cyan;
				break;
			default:
				DownImage.color = Color.clear;
				break;
		}

		selfRect.anchoredPosition = position;
		selfRect.sizeDelta = sizeDelta;

		Vector2 halfSize = new Vector2(sizeDelta.x, sizeDelta.y * 0.5f);
		UpImageRect.anchoredPosition = Vector2.zero;
		UpImageRect.sizeDelta = halfSize;
		DownImageRect.anchoredPosition = new Vector2(0, sizeDelta.y * 0.5f);
		DownImageRect.sizeDelta = halfSize;
	}
}
