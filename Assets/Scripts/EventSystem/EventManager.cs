using UniRx;

namespace FarPlane {
	
	/// <summary>
	/// 事件管理系统的具体事件的管理者
	/// </summary>
	class EventManager {
		
		/// <summary>
		/// 存储 具体事件 订阅者的字典
		/// </summary>
		private readonly DictionaryDisposable<int, CompositeDisposable> _eventDictionary = new DictionaryDisposable<int, CompositeDisposable>();
		
		/// <summary>
		/// 获取订阅 eventType 的所有订阅者
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <param name="createIfNeed"> 如果事件订阅者列表不存在是否需要创建事件订阅者列表 </param>
		/// <returns> 事件订阅者列表 </returns>
		private CompositeDisposable GetDisposableList(int eventType, bool createIfNeed = true) {
			if(_eventDictionary.TryGetValue(eventType, out CompositeDisposable subjectList)) return subjectList;
			if(! createIfNeed) return null;
			_eventDictionary[eventType] = subjectList = new CompositeDisposable();
			return subjectList;
		}

		/// <summary>
		/// 获取订阅 eventType 的订阅者
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <param name="createIfNeed"> 如果事件订阅者不存在是否需要创建事件订阅者 </param>
		/// <typeparam name="TEventData"> 事件数据的类型 </typeparam>
		/// <returns> 事件订阅者 </returns>
		public Subject<TEventData> GetSubject<TEventData>(int eventType, bool createIfNeed = true) {
			CompositeDisposable disposableList = GetDisposableList(eventType, createIfNeed);
			if(disposableList == null) return null;
			Subject<TEventData> subject = new Subject<TEventData>();
			disposableList.Add(subject);
			return subject;
		}

		/// <summary>
		/// 返回订阅该事件的第一个订阅者
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <typeparam name="TEventData"> 事件数据的类型 </typeparam>
		/// <returns> 事件的第一个订阅者 </returns>
		public Subject<TEventData> TryToGetSubject<TEventData>(int eventType) {
			CompositeDisposable disposableList = GetDisposableList(eventType, false);
			return disposableList?.First<Subject<TEventData>>();
		}

		/// <summary>
		/// 结束 eventType 的订阅
		/// </summary>
		/// <param name="eventType"></param>
		/// <returns> 是否成功结束 </returns>
		public bool OnCompleted(int eventType) {
			if(_eventDictionary.Count == 0) return false;
			CompositeDisposable disposableList = GetDisposableList(eventType, false);
			disposableList?.OnCompleted<IEventData>();
			return _eventDictionary.Remove(eventType);
		}

		/// <summary>
		/// 结束该事件管理者下所有事件的订阅
		/// </summary>
		/// <returns> 是否成功结束 </returns>
		public bool OnAllCompleted() {
			if(_eventDictionary.Count == 0) return false;
			_eventDictionary.ForEach(disposableList => disposableList.OnCompleted<IEventData>());
			_eventDictionary.Clear();
			return true;
		}

		/// <summary>
		/// 给订阅 eventType 的订阅者发送数据
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <param name="eventData"> 事件的数据 </param>
		public void Dispatch<TEventData>(int eventType, TEventData eventData) {
			if(_eventDictionary.Count == 0) return;
			CompositeDisposable disposableList = GetDisposableList(eventType, false);
			disposableList?.Dispatch(eventData);
		}

		/// <summary>
		/// 给该事件管理者下所有事件的订阅者发送数据, 订阅的数据类型需要与 TEventData 相同才能收到数据
		/// </summary>
		/// <param name="eventData"> 事件的数据 </param>
		/// <typeparam name="TEventData"> 事件数据的类型 </typeparam>
		public void DispatchAll<TEventData>(TEventData eventData) {
			if(_eventDictionary.Count == 0) return;
			_eventDictionary.ForEach(disposableList => disposableList.Dispatch(eventData));
		}

		/// <summary>
		/// 清除 eventType 的订阅, 如果 subject 非空, 则只清除 subject 的订阅
		/// </summary>
		/// <param name="eventType"> 事件的具体类型 </param>
		/// <param name="subject"> 指定的订阅者 </param>
		/// <typeparam name="TEventData"> 事件数据的类型 </typeparam>
		/// <returns> 清除的结果 </returns>
		public bool Dispose<TEventData>(int eventType, Subject<TEventData> subject = null) {
			if(_eventDictionary.Count == 0) return false;
			if(subject == null) return _eventDictionary.Remove(eventType);

			CompositeDisposable disposableList = GetDisposableList(eventType, false);
			return disposableList != null && disposableList.Remove(subject);
		}

		/// <summary>
		/// 清除该事件管理者下所有事件的订阅
		/// </summary>
		/// <returns> 清除结果 </returns>
		public bool DisposeAll() {
			if(_eventDictionary.Count == 0) return false;
			_eventDictionary.Clear();
			return true;
		}
	}
}
