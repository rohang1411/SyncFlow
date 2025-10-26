using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SyncFlow.Models;
using SyncFlow.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SyncFlow.Tests.Services
{
    [TestClass]
    public class StorageValidationServiceTests
    {
        private StorageValidationService _service;
        private Mock<IFileInventoryService> _mockFileInventoryService;
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            _mockFileInventoryService = new Mock<IFileInventoryService>();
            _service = new StorageValidationService(_mockFileInventoryService.Object);
            _testDirectory = Path.GetTempPath();
        }

        [TestMethod]
        public async Task ValidateStorageSpaceAsync_SufficientSpace_ReturnsTrue()
        {
            // Arrange
            var requiredBytes = 1024L; // 1KB
            
            // Act
            var result = await _service.ValidateStorageSpaceAsync(_testDirectory, requiredBytes);

            // Assert
            Assert.IsTrue(result.HasSufficientSpace);
            Assert.AreEqual(requiredBytes, result.RequiredBytes);
            Assert.IsTrue(result.AvailableBytes > requiredBytes);
        }

        [TestMethod]
        public async Task ValidateStorageSpaceAsync_InsufficientSpace_ReturnsFalse()
        {
            // Arrange
            var requiredBytes = long.MaxValue; // Impossibly large requirement
            
            // Act
            var result = await _service.ValidateStorageSpaceAsync(_testDirectory, requiredBytes);

            // Assert
            Assert.IsFalse(result.HasSufficientSpace);
            Assert.AreEqual(requiredBytes, result.RequiredBytes);
            Assert.IsTrue(result.FormattedMessage.Contains("Insufficient space"));
        }

        [TestMethod]
        public async Task ValidateStorageSpaceAsync_WithProfile_CalculatesTotalSize()
        {
            // Arrange
            var profile = new TransferProfile
            {
                Name = "Test Profile",
                FolderMappings = new List<FolderMapping>
                {
                    new FolderMapping { SourcePath = "C:\\Source1", DestinationPath = _testDirectory },
                    new FolderMapping { SourcePath = "C:\\Source2", DestinationPath = _testDirectory }
                }
            };

            _mockFileInventoryService.Setup(x => x.BuildFileInventoryAsync("C:\\Source1"))
                .ReturnsAsync(new FileInventoryResult { TotalSizeBytes = 1000 });
            _mockFileInventoryService.Setup(x => x.BuildFileInventoryAsync("C:\\Source2"))
                .ReturnsAsync(new FileInventoryResult { TotalSizeBytes = 2000 });

            // Act
            var result = await _service.ValidateStorageSpaceAsync(profile);

            // Assert
            Assert.AreEqual(3000L, result.RequiredBytes);
            _mockFileInventoryService.Verify(x => x.BuildFileInventoryAsync("C:\\Source1"), Times.Once);
            _mockFileInventoryService.Verify(x => x.BuildFileInventoryAsync("C:\\Source2"), Times.Once);
        }

        [TestMethod]
        public async Task GetAvailableFreeSpaceAsync_ValidPath_ReturnsPositiveValue()
        {
            // Act
            var freeSpace = await _service.GetAvailableFreeSpaceAsync(_testDirectory);

            // Assert
            Assert.IsTrue(freeSpace > 0);
        }

        [TestMethod]
        public async Task GetDriveLetterAsync_ValidPath_ReturnsDriveLetter()
        {
            // Act
            var driveLetter = await _service.GetDriveLetterAsync(_testDirectory);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(driveLetter));
            Assert.AreNotEqual("Unknown Drive", driveLetter);
        }

        [TestMethod]
        public void IsSpaceCriticallyLow_WithHighThreshold_ReturnsTrue()
        {
            // Arrange
            var highThreshold = long.MaxValue;

            // Act
            var isCritical = _service.IsSpaceCriticallyLow(_testDirectory, highThreshold);

            // Assert
            Assert.IsTrue(isCritical);
        }

        [TestMethod]
        public void IsSpaceCriticallyLow_WithLowThreshold_ReturnsFalse()
        {
            // Arrange
            var lowThreshold = 1L; // 1 byte

            // Act
            var isCritical = _service.IsSpaceCriticallyLow(_testDirectory, lowThreshold);

            // Assert
            Assert.IsFalse(isCritical);
        }

        [TestMethod]
        public async Task ValidateStorageSpaceAsync_EmptyProfile_ReturnsError()
        {
            // Arrange
            var profile = new TransferProfile
            {
                Name = "Empty Profile",
                FolderMappings = new List<FolderMapping>()
            };

            // Act
            var result = await _service.ValidateStorageSpaceAsync(profile);

            // Assert
            Assert.IsFalse(result.HasSufficientSpace);
            Assert.IsTrue(result.FormattedMessage.Contains("No folder mappings"));
        }
    }
}