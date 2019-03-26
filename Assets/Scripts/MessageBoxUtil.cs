using System;
using System.Runtime.InteropServices;

class MessageBoxUtil
{
    [DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr handle, String message, String title, int type);

    private static string TITLE = "LayoutUtil";

    public static int Show(string message)
    {
        return MessageBox(IntPtr.Zero, message, TITLE, 0);
    }
}