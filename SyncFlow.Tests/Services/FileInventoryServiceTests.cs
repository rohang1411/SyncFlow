using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncFlow.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SyncFlow.Tests.Services
{
    [TestClass]
    public class FileInventoryServiceTests
    {
        private FileInventoryService _service;
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            _service = new FileInventoryService();
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
        public async Task BuildFileInventoryAsync_EmptyDirectory_ReturnsZeroFiles()
        {
            // Act
            var result = await _service.BuildFileInventoryAsync(_testDirectory);

            // Assert
            Assert.AreEqual(0, result.TotalFileCount);
            Assert.AreEqual(0, result.TotalSizeBytes);
            Assert.AreEqual(0, result.Files.Count);
        }

        [TestMethod]
        public async Task BuildFileInventoryAsync_WithFiles_ReturnsCorrectCount()
        {
            // Arrange
            var file1 = Path.Combine(_testDirectory, "test1.txt");
            var file2 = Path.Combine(_testDirectory, "test2.txt");
            
            await File.WriteAllTextAsync(file1, "Hello World");
            await File.WriteAllTextAsync(file2, "Test Content");

            // Act
            var result = await _service.BuildFileInventoryAsync(_testDirectory);

            // Assert
            Assert.AreEqual(2, result.TotalFileCount);
            Assert.IsTrue(result.TotalSizeBytes > 0);
            Assert.AreEqual(2, result.Files.Count);
        }

        [TestMethod]
        public async Task BuildFileInventoryAsync_WithSubdirectories_CountsAllFiles()
        {
            // Arrange
            var subDir = Path.Combine(_testDirectory, "subdir");
            Directory.CreateDirectory(subDir);
            
            await File.WriteAllTextAsync(Path.Combine(_testDirectory, "root.txt"), "Root file");
            await File.WriteAllTextAsync(Path.Combine(subDir, "sub.txt"), "Sub file");

            // Act
            var result = await _service.BuildFileInventoryAsync(_testDirectory);

            // Assert
            Assert.AreEqual(2, result.TotalFileCount);
            Assert.AreEqual(1, result.DirectoryCount);
        }

        [TestMethod]
        public async Task BuildFileInventoryAsync_NonExistentPath_ReturnsError()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDirectory, "nonexistent");

            // Act
            var result = await _service.BuildFileInventoryAsync(nonExistentPath);

            // Assert
            Assert.AreEqual(0, result.TotalFileCount);
            Assert.IsTrue(result.HasErrors);
            Assert.IsTrue(result.InaccessiblePaths.Count > 0);
        }

        [TestMethod]
        public async Task CalculateDirectorySizeAsync_WithFiles_ReturnsCorrectSize()
        {
            // Arrange
            var testContent = "This is test content for size calculation";
            var file1 = Path.Combine(_testDirectory, "size_test.txt");
            await File.WriteAllTextAsync(file1, testContent);

            // Act
            var size = await _service.CalculateDirectorySizeAsync(_testDirectory);

            // Assert
            Assert.IsTrue(size > 0);
            Assert.AreEqual(testContent.Length, size);
        }

        [TestMethod]
        public async Task CountFilesAsync_WithFiles_ReturnsCorrectCount()
        {
            // Arrange
            await File.WriteAllTextAsync(Path.Combine(_testDirectory, "count1.txt"), "test");
            await File.WriteAllTextAsync(Path.Combine(_testDirectory, "count2.txt"), "test");
            await File.WriteAllTextAsync(Path.Combine(_testDirectory, "count3.txt"), "test");

            // Act
            var count = await _service.CountFilesAsync(_testDirectory);

            // Assert
            Assert.AreEqual(3, count);
        }
    }
}