using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class TempArraySkipCountingTag : ATagBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="TempArraySkipCountingTag" /> from scratch.
        /// </summary>
        public TempArraySkipCountingTag()
        { }

        /// <summary>
        /// Initializes <see cref="TempArraySkipCountingTag" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="TempArraySkipCountingTag" /> belongs to.</param>
        public TempArraySkipCountingTag(CLPPage parentPage, Origin origin)
            : base(parentPage, origin)
        { }

        /// <summary>
        /// Initializes <see cref="TempArraySkipCountingTag" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public TempArraySkipCountingTag(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// SUMMARY
        /// </summary>
        public string ArrayName
        {
            get { return GetValue<string>(ArrayNameProperty); }
            set { SetValue(ArrayNameProperty, value); }
        }

        public static readonly PropertyData ArrayNameProperty = RegisterProperty("ArrayName", typeof(string), string.Empty);

        /// <summary>SUMMARY</summary>
        public string EquationInterpretation
        {
            get { return GetValue<string>(EquationInterpretationProperty); }
            set { SetValue(EquationInterpretationProperty, value); }
        }

        public static readonly PropertyData EquationInterpretationProperty = RegisterProperty("EquationInterpretation", typeof (string), string.Empty);      

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.Array; }
        }

        public override string FormattedName
        {
            get { return "Array Skip Counting"; }
        }

        public override string FormattedValue
        {
            get { return string.Format("ARR skip [{0}: \"{1}\"]", ArrayName, EquationInterpretation); }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}