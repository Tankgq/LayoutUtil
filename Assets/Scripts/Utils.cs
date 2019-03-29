using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

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
            MessageBoxUtil.Show($"{e}");
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
            MessageBoxUtil.Show($"{e}");
            return false;
        }
        return true;
    }

    private static readonly Regex RegFileName = new Regex(@"[\\/]([^.\\/]*).[a-zA-Z]*$");
    public static string GetFileNameInPath(string filePath)
    {
        GroupCollection groups = RegFileName.Match(filePath).Groups;
        if (groups.Count > 1) return groups[1].Value;
        return string.Empty;
    }

    public static readonly Regex RegFileDirectory = new Regex(@"([\d\D]*)[\\/][^.]*.[a-zA-Z]*$");
    public static bool CheckFileDirectory(string filePath)
    {
        GroupCollection groups = RegFileDirectory.Match(filePath).Groups;
        if (groups.Count < 2) return false;
        string fileDirectory = groups[1].Value;
        return Directory.Exists(fileDirectory);
    }
}
