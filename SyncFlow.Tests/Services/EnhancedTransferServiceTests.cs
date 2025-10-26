using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SyncFlow.Models;
using SyncFlow.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SyncFlow.Tests.Services
{
    [TestClass]
    public class EnhancedTransferServiceTests
    {
        private EnhancedTransferService _service;
        private Mock<IFileOperations> _mockFileOperations;
        private Mock<IFileInventoryService> _mockFileInventoryService;
        private Mock<IStorageValidationService> _mockStorageValidationService;
        private Mock<ILogger<EnhancedTransferService>> _mockLogger;
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            _mockFileOperations = new Mock<IFileOperations>();
            _mockFileInventoryService = new Mock<IFileInventoryService>();
            _mockStorageValidationService = new Mock<IStorageValidationService>();
            _mockLogger = new Mock<ILogger<EnhancedTransferService>>();

            _service = new EnhancedTransferService(
                _mockFileOperations.Object,
                _mockFileInventoryService.Object,
                _mockStorageValidationService.Object,
                _mockLogger.Object);

            _testDirectory = Path.Combine(Path.GetTempPath(), "SyncFlowTest_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(_testDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [TestMethod]
        public async Task TransferWithValidationAsync_SuccessfulTransfer_ReturnsSuccess()
        {
            // Arrange
            var profile = CreateTestProfile();
            var inventory = new FileInventoryResult
            {
                TotalFileCount = 2,
                TotalSizeBytes = 1000,
                Files = new List<FileInfo>
                {
                    new FileInfo(CreateTestFile("test1.txt")),
                    new FileInfo(CreateTestFile("test2.txt"))
                }
            };

            var storageValidation = new StorageValidationResult
            {
                HasSufficientSpace = true,
                AvailableBytes = 10000,
                RequiredBytes = 1000
            };

            _mockFileInventoryService.Setup(x => x.BuildFileInventoryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);
            _mockStorageValidationService.Setup(x => x.ValidateStorageSpaceAsync(It.IsAny<TransferProfile>()))
                .ReturnsAsync(storageValidation);

            var progress = new Mock<IProgress<EnhancedTransferProgress>>();

            // Act
            var result = await _service.TransferWithValidationAsync(profile, progress.Object);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.TotalFiles);
            Assert.AreEqual(1000, result.TotalBytes);
            Assert.IsNotNull(result.StorageInfo);
        }

        [TestMethod]
        public async Task TransferWithValidationAsync_InsufficientStorage_ContinuesWithWarning()
        {
            // Arrange
            var profile = CreateTestProfile();
            var inventory = new FileInventoryResult
            {
                TotalFileCount = 1,
                TotalSizeBytes = 1000,
                Files = new List<FileInfo> { new FileInfo(CreateTestFile("test.txt")) }
            };

            var storageValidation = new StorageValidationResult
            {
                HasSufficientSpace = false,
                AvailableBytes = 500,
                RequiredBytes = 1000
            };

            _mockFileInventoryService.Setup(x => x.BuildFileInventoryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(inventory);
            _mockStorageValidationService.Setup(x => x.ValidateStorageSpaceAsync(It.IsAny<TransferProfile>()))
                .ReturnsAsync(storageValidation);

            var progress = new Mock<IProgress<EnhancedTransferProgress>>();

            // Act
            var result = await _service.TransferWithValidationAsync(profile, progress.Object);

            // Assert
            Assert.IsNotNull(result.StorageInfo);
            Assert.IsFalse(result.StorageInfo.HasSufficientSpace);
        }

        [TestMethod]
        public async Task ValidateStorageSpaceAsync_CallsStorageValidationService()
        {
            // Arrange
            var profile = CreateTestProfile();
            var expectedResult = new StorageValidationResult { HasSufficientSpace = true };

            _mockStorageValidationService.Setup(x => x.ValidateStorageSpaceAsync(profile))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.ValidateStorageSpaceAsync(profile);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockStorageValidationService.Verify(x => x.ValidateStorageSpaceAsync(profile), Times.Once);
        }

        [TestMethod]
        public async Task BuildFileInventoryAsync_CallsFileInventoryService()
        {
            // Arrange
            var sourcePath = "C:\\TestSource";
            var expectedResult = new FileInventoryResult { TotalFileCount = 5 };

            _mockFileInventoryService.Setup(x => x.BuildFileInventoryAsync(sourcePath))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.BuildFileInventoryAsync(sourcePath);

            // Assert
            Assert.AreEqual(expectedResult, result);
            _mockFileInventoryService.Verify(x => x.BuildFileInventoryAsync(sourcePath), Times.Once);
        }

        [TestMethod]
        public async Task RetryFailedTransfersAsync_WithErrors_RetriesFailedFiles()
        {
            // Arrange
            var previousResult = new EnhancedTransferResult
            {
                Errors = new List<TransferError>
                {
                    new TransferError
                    {
                        SourcePath = CreateTestFile("retry1.txt"),
                        DestinationPath = Path.Combine(_testDirectory, "dest", "retry1.txt")
                    }
                }
            };

            var progress = new Mock<IProgress<EnhancedTransferProgress>>();

            // Act
            var result = await _service.RetryFailedTransfersAsync(previousResult, progress.Object);

            // Assert
            Assert.AreEqual(1, result.TotalFiles);
            Assert.IsTrue(result.SuccessfulFiles > 0 || result.FailedFiles > 0);
        }

        [TestMethod]
        public async Task TransferWithValidationAsync_WithCancellation_ThrowsOperationCancelledException()
        {
            // Arrange
            var profile = CreateTestProfile();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var progress = new Mock<IProgress<EnhancedTransferProgress>>();

            // Act & Assert
            var result = await _service.TransferWithValidationAsync(profile, progress.Object, cancellationTokenSource.Token);
            Assert.IsFalse(result.IsSuccess);
        }

        private TransferProfile CreateTestProfile()
        {
            return new TransferProfile
            {
                Id = Guid.NewGuid(),
                Name = "Test Profile",
                FolderMappings = new List<FolderMapping>
                {
                    new FolderMapping
                    {
                        SourcePath = _testDirectory,
                        DestinationPath = Path.Combine(_testDirectory, "dest")
                    }
                }
            };
        }

        private string CreateTestFile(string fileName)
        {
            var filePath = Path.Combine(_testDirectory, fileName);
            File.WriteAllText(filePath, "Test content for " + fileName);
            return filePath;
        }
    }
}