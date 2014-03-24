using System;
using System.Linq;
using CLP.Entities;

namespace ConsoleScripts
{
    class Program
    {
        static void Main(string[] args)
        {
            DatabaseTesting();
            //while(true)
            //{
            //    Console.Write("> ");
            //    string command = Console.ReadLine();
            //    if(!processCommand(command))
            //    {
            //        break;
            //    }
            //}
        }

        static void DatabaseTesting()
        {
            using(var context = new ClassPeriodContext())
            {
                var classSubject = new ClassSubject
                                   {
                                       Name = "Math",
                                       TeacherName = "Emily Sparks",
                                       GradeLevel = "4",
                                       StartDate = new DateTime(2013, 9, 3),
                                       EndDate = new DateTime(2014, 6, 20),
                                       SchoolName = "King Open School",
                                       SchoolDistrict = "Cambridge Public Schools",
                                       City = "Cambridge",
                                       State = "Massachusetts"
                                   };
                classSubject.ClearDirtyFlag();

                var notebook = new Notebook
                               {
                                   Name = "Blarg"
                               };
                notebook.ClearDirtyFlag();

                for(var i = 0; i < 6; i++)
                {
                    var page = new CLPPage();
                    page.PageType = PageTypes.Animation;
                    page.ClearDirtyFlag();
                    notebook.AddCLPPageToNotebook(page);

                    var page2 = new CLPPage();
                    page2.ClearDirtyFlag();
                    notebook.AddCLPPageToNotebook(page2);
                }

                context.Notebooks.Add(notebook);
                context.SaveChanges();

                var query = from n in context.Notebooks
                            orderby n.Name
                            select n;

                foreach(var notebook1 in query)
                {
                    Console.WriteLine(notebook1.Name);
                    Console.WriteLine("number of pages: {0}", notebook1.CLPPages.Count);
                }

                Console.ReadLine();
            }
        }

        // Process a console command
        // returns true iff the console should accept another command after this one
        static Boolean processCommand(string command)
        {
            if(command.Equals("replace"))
            {
                replace();
            }
            else if(command.Equals("interpret student"))
            {
                batchInterpretStudent();
            }
            else if(command.Equals("interpret teacher"))
            {
                batchInterpretTeacher();
            }
            else if(command.Equals("strip"))
            {
                stripHistory();
            }
            else if(command.Equals("combine"))
            {
                NotebookMerge.Combine();
            }
            else if(command.Equals("xml"))
            {
                XMLImporter.Import();
            }
            else if(command.Equals("exit"))
            {
                return false;
            }
            else
            {
                Console.WriteLine("Command not recognized");
            }
            return true;
        }

        static void replace()
        {
            Console.WriteLine("Starting");

            NotebookMerge.Replace();

            Console.WriteLine("Ended");
        }

        static void batchInterpretStudent()
        {
            BatchInterpreter.InterpretStudentNotebooks();
        }

        static void batchInterpretTeacher()
        {
            BatchInterpreter.InterpretTeacherNotebooks();
        }

        static void stripHistory()
        {
            StripHistory.StripAll();
        }
    }
}