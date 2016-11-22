using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class RepresentationsUsedTag : ATagBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="RepresentationsUsedTag" /> from scratch.</summary>
        public RepresentationsUsedTag() { }

        /// <summary>Initializes <see cref="RepresentationsUsedTag" /> from values.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="ObjectTypesOnPageTag" /> belongs to.</param>
        /// <param name="origin"></param>
        /// <param name="currentUserID"></param>
        public RepresentationsUsedTag(CLPPage parentPage, Origin origin, List<string> allRepresentations, List<string> deletedCodedRepresentations, List<string> finalCodedRepresentations)
            : base(parentPage, origin)
        {
            AllRepresentations = allRepresentations;
            DeletedCodedRepresentations = deletedCodedRepresentations;
            FinalCodedRepresentations = finalCodedRepresentations;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Distinct list of all representations used through the history of the page.</summary>
        public List<string> AllRepresentations
        {
            get { return GetValue<List<string>>(AllRepresentationsProperty); }
            set { SetValue(AllRepresentationsProperty, value); }
        }

        public static readonly PropertyData AllRepresentationsProperty = RegisterProperty("AllRepresentations", typeof(List<string>), () => new List<string>());

        /// <summary>Coded values for all deleted representations.</summary>
        public List<string> DeletedCodedRepresentations
        {
            get { return GetValue<List<string>>(DeletedCodedRepresentationsProperty); }
            set { SetValue(DeletedCodedRepresentationsProperty, value); }
        }

        public static readonly PropertyData DeletedCodedRepresentationsProperty = RegisterProperty("DeletedCodedRepresentations", typeof(List<string>), () => new List<string>());

        /// <summary>Coded values for all final representations.</summary>
        public List<string> FinalCodedRepresentations
        {
            get { return GetValue<List<string>>(FinalCodedRepresentationsProperty); }
            set { SetValue(FinalCodedRepresentationsProperty, value); }
        }

        public static readonly PropertyData FinalCodedRepresentationsProperty = RegisterProperty("FinalCodedRepresentations", typeof(List<string>), () => new List<string>());

        #region ATagBase Overrides

        public override bool IsSingleValueTag => true;

        public string AnalysisCode => $"MR [{string.Join(", ", AllRepresentations)}]";

        public override Category Category => Category.Representation;

        public override string FormattedName => "Representations Used";

        public override string FormattedValue
        {
            get
            {
                if (!AllRepresentations.Any())
                {
                    var isStrokesUsed = ParentPage.InkStrokes.Any() || ParentPage.History.TrashedInkStrokes.Any();
                    return isStrokesUsed ? "Ink Only" : "Blank Page";
                }

                var deletedSection = !DeletedCodedRepresentations.Any() ? string.Empty : string.Format("Deleted Representation(s):\n{0}", string.Join("\n", DeletedCodedRepresentations));
                var finalSectionDelimiter = DeletedCodedRepresentations.Any() && FinalCodedRepresentations.Any() ? "\n" : string.Empty;
                var finalSection = !FinalCodedRepresentations.Any()
                                       ? string.Empty
                                       : string.Format("{0}Final Representation(s):\n{1}", finalSectionDelimiter, string.Join("\n", FinalCodedRepresentations));
                var codeSection = AllRepresentations.Count > 1 ? string.Format("\nCode: {0}", AnalysisCode) : string.Empty;
                return string.Format("{0}{1}{2}", deletedSection, finalSection, codeSection);
            }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties

        #region Static Methods

        public static void AttemptTagGeneration(CLPPage page, List<ISemanticEvent> semanticEvents)
        {
            var allRepresentations = new List<string>();
            var deletedCodedRepresentations = new List<string>();

            var stampedObjectGroups = new Dictionary<string, int>();
            var binGroups = new Dictionary<int, int>();
            var maxStampedObjectGroups = new Dictionary<string, int>();
            var jumpGroups = new Dictionary<string, List<NumberLineJumpSize>>();
            var subArrayGroups = new Dictionary<string, List<string>>();
            foreach (var semanticEvent in semanticEvents)
            {
                #region Stamps

                if (semanticEvent.CodedObject == Codings.OBJECT_STAMPED_OBJECTS)
                {
                    if (semanticEvent.EventType == Codings.EVENT_OBJECT_ADD)
                    {
                        var historyAction = semanticEvent.HistoryActions.First();
                        var objectsChanged = historyAction as ObjectsOnPageChangedHistoryAction;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var stampedObject = objectsChanged.PageObjectsAdded.First() as StampedObject;
                        if (stampedObject == null)
                        {
                            continue;
                        }

                        var parts = stampedObject.Parts;
                        var parentStampID = stampedObject.ParentStampID;
                        var groupID = string.Format("{0} {1}", parts, parentStampID);
                        if (stampedObjectGroups.ContainsKey(groupID))
                        {
                            stampedObjectGroups[groupID]++;
                        }
                        else
                        {
                            stampedObjectGroups.Add(groupID, 1);
                        }

                        maxStampedObjectGroups = stampedObjectGroups;
                    }

                    if (semanticEvent.EventType == Codings.EVENT_OBJECT_DELETE)
                    {
                        var historyAction = semanticEvent.HistoryActions.First();
                        var objectsChanged = historyAction as ObjectsOnPageChangedHistoryAction;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var stampedObject = objectsChanged.PageObjectsRemoved.First() as StampedObject;
                        if (stampedObject == null)
                        {
                            continue;
                        }

                        var parts = stampedObject.Parts;
                        var parentStampID = stampedObject.ParentStampID;
                        var groupID = string.Format("{0} {1}", parts, parentStampID);
                        stampedObjectGroups[groupID]--;
                        if (stampedObjectGroups[groupID] <= 0)
                        {
                            stampedObjectGroups.Remove(groupID);
                        }

                        if (stampedObjectGroups.Keys.Count == 0)
                        {
                            // TODO: Ideally, build entirely off info inside semanticEvent.
                            // Also just use this after the top level for-loop as an end case
                            // test to generate the final reps used.
                            foreach (var key in maxStampedObjectGroups.Keys)
                            {
                                var groupIDSections = key.Split(' ');
                                var stampParts = groupIDSections[0];
                                var obj = Codings.OBJECT_STAMP;
                                var id = stampParts;
                                var componentSection = string.Format(": {0} images", stampedObjectGroups[key]);
                                var codedValue = string.Format("{0} [{1}{2}]", obj, id, componentSection);
                                deletedCodedRepresentations.Add(codedValue);
                                allRepresentations.Add(obj);
                            }
                        }
                    }
                }

                #endregion // Stamps

                #region Number Line

                if (semanticEvent.CodedObject == Codings.OBJECT_NUMBER_LINE)
                {
                    if (semanticEvent.EventType == Codings.EVENT_NUMBER_LINE_JUMP)
                    {
                        var jumpSizesChangedHistoryActions = semanticEvent.HistoryActions.Where(h => h is NumberLineJumpSizesChangedHistoryAction).Cast<NumberLineJumpSizesChangedHistoryAction>().ToList();
                        if (jumpSizesChangedHistoryActions == null ||
                            !jumpSizesChangedHistoryActions.Any())
                        {
                            continue;
                        }

                        var numberLineID = jumpSizesChangedHistoryActions.First().NumberLineID;

                        var allJumps = new List<NumberLineJumpSize>();
                        foreach (var historyAction in jumpSizesChangedHistoryActions)
                        {
                            allJumps.AddRange(historyAction.JumpsAdded);
                        }

                        if (!jumpGroups.ContainsKey(numberLineID))
                        {
                            jumpGroups.Add(numberLineID, allJumps);
                        }
                        else
                        {
                            jumpGroups[numberLineID].AddRange(allJumps);
                        }
                    }

                    if (semanticEvent.EventType == Codings.EVENT_NUMBER_LINE_JUMP_ERASE)
                    {
                        var jumpSizesChangedHistoryActions = semanticEvent.HistoryActions.Where(h => h is NumberLineJumpSizesChangedHistoryAction).Cast<NumberLineJumpSizesChangedHistoryAction>().ToList();
                        if (jumpSizesChangedHistoryActions == null ||
                            !jumpSizesChangedHistoryActions.Any())
                        {
                            continue;
                        }

                        var numberLineID = jumpSizesChangedHistoryActions.First().NumberLineID;

                        var allJumps = new List<NumberLineJumpSize>();
                        foreach (var historyAction in jumpSizesChangedHistoryActions)
                        {
                            allJumps.AddRange(historyAction.JumpsRemoved);
                        }

                        var jumpsToRemove = (from jump in allJumps
                                             from currentJump in jumpGroups[numberLineID]
                                             where jump.JumpSize == currentJump.JumpSize && jump.StartingTickIndex == currentJump.StartingTickIndex
                                             select currentJump).ToList();

                        foreach (var jump in jumpsToRemove)
                        {
                            // BUG: Natalie page 12 has errors here if you don't check ContainsKey, shouldn't happen.
                            if (jumpGroups.ContainsKey(numberLineID))
                            {
                                jumpGroups[numberLineID].Remove(jump);

                                if (!jumpGroups[numberLineID].Any())
                                {
                                    jumpGroups.Remove(numberLineID);
                                }
                            }
                        }
                    }

                    if (semanticEvent.EventType == Codings.EVENT_OBJECT_DELETE)
                    {
                        var historyAction = semanticEvent.HistoryActions.First();
                        var objectsChanged = historyAction as ObjectsOnPageChangedHistoryAction;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var numberLine = objectsChanged.PageObjectsRemoved.First() as NumberLine;
                        if (numberLine == null)
                        {
                            continue;
                        }

                        // TODO: Just like Stamps, use this as end-case to generate final reps
                        var numberLineID = numberLine.ID;

                        var obj = numberLine.CodedName;
                        var id = semanticEvent.CodedObjectID;
                        var components = jumpGroups.ContainsKey(numberLineID) ? NumberLine.ConsolidateJumps(jumpGroups[numberLineID].ToList()) : string.Empty;
                        var componentSection = string.IsNullOrEmpty(components) ? string.Empty : string.Format(": {0}", components);
                        var englishValue = string.Empty;
                        if (!string.IsNullOrEmpty(components))
                        {
                            var jumpsInEnglish = new List<string>();
                            foreach (var codedJump in components.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                try
                                {
                                    var jumpSegments = codedJump.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                    var jumpSize = int.Parse(jumpSegments[0]);
                                    var jumpRange = jumpSegments[1].Split('-');
                                    var start = int.Parse(jumpRange[0]);
                                    var stop = int.Parse(jumpRange[1]);
                                    var numberOfJumps = (stop - start) / jumpSize;
                                    var jumpString = numberOfJumps == 1 ? "jump" : "jumps";
                                    var jumpInEnglish = string.Format("{0} {1} of {2}", numberOfJumps, jumpString, jumpSize);
                                    jumpsInEnglish.Add(jumpInEnglish);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                            englishValue = string.Join("\n  - ", jumpsInEnglish);
                            if (!string.IsNullOrEmpty(englishValue))
                            {
                                englishValue = "\n  - " + englishValue;
                            }
                        }
                        else
                        {
                            englishValue = " (no interaction)";
                        }
                        var codedValue = string.Format("{0} [{1}{2}]{3}", obj, id, componentSection, englishValue);
                        deletedCodedRepresentations.Add(codedValue);
                        if (!string.IsNullOrEmpty(componentSection))
                        {
                            allRepresentations.Add(obj);
                        }
                    }
                }

                #endregion // Number Line

                #region Array

                if (semanticEvent.CodedObject == Codings.OBJECT_ARRAY)
                {
                    if (semanticEvent.EventType == Codings.EVENT_ARRAY_DIVIDE_INK)
                    {
                        var historyAction = semanticEvent.HistoryActions.First();
                        var objectsChanged = historyAction as ObjectsOnPageChangedHistoryAction;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var referenceArrayID = semanticEvent.ReferencePageObjectID;
                        var eventInfo = semanticEvent.EventInformation;
                        var subArrays = eventInfo.Split(new[] { ", " }, StringSplitOptions.None).ToList();
                        if (!subArrayGroups.ContainsKey(referenceArrayID))
                        {
                            subArrayGroups.Add(referenceArrayID, subArrays);
                        }
                        else
                        {
                            subArrayGroups[referenceArrayID].AddRange(subArrays);
                        }
                    }

                    if (semanticEvent.EventType == Codings.EVENT_ARRAY_DIVIDE_INK_ERASE)
                    {
                        var historyAction = semanticEvent.HistoryActions.First();
                        var objectsChanged = historyAction as ObjectsOnPageChangedHistoryAction;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var referenceArrayID = semanticEvent.ReferencePageObjectID;
                        var eventInfo = semanticEvent.EventInformation;
                        var subArrays = eventInfo.Split(new[] { ", " }, StringSplitOptions.None).ToList();
                        foreach (var subArray in subArrays)
                        {
                            if (subArrayGroups.ContainsKey(referenceArrayID))
                            {
                                if (subArrayGroups[referenceArrayID].Contains(subArray))
                                {
                                    subArrayGroups[referenceArrayID].Remove(subArray);
                                    if (!subArrayGroups[referenceArrayID].Any())
                                    {
                                        subArrayGroups.Remove(referenceArrayID);
                                    }
                                }
                            }
                        }
                    }

                    if (semanticEvent.EventType == Codings.EVENT_OBJECT_DELETE)
                    {
                        var historyAction = semanticEvent.HistoryActions.First();
                        var objectsChanged = historyAction as ObjectsOnPageChangedHistoryAction;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var array = objectsChanged.PageObjectsRemoved.First() as CLPArray;
                        if (array == null)
                        {
                            continue;
                        }

                        var obj = array.CodedName;
                        var id = semanticEvent.CodedObjectID;
                        var referenceArrayID = semanticEvent.ReferencePageObjectID;
                        var isInteractedWith =
                            semanticEvents.Where(h => h.ReferencePageObjectID == referenceArrayID)
                                          .Any(
                                               h =>
                                                   h.EventType == Codings.EVENT_ARRAY_DIVIDE_INK || h.EventType == Codings.EVENT_ARRAY_EQN ||
                                                   h.EventType == Codings.EVENT_ARRAY_SKIP ||
                                                   (h.EventType == Codings.EVENT_INK_ADD && h.EventInformation.Contains(Codings.EVENT_INFO_INK_LOCATION_OVER)));

                        var componentSection = !subArrayGroups.ContainsKey(array.ID) ? string.Empty : string.Format(": {0}", string.Join(", ", subArrayGroups[array.ID]));

                        var codedValue = string.Format("{0} [{1}{2}]{3}", obj, id, componentSection, isInteractedWith ? string.Empty : " (no ink)");
                        deletedCodedRepresentations.Add(codedValue);
                        allRepresentations.Add(obj);
                    }
                }

                #endregion // Array
            }

            var finalCodedRepresentations = new List<string>();
            stampedObjectGroups.Clear();
            foreach (var pageObject in page.PageObjects)
            {
                var array = pageObject as CLPArray;
                if (array != null)
                {
                    var obj = array.CodedName;
                    var id = array.CodedID;
                    var referenceArrayID = array.ID;
                    var isInteractedWith =
                        semanticEvents.Where(h => h.ReferencePageObjectID == referenceArrayID)
                                      .Any(
                                           h =>
                                               h.EventType == Codings.EVENT_ARRAY_DIVIDE_INK || h.EventType == Codings.EVENT_ARRAY_EQN ||
                                               h.EventType == Codings.EVENT_ARRAY_SKIP ||
                                               (h.EventType == Codings.EVENT_INK_ADD && h.EventInformation.Contains(Codings.EVENT_INFO_INK_LOCATION_OVER)));

                    var componentSection = !subArrayGroups.ContainsKey(array.ID) ? string.Empty : string.Format(": {0}", string.Join(", ", subArrayGroups[array.ID]));

                    var codedValue = string.Format("{0} [{1}{2}]{3}", obj, id, componentSection, isInteractedWith ? string.Empty : " (no ink)");

                    var formattedSkips = ArraySemanticEvents.StaticSkipCountAnalysis(page, array);
                    if (!string.IsNullOrEmpty(formattedSkips))
                    {
                        // HACK: temporary print out of Wrong Dimension analysis
                        var skipStrings = formattedSkips.Split(' ').ToList().Select(s => s.Replace("\"", string.Empty)).ToList();
                        var skips = new List<int>();
                        foreach (var skip in skipStrings)
                        {
                            if (string.IsNullOrEmpty(skip))
                            {
                                skips.Add(-1);
                                continue;
                            }

                            int number;
                            var isNumber = int.TryParse(skip, out number);
                            if (isNumber)
                            {
                                skips.Add(number);
                                continue;
                            }

                            skips.Add(-1);
                        }

                        var wrongDimensionMatches = 0;
                        for (int i = 0; i < skips.Count - 1; i++)
                        {
                            var currentValue = skips[i];
                            var nextValue = skips[i + 1];
                            if (currentValue == -1 ||
                                nextValue == -1)
                            {
                                continue;
                            }
                            var difference = nextValue - currentValue;
                            if (difference == array.Rows &&
                                array.Rows != array.Columns)
                            {
                                wrongDimensionMatches++;
                            }
                        }

                        var wrongDimensionText = string.Empty;
                        var percentMatchWrongDimensions = wrongDimensionMatches / (skips.Count - 1) * 1.0;
                        if (percentMatchWrongDimensions >= 0.80)
                        {
                            wrongDimensionText = ", wrong dimension";
                        }

                        var skipCodedValue = string.Format("\n  - skip [{0}]{1}", formattedSkips, wrongDimensionText);
                        codedValue = string.Format("{0}{1}", codedValue, skipCodedValue);
                    }

                    finalCodedRepresentations.Add(codedValue);
                    allRepresentations.Add(obj);
                }

                var numberLine = pageObject as NumberLine;
                if (numberLine != null)
                {
                    var obj = numberLine.CodedName;
                    var id = numberLine.CodedID;
                    var components = NumberLine.ConsolidateJumps(numberLine.JumpSizes.ToList());
                    var componentSection = string.IsNullOrEmpty(components) ? string.Empty : string.Format(": {0}", components);
                    var englishValue = string.Empty;
                    if (!string.IsNullOrEmpty(components))
                    {
                        var jumpsInEnglish = new List<string>();
                        foreach (var codedJump in components.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            try
                            {
                                var jumpSegments = codedJump.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                var jumpSize = int.Parse(jumpSegments[0]);
                                var jumpRange = jumpSegments[1].Split('-');
                                var start = int.Parse(jumpRange[0]);
                                var stop = int.Parse(jumpRange[1]);
                                var numberOfJumps = (stop - start) / jumpSize;
                                var jumpString = numberOfJumps == 1 ? "jump" : "jumps";
                                var jumpInEnglish = string.Format("{0} {1} of {2}", numberOfJumps, jumpString, jumpSize);
                                jumpsInEnglish.Add(jumpInEnglish);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        englishValue = string.Join("\n  - ", jumpsInEnglish);
                        if (!string.IsNullOrEmpty(englishValue))
                        {
                            englishValue = "\n  - " + englishValue;
                        }
                    }
                    else
                    {
                        englishValue = " (no interaction)";
                    }
                    var codedValue = string.Format("{0} [{1}{2}]{3}", obj, id, componentSection, englishValue);
                    finalCodedRepresentations.Add(codedValue);
                    allRepresentations.Add(obj);
                }

                var stampedObject = pageObject as StampedObject;
                if (stampedObject != null)
                {
                    var parts = stampedObject.Parts;
                    var parentStampID = stampedObject.ParentStampID;
                    var groupID = string.Format("{0} {1}", parts, parentStampID);
                    if (stampedObjectGroups.ContainsKey(groupID))
                    {
                        stampedObjectGroups[groupID]++;
                    }
                    else
                    {
                        stampedObjectGroups.Add(groupID, 1);
                    }
                }

                var bin = pageObject as Bin;
                if (bin != null)
                {
                    if (binGroups.ContainsKey(bin.Parts))
                    {
                        binGroups[bin.Parts]++;
                    }
                    else
                    {
                        binGroups.Add(bin.Parts, 1);
                    }
                }
            }

            foreach (var key in stampedObjectGroups.Keys)
            {
                var groupIDSections = key.Split(' ');
                var parts = groupIDSections[0];
                var obj = Codings.OBJECT_STAMP;
                var id = parts;
                var componentSection = string.Format(": {0} images", stampedObjectGroups[key]);
                var groupString = stampedObjectGroups[key] == 1 ? "group" : "groups";
                var englishValue = string.Format("{0} {1} of {2}", stampedObjectGroups[key], groupString, parts);
                var codedValue = string.Format("{0} [{1}{2}]\n  - {3}", obj, id, componentSection, englishValue);
                finalCodedRepresentations.Add(codedValue);
                allRepresentations.Add(obj);
            }

            if (binGroups.Keys.Any())
            {
                var id = 0;
                var obj = Codings.OBJECT_BINS;
                var englishValues = new List<string>();
                foreach (var key in binGroups.Keys)
                {
                    var parts = key;
                    var count = binGroups[key];
                    id += count;
                    var binString = count == 1 ? "bin" : "bins";
                    var englishValue = string.Format("{0} {1} of {2}", count, binString, parts);
                    englishValues.Add(englishValue);
                }

                var formattedEnglishValue = string.Join("\n  - ", englishValues);
                var codedValue = string.Format("{0} [{1}]\n  - {2}", obj, id, formattedEnglishValue);
                finalCodedRepresentations.Add(codedValue);
                allRepresentations.Add(obj);
            }

            allRepresentations = allRepresentations.Distinct().ToList();
            var tag = new RepresentationsUsedTag(page, Origin.StudentPageGenerated, allRepresentations, deletedCodedRepresentations, finalCodedRepresentations);
            page.AddTag(tag);
        }

        #endregion // Static Methods
    }
}