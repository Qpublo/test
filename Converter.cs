using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace KeysConversion
{
    internal class Converter
    {
        private string localKey;
        private string salt;
        private string zmk;
        private string zmkSBlock;
        private string HSMAddress;
        private int? HSMPort;
        private bool objectIsInitialized = false;
        public string kcv = null;
        public Converter
            (
            string localKey,
            string salt,
            string zmk,
            string zmkSBlock,
            string HSMAddress,
            int? HSMPort
            )
        {
            this.localKey = localKey;
            this.salt = salt;
            this.zmk = zmk;
            this.zmkSBlock = zmkSBlock;
            this.HSMAddress = HSMAddress;
            this.HSMPort = HSMPort;
            objectIsInitialized = true;
        }

        public string convertKey(string localBdkBase64, bool testSystem)
        {
            kcv = null;
            if (!objectIsInitialized) throw new Exception("Converter object must be initialized");

            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(localKey, Encoding.ASCII.GetBytes(salt));
            byte[] rgbKey = rdb.GetBytes(32);
            byte[] rgbIV = rdb.GetBytes(16);
            
            byte[] bdkArray = KeysConversion.AES.decrypt(Convert.FromBase64String(localBdkBase64), rgbKey, rgbIV, CipherMode.CBC);
            //Console.WriteLine(KeysConversion.Tools.ByteArrayToHexString(bdkArray));
            string bdk = KeysConversion.Tools.ExtendedHexToHexString(KeysConversion.Tools.ByteArrayToHexString(bdkArray)/*, testSystem*/);

            //Console.WriteLine("Encrypted key: " + localBdkBase64);
            //Console.WriteLine("KSN MAC: " + KeysConversion.Tools.ByteArrayToHexString(KeysConversion.AES.decrypt(Convert.FromBase64String("r8eHXIKasRnVC5G2ePwSkA=="), rgbKey, rgbIV, CipherMode.CBC)));
            //Console.WriteLine("KSN MSR: " + KeysConversion.Tools.ByteArrayToHexString(KeysConversion.AES.decrypt(Convert.FromBase64String("OqqFxesyUCZ4ulg9l5XlnQ=="), rgbKey, rgbIV, CipherMode.CBC)));
            //Console.WriteLine("KSN EMV: " + KeysConversion.Tools.ByteArrayToHexString(KeysConversion.AES.decrypt(Convert.FromBase64String("XmkTxXr/yLtv9QmPZ12KOA=="), rgbKey, rgbIV, CipherMode.CBC)));
            //Console.WriteLine("KSN PIN: " + KeysConversion.Tools.ByteArrayToHexString(KeysConversion.AES.decrypt(Convert.FromBase64String("79AFCkTyol92LrNmAEnFWw=="), rgbKey, rgbIV, CipherMode.CBC)));

            //Console.WriteLine("1");

            //Console.WriteLine("BDK MAC: " + KeysConversion.Tools.ExtendedHexToHexString(KeysConversion.Tools.ByteArrayToHexString(KeysConversion.AES.decrypt(Convert.FromBase64String("1DTzKzDgUNFtP+modNuzdhPsF3F2DGJtIDySk8607Asp01GVCVKMMZuHGOrHGqgS"), rgbKey, rgbIV, CipherMode.CBC))));
            //Console.WriteLine("BDK MSR: " + KeysConversion.Tools.ExtendedHexToHexString(KeysConversion.Tools.ByteArrayToHexString(KeysConversion.AES.decrypt(Convert.FromBase64String("AR0UtubWQaXdEkmsHeZhQgsKb2mKIZNkKZP6bVRqy1eeSHYWSADuYQp6NkrIPDy8"), rgbKey, rgbIV, CipherMode.CBC))));
            //Console.WriteLine("BDK EMV: " + KeysConversion.Tools.ExtendedHexToHexString(KeysConversion.Tools.ByteArrayToHexString(KeysConversion.AES.decrypt(Convert.FromBase64String("1eAQoOxR0o1rJDRhT4N/ku4HNl8HCWJONWK0NtC01F0qftqJSsuLGBrSZPxuvMHC"), rgbKey, rgbIV, CipherMode.CBC))));
            //Console.WriteLine("BDK PIN: " + KeysConversion.Tools.ExtendedHexToHexString(KeysConversion.Tools.ByteArrayToHexString(KeysConversion.AES.decrypt(Convert.FromBase64String("QhyvjbeMBvh7faLmxD6pNSEq0MTCkFKyFngKw8CJlUNHHeDc9C0oWca7KF7K6O9g"), rgbKey, rgbIV, CipherMode.CBC))));
            //Console.WriteLine("2");
            //Console.WriteLine("KSN Test: " + KeysConversion.Tools.ByteArrayToHexString(KeysConversion.AES.decrypt(Convert.FromBase64String("ZQF+CDfuandJ/GbKNx7B4A=="), rgbKey, rgbIV, CipherMode.CBC)));


            //Console.WriteLine("Clear BDK: " + bdk);

            byte[] bdkKCV = KeysConversion.Tools.GenerateKCV(KeysConversion.Tools.HexStringToByteArray(bdk));
            byte[] bdkUnderZmk = KeysConversion.TDES.encrypt(KeysConversion.Tools.HexStringToByteArray(bdk), KeysConversion.Tools.HexStringToByteArray(zmk), null, CipherMode.ECB);
            byte[] command = KeysConversion.Tools.ConcatArrays<byte>
                (
                Encoding.ASCII.GetBytes("A6FFF"),
                KeysConversion.Tools.HexStringToByteArray(zmkSBlock),
                Encoding.ASCII.GetBytes("X"),
                Encoding.ASCII.GetBytes(KeysConversion.Tools.ByteArrayToHexString(bdkUnderZmk)),
                Encoding.ASCII.GetBytes("S#B0N00N00")
                );
            //Console.WriteLine("HSM command: " + KeysConversion.Tools.ByteArrayToHexString(command));
            KeysConversion.HSMClient HSM = new KeysConversion.HSMClient(HSMAddress, HSMPort);
            KeysConversion.HSMReply reply = HSM.sendCommand(command);

            byte[] bdkSBlock = null;
            if (reply.ErrorCode == "00" || reply.ErrorCode == "01")
            {
                if (reply.Data != null && reply.Data.Length > 6)
                {
                    byte[] receivedKCVArray = KeysConversion.Tools.PartOfArray(reply.Data, reply.Data.Length - 6, 6);
                    string receivedKCV = KeysConversion.Tools.ExtendedHexToHexString(KeysConversion.Tools.ByteArrayToHexString(receivedKCVArray));
                    //Console.WriteLine("bdkKCV=" + KeysConversion.Tools.ByteArrayToHexString(bdkKCV));
                    //Console.WriteLine("receivedKCV=" + receivedKCV);
                    if (receivedKCV == KeysConversion.Tools.ByteArrayToHexString(bdkKCV).Substring(0, 6))
                    {
                        kcv = receivedKCV;
                        bdkSBlock = KeysConversion.Tools.PartOfArray(reply.Data, 0, reply.Data.Length - 6);
                        return(KeysConversion.Tools.ByteArrayToHexString(bdkSBlock));
                    }
                    else
                    {
                        throw new Exception("Local and HSM KCVs are not equal");
                    }

                }
                else
                {
                    throw new Exception("HSM reply data length is invalid");
                }
            }
            else
            {
                throw new Exception("HSM has returned error code " + reply.ErrorCode);
            }
        }

        public string encryptKey(string keyForEncryption)
        {
            if (!objectIsInitialized) throw new Exception("Converter object must be initialized");

            Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(localKey, Encoding.ASCII.GetBytes(salt));
            byte[] rgbKey = rdb.GetBytes(32);
            byte[] rgbIV = rdb.GetBytes(16);
            
            byte[] encryptedKey = KeysConversion.AES.encrypt(KeysConversion.Tools.HexStringToByteArray(keyForEncryption), rgbKey, rgbIV, CipherMode.CBC);
            return Convert.ToBase64String(encryptedKey);
        }
    }
}
