using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DSAnimStudio
{
    /// <summary>
    /// Miscellaneous utility functions for SoulsFormats, mostly for internal use.
    /// </summary>
    public static class ParamCryptoUtil
    {
        /// <summary>
        /// Converts a hex string in format "AA BB CC DD" to a byte array.
        /// </summary>
        public static byte[] ParseHexString(string str)
        {
            string[] strings = str.Split(' ');
            byte[] bytes = new byte[strings.Length];
            for (int i = 0; i < strings.Length; i++)
                bytes[i] = Convert.ToByte(strings[i], 16);
            return bytes;
        }

        private static byte[] ds3RegulationKey = System.Text.Encoding.ASCII.GetBytes("ds3#jn/8_7(rsY9pg55GFN7VFL#+3n/)");

        /// <summary>
        /// Decrypts and unpacks DS3's regulation BND4 from the specified path.
        /// </summary>
        public static BND4 DecryptDS3Regulation(string path)
        {
            return DecryptDS3Regulation(File.ReadAllBytes(path));
        }

        /// <summary>
        /// Decrypts and unpacks DS3's regulation BND4 from the specified path.
        /// </summary>
        public static BND4 DecryptDS3Regulation(byte[] bytes)
        {
            bytes = DecryptByteArray(ds3RegulationKey, bytes);
            return BND4.Read(bytes);
        }

        /// <summary>
        /// Repacks and encrypts DS3's regulation BND4 to the specified path.
        /// </summary>
        public static void EncryptDS3Regulation(string path, BND4 bnd)
        {
            byte[] bytes = bnd.Write();
            bytes = EncryptByteArray(ds3RegulationKey, bytes);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, bytes);
        }

        private static byte[] EncryptByteArray(byte[] key, byte[] secret)
        {
            using (MemoryStream ms = new MemoryStream())
            using (AesManaged cryptor = new AesManaged())
            {
                cryptor.Mode = CipherMode.CBC;
                cryptor.Padding = PaddingMode.PKCS7;
                cryptor.KeySize = 256;
                cryptor.BlockSize = 128;

                byte[] iv = cryptor.IV;

                using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                {
                    cs.Write(secret, 0, secret.Length);
                }
                byte[] encryptedContent = ms.ToArray();

                byte[] result = new byte[iv.Length + encryptedContent.Length];

                Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);

                return result;
            }
        }

        private static byte[] erRegulationKey = ParseHexString("99 BF FC 36 6A 6B C8 C6 F5 82 7D 09 36 02 D6 76 C4 28 92 A0 1C 20 7F B0 24 D3 AF 4E 49 3F EF 99");

        private static byte[] ac6RegulationKey = ParseHexString("10 CE ED 47 7B 7C D9 D7 E6 93 8E 11 47 13 E7 87 D5 39 13 B1 0D 31 8E C1 35 E4 BE 50 50 4E 0E 10");

        // ERNR TODO
        private static byte[] ernrRegulationKey = ParseHexString("9a 8e e9 0c 4c 01 a4 31 68 a1 7d 9d 75 e4 a7 d0 21 07 eb cf 43 d5 ac b0 55 4f 94 16 01 b5 79 18");

        /// <summary>
        /// Decrypts and unpacks DS3's regulation BND4 from the specified path.
        /// </summary>
        public static BND4 DecryptERRegulation(string path)
        {
            return DecryptERRegulation(File.ReadAllBytes(path));
        }


        /// <summary>
        /// Decrypts and unpacks DS3's regulation BND4 from the specified path.
        /// </summary>
        public static BND4 DecryptAC6Regulation(string path)
        {
            return DecryptAC6Regulation(File.ReadAllBytes(path));
        }


        /// <summary>
        /// Decrypts and unpacks DS3's regulation BND4 from the specified path.
        /// </summary>
        public static BND4 DecryptERRegulation(byte[] bytes)
        {
            bytes = DecryptByteArray(erRegulationKey, bytes);
            //File.WriteAllBytes($"{Main.Directory}\\DEBUG_DECRYPTEDREGULATION_ER.bin", bytes);
            return BND4.Read(bytes);
        }

        /// <summary>
        /// Decrypts and unpacks DS3's regulation BND4 from the specified path.
        /// </summary>
        public static BND4 DecryptENRRRegulation(byte[] bytes)
        {
            bytes = DecryptByteArray(ernrRegulationKey, bytes);
            return BND4.Read(bytes);

            // ERNR DEBUG PLACEHOLDER
            //var memeBytes = File.ReadAllBytes(@"C:\Nightreign Stuff\GameParam_prod.bnd");
            //return BND4.Read(memeBytes);
        }

        /// <summary>
        /// Decrypts and unpacks DS3's regulation BND4 from the specified path.
        /// </summary>
        public static byte[] DecryptERRegulationRaw(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            bytes = DecryptByteArray(erRegulationKey, bytes);
            return bytes;
        }

        /// <summary>
        /// Repacks and encrypts DS3's regulation BND4 to the specified path.
        /// </summary>
        public static void EncryptERRegulation(string path, BND4 bnd)
        {
            byte[] bytes = bnd.Write();
            bytes = EncryptByteArray(erRegulationKey, bytes);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, bytes);
        }





        /// <summary>
        /// Decrypts and unpacks DS3's regulation BND4 from the specified path.
        /// </summary>
        public static BND4 DecryptAC6Regulation(byte[] bytes)
        {
            bytes = DecryptByteArray(ac6RegulationKey, bytes);
            return BND4.Read(bytes);
        }

        /// <summary>
        /// Decrypts and unpacks DS3's regulation BND4 from the specified path.
        /// </summary>
        public static byte[] DecryptEAC6RegulationRaw(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            bytes = DecryptByteArray(ac6RegulationKey, bytes);
            return bytes;
        }

        /// <summary>
        /// Repacks and encrypts DS3's regulation BND4 to the specified path.
        /// </summary>
        public static void EncryptAC6Regulation(string path, BND4 bnd)
        {
            byte[] bytes = bnd.Write();
            bytes = EncryptByteArray(ac6RegulationKey, bytes);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, bytes);
        }





        private static byte[] DecryptByteArray(byte[] key, byte[] secret)
        {
            byte[] iv = new byte[16];
            byte[] encryptedContent = new byte[secret.Length - 16];

            Buffer.BlockCopy(secret, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(secret, iv.Length, encryptedContent, 0, encryptedContent.Length);

            using (MemoryStream ms = new MemoryStream())
            using (AesManaged cryptor = new AesManaged())
            {
                cryptor.Mode = CipherMode.CBC;
                cryptor.Padding = PaddingMode.None;
                cryptor.KeySize = 256;
                cryptor.BlockSize = 128;

                using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateDecryptor(key, iv), CryptoStreamMode.Write))
                {
                    cs.Write(encryptedContent, 0, encryptedContent.Length);
                }
                return ms.ToArray();
            }
        }

        private static byte[] ds2RegulationKey = {
            0x40, 0x17, 0x81, 0x30, 0xDF, 0x0A, 0x94, 0x54, 0x33, 0x09, 0xE1, 0x71, 0xEC, 0xBF, 0x25, 0x4C };

        public static BND4 DecryptDS2Regulation(string path)
        {
            return DecryptDS2Regulation(File.ReadAllBytes(path));
        }

        public static BND4 DecryptDS2Regulation(byte[] bytes)
        {
            byte[] iv = new byte[16];
            iv[0] = 0x80;
            Array.Copy(bytes, 0, iv, 1, 11);
            iv[15] = 1;
            byte[] input = new byte[bytes.Length - 32];
            Array.Copy(bytes, 32, input, 0, bytes.Length - 32);
            using (var ms = new MemoryStream(input))
            {
                byte[] decrypted = CryptographyUtility.DecryptAesCtr(ms, ds2RegulationKey, iv);
                return BND4.Read(decrypted);
            }
        }
    }
}
