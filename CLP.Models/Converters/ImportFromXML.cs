using System.Xml;

namespace CLP.Models.Converters
{
    public static class ImportFromXML
    {
        public static CLPNotebook ImportNotebook(string initialFilePath)
        {
            var notebook = new CLPNotebook();

            var reader = new XmlTextReader(initialFilePath);

            while(reader.Read())
            {
                
            }







            return notebook;
        }
    }
}
