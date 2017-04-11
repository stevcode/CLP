using System;
using System.IO;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner
{
    public class Logger
    {
        private static readonly Logger _instance = new Logger();

        public static Logger Instance
        {
            get { return _instance; }
        }

        private string fileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Logs";
        private string fileName = string.Empty;

        private string filePath;

        private Logger() { }

        public void InitializeLog(ProgramModes currentProgramMode)
        {
            //fileName = "CLPLog" + currentProgramMode + ".log";
            //if (!Directory.Exists(fileDirectory))
            //{
            //    Directory.CreateDirectory(fileDirectory);
            //}

            //filePath = fileDirectory + @"\" + fileName;
            //if (!File.Exists(filePath))
            //{
            //    //File.Create(filePath);
            //    File.WriteAllText(filePath, "**Log File Created**");
            //}

            //var initializeString = "*** New Log Instance - " + DateTime.Now.ToString("MM.dd.yyyy") + " " + DateTime.Now.ToLongTimeString() + " ***";
            //WriteToLog(initializeString);
        }

        public void WriteToLog(string s)
        {
            //File.AppendAllText(filePath,
            //                   Environment.NewLine + DateTime.Now.ToString("MM.dd.yyyy") + " " + DateTime.Now.ToLongTimeString() + " [CLP Logger]: " +
            //                   s);
            //CLogger.AppendToLog(DateTime.Now.ToString("MM.dd.yyyy") + " " + DateTime.Now.ToLongTimeString() + " [CLP Logger]: " + s);
        }
    }
}