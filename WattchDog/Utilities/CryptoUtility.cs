using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WattchDog.Utilities
{
    public static class CryptoUtility
    {
        public static string GetRandomStr(int len)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var output = new StringBuilder();
            var outputLen = 0;

            using (var rng = new RNGCryptoServiceProvider())
            {
                while (outputLen < len)
                {
                    var byteCapture = new byte[1];
                    rng.GetBytes(byteCapture);
                    var character = (char)byteCapture[0];

                    if (validChars.Contains(character))
                    {
                        output.Append(character);
                        outputLen++;
                    }
                }
            }

            return output.ToString();
        }
    }
}