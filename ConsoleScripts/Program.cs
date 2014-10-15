using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Xml.Serialization;
using Catel.Data;
using CLP.Entities;
using Catel.Runtime.Serialization;
using Path = Catel.IO.Path;

namespace ConsoleScripts
{
    class Program
    {
        static void Main(string[] args)
        {
            //while(true)
            //{
            //    Console.Write("> ");
            //    string command = Console.ReadLine();
            //    if(!processCommand(command))
            //    {
            //        break;
            //    }
            //}

            //while(true)
            //{
            //    Console.Write("> ");
            //    var command = Console.ReadLine();
            //    if(command == null)
            //    {
            //        continue;
            //    }
            //    if(command == "exit")
            //    {
            //        return;
            //    }
            //    var compactID = new Guid(command).ToCompactID();
            //    Console.WriteLine("CompactID: " + compactID);
            //}

            Convert();
        }

        static void Convert()
        {
            var convertFromFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Convert");
            var convertToFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Converted");
            if(!Directory.Exists(convertToFolderPath))
            {
                Directory.CreateDirectory(convertToFolderPath);
            }

            var notebookFolderPaths = Directory.EnumerateDirectories(convertFromFolderPath);
            foreach(var notebookFolderPath in notebookFolderPaths)
            {
                var filePath = Path.Combine(notebookFolderPath, "notebook.xml");
                var notebook = ModelBase.Load<Notebook>(filePath, SerializationMode.Xml);
                var pagesFolderPath = Path.Combine(notebookFolderPath, "Pages");
                var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml");
                foreach(var pageFilePath in pageFilePaths)
                {
                    var page = ModelBase.Load<CLPPage>(pageFilePath, SerializationMode.Xml);
                    page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
                    page.History.TrashedInkStrokes = StrokeDTO.LoadInkStrokes(page.History.SerializedTrashedInkStrokes);

                    Console.WriteLine("Loaded page {0}, differentiation {1}, version {2}", page.PageNumber, page.DifferentiationLevel, page.VersionIndex);
                    //Do stuff to each page here.

                    var savedTags = page.Tags.Where(tag => tag is StarredTag || tag is DottedTag || tag is CorrectnessTag).ToList();
                    page.Tags = null;
                    page.Tags = new ObservableCollection<ITag>(savedTags);

                    if (page.Owner == null)
                    {
                        page.Owner = notebook.Owner;
                    }

                    if(page.Owner.ID != Person.Author.ID)
                    {
                       // ArrayAnalysis.AnalyzeHistory(page);
                        DivisionTemplateAnalysis.AnalyzeHistory(page);

                        if(page.VersionIndex != 0)
                        {
                            if (!page.Tags.Any(tag => tag is StarredTag))
                            {
                                page.AddTag(new StarredTag(page, Origin.TeacherPageGenerated, StarredTag.AcceptedValues.Unstarred));
                            }

                            if (!page.Tags.Any(tag => tag is DottedTag))
                            {
                                page.AddTag(new DottedTag(page, Origin.TeacherPageGenerated, DottedTag.AcceptedValues.Undotted));
                            }

                            if (!page.Tags.Any(tag => tag is CorrectnessTag))
                            {
                                page.AddTag(new CorrectnessTag(page, Origin.TeacherPageGenerated, Correctness.Unknown, false));
                            }
                        }
                    }

                    //Finished doing stuff to page, it'll save below.
                    page.ToXML(pageFilePath, true);
                }
            }
        }

        //Clear Authored Histories
        //var undoItemsToRemove = page.History.UndoItems.Where(historyItem => historyItem.OwnerID == Person.Author.ID).ToList();
        //            foreach(var historyItem in undoItemsToRemove)
        //            {
        //                page.History.UndoItems.Remove(historyItem);
        //            }

        //            var redoItemsToRemove = page.History.RedoItems.Where(historyItem => historyItem.OwnerID == Person.Author.ID).ToList();
        //            foreach(var historyItem in redoItemsToRemove)
        //            {
        //                page.History.RedoItems.Remove(historyItem);
        //            }

        //            page.History.OptimizeTrashedItems();
        

        static void DatabaseTesting()
        {
            using(var context = new ClassPeriodContext())
            {
                var classSubject = new ClassSubject
                                   {
                                       Name = "Math",
                                       GradeLevel = "4",
                                       StartDate = new DateTime(2013, 9, 3),
                                       EndDate = new DateTime(2014, 6, 20),
                                       SchoolName = "King Open School",
                                       SchoolDistrict = "Cambridge Public Schools",
                                       City = "Cambridge",
                                       State = "Massachusetts"
                                   };
                var teacher = new Person
                              {
                                  FullName = "Emily Sparks"
                              };
                teacher.ClearDirtyFlag();
                
                classSubject.Teacher = teacher;
                classSubject.TeacherID = teacher.ID;
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
                    Console.WriteLine("number of pages: {0}", notebook1.Pages.Count);
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