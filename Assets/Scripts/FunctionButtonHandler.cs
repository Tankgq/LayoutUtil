using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class FunctionButtonHandler : MonoBehaviour
	{
		public ContainerManager ContainerManager;
		public HierarchyManager HierarchyManager;
		public RectTransform ContainerRect;
		public Slider ScaleSlider;

		public void OnCreateModuleButtonClick()
		{
			ContainerManager.CreateModule();
		}

		public void OnAddButtonClick()
		{
			ContainerManager.AddDisplayObject(null, Element.InvConvertTo(GlobalData.OriginPoint), GlobalData.DefaultSize);
		}

		public void OnRemoveButtonClick()
		{
			ContainerManager.RemoveSelectedDisplayObjectOrModules();
		}

		public void OnUpButtonClick()
		{
			if (GlobalData.CurrentSelectDisplayObjectDic.Count > 0)
			{
				ContainerManager.MoveCurrentSelectDisplayObjectUp();
			}
			else
			{
				HierarchyManager.MoveCurrentModuleUp();
			}
		}

		public void OnDownButtonClick()
		{
			if (GlobalData.CurrentSelectDisplayObjectDic.Count > 0)
			{
				ContainerManager.MoveCurrentSelectDisplayObjectDown();
			}
			else
			{
				HierarchyManager.MoveCurrentModuleDown();
			}
		}

		public void OnCopyButtonClick()
		{
			ContainerManager.ExportCurrentModule();
		}

		public void OnImportButtonClick()
		{
			ContainerManager.CheckImportModules();
		}

		public void OnExportButtonClick()
		{
			ContainerManager.ExportModules();
		}

		public void OnHelpButtonClick()
		{
			DialogManager.ShowInfo(
				"<color=yellow>00.</color> 项目地址: https://github.com/Tankgq/LayoutUtil\n" +
				"<color=yellow>01.</color> BackSpace 是删除按钮的快捷键, Esc 可以清空当前选择的对象\n" +
				"<color=yellow>02.</color> Ctrl + 鼠标滚轮可以放大或缩小工作空间, Shift + 鼠标滚轮可以水平滚动工作空间, 方向键也可以移动工作空间\n" +
				"<color=yellow>03.</color> Shift + 鼠标左键可以增加当前选择的 displayObject, Ctrl + 鼠标左键可以取消点选\n" +
				"<color=yellow>04.</color> 直接将图片拖入工作空间可以直接导入图片, 如果按住 Alt 拖入图片可将图片加载到相应的 displayObject 上\n" +
				"<color=yellow>05.</color> Ctrl + N 可以快速添加一个 displayObject 到鼠标所在位置\n" +
				"<color=yellow>06.</color> Shift + Alt + F 可以切换屏幕显示的模式\n" +
				"<color=yellow>07.</color> Shift + Alt + D 可显示调试窗口\n" +
				"<color=yellow>08.</color> Ctrl + C 和 Ctrl + V 可复制和黏贴多个 displayObject\n" +
				"<color=yellow>09.</color> Ctrl + Z 和 Ctrl + Y 可以撤销或者重做\n" +
				"<color=yellow>10.</color> 按搜索栏的 G 或者 L 可以切换全局搜索或者局部搜索\n" +
				"<color=yellow>11.</color> 点击 displayObject 名称左边的眼睛可以显示或隐藏对象"
				, 800, 330
			);
		}

		public void OnScaleSliderValueChanged(float value)
		{
			value /= 10;
			ContainerRect.localScale = new Vector3(value, value, value);
			ScaleSlider.GetComponentInChildren<Text>().text = $"x{value:0.0}";
		}

		private void Start()
		{
			ScaleSlider.OnValueChangedAsObservable()
				//            .Sample(TimeSpan.FromMilliseconds(500))
				.Subscribe(OnScaleSliderValueChanged);
		}
	}
}