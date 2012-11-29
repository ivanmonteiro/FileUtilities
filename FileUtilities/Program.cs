using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using CommandLine.Utility;
using System.Linq;

namespace FileUtilities
{
    class Program
    {
        static void Main(string[] args)
        {
            bool hasErrors = false;
            Arguments cmdLine = new Arguments(args);

            string rArgument = cmdLine["r"];
            string tArgument = cmdLine["t"];
            string directoryArgument = cmdLine["directory"];

            if (directoryArgument == null)
            {
                Console.WriteLine("Must specify -directory parameter. Example: -directory=\"your_path_here\"");
                hasErrors = true;
            }

            if ((rArgument == null && tArgument == null) || (rArgument != null && tArgument != null))
            {
                Console.Write("Must specify -r parameter for repeated file search or -t parameter for tag count extractor.");
                hasErrors = true;
            }

            if (!hasErrors)
            {
                if (tArgument != null)
                {
                    List<Tag> tagsInFileNames = new TagExtractor().GetTagsInFileNames(directoryArgument);

                    foreach (var tag in tagsInFileNames)
                    {
                        Console.WriteLine("Tag \"{0}\" occuring {1} times.", tag.Name, tag.FilesWithTag.Count);
                    }
                }
                else
                {
                    List<HashedFile> repeatedFiles = new RepeatedFilesFinder().GetRepeatedFiles(directoryArgument);

                    foreach (var hashedFile in repeatedFiles)
                    {
                        Console.WriteLine("Found {0} files for hash {1}", hashedFile.FilesWithHash.Count, hashedFile.ComputedHash);
                        foreach (var fileName in hashedFile.FilesWithHash)
                        {
                            Console.WriteLine(fileName);
                        }
                        Console.WriteLine();
                    }

                    if (repeatedFiles.Count == 0)
                    {
                        Console.WriteLine("No repeated files found.");
                    }
                }
            }

            Console.ReadKey();
        }

    }
}
