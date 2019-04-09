using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class ContainerManager : MonoBehaviour
    {
        private static readonly Dictionary<string, Material> MaterialDic = new Dictionary<string, Material>();
        private static readonly Dictionary<string, Vector2> SizeDic = new Dictionary<string, Vector2>();

        private static readonly List<Transform> DisplayObjectPool = new List<Transform>();
    
        public Text ModuleNameText;
        public Text SelectedDisplayObjectText;
        public Slider ScaleSlider;

        private Transform GetDisplayObject() {
            int length = DisplayObjectPool.Count;
            if (length == 0) return Instantiate(GlobalData.DisplayObjectPrefab.transform, this.transform);
            Transform result = DisplayObjectPool[length - 1];
            DisplayObjectPool.RemoveAt(length - 1);
            return result;
        }

        public void RecycleDisplayObject(Transform displayObject) {
            if (!displayObject) return;
            DisplayObjectManager.DeSelectDisplayObject(displayObject);
            displayObject.GetComponentInChildren<Toggle>().isOn = false;
            displayObject.SetParent(null);
            DisplayObjectPool.Add(displayObject);
        }

        private void RecycleAllDisplayObject()
        {
            int count = GlobalData.CurrentDisplayObjects.Count;
            for(int idx = 0; idx < count; ++ idx) {
            
                RecycleDisplayObject(GlobalData.CurrentDisplayObjects[idx]);
            }
            GlobalData.CurrentDisplayObjects.Clear();
            GlobalData.CurrentDisplayObjectDic.Clear();
        }

        private void LoadAllDisplayObject()
        {
            if (GlobalData.CurrentModule == null) return;
            List<DisplayObject> displayObjectDataList = GlobalData.Modules[GlobalData.CurrentModule];
            int count = displayObjectDataList.Count;
            for (var idx = 0; idx < count; ++idx)
            {
                Transform displayObject = GetDisplayObject();
                displayObject.GetComponent<Image>().color = Color.clear;
                displayObject.SetParent(transform);
                displayObject.GetComponent<RectTransform>().localScale = Vector3.one;
                DisplayObject displayObjectData = displayObjectDataList[idx];
                displayObjectData.InvConvertTo(displayObject);
                GlobalData.CurrentDisplayObjects.Add(displayObject);
                GlobalData.CurrentDisplayObjectDic[$"{GlobalData.CurrentModule}_{displayObject.name}"] = displayObject;
            }
        }
    
        private void Start() {
// #if UNITY_EDITOR
// #if UNITY_EDITOR_WIN
//         AddDisplayObject("X:/Users/TankGq/Desktop/img.jpg", new Vector2(300f, 300f), Vector2.zero);
// #else
//         AddDisplayObject("/Users/Tank/Documents/OneDrive/Documents/icon.png", new Vector2(100f, 0f), Vector2.zero);
// #endif
// #endif
            GlobalData.CurrentSelectDisplayObjectDic.ObserveEveryValueChanged(dic => dic.Count)
                .Subscribe(count =>
                {
                    foreach (Transform displayObjectItem in GlobalData.CurrentDisplayObjects)
                        displayObjectItem.GetComponent<Toggle>().isOn = false;
                    if (count == 0) return;
                    foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic) {
                        pair.Value.GetComponent<Toggle>().isOn = true;
                    }
                });

            GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.CurrentModule)
                .Subscribe(module => {
                    RecycleAllDisplayObject();
                    if (string.IsNullOrEmpty(module))
                    {
                        ModuleNameText.text = "null";
                        return;
                    }
                    ModuleNameText.text = module;
                    Observable.Timer(TimeSpan.Zero)
                        .Subscribe(_ => {
                            RectTransform rt = ModuleNameText.GetComponent<RectTransform>();
                            RectTransform rt2 = SelectedDisplayObjectText.GetComponent<RectTransform>();
                            rt2.anchoredPosition = new Vector2(rt.anchoredPosition.x + rt.sizeDelta.x + 30, rt2.anchoredPosition.y);
                        });
                    GlobalData.CurrentSelectDisplayObjectDic.Clear();
                    // GetComponent<RectTransform>().localScale = Vector3.one;
                    ScaleSlider.value = 10f;
                    GetComponent<RectTransform>().localPosition = Vector2.zero;
                    LoadAllDisplayObject();
                });
            
            GlobalData.CurrentSelectDisplayObjectDic.ObserveEveryValueChanged(dic => dic.Count)
                .Subscribe(count => {
                    if(count == 0) {
                        SelectedDisplayObjectText.text = "null";
                        return;
                    }
                    string text = "";
                    foreach(var pair in GlobalData.CurrentSelectDisplayObjectDic) {
                        text += $"{pair.Value.name}, ";
                    }
                    SelectedDisplayObjectText.text = text.Substring(0, text.Length - 2);
                });
        }

        public static Texture2D LoadTexture2DbyIo(string imageUrl) {
            byte[] bytes = Utils.ReadFile(imageUrl);
            Texture2D texture2D = new Texture2D((int)GlobalData.DefaultSize.x, (int)GlobalData.DefaultSize.y);
            texture2D.LoadImage(bytes);
            return texture2D;
        }

        public Transform AddDisplayObject(string imageUrl, Vector2 pos, Vector2 size, string elementName = null) {
            if(string.IsNullOrEmpty(GlobalData.CurrentModule)) {
                if(GlobalData.Modules.Count == 0) {
                    DialogManager.ShowInfo("请先创建一个 module", 320);
                    return null;
                }
                DialogManager.ShowInfo("请先打开一个 module", 320);
                return null;
            }
            Material material = null;
            if (!string.IsNullOrEmpty(imageUrl)) {
                if (MaterialDic.ContainsKey(imageUrl)) {
                    material = MaterialDic[imageUrl];
                    if (size == Vector2.zero) size = SizeDic[imageUrl];
                } else {
                    Texture2D texture2 = LoadTexture2DbyIo(imageUrl);
                    material = new Material(GlobalData.DefaultShader) {
                        mainTexture = texture2
                    };
                    MaterialDic[imageUrl] = material;
                    SizeDic[imageUrl] = new Vector2(texture2.width, texture2.height);
                    if (size == Vector2.zero) size = SizeDic[imageUrl];
                }
            }
            Transform imageElement = GetDisplayObject();
            imageElement.SetParent(this.transform);
            imageElement.GetComponent<RectTransform>().localScale = Vector3.one;
            int instanceId = imageElement.GetInstanceID();
            imageElement.name = string.IsNullOrEmpty(elementName)
                ? (string.IsNullOrEmpty(imageUrl) ? GlobalData.DefaultName + (++GlobalData.UniqueId) : Utils.GetFileNameInPath(imageUrl))
                : elementName;
            string displayObjectKey = $"{GlobalData.CurrentModule}_{imageElement.name}";
            GlobalData.DisplayObjectPathDic[displayObjectKey] = imageUrl;
            GlobalData.CurrentDisplayObjects.Add(imageElement);
            GlobalData.CurrentDisplayObjectDic[displayObjectKey] = imageElement;
            Image image = imageElement.GetComponent<Image>();
            image.material = material;
            image.color = (material ? Color.white : Color.clear);
            RectTransform rect = imageElement.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size.x, size.y);
            pos += GlobalData.OriginPoint;
            pos.y = - pos.y;
            rect.anchoredPosition = pos;
            GlobalData.Modules[GlobalData.CurrentModule].Add(DisplayObject.ConvertTo(imageElement));
            return imageElement;
        }
    
        public void RemoveSelectedDisplayObject()
        {
            int count = GlobalData.CurrentSelectDisplayObjectDic.Count;
            if (count == 0) {
//            MessageBoxUtil.Show("请先选择要删除的对象");
                DialogManager.ShowInfo("请先选择要删除的对象");
                return;
            }

            if (GlobalData.CurrentDisplayObjects == null) return;
            int length = GlobalData.CurrentDisplayObjects.Count;
            foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic) {
                RecycleDisplayObject(pair.Value);
                if (GlobalData.DisplayObjectPathDic.ContainsKey(pair.Key))
                    GlobalData.DisplayObjectPathDic.Remove(pair.Key);
                var idx = GlobalData.CurrentDisplayObjects.FindIndex(0, element => element.name.Equals(pair.Value.name));
                if (idx < 0 || idx >= length) continue;
                GlobalData.CurrentDisplayObjects.RemoveAt(idx);
                GlobalData.CurrentDisplayObjectDic.Remove(pair.Key);
                --length;
            }

            GlobalData.CurrentSelectDisplayObjectDic.Clear();
        }

        public static void CreateModule() {
            DialogManager.ShowGetValue("请输入 module 名:", "module", txt => {
                if (string.IsNullOrWhiteSpace(txt))
                {
                    DialogManager.ShowError("请输入正确的 module", 0, 0);
                    return;
                }
                if(GlobalData.Modules.ContainsKey(txt)) {
                    DialogManager.ShowError("module 已存在", 0, 0);
                    return;
                }
                UpdateCurrentDisplayObjectData();
                GlobalData.CurrentModule = txt;
                GlobalData.Modules[txt] = new List<DisplayObject>();
                GlobalData.ModuleNames.Add(txt);
            });
        }

        public static void UpdateCurrentDisplayObjectData() {
            Debug.Log($"GlobalData.CurrentModule: {GlobalData.CurrentModule}, GlobalData.Modules: {GlobalData.Modules}");
            if(string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
            List<DisplayObject> displayObjectDataList = GlobalData.Modules[GlobalData.CurrentModule];
            int count = GlobalData.CurrentDisplayObjects.Count;
            for(int idx = 0; idx < count; ++ idx) {
                Transform displayObject = GlobalData.CurrentDisplayObjects[idx];
                DisplayObject displayObjectData = displayObjectDataList[idx];
                displayObjectData.Name = displayObject.name;
                RectTransform rt = displayObject.GetComponent<RectTransform>();
                Vector2 pos = rt.anchoredPosition;
                Vector2 size = rt.sizeDelta;
                displayObjectData.X = DisplayObject.ConvertX(pos.x);
                displayObjectData.Y = DisplayObject.ConvertY(pos.y);
                displayObjectData.Width = size.x;
                displayObjectData.Height = size.y;
            }
        }
    }
}
