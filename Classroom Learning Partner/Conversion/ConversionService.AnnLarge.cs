using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel.Collections;
using Classroom_Learning_Partner.Services;
using CLP.Entities;
using Ann = CLP.Entities.Ann;

namespace Classroom_Learning_Partner
{
    public partial class ConversionService
    {
        #region Ann Conversions

        #region Locations

        public static string AnnCacheFolder => Path.Combine(DataService.DesktopFolderPath, "Cache.Ann.Complete");
        public static string AnnNotebooksFolder => Path.Combine(AnnCacheFolder, "Notebooks");
        public static string AnnClassesFolder => Path.Combine(AnnCacheFolder, "Classes");
        public static string AnnZipFilePath => Path.Combine(DataService.DesktopFolderPath, "Ann - Fall 2014.clp");
        public static string AnnImageFolder => Path.Combine(DataService.DesktopFolderPath, "images");

        #endregion // Locations

        #region Conversion Loop

        public static Notebook ConvertCacheAnnNotebook(string notebookFolder)
        {
            var oldNotebook = Ann.Notebook.LoadLocalFullNotebook(notebookFolder);
            var newNotebook = ConvertNotebook(oldNotebook);

            foreach (var page in oldNotebook.Pages)
            {
                var newPage = ConvertPage(page);
                foreach (var submission in page.Submissions)
                {
                    var newSubmission = ConvertPage(submission);
                    newPage.Submissions.Add(newSubmission);
                }

                newNotebook.Pages.Add(newPage);

                if (!PageNumberToIDMap.ContainsKey(newPage.PageNumber))
                {
                    PageNumberToIDMap.Add(newPage.PageNumber, newPage.ID);
                }

                if (!PageIDToNumberMap.ContainsKey(newPage.ID))
                {
                    PageIDToNumberMap.Add(newPage.ID, newPage.PageNumber);
                }
            }

            return newNotebook;
        }

        public static ClassRoster ConvertCacheAnnClassSubject(string filePath, Notebook notebook)
        {
            var classSubject = Ann.ClassSubject.OpenClassSubject(filePath);
            var classRoster = ConvertAnnClassSubject(classSubject, notebook);

            return classRoster;
        }

        public static Session ConvertCacheAnnClassPeriod(string filePath)
        {
            var classPeriod = Ann.ClassPeriod.LoadLocalClassPeriod(filePath);
            var session = ConvertClassPeriod(classPeriod);

            return session;
        }

        #endregion // Conversion Loop

        #region Notebook Parts

        // 12/7
        public static Person ConvertPerson(Ann.Person person)
        {
            var newPerson = Person.ParseFromFullName(person.FullName, person.IsStudent);
            newPerson.ID = person.ID;
            newPerson.Alias = person.Alias;
            newPerson.CurrentDifferentiationGroup = person.CurrentDifferentiationGroup;
            newPerson.TemporaryDifferentiationGroup = person.TempDifferentiationGroup;

            if (string.IsNullOrWhiteSpace(newPerson.FullName))
            {
                Debug.WriteLine($"[CONVERSION ERROR]: Person.FullName is blank. Original Person.FullName is {person.FullName}.");
            }

            return newPerson;
        }

        // 12/12
        public static ClassRoster ConvertAnnClassSubject(Ann.ClassSubject classSubject, Notebook notebook)
        {
            var notebookSet = new NotebookSet
                              {
                                  NotebookName = notebook.Name,
                                  NotebookID = notebook.ID,
                                  CreationDate = notebook.CreationDate
                              };

            var newClassRoster = new ClassRoster
                                 {
                                     SubjectName = classSubject.Name,
                                     GradeLevel = classSubject.GradeLevel,
                                     StartDate = classSubject.StartDate,
                                     EndDate = classSubject.EndDate,
                                     SchoolName = classSubject.SchoolName,
                                     SchoolDistrict = classSubject.SchoolDistrict,
                                     City = classSubject.City,
                                     State = classSubject.State
                                 };

            newClassRoster.ListOfNotebookSets.Add(notebookSet);

            var teacher = ConvertPerson(classSubject.Teacher);
            newClassRoster.ListOfTeachers.Add(teacher);

            foreach (var person in classSubject.StudentList.OrderBy(p => p.FullName))
            {
                var newPerson = ConvertPerson(person);
                newClassRoster.ListOfStudents.Add(newPerson);
            }

            return newClassRoster;
        }

        private static readonly Dictionary<int, string> PageNumberToIDMap = new Dictionary<int, string>();
        private static readonly Dictionary<string, int> PageIDToNumberMap = new Dictionary<string, int>();

        // 12/7
        public static Session ConvertClassPeriod(Ann.ClassPeriod classPeriod)
        {
            var newSession = new Session
            {
                StartTime = classPeriod.StartTime,
                StartingPageID = classPeriod.StartPageID
            };

            var startPageNumber = PageIDToNumberMap[classPeriod.StartPageID];
            newSession.StartingPageNumber = startPageNumber.ToString();

            var pageNumberRange = Enumerable.Range(startPageNumber, (int)classPeriod.NumberOfPages).ToList();
            var pageIDs = pageNumberRange.Select(i => PageNumberToIDMap[i]).ToList();
            if (!pageIDs.Contains(classPeriod.TitlePageID))
            {
                pageIDs.Insert(0, classPeriod.TitlePageID);
                pageNumberRange.Insert(0, PageIDToNumberMap[classPeriod.TitlePageID]);
            }

            newSession.PageIDs = pageIDs;
            newSession.PageNumbers = RangeHelper.ParseIntNumbersToString(pageNumberRange, false, true);
            newSession.NotebookIDs.Add(classPeriod.NotebookID);

            return newSession;
        }

        // 12/7
        public static Notebook ConvertNotebook(Ann.Notebook notebook)
        {
            var newPerson = ConvertPerson(notebook.Owner);

            var newNotebook = new Notebook
                              {
                                  ID = notebook.ID,
                                  Owner = newPerson,
                                  Name = notebook.Name,
                                  CreationDate = notebook.CreationDate,
                                  LastSavedDate = notebook.LastSavedDate,
                                  CurrentPageID = notebook.CurrentPageID,
                                  CurrentPageVersionIndex = notebook.CurrentPageVersionIndex
                              };

            return newNotebook;
        }

        // 12/7
        public static CLPPage ConvertPage(Ann.CLPPage page)
        {
            var newPerson = ConvertPerson(page.Owner);

            var newPage = new CLPPage
                          {
                              ID = page.ID,
                              Owner = newPerson,
                              PageNumber = (int)page.PageNumber,
                              DifferentiationLevel = page.DifferentiationLevel,
                              VersionIndex = page.VersionIndex,
                              LastVersionIndex = page.LastVersionIndex,
                              CreationDate = page.CreationDate,
                              PageType = page.PageType == Ann.PageTypes.Animation ? PageTypes.Animation : PageTypes.Default,
                              SubmissionType = page.SubmissionType == Ann.SubmissionTypes.Single ? SubmissionTypes.Single : SubmissionTypes.Unsubmitted,
                              SubmissionTime = page.SubmissionTime,
                              Height = page.Height,
                              Width = page.Width,
                              InitialAspectRatio = page.InitialAspectRatio
                          };

            // TODO: Tags

            foreach (var stroke in page.InkStrokes)
            {
                newPage.InkStrokes.Add(stroke);
            }

            foreach (var pageObject in page.PageObjects)
            {
                // Ignores the TextBoxes that were part of the old Multiple Choice
                // TODO: Ignore TextBoxes from Assessment Cache
                if (pageObject is Ann.CLPTextBox &&
                    (pageObject.ID == "Qgvdw-4oTk2JmSUECrftNQ" ||     // page 385
                     pageObject.ID == "F66Db6O3a0uTF0SGRiUemw" ||     // page 384
                     pageObject.ID == "MAnCvJx2HkmhNwY9AMNbNQ" ||     // page 383
                     pageObject.ID == "gXG3pxqR00S8dsJqmCrl9g" ||     // page 382
                     pageObject.ID == "wPC1vKURUk-iZExkAu9cwQ"))      // page 381
                {
                    continue;
                }

                var newPageObject = ConvertPageObject(pageObject, newPage);
                newPage.PageObjects.Add(newPageObject);
                //var divisionTemplate = newPageObject as DivisionTemplate;
                //if (divisionTemplate != null &&
                //    divisionTemplate.RemainderTiles != null)
                //{
                //    // TODO: ?
                //    newPage.PageObjects.Add(divisionTemplate.RemainderTiles);
                //}
            }

            ConvertPageHistory(page.History, newPage);

            return newPage;
        }

        #endregion // Notebook Parts

        #region PageObjects

        public static IPageObject ConvertPageObject(Ann.IPageObject pageObject, CLPPage newPage)
        {
            IPageObject newPageObject = null;

            TypeSwitch.On(pageObject).Case<Ann.Shape>(p =>
            {
                newPageObject = ConvertShape(p, newPage);
            }).Case<Ann.CLPTextBox>(p =>
            {
                newPageObject = ConvertTextBox(p, newPage);
            }).Case<Ann.CLPImage>(p =>
            {
                newPageObject = ConvertImage(p, newPage);
            }).Case<Ann.CLPArray>(p =>
            {
                newPageObject = ConvertArray(p, newPage);
            }).Case<Ann.NumberLine>(p =>
            {
                newPageObject = ConvertNumberLine(p, newPage);
            }).Case<Ann.StampedObject>(p =>
            {
                newPageObject = ConvertStampedObject(p, newPage);
            }).Case<Ann.Stamp>(p =>
            {
                newPageObject = ConvertStamp(p, newPage);
            }).Case<Ann.MultipleChoiceBox>(p =>
            {
                newPageObject = ConvertMultipleChoiceBox(p, newPage);
            });

            if (newPageObject == null)
            {
                Debug.WriteLine($"[ERROR] newPageObject is NULL. Original pageObject is {pageObject.GetType()}");
            }

            return newPageObject;
        }

        // 12/7
        public static Shape ConvertShape(Ann.Shape shape, CLPPage newPage)
        {
            var newShape = new Shape
            {
                ID = shape.ID,
                XPosition = shape.XPosition,
                YPosition = shape.YPosition,
                Height = shape.Height,
                Width = shape.Width,
                OwnerID = shape.OwnerID,
                CreatorID = shape.CreatorID,
                CreationDate = shape.CreationDate,
                PageObjectFunctionalityVersion = "Ann12.19.2014",
                IsManipulatableByNonCreator = shape.IsManipulatableByNonCreator,
                ParentPage = newPage
            };

            switch (shape.ShapeType)
            {
                case Ann.ShapeType.Rectangle:
                    newShape.ShapeType = ShapeType.Rectangle;
                    break;
                case Ann.ShapeType.Ellipse:
                    newShape.ShapeType = ShapeType.Ellipse;
                    break;
                case Ann.ShapeType.Triangle:
                    newShape.ShapeType = ShapeType.Triangle;
                    break;
                case Ann.ShapeType.HorizontalLine:
                    newShape.ShapeType = ShapeType.HorizontalLine;
                    break;
                case Ann.ShapeType.VerticalLine:
                    newShape.ShapeType = ShapeType.VerticalLine;
                    break;
                case Ann.ShapeType.Protractor:
                    newShape.ShapeType = ShapeType.Protractor;
                    break;
                default:
                    newShape.ShapeType = ShapeType.Rectangle;
                    break;
            }

            newShape.Parts = shape.Parts;
            newShape.IsInnerPart = shape.IsInnerPart;
            newShape.IsPartsAutoGenerated = shape.IsPartsAutoGenerated;

            return newShape;
        }

        // 12/7
        public static CLPTextBox ConvertTextBox(Ann.CLPTextBox textBox, CLPPage newPage)
        {
            var newTextBox = new CLPTextBox
            {
                ID = textBox.ID,
                XPosition = textBox.XPosition,
                YPosition = textBox.YPosition,
                Height = textBox.Height,
                Width = textBox.Width,
                OwnerID = textBox.OwnerID,
                CreatorID = textBox.CreatorID,
                CreationDate = textBox.CreationDate,
                PageObjectFunctionalityVersion = "Ann12.19.2014",
                IsManipulatableByNonCreator = textBox.IsManipulatableByNonCreator,
                ParentPage = newPage
            };

            newTextBox.Text = textBox.Text;

            return newTextBox;
        }

        // 12/7
        public static CLPImage ConvertImage(Ann.CLPImage image, CLPPage newPage)
        {
            var newImage = new CLPImage
            {
                ID = image.ID,
                XPosition = image.XPosition,
                YPosition = image.YPosition,
                Height = image.Height,
                Width = image.Width,
                OwnerID = image.OwnerID,
                CreatorID = image.CreatorID,
                CreationDate = image.CreationDate,
                PageObjectFunctionalityVersion = "Ann12.19.2014",
                IsManipulatableByNonCreator = image.IsManipulatableByNonCreator,
                ParentPage = newPage
            };

            newImage.ImageHashID = image.ImageHashID;

            return newImage;
        }

        // 12/7
        public static CLPArray ConvertArray(Ann.CLPArray array, CLPPage newPage)
        {
            var newArray = new CLPArray
            {
                ID = array.ID,
                XPosition = array.XPosition,
                YPosition = array.YPosition,
                Height = array.Height,
                Width = array.Width,
                OwnerID = array.OwnerID,
                CreatorID = array.CreatorID,
                CreationDate = array.CreationDate,
                PageObjectFunctionalityVersion = "Ann12.19.2014",
                IsManipulatableByNonCreator = array.IsManipulatableByNonCreator,
                ParentPage = newPage
            };

            // ACLPArrayBase
            newArray.Rows = array.Rows;
            newArray.Columns = array.Columns;
            newArray.IsGridOn = array.IsGridOn;
            newArray.IsDivisionBehaviorOn = array.IsDivisionBehaviorOn;
            newArray.IsSnappable = array.IsSnappable;
            newArray.IsTopLabelVisible = array.IsTopLabelVisible;
            newArray.IsSideLabelVisible = array.IsSideLabelVisible;

            foreach (var division in array.HorizontalDivisions)
            {
                var newDivision = ConvertArrayDivision(division);
                newArray.HorizontalDivisions.Add(newDivision);
            }

            foreach (var division in array.VerticalDivisions)
            {
                var newDivision = ConvertArrayDivision(division);
                newArray.VerticalDivisions.Add(newDivision);
            }

            // CLPArray
            switch (array.ArrayType)
            {
                case Ann.ArrayTypes.Array:
                    newArray.ArrayType = ArrayTypes.Array;
                    break;
                case Ann.ArrayTypes.ArrayCard:
                    newArray.ArrayType = ArrayTypes.ArrayCard;
                    break;
                case Ann.ArrayTypes.FactorCard:
                    newArray.ArrayType = ArrayTypes.FactorCard;
                    break;
                default:
                    newArray.ArrayType = ArrayTypes.Array;
                    break;
            }

            newArray.CanAcceptStrokes = array.CanAcceptStrokes;
            newArray.AcceptedStrokes = array.AcceptedStrokes;  // TODO: Confirm this is necessary?
            newArray.AcceptedStrokeParentIDs = array.AcceptedStrokeParentIDs;
            newArray.IsInnerPart = array.IsInnerPart;
            newArray.IsPartsAutoGenerated = array.IsPartsAutoGenerated;

            return newArray;
        }

        // 12/7
        public static CLPArrayDivision ConvertArrayDivision(Ann.CLPArrayDivision division)
        {
            var newDivision = new CLPArrayDivision
                              {
                                  Position = division.Position,
                                  Length = division.Length,
                                  Value = division.Value,
                                  Orientation = division.Orientation == Ann.ArrayDivisionOrientation.Horizontal ? ArrayDivisionOrientation.Horizontal : ArrayDivisionOrientation.Vertical
                              };

            return newDivision;
        }

        // 12/7
        public static NumberLine ConvertNumberLine(Ann.NumberLine numberLine, CLPPage newPage)
        {
            var newNumberLine = new NumberLine
                                {
                                    ID = numberLine.ID,
                                    XPosition = numberLine.XPosition,
                                    YPosition = numberLine.YPosition,
                                    Height = numberLine.Height,
                                    Width = numberLine.Width,
                                    OwnerID = numberLine.OwnerID,
                                    CreatorID = numberLine.CreatorID,
                                    CreationDate = numberLine.CreationDate,
                                    PageObjectFunctionalityVersion = "Ann12.19.2014",
                                    IsManipulatableByNonCreator = numberLine.IsManipulatableByNonCreator,
                                    ParentPage = newPage
                                };

            newNumberLine.NumberLineType = NumberLineTypes.NumberLine;
            newNumberLine.NumberLineSize = numberLine.NumberLineSize;
            newNumberLine.IsJumpSizeLabelsVisible = numberLine.IsJumpSizeLabelsVisible;
            newNumberLine.IsAutoArcsVisible = false;

            foreach (var jumpSize in numberLine.JumpSizes)
            {
                var newJumpSize = ConvertNumberLineJumpSize(jumpSize);
                newNumberLine.JumpSizes.Add(newJumpSize);
            }

            foreach (var tick in numberLine.Ticks)
            {
                var newTick = ConvertNumberLineTick(tick);
                newNumberLine.Ticks.Add(newTick);
            }

            newNumberLine.CanAcceptStrokes = numberLine.CanAcceptStrokes;
            newNumberLine.AcceptedStrokes = numberLine.AcceptedStrokes;  // TODO: Confirm this is necessary?
            newNumberLine.AcceptedStrokeParentIDs = numberLine.AcceptedStrokeParentIDs;

            return newNumberLine;
        }

        // 12/7
        public static NumberLineJumpSize ConvertNumberLineJumpSize(Ann.NumberLineJumpSize jumpSize)
        {
            var newJumpeSize = new NumberLineJumpSize
            {
                JumpSize = jumpSize.JumpSize,
                StartingTickIndex = jumpSize.StartingTickIndex,
                JumpColor = "Black"
            };

            return newJumpeSize;
        }

        // 12/7
        public static NumberLineTick ConvertNumberLineTick(Ann.NumberLineTick tick)
        {
            var newTick = new NumberLineTick
            {
                TickValue = tick.TickValue,
                IsNumberVisible = tick.IsNumberVisible,
                IsTickVisible = tick.IsTickVisible,
                IsMarked = tick.IsMarked,
                TickColor = tick.TickColor
            };

            return newTick;
        }

        // 12/7
        public static StampedObject ConvertStampedObject(Ann.StampedObject stampedObject, CLPPage newPage)
        {
            var newStampedObject = new StampedObject
            {
                ID = stampedObject.ID,
                XPosition = stampedObject.XPosition,
                YPosition = stampedObject.YPosition,
                Height = stampedObject.Height,
                Width = stampedObject.Width,
                OwnerID = stampedObject.OwnerID,
                CreatorID = stampedObject.CreatorID,
                CreationDate = stampedObject.CreationDate,
                PageObjectFunctionalityVersion = "Ann12.19.2014",
                IsManipulatableByNonCreator = stampedObject.IsManipulatableByNonCreator,
                ParentPage = newPage
            };

            newStampedObject.ParentStampID = stampedObject.ParentStampID;
            newStampedObject.ImageHashID = stampedObject.ImageHashID;

            switch (stampedObject.StampedObjectType)
            {
                case Ann.StampedObjectTypes.GeneralStampedObject:
                    newStampedObject.StampedObjectType = StampedObjectTypes.GeneralStampedObject;
                    break;
                case Ann.StampedObjectTypes.VisibleParts:
                    newStampedObject.StampedObjectType = StampedObjectTypes.VisibleParts;
                    break;
                case Ann.StampedObjectTypes.GroupStampedObject:
                    newStampedObject.StampedObjectType = StampedObjectTypes.GroupStampedObject;
                    break;
                case Ann.StampedObjectTypes.EmptyGroupStampedObject:
                    newStampedObject.StampedObjectType = StampedObjectTypes.EmptyGroupStampedObject;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var strokeDTO in stampedObject.SerializedStrokes)
            {
                var stroke = strokeDTO.ToStroke();
                newStampedObject.SerializedStrokes.Add(stroke.ToStrokeDTO());
            }

            newStampedObject.IsBoundaryVisible = stampedObject.IsBoundaryVisible;
            newStampedObject.IsPartsLabelVisible = stampedObject.IsPartsLabelVisible;

            newStampedObject.Parts = stampedObject.Parts;
            newStampedObject.IsInnerPart = stampedObject.IsInnerPart;
            newStampedObject.IsPartsAutoGenerated = stampedObject.IsPartsAutoGenerated;

            newStampedObject.CanAcceptPageObjects = stampedObject.CanAcceptPageObjects;
            //newStampedObject.AcceptedPageObjects = stampedObject.AcceptedPageObjects;  //  TODO: How necessary is this?
            newStampedObject.AcceptedPageObjectIDs = stampedObject.AcceptedPageObjectIDs;

            return newStampedObject;
        }

        // 12/7
        public static Stamp ConvertStamp(Ann.Stamp stamp, CLPPage newPage)
        {
            var newStamp = new Stamp
            {
                ID = stamp.ID,
                XPosition = stamp.XPosition,
                YPosition = stamp.YPosition,
                Height = stamp.Height,
                Width = stamp.Width,
                OwnerID = stamp.OwnerID,
                CreatorID = stamp.CreatorID,
                CreationDate = stamp.CreationDate,
                PageObjectFunctionalityVersion = "Ann12.19.2014",
                IsManipulatableByNonCreator = stamp.IsManipulatableByNonCreator,
                ParentPage = newPage
            };

            newStamp.ImageHashID = stamp.ImageHashID;

            switch (stamp.StampType)
            {
                case Ann.StampTypes.GeneralStamp:
                    newStamp.StampType = StampTypes.GeneralStamp;
                    break;
                case Ann.StampTypes.ObservingStamp:
                    newStamp.StampType = StampTypes.ObservingStamp;
                    break;
                case Ann.StampTypes.GroupStamp:
                    newStamp.StampType = StampTypes.GroupStamp;
                    break;
                case Ann.StampTypes.EmptyGroupStamp:
                    newStamp.StampType = StampTypes.EmptyGroupStamp;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            newStamp.Parts = stamp.Parts;
            newStamp.IsInnerPart = stamp.IsInnerPart;
            newStamp.IsPartsAutoGenerated = stamp.IsPartsAutoGenerated;

            newStamp.CanAcceptStrokes = stamp.CanAcceptStrokes;
            newStamp.AcceptedStrokes = stamp.AcceptedStrokes;  // TODO: Confirm this is necessary?
            newStamp.AcceptedStrokeParentIDs = stamp.AcceptedStrokeParentIDs;

            newStamp.CanAcceptPageObjects = stamp.CanAcceptPageObjects;
            //newStamp.AcceptedPageObjects = stamp.AcceptedPageObjects;  //  TODO: How necessary is this?
            newStamp.AcceptedPageObjectIDs = stamp.AcceptedPageObjectIDs;

            return newStamp;
        }

        // 12/12
        public static MultipleChoice ConvertMultipleChoiceBox(Ann.MultipleChoiceBox multipleChoiceBox, CLPPage newPage)
        {
            var newMultipleChoice = new MultipleChoice
            {
                ID = multipleChoiceBox.ID,
                XPosition = multipleChoiceBox.XPosition,
                YPosition = multipleChoiceBox.YPosition,
                Height = 35,
                Width = multipleChoiceBox.Width,
                OwnerID = multipleChoiceBox.OwnerID,
                CreatorID = multipleChoiceBox.CreatorID,
                CreationDate = multipleChoiceBox.CreationDate,
                PageObjectFunctionalityVersion = "Ann12.19.2014",
                IsManipulatableByNonCreator = multipleChoiceBox.IsManipulatableByNonCreator,
                ParentPage = newPage
            };

            newMultipleChoice.Orientation = MultipleChoiceOrientations.Horizontal;
            var segmentWidth = (multipleChoiceBox.Width - 35.0) / 3;
            newMultipleChoice.Width = segmentWidth * 4;

            // TODO: Custom conversion for Assessment Cache
            switch (newPage.PageNumber)
            {
                case 381:
                    {
                        var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = 0,
                            Answer = "8",
                            IsACorrectValue = true
                        };
                        var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth,
                            Answer = "7"
                        };
                        var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth * 2,
                            Answer = "4"
                        };
                        var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth * 3,
                            Answer = "2 + 4"
                        };
                        newMultipleChoice.ChoiceBubbles.Add(b1);
                        newMultipleChoice.ChoiceBubbles.Add(b2);
                        newMultipleChoice.ChoiceBubbles.Add(b3);
                        newMultipleChoice.ChoiceBubbles.Add(b4);
                    }
                    break;
                case 382:
                    {
                        var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = 0,
                            Answer = "2",
                            IsACorrectValue = true
                        };
                        var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth,
                            Answer = "3"
                        };
                        var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth * 2,
                            Answer = "4"
                        };
                        var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth * 3,
                            Answer = "6"
                        };
                        newMultipleChoice.ChoiceBubbles.Add(b1);
                        newMultipleChoice.ChoiceBubbles.Add(b2);
                        newMultipleChoice.ChoiceBubbles.Add(b3);
                        newMultipleChoice.ChoiceBubbles.Add(b4);
                    }
                    break;
                case 383:
                    {
                        var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = 0,
                            Answer = "3"
                        };
                        var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth,
                            Answer = "4"
                        };
                        var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth * 2,
                            Answer = "6"
                        };
                        var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth * 3,
                            Answer = "4",
                            IsACorrectValue = true
                        };
                        newMultipleChoice.ChoiceBubbles.Add(b1);
                        newMultipleChoice.ChoiceBubbles.Add(b2);
                        newMultipleChoice.ChoiceBubbles.Add(b3);
                        newMultipleChoice.ChoiceBubbles.Add(b4);
                    }
                    break;
                case 384:
                    {
                        var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = 0,
                            Answer = "9"
                        };
                        var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth,
                            Answer = "3",
                            IsACorrectValue = true
                        };
                        var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth * 2,
                            Answer = "6"
                        };
                        var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth * 3,
                            Answer = "7"
                        };
                        newMultipleChoice.ChoiceBubbles.Add(b1);
                        newMultipleChoice.ChoiceBubbles.Add(b2);
                        newMultipleChoice.ChoiceBubbles.Add(b3);
                        newMultipleChoice.ChoiceBubbles.Add(b4);
                    }
                    break;
                case 385:
                    {
                        var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = 0,
                            Answer = "8"
                        };
                        var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth,
                            Answer = "4"
                        };
                        var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth * 2,
                            Answer = "52"
                        };
                        var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                        {
                            Offset = segmentWidth * 3,
                            Answer = "6",
                            IsACorrectValue = true
                        };
                        newMultipleChoice.ChoiceBubbles.Add(b1);
                        newMultipleChoice.ChoiceBubbles.Add(b2);
                        newMultipleChoice.ChoiceBubbles.Add(b3);
                        newMultipleChoice.ChoiceBubbles.Add(b4);
                    }
                    break;
                default:
                    newMultipleChoice = null;
                    break;
            }

            return newMultipleChoice;
        }

        #endregion // PageObjects

        #region History

        public static void ConvertPageHistory(Ann.PageHistory pageHistory, CLPPage newPage)
        {
            var newPageHistory = new PageHistory();
            newPage.History = newPageHistory;
            foreach (var trashedPageObject in pageHistory.TrashedPageObjects)
            {
                var newTrashedPageObject = ConvertPageObject(trashedPageObject, newPage);
                newPageHistory.TrashedPageObjects.Add(newTrashedPageObject);
            }

            foreach (var trashedInkStroke in pageHistory.TrashedInkStrokes)
            {
                newPageHistory.TrashedInkStrokes.Add(trashedInkStroke);
            }

            if (pageHistory.RedoItems.Any())
            {
                Console.WriteLine($"[ERROR] PageHistory Has Redo Items. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}");
                return;
            }

            newPageHistory.IsAnimating = true;

            #region Undo

            var unconvertedUndoItems = pageHistory.UndoItems.ToList();
            while (unconvertedUndoItems.Any())
            {
                var historyItemToConvert = unconvertedUndoItems.FirstOrDefault();
                if (historyItemToConvert == null)
                {
                    break;
                }

                unconvertedUndoItems.RemoveFirst();
                var newHistoryAction = ConvertHistoryAction(historyItemToConvert, newPage, unconvertedUndoItems);
                if (newHistoryAction == null)
                {
                    continue;
                }
                newPageHistory.RedoActions.Insert(0, newHistoryAction);
            }

            #endregion // Undo

            #region Redo

            while (newPageHistory.RedoActions.Any())
            {
                // Multiple Choice Fill-In Status Updates
                var multipleChoiceStatus = newPageHistory.RedoActions.FirstOrDefault() as MultipleChoiceBubbleStatusChangedHistoryAction;
                if (multipleChoiceStatus != null)
                {
                    const int THRESHOLD = 80;
                    var multipleChoice = newPage.GetPageObjectByID(multipleChoiceStatus.MultipleChoiceID) as MultipleChoice;
                    var stroke = multipleChoiceStatus.StrokeIDsAdded.Any() ? multipleChoiceStatus.StrokesAdded.First() : multipleChoiceStatus.StrokesRemoved.First();
                    var choiceBubbleStrokeIsOver = multipleChoice.ChoiceBubbleStrokeIsOver(stroke);
                    var strokesOverBubble = multipleChoice.StrokesOverChoiceBubble(choiceBubbleStrokeIsOver);
                    var totalStrokeLength = strokesOverBubble.Sum(s => s.StylusPoints.Count);
                    if (multipleChoiceStatus.ChoiceBubbleStatus == ChoiceBubbleStatuses.FilledIn)
                    {
                        if (totalStrokeLength >= THRESHOLD)
                        {
                            multipleChoiceStatus.ChoiceBubbleStatus = ChoiceBubbleStatuses.AdditionalFilledIn;
                            choiceBubbleStrokeIsOver.IsFilledIn = true;
                        }
                        else
                        {
                            totalStrokeLength += stroke.StylusPoints.Count;
                            if (totalStrokeLength >= THRESHOLD)
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

                        if (totalStrokeLength < THRESHOLD)
                        {
                            multipleChoiceStatus.ChoiceBubbleStatus = ChoiceBubbleStatuses.ErasedPartiallyFilledIn;
                            choiceBubbleStrokeIsOver.IsFilledIn = false;
                        }
                        else
                        {
                            if (otherStrokesStrokeLength < THRESHOLD)
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

                newPageHistory.Redo();
            }

            #endregion // Redo

            newPageHistory.IsAnimating = false;
            newPageHistory.RefreshHistoryIndexes();
        }

        #endregion // History

        #region HistoryActions

        public static IHistoryAction ConvertHistoryAction(Ann.IHistoryItem historyItem, CLPPage newPage, List<Ann.IHistoryItem> unconvertedUndoItems)
        {
            IHistoryAction newHistoryAction = null;

            TypeSwitch.On(historyItem).Case<Ann.AnimationIndicator>(h =>
            {
                newHistoryAction = ConvertAndUndoAnimationIndicator(h, newPage);
            }).Case<Ann.CLPArrayRotateHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoArrayRotate(h, newPage);
            }).Case<Ann.CLPArrayGridToggleHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoArrayGridToggle(h, newPage);
            }).Case<Ann.CLPArraySnapHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoArraySnap(h, newPage);
            }).Case<Ann.CLPArrayDivisionValueChangedHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoArrayDivisionValueChanged(h, newPage);
            }).Case<Ann.CLPArrayDivisionsChangedHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoArrayDivisionsChanged(h, newPage);
            }).Case<Ann.StrokesChangedHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoStrokesChanged(h, newPage);
            }).Case<Ann.PageObjectsAddedHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoPageObjectAdded(h, newPage);
            }).Case<Ann.PageObjectsRemovedHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoPageObjectRemoved(h, newPage);
            }).Case<Ann.PageObjectResizeBatchHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoPageObjectResize(h, newPage);
            }).Case<Ann.PageObjectMoveBatchHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoPageObjectMove(h, newPage);
            }).Case<Ann.PageObjectsMoveBatchHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoPageObjectsMove(h, newPage);
            }).Case<Ann.PageObjectCutHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoPageObjectCut(h, newPage);
            }).Case<Ann.NumberLineEndPointsChangedHistoryItem>(h =>
            {
                newHistoryAction = ConvertAndUndoNumberLineEndPointsChange(h, newPage, unconvertedUndoItems);
            });

            if (newHistoryAction == null)
            {
                Debug.WriteLine($"[ERROR] newHistoryAction is NULL. Original historyItem is {historyItem.GetType()}. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
            }

            return newHistoryAction;
        }

        #region PageObject HistoryItems

        public static AnimationIndicatorHistoryAction ConvertAndUndoAnimationIndicator(Ann.AnimationIndicator historyItem, CLPPage newPage)
        {
            var newHistoryAction = new AnimationIndicatorHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            switch (historyItem.AnimationIndicatorType)
            {
                case Ann.AnimationIndicatorType.Record:
                    newHistoryAction.AnimationIndicatorType = AnimationIndicatorType.Record;
                    break;
                case Ann.AnimationIndicatorType.Stop:
                    newHistoryAction.AnimationIndicatorType = AnimationIndicatorType.Stop;
                    break;
                default:
                    newHistoryAction.AnimationIndicatorType = AnimationIndicatorType.Record;
                    break;
            }

            return newHistoryAction;
        }

        public static ObjectsOnPageChangedHistoryAction ConvertAndUndoPageObjectAdded(Ann.PageObjectsAddedHistoryItem historyItem, CLPPage newPage)
        {
            if (!historyItem.PageObjectIDs.Any())
            {
                Debug.WriteLine($"[NON-ERROR] PageObject Added, no pageObjects added. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var newHistoryAction = new ObjectsOnPageChangedHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            newHistoryAction.PageObjectIDsAdded = historyItem.PageObjectIDs;

            #region Conversion Undo

            foreach (var pageObject in newHistoryAction.PageObjectIDsAdded.Select(newPage.GetVerifiedPageObjectOnPageByID))
            {
                if (pageObject == null)
                {
                    Debug.WriteLine($"[ERROR] PageObject for PageObject Added not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    return null;
                }
                newPage.PageObjects.Remove(pageObject);
                pageObject.OnDeleted(true);
                newPage.History.TrashedPageObjects.Add(pageObject);
            }

            #endregion // Conversion Undo

            return newHistoryAction;
        }

        public static ObjectsOnPageChangedHistoryAction ConvertAndUndoPageObjectRemoved(Ann.PageObjectsRemovedHistoryItem historyItem, CLPPage newPage)
        {
            if (!historyItem.PageObjectIDs.Any())
            {
                Debug.WriteLine($"[NON-ERROR] PageObject Removed, no pageObjects added. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var newHistoryAction = new ObjectsOnPageChangedHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            newHistoryAction.PageObjectIDsRemoved = historyItem.PageObjectIDs;

            #region Conversion Undo

            foreach (var pageObject in newHistoryAction.PageObjectIDsRemoved.Select(newPage.GetVerifiedPageObjectInTrashByID))
            {
                if (pageObject == null)
                {
                    Debug.WriteLine($"[ERROR] PageObject for PageObject Removed not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    return null;
                }
                newPage.History.TrashedPageObjects.Remove(pageObject);
                newPage.PageObjects.Add(pageObject);
                pageObject.OnAdded(true);
            }

            #endregion // Conversion Undo

            return newHistoryAction;
        }

        public static PageObjectResizeBatchHistoryAction ConvertAndUndoPageObjectResize(Ann.PageObjectResizeBatchHistoryItem historyItem, CLPPage newPage)
        {
            // TODO: Revisit after NL end change to see if this causes any issues, also see about having only 1 stretched dimension point being ignored as well.
            if (!historyItem.StretchedDimensions.Any())
            {
                Debug.WriteLine($"[NON-ERROR] PageObject Resize has no Streched Dimensions. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var newHistoryAction = new PageObjectResizeBatchHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            newHistoryAction.PageObjectID = historyItem.PageObjectID;
            newHistoryAction.StretchedDimensions = historyItem.StretchedDimensions.ToList();

            #region Conversion Undo

            var pageObject = newPage.GetVerifiedPageObjectOnPageByID(newHistoryAction.PageObjectID);
            if (pageObject == null)
            {
                Debug.WriteLine($"[ERROR] PageObject for PageObject Resize not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var initialWidth = pageObject.Width;
            var initialHeight = pageObject.Height;

            pageObject.Width = newHistoryAction.OriginalWidth;
            pageObject.Height = newHistoryAction.OriginalHeight;

            pageObject.OnResized(initialWidth, initialHeight, true);

            newHistoryAction.CurrentBatchTickIndex = -1;

            #endregion // Conversion Undo

            return newHistoryAction;
        }

        public static ObjectsMovedBatchHistoryAction ConvertAndUndoPageObjectMove(Ann.PageObjectMoveBatchHistoryItem historyItem, CLPPage newPage)
        {
            if (string.IsNullOrEmpty(historyItem.PageObjectID))
            {
                Debug.WriteLine($"[NON-ERROR] PageObject Move has NULL PageObjectID. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            if (!historyItem.TravelledPositions.Any())
            {
                Debug.WriteLine($"[NON-ERROR] PageObject Move has no Travelled Positions. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            if (historyItem.TravelledPositions.Count == 2 &&
                Math.Abs(historyItem.TravelledPositions.First().X - historyItem.TravelledPositions.Last().X) < 0.00001 &&
                Math.Abs(historyItem.TravelledPositions.First().Y - historyItem.TravelledPositions.Last().Y) < 0.00001)
            {
                Debug.WriteLine($"[NON-ERROR] PageObject Move has the same Travelled Positions. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var newHistoryAction = new ObjectsMovedBatchHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            newHistoryAction.PageObjectIDs = new Dictionary<string, Point>
                                             {
                                                 { historyItem.PageObjectID, new Point(0.0, 0.0) }
                                             };

            newHistoryAction.TravelledPositions = historyItem.TravelledPositions.ToList();

            #region Conversion Undo

            foreach (var pageObjectID in newHistoryAction.PageObjectIDs)
            {
                var pageObject = newPage.GetVerifiedPageObjectOnPageByID(pageObjectID.Key);
                if (pageObject == null)
                {
                    Debug.WriteLine($"[ERROR] PageObject for PageObject Move not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    return null;
                }

                var initialX = pageObject.XPosition;
                var initialY = pageObject.YPosition;

                var originalPosition = newHistoryAction.TravelledPositions.First();

                pageObject.XPosition = originalPosition.X + pageObjectID.Value.X;
                pageObject.YPosition = originalPosition.Y + pageObjectID.Value.Y;

                pageObject.OnMoved(initialX, initialY, true);
            }

            newHistoryAction.CurrentBatchTickIndex = -1;

            #endregion // Conversion Undo

            return newHistoryAction;
        }

        public static ObjectsMovedBatchHistoryAction ConvertAndUndoPageObjectsMove(Ann.PageObjectsMoveBatchHistoryItem historyItem, CLPPage newPage)
        {
            if (!historyItem.PageObjectIDs.Any())
            {
                Debug.WriteLine($"[NON-ERROR] PageObjects Move has no PageObjectIDs. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            if (!historyItem.TravelledPositions.Any())
            {
                Debug.WriteLine($"[NON-ERROR] PageObjects Move has no Travelled Positions. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            if (historyItem.TravelledPositions.Count == 2 &&
                Math.Abs(historyItem.TravelledPositions.First().X - historyItem.TravelledPositions.Last().X) < 0.00001 &&
                Math.Abs(historyItem.TravelledPositions.First().Y - historyItem.TravelledPositions.Last().Y) < 0.00001)
            {
                Debug.WriteLine($"[NON-ERROR] PageObjects Move has the same Travelled Positions. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var newHistoryAction = new ObjectsMovedBatchHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            var pageObjects = historyItem.PageObjectIDs.Select(newPage.GetVerifiedPageObjectOnPageByID).Where(p => p != null).ToList();
            var referencePageObject = pageObjects.First();
            var pageObjectIDs = pageObjects.ToDictionary(p => p.ID, p => new Point(p.XPosition - referencePageObject.XPosition, p.YPosition - referencePageObject.YPosition));
            newHistoryAction.PageObjectIDs = pageObjectIDs;
            newHistoryAction.TravelledPositions = historyItem.TravelledPositions.ToList();

            #region Conversion Undo

            foreach (var pageObjectID in newHistoryAction.PageObjectIDs)
            {
                var pageObject = newPage.GetVerifiedPageObjectOnPageByID(pageObjectID.Key);
                if (pageObject == null)
                {
                    Debug.WriteLine($"[ERROR] PageObjects for PageObject Move not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    return null;
                }

                var initialX = pageObject.XPosition;
                var initialY = pageObject.YPosition;

                var originalPosition = newHistoryAction.TravelledPositions.First();

                pageObject.XPosition = originalPosition.X + pageObjectID.Value.X;
                pageObject.YPosition = originalPosition.Y + pageObjectID.Value.Y;

                pageObject.OnMoved(initialX, initialY, true);
            }

            newHistoryAction.CurrentBatchTickIndex = -1;

            #endregion // Conversion Undo

            return newHistoryAction;
        }

        public static PageObjectCutHistoryAction ConvertAndUndoPageObjectCut(Ann.PageObjectCutHistoryItem historyItem, CLPPage newPage)
        {
            if (string.IsNullOrEmpty(historyItem.CuttingStrokeID))
            {
                Debug.WriteLine($"[NON-ERROR] PageObject Cut has NULL Cutting Stroke ID. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var cuttingStroke = newPage.GetVerifiedStrokeInHistoryByID(historyItem.CuttingStrokeID);
            if (cuttingStroke == null)
            {
                Debug.WriteLine($"[NON-ERROR] PageObject Cut has NULL Cutting Stroke. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            #region No Or One PageObject Cut

            if (historyItem.CutPageObjectIDs.Count <= 1)
            {
                var newHistoryAction = new PageObjectCutHistoryAction
                                       {
                                           ID = historyItem.ID,
                                           OwnerID = historyItem.OwnerID,
                                           ParentPage = newPage
                                       };

                newHistoryAction.CuttingStrokeID = historyItem.CuttingStrokeID;

                if (historyItem.CutPageObjectIDs.Any())
                {
                    newHistoryAction.CutPageObjectID = historyItem.CutPageObjectIDs.First();

                    if (historyItem.HalvedPageObjectIDs.Count < 2)
                    {
                        newHistoryAction.CutPageObjectID = string.Empty;
                        return newHistoryAction;
                    }

                    if (historyItem.HalvedPageObjectIDs.Count > 2)
                    {
                        Debug.WriteLine($"[ERROR] PageObject Cut has one Cut PageObject, but more than 2 Halved PageObjects. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                        return null;
                    }

                    newHistoryAction.HalvedPageObjectIDs = historyItem.HalvedPageObjectIDs.ToList();
                }

                if (string.IsNullOrEmpty(newHistoryAction.CutPageObjectID) ||
                    !newHistoryAction.HalvedPageObjectIDs.Any())
                {
                    return newHistoryAction;
                }

                #region Conversion Undo

                var halvedPageObjects = newHistoryAction.HalvedPageObjectIDs.Select(newPage.GetVerifiedPageObjectOnPageByID).ToList();
                foreach (var halvedPageObject in halvedPageObjects)
                {
                    if (halvedPageObject == null)
                    {
                        Debug.WriteLine($"[ERROR] Halved PageObject for PageObject Cut not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                        return null;
                    }
                    newPage.PageObjects.Remove(halvedPageObject);
                    newPage.History.TrashedPageObjects.Add(halvedPageObject);
                }

                var cutPageObject = newPage.GetVerifiedPageObjectInTrashByID(newHistoryAction.CutPageObjectID);
                if (cutPageObject == null)
                {
                    Debug.WriteLine($"[ERROR] Cut PageObject for PageObject Cut not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    return null;
                }

                newPage.History.TrashedPageObjects.Remove(cutPageObject);
                newPage.PageObjects.Add(cutPageObject);

                AStrokeAccepter.SplitAcceptedStrokes(halvedPageObjects,
                                                     new List<IPageObject>
                                                     {
                                                         cutPageObject
                                                     });

                APageObjectAccepter.SplitAcceptedPageObjects(halvedPageObjects,
                                                             new List<IPageObject>
                                                             {
                                                                 cutPageObject
                                                             });

                #endregion // Conversion Undo

                return newHistoryAction;
            }

            #endregion // No Or One PageObject Cut

            #region Multiple PageObjects Cut

            if (historyItem.CutPageObjectIDs.Count * 2 != historyItem.HalvedPageObjectIDs.Count)
            {
                Debug.WriteLine($"[ERROR] PageObject Cut has mismatched number of Cut PageObjects and Halved PageObjects. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var newHistoryActions = new List<PageObjectCutHistoryAction>();
            foreach (var historyItemCutPageObjectID in historyItem.CutPageObjectIDs)
            {
                var newHistoryAction = new PageObjectCutHistoryAction
                                       {
                                           ID = historyItem.ID,
                                           OwnerID = historyItem.OwnerID,
                                           ParentPage = newPage
                                       };

                newHistoryAction.CuttingStrokeID = historyItem.CuttingStrokeID;

                newHistoryAction.CutPageObjectID = historyItemCutPageObjectID;
                var cutPageObject = newPage.GetVerifiedPageObjectInTrashByID(historyItemCutPageObjectID) as ICuttable;
                if (cutPageObject == null)
                {
                    Debug.WriteLine($"[ERROR] Cut PageObject on PageObject Cut not found in history or on page. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    return null;
                }

                var halvedPageObjectIDs = new List<string>
                                          {
                                              historyItem.HalvedPageObjectIDs[0],
                                              historyItem.HalvedPageObjectIDs[1]
                                          };
                historyItem.HalvedPageObjectIDs.RemoveRange(0, 2);
                newHistoryAction.HalvedPageObjectIDs = halvedPageObjectIDs;
                
                newHistoryActions.Add(newHistoryAction);
            }

            // Undo all in correct order then add to redo items, saving last one as return value

            #region Conversion Undo

            var lastHistoryAction = newHistoryActions.Last();

            foreach (var historyAction in newHistoryActions)
            {
                var halvedPageObjects = historyAction.HalvedPageObjectIDs.Select(newPage.GetVerifiedPageObjectOnPageByID).ToList();
                foreach (var halvedPageObject in halvedPageObjects)
                {
                    if (halvedPageObject == null)
                    {
                        Debug.WriteLine($"[ERROR] Halved PageObject for PageObject Cut not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                        return null;
                    }
                    newPage.PageObjects.Remove(halvedPageObject);
                    newPage.History.TrashedPageObjects.Add(halvedPageObject);
                }

                var cutPageObject = newPage.GetVerifiedPageObjectInTrashByID(historyAction.CutPageObjectID);
                if (cutPageObject == null)
                {
                    Debug.WriteLine($"[ERROR] Cut PageObject for PageObject Cut not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    return null;
                }

                newPage.History.TrashedPageObjects.Remove(cutPageObject);
                newPage.PageObjects.Add(cutPageObject);

                AStrokeAccepter.SplitAcceptedStrokes(halvedPageObjects,
                                                     new List<IPageObject>
                                                     {
                                                         cutPageObject
                                                     });

                APageObjectAccepter.SplitAcceptedPageObjects(halvedPageObjects,
                                                             new List<IPageObject>
                                                             {
                                                                 cutPageObject
                                                             });

                if (historyAction != lastHistoryAction)
                {
                    newPage.History.RedoActions.Insert(0, historyAction);
                }
            }

            #endregion // Conversion Undo

            return lastHistoryAction;

            #endregion // Multiple PageObjects Cut
        }

        #endregion // PageObject HistoryItems

        #region Array HistoryItems

        public static CLPArrayRotateHistoryAction ConvertAndUndoArrayRotate(Ann.CLPArrayRotateHistoryItem historyItem, CLPPage newPage)
        {
            var newHistoryAction = new CLPArrayRotateHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            newHistoryAction.ArrayID = historyItem.ArrayID;

            #region Conversion Undo

            var array = newPage.GetVerifiedPageObjectOnPageByID(newHistoryAction.ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Debug.WriteLine($"[ERROR] Array for Rotate not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            newHistoryAction.NewXPosition = array.XPosition;
            newHistoryAction.NewYPosition = array.YPosition;
            newHistoryAction.NewWidth = array.Width;
            newHistoryAction.NewHeight = array.Height;
            array.RotateArray();
            array.XPosition = historyItem.ArrayXCoord;
            array.YPosition = historyItem.ArrayYCoord;
            newHistoryAction.OldXPosition = historyItem.ArrayXCoord;
            newHistoryAction.OldYPosition = historyItem.ArrayYCoord;
            newHistoryAction.OldWidth = array.Width;
            newHistoryAction.OldHeight = array.Height;
            newHistoryAction.OldRows = array.Rows;
            newHistoryAction.OldColumns = array.Columns;

            #endregion // Conversion Undo

            return newHistoryAction;
        }

        public static CLPArrayGridToggleHistoryAction ConvertAndUndoArrayGridToggle(Ann.CLPArrayGridToggleHistoryItem historyItem, CLPPage newPage)
        {
            var newHistoryAction = new CLPArrayGridToggleHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            newHistoryAction.ArrayID = historyItem.ArrayID;

            #region Conversion Undo

            var array = newPage.GetVerifiedPageObjectOnPageByID(newHistoryAction.ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Debug.WriteLine($"[ERROR] Array for Grid Toggle not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            newHistoryAction.IsToggledOn = array.IsGridOn;
            array.IsGridOn = !newHistoryAction.IsToggledOn;

            #endregion // Conversion Undo

            return newHistoryAction;
        }

        public static CLPArraySnapHistoryAction ConvertAndUndoArraySnap(Ann.CLPArraySnapHistoryItem historyItem, CLPPage newPage)
        {
            var newHistoryAction = new CLPArraySnapHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            newHistoryAction.PersistingArrayID = historyItem.PersistingArrayID;
            newHistoryAction.SnappedArrayID = historyItem.SnappedArrayID;
            newHistoryAction.IsHorizontal = historyItem.IsHorizontal;
            newHistoryAction.SnappedArraySquareSize = historyItem.SnappedArraySquareSize;
            newHistoryAction.PersistingArrayDivisionBehavior = historyItem.PersistingArrayDivisionBehavior;
            newHistoryAction.PersistingArrayRowsOrColumns = historyItem.PersistingArrayRowsOrColumns;
            newHistoryAction.PersistingArrayXOrYPosition = historyItem.PersistingArrayXOrYPosition;

            newHistoryAction.PersistingArrayHorizontalDivisions =
                historyItem.PersistingArrayHorizontalDivisions.Select(
                                                                      d =>
                                                                          new CLPArrayDivision(
                                                                                               d.Orientation == Ann.ArrayDivisionOrientation.Horizontal
                                                                                                   ? ArrayDivisionOrientation.Horizontal
                                                                                                   : ArrayDivisionOrientation.Vertical,
                                                                                               d.Position,
                                                                                               d.Length,
                                                                                               d.Value)).ToList();
            newHistoryAction.PersistingArrayVerticalDivisions =
                historyItem.PersistingArrayVerticalDivisions.Select(
                                                                    d =>
                                                                        new CLPArrayDivision(
                                                                                             d.Orientation == Ann.ArrayDivisionOrientation.Horizontal
                                                                                                 ? ArrayDivisionOrientation.Horizontal
                                                                                                 : ArrayDivisionOrientation.Vertical,
                                                                                             d.Position,
                                                                                             d.Length,
                                                                                             d.Value)).ToList();

            #region Conversion Undo

            var persistingArray = newPage.GetVerifiedPageObjectOnPageByID(newHistoryAction.PersistingArrayID) as CLPArray;
            if (persistingArray == null)
            {
                Debug.WriteLine($"[ERROR] Persisting Array for Snap not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var snappedArray = newPage.GetVerifiedPageObjectInTrashByID(newHistoryAction.SnappedArrayID) as CLPArray;
            if (snappedArray == null)
            {
                Debug.WriteLine($"[ERROR] Snapped Array for Snap not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            snappedArray.SizeArrayToGridLevel(newHistoryAction.SnappedArraySquareSize);
            snappedArray.ParentPage = newPage;
            newPage.PageObjects.Add(snappedArray);
            newPage.History.TrashedPageObjects.Remove(snappedArray);

            var persistingArrayGridSquareSize = persistingArray.GridSquareSize;

            newHistoryAction.RestoreDivisions(persistingArray);
            newHistoryAction.RestoreDimensionsAndPosition(persistingArray);

            persistingArray.IsDivisionBehaviorOn = newHistoryAction.PersistingArrayDivisionBehavior;
            persistingArray.SizeArrayToGridLevel(persistingArrayGridSquareSize, false);

            var oldPageObjects = new List<IPageObject>
                                 {
                                     persistingArray
                                 };
            var newPageObjects = new List<IPageObject>
                                 {
                                     persistingArray,
                                     snappedArray
                                 };

            AStrokeAccepter.SplitAcceptedStrokes(oldPageObjects, newPageObjects);
            APageObjectAccepter.SplitAcceptedPageObjects(oldPageObjects, newPageObjects);

            #endregion // Conversion Undo

            return newHistoryAction;
        }

        public static CLPArrayDivisionValueChangedHistoryAction ConvertAndUndoArrayDivisionValueChanged(Ann.CLPArrayDivisionValueChangedHistoryItem historyItem, CLPPage newPage)
        {
            var newHistoryAction = new CLPArrayDivisionValueChangedHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            newHistoryAction.ArrayID = historyItem.ArrayID;
            newHistoryAction.IsHorizontalDivision = historyItem.IsHorizontalDivision;
            newHistoryAction.DivisionIndex = historyItem.DivisionIndex;
            newHistoryAction.PreviousValue = historyItem.PreviousValue;

            #region Conversion Undo

            var array = newPage.GetVerifiedPageObjectOnPageByID(newHistoryAction.ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Debug.WriteLine($"[ERROR] Array for Division Value Changed not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            try
            {
                var division = newHistoryAction.IsHorizontalDivision ? array.HorizontalDivisions[newHistoryAction.DivisionIndex] : array.VerticalDivisions[newHistoryAction.DivisionIndex];

                newHistoryAction.NewValue = division.Value;
                division.Value = newHistoryAction.PreviousValue;
            }
            catch (Exception)
            {
                Debug.WriteLine($"[ERROR] Division Value Changed, Division Index out of bounds. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            #endregion // Conversion Undo

            return newHistoryAction;
        }

        public static CLPArrayDivisionsChangedHistoryAction ConvertAndUndoArrayDivisionsChanged(Ann.CLPArrayDivisionsChangedHistoryItem historyItem, CLPPage newPage)
        {
            if (!historyItem.AddedDivisions.Any() &&
                !historyItem.RemovedDivisions.Any())
            {
                Debug.WriteLine($"[NON-ERROR] Division Values Changed, empty divisions. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var newHistoryAction = new CLPArrayDivisionsChangedHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            newHistoryAction.ArrayID = historyItem.ArrayID;

            #region Conversion Undo

            var array = newPage.GetVerifiedPageObjectOnPageByID(newHistoryAction.ArrayID) as ACLPArrayBase;
            if (array == null)
            {
                Debug.WriteLine($"[ERROR] Array for Divisions Changed not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            if ((historyItem.AddedDivisions.Any() && historyItem.AddedDivisions[0].Orientation == Ann.ArrayDivisionOrientation.Horizontal) ||
                (historyItem.RemovedDivisions.Any() && historyItem.RemovedDivisions[0].Orientation == Ann.ArrayDivisionOrientation.Horizontal))
            {
                newHistoryAction.NewRegions = array.HorizontalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();

                foreach (var clpArrayDivision in historyItem.AddedDivisions)
                {
                    var matchingArrayDivision =
                        array.HorizontalDivisions.FirstOrDefault(d => d.Length == clpArrayDivision.Length && d.Position == clpArrayDivision.Position && d.Value == clpArrayDivision.Value);

                    array.HorizontalDivisions.Remove(matchingArrayDivision);
                }
                foreach (var clpArrayDivision in historyItem.RemovedDivisions)
                {
                    var newDivision = ConvertArrayDivision(clpArrayDivision);

                    array.HorizontalDivisions.Add(newDivision);
                }

                newHistoryAction.OldRegions = array.HorizontalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            }
            else
            {
                newHistoryAction.NewRegions = array.VerticalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();

                foreach (var clpArrayDivision in historyItem.AddedDivisions)
                {
                    var matchingArrayDivision =
                        array.VerticalDivisions.FirstOrDefault(d => d.Length == clpArrayDivision.Length && d.Position == clpArrayDivision.Position && d.Value == clpArrayDivision.Value);

                    array.VerticalDivisions.Remove(matchingArrayDivision);
                }
                foreach (var clpArrayDivision in historyItem.RemovedDivisions)
                {
                    var newDivision = ConvertArrayDivision(clpArrayDivision);

                    array.VerticalDivisions.Add(newDivision);
                }

                newHistoryAction.OldRegions = array.VerticalDivisions.Select(d => new CLPArrayDivision(d.Orientation, d.Position, d.Length, d.Value, d.IsObscured)).ToList();
            }

            #endregion // Conversion Undo

            return newHistoryAction;
        }

        #endregion // Array HistoryItems

        #region Number Line HistoryItems

        public static NumberLineEndPointsChangedHistoryAction ConvertAndUndoNumberLineEndPointsChange(Ann.NumberLineEndPointsChangedHistoryItem historyItem, CLPPage newPage, List<Ann.IHistoryItem> unconvertedUndoItems)
        {
            // BUG: Original code pulled resizeAction from RedoActions, doesn't seem like that would have been accurate.
            var nextUnconvertedHistoryItem = unconvertedUndoItems.FirstOrDefault();
            if (!(nextUnconvertedHistoryItem is Ann.NumberLineEndPointsChangedHistoryItem) && 
                !(nextUnconvertedHistoryItem is Ann.PageObjectResizeBatchHistoryItem))
            {
                Debug.WriteLine($"[ERROR] Number Line End Point Change not followed by PageObject Resize or another Number Line End Point Change. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var numberLine = newPage.GetVerifiedPageObjectOnPageByID(historyItem.NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                Debug.WriteLine($"[ERROR] Number Line for Number Line End Point Change not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var resizeBatchHistoryItem = nextUnconvertedHistoryItem as Ann.PageObjectResizeBatchHistoryItem;
            if (resizeBatchHistoryItem != null)
            {
                var potentialNumberLineMatch = newPage.GetVerifiedPageObjectOnPageByID(resizeBatchHistoryItem.PageObjectID) as NumberLine;
                if (potentialNumberLineMatch == null ||
                    numberLine.ID != potentialNumberLineMatch.ID)
                {
                    Debug.WriteLine($"[ERROR] Number Line for Number Line End Point Change doesn't match next PageObject Resize Number Line. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    return null;
                }

                unconvertedUndoItems.RemoveFirst();
            }
           
            var newHistoryAction = new NumberLineEndPointsChangedHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            newHistoryAction.NumberLineID = historyItem.NumberLineID;
            newHistoryAction.PreviousStartValue = historyItem.PreviousStartValue;
            newHistoryAction.PreviousEndValue = historyItem.PreviousEndValue;
            newHistoryAction.NewEndValue = numberLine.NumberLineSize;
            newHistoryAction.NewStretchedWidth = numberLine.Width;

            #region Conversion Undo

            if (resizeBatchHistoryItem == null)
            {
                newHistoryAction.PreStretchedWidth = numberLine.Width;
                numberLine.ChangeNumberLineSize(newHistoryAction.PreviousEndValue);
            }
            else
            {
                var previousWidth = resizeBatchHistoryItem.StretchedDimensions.First().X;
                var previousNumberLineWidth = previousWidth - (numberLine.ArrowLength * 2);
                var previousTickLength = previousNumberLineWidth / newHistoryAction.PreviousEndValue;

                var preStretchedWidth = previousWidth + (previousTickLength * (newHistoryAction.NewEndValue - newHistoryAction.PreviousEndValue));
                if (Math.Abs(numberLine.Width - preStretchedWidth) < numberLine.TickLength / 2)
                {
                    preStretchedWidth = numberLine.Width;
                }

                newHistoryAction.PreStretchedWidth = preStretchedWidth;

                if (Math.Abs(newHistoryAction.NewStretchedWidth - newHistoryAction.PreStretchedWidth) >= 0.0001)
                {
                    var oldWidth = numberLine.Width;
                    var oldHeight = numberLine.Height;
                    numberLine.Width = newHistoryAction.PreStretchedWidth;
                    numberLine.OnResized(oldWidth, oldHeight, true);
                }

                numberLine.ChangeNumberLineSize(newHistoryAction.PreviousEndValue);
            }

            #endregion // Conversion Undo

            return newHistoryAction;
        }

        #endregion // Number Line HistoryItems

        #region Strokes HistoryItems

        public static IHistoryAction ConvertAndUndoStrokesChanged(Ann.StrokesChangedHistoryItem historyItem, CLPPage newPage)
        {
            if (!historyItem.StrokeIDsAdded.Any() &&
                !historyItem.StrokeIDsRemoved.Any())
            {
                Debug.WriteLine($"[NON-ERROR] Strokes Changed, no strokes changed. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var newHistoryAction = new ObjectsOnPageChangedHistoryAction
                                   {
                                       ID = historyItem.ID,
                                       OwnerID = historyItem.OwnerID,
                                       ParentPage = newPage
                                   };

            newHistoryAction.StrokeIDsAdded = historyItem.StrokeIDsAdded;
            newHistoryAction.StrokeIDsRemoved = historyItem.StrokeIDsRemoved;

            // Single Add
            if (newHistoryAction.StrokeIDsAdded.Count == 1 &&
                !newHistoryAction.StrokeIDsRemoved.Any())
            {
                var strokeID = newHistoryAction.StrokeIDsAdded.First();
                var addedStroke = newPage.GetVerifiedStrokeOnPageByID(strokeID);

                #region Check for Jump Added

                foreach (var numberLine in newPage.PageObjects.OfType<NumberLine>())
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

                    var jumpsChangedHistoryAction = new NumberLineJumpSizesChangedHistoryAction(newPage,
                                                                                                newPage.Owner,
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

                    #region JumpsChangedHistoryAction Conversion Undo

                    foreach (var stroke in jumpsChangedHistoryAction.AddedJumpStrokeIDs.Select(newPage.GetVerifiedStrokeOnPageByID))
                    {
                        if (stroke == null)
                        {
                            Debug.WriteLine($"[ERROR] Strokes Changed, Stroke in AddedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                            continue;
                        }

                        newPage.InkStrokes.Remove(stroke);
                        newPage.History.TrashedInkStrokes.Add(stroke);
                        numberLine.ChangeAcceptedStrokes(new List<Stroke>(),
                                                         new List<Stroke>
                                                         {
                                                             stroke
                                                         });

                        var jumps = numberLine.RemoveJumpFromStroke(stroke);
                        jumpsChangedHistoryAction.JumpsAdded = jumps;
                    }

                    numberLine.YPosition = jumpsChangedHistoryAction.PreviousYPosition;
                    numberLine.Height = jumpsChangedHistoryAction.PreviousHeight;

                    #endregion // JumpsChangedHistoryAction Conversion Undo

                    return jumpsChangedHistoryAction;
                }

                #endregion // Check for Jump Added

                #region Check for Multiple Choice Fill-In

                var multipleChoice = newPage.PageObjects.FirstOrDefault(p => p is MultipleChoice) as MultipleChoice;
                if (multipleChoice != null)
                {
                    var choiceBubbleStrokeIsOver = multipleChoice.ChoiceBubbleStrokeIsOver(addedStroke);
                    if (choiceBubbleStrokeIsOver != null)
                    {
                        var index = multipleChoice.ChoiceBubbles.IndexOf(choiceBubbleStrokeIsOver);
                        multipleChoice.ChangeAcceptedStrokes(newHistoryAction.StrokesAdded, newHistoryAction.StrokesRemoved);
                        var multipleChoiceBubbleStatusChangedHistoryAction = new MultipleChoiceBubbleStatusChangedHistoryAction(newPage,
                                                                                                                                newPage.Owner,
                                                                                                                                multipleChoice,
                                                                                                                                index,
                                                                                                                                ChoiceBubbleStatuses.FilledIn,
                                                                                                                                newHistoryAction.StrokesAdded,
                                                                                                                                newHistoryAction.StrokesRemoved);

                        #region MultipleChoiceBubbleStatusChangedHistoryAction Conversion Undo

                        var addedStrokesToMultipleChoice = new List<Stroke>();
                        foreach (var stroke in multipleChoiceBubbleStatusChangedHistoryAction.StrokeIDsAdded.Select(newPage.GetVerifiedStrokeOnPageByID))
                        {
                            if (stroke == null)
                            {
                                Debug.WriteLine($"[ERROR] Strokes Changed, Stroke in StrokeIDsAdded in MultipleChoiceBubbleStatusChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                                continue;
                            }

                            addedStrokesToMultipleChoice.Add(stroke);
                            newPage.InkStrokes.Remove(stroke);
                            newPage.History.TrashedInkStrokes.Add(stroke);
                        }

                        var removedStrokesToMultipleChoice = new List<Stroke>();
                        foreach (var stroke in multipleChoiceBubbleStatusChangedHistoryAction.StrokeIDsRemoved.Select(newPage.GetVerifiedStrokeInHistoryByID))
                        {
                            if (stroke == null)
                            {
                                Debug.WriteLine($"[ERROR] Strokes Changed, Stroke in StrokeIDsRemoved in MultipleChoiceBubbleStatusChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                                continue;
                            }

                            removedStrokesToMultipleChoice.Add(stroke);
                            newPage.History.TrashedInkStrokes.Remove(stroke);
                            newPage.InkStrokes.Add(stroke);
                        }

                        multipleChoice.ChangeAcceptedStrokes(removedStrokesToMultipleChoice, addedStrokesToMultipleChoice);

                        switch (multipleChoiceBubbleStatusChangedHistoryAction.ChoiceBubbleStatus)
                        {
                            case ChoiceBubbleStatuses.CompletelyErased:
                                multipleChoiceBubbleStatusChangedHistoryAction.Bubble.IsFilledIn = true;
                                break;
                            case ChoiceBubbleStatuses.FilledIn:
                                multipleChoiceBubbleStatusChangedHistoryAction.Bubble.IsFilledIn = false;
                                break;
                        }

                        #endregion // MultipleChoiceBubbleStatusChangedHistoryAction Conversion Undo

                        return multipleChoiceBubbleStatusChangedHistoryAction;
                    }
                }

                #endregion // Check for Multiple Choice Fill-In
            }
            //Single Remove
            else if (newHistoryAction.StrokeIDsRemoved.Count == 1 &&
                     !newHistoryAction.StrokeIDsAdded.Any())
            {
                var strokeID = newHistoryAction.StrokeIDsRemoved.First();
                var removedStroke = newPage.GetVerifiedStrokeInHistoryByID(strokeID);

                #region Check for Jump Removed

                foreach (var numberLine in newPage.PageObjects.OfType<NumberLine>())
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

                    var jumpsChangedHistoryAction = new NumberLineJumpSizesChangedHistoryAction(newPage,
                                                                                                newPage.Owner,
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

                    #region JumpsChangedHistoryAction Conversion Undo

                    foreach (var stroke in jumpsChangedHistoryAction.RemovedJumpStrokeIDs.Select(newPage.GetVerifiedStrokeInHistoryByID))
                    {
                        if (stroke == null)
                        {
                            Debug.WriteLine($"[ERROR] Strokes Changed, Stroke in RemovedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                            continue;
                        }
                        newPage.History.TrashedInkStrokes.Remove(stroke);
                        newPage.InkStrokes.Add(stroke);
                        numberLine.ChangeAcceptedStrokes(new List<Stroke>
                                                         {
                                                             stroke
                                                         },
                                                         new List<Stroke>());

                        var jumps = numberLine.AddJumpFromStroke(stroke);
                        jumpsChangedHistoryAction.JumpsRemoved = jumps;
                    }

                    numberLine.YPosition = jumpsChangedHistoryAction.PreviousYPosition;
                    numberLine.Height = jumpsChangedHistoryAction.PreviousHeight;

                    #endregion // JumpsChangedHistoryAction Conversion Undo

                    return jumpsChangedHistoryAction;
                }

                #endregion // Check for Jump Removed

                #region Check for Multiple Choice Erase

                var multipleChoice = newPage.PageObjects.FirstOrDefault(p => p is MultipleChoice) as MultipleChoice;
                if (multipleChoice != null)
                {
                    var choiceBubbleStrokeIsOver = multipleChoice.ChoiceBubbleStrokeIsOver(removedStroke);
                    if (choiceBubbleStrokeIsOver != null)
                    {
                        var index = multipleChoice.ChoiceBubbles.IndexOf(choiceBubbleStrokeIsOver);
                        multipleChoice.ChangeAcceptedStrokes(newHistoryAction.StrokesAdded, newHistoryAction.StrokesRemoved);
                        var multipleChoiceBubbleStatusChangedHistoryAction = new MultipleChoiceBubbleStatusChangedHistoryAction(newPage,
                                                                                                                                newPage.Owner,
                                                                                                                                multipleChoice,
                                                                                                                                index,
                                                                                                                                ChoiceBubbleStatuses.CompletelyErased,
                                                                                                                                newHistoryAction.StrokesAdded,
                                                                                                                                newHistoryAction.StrokesRemoved);
                        #region MultipleChoiceBubbleStatusChangedHistoryAction Conversion Undo

                        var addedStrokesToMultipleChoice = new List<Stroke>();
                        foreach (var stroke in multipleChoiceBubbleStatusChangedHistoryAction.StrokeIDsAdded.Select(newPage.GetVerifiedStrokeOnPageByID))
                        {
                            if (stroke == null)
                            {
                                Debug.WriteLine($"[ERROR] Strokes Changed, Stroke in StrokeIDsAdded in MultipleChoiceBubbleStatusChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                                continue;
                            }

                            addedStrokesToMultipleChoice.Add(stroke);
                            newPage.InkStrokes.Remove(stroke);
                            newPage.History.TrashedInkStrokes.Add(stroke);
                        }

                        var removedStrokesToMultipleChoice = new List<Stroke>();
                        foreach (var stroke in multipleChoiceBubbleStatusChangedHistoryAction.StrokeIDsRemoved.Select(newPage.GetVerifiedStrokeInHistoryByID))
                        {
                            if (stroke == null)
                            {
                                Debug.WriteLine($"[ERROR] Strokes Changed, Stroke in StrokeIDsRemoved in MultipleChoiceBubbleStatusChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                                continue;
                            }

                            removedStrokesToMultipleChoice.Add(stroke);
                            newPage.History.TrashedInkStrokes.Remove(stroke);
                            newPage.InkStrokes.Add(stroke);
                        }

                        multipleChoice.ChangeAcceptedStrokes(removedStrokesToMultipleChoice, addedStrokesToMultipleChoice);

                        switch (multipleChoiceBubbleStatusChangedHistoryAction.ChoiceBubbleStatus)
                        {
                            case ChoiceBubbleStatuses.CompletelyErased:
                                multipleChoiceBubbleStatusChangedHistoryAction.Bubble.IsFilledIn = true;
                                break;
                            case ChoiceBubbleStatuses.FilledIn:
                                multipleChoiceBubbleStatusChangedHistoryAction.Bubble.IsFilledIn = false;
                                break;
                        }

                        #endregion // MultipleChoiceBubbleStatusChangedHistoryAction Conversion Undo

                        return multipleChoiceBubbleStatusChangedHistoryAction;
                    }
                }

                #endregion // Check for Multiple Choice Erase
            }
            //Point Erase
            else if (newHistoryAction.StrokesRemoved.Count == 1 &&
                     newHistoryAction.StrokesAdded.Count == 2)
            {
                Debug.WriteLine($"[ERROR] Strokes Changed, Point Erase. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }
            else
            {
                Debug.WriteLine($"[ERROR] Strokes Changed, Not SingleAdd, SingleErase, or PointErase. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            if (!newHistoryAction.IsUsingStrokes)
            {
                Debug.WriteLine($"[ERROR] Strokes Changed, no strokes changed. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            #region ObjectsOnPageChangedHistoryAction Conversion Undo

            var addedStrokes = new List<Stroke>();
            foreach (var stroke in newHistoryAction.StrokeIDsAdded.Select(newPage.GetVerifiedStrokeOnPageByID))
            {
                if (stroke == null)
                {
                    Debug.WriteLine($"[ERROR] Strokes Changed, Null stroke in StrokeIDsAdded. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    continue;
                }
                addedStrokes.Add(stroke);
                newPage.InkStrokes.Remove(stroke);
                newPage.History.TrashedInkStrokes.Add(stroke);
            }

            var removedStrokes = new List<Stroke>();
            foreach (var stroke in newHistoryAction.StrokeIDsRemoved.Select(newPage.GetVerifiedStrokeInHistoryByID))
            {
                if (stroke == null)
                {
                    Debug.WriteLine($"[ERROR] Strokes Changed, Null stroke in StrokeIDsRemoved. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    continue;
                }
                removedStrokes.Add(stroke);
                newPage.History.TrashedInkStrokes.Remove(stroke);
                newPage.InkStrokes.Add(stroke);
            }

            foreach (var pageObject in newPage.PageObjects.OfType<IStrokeAccepter>())
            {
                pageObject.ChangeAcceptedStrokes(new List<Stroke>(), addedStrokes);
            }

            foreach (var stroke in removedStrokes)
            {
                var validStrokeAccepters =
                    newPage.PageObjects.OfType<IStrokeAccepter>().Where(p => (p.CreatorID == newPage.OwnerID || p.IsBackgroundInteractable) && p.IsStrokeOverPageObject(stroke)).ToList();

                IStrokeAccepter closestPageObject = null;
                foreach (var pageObject in validStrokeAccepters)
                {
                    if (closestPageObject == null)
                    {
                        closestPageObject = pageObject;
                        continue;
                    }

                    if (closestPageObject.PercentageOfStrokeOverPageObject(stroke) < pageObject.PercentageOfStrokeOverPageObject(stroke))
                    {
                        closestPageObject = pageObject;
                    }
                }

                closestPageObject?.ChangeAcceptedStrokes(new List<Stroke>
                                                         {
                                                             stroke
                                                         },
                                                         new List<Stroke>());
            }

            #endregion // ObjectsOnPageChangedHistoryAction Conversion Undo

            return newHistoryAction;
        }

        #endregion // Strokes HistoryItems

        #endregion // HistoryActions

        #endregion // Ann Conversions
    }
}
