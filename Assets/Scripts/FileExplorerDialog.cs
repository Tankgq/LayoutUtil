using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class FileExplorerDialog
{
	public int structSize = 0;
	public IntPtr dlgOwner = IntPtr.Zero;
	public IntPtr instance = IntPtr.Zero;
	public String filter = null;
	public String customFilter = null;
	public int maxCustFilter = 0;
	public int filterIndex = 0;
	public String file = null;
	public int maxFile = 0;
	public String fileTitle = null;
	public int maxFileTitle = 0;
	public String initialDir = null;
	public String title = null;
	public int flags = 0;
	public short fileOffset = 0;
	public short fileExtension = 0;
	public String defExt = null;
	public IntPtr custData = IntPtr.Zero;
	public IntPtr hook = IntPtr.Zero;
	public String templateName = null;
	public IntPtr reservedPtr = IntPtr.Zero;
	public int reservedInt = 0;
	public int flagsEx = 0;
}
public class LocalDialog
{
	//链接指定系统函数       打开文件对话框
	[DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	public static extern bool GetOpenFileName([In, Out] FileExplorerDialog ofn);
	public static bool GetOFN([In, Out] FileExplorerDialog ofn)
	{
		return GetOpenFileName(ofn);
	}

	//链接指定系统函数        另存为对话框
	[DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	public static extern bool GetSaveFileName([In, Out] FileExplorerDialog ofn);
	public static bool GetSFN([In, Out] FileExplorerDialog ofn)
	{
		return GetSaveFileName(ofn);
	}
}

public class OpenFileUtil
{
	public static string OpenFile(string regex = "*")
	{
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
		return LocalDialog.GetOpenFileName(fileExplorerDialog) ? fileExplorerDialog.file : "";
	}
}

public class SaveFileUtil {
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