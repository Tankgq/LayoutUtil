using System;
using System.IO;

public class Utils
{
    public static byte[] ReadFile(string filePath)
    {
        filePath = filePath + "/1.txt";
        if (string.IsNullOrEmpty(filePath)) return null;
        byte[] bytes = null;
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                fs.Seek(0, SeekOrigin.Begin);
                bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
                fs.Close();
                fs.Dispose();
            }
        }
        catch (Exception e)
        {
            return bytes;
        }
        
        return bytes;
    }
}
