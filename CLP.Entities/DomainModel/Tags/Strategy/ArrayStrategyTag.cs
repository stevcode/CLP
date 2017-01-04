using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CLP.Entities.Demo
{
    [Serializable]
    public class ArrayStrategyTag : ARepresentationStrategyBaseTag
    {
        #region Constructors

        /// <summary>Initializes <see cref="ArrayStrategyTag" /> from scratch.</summary>
        public ArrayStrategyTag() { }

        /// <summary>Initializes <see cref="ArrayStrategyTag" /> from values.</summary>
        public ArrayStrategyTag(CLPPage parentPage, Origin origin, List<IHistoryAction> historyActions, List<CodedRepresentationStrategy> codedStrategies)
            : base(parentPage, origin, historyActions, codedStrategies) { }

        /// <summary>Initializes <see cref="ArrayStrategyTag" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public ArrayStrategyTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region ATagBase Overrides

        public override string FormattedName
        {
            get { return "Array"; }
        }

        #endregion //ATagBase Overrides

        #region Static Methods

        public static void IdentifyArrayStrategies(CLPPage page, List<IHistoryAction> historyActions)
        {
            var relevantHistoryactions = new List<IHistoryAction>();
            var codedStrategies = new List<CodedRepresentationStrategy>();
            var ignoredHistoryIndexes = new List<int>();
            var skipArithCount = new Dictionary<string, int>();
            var patternStartPoints = new Dictionary<string, string>();
            var patternEndPoints = new List<dynamic>();

            for (var i = 0; i < historyActions.Count; i++)
            {
                if (ignoredHistoryIndexes.Contains(i))
                {
                    continue;
                }

                var currentHistoryAction = historyActions[i];
                var isLastHistoryAction = i + 1 >= historyActions.Count;

                if (currentHistoryAction.CodedObject == Codings.OBJECT_ARITH &&
                    currentHistoryAction.CodedObjectAction == Codings.ACTION_ARITH_ADD)
                {
                    var arrayIDs = patternStartPoints.Keys.ToList();
                    foreach (var arrayID in arrayIDs)
                    {
                        patternStartPoints[arrayID] = Codings.ACTION_ARITH_ADD;
                    }
                }

                if (currentHistoryAction.CodedObject == Codings.OBJECT_ARRAY)
                {
                    if (currentHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_DIVIDE)
                    {
                        relevantHistoryactions.Add(currentHistoryAction);
                        var codedStrategy = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT, Codings.OBJECT_ARRAY, currentHistoryAction.CodedObjectID)
                                            {
                                                CodedIncrementID = currentHistoryAction.CodedObjectIDIncrement,
                                                CodedResultantID = currentHistoryAction.CodedObjectActionID,
                                                StrategySpecifics = Codings.STRATEGY_SPECIFICS_ARRAY_DIVIDE
                                            };
                        codedStrategies.Add(codedStrategy);
                        continue;
                    }

                    if (currentHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_DIVIDE_INK)
                    {
                        relevantHistoryactions.Add(currentHistoryAction);
                        var codedStrategy = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT, Codings.OBJECT_ARRAY, currentHistoryAction.CodedObjectID)
                                            {
                                                CodedIncrementID = currentHistoryAction.CodedObjectIDIncrement,
                                                CodedResultantID = currentHistoryAction.CodedObjectActionID,
                                                StrategySpecifics = Codings.STRATEGY_SPECIFICS_ARRAY_DIVIDE_INK
                                            };
                        codedStrategies.Add(codedStrategy);
                        continue;
                    }

                    if (currentHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_SKIP)
                    {
                        var arrayID = currentHistoryAction.ReferencePageObjectID;
                        var compoundID = string.Format("{0};{1};{2}", currentHistoryAction.CodedObjectID, currentHistoryAction.CodedObjectIDIncrement, arrayID);

                        if (!skipArithCount.ContainsKey(compoundID))
                        {
                            skipArithCount.Add(compoundID, 0);
                        }

                        if (patternStartPoints.Keys.Contains(arrayID))
                        {
                            if (patternStartPoints[arrayID] == Codings.ACTION_ARITH_ADD)
                            {
                                skipArithCount[compoundID]++;
                            }

                            patternStartPoints[arrayID] = Codings.ACTION_ARRAY_SKIP;
                        }
                        else
                        {
                            patternStartPoints.Add(arrayID, Codings.ACTION_ARRAY_SKIP);
                        }
                    }

                    if (currentHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_CUT)
                    {
                        for (int j = i + 1; j < historyActions.Count; j++)
                        {
                            var nextHistoryAction = historyActions[j];
                            if (nextHistoryAction.CodedObject == Codings.OBJECT_ARRAY &&
                                nextHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_SNAP &&
                                nextHistoryAction.CodedObjectID == currentHistoryAction.CodedObjectID)
                            {
                                relevantHistoryactions.Add(currentHistoryAction);
                                relevantHistoryactions.Add(nextHistoryAction);
                                ignoredHistoryIndexes.Add(j);
                                var codedStrategyInner = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT, Codings.OBJECT_ARRAY, currentHistoryAction.CodedObjectID)
                                {
                                    CodedIncrementID = currentHistoryAction.CodedObjectIDIncrement,
                                    CodedResultantID = currentHistoryAction.CodedObjectActionID,
                                    StrategySpecifics = Codings.STRATEGY_SPECIFICS_ARRAY_CUT_SNAP
                                };
                                codedStrategies.Add(codedStrategyInner);
                                continue;
                            }
                        }

                        relevantHistoryactions.Add(currentHistoryAction);
                        var codedStrategy = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT, Codings.OBJECT_ARRAY, currentHistoryAction.CodedObjectID)
                                            {
                                                CodedIncrementID = currentHistoryAction.CodedObjectIDIncrement,
                                                CodedResultantID = currentHistoryAction.CodedObjectActionID,
                                                StrategySpecifics = Codings.STRATEGY_SPECIFICS_ARRAY_CUT
                                            };
                        codedStrategies.Add(codedStrategy);
                        continue;
                    }

                    if (currentHistoryAction.CodedObjectAction == Codings.ACTION_ARRAY_SNAP)
                    {
                        relevantHistoryactions.Add(currentHistoryAction);
                        var codedIDLeft = string.Format("{0}{1}",
                                                        currentHistoryAction.CodedObjectID,
                                                        !string.IsNullOrEmpty(currentHistoryAction.CodedObjectIDIncrement) ? " " + currentHistoryAction.CodedObjectIDIncrement : string.Empty);
                        var codedIDRight = string.Format("{0}{1}",
                                                        currentHistoryAction.CodedObjectSubID,
                                                        !string.IsNullOrEmpty(currentHistoryAction.CodedObjectSubIDIncrement) ? " " + currentHistoryAction.CodedObjectSubIDIncrement : string.Empty);
                        var fullCodedID = string.Format("{0}, {1}", codedIDLeft, codedIDRight);
                        var codedStrategy = new CodedRepresentationStrategy(Codings.STRATEGY_NAME_ARRAY_PARTIAL_PRODUCT, Codings.OBJECT_ARRAY, fullCodedID)
                        {
                            CodedResultantID = currentHistoryAction.CodedObjectActionID,
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

            var tag = new ArrayStrategyTag(page, Origin.StudentPageGenerated, relevantHistoryactions, codedStrategies);
            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}