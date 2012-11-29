using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FileUtilities
{
    public class TagExtractor
    {
        public delegate void StatusEventHandler(string status);
        public event StatusEventHandler StatusChanged;

        public List<Tag> GetTagsInFileNames(string directory)
        {
            OnStatusChanged("Getting files...");
            IList<FileInfo> files = Util.GetFileInfos(directory).ToList();
            OnStatusChanged(files.Count + " files found.");

            OnStatusChanged("Processing tags...");
            List<Tag> tags = new List<Tag>();
            int processedFiles = 0;

            foreach (var fileName in files.Select(x => x.Name))
            {
                string fileNameWithoutExtension = Regex.Replace(fileName, @"\.[a-zA-Z0-9]{3}$", "");

                foreach (string fileNameFragment in fileNameWithoutExtension.Split(' ', ',', '-', ';', '.'))
                {
                    if (fileNameFragment.Trim().Length > 0)
                    {
                        Tag tag = tags.FirstOrDefault(x => x.Name.Equals(fileNameFragment.Trim()));

                        if (tag == null)
                        {
                            Tag newTag = new Tag(fileNameFragment.Trim(), fileName);
                            tags.Add(newTag);
                        }
                        else
                        {
                            tag.FilesWithTag.Add(fileName);
                        }
                    }
                }

                OnStatusChanged("Processed " + processedFiles + " out of " + files.Count);
                processedFiles++;
            }

            OnStatusChanged("Processed all tags.");

            return tags.Where(x => x.FilesWithTag.Count > 1).OrderByDescending(x => x.FilesWithTag.Count).ToList();
        }

        protected void OnStatusChanged(string status)
        {
            if (StatusChanged != null)
                StatusChanged(status);
        }
    }
}
