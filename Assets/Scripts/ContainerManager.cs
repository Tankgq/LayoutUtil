using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ContainerManager : MonoBehaviour
{
    private static readonly Dictionary<string, Material> MaterialDic = new Dictionary<string, Material>();
    private static readonly Dictionary<string, Vector2> SizeDic = new Dictionary<string, Vector2>();

    private static readonly List<Transform> DisplayObjectPool = new List<Transform>();

    public GameObject containerScrollView;
    public Text moduleNameText;
    public Text selectedDisplayObjectText;
    public Slider scaleSlider;

    private Transform GetDisplayObject()
    {
        int length = DisplayObjectPool.Count;
        if (length == 0) return Instantiate(GlobalData.DisplayObjectPrefab.transform, transform);
        Transform result = DisplayObjectPool[length - 1];
        DisplayObjectPool.RemoveAt(length - 1);
        return result;
    }

    private static void RecycleDisplayObject(Transform displayObject)
    {
        if (!displayObject) return;
        DisplayObjectManager.DeSelectDisplayObject(displayObject);
        displayObject.GetComponentInChildren<Toggle>().isOn = false;
        displayObject.SetParent(null);
        DisplayObjectPool.Add(displayObject);
    }

    private static void RecycleAllDisplayObject()
    {
        int count = GlobalData.CurrentDisplayObjects.Count;
        for (int idx = 0; idx < count; ++idx)
            RecycleDisplayObject(GlobalData.CurrentDisplayObjects[idx]);
        GlobalData.CurrentDisplayObjects.Clear();
        GlobalData.CurrentDisplayObjectDic.Clear();
    }

    private void LoadAllDisplayObject()
    {
        if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
        List<Element> elements = GlobalData.ModuleDic[GlobalData.CurrentModule];
        int count = elements.Count;
        for (var idx = 0; idx < count; ++idx)
        {
            Transform displayObject = GetDisplayObject();
            displayObject.GetComponent<Image>().color = Color.clear;
            displayObject.SetParent(transform);
            displayObject.GetComponent<RectTransform>().localScale = Vector3.one;
            Element element = elements[idx];
            element.InvConvertTo(displayObject);
            GlobalData.CurrentDisplayObjects.Add(displayObject);
            GlobalData.CurrentDisplayObjectDic[displayObject.name] = displayObject;
            displayObject.gameObject.SetActive(element.Visible);
        }
    }

    private void Start()
    {
        GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.CurrentModule)
            .SampleFrame(1)
            .Subscribe(module =>
            {
                RecycleAllDisplayObject();
                Observable.Timer(TimeSpan.Zero)
                    .Subscribe(_ =>
                    {
                        RectTransform rt = moduleNameText.GetComponent<RectTransform>();
                        RectTransform rt2 = selectedDisplayObjectText.GetComponent<RectTransform>();
                        rt2.anchoredPosition = new Vector2(rt.anchoredPosition.x + rt.sizeDelta.x + 30,
                            rt2.anchoredPosition.y);
                    });
                if (string.IsNullOrEmpty(module))
                {
                    moduleNameText.text = "null";
                    return;
                }

                moduleNameText.text = module;
                GlobalData.CurrentSelectDisplayObjectDic.Clear();
                scaleSlider.value = 10f;
                GetComponent<RectTransform>().localPosition = Vector2.zero;
                LoadAllDisplayObject();
            });
        Subject<object[]> updateSelectDisplayObjectSubject =
            MessageBroker.GetSubject(MessageBroker.UpdateSelectDisplayObject);
        updateSelectDisplayObjectSubject.SampleFrame(1)
            .Subscribe(_ =>
            {
                foreach (Transform displayObjectItem in GlobalData.CurrentDisplayObjects)
                    displayObjectItem.GetComponent<Toggle>().isOn = false;
                int count = GlobalData.CurrentSelectDisplayObjectDic.Count;
                print($"count: {count}");
                if (count == 0)
                {
                    selectedDisplayObjectText.text = "null";
                    return;
                }

                var sb = new StringBuilder();
                foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
                {
                    sb.Append($"{pair.Value.name}, ");
                    pair.Value.GetComponent<Toggle>().isOn = true;
                }

                selectedDisplayObjectText.text = sb.ToString(0, sb.Length - 2);
            });
        GlobalData.CurrentSelectDisplayObjectDic.ObserveEveryValueChanged(dic => dic.Count)
            .Subscribe(_ => MessageBroker.Send(MessageBroker.UpdateSelectDisplayObject));
        Subject<object[]> haveModifySubject = MessageBroker.GetSubject(MessageBroker.UpdateModifyCount);
        GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.ModifyCount)
            .SampleFrame(1)
            .Subscribe(modifyCount => MessageBroker.Send(MessageBroker.UpdateModifyCount));
        haveModifySubject.SampleFrame(1)
            .Subscribe(_ =>
            {
                string title = GlobalData.ProductName;
                if (!string.IsNullOrWhiteSpace(GlobalData.CurrentFilePath))
                    title = GlobalData.CurrentFilePath;
                Utils.ChangeTitle(GlobalData.ModifyCount != 0 ? $"* {title}" : title);
            });
    }

    private static Texture2D LoadTexture2DbyIo(string imageUrl)
    {
        byte[] bytes = Utils.ReadFile(imageUrl);
        Texture2D texture2D = new Texture2D((int) GlobalData.DefaultSize.x, (int) GlobalData.DefaultSize.y);
        texture2D.LoadImage(bytes);
        return texture2D;
    }

    public Transform AddDisplayObject(string imageUrl, Vector2 pos, Vector2 size, string elementName = null)
    {
        if (string.IsNullOrEmpty(GlobalData.CurrentModule))
        {
            if (GlobalData.ModuleDic.Count == 0)
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
                if (!Utils.IsPointOverGameObject(displayObject.gameObject)) continue;
                string displayObjectKey2 = $"{GlobalData.CurrentModule}_{displayObject.name}";
                GlobalData.DisplayObjectPathDic[displayObjectKey2] = imageUrl;
                Image image2 = displayObject.GetComponent<Image>();
                image2.material = material;
                image2.color = Color.white;
                return displayObject;
            }

            return null;
        }

        Transform imageElement = GetDisplayObject();
        imageElement.SetParent(transform);
        imageElement.GetComponent<RectTransform>().localScale = Vector3.one;
        imageElement.name = string.IsNullOrEmpty(elementName)
            ? string.IsNullOrEmpty(imageUrl) ? GlobalData.DefaultName + ++GlobalData.UniqueId :
            Utils.GetFileNameInPath(imageUrl)
            : elementName;
        string displayObjectKey = $"{GlobalData.CurrentModule}_{imageElement.name}";
        if (GlobalData.CurrentDisplayObjectDic.ContainsKey(imageElement.name))
        {
            imageElement.name = imageElement.name + ++GlobalData.UniqueId;
            displayObjectKey = $"{GlobalData.CurrentModule}_{imageElement.name}";
        }

        GlobalData.DisplayObjectPathDic[displayObjectKey] = imageUrl;
        GlobalData.CurrentDisplayObjects.Add(imageElement);
        GlobalData.CurrentDisplayObjectDic[imageElement.name] = imageElement;
        Image image = imageElement.GetComponent<Image>();
        image.material = material;
        image.color = material ? Color.white : Color.clear;
        RectTransform rect = imageElement.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size.x, size.y);
        pos = Element.ConvertTo(pos);
        rect.anchoredPosition = pos;
        GlobalData.ModuleDic[GlobalData.CurrentModule].Add(Element.ConvertTo(imageElement));
        return imageElement;
    }

    public void RemoveSelectedDisplayObjectOrModules()
    {
        if (GlobalData.CurrentSelectDisplayObjectDic.Count > 0)
            RemoveSelectedDisplayObject();
        else
            CheckRemoveCurrentModule();
    }

    private static void RemoveSelectedDisplayObject()
    {
        int count = GlobalData.CurrentSelectDisplayObjectDic.Count;
        if (count == 0)
        {
            DialogManager.ShowInfo("请先选择要删除的对象");
            return;
        }

        if (GlobalData.CurrentDisplayObjects == null) return;
        List<KeyValuePair<string, Transform>> currentSelectDisplays = GlobalData.CurrentSelectDisplayObjectDic.ToList();
        foreach (var pair in currentSelectDisplays)
        {
            RecycleDisplayObject(pair.Value);
//          string displayObjectKey = $"{GlobalData.CurrentModule}_{pair.Key}";
//			if (GlobalData.DisplayObjectPathDic.ContainsKey(displayObjectKey))
//				GlobalData.DisplayObjectPathDic.Remove(displayObjectKey);
            int idx = GlobalData.CurrentDisplayObjects.FindIndex(0, element => element.name.Equals(pair.Key));
            if (idx != -1)
            {
                GlobalData.CurrentDisplayObjects.RemoveAt(idx);
                GlobalData.CurrentDisplayObjectDic.Remove(pair.Key);
            }

            List<Element> elements = GlobalData.ModuleDic[GlobalData.CurrentModule];
            idx = elements.FindIndex(0, element => element.Name.Equals(pair.Key));
            if (idx != -1) elements.RemoveAt(idx);
        }

        GlobalData.CurrentSelectDisplayObjectDic.Clear();
    }

    private void CheckRemoveCurrentModule()
    {
        if (string.IsNullOrEmpty(GlobalData.CurrentModule))
        {
            // DialogManager.ShowInfo("请先打开一个 module");
            CheckRemoveAllModules();
            return;
        }

        DialogManager.ShowQuestion($"是否删除当前打开的 module: {GlobalData.CurrentModule}", RemoveCurrentModule, null);
    }

    private static void RemoveCurrentModule()
    {
        string module = GlobalData.CurrentModule;
        GlobalData.CurrentModule = null;
        int idx = GlobalData.Modules.FindIndex(0, name => module.Equals(name));
        if (idx != -1) GlobalData.Modules.RemoveAt(idx);
        GlobalData.ModuleDic[module].Clear();
        GlobalData.ModuleDic.Remove(module);
    }

    private void CheckRemoveAllModules()
    {
        if (GlobalData.Modules.Count == 0)
        {
            DialogManager.ShowInfo("当前没有任何 module 可以删除");
            return;
        }

        DialogManager.ShowQuestion($"是否删除所有 module: {GlobalData.CurrentModule}", RemoveAllModules, null);
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

            if (GlobalData.ModuleDic.ContainsKey(txt))
            {
                DialogManager.ShowError("module 已存在", 0, 0);
                return;
            }

            GlobalData.CurrentModule = txt;
            GlobalData.ModuleDic[txt] = new List<Element>();
            GlobalData.Modules.Add(txt);
        });
    }

    public static void UpdateCurrentDisplayObjectData()
    {
        if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
        List<Element> displayObjectDataList = GlobalData.ModuleDic[GlobalData.CurrentModule];
        int count = GlobalData.CurrentDisplayObjects.Count;
        for (int idx = 0; idx < count; ++idx)
        {
            Transform displayObject = GlobalData.CurrentDisplayObjects[idx];
            Element displayObjectData = displayObjectDataList[idx];
            displayObjectData.Name = displayObject.name;
            RectTransform rt = displayObject.GetComponent<RectTransform>();
            Vector2 pos = rt.anchoredPosition;
            Vector2 size = rt.sizeDelta;
            displayObjectData.X = Element.ConvertX(pos.x);
            displayObjectData.Y = Element.ConvertY(pos.y);
            displayObjectData.Width = size.x;
            displayObjectData.Height = size.y;
        }
    }

    public static void MoveCurrentSelectDisplayObjectUp()
    {
        if (GlobalData.CurrentSelectDisplayObjectDic.Count != 1) return;
        // string displayObjectKey = GlobalData.CurrentSelectDisplayObjectDic.First().Key;
        // string displayObjectName = Utils.GetDisplayObjectName(displayObjectKey);
        // Debug.Log(displayObjectName);
        string displayObjectName = GlobalData.CurrentSelectDisplayObjectDic.First().Key;
        int idx = GlobalData.CurrentDisplayObjects.FindIndex(element => element.name.Equals(displayObjectName));
        if (idx <= 0 || idx >= GlobalData.CurrentDisplayObjects.Count) return;
        Transform tmp = GlobalData.CurrentDisplayObjects[idx];
        GlobalData.CurrentDisplayObjects[idx] = GlobalData.CurrentDisplayObjects[idx - 1];
        GlobalData.CurrentDisplayObjects[idx - 1] = tmp;
        int siblingIndex = tmp.GetSiblingIndex();
        tmp.SetSiblingIndex(siblingIndex - 1);
        List<Element> displayObjectDataList = GlobalData.ModuleDic[GlobalData.CurrentModule];
        Element tmp2 = displayObjectDataList[idx];
        displayObjectDataList[idx] = displayObjectDataList[idx - 1];
        displayObjectDataList[idx - 1] = tmp2;
    }

    public static void MoveCurrentSelectDisplayObjectDown()
    {
        if (GlobalData.CurrentSelectDisplayObjectDic.Count != 1) return;
        string displayObjectKey = GlobalData.CurrentSelectDisplayObjectDic.First().Key;
        string displayObjectName = Utils.GetDisplayObjectName(displayObjectKey);
        int idx = GlobalData.CurrentDisplayObjects.FindIndex(element => element.name.Equals(displayObjectName));
        if (idx < 0 || idx >= GlobalData.CurrentDisplayObjects.Count - 1) return;
        Transform tmp = GlobalData.CurrentDisplayObjects[idx];
        GlobalData.CurrentDisplayObjects[idx] = GlobalData.CurrentDisplayObjects[idx + 1];
        GlobalData.CurrentDisplayObjects[idx + 1] = tmp;
        int siblingIndex = tmp.GetSiblingIndex();
        tmp.SetSiblingIndex(siblingIndex + 1);
        List<Element> displayObjectDataList = GlobalData.ModuleDic[GlobalData.CurrentModule];
        Element tmp2 = displayObjectDataList[idx];
        displayObjectDataList[idx] = displayObjectDataList[idx + 1];
        displayObjectDataList[idx + 1] = tmp2;
    }

    public static void ExportCurrentModule()
    {
        if (string.IsNullOrEmpty(GlobalData.CurrentModule))
        {
            DialogManager.ShowWarn("请先打开一个 module");
            return;
        }

        UpdateCurrentDisplayObjectData();
        Module module = new Module {Name = GlobalData.CurrentModule};
        module.Elements = GlobalData.ModuleDic[module.Name];
        string jsonString = JsonConvert.SerializeObject(module, Formatting.Indented);
        GUIUtility.systemCopyBuffer = jsonString;
        DialogManager.ShowInfo("已导出到剪切板");
    }

    public static void CheckExportModules()
    {
        if (GlobalData.Modules.Count == 0)
        {
            DialogManager.ShowInfo("当前没有任何 module, 导出失败");
            return;
        }

        string filePath = SaveFileUtil.SaveFile("json 文件(*.json)\0*.json");
        ExportModules(filePath);
    }

    public static void ExportModules(string filePath, bool showQuickTip = false)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;
        GlobalData.CurrentFilePath = filePath;
        UpdateCurrentDisplayObjectData();
        List<Module> modules = new List<Module>();
        int count = GlobalData.Modules.Count;
        for (int idx = 0; idx < count; ++idx)
        {
            Module module = new Module {Name = GlobalData.Modules[idx]};
            module.Elements = GlobalData.ModuleDic[module.Name];
            Rectangle rect = GetMinRectangleContainsDisplayObjects(module.Elements);
            if (rect != null)
            {
                module.X = rect.X;
                module.Y = rect.Y;
                module.Width = rect.Width;
                module.Height = rect.Height;
            }

            modules.Add(module);
        }

        string jsonString = JsonConvert.SerializeObject(modules, Formatting.Indented);
        bool result = Utils.WriteFile(filePath, Encoding.UTF8.GetBytes(jsonString));
        if (result)
        {
            string message = $"成功导出到 {filePath}";
            if (showQuickTip)
                QuickTipManager.ShowQuickTip(message);
            else
                DialogManager.ShowInfo(message);
            GlobalData.CurrentFilePath = filePath;
            GlobalData.ModifyCount = 0;
        }
        else
        {
            string message = "导出失败";
            if (showQuickTip)
                QuickTipManager.ShowQuickTip(message);
            else
                DialogManager.ShowError(message, 0, 0);
        }
    }

    private static Rectangle GetMinRectangleContainsDisplayObjects(IReadOnlyList<Element> displayObjects)
    {
        if (displayObjects == null || displayObjects.Count == 0) return null;
        Rectangle rect = new Rectangle();
        int count = displayObjects.Count;
        for (int idx = 0; idx < count; ++idx)
        {
            rect.Left = Math.Min(rect.Left, displayObjects[idx].Left);
            rect.Right = Math.Max(rect.Right, displayObjects[idx].Right);
            rect.Top = Math.Min(rect.Top, displayObjects[idx].Top);
            rect.Bottom = Math.Max(rect.Bottom, displayObjects[idx].Bottom);
        }

        return rect;
    }

    public void CheckImportModules()
    {
        string filePath = OpenFileUtil.OpenFile("json 文件(*.json)\0*.json");
        if (string.IsNullOrEmpty(filePath)) return;
        if (GlobalData.Modules.Count == 0)
        {
            ImportModules(filePath);
            return;
        }

        DialogManager.ShowQuestion("导入时会先将所有 modules 都删除, 是否继续导入?",
            () => ImportModules(filePath),
            null, "确定", "取消", 0, 165);
    }

    private void ImportModules(string filePath)
    {
        byte[] bytes = Utils.ReadFile(filePath);
        string jsonStr = Encoding.UTF8.GetString(bytes);
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
                        GlobalData.Modules.Add(module.Name);
                        GlobalData.ModuleDic[module.Name] = module.Elements;
                    }

                    GlobalData.CurrentFilePath = filePath;
                }
                catch (Exception e)
                {
                    DialogManager.ShowError($"导入失败({e})");
                }
            });
    }

    public static bool CheckPointOnAnyDisplayObject()
    {
        if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return false;
        Vector2 pos = Element.ConvertTo(Utils.GetAnchoredPositionInContainer(Input.mousePosition));
        foreach (Element displayObject in GlobalData.ModuleDic[GlobalData.CurrentModule])
        {
            if (displayObject.Contain(pos)) return true;
        }

        return false;
    }

    public static void SelectDisplayObjectsInDisplayObject(Rectangle selectRect)
    {
        if (string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
        foreach (Element displayObject in GlobalData.ModuleDic[GlobalData.CurrentModule])
            if (displayObject.IsCrossing(selectRect))
            {
                if (KeyboardEventManager.GetControl())
                    GlobalData.CurrentSelectDisplayObjectDic.Remove(displayObject.Name);
                else
                    GlobalData.CurrentSelectDisplayObjectDic[displayObject.Name] =
                        GlobalData.CurrentDisplayObjectDic[displayObject.Name];
            }
    }

    private static void RemoveAllModules()
    {
        RecycleAllDisplayObject();
        GlobalData.CurrentDisplayObjectDic.Clear();
        GlobalData.CurrentDisplayObjects.Clear();
        GlobalData.CurrentSelectDisplayObjectDic.Clear();
        GlobalData.CurrentModule = null;
        GlobalData.Modules.Clear();
        foreach (var pair in GlobalData.ModuleDic)
            pair.Value.Clear();
        GlobalData.ModuleDic.Clear();
    }

    public AlignInfo GetAlignLine(Transform displayObject)
    {
        Element displayObjectData = GlobalData.GetElement(displayObject.name);
        if (displayObjectData == null) return null;
        RectTransform rt = GlobalData.DisplayObjectContainer.GetComponent<RectTransform>();
        float closeValue = GlobalData.CloseValue / rt.localScale.x;
        float lineThickness = GlobalData.AlignLineThickness; // * rt.localScale.x;
        AlignInfo alignInfo = new AlignInfo(displayObjectData, closeValue, lineThickness);
        List<Element> elements = GlobalData.ModuleDic[GlobalData.CurrentModule];
        int count = elements.Count;
        for (int idx = 0; idx < count; ++idx)
        {
            if (displayObjectData.Name.Equals(elements[idx].Name)) continue;
            if (GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(elements[idx].Name)) continue;
            alignInfo.Merge(elements[idx]);
        }

        return alignInfo;
    }

    public void CopySelectDisplayObjects()
    {
        if (GlobalData.CurrentSelectDisplayObjectDic.Count == 0) return;
        GlobalData.CurrentCopyDisplayObjects.Clear();
        int count = GlobalData.CurrentDisplayObjects.Count;
        for (int idx = 0; idx < count; ++idx)
        {
            Transform displayObject = GlobalData.CurrentDisplayObjects[idx];
            if (displayObject == null) continue;
            if (GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(displayObject.name))
            {
                Element element = GlobalData.GetElement(displayObject.name);
                GlobalData.CurrentCopyDisplayObjects.Add(new Element
                {
                    Name = element.Name,
                    X = element.X,
                    Y = element.Y,
                    Width = element.Width,
                    Height = element.Height,
                    Visible = true
                });
            }
        }
    }

    private static Vector2 GetCopyDisplayObjectsLeftTop(List<Element> displayObjects)
    {
        if (displayObjects.Count == 0) return Vector2.zero;
        Vector2 result = new Vector2(displayObjects[0].Left, displayObjects[0].Top);
        int count = displayObjects.Count;
        for (int idx = 1; idx < count; ++idx)
        {
            result.x = Math.Min(result.x, displayObjects[idx].X);
            result.y = Math.Min(result.y, displayObjects[idx].Y);
        }

        return result;
    }

    public void PasteDisplayObjects()
    {
        if (GlobalData.CurrentCopyDisplayObjects.Count == 0) return;
        List<Element> copyList = GlobalData.CurrentCopyDisplayObjects;
        Vector2 leftTop = GetCopyDisplayObjectsLeftTop(copyList);
        Vector2 mousePos = Element.InvConvertTo(GlobalData.OriginPoint);
        if (Utils.IsPointOverGameObject(containerScrollView))
            mousePos = Utils.GetRealPositionInContainer(Input.mousePosition);
        int count = copyList.Count;
        for (int idx = 0; idx < count; ++idx)
        {
            Vector2 pos = mousePos - leftTop;
            pos.x += copyList[idx].X;
            pos.y += copyList[idx].Y;
            AddDisplayObject(null, pos, new Vector2(copyList[idx].Width, copyList[idx].Height), copyList[idx].Name);
        }
    }
}