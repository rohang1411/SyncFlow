namespace SyncFlow.Models
{
    public class StorageValidationResult
    {
        public bool HasSufficientSpace { get; set; }
        public long RequiredBytes { get; set; }
        public long AvailableBytes { get; set; }
        public string DestinationDrive { get; set; } = string.Empty;
        public string FormattedMessage { get; set; } = string.Empty;
        public double UsagePercentage => RequiredBytes > 0 ? (double)RequiredBytes / AvailableBytes * 100 : 0;
        
        public string FormattedRequired => FormatBytes(RequiredBytes);
        public string FormattedAvailable => FormatBytes(AvailableBytes);
        public string FormattedShortage => HasSufficientSpace ? string.Empty : FormatBytes(RequiredBytes - AvailableBytes);
        
        private static string FormatBytes(long bytes)
        {
            if (bytes < 0) return "0 B";
            
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