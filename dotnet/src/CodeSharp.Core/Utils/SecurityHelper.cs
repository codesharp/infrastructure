//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CodeSharp.Core.Utils
{
    /// <summary>提供对字符串文本的加密/解密功能
    /// <remarks>
    /// 重要字符串处理，任何非此类实现的处理均不是全局标准，建议以下处理统一使用本类以保证一致性：
    /// DES
    /// RSA
    /// MD5
    /// x509
    /// </remarks>
    /// </summary>
    public sealed class SecurityHelper
    {
        #region //HACK:简单加解密编码 2011-2-21以下加密方式不可变更 直接对应Nhibernate的连接串读取解密
        /// <summary>
        /// Base64编码加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Base64Encrypt(string input)
        {
            return Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(input ?? ""));
        }
        /// <summary>
        /// Base64编码解密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Base64Decrypt(string input)
        {
            return System.Text.Encoding.Unicode.GetString(Convert.FromBase64String(input ?? ""));
        }
        #endregion

        #region DES加密/解密
        private static readonly byte[] des_key_iv = ASCIIEncoding.ASCII.GetBytes("Workflow");//长度至少为8
        /// <summary>
        /// DES加密
        /// <remarks>默认Key&IV=Workflow</remarks>
        /// </summary>
        /// <param name="originalString"></param>
        /// <returns>base64字符串</returns>
        /// <exception cref="ArgumentNullException">参数originalString不能为空</exception>
        public static string DESEncrypt(string originalString)
        {
            return DESEncrypt(originalString, des_key_iv, des_key_iv);
        }
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="originalString"></param>
        /// <param name="rgbKey"></param>
        /// <param name="rgbIV"></param>
        /// <returns>base64字符串</returns>
        /// <exception cref="ArgumentNullException">参数originalString不能为空</exception>
        public static string DESEncrypt(string originalString, byte[] rgbKey, byte[] rgbIV)
        {
            if (string.IsNullOrEmpty(originalString))
                throw new ArgumentNullException("参数originalString不能为空");

            var cryptoProvider = new DESCryptoServiceProvider();
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream
                , cryptoProvider.CreateEncryptor(rgbKey, rgbIV)
                , CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cryptoStream))
            {
                writer.Write(originalString);
                writer.Flush();
                cryptoStream.FlushFinalBlock();
                writer.Flush();
                return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
        }
        /// <summary>
        /// DES解密
        /// <remarks>Key&IV：Workflow</remarks>
        /// </summary>
        /// <param name="cryptedString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">参数cryptedString不能为空</exception>
        public static string DESDecrypt(string cryptedString)
        {
            return DESDecrypt(cryptedString, des_key_iv, des_key_iv);
        }
        /// <summary>
        /// DES解密
        /// <remarks>Key&IV=Workflow</remarks>
        /// </summary>
        /// <param name="cryptedString"></param>
        /// <param name="rgbKey"></param>
        /// <param name="rgbIV"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">参数cryptedString不能为空</exception>
        public static string DESDecrypt(string cryptedString, byte[] rgbKey, byte[] rgbIV)
        {
            if (string.IsNullOrEmpty(cryptedString))
                throw new ArgumentNullException("参数cryptedString不能为空");

            var cryptoProvider = new DESCryptoServiceProvider();
            using (var memoryStream = new MemoryStream(Convert.FromBase64String(cryptedString)))
            using (var cryptoStream = new CryptoStream(memoryStream
                , cryptoProvider.CreateDecryptor(rgbKey, rgbIV)
                , CryptoStreamMode.Read))
            using (var reader = new StreamReader(cryptoStream))
                return reader.ReadToEnd();
        }
        #endregion

        #region RSA加密/解密 use xmlPublicKey or xmlPrivateKey
        /// <summary>  
        /// RAS加密 
        /// </summary>  
        /// <param name="xmlPublicKey">公钥</param>  
        /// <param name="originalString">明文</param>  
        /// <returns>密文</returns>  
        public static string RSAEncrypt(string xmlPublicKey, string originalString)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(xmlPublicKey);
                return RSAEncrypt(rsa, originalString);
            }
        }
        /// <summary>  
        /// RAS解密  
        /// </summary>  
        /// <param name="xmlPrivateKey">私钥</param>  
        /// <param name="cryptedString">密文</param>  
        /// <returns>明文</returns>  
        public static string RSADecrypt(string xmlPrivateKey, string cryptedString)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(xmlPrivateKey);
                return RSADecrypt(rsa, cryptedString);
            }
        }
        /// <summary>  
        /// 产生公钥和私钥对  
        /// </summary>  
        /// <returns>string[] 0:私钥;1:公钥</returns>  
        public static string[] RSAKey()
        {
            var keys = new string[2];
            var rsa = new RSACryptoServiceProvider();
            keys[0] = rsa.ToXmlString(true);
            keys[1] = rsa.ToXmlString(false);
            return keys;
        }
        #endregion

        #region RSA加密/解密 use keystore
        /// <summary>  
        /// 使用KeyStore中的密钥进行RAS加密 
        /// <remarks>
        /// 默认UseMachineKeyStore
        /// </remarks>
        /// </summary>  
        /// <param name="keyContainerName">密钥容器名称</param>  
        /// <param name="originalString">明文</param>  
        /// <returns>密文</returns>  
        public static string RSAEncryptUseKeyStore(string keyContainerName, string originalString)
        {
            var csp = new CspParameters() { KeyContainerName = keyContainerName, Flags = CspProviderFlags.UseMachineKeyStore };
            using (var rsa = new RSACryptoServiceProvider(csp))
                return RSAEncrypt(rsa, originalString);
        }
        /// <summary>  
        /// 使用KeyStore中的密钥进行RAS解密  
        /// <remarks>
        /// 默认UseMachineKeyStore
        /// </remarks>
        /// </summary>  
        /// <param name="keyContainerName">密钥容器名称</param>  
        /// <param name="cryptedString">密文</param>  
        /// <returns>明文</returns>  
        public static string RSADecryptUseKeyStore(string keyContainerName, string cryptedString)
        {
            var csp = new CspParameters() { KeyContainerName = keyContainerName, Flags = CspProviderFlags.UseMachineKeyStore };
            using (var rsa = new RSACryptoServiceProvider(csp))
                return RSADecrypt(rsa, cryptedString);
        }
        #endregion

        #region RSA加密/解密 use x509certificate
        /// <summary>  
        /// 使用证书进行RAS加密 
        /// <remarks>
        /// </remarks>
        /// </summary>  
        /// <param name="fileName">公钥证书文件 如c:\certificate.cer</param> 
        /// <param name="originalString">明文</param>  
        /// <returns>密文</returns>  
        /// <exception cref="CryptographicException"></exception>
        public static string RSAEncryptUseCertificate(string fileName, string originalString)
        {
            return RSAEncryptUseCertificate(new X509Certificate2(fileName), originalString);
        }
        /// <summary>  
        /// 使用证书进行RAS解密  
        /// <remarks>
        /// </remarks>
        /// </summary>  
        /// <param name="fileName">私钥证书文件 如c:\certificate.pfx</param> 
        /// <param name="password">证书的密码</param>
        /// <param name="cryptedString">密文</param>  
        /// <returns>明文</returns> 
        /// <exception cref="CryptographicException"></exception>
        public static string RSADecryptUseCertificate(string fileName, string password, string cryptedString)
        {
            return RSADecryptUseCertificate(new X509Certificate2(fileName, password), cryptedString);
        }
        /// <summary>  
        /// 使用证书进行RAS加密 
        /// <remarks>
        /// </remarks>
        /// </summary>  
        /// <param name="certificate">证书</param> 
        /// <param name="originalString">明文</param>  
        /// <returns>密文</returns>  
        /// <exception cref="CryptographicException"></exception>
        public static string RSAEncryptUseCertificate(X509Certificate2 certificate, string originalString)
        {
            using (var rsa = (RSACryptoServiceProvider)certificate.PublicKey.Key)
                return RSAEncrypt(rsa, originalString);
        }
        /// <summary>  
        /// 使用证书进行RAS解密  
        /// <remarks>
        /// </remarks>
        /// </summary>  
        /// <param name="certificate">证书</param> 
        /// <param name="password">证书的密码</param>
        /// <param name="cryptedString">密文</param>  
        /// <returns>明文</returns> 
        /// <exception cref="CryptographicException"></exception>
        public static string RSADecryptUseCertificate(X509Certificate2 certificate, string cryptedString)
        {
            using (var rsa = (RSACryptoServiceProvider)certificate.PrivateKey)
                return RSADecrypt(rsa, cryptedString);
        }
        /// <summary>
        /// 从计算机的证书存储区域中获取证书
        /// <remarks>StoreName.My, StoreLocation.LocalMachine</remarks>
        /// </summary>
        /// <param name="cn">证书名称</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">证书{0}不存在，请检查是否已经在计算机中导入该证书</exception>
        public static X509Certificate2 GetCertificateFromStore(string cn)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            foreach (var c in store.Certificates)
                if (c.SubjectName.Name == "CN=" + cn)
                    return c;
            throw new InvalidOperationException(string.Format("证书{0}不存在，请检查是否已经在计算机中导入该证书", cn));
        }
        #endregion

        /// <summary>MD5加密
        /// </summary>
        /// <param name="originalString"></param>
        /// <returns></returns>
        public static string MD5Encrypt(string originalString)
        {
            if (string.IsNullOrEmpty(originalString))
                throw new ArgumentNullException("参数originalString不能为空");

            var bytes = MD5.Create().ComputeHash(Encoding.Default.GetBytes(originalString));
            var strb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
                strb.Append(bytes[i].ToString("X2"));
            return strb.ToString();
        }

        //Private
        private static string RSAEncrypt(RSACryptoServiceProvider rsa, string originalString)
        {
            return Convert.ToBase64String(rsa.Encrypt(Encoding.Unicode.GetBytes(originalString), false));
        }
        private static string RSADecrypt(RSACryptoServiceProvider rsa, string cryptedString)
        {
            return Encoding.Unicode.GetString(rsa.Decrypt(Convert.FromBase64String(cryptedString), false));
        }
    }
}