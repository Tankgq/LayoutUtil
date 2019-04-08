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
        // public HierarchyManager HierarchyManager;

        private string PreviousSearch = null;

        private void Start()
        {
            SearchInputField.OnValueChangedAsObservable()
                .Where(txt => ! string.IsNullOrWhiteSpace(txt) && ! txt.Equals(PreviousSearch))
                .Sample(TimeSpan.FromMilliseconds(500))
                .Subscribe(txt => {
                    HierarchyManager.Search(txt);
                    PreviousSearch = txt;
                });
        }
    }
}