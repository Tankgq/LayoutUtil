using System;
using System.Runtime.InteropServices;

public static class WinTitleUtil
{

	#region WIN32API

	private delegate bool EnumWindowsCallBack(IntPtr hwnd, IntPtr lParam);
	[DllImport("user32", CharSet = CharSet.Unicode)]
	private static extern bool SetWindowTextW(IntPtr hwnd, string title);
	[DllImport("user32")]
	private static extern int EnumWindows(EnumWindowsCallBack lpEnumFunc, IntPtr lParam);
	[DllImport("user32")]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref IntPtr lpdwProcessId);
	#endregion
	private static IntPtr WindowHandle = IntPtr.Zero;
	private static bool EnumWindCallback(IntPtr hwnd, IntPtr lParam)
	{
		IntPtr pid = IntPtr.Zero;
		GetWindowThreadProcessId(hwnd, ref pid);
		if (pid != lParam) return true;
		WindowHandle = hwnd;
		return false;
	}

	public static void ChangeTitle(string title)
	{
		if (WindowHandle == IntPtr.Zero)
		{
			IntPtr handle = (IntPtr)System.Diagnostics.Process.GetCurrentProcess().Id;
			EnumWindows(EnumWindCallback, handle);
		}
		
		SetWindowTextW(WindowHandle, title);
	}
}
