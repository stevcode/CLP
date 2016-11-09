using System;

namespace CLP.Entities
{
    public interface ITag
    {
        /// <summary>Unique Identifier for the <see cref="ATagBase" />.</summary>
        string ID { get; set; }

        /// <summary>Unique Identifier for the <see cref="Person" /> who owns the <see cref="ATagBase" />.</summary>
        string OwnerID { get; set; }

        /// <summary>Date and Time the <see cref="ATagBase" /> was created.</summary>
        DateTime CreationDate { get; set; }

        /// <summary>Designates the <see cref="ITag" /> as invisible in the PageInfoPanel.</summary>
        bool IsHiddenTag { get; set; }

        /// <summary>From where the <see cref="ATagBase" /> originates.</summary>
        Origin Origin { get; set; }

        /// <summary>The <see cref="ATagBase" />'s parent <see cref="CLPPage" />.</summary>
        CLPPage ParentPage { get; set; }

        /// <summary>Determines if the <see cref="ATagBase" /> can have more than one value.</summary>
        bool IsSingleValueTag { get; }

        /// <summary>Category the Tag belongs to for sorting purposes.</summary>
        Category Category { get; }

        /// <summary>Produces a human-readable string that describes the name of the tag.</summary>
        string FormattedName { get; }

        /// <summary>Produces a human-readable string that describes the value of the tag.</summary>
        string FormattedValue { get; }
    }
}