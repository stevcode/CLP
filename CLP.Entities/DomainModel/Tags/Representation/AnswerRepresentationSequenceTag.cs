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
            get => GetValue<List<string>>(SequenceProperty);
            set => SetValue(SequenceProperty, value);
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
                var analysisCodes = string.Join("\n", QueryCodes.Select(c => c.FormattedValue));
                var codedSection = QueryCodes.Any() ? $"\nCodes:\n{analysisCodes}" : string.Empty;

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
                var previousSequenceItems = sequence.Take(firstRepresentationIndex).ToList();
                if (previousSequenceItems.Any(i => i.Contains(Codings.CORRECTNESS_CORRECT) && i.Contains(FINAL_ANSWER_SEQUENCE_IDENTIFIER)))
                {
                    AnalysisCode.AddFinalAnswerBeforeRepresentation(tag, Correctness.Correct);
                }

                if (previousSequenceItems.Any(i => i.Contains(Codings.CORRECTNESS_INCORRECT) && i.Contains(FINAL_ANSWER_SEQUENCE_IDENTIFIER)))
                {
                    AnalysisCode.AddFinalAnswerBeforeRepresentation(tag, Correctness.Incorrect);
                }

                if (previousSequenceItems.Any(i => i.Contains(Codings.CORRECTNESS_CORRECT) && i.Contains(INTERMEDIARY_ANSWER_SEQUENCE_IDENTIFIER)))
                {
                    AnalysisCode.AddIntermediaryAnswerBeforeRepresentation(tag, Correctness.Correct);
                }

                if (previousSequenceItems.Any(i => i.Contains(Codings.CORRECTNESS_INCORRECT) && i.Contains(INTERMEDIARY_ANSWER_SEQUENCE_IDENTIFIER)))
                {
                    AnalysisCode.AddIntermediaryAnswerBeforeRepresentation(tag, Correctness.Incorrect);
                }
            }

            // RAA
            var firstCorrectFinalAnswerIndex = sequence.IndexOf("FA-COR");
            var firstIncorrectFinalAnswerIndex = sequence.IndexOf("FA-INC");
            var firstIllegibleFinalAnswerIndex = sequence.IndexOf("FA-ILL");
            var finalAnswerIndexes = new List<int>();
            if (firstCorrectFinalAnswerIndex != -1)
            {
                finalAnswerIndexes.Add(firstCorrectFinalAnswerIndex);
            }
            if (firstIncorrectFinalAnswerIndex != -1)
            {
                finalAnswerIndexes.Add(firstIncorrectFinalAnswerIndex);
            }
            if (firstIllegibleFinalAnswerIndex != -1)
            {
                finalAnswerIndexes.Add(firstIllegibleFinalAnswerIndex);
            }
            var firstFinalAnswerIndex = !finalAnswerIndexes.Any() ? -1 : finalAnswerIndexes.Min();
            var sequenceAfterFirstFinalAnswer = sequence.Skip(firstFinalAnswerIndex + 1).ToList();
            if (firstFinalAnswerIndex != -1 &&
                sequenceAfterFirstFinalAnswer.Any(i => i == REPRESENTATION_SEQUENCE_IDENTIFIER))
            {
                AnalysisCode.AddRepresentationAfterFinalAnswer(tag, Correctness.Unknown);
            }

            var firstCorrectIntermediaryAnswerIndex = sequence.IndexOf("IA-COR");
            var firstIncorrectIntermediaryAnswerIndex = sequence.IndexOf("IA-INC");
            var firstIllegibleIntermediaryAnswerIndex = sequence.IndexOf("IA-ILL");
            var intermediaryAnswerIndexes = new List<int>();
            if (firstCorrectIntermediaryAnswerIndex != -1)
            {
                intermediaryAnswerIndexes.Add(firstCorrectIntermediaryAnswerIndex);
            }
            if (firstIncorrectIntermediaryAnswerIndex != -1)
            {
                intermediaryAnswerIndexes.Add(firstIncorrectIntermediaryAnswerIndex);
            }
            if (firstIllegibleIntermediaryAnswerIndex != -1)
            {
                intermediaryAnswerIndexes.Add(firstIllegibleIntermediaryAnswerIndex);
            }
            var firstIntermediaryAnswerIndex = !intermediaryAnswerIndexes.Any() ? -1 : intermediaryAnswerIndexes.Min();
            var sequenceAfterFirstIntermediaryAnswer = sequence.Skip(firstIntermediaryAnswerIndex + 1).ToList();
            if (firstIntermediaryAnswerIndex != -1 &&
                sequenceAfterFirstIntermediaryAnswer.Any(i => i == REPRESENTATION_SEQUENCE_IDENTIFIER))
            {
                AnalysisCode.AddRepresentationAfterIntermediaryAnswer(tag, Correctness.Unknown);
            }

            // ARA
            var startItem = string.Empty;
            var isRepresentationUsedAfterAnswer = false;
            foreach (var item in sequence)
            {
                var isItemAnswer = item == "FA-COR" || item == "FA-INC" || item == "FA-ILL";
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

                var isStartCOR = startItem == "FA-COR";
                var isCurrentCOR = item == "FA-COR";

                if (isStartCOR && isCurrentCOR)
                {
                    AnalysisCode.AddChangedAnswerAfterRepresentation(tag, Correctness.Correct, Correctness.Correct);
                    startItem = item;
                    isRepresentationUsedAfterAnswer = false;
                }
                else if (!isStartCOR &&
                         !isCurrentCOR)
                {
                    AnalysisCode.AddChangedAnswerAfterRepresentation(tag, Correctness.Incorrect, Correctness.Incorrect);
                    startItem = item;
                    isRepresentationUsedAfterAnswer = false;
                }
                else if (isStartCOR && !isCurrentCOR)
                {
                    AnalysisCode.AddChangedAnswerAfterRepresentation(tag, Correctness.Correct, Correctness.Incorrect);
                    startItem = item;
                    isRepresentationUsedAfterAnswer = false;
                }
                else if (!isStartCOR && isCurrentCOR)
                {
                    AnalysisCode.AddChangedAnswerAfterRepresentation(tag, Correctness.Incorrect, Correctness.Correct);
                    startItem = item;
                    isRepresentationUsedAfterAnswer = false;
                }
            }

            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}