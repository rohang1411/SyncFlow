using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncFlow.Models;
using SyncFlow.Services;

namespace SyncFlow.Tests;

[TestClass]
public class BasicTests
{
    [TestMethod]
    public void TransferProfile_Creation_SetsDefaultValues()
    {
        // Act
        var profile = new TransferProfile();

        // Assert
        Assert.AreNotEqual(Guid.Empty, profile.Id);
        Assert.AreEqual(string.Empty, profile.Name);
        Assert.IsNotNull(profile.SourceFolders);
        Assert.AreEqual(0, profile.SourceFolders.Count);
        Assert.AreEqual(string.Empty, profile.DestinationFolder);
        Assert.IsTrue(profile.CreatedDate > DateTime.MinValue);
        Assert.IsTrue(profile.LastModified > DateTime.MinValue);
    }

    [TestMethod]
    public void TransferProfile_IsValid_ValidProfile_ReturnsTrue()
    {
        // Arrange
        var profile = new TransferProfile
        {
            Name = "Test Profile",
            SourceFolders = new List<string> { "C:\\Source1", "C:\\Source2" },
            DestinationFolder = "D:\\Destination"
        };

        // Act
        var isValid = profile.IsValid();

        // Assert
        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public void TransferProfile_IsValid_EmptyName_ReturnsFalse()
    {
        // Arrange
        var profile = new TransferProfile
        {
            Name = "",
            SourceFolders = new List<string> { "C:\\Source" },
            DestinationFolder = "D:\\Destination"
        };

        // Act
        var isValid = profile.IsValid();

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void TransferResult_GetSummary_SuccessfulTransfer_ReturnsSuccessMessage()
    {
        // Arrange
        var result = new TransferResult
        {
            Success = true,
            FilesTransferred = 10,
            FilesSkipped = 2
        };

        // Act
        var message = result.GetSummary();

        // Assert
        Assert.IsTrue(message.Contains("Transfer completed successfully"));
        Assert.IsTrue(message.Contains("10 files transferred"));
    }

    [TestMethod]
    public void TransferStatus_GetDisplayMessage_AllStatuses_ReturnsCorrectMessages()
    {
        // Test all enum values
        Assert.AreEqual("Ready", TransferStatus.Ready.GetDisplayMessage());
        Assert.AreEqual("Running...", TransferStatus.Running.GetDisplayMessage());
        Assert.AreEqual("Verifying...", TransferStatus.Verifying.GetDisplayMessage());
        Assert.AreEqual("Completed Successfully", TransferStatus.Completed.GetDisplayMessage());
        Assert.AreEqual("Failed with Errors", TransferStatus.Failed.GetDisplayMessage());
        Assert.AreEqual("Cancelled", TransferStatus.Cancelled.GetDisplayMessage());
    }
}