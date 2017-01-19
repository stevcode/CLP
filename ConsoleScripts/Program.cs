using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel.Collections;
using Catel.Data;
using CLP.Entities;
using Path = Catel.IO.Path;

namespace ConsoleScripts
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Convert();
            Debug.WriteLine("*****Finished*****");
            Console.ReadLine();
        }

        private static void Convert()
        {
            var convertFromFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Convert");
            //var convertToFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Converted");
            //if (!Directory.Exists(convertToFolderPath))
            //{
            //    Directory.CreateDirectory(convertToFolderPath);
            //}

            var notebookFolderPaths = Directory.EnumerateDirectories(convertFromFolderPath);
            foreach (var notebookFolderPath in notebookFolderPaths)
            {
                //var filePath = Path.Combine(notebookFolderPath, "notebook.xml");
                //var notebook = ModelBase.Load<Notebook>(filePath, SerializationMode.Xml);
                var pagesFolderPath = Path.Combine(notebookFolderPath, "Pages");
                var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml");
                foreach (var pageFilePath in pageFilePaths)
                {
                    var page = AEntityBase.Load<CLPPage>(pageFilePath, SerializationMode.Xml);
                    //page.AfterDeserialization();

                    Debug.WriteLine("Loaded {3}'s page {0}, differentiation {1}, version {2}", page.PageNumber, page.DifferentiationLevel, page.VersionIndex, page.Owner.FullName);
                    //Do stuff to each page here. 

                    _isConvertingEmilyCache = false;
                    _isConvertingAssessmentCache = true;
                    ReplaceMultipleChoiceBoxes(page);
                    ConvertDivisionTemplatesToUseNewRemainderTiles(page);
                    TheSlowRewind(page);

                    //Finished doing stuff to page, it'll save below.
                    page.ToXML(pageFilePath, true);
                }
            }
        }

        private static bool _isConvertingEmilyCache;
        private static bool _isConvertingAssessmentCache;

        public static void ReplaceMultipleChoiceBoxes(CLPPage page)
        {
            if (!_isConvertingAssessmentCache)
            {
                return;
            }

            var multipleChoiceBox = page.PageObjects.FirstOrDefault(p => p is MultipleChoiceBox);
            if (multipleChoiceBox != null)
            {
                page.PageObjects.Remove(multipleChoiceBox);
            }

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

                    var textBoxToRemove = page.PageObjects.First(p => p.ID == "hsHhMK1dM0mfY0Rl3GCGqw") as CLPTextBox;
                    page.PageObjects.Remove(textBoxToRemove);

                    var multipleChoice = new MultipleChoice(page)
                                         {
                                             ID = "iXOZfN4o70GvbTVCIolBMg",
                                             XPosition = 94.202626641650909,
                                             YPosition = 244.38649155722328,
                                             Height = 35,
                                             Width = 736.17448405253276,
                                             CreatorID = Person.Author.ID,
                                             OwnerID = Person.Author.ID,
                                             Orientation = MultipleChoiceOrientations.Horizontal
                                         };
                    var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "4"
                             };
                    var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 184.04362101313319,
                                 Answer = "5",
                                 IsACorrectValue = true
                             };
                    var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 368.08724202626638,
                                 Answer = "7"
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 552.13086303939963,
                                 Answer = "9"
                             };
                    multipleChoice.ChoiceBubbles.Add(b1);
                    multipleChoice.ChoiceBubbles.Add(b2);
                    multipleChoice.ChoiceBubbles.Add(b3);
                    multipleChoice.ChoiceBubbles.Add(b4);
                    page.PageObjects.Insert(0, multipleChoice);
                    break;
                }
                case "UvLXlXlpCEuLF1309g5zPA": // Page 3
                {
                    textBox = page.PageObjects.First(p => p.ID == "LZlupX4OskOkxC-VQv1pKg") as CLPTextBox;
                    textBox.TextContext = TextContexts.NonWordProblem;
                    problemTypes.Add(ProblemTypes.NonWordProblem);
                    problemTypes.Add(ProblemTypes.Multiplication);

                    var textBoxToRemove = page.PageObjects.First(p => p.ID == "1d_OeI1Kl0yJjXdvfrEeHA") as CLPTextBox;
                    page.PageObjects.Remove(textBoxToRemove);

                    var multipleChoice = new MultipleChoice(page)
                                         {
                                             ID = "kXk-V-tqC0mitEx2e7N3YQ",
                                             XPosition = 91.395872420262549,
                                             YPosition = 231.75609756097555,
                                             Height = 35,
                                             Width = 900.36960600375278,
                                             CreatorID = Person.Author.ID,
                                             OwnerID = Person.Author.ID,
                                             Orientation = MultipleChoiceOrientations.Horizontal
                                         };
                    var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "9 + 7"
                             };
                    var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 225.09240150093819,
                                 Answer = "9 - 7"
                             };
                    var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 450.18480300187639,
                                 Answer = "7 x 9",
                                 IsACorrectValue = true
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 675.27720450281458,
                                 Answer = "63 ÷ 9"
                             };
                    multipleChoice.ChoiceBubbles.Add(b1);
                    multipleChoice.ChoiceBubbles.Add(b2);
                    multipleChoice.ChoiceBubbles.Add(b3);
                    multipleChoice.ChoiceBubbles.Add(b4);
                    page.PageObjects.Insert(0, multipleChoice);
                    break;
                }
                case "526u6U8sQUqjFkCXTJZYiA": // Page 4
                {
                    textBox = page.PageObjects.First(p => p.ID == "DvQf2cvBkU-WFEFmLBEuoA") as CLPTextBox;
                    textBox.TextContext = TextContexts.NonWordProblem;
                    problemTypes.Add(ProblemTypes.NonWordProblem);
                    problemTypes.Add(ProblemTypes.Multiplication);

                    var textBoxToRemove = page.PageObjects.First(p => p.ID == "HXP2fZiWS0-Nc_NxIBBxRg") as CLPTextBox;
                    page.PageObjects.Remove(textBoxToRemove);

                    var multipleChoice = new MultipleChoice(page)
                                         {
                                             ID = "WW8W_qUUlky__piyTo9edQ",
                                             XPosition = 91.395872420262549,
                                             YPosition = 226.14258911819883,
                                             Height = 35,
                                             Width = 731.96435272045,
                                             CreatorID = Person.Author.ID,
                                             OwnerID = Person.Author.ID,
                                             Orientation = MultipleChoiceOrientations.Horizontal
                                         };
                    var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "2"
                             };
                    var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 182.99108818011251,
                                 Answer = "3",
                                 IsACorrectValue = true
                             };
                    var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 365.982176360225,
                                 Answer = "5"
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 548.9732645403376,
                                 Answer = "8"
                             };
                    multipleChoice.ChoiceBubbles.Add(b1);
                    multipleChoice.ChoiceBubbles.Add(b2);
                    multipleChoice.ChoiceBubbles.Add(b3);
                    multipleChoice.ChoiceBubbles.Add(b4);
                    page.PageObjects.Insert(0, multipleChoice);
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

                    var textBoxToRemove = page.PageObjects.First(p => p.ID == "rcBWT95ExEW9DuS8xkK2Xw") as CLPTextBox;
                    page.PageObjects.Remove(textBoxToRemove);

                    var multipleChoice = new MultipleChoice(page)
                                         {
                                             ID = "me90TgnrPUKEN1CD6AzinQ",
                                             XPosition = 83.764739279027367,
                                             YPosition = 280.02783329603585,
                                             Height = 35,
                                             Width = 990.04844076613745,
                                             CreatorID = Person.Author.ID,
                                             OwnerID = Person.Author.ID,
                                             Orientation = MultipleChoiceOrientations.Horizontal
                                         };
                    var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "16",
                                 AnswerLabel = "years old"
                             };
                    var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 247.51211019153436,
                                 Answer = "24",
                                 AnswerLabel = "years old"
                             };
                    var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 495.02422038306872,
                                 Answer = "64",
                                 AnswerLabel = "years old",
                                 IsACorrectValue = true
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 742.53633057460308,
                                 Answer = "80",
                                 AnswerLabel = "years old"
                             };
                    multipleChoice.ChoiceBubbles.Add(b1);
                    multipleChoice.ChoiceBubbles.Add(b2);
                    multipleChoice.ChoiceBubbles.Add(b3);
                    multipleChoice.ChoiceBubbles.Add(b4);
                    page.PageObjects.Insert(0, multipleChoice);
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

                    var textBoxToRemove = page.PageObjects.First(p => p.ID == "QPzA5GnIUkSE5opKl8nm8g") as CLPTextBox;
                    page.PageObjects.Remove(textBoxToRemove);

                    var multipleChoice = new MultipleChoice(page)
                                         {
                                             ID = "u0WfKRSe00mftRwyarge6A",
                                             XPosition = 98.412757973733619,
                                             YPosition = 264.0337711069418,
                                             Height = 35,
                                             Width = 778.27579737335884,
                                             CreatorID = Person.Author.ID,
                                             OwnerID = Person.Author.ID,
                                             Orientation = MultipleChoiceOrientations.Horizontal
                                         };
                    var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "$5"
                             };
                    var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 194.56894934333971,
                                 Answer = "$7"
                             };
                    var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 389.13789868667942,
                                 Answer = "$8",
                                 IsACorrectValue = true
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 583.70684803001916,
                                 Answer = "$55"
                             };
                    multipleChoice.ChoiceBubbles.Add(b1);
                    multipleChoice.ChoiceBubbles.Add(b2);
                    multipleChoice.ChoiceBubbles.Add(b3);
                    multipleChoice.ChoiceBubbles.Add(b4);
                    page.PageObjects.Insert(0, multipleChoice);
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

        public static void ConvertDivisionTemplatesToUseNewRemainderTiles(CLPPage page)
        {
            foreach (var divisionTemplate in page.PageObjects.OfType<DivisionTemplate>().Where(d => d.RemainderTiles != null))
            {
                divisionTemplate.IsRemainderTilesVisible = true;
            }

            foreach (var divisionTemplate in page.History.TrashedPageObjects.OfType<DivisionTemplate>().Where(d => d.RemainderTiles != null))
            {
                divisionTemplate.IsRemainderTilesVisible = true;
            }
        }

        public static void FixOldDivisionTemplateSizing(DivisionTemplate divisionTemplate)
        {
            if (!_isConvertingEmilyCache)
            {
                return;
            }

            var gridSize = divisionTemplate.ArrayHeight / divisionTemplate.Rows;

            divisionTemplate.SizeArrayToGridLevel(gridSize, false);

            var position = 0.0;
            foreach (var division in divisionTemplate.VerticalDivisions)
            {
                division.Position = position;
                division.Length = divisionTemplate.GridSquareSize * division.Value;
                position = division.Position + division.Length;
            }
        }

        public static void TheSlowRewind(CLPPage page)
        {
            //Rewind entire page
            page.History.IsAnimating = true;

            page.History.RefreshHistoryIndexes();

            while (page.History.UndoActions.Any())
            {
                var historyItemToUndo = page.History.UndoActions.FirstOrDefault();
                if (historyItemToUndo == null)
                {
                    break;
                }

                Debug.WriteLine("History Index: {0}", historyItemToUndo.HistoryActionIndex);

                #region WorksAsIs

                if (historyItemToUndo is DivisionTemplateArrayRemovedHistoryAction ||
                    historyItemToUndo is DivisionTemplateArraySnappedInHistoryAction)
                {
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //WorksAsIs

                #region CLPArrayDivisionsChanged fix

                if (historyItemToUndo is CLPArrayDivisionsChangedHistoryAction)
                {
                    var divisionsChanged = historyItemToUndo as CLPArrayDivisionsChangedHistoryAction;
                    if (!divisionsChanged.AddedDivisions.Any() &&
                        !divisionsChanged.RemovedDivisions.Any())
                    {
                        page.History.UndoActions.RemoveFirst();
                        continue;
                    }

                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //CLPArrayDivisionsChanged fix

                #region PageObjectResize fix for old Division Templates

                if (historyItemToUndo is PageObjectResizeBatchHistoryAction)
                {
                    var pageObjectResized = historyItemToUndo as PageObjectResizeBatchHistoryAction;
                    var divisionTemplate = page.GetVerifiedPageObjectOnPageByID(pageObjectResized.PageObjectID) as DivisionTemplate;
                    if (divisionTemplate != null)
                    {
                        FixOldDivisionTemplateSizing(divisionTemplate);
                        var fixStretchedDimensions = (from point in pageObjectResized.StretchedDimensions
                                                      let height = point.Y
                                                      let gridSize = (height - (2 * divisionTemplate.LabelLength)) / divisionTemplate.Rows
                                                      let newWidth = (gridSize * divisionTemplate.Columns) + divisionTemplate.LabelLength + divisionTemplate.LargeLabelLength
                                                      select new Point(newWidth, point.Y)).ToList();

                        pageObjectResized.StretchedDimensions = fixStretchedDimensions;
                    }

                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectResize fix for old Division Templates

                #region PageObjectsAdded to ObjectsOnPageChanged

                if (historyItemToUndo is PageObjectsAddedHistoryItem)
                {
                    var pageObjectsAdded = historyItemToUndo as PageObjectsAddedHistoryItem;
                    page.History.UndoActions.RemoveFirst();
                    if (!pageObjectsAdded.PageObjectIDs.Any())
                    {
                        continue;
                    }

                    var objectsChanged = new ObjectsOnPageChangedHistoryAction(pageObjectsAdded);

                    foreach (var id in objectsChanged.PageObjectIDsAdded)
                    {
                        var divisionTemplate = page.GetVerifiedPageObjectOnPageByID(id) as DivisionTemplate;
                        if (divisionTemplate != null)
                        {
                            FixOldDivisionTemplateSizing(divisionTemplate);
                        }
                    }

                    page.History.UndoActions.Insert(0, objectsChanged);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectsAdded to ObjectsOnPageChanged

                #region PageObjectsRemoved to ObjectsOnPageChanged

                if (historyItemToUndo is PageObjectsRemovedHistoryItem)
                {
                    var pageObjectsRemoved = historyItemToUndo as PageObjectsRemovedHistoryItem;
                    page.History.UndoActions.RemoveFirst();
                    if (!pageObjectsRemoved.PageObjectIDs.Any())
                    {
                        continue;
                    }

                    var objectsChanged = new ObjectsOnPageChangedHistoryAction(pageObjectsRemoved);

                    foreach (var id in objectsChanged.PageObjectIDsRemoved)
                    {
                        var divisionTemplate = page.GetVerifiedPageObjectInTrashByID(id) as DivisionTemplate;
                        if (divisionTemplate != null)
                        {
                            FixOldDivisionTemplateSizing(divisionTemplate);
                        }
                    }

                    page.History.UndoActions.Insert(0, objectsChanged);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectsRemoved to ObjectsOnPageChanged

                #region PageObjectMove to ObjectsMovedChanged

                if (historyItemToUndo is PageObjectMoveBatchHistoryItem)
                {
                    var pageObjectMove = historyItemToUndo as PageObjectMoveBatchHistoryItem;
                    page.History.UndoActions.RemoveFirst();
                    var pageObject = page.GetPageObjectByIDOnPageOrInHistory(pageObjectMove.PageObjectID);
                    if (!pageObjectMove.TravelledPositions.Any() ||
                        string.IsNullOrEmpty(pageObjectMove.PageObjectID) ||
                        pageObject == null)
                    {
                        continue;
                    }

                    var objectsMoved = new ObjectsMovedBatchHistoryAction(pageObjectMove);
                    if (objectsMoved.TravelledPositions.Count == 2 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().X - objectsMoved.TravelledPositions.Last().X) < 0.00001 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().Y - objectsMoved.TravelledPositions.Last().Y) < 0.00001)
                    {
                        continue;
                    }

                    page.History.UndoActions.Insert(0, objectsMoved);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectMove to ObjectsOnPageChanged

                #region PageObjectsMove to ObjectsMovedChanged

                if (historyItemToUndo is PageObjectsMoveBatchHistoryItem)
                {
                    var pageObjectsMove = historyItemToUndo as PageObjectsMoveBatchHistoryItem;
                    page.History.UndoActions.RemoveFirst();
                    var pageObjects = pageObjectsMove.PageObjectIDs.Select(id => pageObjectsMove.ParentPage.GetVerifiedPageObjectOnPageByID(id)).ToList();
                    pageObjects = pageObjects.Where(p => p != null).ToList();
                    if (!pageObjectsMove.TravelledPositions.Any() ||
                        !pageObjectsMove.PageObjectIDs.Any() ||
                        !pageObjects.Any())
                    {
                        continue;
                    }

                    var objectsMoved = new ObjectsMovedBatchHistoryAction(pageObjectsMove);
                    if (objectsMoved.TravelledPositions.Count == 2 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().X - objectsMoved.TravelledPositions.Last().X) < 0.00001 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().Y - objectsMoved.TravelledPositions.Last().Y) < 0.00001)
                    {
                        continue;
                    }
                    page.History.UndoActions.Insert(0, objectsMoved);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectMove to ObjectssOnPageChanged

                #region PageObjectCut fix

                //if (historyItemToUndo is PageObjectCutHistoryAction)
                //{
                //    //BUG: Fix to allow strokes that don't cut any pageObjects.
                //    var pageObjectCut = historyItemToUndo as PageObjectCutHistoryAction;
                //    if (!string.IsNullOrEmpty(pageObjectCut.CutPageObjectID))
                //    {
                //        page.History.ConversionUndo();
                //        continue;
                //    }
                //    var cuttingStroke = pageObjectCut.ParentPage.GetVerifiedStrokeInHistoryByID(pageObjectCut.CuttingStrokeID);
                //    if (!pageObjectCut.CutPageObjectIDs.Any() ||
                //        cuttingStroke == null)
                //    {
                //        page.History.UndoActions.RemoveFirst();
                //        continue;
                //    }
                //    if (pageObjectCut.CutPageObjectIDs.Count == 1)
                //    {
                //        pageObjectCut.CutPageObjectID = pageObjectCut.CutPageObjectIDs.First();
                //        pageObjectCut.CutPageObjectIDs.Clear();
                //        page.History.ConversionUndo();
                //        continue;
                //    }

                //    var newHistoryItems = new List<PageObjectCutHistoryAction>();
                //    foreach (var cutPageObjectID in pageObjectCut.CutPageObjectIDs)
                //    {
                //        var cutPageObject = pageObjectCut.ParentPage.GetVerifiedPageObjectInTrashByID(cutPageObjectID) as ICuttable;
                //        if (pageObjectCut.HalvedPageObjectIDs.Count < 2)
                //        {
                //            continue;
                //        }
                //        var halvedPageObjectIDs = new List<string>
                //                                  {
                //                                      pageObjectCut.HalvedPageObjectIDs[0],
                //                                      pageObjectCut.HalvedPageObjectIDs[1]
                //                                  };
                //        pageObjectCut.HalvedPageObjectIDs.RemoveRange(0, 2);
                //        if (cutPageObject == null)
                //        {
                //            continue;
                //        }
                //        var newCutHistoryItem = new PageObjectCutHistoryAction(pageObjectCut.ParentPage, pageObjectCut.ParentPage.Owner, cuttingStroke, cutPageObject, halvedPageObjectIDs);
                //        newHistoryItems.Add(newCutHistoryItem);
                //    }

                //    page.History.UndoActions.RemoveFirst();

                //    foreach (var pageObjectCutHistoryItem in newHistoryItems)
                //    {
                //        page.History.UndoActions.Insert(0, pageObjectCutHistoryItem);
                //    }

                //    continue;
                //}

                #endregion //PageObjectCut fix

                #region EndPointChangedHistoryItem Adjustments

                if (historyItemToUndo is NumberLineEndPointsChangedHistoryAction)
                {
                    var endPointsChangedHistoryItem = historyItemToUndo as NumberLineEndPointsChangedHistoryAction;
                    var resizeBatchHistoryItem = page.History.RedoActions.FirstOrDefault() as PageObjectResizeBatchHistoryAction;

                    if (resizeBatchHistoryItem != null)
                    {
                        var numberLine = page.GetVerifiedPageObjectOnPageByID(endPointsChangedHistoryItem.NumberLineID) as NumberLine;
                        if (numberLine == null)
                        {
                            page.History.UndoActions.RemoveFirst();
                            continue;
                        }

                        var potentialNumberLineMatch = page.GetVerifiedPageObjectOnPageByID(resizeBatchHistoryItem.PageObjectID) as NumberLine;
                        if (potentialNumberLineMatch == null)
                        {
                            page.History.ConversionUndo();
                            continue;
                        }

                        if (numberLine.ID == potentialNumberLineMatch.ID)
                        {
                            var previousWidth = resizeBatchHistoryItem.StretchedDimensions.First().X;
                            var currentEndPoint = numberLine.NumberLineSize;
                            var previousEndPoint = endPointsChangedHistoryItem.PreviousEndValue;

                            var previousNumberLineWidth = previousWidth - (numberLine.ArrowLength * 2);
                            var previousTickLength = previousNumberLineWidth / previousEndPoint;

                            var preStretchedWidth = previousWidth + (previousTickLength * (currentEndPoint - previousEndPoint));
                            if (Math.Abs(numberLine.Width - preStretchedWidth) < numberLine.TickLength / 2)
                            {
                                preStretchedWidth = numberLine.Width;
                            }
                            endPointsChangedHistoryItem.PreStretchedWidth = preStretchedWidth;
                        }
                    }

                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //EndPointChangedHistoryItem Adjustments

                #region StrokesChanged to ObjectsOnPageChanged

                if (historyItemToUndo is StrokesChangedHistoryItem)
                {
                    var strokesChanged = historyItemToUndo as StrokesChangedHistoryItem;
                    page.History.UndoActions.RemoveFirst();
                    if (!strokesChanged.StrokeIDsAdded.Any() &&
                        !strokesChanged.StrokeIDsRemoved.Any())
                    {
                        continue;
                    }

                    var objectsChanged = new ObjectsOnPageChangedHistoryAction(strokesChanged);
                    var strokesAdded = objectsChanged.StrokesAdded;
                    var strokesRemoved = objectsChanged.StrokesRemoved;

                    // Single Add
                    if (strokesAdded.Count == 1 &&
                        !strokesRemoved.Any())
                    {
                        var strokeID = objectsChanged.StrokeIDsAdded.First();
                        var addedStroke = page.GetVerifiedStrokeOnPageByID(strokeID);

                        // Check for Jump Added
                        var wasJumpAdded = false;
                        foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                        {
                            var tickR = numberLine.FindClosestTickToArcStroke(addedStroke, true);
                            var tickL = numberLine.FindClosestTickToArcStroke(addedStroke, false);
                            if (tickR == null ||
                                tickL == null ||
                                tickR == tickL)
                            {
                                continue;
                            }

                            var oldHeight = numberLine.JumpSizes.Count == 1 ? numberLine.NumberLineHeight : numberLine.Height;
                            var oldYPosition = numberLine.JumpSizes.Count == 1 ? numberLine.YPosition + numberLine.Height - numberLine.NumberLineHeight : numberLine.YPosition;
                            var jumpsChangedHistoryItem = new NumberLineJumpSizesChangedHistoryAction(page,
                                                                                                    page.Owner,
                                                                                                    numberLine.ID,
                                                                                                    new List<Stroke>
                                                                                                    {
                                                                                                        addedStroke
                                                                                                    },
                                                                                                    new List<Stroke>(),
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    oldHeight,
                                                                                                    oldYPosition,
                                                                                                    numberLine.Height,
                                                                                                    numberLine.YPosition,
                                                                                                    true);

                            page.History.UndoActions.Insert(0, jumpsChangedHistoryItem);
                            page.History.ConversionUndo();
                            wasJumpAdded = true;
                            break;
                        }

                        if (wasJumpAdded)
                        {
                            continue;
                        }

                        // Check for Multiple Choice Fill-In
                        var multipleChoice = page.PageObjects.FirstOrDefault(p => p is MultipleChoice) as MultipleChoice;
                        if (multipleChoice != null)
                        {
                            var choiceBubbleStrokeIsOver = multipleChoice.ChoiceBubbleStrokeIsOver(addedStroke);
                            if (choiceBubbleStrokeIsOver != null)
                            {
                                var status = ChoiceBubbleStatuses.FilledIn;
                                var threshold = 80;
                                var index = multipleChoice.ChoiceBubbles.IndexOf(choiceBubbleStrokeIsOver);
                                multipleChoice.ChangeAcceptedStrokes(strokesAdded, strokesRemoved);
                                var multipleChoiceStatus = new MultipleChoiceBubbleStatusChangedHistoryAction(page, page.Owner, multipleChoice, index, status, strokesAdded, strokesRemoved);
                                page.History.UndoActions.Insert(0, multipleChoiceStatus);
                                page.History.ConversionUndo();
                                continue;
                            }
                        }
                    }
                    else if (strokesRemoved.Count == 1 && //Single Remove
                             !strokesAdded.Any())
                    {
                        var strokeID = objectsChanged.StrokeIDsRemoved.First();
                        var removedStroke = page.GetVerifiedStrokeInHistoryByID(strokeID);

                        // Check for Jump Removed
                        var wasJumpRemoved = false;
                        foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                        {
                            var tickR = numberLine.FindClosestTickToArcStroke(removedStroke, true);
                            var tickL = numberLine.FindClosestTickToArcStroke(removedStroke, false);
                            if (tickR == null ||
                                tickL == null ||
                                tickR == tickL)
                            {
                                continue;
                            }

                            var oldHeight = numberLine.Height;
                            var oldYPosition = numberLine.YPosition;
                            if (numberLine.JumpSizes.Count == 0)
                            {
                                var tallestPoint = removedStroke.GetBounds().Top;
                                tallestPoint = tallestPoint - 40;

                                if (tallestPoint < 0)
                                {
                                    tallestPoint = 0;
                                }

                                if (tallestPoint > numberLine.YPosition + numberLine.Height - numberLine.NumberLineHeight)
                                {
                                    tallestPoint = numberLine.YPosition + numberLine.Height - numberLine.NumberLineHeight;
                                }

                                oldHeight += (numberLine.YPosition - tallestPoint);
                                oldYPosition = tallestPoint;
                            }
                            var jumpsChangedHistoryItem = new NumberLineJumpSizesChangedHistoryAction(page,
                                                                                                    page.Owner,
                                                                                                    numberLine.ID,
                                                                                                    new List<Stroke>(),
                                                                                                    new List<Stroke>
                                                                                                    {
                                                                                                        removedStroke
                                                                                                    },
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    oldHeight,
                                                                                                    oldYPosition,
                                                                                                    numberLine.Height,
                                                                                                    numberLine.YPosition,
                                                                                                    true);

                            page.History.UndoActions.Insert(0, jumpsChangedHistoryItem);
                            page.History.ConversionUndo();
                            wasJumpRemoved = true;
                            break;
                        }

                        if (wasJumpRemoved)
                        {
                            continue;
                        }

                        // Check for Multiple Choice Fill-In
                        var multipleChoice = page.PageObjects.FirstOrDefault(p => p is MultipleChoice) as MultipleChoice;
                        if (multipleChoice != null)
                        {
                            var status = ChoiceBubbleStatuses.CompletelyErased;
                            var threshold = 80;
                            var choiceBubbleStrokeIsOver = multipleChoice.ChoiceBubbleStrokeIsOver(removedStroke);
                            if (choiceBubbleStrokeIsOver != null)
                            {
                                var index = multipleChoice.ChoiceBubbles.IndexOf(choiceBubbleStrokeIsOver);
                                multipleChoice.ChangeAcceptedStrokes(strokesAdded, strokesRemoved);
                                var multipleChoiceStatus = new MultipleChoiceBubbleStatusChangedHistoryAction(page, page.Owner, multipleChoice, index, status, strokesAdded, strokesRemoved);
                                page.History.UndoActions.Insert(0, multipleChoiceStatus);
                                page.History.ConversionUndo();
                                continue;
                            }
                        }
                    }
                    else if (strokesRemoved.Count == 1 && //Point Erase
                             strokesAdded.Count == 2)
                    {
                        // TODO: Handle this use case?
                    }
                    else
                    {
                        // TODO: ERROR - This shouldn't be possible
                        Debug.WriteLine("[ERROR]: StrokesChangedHistoryItem is not a single add, single erase, or point erase!!!!!");
                    }

                    if (objectsChanged.IsUsingStrokes)
                    {
                        page.History.UndoActions.Insert(0, objectsChanged);
                        page.History.ConversionUndo();
                    }

                    continue;
                }

                #endregion //StrokesChanged to ObjectsOnPageChanged

                page.History.ConversionUndo();
            }

            page.History.RefreshHistoryIndexes();
            while (page.History.RedoActions.Any())
            {
                if (_isConvertingAssessmentCache)
                {
                    var multipleChoiceStatus = page.History.RedoActions.FirstOrDefault() as MultipleChoiceBubbleStatusChangedHistoryAction;
                    if (multipleChoiceStatus != null)
                    {
                        var threshold = 80;
                        var multipleChoice = page.GetPageObjectByID(multipleChoiceStatus.MultipleChoiceID) as MultipleChoice;
                        var stroke = multipleChoiceStatus.StrokeIDsAdded.Any() ? multipleChoiceStatus.StrokesAdded.First() : multipleChoiceStatus.StrokesRemoved.First();
                        var choiceBubbleStrokeIsOver = multipleChoice.ChoiceBubbleStrokeIsOver(stroke);
                        var strokesOverBubble = multipleChoice.StrokesOverChoiceBubble(choiceBubbleStrokeIsOver);
                        var totalStrokeLength = strokesOverBubble.Sum(s => s.StylusPoints.Count);
                        if (multipleChoiceStatus.ChoiceBubbleStatus == ChoiceBubbleStatuses.FilledIn)
                        {
                            if (totalStrokeLength >= threshold)
                            {
                                multipleChoiceStatus.ChoiceBubbleStatus = ChoiceBubbleStatuses.AdditionalFilledIn;
                                choiceBubbleStrokeIsOver.IsFilledIn = true;
                            }
                            else
                            {
                                totalStrokeLength += stroke.StylusPoints.Count;
                                if (totalStrokeLength >= threshold)
                                {
                                    multipleChoiceStatus.ChoiceBubbleStatus = ChoiceBubbleStatuses.FilledIn;
                                    choiceBubbleStrokeIsOver.IsFilledIn = true;
                                }
                                else
                                {
                                    multipleChoiceStatus.ChoiceBubbleStatus = ChoiceBubbleStatuses.PartiallyFilledIn;
                                    choiceBubbleStrokeIsOver.IsFilledIn = false;
                                }
                            }
                        }
                        else if (multipleChoiceStatus.ChoiceBubbleStatus == ChoiceBubbleStatuses.CompletelyErased)
                        {
                            var otherStrokes = strokesOverBubble.Where(s => s.GetStrokeID() != stroke.GetStrokeID()).ToList();
                            var otherStrokesStrokeLength = otherStrokes.Sum(s => s.StylusPoints.Count);

                            if (totalStrokeLength < threshold)
                            {
                                multipleChoiceStatus.ChoiceBubbleStatus = ChoiceBubbleStatuses.ErasedPartiallyFilledIn;
                                choiceBubbleStrokeIsOver.IsFilledIn = false;
                            }
                            else
                            {
                                if (otherStrokesStrokeLength < threshold)
                                {
                                    multipleChoiceStatus.ChoiceBubbleStatus = ChoiceBubbleStatuses.CompletelyErased;
                                    choiceBubbleStrokeIsOver.IsFilledIn = false;
                                }
                                else
                                {
                                    multipleChoiceStatus.ChoiceBubbleStatus = ChoiceBubbleStatuses.IncompletelyErased;
                                    choiceBubbleStrokeIsOver.IsFilledIn = true;
                                }
                            }
                        }
                    }
                }

                page.History.Redo();
            }

            page.History.IsAnimating = false;
        }

        //Clear Authored Histories
        //var undoItemsToRemove = page.History.UndoActions.Where(historyAction => historyAction.OwnerID == Person.Author.ID).ToList();
        //            foreach(var historyAction in undoItemsToRemove)
        //            {
        //                page.History.UndoActions.Remove(historyAction);
        //            }

        //            var redoItemsToRemove = page.History.RedoActions.Where(historyAction => historyAction.OwnerID == Person.Author.ID).ToList();
        //            foreach(var historyAction in redoItemsToRemove)
        //            {
        //                page.History.RedoActions.Remove(historyAction);
        //            }

        //            page.History.OptimizeTrashedItems();

        // Process a console command
        // returns true iff the console should accept another command after this one
    }
}