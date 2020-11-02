using System;
using System.IO;
using System.Security.Cryptography;

namespace DBGang.Configuration.SecuredJson
{
    public static class UtilHelper
    {
        private static readonly int _interation = 1000;
        private static readonly byte[] _salt = 
            {
                0x84, 0x9f, 0x0c, 0x69, 0x1b, 0x70, 0x26, 0x12,
                0x3b, 0x7e, 0xdc, 0xf2, 0xc3, 0xad, 0x0c, 0xf7,
                0xf6, 0xf5, 0xfd, 0xb4, 0xe6, 0xa2, 0xcd, 0xe7,
                0xe2, 0xf9, 0xe0, 0x6d, 0x6a, 0x9b, 0xa9, 0xbe
            };

        public static string Encrypt(string plainText, string passPhrase)
        {
            var key = new Rfc2898DeriveBytes(passPhrase, _salt, _interation);

            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            var key = new Rfc2898DeriveBytes(passPhrase, _salt, _interation);

            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);

            var result = cipherText;
            try
            {
                using var memoryStream = new MemoryStream(Convert.FromBase64String(result));
                using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var streamReader = new StreamReader(cryptoStream);

                result = streamReader.ReadToEnd();
            }
            catch (Exception)
            {

            }

            return result;
        }
    }
}
