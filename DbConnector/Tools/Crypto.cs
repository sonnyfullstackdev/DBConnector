﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DbConnector.Tools
{
    internal sealed class Crypto //cannot be inherited, and can only be accessed within the assembly
    {
        private static readonly Crypto _instance = new Crypto();
        private Crypto() { }
        static Crypto() { }
        public static Crypto Instance { get { return _instance; } }

        public string SimpleEncrypt(string input)
        {
            if (!string.IsNullOrEmpty(input))
                return Convert.ToBase64String(Encoding.Unicode.GetBytes(input));

            return input;
        }

        public string AdvancedEncryptDecrypt(string input)
        {
            byte[] keyArray;
            string key = "cohort35";
            MD5CryptoServiceProvider hash = new MD5CryptoServiceProvider();
            keyArray = hash.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            hash.Clear();
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            if (!input.IsBase64String()) //encrypt
            {
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(input);

                ICryptoTransform ctrans = tdes.CreateEncryptor();
                byte[] resultArray = ctrans.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                tdes.Clear();

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            else //decrypt
            {
                byte[] toDecryptArray = Convert.FromBase64String(input);
                ICryptoTransform ctrans = tdes.CreateDecryptor();
                try
                {
                    byte[] resultsArray = ctrans.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
                    tdes.Clear();

                    return UTF8Encoding.UTF8.GetString(resultsArray, 0, resultsArray.Length);
                }
                catch { throw; }
            }
        
        }

        //public string AdvancedEncrypt(string input)
        //{
        //    if (!string.IsNullOrEmpty(input))
        //    {
        //        byte[] keyArray;
        //        byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(input);
        //        string key = "cohort35";
        //        MD5CryptoServiceProvider hash = new MD5CryptoServiceProvider();
        //        keyArray = hash.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
        //        hash.Clear();

        //        TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
        //        tdes.Key = keyArray;
        //        tdes.Mode = CipherMode.ECB;
        //        tdes.Padding = PaddingMode.PKCS7;
        //        ICryptoTransform ctrans = tdes.CreateEncryptor();
        //        byte[] resultArray = ctrans.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        //        tdes.Clear();

        //        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        //    }

        //    return null;
        //}
        //public string AdvancedDecrypt(string input)
        //{
        //    if (!string.IsNullOrEmpty(input))
        //    {
        //        byte[] keyArray;
        //        byte[] toDecryptArray = Convert.FromBase64String(input);
        //        string key = "cohort35";
        //        MD5CryptoServiceProvider hash = new MD5CryptoServiceProvider();
        //        keyArray = hash.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
        //        hash.Clear();

        //        TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
        //        tdes.Key = keyArray;
        //        tdes.Mode = CipherMode.ECB;
        //        tdes.Padding = PaddingMode.PKCS7;
        //        ICryptoTransform ctrans = tdes.CreateDecryptor();
        //        try
        //        {
        //            byte[] resultsArray = ctrans.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
        //            tdes.Clear();

        //            return UTF8Encoding.UTF8.GetString(resultsArray, 0, resultsArray.Length);
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //    }
        //    return null;
        //}

        public string SimpleDecrypt(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return Encoding.Unicode.GetString(Convert.FromBase64String(input));          
            }
            return input;
           
        }
    }

    public static class EncryptDecrypt
    {
        public static string EncryptDecryptString(this string input)
        {
            return Crypto.Instance.AdvancedEncryptDecrypt(input);
        }
        //public static string DecryptString(this string input)
        //{
        //    return Crypto.Instance.AdvancedDecrypt(input);
        //}
        public static bool IsBase64String(this string input)
        {
            input = input.Trim();
            return (input.Length % 4 == 0) && Regex.IsMatch(input, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }
    }

}