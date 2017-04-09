using System;
using System.IO;
using Catel;

namespace CLP.Entities
{
    public static class CLogger
    {
        #region Constants

        private const string DEFAULT_CLP_DATA_FOLDER_NAME = "CLPData";
        private const string DEFAULT_LOGS_FOLDER_NAME = "Logs";
        private const string DEFAULT_LOG_FILE_NAME = "Classroom Learning Partner";
        private const string DEFAULT_LOG_FILE_EXTENSION = "log";

        #endregion // Constants

        #region Constructor

        static CLogger()
        {
            CachedLogFilePath = DefaultCLPLogFilePath;
        }

        #endregion // Constructor

        #region Properties

        private static readonly object LogLock = new object();

        private static readonly string CachedLogFilePath;

        private static string WindowsDriveFolderPath => Path.GetPathRoot(Environment.SystemDirectory);

        private static string DefaultCLPDataFolderPath
        {
            get
            {
                var folderPath = Path.Combine(WindowsDriveFolderPath, DEFAULT_CLP_DATA_FOLDER_NAME);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        private static string DefaultCLPLogsFolderPath
        {
            get
            {
                var folderPath = Path.Combine(DefaultCLPDataFolderPath, DEFAULT_LOGS_FOLDER_NAME);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }

        private static string DefaultCLPLogFilePath
        {
            get
            {
                var fileName = $"{DEFAULT_LOG_FILE_NAME}.{DEFAULT_LOG_FILE_EXTENSION}";
                var filePath = Path.Combine(DefaultCLPLogsFolderPath, fileName);

                return filePath;
            }
        }

        #endregion // Properties

        #region Methods

        public static void AppendToLog(string appendString)
        {
            lock (LogLock)
            {
                var currentTime = FastDateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var loggedString = $"{Environment.NewLine}{currentTime} - {appendString}";
                File.AppendAllText(CachedLogFilePath, loggedString);
                BackupLog();
            }
        }

        private static void BackupLog()
        {
            var fileInfo = new FileInfo(CachedLogFilePath);
            var fileSizeInKb = fileInfo.Length / 32; //1024;
            if (fileSizeInKb <= 1024.0)
            {
                return;
            }

            ForceNewLogFile();
        }

        public static void ForceNewLogFile()
        {
            var currentTime = FastDateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
            var backupFileName = $"{currentTime}.{DEFAULT_LOG_FILE_NAME}.{DEFAULT_LOG_FILE_EXTENSION}";
            var backupFilePath = Path.Combine(DefaultCLPLogsFolderPath, backupFileName);
            File.Move(CachedLogFilePath, backupFilePath);
        }

        #endregion // Methods
    }
}