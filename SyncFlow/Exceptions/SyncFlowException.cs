namespace SyncFlow.Exceptions;

/// <summary>
/// Base exception class for SyncFlow application errors
/// </summary>
public class SyncFlowException : Exception
{
    public SyncFlowException() : base() { }

    public SyncFlowException(string message) : base(message) { }

    public SyncFlowException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when file transfer operations fail
/// </summary>
public class TransferException : SyncFlowException
{
    public string? SourcePath { get; }
    public string? DestinationPath { get; }

    public TransferException(string message) : base(message) { }

    public TransferException(string message, Exception innerException) : base(message, innerException) { }

    public TransferException(string message, string sourcePath, string destinationPath) : base(message)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
    }

    public TransferException(string message, string sourcePath, string destinationPath, Exception innerException) 
        : base(message, innerException)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
    }
}

/// <summary>
/// Exception thrown when profile validation fails
/// </summary>
public class ProfileValidationException : SyncFlowException
{
    public List<string> ValidationErrors { get; }

    public ProfileValidationException(string message, List<string> validationErrors) : base(message)
    {
        ValidationErrors = validationErrors ?? new List<string>();
    }

    public ProfileValidationException(List<string> validationErrors) 
        : base($"Profile validation failed: {string.Join(", ", validationErrors)}")
    {
        ValidationErrors = validationErrors ?? new List<string>();
    }
}