using System;
using UniRx;
using UnityEngine;

namespace FarPlane {
	public class Example : MonoBehaviour {
		private void Start() {
			// 任意按键按下的事件流
			var keyDownObservable = Observable.EveryUpdate().Where(_ => Input.anyKeyDown);
			// 当按键 A 按下的时候发送数据
			keyDownObservable.Where(_ => Input.GetKeyDown(KeyCode.A))
							 .Select(keyDownTime => new KeyDownEventData(keyDownTime))
							 .Subscribe(eventData => UlEventSystem.Dispatch<KeyDownEventType, KeyDownEventData>(KeyDownEventType.KeyA, eventData));
			// 当按键 B 按下的时候发送数据
			keyDownObservable.Where(_ => Input.GetKeyDown(KeyCode.B))
							 .Select(keyDownTime => new KeyDownEventData(keyDownTime))
							 .Subscribe(eventData => UlEventSystem.Dispatch<KeyDownEventType, KeyDownEventData>(KeyDownEventType.KeyB, eventData));
			// 当按键 C 按下的时候发送数据
			keyDownObservable.Where(_ => Input.GetKeyDown(KeyCode.C))
							 .Select(keyDownTime => new KeyDownEventData(keyDownTime))
							 .Subscribe(eventData => UlEventSystem.Dispatch<KeyDownEventType, KeyDownEventData>(KeyDownEventType.KeyC, eventData));

			// 订阅 按键 A 按下的事件
			UlEventSystem.GetSubject<KeyDownEventType, KeyDownEventData>(KeyDownEventType.KeyA)
					   .Subscribe(eventData => Debug.Log("keyA: " + eventData.KeyDownTime));
			// 订阅 按键 B 按下的事件
			UlEventSystem.GetSubject<KeyDownEventType, KeyDownEventData>(KeyDownEventType.KeyB)
					   .Subscribe(eventData => Debug.Log("keyB: " + eventData.KeyDownTime));
			// 订阅 按键 C 按下的事件
			UlEventSystem.GetSubject<KeyDownEventType, KeyDownEventData>(KeyDownEventType.KeyC)
					   .Subscribe(eventData => Debug.Log("keyC: " + eventData.KeyDownTime));
			// 3s 后清除掉 按键 A 按下事件的订阅
			Observable.Timer(TimeSpan.FromSeconds(3))
					  .Subscribe(_ => {
						   UlEventSystem.Dispose<KeyDownEventType, KeyDownEventData>(KeyDownEventType.KeyA);
						   Debug.Log("KeyA is Disposed.");
					   });
			// 6s 后结束掉 按键 B 按下事件的订阅
			Observable.Timer(TimeSpan.FromSeconds(6))
					  .Subscribe(_ => {
						   UlEventSystem.OnCompleted<KeyDownEventType>(KeyDownEventType.KeyB);
						   Debug.Log("KeyB is Complete.");
					   });
			// 9s 后清除掉 KeyDownEventType 下所有事件的订阅
			Observable.Timer(TimeSpan.FromSeconds(9))
					  .Subscribe(_ => {
						   UlEventSystem.DisposeAll<KeyDownEventType>();
						   Debug.Log("All is Disposed.");
					   });
		}
	}
}
