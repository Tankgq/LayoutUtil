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

        public void OnCreateModuleButtonClick() {
            ContainerManager.CreateModule();
        }

        public void OnAddButtonClick()
        {
            ContainerManager.AddDisplayObject(null, Vector2.zero, GlobalData.DefaultSize);
        }

        public void OnRemoveButtonClick()
        {
            if(GlobalData.CurrentSelectDisplayObjectDic.Count > 0) {
                ContainerManager.RemoveSelectedDisplayObject();
            } else {
                ContainerManager.CheckRemoveCurrentModule();
            }
        }

        public void OnUpButtonClick()
        {
            if(GlobalData.CurrentSelectDisplayObjectDic.Count > 0) {
                ContainerManager.MoveCurrentSelectDisplayObjectUp();
            } else {
                HierarchyManager.MoveCurrentModuleUp();
            }
        }

        public void OnDownButtonClick()
        {
            if(GlobalData.CurrentSelectDisplayObjectDic.Count > 0) {
                ContainerManager.MoveCurrentSelectDisplayObjectDown();
            } else {
                HierarchyManager.MoveCurrentModuleDown();
            }
        }

        public void OnImportButtonClick()
        {
            string filePath = OpenFileUtil.OpenFile("json 文件(*.json)\0*.json");
            if (string.IsNullOrEmpty(filePath)) return;
            byte[] bytes = Utils.ReadFile(filePath);
            string jsonStr = System.Text.Encoding.UTF8.GetString(bytes);
            GlobalData.CurrentDisplayObjectDic.Clear();
            GlobalData.CurrentDisplayObjects.Clear();
            GlobalData.CurrentSelectDisplayObjectDic.Clear();
            GlobalData.ModuleNames.Clear();
            foreach (var pair in GlobalData.Modules)
                pair.Value.Clear();
            GlobalData.Modules.Clear();
            GlobalData.CurrentModule = null;
            Observable.Timer(TimeSpan.Zero)
                .Subscribe(_ =>
                {
                    try {
                        List<Module> modules = JsonConvert.DeserializeObject<List<Module>>(jsonStr);
                        int count = modules.Count;
                        for(int idx = 0; idx < count; ++ idx) {
                            Module module = modules[idx];
                            GlobalData.ModuleNames.Add(module.Name);
                            GlobalData.Modules[module.Name] = module.DisplayObjects;
                        }
                    } catch (Exception e) {
                        DialogManager.ShowError($"导入失败({e})");
                    }
                });
        }

        public void OnExportButtonClick()
        {
            string filePath = SaveFileUtil.SaveFile("json 文件(*.json)\0*.json");
            if (string.IsNullOrEmpty(filePath)) return;
            ContainerManager.UpdateCurrentDisplayObjectData();
            List<Module> modules = new List<Module>();
            int count = GlobalData.ModuleNames.Count;
            for(int idx = 0; idx < count; ++ idx) {
                Module module = new Module();
                module.Name = GlobalData.ModuleNames[idx];
                module.DisplayObjects = GlobalData.Modules[module.Name];
                modules.Add(module);
            }
            string jsonString = JsonConvert.SerializeObject(modules, Formatting.Indented);
            bool result = Utils.WriteFile(filePath, System.Text.Encoding.UTF8.GetBytes(jsonString));
            if (result) DialogManager.ShowInfo($"成功导出到 {filePath}");
            else DialogManager.ShowError($"导出失败", 0, 0);
        }

        public void OnHelpButtonClick() {
            DialogManager.ShowInfo(
                "<color=yellow>1.</Color> BackSpace 是删除按钮的快捷键, Esc 可以清空当前选择的对象\n" +
                "<color=yellow>2.</Color> Ctrl + 鼠标滚轮可以放大或缩小工作空间, Shift + 鼠标滚轮可以水平滚动工作空间, 方向键也可以移动工作空间\n" +
                "<color=yellow>3.</Color> Shift + 鼠标左键可以增加当前选择的 displayObject, Ctrl + 鼠标左键可以取消点选\n" +
                "<color=yellow>4.</Color> 直接将图片拖入工作空间可以直接导入图片, 如果按住 Alt 拖入图片可将图片加载到相应的 displayObject 上\n" +
                "<color=yellow>5.</Color> Ctrl + N 可以快速添加一个 displayObject 到鼠标所在位置\n" +
                "<color=yellow>6.</Color> Shift + Alt + F 可以切换屏幕显示的模式\n" +
                "<color=yellow>7.</Color> Shift + Alt + D 可显示调试窗口"
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