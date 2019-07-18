using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
	public class ObservableLongPointerDownTrigger : ObservableTriggerBase, IPointerEnterHandler, IPointerExitHandler
	{
		public float IntervalSecond = 0.5f;

		private Subject<Unit> onLongHoverDown;

		private float? raiseTime;

		void Update()
		{
			if (raiseTime != null && raiseTime <= Time.realtimeSinceStartup)
			{
				if (onLongHoverDown != null) onLongHoverDown.OnNext(UniRx.Unit.Default);
				raiseTime = null;
			}
		}

		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{
			raiseTime = Time.realtimeSinceStartup + IntervalSecond;
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
			if (onLongHoverDown != null)
			{
				onLongHoverDown.OnCompleted();
			}
		}
	}
}