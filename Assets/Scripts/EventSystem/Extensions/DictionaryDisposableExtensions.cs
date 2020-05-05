using System;
using UniRx;

namespace FarPlane {
	
	/// <summary>
	/// DictionaryDisposable 的拓展
	/// </summary>
	public static class DictionaryDisposableExtensions {
		
		/// <summary>
		/// 遍历 DictionaryDisposable
		/// </summary>
		/// <param name="disposableDictionary"> 存储订阅的字典 </param>
		/// <param name="action"> 遍历的具体行为 </param>
		/// <typeparam name="TKey"> 存储订阅的 Key 的类型 </typeparam>
		/// <typeparam name="TValue"> 存储订阅的值 </typeparam>
		public static void ForEach<TKey, TValue>(this DictionaryDisposable<TKey, TValue> disposableDictionary, Action<TValue> action) where TValue: IDisposable {
			if(disposableDictionary.Count == 0 || action == null) return;
			foreach(var pair in disposableDictionary) {
				action(pair.Value);
			}
		}
	}
}
