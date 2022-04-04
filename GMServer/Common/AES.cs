using GMServer.Models.Settings;
using System;
using System.IO;
using System.Security.Cryptography;

namespace GMServer.Common
{
    public static class AES
    {
        private static byte[] GetKeyBytes(string key)
        {
            return System.Text.Encoding.UTF8.GetBytes(key);
        }

        public static string Encrypt(string plainText, EncryptionSettings settings)
        {
            using (Aes algo = Aes.Create())
            {
                algo.Key = GetKeyBytes(settings.Key);

                // Create a decrytor to perform the stream transform.
                var encryptor = algo.CreateEncryptor(algo.Key, algo.IV);

                // Create the streams used for encryption.
                using (var ms = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write IV first
                            ms.Write(algo.IV, 0, algo.IV.Length);

                            // Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }

                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, EncryptionSettings settings)
        {
            using (Aes algo = Aes.Create())
            {
                algo.Key = GetKeyBytes(settings.Key);

                // Get bytes from input string
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // Create the streams used for decryption.
                using (MemoryStream ms = new MemoryStream(cipherBytes))
                {
                    // Read IV first
                    byte[] IV = new byte[16];
                    ms.Read(IV, 0, IV.Length);

                    // Assign IV to an algorithm
                    algo.IV = IV;

                    // Create a decrytor to perform the stream transform.
                    var decryptor = algo.CreateDecryptor(algo.Key, algo.IV);

                    using (var csDecrypt = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
