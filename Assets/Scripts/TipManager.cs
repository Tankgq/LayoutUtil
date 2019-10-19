using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class TipManager : MonoBehaviour {
	public string tipInfo = "TipInfo";
	public int offsetX;
	public int offsetY;
	public bool isOnLeft;

	private static readonly List<Transform> TipPool = new List<Transform>();

	private static Transform GetTip() {
		int length = TipPool.Count;
		if(length == 0) return Instantiate(GlobalData.TipPrefab.transform, GlobalData.RootCanvas.transform);
		Transform result = TipPool[length - 1];
		result.gameObject.SetActive(true);
		TipPool.RemoveAt(length - 1);
		return result;
	}

	private static void RecycleTip(Transform tip) {
		if(! tip) return;
		tip.gameObject.SetActive(false);
		tip.GetComponentInChildren<Text>().text = "TipInfo";
		TipPool.Add(tip);
	}

	private Transform _tip;
	private IDisposable _disposable;

	private void Awake() {
		gameObject.AddComponent<ObservableLongPointerDownTrigger>()
				  .OnLongHoverAsObservable()
				  .Where(_ => ! _tip)
				  .Subscribe(_ => {
					   _tip = GetTip();
					   _disposable = _tip.GetComponent<Graphic>()
										 .OnPointerExitAsObservable()
										 .Subscribe(__ => RecycleAndCleanTip());
					   _tip.GetComponentInChildren<Text>().text = tipInfo;
					   RectTransform rt = _tip.GetComponent<RectTransform>();
					   Vector2 pos = Utils.GetAnchoredPositionInCanvas(transform);
					   pos.x += offsetX;
					   if(isOnLeft) {
						   pos.x += transform.GetComponent<RectTransform>().rect.width;
						   // 不移到屏幕外的话就会因为一开始鼠标停留在 Tip 上, 然后 Tip 因为下面的定时器移除一段距离,
						   // 如果这时候鼠标正好不在 Tip 上 Tip 会被关掉, 表现出来就是 Tip 闪一下就消失了.
						   // 如果是通过 setActive 来处理这个问题, 则会导致 Tip 的大小没有重新计算
						   pos.x += 1000000;
						   Observable.Timer(TimeSpan.Zero)
									 .Subscribe(__ => {
										  pos.x -= 1000000;
										  pos.x += rt.rect.width;
										  rt.anchoredPosition = pos;
									  });
					   }

					   pos.y += offsetY;
					   rt.anchoredPosition = pos;
				   });
		transform.GetComponentInChildren<Graphic>()
				 .OnPointerExitAsObservable()
				 .Where(_ => _tip)
				 .Subscribe(_ => {
					  if(Utils.IsPointOverGameObject(_tip.gameObject)) return;
					  RecycleAndCleanTip();
				  });
	}

	private void RecycleAndCleanTip() {
		if(_disposable != null) {
			_disposable.Dispose();
			_disposable = null;
		}

		RecycleTip(_tip);
		_tip = null;
	}
}
