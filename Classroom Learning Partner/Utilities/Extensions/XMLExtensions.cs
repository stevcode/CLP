using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Classroom_Learning_Partner
{
    public static class XMLExtensions
    {
        public static XAttribute AttributeAnyNS<T>(this T source, string localName) where T : XElement
        {
            return source.Attributes().SingleOrDefault(e => e.Name.LocalName == localName);
        }

        public static IEnumerable<XElement> ElementsAnyNS<T>(this IEnumerable<T> source, string localName) where T : XContainer
        {
            return source.Elements().Where(e => e.Name.LocalName == localName);
        }

        public static XElement ElementAnyNS<T>(this T source, string localName) where T : XContainer
        {
            return source.Elements().FirstOrDefault(e => e.Name.LocalName == localName);
        }
    }
}
