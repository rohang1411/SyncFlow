using System.Collections.Generic;
using System.IO;

namespace SyncFlow.Models
{
    public class FileInventoryResult
    {
        public int TotalFileCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public List<FileInfo> Files { get; set; } = new List<FileInfo>();
        public List<string> InaccessiblePaths { get; set; } = new List<string>();
        public int DirectoryCount { get; set; }
        public bool HasErrors => InaccessiblePaths.Count > 0;
        
        public string FormattedSize => FormatBytes(TotalSizeBytes);
        public string Summary => $"{TotalFileCount} files ({FormattedSize}) in {DirectoryCount} directories";
        
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (System.Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}