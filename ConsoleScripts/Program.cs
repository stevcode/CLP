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

            ConvertToNewIDs();
        }

        static void ConvertToNewIDs()
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
                var pageAndHistoryFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml");
                var pages = new List<CLPPage>();
                foreach(var pageAndHistoryFilePath in pageAndHistoryFilePaths)
                {
                    var pageAndHistoryFileName = System.IO.Path.GetFileNameWithoutExtension(pageAndHistoryFilePath);
                    if(pageAndHistoryFileName != null)
                    {
                        var pageAndHistoryInfo = pageAndHistoryFileName.Split(';');
                        if(pageAndHistoryInfo.Length != 5 ||
                           pageAndHistoryInfo[0] != "Page")
                        {
                            continue;
                        }
                        if(pageAndHistoryInfo[4] != "0")
                        {
                            continue;
                        }
                    }

                    var page = ModelBase.Load<CLPPage>(pageAndHistoryFilePath, SerializationMode.Xml);
                    pages.Add(page);
                }

                var notebookPages = new List<CLPPage>();

                foreach(var notebookPage in pages)
                {
                    if(notebookPage.VersionIndex != 0)
                    {
                        continue;
                    }
                    notebookPages.Add(notebookPage);
                }

                notebook.Pages = new ObservableCollection<CLPPage>(notebookPages.OrderBy(x => x.PageNumber));

                var compactOwnerID = new Guid(notebook.OwnerID).ToCompactID();
                notebook.Owner.ID = compactOwnerID;
                notebook.OwnerID = compactOwnerID;
                notebook.ID = new Guid(notebook.ID).ToCompactID();

                foreach(var page in notebook.Pages)
                {
                    page.ID = new Guid(page.ID).ToCompactID();
                    page.OwnerID = compactOwnerID;
                    page.Owner.ID = compactOwnerID;

                    foreach(var pageObject in page.PageObjects)
                    {
                        pageObject.ID = new Guid(pageObject.ID).ToCompactID();
                        pageObject.ParentPage = page;
                        if(pageObject is Shape ||
                           pageObject is CLPTextBox)
                        {
                            pageObject.OwnerID = Person.Author.ID;
                            pageObject.CreatorID = Person.Author.ID;
                        }
                        else
                        {
                            pageObject.OwnerID = compactOwnerID;
                            pageObject.CreatorID = compactOwnerID;
                        }

                    }

                    foreach(var stroke in page.SerializedStrokes)
                    {
                        stroke.ID = new Guid(stroke.ID).ToCompactID();
                        stroke.PersonID = compactOwnerID;
                    }

                    page.InkStrokes = StrokeDTO.LoadInkStrokes(page.SerializedStrokes);
                }
                notebook.CurrentPage = notebook.Pages.First();
                var folderPath = Path.Combine(convertToFolderPath, notebook.Name + ";" + notebook.ID + ";" + notebook.Owner.FullName + ";" + notebook.OwnerID);
                notebook.SaveNotebook(folderPath);
            }
        }

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