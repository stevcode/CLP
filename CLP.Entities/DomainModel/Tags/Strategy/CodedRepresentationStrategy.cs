using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities.Demo
{
    [Serializable]
    public class CodedRepresentationStrategy : AEntityBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CodedRepresentationStrategy" /> from scratch.</summary>
        public CodedRepresentationStrategy() { }

        /// <summary>Initializes <see cref="CodedRepresentationStrategy" /> from values.</summary>
        public CodedRepresentationStrategy(string codedStrategy, string codedRepresentation, string codedID)
        {
            CodedStrategy = codedStrategy;
            CodedRepresentation = codedRepresentation;
            CodedID = codedID;
        }

        /// <summary>Initializes <see cref="CodedRepresentationStrategy" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CodedRepresentationStrategy(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>Name of the strategy used.</summary>
        public string CodedStrategy
        {
            get { return GetValue<string>(CodedStrategyProperty); }
            set { SetValue(CodedStrategyProperty, value); }
        }

        public static readonly PropertyData CodedStrategyProperty = RegisterProperty("CodedStrategy", typeof(string), string.Empty);

        /// <summary>Coded value of the representation the strategy applies to.</summary>
        public string CodedRepresentation
        {
            get { return GetValue<string>(CodedRepresentationProperty); }
            set { SetValue(CodedRepresentationProperty, value); }
        }

        public static readonly PropertyData CodedRepresentationProperty = RegisterProperty("CodedRepresentation", typeof(string), string.Empty);

        /// <summary>Coded ID for the representation.</summary>
        public string CodedID
        {
            get { return GetValue<string>(CodedIDProperty); }
            set { SetValue(CodedIDProperty, value); }
        }

        public static readonly PropertyData CodedIDProperty = RegisterProperty("CodedID", typeof(string), string.Empty);

        /// <summary>Coded incrementID for the representation, if necessary.</summary>
        public string CodedIncrementID
        {
            get { return GetValue<string>(CodedIncrementIDProperty); }
            set { SetValue(CodedIncrementIDProperty, value); }
        }

        public static readonly PropertyData CodedIncrementIDProperty = RegisterProperty("CodedIncrementID", typeof(string), string.Empty);

        /// <summary>Coded resultantID for when a strategy utilized a change in Coded ID.</summary>
        public string CodedResultantID
        {
            get { return GetValue<string>(CodedResultantIDProperty); }
            set { SetValue(CodedResultantIDProperty, value); }
        }

        public static readonly PropertyData CodedResultantIDProperty = RegisterProperty("CodedResultantID", typeof(string), string.Empty);

        /// <summary>Any relevant specifics for the strategy used.</summary>
        public string StrategySpecifics
        {
            get { return GetValue<string>(StrategySpecificsProperty); }
            set { SetValue(StrategySpecificsProperty, value); }
        }

        public static readonly PropertyData StrategySpecificsProperty = RegisterProperty("StrategySpecifics", typeof(string), string.Empty);

        #region Backing Properties

        /// <summary>Cached value of CodedValue.</summary>
        public string CachedCodedValue
        {
            get { return GetValue<string>(CachedCodedValueProperty); }
            set { SetValue(CachedCodedValueProperty, value); }
        }

        public static readonly PropertyData CachedCodedValueProperty = RegisterProperty("CachedCodedValue", typeof(string), string.Empty);

        #endregion // Backing Properties

        #region Calculated Properties

        /// <summary>
        ///     Takes the following form: STRATEGY: REPRESENTATION [ID increment_id: resultant_id] specifics
        /// </summary>
        public string CodedValue
        {
            get
            {
                var incrementID = !string.IsNullOrWhiteSpace(CodedIncrementID) ? " " + CodedIncrementID : string.Empty;
                var resultantID = !string.IsNullOrWhiteSpace(CodedResultantID) ? ": " + CodedResultantID : string.Empty;
                var strategySpecifics = !string.IsNullOrWhiteSpace(StrategySpecifics) ? " " + StrategySpecifics : string.Empty;
                return string.Format("{0}: {1} [{2}{3}{4}]{5}", CodedStrategy, CodedRepresentation, CodedID, incrementID, resultantID, strategySpecifics);
            }
        }

        #endregion //Calculated Properties

        #endregion // Properties
    }
}