using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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

        private Transform GetDisplayObject()
        {
            int length = DisplayObjectPool.Count;
            if (length == 0) return Instantiate(GlobalData.DisplayObjectPrefab.transform, this.transform);
            Transform result = DisplayObjectPool[length - 1];
            DisplayObjectPool.RemoveAt(length - 1);
            return result;
        }

        public void RecycleDisplayObject(Transform displayObject)
        {
            if (!displayObject) return;
            DisplayObjectManager.DeSelectDisplayObject(displayObject);
            displayObject.GetComponentInChildren<Toggle>().isOn = false;
            displayObject.SetParent(null);
            DisplayObjectPool.Add(displayObject);
        }

        private void RecycleAllDisplayObject()
        {
            int count = GlobalData.CurrentDisplayObjects.Count;
            for (int idx = 0; idx < count; ++idx)
            {

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
                GlobalData.CurrentDisplayObjectDic[displayObject.name] = displayObject;
            }
        }

        private void Start()
        {
            GlobalData.CurrentSelectDisplayObjectDic.ObserveEveryValueChanged(dic => dic.Count)
                .Subscribe(count =>
                {
                    foreach (Transform displayObjectItem in GlobalData.CurrentDisplayObjects)
                        displayObjectItem.GetComponent<Toggle>().isOn = false;
                    if (count == 0) return;
                    foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
                    {
                        pair.Value.GetComponent<Toggle>().isOn = true;
                    }
                });

            GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.CurrentModule)
                .Subscribe(module =>
                {
                    RecycleAllDisplayObject();
                    if (string.IsNullOrEmpty(module))
                    {
                        ModuleNameText.text = "null";
                        return;
                    }
                    ModuleNameText.text = module;
                    Observable.Timer(TimeSpan.Zero)
                        .Subscribe(_ =>
                        {
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
                .Subscribe(count =>
                {
                    if (count == 0)
                    {
                        SelectedDisplayObjectText.text = "null";
                        return;
                    }
                    string text = "";
                    foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
                    {
                        text += $"{pair.Value.name}, ";
                    }
                    SelectedDisplayObjectText.text = text.Substring(0, text.Length - 2);
                });
        }

        public static Texture2D LoadTexture2DbyIo(string imageUrl)
        {
            byte[] bytes = Utils.ReadFile(imageUrl);
            Texture2D texture2D = new Texture2D((int)GlobalData.DefaultSize.x, (int)GlobalData.DefaultSize.y);
            texture2D.LoadImage(bytes);
            return texture2D;
        }

        public Transform AddDisplayObject(string imageUrl, Vector2 pos, Vector2 size, string elementName = null)
        {
            if (string.IsNullOrEmpty(GlobalData.CurrentModule))
            {
                if (GlobalData.Modules.Count == 0)
                {
                    DialogManager.ShowInfo("请先创建一个 module", 320);
                    return null;
                }
                DialogManager.ShowInfo("请先打开一个 module", 320);
                return null;
            }
            Material material = null;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                if (MaterialDic.ContainsKey(imageUrl))
                {
                    material = MaterialDic[imageUrl];
                    if (size == Vector2.zero) size = SizeDic[imageUrl];
                }
                else
                {
                    Texture2D texture2 = LoadTexture2DbyIo(imageUrl);
                    material = new Material(GlobalData.DefaultShader)
                    {
                        mainTexture = texture2
                    };
                    MaterialDic[imageUrl] = material;
                    SizeDic[imageUrl] = new Vector2(texture2.width, texture2.height);
                    if (size == Vector2.zero) size = SizeDic[imageUrl];
                }
            }
            if (!string.IsNullOrEmpty(imageUrl) && KeyboardEventManager.GetAlt())
            {
                int length = GlobalData.CurrentDisplayObjects.Count;
                for (int idx = length - 1; idx >= 0; --idx)
                {
                    Transform displayObject = GlobalData.CurrentDisplayObjects[idx];
                    if (Utils.IsPointOverGameObject(displayObject.gameObject))
                    {
                        string displayObjectKey2 = $"{GlobalData.CurrentModule}_{displayObject.name}";
                        GlobalData.DisplayObjectPathDic[displayObjectKey2] = imageUrl;
                        Image image2 = displayObject.GetComponent<Image>();
                        image2.material = material;
                        image2.color = Color.white;
                        return displayObject;
                    }
                }
                return null;
            }
            Transform imageElement = GetDisplayObject();
            imageElement.SetParent(this.transform);
            imageElement.GetComponent<RectTransform>().localScale = Vector3.one;
            imageElement.name = string.IsNullOrEmpty(elementName)
                ? (string.IsNullOrEmpty(imageUrl) ? GlobalData.DefaultName + (++GlobalData.UniqueId) : Utils.GetFileNameInPath(imageUrl))
                : elementName;
            string displayObjectKey = $"{GlobalData.CurrentModule}_{imageElement.name}";
            if (GlobalData.CurrentDisplayObjectDic.ContainsKey(imageElement.name))
            {
                imageElement.name = imageElement.name + (++GlobalData.UniqueId);
                displayObjectKey = $"{GlobalData.CurrentModule}_{imageElement.name}";
            }
            GlobalData.DisplayObjectPathDic[displayObjectKey] = imageUrl;
            GlobalData.CurrentDisplayObjects.Add(imageElement);
            GlobalData.CurrentDisplayObjectDic[imageElement.name] = imageElement;
            Image image = imageElement.GetComponent<Image>();
            image.material = material;
            image.color = (material ? Color.white : Color.clear);
            RectTransform rect = imageElement.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size.x, size.y);
            pos = DisplayObject.ConvertTo(pos);
            rect.anchoredPosition = pos;
            GlobalData.Modules[GlobalData.CurrentModule].Add(DisplayObject.ConvertTo(imageElement));
            return imageElement;
        }

        public void RemoveSelectedDisplayObjectOrModules()
        {
            if (GlobalData.CurrentSelectDisplayObjectDic.Count > 0)
                RemoveSelectedDisplayObject();
            else
                CheckRemoveCurrentModule();
        }

        public void RemoveSelectedDisplayObject()
        {
            int count = GlobalData.CurrentSelectDisplayObjectDic.Count;
            if (count == 0)
            {
                DialogManager.ShowInfo("请先选择要删除的对象");
                return;
            }

            if (GlobalData.CurrentDisplayObjects == null) return;
            int length = GlobalData.CurrentDisplayObjects.Count;
            List<KeyValuePair<string, Transform>> currentSelectDisplays = GlobalData.CurrentSelectDisplayObjectDic.ToList();
            foreach (var pair in currentSelectDisplays)
            {
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

        public void CheckRemoveCurrentModule()
        {
            if (string.IsNullOrEmpty(GlobalData.CurrentModule))
            {
                // DialogManager.ShowInfo("请先打开一个 module");
                CheckRemoveAllModules();
                return;
            }
            DialogManager.ShowQuestion($"是否删除当前打开的 module: {GlobalData.CurrentModule}", () =>
            {
                string module = GlobalData.CurrentModule;
                GlobalData.CurrentModule = null;
                int idx = GlobalData.ModuleNames.FindIndex(0, name => module.Equals(name));
                if (idx != -1) GlobalData.ModuleNames.RemoveAt(idx);
                GlobalData.Modules.Remove(module);
            }, null);
        }

        public void CheckRemoveAllModules()
        {
            if (GlobalData.ModuleNames.Count == 0)
            {
                DialogManager.ShowInfo("当前没有任何 module 可以删除");
                return;
            }
            DialogManager.ShowQuestion($"是否删除所有 module: {GlobalData.CurrentModule}", () => RemoveAllModules(), null);
        }

        public static void CreateModule()
        {
            DialogManager.ShowGetValue("请输入 module 名:", "module", txt =>
            {
                if (string.IsNullOrWhiteSpace(txt))
                {
                    DialogManager.ShowError("请输入正确的 module", 0, 0);
                    return;
                }
                if (GlobalData.Modules.ContainsKey(txt))
                {
                    DialogManager.ShowError("module 已存在", 0, 0);
                    return;
                }
                GlobalData.CurrentModule = txt;
                GlobalData.Modules[txt] = new List<DisplayObject>();
                GlobalData.ModuleNames.Add(txt);
            });
        }

        public static void UpdateCurrentDisplayObjectData()
        {
            if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
            List<DisplayObject> displayObjectDataList = GlobalData.Modules[GlobalData.CurrentModule];
            int count = GlobalData.CurrentDisplayObjects.Count;
            for (int idx = 0; idx < count; ++idx)
            {
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

        public void MoveCurrentSelectDisplayObjectUp()
        {
            if (GlobalData.CurrentSelectDisplayObjectDic.Count != 1) return;
            string displayObjectKey = GlobalData.CurrentSelectDisplayObjectDic.First().Key;
            string displayObjectName = Utils.GetDisplayObjectName(displayObjectKey);
            Debug.Log(displayObjectName);
            int idx = GlobalData.CurrentDisplayObjects.FindIndex(element => element.name.Equals(displayObjectName));
            if (idx <= 0 || idx >= GlobalData.CurrentDisplayObjects.Count) return;
            Transform tmp = GlobalData.CurrentDisplayObjects[idx];
            GlobalData.CurrentDisplayObjects[idx] = GlobalData.CurrentDisplayObjects[idx - 1];
            GlobalData.CurrentDisplayObjects[idx - 1] = tmp;
            int slblingIndex = tmp.GetSiblingIndex();
            tmp.SetSiblingIndex(slblingIndex - 1);
            List<DisplayObject> displayObjectDataList = GlobalData.Modules[GlobalData.CurrentModule];
            DisplayObject tmp2 = displayObjectDataList[idx];
            displayObjectDataList[idx] = displayObjectDataList[idx - 1];
            displayObjectDataList[idx - 1] = tmp2;
        }

        public void MoveCurrentSelectDisplayObjectDown()
        {
            if (GlobalData.CurrentSelectDisplayObjectDic.Count != 1) return;
            string displayObjectKey = GlobalData.CurrentSelectDisplayObjectDic.First().Key;
            string displayObjectName = Utils.GetDisplayObjectName(displayObjectKey);
            int idx = GlobalData.CurrentDisplayObjects.FindIndex(element => element.name.Equals(displayObjectName));
            if (idx < 0 || idx >= GlobalData.CurrentDisplayObjects.Count - 1) return;
            Transform tmp = GlobalData.CurrentDisplayObjects[idx];
            GlobalData.CurrentDisplayObjects[idx] = GlobalData.CurrentDisplayObjects[idx + 1];
            GlobalData.CurrentDisplayObjects[idx + 1] = tmp;
            int slblingIndex = tmp.GetSiblingIndex();
            tmp.SetSiblingIndex(slblingIndex + 1);
            List<DisplayObject> displayObjectDataList = GlobalData.Modules[GlobalData.CurrentModule];
            DisplayObject tmp2 = displayObjectDataList[idx];
            displayObjectDataList[idx] = displayObjectDataList[idx + 1];
            displayObjectDataList[idx + 1] = tmp2;
        }

        public void ExportCurrentModule()
        {
            if (string.IsNullOrEmpty(GlobalData.CurrentModule))
            {
                DialogManager.ShowWarn("请先打开一个 module");
                return;
            }
            ContainerManager.UpdateCurrentDisplayObjectData();
            Module module = new Module();
            module.Name = GlobalData.CurrentModule;
            module.Elements = GlobalData.Modules[module.Name];
            string jsonString = JsonConvert.SerializeObject(module, Formatting.Indented);
            GUIUtility.systemCopyBuffer = jsonString;
            DialogManager.ShowInfo("已导出到剪切板");
        }

        public void ExportModules()
        {
            string filePath = SaveFileUtil.SaveFile("json 文件(*.json)\0*.json");
            if (string.IsNullOrEmpty(filePath)) return;
            ContainerManager.UpdateCurrentDisplayObjectData();
            List<Module> modules = new List<Module>();
            int count = GlobalData.ModuleNames.Count;
            for (int idx = 0; idx < count; ++idx)
            {
                Module module = new Module();
                module.Name = GlobalData.ModuleNames[idx];
                module.Elements = GlobalData.Modules[module.Name];
                modules.Add(module);
            }
            string jsonString = JsonConvert.SerializeObject(modules, Formatting.Indented);
            bool result = Utils.WriteFile(filePath, System.Text.Encoding.UTF8.GetBytes(jsonString));
            if (result) DialogManager.ShowInfo($"成功导出到 {filePath}");
            else DialogManager.ShowError($"导出失败", 0, 0);
        }

        public void ImportModules()
        {
            string filePath = OpenFileUtil.OpenFile("json 文件(*.json)\0*.json");
            if (string.IsNullOrEmpty(filePath)) return;
            byte[] bytes = Utils.ReadFile(filePath);
            string jsonStr = System.Text.Encoding.UTF8.GetString(bytes);
            RemoveAllModules();
            Observable.Timer(TimeSpan.Zero)
                .Subscribe(_ =>
                {
                    try
                    {
                        List<Module> modules = JsonConvert.DeserializeObject<List<Module>>(jsonStr);
                        int count = modules.Count;
                        for (int idx = 0; idx < count; ++idx)
                        {
                            Module module = modules[idx];
                            GlobalData.ModuleNames.Add(module.Name);
                            GlobalData.Modules[module.Name] = module.Elements;
                        }
                    }
                    catch (Exception e)
                    {
                        DialogManager.ShowError($"导入失败({e})");
                    }
                });
        }

        public bool CheckPointOnAnyDisplayObject()
        {
            if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return false;
            Vector2 pos = DisplayObject.ConvertTo(Utils.GetAnchoredPositionInContainer(Input.mousePosition));
            foreach (DisplayObject displayObject in GlobalData.Modules[GlobalData.CurrentModule])
            {
                if (displayObject.Contain(pos)) return true;
            }
            return false;
        }

        public void SelectDisplayObjectsInDisplayObject(Rectangle selectRect)
        {
            if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
            foreach (DisplayObject displayObject in GlobalData.Modules[GlobalData.CurrentModule])
                if (displayObject.IsCrossing(selectRect))
                {
                    if (KeyboardEventManager.GetControl())
                        GlobalData.CurrentSelectDisplayObjectDic.Remove(displayObject.Name);
                    else
                        GlobalData.CurrentSelectDisplayObjectDic[displayObject.Name] = GlobalData.CurrentDisplayObjectDic[displayObject.Name];
                }
        }

        private void RemoveAllModules()
        {
            GlobalData.CurrentDisplayObjectDic.Clear();
            GlobalData.CurrentDisplayObjects.Clear();
            GlobalData.CurrentSelectDisplayObjectDic.Clear();
            GlobalData.ModuleNames.Clear();
            foreach (var pair in GlobalData.Modules)
                pair.Value.Clear();
            GlobalData.Modules.Clear();
            GlobalData.CurrentModule = null;
        }

        public Rectangle GetAlignLine(Transform displayObject)
        {
            DisplayObject displayObjectData = GlobalData.GetDisplayObjectData(displayObject);
            if (displayObjectData == null) return null;
            RectTransform rt = GlobalData.DisplayObjectContainer.GetComponent<RectTransform>();
            float closeValue = GlobalData.CLOSE_VALUE * rt.localScale.x;
            float alignExtensionValue = GlobalData.ALIGN_EXTENSION_VALUE * rt.localScale.x;
            float lineWidth = GlobalData.ALIGN_LINE_WIDTH; // * rt.localScale.x;
            List<DisplayObject> elements = GetCloseDisplayObjects(displayObjectData, GetLeft, closeValue);
            if (elements.Count < 2)
                return GetAlignLineInDisplayObjects(elements, TYPE_LEFT, alignExtensionValue, lineWidth);
            elements = GetCloseDisplayObjects(displayObjectData, GetRight, closeValue);
            if (elements.Count < 2)
                return GetAlignLineInDisplayObjects(elements, TYPE_RIGHT, alignExtensionValue, lineWidth);
            elements = GetCloseDisplayObjects(displayObjectData, GetTop, closeValue);
            if (elements.Count < 2)
                return GetAlignLineInDisplayObjects(elements, TYPE_TOP, alignExtensionValue, lineWidth);
            elements = GetCloseDisplayObjects(displayObjectData, GetBottom, closeValue);
            if (elements.Count < 2)
                return GetAlignLineInDisplayObjects(elements, TYPE_BOTTOM, alignExtensionValue, lineWidth);
            return null;
        }

        private static readonly List<DisplayObject> _tmpElements = new List<DisplayObject>();
        delegate float RectangleAction(Rectangle rect);
        private static readonly RectangleAction GetLeft = (Rectangle rect) => rect.Left;
        private static readonly RectangleAction GetRight = (Rectangle rect) => rect.Right;
        private static readonly RectangleAction GetTop = (Rectangle rect) => rect.Top;
        private static readonly RectangleAction GetBottom = (Rectangle rect) => rect.Bottom;
        private List<DisplayObject> GetCloseDisplayObjects(DisplayObject displayObject, RectangleAction action, float closeValue)
        {
            _tmpElements.Clear();
            if (displayObject == null || action == null) return _tmpElements;
            float minCloseValue = closeValue;
            float value;
            foreach (DisplayObject element in GlobalData.Modules[GlobalData.CurrentModule])
            {
				if(displayObject == element) continue;
                value = Math.Abs(action(displayObject) - action(element));
                if (value > minCloseValue) continue;
                if (value < minCloseValue) _tmpElements.Clear();
                _tmpElements.Add(element);
            }
			_tmpElements.Add(displayObject);
            return _tmpElements;
        }

        private const int TYPE_LEFT = 1;
        private const int TYPE_RIGHT = 2;
        private const int TYPE_TOP = 3;
        private const int TYPE_BOTTOM = 4;
        private Rectangle GetAlignLineInDisplayObjects(List<DisplayObject> elements, int type, float alignExtensionValue, float lineWidth)
        {
            if (elements == null || elements.Count == 0) return null;
            Rectangle result = new Rectangle();
            int length = elements.Count;
            switch (type)
            {
                case TYPE_LEFT:
                case TYPE_RIGHT:
                    if (type == TYPE_LEFT) result.Set(elements[0].Left, elements[0].Top, lineWidth, elements[0].Height);
                    else result.Set(elements[0].Right, elements[0].Top, lineWidth, elements[0].Height);
                    for (int idx = 1; idx < length; ++idx)
                    {
                        if (result.Top > elements[idx].Top)
                        {
                            result.Top = elements[idx].Top;
                            result.Height = result.Bottom - result.Top;
                        }
                        if (result.Bottom < elements[idx].Bottom)
                        {
                            result.Bottom = elements[idx].Bottom;
                            result.Height = result.Bottom - result.Top;
                        }
                    }
                    result.Top = result.Top - alignExtensionValue;
                    result.Height = result.Height + 2 * alignExtensionValue;
                    break;
                case TYPE_TOP:
                case TYPE_BOTTOM:
                    if (type == TYPE_TOP) result.Set(elements[0].Left, elements[0].Top, elements[0].Width, lineWidth);
                    else result.Set(elements[0].Left, elements[0].Top, elements[0].Width, lineWidth);
                    for (int idx = 1; idx < length; ++idx)
                    {
                        if (result.Left > elements[idx].Left)
                        {
                            result.Left = elements[idx].Left;
                            result.Width = result.Right - result.Left;
                        }
                        if (result.Right < elements[idx].Right)
                        {
                            result.Right = elements[idx].Right;
                            result.Width = result.Right - result.Left;
                        }
                    }
                    result.Left = result.Left - alignExtensionValue;
                    result.Width = result.Width + 2 * alignExtensionValue;
                    break;
                default:
                    return null;
            }
            return result;
        }
    }
}
