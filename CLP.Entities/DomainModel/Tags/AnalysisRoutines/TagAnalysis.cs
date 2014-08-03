using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CLP.Entities
{
    public static class TagAnalysis
    {
        /// <summary>
        /// Method to invoke when the AnalyzeArrayCommand command is executed.
        /// </summary>
        public static void AnalyzeArray(CLPPage page)
        {

            // We have a page definition and an array, so we're good to go!
            //Logger.Instance.WriteToLog("Array found! Dimensions: " + array.Columns + " by " + array.Rows);

            //////////var arrayWidth = array.Columns;
            //////////var arrayHeight = array.Rows;
            //////////var arrayArea = arrayWidth * arrayHeight;


  

            ////////////Logger.Instance.WriteToLog("Tag added: " + strategyTagY.TagType.Name + " -> " + strategyTagY.Value[0].Value);
            ////////////Logger.Instance.WriteToLog("Tag added: " + strategyTagX.TagType.Name + " -> " + strategyTagX.Value[0].Value);

            //////////// Add a strategy tag for inner products (if applicable)
            //////////var partialProducts = array.GetPartialProducts();

            //////////var friendlyFound = false;
            //////////var incomplete = false;

            //////////var foundProducts = new ObservableCollection<int>();
            //////////foreach(var partialProduct in partialProducts)
            //////////{
            //////////    if(partialProduct == 0)
            //////////    {
            //////////        incomplete = true;
            //////////        break;
            //////////    }

            //////////    if(partialProduct % 100 == 0 &&
            //////////       !friendlyFound)
            //////////    {
            //////////        var tag = new ArrayRegionStrategyTag(page, ArrayRegionStrategyTag.AcceptedValues.FriendlyNumbers);
            //////////        page.Tags.Add(tag);
            //////////        friendlyFound = true;
            //////////    }

            //////////    if(!foundProducts.Contains(partialProduct))
            //////////    {
            //////////        foundProducts.Add(partialProduct);
            //////////    }
            //////////}

            //////////if(!incomplete)
            //////////{
            //////////    if(foundProducts.Count > 1 &&
            //////////       foundProducts.Count < (horizDivs.Count * vertDivs.Count))
            //////////    {
            //////////        var tag = new ArrayRegionStrategyTag(page, ArrayRegionStrategyTag.AcceptedValues.SomeRepeated);
            //////////        page.Tags.Add(tag);
            //////////    }
            //////////    else
            //////////    {
            //////////        if(foundProducts.Count == 1 &&
            //////////           (horizDivs.Count * vertDivs.Count) > 1)
            //////////        {
            //////////            var tag = new ArrayRegionStrategyTag(page, ArrayRegionStrategyTag.AcceptedValues.AllRepeated);
            //////////            page.Tags.Add(tag);
            //////////        }
            //////////    }
            //////////}

            // Add an array divider correctness tag
            //Tag divisionCorrectnessTag = CheckArrayDivisionCorrectness(array);
            //tags.Add(divisionCorrectnessTag);

            //Logger.Instance.WriteToLog("Tag added: " + divisionCorrectnessTag.TagType.Name + " -> " + divisionCorrectnessTag.Value[0].Value);

            //// Add tags for the number of horizontal and vertical divisions
            //Tag horizDivsTag = new Tag(Tag.Origins.Generated, ArrayHorizontalDivisionsTagType.Instance);
            //int horizRegions = array.HorizontalDivisions.Count == 0 ? 1 : array.HorizontalDivisions.Count;
            //horizDivsTag.Value.Add(new TagOptionValue(horizRegions.ToString() + " region" + (horizRegions == 1 ? "" : "s")));
            //tags.Add(horizDivsTag);

            //Tag vertDivsTag = new Tag(Tag.Origins.Generated, ArrayVerticalDivisionsTagType.Instance);
            //int vertRegions = array.VerticalDivisions.Count == 0 ? 1 : array.VerticalDivisions.Count;
            //vertDivsTag.Value.Add(new TagOptionValue(vertRegions.ToString() + " region" + (horizRegions == 1 ? "" : "s")));
            //tags.Add(vertDivsTag);

            //Logger.Instance.WriteToLog("Tag added: " + horizDivsTag.TagType.Name + " -> " + horizDivsTag.Value[0].Value);
            //Logger.Instance.WriteToLog("Tag added: " + vertDivsTag.TagType.Name + " -> " + vertDivsTag.Value[0].Value);
        }

        // Get the (sorted) list of subdivisions of startingValue, using the place value strategy
        private static List<int> PlaceValueStrategyDivisions(int startingValue)
        {
            var currentValue = startingValue;
            var currentPlace = 1;
            var output = new List<int>();
            while(currentValue > 0)
            {
                if(currentValue % 10 > 0)
                {
                    output.Add((currentValue % 10) * currentPlace);
                }
                currentValue /= 10;
                currentPlace *= 10;
            }
            output.Sort();

            /*String outputString = "";
            foreach(int x in output)
            {
                outputString += (x.ToString() + " ");
            }
            Logger.Instance.WriteToLog("Place value divisions of " + startingValue + ": " + outputString);*/

            return output;
        }

        // Get the (sorted) list of subdivisions of startingValue, using the tens strategy
        // Assumes 20 < startingValue < 100
        private static List<int> TensStrategyDivisions(int startingValue)
        {
            var currentValue = startingValue;
            var output = new List<int>();
            if(currentValue % 10 > 0)
            {
                output.Add(currentValue % 10);
                currentValue -= (currentValue % 10);
            }
            while(currentValue > 0)
            {
                output.Add(10);
                currentValue -= 10;
            }
            output.Sort();

            return output;
        }

        // Get the (sorted) list of subdivisions of startingValue, using the halving strategy
        // Assumes startingValue is even
        private static List<int> HalvingStrategyDivisions(int startingValue)
        {
            var output = new List<int>();
            output.Add(startingValue / 2);
            output.Add(startingValue / 2);

            return output;
        }

        //private static ATagBase CheckArrayDivisionCorrectness(CLPArray array)
        //{
        //Tag tag = new Tag(Tag.Origins.Generated, ArrayDivisionCorrectnessTagType.Instance);

        //bool horizUnfinished = false;
        //bool vertUnfinished = false;

        //// First check horizontal divisions
        //if(array.HorizontalDivisions.Count > 0)
        //{
        //    int sum = 0;
        //    foreach(CLPArrayDivision div in array.HorizontalDivisions)
        //    {
        //        if(div.Value == 0)
        //        {
        //            horizUnfinished = true;
        //        }
        //        sum += div.Value;
        //    }
        //    if(!horizUnfinished && sum != array.Rows)
        //    {
        //        tag.AddTagOptionValue(new TagOptionValue("Incorrect"));
        //        return tag;
        //    }
        //}

        //// Then check vertical divisions
        //if(array.VerticalDivisions.Count > 0)
        //{
        //    int sum = 0;
        //    foreach(CLPArrayDivision div in array.VerticalDivisions)
        //    {
        //        if(div.Value == 0)
        //        {
        //            vertUnfinished = true;
        //        }
        //        sum += div.Value;
        //    }
        //    if(!vertUnfinished && sum != array.Columns)
        //    {
        //        tag.AddTagOptionValue(new TagOptionValue("Incorrect"));
        //        return tag;
        //    }
        //}

        //if(horizUnfinished || vertUnfinished)
        //{
        //    tag.AddTagOptionValue(new TagOptionValue("Unfinished"));
        //    return tag;
        //}
        //else
        //{
        //    tag.AddTagOptionValue(new TagOptionValue("Correct"));
        //    return tag;
        //}
        //}

        /// <summary>
        /// Method to invoke when the AnalyzeStampsCommand command is executed.
        /// </summary>
        public static void AnalyzeStamps(CLPPage page)
        {
            //    Logger.Instance.WriteToLog("Analyzing stamp grouping region...");

            //    ObservableCollection<Tag> tags = page.PageTags;
            //    ProductRelation relation = null;
            //    foreach(Tag tag in tags)
            //    {
            //        if(tag.TagType.Name == PageDefinitionTagType.Instance.Name)
            //        {
            //            relation = (ProductRelation)tag.Value[0].Value;
            //            break;
            //        }
            //    }

            //    if(relation == null)
            //    {
            //        // No definition for the page!
            //        Logger.Instance.WriteToLog("No page definition found! :(");
            //        return;
            //    }

            //    // Find an array object on the page (just use the first one we find), or be sad if we don't find one
            //    ObservableCollection<ICLPPageObject> objects = page.PageObjects;
            //    CLPGroupingRegion region = null;

            //    foreach(ICLPPageObject pageObject in objects)
            //    {
            //        if(pageObject.GetType() == typeof(CLPGroupingRegion))
            //        {
            //            region = (CLPGroupingRegion)pageObject;
            //            break;
            //        }
            //    }

            //    if(region == null)
            //    {
            //        // No CLPGroupingRegion on this page!
            //        Logger.Instance.WriteToLog("No grouping region found! :(");
            //        return;
            //    }

            //    region.DoInterpretation();
            //    Logger.Instance.WriteToLog("Done with stamps interpretation");

            //    // Now we have a list of the possible interpretations of the student's stamps
            //    ObservableCollection<CLPGrouping> groupings = region.Groupings;

            //    // Clear out any old stamp-related Tags
            //    foreach(Tag tag in tags.ToList())
            //    {
            //        if(tag.TagType == null ||                                           // Clear out any tags that somehow never got a TagType!
            //            tag.TagType.Name == RepresentationCorrectnessTagType.Instance.Name ||
            //            tag.TagType.Name == StampPartsPerStampTagType.Instance.Name ||
            //            tag.TagType.Name == StampGroupingTypeTagType.Instance.Name)
            //        {
            //            tags.Remove(tag);
            //        }
            //    }

            //    ObservableCollection<Tag> newTags = GetStampTags(groupings, relation);

            //    foreach(Tag tag in newTags)
            //    {
            //        tags.Add(tag);
            //    }
            //}

            ///// <summary>
            ///// Returns an appropriate StampCorrectnessTag for the given interpretation and product relation
            ///// </summary>
            //private static ObservableCollection<Tag> GetStampTags(ObservableCollection<CLPGrouping> groupings, ProductRelation relation)
            //{
            //    ObservableCollection<Tag> tags = new ObservableCollection<Tag>();
            //    Tag correctnessTag = new Tag(Tag.Origins.Generated, RepresentationCorrectnessTagType.Instance);
            //    correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Other")); // The student's work is assumed incorrect until proven correct

            //    Tag partsPerStampTag = new Tag(Tag.Origins.Generated, StampPartsPerStampTagType.Instance);
            //    Tag groupingTypeTag = new Tag(Tag.Origins.Generated, StampGroupingTypeTagType.Instance);

            //    foreach(CLPGrouping grouping in groupings)
            //    {
            //        if(HasEqualGroups(grouping) && grouping.Groups[0].Values.Count > 0) // If we can assume that this grouping has a homogeneous structure...
            //        {
            //            int numGroups = grouping.Groups.Count;
            //            List<ICLPPageObject> objList = grouping.Groups[0].Values.ToList()[0];
            //            int objectsPerGroup = objList.Count;
            //            int partsPerObject = objList[0].Parts;
            //            int partsPerGroup = objectsPerGroup * partsPerObject;

            //            partsPerStampTag.Value.Add(new TagOptionValue(partsPerObject.ToString() + (partsPerObject == 1 ? " part" : " parts")));
            //            groupingTypeTag.AddTagOptionValue(new TagOptionValue(grouping.GroupingType));

            //            // We're a little stricter about correctness if it's specifically an equal-grouping problem
            //            if(relation.RelationType == ProductRelation.ProductRelationTypes.EqualGroups)
            //            {
            //                if(relation.Factor1.Equals(numGroups.ToString()) && relation.Factor2.Equals(partsPerGroup.ToString()))
            //                {
            //                    correctnessTag.AddTagOptionValue(new TagOptionValue("Correct"));
            //                    tags.Add(partsPerStampTag);
            //                    tags.Add(groupingTypeTag);
            //                    break;
            //                }
            //                else
            //                {
            //                    if(relation.Factor2.Equals(numGroups.ToString()) && relation.Factor1.Equals(partsPerGroup.ToString()))
            //                    {
            //                        correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Swapped Factors"));
            //                        tags.Add(partsPerStampTag);
            //                        tags.Add(groupingTypeTag);
            //                        break;
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                if((relation.Factor1.Equals(numGroups.ToString()) && relation.Factor2.Equals(partsPerGroup.ToString())) ||
            //                    (relation.Factor2.Equals(numGroups.ToString()) && relation.Factor1.Equals(partsPerGroup.ToString())))
            //                {
            //                    correctnessTag.AddTagOptionValue(new TagOptionValue("Correct"));
            //                    tags.Add(partsPerStampTag);
            //                    tags.Add(groupingTypeTag);
            //                    break;
            //                }
            //            }

            //            // If we haven't hit a break yet, then this isn't looking good. Check for a student using the wrong operator
            //            ObservableCollection<int> givens = new ObservableCollection<int>();
            //            if(relation.Factor1Given)
            //            {
            //                givens.Add(Convert.ToInt32(relation.Factor1));
            //            }
            //            if(relation.Factor2Given)
            //            {
            //                givens.Add(Convert.ToInt32(relation.Factor2));
            //            }
            //            if(relation.ProductGiven)
            //            {
            //                givens.Add(Convert.ToInt32(relation.Product));
            //            }

            //            ObservableCollection<int> numbersUsed = new ObservableCollection<int>();
            //            numbersUsed.Add(numGroups);
            //            numbersUsed.Add(partsPerGroup);
            //            numbersUsed.Add(numGroups * partsPerGroup);

            //            if(givens.Count == 2 && numbersUsed.Contains(givens[0]) && numbersUsed.Contains(givens[1]))
            //            {
            //                correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Misused Givens"));
            //                tags.Add(partsPerStampTag);
            //                tags.Add(groupingTypeTag);
            //                break;
            //            }
            //        }
            //    }
            //    tags.Add(correctnessTag); // A correctness tag always gets added
            //    return tags;
        }

        /// <summary>
        /// Checks a grouping for overall homogeneity.
        /// Returns true iff each subgroup in this grouping contains the same number of page objects, each of which has the same number of parts
        /// </summary>
        //private static Boolean HasEqualGroups(CLPGrouping grouping)
        //{
        //    int expectedPartsPerObject = -1;
        //    int expectedObjectsPerGroup = -1;

        //    foreach(Dictionary<string, List<ICLPPageObject>> groupEntry in grouping.Groups)
        //    {
        //        foreach(KeyValuePair<string, List<ICLPPageObject>> kvp in groupEntry)
        //        {
        //            List<ICLPPageObject> objList = kvp.Value;
        //            if(((CLPStampCopy)(objList[0])).IsCollectionCopy)
        //            {
        //                continue;
        //            } // Ignore collection stamps

        //            int objectsInGroup = objList.Count;
        //            if(expectedObjectsPerGroup == -1)
        //            {
        //                expectedObjectsPerGroup = objectsInGroup;
        //            }
        //            else if(expectedObjectsPerGroup != objectsInGroup)
        //            {
        //                return false;
        //            }

        //            foreach(ICLPPageObject pageObj in objList)
        //            {
        //                int parts = pageObj.Parts;
        //                if(expectedPartsPerObject == -1)
        //                {
        //                    expectedPartsPerObject = parts;
        //                }
        //                else if(expectedPartsPerObject != parts)
        //                {
        //                    return false;
        //                }
        //            }
        //        }
        //    }

        //    return true;
        //}
    }
}