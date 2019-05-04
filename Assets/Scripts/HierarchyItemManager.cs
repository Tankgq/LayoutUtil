﻿using System.Collections;
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
            if (HierarchyManager.InSearchMode())
            {
                string module = HierarchyManager.GetModuleName(transform.GetSiblingIndex());
                if (string.IsNullOrEmpty(module)) return;
                GlobalData.CurrentModule = module;
                string name = Utils.CancelHighlight(transform.name);
                Observable.TimerFrame(1, FrameCountType.EndOfFrame)
                          .Subscribe(_ =>
                          {
                              Dictionary<string, Transform> dic = GlobalData.CurrentDisplayObjectDic;
                              GlobalData.CurrentSelectDisplayObjectDic.Add(name, GlobalData.CurrentDisplayObjectDic[name]);
                          });
                return;
            }
            if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
            Transform displayObject = GlobalData.CurrentDisplayObjectDic[gameObject.name];
            if (displayObject.parent == null) return;
            bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(gameObject.name);
            if (isSelect)
            {
                if (KeyboardEventManager.GetControl())
                {
                    GlobalData.CurrentSelectDisplayObjectDic.Remove(gameObject.name);
                }
            }
            else
            {
                if (!KeyboardEventManager.GetShift())
                    DeselectAllDisplayObjectItem();
                GlobalData.CurrentSelectDisplayObjectDic.Add(displayObject.name, displayObject);
                MessageBroker.Send(MessageBroker.UPDATE_SELECT_DISPLAY_OBJECT);
            }
        }

        public static void DeselectAllDisplayObjectItem()
        {
            GlobalData.CurrentSelectDisplayObjectDic.Clear();
        }
    }
}