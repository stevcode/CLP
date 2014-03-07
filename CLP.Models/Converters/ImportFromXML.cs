using System.Xml;
using Catel.IO;

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

            







            return notebook;
        }
    }
}
