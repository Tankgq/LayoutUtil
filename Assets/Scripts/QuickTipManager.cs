using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class QuickTipManager : MonoBehaviour
{
	private static readonly List<Transform> QuickTipPool = new List<Transform>();
	private static readonly List<Transform> QuickTipList = new List<Transform>();
	private static readonly Dictionary<Transform, TweenerCore<Color, Color, ColorOptions>> TweenDic
		= new Dictionary<Transform, TweenerCore<Color, Color, ColorOptions>>();

	private static Transform GetQuickTip()
	{
		int length = QuickTipPool.Count;
		if (length == 0) return Instantiate(GlobalData.QuickTipPrefab.transform, GlobalData.QuickTipContainer.transform);
		Transform result = QuickTipPool[length - 1];
		result.gameObject.SetActive(true);
		QuickTipPool.RemoveAt(length - 1);
		return result;
	}

	private static readonly Color QuickTipColor = GlobalData.QuickTipPrefab.GetComponent<Graphic>().color;

	private static void RecycleTip(Transform tip)
	{
		if (!tip) return;
		tip.gameObject.SetActive(false);
		tip.GetComponent<Text>().text = "QuickTip";
		tip.GetComponent<Graphic>().color = QuickTipColor;
		QuickTipPool.Add(tip);
	}

	public static void ShowQuickTip(string message, float duration = GlobalData.QuickTipDuration)
	{
		if (QuickTipList.Count >= GlobalData.QuickTipMaxCount)
			RemoveTopQuickTip(QuickTipList.Count - GlobalData.QuickTipMaxCount + 1);

		AddQuickTipAtTail(message, duration);
	}
	
	private static void OnAlphaChangeComplete(Transform quickTip)
	{
		if (quickTip == null || QuickTipList.Count == 0 || quickTip != QuickTipList[0])
			return;
		RemoveTopQuickTip(1);
	}

	private static void AddQuickTipAtTail(string message, float duration = GlobalData.QuickTipDuration) {
		Transform quickTip = GetQuickTip();
		if (quickTip == null) return;
		QuickTipList.Add(quickTip);
		RectTransform rect;
		if (QuickTipList.Count == 1)
		{
			rect = QuickTipList[0].GetComponent<RectTransform>();
			rect.position = Vector3.zero;
		}
		else
		{
			rect = QuickTipList[QuickTipList.Count - 2].GetComponent<RectTransform>();
			Vector3 pos = rect.anchoredPosition;
			pos.y += rect.sizeDelta.y;
			rect = quickTip.GetComponent<RectTransform>();
			rect.anchoredPosition = pos;
		}
		Text text = quickTip.GetComponent<Text>();
		text.text = message;
		TweenerCore<Color, Color, ColorOptions> tween = text.DOFade(0, duration);
		tween.onComplete = () => OnAlphaChangeComplete(quickTip);
		if (TweenDic.ContainsKey(quickTip))
		{
			TweenDic[quickTip]?.Kill();
			TweenDic[quickTip] = tween;
		}
		else
			TweenDic.Add(quickTip, tween);
	}

	private static void RemoveTopQuickTip(int removeCount)
	{
		RectTransform rect = QuickTipList[0].GetComponent<RectTransform>();
		float deltaHeight = -rect.sizeDelta.y * removeCount;
		for (int idx = 0; idx < removeCount; ++idx)
		{
			Transform quickTip = QuickTipList[idx];
			TweenerCore<Color, Color, ColorOptions> tween = TweenDic[quickTip];
			if (tween != null)
			{
				tween.Kill();
				TweenDic[quickTip] = null;
			}
			RecycleTip(quickTip);
		}
		for (int idx = removeCount; idx < QuickTipList.Count; ++idx)
		{
			QuickTipList[idx - removeCount] = QuickTipList[idx];
			rect = QuickTipList[idx].GetComponent<RectTransform>();
			Vector3 pos = rect.anchoredPosition;
			pos.y += deltaHeight;
			rect.anchoredPosition = pos;
		}
		QuickTipList.RemoveRange(QuickTipList.Count - removeCount, removeCount);
	}
}