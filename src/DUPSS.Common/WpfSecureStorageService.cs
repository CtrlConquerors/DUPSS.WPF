using System;
using System.IO;
using System.Security.Cryptography; // ADDED: Required for ProtectedData
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using DUPSS.Common; // Reference to your IProtectedLocalStorage from DUPSS.Common

namespace DUPSS.Common // Namespace is now DUPSS.Common
{
    /// <summary>
    /// Provides secure local storage for WPF applications using Windows Data Protection API (DPAPI).
    /// This implementation stores data in an encrypted file unique to the user and application.
    /// </summary>
    public class WpfSecureStorageService : IProtectedLocalStorage // Ensure this implements the DUPSS.Common.IProtectedLocalStorage
    {
        // Define a subfolder within ApplicationData to store your app's data
        private const string AppDataSubFolder = "DUPSS_WPF_Data";

        // Define a purpose string for DPAPI to ensure data separation and integrity.
        // This string is used to derive an encryption key specific to your application.
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("DUPSS.WPF.SecureStorage.Entropy");

        /// <summary>
        /// Gets the full path to the secure storage file for a given key.
        /// </summary>
        /// <param name="key">The key for the stored item.</param>
        /// <returns>The full file path.</returns>
        private string GetFilePath(string key)
        {
            // Get the path to the user's application data folder
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Combine with the application-specific subfolder
            string appSpecificPath = Path.Combine(appDataPath, AppDataSubFolder);

            // Create the directory if it doesn't exist
            if (!Directory.Exists(appSpecificPath))
            {
                Directory.CreateDirectory(appSpecificPath);
            }

            // Use a hash of the key to create a unique and safe filename
            // This prevents issues with invalid characters in keys and provides a consistent file naming.
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(key));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                string hashedKey = builder.ToString();
                return Path.Combine(appSpecificPath, $"{hashedKey}.dat");
            }
        }

        /// <summary>
        /// Retrieves a value from secure local storage asynchronously.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key under which the value is stored.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains
        /// a ProtectedBrowserStorageResult indicating success and the retrieved value,
        /// or a default value if not found.</returns>
        public Task<ProtectedBrowserStorageResult<TValue>> GetAsync<TValue>(string key)
        {
            return Task.Run(() =>
            {
                string filePath = GetFilePath(key);
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Secure storage: Key '{key}' not found at '{filePath}'.");
                    return new ProtectedBrowserStorageResult<TValue>(false, default);
                }

                try
                {
                    // Read encrypted bytes from the file
                    byte[] encryptedData = File.ReadAllBytes(filePath);

                    // Decrypt the data using DPAPI
                    byte[] decryptedData = ProtectedData.Unprotect(encryptedData, Entropy, DataProtectionScope.CurrentUser);

                    // Convert bytes back to string
                    string json = Encoding.UTF8.GetString(decryptedData);

                    // Deserialize the JSON string to the target type
                    TValue? value = JsonSerializer.Deserialize<TValue>(json);

                    Console.WriteLine($"Secure storage: Successfully retrieved and decrypted data for key '{key}'.");
                    return new ProtectedBrowserStorageResult<TValue>(true, value);
                }
                catch (CryptographicException ex)
                {
                    // This can happen if the data is tampered with, or if the user profile changes.
                    Console.WriteLine($"Secure storage error: Failed to decrypt data for key '{key}'. Data might be corrupted or inaccessible. {ex.Message}");
                    // Consider deleting the corrupted file here: File.Delete(filePath);
                    return new ProtectedBrowserStorageResult<TValue>(false, default);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Secure storage error: Failed to retrieve data for key '{key}'. {ex.Message}");
                    return new ProtectedBrowserStorageResult<TValue>(false, default);
                }
            });
        }

        /// <summary>
        /// Stores a value in secure local storage asynchronously.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to store.</typeparam>
        /// <param name="key">The key under which to store the value.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task SetAsync<TValue>(string key, TValue value)
        {
            return Task.Run(() =>
            {
                string filePath = GetFilePath(key);
                try
                {
                    // Serialize the value to a JSON string
                    string json = JsonSerializer.Serialize(value);
                    byte[] data = Encoding.UTF8.GetBytes(json);

                    // Encrypt the data using DPAPI
                    byte[] encryptedData = ProtectedData.Protect(data, Entropy, DataProtectionScope.CurrentUser);

                    // Write encrypted bytes to the file
                    File.WriteAllBytes(filePath, encryptedData);
                    Console.WriteLine($"Secure storage: Successfully encrypted and stored data for key '{key}'.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Secure storage error: Failed to store data for key '{key}'. {ex.Message}");
                    // Log the exception for debugging
                }
            });
        }

        /// <summary>
        /// Deletes a value from secure local storage asynchronously.
        /// </summary>
        /// <param name="key">The key of the value to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task DeleteAsync(string key)
        {
            return Task.Run(() =>
            {
                string filePath = GetFilePath(key);
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        Console.WriteLine($"Secure storage: Successfully deleted data for key '{key}'.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Secure storage error: Failed to delete data for key '{key}'. {ex.Message}");
                        // Log the exception for debugging
                    }
                }
                else
                {
                    Console.WriteLine($"Secure storage: No data found for key '{key}' to delete.");
                }
            });
        }
    }
}
