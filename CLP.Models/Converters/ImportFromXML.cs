using System;
using System.IO;
using System.Linq;
using System.Xml;
using Path = Catel.IO.Path;

namespace CLP.Models.Converters
{
    public static class ImportFromXML
    {
        public static CLPNotebook ImportNotebook(string rootXMLFolderPath)
        {
            var pagesXMLFolderPath = Path.Combine(rootXMLFolderPath, "Pages");
            var submissionsXMLFolderPath = Path.Combine(rootXMLFolderPath, "Submissions");
            var notebookXMLFilePath = Path.Combine(rootXMLFolderPath, "Notebook.xml");

            var notebook = new CLPNotebook(notebookXMLFilePath);
            notebook.Pages.Clear();

            var pageXMLFiles = from file in Directory.EnumerateFiles(pagesXMLFolderPath, "*.xml")
                               where !file.ToLower().Contains("history")
                               select file;

            foreach(var pageXMLFilePath in pageXMLFiles)
            {
                ICLPPage page = null;

                var reader = new XmlTextReader(pageXMLFilePath);

                while(reader.Read())
                {
                    if(reader.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    switch(reader.Name)
                    {
                        case "Page":
                            var pageType = reader.GetAttribute("PageType");
                            if(pageType == "CLPPage")
                            {
                                page = new CLPPage(pageXMLFilePath);
                                reader.Close();
                            }
                            else if(pageType == "CLPAnimationPage")
                            {
                                page = new CLPAnimationPage(pageXMLFilePath);
                                reader.Close();
                            }
                            break;
                    }
                }

                if(page == null)
                {
                    Console.WriteLine("Failed to convert page to XML");
                    continue;
                }

                notebook.AddPage(page);
            }





            notebook.MirrorDisplay.AddPageToDisplay(notebook.Pages.First());

            return notebook;
        }
    }
}
