using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLP.Models
{
    public static class PageAnalysis
    {

        /// <summary>
        /// Method to invoke when the AnalyzeArrayCommand command is executed.
        /// </summary>
        public static void AnalyzeArray(ICLPPage page)
        {
            Logger.Instance.WriteToLog("Start of PageAnalysis.AnalyzeArray");
            ObservableCollection<Tag> tags = page.PageTags;
            ProductRelation relation = null;
            foreach(Tag tag in tags)
            {
                if(tag.TagType.Name == PageDefinitionTagType.Instance.Name)
                {
                    relation = (ProductRelation)tag.Value[0].Value;
                    break;
                }
            }

            if(relation == null)
            {
                // No definition for the page!
                Logger.Instance.WriteToLog("No page definition found! :(");
                return;
            }


            // Find an array object on the page (just use the first one we find), or be sad if we don't find one
            ObservableCollection<ICLPPageObject> objects = page.PageObjects;
            CLPArray array = null;

            foreach(ICLPPageObject pageObject in objects)
            {
                if(pageObject.GetType() == typeof(CLPArray))
                {
                    array = (CLPArray)pageObject;
                    break;
                }
            }

            if(array == null)
            {
                // No array on the page!
                Logger.Instance.WriteToLog("No array found! :(");
                return;
            }

            // We have a page definition and an array, so we're good to go!
            Logger.Instance.WriteToLog("Array found! Dimensions: " + array.Columns + " by " + array.Rows);

            int arrayWidth = array.Columns;
            int arrayHeight = array.Rows;
            int arrayArea = arrayWidth * arrayHeight;

            // TODO: Add handling for variables in math relation
            int factor1 = Convert.ToInt32(relation.Factor1);
            int factor2 = Convert.ToInt32(relation.Factor2);
            int product = Convert.ToInt32(relation.Product);

            // Make a tag based on the array's orientation
            // First, clear out any old ArrayOrientationTagTypes
            foreach(Tag tag in tags.ToList())
            {
                if(tag.TagType == null ||
                    tag.TagType.Name == RepresentationCorrectnessTagType.Instance.Name ||
                    tag.TagType.Name == ArrayOrientationTagType.Instance.Name ||
                    tag.TagType.Name == ArrayXAxisStrategyTagType.Instance.Name ||
                    tag.TagType.Name == ArrayYAxisStrategyTagType.Instance.Name ||
                    tag.TagType.Name == ArrayPartialProductsStrategyTagType.Instance.Name ||
                    tag.TagType.Name == ArrayDivisionCorrectnessTagType.Instance.Name ||
                    tag.TagType.Name == ArrayVerticalDivisionsTagType.Instance.Name ||
                    tag.TagType.Name == ArrayHorizontalDivisionsTagType.Instance.Name)
                {
                    tags.Remove(tag);
                }
            }

            // Apply a representation correctness tag
            Tag correctnessTag = new Tag(Tag.Origins.Generated, RepresentationCorrectnessTagType.Instance);
            correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Other"));

            if(factor1 == arrayWidth && factor2 == arrayHeight)
            {
                correctnessTag.AddTagOptionValue(new TagOptionValue("Correct"));
            }
            else
            {
                if(factor2 == arrayWidth && factor1 == arrayHeight)
                {
                    // Are the factors swapped? Depends on whether we care about width vs. height
                    if(relation.RelationType == ProductRelation.ProductRelationTypes.Area)
                    {
                        correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Swapped Factors"));
                    }
                    else
                    {
                        correctnessTag.AddTagOptionValue(new TagOptionValue("Correct"));
                    }
                }
                else
                {
                    // One more possibility: The representation uses the givens in the problem, but in the wrong way
                    ObservableCollection<int> givens = new ObservableCollection<int>();
                    if(relation.Factor1Given) { givens.Add(factor1); }
                    if(relation.Factor2Given) { givens.Add(factor2); }
                    if(relation.ProductGiven) { givens.Add(product); }

                    ObservableCollection<int> numbersUsed = new ObservableCollection<int>();
                    numbersUsed.Add(arrayWidth);
                    numbersUsed.Add(arrayHeight);
                    numbersUsed.Add(arrayWidth * arrayHeight);

                    if(givens.Count == 2 && numbersUsed.Contains(givens[0]) && numbersUsed.Contains(givens[1]))
                    {
                        correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Misused Givens"));
                    }
                }
            }

            tags.Add(correctnessTag);

            // Apply an orientation tag
            Tag orientationTag = new Tag(Tag.Origins.Generated, ArrayOrientationTagType.Instance);
            if(arrayWidth == factor1 && arrayHeight == factor2)
            {
                orientationTag.AddTagOptionValue(new TagOptionValue("First factor is width"));
            }
            else if(arrayWidth == factor2 && arrayHeight == factor1)
            {
                orientationTag.AddTagOptionValue(new TagOptionValue("First factor is height"));
            }
            else
            {
                orientationTag.AddTagOptionValue(new TagOptionValue("unknown"));
            }
            tags.Add(orientationTag);
            Logger.Instance.WriteToLog("Tag added: " + orientationTag.TagType.Name + " -> " + orientationTag.Value[0].Value);

            // Apply a strategy tag
            Tag strategyTagY = new Tag(Tag.Origins.Generated, ArrayYAxisStrategyTagType.Instance);
            Tag strategyTagX = new Tag(Tag.Origins.Generated, ArrayXAxisStrategyTagType.Instance);

            // First check the horizontal divisions
            // Create a sorted list of the divisions' labels (as entered by the student)
            List<int> horizDivs = new List<int>();
            foreach(CLPArrayDivision div in array.HorizontalDivisions)
            {
                horizDivs.Add(div.Value);
            }
            horizDivs.Sort();

            // special case where no dividers have been added to axis
            if(array.HorizontalDivisions.Count == 0)
            {
                horizDivs.Add(array.Rows);
            }

            /*String horizDivsString = "";
            foreach(int x in horizDivs)
            {
                horizDivsString += (x.ToString() + " ");
            }
            Logger.Instance.WriteToLog("Number of horizontal regions: " + horizDivs.Count);
            Logger.Instance.WriteToLog("Student's horizontal divisions (sorted): " + horizDivsString);*/

            // Now check the student's divisions against known strategies
            if(array.HorizontalDivisions.Count == 0)
            {
                strategyTagY.AddTagOptionValue(new TagOptionValue("none"));
            }
            else if(horizDivs.SequenceEqual(PlaceValueStrategyDivisions(arrayHeight)))
            {
                strategyTagY.AddTagOptionValue(new TagOptionValue("place value"));
            }
            else if(arrayHeight > 20 && arrayHeight < 100 && horizDivs.SequenceEqual(TensStrategyDivisions(arrayHeight)))
            {
                strategyTagY.AddTagOptionValue(new TagOptionValue("10's"));
            }
            else if((arrayHeight % 2 == 0) && horizDivs.SequenceEqual(HalvingStrategyDivisions(arrayHeight)))
            {
                strategyTagY.AddTagOptionValue(new TagOptionValue("half"));
            }
            else
            {
                strategyTagY.AddTagOptionValue(new TagOptionValue("other"));
            }

            // Then the vertical divisions
            List<int> vertDivs = new List<int>();
            foreach(CLPArrayDivision div in array.VerticalDivisions)
            {
                vertDivs.Add(div.Value);
            }
            vertDivs.Sort();

            // special case where no dividers have been added to axis
            if(array.VerticalDivisions.Count == 0)
            {
                vertDivs.Add(array.Columns);
            }

            // Now check the student's divisions against known strategies
            if(array.VerticalDivisions.Count == 0)
            {
                strategyTagX.AddTagOptionValue(new TagOptionValue("none"));
            }
            else if(vertDivs.SequenceEqual(PlaceValueStrategyDivisions(arrayWidth)))
            {
                strategyTagX.AddTagOptionValue(new TagOptionValue("place value"));
            }
            else if(arrayWidth > 20 && arrayWidth < 100 && vertDivs.SequenceEqual(TensStrategyDivisions(arrayWidth)))
            {
                strategyTagX.AddTagOptionValue(new TagOptionValue("10's"));
            }
            else if((arrayWidth % 2 == 0) && vertDivs.SequenceEqual(HalvingStrategyDivisions(arrayWidth)))
            {
                strategyTagX.AddTagOptionValue(new TagOptionValue("half"));
            }
            else
            {
                strategyTagX.AddTagOptionValue(new TagOptionValue("other"));
            }

            tags.Add(strategyTagY);
            tags.Add(strategyTagX);

            //Logger.Instance.WriteToLog("Tag added: " + strategyTagY.TagType.Name + " -> " + strategyTagY.Value[0].Value);
            //Logger.Instance.WriteToLog("Tag added: " + strategyTagX.TagType.Name + " -> " + strategyTagX.Value[0].Value);

            // Add a strategy tag for inner products (if applicable)
            int[,] partialProducts = array.GetPartialProducts();

            Tag partialProductStrategyTag = new Tag(Tag.Origins.Generated, ArrayPartialProductsStrategyTagType.Instance);
            Boolean friendlyFound = false;
            Boolean incomplete = false;
            
            ObservableCollection<int> foundProducts = new ObservableCollection<int>();
            foreach(int partialProduct in partialProducts)
            {
                if(partialProduct == 0)
                {
                    incomplete = true;
                    break;
                }

                if(partialProduct % 100 == 0 && ! friendlyFound)
                {
                    partialProductStrategyTag.AddTagOptionValue(new TagOptionValue("friendly numbers"));
                    friendlyFound = true;
                }

                if(!foundProducts.Contains(partialProduct))
                {
                    foundProducts.Add(partialProduct);
                }
            }

            if(!incomplete)
            {
                if(foundProducts.Count > 1 && foundProducts.Count < (horizDivs.Count * vertDivs.Count))
                {
                    partialProductStrategyTag.AddTagOptionValue(new TagOptionValue("some repeated"));
                }
                else
                {
                    if(foundProducts.Count == 1 && (horizDivs.Count * vertDivs.Count) > 1)
                    {
                        partialProductStrategyTag.AddTagOptionValue(new TagOptionValue("all repeated"));
                    }
                }
            }

            if(partialProductStrategyTag.Value.Count > 0)
            {
                tags.Add(partialProductStrategyTag);
            }

            // Add an array divider correctness tag
            Tag divisionCorrectnessTag = CheckArrayDivisionCorrectness(array);
            tags.Add(divisionCorrectnessTag);

            Logger.Instance.WriteToLog("Tag added: " + divisionCorrectnessTag.TagType.Name + " -> " + divisionCorrectnessTag.Value[0].Value);

            // Add tags for the number of horizontal and vertical divisions
            Tag horizDivsTag = new Tag(Tag.Origins.Generated, ArrayHorizontalDivisionsTagType.Instance);
            int horizRegions = array.HorizontalDivisions.Count == 0 ? 1 : array.HorizontalDivisions.Count;
            horizDivsTag.Value.Add(new TagOptionValue(horizRegions.ToString() + " region" + (horizRegions == 1 ? "" : "s")));
            tags.Add(horizDivsTag);

            Tag vertDivsTag = new Tag(Tag.Origins.Generated, ArrayVerticalDivisionsTagType.Instance);
            int vertRegions = array.VerticalDivisions.Count == 0 ? 1 : array.VerticalDivisions.Count;
            vertDivsTag.Value.Add(new TagOptionValue(vertRegions.ToString() + " region" + (horizRegions == 1 ? "" : "s")));
            tags.Add(vertDivsTag);

            Logger.Instance.WriteToLog("Tag added: " + horizDivsTag.TagType.Name + " -> " + horizDivsTag.Value[0].Value);
            Logger.Instance.WriteToLog("Tag added: " + vertDivsTag.TagType.Name + " -> " + vertDivsTag.Value[0].Value);
        }

        // Get the (sorted) list of subdivisions of startingValue, using the place value strategy
        private static List<int> PlaceValueStrategyDivisions(int startingValue)
        {
            int currentValue = startingValue;
            int currentPlace = 1;
            List<int> output = new List<int>();
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
            int currentValue = startingValue;
            List<int> output = new List<int>();
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
            List<int> output = new List<int>();
            output.Add(startingValue / 2);
            output.Add(startingValue / 2);

            return output;
        }

        private static Tag CheckArrayDivisionCorrectness(CLPArray array)
        {
            Tag tag = new Tag(Tag.Origins.Generated, ArrayDivisionCorrectnessTagType.Instance);

            bool horizUnfinished = false;
            bool vertUnfinished = false;

            // First check horizontal divisions
            if(array.HorizontalDivisions.Count > 0)
            {
                int sum = 0;
                foreach(CLPArrayDivision div in array.HorizontalDivisions)
                {
                    if(div.Value == 0)
                    {
                        horizUnfinished = true;
                    }
                    sum += div.Value;
                }
                if(!horizUnfinished && sum != array.Rows)
                {
                    tag.AddTagOptionValue(new TagOptionValue("Incorrect"));
                    return tag;
                }
            }

            // Then check vertical divisions
            if(array.VerticalDivisions.Count > 0)
            {
                int sum = 0;
                foreach(CLPArrayDivision div in array.VerticalDivisions)
                {
                    if(div.Value == 0)
                    {
                        vertUnfinished = true;
                    }
                    sum += div.Value;
                }
                if(!vertUnfinished && sum != array.Columns)
                {
                    tag.AddTagOptionValue(new TagOptionValue("Incorrect"));
                    return tag;
                }
            }

            if(horizUnfinished || vertUnfinished)
            {
                tag.AddTagOptionValue(new TagOptionValue("Unfinished"));
                return tag;
            }
            else
            {
                tag.AddTagOptionValue(new TagOptionValue("Correct"));
                return tag;
            }
        }

        public static bool FFCUsedOnesStrategy(List<int> divs)
        {
            foreach(int div in divs)
            {
                if(div > 1)
                {
                    return false;
                }
            }
            return (divs.First() == 1);
        }

        /// <summary>
        /// Method to invoke when the AnalyzeFuzzyFactorCardCommand command is executed.
        /// </summary>
        public static void AnalyzeFuzzyFactorCard(ICLPPage page)
        {
            Logger.Instance.WriteToLog("Start of PageAnalysis.AnalyzeFuzzyFactorCard");
            ObservableCollection<Tag> tags = page.PageTags;
            ProductRelation relation = null;
            foreach(Tag tag in tags)
            {
                if(tag.TagType.Name == PageDefinitionTagType.Instance.Name)
                {
                    relation = (ProductRelation)tag.Value[0].Value;
                    break;
                }
            }

            if(relation == null)
            {
                // No definition for the page!
                Logger.Instance.WriteToLog("No page definition found! :(");
                return;
            }


            // Find FFC object on the page (just use the first one we find), or be sad if we don't find one
            ObservableCollection<ICLPPageObject> objects = page.PageObjects;
            CLPFuzzyFactorCard ffc = null;

            foreach(ICLPPageObject pageObject in objects)
            {
                if(pageObject.GetType() == typeof(CLPFuzzyFactorCard))
                {
                    ffc = (CLPFuzzyFactorCard)pageObject;
                    break;
                }
            }

            if(ffc == null)
            {
                // No FFC on the page!
                Logger.Instance.WriteToLog("No fuzzy factor card found! :(");
                return;
            }

            //TODO Liz: make work for rotated FFCs

            // We have a page definition and an array, so we're good to go!
            Logger.Instance.WriteToLog("FuzzyFactorCard found! Product: " + ffc.Dividend + " Factor: " + ffc.Rows);

            int ffcWidth = ffc.Columns;
            int ffcHeight = ffc.Rows;
            int ffcDividend = ffc.Dividend;

            // TODO: Add handling for variables in math relation
            int factor1 = Convert.ToInt32(relation.Factor1);
            int factor2 = Convert.ToInt32(relation.Factor2);
            int product = Convert.ToInt32(relation.Product);

            // First, clear out any old FFC TagTypes
            foreach(Tag tag in tags.ToList())
            {
                if(tag.TagType == null ||
                    tag.TagType.Name == RepresentationCorrectnessTagType.Instance.Name ||
                    tag.TagType.Name == FuzzyFactorCardStrategyTagType.Instance.Name ||
                    tag.TagType.Name == FuzzyFactorCardCorrectnessTagType.Instance.Name)
                {
                    tags.Remove(tag);
                }
            }

            // Apply a representation correctness tag
            Tag correctnessTag = new Tag(Tag.Origins.Generated, RepresentationCorrectnessTagType.Instance);
            correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Other"));

            if(product == ffcDividend && ( factor1 == ffcHeight || factor2 == ffcHeight) )
            {
                correctnessTag.AddTagOptionValue(new TagOptionValue("Correct"));
            }
            else
            {
                // One more possibility: The representation uses the givens in the problem, but in the wrong way
                ObservableCollection<int> givens = new ObservableCollection<int>();
                if(relation.Factor1Given)
                {
                    givens.Add(factor1);
                }
                if(relation.Factor2Given)
                {
                    givens.Add(factor2);
                }
                if(relation.ProductGiven)
                {
                    givens.Add(product);
                }

                ObservableCollection<int> numbersUsed = new ObservableCollection<int>();
                numbersUsed.Add(ffcWidth);
                numbersUsed.Add(ffcHeight);
                numbersUsed.Add(ffcDividend);

                if(givens.Count == 2 && numbersUsed.Contains(givens[0]) && numbersUsed.Contains(givens[1]))
                {
                    correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Misused Givens"));
                }
            }

            tags.Add(correctnessTag);

            // Create a list of the divisions' values
            List<int> divs = new List<int>();
            foreach(CLPArrayDivision div in ffc.VerticalDivisions)
            {
                divs.Add(div.Value);
            }

            // Apply a correctness tag
            Tag ffcCorrectnessTag = new Tag(Tag.Origins.Generated, FuzzyFactorCardCorrectnessTagType.Instance);

            if(ffc.VerticalDivisions.Count < 2)
            {
                ffcCorrectnessTag.AddTagOptionValue(new TagOptionValue("no arrays"));
            }
            else if(divs.Sum() == ffcWidth)
            {
                ffcCorrectnessTag.AddTagOptionValue(new TagOptionValue("complete"));
            }
            else
            {
                ffcCorrectnessTag.AddTagOptionValue(new TagOptionValue("not enough arrays"));
            }

            Logger.Instance.WriteToLog("Tag added: " + ffcCorrectnessTag.TagType.Name + " -> " + ffcCorrectnessTag.Value[0].Value);
            tags.Add(ffcCorrectnessTag);

            if(ffc.VerticalDivisions.Count > 0)
            {
                // Apply a strategy tag
                Tag strategyTag = new Tag(Tag.Origins.Generated, FuzzyFactorCardStrategyTagType.Instance);

                // Now check the student's divisions against known strategies
                if(ffc.VerticalDivisions.Count == 2)
                {
                    strategyTag.AddTagOptionValue(new TagOptionValue("one array"));
                }
                else if(FFCUsedOnesStrategy(divs))
                {
                    strategyTag.AddTagOptionValue(new TagOptionValue("1's"));
                }
                else if(divs.First() == 10)
                {
                    strategyTag.AddTagOptionValue(new TagOptionValue("10's"));
                }
                else if(divs.First() == 5)
                {
                    strategyTag.AddTagOptionValue(new TagOptionValue("5's"));
                }
                else if(divs.Count > 1 && divs.First() == divs.ElementAt(1))
                {
                    strategyTag.AddTagOptionValue(new TagOptionValue("repeat"));
                }
                else
                {
                    strategyTag.AddTagOptionValue(new TagOptionValue("other"));
                }

                Logger.Instance.WriteToLog("Tag added: " + strategyTag.TagType.Name + " -> " + strategyTag.Value[0].Value);
                tags.Add(strategyTag);
            }
        }

        /// <summary>
        /// Method to invoke when the AnalyzeStampsCommand command is executed.
        /// </summary>
        public static void AnalyzeStamps(ICLPPage page)
        {
            Logger.Instance.WriteToLog("Analyzing stamp grouping region...");

            ObservableCollection<Tag> tags = page.PageTags;
            ProductRelation relation = null;
            foreach(Tag tag in tags)
            {
                if(tag.TagType.Name == PageDefinitionTagType.Instance.Name)
                {
                    relation = (ProductRelation)tag.Value[0].Value;
                    break;
                }
            }

            if(relation == null)
            {
                // No definition for the page!
                Logger.Instance.WriteToLog("No page definition found! :(");
                return;
            }


            // Find an array object on the page (just use the first one we find), or be sad if we don't find one
            ObservableCollection<ICLPPageObject> objects = page.PageObjects;
            CLPGroupingRegion region = null;

            foreach(ICLPPageObject pageObject in objects)
            {
                if(pageObject.GetType() == typeof(CLPGroupingRegion))
                {
                    region = (CLPGroupingRegion)pageObject;
                    break;
                }
            }

            if(region == null)
            {
                // No CLPGroupingRegion on this page!
                Logger.Instance.WriteToLog("No grouping region found! :(");
                return;
            }

            region.DoInterpretation();
            Logger.Instance.WriteToLog("Done with stamps interpretation");

            // Now we have a list of the possible interpretations of the student's stamps
            ObservableCollection<CLPGrouping> groupings = region.Groupings;

            // Clear out any old stamp-related Tags
            foreach(Tag tag in tags.ToList())
            {
                if(tag.TagType == null ||                                           // Clear out any tags that somehow never got a TagType!
                    tag.TagType.Name == RepresentationCorrectnessTagType.Instance.Name ||
                    tag.TagType.Name == StampPartsPerStampTagType.Instance.Name ||
                    tag.TagType.Name == StampGroupingTypeTagType.Instance.Name)
                {
                    tags.Remove(tag);
                }
            }

            ObservableCollection<Tag> newTags = GetStampTags(groupings, relation);

            foreach(Tag tag in newTags)
            {
                tags.Add(tag);
            }
        }

        /// <summary>
        /// Returns an appropriate StampCorrectnessTag for the given interpretation and product relation
        /// </summary>
        private static ObservableCollection<Tag> GetStampTags(ObservableCollection<CLPGrouping> groupings, ProductRelation relation)
        {
            ObservableCollection<Tag> tags = new ObservableCollection<Tag>();
            Tag correctnessTag = new Tag(Tag.Origins.Generated, RepresentationCorrectnessTagType.Instance);
            correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Other")); // The student's work is assumed incorrect until proven correct

            Tag partsPerStampTag = new Tag(Tag.Origins.Generated, StampPartsPerStampTagType.Instance);
            Tag groupingTypeTag = new Tag(Tag.Origins.Generated, StampGroupingTypeTagType.Instance);

            foreach(CLPGrouping grouping in groupings)
            {
                if(HasEqualGroups(grouping) && grouping.Groups[0].Values.Count > 0) // If we can assume that this grouping has a homogeneous structure...
                {
                    int numGroups = grouping.Groups.Count;
                    List<ICLPPageObject> objList = grouping.Groups[0].Values.ToList()[0];
                    int objectsPerGroup = objList.Count;
                    int partsPerObject = objList[0].Parts;
                    int partsPerGroup = objectsPerGroup * partsPerObject;

                    partsPerStampTag.Value.Add(new TagOptionValue(partsPerObject.ToString() + (partsPerObject == 1 ? " part" : " parts")));
                    groupingTypeTag.AddTagOptionValue(new TagOptionValue(grouping.GroupingType));

                    // We're a little stricter about correctness if it's specifically an equal-grouping problem
                    if(relation.RelationType == ProductRelation.ProductRelationTypes.EqualGroups)
                    {
                        if(relation.Factor1.Equals(numGroups.ToString()) && relation.Factor2.Equals(partsPerGroup.ToString()))
                        {
                            correctnessTag.AddTagOptionValue(new TagOptionValue("Correct"));
                            tags.Add(partsPerStampTag);
                            tags.Add(groupingTypeTag);
                            break;
                        }
                        else
                        {
                            if(relation.Factor2.Equals(numGroups.ToString()) && relation.Factor1.Equals(partsPerGroup.ToString()))
                            {
                                correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Swapped Factors"));
                                tags.Add(partsPerStampTag);
                                tags.Add(groupingTypeTag);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if((relation.Factor1.Equals(numGroups.ToString()) && relation.Factor2.Equals(partsPerGroup.ToString())) ||
                            (relation.Factor2.Equals(numGroups.ToString()) && relation.Factor1.Equals(partsPerGroup.ToString())))
                        {
                            correctnessTag.AddTagOptionValue(new TagOptionValue("Correct"));
                            tags.Add(partsPerStampTag);
                            tags.Add(groupingTypeTag);
                            break;
                        }
                    }

                    // If we haven't hit a break yet, then this isn't looking good. Check for a student using the wrong operator
                    ObservableCollection<int> givens = new ObservableCollection<int>();
                    if(relation.Factor1Given) { givens.Add(Convert.ToInt32(relation.Factor1)); }
                    if(relation.Factor2Given) { givens.Add(Convert.ToInt32(relation.Factor2)); }
                    if(relation.ProductGiven) { givens.Add(Convert.ToInt32(relation.Product)); }

                    ObservableCollection<int> numbersUsed = new ObservableCollection<int>();
                    numbersUsed.Add(numGroups);
                    numbersUsed.Add(partsPerGroup);
                    numbersUsed.Add(numGroups * partsPerGroup);

                    if(givens.Count == 2 && numbersUsed.Contains(givens[0]) && numbersUsed.Contains(givens[1]))
                    {
                        correctnessTag.AddTagOptionValue(new TagOptionValue("Error: Misused Givens"));
                        tags.Add(partsPerStampTag);
                        tags.Add(groupingTypeTag);
                        break;
                    }
                }
            }
            tags.Add(correctnessTag); // A correctness tag always gets added
            return tags;
        }

        /// <summary>
        /// Checks a grouping for overall homogeneity.
        /// Returns true iff each subgroup in this grouping contains the same number of page objects, each of which has the same number of parts
        /// </summary>
        private static Boolean HasEqualGroups(CLPGrouping grouping)
        {
            int expectedPartsPerObject = -1;
            int expectedObjectsPerGroup = -1;

            foreach(Dictionary<string, List<ICLPPageObject>> groupEntry in grouping.Groups)
            {
                foreach(KeyValuePair<string, List<ICLPPageObject>> kvp in groupEntry)
                {
                    List<ICLPPageObject> objList = kvp.Value;
                    if(((CLPStampCopy)(objList[0])).IsCollectionCopy) { continue; } // Ignore collection stamps

                    int objectsInGroup = objList.Count;
                    if(expectedObjectsPerGroup == -1) { expectedObjectsPerGroup = objectsInGroup; }
                    else if(expectedObjectsPerGroup != objectsInGroup) {
                        return false;
                    }

                    foreach(ICLPPageObject pageObj in objList)
                    {
                        int parts = pageObj.Parts;
                        if(expectedPartsPerObject == -1) { expectedPartsPerObject = parts; }
                        else if(expectedPartsPerObject != parts) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

    }
}
