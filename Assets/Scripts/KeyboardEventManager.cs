using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class KeyboardEventManager : MonoBehaviour
    {
        public ContainerManager ContainerManager;
        public ScrollRect ContainerScrollRect;
        public RectTransform ContainerRect;
        public float ContainerKeyMoveSensitivity;
        public Slider ScaleSlider;

        public static bool IsShiftDown()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public static bool IsControlDown() {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        public static bool IsEnterDown()
        {
            return Input.GetKey(KeyCode.KeypadEnter);
        }

        private void Start()
        {
            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Backspace) && GlobalData.CurrentSelectDisplayObjectDic.Count != 0 && ! Utils.IsFocusOnInputText())
                .Subscribe(_ => ContainerManager.RemoveSelectedDisplayObject());
            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape) && GlobalData.CurrentSelectDisplayObjectDic.Count != 0 && ! Utils.IsFocusOnInputText())
                .Subscribe(_ => GlobalData.CurrentSelectDisplayObjectDic.Clear());
            Observable.EveryUpdate()
                .Where(_ => IsControlDown() && Math.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.001f)
                .Subscribe(_ => ScaleSlider.value += Input.GetAxis("Mouse ScrollWheel") * 10);
            Observable.EveryUpdate()
                .Subscribe(_ => {
                    if(Input.GetMouseButton(0))
                    {
                        bool canMove = !GlobalData.IsDragGui;
                        ContainerScrollRect.horizontal = canMove;
                        ContainerScrollRect.vertical = canMove;
                    } else {
                        bool isShiftDown = IsShiftDown();
                        ContainerScrollRect.horizontal = isShiftDown;
                        ContainerScrollRect.vertical = ! isShiftDown;
                        ContainerScrollRect.scrollSensitivity = Math.Abs(ContainerScrollRect.scrollSensitivity) * (isShiftDown ? -1 : 1);
                    }
                    Vector2 pos = ContainerRect.anchoredPosition;
                    if(Input.GetKey(KeyCode.UpArrow))
                        pos += Vector2.up * ContainerKeyMoveSensitivity;
                    if(Input.GetKey(KeyCode.DownArrow))
                        pos += Vector2.down * ContainerKeyMoveSensitivity;
                    else if(Input.GetKey(KeyCode.LeftArrow))
                        pos += Vector2.left * ContainerKeyMoveSensitivity;
                    else if(Input.GetKey(KeyCode.RightArrow))
                        pos += Vector2.right * ContainerKeyMoveSensitivity;
                    ContainerRect.anchoredPosition = pos;
                });
            Observable.EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.D))
                .Sample(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    Debugger.ShowDebugging = !Debugger.ShowDebugging;
                    Debug.Log($"Debugger.ShowDebugging: {Debugger.ShowDebugging}");
                });
        }
    }
}
