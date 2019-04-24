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

		public static bool IsControlDown()
		{
			return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		}

		public static bool IsAltDown()
		{
			return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
		}

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
				.Where(_ => IsControlDown() && Math.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.001f)
				.Subscribe(_ => ScaleSlider.value += Input.GetAxis("Mouse ScrollWheel") * 10);
			Observable.EveryUpdate()
				.Subscribe(_ =>
				{
					if (Input.GetMouseButton(0))
					{
						bool canMove = !GlobalData.IsDragGui;
						ContainerScrollRect.horizontal = canMove;
						ContainerScrollRect.vertical = canMove;
					}
					else if (IsControlDown())
					{
						ContainerScrollRect.horizontal = false;
						ContainerScrollRect.vertical = false;
					}
					else
					{
						bool isShiftDown = IsShiftDown();
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
						Debug.Log($"Container x: {ContainerRect.anchoredPosition.x}, y: {ContainerRect.anchoredPosition.y}");
					}
				});
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.N) && IsControlDown())
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
				.Where(_ => Input.GetKeyDown(KeyCode.D) && IsShiftDown() && IsAltDown())
				.Sample(TimeSpan.FromMilliseconds(100))
				.Subscribe(_ =>
				{
					Debugger.ShowDebugging = !Debugger.ShowDebugging;
					Debug.Log($"Debugger.ShowDebugging: {Debugger.ShowDebugging}");
				});
			Observable.EveryUpdate()
					  .Where(_ => Input.GetKeyDown(KeyCode.F) && IsShiftDown() && IsAltDown())
					  .Subscribe(_ => Screen.fullScreen = !Screen.fullScreen);
		}
	}
}
