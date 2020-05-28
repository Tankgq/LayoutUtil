using System;
using System.Collections.Generic;
using UniRx;

namespace FarPlane {
	/// <summary>
	/// 基于 UniRx 的事件系统, 使用的时候需要先定义好实现 IEventType 接口的事件类型 TEventType, <br />
	/// 和实现 IEventData 接口的事件数据的 TEventData, <br />
	/// 每个事件会被划分到相应的 TEventType 内, 然后让 TEventType 对应的 EventManager 来处理事件的具体逻辑
	/// </summary>
	public static class UlEventSystem {
		/// <summary>
		/// 存储 事件管理者 的字典
		/// </summary>
		private static readonly Dictionary<Type, EventManager> EventManagers = new Dictionary<Type, EventManager>();

		/// <summary>
		/// 通过 TEventType 和 TEventData 来获取相应的事件管理者 
		/// </summary>
		/// <param name="createIfNeed"> 如果事件管理者不存在是否需要创建事件管理者 </param>
		/// <typeparam name="TEventType"> 事件的类型 </typeparam>
		/// <returns> 相应的事件管理者 </returns>
		private static EventManager GetEventManager<TEventType>(bool createIfNeed = true) where TEventType: IEventType {
			Type key = typeof(TEventType);
			if(EventManagers.TryGetValue(key, out EventManager eventManager)) return eventManager;

			if(! createIfNeed) return null;
			EventManagers[key] = eventManager = new EventManager();
			return eventManager;
		}

		/// <summary>
		/// 根据 TEventType, TEventData 以及 eventType 来获取相应的 Subject,<br />
		/// 每个 Subject 只能订阅一次, 每次调用都会获取到一个新的 Subject
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <typeparam name="TEventType"> 事件的类型 </typeparam>
		/// <typeparam name="TEventData"> 事件数据的类型 </typeparam>
		/// <returns> 一个新的 subject </returns>
		public static Subject<TEventData> GetSubject<TEventType, TEventData>(int eventType)
				where TEventType: IEventType where TEventData: IEventData {
			EventManager eventManager = GetEventManager<TEventType>();
			return eventManager.GetSubject<TEventData>(eventType);
		}

		/// <summary>
		/// 根据 TEventType, TriggerEventData 以及 eventType 来获取相应的 Subject,<br />
		/// 每个 Subject 只能订阅一次, 每次调用都会获取到一个新的 Subject
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <typeparam name="TEventType"> 事件的类型 </typeparam>
		/// <returns> 一个新的 subject </returns>
		public static Subject<TriggerEventData> GetTriggerSubject<TEventType>(int eventType) where TEventType: IEventType {
			return GetSubject<TEventType, TriggerEventData>(eventType);
		}

		/// <summary>
		/// 根据 TEventType, TEventData 以及 eventType 来获取第一个订阅该事件的 Subject, 如果没有订阅者, 则返回 null
		/// 可用于判断该 事件具体类型 是否有订阅者
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <typeparam name="TEventType"> 事件的类型 </typeparam>
		/// <typeparam name="TEventData"> 事件数据的类型 </typeparam>
		/// <returns> 第一个订阅者或者 null </returns>
		public static Subject<TEventData> TryToGetSubject<TEventType, TEventData>(int eventType)
				where TEventType: IEventType where TEventData: IEventData {
			EventManager eventManager = GetEventManager<TEventType>(false);
			return eventManager?.TryToGetSubject<TEventData>(eventType);
		}

		/// <summary>
		/// 给订阅 eventType 的订阅者发送事件数据
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <param name="eventData"> 事件数据 </param>
		/// <typeparam name="TEventType"> 事件的类型 </typeparam>
		/// <typeparam name="TEventData"> 事件数据的类型 </typeparam>
		public static void Dispatch<TEventType, TEventData>(int eventType, TEventData eventData)
				where TEventType: IEventType where TEventData: IEventData {
			EventManager eventManager = GetEventManager<TEventType>(false);
			eventManager?.Dispatch(eventType, eventData);
		}

		/// <summary>
		/// 给订阅 eventType 的订阅者发送空的事件数据 (TriggerEventData) 作为触发器
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <typeparam name="TEventType"> 事件的类型 </typeparam>
		public static void DispatchTrigger<TEventType>(int eventType) where TEventType: IEventType {
			Dispatch<TEventType, TriggerEventData>(eventType, TriggerEventData.Default);
		}

		/// <summary>
		/// 给 TEventType 下的所有事件的订阅者发送事件数据
		/// </summary>
		/// <param name="eventData"> 事件数据 </param>
		/// <typeparam name="TEventType"> 事件的类型 </typeparam>
		/// <typeparam name="TEventData"> 事件数据的类型 </typeparam>
		public static void DispatchAll<TEventType, TEventData>(TEventData eventData)
				where TEventType: IEventType where TEventData: IEventData {
			EventManager eventManager = GetEventManager<TEventType>(false);
			eventManager?.DispatchAll(eventData);
		}

		/// <summary>
		/// 结束 eventType 事件的订阅者的订阅
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <typeparam name="TEventType"> 事件的类型 </typeparam>
		public static void OnCompleted<TEventType>(int eventType) where TEventType: IEventType {
			EventManager eventManager = GetEventManager<TEventType>(false);
			eventManager?.OnCompleted(eventType);
		}

		/// <summary>
		/// 结束属于 TEventType 的所有事件的订阅者的订阅
		/// </summary>
		/// <typeparam name="TEventType"> 事件的类型 </typeparam>
		/// <typeparam name="TEventData"> 事件数据的类型 </typeparam>
		public static void OnAllCompleted<TEventType, TEventData>()
				where TEventType: IEventType where TEventData: IEventData {
			EventManager eventManager = GetEventManager<TEventType>(false);
			eventManager?.OnAllCompleted();
		}

		/// <summary>
		/// 去除订阅 eventType 的订阅者, 如果 subject 非空, 则如果 subject 有订阅 eventType, 取消其订阅
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <param name="subject"> 订阅了 eventType 的订阅者 </param>
		/// <typeparam name="TEventType"> 事件的类型 </typeparam>
		/// <typeparam name="TEventData"> 事件数据的类型 </typeparam>
		/// <returns> 是否成功执行 Dispose </returns>
		public static bool Dispose<TEventType, TEventData>(int eventType, Subject<TEventData> subject = null)
				where TEventType: IEventType where TEventData: IEventData {
			EventManager eventManager = GetEventManager<TEventType>(false);
			return eventManager != null && eventManager.Dispose(eventType, subject);
		}

		/// <summary>
		/// 去除订阅属于 TEventType 的所有事件的订阅者
		/// </summary>
		/// <typeparam name="TEventType"> 事件的类型 </typeparam>
		/// <returns> 是否成功执行 DisposeAll </returns>
		public static void DisposeAll<TEventType>() where TEventType: IEventType {
			EventManager eventManager = GetEventManager<TEventType>(false);
			eventManager?.DisposeAll();
		}
	}
}
