using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Ink;
using System.Windows.Media;
using System.Xml;
using Path = Catel.IO.Path;

namespace CLP.Models
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

                var reader = new XmlTextReader(pageXMLFilePath)
                             {
                                 WhitespaceHandling = WhitespaceHandling.None
                             };

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
                            switch(pageType)
                            {
                                case "CLPPage":
                                    page = new CLPPage(pageXMLFilePath);
                                    reader.Close();
                                    break;
                                case "CLPAnimationPage":
                                    page = new CLPAnimationPage(pageXMLFilePath);
                                    reader.Close();
                                    break;
                            }
                            break;
                    }
                }

                if(page == null)
                {
                    Console.WriteLine("Failed to convert page to XML");
                    continue;
                }

                notebook.AddPage(page);  //files read in alphbetical order: page 1, page 10, page 2, page 3, etc...
            }





            notebook.MirrorDisplay.AddPageToDisplay(notebook.Pages.First());

            return notebook;
        }

        public static StrokeDTO ParseStroke(XmlTextReader reader)
        {
            var stroke = new StrokeDTO
                         {
                             StrokeID = reader.GetAttribute("StrokeID"),
                             StrokeOwnerID = reader.GetAttribute("StrokeOwnerID"),
                             StrokeDrawingAttributes = {
                                                           Height = Convert.ToDouble(reader.GetAttribute("Height")),
                                                           Width = Convert.ToDouble(reader.GetAttribute("Width")),
                                                           IsHighlighter = Convert.ToBoolean(reader.GetAttribute("IsHighlighter")),
                                                           FitToCurve = Convert.ToBoolean(reader.GetAttribute("FitToCurve")),
                                                           IgnorePressure = Convert.ToBoolean(reader.GetAttribute("IgnorePressure")),
                                                           StrokeColor = reader.GetAttribute("StrokeColor"),
                                                           StylusTip = reader.GetAttribute("StylusTip") == "Ellipse" ? StylusTip.Ellipse : StylusTip.Rectangle,
                                                           StylusTripTransform = new Matrix()
                                                       }
                         };

            reader.Read();
            reader.MoveToContent();
            reader.ReadElementContentAsString();  //Skip StylusTipTransform for now, it should always be Identity.

            reader.MoveToContent();
            if(reader.Name != "StrokePoints")
            {
                Console.WriteLine("Zero Length Stroke skipped during XML Load");
                return null;
            }

            var strokePointsString = Regex.Replace(reader.ReadElementContentAsString(), @"\{|\}|\s*", string.Empty);
            var pointGroups = strokePointsString.Split(',');
            foreach(var stylusPoint in pointGroups.Select(pointGroup => pointGroup.Split(':')).Select(pointValues => new StylusPointDTO
                                                                                                                     {
                                                                                                                         X = Convert.ToDouble(pointValues[0]),
                                                                                                                         Y = Convert.ToDouble(pointValues[1]),
                                                                                                                         PressureFactor = Convert.ToSingle(pointValues[2])
                                                                                                                     })) {
                                                                                                                         stroke.StrokePoints.Add(stylusPoint);
                                                                                                                     }

            return stroke;
        }
    }
}
