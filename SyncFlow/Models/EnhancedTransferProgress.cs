using System;
using System.Collections.Generic;

namespace SyncFlow.Models
{
    public class EnhancedTransferProgress
    {
        public int TotalFiles { get; set; }
        public int ProcessedFiles { get; set; }
        public int SuccessfulFiles { get; set; }
        public int FailedFiles { get; set; }
        public long TotalBytes { get; set; }
        public long ProcessedBytes { get; set; }
        public List<TransferError> Errors { get; set; } = new List<TransferError>();
        public double PercentComplete => TotalFiles > 0 ? (double)ProcessedFiles / TotalFiles * 100 : 0;
        public string CurrentFile { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public TimeSpan ElapsedTime => DateTime.Now - StartTime;
        
        public string FormattedProgress => $"{ProcessedFiles}/{TotalFiles} files ({PercentComplete:F1}%)";
        public string FormattedBytes => $"{FormatBytes(ProcessedBytes)} / {FormatBytes(TotalBytes)}";
        
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}