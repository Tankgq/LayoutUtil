using System.Collections;
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
	private static readonly Dictionary<Transform, TweenerCore<Color, Color, ColorOptions>> TweenerDic
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

	private static void RecycleTip(Transform tip)
	{
		if (!tip) return;
		tip.gameObject.SetActive(false);
		tip.GetComponent<Text>().text = "QuickTip";
		Color color = tip.GetComponent<Graphic>().color;
		color.a = 255;
		tip.GetComponent<Graphic>().color = color;
		QuickTipPool.Add(tip);
	}

	public static void ShowQuickTip(string message)
	{
		if (QuickTipList.Count >= GlobalData.QuickTipMaxCount)
			RemoveTopQuickTip(QuickTipList.Count - GlobalData.QuickTipMaxCount + 1);

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
		TweenerCore<Color, Color, ColorOptions> tweener = DOTween.ToAlpha(() => text.color, (cl) => text.color = cl, 0, 3);
		tweener.onComplete = () => OnAlphaChangeComplete(quickTip);
		if (TweenerDic.ContainsKey(quickTip))
		{
			if (TweenerDic[quickTip] != null)
				TweenerDic[quickTip].Kill();
			TweenerDic[quickTip] = tweener;
		}
		else
			TweenerDic.Add(quickTip, tweener);
	}

	private static void OnAlphaChangeComplete(Transform quickTip)
	{
		if (quickTip == null || QuickTipList.Count == 0 || quickTip != QuickTipList[0])
			return;
		RemoveTopQuickTip(1);
	}

	private static void RemoveTopQuickTip(int removeCount)
	{
		RectTransform rect = QuickTipList[0].GetComponent<RectTransform>();
		float deltaHeight = -rect.sizeDelta.y * removeCount;
		for (int idx = 0; idx < removeCount; ++idx)
		{
			Transform quickTip = QuickTipList[idx];
			TweenerCore<Color, Color, ColorOptions> tweener = TweenerDic[quickTip];
			if (tweener != null)
			{
				tweener.Kill();
				TweenerDic[quickTip] = null;
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