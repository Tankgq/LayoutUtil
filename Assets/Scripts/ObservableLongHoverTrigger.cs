using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ObservableLongPointerDownTrigger : ObservableTriggerBase, IPointerEnterHandler, IPointerExitHandler
{
	public float intervalSecond = 0.5f;

	private Subject<Unit> onLongHoverDown;

	private float? raiseTime;

	private void Update()
	{
		if (raiseTime == null
		 || !(raiseTime <= Time.realtimeSinceStartup)) return;
		onLongHoverDown?.OnNext(Unit.Default);
		raiseTime = null;
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		raiseTime = Time.realtimeSinceStartup + intervalSecond;
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		raiseTime = null;
	}

	public IObservable<Unit> OnLongHoverAsObservable()
	{
		return onLongHoverDown ?? (onLongHoverDown = new Subject<Unit>());
	}

	protected override void RaiseOnCompletedOnDestroy()
	{
		onLongHoverDown?.OnCompleted();
	}
}