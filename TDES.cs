using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace KeysConversion
{
    internal class TDES
    {
        TDES() { }

        public static readonly byte[] DEFAULT_DES_IV = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static byte[] encrypt(byte[] data, byte[] key, byte[] iv, CipherMode mode)
        {
            byte[] vector;
            if (iv == null) vector = DEFAULT_DES_IV;
            else vector = iv;
            TripleDES alg = TripleDES.Create();
            alg.Mode = mode;
            //alg.Padding = PaddingMode.None;
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
