using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Utils
    {
        public static byte[] ReadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || ! File.Exists(filePath)) return null;
            byte[] bytes = null;
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                    fs.Seek(0, SeekOrigin.Begin);
                    bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, (int)fs.Length);
                    fs.Close();
                }
            }
            catch (Exception e)
            {
    //            MessageBoxUtil.Show($"{e}");
                DialogManager.ShowError($"{e}");
                return null;
            }
            
            return bytes;
        }

        public static bool WriteFile(string filePath, byte[] content)
        {
            if (content == null || content.Length == 0) return false;
            Debug.Log(content.Length);
            bool isExist = CheckFileDirectory(filePath);
            if (!isExist) return false;
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.Write(content, 0, (int)content.Length);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch (Exception e)
            {
    //            MessageBoxUtil.Show($"{e}");
                DialogManager.ShowError($"{e}");
                return false;
            }
            return true;
        }

        private static readonly Regex RegFileName = new Regex(@"[\\/]([^.\\/]*).[a-zA-Z]*$");
        public static string GetFileNameInPath(string filePath)
        {
            GroupCollection groups = RegFileName.Match(filePath).Groups;
            return groups.Count > 1 ? groups[1].Value : string.Empty;
        }

        private static readonly Regex RegFileDirectory = new Regex(@"([\d\D]*)[\\/][^.]*.[a-zA-Z]*$");
        public static bool CheckFileDirectory(string filePath)
        {
            GroupCollection groups = RegFileDirectory.Match(filePath).Groups;
            if (groups.Count < 2) return false;
            string fileDirectory = groups[1].Value;
            return Directory.Exists(fileDirectory);
        }

        public static string CancelHighlight(string text)
        {
            return string.IsNullOrEmpty(text) ? text : Regex.Replace(text, @"<color=yellow><size=25><b>(?<str>.*?)</b></size></color>", @"${str}");
        }

        public static string GetHighlight(string text, string needHighlight)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(needHighlight)) return text;
            return text.Replace(needHighlight, "<color=yellow><size=25><b>" + needHighlight + "</b></size></color>");
        }

        private static readonly Regex RegGetDisplayName = new Regex(@"_([^_]*)$");
        public static string GetDisplayObjectName(string displayObjectKey)
        {
            if (string.IsNullOrEmpty(displayObjectKey)) return displayObjectKey;
            GroupCollection groups = RegGetDisplayName.Match(displayObjectKey).Groups;
            return groups.Count < 2 ? displayObjectKey : groups[1].Value;
        }


        public static bool IsFocusOnInputText()
        {
            GameObject focusGameObject = EventSystem.current.currentSelectedGameObject;
            return focusGameObject && focusGameObject.GetComponent<InputField>() != null;
        }
    }
}