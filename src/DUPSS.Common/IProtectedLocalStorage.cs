using System.Threading.Tasks;

namespace DUPSS.Common // CHANGED: Namespace is now DUPSS.Common
{
    /// <summary>
    /// Defines the interface for a service that provides secure local storage.
    /// This is a simplified interface mimicking key aspects of Blazor's ProtectedLocalStorage
    /// for WPF applications, focusing on secure data persistence.
    /// </summary>
    public interface IProtectedLocalStorage
    {
        /// <summary>
        /// Retrieves a value from secure local storage asynchronously.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key under which the value is stored.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains
        /// a ProtectedBrowserStorageResult indicating success and the retrieved value,
        /// or a default value if not found.</returns>
        Task<ProtectedBrowserStorageResult<TValue>> GetAsync<TValue>(string key);

        /// <summary>
        /// Stores a value in secure local storage asynchronously.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to store.</typeparam>
        /// <param name="key">The key under which to store the value.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SetAsync<TValue>(string key, TValue value);

        /// <summary>
        /// Deletes a value from secure local storage asynchronously.
        /// </summary>
        /// <param name="key">The key of the value to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteAsync(string key);
    }

    /// <summary>
    /// Represents the result of a storage operation, indicating success and containing the value.
    /// This mimics the structure of Blazor's ProtectedBrowserStorageResult.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class ProtectedBrowserStorageResult<TValue>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the value retrieved from storage.
        /// </summary>
        public TValue? Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectedBrowserStorageResult{TValue}"/> class.
        /// </summary>
        /// <param name="success">True if the operation was successful; otherwise, false.</param>
        /// <param name="value">The retrieved value.</param>
        public ProtectedBrowserStorageResult(bool success, TValue? value)
        {
            Success = success;
            Value = value;
        }
    }
}
