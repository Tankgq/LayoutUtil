using System.Collections.Generic;
using UniRx;

public static class MessageBroker
{
	public const int UpdateSelectDisplayObject = 1;
	public const int UpdateModifyCount = 2;

	private static readonly Dictionary<int, Subject<object[]>> SubjectDic = new Dictionary<int, Subject<object[]>>();

	public static void AddSubject(int msgId, Subject<object[]> subject)
	{
		if (msgId == 0 || subject == null) return;
		// DisposeSubject(msgId);
		if (SubjectDic.ContainsKey(msgId))
			SubjectDic[msgId].Subscribe(subject);
		else
			SubjectDic.Add(msgId, subject);
	}

	public static Subject<object[]> GetSubject(int msgId)
	{
		Subject<object[]> subject;
		if (SubjectDic.TryGetValue(msgId, out subject)) return subject;
		subject = new Subject<object[]>();
		SubjectDic.Add(msgId, subject);
		return subject;
	}

	public static void Send(int msgId, params object[] msg)
	{
		if (msgId == 0 || !SubjectDic.ContainsKey(msgId)) return;
		SubjectDic[msgId].OnNext(msg);
	}

	public static void DisposeSubjct(int msgId)
	{
		if (msgId == 0 || !SubjectDic.ContainsKey(msgId)) return;
		SubjectDic[msgId].Dispose();
		SubjectDic[msgId] = null;
	}
}