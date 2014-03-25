﻿using System.Runtime.Serialization;

namespace CLP.Entities
{
    public class PageTopicTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="PageTopicTag" /> from scratch.
        /// </summary>
        public PageTopicTag() { }

        /// <summary>
        /// Initializes <see cref="PageTopicTag" /> from a value.
        /// </summary>
        /// <param name="value">The value of the <see cref="PageTopicTag" />.</param>
        public PageTopicTag(string value) { Value = value; }

        /// <summary>
        /// Initializes <see cref="PageTopicTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public PageTopicTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors
    }
}