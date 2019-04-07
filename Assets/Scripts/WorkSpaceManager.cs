using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    public class WorkSpaceManager : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if(Input.GetMouseButtonDown(0))
                OnMouseLeftButtonDown();
            else if(Input.GetMouseButton(1))
                OnMouseRightButtonDown();
        }

        private void OnMouseLeftButtonDown()
        {
            DisplayObjectManager.DeselectAllDisplayObject();
        }

        private void OnMouseRightButtonDown()
        {
            Debug.Log("OnMouseRightDown");
        }
    }
}