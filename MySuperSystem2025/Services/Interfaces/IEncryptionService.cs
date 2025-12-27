namespace MySuperSystem2025.Services.Interfaces
{
    /// <summary>
    /// Interface for AES encryption service used to encrypt stored passwords.
    /// Never store passwords as plain text - always encrypt.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts a plain text password using AES encryption
        /// </summary>
        /// <param name="plainText">The plain text password to encrypt</param>
        /// <returns>Encrypted password as base64 string</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts an encrypted password back to plain text
        /// </summary>
        /// <param name="cipherText">The encrypted password (base64 string)</param>
        /// <returns>Decrypted plain text password</returns>
        string Decrypt(string cipherText);
    }
}
