namespace SyncFlow.Models;

/// <summary>
/// Represents a detailed verification report comparing source and destination folders
/// </summary>
public class VerificationReport
{
    /// <summary>
    /// Files that exist in source but not in destination
    /// </summary>
    public List<string> MissingFiles { get; set; } = new();

    /// <summary>
    /// Files that exist in destination but not in source
    /// </summary>
    public List<string> ExtraFiles { get; set; } = new();

    /// <summary>
    /// Files that have size mismatches
    /// </summary>
    public Dictionary<string, string> SizeMismatches { get; set; } = new();

    /// <summary>
    /// Total number of files in source
    /// </summary>
    public int TotalSourceFiles { get; set; }

    /// <summary>
    /// Total number of files in destination
    /// </summary>
    public int TotalDestinationFiles { get; set; }

    /// <summary>
    /// Number of files that match
    /// </summary>
    public int MatchingFiles { get; set; }

    /// <summary>
    /// Indicates whether verification was successful (all files match)
    /// </summary>
    public bool IsSuccessful => MissingFiles.Count == 0 && SizeMismatches.Count == 0;

    /// <summary>
    /// Summary message
    /// </summary>
    public string Summary
    {
        get
        {
            if (IsSuccessful)
                return $"✓ Verification successful! All {TotalSourceFiles} files match.";

            var issues = new List<string>();
            if (MissingFiles.Count > 0)
                issues.Add($"{MissingFiles.Count} missing files");
            if (SizeMismatches.Count > 0)
                issues.Add($"{SizeMismatches.Count} size mismatches");
            if (ExtraFiles.Count > 0)
                issues.Add($"{ExtraFiles.Count} extra files");

            return $"⚠ Verification found issues: {string.Join(", ", issues)}";
        }
    }

    /// <summary>
    /// Detailed report as formatted text
    /// </summary>
    public string GetDetailedReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine($"Verification Report");
        report.AppendLine($"==================");
        report.AppendLine();
        report.AppendLine($"Total Source Files: {TotalSourceFiles}");
        report.AppendLine($"Total Destination Files: {TotalDestinationFiles}");
        report.AppendLine($"Matching Files: {MatchingFiles}");
        report.AppendLine();

        if (MissingFiles.Count > 0)
        {
            report.AppendLine($"Missing Files ({MissingFiles.Count}):");
            foreach (var file in MissingFiles.Take(10))
            {
                report.AppendLine($"  - {file}");
            }
            if (MissingFiles.Count > 10)
                report.AppendLine($"  ... and {MissingFiles.Count - 10} more");
            report.AppendLine();
        }

        if (SizeMismatches.Count > 0)
        {
            report.AppendLine($"Size Mismatches ({SizeMismatches.Count}):");
            foreach (var mismatch in SizeMismatches.Take(10))
            {
                report.AppendLine($"  - {mismatch.Key}: {mismatch.Value}");
            }
            if (SizeMismatches.Count > 10)
                report.AppendLine($"  ... and {SizeMismatches.Count - 10} more");
            report.AppendLine();
        }

        if (ExtraFiles.Count > 0)
        {
            report.AppendLine($"Extra Files in Destination ({ExtraFiles.Count}):");
            foreach (var file in ExtraFiles.Take(10))
            {
                report.AppendLine($"  - {file}");
            }
            if (ExtraFiles.Count > 10)
                report.AppendLine($"  ... and {ExtraFiles.Count - 10} more");
        }

        return report.ToString();
    }
}
