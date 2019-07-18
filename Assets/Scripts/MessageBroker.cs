using UniRx;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Assets.Scripts
{
	public class MessageBroker
	{
		public const int UPDATE_SELECT_DISPLAY_OBJECT = 1;

		private static readonly Dictionary<int, Subject<object[]>> _subjectDic = new Dictionary<int, Subject<object[]>>();

		public static void AddSubject(int msgId, Subject<object[]> subject)
		{
			if (msgId == 0 || subject == null) return;
			// DisposeSubjct(msgId);
			if (_subjectDic.ContainsKey(msgId))
				_subjectDic[msgId].Subscribe(subject);
			else
				_subjectDic.Add(msgId, subject);
		}

		public static Subject<object[]> GetSubject(int msgId)
		{
			Subject<object[]> subject = null;
			if (!_subjectDic.TryGetValue(msgId, out subject))
			{
				subject = new Subject<object[]>();
				_subjectDic.Add(msgId, subject);
			}
			return subject;
		}

		public static void Send(int msgId, params object[] msg)
		{
			if (msgId == 0 || !_subjectDic.ContainsKey(msgId)) return;
			_subjectDic[msgId].OnNext(msg);
		}

		public static void DisposeSubjct(int msgId)
		{
			if (msgId == 0 || !_subjectDic.ContainsKey(msgId)) return;
			_subjectDic[msgId].Dispose();
			_subjectDic[msgId] = null;
		}
	}
}