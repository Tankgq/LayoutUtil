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

		public static bool GetShift()
		{
			return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		}

		public static bool GetShiftDown()
		{
			return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
		}

		public static bool GetControl()
		{
			return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		}

		public static bool GetControlDown()
		{
			return Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
		}

		public static bool GetAlt()
		{
			return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		}

		public static bool GetAltDown()
		{
			return Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);
		}

		private Vector3 _ContainerOffset = Vector3.zero;

		private void Start()
		{
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.Backspace) && GlobalData.CurrentSelectDisplayObjectDic.Count != 0 && !Utils.IsFocusOnInputText())
				.Subscribe(_ =>
				{
					if (GlobalData.CurrentSelectDisplayObjectDic.Count > 0)
						ContainerManager.RemoveSelectedDisplayObject();
					else
						ContainerManager.CheckRemoveCurrentModule();
				});
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.Escape) && GlobalData.CurrentSelectDisplayObjectDic.Count != 0 && !Utils.IsFocusOnInputText())
				.Subscribe(_ => GlobalData.CurrentSelectDisplayObjectDic.Clear());
			Observable.EveryUpdate()
				.Where(_ => GetControl() && Math.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.001f)
				.Subscribe(_ => ScaleSlider.value += Input.GetAxis("Mouse ScrollWheel") * 10);
			Observable.EveryUpdate()
				.Subscribe(_ =>
				{
					/* if (Input.GetMouseButton(0))
					{
						bool canMove = !GlobalData.IsDragGui;
						ContainerScrollRect.horizontal = canMove;
						ContainerScrollRect.vertical = canMove;
					}
					else */if (GetControl() || Input.GetMouseButton(0))
					{
						ContainerScrollRect.horizontal = false;
						ContainerScrollRect.vertical = false;
					}
					else if(Input.GetMouseButton(2)) {
						ContainerScrollRect.horizontal = true;
						ContainerScrollRect.vertical = true;
						if(Input.GetMouseButtonDown(2)) {
							_ContainerOffset = ContainerRect.anchoredPosition3D - Input.mousePosition;
						} else {
							ContainerRect.anchoredPosition3D = Input.mousePosition + _ContainerOffset;
						}
					}
					else
					{
						bool isShiftDown = GetShift();
						ContainerScrollRect.horizontal = isShiftDown;
						ContainerScrollRect.vertical = !isShiftDown;
						ContainerScrollRect.scrollSensitivity = Math.Abs(ContainerScrollRect.scrollSensitivity) * (isShiftDown ? -1 : 1);
					}
					Vector2 delta = Vector2.zero;
					if (Input.GetKey(KeyCode.UpArrow))
					{
						delta += Vector2.up * ContainerKeyMoveSensitivity;
					}
					if (Input.GetKey(KeyCode.DownArrow))
					{
						delta += Vector2.down * ContainerKeyMoveSensitivity;
					}
					if (Input.GetKey(KeyCode.LeftArrow))
					{
						delta += Vector2.left * ContainerKeyMoveSensitivity;
					}
					if (Input.GetKey(KeyCode.RightArrow))
					{
						delta += Vector2.right * ContainerKeyMoveSensitivity;
					}
					if (!Utils.IsFocusOnInputText() && (delta.x != 0 || delta.y != 0))
					{
						Vector2 pos = ContainerRect.anchoredPosition;
						ContainerRect.anchoredPosition = pos + delta;
					}
				});
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.N) && GetControl())
				.Subscribe(_ =>
				{
					RectTransform rt = ContainerManager.GetComponent<RectTransform>();
					Vector2 pos = rt.anchoredPosition;
					Vector2 mousePos = Input.mousePosition;
					mousePos.y = Screen.height - mousePos.y;
					pos.x = mousePos.x - pos.x;
					pos.y = mousePos.y + pos.y;
					pos /= rt.localScale.x;
					pos -= GlobalData.OriginPoint;
					ContainerManager.AddDisplayObject(null, pos, GlobalData.DefaultSize);
				});
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.D) && GetShift() && GetAlt())
				.Sample(TimeSpan.FromMilliseconds(100))
				.Subscribe(_ =>
				{
					Debugger.ShowDebugging = !Debugger.ShowDebugging;
					Debug.Log($"Debugger.ShowDebugging: {Debugger.ShowDebugging}");
				});
			Observable.EveryUpdate()
					  .Where(_ => Input.GetKeyDown(KeyCode.F) && GetShift() && GetAlt())
					  .Subscribe(_ => Screen.fullScreen = !Screen.fullScreen);
		}
	}
}
