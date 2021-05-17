using System;
using System.Globalization;
using System.Linq;
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
            var a = s.Split(',', '.', '\'', ' ').ToList();

            var sIntPart = a.FirstOrDefault();
            var sDecimalPart = String.Join("", a.Skip(1));

            string sCorrectString = string.IsNullOrEmpty(sDecimalPart) ? sIntPart : $"{sIntPart}.{sDecimalPart}";

            return Convert.ToDouble(sCorrectString, CultureInfo.InvariantCulture);
        }

    }
}
