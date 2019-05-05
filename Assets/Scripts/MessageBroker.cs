using UniRx;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Assets.Scripts
{
	public class MessageBroker
	{
		public const int UPDATE_SELECT_DISPLAY_OBJECT = 1;

		private static readonly Dictionary<int, List<Subject<object[]>>> _subjectDic = new Dictionary<int, List<Subject<object[]>>>();

		public static void AddSubject(int msgId, Subject<object[]> subject)
		{
			if (msgId == 0 || subject == null) return;
			if (!_subjectDic.ContainsKey(msgId))
				_subjectDic.Add(msgId, new List<Subject<object[]>>());
			int count = _subjectDic[msgId].Count;
			_subjectDic[msgId].Add(subject);
		}

		public static void Send(int msgId, params object[] msg)
		{
			if (msgId == 0 || !_subjectDic.ContainsKey(msgId)) return;
			int count = _subjectDic[msgId].Count;
			for (int idx = 0; idx < count; ++idx)
				_subjectDic[msgId][idx].OnNext(msg);
			Debug.Log($"length: {msg.Length}, msg: {msg}");
		}

		public static void DisposeSubjct(int msgId)
		{
			if (msgId == 0 || !_subjectDic.ContainsKey(msgId)) return;
			int count = _subjectDic[msgId].Count;
			for (int idx = 0; idx < count; ++idx)
				_subjectDic[msgId][idx].Dispose();
			_subjectDic[msgId].Clear();
		}

		public static void DisposeSubjctById(int msgId, Subject<object[]> subject)
		{
			if (msgId == 0 || !_subjectDic.ContainsKey(msgId)) return;
			bool result = _subjectDic[msgId].Remove(subject);
			if (result) subject.Dispose();
		}
	}
}