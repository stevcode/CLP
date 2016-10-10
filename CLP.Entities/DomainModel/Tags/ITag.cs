using System;
using System.Xml.Serialization;
using Catel.Runtime.Serialization;

namespace CLP.Entities.Old
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
        /// Determines if the <see cref="ATagBase" /> can have more than one value.
        /// </summary>
        bool IsSingleValueTag { get; set; }

        /// <summary>
        /// Value of the <see cref="ATagBase" />.
        /// </summary>
        string Value { get; set; }

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
        [XmlIgnore]
        [ExcludeFromSerialization]
        CLPPage ParentPage { get; set; }
    }
}