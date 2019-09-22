using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class Utils {
	public static byte[] ReadFile(string filePath) {
		if(string.IsNullOrEmpty(filePath) || ! File.Exists(filePath)) return null;
		byte[] bytes;
		try {
			using(FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
				fs.Seek(0, SeekOrigin.Begin);
				bytes = new byte[fs.Length];
				fs.Read(bytes, 0, (int)fs.Length);
				fs.Close();
			}
		} catch(Exception e) {
			DialogManager.ShowError($"{e}");
			return null;
		}

		return bytes;
	}

	public static bool WriteFile(string filePath, byte[] content) {
		if(content == null || content.Length == 0) return false;
		Debug.Log(content.Length);
		bool isExist = CheckFileDirectory(filePath);
		if(! isExist) return false;
		try {
			using(FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
				fs.Seek(0, SeekOrigin.Begin);
				fs.Write(content, 0, (int)content.Length);
				fs.Flush();
				fs.Close();
			}
		} catch(Exception e) {
			DialogManager.ShowError($"{e}");
			return false;
		}
		return true;
	}

	private static readonly Regex RegFileName = new Regex(@"[\\/]([^.\\/]*).[a-zA-Z]*$");

	public static string GetFileNameInPath(string filePath) {
		GroupCollection groups = RegFileName.Match(filePath).Groups;
		return groups.Count > 1 ? groups[1].Value : string.Empty;
	}

	private static readonly Regex RegFileDirectory = new Regex(@"([\d\D]*)[\\/][^.]*.[a-zA-Z]*$");

	private static bool CheckFileDirectory(string filePath) {
		GroupCollection groups = RegFileDirectory.Match(filePath).Groups;
		if(groups.Count < 2) return false;
		string fileDirectory = groups[1].Value;
		return Directory.Exists(fileDirectory);
	}

	public static string CancelHighlight(string text) {
		return string.IsNullOrEmpty(text) ? text : Regex.Replace(text, @"<color=[a-zA-Z]+><size=\d+><b>(?<str>.*?)</b></size></color>", @"${str}");
	}

	public static string GetHighlight(string text, string needHighlight) {
		if(string.IsNullOrEmpty(text) || string.IsNullOrEmpty(needHighlight)) return text;
		return text.Replace(needHighlight, "<color=yellow><size=25><b>" + needHighlight + "</b></size></color>");
	}

	private static readonly Regex RegGetDisplayName = new Regex(@"_([^_]*)$");

	public static string GetDisplayObjectName(string displayObjectKey) {
		if(string.IsNullOrEmpty(displayObjectKey)) return displayObjectKey;
		GroupCollection groups = RegGetDisplayName.Match(displayObjectKey).Groups;
		return groups.Count < 2 ? displayObjectKey : groups[1].Value;
	}

	public static bool IsFocusOnInputText() {
		GameObject focusGameObject = EventSystem.current.currentSelectedGameObject;
		return focusGameObject && focusGameObject.GetComponent<InputField>() != null;
	}

	public static Vector2 GetAnchoredPositionInCanvas(Transform element) {
		System.Diagnostics.Debug.Assert(Camera.main != null, "Camera.main != null");
		Vector2 pos = Camera.main.WorldToScreenPoint(element.position);
		RectTransform crt = GlobalData.RootCanvas.transform.GetComponent<RectTransform>();
		pos.x = pos.x - crt.rect.width * crt.pivot.x;
		pos.y = pos.y - crt.rect.height * crt.pivot.y;
		return pos;
	}

	public static bool IsPointOverGameObject(GameObject go) {
		return go && RectTransformUtility.RectangleContainsScreenPoint(go.GetComponent<RectTransform>(), Input.mousePosition, Camera.main);
	}

	public static bool IsPointOverTransform(Transform transform) {
		return transform && RectTransformUtility.RectangleContainsScreenPoint(transform.GetComponent<RectTransform>(), Input.mousePosition, Camera.main);
	}

	/**
     * 获取 position 对应在 Container 中实际存储的位置
     * type 为 0 的时候表示传的是 Input.mousePosition, 需要将 y 值换算一下
     */
	public static Vector2 GetRealPositionInContainer(Vector2 position, int type = 0) {
		RectTransform rt = GlobalData.DisplayObjectContainer.GetComponent<RectTransform>();
		Vector2 pos = rt.anchoredPosition;
		if(type == 0) position.y = Screen.height - position.y;
		pos.x = position.x - pos.x;
		pos.y = position.y + pos.y;
		pos /= rt.localScale.x;
		pos += GlobalData.OriginPoint;
		return pos;
	}

	public static Vector2 GetAnchoredPositionInContainer(Vector2 position, int type = 0) {
		return Element.ConvertTo(GetRealPositionInContainer(position, type));
	}

	private const float Eps = 0.000001f;

	public static bool IsEqual(float rhs, float lhs) {
		return Math.Abs(rhs - lhs) < Eps;
	}

	public static void ChangeTitle(string title) {
		WinTitleUtil.ChangeTitle(title);
	}

	public static Texture2D LoadTexture2DbyIo(string imageUrl) {
		byte[] bytes = Utils.ReadFile(imageUrl);
		Texture2D texture2D = new Texture2D((int)GlobalData.DefaultSize.x, (int)GlobalData.DefaultSize.y);
		texture2D.LoadImage(bytes);
		return texture2D;
	}
}
