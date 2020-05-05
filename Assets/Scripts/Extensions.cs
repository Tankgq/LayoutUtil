using System.Collections.Generic;
using System.Linq;

public static class Extensions {

	/// <summary>
	/// 如果字典内只有一个 Key, 那么返回这个 Key 对应的 Value, 否则返回 null
	/// </summary>
	/// <returns></returns>
	public static TValue OnlyValue<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary) where TValue: class {
		return dictionary.Count != 1 ? null : dictionary.First().Value;
	}

	public static List<TKey> KeyList<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary, bool createIfNeed = false) {
		if(dictionary.Count != 0) return dictionary.Select(pair => pair.Key).ToList();
		return createIfNeed ? new List<TKey>() : null;
	}
}
