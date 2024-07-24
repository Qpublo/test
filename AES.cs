using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace KeysConversion
{
    internal class AES
    {
        AES() { }
        public static byte[] decrypt(byte[] data, byte[] key, byte[] iv, CipherMode mode)
        {
            byte[] vector;
            if (iv == null) vector = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            else vector = iv;
            Aes alg = Aes.Create();
            alg.Mode = mode;
            alg.Padding = PaddingMode.Zeros;
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, alg.CreateDecryptor(key, vector), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }

        public static byte[] encrypt(byte[] data, byte[] key, byte[] iv, CipherMode mode)
        {
            byte[] vector;
            if (iv == null) vector = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            else vector = iv;
            Aes alg = Aes.Create();
            alg.Mode = mode;
            alg.Padding = PaddingMode.Zeros;
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, alg.CreateEncryptor(key, vector), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
