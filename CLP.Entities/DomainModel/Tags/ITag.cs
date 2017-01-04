using System;

namespace CLP.Entities.Demo
{
    public interface ITag {
        /// <summary>
        /// Unique Identifier for the <see cref="ATagBase" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// </remarks>
        string ID { get; set; }

        /// <summary>
        /// Unique Identifier for the <see cref="Person" /> who owns the <see cref="ATagBase" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// </remarks>
        string OwnerID { get; set; }

        /// <summary>
        /// Version Index of the <see cref="ATagBase" />.
        /// </summary>
        /// <remarks>
        /// Composite Primary Key.
        /// </remarks>
        uint VersionIndex { get; set; }

        /// <summary>
        /// Version Index of the latest submission.
        /// </summary>
        uint? LastVersionIndex { get; set; }

        /// <summary>
        /// Date and Time the <see cref="ATagBase" /> was created.
        /// </summary>
        DateTime CreationDate { get; set; }

        /// <summary>
        /// <see cref="Origin" /> from which the <see cref="ATagBase" /> originates.
        /// </summary>
        Origin Origin { get; set; }

        /// <summary>
        /// <see cref="Category" /> the <see cref="ITag" /> falls into for sorting purposes.
        /// </summary>
        Category Category { get; }

        /// <summary>
        /// Determines if the <see cref="ATagBase" /> can have more than one value.
        /// </summary>
        bool IsSingleValueTag { get; set; }

        /// <summary>
        /// Designates the <see cref="ITag" /> as invisible in the PageInfoPanel.
        /// </summary>
        bool IsHiddenTag { get; set; }

        /// <summary>Produces a human-readable string that describes the name of the tag.</summary>
        string FormattedName { get; }

        /// <summary>
        /// Produces a human-readable string that describes the value of the tag.
        /// </summary>
        string FormattedValue { get; }

        /// <summary>
        /// Unique Identifier for the <see cref="IPageObject" />'s parent <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Composite Foreign Key.
        /// </remarks>
        string ParentPageID { get; set; }

        /// <summary>
        /// Unique Identifier of the <see cref="Person" /> who owns the parent <see cref="CLPPage" /> of the <see cref="ATagBase" />.
        /// </summary>
        /// <remarks>
        /// Composite Foreign Key.
        /// </remarks>
        string ParentPageOwnerID { get; set; }

        /// <summary>
        /// The parent <see cref="CLPPage" />'s Version Index.
        /// </summary>
        uint ParentPageVersionIndex { get; set; }

        /// <summary>
        /// The <see cref="ATagBase" />'s parent <see cref="CLPPage" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        CLPPage ParentPage { get; set; }
    }
}