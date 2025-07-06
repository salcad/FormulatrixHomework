using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RepositoryManager.Tests
{
    [TestClass]
    public class RepositoryManagerTests
    {
        private RepositoryManager _repository;
        private MockStorageProvider _mockStorage;
        private MockContentValidator _mockValidator;

        [TestInitialize]
        public void Setup()
        {
            _mockStorage = new MockStorageProvider();
            _mockValidator = new MockContentValidator();
            _repository = new RepositoryManager(_mockStorage, _mockValidator);
        }

        #region Initialization Tests

        [TestMethod]
        public void Initialize_CalledMultipleTimes_ShouldOnlyInitializeOnce()
        {
            // Act
            _repository.Initialize();
            _repository.Initialize();
            _repository.Initialize();

            // Assert
            Assert.AreEqual(1, _mockStorage.InitializeCallCount);
        }

        [TestMethod]
        public void Initialize_ConcurrentCalls_ShouldOnlyInitializeOnce()
        {
            // Arrange
            var tasks = new Task[10];
            
            // Act
            for (int i = 0; i < 10; i++)
            {
                tasks[i] = Task.Run(() => _repository.Initialize());
            }
            Task.WaitAll(tasks);

            // Assert
            Assert.AreEqual(1, _mockStorage.InitializeCallCount);
        }

        #endregion

        #region Register Tests

        [TestMethod]
        public void Register_ValidJsonContent_ShouldStoreSuccessfully()
        {
            // Arrange
            _repository.Initialize();
            var itemName = "test-json";
            var jsonContent = "{\"key\": \"value\"}";
            _mockValidator.SetValidationResult(true);

            // Act
            _repository.Register(itemName, jsonContent, 1);

            // Assert
            Assert.IsTrue(_mockStorage.Items.ContainsKey(itemName));
            Assert.AreEqual(jsonContent, _mockStorage.Items[itemName].Content);
            Assert.AreEqual(ContentType.Json, _mockStorage.Items[itemName].ContentType);
        }

        [TestMethod]
        public void Register_ValidXmlContent_ShouldStoreSuccessfully()
        {
            // Arrange
            _repository.Initialize();
            var itemName = "test-xml";
            var xmlContent = "<root><item>value</item></root>";
            _mockValidator.SetValidationResult(true);

            // Act
            _repository.Register(itemName, xmlContent, 2);

            // Assert
            Assert.IsTrue(_mockStorage.Items.ContainsKey(itemName));
            Assert.AreEqual(xmlContent, _mockStorage.Items[itemName].Content);
            Assert.AreEqual(ContentType.Xml, _mockStorage.Items[itemName].ContentType);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Register_NotInitialized_ShouldThrowException()
        {
            // Act
            _repository.Register("test", "{}", 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_NullItemName_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();

            // Act
            _repository.Register(null, "{}", 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_EmptyItemName_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();

            // Act
            _repository.Register("", "{}", 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_NullContent_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();

            // Act
            _repository.Register("test", null, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_EmptyContent_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();

            // Act
            _repository.Register("test", "", 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_InvalidItemType_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();

            // Act
            _repository.Register("test", "{}", 99);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_InvalidContent_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();
            _mockValidator.SetValidationResult(false);

            // Act
            _repository.Register("test", "invalid json", 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_DuplicateItemName_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();
            _mockValidator.SetValidationResult(true);
            _repository.Register("test", "{}", 1);

            // Act
            _repository.Register("test", "{\"other\": \"value\"}", 1);
        }

        #endregion

        #region Retrieve Tests

        [TestMethod]
        public void Retrieve_ExistingItem_ShouldReturnContent()
        {
            // Arrange
            _repository.Initialize();
            var itemName = "test";
            var content = "{}";
            _mockValidator.SetValidationResult(true);
            _repository.Register(itemName, content, 1);

            // Act
            var result = _repository.Retrieve(itemName);

            // Assert
            Assert.AreEqual(content, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Retrieve_NotInitialized_ShouldThrowException()
        {
            // Act
            _repository.Retrieve("test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Retrieve_NullItemName_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();

            // Act
            _repository.Retrieve(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void Retrieve_NonExistentItem_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();

            // Act
            _repository.Retrieve("non-existent");
        }

        #endregion

        #region GetType Tests

        [TestMethod]
        public void GetType_JsonItem_ShouldReturn1()
        {
            // Arrange
            _repository.Initialize();
            var itemName = "test-json";
            _mockValidator.SetValidationResult(true);
            _repository.Register(itemName, "{}", 1);

            // Act
            var result = _repository.GetType(itemName);

            // Assert
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void GetType_XmlItem_ShouldReturn2()
        {
            // Arrange
            _repository.Initialize();
            var itemName = "test-xml";
            _mockValidator.SetValidationResult(true);
            _repository.Register(itemName, "<root></root>", 2);

            // Act
            var result = _repository.GetType(itemName);

            // Assert
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetType_NotInitialized_ShouldThrowException()
        {
            // Act
            _repository.GetType("test");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetType_NonExistentItem_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();

            // Act
            _repository.GetType("non-existent");
        }

        #endregion

        #region Deregister Tests

        [TestMethod]
        public void Deregister_ExistingItem_ShouldRemoveItem()
        {
            // Arrange
            _repository.Initialize();
            var itemName = "test";
            _mockValidator.SetValidationResult(true);
            _repository.Register(itemName, "{}", 1);

            // Act
            _repository.Deregister(itemName);

            // Assert
            Assert.IsFalse(_mockStorage.Items.ContainsKey(itemName));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Deregister_NotInitialized_ShouldThrowException()
        {
            // Act
            _repository.Deregister("test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Deregister_NullItemName_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();

            // Act
            _repository.Deregister(null);
        }

        [TestMethod]
        public void Deregister_NonExistentItem_ShouldNotThrowException()
        {
            // Arrange
            _repository.Initialize();

            // Act & Assert (should not throw)
            _repository.Deregister("non-existent");
        }

        #endregion
    }

    #region Test Helper Classes

    public class MockStorageProvider : IStorageProvider
    {
        public Dictionary<string, RepositoryItem> Items { get; } = new Dictionary<string, RepositoryItem>();
        public bool InitializeCalled { get; private set; }
        public int InitializeCallCount { get; private set; }

        public void Initialize()
        {
            InitializeCalled = true;
            InitializeCallCount++;
        }

        public void Store(string key, RepositoryItem item)
        {
            Items[key] = item;
        }

        public RepositoryItem Retrieve(string key)
        {
            Items.TryGetValue(key, out var item);
            return item;
        }

        public bool Exists(string key)
        {
            return Items.ContainsKey(key);
        }

        public void Remove(string key)
        {
            Items.Remove(key);
        }
    }

    public class MockContentValidator : IContentValidator
    {
        private bool _validationResult = true;

        public void SetValidationResult(bool result)
        {
            _validationResult = result;
        }

        public bool IsValid(string content, ContentType contentType)
        {
            return _validationResult;
        }
    }

    #endregion

    #region Integration Tests with Real Implementations

    [TestClass]
    public class RepositoryManagerIntegrationTests
    {
        private RepositoryManager _repository;

        [TestInitialize]
        public void Setup()
        {
            _repository = new RepositoryManager();
        }

        [TestMethod]
        public void Register_ValidJson_ShouldWork()
        {
            // Arrange
            _repository.Initialize();
            var validJson = "{\"name\": \"test\", \"value\": 123}";

            // Act & Assert
            _repository.Register("test-json", validJson, 1);
            var retrieved = _repository.Retrieve("test-json");
            Assert.AreEqual(validJson, retrieved);
            Assert.AreEqual(1, _repository.GetType("test-json"));
        }

        [TestMethod]
        public void Register_ValidXml_ShouldWork()
        {
            // Arrange
            _repository.Initialize();
            var validXml = "<root><name>test</name><value>123</value></root>";

            // Act & Assert
            _repository.Register("test-xml", validXml, 2);
            var retrieved = _repository.Retrieve("test-xml");
            Assert.AreEqual(validXml, retrieved);
            Assert.AreEqual(2, _repository.GetType("test-xml"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_InvalidJson_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();
            var invalidJson = "{invalid json}";

            // Act
            _repository.Register("test", invalidJson, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Register_InvalidXml_ShouldThrowException()
        {
            // Arrange
            _repository.Initialize();
            var invalidXml = "<root><unclosed>";

            // Act
            _repository.Register("test", invalidXml, 2);
        }

        [TestMethod]
        public void FullWorkflow_ShouldWork()
        {
            // Arrange
            _repository.Initialize();
            var jsonContent = "{\"test\": true}";
            var xmlContent = "<test>true</test>";

            // Act & Assert
            _repository.Register("json-item", jsonContent, 1);
            _repository.Register("xml-item", xmlContent, 2);

            Assert.AreEqual(jsonContent, _repository.Retrieve("json-item"));
            Assert.AreEqual(xmlContent, _repository.Retrieve("xml-item"));
            Assert.AreEqual(1, _repository.GetType("json-item"));
            Assert.AreEqual(2, _repository.GetType("xml-item"));

            _repository.Deregister("json-item");
            
            try
            {
                _repository.Retrieve("json-item");
                Assert.Fail("Should have thrown KeyNotFoundException");
            }
            catch (KeyNotFoundException)
            {
                // Expected
            }

            // XML item should still exist
            Assert.AreEqual(xmlContent, _repository.Retrieve("xml-item"));
        }
    }

    #endregion

    #region Component Tests

    [TestClass]
    public class RepositoryItemTests
    {
        [TestMethod]
        public void Constructor_ValidParameters_ShouldCreateItem()
        {
            // Arrange
            var name = "test";
            var content = "{}";
            var contentType = ContentType.Json;
            var beforeCreation = DateTime.UtcNow;

            // Act
            var item = new RepositoryItem(name, content, contentType);

            // Assert
            Assert.AreEqual(name, item.Name);
            Assert.AreEqual(content, item.Content);
            Assert.AreEqual(contentType, item.ContentType);
            Assert.IsTrue(item.CreatedAt >= beforeCreation);
            Assert.IsTrue(item.CreatedAt <= DateTime.UtcNow);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullName_ShouldThrowException()
        {
            // Act
            new RepositoryItem(null, "{}", ContentType.Json);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullContent_ShouldThrowException()
        {
            // Act
            new RepositoryItem("test", null, ContentType.Json);
        }
    }

    [TestClass]
    public class DefaultContentValidatorTests
    {
        private DefaultContentValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _validator = new DefaultContentValidator();
        }

        [TestMethod]
        public void IsValid_ValidJson_ShouldReturnTrue()
        {
            // Arrange
            var validJson = "{\"key\": \"value\", \"number\": 123}";

            // Act
            var result = _validator.IsValid(validJson, ContentType.Json);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValid_ValidXml_ShouldReturnTrue()
        {
            // Arrange
            var validXml = "<root><item>value</item></root>";

            // Act
            var result = _validator.IsValid(validXml, ContentType.Xml);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValid_InvalidJson_ShouldReturnFalse()
        {
            // Arrange
            var invalidJson = "{invalid json}";

            // Act
            var result = _validator.IsValid(invalidJson, ContentType.Json);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_InvalidXml_ShouldReturnFalse()
        {
            // Arrange
            var invalidXml = "<root><unclosed>";

            // Act
            var result = _validator.IsValid(invalidXml, ContentType.Xml);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_NullContent_ShouldReturnFalse()
        {
            // Act
            var jsonResult = _validator.IsValid(null, ContentType.Json);
            var xmlResult = _validator.IsValid(null, ContentType.Xml);

            // Assert
            Assert.IsFalse(jsonResult);
            Assert.IsFalse(xmlResult);
        }

        [TestMethod]
        public void IsValid_EmptyContent_ShouldReturnFalse()
        {
            // Act
            var jsonResult = _validator.IsValid("", ContentType.Json);
            var xmlResult = _validator.IsValid("", ContentType.Xml);

            // Assert
            Assert.IsFalse(jsonResult);
            Assert.IsFalse(xmlResult);
        }
    }

    [TestClass]
    public class InMemoryStorageProviderTests
    {
        private InMemoryStorageProvider _storage;
        private RepositoryItem _testItem;

        [TestInitialize]
        public void Setup()
        {
            _storage = new InMemoryStorageProvider();
            _testItem = new RepositoryItem("test", "{}", ContentType.Json);
        }

        [TestMethod]
        public void Initialize_ShouldNotThrow()
        {
            // Act & Assert
            _storage.Initialize();
        }

        [TestMethod]
        public void Store_ShouldStoreItem()
        {
            // Act
            _storage.Store("test", _testItem);

            // Assert
            Assert.IsTrue(_storage.Exists("test"));
            Assert.AreEqual(_testItem, _storage.Retrieve("test"));
        }

        [TestMethod]
        public void Retrieve_NonExistentItem_ShouldReturnNull()
        {
            // Act
            var result = _storage.Retrieve("non-existent");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Exists_ExistingItem_ShouldReturnTrue()
        {
            // Arrange
            _storage.Store("test", _testItem);

            // Act
            var result = _storage.Exists("test");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Exists_NonExistentItem_ShouldReturnFalse()
        {
            // Act
            var result = _storage.Exists("non-existent");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Remove_ExistingItem_ShouldRemoveItem()
        {
            // Arrange
            _storage.Store("test", _testItem);

            // Act
            _storage.Remove("test");

            // Assert
            Assert.IsFalse(_storage.Exists("test"));
            Assert.IsNull(_storage.Retrieve("test"));
        }

        [TestMethod]
        public void Remove_NonExistentItem_ShouldNotThrow()
        {
            // Act & Assert
            _storage.Remove("non-existent");
        }
    }

    #endregion
}