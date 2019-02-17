using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WattchDog.Extensions
{
    public static class StringExtensions
    {
        public static string ComputeSHA256(this string text)
        {
            using (var hash = SHA256.Create())
            {
                var outputBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(outputBytes).Replace("-", "").ToUpper();
            }
        }
    }
}