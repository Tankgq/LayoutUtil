using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class TipManager : MonoBehaviour
    {
        public string TipInfo = "TipInfo";
        public int OffsetX = 0;
        public int OffsetY = 0;
        public bool IsOnLeft = false;

        private static readonly List<Transform> TipPool = new List<Transform>();

        private static Transform GetTip()
        {
            int length = TipPool.Count;
            if (length == 0) return Instantiate(GlobalData.TipPrefab.transform, GlobalData.RootCanvas.transform);
            Transform result = TipPool[length - 1];
            result.gameObject.SetActive(true);
            TipPool.RemoveAt(length - 1);
            return result;
        }

        private static void RecycleTip(Transform tip)
        {
            if (!tip) return;
            tip.gameObject.SetActive(false);
            tip.GetComponentInChildren<Text>().text = "TipInfo";
            TipPool.Add(tip);
        }

        private Transform Tip = null;
        private IDisposable _disposable = null;
        void Awake()
        {
            Graphic graphic = transform.GetComponentInChildren<Graphic>();
            var trigger = gameObject.AddComponent<ObservableLongPointerDownTrigger>();
            trigger.OnLongHoverAsObservable()
                   .Where(_ => !Tip)
                   .Subscribe(_ =>
                   {
                       Tip = GetTip();
                       _disposable = Tip.GetComponent<Image>()
                                        .OnPointerExitAsObservable()
                                        .Subscribe(__ => RecycleAndCleanTip());
                       Tip.GetComponentInChildren<Text>().text = TipInfo;
                       RectTransform rt = Tip.GetComponent<RectTransform>();
                       Vector2 pos = Utils.GetAnchoredPositionInCanvas(transform);
                       pos.x += OffsetX;
                       if(IsOnLeft) {
                           pos.x += transform.GetComponent<RectTransform>().rect.width;
                           // 不移到屏幕外的话就会因为一开始鼠标停留在 Tip 上, 然后 Tip 因为下面的定时器移除一段距离,
                           // 如果这时候鼠标正好不在 Tip 上 Tip 会被关掉, 表现出来就是 Tip 闪一下就消失了.
                           // 如果是通过 setActive 来处理这个问题, 则会导致 Tip 的大小没有重新计算
                           pos.x += 1000000;
                           Observable.Timer(TimeSpan.Zero).Subscribe(__ => {
                               Debug.Log(rt.rect.width);
                               pos.x -= 1000000;
                               pos.x += rt.rect.width;
                               rt.anchoredPosition = pos;
                           });
                       }
                       pos.y += OffsetY;
                       rt.anchoredPosition = pos;
                   });
            graphic.OnPointerExitAsObservable()
                .Where(_ => Tip)
                .Subscribe(_ =>
                {
                    if (Utils.IsPointOverGameObject(Tip.gameObject))
                    {
                        return;
                    }
                    RecycleAndCleanTip();
                });
        }

        public void RecycleAndCleanTip()
        {
            if (_disposable != null)
            {
                _disposable.Dispose();
                _disposable = null;
            }
            RecycleTip(Tip);
            Tip = null;
        }
    }
}