using System;
using System.IO;
using CLP.Models;

namespace ConsoleScripts
{
    class XMLImporter
    {
        public static string _rootXMLFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NotebookXML - Bulk");
        public static string _rootNotebookFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Notebooks - Bulk");
        public static void Import()
        {
            if(!Directory.Exists(_rootNotebookFolderPath))
            {
                Directory.CreateDirectory(_rootNotebookFolderPath);
            }

            foreach(var xmlFolderPath in Directory.EnumerateDirectories(_rootXMLFolderPath))
            {
                Console.WriteLine("Converting Notebook at location: " + xmlFolderPath);
                var notebook = ImportFromXML.ImportNotebook(xmlFolderPath);

                if(notebook == null)
                {
                    continue;
                }

                Console.WriteLine("Notebook Converted");
                var filePath = Path.Combine(_rootNotebookFolderPath, notebook.NotebookName + @".clp");

                var saveTime = DateTime.Now;
                notebook.LastSavedTime = saveTime;
            
                Console.WriteLine("Notebook Saving");
                notebook.Save(filePath);
                Console.WriteLine("Notebook Saved");
            }
        }
    }
}
