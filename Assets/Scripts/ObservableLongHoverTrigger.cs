using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ObservableLongPointerDownTrigger : ObservableTriggerBase, IPointerEnterHandler, IPointerExitHandler
{
	public float intervalSecond = 0.5f;

	private Subject<Unit> _onLongHoverDown;

	private float? _raiseTime;

	private void Update()
	{
		if (_raiseTime == null
		 || !(_raiseTime <= Time.realtimeSinceStartup)) return;
		_onLongHoverDown?.OnNext(Unit.Default);
		_raiseTime = null;
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		_raiseTime = Time.realtimeSinceStartup + intervalSecond;
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		_raiseTime = null;
	}

	public IObservable<Unit> OnLongHoverAsObservable()
	{
		return _onLongHoverDown ?? (_onLongHoverDown = new Subject<Unit>());
	}

	protected override void RaiseOnCompletedOnDestroy()
	{
		_onLongHoverDown?.OnCompleted();
	}
}