using System;
using System.IO;

namespace BoulderScape;

public static class FileManager
{
    public static string GetFileInStorage(string filename)
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), filename);
    }

    public static string GetFileInApp(string filename)
    {
        return string.Format("{0}/{1}", Environment.CurrentDirectory, filename);
    }
}