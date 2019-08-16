using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class QuickTipManager : MonoBehaviour
	{
		private static readonly List<Transform> QuickTipPool = new List<Transform>();
		private static readonly Queue<Transform> QuickTipList = new Queue<Transform>();
		private static readonly Dictionary<Transform, object> TweenerDic = new Dictionary<Transform, object>();

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
			QuickTipPool.Add(tip);
		}

		public static void ShowQuickTip(string message)
		{
			Transform quickTip = GetQuickTip();
			if (quickTip == null) return;
			Text text = quickTip.GetComponent<Text>();
			var tweener = DOTween.ToAlpha(() => text.color, (cl) => text.color = cl, 0, 2);
			tweener.onComplete = () => OnAlphaChangeComplete(quickTip);
			TweenerDic.Add(quickTip, tweener);
		}

		private static void OnAlphaChangeComplete(Transform quickTip) {
			
		}
	}
}
