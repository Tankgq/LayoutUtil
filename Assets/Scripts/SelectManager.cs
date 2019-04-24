using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class SelectManager : MonoBehaviour
    {
        private GameObject SelectGameObject = null;
        private RectTransform SelectRt = null;
        private Vector3 _startPos = Vector3.zero;

        private void OnBeginDrag()
        {
            Debug.Log("OnBeginDrag");
            if (!SelectGameObject)
            {
                SelectGameObject = Instantiate(GlobalData.SelectPrefab, GlobalData.DisplayObjectContainer.transform);
            }
            SelectGameObject.SetActive(true);
            if (!SelectRt) SelectRt = SelectGameObject.GetComponent<RectTransform>();
            _startPos = Input.mousePosition;
        }

        private void OnDrag()
        {
            Vector2 size = Input.mousePosition - _startPos;
            Debug.Log(size);
            SelectRt.sizeDelta = size;
        }

        private void OnEndDrag()
        {
            if (SelectGameObject)
                SelectGameObject.SetActive(false);
        }

        void Start()
        {
            GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => Input.mousePosition)
                .Subscribe(_ =>
                {
                    if (!Utils.IsPointOverGameObject(GlobalData.DisplayObjectContainer))
                    {
                        // OnEndDrag();
                        return;
                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        OnBeginDrag();
                        return;
                    }
                    if(Input.GetMouseButton(0) && SelectGameObject && SelectGameObject.activeSelf)
                        OnDrag();
                    // else OnEndDrag();
                }
                );
        }
    }
}