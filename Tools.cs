using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace KeysConversion
{
    internal class Tools
    {
        public static int[] HEX_VALUES = new int[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
        public const string HEX_ALPHABET = "0123456789ABCDEF";
        public static Dictionary<string, string> hex2hex = new Dictionary<string, string>()
            {
                { "30", "0"},
                { "31", "1"},
                { "32", "2"},
                { "33", "3"},
                { "34", "4"},
                { "35", "5"},
                { "36", "6"},
                { "37", "7"},
                { "38", "8"},
                { "39", "9"},
                { "41", "A"},
                { "42", "B"},
                { "43", "C"},
                { "44", "D"},
                { "45", "E"},
                { "46", "F"}
            };
        private const string padpair = "10";
        public static readonly byte[] KCV_ZEROES = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static readonly byte[] DEFAULT_DES_IV = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public static byte[] HexStringToByteArray(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int x = 0, i = 0; i < hex.Length; i += 2, x += 1)
            {
                bytes[x] = (byte)(HEX_VALUES[Char.ToUpper(hex[i + 0]) - '0'] << 4 | HEX_VALUES[Char.ToUpper(hex[i + 1]) - '0']);
            }
            return bytes;
        }
        public static string ByteArrayToHexString(byte[] bytes)
        {
            if (bytes is null) return "";
            StringBuilder Result = new StringBuilder(bytes.Length * 2);
            
            foreach (byte b in bytes)
            {
                Result.Append(HEX_ALPHABET[(int)(b >> 4)]);
                Result.Append(HEX_ALPHABET[(int)(b & 0xF)]);
            }
            return Result.ToString();
        }
        
        public static string ExtendedHexToHexString(string str)
        {
            //Console.WriteLine(str);
            if (str.Length % 2 != 0)
            {
                throw new ArgumentException("Incoming string length must be even");
            }
            string res = "";
            string pair;
            string? tempSymbol;
            for (int i=0; i< str.Length; i=i+2)
            {
                pair = str.Substring(i, 2);
                if (pair == padpair) return res;
                if (hex2hex.TryGetValue(pair, out tempSymbol)) res = res + tempSymbol;
                else throw new ArgumentException("Invalid pair: " + pair);
            }
            return res;
        }
        /*
        public static string ExtendedHexToHexString(string str, bool testSystem)
        {
            Console.WriteLine("2: " + str);
            if (str.Length != 96)
            {
                Console.WriteLine("3: " + str);
                throw new ArgumentException("Unexpected key length");
            }
            int len;
            if (testSystem) len = 64;
            else len = 96;
            string res = "";
            string pair;
            string? tempSymbol;
            for (int i = 0; i < len; i = i + 2)
            {
                pair = str.Substring(i, 2);
                if (pair == padpair) return res;
                if (hex2hex.TryGetValue(pair, out tempSymbol)) res = res + tempSymbol;
                else throw new ArgumentException("Invalid pair: " + pair);
            }
            return res;
        }
        */
        public static byte[] GenerateKCV(byte[] key)
        {
            byte[] kcv = KeysConversion.TDES.encrypt(KCV_ZEROES, key, DEFAULT_DES_IV, CipherMode.CBC);
            return kcv;
        }

        public static T[] ConcatArrays<T>(params T[]?[] arrays)
        {
            T[] result = new T[arrays.Sum(param => (param is null) ? 0 : param.Length)];
            int position = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                if (arrays[i] != null)
                {
                    arrays[i].CopyTo(result, position);
                    position = position + arrays[i].Length;
                }
            }
            return result; ;
        }

        public static T[] PartOfArray<T>(T[] array, int start, int count)
        {
            T[] ret = new T[count];
            Array.Copy(array, start, ret, 0, count);
            return ret;
        }

        public static byte[] applyMultiplicity(byte[] arr, int multiplicity, byte filler)
        {
            int filLength = arr.Length % multiplicity;
            if (filLength == 0) return arr;
            filLength = multiplicity - filLength; 
            byte[] fillerArr = new byte[filLength];
            for (int i=0; i< filLength; i++) fillerArr[i] = filler;
            return ConcatArrays(arr, fillerArr);
        }
    }
}
