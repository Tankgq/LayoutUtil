using UniRx;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Assets.Scripts
{
    public class MessageBroker
    {
        public const int UPDATE_SELECT_DISPLAY_OBJECT = 1;

        private static readonly Dictionary<int, Subject<int>> _subjectDic = new Dictionary<int, Subject<int>>();

        public static void AddSubject(int subjectId, Subject<int> subject) {
            if(subjectId == 0 || subject == null) return;
            DisposeSubjct(subjectId);
            _subjectDic.Add(subjectId, subject);
        }

        public static void Send(int subjectId, int msg = 1) {
            if(subjectId == 0 || ! _subjectDic.ContainsKey(subjectId)) return;
            _subjectDic[subjectId].OnNext(msg);
        }

        public static void DisposeSubjct(int subjectId) {
            if(subjectId == 0 || ! _subjectDic.ContainsKey(subjectId)) return;
            _subjectDic[subjectId].Dispose();
            _subjectDic.Remove(subjectId);
        }
    }
}