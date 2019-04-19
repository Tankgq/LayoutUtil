﻿using System;
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

        public void OnCreateModuleButtonClick() {
            ContainerManager.CreateModule();
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