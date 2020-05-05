using System;
using UniRx;

namespace FarPlane {
	
	/// <summary>
	/// CompositeDisposable 的拓展
	/// </summary>
	public static class CompositeDisposableExtensions {
		
		/// <summary>
		/// 遍历 CompositeDisposable
		/// </summary>
		/// <param name="disposableList"> 订阅列表 </param>
		/// <param name="action"> 遍历的具体行为 </param>
		/// <typeparam name="T"> 订阅类型 </typeparam>
		public static void ForEach<T>(this CompositeDisposable disposableList, Action<T> action) {
			if(disposableList.Count == 0 || action == null) return;
			foreach(IDisposable disposable in disposableList) {
				action((T)disposable);
			}
		}
		
		/// <summary>
		/// 给 CompositeDisposable 中存储的每个 Subject 发送事件数据, 如果 TEventData 与 subject 使用的数据类型不一致则不发
		/// </summary>
		/// <param name="disposableList"> 订阅列表 </param>
		/// <param name="eventData"> 事件数据 </param>
		/// <typeparam name="TEventData"> 事件数据类型 </typeparam>
		public static void Dispatch<TEventData>(this CompositeDisposable disposableList, TEventData eventData) {
			if(disposableList == null || disposableList.Count == 0) return;
			foreach(var disposable in disposableList) {
				Subject<TEventData> subject = disposable as Subject<TEventData>;
				subject?.OnNext(eventData);
			}
		}
		
		/// <summary>
		/// 调用 CompositeDisposable 中存储的每个 Subject 的 onComplete 事件, 并清除所有订阅
		/// </summary>
		/// <param name="disposableList"> 订阅列表 </param>
		/// <typeparam name="TEventData"> 事件数据类型 </typeparam>
		/// <returns> 是否成功执行 OnComplete </returns>
		public static bool OnCompleted<TEventData>(this CompositeDisposable disposableList) {
			if(disposableList == null || disposableList.Count == 0) return false;
			disposableList.ForEach<Subject<TEventData>>(subject => subject?.OnCompleted());
			disposableList.Clear();
			return true;
		}

		/// <summary>
		/// 获取 CompositeDisposable 中第一个订阅者
		/// </summary>
		/// <param name="disposableList"> 订阅列表 </param>
		/// <typeparam name="T"> 订阅类型 </typeparam>
		/// <returns> 第一个订阅者, 可能为 null </returns>
		public static T First<T>(this CompositeDisposable disposableList) {
			if(disposableList.Count == 0) return default;
			return (T)disposableList.GetEnumerator().Current;
		}
	}
}
