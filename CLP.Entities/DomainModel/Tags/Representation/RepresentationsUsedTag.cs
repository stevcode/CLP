using System;
using System.Collections.Generic;
using System.Linq;
using Catel.Data;

namespace CLP.Entities
{
    public enum RepresentationsUsedTypes
    {
        BlankPage,
        InkOnly,
        RepresentationsUsed
    }

    [Serializable]
    public class RepresentationsUsedTag : AAnalysisTagBase
    {
        #region Constructors

        public RepresentationsUsedTag() { }

        public RepresentationsUsedTag(CLPPage parentPage, Origin origin, List<string> allRepresentations, List<string> deletedCodedRepresentations, List<string> finalCodedRepresentations)
            : base(parentPage, origin)
        {
            AllRepresentations = allRepresentations;
            DeletedCodedRepresentations = deletedCodedRepresentations;
            FinalCodedRepresentations = finalCodedRepresentations;
        }

        #endregion //Constructors

        #region Properties

        /// <summary>Identifies 3 possible usages scenarios: Ink Only, Blank Page, or Representations Used.</summary>
        public RepresentationsUsedTypes RepresentationsUsedType
        {
            get { return GetValue<RepresentationsUsedTypes>(RepresentationsUsedTypeProperty); }
            set { SetValue(RepresentationsUsedTypeProperty, value); }
        }

        public static readonly PropertyData RepresentationsUsedTypeProperty = RegisterProperty("RepresentationsUsedType", typeof(RepresentationsUsedTypes), RepresentationsUsedTypes.BlankPage);
        




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
                switch (RepresentationsUsedType)
                {
                    case RepresentationsUsedTypes.BlankPage:
                        return "Blank Page";
                    case RepresentationsUsedTypes.InkOnly:
                        return "Ink Only";
                }


                if (!AllRepresentations.Any())
                {
                    var isStrokesUsed = ParentPage.InkStrokes.Any() || ParentPage.History.TrashedInkStrokes.Any();
                    return isStrokesUsed ? "Ink Only" : "Blank Page";
                }

                var deletedSection = !DeletedCodedRepresentations.Any() ? string.Empty : $"Deleted Representation(s):\n{string.Join("\n", DeletedCodedRepresentations)}";
                var finalSectionDelimiter = DeletedCodedRepresentations.Any() && FinalCodedRepresentations.Any() ? "\n" : string.Empty;
                var finalSection = !FinalCodedRepresentations.Any()
                                       ? string.Empty
                                       : $"{finalSectionDelimiter}Final Representation(s):\n{string.Join("\n", FinalCodedRepresentations)}";
                var codeSection = AllRepresentations.Count > 1 ? $"\nCode: {AnalysisCode}" : string.Empty;
                return $"{deletedSection}{finalSection}{codeSection}";
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
                        var groupID = $"{parts} {parentStampID}";
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
                        var groupID = $"{parts} {parentStampID}";
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
                                var componentSection = $": {stampedObjectGroups[key]} images";
                                var codedValue = $"{obj} [{id}{componentSection}]";
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
                                    var jumpInEnglish = $"{numberOfJumps} {jumpString} of {jumpSize}";
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
                        var codedValue = $"{obj} [{id}{componentSection}]{englishValue}";
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
                        var historyAction = semanticEvent.HistoryActions.FirstOrDefault();
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
                        var historyAction = semanticEvent.HistoryActions.FirstOrDefault();
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
                        var historyAction = semanticEvent.HistoryActions.FirstOrDefault();
                        var objectsChanged = historyAction as ObjectsOnPageChangedHistoryAction;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var array = objectsChanged.PageObjectsRemoved.FirstOrDefault() as CLPArray;
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

                        var componentSection = !subArrayGroups.ContainsKey(array.ID) ? string.Empty : $": {string.Join(", ", subArrayGroups[array.ID])}";

                        var codedValue = $"{obj} [{id}{componentSection}]{(isInteractedWith ? string.Empty : " (no ink)")}";
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

                    var componentSection = !subArrayGroups.ContainsKey(array.ID) ? string.Empty : $": {string.Join(", ", subArrayGroups[array.ID])}";

                    var codedValue = $"{obj} [{id}{componentSection}]{(isInteractedWith ? string.Empty : " (no ink)")}";

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

                        var skipCodedValue = $"\n  - skip [{formattedSkips}]{wrongDimensionText}";
                        codedValue = $"{codedValue}{skipCodedValue}";
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
                    var componentSection = string.IsNullOrEmpty(components) ? string.Empty : $": {components}";
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
                                var jumpInEnglish = $"{numberOfJumps} {jumpString} of {jumpSize}";
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
                    var codedValue = $"{obj} [{id}{componentSection}]{englishValue}";
                    finalCodedRepresentations.Add(codedValue);
                    allRepresentations.Add(obj);
                }

                var stampedObject = pageObject as StampedObject;
                if (stampedObject != null)
                {
                    var parts = stampedObject.Parts;
                    var parentStampID = stampedObject.ParentStampID;
                    var groupID = $"{parts} {parentStampID}";
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
                var componentSection = $": {stampedObjectGroups[key]} images";
                var groupString = stampedObjectGroups[key] == 1 ? "group" : "groups";
                var englishValue = $"{stampedObjectGroups[key]} {groupString} of {parts}";
                var codedValue = $"{obj} [{id}{componentSection}]\n  - {englishValue}";
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
                    var englishValue = $"{count} {binString} of {parts}";
                    englishValues.Add(englishValue);
                }

                var formattedEnglishValue = string.Join("\n  - ", englishValues);
                var codedValue = $"{obj} [{id}]\n  - {formattedEnglishValue}";
                finalCodedRepresentations.Add(codedValue);
                allRepresentations.Add(obj);
            }

            allRepresentations = allRepresentations.Distinct().ToList();
            var tag = new RepresentationsUsedTag(page, Origin.StudentPageGenerated, allRepresentations, deletedCodedRepresentations, finalCodedRepresentations);
            page.AddTag(tag);
        }

        public static void GenerateArraysUsedInformation(CLPPage page, RepresentationsUsedTag tag, List<ISemanticEvent> semanticEvents)
        {
            // This code looks for any of the following patterns for each array, where all other Event types are ignored:
            // ARR add, INK change, ARR delete/ARR move/End
            // ARR move, INK change, ARR delete/ARR move/End
            // When one of the above patterns is recognized, the start and end historyIndex and associated arrayID are stored in a list
            // of patternEndPoints. Every identified patternEndPoint will be analyzed at that point in history for skip counting
            // and any skip count strokes found at that point will be isolated in their own clusters.
            var patternStartPoints = new Dictionary<string, string>();
            var patternEndPoints = new List<dynamic>();

            #region Recognized Pattern End Points

            foreach (var currentSemanticEvent in semanticEvents)
            {
                if (currentSemanticEvent.CodedObject == Codings.OBJECT_ARRAY)
                {
                    var arrayID = currentSemanticEvent.ReferencePageObjectID;

                    if (currentSemanticEvent.EventType == Codings.EVENT_OBJECT_ADD)
                    {
                        if (!patternStartPoints.Keys.Contains(arrayID))
                        {
                            var startPattern = $"{Codings.EVENT_OBJECT_ADD};{currentSemanticEvent.HistoryActions.First().HistoryActionIndex}";
                            patternStartPoints.Add(arrayID, startPattern);
                        }
                    }
                    else if (currentSemanticEvent.EventType == Codings.EVENT_OBJECT_MOVE)
                    {
                        var startPattern = $"{Codings.EVENT_OBJECT_MOVE};{currentSemanticEvent.HistoryActions.First().HistoryActionIndex}";
                        if (patternStartPoints.Keys.Contains(arrayID))
                        {
                            if (patternStartPoints[arrayID].Contains(Codings.EVENT_INK_CHANGE))
                            {
                                var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                                var endHistoryIndex = currentSemanticEvent.HistoryActions.Last().HistoryActionIndex;
                                patternEndPoints.Add(new
                                {
                                    ArrayID = arrayID,
                                    StartHistoryIndex = startHistoryIndex,
                                    EndHistoryIndex = endHistoryIndex
                                });
                            }

                            patternStartPoints[arrayID] = startPattern;
                        }
                        else
                        {
                            patternStartPoints.Add(arrayID, startPattern);
                        }
                    }
                    else if (currentSemanticEvent.EventType == Codings.EVENT_OBJECT_DELETE)
                    {
                        if (!patternStartPoints.Keys.Contains(arrayID))
                        {
                            continue;
                        }

                        if (patternStartPoints[arrayID].Contains(Codings.EVENT_INK_CHANGE))
                        {
                            var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                            var endHistoryIndex = currentSemanticEvent.HistoryActions.Last().HistoryActionIndex;
                            patternEndPoints.Add(new
                            {
                                ArrayID = arrayID,
                                StartHistoryIndex = startHistoryIndex,
                                EndHistoryIndex = endHistoryIndex
                            });
                        }

                        patternStartPoints.Remove(arrayID);
                    }
                }
                else if (currentSemanticEvent.EventType == Codings.EVENT_INK_CHANGE)
                {
                    var arrayIDs = patternStartPoints.Keys.ToList();
                    foreach (var arrayID in arrayIDs)
                    {
                        var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                        var startPattern = $"{Codings.EVENT_INK_CHANGE};{startHistoryIndex}";
                        patternStartPoints[arrayID] = startPattern;
                    }
                }
            }

            foreach (var arrayID in patternStartPoints.Keys)
            {
                if (!patternStartPoints[arrayID].Contains(Codings.EVENT_INK_CHANGE))
                {
                    continue;
                }

                var startHistoryIndex = patternStartPoints[arrayID].Split(';')[1];
                var endHistoryIndex = semanticEvents.Last().HistoryActions.Last().HistoryActionIndex;
                patternEndPoints.Add(new
                {
                    ArrayID = arrayID,
                    StartHistoryIndex = startHistoryIndex,
                    EndHistoryIndex = endHistoryIndex
                });
            }

            #endregion // Recognized Pattern End Points

            #region other

            var subArrayGroups = new Dictionary<string, List<string>>();
            foreach (var semanticEvent in semanticEvents)
            {
                if (semanticEvent.CodedObject == Codings.OBJECT_ARRAY)
                {
                    if (semanticEvent.EventType == Codings.EVENT_ARRAY_DIVIDE_INK)
                    {
                        var historyAction = semanticEvent.HistoryActions.FirstOrDefault();
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
                        var historyAction = semanticEvent.HistoryActions.FirstOrDefault();
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
                        var historyAction = semanticEvent.HistoryActions.FirstOrDefault();
                        var objectsChanged = historyAction as ObjectsOnPageChangedHistoryAction;
                        if (objectsChanged == null)
                        {
                            continue;
                        }

                        var array = objectsChanged.PageObjectsRemoved.FirstOrDefault() as CLPArray;
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

                        var componentSection = !subArrayGroups.ContainsKey(array.ID) ? string.Empty : $": {string.Join(", ", subArrayGroups[array.ID])}";

                        var codedValue = $"{obj} [{id}{componentSection}]{(isInteractedWith ? string.Empty : " (no ink)")}";
                        deletedCodedRepresentations.Add(codedValue);
                        allRepresentations.Add(obj);
                    }
                }
            }

            foreach (var array in page.PageObjects.OfType<CLPArray>())
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

                var componentSection = !subArrayGroups.ContainsKey(array.ID) ? string.Empty : $": {string.Join(", ", subArrayGroups[array.ID])}";

                var codedValue = $"{obj} [{id}{componentSection}]{(isInteractedWith ? string.Empty : " (no ink)")}";

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

                    var skipCodedValue = $"\n  - skip [{formattedSkips}]{wrongDimensionText}";
                    codedValue = $"{codedValue}{skipCodedValue}";
                }

                finalCodedRepresentations.Add(codedValue);
                allRepresentations.Add(obj);
            }

            #endregion // other
        }

        private class PatternPoint
        {
            public PatternPoint()
            {
                StartHistoryActionIndex = -1;
                StartSemanticEventIndex = -1;
                EndHistoryActionIndex = -1;
                EndSemanticEventIndex = -1;
            }

            public string PageObjectID { get; set; }
           
            public int StartHistoryActionIndex { get; set; }
            public int StartSemanticEventIndex { get; set; }
            public int EndHistoryActionIndex { get; set; }
            public int EndSemanticEventIndex { get; set; }
            public string StartEventType { get; set; }
            public string EndEventType { get; set; }
        }

        public static void GenerateNumberLinesUsedInformation(CLPPage page, RepresentationsUsedTag tag, List<ISemanticEvent> semanticEvents, SimplifiedRelation leftRelation, SimplifiedRelation rightRelation, SimplifiedRelation alternativeRelation)
        {
            var patternPoints = new List<PatternPoint>();
            var jumpGroups = new Dictionary<string, List<NumberLineJumpSize>>();
            var jumpEraseCount = new Dictionary<string, int>();

            #region Find Pattern Points

            foreach (var semanticEvent in semanticEvents.Where(e => e.CodedObject == Codings.OBJECT_NUMBER_LINE))
            {
                var numberLineID = semanticEvent.ReferencePageObjectID;

                if (semanticEvent.EventType == Codings.EVENT_OBJECT_ADD)
                {
                    var historyAction = semanticEvent.HistoryActions.FirstOrDefault();
                    var objectsChanged = historyAction as ObjectsOnPageChangedHistoryAction;
                    if (objectsChanged == null)
                    {
                        continue;
                    }

                    var patternPoint = new PatternPoint
                    {
                        PageObjectID = numberLineID,
                        StartHistoryActionIndex = objectsChanged.HistoryActionIndex,
                        StartSemanticEventIndex = semanticEvent.SemanticEventIndex,
                        StartEventType = semanticEvent.EventType
                    };
                    patternPoints.Add(patternPoint);
                }
                else if (semanticEvent.EventType == Codings.EVENT_NUMBER_LINE_JUMP)
                {
                    var jumpSizesChangedHistoryActions = semanticEvent.HistoryActions.OfType<NumberLineJumpSizesChangedHistoryAction>().ToList();
                    if (!jumpSizesChangedHistoryActions.Any())
                    {
                        continue;
                    }

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
                else if (semanticEvent.EventType == Codings.EVENT_NUMBER_LINE_JUMP_ERASE)
                {
                    var jumpSizesChangedHistoryActions = semanticEvent.HistoryActions.OfType<NumberLineJumpSizesChangedHistoryAction>().ToList();
                    if (!jumpSizesChangedHistoryActions.Any())
                    {
                        continue;
                    }

                    var allJumps = new List<NumberLineJumpSize>();
                    foreach (var historyAction in jumpSizesChangedHistoryActions)
                    {
                        allJumps.AddRange(historyAction.JumpsRemoved);
                    }

                    var jumpsToRemove = (from jump in allJumps
                                         from currentJump in jumpGroups[numberLineID]
                                         where jump.JumpSize == currentJump.JumpSize && jump.StartingTickIndex == currentJump.StartingTickIndex
                                         select currentJump).ToList();

                    if (!jumpEraseCount.ContainsKey(numberLineID))
                    {
                        jumpEraseCount.Add(numberLineID, 0);
                    }
                    jumpEraseCount[numberLineID] += jumpsToRemove.Count;

                    foreach (var jump in jumpsToRemove)
                    {
                        jumpGroups[numberLineID].Remove(jump);
                        if (!jumpGroups[numberLineID].Any())
                        {
                            jumpGroups.Remove(numberLineID);
                        }
                    }
                }
                else if (semanticEvent.EventType == Codings.EVENT_OBJECT_DELETE)
                {
                    var historyAction = semanticEvent.HistoryActions.FirstOrDefault();
                    var objectsChanged = historyAction as ObjectsOnPageChangedHistoryAction;
                    if (objectsChanged == null)
                    {
                        continue;
                    }

                    var patternPoint = patternPoints.FirstOrDefault(p => p.PageObjectID == numberLineID);
                    if (patternPoint == null)
                    {
                        continue;
                    }

                    patternPoint.EndHistoryActionIndex = objectsChanged.HistoryActionIndex;
                    patternPoint.EndSemanticEventIndex = semanticEvent.SemanticEventIndex;
                    patternPoint.EndEventType = semanticEvent.EventType;
                }
            }

            #endregion // Find Pattern Points

            foreach (var patternPoint in patternPoints)
            {
                // OJB [ID] RepresentationInfo
                //      - correctness
                //      - definition sided with if correct/par (otherwise neither)
                //      - is final representation
                //      - is used/interacted with
                //      - extra info
                //      - extra info
                //      - Analysis Codes:

                var numberLineID = patternPoint.PageObjectID;
                var numberLine = page.GetPageObjectByIDOnPageOrInHistory(numberLineID);
                if (numberLine == null)
                {
                    continue;
                }

                #region Basic Representation Info

                var isFinalRepresentation = false;
                if (patternPoint.EndHistoryActionIndex == -1)
                {
                    patternPoint.EndHistoryActionIndex = semanticEvents.Last().HistoryActions.Last().HistoryActionIndex;
                    patternPoint.EndSemanticEventIndex = semanticEvents.Last().SemanticEventIndex;
                    isFinalRepresentation = true;
                }

                var codedObject = Codings.OBJECT_NUMBER_LINE;
                var codedID = numberLine.GetCodedIDAtHistoryIndex(patternPoint.EndHistoryActionIndex);
                var isUsed = jumpGroups.ContainsKey(numberLineID);
                var representationInfo = string.Empty;
                if (isUsed)
                {
                    representationInfo = NumberLine.ConsolidateJumps(jumpGroups[numberLineID].ToList());
                }

                var extraInfo = new List<string>();
                if (!string.IsNullOrEmpty(representationInfo))
                {
                    var jumpsInEnglish = new List<string>();
                    foreach (var codedJump in representationInfo.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries))
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
                            var jumpInEnglish = $"{numberOfJumps} {jumpString} of {jumpSize}";
                            jumpsInEnglish.Add(jumpInEnglish);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    extraInfo.AddRange(jumpsInEnglish);
                }

                var analysisCodes = new List<string>();
                if (jumpEraseCount.ContainsKey(numberLineID) &&
                    jumpEraseCount[numberLineID] > 1)
                {
                    analysisCodes.Add(Codings.NUMBER_LINE_NLJE);
                }

                #endregion // Basic Representation Info

                #region Representation Correctness

                var matchedRelationSide = Codings.MATCHED_RELATION_NONE;
                var representationCorrectness = Correctness.Unknown;
                if (isUsed)
                {
                    var jumpSizes = jumpGroups[numberLineID];
                    var representationRelation = RepresentationCorrectnessTag.GenerateNumberLineRelation(jumpSizes);

                    var leftCorrectness = RepresentationCorrectnessTag.CompareSimplifiedRelations(representationRelation, leftRelation);
                    var rightCorrectness = RepresentationCorrectnessTag.CompareSimplifiedRelations(representationRelation, rightRelation);
                    var alternativeCorrectness = RepresentationCorrectnessTag.CompareSimplifiedRelations(representationRelation, alternativeRelation);

                    if (leftCorrectness == Correctness.Correct)
                    {
                        matchedRelationSide = Codings.MATCHED_RELATION_LEFT;
                        representationCorrectness = Correctness.Correct;
                    }
                    else if (rightCorrectness == Correctness.Correct)
                    {
                        matchedRelationSide = Codings.MATCHED_RELATION_RIGHT;
                        representationCorrectness = Correctness.Correct;
                    }
                    else if (alternativeCorrectness == Correctness.Correct)
                    {
                        matchedRelationSide = Codings.MATCHED_RELATION_ALTERNATIVE;
                        representationCorrectness = Correctness.Correct;
                    }
                    else if (leftCorrectness == Correctness.PartiallyCorrect)
                    {
                        matchedRelationSide = Codings.MATCHED_RELATION_LEFT;
                        representationCorrectness = Correctness.PartiallyCorrect;
                    }
                    else if (rightCorrectness == Correctness.PartiallyCorrect)
                    {
                        matchedRelationSide = Codings.MATCHED_RELATION_RIGHT;
                        representationCorrectness = Correctness.PartiallyCorrect;
                    }
                    else if (alternativeCorrectness == Correctness.PartiallyCorrect)
                    {
                        matchedRelationSide = Codings.MATCHED_RELATION_ALTERNATIVE;
                        representationCorrectness = Correctness.PartiallyCorrect;
                    }
                    else if (leftCorrectness == Correctness.Incorrect ||
                             rightCorrectness == Correctness.Incorrect ||
                             alternativeCorrectness == Correctness.Incorrect)
                    {
                        representationCorrectness = Correctness.Incorrect;
                    }
                }
                else
                {
                    if (leftRelation?.PageDefinition is DivisionRelationDefinitionTag)
                    {
                        var product = leftRelation.Product;
                        double numberLineEndPoint;
                        if (double.TryParse(codedID, out numberLineEndPoint) &&
                            Math.Abs(product - numberLineEndPoint) < 0.0001)
                        {
                            analysisCodes.Add(Codings.NUMBER_LINE_BLANK_PARTIAL_MATCH_LEFT);
                        }
                    }

                    if (rightRelation?.PageDefinition is DivisionRelationDefinitionTag)
                    {
                        var product = rightRelation.Product;
                        double numberLineEndPoint;
                        if (double.TryParse(codedID, out numberLineEndPoint) &&
                            Math.Abs(product - numberLineEndPoint) < 0.0001)
                        {
                            analysisCodes.Add(Codings.NUMBER_LINE_BLANK_PARTIAL_MATCH_LEFT);
                        }
                    }

                    if (alternativeRelation?.PageDefinition is DivisionRelationDefinitionTag)
                    {
                        var product = alternativeRelation.Product;
                        double numberLineEndPoint;
                        if (double.TryParse(codedID, out numberLineEndPoint) &&
                            Math.Abs(product - numberLineEndPoint) < 0.0001)
                        {
                            analysisCodes.Add(Codings.NUMBER_LINE_BLANK_PARTIAL_MATCH_LEFT);
                        }
                    }
                }

                #endregion // Representation Correctness
            }
        }

        #endregion // Static Methods
    }
}