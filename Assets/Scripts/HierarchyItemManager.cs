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
            string displayObjectKey = $"{GlobalData.CurrentModule}_{gameObject.name}";
            Transform displayObject = GlobalData.CurrentDisplayObjectDic[displayObjectKey];
            if (displayObject.parent == null) return;
            bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(displayObjectKey);
            if (isSelect) {
                if (KeyboardEventManager.IsControlDown())
                    GlobalData.CurrentSelectDisplayObjectDic.Remove(displayObjectKey);
            } else {
                if (!KeyboardEventManager.IsShiftDown())
                    DeselectAllDisplayObjectItem();
                GlobalData.AddCurrentSelectObject(GlobalData.CurrentModule, displayObject);
            }
        }
        
        public static void DeselectAllDisplayObjectItem() {
            GlobalData.CurrentSelectDisplayObjectDic.Clear();
        }
    }
}