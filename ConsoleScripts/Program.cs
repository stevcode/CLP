using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using Catel.Collections;
using Catel.Data;
using CLP.Entities;
using Path = Catel.IO.Path;

namespace ConsoleScripts
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Convert();
            Debug.WriteLine("*****Finished*****");
            Console.ReadLine();
        }

        private static void Convert()
        {
            var convertFromFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Convert");
            //var convertToFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Converted");
            //if (!Directory.Exists(convertToFolderPath))
            //{
            //    Directory.CreateDirectory(convertToFolderPath);
            //}

            var notebookFolderPaths = Directory.EnumerateDirectories(convertFromFolderPath);
            foreach (var notebookFolderPath in notebookFolderPaths)
            {
                //var filePath = Path.Combine(notebookFolderPath, "notebook.xml");
                //var notebook = ModelBase.Load<Notebook>(filePath, SerializationMode.Xml);
                var pagesFolderPath = Path.Combine(notebookFolderPath, "Pages");
                var pageFilePaths = Directory.EnumerateFiles(pagesFolderPath, "*.xml");
                foreach (var pageFilePath in pageFilePaths)
                {
                    //var page = AEntityBase.Load<CLPPage>(pageFilePath, SerializationMode.Xml);
                    ////page.AfterDeserialization();

                    //Debug.WriteLine("Loaded {3}'s page {0}, differentiation {1}, version {2}", page.PageNumber, page.DifferentiationLevel, page.VersionIndex, page.Owner.FullName);
                    ////Do stuff to each page here. 

               

                    ////Finished doing stuff to page, it'll save below.
                    //page.ToXML(pageFilePath, true);
                }
            }
        }
    }
}