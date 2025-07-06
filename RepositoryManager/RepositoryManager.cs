using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RepositoryManager
{
    /// <summary>
    /// Repository Manager for storing and retrieving JSON or XML strings with thread-safe operations
    /// </summary>
    public class RepositoryManager
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IContentValidator _contentValidator;
        // The proper locking for initialization to ensure it's called only once
        private volatile bool _isInitialized = false;
        private readonly object _initializationLock = new object();

        /// <summary>
        /// Default constructor using in-memory storage
        /// </summary>
        public RepositoryManager() : this(new InMemoryStorageProvider(), new DefaultContentValidator())
        {
        }

        /// <summary>
        /// Constructor with dependency injection for storage and validation
        /// </summary>
        /// <param name="storageProvider">Storage provider implementation</param>
        /// <param name="contentValidator">Content validator implementation</param>
        public RepositoryManager(IStorageProvider storageProvider, IContentValidator contentValidator)
        {
            _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            _contentValidator = contentValidator ?? throw new ArgumentNullException(nameof(contentValidator));
        }

        /// <summary>
        /// Prepares the repository for use. Can only be called once.
        /// Uses double-checked locking pattern to ensure thread-safe initialization.
        /// </summary>
        public void Initialize()
        {
            // First check: Quick exit if already initialized (avoids unnecessary locking)
            // This check is outside the lock for performance - most calls after initialization
            // will return immediately without acquiring the lock
            if (_isInitialized)
                return;
        
            // Enter critical section: Only one thread can execute this block at a time
            // Using a dedicated lock object (_initializationLock) to avoid lock contention
            // with other operations and prevent potential deadlocks
            lock (_initializationLock)
            {
                // Second check: Re-verify initialization status inside the lock
                // This handles the race condition where multiple threads might have
                // passed the first check before any of them acquired the lock
                // Without this check, multiple threads could initialize the repository
                if (_isInitialized)
                    return;
        
                // Perform actual initialization - this code runs exactly once
                // regardless of how many threads call Initialize() concurrently
                _storageProvider.Initialize();
                
                // Set the flag to indicate initialization is complete
                // The volatile keyword on _isInitialized ensures this write
                // is immediately visible to all other threads
                _isInitialized = true;
            }
            // Lock is automatically released here when exiting the lock block
        }

        /// <summary>
        /// Stores an item in the repository with validation
        /// </summary>
        /// <param name="itemName">Unique identifier for the item</param>
        /// <param name="itemContent">Content to store</param>
        /// <param name="itemType">Content type (1 = JSON, 2 = XML)</param>
        /// <exception cref="ArgumentException">Thrown when validation fails or item already exists</exception>
        /// <exception cref="InvalidOperationException">Thrown when repository is not initialized</exception>
        public void Register(string itemName, string itemContent, int itemType)
        {
            EnsureInitialized();
            ValidateInput(itemName, itemContent, itemType);

            var contentType = (ContentType)itemType;
            
            // Validate content based on type
            if (!_contentValidator.IsValid(itemContent, contentType))
            {
                throw new ArgumentException($"Invalid {contentType} content provided.", nameof(itemContent));
            }

            // Check if item already exists
            if (_storageProvider.Exists(itemName))
            {
                throw new ArgumentException($"Item with name '{itemName}' already exists.", nameof(itemName));
            }

            var item = new RepositoryItem(itemName, itemContent, contentType);
            _storageProvider.Store(itemName, item);
        }

        /// <summary>
        /// Retrieves the content of an item by its name
        /// </summary>
        /// <param name="itemName">Name of the item to retrieve</param>
        /// <returns>Content of the item</returns>
        /// <exception cref="KeyNotFoundException">Thrown when item is not found</exception>
        /// <exception cref="InvalidOperationException">Thrown when repository is not initialized</exception>
        public string Retrieve(string itemName)
        {
            EnsureInitialized();
            ValidateItemName(itemName);

            var item = _storageProvider.Retrieve(itemName);
            if (item == null)
            {
                throw new KeyNotFoundException($"Item with name '{itemName}' not found.");
            }

            return item.Content;
        }

        /// <summary>
        /// Returns the type of the item (1 for JSON, 2 for XML)
        /// </summary>
        /// <param name="itemName">Name of the item</param>
        /// <returns>Type of the item</returns>
        /// <exception cref="KeyNotFoundException">Thrown when item is not found</exception>
        /// <exception cref="InvalidOperationException">Thrown when repository is not initialized</exception>
        public int GetType(string itemName)
        {
            EnsureInitialized();
            ValidateItemName(itemName);

            var item = _storageProvider.Retrieve(itemName);
            if (item == null)
            {
                throw new KeyNotFoundException($"Item with name '{itemName}' not found.");
            }

            return (int)item.ContentType;
        }

        /// <summary>
        /// Removes an item from the repository
        /// </summary>
        /// <param name="itemName">Name of the item to remove</param>
        /// <exception cref="InvalidOperationException">Thrown when repository is not initialized</exception>
        public void Deregister(string itemName)
        {
            EnsureInitialized();
            ValidateItemName(itemName);

            _storageProvider.Remove(itemName);
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Repository must be initialized before use. Call Initialize() method.");
            }
        }

        private void ValidateInput(string itemName, string itemContent, int itemType)
        {
            ValidateItemName(itemName);
            
            if (string.IsNullOrWhiteSpace(itemContent))
                throw new ArgumentException("Item content cannot be null or empty.", nameof(itemContent));

            if (!Enum.IsDefined(typeof(ContentType), itemType))
                throw new ArgumentException($"Invalid item type: {itemType}. Valid types are 1 (JSON) and 2 (XML).", nameof(itemType));
        }

        private void ValidateItemName(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentException("Item name cannot be null or empty.", nameof(itemName));
        }
    }

    /// <summary>
    /// Represents content types supported by the repository
    /// </summary>
    public enum ContentType
    {
        Json = 1,
        Xml = 2
    }

    /// <summary>
    /// Represents an item stored in the repository
    /// </summary>
    public class RepositoryItem
    {
        public string Name { get; }
        public string Content { get; }
        public ContentType ContentType { get; }
        public DateTime CreatedAt { get; }

        public RepositoryItem(string name, string content, ContentType contentType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ContentType = contentType;
            CreatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Interface for storage providers to enable easy switching between storage implementations
    /// </summary>
    public interface IStorageProvider
    {
        void Initialize();
        void Store(string key, RepositoryItem item);
        RepositoryItem Retrieve(string key);
        bool Exists(string key);
        void Remove(string key);
    }

    /// <summary>
    /// Interface for content validators to enable extensibility for new content types
    /// </summary>
    public interface IContentValidator
    {
        bool IsValid(string content, ContentType contentType);
    }

    /// <summary>
    /// Thread-safe in-memory storage implementation
    /// </summary>
    public class InMemoryStorageProvider : IStorageProvider
    {
        // Uses ConcurrentDictionary for thread-safe storage operations
        private readonly ConcurrentDictionary<string, RepositoryItem> _storage = new ConcurrentDictionary<string, RepositoryItem>();

        public void Initialize()
        {
            // No initialization required for in-memory storage
        }

        public void Store(string key, RepositoryItem item)
        {
            _storage.TryAdd(key, item);
        }

        public RepositoryItem Retrieve(string key)
        {
            _storage.TryGetValue(key, out var item);
            return item;
        }

        public bool Exists(string key)
        {
            return _storage.ContainsKey(key);
        }

        public void Remove(string key)
        {
            _storage.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Default content validator with real validation logic
    /// </summary>
    public class DefaultContentValidator : IContentValidator
    {
        public bool IsValid(string content, ContentType contentType)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            switch (contentType)
            {
                case ContentType.Json:
                    return IsValidJson(content);
                case ContentType.Xml:
                    return IsValidXml(content);
                default:
                    return false;
            }
        }

        private bool IsValidJson(string content)
        {
            try
            {
                JToken.Parse(content);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsValidXml(string content)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(content);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
