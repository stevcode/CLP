using System;
using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLineStrategyTag : ARepresentationStrategyBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="NumberLineStrategyTag" /> from scratch.</summary>
        public NumberLineStrategyTag() { }

        /// <summary>Initializes <see cref="NumberLineStrategyTag" /> from values.</summary>
        public NumberLineStrategyTag(CLPPage parentPage, Origin origin, List<ISemanticEvent> semanticEvents, List<CodedRepresentationStrategy> codedStrategies)
            : base(parentPage, origin, semanticEvents, codedStrategies) { }

        #endregion //Constructors

        #region ATagBase Overrides

        public override string FormattedName => "Number Line";

        #endregion //ATagBase Overrides

        #region Static Methods

        public static void IdentifyNumberLineStrategies(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var relevantSemanticEvents = new List<ISemanticEvent>();
            var codedStrategies = new List<CodedRepresentationStrategy>();
            var ignoredHistoryIndexes = new List<int>();
            var jumpArithCount = new Dictionary<string, int>();
            var patternStartPoints = new Dictionary<string, string>();
            var patternEndPoints = new List<dynamic>();

            for (var i = 0; i < semanticEvents.Count; i++)
            {
                if (ignoredHistoryIndexes.Contains(i))
                {
                    continue;
                }

                var currentSemanticEvent = semanticEvents[i];
                var isLastSemanticEvent = i + 1 >= semanticEvents.Count;

                if (currentSemanticEvent.CodedObject == Codings.OBJECT_ARITH &&
                    currentSemanticEvent.EventType == Codings.EVENT_ARITH_ADD)
                {
                    var numberLineIDs = patternStartPoints.Keys.ToList();
                    foreach (var numberLineID in numberLineIDs)
                    {
                        patternStartPoints[numberLineID] = Codings.EVENT_ARITH_ADD;
                    }
                }

                if (currentSemanticEvent.CodedObject == Codings.OBJECT_NUMBER_LINE &&
                    currentSemanticEvent.EventType == Codings.EVENT_NUMBER_LINE_JUMP)
                {
                    var numberLineID = currentSemanticEvent.ReferencePageObjectID;
                    var compoundID = $"{currentSemanticEvent.CodedObjectID};{currentSemanticEvent.CodedObjectIDIncrement};{numberLineID}";

                    if (!jumpArithCount.ContainsKey(compoundID))
                    {
                        jumpArithCount.Add(compoundID, 0);
                    }

                    if (patternStartPoints.Keys.Contains(numberLineID))
                    {
                        if (patternStartPoints[numberLineID] == Codings.EVENT_ARITH_ADD)
                        {
                            jumpArithCount[compoundID]++;
                        }

                        patternStartPoints[numberLineID] = Codings.EVENT_NUMBER_LINE_JUMP;
                    }
                    else
                    {
                        patternStartPoints.Add(numberLineID, Codings.EVENT_NUMBER_LINE_JUMP);
                    }
                }
            }

            foreach (var key in jumpArithCount.Keys)
            {
                var keySegments = key.Split(';');
                var codedID = keySegments[0];
                var incrementID = keySegments[1];
                var count = jumpArithCount[key];

                if (count <= 0)
                {
                    continue;
                }

                var codedStrategy = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_NUMBER_LINE_JUMP, Codings.OBJECT_NUMBER_LINE, codedID)
                                    {
                                        CodedIncrementID = incrementID,
                                        StrategySpecifics = $"{Codings.STRATEGY_SPECIFICS_NUMBER_LINE_ARITH} ({count})"
                                    };

                codedStrategies.Add(codedStrategy);
            }

            if (!codedStrategies.Any())
            {
                return;
            }

            var tag = new NumberLineStrategyTag(page, Origin.StudentPageGenerated, relevantSemanticEvents, codedStrategies);
            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}