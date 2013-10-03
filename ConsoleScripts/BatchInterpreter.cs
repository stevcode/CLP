using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLP.Models;

namespace ConsoleScripts
{
    class BatchInterpreter
    {
        public static void InterpretStudentNotebooks()
        {
            // We will interpret all the notebooks in Desktop\StudentNotebooks\
            string studentDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\StudentNotebooks";
            Console.WriteLine("studentDirectoryPath = " + studentDirectoryPath);

            if(Directory.Exists(studentDirectoryPath))
            {
                foreach(string studentFilePath in Directory.GetFiles(studentDirectoryPath, "*.clp"))
                {
                    CLPNotebook studentNotebook = MassSubmit.OpenNotebook(studentFilePath);

                    if(studentNotebook != null)
                    {
                        Console.WriteLine("Student Notebook Open: " + studentNotebook.NotebookName);
                        foreach(CLPPage page in studentNotebook.Pages)
                        {
                            Console.WriteLine("Processing page " + page.PageIndex);

                            PageAnalysis.AnalyzeArray(page);
                            PageAnalysis.AnalyzeStamps(page);
                        }
                        //Console.WriteLine("Done interpreting notebook " + studentNotebook.NotebookName + "; press any key to continue");
                        //Console.ReadKey();

                        Console.WriteLine("Done interpreting notebook " + studentNotebook.NotebookName);
                    }
                } // End of foreach loop iterating over .clp files
            }
            else
            {
                Console.WriteLine("StudentNotebooks directory does not exist.");
            }
        }

        public static void InterpretTeacherNotebooks()
        {
            // We will interpret all the notebooks in Desktop\Notebooks\
            string notebookDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Notebooks";
            Console.WriteLine("notebookDirectoryPath = " + notebookDirectoryPath);

            if(Directory.Exists(notebookDirectoryPath))
            {
                foreach(string studentFilePath in Directory.GetFiles(notebookDirectoryPath, "*.clp"))
                {
                    CLPNotebook notebook = MassSubmit.OpenNotebook(studentFilePath);

                    if(notebook != null)
                    {
                        Console.WriteLine("Notebook Open: " + notebook.NotebookName);
                        foreach(CLPPage page in notebook.Pages)
                        {
                            Console.WriteLine("Processing page " + page.PageIndex);
                            if(notebook.Submissions.ContainsKey(page.UniqueID))
                            {
                                foreach(ICLPPage submission in notebook.Submissions[page.UniqueID])
                                {
                                    Console.WriteLine("Analyzing submission from " + submission.Submitter.FullName);
                                    PageAnalysis.AnalyzeArray(submission);
                                    PageAnalysis.AnalyzeStamps(submission);
                                }
                            }
                        }
                        //Console.WriteLine("Done interpreting notebook " + studentNotebook.NotebookName + "; press any key to continue");
                        //Console.ReadKey();

                        Console.WriteLine("Done interpreting notebook " + notebook.NotebookName);
                    }
                } // End of foreach loop iterating over .clp files
            }
            else
            {
                Console.WriteLine("Notebooks directory does not exist.");
            }
        }
    }
}
