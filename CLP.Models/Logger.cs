using System;
using System.IO;

namespace CLP.Models
{
    public class Logger
    {
        static readonly Logger _instance = new Logger();

        public static Logger Instance
        {
            get { return _instance; }
        }

        private string fileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Logs";
        private string fileName = "CLPLogModel" + ".log";
        
        private string filePath;

        private Logger()
        {
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            filePath = fileDirectory + @"\" + fileName;
            if (!File.Exists(filePath))
            {
                //File.Create(filePath);
                File.WriteAllText(filePath, "**Log File Created**");
            }
        }

        public void InitializeLog()
        {
            string initializeString = "*** New Log Instance - " + DateTime.Now.ToString("MM.dd.yyyy") + " " + DateTime.Now.ToLongTimeString() + " ***";
            WriteToLog(initializeString);
        }

        public void WriteToLog(string s)
        {
            File.AppendAllText(filePath, Environment.NewLine + DateTime.Now.ToString("MM.dd.yyyy") + " " + DateTime.Now.ToLongTimeString() + " [CLP Logger]: " + s);
            Console.WriteLine(DateTime.Now.ToString("MM.dd.yyyy") + " " + DateTime.Now.ToLongTimeString() + " [CLP Logger]: " + s);
        }

        public void WriteErrorToLog(string s, Exception ex)
        {
            Instance.WriteToLog(s);
            Instance.WriteToLog("Exception: " + ex.Message);
            Instance.WriteToLog("[UNHANDLED ERROR] - " + ex.Message + " " +
                                       (ex.InnerException != null ? "\n" + ex.InnerException.Message : null));
            Instance.WriteToLog("[HResult]: " + ex.HResult);
            Instance.WriteToLog("[Source]: " + ex.Source);
            Instance.WriteToLog("[Method]: " + ex.TargetSite);
            Instance.WriteToLog("[StackTrace]: " + ex.StackTrace);
        }
    }
}
