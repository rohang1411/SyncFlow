using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncFlow.Models
{
    public class EnhancedTransferResult
    {
        public bool IsSuccess { get; set; }
        public int TotalFiles { get; set; }
        public int SuccessfulFiles { get; set; }
        public int FailedFiles { get; set; }
        public long TotalBytes { get; set; }
        public long TransferredBytes { get; set; }
        public TimeSpan Duration { get; set; }
        public List<TransferError> Errors { get; set; } = new List<TransferError>();
        public StorageValidationResult? StorageInfo { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public string Summary => $"Transferred {SuccessfulFiles}/{TotalFiles} files successfully";
        public double SuccessRate => TotalFiles > 0 ? (double)SuccessfulFiles / TotalFiles * 100 : 0;
        public string FormattedDuration => Duration.ToString(@"hh\:mm\:ss");
        public string FormattedBytes => $"{FormatBytes(TransferredBytes)} / {FormatBytes(TotalBytes)}";
        
        public string DetailedSummary
        {
            get
            {
                var summary = $"Transfer completed in {FormattedDuration}\n";
                summary += $"Files: {SuccessfulFiles} successful, {FailedFiles} failed out of {TotalFiles} total\n";
                summary += $"Data: {FormattedBytes}\n";
                summary += $"Success rate: {SuccessRate:F1}%";
                
                if (Errors.Any())
                {
                    summary += $"\n\nErrors ({Errors.Count}):\n";
                    summary += string.Join("\n", Errors.Take(5).Select(e => $"• {e.ShortPath}: {e.ErrorMessage}"));
                    if (Errors.Count > 5)
                        summary += $"\n... and {Errors.Count - 5} more errors";
                }
                
                return summary;
            }
        }
        
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