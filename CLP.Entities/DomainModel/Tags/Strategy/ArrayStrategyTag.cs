using System;
using System.Collections.Generic;
using System.Linq;

namespace CLP.Entities
{
    [Serializable]
    public class ArrayStrategyTag : ARepresentationStrategyBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="ArrayStrategyTag" /> from scratch.</summary>
        public ArrayStrategyTag() { }

        /// <summary>Initializes <see cref="ArrayStrategyTag" /> from values.</summary>
        public ArrayStrategyTag(CLPPage parentPage, Origin origin, List<ISemanticEvent> semanticEvents, List<CodedRepresentationStrategy> codedStrategies)
            : base(parentPage, origin, semanticEvents, codedStrategies) { }

        #endregion //Constructors

        #region ATagBase Overrides
        
        public override string FormattedName => "Array";

        #endregion //ATagBase Overrides

        #region Static Methods

        public static void IdentifyArrayStrategies(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var relevantSemanticEvents = new List<ISemanticEvent>();
            var codedStrategies = new List<CodedRepresentationStrategy>();
            var ignoredHistoryIndexes = new List<int>();
            var skipArithCount = new Dictionary<string, int>();
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
                    var arrayIDs = patternStartPoints.Keys.ToList();
                    foreach (var arrayID in arrayIDs)
                    {
                        patternStartPoints[arrayID] = Codings.EVENT_ARITH_ADD;
                    }
                }

                if (currentSemanticEvent.CodedObject == Codings.OBJECT_ARRAY)
                {
                    if (currentSemanticEvent.EventType == Codings.EVENT_ARRAY_DIVIDE)
                    {
                        relevantSemanticEvents.Add(currentSemanticEvent);
                        var codedStrategy = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT, Codings.OBJECT_ARRAY, currentSemanticEvent.CodedObjectID)
                                            {
                                                CodedIncrementID = currentSemanticEvent.CodedObjectIDIncrement,
                                                CodedResultantID = currentSemanticEvent.EventInformation,
                                                StrategySpecifics = Codings.STRATEGY_SPECIFICS_ARRAY_DIVIDE
                                            };
                        codedStrategies.Add(codedStrategy);
                        continue;
                    }

                    if (currentSemanticEvent.EventType == Codings.EVENT_ARRAY_DIVIDE_INK)
                    {
                        relevantSemanticEvents.Add(currentSemanticEvent);
                        var codedStrategy = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT, Codings.OBJECT_ARRAY, currentSemanticEvent.CodedObjectID)
                                            {
                                                CodedIncrementID = currentSemanticEvent.CodedObjectIDIncrement,
                                                CodedResultantID = currentSemanticEvent.EventInformation,
                                                StrategySpecifics = Codings.STRATEGY_SPECIFICS_ARRAY_DIVIDE_INK
                                            };
                        codedStrategies.Add(codedStrategy);
                        continue;
                    }

                    if (currentSemanticEvent.EventType == Codings.EVENT_ARRAY_SKIP)
                    {
                        var arrayID = currentSemanticEvent.ReferencePageObjectID;
                        var compoundID = string.Format("{0};{1};{2}", currentSemanticEvent.CodedObjectID, currentSemanticEvent.CodedObjectIDIncrement, arrayID);

                        if (!skipArithCount.ContainsKey(compoundID))
                        {
                            skipArithCount.Add(compoundID, 0);
                        }

                        if (patternStartPoints.Keys.Contains(arrayID))
                        {
                            if (patternStartPoints[arrayID] == Codings.EVENT_ARITH_ADD)
                            {
                                skipArithCount[compoundID]++;
                            }

                            patternStartPoints[arrayID] = Codings.EVENT_ARRAY_SKIP;
                        }
                        else
                        {
                            patternStartPoints.Add(arrayID, Codings.EVENT_ARRAY_SKIP);
                        }
                    }

                    if (currentSemanticEvent.EventType == Codings.EVENT_CUT)
                    {
                        for (int j = i + 1; j < semanticEvents.Count; j++)
                        {
                            var nextSemanticEvent = semanticEvents[j];
                            if (nextSemanticEvent.CodedObject == Codings.OBJECT_ARRAY &&
                                nextSemanticEvent.EventType == Codings.EVENT_ARRAY_SNAP &&
                                nextSemanticEvent.CodedObjectID == currentSemanticEvent.CodedObjectID)
                            {
                                relevantSemanticEvents.Add(currentSemanticEvent);
                                relevantSemanticEvents.Add(nextSemanticEvent);
                                ignoredHistoryIndexes.Add(j);
                                var codedStrategyInner = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT, Codings.OBJECT_ARRAY, currentSemanticEvent.CodedObjectID)
                                                         {
                                                             CodedIncrementID = currentSemanticEvent.CodedObjectIDIncrement,
                                                             CodedResultantID = currentSemanticEvent.EventInformation,
                                                             StrategySpecifics = Codings.STRATEGY_SPECIFICS_ARRAY_CUT_SNAP
                                                         };
                                codedStrategies.Add(codedStrategyInner);
                                continue;
                            }
                        }

                        relevantSemanticEvents.Add(currentSemanticEvent);
                        var codedStrategy = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT, Codings.OBJECT_ARRAY, currentSemanticEvent.CodedObjectID)
                                            {
                                                CodedIncrementID = currentSemanticEvent.CodedObjectIDIncrement,
                                                CodedResultantID = currentSemanticEvent.EventInformation,
                                                StrategySpecifics = Codings.STRATEGY_SPECIFICS_ARRAY_CUT
                                            };
                        codedStrategies.Add(codedStrategy);
                        continue;
                    }

                    if (currentSemanticEvent.EventType == Codings.EVENT_ARRAY_SNAP)
                    {
                        relevantSemanticEvents.Add(currentSemanticEvent);
                        var codedIDLeft = string.Format("{0}{1}",
                                                        currentSemanticEvent.CodedObjectID,
                                                        !string.IsNullOrEmpty(currentSemanticEvent.CodedObjectIDIncrement) ? " " + currentSemanticEvent.CodedObjectIDIncrement : string.Empty);
                        var codedIDRight = string.Format("{0}{1}",
                                                         currentSemanticEvent.CodedObjectSubID,
                                                         !string.IsNullOrEmpty(currentSemanticEvent.CodedObjectSubIDIncrement) ? " " + currentSemanticEvent.CodedObjectSubIDIncrement : string.Empty);
                        var fullCodedID = string.Format("{0}, {1}", codedIDLeft, codedIDRight);
                        var codedStrategy = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT, Codings.OBJECT_ARRAY, fullCodedID)
                                            {
                                                CodedResultantID = currentSemanticEvent.EventInformation,
                                                StrategySpecifics = Codings.STRATEGY_SPECIFICS_ARRAY_SNAP
                                            };
                        codedStrategies.Add(codedStrategy);
                        continue;
                    }
                }
            }

            foreach (var key in skipArithCount.Keys)
            {
                var keySegments = key.Split(';');
                var codedID = keySegments[0];
                var incrementID = keySegments[1];
                var count = skipArithCount[key];

                var codedStrategy = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_ARRAY_SKIP, Codings.OBJECT_ARRAY, codedID)
                                    {
                                        CodedIncrementID = incrementID
                                    };

                if (count > 0)
                {
                    codedStrategy.StrategySpecifics = string.Format("{0} ({1})", Codings.STRATEGY_SPECIFICS_ARRAY_ARITH, count);
                }

                codedStrategies.Add(codedStrategy);
            }

            if (!codedStrategies.Any())
            {
                return;
            }

            var tag = new ArrayStrategyTag(page, Origin.StudentPageGenerated, relevantSemanticEvents, codedStrategies);
            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}