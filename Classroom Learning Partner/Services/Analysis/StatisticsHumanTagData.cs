using System.Collections.Generic;

namespace Classroom_Learning_Partner.Services
{
    public class StatisticsHumanTagData
    {
        public struct TagParts
        {
            public TagParts(string tag, string content)
            {
                Tag = tag;
                Content = content;
            }

            public string Tag { get; }
            public string Content { get; }

            public string FormattedValue
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(Content))
                    {
                        return Tag;
                    }

                    return $"{Tag} [{Content}]";
                }
            }
        }

        public StatisticsHumanTagData(int pageNumber, string studentName, string humanTags)
        {
            PageNumber = pageNumber;
            StudentName = studentName;

            foreach (var tag in humanTags.Split(';'))
            {
                var firstBracketIndex = tag.IndexOf('[');
                if (firstBracketIndex == -1)
                {
                    HumanTags.Add(new TagParts(tag.Trim(), string.Empty));
                    continue;
                }

                var firstPart = tag.Substring(0, firstBracketIndex).Trim();
                var secondPart = tag.Substring(firstBracketIndex).Replace("[", string.Empty).Replace("]", string.Empty).Trim();
                HumanTags.Add(new TagParts(firstPart, secondPart));
            }
        }

        #region Properties

        public int PageNumber { get; }
        public string StudentName { get; }
        public List<TagParts> HumanTags { get; } = new List<TagParts>();

        #endregion
    }
}
