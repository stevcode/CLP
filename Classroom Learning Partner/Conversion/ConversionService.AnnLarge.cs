using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel.Collections;
using Classroom_Learning_Partner.Services;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;
using Ionic.Zip;
using Ionic.Zlib;
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
        public static string AnnAuthorTagsFolder => Path.Combine(DataService.DesktopFolderPath, "Large Cache Tags");
        public static string AnnAuthorTagsEric => Path.Combine(AnnAuthorTagsFolder, "Ann - Fall 2014 - Fixed - Eric.clp");
        public static string AnnAuthorTagsAndee => Path.Combine(AnnAuthorTagsFolder, "Ann - Fall 2014 - Fixed - Andee.clp");
        public static string AnnAuthorTagsLily2 => Path.Combine(AnnAuthorTagsFolder, "Ann - Fall 2014 - Fixed - LK2.clp");
        public static string AnnAuthorTagsLily1 => Path.Combine(AnnAuthorTagsFolder, "Ann - Fall 2014 - Fixed - LK.clp");
        public static string AnnAuthorTagsStitched => Path.Combine(AnnAuthorTagsFolder, "Ann - Fall 2014 - Stitched.clp");

        public static string AssessmentCacheFolder => Path.Combine(DataService.DesktopFolderPath, "Cache.Chapter6.Assessment");
        public static string AssessmentNotebooksFolder => Path.Combine(AssessmentCacheFolder, "Notebooks");
        public static string AssessmentClassesFolder => Path.Combine(AssessmentCacheFolder, "Classes");
        public static string AssessmentZipFilePath => Path.Combine(DataService.DesktopFolderPath, "Ann - Assessment 2014.clp");
        public static string AssessmentImageFolder => Path.Combine(DataService.DesktopFolderPath, "assessment images");

        #endregion // Locations

        #region Conversion Loop

        public static void Combine()
        {
            // *****Important Note: Ensure IS_LARGE_CACHE is set to true in ConversionService*****

            CLogger.ForceNewLogFile();

            CLogger.AppendToLog("Beginning Combination.");

            var zipFilePath = Path.Combine(DataService.DesktopFolderPath, "Ann - Fall 2014 - Combined.clp");

            ///////////

            var notebooksFolderPath = AnnNotebooksFolder;
            var classesFolderPath = AnnClassesFolder;
            var imagesFolderPath = AnnImageFolder;
            const string SUBJECT_FILE_NAME = "subject;L6xDfDuP-kCMBjQ3-HdAPQ.xml";

            var notebooks = new List<Notebook>();
            Notebook authorNotebook = null;

            var dirInfo = new DirectoryInfo(notebooksFolderPath);
            foreach (var directory in dirInfo.EnumerateDirectories())
            {
                var notebookFolder = directory.FullName;

                CLogger.AppendToLog($"Loading Notebook To Convert: {notebookFolder}");
                var oldNotebook = Ann.Notebook.LoadLocalFullNotebook(notebookFolder);
                CLogger.AppendToLog("Notebook Loaded");

                CLogger.AppendToLog("Converting Notebook");
                var newNotebook = ConvertNotebook(oldNotebook);
                CLogger.AppendToLog("Notebook Converted");

                notebooks.Add(newNotebook);

                if (newNotebook.OwnerID == Person.AUTHOR_ID)
                {
                    authorNotebook = newNotebook;
                }

                if (newNotebook.Owner.IsStudent)
                {
                    continue;
                }

                foreach (var page in oldNotebook.Pages)
                {
                    CLogger.AppendToLog($"Converting Page {page.PageNumber} for {page.Owner.FullName}");
                    var newPage = ConvertPage(page);
                    CLogger.AppendToLog($"Finished Converting Page {page.PageNumber} for {page.Owner.FullName}");

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
            }

            //

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

                if (notebook.Owner.IsStudent)
                {
                    continue;
                }

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

            //

            var subjectFilePath = Path.Combine(classesFolderPath, SUBJECT_FILE_NAME);
            var classRoster = ConvertCacheAnnClassSubject(subjectFilePath, authorNotebook);
            SaveClassRosterToZip(zipFilePath, classRoster);

            SaveImagesToZip(zipFilePath, imagesFolderPath);

            var classesDirInfo = new DirectoryInfo(classesFolderPath);
            var sessions = classesDirInfo.EnumerateFiles("period;*.xml").Select(file => file.FullName).Select(ConvertCacheAnnClassPeriod).OrderBy(s => s.StartTime).ToList();
            var i = 1;
            foreach (var session in sessions)
            {
                session.SessionTitle = $"Class {i}";
                i++;
            }

            SaveSessionsToZip(zipFilePath, sessions, authorNotebook);

            foreach (var notebook in notebooks)
            {
                if (!notebook.Owner.IsStudent)
                {
                    continue;
                }

                CLogger.AppendToLog($"Moving page xml files for {notebook.Owner.FullName}");

                var internalPagesDirectoryPath = notebook.NotebookPagesDirectoryPath;

                var combineFolderPath = Path.Combine(DataService.DesktopFolderPath, "Combine");
                var combineStudentFolderName = $"{notebook.Owner.FullName};{notebook.Owner.ID}";
                var combineStudentFolderPath = Path.Combine(combineFolderPath, combineStudentFolderName);

                using (var zip = ZipFile.Read(zipFilePath))
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    //zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.CaseSensitiveRetrieval = true;

                    var directoryInfo = new DirectoryInfo(combineStudentFolderPath);
                    foreach (var fileInfo in directoryInfo.GetFiles())
                    {
                        var fileNameWithExtension = fileInfo.Name;
                        var pageFilePath = fileInfo.FullName;

                        var internalFilePath = ZipExtensions.CombineEntryDirectoryAndName(internalPagesDirectoryPath, fileNameWithExtension);
                        if (zip.ContainsEntry(internalFilePath))
                        {
                            continue;
                        }

                        var entry = zip.AddFile(pageFilePath);
                        entry.FileName = internalFilePath;
                    }

                    zip.Save();
                }

                CLogger.AppendToLog($"Finished moving page xml files for {notebook.Owner.FullName}");
            }

            CLogger.AppendToLog("Finished Combination.");            
        }

        public static void Stitch()
        {
            #region Constraints

            var ericPageNumbers = new List<int> { 31, 44, 49, 51, 91, 103, 104, 105, 106, 107, 108, 109, 119, 120, 121, 122, 123, 134, 125, 126, 127, 128, 140, 141, 142, 143, 144, 145, 146, 147, 152, 153, 154, 155, 156, 157, 158, 159, 160, 164, 165, 166, 167, 168, 169, 170, 171, 172, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 227, 228, 229, 230, 231, 242, 243, 244, 245, 246, 249, 278, 280, 287, 289, 295, 296, 298, 308, 310, 321, 362, 363, 364, 365, 366, 367, 369, 376 };
            var andeePageNumbers = new List<int> { 12, 14, 15, 16, 17, 18, 19, 20, 66, 300, 302, 306, 312, 318, 332, 322, 323, 324, 325, 337, 338, 339, 342, 344, 354, 355, 356, 357, 377, 378 };
            // 1 and 2 should both look in LK2 .clp file as it contains everything from LK .clp file
            var lily1PageNumbers = new List<int> { 11, 43, 56, 59, 61, 62, 65, 86, 148, 149, 150, 151, 161, 162, 163, 173, 174, 175, 198, 199, 200, 213, 214, 215, 217, 218, 219, 221, 225, 226, 232, 233, 234, 236, 240, 241, 248, 251, 252, 253, 254, 255, 258, 259, 260, 261, 262, 263, 264, 285, 286, 288, 290, 291, 293, 297, 299, 301, 303, 304, 307, 309, 311, 313, 314, 315, 326, 327, 328, 329, 330, 331, 332, 333, 334, 335, 336, 341, 343, 345, 346, 347, 348, 349, 350, 351, 352, 353, 368, 379, 380, 381, 382, 383, 384, 385 };
            var lily2PageNumbers = new List<int> { 275, 276, 281, 10, 21, 22, 28, 29, 30, 32, 45, 50, 67, 68, 110, 111, 112, 118, 129, 130, 131, 201, 216, 220, 222, 223, 224, 235, 237, 238, 239, 256, 257, 266, 267, 283, 292, 305, 316, 317 };

            #endregion // Constraints

            // 386

            var pageNumbers = Enumerable.Range(1, 386);

            var authorPages = new List<CLPPage>();

            foreach (var pageNumber in pageNumbers)
            {
                var authorTagCachePath = string.Empty;
                if (lily1PageNumbers.Contains(pageNumber))
                {
                    authorTagCachePath = AnnAuthorTagsLily1;
                }
                if (lily2PageNumbers.Contains(pageNumber))
                {
                    authorTagCachePath = AnnAuthorTagsLily2;
                }
                else if (andeePageNumbers.Contains(pageNumber))
                {
                    authorTagCachePath = AnnAuthorTagsAndee;
                }
                else if (ericPageNumbers.Contains(pageNumber))
                {
                    authorTagCachePath = AnnAuthorTagsEric;
                }

                if (string.IsNullOrWhiteSpace(authorTagCachePath))
                {
                    authorTagCachePath = AnnAuthorTagsLily1;
                }

                #region Load Author Page

                var zipContainerFilePath = authorTagCachePath;

                var pageZipEntryLoaders = new List<DataService.PageZipEntryLoader>();
                using (var zip = ZipFile.Read(zipContainerFilePath))
                {
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.CompressionLevel = CompressionLevel.None;
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.CaseSensitiveRetrieval = true;

                    var internalPagesDirectory = "notebooks/A;AUTHOR;AUTHOR0000000000000000/pages/";
                    var allPageEntries = zip.GetEntriesInDirectory(internalPagesDirectory).ToList();
                    var pageEntries = (from pageEntry in allPageEntries
                                       let nameComposite = CLPPage.NameComposite.ParseFromString(pageEntry.GetEntryNameWithoutExtension())
                                       where nameComposite.PageNumber == pageNumber
                                       select pageEntry).ToList();

                    pageZipEntryLoaders = DataService.GetPageZipEntryLoadersFromEntries(pageEntries);
                }

                var authorPage = DataService.GetPagesFromPageZipEntryLoaders(pageZipEntryLoaders, zipContainerFilePath).FirstOrDefault();
                authorPages.Add(authorPage);

                #endregion // Load Author Page
            }

            var zipPath = Path.Combine(DataService.DesktopFolderPath, "Ann - Fall 2014 - Stitched.clp");
            var notebooks = new List<Notebook>();

            using (var zip = ZipFile.Read(AnnAuthorTagsLily1))
            {
                var notebookEntry = zip.SelectEntries($"*{Notebook.DEFAULT_INTERNAL_FILE_NAME}.xml").First();
                var notebookString = notebookEntry.ExtractXmlString();
                var notebook = ASerializableBase.FromXmlString<Notebook>(notebookString);
                notebook.ContainerZipFilePath = zipPath;

                notebook.Pages = authorPages.OrderBy(p => p.PageNumber).ToObservableCollection();
                notebook.CurrentPage = notebook.Pages.FirstOrDefault();

                notebooks.Add(notebook);
            }
            
            SaveNotebooksToZip(zipPath, notebooks);
        }

        public static Notebook ConvertCacheAnnNotebook(string notebookFolder)
        {
            CLogger.AppendToLog($"Loading Notebook To Convert: {notebookFolder}");
            Ann.Notebook oldNotebook;

#pragma warning disable 162
            if (IS_LARGE_CACHE)

            {
                oldNotebook = AnnCustomPartialNotebookLoading(notebookFolder);
                //oldNotebook = Ann.Notebook.LoadLocalFullNotebook(notebookFolder);
            }
            else
            {
                oldNotebook = Ann.Notebook.LoadLocalFullNotebook(notebookFolder);
            }
#pragma warning restore 162

            CLogger.AppendToLog("Notebook Loaded");
            var newNotebook = ConvertNotebook(oldNotebook);
            CLogger.AppendToLog("Notebook Converted");

            foreach (var page in oldNotebook.Pages)
            {
                CLogger.AppendToLog($"Converting Page {page.PageNumber} for {page.Owner.FullName}");
                var newPage = ConvertPage(page);
                CLogger.AppendToLog($"Finished Converting Page {page.PageNumber} for {page.Owner.FullName}");
                foreach (var submission in page.Submissions)
                {
                    CLogger.AppendToLog($"Converting Submission Version {submission.VersionIndex} for Page {page.PageNumber} for {page.Owner.FullName}");
                    var newSubmission = ConvertPage(submission);
                    CLogger.AppendToLog($"Finished Converting Submission Version {submission.VersionIndex} for Page {page.PageNumber} for {page.Owner.FullName}");
                    newPage.Submissions.Add(newSubmission);
                }

                PurifyPageSubmissions(newPage);

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

        public static void PurifyPageSubmissions(CLPPage page)
        {
            var purifiedSubmissions = page.Submissions.GroupBy(s => s.History.UndoActions.Count).Select(s => s.Last()).ToList();
            page.Submissions = purifiedSubmissions.OrderBy(s => s.SubmissionTime).ToObservableCollection();
            var versionIndex = 1;
            foreach (var submission in page.Submissions)
            {
                submission.VersionIndex = (uint)versionIndex;
                submission.LastVersionIndex = (uint)versionIndex;
                versionIndex++;
            }

            page.LastVersionIndex = page.Submissions.Any() ? (uint?)page.Submissions.Count : null;
        }

        public static Ann.Notebook AnnCustomPartialNotebookLoading(string notebookFolderPath)
        {
            var notebook = Ann.Notebook.LoadLocalNotebook(notebookFolderPath);
            if (ReferenceEquals(null, notebook))
            {
                return null;
            }

            #region Constraints

            //var pageNumbersToLoad = new List<int> { 209, 218, 222, 235, 253, 258, 259, 262, 275, 276, 278, 281, 295, 299, 300, 307, 323, 328, 346, 360, 368, 370, 382, 383, 384, 385 };
            //var pageNumbersToLoad = new List<int>
            //                        {
            //                            218,
            //                            276,
            //                            323,
            //                            328,
            //                            346,
            //                            253,
            //                            383,
            //                            281,
            //                            328,
            //                            368,
            //                            275,
            //                            295,
            //                            281,
            //                            300
            //                        };

            // NL Playback issues
            //var pageNumbersToLoad = new List<int>
            //                        {
            //                            253,
            //                            323,
            //                            328
            //                        };

            //var pageNumbersToLoad = new List<int>
            //                        {
            //                            222,
            //                            235,
            //                            258,
            //                            259,
            //                            262,
            //                            278,
            //                            299,
            //                            307,
            //                            360,
            //                            370,
            //                            384,
            //                            385
            //                        };

            // On  8/11/2017
            var pageNumbersToLoad = new List<int>
                                    {
                                        306,
                                        308,
                                        310,
                                        312,
                                        314,
                                        318,
                                        321,
                                        322,
                                        323,
                                        324,
                                        325,
                                        342,
                                        344,
                                        362,
                                        363,
                                        364,
                                        365,
                                        366,
                                        367,
                                        368,
                                        369,
                                        376,
                                        377,
                                        378
                                    };

            pageNumbersToLoad = pageNumbersToLoad.Distinct().ToList();

            #endregion // Constraints

            #region Load Pages

            var pagesFolderPath = Path.Combine(notebookFolderPath, "Pages");
            var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml");
            var loadedPages = new List<Ann.CLPPage>();

            foreach (var pageFilePath in pageFilePaths)
            {
                var pageNameComposite = Ann.PageNameComposite.ParseFilePathToNameComposite(pageFilePath);
                if (ReferenceEquals(null, pageNameComposite))
                {
                    continue;
                }

                if (pageNameComposite.VersionIndex != "0" &&
                    !IS_CONVERTING_SUBMISSIONS)
                {
                    continue;
                }

                var isPageToBeLoaded = pageNumbersToLoad.Any(n => n.ToString() == pageNameComposite.PageNumber);
                if (!isPageToBeLoaded)
                {
                    continue;
                }

                var page = Ann.CLPPage.LoadLocalPage(pageFilePath);
                if (ReferenceEquals(null, page))
                {
                    continue;
                }

                loadedPages.Add(page);
            }

            #endregion // Load Pages

            var notebookPages = loadedPages.Where(p => p.VersionIndex == 0).OrderBy(p => p.PageNumber).ToList();
            if (IS_CONVERTING_SUBMISSIONS)
            {
                foreach (var notebookPage in notebookPages)
                {
                    var mostRecentSubmissions = loadedPages.Where(p => p.PageNumber == notebookPage.PageNumber && p.VersionIndex != 0).OrderBy(p => p.VersionIndex).LastOrDefault();
                    if (!ReferenceEquals(null, mostRecentSubmissions))
                    {
                        notebookPage.Submissions.Add(mostRecentSubmissions);
                    }
                }
            }
            notebook.Pages = new ObservableCollection<Ann.CLPPage>(notebookPages);
            notebook.CurrentPage = notebook.Pages.FirstOrDefault();

            return notebook;
        }

        public static Notebook ConvertCacheAnnNotebookFile(string notebookFolderPath)
        {
            var oldNotebook = Ann.Notebook.LoadLocalNotebook(notebookFolderPath);
            if (ReferenceEquals(null, oldNotebook))
            {
                return null;
            }

            var newNotebook = ConvertNotebook(oldNotebook);
            return newNotebook;
        }

        public static string GetPageFilePathFromPageNumber(string studentPagesFolderPath, int pageNumber)
        {
            var pageFilePaths = Directory.EnumerateFiles(studentPagesFolderPath, "*.xml");
            foreach (var pageFilePath in pageFilePaths)
            {
                var pageNameComposite = Ann.PageNameComposite.ParseFilePathToNameComposite(pageFilePath);
                if (ReferenceEquals(null, pageNameComposite))
                {
                    continue;
                }

                if (pageNameComposite.VersionIndex != "0")
                {
                    continue;
                }

                if (pageNumber.ToString() != pageNameComposite.PageNumber)
                {
                    continue;
                }

                return pageFilePath;
            }

            return string.Empty;
        }

        public static CLPPage ConvertCacheAnnPageFile(string pageFilePath)
        {
            var oldPage = Ann.CLPPage.LoadLocalPage(pageFilePath);
            if (ReferenceEquals(null, oldPage))
            {
                return null;
            }

            var newPage = ConvertPage(oldPage);
            return newPage;
        }

        public static ClassRoster ConvertCacheAnnClassSubject(string filePath, Notebook notebook)
        {
            CLogger.AppendToLog($"Loading Subject To Convert: {filePath}");
            var classSubject = Ann.ClassSubject.OpenClassSubject(filePath);
            CLogger.AppendToLog("Subject Loaded");
            var classRoster = ConvertAnnClassSubject(classSubject, notebook);
            CLogger.AppendToLog("Subject Converted");

            return classRoster;
        }

        public static Session ConvertCacheAnnClassPeriod(string filePath)
        {
            CLogger.AppendToLog($"Loading Class Period To Convert: {filePath}");
            var classPeriod = Ann.ClassPeriod.LoadLocalClassPeriod(filePath);
            CLogger.AppendToLog("Class Period Loaded");
            var session = ConvertClassPeriod(classPeriod);
            CLogger.AppendToLog("Class Period Converted");

            return session;
        }

        #endregion // Conversion Loop

        #region Notebook Parts

        // 12/7
        public static Person ConvertPerson(Ann.Person person)
        {
            var name = person.FullName;
            if (IS_ANONYMIZED_CACHE)
            {
                const string TEXT_FILE_NAME = "AnonymousNames - Ann.txt";
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
            newPerson.CurrentDifferentiationGroup = person.CurrentDifferentiationGroup;
            newPerson.TemporaryDifferentiationGroup = person.TempDifferentiationGroup;

            if (string.IsNullOrWhiteSpace(newPerson.FullName))
            {
                CLogger.AppendToLog($"[CONVERSION ERROR]: Person.FullName is blank. Original Person.FullName is {person.FullName}.");
            }

            #region Gender Assignments

            newPerson.Gender = Genders.Female;
            var maleIDs = new List<string>
                          {
                              "nUd1x4-oukipFNwazvDrfQ",
                              "5Saq8eHcvkW4m5ILxSHFZg",
                              "VKEj_15f30-yNA4oqRiHhA",
                              "SKcmfl_FwEC6zgSrUv7N7Q",
                              "OwuTe3Bzo0WFDcUDQRd4rQ",
                              "L6H-WqceTk-2A31gWRwHLg",
                              "eO9HFRoY-0aLtcL2iA5-tQ",
                              "Dn7CAxRwF0W9RX3Lwe9_uA"
                          };

            if (maleIDs.Contains(newPerson.ID))
            {
                newPerson.Gender = Genders.Male;
            }

            #endregion // Gender Assignments

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

            if (PageIDToNumberMap.ContainsKey(classPeriod.StartPageID))
            {
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
            }
            
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

            foreach (var stroke in page.InkStrokes)
            {
                newPage.InkStrokes.Add(stroke);
            }

            foreach (var pageObject in page.PageObjects)
            {
                // Ignores the TextBoxes that were part of the old Multiple Choice for Large Cache
                if (pageObject is Ann.CLPTextBox &&
                    (pageObject.ID == "Qgvdw-4oTk2JmSUECrftNQ" ||     // page 385
                     pageObject.ID == "F66Db6O3a0uTF0SGRiUemw" ||     // page 384
                     pageObject.ID == "MAnCvJx2HkmhNwY9AMNbNQ" ||     // page 383
                     pageObject.ID == "gXG3pxqR00S8dsJqmCrl9g" ||     // page 382
                     pageObject.ID == "wPC1vKURUk-iZExkAu9cwQ"))      // page 381
                {
                    continue;
                }

                // Ignores the TextBoxes that were part of the old Multiple Choice for Assessment
                if (pageObject is Ann.CLPTextBox &&
                    (pageObject.ID == "hsHhMK1dM0mfY0Rl3GCGqw" ||     // page 2
                     pageObject.ID == "1d_OeI1Kl0yJjXdvfrEeHA" ||     // page 3
                     pageObject.ID == "HXP2fZiWS0-Nc_NxIBBxRg" ||     // page 4
                     pageObject.ID == "rcBWT95ExEW9DuS8xkK2Xw" ||     // page 5
                     pageObject.ID == "QPzA5GnIUkSE5opKl8nm8g"))      // page 6
                {
                    continue;
                }

                var newPageObject = ConvertPageObject(pageObject, newPage);
                if (newPageObject == null)
                {
                    continue;
                }
                newPage.PageObjects.Add(newPageObject);
            }

#pragma warning disable 162
            if (IS_LARGE_CACHE)
            {
                AddLargeCacheTagsAndInterpretationRegions(newPage);
            }
            else
            {
                AddAssessmentInterpretationRegions(newPage);
                AddAssessmentRelationDefinitionTags(newPage);
            }
#pragma warning restore 162

            ConvertPageHistory(page.History, newPage);


            HistoryAnalysis.GenerateSemanticEvents(newPage);
            if (!IS_LARGE_CACHE)
            {
                //AnalysisPanelViewModel.AnalyzeSkipCountingStatic(newPage);
            }

            return newPage;
        }

        #endregion // Notebook Parts

        #region PageObjects

        public static List<string> CapturedStrokesLog = new List<string>();

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
                CLogger.AppendToLog($"[ERROR] newPageObject is NULL. Original pageObject is {pageObject.GetType()}");
            }

            return newPageObject;
        }

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

            #region Assessment Cache Adjustments

            switch (newTextBox.ID)
            {
                case "lpIezx13R0-fHaXnVqgT6A":
                    newTextBox.TextContext = TextContexts.NonWordProblem;   // Page 2
                    break;
                case "LZlupX4OskOkxC-VQv1pKg":
                    newTextBox.TextContext = TextContexts.NonWordProblem;   // Page 3
                    break;
                case "DvQf2cvBkU-WFEFmLBEuoA":
                    newTextBox.TextContext = TextContexts.NonWordProblem;   // Page 4
                    break;
                case "JsuHVsdb6k2zYQGS8HdeJA":
                    newTextBox.TextContext = TextContexts.WordProblem;      // Page 5
                    break;
                case "3A0ABSEEdUa487Mkvp9CcQ":
                    newTextBox.TextContext = TextContexts.WordProblem;      // Page 6
                    break;
                case "bC1g8LJ6okmsezeVSRub4A":
                    newTextBox.TextContext = TextContexts.NonWordProblem;   // Page 7
                    break;
                case "_0qgnvZ1EkyYgEU49l5dNw":
                    newTextBox.TextContext = TextContexts.NonWordProblem;   // Page 8
                    break;
                case "DisblHoHakqYkPzMu9_bxQ":
                    newTextBox.TextContext = TextContexts.NonWordProblem;   // Page 9
                    break;
                case "GBXW7G0YmEKXQ4Q_MMIn5g":
                    newTextBox.TextContext = TextContexts.WordProblem;      // Page 10
                    break;
                case "MtZusuAFZEOqTr8KRlFlMA":
                    newTextBox.TextContext = TextContexts.WordProblem;      // Page 11
                    break;
                case "JleS1FBQiEGyoe4VseiPMA":
                    newTextBox.TextContext = TextContexts.WordProblem;      // Page 12
                    break;
                case "SNY1QJrMUUqUeK3hCIDDRA":
                    newTextBox.TextContext = TextContexts.WordProblem;      // Page 13
                    break;
            }

            #endregion // Assessment Cache Adjustments

            return newTextBox;
        }

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
            newArray.AcceptedStrokeParentIDs = array.AcceptedStrokeParentIDs;
            newArray.IsInnerPart = array.IsInnerPart;
            newArray.IsPartsAutoGenerated = array.IsPartsAutoGenerated;

            foreach (var acceptedStrokeParentID in newArray.AcceptedStrokeParentIDs)
            {
                var stroke = newPage.GetStrokeByIDOnPageOrInHistory(acceptedStrokeParentID);
                newArray.AcceptedStrokes.Add(stroke);

                var logString = $"Array - {newPage.PageNumber} - {newPage.Owner.FullName}";
                CapturedStrokesLog.Add(logString);
            }

            return newArray;
        }

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
            newNumberLine.AcceptedStrokeParentIDs = numberLine.AcceptedStrokeParentIDs;

            foreach (var acceptedStrokeParentID in newNumberLine.AcceptedStrokeParentIDs)
            {
                var stroke = newPage.GetStrokeByIDOnPageOrInHistory(acceptedStrokeParentID);
                newNumberLine.AcceptedStrokes.Add(stroke);
            }

            return newNumberLine;
        }

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
            newStampedObject.AcceptedPageObjectIDs = stampedObject.AcceptedPageObjectIDs;

            if (newStampedObject.AcceptedPageObjectIDs.Any())
            {
                var logString = $"StampObject has Captured PageObjects - {newPage.PageNumber} - {newPage.Owner.FullName}";
                CapturedStrokesLog.Add(logString);
            }

            return newStampedObject;
        }

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
            newStamp.AcceptedStrokeParentIDs = stamp.AcceptedStrokeParentIDs;

            foreach (var acceptedStrokeParentID in newStamp.AcceptedStrokeParentIDs)
            {
                var stroke = newPage.GetStrokeByIDOnPageOrInHistory(acceptedStrokeParentID);
                newStamp.AcceptedStrokes.Add(stroke);

                var logString = $"Stamp - {newPage.PageNumber} - {newPage.Owner.FullName}";
                CapturedStrokesLog.Add(logString);
            }

            newStamp.CanAcceptPageObjects = stamp.CanAcceptPageObjects;
            newStamp.AcceptedPageObjectIDs = stamp.AcceptedPageObjectIDs;

            if (newStamp.AcceptedPageObjectIDs.Any())
            {
                var logString = $"Stamp has Captured PageObjects - {newPage.PageNumber} - {newPage.Owner.FullName}";
                CapturedStrokesLog.Add(logString);
            }

            return newStamp;
        }

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

            #region Large Cache Conversion

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
            }

            #endregion // Large Cache Conversion

            #region Assessment Cache Conversion

            switch (newPage.ID)
            {
                case "-zOauyypbEmgpo3f_dalNA": // Page 2
                {
                    var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "4"
                             };
                    var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth,
                                 Answer = "5",
                                 IsACorrectValue = true
                             };
                    var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth * 2,
                                 Answer = "7"
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth * 3,
                                 Answer = "9"
                             };
                    newMultipleChoice.ChoiceBubbles.Add(b1);
                    newMultipleChoice.ChoiceBubbles.Add(b2);
                    newMultipleChoice.ChoiceBubbles.Add(b3);
                    newMultipleChoice.ChoiceBubbles.Add(b4);
                    break;
                }
                case "UvLXlXlpCEuLF1309g5zPA": // Page 3
                {
                    var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "9 + 7"
                             };
                    var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth,
                                 Answer = "9 - 7"
                             };
                    var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth * 2,
                                 Answer = "7 x 9",
                                 IsACorrectValue = true
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth * 3,
                                 Answer = "63 / 9"
                             };
                    newMultipleChoice.ChoiceBubbles.Add(b1);
                    newMultipleChoice.ChoiceBubbles.Add(b2);
                    newMultipleChoice.ChoiceBubbles.Add(b3);
                    newMultipleChoice.ChoiceBubbles.Add(b4);
                    break;
                }
                case "526u6U8sQUqjFkCXTJZYiA": // Page 4
                {
                    var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "2"
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
                                 Answer = "5"
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth * 3,
                                 Answer = "8"
                             };
                    newMultipleChoice.ChoiceBubbles.Add(b1);
                    newMultipleChoice.ChoiceBubbles.Add(b2);
                    newMultipleChoice.ChoiceBubbles.Add(b3);
                    newMultipleChoice.ChoiceBubbles.Add(b4);
                    break;
                }
                case "y-wako1KCk6Aurwrn5QbVg": // Page 5
                {
                    var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "16",
                                 AnswerLabel = "years old"
                             };
                    var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth,
                                 Answer = "24",
                                 AnswerLabel = "years old"
                             };
                    var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth * 2,
                                 Answer = "64",
                                 AnswerLabel = "years old",
                                 IsACorrectValue = true
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth * 3,
                                 Answer = "80",
                                 AnswerLabel = "years old"
                             };
                    newMultipleChoice.ChoiceBubbles.Add(b1);
                    newMultipleChoice.ChoiceBubbles.Add(b2);
                    newMultipleChoice.ChoiceBubbles.Add(b3);
                    newMultipleChoice.ChoiceBubbles.Add(b4);
                    break;
                }
                case "_024ibxTi0qlw4gzCD7QXA": // Page 6
                {
                    var b1 = new ChoiceBubble(0, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = 0,
                                 Answer = "$5"
                             };
                    var b2 = new ChoiceBubble(1, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth,
                                 Answer = "$7"
                             };
                    var b3 = new ChoiceBubble(2, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth * 2,
                                 Answer = "$8",
                                 IsACorrectValue = true
                             };
                    var b4 = new ChoiceBubble(3, MultipleChoiceLabelTypes.Letters)
                             {
                                 Offset = segmentWidth * 3,
                                 Answer = "$55"
                             };
                    newMultipleChoice.ChoiceBubbles.Add(b1);
                    newMultipleChoice.ChoiceBubbles.Add(b2);
                    newMultipleChoice.ChoiceBubbles.Add(b3);
                    newMultipleChoice.ChoiceBubbles.Add(b4);
                    break;
                }
            }

            #endregion // Assessment Cache Conversion

            if (!newMultipleChoice.ChoiceBubbles.Any())
            {
                CLogger.AppendToLog($"[ERROR] Unhandled Multiple Choice Box during conversion. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}");

                newMultipleChoice = null;
            }

            return newMultipleChoice;
        }

        public static void AddAssessmentInterpretationRegions(CLPPage newPage)
        {
            var interpretationRegion = new InterpretationRegion(newPage)
                                       {
                                           CreatorID = Person.AUTHOR_ID,
                                           OwnerID = Person.AUTHOR_ID
                                       };
            interpretationRegion.Interpreters.Add(Interpreters.Handwriting);

            switch (newPage.ID)
            {
                case "_ctKrAO-MEK-g9PtqpFzVQ": // Page 7
                    {
                        interpretationRegion.ID = "_ctKrAO-MEK-g9PtqpFmoo";
                        interpretationRegion.XPosition = 235.3954;
                        interpretationRegion.YPosition = 220.9490;
                        interpretationRegion.Height = 80;
                        interpretationRegion.Width = 80;
                        break;
                    }
                case "gdruAzwX6kWe2k-etZ6gcQ": // Page 8
                    {

                        interpretationRegion.ID = "gdruAzwX6kWe2k-etZ6moo";
                        interpretationRegion.XPosition = 253.1625;
                        interpretationRegion.YPosition = 221.9357;
                        interpretationRegion.Height = 80;
                        interpretationRegion.Width = 80;
                        break;
                    }
                case "yzvpdIROIEOFrndOASGjvA": // Page 9
                    {
                        interpretationRegion.ID = "yzvpdIROIEOFrndOASGmoo";
                        interpretationRegion.XPosition = 106.74036;
                        interpretationRegion.YPosition = 223.4880;
                        interpretationRegion.Height = 80;
                        interpretationRegion.Width = 80;
                        break;
                    }
                case "gsQu4sdxVEKGZsgCD_zfWQ": // Page 10
                    {
                        interpretationRegion.ID = "gsQu4sdxVEKGZsgCD_zmoo";
                        interpretationRegion.XPosition = 98.90192;
                        interpretationRegion.YPosition = 205.11349;
                        interpretationRegion.Height = 102.1971;
                        interpretationRegion.Width = 171.0040;
                        break;
                    }
                case "MtZusuAFZEOqTr8KRlFlMA": // Page 11
                    {
                        interpretationRegion.ID = "MtZusuAFZEOqTr8KRlFmoo";
                        interpretationRegion.XPosition = 103.60754;
                        interpretationRegion.YPosition = 243.3032;
                        interpretationRegion.Height = 93.2830;
                        interpretationRegion.Width = 146.4150;
                        break;
                    }
                case "QHJ7pFHY3ECr8u6bSFRCkA": // Page 12
                    {
                        interpretationRegion.ID = "QHJ7pFHY3ECr8u6bSFRmoo";
                        interpretationRegion.XPosition = 234.6666;
                        interpretationRegion.YPosition = 668.9809;
                        interpretationRegion.Height = 116.3069;
                        interpretationRegion.Width = 128.3371;
                        break;
                    }
                case "cgXYlAbAM0GGy8iBI4tyGw": // Page 13
                    {
                        interpretationRegion.ID = "cgXYlAbAM0GGy8iBI4tmoo";
                        interpretationRegion.XPosition = 220.3143;
                        interpretationRegion.YPosition = 661.8379;
                        interpretationRegion.Height = 131.1860;
                        interpretationRegion.Width = 123.4241;
                        break;
                    }
                default:
                    return;
            }

            newPage.PageObjects.Add(interpretationRegion);
        }

        #endregion // PageObjects

        #region History

        public static void ConvertPageHistory(Ann.PageHistory pageHistory, CLPPage newPage)
        {
            var newPageHistory = new PageHistory();
            newPage.History = newPageHistory;

            foreach (var trashedInkStroke in pageHistory.TrashedInkStrokes)
            {
                newPageHistory.TrashedInkStrokes.Add(trashedInkStroke);
            }

            foreach (var trashedPageObject in pageHistory.TrashedPageObjects)
            {
                var newTrashedPageObject = ConvertPageObject(trashedPageObject, newPage);
                if (newTrashedPageObject == null)
                {
                    continue;
                }
                newPageHistory.TrashedPageObjects.Add(newTrashedPageObject);
            }

            if (pageHistory.RedoItems.Any())
            {
                CLogger.AppendToLog($"[ERROR] PageHistory Has Redo Items. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}");
                return;
            }

            newPageHistory.IsAnimating = true;

            #region Undo

            var unconvertedUndoItems = pageHistory.UndoItems.Where(h => h.OwnerID != Person.Author.ID).ToList();
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
            newPageHistory.RefreshCachedFormattedValues();
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
#pragma warning disable CS0618 // Type or member is obsolete
            }).Case<Ann.PageObjectMoveBatchHistoryItem>(h =>
#pragma warning restore CS0618 // Type or member is obsolete
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
                CLogger.AppendToLog($"[ERROR] newHistoryAction is NULL. Original historyItem is {historyItem.GetType()}. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[NON-ERROR] PageObject Added, no pageObjects added. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                    CLogger.AppendToLog($"[ERROR] PageObject for PageObject Added not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[NON-ERROR] PageObject Removed, no pageObjects added. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                    CLogger.AppendToLog($"[ERROR] PageObject for PageObject Removed not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
            if (historyItem.StretchedDimensions.Count < 2)
            {
                CLogger.AppendToLog($"[NON-ERROR] PageObject Resize has no Streched Dimensions (or 1). Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[ERROR] PageObject for PageObject Resize not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[NON-ERROR] PageObject Move has NULL PageObjectID. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            if (!historyItem.TravelledPositions.Any())
            {
                CLogger.AppendToLog($"[NON-ERROR] PageObject Move has no Travelled Positions. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            if (historyItem.TravelledPositions.Count == 2 &&
                Math.Abs(historyItem.TravelledPositions.First().X - historyItem.TravelledPositions.Last().X) < 0.00001 &&
                Math.Abs(historyItem.TravelledPositions.First().Y - historyItem.TravelledPositions.Last().Y) < 0.00001)
            {
                CLogger.AppendToLog($"[NON-ERROR] PageObject Move has the same Travelled Positions. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                    CLogger.AppendToLog($"[ERROR] PageObject for PageObject Move not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[NON-ERROR] PageObjects Move has no PageObjectIDs. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            if (!historyItem.TravelledPositions.Any())
            {
                CLogger.AppendToLog($"[NON-ERROR] PageObjects Move has no Travelled Positions. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            if (historyItem.TravelledPositions.Count == 2 &&
                Math.Abs(historyItem.TravelledPositions.First().X - historyItem.TravelledPositions.Last().X) < 0.00001 &&
                Math.Abs(historyItem.TravelledPositions.First().Y - historyItem.TravelledPositions.Last().Y) < 0.00001)
            {
                CLogger.AppendToLog($"[NON-ERROR] PageObjects Move has the same Travelled Positions. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                    CLogger.AppendToLog($"[ERROR] PageObjects for PageObject Move not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[NON-ERROR] PageObject Cut has NULL Cutting Stroke ID. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var cuttingStroke = newPage.GetVerifiedStrokeInHistoryByID(historyItem.CuttingStrokeID);
            if (cuttingStroke == null)
            {
                CLogger.AppendToLog($"[NON-ERROR] PageObject Cut has NULL Cutting Stroke. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                        CLogger.AppendToLog($"[ERROR] PageObject Cut has one Cut PageObject, but more than 2 Halved PageObjects. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                        CLogger.AppendToLog($"[ERROR] Halved PageObject for PageObject Cut not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                        return null;
                    }
                    newPage.PageObjects.Remove(halvedPageObject);
                    newPage.History.TrashedPageObjects.Add(halvedPageObject);
                }

                var cutPageObject = newPage.GetVerifiedPageObjectInTrashByID(newHistoryAction.CutPageObjectID);
                if (cutPageObject == null)
                {
                    CLogger.AppendToLog($"[ERROR] Cut PageObject for PageObject Cut not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[ERROR] PageObject Cut has mismatched number of Cut PageObjects and Halved PageObjects. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                    CLogger.AppendToLog($"[ERROR] Cut PageObject on PageObject Cut not found in history or on page. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                        CLogger.AppendToLog($"[ERROR] Halved PageObject for PageObject Cut not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                        return null;
                    }
                    newPage.PageObjects.Remove(halvedPageObject);
                    newPage.History.TrashedPageObjects.Add(halvedPageObject);
                }

                var cutPageObject = newPage.GetVerifiedPageObjectInTrashByID(historyAction.CutPageObjectID);
                if (cutPageObject == null)
                {
                    CLogger.AppendToLog($"[ERROR] Cut PageObject for PageObject Cut not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[ERROR] Array for Rotate not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[ERROR] Array for Grid Toggle not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[ERROR] Persisting Array for Snap not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var snappedArray = newPage.GetVerifiedPageObjectInTrashByID(newHistoryAction.SnappedArrayID) as CLPArray;
            if (snappedArray == null)
            {
                CLogger.AppendToLog($"[ERROR] Snapped Array for Snap not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[ERROR] Array for Division Value Changed not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[ERROR] Division Value Changed, Division Index out of bounds. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[NON-ERROR] Division Values Changed, empty divisions. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[ERROR] Array for Divisions Changed not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                CLogger.AppendToLog($"[ERROR] Number Line End Point Change not followed by PageObject Resize or another Number Line End Point Change. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var numberLine = newPage.GetVerifiedPageObjectOnPageByID(historyItem.NumberLineID) as NumberLine;
            if (numberLine == null)
            {
                CLogger.AppendToLog($"[ERROR] Number Line for Number Line End Point Change not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            var resizeBatchHistoryItem = nextUnconvertedHistoryItem as Ann.PageObjectResizeBatchHistoryItem;
            if (!ReferenceEquals(null, resizeBatchHistoryItem))
            {
                var potentialNumberLineMatch = newPage.GetVerifiedPageObjectOnPageByID(resizeBatchHistoryItem.PageObjectID) as NumberLine;
                if (potentialNumberLineMatch == null ||
                    numberLine.ID != potentialNumberLineMatch.ID)
                {
                    CLogger.AppendToLog($"[ERROR] Number Line for Number Line End Point Change doesn't match next PageObject Resize Number Line. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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

            if (ReferenceEquals(null, resizeBatchHistoryItem))
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
                CLogger.AppendToLog($"[NON-ERROR] Strokes Changed, no strokes changed. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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

                if (addedStroke == null)
                {
                    CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke for AddedID doesn't exist on page or in trash. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    return null;
                }

                #region Check for Multiple Choice Fill-In

                // TODO: Necessary to handle all MC on page?
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
                                CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke in StrokeIDsAdded in MultipleChoiceBubbleStatusChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                                CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke in StrokeIDsRemoved in MultipleChoiceBubbleStatusChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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

                #region Check for Interpretation Region Fill-In

                foreach (var interpretationRegion in newPage.PageObjects.OfType<InterpretationRegion>())
                {
                    var isStrokeOver = interpretationRegion.IsStrokeOverPageObject(addedStroke);
                    if (!isStrokeOver)
                    {
                        continue;
                    }

                    interpretationRegion.ChangeAcceptedStrokes(newHistoryAction.StrokesAdded, newHistoryAction.StrokesRemoved);
                    var fillInAnswerChangedHistoryAction = new FillInAnswerChangedHistoryAction(newPage,
                                                                                                newPage.Owner,
                                                                                                interpretationRegion,
                                                                                                newHistoryAction.StrokesAdded,
                                                                                                newHistoryAction.StrokesRemoved);

                    #region FillInAnswerChangedHistoryAction Conversion Undo

                    var addedStrokesToFillInRegion = new List<Stroke>();
                    foreach (var stroke in fillInAnswerChangedHistoryAction.StrokeIDsAdded.Select(newPage.GetVerifiedStrokeOnPageByID))
                    {
                        if (stroke == null)
                        {
                            CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke in StrokeIDsAdded in FillInAnswerChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                            continue;
                        }

                        addedStrokesToFillInRegion.Add(stroke);
                        newPage.InkStrokes.Remove(stroke);
                        newPage.History.TrashedInkStrokes.Add(stroke);
                    }

                    var removedStrokesToFillInRegion = new List<Stroke>();
                    foreach (var stroke in fillInAnswerChangedHistoryAction.StrokeIDsRemoved.Select(newPage.GetVerifiedStrokeInHistoryByID))
                    {
                        if (stroke == null)
                        {
                            CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke in StrokeIDsRemoved in FillInAnswerChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                            continue;
                        }

                        removedStrokesToFillInRegion.Add(stroke);
                        newPage.History.TrashedInkStrokes.Remove(stroke);
                        newPage.InkStrokes.Add(stroke);
                    }

                    interpretationRegion.ChangeAcceptedStrokes(removedStrokesToFillInRegion, addedStrokesToFillInRegion);

                    #endregion // FillInAnswerChangedHistoryAction Conversion Undo

                    return fillInAnswerChangedHistoryAction;
                }

                #endregion // Check for Interpretation Region Fill-In

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
                            CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke in AddedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
            }
            // Single or Multiple Remove
            // HACK: This originally dealt with only a Single Stroke Remove and had the following if-statement:
            // else if (newHistoryAction.StrokeIDsRemoved.Count == 1 &&
            // !newHistoryAction.StrokeIDsAdded.Any())
            // Now if foreach loops through all StrokeIDsRemoved, handling lasso erases as well, but dividing each erased stroke into it's own historyAction
            else if (newHistoryAction.StrokeIDsRemoved.Any() &&
                     !newHistoryAction.StrokeIDsAdded.Any())
            {
                var newHistoryActions = new List<IHistoryAction>();
                var removedStrokeIDHandledByOtherHistoryAction = new List<string>();
                foreach (var strokeID in newHistoryAction.StrokeIDsRemoved)
                {
                    var removedStroke = newPage.GetVerifiedStrokeInHistoryByID(strokeID);

                    if (removedStroke == null)
                    {
                        CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke for RemovedID doesn't exist on page or in trash. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                        continue;
                    }

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
                                    CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke in StrokeIDsAdded in MultipleChoiceBubbleStatusChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                                    CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke in StrokeIDsRemoved in MultipleChoiceBubbleStatusChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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

                            newHistoryActions.Add(multipleChoiceBubbleStatusChangedHistoryAction);
                            removedStrokeIDHandledByOtherHistoryAction.Add(strokeID);
                            continue;
                        }
                    }

                    #endregion // Check for Multiple Choice Erase

                    #region Check for Interpretation Region Erase

                    var isInterpretationRegionErase = false;
                    foreach (var interpretationRegion in newPage.PageObjects.OfType<InterpretationRegion>())
                    {
                        var isStrokeOver = interpretationRegion.IsStrokeOverPageObject(removedStroke);
                        if (!isStrokeOver)
                        {
                            continue;
                        }

                        interpretationRegion.ChangeAcceptedStrokes(newHistoryAction.StrokesAdded, newHistoryAction.StrokesRemoved);
                        var fillInAnswerChangedHistoryAction = new FillInAnswerChangedHistoryAction(newPage,
                                                                                                    newPage.Owner,
                                                                                                    interpretationRegion,
                                                                                                    newHistoryAction.StrokesAdded,
                                                                                                    newHistoryAction.StrokesRemoved);

                        #region FillInAnswerChangedHistoryAction Conversion Undo

                        var addedStrokesToFillInRegion = new List<Stroke>();
                        foreach (var stroke in fillInAnswerChangedHistoryAction.StrokeIDsAdded.Select(newPage.GetVerifiedStrokeOnPageByID))
                        {
                            if (stroke == null)
                            {
                                CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke in StrokeIDsAdded in FillInAnswerChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                                continue;
                            }

                            addedStrokesToFillInRegion.Add(stroke);
                            newPage.InkStrokes.Remove(stroke);
                            newPage.History.TrashedInkStrokes.Add(stroke);
                        }

                        var removedStrokesToFillInRegion = new List<Stroke>();
                        foreach (var stroke in fillInAnswerChangedHistoryAction.StrokeIDsRemoved.Select(newPage.GetVerifiedStrokeInHistoryByID))
                        {
                            if (stroke == null)
                            {
                                CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke in StrokeIDsRemoved in FillInAnswerChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                                continue;
                            }

                            removedStrokesToFillInRegion.Add(stroke);
                            newPage.History.TrashedInkStrokes.Remove(stroke);
                            // HACK: Djemimah Filois's page 353 conversion fails when it tries to add a stroke already on the page
                            if (!newPage.InkStrokes.Contains(stroke))
                            {
                                newPage.InkStrokes.Add(stroke);
                            }
                        }

                        interpretationRegion.ChangeAcceptedStrokes(removedStrokesToFillInRegion, addedStrokesToFillInRegion);

                        #endregion // FillInAnswerChangedHistoryAction Conversion Undo

                        newHistoryActions.Add(fillInAnswerChangedHistoryAction);
                        removedStrokeIDHandledByOtherHistoryAction.Add(strokeID);
                        isInterpretationRegionErase = true;
                    }

                    if (isInterpretationRegionErase)
                    {
                        continue;
                    }

                    #endregion // Check for Interpretation Region Erase

                    #region Check for Jump Removed

                    var isJumpErase = false;
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
                                CLogger.AppendToLog($"[ERROR] Strokes Changed, Stroke in RemovedJumpStrokeIDs in NumberLineJumpSizesChangedHistoryAction not found on page or in history. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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

                        newHistoryActions.Add(jumpsChangedHistoryAction);
                        removedStrokeIDHandledByOtherHistoryAction.Add(strokeID);
                        isJumpErase = true;
                    }

                    if (isJumpErase)
                    {
                        continue;
                    }

                    #endregion // Check for Jump Removed
                }

                foreach (var historyAction in newHistoryActions)
                {
                    newPage.History.RedoActions.Insert(0, historyAction);
                }

                foreach (var removedStrokeID in removedStrokeIDHandledByOtherHistoryAction)
                {
                    if (newHistoryAction.StrokeIDsRemoved.Contains(removedStrokeID))
                    {
                        newHistoryAction.StrokeIDsRemoved.Remove(removedStrokeID);
                    }
                }

                if (!newHistoryAction.StrokeIDsRemoved.Any())
                {
                    CLogger.AppendToLog($"[NON-ERROR] Strokes Changed, single removed stroke handled by other historyAction. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                    return null;
                }
            }
            // Point Erase
            else if (newHistoryAction.StrokesRemoved.Count == 1 &&
                     newHistoryAction.StrokesAdded.Count == 2)
            {
                CLogger.AppendToLog($"[ERROR] Strokes Changed, Point Erase. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }
            else
            {
                CLogger.AppendToLog($"[ERROR] Strokes Changed, Not Single Add, Single or Multiple Erase, or Point Erase. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            if (!newHistoryAction.IsUsingStrokes)
            {
                CLogger.AppendToLog($"[ERROR] Strokes Changed, no strokes changed. Next newHistoryAction is NULL ERROR ignorable. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
                return null;
            }

            #region ObjectsOnPageChangedHistoryAction Conversion Undo

            var addedStrokes = new List<Stroke>();
            foreach (var stroke in newHistoryAction.StrokeIDsAdded.Select(newPage.GetVerifiedStrokeOnPageByID))
            {
                if (stroke == null)
                {
                    CLogger.AppendToLog($"[ERROR] Strokes Changed, Null stroke in StrokeIDsAdded. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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
                    CLogger.AppendToLog($"[ERROR] Strokes Changed, Null stroke in StrokeIDsRemoved. Page {newPage.PageNumber}, VersionIndex {newPage.VersionIndex}, Owner: {newPage.Owner.FullName}. HistoryItemID: {historyItem.ID}");
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

        #region Tags

        public static void AddAssessmentRelationDefinitionTags(CLPPage newPage)
        {
            switch (newPage.PageNumber)
            {
                case 2:
                {
                    var firstFactorL = new NumericValueDefinitionTag(newPage, Origin.Author);
                    firstFactorL.NumericValue = 42.0;

                    var secondFactorL = new NumericValueDefinitionTag(newPage, Origin.Author);
                    secondFactorL.NumericValue = 6.0;

                    var leftRelationPart = new DivisionRelationDefinitionTag(newPage, Origin.Author);
                    leftRelationPart.Quotient = 7.0;
                    leftRelationPart.RelationType = DivisionRelationDefinitionTag.RelationTypes.GeneralDivision;
                    leftRelationPart.Dividend = firstFactorL;
                    leftRelationPart.Divisor = secondFactorL;

                    var firstFactorR = new NumericValueDefinitionTag(newPage, Origin.Author);
                    firstFactorR.NumericValue = 2.0;

                    var secondFactorR = new NumericValueDefinitionTag(newPage, Origin.Author);
                    secondFactorR.NumericValue = 5.0;
                    secondFactorR.IsNotGiven = true;

                    var rightRelationPart = new AdditionRelationDefinitionTag(newPage, Origin.Author);
                    rightRelationPart.Sum = 7.0;
                    rightRelationPart.RelationType = AdditionRelationDefinitionTag.RelationTypes.GeneralAddition;
                    rightRelationPart.Addends.Add(firstFactorR);
                    rightRelationPart.Addends.Add(secondFactorR);

                    var equivTag = new EquivalenceRelationDefinitionTag(newPage, Origin.Author);
                    equivTag.LeftRelationPart = leftRelationPart;
                    equivTag.RightRelationPart = rightRelationPart;

                    newPage.AddTag(equivTag);
                    break;
                }
                case 4:
                {
                    var firstFactorL = new NumericValueDefinitionTag(newPage, Origin.Author);
                    firstFactorL.NumericValue = 72.0;

                    var secondFactorL = new NumericValueDefinitionTag(newPage, Origin.Author);
                    secondFactorL.NumericValue = 9.0;

                    var leftRelationPart = new DivisionRelationDefinitionTag(newPage, Origin.Author);
                    leftRelationPart.Quotient = 8.0;
                    leftRelationPart.RelationType = DivisionRelationDefinitionTag.RelationTypes.GeneralDivision;
                    leftRelationPart.Dividend = firstFactorL;
                    leftRelationPart.Divisor = secondFactorL;

                    var firstFactorR = new NumericValueDefinitionTag(newPage, Origin.Author);
                    firstFactorR.NumericValue = 5.0;

                    var secondFactorR = new NumericValueDefinitionTag(newPage, Origin.Author);
                    secondFactorR.NumericValue = 3.0;
                    secondFactorR.IsNotGiven = true;

                    var rightRelationPart = new AdditionRelationDefinitionTag(newPage, Origin.Author);
                    rightRelationPart.Sum = 8.0;
                    rightRelationPart.RelationType = AdditionRelationDefinitionTag.RelationTypes.GeneralAddition;
                    rightRelationPart.Addends.Add(firstFactorR);
                    rightRelationPart.Addends.Add(secondFactorR);

                    var equivTag = new EquivalenceRelationDefinitionTag(newPage, Origin.Author);
                    equivTag.LeftRelationPart = leftRelationPart;
                    equivTag.RightRelationPart = rightRelationPart;

                    newPage.AddTag(equivTag);
                    break;
                }
                case 7:
                {
                    var firstFactorL = new NumericValueDefinitionTag(newPage, Origin.Author);
                    firstFactorL.NumericValue = 4.0;

                    var secondFactorL = new NumericValueDefinitionTag(newPage, Origin.Author);
                    secondFactorL.NumericValue = 9.0;

                    var leftRelationPart = new MultiplicationRelationDefinitionTag(newPage, Origin.Author);
                    leftRelationPart.Product = 36.0;
                    leftRelationPart.RelationType = MultiplicationRelationDefinitionTag.RelationTypes.GeneralMultiplication;
                    leftRelationPart.Factors.Add(firstFactorL);
                    leftRelationPart.Factors.Add(secondFactorL);

                    var firstFactorR = new NumericValueDefinitionTag(newPage, Origin.Author);
                    firstFactorR.NumericValue = 6.0;

                    var secondFactorR = new NumericValueDefinitionTag(newPage, Origin.Author);
                    secondFactorR.NumericValue = 6.0;
                    secondFactorR.IsNotGiven = true;

                    var rightRelationPart = new MultiplicationRelationDefinitionTag(newPage, Origin.Author);
                    rightRelationPart.Product = 36.0;
                    rightRelationPart.RelationType = MultiplicationRelationDefinitionTag.RelationTypes.GeneralMultiplication;
                    rightRelationPart.Factors.Add(firstFactorR);
                    rightRelationPart.Factors.Add(secondFactorR);

                    var equivTag = new EquivalenceRelationDefinitionTag(newPage, Origin.Author);
                    equivTag.LeftRelationPart = leftRelationPart;
                    equivTag.RightRelationPart = rightRelationPart;

                    newPage.AddTag(equivTag);
                    break;
                }
                case 8:
                {
                    var firstFactorL = new NumericValueDefinitionTag(newPage, Origin.Author);
                    firstFactorL.NumericValue = 7.0;

                    var secondFactorL = new NumericValueDefinitionTag(newPage, Origin.Author);
                    secondFactorL.NumericValue = 7.0;

                    var leftRelationPart = new MultiplicationRelationDefinitionTag(newPage, Origin.Author);
                    leftRelationPart.Product = 49.0;
                    leftRelationPart.RelationType = MultiplicationRelationDefinitionTag.RelationTypes.GeneralMultiplication;
                    leftRelationPart.Factors.Add(firstFactorL);
                    leftRelationPart.Factors.Add(secondFactorL);

                    var firstFactorR = new NumericValueDefinitionTag(newPage, Origin.Author);
                    firstFactorR.NumericValue = 30.0;

                    var secondFactorR = new NumericValueDefinitionTag(newPage, Origin.Author);
                    secondFactorR.NumericValue = 19.0;
                    secondFactorR.IsNotGiven = true;

                    var rightRelationPart = new AdditionRelationDefinitionTag(newPage, Origin.Author);
                    rightRelationPart.Sum = 49.0;
                    rightRelationPart.RelationType = AdditionRelationDefinitionTag.RelationTypes.GeneralAddition;
                    rightRelationPart.Addends.Add(firstFactorR);
                    rightRelationPart.Addends.Add(secondFactorR);

                    var equivTag = new EquivalenceRelationDefinitionTag(newPage, Origin.Author);
                    equivTag.LeftRelationPart = leftRelationPart;
                    equivTag.RightRelationPart = rightRelationPart;

                    newPage.AddTag(equivTag);
                    break;
                }
                case 9:
                {
                    var firstFactorL = new NumericValueDefinitionTag(newPage, Origin.Author);
                    firstFactorL.NumericValue = 35.0;
                    firstFactorL.IsNotGiven = true;

                    var secondFactorL = new NumericValueDefinitionTag(newPage, Origin.Author);
                    secondFactorL.NumericValue = 7.0;

                    var leftRelationPart = new DivisionRelationDefinitionTag(newPage, Origin.Author);
                    leftRelationPart.Quotient = 5.0;
                    leftRelationPart.RelationType = DivisionRelationDefinitionTag.RelationTypes.GeneralDivision;
                    leftRelationPart.Dividend = firstFactorL;
                    leftRelationPart.Divisor = secondFactorL;

                    var firstFactorR = new NumericValueDefinitionTag(newPage, Origin.Author);
                    firstFactorR.NumericValue = 40.0;

                    var secondFactorR = new NumericValueDefinitionTag(newPage, Origin.Author);
                    secondFactorR.NumericValue = 8.0;

                    var rightRelationPart = new DivisionRelationDefinitionTag(newPage, Origin.Author);
                    rightRelationPart.Quotient = 5.0;
                    rightRelationPart.RelationType = DivisionRelationDefinitionTag.RelationTypes.GeneralDivision;
                    rightRelationPart.Dividend = firstFactorR;
                    rightRelationPart.Divisor = secondFactorR;

                    var equivTag = new EquivalenceRelationDefinitionTag(newPage, Origin.Author);
                    equivTag.LeftRelationPart = leftRelationPart;
                    equivTag.RightRelationPart = rightRelationPart;

                    newPage.AddTag(equivTag);
                    break;
                }
            }

            ITag relationDefinitionToAdd = null;

            switch (newPage.ID)
            {
                case "y-wako1KCk6Aurwrn5QbVg": // Page 5
                {
                    relationDefinitionToAdd = new MultiplicationRelationDefinitionTag(newPage, Origin.Author)
                                              {
                                                  ID = "l-WC1c1mGkukYDgVm937KQ",
                                                  OwnerID = Person.Author.ID,
                                                  Product = 64,
                                                  RelationType = MultiplicationRelationDefinitionTag.RelationTypes.GeneralMultiplication
                                              };

                    var firstFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                      {
                                          ID = "8L-RIJBn_06lpKH4yBlOzg",
                                          OwnerID = Person.Author.ID,
                                          NumericValue = 8
                                      };

                    var secondFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                       {
                                           ID = "dk5lXzsvu0GHmpGgwoh-vA",
                                           OwnerID = Person.Author.ID,
                                           NumericValue = 8
                                       };

                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Clear();
                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Add(firstFactor);
                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Add(secondFactor);
                    break;
                }
                case "_024ibxTi0qlw4gzCD7QXA": // Page 6
                {
                    relationDefinitionToAdd = new DivisionRelationDefinitionTag(newPage, Origin.Author)
                                              {
                                                  ID = "U18DjuOfc0WJ7OIDClEC3A",
                                                  OwnerID = Person.Author.ID,
                                                  Quotient = 8,
                                                  Remainder = 0,
                                                  RelationType = DivisionRelationDefinitionTag.RelationTypes.GeneralDivision
                                              };

                    var dividend = new NumericValueDefinitionTag(newPage, Origin.Author)
                                   {
                                       ID = "AL-RIJBn_06lpKH4yBlOzg",
                                       OwnerID = Person.Author.ID,
                                       NumericValue = 56
                                   };

                    var divisor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                  {
                                      ID = "Ak5lXzsvu0GHmpGgwoh-vA",
                                      OwnerID = Person.Author.ID,
                                      NumericValue = 7
                                  };

                    ((DivisionRelationDefinitionTag)relationDefinitionToAdd).Dividend = dividend;
                    ((DivisionRelationDefinitionTag)relationDefinitionToAdd).Divisor = divisor;
                    break;
                }
                case "gsQu4sdxVEKGZsgCD_zfWQ": // Page 10
                {
                    relationDefinitionToAdd = new MultiplicationRelationDefinitionTag(newPage, Origin.Author)
                                              {
                                                  ID = "ZipMYNwixkq61bBN2_HD5g",
                                                  OwnerID = Person.Author.ID,
                                                  Product = 28,
                                                  RelationType = MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups
                                              };

                    var firstFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                      {
                                          ID = "WK6bKs_ByUCH8BOWgWxgUA",
                                          OwnerID = Person.Author.ID,
                                          NumericValue = 4
                                      };

                    var secondFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                       {
                                           ID = "2Ra04Eclg06aMGDGKtZ6fQ",
                                           OwnerID = Person.Author.ID,
                                           NumericValue = 7
                                       };

                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Clear();
                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Add(firstFactor);
                    ((MultiplicationRelationDefinitionTag)relationDefinitionToAdd).Factors.Add(secondFactor);
                    break;
                }
                case "MtZusuAFZEOqTr8KRlFlMA": // Page 11
                {
                    relationDefinitionToAdd = new DivisionRelationDefinitionTag(newPage, Origin.Author)
                                              {
                                                  ID = "J8Sflc0rWEyodHSiD6BOoQ",
                                                  OwnerID = Person.Author.ID,
                                                  Quotient = 6,
                                                  Remainder = 0,
                                                  RelationType = DivisionRelationDefinitionTag.RelationTypes.GeneralDivision
                                              };

                    var dividend = new NumericValueDefinitionTag(newPage, Origin.Author)
                                   {
                                       ID = "BL-RIJBn_06lpKH4yBlOzg",
                                       OwnerID = Person.Author.ID,
                                       NumericValue = 48
                                   };

                    var divisor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                  {
                                      ID = "Bk5lXzsvu0GHmpGgwoh-vA",
                                      OwnerID = Person.Author.ID,
                                      NumericValue = 8
                                  };

                    ((DivisionRelationDefinitionTag)relationDefinitionToAdd).Dividend = dividend;
                    ((DivisionRelationDefinitionTag)relationDefinitionToAdd).Divisor = divisor;
                    break;
                }
                case "QHJ7pFHY3ECr8u6bSFRCkA": // Page 12
                {
                    relationDefinitionToAdd = new AdditionRelationDefinitionTag(newPage, Origin.Author)
                                              {
                                                  ID = "TxyRU2oIuUek0hmqfV3wSQ",
                                                  OwnerID = Person.Author.ID,
                                                  Sum = 72,
                                                  RelationType = AdditionRelationDefinitionTag.RelationTypes.GeneralAddition
                                              };

                    var firstPart = new MultiplicationRelationDefinitionTag(newPage, Origin.Author)
                                    {
                                        ID = "jzGI6KOkTUCr1PEohXIAtQ",
                                        OwnerID = Person.Author.ID,
                                        Product = 32,
                                        RelationType = MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups
                                    };

                    var firstFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                      {
                                          ID = "pY-jWRet_UyIeMSD6rpfzw",
                                          OwnerID = Person.Author.ID,
                                          NumericValue = 4
                                      };

                    var secondFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                       {
                                           ID = "xppeMKzxQ06UVwgBg0sR8Q",
                                           OwnerID = Person.Author.ID,
                                           NumericValue = 8
                                       };

                    firstPart.Factors.Clear();
                    firstPart.Factors.Add(firstFactor);
                    firstPart.Factors.Add(secondFactor);

                    var secondPart = new MultiplicationRelationDefinitionTag(newPage, Origin.Author)
                                     {
                                         ID = "FoHyUvBjI0ONc8TF7vRmkw",
                                         OwnerID = Person.Author.ID,
                                         Product = 40,
                                         RelationType = MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups
                                     };

                    firstFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                  {
                                      ID = "usHRHdsaiEao5KY8oxv-9g",
                                      OwnerID = Person.Author.ID,
                                      NumericValue = 5
                                  };

                    secondFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                   {
                                       ID = "KMsWhQClX0KM0-vT_SvFOw",
                                       OwnerID = Person.Author.ID,
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
                    relationDefinitionToAdd = new AdditionRelationDefinitionTag(newPage, Origin.Author)
                                              {
                                                  ID = "qey_Bae27kmq42CLfvUg1Q",
                                                  OwnerID = Person.Author.ID,
                                                  Sum = 86,
                                                  RelationType = AdditionRelationDefinitionTag.RelationTypes.GeneralAddition
                                              };

                    var firstPart = new MultiplicationRelationDefinitionTag(newPage, Origin.Author)
                                    {
                                        ID = "grr6c_grIEWYK8dsuRqtvA",
                                        OwnerID = Person.Author.ID,
                                        Product = 32,
                                        RelationType = MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups
                                    };

                    var firstFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                      {
                                          ID = "ZNl4KqUKgkytz4c1m0ehYw",
                                          OwnerID = Person.Author.ID,
                                          NumericValue = 4
                                      };

                    var secondFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                       {
                                           ID = "9MPttARngE24S-vrhnnnHQ",
                                           OwnerID = Person.Author.ID,
                                           NumericValue = 8
                                       };

                    firstPart.Factors.Clear();
                    firstPart.Factors.Add(firstFactor);
                    firstPart.Factors.Add(secondFactor);

                    var secondPart = new MultiplicationRelationDefinitionTag(newPage, Origin.Author)
                                     {
                                         ID = "ayeqY8cbIEyJ8h7_4VM70Q",
                                         OwnerID = Person.Author.ID,
                                         Product = 54,
                                         RelationType = MultiplicationRelationDefinitionTag.RelationTypes.EqualGroups
                                     };

                    firstFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                  {
                                      ID = "BLEYzh9iekaPZAC-09sPyg",
                                      OwnerID = Person.Author.ID,
                                      NumericValue = 9
                                  };

                    secondFactor = new NumericValueDefinitionTag(newPage, Origin.Author)
                                   {
                                       ID = "sTEDno-Uk0uxt3TupBNiYA",
                                       OwnerID = Person.Author.ID,
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

            newPage.AddTag(relationDefinitionToAdd);
        }

        public static void AddLargeCacheTagsAndInterpretationRegions(CLPPage newPage)
        {
            #region Constraints

            var ericPageNumbers = new List<int> { 31, 44, 49, 51, 91, 103, 104, 105, 106, 107, 108, 109, 119, 120, 121, 122, 123, 134, 125, 126, 127, 128, 140, 141, 142, 143, 144, 145, 146, 147, 152, 153, 154, 155, 156, 157, 158, 159, 160, 164, 165, 166, 167, 168, 169, 170, 171, 172, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 227, 228, 229, 230, 231, 242, 243, 244, 245, 246, 249, 278, 280, 287, 289, 295, 296, 298, 308, 310, 321, 362, 363, 364, 365, 366, 367, 369, 376 };
            var andeePageNumbers = new List<int> { 12, 14, 15, 16, 17, 18, 19, 20, 66, 300, 302, 306, 312, 318, 332, 322, 323, 324, 325, 337, 338, 339, 342, 344, 354, 355, 356, 357, 377, 378 };
            // 1 and 2 should both look in LK2 .clp file as it contains everything from LK .clp file
            var lily1PageNumbers = new List<int> { 11, 43, 56, 59, 61, 62, 65, 86, 148, 149, 150, 151, 161, 162, 163, 173, 174, 175, 198, 199, 200, 213, 214, 215, 217, 218, 219, 221, 225, 226, 232, 233, 234, 236, 240, 241, 248, 251, 252, 253, 254, 255, 258, 259, 260, 261, 262, 263, 264, 285, 286, 288, 290, 291, 293, 297, 299, 301, 303, 304, 307, 309, 311, 313, 314, 315, 326, 327, 328, 329, 330, 331, 332, 333, 334, 335, 336, 341, 343, 345, 346, 347, 348, 349, 350, 351, 352, 353, 368, 379, 380, 381, 382, 383, 384, 385 };
            var lily2PageNumbers = new List<int> { 275, 276, 281, 10, 21, 22, 28, 29, 30, 32, 45, 50, 67, 68, 110, 111, 112, 118, 129, 130, 131, 201, 216, 220, 222, 223, 224, 235, 237, 238, 239, 256, 257, 266, 267, 283, 292, 305, 316, 317 };

            var noMetaDataTagsPageNumbers = new List<int> { 93, 94, 113, 114, 132, 133, 134, 135, 136, 137, 138, 176, 177, 178, 179, 180, 181, 182, 247, 358, 359, 360, 370, 371, 372, 373, 374, 375 };

            var neverAnalyzePageNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 13, 23, 24, 25, 26, 27, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 46, 47, 48, 52, 53, 54, 55, 56, 57, 60, 63, 64, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 87, 88, 89, 90, 92, 95, 96, 97, 98, 99, 100, 101, 102, 115, 116, 117, 139, 183, 184, 250, 274, 294, 319, 340, 361, 386 };

            #endregion // Constraints

            var pageNumber = newPage.PageNumber;
            var isUsingStitched = true;

            var authorTagCachePath = isUsingStitched ? AnnAuthorTagsStitched : string.Empty;
            if (lily1PageNumbers.Contains(pageNumber))
            {
                authorTagCachePath = AnnAuthorTagsLily1;
            }
            if (lily2PageNumbers.Contains(pageNumber))
            {
                authorTagCachePath = AnnAuthorTagsLily2;
            }
            else if (andeePageNumbers.Contains(pageNumber))
            {
                authorTagCachePath = AnnAuthorTagsAndee;
            }
            else if (ericPageNumbers.Contains(pageNumber))
            {
                authorTagCachePath = AnnAuthorTagsEric;
            }

            if (string.IsNullOrWhiteSpace(authorTagCachePath))
            {
                return;
            }

            #region Load Author Page

            var zipContainerFilePath = isUsingStitched ? AnnAuthorTagsStitched : authorTagCachePath;

            var pageZipEntryLoaders = new List<DataService.PageZipEntryLoader>();
            using (var zip = ZipFile.Read(zipContainerFilePath))
            {
                zip.CompressionMethod = CompressionMethod.None;
                zip.CompressionLevel = CompressionLevel.None;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                zip.CaseSensitiveRetrieval = true;

                var internalPagesDirectory = isUsingStitched ? "notebooks/Math Notebook;pUVQ-qBPyUWCuHWMs9dryA/A;AUTHOR;AUTHOR0000000000000000/pages/" : "notebooks/A;AUTHOR;AUTHOR0000000000000000/pages/";
                var allPageEntries = zip.GetEntriesInDirectory(internalPagesDirectory).ToList();
                var pageEntries = (from pageEntry in allPageEntries
                                   let nameComposite = CLPPage.NameComposite.ParseFromString(pageEntry.GetEntryNameWithoutExtension())
                                   where nameComposite.ID == newPage.ID
                                   select pageEntry).ToList();

                pageZipEntryLoaders = DataService.GetPageZipEntryLoadersFromEntries(pageEntries);
            }

            var authorPage = DataService.GetPagesFromPageZipEntryLoaders(pageZipEntryLoaders, zipContainerFilePath).FirstOrDefault();

            #endregion // Load Author Page

            foreach (var authorPageTag in authorPage.Tags)
            {
                RecursiveParentPageTagSet(authorPageTag, newPage);

                newPage.AddTag(authorPageTag);
            }

            foreach (var interpretationRegion in authorPage.PageObjects.OfType<InterpretationRegion>())
            {
                newPage.PageObjects.Add(interpretationRegion);
            }
        }

        public static void RecursiveParentPageTagSet(ITag tag, CLPPage page)
        {
            if (tag == null)
            {
                return;
            }

            tag.ParentPage = page;

            if (tag is NumericValueDefinitionTag)
            {
                return;
            }

            var divisionTag = tag as DivisionRelationDefinitionTag;
            if (divisionTag != null)
            {
                var dividend = divisionTag.Dividend as ITag;
                RecursiveParentPageTagSet(dividend, page);

                var divisor = divisionTag.Divisor as ITag;
                RecursiveParentPageTagSet(divisor, page);
            }

            var additionTag = tag as AdditionRelationDefinitionTag;
            if (additionTag != null)
            {
                foreach (var addend in additionTag.Addends)
                {
                    var addendTag = addend as ITag;
                    RecursiveParentPageTagSet(addendTag, page);
                }
            }

            var multiplicationTag = tag as MultiplicationRelationDefinitionTag;
            if (multiplicationTag != null)
            {
                foreach (var factor in multiplicationTag.Factors)
                {
                    var factorTag = factor as ITag;
                    RecursiveParentPageTagSet(factorTag, page);
                }
            }

            var equivalenceTag = tag as EquivalenceRelationDefinitionTag;
            if (equivalenceTag != null)
            {
                var leftRelation = equivalenceTag.LeftRelationPart as ITag;
                RecursiveParentPageTagSet(leftRelation, page);

                var rightRelation = equivalenceTag.RightRelationPart as ITag;
                RecursiveParentPageTagSet(rightRelation, page);
            }
        }

        #endregion // Tags

        #endregion // Ann Conversions
    }
}
