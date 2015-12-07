using System;
using System.Collections.Generic;
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
            //while(true)
            //{
            //    Console.Write("> ");
            //    string command = Console.ReadLine();
            //    if(!processCommand(command))
            //    {
            //        break;
            //    }
            //}

            //while(true)
            //{
            //    Console.Write("> ");
            //    var command = Console.ReadLine();
            //    if(command == null)
            //    {
            //        continue;
            //    }
            //    if(command == "exit")
            //    {
            //        return;
            //    }
            //    var compactID = new Guid(command).ToCompactID();
            //    Console.WriteLine("CompactID: " + compactID);
            //}

            Convert();
            Console.WriteLine("*****Finished*****");
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
                    var page = ModelBase.Load<CLPPage>(pageFilePath, SerializationMode.Xml);
                    page.AfterDeserialization();

                    Console.WriteLine("Loaded {3}'s page {0}, differentiation {1}, version {2}", page.PageNumber, page.DifferentiationLevel, page.VersionIndex, page.Owner.FullName);
                    //Do stuff to each page here. 

                    _isConvertingEmilyCache = false;
                    _isConvertingAssessmentCache = true;
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
                                 Offset = 0,
                                 Answer = "5",
                                 IsACorrectValue = true
                             };
                    var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "7"
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
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
                                 Offset = 0,
                                 Answer = "9 - 7"
                             };
                    var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "7 x 9",
                                 IsACorrectValue = true
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
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
                            Offset = 0,
                            Answer = "3",
                            IsACorrectValue = true
                        };
                        var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = 0,
                            Answer = "5"
                        };
                        var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = 0,
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
                            Offset = 0,
                            Answer = "24",
                            AnswerLabel = "years old"
                        };
                        var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = 0,
                            Answer = "64",
                            AnswerLabel = "years old",
                            IsACorrectValue = true
                        };
                        var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = 0,
                            Answer = "80",
                            AnswerLabel = "years old"
                        };
                        multipleChoice.ChoiceBubbles.Add(b1);
                        multipleChoice.ChoiceBubbles.Add(b2);
                        multipleChoice.ChoiceBubbles.Add(b3);
                        multipleChoice.ChoiceBubbles.Add(b4);
                        page.PageObjects.Insert(0, multipleChoice);

                    //    if (page.Owner.FullName.Contains("John"))
                    //{
                    //    var tag = new NumberLineRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, "AcYvs_jEXEW9sXBGZ4euyA", 0, 64, 1, Correctness.Correct);
                    //    page.AddTag(tag);

                    //    var tag2 = new NumberLineJumpEraseTag(page, Origin.StudentPageGenerated);
                    //    page.AddTag(tag2);
                    //}
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
                            Offset = 0,
                            Answer = "$7"
                        };
                        var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = 0,
                            Answer = "$8",
                            IsACorrectValue = true
                        };
                        var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = 0,
                            Answer = "$55"
                        };
                        multipleChoice.ChoiceBubbles.Add(b1);
                        multipleChoice.ChoiceBubbles.Add(b2);
                        multipleChoice.ChoiceBubbles.Add(b3);
                        multipleChoice.ChoiceBubbles.Add(b4);
                        page.PageObjects.Insert(0, multipleChoice);

                        //if (page.Owner.FullName.Contains("John"))
                        //{
                        //    var tag = new NumberLineRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, "OqQXQpO4ukq5IGg5Zyo6ig", 0, 56, 1, Correctness.Correct);
                        //    page.AddTag(tag);
                        //}
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

                    //if (page.Owner.FullName.Contains("Julia"))
                    //{
                    //    var tag1 = new ArrayRepresentationCorrectnessTag(page, Origin.StudentPageGenerated, Correctness.Correct, new List<IHistoryAction>());
                    //    page.AddTag(tag1);

                    //    var tag2 = new ArrayRepresentationCorrectnessTag(page,
                    //                                                     Origin.StudentPageGenerated,
                    //                                                     Correctness.Correct,
                    //                                                     new List<IHistoryAction>
                    //                                                     {
                    //                                                         new HistoryAction()
                    //                                                     });
                    //    page.AddTag(tag2);
                    //}
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
            foreach (var divisionTemplate in page.PageObjects.OfType<FuzzyFactorCard>().Where(d => d.RemainderTiles != null))
            {
                divisionTemplate.IsRemainderTilesVisible = true;
            }

            foreach (var divisionTemplate in page.History.TrashedPageObjects.OfType<FuzzyFactorCard>().Where(d => d.RemainderTiles != null))
            {
                divisionTemplate.IsRemainderTilesVisible = true;
            }
        }

        public static void FixOldDivisionTemplateSizing(FuzzyFactorCard divisionTemplate)
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

            while (page.History.UndoItems.Any())
            {
                var historyItemToUndo = page.History.UndoItems.FirstOrDefault();
                if (historyItemToUndo == null)
                {
                    break;
                }

                Console.WriteLine("History Index: {0}", historyItemToUndo.HistoryIndex);

                #region WorksAsIs

                if (historyItemToUndo is AnimationIndicator ||
                    historyItemToUndo is CLPArrayRotateHistoryItem ||
                    historyItemToUndo is CLPArrayGridToggleHistoryItem ||
                    historyItemToUndo is CLPArrayDivisionValueChangedHistoryItem ||
                    historyItemToUndo is FFCArrayRemovedHistoryItem ||
                    historyItemToUndo is FFCArraySnappedInHistoryItem ||
                    historyItemToUndo is RemainderTilesVisibilityToggledHistoryItem ||
                    historyItemToUndo is PartsValueChangedHistoryItem ||
                    historyItemToUndo is CLPArraySnapHistoryItem)
                {
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //WorksAsIs

                #region CLPArrayDivisionsChanged fix

                if (historyItemToUndo is CLPArrayDivisionsChangedHistoryItem)
                {
                    var divisionsChanged = historyItemToUndo as CLPArrayDivisionsChangedHistoryItem;
                    if (!divisionsChanged.AddedDivisions.Any() &&
                        !divisionsChanged.RemovedDivisions.Any())
                    {
                        page.History.UndoItems.RemoveFirst();
                        continue;
                    }

                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //CLPArrayDivisionsChanged fix

                #region PageObjectResize fix for old Division Templates

                if (historyItemToUndo is PageObjectResizeBatchHistoryItem)
                {
                    var pageObjectResized = historyItemToUndo as PageObjectResizeBatchHistoryItem;
                    var divisionTemplate = page.GetVerifiedPageObjectOnPageByID(pageObjectResized.PageObjectID) as FuzzyFactorCard;
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
                    page.History.UndoItems.RemoveFirst();
                    if (!pageObjectsAdded.PageObjectIDs.Any())
                    {
                        continue;
                    }

                    var objectsChanged = new ObjectsOnPageChangedHistoryItem(pageObjectsAdded);

                    foreach (var id in objectsChanged.PageObjectIDsAdded)
                    {
                        var divisionTemplate = page.GetVerifiedPageObjectOnPageByID(id) as FuzzyFactorCard;
                        if (divisionTemplate != null)
                        {
                            FixOldDivisionTemplateSizing(divisionTemplate);
                        }
                    }

                    page.History.UndoItems.Insert(0, objectsChanged);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectsAdded to ObjectsOnPageChanged

                #region PageObjectsRemoved to ObjectsOnPageChanged

                if (historyItemToUndo is PageObjectsRemovedHistoryItem)
                {
                    var pageObjectsRemoved = historyItemToUndo as PageObjectsRemovedHistoryItem;
                    page.History.UndoItems.RemoveFirst();
                    if (!pageObjectsRemoved.PageObjectIDs.Any())
                    {
                        continue;
                    }

                    var objectsChanged = new ObjectsOnPageChangedHistoryItem(pageObjectsRemoved);

                    foreach (var id in objectsChanged.PageObjectIDsRemoved)
                    {
                        var divisionTemplate = page.GetVerifiedPageObjectInTrashByID(id) as FuzzyFactorCard;
                        if (divisionTemplate != null)
                        {
                            FixOldDivisionTemplateSizing(divisionTemplate);
                        }
                    }

                    page.History.UndoItems.Insert(0, objectsChanged);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectsRemoved to ObjectsOnPageChanged

                #region PageObjectMove to ObjectsMovedChanged

                if (historyItemToUndo is PageObjectMoveBatchHistoryItem)
                {
                    var pageObjectMove = historyItemToUndo as PageObjectMoveBatchHistoryItem;
                    page.History.UndoItems.RemoveFirst();
                    var pageObject = page.GetPageObjectByIDOnPageOrInHistory(pageObjectMove.PageObjectID);
                    if (!pageObjectMove.TravelledPositions.Any() ||
                        string.IsNullOrEmpty(pageObjectMove.PageObjectID) ||
                        pageObject == null)
                    {
                        continue;
                    }

                    var objectsMoved = new ObjectsMovedBatchHistoryItem(pageObjectMove);
                    if (objectsMoved.TravelledPositions.Count == 2 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().X - objectsMoved.TravelledPositions.Last().X) < 0.00001 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().Y - objectsMoved.TravelledPositions.Last().Y) < 0.00001)
                    {
                        continue;
                    }

                    page.History.UndoItems.Insert(0, objectsMoved);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectMove to ObjectsOnPageChanged

                #region PageObjectsMove to ObjectsMovedChanged

                if (historyItemToUndo is PageObjectsMoveBatchHistoryItem)
                {
                    var pageObjectsMove = historyItemToUndo as PageObjectsMoveBatchHistoryItem;
                    page.History.UndoItems.RemoveFirst();
                    var pageObjects = pageObjectsMove.PageObjectIDs.Select(id => pageObjectsMove.ParentPage.GetVerifiedPageObjectOnPageByID(id)).ToList();
                    pageObjects = pageObjects.Where(p => p != null).ToList();
                    if (!pageObjectsMove.TravelledPositions.Any() ||
                        !pageObjectsMove.PageObjectIDs.Any() ||
                        !pageObjects.Any())
                    {
                        continue;
                    }

                    var objectsMoved = new ObjectsMovedBatchHistoryItem(pageObjectsMove);
                    if (objectsMoved.TravelledPositions.Count == 2 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().X - objectsMoved.TravelledPositions.Last().X) < 0.00001 &&
                        Math.Abs(objectsMoved.TravelledPositions.First().Y - objectsMoved.TravelledPositions.Last().Y) < 0.00001)
                    {
                        continue;
                    }
                    page.History.UndoItems.Insert(0, objectsMoved);
                    page.History.ConversionUndo();
                    continue;
                }

                #endregion //PageObjectMove to ObjectssOnPageChanged

                #region PageObjectCut fix

                if (historyItemToUndo is PageObjectCutHistoryItem)
                {
                    //BUG: Fix to allow strokes that don't cut any pageObjects.
                    var pageObjectCut = historyItemToUndo as PageObjectCutHistoryItem;
                    if (!string.IsNullOrEmpty(pageObjectCut.CutPageObjectID))
                    {
                        page.History.ConversionUndo();
                        continue;
                    }
                    var cuttingStroke = pageObjectCut.ParentPage.GetVerifiedStrokeInHistoryByID(pageObjectCut.CuttingStrokeID);
                    if (!pageObjectCut.CutPageObjectIDs.Any() ||
                        cuttingStroke == null)
                    {
                        page.History.UndoItems.RemoveFirst();
                        continue;
                    }
                    if (pageObjectCut.CutPageObjectIDs.Count == 1)
                    {
                        pageObjectCut.CutPageObjectID = pageObjectCut.CutPageObjectIDs.First();
                        pageObjectCut.CutPageObjectIDs.Clear();
                        page.History.ConversionUndo();
                        continue;
                    }

                    var newHistoryItems = new List<PageObjectCutHistoryItem>();
                    foreach (var cutPageObjectID in pageObjectCut.CutPageObjectIDs)
                    {
                        var cutPageObject = pageObjectCut.ParentPage.GetVerifiedPageObjectInTrashByID(cutPageObjectID) as ICuttable;
                        if (pageObjectCut.HalvedPageObjectIDs.Count < 2)
                        {
                            continue;
                        }
                        var halvedPageObjectIDs = new List<string>
                                                  {
                                                      pageObjectCut.HalvedPageObjectIDs[0],
                                                      pageObjectCut.HalvedPageObjectIDs[1]
                                                  };
                        pageObjectCut.HalvedPageObjectIDs.RemoveRange(0, 2);
                        if (cutPageObject == null)
                        {
                            continue;
                        }
                        var newCutHistoryItem = new PageObjectCutHistoryItem(pageObjectCut.ParentPage, pageObjectCut.ParentPage.Owner, cuttingStroke, cutPageObject, halvedPageObjectIDs);
                        newHistoryItems.Add(newCutHistoryItem);
                    }

                    page.History.UndoItems.RemoveFirst();

                    foreach (var pageObjectCutHistoryItem in newHistoryItems)
                    {
                        page.History.UndoItems.Insert(0, pageObjectCutHistoryItem);
                    }

                    continue;
                }

                #endregion //PageObjectCut fix

                #region EndPointChangedHistoryItem Adjustments

                if (historyItemToUndo is NumberLineEndPointsChangedHistoryItem)
                {
                    var endPointsChangedHistoryItem = historyItemToUndo as NumberLineEndPointsChangedHistoryItem;
                    var resizeBatchHistoryItem = page.History.RedoItems.FirstOrDefault() as PageObjectResizeBatchHistoryItem;

                    if (resizeBatchHistoryItem != null)
                    {
                        var numberLine = page.GetVerifiedPageObjectOnPageByID(endPointsChangedHistoryItem.NumberLineID) as NumberLine;
                        if (numberLine == null)
                        {
                            page.History.UndoItems.RemoveFirst();
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
                    page.History.UndoItems.RemoveFirst();
                    if (!strokesChanged.StrokeIDsAdded.Any() &&
                        !strokesChanged.StrokeIDsRemoved.Any())
                    {
                        continue;
                    }

                    var objectsChanged = new ObjectsOnPageChangedHistoryItem(strokesChanged);
                    if (objectsChanged.IsUsingPageObjects)
                    {
                        page.History.UndoItems.Insert(0, objectsChanged);
                        page.History.ConversionUndo(); //?
                    }

                    #region JumpSizeConversion

                    var removedJumpStrokeIDs = new List<string>();
                    var jumpsRemoved = new List<NumberLineJumpSize>();
                    foreach (var strokeID in objectsChanged.StrokeIDsRemoved)
                    {
                        var stroke = page.GetVerifiedStrokeInHistoryByID(strokeID);
                        if (stroke == null)
                        {
                            removedJumpStrokeIDs.Add(strokeID);
                            continue;
                        }

                        foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                        {
                            var tickR = numberLine.FindClosestTickToArcStroke(stroke, true);
                            var tickL = numberLine.FindClosestTickToArcStroke(stroke, false);
                            if (tickR == null ||
                                tickL == null ||
                                tickR == tickL)
                            {
                                continue;
                            }

                            removedJumpStrokeIDs.Add(stroke.GetStrokeID());

                            var oldHeight = numberLine.Height;
                            var oldYPosition = numberLine.YPosition;
                            if (numberLine.JumpSizes.Count == 0)
                            {
                                var tallestPoint = stroke.GetBounds().Top;
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
                            var jumpsChangedHistoryItem = new NumberLineJumpSizesChangedHistoryItem(page,
                                                                                                    page.Owner,
                                                                                                    numberLine.ID,
                                                                                                    new List<Stroke>(),
                                                                                                    new List<Stroke>
                                                                                                    {
                                                                                                        stroke
                                                                                                    },
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    oldHeight,
                                                                                                    oldYPosition,
                                                                                                    numberLine.Height,
                                                                                                    numberLine.YPosition,
                                                                                                    true);

                            page.History.UndoItems.Insert(0, jumpsChangedHistoryItem);
                            page.History.ConversionUndo();
                            break;
                        }
                    }

                    var addedJumpStrokeIDs = new List<string>();
                    foreach (var strokeID in objectsChanged.StrokeIDsAdded)
                    {
                        var stroke = page.GetVerifiedStrokeOnPageByID(strokeID);
                        if (stroke == null)
                        {
                            addedJumpStrokeIDs.Add(strokeID);
                            continue;
                        }

                        foreach (var numberLine in page.PageObjects.OfType<NumberLine>())
                        {
                            var tickR = numberLine.FindClosestTickToArcStroke(stroke, true);
                            var tickL = numberLine.FindClosestTickToArcStroke(stroke, false);
                            if (tickR == null ||
                                tickL == null ||
                                tickR == tickL)
                            {
                                continue;
                            }

                            addedJumpStrokeIDs.Add(stroke.GetStrokeID());

                            var oldHeight = numberLine.JumpSizes.Count == 1 ? numberLine.NumberLineHeight : numberLine.Height;
                            var oldYPosition = numberLine.JumpSizes.Count == 1 ? numberLine.YPosition + numberLine.Height - numberLine.NumberLineHeight : numberLine.YPosition;
                            var jumpsChangedHistoryItem = new NumberLineJumpSizesChangedHistoryItem(page,
                                                                                                    page.Owner,
                                                                                                    numberLine.ID,
                                                                                                    new List<Stroke>
                                                                                                    {
                                                                                                        stroke
                                                                                                    },
                                                                                                    new List<Stroke>(),
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    new List<NumberLineJumpSize>(),
                                                                                                    oldHeight,
                                                                                                    oldYPosition,
                                                                                                    numberLine.Height,
                                                                                                    numberLine.YPosition,
                                                                                                    true);

                            page.History.UndoItems.Insert(0, jumpsChangedHistoryItem);
                            page.History.ConversionUndo();
                            break;
                        }
                    }

                    objectsChanged.StrokeIDsRemoved.RemoveAll(removedJumpStrokeIDs.Contains);
                    objectsChanged.StrokeIDsAdded.RemoveAll(addedJumpStrokeIDs.Contains);

                    #endregion //JumpSizeConversion

                    if (objectsChanged.IsUsingStrokes)
                    {
                        page.History.UndoItems.Insert(0, objectsChanged);
                        page.History.ConversionUndo();
                    }

                    continue;
                }

                #endregion //StrokesChanged to ObjectsOnPageChanged

                page.History.ConversionUndo();
            }

            page.History.RefreshHistoryIndexes();
            while (page.History.RedoItems.Any())
            {
                page.History.Redo();
            }

            page.History.IsAnimating = false;
        }

        //Clear Authored Histories
        //var undoItemsToRemove = page.History.UndoItems.Where(historyItem => historyItem.OwnerID == Person.Author.ID).ToList();
        //            foreach(var historyItem in undoItemsToRemove)
        //            {
        //                page.History.UndoItems.Remove(historyItem);
        //            }

        //            var redoItemsToRemove = page.History.RedoItems.Where(historyItem => historyItem.OwnerID == Person.Author.ID).ToList();
        //            foreach(var historyItem in redoItemsToRemove)
        //            {
        //                page.History.RedoItems.Remove(historyItem);
        //            }

        //            page.History.OptimizeTrashedItems();

        // Process a console command
        // returns true iff the console should accept another command after this one

        private static Boolean processCommand(string command)
        {
            if (command.Equals("replace"))
            {
                replace();
            }
            else if (command.Equals("interpret student"))
            {
                batchInterpretStudent();
            }
            else if (command.Equals("interpret teacher"))
            {
                batchInterpretTeacher();
            }
            else if (command.Equals("strip"))
            {
                stripHistory();
            }
            else if (command.Equals("combine"))
            {
                NotebookMerge.Combine();
            }
            else if (command.Equals("xml"))
            {
                XMLImporter.Import();
            }
            else if (command.Equals("exit"))
            {
                return false;
            }
            else
            {
                Console.WriteLine("Command not recognized");
            }
            return true;
        }

        private static void replace()
        {
            Console.WriteLine("Starting");

            NotebookMerge.Replace();

            Console.WriteLine("Ended");
        }

        private static void batchInterpretStudent() { BatchInterpreter.InterpretStudentNotebooks(); }

        private static void batchInterpretTeacher() { BatchInterpreter.InterpretTeacherNotebooks(); }

        private static void stripHistory() { StripHistory.StripAll(); }
    }
}