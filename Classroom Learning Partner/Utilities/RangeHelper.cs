using System;
using System.Collections.Generic;
using System.Linq;
using ServiceModelEx;

namespace Classroom_Learning_Partner
{
    public static class RangeHelper
    {
        /// <summary>Parses a string containing numbers, commas, and dashes into a List of ints that represent the range of numbers in the string.</summary>
        public static List<int> ParseStringToIntNumbers(string ranges, bool isSorted = true, bool isDistinct = true)
        {
            var groups = ranges.Split(',');
            var numberList = groups.SelectMany(GetRangeNumbers).ToList();

            if (isSorted)
            {
                numberList.Sort();
            }

            if (isDistinct)
            {
                numberList = numberList.Distinct().ToList();
            }

            return numberList;
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

        public static string ParseIntNumbersToString(IEnumerable<int> numberList, bool isSort = false, bool isDistinct = false)
        {
            var enumeratedList = numberList as IList<int> ?? numberList.ToList();
            if (isSort)
            {
                enumeratedList.Sort();
            }

            if (isDistinct)
            {
                enumeratedList = enumeratedList.Distinct().ToList();
            }

            return string.Join(",", NumListToPossiblyDegenerateRanges(enumeratedList).Select(PrettyRange));
        }

        /// <summary>e.g. 1,3,5,6,7,8,9,10,12 becomes (1,1),(3,3),(5,10),(12,12)</summary>
        private static IEnumerable<Tuple<int, int>> NumListToPossiblyDegenerateRanges(IEnumerable<int> numList)
        {
            Tuple<int, int> currentRange = null;
            foreach (var num in numList)
            {
                if (currentRange == null)
                {
                    currentRange = Tuple.Create(num, num);
                }
                else if (currentRange.Item2 == num - 1)
                {
                    currentRange = Tuple.Create(currentRange.Item1, num);
                }
                else
                {
                    yield return currentRange;
                    currentRange = Tuple.Create(num, num);
                }
            }
            if (currentRange != null)
            {
                yield return currentRange;
            }
        }

        /// <summary>e.g. (1,1) becomes "1" (1,3) becomes "1-3"</summary>
        private static string PrettyRange(Tuple<int, int> range)
        {
            return range.Item1 == range.Item2 ? range.Item1.ToString() : string.Format("{0}-{1}", range.Item1, range.Item2);
        }
    }
}