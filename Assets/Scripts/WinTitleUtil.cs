using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class WinTitleUtil
{

    [DllImport("user32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode)]
    public static extern bool SetWindowTextW(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);

    public static void ChangeTitle(string title)
    {
        System.IntPtr windowPtr = FindWindow(null, "LayoutUtil");
        SetWindowTextW(windowPtr, title);
    }
}
