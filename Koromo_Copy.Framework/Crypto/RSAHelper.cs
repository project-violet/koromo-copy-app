// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Koromo_Copy.Framework.Crypto
{
    public class RSAHelper
    {
        /// <summary>
        /// Create new private key, pulibc key pair.
        /// </summary>
        /// <returns></returns>
        public static (string, string) CreateKey()
        {
            var rsa = new RSACryptoServiceProvider(2048);
            var private_key = RSAKeyExtensions.ToXmlString(rsa, true);
            var public_key = RSAKeyExtensions.ToXmlString(rsa, false);
            return (private_key, public_key);
        }

        public static byte[] Encrypt(byte[] target, string public_key)
        {
            var rsa = new RSACryptoServiceProvider(2048);
            RSAKeyExtensions.FromXmlString(rsa, public_key);
            return rsa.Encrypt(target, false);
        }

        public static byte[] Decrypt(byte[] target, string private_key)
        {
            var rsa = new RSACryptoServiceProvider(2048);
            RSAKeyExtensions.FromXmlString(rsa, private_key);
            return rsa.Decrypt(target, false);
        }
    }
}
