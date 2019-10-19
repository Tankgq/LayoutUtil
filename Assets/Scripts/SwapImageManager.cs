using UnityEngine;
using UnityEngine.UI;

public class SwapImageManager : MonoBehaviour {
	public Sprite originImage;
	public Sprite swapImage;

	public bool isSwap;

	public void UpdateSwapImage(bool swapped) {
		isSwap = swapped;
		Image image = GetComponent<Image>();
		if(image) image.sprite = isSwap ? swapImage : originImage;
	}
}
