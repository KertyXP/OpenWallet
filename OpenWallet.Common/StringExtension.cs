using System;
using System.Text;

namespace OpenWallet.Common
{
    public static class StringExtension
    {

        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }


        public static int ToInt(this string s, int nDefaultValue)
        {
            int result;
            if (string.IsNullOrEmpty(s) || !int.TryParse(s, out result))
                return nDefaultValue;
            return result;
        }


        public static double ToDouble(this string s)
        {
            if (s.Contains(",") && s.Contains("."))
                s = s.Replace(".", "");
            return Convert.ToDouble(s.Replace('.', ','));
        }

    }
}
