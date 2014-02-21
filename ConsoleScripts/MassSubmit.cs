using System;
using System.Diagnostics;
using System.IO;
using Catel.Data;
using CLP.Models;

namespace ConsoleScripts
{
    public class MassSubmit
    {
        public static void SubmitSinglePage(int pageIndex)
        {
            string teacherFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\TeacherNotebook" + @"\" + @"1 Teacher - Inquiry Project - Grade 3" + @".clp";
            string studentDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\StudentNotebooks";

            CLPNotebook teacherNotebook = OpenNotebook(teacherFilePath);

            if(Directory.Exists(studentDirectoryPath) && teacherNotebook != null)
            {
                foreach(string studentFilePath in Directory.GetFiles(studentDirectoryPath, "*.clp"))
                {
                    CLPNotebook studentNotebook = OpenNotebook(studentFilePath);

                    if (studentNotebook != null)
                    {
                      //  Console.WriteLine("Student Notebook Open: " + studentNotebook.UserName);
                        var submission = studentNotebook.Pages[pageIndex];

                        teacherNotebook.AddStudentSubmission(submission.UniqueID, submission);
                    }
                }

                teacherNotebook.Save(teacherFilePath);
            }
            else
            {
                Console.WriteLine("Student Filepath Wrong or Teacher Notebook Failed to Open");
            }
        }

        public static CLPNotebook OpenNotebook(string filePath)
        {
            if(!File.Exists(filePath))
            {
                return null;
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            CLPNotebook notebook = null;

            try
            {     
                ModelBase.GlobalLeanAndMeanModel = true;
                notebook = ModelBase.Load<CLPNotebook>(filePath, SerializationMode.Binary);
                ModelBase.GlobalLeanAndMeanModel = false;
            }
            catch(Exception ex)
            {
                Console.WriteLine("[ERROR] - Notebook could not be loaded: " + ex.Message);
            }

            stopWatch.Stop();
            Console.WriteLine("Time to OPEN notebook (In Seconds): " + stopWatch.ElapsedMilliseconds / 1000.0);

            if(notebook == null)
            {
                Console.WriteLine("Notebook could not be opened. Check error log.");
                return null;
            }


            var stopWatch2 = new Stopwatch();
            stopWatch2.Start();

            foreach(var page in notebook.Pages)
            {
                ACLPPageBase.Deserialize(page);
                if(!notebook.Submissions.ContainsKey(page.UniqueID))
                {
                    continue;
                }
                foreach(var submission in notebook.Submissions[page.UniqueID])
                {
                    ACLPPageBase.Deserialize(submission);
                }
            }

            stopWatch2.Stop();
            Console.WriteLine("Time to DESERIALIZE PAGES in notebook (In Seconds): " + stopWatch2.ElapsedMilliseconds / 1000.0);

            var stopWatch3 = new Stopwatch();
            stopWatch3.Start();

            notebook.InitializeAfterDeserialize();

            stopWatch3.Stop();
            Console.WriteLine("Time to INITIALIZE notebook (In Seconds): " + stopWatch3.ElapsedMilliseconds / 1000.0);

            return notebook;
        }
    }
}
