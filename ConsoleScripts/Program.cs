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
            using(var context = new NotebookContext())
            {
                var notebook = new Notebook
                               {
                                   Name = "Test",
                                   CreationDate = DateTime.Now
                               };
                notebook.ClearDirtyFlag();

                context.Notebooks.Add(notebook);
                context.SaveChanges();

                var query = from n in context.Notebooks
                            orderby n.Name
                            select n;

                foreach(var notebook1 in query)
                {
                    Console.WriteLine(notebook1.Name);
                }
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