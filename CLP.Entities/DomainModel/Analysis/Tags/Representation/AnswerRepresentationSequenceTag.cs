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

        #region Properties

        /// <summary>Sequence of Answers and Representations where sequential useage of the same type are collapsed into a single entry.</summary>
        public List<string> CondensedSequence
        {
            get => GetValue<List<string>>(SequenceProperty);
            set => SetValue(SequenceProperty, value);
        }

        public static readonly PropertyData SequenceProperty = RegisterProperty(nameof(CondensedSequence), typeof(List<string>), () => new List<string>());

        #endregion // Properties

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public override Category Category => Category.Representation;

        public override string FormattedName => "Answer-Representation Sequence";

        public override string FormattedValue
        {
            get
            {
                var sequence = string.Join(", ", CondensedSequence);
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

            var lastFinalAnswerEvent = semanticEvents.LastOrDefault(Codings.IsFinalAnswerEvent);
            foreach (var semanticEvent in semanticEvents)
            {
                if (Codings.IsFinalAnswerEvent(semanticEvent) && semanticEvent != lastFinalAnswerEvent)
                {
                    var interpretation = Codings.GetFinalAnswerEventStudentAnswer(semanticEvent);
                    if (interpretation == "\"\"")
                    {
                        continue;
                    }
                }

                if (semanticEvent?.CodedObject == Codings.OBJECT_MULTIPLE_CHOICE &&
                    Codings.IsMultipleChoiceEventAnErase(semanticEvent))
                {
                    continue;
                }

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
                        var studentAnswer = Codings.GetFinalAnswerEventStudentAnswer(mostRecentSequenceItem);
                        var sequenceIdentifier = $"{INTERMEDIARY_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}-{studentAnswer}";
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
                        var studentAnswer = Codings.GetFinalAnswerEventStudentAnswer(mostRecentSequenceItem);
                        var sequenceIdentifier = $"{FINAL_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}-{studentAnswer}";
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
                    // BUG: Only deals with ADD, but could realistically need to track an R when snapping/cutting occurs
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
                        var studentAnswer = Codings.GetFinalAnswerEventStudentAnswer(mostRecentSequenceItem);
                        var sequenceIdentifier = $"{FINAL_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}-{studentAnswer}";
                        sequence.Add(sequenceIdentifier);

                        mostRecentSequenceItem = semanticEvent;
                    }
                    else if (Codings.IsFinalAnswerEvent(mostRecentSequenceItem) &&
                             mostRecentSequenceItem.CodedObject == Codings.OBJECT_INTERMEDIARY_FILL_IN)
                    {
                        var correctness = Codings.GetFinalAnswerEventCorrectness(mostRecentSequenceItem);
                        var studentAnswer = Codings.GetFinalAnswerEventStudentAnswer(mostRecentSequenceItem);
                        var sequenceIdentifier = $"{INTERMEDIARY_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}-{studentAnswer}";
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
                var studentAnswer = Codings.GetFinalAnswerEventStudentAnswer(mostRecentSequenceItem);
                var sequenceIdentifier = $"{FINAL_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}-{studentAnswer}";
                sequence.Add(sequenceIdentifier);
            }
            else if (Codings.IsFinalAnswerEvent(mostRecentSequenceItem) &&
                     mostRecentSequenceItem.CodedObject == Codings.OBJECT_INTERMEDIARY_FILL_IN)
            {
                var correctness = Codings.GetFinalAnswerEventCorrectness(mostRecentSequenceItem);
                var studentAnswer = Codings.GetFinalAnswerEventStudentAnswer(mostRecentSequenceItem);
                var sequenceIdentifier = $"{INTERMEDIARY_ANSWER_SEQUENCE_IDENTIFIER}-{correctness}-{studentAnswer}";
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
                          CondensedSequence = sequence
                      };

            // REP_ORDER
            var firstRepresentationEvent = semanticEvents.FirstOrDefault(e => Codings.IsRepresentationEvent(e) && e.EventType == Codings.EVENT_OBJECT_ADD);
            var lastRepresentationEvent = semanticEvents.LastOrDefault(e => Codings.IsRepresentationEvent(e) && e.EventType == Codings.EVENT_OBJECT_ADD);
            if (firstRepresentationEvent != null &&
                lastRepresentationEvent != null &&
                firstRepresentationEvent.ID != lastRepresentationEvent.ID)
            {
                AnalysisCode.AddRepresentationOrder(tag, firstRepresentationEvent.CodedObject, lastRepresentationEvent.CodedObject);
            }

            // ABR
            var firstRepresentationIndex = sequence.IndexOf(REPRESENTATION_SEQUENCE_IDENTIFIER);
            if (firstRepresentationIndex > 0)
            {
                var previousSequenceItems = sequence.Take(firstRepresentationIndex).ToList();
                foreach (var sequenceItem in previousSequenceItems)
                {
                    var objectType = sequenceItem.Substring(0, 2);
                    var codedCorrectness = sequenceItem.Substring(3, 3);
                    var studentAnswer = sequenceItem.Substring(7);

                    var correctness = Codings.CodedCorrectnessToCorrectness(codedCorrectness);
                    if (objectType == FINAL_ANSWER_SEQUENCE_IDENTIFIER)
                    {
                        AnalysisCode.AddFinalAnswerBeforeRepresentation(tag, correctness, studentAnswer);
                    }
                    else
                    {
                        AnalysisCode.AddIntermediaryAnswerBeforeRepresentation(tag, correctness, studentAnswer);
                    }
                }
            }

            // RAA
            //var firstCorrectFinalAnswerIndex = sequence.IndexOf("FA-COR");
            //var firstIncorrectFinalAnswerIndex = sequence.IndexOf("FA-INC");
            //var firstIllegibleFinalAnswerIndex = sequence.IndexOf("FA-ILL");
            //var finalAnswerIndexes = new List<int>();
            //if (firstCorrectFinalAnswerIndex != -1)
            //{
            //    finalAnswerIndexes.Add(firstCorrectFinalAnswerIndex);
            //}
            //if (firstIncorrectFinalAnswerIndex != -1)
            //{
            //    finalAnswerIndexes.Add(firstIncorrectFinalAnswerIndex);
            //}
            //if (firstIllegibleFinalAnswerIndex != -1)
            //{
            //    finalAnswerIndexes.Add(firstIllegibleFinalAnswerIndex);
            //}
            //var firstFinalAnswerIndex = !finalAnswerIndexes.Any() ? -1 : finalAnswerIndexes.Min();
            //var sequenceAfterFirstFinalAnswer = sequence.Skip(firstFinalAnswerIndex + 1).ToList();
            //if (firstFinalAnswerIndex != -1 &&
            //    sequenceAfterFirstFinalAnswer.Any(i => i == REPRESENTATION_SEQUENCE_IDENTIFIER))
            //{
            //    AnalysisCode.AddRepresentationAfterFinalAnswer(tag, Correctness.Unknown);
            //}

            //var firstCorrectIntermediaryAnswerIndex = sequence.IndexOf("IA-COR");
            //var firstIncorrectIntermediaryAnswerIndex = sequence.IndexOf("IA-INC");
            //var firstIllegibleIntermediaryAnswerIndex = sequence.IndexOf("IA-ILL");
            //var intermediaryAnswerIndexes = new List<int>();
            //if (firstCorrectIntermediaryAnswerIndex != -1)
            //{
            //    intermediaryAnswerIndexes.Add(firstCorrectIntermediaryAnswerIndex);
            //}
            //if (firstIncorrectIntermediaryAnswerIndex != -1)
            //{
            //    intermediaryAnswerIndexes.Add(firstIncorrectIntermediaryAnswerIndex);
            //}
            //if (firstIllegibleIntermediaryAnswerIndex != -1)
            //{
            //    intermediaryAnswerIndexes.Add(firstIllegibleIntermediaryAnswerIndex);
            //}
            //var firstIntermediaryAnswerIndex = !intermediaryAnswerIndexes.Any() ? -1 : intermediaryAnswerIndexes.Min();
            //var sequenceAfterFirstIntermediaryAnswer = sequence.Skip(firstIntermediaryAnswerIndex + 1).ToList();
            //if (firstIntermediaryAnswerIndex != -1 &&
            //    sequenceAfterFirstIntermediaryAnswer.Any(i => i == REPRESENTATION_SEQUENCE_IDENTIFIER))
            //{
            //    AnalysisCode.AddRepresentationAfterIntermediaryAnswer(tag, Correctness.Unknown);
            //}

            // CAAR
            var startItem = string.Empty;
            var isRepresentationUsedAfterAnswer = false;
            foreach (var item in sequence)
            {
                var adjustedItem = item == "R" ? item : item.Substring(0, 6);

                var isItemAnswer = adjustedItem == "FA-COR" || adjustedItem == "FA-INC" || adjustedItem == "FA-ILL";
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
                    startItem = adjustedItem;
                    continue;
                }

                if (!isRepresentationUsedAfterAnswer)
                {
                    continue;
                }

                var startCorrectness = startItem == "FA-COR" ? Correctness.Correct : startItem == "FA-INC" ? Correctness.Incorrect : Correctness.Illegible;
                var endCorrectness = adjustedItem == "FA-COR" ? Correctness.Correct : adjustedItem == "FA-INC" ? Correctness.Incorrect : Correctness.Illegible;
                AnalysisCode.AddChangedAnswerAfterRepresentation(tag, startCorrectness, endCorrectness);
                startItem = adjustedItem;
                isRepresentationUsedAfterAnswer = false;

                //var isStartCOR = startItem == "FA-COR";
                //var isCurrentCOR = item == "FA-COR";

                //if (isStartCOR && isCurrentCOR)
                //{
                //    AnalysisCode.AddChangedAnswerAfterRepresentation(tag, Correctness.Correct, Correctness.Correct);
                //    startItem = item;
                //    isRepresentationUsedAfterAnswer = false;
                //}
                //else if (!isStartCOR &&
                //         !isCurrentCOR)
                //{
                //    AnalysisCode.AddChangedAnswerAfterRepresentation(tag, Correctness.Incorrect, Correctness.Incorrect);
                //    startItem = item;
                //    isRepresentationUsedAfterAnswer = false;
                //}
                //else if (isStartCOR && !isCurrentCOR)
                //{
                //    AnalysisCode.AddChangedAnswerAfterRepresentation(tag, Correctness.Correct, Correctness.Incorrect);
                //    startItem = item;
                //    isRepresentationUsedAfterAnswer = false;
                //}
                //else if (!isStartCOR && isCurrentCOR)
                //{
                //    AnalysisCode.AddChangedAnswerAfterRepresentation(tag, Correctness.Incorrect, Correctness.Correct);
                //    startItem = item;
                //    isRepresentationUsedAfterAnswer = false;
                //}
            }

            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}