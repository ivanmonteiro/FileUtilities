using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FileUtilities
{
    public class RepeatedFilesFinder
    {
        public delegate void ProgressChangedEventHandler(int progress);
        public delegate void StatusEventHandler(string status);
        public event ProgressChangedEventHandler ProgressChanged;
        public event StatusEventHandler StatusChanged;

        public List<HashedFile> GetRepeatedFiles(string directory)
        {
            List<HashedFile> hashedFiles = new List<HashedFile>();
            SHA1CryptoServiceProvider sha1Hasher = new SHA1CryptoServiceProvider();
            int processedHashesCount = 0;
            DateTime lastInfoMessageDate = DateTime.Now;

            OnStatusChanged("Getting files...");
            IList<FileInfo> files = Util.GetFileInfos(directory).ToList();
            IList<FileInfo> filesToHash = files.Where(x => files.Count(y => y.Length == x.Length) > 1).ToList();
            OnStatusChanged(files.Count + " files found. " + filesToHash.Count + " to hash.");

            OnStatusChanged("Processing hashes...");
            //Get all files with same size and hash only them
            foreach (FileInfo file in filesToHash)
            {
                string fileName = file.FullName;

                using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] computedHash = sha1Hasher.ComputeHash(fileStream);
                    string computedHashString = BitConverter.ToString(computedHash).Replace("-", "");
                    HashedFile hashedFile = hashedFiles.FirstOrDefault(x => x.ComputedHash.Equals(computedHashString));
                    if (hashedFile == null)
                        hashedFiles.Add(new HashedFile(computedHashString, fileName));
                    else
                        hashedFile.FilesWithHash.Add(fileName);
                    processedHashesCount++;

                    if (lastInfoMessageDate.AddSeconds(1) <= DateTime.Now)
                    {
                        lastInfoMessageDate = DateTime.Now;
                        OnProgressChanged((processedHashesCount / filesToHash.Count) * 100);
                        OnStatusChanged("Processed " + processedHashesCount + " out of " + filesToHash.Count);
                    }
                }
            }

            OnStatusChanged("Processed all hashes.");

            return hashedFiles.Where(x => x.FilesWithHash.Count > 1).ToList();
        }

        protected void OnProgressChanged(int progress)
        {
            if (ProgressChanged != null)
                ProgressChanged(progress);
        }

        protected void OnStatusChanged(string status)
        {
            if (StatusChanged != null)
                StatusChanged(status);
        }

    }
}
