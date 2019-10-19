using System.Collections.Generic;
using UniRx;

public enum MessageCode {
	Null,
	UpdateSelectDisplayObjectDic,
	UpdateSwapImage,
	UpdateInspectorInfo,
	UpdateTitle,
	UpdateModuleTxtWidth,
	UpdateHierarchy
}

public static class MessageBroker {
	private static readonly Dictionary<MessageCode, Subject<object[]>> SubjectDic = new Dictionary<MessageCode, Subject<object[]>>();

	public static bool HasSubject(MessageCode messageCode) {
		return messageCode != MessageCode.Null && SubjectDic.ContainsKey(messageCode);
	}

	public static void AddSubject(MessageCode messageCode, Subject<object[]> subject) {
		if(messageCode == MessageCode.Null || subject == null) return;
		// DisposeSubject(msgId);
		if(SubjectDic.ContainsKey(messageCode))
			SubjectDic[messageCode].Subscribe(subject);
		else
			SubjectDic.Add(messageCode, subject);
	}

	public static Subject<object[]> GetSubject(MessageCode messageCode) {
		Subject<object[]> subject;
		if(SubjectDic.TryGetValue(messageCode, out subject)) return subject;
		subject = new Subject<object[]>();
		SubjectDic.Add(messageCode, subject);
		return subject;
	}

	private static void Send(MessageCode messageCode, params object[] msg) {
		if(messageCode == MessageCode.Null || ! SubjectDic.ContainsKey(messageCode)) return;
		SubjectDic[messageCode].OnNext(msg);
	}

	public static void SendUpdateSelectDisplayObjectDic(List<string> addElements = null, List<string> removeElements = null) {
		Send(MessageCode.UpdateSelectDisplayObjectDic, addElements, removeElements);
	}

	public static void SendUpdateSwapImage(string moduleName, string elementName, bool isSwap) {
		Send(MessageCode.UpdateSwapImage, moduleName, elementName, isSwap);
	}

	public static void SendUpdateInspectorInfo() {
		Send(MessageCode.UpdateInspectorInfo);
	}

	public static void SendUpdateTitle() {
		Send(MessageCode.UpdateTitle);
	}

	public static void SendUpdateModuleTxtWidth() {
		Send(MessageCode.UpdateModuleTxtWidth);
	}

	public static void SendUpdateHierarchy() {
		Send(MessageCode.UpdateHierarchy);
	}

	public static void DisposeSubject(MessageCode messageCode) {
		if(messageCode == MessageCode.Null || ! SubjectDic.ContainsKey(messageCode)) return;
		SubjectDic[messageCode].Dispose();
		SubjectDic[messageCode] = null;
	}
}
