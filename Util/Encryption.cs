using System;
using System.Security.Cryptography;
using System.Text;

namespace com.caijunxiong.util
{
    public class Encryption
    {
        //密钥
        private static string key = "realperson2017-10-15realperson2017-10-15realperson2017-10-15";

        //密码密钥
        private static string passwordKey = "realperson2017-10-10";

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="strText">要加密的字符串</param>
        /// <returns></returns>
        public static string Encrypt(string strText)
        {
            return Sha512Encrypt(strText);
            //return HMACSHA1Text(strText, key);
        }

        /// <summary>
        /// 加密密码
        /// </summary>
        /// <param name="password">要加密的密码</param>
        /// <returns></returns>
        public static string EncryptPassword(string password)
        {
            //            return Sha512Encrypt(password);
            password = MD5(password);
            return ToBase64hmac( password, passwordKey);
        }

        /// <summary>
        /// md5加密
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns></returns>
        public static string MD5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
            string str2 = "";
            for (int i = 0; i < result.Length; i++)
            {
                str2 += string.Format("{0:x}", result[i]);
            }
            return str2;
        }




        /// <summary>
        /// HMACSHA1算法加密并返回ToBase64String
        /// </summary>
        /// <param name="strText">签名参数字符串</param>
        /// <param name="strKey">密钥参数</param>
        /// <returns>返回一个签名值(即哈希值)</returns>
        public static string ToBase64hmac(string strText, string strKey)
        {
            HMACSHA1 myHMACSHA1 = new HMACSHA1(Encoding.UTF8.GetBytes(strKey));
            byte[] byteText = myHMACSHA1.ComputeHash(Encoding.UTF8.GetBytes(strText));
            return System.Convert.ToBase64String(byteText);
        }

        public static string Sha512Encrypt(string str)
        {
            string result = "";
            SHA512 s512 = new SHA512Managed();
            byte[] s = s512.ComputeHash(Encoding.UTF8.GetBytes(str));
            for (int i = 0; i < s.Length; i++)
            {
                result += s[i].ToString("X");
            }
            s512.Clear();
            return result;
        }

        /// <summary>
        /// HMACSHA1算法加密并返回String
        /// </summary>
        /// <param name="EncryptText">签名参数字符串</param>
        /// <param name="EncryptKey">密钥参数</param>
        /// <returns>返回一个签名值(即哈希值)</returns>
        public static string HMACSHA1Text(string EncryptText, string EncryptKey)
        {
            //HMACSHA1加密
            string message;
            string key;
            message = EncryptText;
            key = EncryptKey;
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(key);
            HMACSHA1 hmacsha1 = new HMACSHA1(keyByte);
            byte[] messageBytes = encoding.GetBytes(message);
            //byte[] hashmessage = hmacsha1.ComputeHash(messageBytes);
            SHA512 sha512 = SHA512.Create();
            byte[] hashmessage = sha512.ComputeHash(messageBytes);
            return Encoding.UTF8.GetString(hashmessage);

            //string[] theResStrings = BitConverter.ToString(hashmessage).Split('-');
            //string result = string.Concat(theResStrings);
            //return result;
        }

        /// <summary>
        /// HMACSHA1算法2加密并返回String
        /// </summary>
        /// <param name="EncryptText">签名参数字符串</param>
        /// <param name="EncryptKey">密钥参数</param>
        /// <returns>返回一个签名值(即哈希值)</returns>
        public static string HMACSHA1(string EncryptText, string EncryptKey)
        {
            //HMACSHA1加密
            HMACSHA1 hmacsha1 = new HMACSHA1();
            hmacsha1.Key = System.Text.Encoding.UTF8.GetBytes(EncryptKey);
            byte[] dataBuffer = System.Text.Encoding.UTF8.GetBytes(EncryptText);
            byte[] hashBytes = hmacsha1.ComputeHash(dataBuffer);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
