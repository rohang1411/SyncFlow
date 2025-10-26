using System;

namespace SyncFlow.Models
{
    public class TransferError
    {
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string ErrorType { get; set; } = string.Empty;
        
        public string FormattedError => $"[{Timestamp:HH:mm:ss}] {ErrorType}: {ErrorMessage}";
        public string ShortPath => System.IO.Path.GetFileName(SourcePath);
    }
}