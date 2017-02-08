﻿using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AnswerRepresentationSequence : AAnalysisTagBase
    {
        #region Constructors

        public AnswerRepresentationSequence() { }

        public AnswerRepresentationSequence(CLPPage parentPage, Origin origin)
            : base(parentPage, origin) { }

        #endregion //Constructors

        /// <summary>Sequence of Final Answers and Representations.</summary>
        public List<string> Sequence
        {
            get { return GetValue<List<string>>(SequenceProperty); }
            set { SetValue(SequenceProperty, value); }
        }

        public static readonly PropertyData SequenceProperty = RegisterProperty("Sequence", typeof(List<string>), () => new List<string>());

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.Representation;

        public override string FormattedName => "Answer-Representation Sequence";

        public override string FormattedValue
        {
            get
            {
                var sequence = string.Join(", ", Sequence);
                var analysisCodes = string.Join(", ", AnalysisCodes);
                var codedSection = AnalysisCodes.Any() ? $"\nCodes: {analysisCodes}" : string.Empty;

                return $"{sequence}{codedSection}";
            }
        }

        #endregion //ATagBase Overrides

        #region Static Methods

        public static void AttemptTagGeneration(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            const string REPRESENTATION_SEQUENCE_IDENTIFIER = "R";
            const string ANSWER_SEQUENCE_IDENTIFIER = "A";

            var sequence = new List<string>();
            ISemanticEvent mostRecentSequenceItem = null;

            foreach (var semanticEvent in semanticEvents)
            {
                if (Codings.IsFinalAnswerEvent(semanticEvent))
                {
                    if (mostRecentSequenceItem == null ||
                        Codings.IsFinalAnswerEvent(mostRecentSequenceItem))
                    {
                        mostRecentSequenceItem = semanticEvent;
                    }
                    else if (Codings.IsRepresentationEvent(mostRecentSequenceItem))
                    {
                        sequence.Add(REPRESENTATION_SEQUENCE_IDENTIFIER);

                        mostRecentSequenceItem = semanticEvent;
                    }
                }
                else if (Codings.IsRepresentationEvent(semanticEvent) && 
                         semanticEvent.EventType == Codings.EVENT_OBJECT_ADD)
                {
                    if (mostRecentSequenceItem == null ||
                        Codings.IsRepresentationEvent(mostRecentSequenceItem))
                    {
                        mostRecentSequenceItem = semanticEvent;
                    }
                    else if (Codings.IsFinalAnswerEvent(mostRecentSequenceItem))
                    {
                        var correctness = Codings.GetFinalAnswerEventCorrectness(mostRecentSequenceItem);
                        var sequenceIdentifier = $"{ANSWER_SEQUENCE_IDENTIFIER}-{correctness}";
                        sequence.Add(sequenceIdentifier);

                        mostRecentSequenceItem = semanticEvent;
                    }
                }
            }

            var tag = new AnswerRepresentationSequence(page, Origin.StudentPageGenerated)
                      {
                          Sequence = sequence
                      };
            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}
