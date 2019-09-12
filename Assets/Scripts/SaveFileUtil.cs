using System.Runtime.InteropServices;
using UnityEngine;

public static class SaveFileUtil {
	public static string SaveFile(string regex = "*") {
		FileExplorerDialog fileExplorerDialog = new FileExplorerDialog();
		fileExplorerDialog.structSize = Marshal.SizeOf(fileExplorerDialog);
		fileExplorerDialog.filter = regex;
		fileExplorerDialog.file = new string(new char[256]);
		fileExplorerDialog.maxFile = fileExplorerDialog.file.Length;
		fileExplorerDialog.fileTitle = new string(new char[64]);
		fileExplorerDialog.maxFileTitle = fileExplorerDialog.fileTitle.Length;
		fileExplorerDialog.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
		fileExplorerDialog.title = "窗口标题";
		fileExplorerDialog.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
		return LocalDialog.GetSaveFileName(fileExplorerDialog) ? fileExplorerDialog.file : "";
	}
}