using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using B83.Win32;
using UnityEngine;

namespace Assets.Scripts
{
    public class DragFileHandler : MonoBehaviour
    {
        public ContainerManager ContainerManager;
        private UnityDragAndDropHook hook;

        private void Start()
        {
#if UNITY_STANDALONE_WIN && ! UNITY_EDITOR
		hook = new UnityDragAndDropHook();
		hook.InstallHook();
		hook.OnDroppedFiles += OnFiles;
#endif
        }

        private void OnDisable()
        {
            hook?.UninstallHook();
        }

        private void OnDestroy()
        {
            if (null == hook) return;
            hook.UninstallHook();
            GC.Collect();
        }

        private static readonly Regex REG_IMAGE_SUFFIX = new Regex(@"\.(jpe?g|png)$");

        private void OnFiles(List<string> aFiles, POINT aPos)
        {
            Debug.Log("Dropped " + aFiles.Count + " files at: " + aPos + "\n" + aFiles.Aggregate((a, b) => a + "\n" + b));
            var sb = new StringBuilder();
            sb.Append("拖拽文件:\n");
            foreach (var path in aFiles)
            {
                sb.Append(path);
                try
                {
                    if (REG_IMAGE_SUFFIX.IsMatch(path))
                    {
                        Vector2 pos = Utils.GetRealPositionInContainer(new Vector2(aPos.x, aPos.y), 1);
                        ContainerManager.AddDisplayObject(path, pos, Vector2.zero);
                    }
                    sb.Append(" isMatch.");
                }
                catch (Exception e)
                {
                    DialogManager.ShowError(e.ToString());
                }
                sb.Append("\n");
            }
        }
    }
}