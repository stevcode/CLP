using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                        Console.WriteLine("Student Notebook Open: " + studentNotebook.UserName);
                        CLPPage submission = studentNotebook.Pages[pageIndex];

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
            CLPNotebook notebook = null;

            if(File.Exists(filePath))
            {
                //Steve - Conversion happens here
                try
                {
                    notebook = CLPNotebook.Load(filePath, true);
                }
                catch(Exception)
                {
                    Console.WriteLine("Exception Opening Notebook at {0}", filePath);
                }

                if(notebook != null)
                {
                    foreach(CLPPage page in notebook.Pages)
                    {
                        foreach(ICLPPageObject pageObject in page.PageObjects)
                        {
                            pageObject.ParentPage = page;
                        }
                        if(notebook.Submissions.ContainsKey(page.UniqueID))
                        {
                            foreach(CLPPage submission in notebook.Submissions[page.UniqueID])
                            {
                                foreach(ICLPPageObject pageObject in submission.PageObjects)
                                {
                                    pageObject.ParentPage = submission;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Couldn't Open Notebook at {0}", filePath);
                }
            }
            else
            {
                Console.WriteLine("Filepath Wrong at {0}", filePath);
            }

            return notebook;
        }
    }
}
