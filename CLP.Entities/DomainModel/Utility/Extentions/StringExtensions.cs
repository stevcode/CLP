using System.Text;
using Catel;

namespace CLP.Entities
{
    public static class StringExtensions
    {
        /// <summary>
        /// Tests a string to see if it is a numeric value (int, double).
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNumber(this string s)
        {
            Argument.IsNotNull("s", s);

            int intValue;
            var isInt = int.TryParse(s, out intValue);
            if (isInt)
            {
                return true;
            }

            double doubleValue;
            var isDouble = double.TryParse(s, out doubleValue);
            if (isDouble)
            {
                return true;
            }

            return false;
        }

        public static
    }
}