using System;
using System.Collections.Generic;
using System.Linq;

namespace Classroom_Learning_Partner
{
    public static class RangeHelper
    {
        public static List<int> ParseStringToNumbers(string ranges)
        {
            var groups = ranges.Split(',');
            return groups.SelectMany(GetRangeNumbers).ToList();
        }

        private static List<int> GetRangeNumbers(string range)
        {
            //string justNumbers = new String(text.Where(Char.IsDigit).ToArray());

            var rangeNumbers = range.Split('-').Select(t => new String(t.Where(Char.IsDigit).ToArray())) // Digits Only
                                    .Where(t => !string.IsNullOrWhiteSpace(t)) // Only if has a value
                                    .Select(int.Parse) // digit to int
                                    .ToList();
            return rangeNumbers.Count == 2 ? Enumerable.Range(rangeNumbers.Min(), (rangeNumbers.Max() + 1) - rangeNumbers.Min()).ToList() : rangeNumbers;
        }
    }
}