using System.Collections.Generic;
using System.Linq;
using Catel;

namespace CLP.Entities
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> SubSetsOf<T>(this IEnumerable<T> source)
        {
            Argument.IsNotNull("source", source);

            var sourceList = source as IList<T> ?? source.ToList();
            if (!sourceList.Any())
            {
                return Enumerable.Repeat(Enumerable.Empty<T>(), 1);
            }

            var element = sourceList.Take(1);

            var haveNots = SubSetsOf(sourceList.Skip(1));
            var haves = haveNots.Select(set => element.Concat(set));

            return haves.Concat(haveNots);
        }
    }
}