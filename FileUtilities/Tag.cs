using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileUtilities
{
    public class Tag
    {
        public Tag(string tagName, string fileName)
        {
            Name = tagName;
            FilesWithTag = new List<string>();
            FilesWithTag.Add(fileName);
        }

        public string Name { get; set; }
        public List<string> FilesWithTag { get; set; }
    }
}
