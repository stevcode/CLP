using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CLP.Entities;
using Ionic.Zip;
using Ionic.Zlib;
using Emily = CLP.Entities.Old;
using Ann = CLP.Entities.Ann;

namespace Classroom_Learning_Partner.Services
{
    public class ConversionService
    {
        public ConversionService() { }

        #region Emily Conversions

        #region Locations

        public static string EmilyCacheFolder => Path.Combine(DataService.DesktopFolderPath, "CacheT");
        public static string EmilyNotebooksFolder => Path.Combine(EmilyCacheFolder, "Notebooks");
        public static string EmilyClassesFolder => Path.Combine(EmilyCacheFolder, "Classes");
        public static string EmilyZipFilePath => Path.Combine(DataService.DesktopFolderPath, "Emily - Spring 2014.clp");

        #endregion // Locations

        #region Conversion Loop

        public static Notebook ConvertCacheNotebook(string notebookFolder)
        {
            var oldNotebook = Emily.Notebook.OpenNotebook(notebookFolder);
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
            }

            return newNotebook;
        }

        public static Session ConvertCacheClassPeriod(string filePath)
        {
            var classPeriod = Emily.ClassPeriod.OpenClassPeriod(filePath);
            var session = ConvertClassPeriod(classPeriod);

            return session;
        }

        public static void SaveNotebookToZip(string zipFilePath, Notebook notebook)
        {
            if (File.Exists(zipFilePath))
            {
                return;
            }

            var parentNotebookName = notebook.InternalZipFileDirectoryName;
            var entryList = new List<DataService.ZipEntrySaver>
                            {
                                new DataService.ZipEntrySaver(notebook, parentNotebookName)
                            };

            foreach (var page in notebook.Pages)
            {
                entryList.Add(new DataService.ZipEntrySaver(page, parentNotebookName));
                foreach (var submission in page.Submissions)
                {
                    entryList.Add(new DataService.ZipEntrySaver(submission, parentNotebookName));
                }
            }

            using (var zip = new ZipFile())
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip);
                }

                zip.Save(zipFilePath);
            }
        }

        public static void SaveNotebooksToZip(string zipFilePath, List<Notebook> notebooks)
        {
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            var entryList = new List<DataService.ZipEntrySaver>();
            foreach (var notebook in notebooks)
            {
                notebook.ContainerZipFilePath = zipFilePath;

                var parentNotebookName = notebook.InternalZipFileDirectoryName;
                entryList.Add(new DataService.ZipEntrySaver(notebook, parentNotebookName));

                foreach (var page in notebook.Pages)
                {
                    page.ContainerZipFilePath = zipFilePath;
                    entryList.Add(new DataService.ZipEntrySaver(page, parentNotebookName));
                    foreach (var submission in page.Submissions)
                    {
                        submission.ContainerZipFilePath = zipFilePath;
                        entryList.Add(new DataService.ZipEntrySaver(submission, parentNotebookName));
                    }
                }
            }

            using (var zip = new ZipFile())
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip);
                }

                zip.Save(zipFilePath);
            }
        }

        public static void SaveSessionsToZip(string zipFilePath, List<Session> sessions)
        {
            if (!File.Exists(zipFilePath))
            {
                return;
            }

            var mappedIDs = new Dictionary<string, int>();
            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var allPageIDs = DataService.GetAllPageIDsInNotebook(zip, Person.Author);
                mappedIDs = DataService.GetPageNumbersFromPageIDs(zip, allPageIDs);
            }

            var entryList = new List<DataService.ZipEntrySaver>();
            foreach (var session in sessions)
            {
                session.ContainerZipFilePath = zipFilePath;
                session.StartingPageNumber = mappedIDs[session.StartingPageID].ToString();
                var pageNumbers = new List<int>();
                foreach (var pageID in session.PageIDs)
                {
                    var pageNumber = mappedIDs[pageID];
                    pageNumbers.Add((int)pageNumber.ToInt());
                }

                session.PageNumbers = RangeHelper.ParseIntNumbersToString(pageNumbers, false, true);

                entryList.Add(new DataService.ZipEntrySaver(session));
            }

            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip);
                }

                zip.Save();
            }
        }

        #endregion // Conversion Loop

        #region Notebook Parts

        public static Session ConvertClassPeriod(Emily.ClassPeriod classPeriod)
        {
            var newSession = new Session
            {
                StartTime = classPeriod.StartTime,
                PageIDs = classPeriod.PageIDs,
                StartingPageID = classPeriod.PageIDs.FirstOrDefault()
            };

            newSession.NotebookIDs.Add(classPeriod.NotebookID);

            return newSession;
        }

        public static Person ConvertPerson(Emily.Person person)
        {
            var newPerson = Person.ParseFromFullName(person.FullName, person.IsStudent);
            newPerson.ID = person.ID;
            newPerson.Alias = person.Alias;

            if (string.IsNullOrWhiteSpace(newPerson.FullName))
            {
                Console.WriteLine("[CONVERSION ERROR]: Person.FullName is blank.");
            }

            return newPerson;
        }

        public static Notebook ConvertNotebook(Emily.Notebook notebook)
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

        public static CLPPage ConvertPage(Emily.CLPPage page)
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
                PageType = page.PageType == Emily.PageTypes.Animation ? PageTypes.Animation : PageTypes.Default,
                SubmissionType = page.SubmissionType == Emily.SubmissionTypes.Single ? SubmissionTypes.Single : SubmissionTypes.Unsubmitted,
                SubmissionTime = page.SubmissionTime,
                Height = page.Height,
                Width = page.Width,
                InitialAspectRatio = page.InitialAspectRatio
            };

            // TODO: Convert History
            // TODO: Tags

            foreach (var stroke in page.InkStrokes)
            {
                newPage.InkStrokes.Add(stroke);
            }

            foreach (var pageObject in page.PageObjects)
            {
                var newPageObject = ConvertPageObject(pageObject, newPage);
                newPage.PageObjects.Add(newPageObject);
                var divisionTemplate = newPageObject as DivisionTemplate;
                if (divisionTemplate != null &&
                    divisionTemplate.RemainderTiles != null)
                {
                    newPage.PageObjects.Add(divisionTemplate.RemainderTiles);
                }
            }

            return newPage;
        }

        #endregion // Notebook Parts

        #region PageObjects

        public static IPageObject ConvertPageObject(Emily.IPageObject pageObject, CLPPage newPage)
        {
            IPageObject newPageObject = null;

            TypeSwitch.On(pageObject).Case<Emily.Shape>(p =>
            {
                newPageObject = ConvertShape(p, newPage);
            }).Case<Emily.CLPTextBox>(p =>
            {
                newPageObject = ConvertTextBox(p, newPage);
            }).Case<Emily.CLPImage>(p =>
            {
                newPageObject = ConvertImage(p, newPage);
            }).Case<Emily.CLPArray>(p =>
            {
                newPageObject = ConvertArray(p, newPage);
            }).Case<Emily.FuzzyFactorCard>(p =>
            {
                newPageObject =
                    ConvertDivisionTemplate(p,
                                            newPage);
            });

            return newPageObject;
        }

        public static Shape ConvertShape(Emily.Shape shape, CLPPage newPage)
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
                PageObjectFunctionalityVersion = "Emily5.22.2014",
                IsManipulatableByNonCreator = shape.IsManipulatableByNonCreator,
                ParentPage = newPage
            };

            switch (shape.ShapeType)
            {
                case Emily.ShapeType.Rectangle:
                    newShape.ShapeType = ShapeType.Rectangle;
                    break;
                case Emily.ShapeType.Ellipse:
                    newShape.ShapeType = ShapeType.Ellipse;
                    break;
                case Emily.ShapeType.Triangle:
                    newShape.ShapeType = ShapeType.Triangle;
                    break;
                case Emily.ShapeType.HorizontalLine:
                    newShape.ShapeType = ShapeType.HorizontalLine;
                    break;
                case Emily.ShapeType.VerticalLine:
                    newShape.ShapeType = ShapeType.VerticalLine;
                    break;
                case Emily.ShapeType.Protractor:
                    newShape.ShapeType = ShapeType.Protractor;
                    break;
                default:
                    newShape.ShapeType = ShapeType.Rectangle;
                    break;
            }

            return newShape;
        }

        public static CLPTextBox ConvertTextBox(Emily.CLPTextBox textBox, CLPPage newPage)
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
                PageObjectFunctionalityVersion = "Emily5.22.2014",
                IsManipulatableByNonCreator = textBox.IsManipulatableByNonCreator,
                ParentPage = newPage
            };
            newTextBox.Text = textBox.Text;

            return newTextBox;
        }

        public static CLPImage ConvertImage(Emily.CLPImage image, CLPPage newPage)
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
                PageObjectFunctionalityVersion = "Emily5.22.2014",
                IsManipulatableByNonCreator = image.IsManipulatableByNonCreator,
                ParentPage = newPage
            };
            newImage.ImageHashID = image.ImageHashID;

            return newImage;
        }

        public static CLPArray ConvertArray(Emily.CLPArray array, CLPPage newPage)
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
                PageObjectFunctionalityVersion = "Emily5.22.2014",
                IsManipulatableByNonCreator = array.IsManipulatableByNonCreator,
                ParentPage = newPage,
                Rows = array.Rows,
                Columns = array.Columns,
                IsGridOn = array.IsGridOn,
                IsDivisionBehaviorOn = array.IsDivisionBehaviorOn,
                IsSnappable = array.IsSnappable,
                IsTopLabelVisible = array.IsTopLabelVisible,
                IsSideLabelVisible = array.IsSideLabelVisible,
                CanAcceptStrokes = false
            };

            switch (array.ArrayType)
            {
                case Emily.ArrayTypes.Array:
                    newArray.ArrayType = ArrayTypes.Array;
                    break;
                case Emily.ArrayTypes.ArrayCard:
                    newArray.ArrayType = ArrayTypes.ArrayCard;
                    break;
                case Emily.ArrayTypes.FactorCard:
                    newArray.ArrayType = ArrayTypes.FactorCard;
                    break;
                default:
                    newArray.ArrayType = ArrayTypes.Array;
                    break;
            }

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

            return newArray;
        }

        public static CLPArrayDivision ConvertArrayDivision(Emily.CLPArrayDivision division)
        {
            var newDivision = new CLPArrayDivision
            {
                Position = division.Position,
                Length = division.Length,
                Value = division.Value,
                Orientation = division.Orientation == Emily.ArrayDivisionOrientation.Horizontal ? ArrayDivisionOrientation.Horizontal : ArrayDivisionOrientation.Vertical
            };

            return newDivision;
        }

        public static DivisionTemplate ConvertDivisionTemplate(Emily.FuzzyFactorCard ffc, CLPPage newPage)
        {
            var newDivisionTemplate = new DivisionTemplate
            {
                ID = ffc.ID,
                XPosition = ffc.XPosition,
                YPosition = ffc.YPosition,
                Height = ffc.Height,
                Width = ffc.Width,
                OwnerID = ffc.OwnerID,
                CreatorID = ffc.CreatorID,
                CreationDate = ffc.CreationDate,
                PageObjectFunctionalityVersion = "Emily5.22.2014",
                IsManipulatableByNonCreator = ffc.IsManipulatableByNonCreator,
                ParentPage = newPage,
                Rows = ffc.Rows,
                Columns = ffc.Columns,
                IsGridOn = ffc.IsGridOn,
                IsDivisionBehaviorOn = ffc.IsDivisionBehaviorOn,
                IsSnappable = ffc.IsSnappable,
                IsTopLabelVisible = ffc.IsTopLabelVisible,
                IsSideLabelVisible = ffc.IsSideLabelVisible,
                Dividend = ffc.Dividend
            };

            if (!object.Equals(ffc.RemainderTiles, null))
            {
                var newRemainderTiles = ConvertRemainderTiles(ffc.RemainderTiles, newPage);
                newDivisionTemplate.RemainderTiles = newRemainderTiles;
                newDivisionTemplate.IsRemainderTilesVisible = true;
            }

            foreach (var division in ffc.HorizontalDivisions)
            {
                var newDivision = ConvertArrayDivision(division);
                newDivisionTemplate.HorizontalDivisions.Add(newDivision);
            }

            foreach (var division in ffc.VerticalDivisions)
            {
                var newDivision = ConvertArrayDivision(division);
                newDivisionTemplate.VerticalDivisions.Add(newDivision);
            }

            return newDivisionTemplate;
        }

        public static RemainderTiles ConvertRemainderTiles(Emily.RemainderTiles remainderTiles, CLPPage newPage)
        {
            var newRemainderTiles = new RemainderTiles
            {
                ID = remainderTiles.ID,
                XPosition = remainderTiles.XPosition,
                YPosition = remainderTiles.YPosition,
                Height = remainderTiles.Height,
                Width = remainderTiles.Width,
                OwnerID = remainderTiles.OwnerID,
                CreatorID = remainderTiles.CreatorID,
                CreationDate = remainderTiles.CreationDate,
                PageObjectFunctionalityVersion = "Emily5.22.2014",
                IsManipulatableByNonCreator = remainderTiles.IsManipulatableByNonCreator,
                ParentPage = newPage
            };
            newRemainderTiles.TileColors = remainderTiles.TileOffsets;

            return newRemainderTiles;
        }

        #endregion // PageObjects

        #endregion // Emily Conversions

        #region Ann Conversions

        #region Locations

        public static string AnnCacheFolder => Path.Combine(DataService.DesktopFolderPath, "Cache.Ann.Complete");
        public static string AnnNotebooksFolder => Path.Combine(AnnCacheFolder, "Notebooks");
        public static string AnnClassesFolder => Path.Combine(AnnCacheFolder, "Classes");
        public static string AnnZipFilePath => Path.Combine(DataService.DesktopFolderPath, "Ann - Fall 2014.clp");

        #endregion // Locations

        #region Conversion Loop

        public static Notebook ConvertCacheAnnNotebook(string notebookFolder)
        {
            var oldNotebook = Ann.Notebook.LoadLocalFullNotebook(notebookFolder);
            var newNotebook = ConvertNotebook(oldNotebook);

            foreach (var page in oldNotebook.Pages)
            {
                var newPage = ConvertPage(page);
                //foreach (var submission in page.Submissions)
                //{
                //    var newSubmission = ConvertPage(submission);
                //    newPage.Submissions.Add(newSubmission);
                //}

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

        public static Session ConvertCacheAnnClassPeriod(string filePath)
        {
            var classPeriod = Ann.ClassPeriod.LoadLocalClassPeriod(filePath);
            var session = ConvertClassPeriod(classPeriod);

            return session;
        }

        public static ClassRoster ConvertCacheAnnClassSubject(string filePath, Notebook notebook)
        {
            var classSubject = Ann.ClassSubject.OpenClassSubject(filePath);
            var classRoster = ConvertAnnClassSubject(classSubject, notebook);

            return classRoster;
        }

        public static void SaveAnnNotebookToZip(string zipFilePath, Notebook notebook)
        {
            if (File.Exists(zipFilePath))
            {
                return;
            }

            var parentNotebookName = notebook.InternalZipFileDirectoryName;
            var entryList = new List<DataService.ZipEntrySaver>
                            {
                                new DataService.ZipEntrySaver(notebook, parentNotebookName)
                            };

            foreach (var page in notebook.Pages)
            {
                entryList.Add(new DataService.ZipEntrySaver(page, parentNotebookName));
                foreach (var submission in page.Submissions)
                {
                    entryList.Add(new DataService.ZipEntrySaver(submission, parentNotebookName));
                }
            }

            using (var zip = new ZipFile())
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip);
                }

                zip.Save(zipFilePath);
            }
        }

        public static void SaveAnnClassRosterToZip(string zipFilePath, ClassRoster classRoster)
        {
            if (!File.Exists(zipFilePath))
            {
                return;
            }

            var entryList = new List<DataService.ZipEntrySaver>();
            classRoster.ContainerZipFilePath = zipFilePath;

            entryList.Add(new DataService.ZipEntrySaver(classRoster));

            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip);
                }

                zip.Save();
            }
        }

        public static void SaveAnnSessionsToZip(string zipFilePath, List<Session> sessions)
        {
            if (!File.Exists(zipFilePath))
            {
                return;
            }

            var entryList = new List<DataService.ZipEntrySaver>();
            foreach (var session in sessions)
            {
                session.ContainerZipFilePath = zipFilePath;

                entryList.Add(new DataService.ZipEntrySaver(session));
            }

            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip);
                }

                zip.Save();
            }
        }

        #endregion // Conversion Loop

        #region Notebook Parts

        public static Dictionary<int, string> PageNumberToIDMap = new Dictionary<int, string>();
        public static Dictionary<string, int> PageIDToNumberMap = new Dictionary<string, int>();

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
                Console.WriteLine($"[CONVERSION ERROR]: Person.FullName is blank. Original Person.FullName is {person.FullName}.");
            }

            return newPerson;
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

            // TODO: Convert History
            // TODO: Tags

            foreach (var stroke in page.InkStrokes)
            {
                newPage.InkStrokes.Add(stroke);
            }

            foreach (var pageObject in page.PageObjects)
            {
                // Ignores the TextBoxes that were part of the old Multiple Choice
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
                Console.WriteLine($"[ERROR] newPageObject is NULL. Original pageObject is {pageObject.GetType()}");
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

        #endregion // Ann Conversions
    }
}