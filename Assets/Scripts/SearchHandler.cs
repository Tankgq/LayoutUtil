using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SearchHandler : MonoBehaviour
{
    public InputField SearchInputField;
    public Transform DisplayObjectListContent;

    private string PreviousSearch = null;

    private void Start()
    {
        SearchInputField.OnValueChangedAsObservable()
            .Where(txt => GlobalData.DisplayObjects.Count > 0 && txt != null && ! txt.Equals(PreviousSearch))
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(txt =>
            {
                Text[] displayObjectNameList = DisplayObjectListContent.GetComponentsInChildren<Text>();
                foreach (Text nameText in displayObjectNameList) {
                    string text = Utils.CancelHighlight(nameText.text);
                    nameText.text = Utils.GetHighlight(text, txt);
                }
                PreviousSearch = txt;
            });
    }
}