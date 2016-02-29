using System.Text;
using Catel;

namespace CLP.Entities
{
    public static class IntegerExtensions
    {
        private const int COLUMN_BASE = 26;
        private const int DIGIT_MAX = 7;
        private const string DIGITS = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Converts int to a lowercase letter representation, where 1 = "a", 2 = "b", 27 = "aa", etc.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ToLetter(this int number)
        {
            Argument.IsNotNull("number", number);

            if (number <= 0)
            {
                return string.Empty;
            }

            if (number <= COLUMN_BASE)
            {
                return DIGITS[number - 1].ToString();
            }

            var sb = new StringBuilder().Append(' ', DIGIT_MAX);
            var current = number;
            var offset = DIGIT_MAX;
            while (current > 0)
            {
                sb[--offset] = DIGITS[--current % COLUMN_BASE];
                current /= COLUMN_BASE;
            }

            return sb.ToString(offset, DIGIT_MAX - offset);
        }
    }
}