﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Classroom_Learning_Partner.Services;
using CLP.Entities;
using Ionic.Zip;
using Ionic.Zlib;
using Emily = CLP.Entities.Old;


namespace Classroom_Learning_Partner
{
    public partial class ConversionService
    {
        public const bool IS_LARGE_CACHE = false;
        public const bool IS_ANONYMIZED_CACHE = true;
        public const bool IS_CONVERTING_SUBMISSIONS = true;

        #region Rolling

        public static string ConvertedFolder => Path.Combine(DataService.DesktopFolderPath, "Converted");
        public static string PageTrackerFilePath => Path.Combine(ConvertedFolder, "tracking.txt");

        #endregion // Rolling

        #region Loop

        public static void ConvertAnnCache()
        {
            if (!Directory.Exists(ConvertedFolder))
            {
                Directory.CreateDirectory(ConvertedFolder);
            }

            if (!File.Exists(PageTrackerFilePath))
            {
                using (File.CreateText(PageTrackerFilePath)) { }
            }

            var cacheType = IS_LARGE_CACHE ? "Large" : "Assessment";
            CLogger.AppendToLog($"Beginning Conversion of Ann's {cacheType} Cache.");
            
            var notebooksFolderPath = IS_LARGE_CACHE ? AnnNotebooksFolder : AssessmentNotebooksFolder;
            var dirInfo = new DirectoryInfo(notebooksFolderPath);
            foreach (var directory in dirInfo.EnumerateDirectories())
            {
                var notebookFolder = directory.FullName;
                RollingConversion(notebookFolder);
            }
        }

        public static void CreateSavedZip()
        {
            var zipPath = IS_LARGE_CACHE ? AnnZipFilePath : AssessmentZipFilePath;
            var classesFolderPath = IS_LARGE_CACHE ? AnnClassesFolder : AssessmentClassesFolder;
            var imagesFolderPath = IS_LARGE_CACHE ? AnnImageFolder : AssessmentImageFolder;
            const string SUBJECT_FILE_NAME = "subject;L6xDfDuP-kCMBjQ3-HdAPQ.xml";

            Notebook authorNotebook = null;
            var rootFolderName = "notebooks";
            var rootFolderPath = Path.Combine(ConvertedFolder, rootFolderName);
            var setFolderName = IS_LARGE_CACHE ? "Math Notebook;pUVQ-qBPyUWCuHWMs9dryA" : "Math Assessment;_455DjGei0661H49s5f4Cw";
            var setFolderPath = Path.Combine(rootFolderPath, setFolderName);
            var dirInfo = new DirectoryInfo(setFolderPath);
            foreach (var directory in dirInfo.EnumerateDirectories())
            {
                var notebookFolderPath = directory.FullName;
                
                var pagesFolderPath = Path.Combine(notebookFolderPath, "pages");
                var pagesDirectoryInfo = new DirectoryInfo(pagesFolderPath);
                var pages = new List<CLPPage>();
                foreach (var pageFile in pagesDirectoryInfo.EnumerateFiles())
                {
                    var pageFilePath = pageFile.FullName;
                    var page = ASerializableBase.FromXmlFile<CLPPage>(pageFilePath);

                    pages.Add(page);

                    if (!PageNumberToIDMap.ContainsKey(page.PageNumber))
                    {
                        PageNumberToIDMap.Add(page.PageNumber, page.ID);
                    }

                    if (!PageIDToNumberMap.ContainsKey(page.ID))
                    {
                        PageIDToNumberMap.Add(page.ID, page.PageNumber);
                    }
                }

                var submissionsFolderPath = Path.Combine(notebookFolderPath, "submissions");
                var submissionsDirectoryInfo = new DirectoryInfo(submissionsFolderPath);
                var submissions = new List<CLPPage>();
                foreach (var submissionFile in submissionsDirectoryInfo.EnumerateFiles())
                {
                    var submissionFilePath = submissionFile.FullName;
                    var submission = ASerializableBase.FromXmlFile<CLPPage>(submissionFilePath);

                    submissions.Add(submission);
                }

                var notebookFilePath = Path.Combine(notebookFolderPath, "notebook.xml");
                var notebook = ASerializableBase.FromXmlFile<Notebook>(notebookFilePath);

                SaveNotebookToZip(zipPath, notebook, pages, submissions);

                if (notebook.Owner.ID == Person.AUTHOR_ID)
                {
                    authorNotebook = notebook;
                }
            }

            var subjectFilePath = Path.Combine(classesFolderPath, SUBJECT_FILE_NAME);
            var classRoster = ConvertCacheAnnClassSubject(subjectFilePath, authorNotebook);
            SaveClassRosterToZip(zipPath, classRoster);

            SaveImagesToZip(zipPath, imagesFolderPath);

            var classesDirInfo = new DirectoryInfo(classesFolderPath);
            var sessions = classesDirInfo.EnumerateFiles("period;*.xml").Select(file => file.FullName).Select(ConvertCacheAnnClassPeriod).OrderBy(s => s.StartTime).ToList();
            var i = 1;
            foreach (var session in sessions)
            {
                session.SessionTitle = $"Class {i}";
                i++;
            }

            SaveSessionsToZip(zipPath, sessions, authorNotebook);

            CLogger.AppendToLog($"Finished Conversion of Ann's Cache.");

            if (!IS_LARGE_CACHE && IS_ANONYMIZED_CACHE)
            {
                AnonymizationFixesForAssessmentCache();
            }
        }

        public static void AnonymizationFixesForAssessmentCache()
        {
            CLogger.AppendToLog("Beginning fixes for anonymized cache.");

            var allNotebooks = DataService.LoadAllNotebooksFromCLPContainer(AssessmentZipFilePath);

            #region Anonymize Page 1

            var authorNotebook = allNotebooks.First(n => n.Owner.ID == Person.AUTHOR_ID);
            DataService.LoadPagesIntoNotebook(authorNotebook,
                                              new List<int>
                                              {
                                                  1
                                              });
            var authorPage = authorNotebook.Pages.First();

            foreach (var notebook in allNotebooks.Where(n => n.Owner.IsStudent))
            {
                CLogger.AppendToLog($"Loading {notebook.Owner.DisplayName}'s notebook.");
                DataService.LoadPagesIntoNotebook(notebook,
                                                  new List<int>
                                                  {
                                                      1
                                                  });

                var page = notebook.Pages.First();
                var submissions = new List<CLPPage>();
                submissions.AddRange(page.Submissions);
                CLogger.AppendToLog($"Removing {notebook.Owner.DisplayName}'s page 1 submissions.");
                using (var zip = ZipFile.Read(AssessmentZipFilePath))
                {
                    foreach (var submission in submissions)
                    {
                        zip.RemoveEntry(submission.GetZipEntryFullPath(notebook));
                    }

                    zip.Save();
                }

                CLogger.AppendToLog($"Submissions removed.");

                var replacementPage = authorPage.CopyForNewOwner(notebook.Owner);
                var replacementEntries = new List<DataService.ZipEntrySaver>
                              {
                                  new DataService.ZipEntrySaver(replacementPage, notebook)
                              };

                DataService.SaveZipEntries(AssessmentZipFilePath, replacementEntries);
                CLogger.AppendToLog($"New Page 1 saved.");
            }

            #endregion // Anonymize Page 1

            #region Specific fix for inked student name

            CLogger.AppendToLog($"Starting Page 3 fix.");

            var specificStudentNotebook = allNotebooks.First(n => n.Owner.ID == "_3ll1DzkbU6LPZVShajTWg");
            specificStudentNotebook.Pages.Clear();
            DataService.LoadPagesIntoNotebook(specificStudentNotebook,
                                              new List<int>
                                              {
                                                  3
                                              });

            var pageToAlter = specificStudentNotebook.Pages.First();
            var submissionToAlter = pageToAlter.Submissions.First();
            var toAlter = new List<CLPPage>
                          {
                              pageToAlter,
                              submissionToAlter
                          };

            foreach (var page in toAlter)
            {
                var historyActionsToRemove = page.History.CompleteOrderedHistoryActions.Where(h => h.HistoryActionIndex >= 126 && h.HistoryActionIndex <= 165)
                                                 .Cast<ObjectsOnPageChangedHistoryAction>()
                                                 .ToList();
                var strokesToRemove = historyActionsToRemove.SelectMany(h => h.StrokesAdded).Distinct().ToList();

                foreach (var stroke in strokesToRemove)
                {
                    if (page.InkStrokes.Contains(stroke))
                    {
                        page.InkStrokes.Remove(stroke);
                    }

                    if (page.History.TrashedInkStrokes.Contains(stroke))
                    {
                        page.History.TrashedInkStrokes.Remove(stroke);
                    }
                }

                foreach (var objectsOnPageChangedHistoryAction in historyActionsToRemove)
                {
                    page.History.UndoActions.Remove(objectsOnPageChangedHistoryAction);
                }

                page.History.RefreshHistoryIndexes();
                //HistoryAnalysis.GenerateSemanticEvents(page);
                //AnalysisPanelViewModel.AnalyzeSkipCountingStatic(page);
            }

            var entries = new List<DataService.ZipEntrySaver>
                          {
                              new DataService.ZipEntrySaver(pageToAlter, specificStudentNotebook),
                              new DataService.ZipEntrySaver(submissionToAlter, specificStudentNotebook)
                          };

            DataService.SaveZipEntries(AssessmentZipFilePath, entries);

            #endregion // Specific fix for inked student name

            CLogger.AppendToLog("Finished fixes for anonymized cache.");
        }

        #endregion // Loop

        #region All

        public static void SaveNotebookToZip(string zipFilePath, Notebook notebook, List<CLPPage> pages, List<CLPPage> submissions)
        {
            CLogger.AppendToLog($"Saving {notebook.Owner.DisplayName}'s Notebook To Zip.");
            notebook.ContainerZipFilePath = zipFilePath;
            var entryList = new List<DataService.ZipEntrySaver>
                            {
                                new DataService.ZipEntrySaver(notebook, notebook)
                            };

            foreach (var page in pages)
            {
                page.ContainerZipFilePath = zipFilePath;
                entryList.Add(new DataService.ZipEntrySaver(page, notebook));
            }

            foreach (var submission in submissions)
            {
                submission.ContainerZipFilePath = zipFilePath;
                entryList.Add(new DataService.ZipEntrySaver(submission, notebook));
            }

            if (!File.Exists(zipFilePath))
            {
                using (var zip = new ZipFile())
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    //zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.CaseSensitiveRetrieval = true;

                    foreach (var zipEntrySaver in entryList)
                    {
                        zipEntrySaver.UpdateEntry(zip);
                    }

                    zip.Save(zipFilePath);
                }
            }
            else
            {
                using (var zip = ZipFile.Read(zipFilePath))
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    //zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.CaseSensitiveRetrieval = true;

                    foreach (var zipEntrySaver in entryList)
                    {
                        zipEntrySaver.UpdateEntry(zip);
                    }

                    zip.Save();
                }
            }
            

            CLogger.AppendToLog($"Finished Saving {notebook.Owner.DisplayName}'s Notebook To Zip.");
        }

        public static void SaveNotebooksToZip(string zipFilePath, List<Notebook> notebooks)
        {
            CLogger.AppendToLog("Saving Notebooks To Zip.");

            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            var entryList = new List<DataService.ZipEntrySaver>();
            foreach (var notebook in notebooks)
            {
                notebook.ContainerZipFilePath = zipFilePath;

                entryList.Add(new DataService.ZipEntrySaver(notebook, notebook));

                foreach (var page in notebook.Pages)
                {
                    page.ContainerZipFilePath = zipFilePath;
                    entryList.Add(new DataService.ZipEntrySaver(page, notebook));
                    foreach (var submission in page.Submissions)
                    {
                        submission.ContainerZipFilePath = zipFilePath;
                        entryList.Add(new DataService.ZipEntrySaver(submission, notebook));
                    }
                }
            }

            using (var zip = new ZipFile())
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                //zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip);
                }

                zip.Save(zipFilePath);
            }

            CLogger.AppendToLog("Finished Saving Notebooks To Zip.");
        }

        public static void SaveClassRosterToZip(string zipFilePath, ClassRoster classRoster)
        {
            CLogger.AppendToLog("Saving Roster To Zip.");

            if (!File.Exists(zipFilePath))
            {
                CLogger.AppendToLog("[ERROR] Failed Saving Roster To Zip, Zip file doesn't exist.");
                return;
            }

            var entryList = new List<DataService.ZipEntrySaver>();
            classRoster.ContainerZipFilePath = zipFilePath;

            entryList.Add(new DataService.ZipEntrySaver(classRoster));

            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                //zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip);
                }

                zip.Save();
            }

            CLogger.AppendToLog("Finished Saving Roster To Zip.");
        }

        public static void SaveSessionsToZip(string zipFilePath, List<Session> sessions, Notebook authorNotebook)
        {
            CLogger.AppendToLog("Saving Sessions To Zip.");

            if (!File.Exists(zipFilePath))
            {
                CLogger.AppendToLog("[ERROR] Failed Saving Sessions To Zip, Zip file doesn't exist.");
                return;
            }

            var mappedIDs = new Dictionary<string, int>();
            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                //zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var allPageIDs = DataService.GetAllPageIDsInNotebook(zip, authorNotebook);
                mappedIDs = DataService.GetPageNumbersFromPageIDs(zip, authorNotebook, allPageIDs);
            }

            var entryList = new List<DataService.ZipEntrySaver>();
            foreach (var session in sessions)
            {
                session.ContainerZipFilePath = zipFilePath;

                if (mappedIDs.ContainsKey(session.StartingPageID))
                {
                    session.StartingPageNumber = mappedIDs[session.StartingPageID].ToString();
                    var pageNumbers = new List<int>();
                    foreach (var pageID in session.PageIDs)
                    {
                        if (!mappedIDs.ContainsKey(pageID))
                        {
                            continue;
                        }
                        var pageNumber = mappedIDs[pageID];
                        pageNumbers.Add((int)pageNumber.ToInt());
                    }

                    session.PageNumbers = RangeHelper.ParseIntNumbersToString(pageNumbers, false, true);
                }

                entryList.Add(new DataService.ZipEntrySaver(session));
            }

            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                //zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                foreach (var zipEntrySaver in entryList)
                {
                    zipEntrySaver.UpdateEntry(zip);
                }

                zip.Save();
            }

            CLogger.AppendToLog("Finished Saving Sessions To Zip.");
        }

        public static void SaveImagesToZip(string zipFilePath, string imageDirectoryPath)
        {
            CLogger.AppendToLog("Saving Images To Zip.");

            if (!File.Exists(zipFilePath))
            {
                CLogger.AppendToLog("[ERROR] Failed Saving Images To Zip, Zip file doesn't exist.");
                return;
            }

            using (var zip = ZipFile.Read(zipFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                //zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var directoryInfo = new DirectoryInfo(imageDirectoryPath);
                foreach (var fileInfo in directoryInfo.GetFiles())
                {
                    var fileNameWithExtension = fileInfo.Name;
                    var imageFilePath = fileInfo.FullName;

                    var internalFilePath = ZipExtensions.CombineEntryDirectoryAndName(AInternalZipEntryFile.ZIP_IMAGES_FOLDER_NAME, fileNameWithExtension);
                    if (zip.ContainsEntry(internalFilePath))
                    {
                        continue;
                    }

                    var entry = zip.AddFile(imageFilePath);
                    entry.FileName = internalFilePath;
                }

                zip.Save();
            }

            CLogger.AppendToLog("Finished Saving Images To Zip.");
        }

        #endregion // All

        #region Emily Conversions

        #region Locations

        public static string EmilyCacheFolder => Path.Combine(DataService.DesktopFolderPath, "CacheT");
        public static string EmilyNotebooksFolder => Path.Combine(EmilyCacheFolder, "Notebooks");
        public static string EmilyClassesFolder => Path.Combine(EmilyCacheFolder, "Classes");
        public static string EmilyZipFilePath => Path.Combine(DataService.DesktopFolderPath, "Emily - Spring 2014.clp");

        #endregion // Locations

        #region Conversion Loop

        public static Notebook ConvertCacheEmilyNotebook(string notebookFolder)
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

        public static ClassRoster ConvertCacheEmilyClassSubject(string filePath, Notebook notebook)
        {
            var classSubject = Emily.ClassSubject.OpenClassSubject(filePath);
            var classRoster = ConvertEmilyClassSubject(classSubject, notebook);

            return classRoster;
        }

        public static Session ConvertCacheEmilyClassPeriod(string filePath)
        {
            var classPeriod = Emily.ClassPeriod.OpenClassPeriod(filePath);
            var session = ConvertClassPeriod(classPeriod);

            return session;
        }

        #endregion // Conversion Loop

        #region Notebook Parts

        public static Person ConvertPerson(Emily.Person person)
        {
            var name = person.FullName;
            if (IS_ANONYMIZED_CACHE)
            {
                const string TEXT_FILE_NAME = "AnonymousNames - Emily.txt";
                var anonymousTextFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), TEXT_FILE_NAME);
                if (!File.Exists(anonymousTextFilePath))
                {
                    MessageBox.Show("You are missing AnonymousNames.txt on the Desktop.");
                }

                var names = new Dictionary<string, string>();
                var textFile = new StreamReader(anonymousTextFilePath);
                string line;
                while ((line = textFile.ReadLine()) != null)
                {
                    var parts = line.Split(',');
                    if (parts.Length != 2)
                    {
                        MessageBox.Show("AnonymousNames.txt is in the wrong format.");
                        break;
                    }

                    var oldName = parts[0];
                    var newName = parts[1];
                    newName = newName.Replace("\t", string.Empty);
                    newName = newName.Replace("\n", string.Empty);
                    newName = newName.Trim();

                    if (!names.ContainsKey(oldName))
                    {
                        names.Add(oldName, newName);
                    }
                }

                textFile.Close();

                if (names.ContainsKey(person.FullName))
                {
                    name = names[person.FullName];
                }
            }

            var newPerson = Person.ParseFromFullName(name, person.IsStudent);
            newPerson.ID = person.ID;
            newPerson.Alias = person.Alias;

            if (string.IsNullOrWhiteSpace(newPerson.FullName))
            {
                CLogger.AppendToLog("[CONVERSION ERROR]: Person.FullName is blank.");
            }

            return newPerson;
        }

        public static ClassRoster ConvertEmilyClassSubject(Emily.ClassSubject classSubject, Notebook notebook)
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
    }
}