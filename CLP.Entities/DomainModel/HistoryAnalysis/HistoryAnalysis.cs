using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel.Collections;

namespace CLP.Entities
{
    public static class HistoryAnalysis
    {
        public static void BoxConversions(CLPPage page)
        {
            CLPTextBox textBox;
            IPageObject pageObjectToAdd = null;
            ITag relationDefinitionToAdd = null;
            var interpretationRegion = new InterpretationRegion(page)
                                       {
                                           CreatorID = "AUTHOR0000000000000000",
                                           OwnerID = "AUTHOR0000000000000000",
                                           VersionIndex = page.VersionIndex
                                       };
            interpretationRegion.Interpreters.Add(Interpreters.Handwriting);

            var problemTypes = new List<ProblemTypes>();

            switch (page.ID)
            {
                case "D2Op0HfG10aoQtL2W0BX6Q": // Page 1
                    break;
                case "-zOauyypbEmgpo3f_dalNA": // Page 2
                {
                    textBox = page.PageObjects.First(p => p.ID == "lpIezx13R0-fHaXnVqgT6A") as CLPTextBox;
                    textBox.TextContext = TextContexts.NonWordProblem;
                    problemTypes.Add(ProblemTypes.NonWordProblem);
                    problemTypes.Add(ProblemTypes.Division);
                    break;
                }
                case "UvLXlXlpCEuLF1309g5zPA": // Page 3
                {
                    textBox = page.PageObjects.First(p => p.ID == "LZlupX4OskOkxC-VQv1pKg") as CLPTextBox;
                    textBox.TextContext = TextContexts.NonWordProblem;
                    problemTypes.Add(ProblemTypes.NonWordProblem);
                    problemTypes.Add(ProblemTypes.Multiplication);
                    break;
                }
                case "526u6U8sQUqjFkCXTJZYiA": // Page 4
                {
                    textBox = page.PageObjects.First(p => p.ID == "DvQf2cvBkU-WFEFmLBEuoA") as CLPTextBox;
                    textBox.TextContext = TextContexts.NonWordProblem;
                    problemTypes.Add(ProblemTypes.NonWordProblem);
                    problemTypes.Add(ProblemTypes.Multiplication);
                    break;
                }
                case "y-wako1KCk6Aurwrn5QbVg": // Page 5
                {
                    textBox = page.PageObjects.First(p => p.ID == "JsuHVsdb6k2zYQGS8HdeJA") as CLPTextBox;
                    textBox.TextContext = TextContexts.WordProblem;
                    problemTypes.Add(ProblemTypes.WordProblem);
                    problemTypes.Add(ProblemTypes.Multiplication);
                    relationDefinitionToAdd = new MultiplicationRelationDefinitionTag(page, Origin.Author)
                    {
                        ID = "l-WC1c1mGkukYDgVm937KQ",
                        OwnerID = Person.Author.ID,
                        LastVersionIndex = page.LastVersionIndex,
                        VersionIndex = page.VersionIndex,
                        Product = 64,
                        RelationType = MultiplicationRelationDefinitionTag.RelationTypes.GeneralMultiplication
                    };

                    var firstFactor = new NumericValueDefinitionTag(page, Origin.Author)
                    {
                        ID = "8L-RIJBn_06lpKH4yBlOzg",
                        OwnerID = Person.Author.ID,
                        LastVersionIndex = page.LastVersionIndex,
                        VersionIndex = page.VersionIndex,
                        NumericValue = 8
                    };

                    var secondFactor = new NumericValueDefinitionTag(page, Origin.Author)
                    {
                        ID = "dk5lXzsvu0GHmpGgwoh-vA",
                        OwnerID = Person.Author.ID,
                        LastVersionIndex = page.LastVersionIndex,
                        VersionIndex = page.VersionIndex,
                        NumericValue = 8
                    };

                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Clear();
                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Add(firstFactor);
                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Add(secondFactor);


                    if (page.Owner.FullName.Contains("John"))
                    {
                        var tag = new NumberLineRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, "AcYvs_jEXEW9sXBGZ4euyA", 0, 64, 1, Correctness.Correct);
                        page.AddTag(tag);

                        var tag2 = new NumberLineJumpEraseTag(page, Origin.StudentPageGenerated);
                        page.AddTag(tag2);
                    }
                    break;
                }
                case "_024ibxTi0qlw4gzCD7QXA": // Page 6
                {
                    textBox = page.PageObjects.First(p => p.ID == "3A0ABSEEdUa487Mkvp9CcQ") as CLPTextBox;
                    textBox.TextContext = TextContexts.WordProblem;
                    problemTypes.Add(ProblemTypes.WordProblem);
                    problemTypes.Add(ProblemTypes.Division);
                    relationDefinitionToAdd = new DivisionRelationDefinitionTag(page, Origin.Author)
                                              {
                                                  ID = "U18DjuOfc0WJ7OIDClEC3A",
                                                  OwnerID = Person.Author.ID,
                                                  LastVersionIndex = page.LastVersionIndex,
                                                  VersionIndex = page.VersionIndex,
                                                  Dividend = 56,
                                                  Divisor = 7,
                                                  Quotient = 8,
                                                  Remainder = 0,
                                                  RelationType = DivisionRelationDefinitionTag.RelationTypes.GeneralDivision
                                              };

                    if (page.Owner.FullName.Contains("John"))
                    {
                        var tag = new NumberLineRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, "OqQXQpO4ukq5IGg5Zyo6ig", 0, 56, 1, Correctness.Correct);
                        page.AddTag(tag);
                    }
                    break;
                }
                case "_ctKrAO-MEK-g9PtqpFzVQ": // Page 7
                {
                    textBox = page.PageObjects.First(p => p.ID == "bC1g8LJ6okmsezeVSRub4A") as CLPTextBox;
                    textBox.TextContext = TextContexts.NonWordProblem;
                    problemTypes.Add(ProblemTypes.NonWordProblem);
                    problemTypes.Add(ProblemTypes.Multiplication);
                    problemTypes.Add(ProblemTypes.Division);
                    interpretationRegion.ID = "_ctKrAO-MEK-g9PtqpFmoo";
                    interpretationRegion.XPosition = 235.3954;
                    interpretationRegion.YPosition = 220.9490;
                    interpretationRegion.Height = 80;
                    interpretationRegion.Width = 80;
                    pageObjectToAdd = interpretationRegion;
                    break;
                }
                case "gdruAzwX6kWe2k-etZ6gcQ": // Page 8
                {
                    textBox = page.PageObjects.First(p => p.ID == "_0qgnvZ1EkyYgEU49l5dNw") as CLPTextBox;
                    textBox.TextContext = TextContexts.NonWordProblem;
                    problemTypes.Add(ProblemTypes.NonWordProblem);
                    problemTypes.Add(ProblemTypes.Multiplication);
                    interpretationRegion.ID = "gdruAzwX6kWe2k-etZ6moo";
                    interpretationRegion.XPosition = 253.1625;
                    interpretationRegion.YPosition = 221.9357;
                    interpretationRegion.Height = 80;
                    interpretationRegion.Width = 80;
                    pageObjectToAdd = interpretationRegion;
                    break;
                }
                case "yzvpdIROIEOFrndOASGjvA": // Page 9
                {
                    textBox = page.PageObjects.First(p => p.ID == "DisblHoHakqYkPzMu9_bxQ") as CLPTextBox;
                    textBox.TextContext = TextContexts.NonWordProblem;
                    problemTypes.Add(ProblemTypes.NonWordProblem);
                    problemTypes.Add(ProblemTypes.Multiplication);
                    problemTypes.Add(ProblemTypes.Division);
                    interpretationRegion.ID = "yzvpdIROIEOFrndOASGmoo";
                    interpretationRegion.XPosition = 106.74036;
                    interpretationRegion.YPosition = 223.4880;
                    interpretationRegion.Height = 80;
                    interpretationRegion.Width = 80;
                    pageObjectToAdd = interpretationRegion;
                    break;
                }
                case "gsQu4sdxVEKGZsgCD_zfWQ": // Page 10
                {
                    textBox = page.PageObjects.First(p => p.ID == "GBXW7G0YmEKXQ4Q_MMIn5g") as CLPTextBox;
                    textBox.TextContext = TextContexts.WordProblem;
                    problemTypes.Add(ProblemTypes.WordProblem);
                    problemTypes.Add(ProblemTypes.Multiplication);
                    interpretationRegion.ID = "gsQu4sdxVEKGZsgCD_zmoo";
                    interpretationRegion.XPosition = 98.90192;
                    interpretationRegion.YPosition = 205.11349;
                    interpretationRegion.Height = 102.1971;
                    interpretationRegion.Width = 171.0040;
                    pageObjectToAdd = interpretationRegion;
                    relationDefinitionToAdd = new MultiplicationRelationDefinitionTag(page, Origin.Author)
                    {
                        ID = "ZipMYNwixkq61bBN2_HD5g",
                        OwnerID = Person.Author.ID,
                        LastVersionIndex = page.LastVersionIndex,
                        VersionIndex = page.VersionIndex,
                        Product = 28,
                        RelationType = MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups
                    };

                    var firstFactor = new NumericValueDefinitionTag(page, Origin.Author)
                    {
                        ID = "WK6bKs_ByUCH8BOWgWxgUA",
                        OwnerID = Person.Author.ID,
                        LastVersionIndex = page.LastVersionIndex,
                        VersionIndex = page.VersionIndex,
                        NumericValue = 4
                    };

                    var secondFactor = new NumericValueDefinitionTag(page, Origin.Author)
                    {
                        ID = "2Ra04Eclg06aMGDGKtZ6fQ",
                        OwnerID = Person.Author.ID,
                        LastVersionIndex = page.LastVersionIndex,
                        VersionIndex = page.VersionIndex,
                        NumericValue = 7
                    };

                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Clear();
                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Add(firstFactor);
                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Add(secondFactor);
                    break;
                }
                case "MtZusuAFZEOqTr8KRlFlMA": // Page 11
                {
                    textBox = page.PageObjects.First(p => p.ID == "oiQrn_vQbUOsbzWYtNIejA") as CLPTextBox;
                    textBox.TextContext = TextContexts.WordProblem;
                    problemTypes.Add(ProblemTypes.WordProblem);
                    problemTypes.Add(ProblemTypes.Division);
                    interpretationRegion.ID = "MtZusuAFZEOqTr8KRlFmoo";
                    interpretationRegion.XPosition = 103.60754;
                    interpretationRegion.YPosition = 243.3032;
                    interpretationRegion.Height = 93.2830;
                    interpretationRegion.Width = 146.4150;
                    pageObjectToAdd = interpretationRegion;
                    relationDefinitionToAdd = new DivisionRelationDefinitionTag(page, Origin.Author)
                                              {
                                                  ID = "J8Sflc0rWEyodHSiD6BOoQ",
                                                  OwnerID = Person.Author.ID,
                                                  LastVersionIndex = page.LastVersionIndex,
                                                  VersionIndex = page.VersionIndex,
                                                  Dividend = 48,
                                                  Divisor = 8,
                                                  Quotient = 6,
                                                  Remainder = 0,
                                                  RelationType = DivisionRelationDefinitionTag.RelationTypes.GeneralDivision
                                              };
                    break;
                }
                case "QHJ7pFHY3ECr8u6bSFRCkA": // Page 12
                {
                    textBox = page.PageObjects.First(p => p.ID == "JleS1FBQiEGyoe4VseiPMA") as CLPTextBox;
                    textBox.TextContext = TextContexts.WordProblem;
                    problemTypes.Add(ProblemTypes.WordProblem);
                    problemTypes.Add(ProblemTypes.Multiplication);
                    interpretationRegion.ID = "QHJ7pFHY3ECr8u6bSFRmoo";
                    interpretationRegion.XPosition = 234.6666;
                    interpretationRegion.YPosition = 668.9809;
                    interpretationRegion.Height = 116.3069;
                    interpretationRegion.Width = 108.3371;
                    pageObjectToAdd = interpretationRegion;
                    relationDefinitionToAdd = new AdditionRelationDefinitionTag(page, Origin.Author)
                                              {
                                                  ID = "TxyRU2oIuUek0hmqfV3wSQ",
                                                  OwnerID = Person.Author.ID,
                                                  LastVersionIndex = page.LastVersionIndex,
                                                  VersionIndex = page.VersionIndex,
                                                  Sum = 72,
                                                  RelationType = AdditionRelationDefinitionTag.RelationTypes.GeneralAddition
                                              };

                    var firstPart = new MultiplicationRelationDefinitionTag(page, Origin.Author)
                                    {
                                        ID = "jzGI6KOkTUCr1PEohXIAtQ",
                                        OwnerID = Person.Author.ID,
                                        LastVersionIndex = page.LastVersionIndex,
                                        VersionIndex = page.VersionIndex,
                                        Product = 32,
                                        RelationType = MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups
                                    };

                    var firstFactor = new NumericValueDefinitionTag(page, Origin.Author)
                                      {
                                          ID = "pY-jWRet_UyIeMSD6rpfzw",
                                          OwnerID = Person.Author.ID,
                                          LastVersionIndex = page.LastVersionIndex,
                                          VersionIndex = page.VersionIndex,
                                          NumericValue = 4
                                      };

                    var secondFactor = new NumericValueDefinitionTag(page, Origin.Author)
                                       {
                                           ID = "xppeMKzxQ06UVwgBg0sR8Q",
                                           OwnerID = Person.Author.ID,
                                           LastVersionIndex = page.LastVersionIndex,
                                           VersionIndex = page.VersionIndex,
                                           NumericValue = 8
                                       };

                    firstPart.Factors.Clear();
                    firstPart.Factors.Add(firstFactor);
                    firstPart.Factors.Add(secondFactor);

                    var secondPart = new MultiplicationRelationDefinitionTag(page, Origin.Author)
                                     {
                                         ID = "FoHyUvBjI0ONc8TF7vRmkw",
                                         OwnerID = Person.Author.ID,
                                         LastVersionIndex = page.LastVersionIndex,
                                         VersionIndex = page.VersionIndex,
                                         Product = 40,
                                         RelationType = MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups
                                     };

                    firstFactor = new NumericValueDefinitionTag(page, Origin.Author)
                                  {
                                      ID = "usHRHdsaiEao5KY8oxv-9g",
                                      OwnerID = Person.Author.ID,
                                      LastVersionIndex = page.LastVersionIndex,
                                      VersionIndex = page.VersionIndex,
                                      NumericValue = 5
                                  };

                    secondFactor = new NumericValueDefinitionTag(page, Origin.Author)
                                   {
                                       ID = "KMsWhQClX0KM0-vT_SvFOw",
                                       OwnerID = Person.Author.ID,
                                       LastVersionIndex = page.LastVersionIndex,
                                       VersionIndex = page.VersionIndex,
                                       NumericValue = 8
                                   };

                    secondPart.Factors.Clear();
                    secondPart.Factors.Add(firstFactor);
                    secondPart.Factors.Add(secondFactor);

                    ((AdditionRelationDefinitionTag)relationDefinitionToAdd).Addends.Clear();
                    ((AdditionRelationDefinitionTag)relationDefinitionToAdd).Addends.Add(firstPart);
                    ((AdditionRelationDefinitionTag)relationDefinitionToAdd).Addends.Add(secondPart);


                    if (page.Owner.FullName.Contains("Julia"))
                    {
                        var tag1 = new ArrayRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Correct, new List<IHistoryAction>());
                        page.AddTag(tag1);

                        var tag2 = new ArrayRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Correct, new List<IHistoryAction> { new HistoryAction()});
                        page.AddTag(tag2);
                    }
                    break;
                }
                case "cgXYlAbAM0GGy8iBI4tyGw": // Page 13
                {
                    textBox = page.PageObjects.First(p => p.ID == "SNY1QJrMUUqUeK3hCIDDRA") as CLPTextBox;
                    textBox.TextContext = TextContexts.WordProblem;
                    problemTypes.Add(ProblemTypes.WordProblem);
                    problemTypes.Add(ProblemTypes.Multiplication);
                    interpretationRegion.ID = "cgXYlAbAM0GGy8iBI4tmoo";
                    interpretationRegion.XPosition = 240.3143;
                    interpretationRegion.YPosition = 661.8379;
                    interpretationRegion.Height = 131.1860;
                    interpretationRegion.Width = 103.4241;
                    pageObjectToAdd = interpretationRegion;
                    relationDefinitionToAdd = new AdditionRelationDefinitionTag(page, Origin.Author)
                                              {
                                                  ID = "qey_Bae27kmq42CLfvUg1Q",
                                                  OwnerID = Person.Author.ID,
                                                  LastVersionIndex = page.LastVersionIndex,
                                                  VersionIndex = page.VersionIndex,
                                                  Sum = 86,
                                                  RelationType = AdditionRelationDefinitionTag.RelationTypes.GeneralAddition
                                              };

                    var firstPart = new MultiplicationRelationDefinitionTag(page, Origin.Author)
                                    {
                                        ID = "grr6c_grIEWYK8dsuRqtvA",
                                        OwnerID = Person.Author.ID,
                                        LastVersionIndex = page.LastVersionIndex,
                                        VersionIndex = page.VersionIndex,
                                        Product = 32,
                                        RelationType = MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups
                                    };

                    var firstFactor = new NumericValueDefinitionTag(page, Origin.Author)
                                      {
                                          ID = "ZNl4KqUKgkytz4c1m0ehYw",
                                          OwnerID = Person.Author.ID,
                                          LastVersionIndex = page.LastVersionIndex,
                                          VersionIndex = page.VersionIndex,
                                          NumericValue = 4
                                      };

                    var secondFactor = new NumericValueDefinitionTag(page, Origin.Author)
                                       {
                                           ID = "9MPttARngE24S-vrhnnnHQ",
                                           OwnerID = Person.Author.ID,
                                           LastVersionIndex = page.LastVersionIndex,
                                           VersionIndex = page.VersionIndex,
                                           NumericValue = 8
                                       };

                    firstPart.Factors.Clear();
                    firstPart.Factors.Add(firstFactor);
                    firstPart.Factors.Add(secondFactor);

                    var secondPart = new MultiplicationRelationDefinitionTag(page, Origin.Author)
                                     {
                                         ID = "ayeqY8cbIEyJ8h7_4VM70Q",
                                         OwnerID = Person.Author.ID,
                                         LastVersionIndex = page.LastVersionIndex,
                                         VersionIndex = page.VersionIndex,
                                         Product = 54,
                                         RelationType = MultiplicationRelationDefinitionTag.RelationTypes.OrderedEqualGroups
                                     };

                    firstFactor = new NumericValueDefinitionTag(page, Origin.Author)
                                  {
                                      ID = "BLEYzh9iekaPZAC-09sPyg",
                                      OwnerID = Person.Author.ID,
                                      LastVersionIndex = page.LastVersionIndex,
                                      VersionIndex = page.VersionIndex,
                                      NumericValue = 9
                                  };

                    secondFactor = new NumericValueDefinitionTag(page, Origin.Author)
                                   {
                                       ID = "sTEDno-Uk0uxt3TupBNiYA",
                                       OwnerID = Person.Author.ID,
                                       LastVersionIndex = page.LastVersionIndex,
                                       VersionIndex = page.VersionIndex,
                                       NumericValue = 6
                                   };

                    secondPart.Factors.Clear();
                    secondPart.Factors.Add(firstFactor);
                    secondPart.Factors.Add(secondFactor);

                    ((AdditionRelationDefinitionTag)relationDefinitionToAdd).Addends.Clear();
                    ((AdditionRelationDefinitionTag)relationDefinitionToAdd).Addends.Add(firstPart);
                    ((AdditionRelationDefinitionTag)relationDefinitionToAdd).Addends.Add(secondPart);
                    break;
                }
                default:
                    return;
            }

            if (relationDefinitionToAdd != null)
            {
                page.AddTag(relationDefinitionToAdd);
            }

            problemTypes = problemTypes.Distinct().ToList();
            if (problemTypes.Any())
            {
                var problemTypesTag = new ProblemTypeTag(page, Origin.Author, problemTypes);
                page.AddTag(problemTypesTag);
            }

            if (pageObjectToAdd == null ||
                page.PageObjects.Any(p => p.ID == pageObjectToAdd.ID))
            {
                return;
            }

            page.PageObjects.Add(pageObjectToAdd);
        }

        public static void GenerateHistoryActions(CLPPage page)
        {
            BoxConversions(page);

            HistoryAction.CurrentIncrementID.Clear();
            HistoryAction.MaxIncrementID.Clear();
            page.History.HistoryActions.Clear();

            var desktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileDirectory = Path.Combine(desktopDirectory, "HistoryActions");
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            var filePath = Path.Combine(fileDirectory, PageNameComposite.ParsePage(page).ToFileName() + ".txt");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, "");
            File.AppendAllText(filePath, "*****Coded Actions/Steps*****" + "\n\n");

            // First Pass
            page.History.HistoryActions.Add(new HistoryAction(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "1"
                                            });
            var initialHistoryActions = GenerateInitialHistoryActions(page);
            page.History.HistoryActions.AddRange(initialHistoryActions);
            
            File.AppendAllText(filePath, "PASS [1]" + "\n");
            foreach (var item in initialHistoryActions)
            {
                var semi = item == initialHistoryActions.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, item.CodedValue + semi);
            }

            // Second Pass
            page.History.HistoryActions.Add(new HistoryAction(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "2"
                                            });
            var refinedInkHistoryActions = RefineInkHistoryActions(page, initialHistoryActions);
            page.History.HistoryActions.AddRange(refinedInkHistoryActions);

            File.AppendAllText(filePath, "\nPASS [2]" + "\n");
            foreach (var item in refinedInkHistoryActions)
            {
                var semi = item == refinedInkHistoryActions.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, item.CodedValue + semi);
            }

            // Third Pass
            page.History.HistoryActions.Add(new HistoryAction(page, new List<IHistoryItem>())
                                            {
                                                CodedObject = "PASS",
                                                CodedObjectID = "3"
                                            });
            var interpretedHistoryActions = InterpretHistoryActions(page, refinedInkHistoryActions);
            page.History.HistoryActions.AddRange(interpretedHistoryActions);

            File.AppendAllText(filePath, "\nPASS [3]" + "\n");
            foreach (var item in interpretedHistoryActions)
            {
                var semi = item == interpretedHistoryActions.Last() ? string.Empty : "; ";
                File.AppendAllText(filePath, item.CodedValue + semi);
            }

            // Last Pass
            GenerateTags(page, interpretedHistoryActions);

            File.AppendAllText(filePath, "\n\n\n*****Tags*****" + "\n\n");
            foreach (var tag in page.Tags)
            {
                File.AppendAllText(filePath, "*" + tag.FormattedName + "*\n");
                File.AppendAllText(filePath, tag.FormattedValue + "\n\n");
            }

            File.AppendAllText(filePath, "\n*****History Items*****" + "\n\n");
            foreach (var historyItem in page.History.CompleteOrderedHistoryItems)
            {
                File.AppendAllText(filePath, historyItem.FormattedValue + "\n");
            }
        }

        #region First Pass: Initialization

        public static List<IHistoryAction> GenerateInitialHistoryActions(CLPPage page)
        {
            var historyItemBuffer = new List<IHistoryItem>();
            var initialHistoryActions = new List<IHistoryAction>();
            var historyItems = page.History.CompleteOrderedHistoryItems;

            for (var i = 0; i < historyItems.Count; i++)
            {
                var currentHistoryItem = historyItems[i];
                historyItemBuffer.Add(currentHistoryItem);
                if (historyItemBuffer.Count == 1)
                {
                    var singleHistoryAction = VerifyAndGenerateSingleItemAction(page, historyItemBuffer.First());
                    if (singleHistoryAction != null)
                    {
                        initialHistoryActions.Add(singleHistoryAction);
                        historyItemBuffer.Clear();
                        continue;
                    }
                }

                var nextHistoryItem = i + 1 < historyItems.Count ? historyItems[i + 1] : null;
                var compoundHistoryAction = VerifyAndGenerateCompoundItemAction(page, historyItemBuffer, nextHistoryItem);
                if (compoundHistoryAction != null)
                {
                    initialHistoryActions.Add(compoundHistoryAction);
                    historyItemBuffer.Clear();
                }
            }

            return initialHistoryActions;
        }

        public static IHistoryAction VerifyAndGenerateSingleItemAction(CLPPage page, IHistoryItem historyItem)
        {
            if (historyItem == null)
            {
                return null;
            }

            IHistoryAction historyAction = null;
            TypeSwitch.On(historyItem).Case<ObjectsOnPageChangedHistoryItem>(h =>
                                                                             {
                                                                                 // HACK: Temporarily in place until MC Boxes are re-written and converted.
                                                                                 if (h.IsUsingPageObjects ||
                                                                                     !h.IsUsingStrokes)
                                                                                 {
                                                                                     historyAction = ObjectCodedActions.Add(page, h) ?? ObjectCodedActions.Delete(page, h);
                                                                                         // HACK: This is the only line that should be in here.
                                                                                     return;
                                                                                 }

                                                                                 var strokes = h.StrokesAdded;
                                                                                 var isInkAdd = true;
                                                                                 if (!strokes.Any())
                                                                                 {
                                                                                     strokes = h.StrokesRemoved;
                                                                                     isInkAdd = false;
                                                                                 }

                                                                                 var stroke = strokes.FirstOrDefault();
                                                                                 if (stroke == null)
                                                                                 {
                                                                                     return;
                                                                                 }

                                                                                 var pageObjectsOnPage = ObjectCodedActions.GetPageObjectsOnPageAtHistoryIndex(page, h.HistoryIndex, true);
                                                                                 var currentPageObjectReference = InkCodedActions.FindMostOverlappedPageObjectAtHistoryIndex(page,
                                                                                                                                                                             pageObjectsOnPage,
                                                                                                                                                                             stroke,
                                                                                                                                                                             h.HistoryIndex);
                                                                                 var multipleChoiceBox = currentPageObjectReference as MultipleChoiceBox;
                                                                                 if (multipleChoiceBox == null)
                                                                                 {
                                                                                     return;
                                                                                 }

                                                                                 CurrentMultipleChoiceBoxStrokes = new List<Stroke>();
                                                                                 CurrentMostFilledBubbleIndex = -1;
                                                                                 LastMarkedBubbleIndex = -1;

                                                                                 MultipleChoiceBubble mostFilledBubble = null;
                                                                                 var previousStrokeLength = 0;
                                                                                 var indexOfBubbleCurrentStrokeIsOver = -1;
                                                                                 foreach (var multipleChoiceBubble in multipleChoiceBox.ChoiceBubbles)
                                                                                 {
                                                                                     multipleChoiceBubble.IsMarked = false;

                                                                                     var bubbleBoundary = new Rect(multipleChoiceBox.XPosition + multipleChoiceBubble.ChoiceBubbleIndex * multipleChoiceBox.ChoiceBubbleGapLength, multipleChoiceBox.YPosition, multipleChoiceBox.ChoiceBubbleDiameter, multipleChoiceBox.ChoiceBubbleDiameter);
                                                                                     var isStrokeOverBubble = stroke.HitTest(bubbleBoundary, 80);
                                                                                     if (isStrokeOverBubble)
                                                                                     {
                                                                                         indexOfBubbleCurrentStrokeIsOver = multipleChoiceBubble.ChoiceBubbleIndex;
                                                                                         if (isInkAdd)
                                                                                         {
                                                                                             CurrentMultipleChoiceBoxStrokes.Add(stroke);
                                                                                         }
                                                                                         else
                                                                                         {
                                                                                             CurrentMultipleChoiceBoxStrokes.RemoveAll(s => s.GetStrokeID() == stroke.GetStrokeID());
                                                                                         }
                                                                                     }

                                                                                     var strokesOverBubble = CurrentMultipleChoiceBoxStrokes.Where(s => s.HitTest(bubbleBoundary, 80));

                                                                                     var totalStrokeLength = strokesOverBubble.Sum(s => s.StylusPoints.Count);
                                                                                     if (totalStrokeLength <= previousStrokeLength ||
                                                                                         totalStrokeLength <= 100)
                                                                                     {
                                                                                         continue;
                                                                                     }

                                                                                     mostFilledBubble = multipleChoiceBubble;
                                                                                     previousStrokeLength = totalStrokeLength;
                                                                                 }

                                                                                 if (indexOfBubbleCurrentStrokeIsOver == -1)
                                                                                 {
                                                                                     return;
                                                                                 }

                                                                                 var correctBubble = multipleChoiceBox.ChoiceBubbles.FirstOrDefault(c => c.IsACorrectValue);
                                                                                 var currentBubble = multipleChoiceBox.ChoiceBubbles.FirstOrDefault(c => c.ChoiceBubbleIndex == indexOfBubbleCurrentStrokeIsOver);
                                                                                 if (correctBubble == null ||
                                                                                     currentBubble == null)
                                                                                 {
                                                                                     Console.WriteLine("ERROR, no correct bubble marked");
                                                                                     return;
                                                                                 }


                                                                                 var correctness = correctBubble.ChoiceBubbleIndex == currentBubble.ChoiceBubbleIndex ? "COR" : "INC";

                                                                                 var objectAction = string.Empty;
                                                                                 if (isInkAdd)
                                                                                 {
                                                                                     if (CurrentMostFilledBubbleIndex == -1)
                                                                                     {
                                                                                         objectAction = Codings.ACTION_MULTIPLE_CHOICE_ADD;
                                                                                     }
                                                                                     else
                                                                                     {
                                                                                         if (mostFilledBubble.ChoiceBubbleIndex == indexOfBubbleCurrentStrokeIsOver)
                                                                                         {
                                                                                             if (mostFilledBubble.ChoiceBubbleIndex == CurrentMostFilledBubbleIndex)
                                                                                             {
                                                                                                 if (LastMarkedBubbleIndex == indexOfBubbleCurrentStrokeIsOver)
                                                                                                 {
                                                                                                     objectAction = Codings.ACTION_MULTIPLE_CHOICE_ADD_PARTIAL;
                                                                                                 }
                                                                                                 else
                                                                                                 {
                                                                                                     objectAction = Codings.ACTION_MULTIPLE_CHOICE_ADD_REPEAT;
                                                                                                 }
                                                                                             }
                                                                                             else
                                                                                             {
                                                                                                 objectAction = Codings.ACTION_MULTIPLE_CHOICE_ADD_CHANGE;
                                                                                             }
                                                                                         }
                                                                                         else
                                                                                         {
                                                                                             objectAction = Codings.ACTION_MULTIPLE_CHOICE_ADD_OTHER;
                                                                                         }
                                                                                     }
                                                                                 }
                                                                                 else
                                                                                 {
                                                                                     objectAction = mostFilledBubble == null
                                                                                                        ? Codings.ACTION_MULTIPLE_CHOICE_ERASE
                                                                                                        : indexOfBubbleCurrentStrokeIsOver == mostFilledBubble.ChoiceBubbleIndex
                                                                                                              ? Codings.ACTION_MULTIPLE_CHOICE_ERASE_PARTIAL
                                                                                                              : Codings.ACTION_MULTIPLE_CHOICE_ERASE_OTHER;
                                                                                 }
                                                                                 CurrentMostFilledBubbleIndex = mostFilledBubble == null ? -1 : mostFilledBubble.ChoiceBubbleIndex;
                                                                                 LastMarkedBubbleIndex = indexOfBubbleCurrentStrokeIsOver;

                                                                                 historyAction = new HistoryAction(page, h)
                                                                                 {
                                                                                     CodedObject = Codings.OBJECT_MULTIPLE_CHOICE,
                                                                                     CodedObjectAction = objectAction,
                                                                                     IsObjectActionVisible = objectAction != Codings.ACTION_MULTIPLE_CHOICE_ADD,
                                                                                     CodedObjectID = correctBubble.ChoiceBubbleLabel,
                                                                                     CodedObjectActionID = string.Format("{0}, {1}", currentBubble.ChoiceBubbleLabel, correctness)
                                                                                 };

                                                                                 
                                                                             })
                      .Case<CLPArrayRotateHistoryItem>(h => { historyAction = ArrayCodedActions.Rotate(page, h); })
                      .Case<PageObjectCutHistoryItem>(h => { historyAction = ArrayCodedActions.Cut(page, h); })
                      .Case<CLPArraySnapHistoryItem>(h => { historyAction = ArrayCodedActions.Snap(page, h); })
                      .Case<CLPArrayDivisionsChangedHistoryItem>(h => { historyAction = ArrayCodedActions.Divide(page, h); });

            return historyAction;
        }

        private static List<Stroke> CurrentMultipleChoiceBoxStrokes { get; set; }
        private static int CurrentMostFilledBubbleIndex = -1;
        private static int LastMarkedBubbleIndex = -1;

        public static IHistoryAction VerifyAndGenerateCompoundItemAction(CLPPage page, List<IHistoryItem> historyItems, IHistoryItem nextHistoryItem)
        {
            if (!historyItems.Any())
            {
                return null;
            }

            if (historyItems.All(h => h is ObjectsOnPageChangedHistoryItem))
            {
                var objectsChangedHistoryItems = historyItems.Cast<ObjectsOnPageChangedHistoryItem>().ToList();
                // TODO: Edge case that recognizes multiple bins added at once.

                if (objectsChangedHistoryItems.All(h => h.IsUsingStrokes && !h.IsUsingPageObjects))
                {
                    var nextObjectsChangedHistoryItem = nextHistoryItem as ObjectsOnPageChangedHistoryItem;
                    if (nextObjectsChangedHistoryItem != null &&
                        nextObjectsChangedHistoryItem.IsUsingStrokes &&
                        !nextObjectsChangedHistoryItem.IsUsingPageObjects)
                    {
                        // HACK: Another temp hack to recognize multiple choice box answers. Normally just return null.
                        var h = VerifyAndGenerateSingleItemAction(page, nextHistoryItem);
                        if (h == null)
                        {
                            return null;
                        }
                    }

                    var historyAction = InkCodedActions.ChangeOrIgnore(page, objectsChangedHistoryItems);
                    return historyAction;
                }
            }

            if (historyItems.All(h => h is ObjectsMovedBatchHistoryItem))
            {
                var objectsMovedHistoryItems = historyItems.Cast<ObjectsMovedBatchHistoryItem>().ToList();

                var firstIDSequence = objectsMovedHistoryItems.First().PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList();
                if (objectsMovedHistoryItems.All(h => firstIDSequence.SequenceEqual(h.PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList())))
                {
                    var nextMovedHistoryItem = nextHistoryItem as ObjectsMovedBatchHistoryItem;
                    if (nextMovedHistoryItem != null &&
                        firstIDSequence.SequenceEqual(nextMovedHistoryItem.PageObjectIDs.Keys.Distinct().OrderBy(id => id).ToList()))
                    {
                        return null;
                    }

                    var historyAction = ObjectCodedActions.Move(page, objectsMovedHistoryItems);
                    return historyAction;
                }
            }

            if (historyItems.All(h => h is NumberLineEndPointsChangedHistoryItem))
            {
                var endPointsChangedHistoryItems = historyItems.Cast<NumberLineEndPointsChangedHistoryItem>().ToList();

                var firstNumberLineID = endPointsChangedHistoryItems.First().NumberLineID;
                if (endPointsChangedHistoryItems.All(h => h.NumberLineID == firstNumberLineID))
                {
                    var nextEndPointsChangedHistoryItem = nextHistoryItem as NumberLineEndPointsChangedHistoryItem;
                    if (nextEndPointsChangedHistoryItem != null &&
                        nextEndPointsChangedHistoryItem.NumberLineID == firstNumberLineID)
                    {
                        return null;
                    }

                    var historyAction = NumberLineCodedActions.EndPointsChange(page, endPointsChangedHistoryItems);
                    return historyAction;
                }
            }

            if (historyItems.All(h => h is NumberLineJumpSizesChangedHistoryItem))
            {
                var jumpSizesChangedHistoryItems = historyItems.Cast<NumberLineJumpSizesChangedHistoryItem>().ToList();

                var firstNumberLineID = jumpSizesChangedHistoryItems.First().NumberLineID;
                var isAdding = jumpSizesChangedHistoryItems.First().JumpsAdded.Any() && !jumpSizesChangedHistoryItems.First().JumpsRemoved.Any();
                if (jumpSizesChangedHistoryItems.All(h => h.NumberLineID == firstNumberLineID))
                {
                    var nextJumpsChangedHistoryItem = nextHistoryItem as NumberLineJumpSizesChangedHistoryItem;
                    if (nextJumpsChangedHistoryItem != null &&
                        nextJumpsChangedHistoryItem.NumberLineID == firstNumberLineID &&
                        isAdding == (nextJumpsChangedHistoryItem.JumpsAdded.Any() && !nextJumpsChangedHistoryItem.JumpsRemoved.Any()))
                    {
                        return null;
                    }

                    var historyAction = NumberLineCodedActions.JumpSizesChange(page, jumpSizesChangedHistoryItems);
                    return historyAction;
                }
            }

            return null;
        }

        #endregion // First Pass: Initialization

        #region Second Pass: Ink Refinement

        // HANNAH CHANGES HERE
        public const double MAX_DISTANCE_Z_SCORE = 3.0;
        public const double DIMENSION_MULTIPLIER_THRESHOLD = 3.0;

        public static List<IHistoryAction> RefineInkHistoryActions(CLPPage page, List<IHistoryAction> historyActions)
        {
            var refinedHistoryActions = new List<IHistoryAction>();

            foreach (var historyAction in historyActions)
            {
                if (historyAction.CodedObject == Codings.OBJECT_INK &&
                    historyAction.CodedObjectAction == Codings.ACTION_INK_CHANGE)
                {
                    var refinedInkActions = ProcessInkChangeHistoryAction(page, historyAction);
                    refinedHistoryActions.AddRange(refinedInkActions);
                }
                else
                {
                    refinedHistoryActions.Add(historyAction);
                }
            }

            return refinedHistoryActions;
        }

        /// <summary>Processes "INK change" action into "INK strokes (erase) [ID: location RefObject [RefObjectID]]" actions</summary>
        /// <param name="page">Parent page the history action belongs to.</param>
        /// <param name="historyAction">"INK change" history action to process</param>
        public static List<IHistoryAction> ProcessInkChangeHistoryAction(CLPPage page, IHistoryAction historyAction)
        {
            var historyItems = historyAction.HistoryItems.Cast<ObjectsOnPageChangedHistoryItem>().OrderBy(h => h.HistoryIndex).ToList();
            var processedInkActions = new List<IHistoryAction>();
            var pageObjectsOnPage = ObjectCodedActions.GetPageObjectsOnPageAtHistoryIndex(page, historyItems.First().HistoryIndex);
            // TODO: validation

            var averageStrokeDimensions = InkCodedActions.GetAverageStrokeDimensions(page);
            var averageClosestStrokeDistance = InkCodedActions.GetAverageClosestStrokeDistance(page);
            var closestStrokeDistanceStandardDeviation = InkCodedActions.GetStandardDeviationOfClosestStrokeDistance(page);
            var rollingStatsForDistanceFromCluster = new RollingStandardDeviation();
            var historyItemBuffer = new List<IHistoryItem>();
            IPageObject currentPageObjectReference = null;
            Stroke currentStrokeReference = null;
            var currentLocationReference = Codings.ACTIONID_INK_LOCATION_NONE;
            var isInkAdd = true;
            var currentClusterCentroid = new Point(0, 0);
            var currentClusterWeight = 0.0;
            var isMatchingAgainstCluster = false;

            for (var i = 0; i < historyItems.Count; i++)
            {
                var currentHistoryItem = historyItems[i];
                historyItemBuffer.Add(currentHistoryItem);
                if (historyItemBuffer.Count == 1)
                {
                    var strokes = currentHistoryItem.StrokesAdded;
                    isInkAdd = true;
                    if (!strokes.Any())
                    {
                        strokes = currentHistoryItem.StrokesRemoved;
                        isInkAdd = false;
                    }

                    // TODO: If strokes.count != 1, deal with point erase
                    // TODO: Validation (strokes is empty)
                    currentStrokeReference = strokes.First();
                    currentPageObjectReference = InkCodedActions.FindMostOverlappedPageObjectAtHistoryIndex(page, pageObjectsOnPage, currentStrokeReference, currentHistoryItem.HistoryIndex);
                    currentLocationReference = Codings.ACTIONID_INK_LOCATION_OVER;
                    isMatchingAgainstCluster = false;
                    if (currentPageObjectReference == null)
                    {
                        isMatchingAgainstCluster = true;

                        var strokeCopy = currentStrokeReference.GetStrokeCopyAtHistoryIndex(page, currentHistoryItem.HistoryIndex);
                        currentClusterCentroid = strokeCopy.WeightedCenter();
                        currentClusterWeight = strokeCopy.StrokeWeight();

                        currentPageObjectReference = InkCodedActions.FindClosestPageObjectByPointAtHistoryIndex(page, pageObjectsOnPage, currentClusterCentroid, currentHistoryItem.HistoryIndex);
                        if (currentPageObjectReference != null)
                        {
                            currentLocationReference = InkCodedActions.FindLocationReferenceAtHistoryLocation(page, currentPageObjectReference, currentClusterCentroid, currentHistoryItem.HistoryIndex);
                        }
                    }
                }

                var nextHistoryItem = i + 1 < historyItems.Count ? historyItems[i + 1] : null;
                if (nextHistoryItem != null)
                {
                    var nextStrokes = nextHistoryItem.StrokesAdded;

                    if (!nextStrokes.Any())
                    {
                        nextStrokes = nextHistoryItem.StrokesRemoved;
                    }

                    var nextStroke = nextStrokes.First();
                    var nextPageObjectReference = InkCodedActions.FindMostOverlappedPageObjectAtHistoryIndex(page, pageObjectsOnPage, nextStroke, nextHistoryItem.HistoryIndex);
                    var nextLocationReference = Codings.ACTIONID_INK_LOCATION_OVER;
                    var isNextPartOfCurrentCluster = false;
                    if (nextPageObjectReference == null)
                    {
                        if (isMatchingAgainstCluster)
                        {
                            var currentStrokeCopy = currentStrokeReference.GetStrokeCopyAtHistoryIndex(page, currentHistoryItem.HistoryIndex);
                            var currentCentroid = currentStrokeCopy.WeightedCenter();
                            var nextStrokeCopy = nextStroke.GetStrokeCopyAtHistoryIndex(page, nextHistoryItem.HistoryIndex);
                            var nextCentroid = nextStrokeCopy.WeightedCenter();
                            var distanceFromLastStroke = Math.Sqrt(InkCodedActions.DistanceSquaredBetweenPoints(currentCentroid, nextCentroid));
                            var lastStrokeDistanceZScore = (distanceFromLastStroke - averageClosestStrokeDistance) / closestStrokeDistanceStandardDeviation;
                            var isCloseToLastStroke = lastStrokeDistanceZScore <= MAX_DISTANCE_Z_SCORE || distanceFromLastStroke <= averageStrokeDimensions.X * DIMENSION_MULTIPLIER_THRESHOLD ||
                                                      distanceFromLastStroke <= averageStrokeDimensions.Y * DIMENSION_MULTIPLIER_THRESHOLD;

                            bool isCloseToCluster;
                            var distanceFromCluster = Math.Sqrt(InkCodedActions.DistanceSquaredBetweenPoints(currentClusterCentroid, nextCentroid));
                            if (historyItemBuffer.Count == 1)
                            {
                                isCloseToCluster = isCloseToLastStroke;
                            }
                            else
                            {
                                var clusterDistanceZScore = (distanceFromCluster - rollingStatsForDistanceFromCluster.Mean) / rollingStatsForDistanceFromCluster.StandardDeviation;
                                isCloseToCluster = clusterDistanceZScore <= MAX_DISTANCE_Z_SCORE;
                            }

                            if (isCloseToLastStroke || isCloseToCluster)
                            {
                                isNextPartOfCurrentCluster = true;

                                nextPageObjectReference = InkCodedActions.FindClosestPageObjectByPointAtHistoryIndex(page, pageObjectsOnPage, currentClusterCentroid, nextHistoryItem.HistoryIndex);
                                if (nextPageObjectReference != null)
                                {
                                    nextLocationReference = InkCodedActions.FindLocationReferenceAtHistoryLocation(page, nextPageObjectReference, currentClusterCentroid, nextHistoryItem.HistoryIndex);
                                }

                                var oldClusterWeight = currentClusterWeight;
                                var nextStrokeWeight = nextStrokeCopy.StrokeWeight();
                                currentClusterWeight += nextStrokeWeight;

                                var totalImportance = oldClusterWeight / currentClusterWeight;
                                var importance = nextStrokeWeight / currentClusterWeight;
                                var weightedXAverage = (totalImportance + currentClusterCentroid.X) + (importance * nextCentroid.X);
                                var weightedYAverage = (totalImportance + currentClusterCentroid.Y) + (importance * nextCentroid.Y);
                                currentClusterCentroid = new Point(weightedXAverage, weightedYAverage);

                                rollingStatsForDistanceFromCluster.Update(distanceFromCluster);
                            }
                        }
                    }

                    var isNextInkPartOfCurrent = isInkAdd == nextHistoryItem.StrokesAdded.Any() && isInkAdd == !nextHistoryItem.StrokesRemoved.Any();
                    var isNextPageObjectReferencePartOfCurrent = nextPageObjectReference == null && currentPageObjectReference == null;
                    if (nextPageObjectReference != null &&
                        currentPageObjectReference != null)
                    {
                        isNextPageObjectReferencePartOfCurrent = nextPageObjectReference.ID == currentPageObjectReference.ID;
                    }
                    var isNextLocationReferencePartOfCurrent = nextLocationReference == currentLocationReference;
                    if (isNextInkPartOfCurrent && (isNextPartOfCurrentCluster || (currentLocationReference == Codings.ACTIONID_INK_LOCATION_OVER && isNextPageObjectReferencePartOfCurrent)))
                    {
                        continue;
                    }
                }

                var refinedHistoryAction = InkCodedActions.GroupAddOrErase(page, historyItemBuffer.Cast<ObjectsOnPageChangedHistoryItem>().ToList(), isInkAdd);
                refinedHistoryAction.CodedObjectID = "A";
                if (currentPageObjectReference != null)
                {
                    refinedHistoryAction.CodedObjectActionID = string.Format("{0} {1} [{2}]",
                                                                             currentLocationReference,
                                                                             currentPageObjectReference.CodedName,
                                                                             currentPageObjectReference.GetCodedIDAtHistoryIndex(refinedHistoryAction.HistoryItems.Last().HistoryIndex));
                }
                refinedHistoryAction.MetaData.Add("REFERENCE_PAGE_OBJECT_ID", currentPageObjectReference.ID);

                processedInkActions.Add(refinedHistoryAction);
                historyItemBuffer.Clear();
            }

            return processedInkActions;
        }

        #endregion // Second Pass: Ink Refinement

        #region Third Pass: Interpretation

        public static List<IHistoryAction> InterpretHistoryActions(CLPPage page, List<IHistoryAction> historyActions)
        {
            var allInterpretedHistoryActions = new List<IHistoryAction>();

            foreach (var historyAction in historyActions)
            {
                if (historyAction.CodedObject == Codings.OBJECT_INK)
                {
                    var interpretedHistoryActions = AttemptHistoryActionInterpretation(page, historyAction);
                    allInterpretedHistoryActions.AddRange(interpretedHistoryActions);
                }
                else
                {
                    allInterpretedHistoryActions.Add(historyAction);
                }
            }

            return allInterpretedHistoryActions;
        }

        public static List<IHistoryAction> AttemptHistoryActionInterpretation(CLPPage page, IHistoryAction historyaction)
        {
            var allInterpretedActions = new List<IHistoryAction>();

            if (historyaction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_OVER) &&
                historyaction.CodedObjectActionID.Contains(Codings.OBJECT_FILL_IN))
            {
                // HACK: discuss structure of history action

                var interpretedAction = InkCodedActions.FillInInterpretation(page, historyaction); // TODO: Potentionally needs a recursive pass through.
                if (interpretedAction != null)
                {
                    allInterpretedActions.Add(interpretedAction);
                }
            }

            if (historyaction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_OVER) &&
                historyaction.CodedObjectActionID.Contains(Codings.OBJECT_ARRAY))
            {
                var interpretedActions = ArrayCodedActions.InkDivide(page, historyaction); // TODO: Potentionally needs a recursive pass through.
                allInterpretedActions.AddRange(interpretedActions);
            }

            if (historyaction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_RIGHT) &&
                historyaction.CodedObjectActionID.Contains(Codings.OBJECT_ARRAY))
            {
                var interpretedAction = ArrayCodedActions.SkipCounting(page, historyaction);
                if (interpretedAction != null)
                {
                    allInterpretedActions.Add(interpretedAction);
                }
            }

            if (!historyaction.CodedObjectActionID.Contains(Codings.ACTIONID_INK_LOCATION_OVER))
            {
                var interpretedAction = InkCodedActions.Arithmetic(page, historyaction);
                if (interpretedAction != null)
                {
                    allInterpretedActions.Add(interpretedAction);
                }
            }

            if (!allInterpretedActions.Any())
            {
                allInterpretedActions.Add(historyaction);
            }

            return allInterpretedActions;
        }

        #endregion // Third Pass: Interpretation

        // 4th pass: simple pattern interpretations

        // 5th pass: complex pattern interpretations

        // 6th pass: Tag generation

        #region Last Pass: Tag Generation

        public static void GenerateTags(CLPPage page, List<IHistoryAction> historyActions)
        {
            AttemptAnswerBeforeRepresentationTag(page, historyActions);
            AttemptAnswerChangedAfterRepresentationTag(page, historyActions);
            AttemptAnswerTag(page, historyActions);
        }

        public static void AttemptAnswerBeforeRepresentationTag(CLPPage page, List<IHistoryAction> historyActions)
        {
            var answerActions = historyActions.Where(Codings.IsAnswerObject).ToList();
            if (answerActions.Count < 1)
            {
                return;
            }

            var firstAnswer = historyActions.First(Codings.IsAnswerObject);
            var firstIndex = historyActions.IndexOf(firstAnswer);

            var beforeActions = historyActions.Take(firstIndex + 1).ToList();
            var isUsingRepresentationsBefore = beforeActions.Any(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD);

            if (isUsingRepresentationsBefore)
            {
                return;
            }

            var afterActions = historyActions.Skip(firstIndex).ToList();
            var isUsingRepresentationsAfter = afterActions.Any(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD);

            if (!isUsingRepresentationsAfter)
            {
                return;
            }

            // TODO: Derive this entire Analysis Code from ARA Tag and don't use this Tag
            var tag = new AnswerBeforeRepresentationTag(page, Origin.StudentPageGenerated, afterActions);
            page.AddTag(tag);
        }

        public static void AttemptAnswerChangedAfterRepresentationTag(CLPPage page, List<IHistoryAction> historyActions)
        {
            var answerActions = historyActions.Where(Codings.IsAnswerObject).ToList();
            if (answerActions.Count < 2)
            {
                return;
            }

            var firstAnswer = historyActions.First(Codings.IsAnswerObject);
            var firstIndex = historyActions.IndexOf(firstAnswer);
            var lastAnswer = historyActions.Last(Codings.IsAnswerObject);
            var lastIndex = historyActions.IndexOf(lastAnswer);

            var possibleTagActions = historyActions.Skip(firstIndex).Take(lastIndex - firstIndex + 1).ToList();
            var isUsingRepresentations = possibleTagActions.Any(h => Codings.IsRepresentationObject(h) && h.CodedObjectAction == Codings.ACTION_OBJECT_ADD);

            if (!isUsingRepresentations)
            {
                return;
            }

            var tag = new AnswerChangedAfterRepresentationTag(page, Origin.StudentPageGenerated, possibleTagActions);
            page.AddTag(tag);
        }

        public static void AttemptAnswerTag(CLPPage page, List<IHistoryAction> historyActions)
        {
            var lastAnswerAction = historyActions.LastOrDefault(Codings.IsAnswerObject);
            if (lastAnswerAction == null ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_MULTIPLE_CHOICE_ERASE ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_MULTIPLE_CHOICE_ERASE_OTHER ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_MULTIPLE_CHOICE_ERASE_PARTIAL ||
                lastAnswerAction.CodedObjectAction == Codings.ACTION_FILL_IN_ERASE)
            {
                return;
            }

            var tag = new AnswerCorrectnessTag(page, Origin.StudentPageGenerated, new List<IHistoryAction> { lastAnswerAction });
            page.AddTag(tag);
        }

        #endregion // Last Pass: Tag Generation
    }
}