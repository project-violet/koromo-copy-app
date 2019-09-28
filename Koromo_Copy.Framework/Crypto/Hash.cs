// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Koromo_Copy.Framework.Crypto
{
    public static class Hash
    {
        public static string GetFileHash(this string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                SHA512Managed sha = new SHA512Managed();
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }

        public static string GetHashSHA1(this string str)
        {
            SHA1Managed sha = new SHA1Managed();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public static string GetHashSHA256(this string str)
        {
            SHA256Managed sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public static string GetHashSHA512(this string str)
        {
            SHA512Managed sha = new SHA512Managed();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }

        public static string GetHashMD5(this string str)
        {
            var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }
    }
}
