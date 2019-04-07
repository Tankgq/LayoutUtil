using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class SearchHandler : MonoBehaviour
    {
        public InputField SearchInputField;
        public HierarchyManager HierarchyManager;

        private string PreviousSearch = null;

        private void Start()
        {
            SearchInputField.OnValueChangedAsObservable()
                .Where(txt => GlobalData.CurrentDisplayObjects.Count > 0 && txt != null && ! txt.Equals(PreviousSearch))
                .Sample(TimeSpan.FromSeconds(1))
                .Subscribe(txt =>
                {
                    int count = GlobalData.ModuleNames.Count;

                    // Text[] displayObjectNameList = DisplayObjectListContent.GetComponentsInChildren<Text>();
                    // foreach (Text nameText in displayObjectNameList) {
                    //     string text = Utils.CancelHighlight(nameText.text);
                    //     nameText.text = Utils.GetHighlight(text, txt);
                    // }
                    // PreviousSearch = txt;
                });
        }
    }
}