using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class DisplayObjectManager : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        private Vector2 _offset;
		private static GameObject AlignLine = null;

        public RectTransform SelfRect;

		private void Start()
		{
			if(AlignLine == null)
				AlignLine = Instantiate<GameObject>(GlobalData.LinePrefab, GlobalData.DisplayObjectContainer.transform);
			AlignLine.GetComponent<RectTransform>().anchoredPosition = new Vector2(1000000, 0);
		}

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 pos = Utils.GetAnchoredPositionInContainer(Input.mousePosition) - _offset;
            Vector2 offset = pos - SelfRect.anchoredPosition;
            UpdateDisplayObjectPosition(SelfRect, transform.name, pos);
			Rectangle alignLine = GlobalData.ContainerManager.GetAlignLine(transform);
			if(alignLine != null) {
				RectTransform rt = AlignLine.GetComponent<RectTransform>();
				rt.anchoredPosition = DisplayObject.ConvertTo(new Vector2(alignLine.X, alignLine.Y));
				rt.sizeDelta = new Vector2(alignLine.Width, alignLine.Height);
			} else {
				AlignLine.GetComponent<RectTransform>().anchoredPosition = new Vector2(1000000, 0);
			}
            if (GlobalData.CurrentSelectDisplayObjectDic.Count == 1) return;
            foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
            {
                if (pair.Value == transform) continue;
                RectTransform rt = pair.Value.GetComponent<RectTransform>();
                UpdateDisplayObjectPosition(rt, pair.Key, rt.anchoredPosition + offset);
            }
        }

        private void UpdateDisplayObjectPosition(RectTransform rt, string name, Vector3 pos)
        {
            rt.anchoredPosition = pos;
            DisplayObject displayObjectData = GlobalData.Modules[GlobalData.CurrentModule].Find(element => element.Name.Equals(transform.name));
            if (displayObjectData == null) return;
            displayObjectData.X = DisplayObject.ConvertX(pos.x);
            displayObjectData.Y = DisplayObject.ConvertY(pos.y);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(transform.name);
            if (isSelect)
            {
                if (KeyboardEventManager.GetControl())
                    GlobalData.CurrentSelectDisplayObjectDic.Remove(transform.name);
            }
            else
            {
                if (!KeyboardEventManager.GetShift())
                    DeselectAllDisplayObject();
                GlobalData.AddCurrentSelectObject(GlobalData.CurrentModule, this.transform);
            }
            var mousePos = eventData.position;
            Vector2 offset;
            var isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(SelfRect, mousePos, eventData.enterEventCamera, out offset);
            if (isRect) _offset = offset;
        }

        public static bool DeSelectDisplayObject(Transform displayObject)
        {
            if (!displayObject) return false;
            if (!GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(displayObject.name)) return false;
            GlobalData.CurrentSelectDisplayObjectDic.Remove(displayObject.name);
            return true;
        }

        public static void DeselectAllDisplayObject()
        {
            GlobalData.CurrentSelectDisplayObjectDic.Clear();
        }
    }
}