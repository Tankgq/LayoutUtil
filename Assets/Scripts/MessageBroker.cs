using System.Collections.Generic;
using UniRx;

public static class MessageBroker
{
	public enum Code {
		Null,
		UpdateSelectDisplayObjectDic,
		UpdateSwapImage,
		UpdateDisplayObjectPos,
		UpdateTitle,
		UpdateModuleTxtWidth
	}
	
	private static readonly Dictionary<Code, Subject<object[]>> SubjectDic = new Dictionary<Code, Subject<object[]>>();

	public static bool HasSubject(Code code)
	{
		return code != Code.Null && SubjectDic.ContainsKey(code);
	}

	public static void AddSubject(Code code, Subject<object[]> subject)
	{
		if (code == Code.Null || subject == null) return;
		// DisposeSubject(msgId);
		if (SubjectDic.ContainsKey(code))
			SubjectDic[code].Subscribe(subject);
		else
			SubjectDic.Add(code, subject);
	}

	public static Subject<object[]> GetSubject(Code code)
	{
		Subject<object[]> subject;
		if (SubjectDic.TryGetValue(code, out subject)) return subject;
		subject = new Subject<object[]>();
		SubjectDic.Add(code, subject);
		return subject;
	}

	public static void Send(Code code, params object[] msg)
	{
		if (code == Code.Null || !SubjectDic.ContainsKey(code)) return;
		SubjectDic[code].OnNext(msg);
	}

	public static void SendUpdateSelectDisplayObjectDic(List<string> addElements = null, List<string> removeElements = null) {
		Send(Code.UpdateSelectDisplayObjectDic, addElements, removeElements);
	}

	public static void DisposeSubject(Code code)
	{
		if (code == Code.Null || !SubjectDic.ContainsKey(code)) return;
		SubjectDic[code].Dispose();
		SubjectDic[code] = null;
	}
}