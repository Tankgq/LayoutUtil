using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class SwapImageManager : MonoBehaviour
	{
		public Sprite OriginImage;
		public Sprite SwapImage;
		public bool IsSwap = false;

		private IDisposable CancelObserveImageChange = null;

		public bool HasObserveImageChange()
		{
			return CancelObserveImageChange != null;
		}

		public void StartObserveImageChange(Action<bool> onImageChange = null)
		{
			if (HasObserveImageChange()) return;
			GetComponent<Image>().sprite = IsSwap ? SwapImage : OriginImage;
			CancelObserveImageChange = this.ObserveEveryValueChanged(_ => IsSwap)
				.Subscribe(isSwap =>
				{
					GetComponent<Image>().sprite = IsSwap ? SwapImage : OriginImage;
					onImageChange?.Invoke(isSwap);
				});
		}

		public void ForceUpdate()
		{
			GetComponent<Image>().sprite = IsSwap ? SwapImage : OriginImage;
		}

		public void StopObserveImageChange()
		{
			if (CancelObserveImageChange == null) return;
			CancelObserveImageChange.Dispose();
			CancelObserveImageChange = null;
		}
	}
}
