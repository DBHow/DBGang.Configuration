using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace DBGang.Configuration.SecuredJson
{
    public static class SecuredJsonConfigurationEncryptor
    {
        private const int BlockSize = 128;
        private const int KeySize = 256;
        private const int BlockByteSize = BlockSize / 8;
        private const int SaltByteSize = BlockSize / 8;
        private const int KeyByteSize = KeySize / 8;
        private const int SignatureByteSize = KeySize / 8;
        private const int Iteration = 100000;
        private const int MinimumMessageSize = 2 * SaltByteSize + 2 * BlockByteSize + SignatureByteSize;

        private static readonly RandomNumberGenerator _randomNumber = RandomNumberGenerator.Create();

        public static string Encrypt(string plainText, string passPhrase)
        {
            var authSalt = GetRandomBytes(SaltByteSize);
            var keySalt = GetRandomBytes(SaltByteSize);
            var iv = GetRandomBytes(BlockByteSize);
            var key = GetKey(passPhrase, keySalt);
            var authKey = GetKey(passPhrase, authSalt);

            // encrypt
            using var aes = CreateAes(key, iv);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            byte[] encrypted = memoryStream.ToArray();
            var mergedResult = MergeBytes(additionalCapacity: SignatureByteSize, authSalt, keySalt, iv, encrypted);

            // sign
            using var hmac = new HMACSHA256(authKey);
            var payloadLength = mergedResult.Length - SignatureByteSize;
            var signatureTag = hmac.ComputeHash(mergedResult, 0, payloadLength);
            signatureTag.CopyTo(mergedResult, payloadLength);

            return Convert.ToBase64String(mergedResult);
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                throw new ArgumentNullException(nameof(cipherText));
            }

            var cipherBytes = Convert.FromBase64String(cipherText);
            if (cipherBytes.Length < MinimumMessageSize)
            {
                throw new ArgumentException("Invalid length of encrypted data");
            }

            var authSalt = cipherBytes.AsSpan(0, SaltByteSize).ToArray();
            var keySalt = cipherBytes.AsSpan(SaltByteSize, SaltByteSize).ToArray();
            var iv = cipherBytes.AsSpan(2 * SaltByteSize, BlockByteSize).ToArray();
            var signatureTag = cipherBytes.AsSpan(cipherBytes.Length - SignatureByteSize, SignatureByteSize).ToArray();
            var start = authSalt.Length + keySalt.Length + iv.Length;
            var encrypted = cipherBytes.AsSpan(start, cipherBytes.Length - start - signatureTag.Length).ToArray();

            var authKey = GetKey(passPhrase, authSalt);
            var key = GetKey(passPhrase, keySalt);

            // verify signature
            using var hmac = new HMACSHA256(authKey);
            var payloadLength = cipherBytes.Length - SignatureByteSize;
            var signatureTagExpected = hmac.ComputeHash(cipherBytes, 0, payloadLength);

            // constant time checking to prevent timing attacks
            var signatureVerificationResult = 0;
            for (int i = 0; i < signatureTag.Length; i++)
            {
                signatureVerificationResult |= signatureTag[i] ^ signatureTagExpected[i];
            }

            if (signatureVerificationResult != 0)
            {
                throw new CryptographicException("Invalid signature");
            }

            // decrypt
            using var aes = CreateAes(key, iv);
            using var memoryStream = new MemoryStream(encrypted);
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }

        private static Aes CreateAes(byte[] key, byte[] iv)
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Key = key;
            aes.IV = iv;

            return aes;
        }

        private static byte[] GetKey(string passPhrase, byte[] saltBytes)
        {
            using var derivator = new Rfc2898DeriveBytes(passPhrase, saltBytes, Iteration, HashAlgorithmName.SHA256);
            return derivator.GetBytes(KeyByteSize);
        }

        private static byte[] GetRandomBytes(int numberOfBytes)
        {
            var randomBytes = new byte[numberOfBytes];
            _randomNumber.GetBytes(randomBytes);
            return randomBytes;
        }

        private static byte[] MergeBytes(int additionalCapacity = 0, params byte[][] byteArrarys)
        {
            var result = new byte[byteArrarys.Sum(each => each.Length) + additionalCapacity];
            var index = 0;

            for (int i = 0; i < byteArrarys.GetLength(0); i++)
            {
                byteArrarys[i].CopyTo(result, index);
                index += byteArrarys[i].Length;
            }

            return result;
        }
    }
}
