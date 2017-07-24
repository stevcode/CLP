using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Catel;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public static class AnalysisService
    {
        #region Constants

        private const string ANALYSIS_TRACKER_FILE_NAME = "Analysis Tracker.json";
        private const string ROLLING_ANALYSIS_FILE_NAME = "Batch Analysis.tsv";
        private const string ANN_CACHE_FILE_NAME = "Ann - Fall 2014.clp";
        private const string SUBJECT_FILE_NAME = "subject;L6xDfDuP-kCMBjQ3-HdAPQ.xml";

        #endregion // Constants

        #region Members

        private static string AnalysisFolder => Path.Combine(DataService.DesktopFolderPath, "Rolling Analysis");
        private static string ConvertedPagesFolder => Path.Combine(AnalysisFolder, "Converted Pages");
        
        private static string AnalysisTrackerFilePath => Path.Combine(AnalysisFolder, ANALYSIS_TRACKER_FILE_NAME);
        private static string RollingAnalysisFilePath => Path.Combine(AnalysisFolder, ROLLING_ANALYSIS_FILE_NAME);
        private static string AnnFullZipFilePath => Path.Combine(AnalysisFolder, ANN_CACHE_FILE_NAME);

        private static readonly string NotebooksFolderPath = ConversionService.AnnNotebooksFolder;
        private static readonly string ClassesFolderPath = ConversionService.AnnClassesFolder;
        private static readonly string ImagesFolderPath = ConversionService.AnnImageFolder;

        private static readonly Dictionary<string,string> StudentIDToNotebookPagesFolderPath = new Dictionary<string,string>();

        #region Page Number Lists

        public static List<int> MainPageNumbersToAnalyze = new List<int>
                                                             {
                                                                 11,
                                                                 12,
                                                                 14,
                                                                 16,
                                                                 18,
                                                                 20,
                                                                 31,
                                                                 43,
                                                                 44,
                                                                 49,
                                                                 51,
                                                                 65,
                                                                 66,
                                                                 86,
                                                                 91,
                                                                 103,
                                                                 104,
                                                                 105,
                                                                 106,
                                                                 107,
                                                                 108,
                                                                 109,
                                                                 119,
                                                                 120,
                                                                 121,
                                                                 122,
                                                                 123,
                                                                 124,
                                                                 125,
                                                                 126,
                                                                 127,
                                                                 128,
                                                                 140,
                                                                 141,
                                                                 142,
                                                                 143,
                                                                 144,
                                                                 145,
                                                                 146,
                                                                 147,
                                                                 148,
                                                                 149,
                                                                 150,
                                                                 151,
                                                                 152,
                                                                 153,
                                                                 154,
                                                                 155,
                                                                 156,
                                                                 157,
                                                                 158,
                                                                 159,
                                                                 160,
                                                                 161,
                                                                 162,
                                                                 163,
                                                                 164,
                                                                 165,
                                                                 166,
                                                                 167,
                                                                 168,
                                                                 169,
                                                                 170,
                                                                 171,
                                                                 172,
                                                                 173,
                                                                 174,
                                                                 175,
                                                                 185,
                                                                 186,
                                                                 187,
                                                                 188,
                                                                 189,
                                                                 190,
                                                                 191,
                                                                 192,
                                                                 193,
                                                                 194,
                                                                 195,
                                                                 196,
                                                                 197,
                                                                 198,
                                                                 199,
                                                                 200,
                                                                 202,
                                                                 203,
                                                                 204,
                                                                 205,
                                                                 206,
                                                                 207,
                                                                 208,
                                                                 209,
                                                                 210,
                                                                 211,
                                                                 212,
                                                                 213,
                                                                 214,
                                                                 215,
                                                              //   217,  Removed, something wrong with page definition. No hidden value?
                                                                 218,
                                                                 219,
                                                              //   221,   same
                                                                 225,
                                                                 226,
                                                                 227,
                                                                 228,
                                                                 229,
                                                                 230,
                                                                 231,
                                                                 232,
                                                                 233,
                                                                 234,
                                                              //   236,  same
                                                                 240,
                                                                 241,
                                                                 242,
                                                                 243,
                                                                 244,
                                                                 245,
                                                                 246,
                                                                 248,
                                                                 249,
                                                                 251,
                                                                 252,
                                                                 253,
                                                                 254,
                                                                 255,
                                                                 258,
                                                                 259,
                                                                 260,
                                                                 261,
                                                                 262,
                                                                 263,
                                                                 264,
                                                                 265,
                                                                 268,
                                                                 269,
                                                                 270,
                                                                 271,
                                                                 272,
                                                                 273,
                                                                 275,
                                                                 276,
                                                                 277,
                                                                 278,
                                                                 279,
                                                                 280,
                                                                 281,
                                                                 282,
                                                                 284,
                                                                 285,
                                                                 286,
                                                                 287,
                                                                 288,
                                                                 289,
                                                                 290,
                                                                 291,
                                                                 293,
                                                                 295,
                                                                 296,
                                                                 297,
                                                                 298,
                                                                 299,
                                                                 300,
                                                                 301,
                                                                 302,
                                                                 303,
                                                                 304,
                                                                 306,
                                                                 307,
                                                                 308,
                                                                 309,
                                                                 310,
                                                                 311,
                                                                 312,
                                                                 313,
                                                                 314,
                                                                 315,
                                                                 318,
                                                                 320,
                                                                 321,
                                                                 322,
                                                                 323,
                                                                 324,
                                                                 325,
                                                                 326,
                                                                 337,
                                                                 338,
                                                                 339,
                                                                 341,
                                                                 342,
                                                                 343,
                                                                 344,
                                                                 354,
                                                                 355,
                                                                 356,
                                                                 357,
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
                                                                 378,
                                                                 379,
                                                                 380,
                                                                 381,
                                                                 382,
                                                                 383,
                                                                 384,
                                                                 385
                                                             };

        public static List<int> LastPageNumbersToAnalyze = new List<int>
                                                           {
                                                               217,
                                                               221,
                                                               236
                                                           };

        public static List<int> FixedPageNumbersToAnalyze = new List<int>
                                                           {
                                                               278,
                                                               280,
                                                               287,
                                                               289,
                                                               296,
                                                               298,
                                                               308,
                                                               310,
                                                               320
                                                           };

        public static List<int> OtherPageNumbersToAnalyze = new List<int>
                                                              {
                                                                  10,
                                                                  15,
                                                                  17,
                                                                  19,
                                                                  21,
                                                                  22,
                                                                  28,
                                                                  29,
                                                                  30,
                                                                  32,
                                                                  45,
                                                                  50,
                                                                  67,
                                                                  68,
                                                                  93,
                                                                  94,
                                                                  110,
                                                                  111,
                                                                  112,
                                                                  113,
                                                                  114,
                                                                  118,
                                                                  129,
                                                                  130,
                                                                  131,
                                                                  132,
                                                                  133,
                                                                  134,
                                                                  135,
                                                                  136,
                                                                  137,
                                                                  138,
                                                                  176,
                                                                  177,
                                                                  178,
                                                                  179,
                                                                  180,
                                                                  181,
                                                                  182,
                                                                  201,
                                                                  216,
                                                                  220,
                                                                  222,
                                                                  223,
                                                                  224,
                                                                  235,
                                                                  237,
                                                                  238,
                                                                  239,
                                                                  247,
                                                                  256,
                                                                  257,
                                                                  266,
                                                                  267,
                                                                  283,
                                                                  292,
                                                                  305,
                                                                  316,
                                                                  317,
                                                                  358,
                                                                  359,
                                                                  360,
                                                                  370,
                                                                  371,
                                                                  372,
                                                                  373,
                                                                  374,
                                                                  375,
                                                                  58,
                                                                  59,
                                                                  61,
                                                                  62,
                                                                  327,
                                                                  328,
                                                                  329,
                                                                  330,
                                                                  331,
                                                                  332,
                                                                  333,
                                                                  334,
                                                                  335,
                                                                  336,
                                                                  345,
                                                                  346,
                                                                  347,
                                                                  348,
                                                                  349,
                                                                  350,
                                                                  351,
                                                                  352,
                                                                  353
                                                              };

        public static List<int> AllPageNumbersToAnalyze = MainPageNumbersToAnalyze.Concat(OtherPageNumbersToAnalyze).Concat(LastPageNumbersToAnalyze).Distinct().ToList();

        #endregion // Page Number Lists

        #endregion // Members

        public static void RunAnalysisOnLoadedNotebook(Notebook notebook)
        {
            var analysisRows = new List<string>();
            foreach (var page in notebook.Pages)
            {
                //if (page.PageNumber != 276)
                //{
                //    continue;
                //}

                HistoryAnalysis.GenerateSemanticEvents(page);
                var analysisEntry = GenerateAnalysisEntryForPage(page);
                var analysisRow = analysisEntry.BuildEntryLine();
                analysisRows.Add(analysisRow);
            }

            var desktopDirectory = DataService.DesktopFolderPath;
            var filePath = Path.Combine(desktopDirectory, "BatchAnalysis.tsv");
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "");

                var headerRow = AnalysisEntry.BuildHeaderEntryLine();
                File.AppendAllText(filePath, headerRow);
            }

            foreach (var analysisRow in analysisRows)
            {
                File.AppendAllText(filePath, Environment.NewLine + analysisRow);
            }
        }

        public static void RunFullBatchAnalysis(List<int> pageNumbersToAnalyze)
        {
            // *****Important Note: Ensure IS_LARGE_CACHE is set to true in ConversionService*****

            CLogger.ForceNewLogFile();

            CLogger.AppendToLog("Beginning Rolling Batch Analysis of cache.");

            if (!Directory.Exists(ConvertedPagesFolder))
            {
                Directory.CreateDirectory(ConvertedPagesFolder);
            }

            InitializeRollingAnalysisFile();

            var analysisTracker = InitializeAnalysisTrackerFile();
            CLogger.AppendToLog("Analysis Tracker loaded.");

            InitializePagesFolderPaths();

            var pageNumbersLeftToAnalyze = pageNumbersToAnalyze.Except(analysisTracker.CompletedPageNumbers).ToList();
            var allStudentIDs = analysisTracker.StudentNotebooks.Select(n => n.Owner.ID).ToList();
            var backupCount = 0;

            foreach (var pageNumber in pageNumbersLeftToAnalyze)
            {
                CLogger.AppendToLog($"Beginning loop through analysis of page {pageNumber}.");
                if (!analysisTracker.InProgressPages.Any(pp => pp.PageNumber == pageNumber))
                {
                    var newPageProgress = new AnalysisTracker.PageProgress
                                          {
                                              PageNumber = pageNumber
                                          };
                    analysisTracker.InProgressPages.Add(newPageProgress);
                }

                var pageProgress = analysisTracker.InProgressPages.First(pp => pp.PageNumber == pageNumber);

                foreach (var studentNotebook in analysisTracker.StudentNotebooks)
                {
                    var studentID = studentNotebook.Owner.ID;

                    if (pageProgress.StudentIDs.Contains(studentID))
                    {
                        continue;
                    }

                    var studentName = studentNotebook.Owner.FullName;
                    var stopWatch = new Stopwatch();

                    #region Conversion

                    CLogger.AppendToLog($"Beginning conversion of {studentName}'s page {pageNumber}.");

                    stopWatch.Start();

                    var pagesFolderPath = StudentIDToNotebookPagesFolderPath[studentID];
                    var pageFilePath = ConversionService.GetPageFilePathFromPageNumber(pagesFolderPath, pageNumber);
                    if (string.IsNullOrWhiteSpace(pageFilePath))
                    {
                        pageProgress.StudentIDs.Add(studentID);
                        CLogger.AppendToLog($"[INFO] Page Path File did not exist for {studentName}'s page {pageNumber}.");
                        continue;
                    }
                    var page = ConversionService.ConvertCacheAnnPageFile(pageFilePath);
                    if (page == null)
                    {
                        pageProgress.StudentIDs.Add(studentID);
                        CLogger.AppendToLog($"[ERROR] NULL Page after conversion for {studentName}'s page {pageNumber}.");
                        continue;
                    }

                    stopWatch.Stop();

                    var conversionTimeInMilliseconds = stopWatch.ElapsedMilliseconds;

                    CLogger.AppendToLog($"Finished conversion of {studentName}'s page {pageNumber}.");

                    #endregion // Conversion

                    #region Analysis

                    CLogger.AppendToLog($"Beginning analysis of {studentName}'s page {pageNumber}.");

                    stopWatch.Reset();
                    stopWatch.Start();

                    HistoryAnalysis.GenerateSemanticEvents(page);

                    stopWatch.Stop();

                    var analysisTimeInMilliseconds = stopWatch.ElapsedMilliseconds;

                    CLogger.AppendToLog($"Finished analysis of {studentName}'s page {pageNumber}.");

                    #endregion // Analysis

                    #region Compiling Statistics

                    CLogger.AppendToLog($"Beginning compiling statistics for {studentName}'s page {pageNumber}.");

                    stopWatch.Reset();
                    stopWatch.Start();

                    var analysisEntry = GenerateAnalysisEntryForPage(page);
                    var analysisRow = analysisEntry.BuildEntryLine();

                    stopWatch.Stop();

                    var statisticsComplingTimeInMilliseconds = stopWatch.ElapsedMilliseconds;

                    CLogger.AppendToLog($"Finished compiling statistics for {studentName}'s page {pageNumber}.");

                    #endregion // Compiling Statistics

                    #region Setting Up Paths

                    var convertedStudentPagesFolderName = $"{studentName};{studentID}";
                    var convertedStudentPagesFolderPath = Path.Combine(ConvertedPagesFolder, convertedStudentPagesFolderName);

                    var pageJsonFileName = $"{page.DefaultZipEntryName}.json";
                    var pageJsonFilePath = Path.Combine(convertedStudentPagesFolderPath, pageJsonFileName);

                    #endregion // Setting Up Paths

                    #region Updating Analysis Tracker

                    pageProgress.StudentIDs.Add(studentID);
                    var remainingStudentIDs = allStudentIDs.Except(pageProgress.StudentIDs);
                    if (!remainingStudentIDs.Any())
                    {
                        analysisTracker.CompletedPageNumbers.Add(pageNumber);
                        analysisTracker.InProgressPages.Remove(pageProgress);
                    }

                    var totalPageConversionAndAnalysisEntryGenerationTimeInMilliseconds = conversionTimeInMilliseconds + analysisTimeInMilliseconds + statisticsComplingTimeInMilliseconds;
                    analysisTracker.FullPageConversionAndAnalysisEntryGenerationTimeInMilliseconds += totalPageConversionAndAnalysisEntryGenerationTimeInMilliseconds;
                    analysisTracker.PageAnalysisTimeInMilliseconds += analysisTimeInMilliseconds;
                    analysisTracker.TotalPagesAnalyzed++;
                    analysisTracker.TotalHistoryActionsAnalyzed += page.History.CompleteOrderedHistoryActions.Count;

                    var pageNumbersStillLeftToAnalyzeCount = pageNumbersToAnalyze.Except(analysisTracker.CompletedPageNumbers).Count();
                    var totalPagesStillLeftToAnalyzeCount = pageNumbersStillLeftToAnalyzeCount * allStudentIDs.Count;
                    if (remainingStudentIDs.Any())
                    {
                        totalPagesStillLeftToAnalyzeCount -= (allStudentIDs.Count - remainingStudentIDs.Count());
                    }
                    var timeRemainingInMilliseconds = totalPagesStillLeftToAnalyzeCount * analysisTracker.AverageFullPageConversionAndAnalysisEntryGenerationTimeInMilliseconds;
                    var timeSpan = new TimeSpan(0, 0, 0, 0, (int)timeRemainingInMilliseconds.ToInt());
                    var formattedTimeRemaining = timeSpan.ToString(@"hh\:mm\:ss");
                    analysisTracker.AnalysisTimeRemaining = formattedTimeRemaining;

                    analysisTracker.SaveTime = FastDateTime.Now;

                    #endregion // Updating Analysis Tracker

                    #region Saving Files

                    CLogger.AppendToLog($"Saving files for {studentName}'s page {pageNumber}.");

                    page.ToJsonFile(pageJsonFilePath);

                    var analysisTrackerBackupFilePath = $"{AnalysisTrackerFilePath}.bak{backupCount}";
                    if (File.Exists(AnalysisTrackerFilePath) &&
                        File.Exists(analysisTrackerBackupFilePath))
                    {
                        File.Delete(analysisTrackerBackupFilePath);
                    }
                    File.Move(AnalysisTrackerFilePath, analysisTrackerBackupFilePath);
                    analysisTracker.ToJsonFile(AnalysisTrackerFilePath);

                    var rollingAnalysisBackupFilePath = $"{RollingAnalysisFilePath}.bak{backupCount}";
                    if (File.Exists(RollingAnalysisFilePath) &&
                        File.Exists(rollingAnalysisBackupFilePath))
                    {
                        File.Delete(rollingAnalysisBackupFilePath);
                    }
                    File.Copy(RollingAnalysisFilePath, rollingAnalysisBackupFilePath);
                    File.AppendAllText(RollingAnalysisFilePath, Environment.NewLine + analysisRow);

                    backupCount++;
                    if (backupCount > 44)
                    {
                        backupCount = 0;
                    }

                    CLogger.AppendToLog($"Finished saving files for {studentName}'s page {pageNumber}.");

                    #endregion // Saving Files

                    CLogger.AppendToLog($"Time Remaining: {formattedTimeRemaining}");
                }

                var isModified = false;
                if (!analysisTracker.CompletedPageNumbers.Contains(pageNumber))
                {
                    analysisTracker.CompletedPageNumbers.Add(pageNumber);
                    isModified = true;
                }

                if (analysisTracker.InProgressPages.Any(pp => pp.PageNumber == pageNumber))
                {
                    analysisTracker.InProgressPages.Remove(pageProgress);
                    isModified = true;
                }

                if (isModified)
                {
                    var analysisTrackerBackupFilePath = $"{AnalysisTrackerFilePath}.bak{backupCount}";
                    if (File.Exists(AnalysisTrackerFilePath) &&
                        File.Exists(analysisTrackerBackupFilePath))
                    {
                        File.Delete(analysisTrackerBackupFilePath);
                    }
                    File.Move(AnalysisTrackerFilePath, analysisTrackerBackupFilePath);

                    analysisTracker.SaveTime = FastDateTime.Now;
                    analysisTracker.ToJsonFile(AnalysisTrackerFilePath);

                    backupCount++;
                    if (backupCount > 44)
                    {
                        backupCount = 0;
                    }
                }
            }

            CLogger.AppendToLog("Ending Rolling Batch Analysis of cache.");
        }

        public static void RunFullBatchAnalysisOnAlreadyConvertedPages()
        {
            CLogger.ForceNewLogFile();

            CLogger.AppendToLog("Beginning Rolling Batch Analysis of cache.");

            if (!Directory.Exists(AnalysisFolder))
            {
                Directory.CreateDirectory(AnalysisFolder);
            }

            InitializeRollingAnalysisFile();

            var combineFolderPath = Path.Combine(DataService.DesktopFolderPath, "Combine");
            var directoryInfo = new DirectoryInfo(combineFolderPath);
            var pageFiles = directoryInfo.GetFiles("*.json", SearchOption.AllDirectories);

            var totalPagesLoaded = 0;
            var totalPageLoadTimeInMilliseconds = 0.0;
            long totalFileSizeInBytes = 0;

            var backupCount = 0;

            foreach (var pageFileInfo in pageFiles)
            {
                var pagesRemaining = pageFiles.Length - totalPagesLoaded;
                CLogger.AppendToLog($"Pages Remaining: {pagesRemaining}");
                var filePath = pageFileInfo.FullName;
                CLogger.AppendToLog($"Loading Page: {filePath}");
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var page = ASerializableBase.FromJsonFile<CLPPage>(filePath);
                stopWatch.Stop();
                CLogger.AppendToLog($"Page Loaded: {filePath}");

                CLogger.AppendToLog($"Analyzing Page: {filePath}");
                HistoryAnalysis.GenerateSemanticEvents(page);
                CLogger.AppendToLog($"Page Analyzed: {filePath}");

                var analysisEntry = GenerateAnalysisEntryForPage(page);
                var analysisRow = analysisEntry.BuildEntryLine();

                var rollingAnalysisBackupFilePath = $"{RollingAnalysisFilePath}.bak{backupCount}";
                if (File.Exists(RollingAnalysisFilePath) &&
                    File.Exists(rollingAnalysisBackupFilePath))
                {
                    File.Delete(rollingAnalysisBackupFilePath);
                }
                File.Copy(RollingAnalysisFilePath, rollingAnalysisBackupFilePath);
                File.AppendAllText(RollingAnalysisFilePath, Environment.NewLine + analysisRow);

                backupCount++;
                if (backupCount > 44)
                {
                    backupCount = 0;
                }

                #region Stats

                var pageLoadTimeInMilliseconds = stopWatch.ElapsedMilliseconds;
                var fileSizeInBytes = pageFileInfo.Length;

                totalPagesLoaded++;
                totalPageLoadTimeInMilliseconds += pageLoadTimeInMilliseconds;
                totalFileSizeInBytes += fileSizeInBytes;

                #endregion // Stats
            }

            var averageTimeToLoadPageInMilliseconds = totalPageLoadTimeInMilliseconds / totalPagesLoaded;
            CLogger.AppendToLog($"Average Time to Load Page: {averageTimeToLoadPageInMilliseconds} ms");
            var averageTimeToLoadByteInMilliseconds = totalPageLoadTimeInMilliseconds / totalFileSizeInBytes;
            CLogger.AppendToLog($"Average Time to Load 1 byte: {averageTimeToLoadByteInMilliseconds} ms");
            CLogger.AppendToLog("*****DONE*****");
        }

        private static AnalysisTracker InitializeAnalysisTrackerFile()
        {
            if (File.Exists(AnalysisTrackerFilePath))
            {
                CLogger.AppendToLog("Analysis Tracker already exists, loading...");
                return ASerializableBase.FromJsonFile<AnalysisTracker>(AnalysisTrackerFilePath);
            }

            CLogger.AppendToLog("Initializing new Analysis Tracker...");

            var analysisTracker = new AnalysisTracker();

            var dirInfo = new DirectoryInfo(NotebooksFolderPath);
            foreach (var directory in dirInfo.EnumerateDirectories())
            {
                var folderName = directory.Name;
                var folderNameParts = folderName.Split(';');
                if (folderNameParts.Length != 5)
                {
                    continue;
                }

                var ownerType = folderNameParts[4];
                if (ownerType != "S")
                {
                    continue;
                }

                var notebookFolderPath = directory.FullName;
                var studentNotebook = ConversionService.ConvertCacheAnnNotebookFile(notebookFolderPath);
                if (studentNotebook == null)
                {
                    CLogger.AppendToLog($"[ERROR] Failed to convert student notebook file in the following notebook folder path: {notebookFolderPath}");
                    continue;
                }

                analysisTracker.StudentNotebooks.Add(studentNotebook);

                var studentName = studentNotebook.Owner.FullName;
                var studentID = studentNotebook.Owner.ID;
                var convertedStudentPagesFolderName = $"{studentName};{studentID}";
                var convertedStudentPagesFolderPath = Path.Combine(ConvertedPagesFolder, convertedStudentPagesFolderName);
                if (!Directory.Exists(convertedStudentPagesFolderPath))
                {
                    Directory.CreateDirectory(convertedStudentPagesFolderPath);
                }
            }

            analysisTracker.SaveTime = FastDateTime.Now;
            analysisTracker.ToJsonFile(AnalysisTrackerFilePath);
            return analysisTracker;
        }

        private static void InitializePagesFolderPaths()
        {
            var dirInfo = new DirectoryInfo(NotebooksFolderPath);
            foreach (var directory in dirInfo.EnumerateDirectories())
            {
                var folderName = directory.Name;
                var folderNameParts = folderName.Split(';');
                if (folderNameParts.Length != 5)
                {
                    continue;
                }

                var ownerType = folderNameParts[4];
                if (ownerType != "S")
                {
                    continue;
                }

                var notebookFolderPath = directory.FullName;
                var pagesFolderPath = Path.Combine(notebookFolderPath, "Pages");

                var studentID = folderNameParts[3];
                StudentIDToNotebookPagesFolderPath.Add(studentID, pagesFolderPath);
            }
        }

        private static void InitializeRollingAnalysisFile()
        {
            if (File.Exists(RollingAnalysisFilePath))
            {
                return;
            }

            File.WriteAllText(RollingAnalysisFilePath, "");

            var headerRow = AnalysisEntry.BuildHeaderEntryLine();
            File.AppendAllText(RollingAnalysisFilePath, headerRow);
        }

        public static AnalysisEntry GenerateAnalysisEntryForPage(CLPPage page)
        {
            #region Page Identification

            var entry = new AnalysisEntry(page.Owner.FullName, page.PageNumber);
            entry.SubmissionTime = page.SubmissionTime == null ? AnalysisEntry.UNSUBMITTED : $"{page.SubmissionTime:yyyy-MM-dd HH:mm:ss}";

            #endregion // Page Identification

            #region Set up variables

            var pass3Event = page.History.SemanticEvents.FirstOrDefault(h => h.CodedObject == "PASS" && h.CodedObjectID == "3");
            var pass3Index = page.History.SemanticEvents.IndexOf(pass3Event);
            var pass3 = page.History.SemanticEvents.Skip(pass3Index + 1).ToList();

            #endregion // Set up variables

            #region Problem Characteristics

            var pageDefinition = page.Tags.FirstOrDefault(t => t.Category == Category.Definition);
            if (pageDefinition == null)
            {
                entry.ProblemType = AnalysisEntry.NONE;
                entry.LeftSideOperation = AnalysisEntry.NA;
                entry.RightSideOperation = AnalysisEntry.NA;
                entry.DivisionType = AnalysisEntry.NA;
            }
            else if (pageDefinition is AdditionRelationDefinitionTag)
            {
                entry.ProblemType = AnalysisEntry.PROBLEM_TYPE_2_PART;
                var additionDefinition = pageDefinition as AdditionRelationDefinitionTag;
                var leftSide = additionDefinition.Addends[0];
                var rightSide = additionDefinition.Addends[1];

                if (leftSide is DivisionRelationDefinitionTag)
                {
                    entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_NONE;
                }
                else if (leftSide is MultiplicationRelationDefinitionTag)
                {
                    entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_NONE;
                }
                else
                {
                    entry.LeftSideOperation = AnalysisEntry.NA;
                }

                if (rightSide is DivisionRelationDefinitionTag)
                {
                    entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_NONE;
                }
                else if (rightSide is MultiplicationRelationDefinitionTag)
                {
                    entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_NONE;
                }
                else
                {
                    entry.RightSideOperation = AnalysisEntry.NA;
                }

                entry.DivisionType = AnalysisEntry.NA;
            }
            else if (pageDefinition is EquivalenceRelationDefinitionTag)
            {
                entry.ProblemType = AnalysisEntry.PROBLEM_TYPE_EQUIVALENCE;
                var equivalenceDefinition = pageDefinition as EquivalenceRelationDefinitionTag;
                if (equivalenceDefinition.LeftRelationPart is DivisionRelationDefinitionTag)
                {
                    var divisionDefinition = equivalenceDefinition.LeftRelationPart as DivisionRelationDefinitionTag;
                    var dividend = divisionDefinition.Dividend as NumericValueDefinitionTag;
                    var divisor = divisionDefinition.Divisor as NumericValueDefinitionTag;
                    if (dividend.IsNotGiven)
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_DIVIDEND;
                    }
                    else if (divisor.IsNotGiven)
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_DIVISOR;
                    }
                    else
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_NONE;
                    }
                }
                else if (equivalenceDefinition.LeftRelationPart is MultiplicationRelationDefinitionTag)
                {
                    var multiplicationDefinition = equivalenceDefinition.LeftRelationPart as MultiplicationRelationDefinitionTag;
                    var firstFactor = multiplicationDefinition.Factors[0] as NumericValueDefinitionTag;
                    var secondFactor = multiplicationDefinition.Factors[1] as NumericValueDefinitionTag;
                    if (firstFactor.IsNotGiven)
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_FIRST_FACTOR;
                    }
                    else if (secondFactor.IsNotGiven)
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_LAST_FACTOR;
                    }
                    else
                    {
                        entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_NONE;
                    }
                }
                else
                {
                    entry.LeftSideOperation = AnalysisEntry.NA;
                }

                if (equivalenceDefinition.RightRelationPart is DivisionRelationDefinitionTag)
                {
                    var divisionDefinition = equivalenceDefinition.RightRelationPart as DivisionRelationDefinitionTag;
                    var dividend = divisionDefinition.Dividend as NumericValueDefinitionTag;
                    var divisor = divisionDefinition.Divisor as NumericValueDefinitionTag;
                    if (dividend.IsNotGiven)
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_DIVIDEND;
                    }
                    else if (divisor.IsNotGiven)
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_DIVISOR;
                    }
                    else
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_NONE;
                    }
                }
                else if (equivalenceDefinition.RightRelationPart is MultiplicationRelationDefinitionTag)
                {
                    var multiplicationDefinition = equivalenceDefinition.RightRelationPart as MultiplicationRelationDefinitionTag;
                    var firstFactor = multiplicationDefinition.Factors[0] as NumericValueDefinitionTag;
                    var secondFactor = multiplicationDefinition.Factors[1] as NumericValueDefinitionTag;
                    if (firstFactor.IsNotGiven)
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_FIRST_FACTOR;
                    }
                    else if (secondFactor.IsNotGiven)
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_LAST_FACTOR;
                    }
                    else
                    {
                        entry.RightSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_NONE;
                    }
                }
                else
                {
                    entry.RightSideOperation = AnalysisEntry.NA;
                }

                entry.DivisionType = AnalysisEntry.NA;
            }
            else if (pageDefinition is MultiplicationRelationDefinitionTag)
            {
                entry.ProblemType = AnalysisEntry.PROBLEM_TYPE_1_PART;
                entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_MULTIPLICATION_MISSING_NONE;
                entry.RightSideOperation = AnalysisEntry.NA;
                entry.DivisionType = AnalysisEntry.NA;
            }
            else if (pageDefinition is DivisionRelationDefinitionTag)
            {
                entry.ProblemType = AnalysisEntry.PROBLEM_TYPE_1_PART;
                entry.LeftSideOperation = AnalysisEntry.OPERATION_TYPE_DIVISION_MISSING_NONE;
                entry.RightSideOperation = AnalysisEntry.NA;

                var divisionDefinition = pageDefinition as DivisionRelationDefinitionTag;
                if (divisionDefinition.RelationType == DivisionRelationDefinitionTag.RelationTypes.Partitive)
                {
                    entry.DivisionType = AnalysisEntry.DIVISION_TYPE_PARTATIVE;
                }
                else if (divisionDefinition.RelationType == DivisionRelationDefinitionTag.RelationTypes.Quotative)
                {
                    entry.DivisionType = AnalysisEntry.DIVISION_TYPE_QUOTATIVE;
                }
                else
                {
                    entry.DivisionType = AnalysisEntry.DIVISION_TYPE_GENERAL;
                }
            }
            else
            {
                entry.ProblemType = AnalysisEntry.NONE;
                entry.LeftSideOperation = AnalysisEntry.NA;
                entry.RightSideOperation = AnalysisEntry.NA;
                entry.DivisionType = AnalysisEntry.NA;
            }


            var metaDataViewModel = new MetaDataTagsViewModel(page);
            entry.WordType = metaDataViewModel.IsWordProblem ? AnalysisEntry.WORD_TYPE_WORD : AnalysisEntry.WORD_TYPE_NON_WORD;

            entry.IsMultipleChoiceBoxOnPage = page.PageObjects.OfType<MultipleChoice>().Any() ? AnalysisEntry.YES : AnalysisEntry.NO;

            switch (metaDataViewModel.DifficultyLevel)
            {
                case MetaDataTagsViewModel.DifficultyLevels.None:
                    entry.DifficultyLevel = AnalysisEntry.DIFFICULTY_LEVEL_NONE;
                    break;
                case MetaDataTagsViewModel.DifficultyLevels.Easy:
                    entry.DifficultyLevel = AnalysisEntry.DIFFICULTY_LEVEL_EASY;
                    break;
                case MetaDataTagsViewModel.DifficultyLevels.Medium:
                    entry.DifficultyLevel = AnalysisEntry.DIFFICULTY_LEVEL_MEDIUM;
                    break;
                case MetaDataTagsViewModel.DifficultyLevels.Hard:
                    entry.DifficultyLevel = AnalysisEntry.DIFFICULTY_LEVEL_HARD;
                    break;
                default:
                    entry.DifficultyLevel = AnalysisEntry.DIFFICULTY_LEVEL_NONE;
                    break;
            }

            if (pageDefinition == null)
            {
                entry.PageDefinitionEquation = AnalysisEntry.NONE;
            }
            else
            {
                var equivalenceDefinition = pageDefinition as EquivalenceRelationDefinitionTag;
                if (equivalenceDefinition != null)
                {
                    entry.PageDefinitionEquation = $"{equivalenceDefinition.LeftRelationPart.ExpandedFormattedRelation} = {equivalenceDefinition.RightRelationPart.ExpandedFormattedRelation}";
                }

                var relationPartDefinition = pageDefinition as IRelationPart;
                if (relationPartDefinition != null)
                {
                    entry.PageDefinitionEquation = relationPartDefinition.ExpandedFormattedRelation;
                    var additionDefinition = pageDefinition as AdditionRelationDefinitionTag;
                    if (additionDefinition != null &&
                        !string.IsNullOrWhiteSpace(additionDefinition.AlternateFormattedRelation))
                    {
                        entry.PageDefinitionEquation += $" AND {additionDefinition.AlternateFormattedRelation}";
                    }
                }
            }

            if (metaDataViewModel.IsArrayRequired)
            {
                entry.RequiredRepresentations = "ARR";
            }
            else if (metaDataViewModel.IsNumberLineRequired)
            {
                entry.RequiredRepresentations = "NL";
            }
            else if (metaDataViewModel.IsStampRequired)
            {
                entry.RequiredRepresentations = "ST";
            }
            else if (metaDataViewModel.IsArrayOrNumberLineRequired)
            {
                entry.RequiredRepresentations = "ARR or NL";
            }
            else if (metaDataViewModel.IsArrayAndStampRequired)
            {
                entry.RequiredRepresentations = "ARR&ST";
            }
            else
            {
                entry.RequiredRepresentations = AnalysisEntry.NONE;
            }

            if (metaDataViewModel.IsCommutativeEquivalence)
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.SPECIAL_INTEREST_GROUP_CE);
            }
            if (metaDataViewModel.IsMultiplicationWithZero)
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.SPECIAL_INTEREST_GROUP_ZERO);
            }
            if (metaDataViewModel.IsScaffolded)
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.SPECIAL_INTEREST_GROUP_SCAF);
            }
            if (metaDataViewModel.Is2PSF)
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.SPECIAL_INTEREST_GROUP_2PSF);
            }
            if (metaDataViewModel.Is2PSS)
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.SPECIAL_INTEREST_GROUP_2PSS);
            }
            if (!entry.SpecialInterestGroups.Any())
            {
                entry.SpecialInterestGroups.Add(AnalysisEntry.NONE);
            }

            #endregion // Problem Characteristics

            #region Whole Page Characteristics

            var representationsUsedTag = page.Tags.OfType<RepresentationsUsedTag>().FirstOrDefault();
            if (representationsUsedTag == null)
            {
                entry.IsInkOnly = AnalysisEntry.UNKOWN_ERROR;
                entry.IsBlank = AnalysisEntry.UNKOWN_ERROR;
            }
            else
            {
                switch (representationsUsedTag.RepresentationsUsedType)
                {
                    case RepresentationsUsedTypes.BlankPage:
                        entry.IsInkOnly = AnalysisEntry.NO;
                        entry.IsBlank = AnalysisEntry.YES;
                        break;
                    case RepresentationsUsedTypes.InkOnly:
                        entry.IsInkOnly = AnalysisEntry.YES;
                        entry.IsBlank = AnalysisEntry.NO;
                        break;
                    case RepresentationsUsedTypes.RepresentationsUsed:
                        entry.IsInkOnly = AnalysisEntry.NO;
                        entry.IsBlank = AnalysisEntry.NO;
                        break;
                    default:
                        entry.IsInkOnly = AnalysisEntry.UNKOWN_ERROR;
                        entry.IsBlank = AnalysisEntry.UNKOWN_ERROR;
                        break;
                }
            }

            entry.ArrayDeletedCount = representationsUsedTag.RepresentationsUsed.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && !r.IsFinalRepresentation && r.IsUsed);
            entry.NumberLineDeletedCount = representationsUsedTag.RepresentationsUsed.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && !r.IsFinalRepresentation && r.IsUsed);
            entry.IndividualStampImageDeletedCount = pass3.Count(e => e.CodedObject == Codings.OBJECT_STAMPED_OBJECT && e.EventType == Codings.EVENT_OBJECT_DELETE);
            entry.StampImageRepresentationDeletedCount = representationsUsedTag.RepresentationsUsed.Count(r => r.CodedObject == Codings.OBJECT_STAMP && !r.IsFinalRepresentation && r.IsUsed);

            #endregion // Whole Page Characteristics

            #region Left Side

            var stampIDsOnPage = page.PageObjects.OfType<Stamp>().Select(s => s.ID).ToList();

            var leftRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_LEFT).ToList();

            #region Arrays

            var leftArrays = leftRepresentations.Where(r => r.CodedObject == Codings.OBJECT_ARRAY).ToList();
            var leftUsedArrays = leftArrays.Where(r => r.IsUsed).ToList();

            entry.LeftArrayCreatedCount = leftUsedArrays.Count;

            entry.LeftArrayCutCount = leftUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Deleted by Cut")));

            entry.LeftArraySnapCount = leftUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Created by Snap")));

            var leftArraysWithInkDivides = leftUsedArrays.Where(r => r.AdditionalInformation.Any(s => s.Contains("Total Ink Divides"))).ToList();
            foreach (var leftArrayWithInkDivides in leftArraysWithInkDivides)
            {
                var inkDividesInformation = leftArrayWithInkDivides.AdditionalInformation.First(s => s.Contains("Total Ink Divides"));
                var inkDividesInformationParts = inkDividesInformation.Split(" : ");
                if (inkDividesInformationParts.Length != 2)
                {
                    continue;
                }

                entry.LeftArrayDivideCount += (int)inkDividesInformationParts[1].ToInt();
            }
            
            var leftArraysWithSkips = leftUsedArrays.Where(r => r.AdditionalInformation.Any(a => a.Contains("skip"))).ToList();

            entry.LeftArraySkipCount = leftArraysWithSkips.Count;

            foreach (var usedRepresentation in leftArraysWithSkips)
            {
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = usedRepresentation.CodedID;
                var skipInformations = usedRepresentation.AdditionalInformation.Where(a => a.Contains("skip")).ToList();

                var skips = new List<string>();
                foreach (var skipInformation in skipInformations)
                {
                    var skipCorrectness = string.Empty;
                    if (skipInformation.Contains("correct"))
                    {
                        skipCorrectness = "C";
                    }
                    else if (skipInformation.Contains("wrong dimension"))
                    {
                        skipCorrectness = "WD";
                    }
                    else
                    {
                        skipCorrectness = "O";
                    }

                    var skipSide = skipInformation.Contains("bottom") ? "bottom" : "right";
                    var skip = $"{skipSide}, {skipCorrectness}";
                    skips.Add(skip);
                }

                entry.LeftArraySkipCountingCorretness.Add($"{codedObject} [{codedID}] {string.Join("; ", skips)}");
            }

            #endregion // Arrays

            #region Number Lines

            var leftNumberLines = leftRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var leftUsedNumberLines = leftNumberLines.Where(r => r.IsUsed).ToList();

            entry.LeftNumberLineCreatedCount = leftUsedNumberLines.Count;

            if (entry.LeftNumberLineCreatedCount == 0)
            {
                entry.LeftNLJE = AnalysisEntry.NA;
            }
            else
            {
                var isNLJEUsed = leftUsedNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_NLJE));
                entry.LeftNLJE = isNLJEUsed ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            entry.LeftNumberLineSwitched = leftUsedNumberLines.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            entry.LeftNumberLineBlankCount = leftNumberLines.Count(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_BLANK_PARTIAL_MATCH));

            #endregion // Number Lines

            #region Stamps

            var leftStampImages = leftRepresentations.Where(r => r.CodedObject == Codings.OBJECT_STAMP).ToList();

            var leftStampIDsCreated = new List<string>();
            var leftStampIDsDeleted = new List<string>();
            foreach (var usedRepresentation in leftStampImages)
            {
                //var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (PSID)"));
                //var parentStampInfoParts = parentStampAdditionalInfo.Split(" : ");
                //if (parentStampInfoParts.Length == 2)
                //{
                //    var parentStampIDsComposite = parentStampInfoParts[1];
                //    var parentStampIDs = parentStampIDsComposite.Split(" ; ").ToList();
                //    leftStampIDsCreated.AddRange(parentStampIDs);
                //    leftStampIDsDeleted.AddRange(parentStampIDs.Where(id => !stampIDsOnPage.Contains(id)));
                //}

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.LeftStampImagesCreatedCount += stampImageCount;
                }

                //var companionStampedObjectAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (COID)"));
                //var companionStampedObjectInfoParts = companionStampedObjectAdditionalInfo.Split(" : ");
                //if (companionStampedObjectInfoParts.Length == 2)
                //{
                //    var companionStampedObjectIDsComposite = companionStampedObjectInfoParts[1];
                //    var companionStampedObjectIDs = companionStampedObjectIDsComposite.Split(" ; ").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                //    entry.LeftStampImagesCreatedCount += companionStampedObjectIDs.Count;
                //}
            }

            entry.LeftStampsCreatedCount += leftStampIDsCreated.Count;
            entry.StampDeletedCount += leftStampIDsDeleted.Count;

            entry.LeftStampImagesSwitched = leftStampImages.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Stamps

            #region Representation Correctness Counts

            entry.LeftArrayCorrectCount = leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.Correct);
            entry.LeftArrayPartiallyCorrectCount = leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.PartiallyCorrect);

            entry.LeftNumberLineCorrectCount = leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.Correct);
            entry.LeftNumberLinePartiallyCorrectCount =
                leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.LeftNumberLinePartiallyCorrectSwappedCount =
                leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            entry.LeftStampCorrectCount = leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.Correct);
            entry.LeftStampPartiallyCorrectCount =
                leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.LeftStampPartiallyCorrectSwappedCount =
                leftRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            #endregion // Representation Correctness Counts

            entry.LeftRepresentationsAndCorrectness =
                leftRepresentations.Select(
                                           r =>
                                                   $"{r.CodedObject} [{r.CodedID}] {(r.CodedObject == Codings.OBJECT_STAMP ? r.RepresentationInformation : string.Empty)}{(r.CodedObject == Codings.OBJECT_NUMBER_LINE ? r.RepresentationInformation : string.Empty)} {Codings.CorrectnessToCodedCorrectness(r.Correctness)}")
                                   .ToList();

            entry.IsLeftMR = leftRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Left Side

            #region Right Side

            var rightRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_RIGHT).ToList();

            #region Arrays

            var rightArrays = rightRepresentations.Where(r => r.CodedObject == Codings.OBJECT_ARRAY).ToList();
            var rightUsedArrays = rightArrays.Where(r => r.IsUsed).ToList();

            entry.RightArrayCreatedCount = rightUsedArrays.Count;

            entry.RightArrayCutCount = rightUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Deleted by Cut")));

            entry.RightArraySnapCount = rightUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Created by Snap")));

            var rightArraysWithInkDivides = rightUsedArrays.Where(r => r.AdditionalInformation.Any(s => s.Contains("Total Ink Divides"))).ToList();
            foreach (var rightArrayWithInkDivides in rightArraysWithInkDivides)
            {
                var inkDividesInformation = rightArrayWithInkDivides.AdditionalInformation.First(s => s.Contains("Total Ink Divides"));
                var inkDividesInformationParts = inkDividesInformation.Split(" : ");
                if (inkDividesInformationParts.Length != 2)
                {
                    continue;
                }

                entry.RightArrayDivideCount += (int)inkDividesInformationParts[1].ToInt();
            }

            var rightArraysWithSkips = rightUsedArrays.Where(r => r.AdditionalInformation.Any(a => a.Contains("skip"))).ToList();

            entry.RightArraySkipCount = rightArraysWithSkips.Count;

            foreach (var usedRepresentation in rightArraysWithSkips)
            {
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = usedRepresentation.CodedID;
                var skipInformations = usedRepresentation.AdditionalInformation.Where(a => a.Contains("skip")).ToList();

                var skips = new List<string>();
                foreach (var skipInformation in skipInformations)
                {
                    var skipCorrectness = string.Empty;
                    if (skipInformation.Contains("correct"))
                    {
                        skipCorrectness = "C";
                    }
                    else if (skipInformation.Contains("wrong dimension"))
                    {
                        skipCorrectness = "WD";
                    }
                    else
                    {
                        skipCorrectness = "O";
                    }

                    var skipSide = skipInformation.Contains("bottom") ? "bottom" : "right";
                    var skip = $"{skipSide}, {skipCorrectness}";
                    skips.Add(skip);
                }

                entry.RightArraySkipCountingCorretness.Add($"{codedObject} [{codedID}] {string.Join("; ", skips)}");
            }

            #endregion // Arrays

            #region Number Lines

            var rightNumberLines = rightRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var rightUsedNumberLines = rightNumberLines.Where(r => r.IsUsed).ToList();

            entry.RightNumberLineCreatedCount = rightUsedNumberLines.Count;

            if (entry.RightNumberLineCreatedCount == 0)
            {
                entry.RightNLJE = AnalysisEntry.NA;
            }
            else
            {
                var isNLJEUsed = rightUsedNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_NLJE));
                entry.RightNLJE = isNLJEUsed ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            entry.RightNumberLineSwitched = rightUsedNumberLines.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            entry.RightNumberLineBlankCount = rightNumberLines.Count(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_BLANK_PARTIAL_MATCH));

            #endregion // Number Lines

            #region Stamps

            var rightStampImages = rightRepresentations.Where(r => r.CodedObject == Codings.OBJECT_STAMP).ToList();

            var rightStampIDsCreated = new List<string>();
            var rightStampIDsDeleted = new List<string>();
            foreach (var usedRepresentation in rightStampImages)
            {
                var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (PSID)"));
                var parentStampInfoParts = parentStampAdditionalInfo.Split(" : ");
                if (parentStampInfoParts.Length == 2)
                {
                    var parentStampIDsComposite = parentStampInfoParts[1];
                    var parentStampIDs = parentStampIDsComposite.Split(" ; ").ToList();
                    rightStampIDsCreated.AddRange(parentStampIDs);
                    rightStampIDsDeleted.AddRange(parentStampIDs.Where(id => !stampIDsOnPage.Contains(id)));
                }

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.RightStampImagesCreatedCount += stampImageCount;
                }

                var companionStampedObjectAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (COID)"));
                var companionStampedObjectInfoParts = companionStampedObjectAdditionalInfo.Split(" : ");
                if (companionStampedObjectInfoParts.Length == 2)
                {
                    var companionStampedObjectIDsComposite = companionStampedObjectInfoParts[1];
                    var companionStampedObjectIDs = companionStampedObjectIDsComposite.Split(" ; ").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    entry.RightStampImagesCreatedCount += companionStampedObjectIDs.Count;
                }
            }

            entry.RightStampsCreatedCount += rightStampIDsCreated.Count;
            entry.StampDeletedCount += rightStampIDsDeleted.Count;

            entry.RightStampImagesSwitched = rightStampImages.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Stamps

            #region Representation Correctness Counts

            entry.RightArrayCorrectCount = rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.Correct);
            entry.RightArrayPartiallyCorrectCount = rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.PartiallyCorrect);

            entry.RightNumberLineCorrectCount = rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.Correct);
            entry.RightNumberLinePartiallyCorrectCount =
                rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.RightNumberLinePartiallyCorrectSwappedCount =
                rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            entry.RightStampCorrectCount = rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.Correct);
            entry.RightStampPartiallyCorrectCount =
                rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.RightStampPartiallyCorrectSwappedCount =
                rightRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            #endregion // Representation Correctness Counts

            entry.RightRepresentationsAndCorrectness =
                rightRepresentations.Select(
                                            r =>
                                                    $"{r.CodedObject} [{r.CodedID}] {(r.CodedObject == Codings.OBJECT_STAMP ? r.RepresentationInformation : string.Empty)}{(r.CodedObject == Codings.OBJECT_NUMBER_LINE ? r.RepresentationInformation : string.Empty)} {Codings.CorrectnessToCodedCorrectness(r.Correctness)}")
                                    .ToList();

            entry.IsRightMR = rightRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Right Side

            #region Alternative Side

            var alternativeRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_ALTERNATIVE).ToList();

            #region Arrays

            var alternativeArrays = alternativeRepresentations.Where(r => r.CodedObject == Codings.OBJECT_ARRAY).ToList();
            var alternativeUsedArrays = alternativeArrays.Where(r => r.IsUsed).ToList();

            entry.AlternativeArrayCreatedCount = alternativeUsedArrays.Count;

            entry.AlternativeArrayCutCount = alternativeUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Deleted by Cut")));

            entry.AlternativeArraySnapCount = alternativeUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Created by Snap")));

            var alternativeArraysWithInkDivides = alternativeUsedArrays.Where(r => r.AdditionalInformation.Any(s => s.Contains("Total Ink Divides"))).ToList();
            foreach (var alternativeArrayWithInkDivides in alternativeArraysWithInkDivides)
            {
                var inkDividesInformation = alternativeArrayWithInkDivides.AdditionalInformation.First(s => s.Contains("Total Ink Divides"));
                var inkDividesInformationParts = inkDividesInformation.Split(" : ");
                if (inkDividesInformationParts.Length != 2)
                {
                    continue;
                }

                entry.AlternativeArrayDivideCount += (int)inkDividesInformationParts[1].ToInt();
            }

            var alternativeArraysWithSkips = alternativeUsedArrays.Where(r => r.AdditionalInformation.Any(a => a.Contains("skip"))).ToList();

            entry.AlternativeArraySkipCount = alternativeArraysWithSkips.Count;

            foreach (var usedRepresentation in alternativeArraysWithSkips)
            {
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = usedRepresentation.CodedID;
                var skipInformations = usedRepresentation.AdditionalInformation.Where(a => a.Contains("skip")).ToList();

                var skips = new List<string>();
                foreach (var skipInformation in skipInformations)
                {
                    var skipCorrectness = string.Empty;
                    if (skipInformation.Contains("correct"))
                    {
                        skipCorrectness = "C";
                    }
                    else if (skipInformation.Contains("wrong dimension"))
                    {
                        skipCorrectness = "WD";
                    }
                    else
                    {
                        skipCorrectness = "O";
                    }

                    var skipSide = skipInformation.Contains("bottom") ? "bottom" : "right";
                    var skip = $"{skipSide}, {skipCorrectness}";
                    skips.Add(skip);
                }

                entry.AlternativeArraySkipCountingCorretness.Add($"{codedObject} [{codedID}] {string.Join("; ", skips)}");
            }

            #endregion // Arrays

            #region Number Lines

            var alternativeNumberLines = alternativeRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var alternativeUsedNumberLines = alternativeNumberLines.Where(r => r.IsUsed).ToList();

            entry.AlternativeNumberLineCreatedCount = alternativeUsedNumberLines.Count;

            if (entry.AlternativeNumberLineCreatedCount == 0)
            {
                entry.AlternativeNLJE = AnalysisEntry.NA;
            }
            else
            {
                var isNLJEUsed = alternativeUsedNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_NLJE));
                entry.AlternativeNLJE = isNLJEUsed ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            entry.AlternativeNumberLineSwitched = alternativeUsedNumberLines.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            entry.AlternativeNumberLineBlankCount = alternativeNumberLines.Count(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_BLANK_PARTIAL_MATCH));
            
            #endregion // Number Lines

            #region Stamps

            var alternativeStampImages = alternativeRepresentations.Where(r => r.CodedObject == Codings.OBJECT_STAMP).ToList();

            var alternativeStampIDsCreated = new List<string>();
            var alternativeStampIDsDeleted = new List<string>();
            foreach (var usedRepresentation in alternativeStampImages)
            {
                var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (PSID)"));
                var parentStampInfoParts = parentStampAdditionalInfo.Split(" : ");
                if (parentStampInfoParts.Length == 2)
                {
                    var parentStampIDsComposite = parentStampInfoParts[1];
                    var parentStampIDs = parentStampIDsComposite.Split(" ; ").ToList();
                    alternativeStampIDsCreated.AddRange(parentStampIDs);
                    alternativeStampIDsDeleted.AddRange(parentStampIDs.Where(id => !stampIDsOnPage.Contains(id)));
                }

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.AlternativeStampImagesCreatedCount += stampImageCount;
                }

                var companionStampedObjectAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (COID)"));
                var companionStampedObjectInfoParts = companionStampedObjectAdditionalInfo.Split(" : ");
                if (companionStampedObjectInfoParts.Length == 2)
                {
                    var companionStampedObjectIDsComposite = companionStampedObjectInfoParts[1];
                    var companionStampedObjectIDs = companionStampedObjectIDsComposite.Split(" ; ").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    entry.AlternativeStampImagesCreatedCount += companionStampedObjectIDs.Count;
                }
            }

            entry.AlternativeStampsCreatedCount += alternativeStampIDsCreated.Count;
            entry.StampDeletedCount += alternativeStampIDsDeleted.Count;

            entry.AlternativeStampImagesSwitched = alternativeStampImages.Any(r => r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED) ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Stamps

            #region Representation Correctness Counts

            entry.AlternativeArrayCorrectCount = alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.Correct);
            entry.AlternativeArrayPartiallyCorrectCount = alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.PartiallyCorrect);

            entry.AlternativeNumberLineCorrectCount = alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.Correct);
            entry.AlternativeNumberLinePartiallyCorrectCount =
                alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.AlternativeNumberLinePartiallyCorrectSwappedCount =
                alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            entry.AlternativeStampCorrectCount = alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.Correct);
            entry.AlternativeStampPartiallyCorrectCount =
                alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason != Codings.PARTIAL_REASON_SWAPPED);
            entry.AlternativeStampPartiallyCorrectSwappedCount =
                alternativeRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect && r.CorrectnessReason == Codings.PARTIAL_REASON_SWAPPED);

            #endregion // Representation Correctness Counts

            entry.AlternativeRepresentationsAndCorrectness =
                alternativeRepresentations.Select(
                                                  r =>
                                                          $"{r.CodedObject} [{r.CodedID}] {(r.CodedObject == Codings.OBJECT_STAMP ? r.RepresentationInformation : string.Empty)}{(r.CodedObject == Codings.OBJECT_NUMBER_LINE ? r.RepresentationInformation : string.Empty)} {Codings.CorrectnessToCodedCorrectness(r.Correctness)}")
                                          .ToList();

            entry.IsAlternativeMR = alternativeRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Alternative Side

            #region Unmatched

            var unmatchedRepresentations = representationsUsedTag.RepresentationsUsed.Where(r => r.MatchedRelationSide == Codings.MATCHED_RELATION_NONE).ToList();

            #region Arrays

            var unmatchedArrays = unmatchedRepresentations.Where(r => r.CodedObject == Codings.OBJECT_ARRAY).ToList();
            var unmatchedUsedArrays = unmatchedArrays.Where(r => r.IsUsed).ToList();

            entry.UnmatchedArrayCreatedCount = unmatchedUsedArrays.Count;

            entry.UnmatchedArrayCutCount = unmatchedUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Deleted by Cut")));

            entry.UnmatchedArraySnapCount = unmatchedUsedArrays.Count(r => r.AdditionalInformation.Any(a => a.Contains("Created by Snap")));

            var unmatchedArraysWithInkDivides = unmatchedUsedArrays.Where(r => r.AdditionalInformation.Any(s => s.Contains("Total Ink Divides"))).ToList();
            foreach (var unmatchedArrayWithInkDivides in unmatchedArraysWithInkDivides)
            {
                var inkDividesInformation = unmatchedArrayWithInkDivides.AdditionalInformation.First(s => s.Contains("Total Ink Divides"));
                var inkDividesInformationParts = inkDividesInformation.Split(" : ");
                if (inkDividesInformationParts.Length != 2)
                {
                    continue;
                }

                entry.UnmatchedArrayDivideCount += (int)inkDividesInformationParts[1].ToInt();
            }

            var unmatchedArraysWithSkips = unmatchedUsedArrays.Where(r => r.AdditionalInformation.Any(a => a.Contains("skip"))).ToList();

            entry.UnmatchedArraySkipCount = unmatchedArraysWithSkips.Count;

            foreach (var usedRepresentation in unmatchedArraysWithSkips)
            {
                var codedObject = Codings.OBJECT_ARRAY;
                var codedID = usedRepresentation.CodedID;
                var skipInformations = usedRepresentation.AdditionalInformation.Where(a => a.Contains("skip")).ToList();

                var skips = new List<string>();
                foreach (var skipInformation in skipInformations)
                {
                    var skipCorrectness = string.Empty;
                    if (skipInformation.Contains("correct"))
                    {
                        skipCorrectness = "C";
                    }
                    else if (skipInformation.Contains("wrong dimension"))
                    {
                        skipCorrectness = "WD";
                    }
                    else
                    {
                        skipCorrectness = "O";
                    }

                    var skipSide = skipInformation.Contains("bottom") ? "bottom" : "right";
                    var skip = $"{skipSide}, {skipCorrectness}";
                    skips.Add(skip);
                }

                entry.UnmatchedArraySkipCountingCorretness.Add($"{codedObject} [{codedID}] {string.Join("; ", skips)}");
            }

            #endregion // Arrays

            #region Number Lines

            var unmatchedNumberLines = unmatchedRepresentations.Where(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE).ToList();
            var unmatchedUsedNumberLines = unmatchedNumberLines.Where(r => r.IsUsed).ToList();

            entry.UnmatchedNumberLineCreatedCount = unmatchedUsedNumberLines.Count;

            if (entry.UnmatchedNumberLineCreatedCount == 0)
            {
                entry.UnmatchedNLJE = AnalysisEntry.NA;
            }
            else
            {
                var isNLJEUsed = unmatchedUsedNumberLines.Any(r => r.AnalysisCodes.Contains(Codings.NUMBER_LINE_NLJE));
                entry.UnmatchedNLJE = isNLJEUsed ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            #endregion // Number Lines

            #region Stamps

            var unmatchedStampImages = unmatchedRepresentations.Where(r => r.CodedObject == Codings.OBJECT_STAMP).ToList();

            var unmatchedStampIDsCreated = new List<string>();
            var unmatchedStampIDsDeleted = new List<string>();
            foreach (var usedRepresentation in unmatchedStampImages)
            {
                var parentStampAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (PSID)"));
                var parentStampInfoParts = parentStampAdditionalInfo.Split(" : ");
                if (parentStampInfoParts.Length == 2)
                {
                    var parentStampIDsComposite = parentStampInfoParts[1];
                    var parentStampIDs = parentStampIDsComposite.Split(" ; ").ToList();
                    unmatchedStampIDsCreated.AddRange(parentStampIDs);
                    unmatchedStampIDsDeleted.AddRange(parentStampIDs.Where(id => !stampIDsOnPage.Contains(id)));
                }

                var representationInfoParts = usedRepresentation.RepresentationInformation.Split(' ');
                if (representationInfoParts.Length == 2)
                {
                    var stampImageCount = (int)representationInfoParts[0].ToInt();
                    entry.UnmatchedStampImagesCreatedCount += stampImageCount;
                }

                var companionStampedObjectAdditionalInfo = usedRepresentation.AdditionalInformation.FirstOrDefault(a => a.Contains("UNLISTED (COID)"));
                var companionStampedObjectInfoParts = companionStampedObjectAdditionalInfo.Split(" : ");
                if (companionStampedObjectInfoParts.Length == 2)
                {
                    var companionStampedObjectIDsComposite = companionStampedObjectInfoParts[1];
                    var companionStampedObjectIDs = companionStampedObjectIDsComposite.Split(" ; ").Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    entry.UnmatchedStampImagesCreatedCount += companionStampedObjectIDs.Count;
                }
            }

            entry.UnmatchedStampsCreatedCount += unmatchedStampIDsCreated.Count;
            entry.StampDeletedCount += unmatchedStampIDsDeleted.Count;

            #endregion // Stamps

            #region Representation Correctness Counts

            entry.UnmatchedArrayPartiallyCorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.PartiallyCorrect);
            entry.UnmatchedArrayIncorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_ARRAY && r.Correctness == Correctness.Incorrect);

            entry.UnmatchedNumberLinePartiallyCorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.PartiallyCorrect);
            entry.UnmatchedNumberLineIncorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.Incorrect);
            entry.UnmatchedNumberLineUnknownCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_NUMBER_LINE && r.Correctness == Correctness.Unknown);

            entry.UnmatchedStampPartiallyCorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.PartiallyCorrect);
            entry.UnmatchedStampIncorrectCount = unmatchedRepresentations.Count(r => r.CodedObject == Codings.OBJECT_STAMP && r.Correctness == Correctness.Incorrect);

            #endregion // Representation Correctness Counts

            entry.UnmatchedRepresentationsAndCorrectness =
                unmatchedRepresentations.Select(
                                                r =>
                                                        $"{r.CodedObject} [{r.CodedID}] {(r.CodedObject == Codings.OBJECT_STAMP ? r.RepresentationInformation : string.Empty)}{(r.CodedObject == Codings.OBJECT_NUMBER_LINE ? r.RepresentationInformation : string.Empty)} {Codings.CorrectnessToCodedCorrectness(r.Correctness)}")
                                        .ToList();

            entry.IsUnmatchedMR = unmatchedRepresentations.Select(r => r.CodedObject).Distinct().Count() > 1 ? AnalysisEntry.YES : AnalysisEntry.NO;

            #endregion // Unmatched

            #region Whole Page Analysis

            var totalNumberLineUsedCount = entry.LeftNumberLineCreatedCount + entry.RightNumberLineCreatedCount + entry.AlternativeNumberLineCreatedCount + entry.UnmatchedNumberLineCreatedCount;
            if (totalNumberLineUsedCount == 0)
            {
                entry.NLJE = AnalysisEntry.NA;
            }
            else
            {
                var isNLJEUsed = entry.LeftNLJE == AnalysisEntry.YES || entry.RightNLJE == AnalysisEntry.YES || entry.AlternativeNLJE == AnalysisEntry.YES || entry.UnmatchedNLJE == AnalysisEntry.YES;
                entry.NLJE = isNLJEUsed ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            entry.IsMR2STEP = AnalysisEntry.NA;
            if (representationsUsedTag != null &&
                representationsUsedTag.RepresentationsUsedType == RepresentationsUsedTypes.RepresentationsUsed &&
                entry.ProblemType != AnalysisEntry.PROBLEM_TYPE_1_PART)
            {
                var isMR2STEP = representationsUsedTag.AnalysisCodes.Contains(Codings.REPRESENTATIONS_MR2STEP);
                entry.IsMR2STEP = isMR2STEP ? AnalysisEntry.YES : AnalysisEntry.NO;
            }

            var intermediaryAnswerCorrectnessTag = page.Tags.OfType<IntermediaryAnswerCorrectnessTag>().FirstOrDefault();
            if (entry.ProblemType == AnalysisEntry.PROBLEM_TYPE_1_PART)
            {
                entry.IntermediaryAnswerCorrectness = AnalysisEntry.NA;
            }
            else if (intermediaryAnswerCorrectnessTag == null)
            {
                entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNKNOWN;
            }
            else
            {
                switch (intermediaryAnswerCorrectnessTag.IntermediaryAnswerCorrectness)
                {
                    case Correctness.Correct:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_CORRECT;
                        break;
                    case Correctness.Incorrect:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_INCORRECT;
                        break;
                    case Correctness.PartiallyCorrect:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_PARTIAL;
                        break;
                    case Correctness.Illegible:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_ILLEGIBLE;
                        break;
                    case Correctness.Unanswered:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNANSWERED;
                        break;
                    default:
                        entry.IntermediaryAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNKNOWN;
                        break;
                }
            }

            var finalAnswerCorrectnessTag = page.Tags.OfType<FinalAnswerCorrectnessTag>().FirstOrDefault();
            if (finalAnswerCorrectnessTag == null)
            {
                entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNKNOWN;
            }
            else
            {
                switch (finalAnswerCorrectnessTag.FinalAnswerCorrectness)
                {
                    case Correctness.Correct:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_CORRECT;
                        break;
                    case Correctness.Incorrect:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_INCORRECT;
                        break;
                    case Correctness.PartiallyCorrect:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_PARTIAL;
                        break;
                    case Correctness.Illegible:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_ILLEGIBLE;
                        break;
                    case Correctness.Unanswered:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNANSWERED;
                        break;
                    default:
                        entry.FinalAnswerCorrectness = AnalysisEntry.CORRECTNESS_UNKNOWN;
                        break;
                }
            }

            var answerRepresentationSequenceTag = page.Tags.OfType<AnswerRepresentationSequenceTag>().FirstOrDefault();
            if (answerRepresentationSequenceTag == null)
            {
                entry.ABR_RAA.Add(AnalysisEntry.NA);
                entry.AnswersChangedAfterRepresentation.Add(AnalysisEntry.NO);
            }
            else
            {
                if (answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_FINAL_ANS_COR_BEFORE_REP) ||
                    answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_FINAL_ANS_INC_BEFORE_REP))
                {
                    entry.ABR_RAA.Add(AnalysisEntry.FABR);
                }

                if (answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_INTERMEDIARY_ANS_COR_BEFORE_REP) ||
                    answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_INTERMEDIARY_ANS_INC_BEFORE_REP))
                {
                    entry.ABR_RAA.Add(AnalysisEntry.IABR);
                }

                if (answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_REP_AFTER_FINAL_ANSWER))
                {
                    entry.ABR_RAA.Add(AnalysisEntry.RAFA);
                }

                if (answerRepresentationSequenceTag.AnalysisCodes.Contains(Codings.ANALYSIS_REP_AFTER_INTERMEDIARY_ANSWER))
                {
                    entry.ABR_RAA.Add(AnalysisEntry.RAIA);
                }

                if (!entry.ABR_RAA.Any())
                {
                    entry.ABR_RAA.Add(AnalysisEntry.NA);
                }

                foreach (var analysisCode in answerRepresentationSequenceTag.AnalysisCodes)
                {
                    if (analysisCode == Codings.ANALYSIS_FINAL_ANS_COR_BEFORE_REP ||
                        analysisCode == Codings.ANALYSIS_FINAL_ANS_INC_BEFORE_REP ||
                        analysisCode == Codings.ANALYSIS_INTERMEDIARY_ANS_COR_BEFORE_REP ||
                        analysisCode == Codings.ANALYSIS_INTERMEDIARY_ANS_INC_BEFORE_REP ||
                        analysisCode == Codings.ANALYSIS_REP_AFTER_FINAL_ANSWER ||
                        analysisCode == Codings.ANALYSIS_REP_AFTER_INTERMEDIARY_ANSWER)
                    {
                        continue;
                    }

                    entry.AnswersChangedAfterRepresentation.Add(analysisCode);
                }

                if (!entry.AnswersChangedAfterRepresentation.Any())
                {
                    entry.AnswersChangedAfterRepresentation.Add(AnalysisEntry.NO);
                }
            }

            var studentInkStrokes = page.InkStrokes.Concat(page.History.TrashedInkStrokes).Where(s => s.GetStrokeOwnerID() == page.Owner.ID).ToList();
            var colorsUsed = studentInkStrokes.Select(s => s.DrawingAttributes.Color).Distinct();
            entry.InkColorsUsedCount = colorsUsed.Count();

            #endregion // Whole Page Analysis

            #region Total History

            var pass3CodedValues = pass3.Select(h => h.CodedValue).ToList();
            entry.FinalSemanticEvents = pass3CodedValues;

            #endregion // Total History

            return entry;
        }
    }
}
