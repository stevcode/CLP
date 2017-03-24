using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class AnswerRepresentationSequenceTag : AAnalysisTagBase
    {
        #region Constructors

        public AnswerRepresentationSequenceTag() { }

        public AnswerRepresentationSequenceTag(CLPPage parentPage, Origin origin)
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
            const string FINAL_ANSWER_SEQUENCE_IDENTIFIER = "FA";
            const string INTERMEDIARY_ANSWER_SEQUENCE_IDENTIFIER = "IA";

            var sequence = new List<string>();
            ISemanticEvent mostRecentSequenceItem = null;

            foreach (var semanticEvent in semanticEvents)
            {
                if (Codings.IsFinalAnswerEvent(semanticEvent) &&
                    semanticEvent.CodedObject != Codings.OBJECT_INTERMEDIARY_FILL_IN)
                {
                    if (mostRecentSequenceItem == null)
                    {
                        mostRecentSequenceItem = semanticEvent;
                    }
                    else if (Codings.IsFinalAnswerEvent(mostRecentSequenceItem) &&
                             mostRecentSequenceItem.CodedObject != Codings.OBJECT_INTERMEDIARY_FILL_IN)
                    {
                        mostRecentSequenceItem = semanticEvent;
                    }
                    else if (Codings.IsFinalAnswerEvent(mostRecentSequenceItem) &&
                             mostRecentSequenceItem.CodedObject == Codings.OBJECT_INTERMEDIARY_FILL_IN)
                    {
                        var correctness = Codings.GetFinalAnswerEventCorrectness(mostRecentSequenceItem);
                        var sequenceIdentifier = $"{INTERMEDIARY_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}";
                        sequence.Add(sequenceIdentifier);

                        mostRecentSequenceItem = semanticEvent;
                    }
                    else if (Codings.IsRepresentationEvent(mostRecentSequenceItem))
                    {
                        sequence.Add(REPRESENTATION_SEQUENCE_IDENTIFIER);
                        mostRecentSequenceItem = semanticEvent;
                    }
                }
                else if (Codings.IsFinalAnswerEvent(semanticEvent) &&
                         semanticEvent.CodedObject == Codings.OBJECT_INTERMEDIARY_FILL_IN)
                {
                    if (mostRecentSequenceItem == null)
                    {
                        mostRecentSequenceItem = semanticEvent;
                    }
                    else if (Codings.IsFinalAnswerEvent(mostRecentSequenceItem) &&
                             mostRecentSequenceItem.CodedObject == Codings.OBJECT_INTERMEDIARY_FILL_IN)
                    {
                        mostRecentSequenceItem = semanticEvent;
                    }
                    else if (Codings.IsFinalAnswerEvent(mostRecentSequenceItem) &&
                             mostRecentSequenceItem.CodedObject != Codings.OBJECT_INTERMEDIARY_FILL_IN)
                    {
                        var correctness = Codings.GetFinalAnswerEventCorrectness(mostRecentSequenceItem);
                        var sequenceIdentifier = $"{FINAL_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}";
                        sequence.Add(sequenceIdentifier);

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
                    else if (Codings.IsFinalAnswerEvent(mostRecentSequenceItem) &&
                             mostRecentSequenceItem.CodedObject != Codings.OBJECT_INTERMEDIARY_FILL_IN)
                    {
                        var correctness = Codings.GetFinalAnswerEventCorrectness(mostRecentSequenceItem);
                        var sequenceIdentifier = $"{FINAL_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}";
                        sequence.Add(sequenceIdentifier);

                        mostRecentSequenceItem = semanticEvent;
                    }
                    else if (Codings.IsFinalAnswerEvent(mostRecentSequenceItem) &&
                             mostRecentSequenceItem.CodedObject == Codings.OBJECT_INTERMEDIARY_FILL_IN)
                    {
                        var correctness = Codings.GetFinalAnswerEventCorrectness(mostRecentSequenceItem);
                        var sequenceIdentifier = $"{INTERMEDIARY_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}";
                        sequence.Add(sequenceIdentifier);

                        mostRecentSequenceItem = semanticEvent;
                    }
                }
            }

            if (mostRecentSequenceItem == null)
            {
                return;
            }

            if (Codings.IsFinalAnswerEvent(mostRecentSequenceItem) &&
                mostRecentSequenceItem.CodedObject != Codings.OBJECT_INTERMEDIARY_FILL_IN)
            {
                var correctness = Codings.GetFinalAnswerEventCorrectness(mostRecentSequenceItem);
                var sequenceIdentifier = $"{FINAL_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}";
                sequence.Add(sequenceIdentifier);
            }
            else if (Codings.IsFinalAnswerEvent(mostRecentSequenceItem) &&
                     mostRecentSequenceItem.CodedObject == Codings.OBJECT_INTERMEDIARY_FILL_IN)
            {
                var correctness = Codings.GetFinalAnswerEventCorrectness(mostRecentSequenceItem);
                var sequenceIdentifier = $"{INTERMEDIARY_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}";
                sequence.Add(sequenceIdentifier);
            }
            else if (Codings.IsRepresentationEvent(mostRecentSequenceItem))
            {
                sequence.Add(REPRESENTATION_SEQUENCE_IDENTIFIER);
            }

            if (!sequence.Any())
            {
                return;
            }

            var tag = new AnswerRepresentationSequenceTag(page, Origin.StudentPageGenerated)
                      {
                          Sequence = sequence
                      };

            // ABR
            var firstRepresentationIndex = sequence.IndexOf(REPRESENTATION_SEQUENCE_IDENTIFIER);
            if (firstRepresentationIndex > 0)
            {
                var previousSequenceItems = sequence.Take(firstRepresentationIndex);
                if (previousSequenceItems.Any(i => i.Contains(Codings.CORRECTNESS_CORRECT) && i.Contains(FINAL_ANSWER_SEQUENCE_IDENTIFIER)))
                {
                    tag.AnalysisCodes.Add(Codings.ANALYSIS_FINAL_ANS_COR_BEFORE_REP);
                }

                if (previousSequenceItems.Any(i => i.Contains(Codings.CORRECTNESS_INCORRECT) && i.Contains(FINAL_ANSWER_SEQUENCE_IDENTIFIER)))
                {
                    tag.AnalysisCodes.Add(Codings.ANALYSIS_FINAL_ANS_INC_BEFORE_REP);
                }

                if (previousSequenceItems.Any(i => i.Contains(Codings.CORRECTNESS_CORRECT) && i.Contains(INTERMEDIARY_ANSWER_SEQUENCE_IDENTIFIER)))
                {
                    tag.AnalysisCodes.Add(Codings.ANALYSIS_INTERMEDIARY_ANS_COR_BEFORE_REP);
                }

                if (previousSequenceItems.Any(i => i.Contains(Codings.CORRECTNESS_INCORRECT) && i.Contains(INTERMEDIARY_ANSWER_SEQUENCE_IDENTIFIER)))
                {
                    tag.AnalysisCodes.Add(Codings.ANALYSIS_INTERMEDIARY_ANS_INC_BEFORE_REP);
                }
            }

            // RAA
            var lastCorrectFinalAnswerIndex = sequence.LastIndexOf("FA-COR");
            var lastIncorrectFinalAnswerIndex = sequence.LastIndexOf("FA-INC");
            var lastFinalAnswerIndex = Math.Max(lastCorrectFinalAnswerIndex, lastIncorrectFinalAnswerIndex);
            if (lastFinalAnswerIndex >= 0 &&
                lastFinalAnswerIndex < sequence.Count - 1)
            {
                tag.AnalysisCodes.Add(Codings.ANALYSIS_REP_AFTER_FINAL_ANSWER);
            }

            var lastCorrectIntermediaryAnswerIndex = sequence.LastIndexOf("IA-COR");
            var lastIncorrectIntermediaryAnswerIndex = sequence.LastIndexOf("IA-INC");
            var lastIntermediaryAnswerIndex = Math.Max(lastCorrectIntermediaryAnswerIndex, lastIncorrectIntermediaryAnswerIndex);
            if (lastIntermediaryAnswerIndex >= 0 &&
                lastIntermediaryAnswerIndex < sequence.Count - 1)
            {
                tag.AnalysisCodes.Add(Codings.ANALYSIS_REP_AFTER_INTERMEDIARY_ANSWER);
            }

            // ARA
            var startItem = string.Empty;
            var isRepresentationUsedAfterAnswer = false;
            foreach (var item in sequence)
            {
                var isItemAnswer = item == "FA-COR" || item == "FA-INC";
                if (!isItemAnswer)
                {
                    if (!string.IsNullOrWhiteSpace(startItem))
                    {
                        isRepresentationUsedAfterAnswer = true;
                    }
                    continue;
                }
                
                if (string.IsNullOrWhiteSpace(startItem))
                {
                    startItem = item;
                    continue;
                }

                if (!isRepresentationUsedAfterAnswer)
                {
                    continue;
                }

                var isStartCOR = startItem == "A-COR";
                var isCurrentCOR = item == "A-COR";

                if (isStartCOR && isCurrentCOR)
                {
                    tag.AnalysisCodes.Add(Codings.ANALYSIS_COR_TO_COR_AFTER_REP);
                    startItem = item;
                    isRepresentationUsedAfterAnswer = false;
                }
                else if (!isStartCOR && !isCurrentCOR)
                {
                    tag.AnalysisCodes.Add(Codings.ANALYSIS_INC_TO_INC_AFTER_REP);
                    startItem = item;
                    isRepresentationUsedAfterAnswer = false;
                }
                else if (isStartCOR && !isCurrentCOR)
                {
                    tag.AnalysisCodes.Add(Codings.ANALYSIS_COR_TO_INC_AFTER_REP);
                    startItem = item;
                    isRepresentationUsedAfterAnswer = false;
                }
                else if (!isStartCOR && isCurrentCOR)
                {
                    tag.AnalysisCodes.Add(Codings.ANALYSIS_INC_TO_COR_AFTER_REP);
                    startItem = item;
                    isRepresentationUsedAfterAnswer = false;
                }
            }

            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}
