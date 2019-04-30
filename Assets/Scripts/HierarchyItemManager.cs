using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class HierarchyItemManager : MonoBehaviour, IPointerDownHandler
	{
		public int ItemType = 0;

		public void OnPointerDown(PointerEventData eventData)
		{
			if (ItemType == 1)
			{
				ContainerManager.UpdateCurrentDisplayObjectData();
				if (gameObject.name.Equals(GlobalData.CurrentModule))
				{
					GlobalData.CurrentModule = null;
					return;
				}
				GlobalData.CurrentModule = gameObject.name;
				return;
			}
			if (ItemType != 2) return;
			if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
			Transform displayObject = GlobalData.CurrentDisplayObjectDic[gameObject.name];
			if (displayObject.parent == null) return;
			bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(gameObject.name);
			if (isSelect)
			{
				if (KeyboardEventManager.GetControl())
					GlobalData.CurrentSelectDisplayObjectDic.Remove(gameObject.name);
			}
			else
			{
				if (!KeyboardEventManager.GetShift())
					DeselectAllDisplayObjectItem();
				GlobalData.AddCurrentSelectObject(GlobalData.CurrentModule, displayObject);
			}
		}

		public static void DeselectAllDisplayObjectItem()
		{
			GlobalData.CurrentSelectDisplayObjectDic.Clear();
		}
	}
}