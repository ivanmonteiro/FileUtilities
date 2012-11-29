using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileUtilities
{
    public class Util
    {
        public static IEnumerable<FileInfo> GetFileInfos(string path)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                FileInfo[] files = null;
                try
                {
                    files = new DirectoryInfo(path).GetFiles();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Count(); i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }
    }
}
