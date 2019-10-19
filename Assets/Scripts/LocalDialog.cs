using System.Runtime.InteropServices;

public static class LocalDialog {
	//链接指定系统函数       打开文件对话框
	[DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	public static extern bool GetOpenFileName([In, Out]FileExplorerDialog ofn);

	public static bool GetOFN([In, Out]FileExplorerDialog ofn) {
		return GetOpenFileName(ofn);
	}

	//链接指定系统函数        另存为对话框
	[DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
	public static extern bool GetSaveFileName([In, Out]FileExplorerDialog ofn);

	public static bool GetSFN([In, Out]FileExplorerDialog ofn) {
		return GetSaveFileName(ofn);
	}
}
