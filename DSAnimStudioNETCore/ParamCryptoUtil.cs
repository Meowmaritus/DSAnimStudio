using SoulsAssetPipeline;
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

        private static object _lock_regulationKeys = new object();
        private static Dictionary<SoulsGames, byte[]> regulationKeys = new Dictionary<SoulsGames, byte[]>();

        public static byte[] GetRegulationKey(SoulsGames game)
        {
            byte[] result = null;
            lock (_lock_regulationKeys)
            {
                if (regulationKeys.ContainsKey(game))
                {
                    result = regulationKeys[game];
                }
                else
                {
                    var regKeyFileName = $@"{Main.Directory}\Res\RegulationKeys\{game}.bin";
                    if (File.Exists(regKeyFileName))
                    {
                        regulationKeys[game] = result = File.ReadAllBytes(regKeyFileName);
                    }
                }
            }
            return result;
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

        //private static byte[] erRegulationKey = ParseHexString("99 BF FC 36 6A 6B C8 C6 F5 82 7D 09 36 02 D6 76 C4 28 92 A0 1C 20 7F B0 24 D3 AF 4E 49 3F EF 99");

        //private static byte[] ac6RegulationKey = ParseHexString("10 CE ED 47 7B 7C D9 D7 E6 93 8E 11 47 13 E7 87 D5 39 13 B1 0D 31 8E C1 35 E4 BE 50 50 4E 0E 10");

        // ERNR TODO
        //private static byte[] ernrRegulationKey = ParseHexString("9a 8e e9 0c 4c 01 a4 31 68 a1 7d 9d 75 e4 a7 d0 21 07 eb cf 43 d5 ac b0 55 4f 94 16 01 b5 79 18");

        /// <summary>
        /// Decrypts and unpacks DS3's regulation BND4 from the specified path.
        /// </summary>
        public static BND4 DecryptRegulation(string path, SoulsGames game)
        {
            return DecryptRegulation(File.ReadAllBytes(path), game);
        }


       

        /// <summary>
        /// Decrypts regulation only and returns file bytes.
        /// </summary>
        public static byte[] DecryptRegulationRaw(string path, SoulsGames game)
        {
            byte[] bytes = File.ReadAllBytes(path);
            bytes = DecryptByteArray(GetRegulationKey(game), bytes);
            return bytes;
        }

        /// <summary>
        /// Repacks and encrypts regulation BND4 to the specified path.
        /// </summary>
        public static void EncryptRegulation(string path, BND4 bnd, SoulsGames game)
        {
            byte[] bytes = bnd.Write();
            bytes = EncryptByteArray(GetRegulationKey(game), bytes);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, bytes);
        }

        /// <summary>
        /// Decrypts and unpacks regulation BND4 from the specified path.
        /// </summary>
        public static BND4 DecryptRegulation(byte[] bytes, SoulsGames game)
        {
            bytes = DecryptByteArray(GetRegulationKey(game), bytes);
            return BND4.Read(bytes);
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

    }
}
