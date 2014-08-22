﻿using System;
using System.IO;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner
{
    public class Logger
    {
        static readonly Logger _instance = new Logger();

        public static Logger Instance
        {
            get { return _instance; }
        }

        private string fileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Logs";
        private string fileName = "CLPLog" + MainWindowViewModel.CurrentProgramMode.ToString() + ".log";
        
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
    }
}
