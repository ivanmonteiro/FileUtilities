using System;
using System.Collections.Generic;
using System.Text;

namespace FileUtilities
{
    public class HashedFile
    {
        public HashedFile(string hash, string fileName)
        {
            ComputedHash = hash;
            FilesWithHash = new List<string>();
            FilesWithHash.Add(fileName);
        }

        public string ComputedHash { get; set; }
        public List<string> FilesWithHash { get; set; }
    }
}
