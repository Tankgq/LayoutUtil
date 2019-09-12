using System;
using System.Runtime.InteropServices;

class MessageBoxUtil
{
    [DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern int MessageBox(IntPtr handle, string message, string title, int type);

    private static readonly string TITLE = "LayoutUtil";

    public static int Show(string message)
    {
        return MessageBox(IntPtr.Zero, message, TITLE, 0);
    }
}