using System;
using System.IO;
using CLP.Models;

namespace ConsoleScripts
{
    public class StripHistory
    {
        public static void StripAll()
        {
            string studentDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\StripHistories";
            if (Directory.Exists(studentDirectoryPath))
            {
                foreach (string studentFilePath in Directory.GetFiles(studentDirectoryPath, "*.clp"))
                {
                }
            }
        }

        public static void Strip(CLPNotebook notebookToStrip)
        {
            foreach (var page in notebookToStrip.Pages)
            {
                page.PageHistory.ClearHistory();
                foreach (var submission in notebookToStrip.Submissions[page.UniqueID])
                {
                    submission.PageHistory.ClearHistory();
                }
            }
        }
    }
}
