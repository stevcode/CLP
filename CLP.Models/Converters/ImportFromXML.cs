using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
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

            var notebookPages = new List<ICLPPage>();
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

                ACLPPageBase.Deserialize(page);
                notebookPages.Add(page);  //files read in alphbetical order: page 1, page 10, page 2, page 3, etc...
            }

            notebookPages = notebookPages.OrderBy(x => x.PageIndex).ToList();
            foreach(var page in notebookPages)
            {
                notebook.AddPage(page);
            }

            var submissionXMLFiles = from file in Directory.EnumerateFiles(submissionsXMLFolderPath, "*.xml")
                                     where !file.ToLower().Contains("history")
                                     select file;

            foreach(var submissionXMLFilePath in submissionXMLFiles)
            {
                ICLPPage submission = null;

                var reader = new XmlTextReader(submissionXMLFilePath)
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
                                    submission = new CLPPage(submissionXMLFilePath);
                                    reader.Close();
                                    break;
                                case "CLPAnimationPage":
                                    submission = new CLPAnimationPage(submissionXMLFilePath);
                                    reader.Close();
                                    break;
                            }
                            break;
                    }
                }

                if(submission == null)
                {
                    Console.WriteLine("Failed to convert submission to XML");
                    continue;
                }

                ACLPPageBase.Deserialize(submission);
                notebook.Submissions[submission.UniqueID].Add(submission);
            }

            notebook.MirrorDisplay.DisplayPageIDs.Add(notebook.Pages.First().UniqueID);
            notebook.InitializeAfterDeserialize();

            return notebook;
        }

        public static StrokeDTO ParseStroke(XmlReader reader)
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

           // var strokePointsString = Regex.Replace(reader.ReadElementContentAsString(), @"\{|\}|\s*", string.Empty);
            var strokePointsString = reader.ReadElementContentAsString();
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

        public static ICLPPageObject ParsePageObject(XmlTextReader reader, ICLPPage page)
        {
            ICLPPageObject pageObject = null;

            var pageObjectType = reader.GetAttribute("Type");

            switch(pageObjectType)
            {
                case "CLPTextBox":
                    pageObject = new CLPTextBox(page);
                    break;
                case "CLPArray":
                    pageObject = new CLPArray(0, 0, page);
                    break;
                case "CLPShape":
                    pageObject = new CLPShape(CLPShape.CLPShapeType.Ellipse, page);
                    break;
            }

            if(pageObject == null)
            {
                return null;
            }

            pageObject.ParentPageID = reader.GetAttribute("ParentPageID");
            pageObject.ParentID = reader.GetAttribute("ParentID");
            pageObject.CreationDate = Convert.ToDateTime(reader.GetAttribute("CreationDate"));
            pageObject.UniqueID = reader.GetAttribute("UniqueID");
            pageObject.CanAcceptStrokes = Convert.ToBoolean(reader.GetAttribute("CanAcceptStrokes"));
            pageObject.CanAcceptPageObjects = Convert.ToBoolean(reader.GetAttribute("CanAcceptPageObjects"));
            pageObject.XPosition = Convert.ToDouble(reader.GetAttribute("XPosition"));
            pageObject.YPosition = Convert.ToDouble(reader.GetAttribute("YPosition"));
            pageObject.Height = Convert.ToDouble(reader.GetAttribute("Height"));
            pageObject.Width = Convert.ToDouble(reader.GetAttribute("Width"));
            pageObject.IsBackground = Convert.ToBoolean(reader.GetAttribute("IsBackground"));
            pageObject.CanAdornersShow = Convert.ToBoolean(reader.GetAttribute("CanAdornersShow"));
            pageObject.Parts = Convert.ToInt32(reader.GetAttribute("Parts"));
            pageObject.IsInternalPageObject = Convert.ToBoolean(reader.GetAttribute("IsInternalPageObject"));

            reader.Read();
            reader.MoveToContent();
            if(!reader.IsEmptyElement)
            {
                var pageObjectStrokeParentIDs = reader.ReadElementContentAsString();
                foreach(var pageObjectStrokeParentID in pageObjectStrokeParentIDs.Split(','))
                {
                    pageObject.PageObjectStrokeParentIDs.Add(pageObjectStrokeParentID);
                }
            }
            else
            {
                reader.Read();
            }

            reader.MoveToContent();
            if(!reader.IsEmptyElement)
            {
                var pageObjectObjectParentIDs = reader.ReadElementContentAsString();
                foreach(var pageObjectObjectParentID in pageObjectObjectParentIDs.Split(','))
                {
                    pageObject.PageObjectObjectParentIDs.Add(pageObjectObjectParentID);
                }
            }
            else
            {
                reader.Read();
            }

            reader.MoveToContent();

            switch(pageObjectType)
            {
                case "CLPTextBox":
                    (pageObject as CLPTextBox).Text = reader.ReadElementContentAsString();
                    break;
                case "CLPArray":
                    var array = pageObject as CLPArray;

                    array.IsGridOn = Convert.ToBoolean(reader.ReadElementContentAsString());

                    reader.MoveToContent();
                    array.IsDivisionBehaviorOn = Convert.ToBoolean(reader.ReadElementContentAsString());

                    reader.MoveToContent();
                    array.ArrayHeight = reader.ReadElementContentAsDouble();

                    reader.MoveToContent();
                    array.ArrayWidth = reader.ReadElementContentAsDouble();

                    reader.MoveToContent();
                    array.Rows = reader.ReadElementContentAsInt();

                    reader.MoveToContent();
                    array.Columns = reader.ReadElementContentAsInt();

                    reader.MoveToContent();
                    if(!reader.IsEmptyElement)
                    {
                        var horizontalGridLinePositions = reader.ReadElementContentAsString();
                        foreach(var position in horizontalGridLinePositions.Split(','))
                        {
                            array.HorizontalGridLines.Add(Convert.ToDouble(position));
                        }
                    }
                    else
                    {
                        reader.Read();
                    }

                    reader.MoveToContent();
                    if(!reader.IsEmptyElement)
                    {
                        var verticalGridLinePositions = reader.ReadElementContentAsString();
                        foreach(var position in verticalGridLinePositions.Split(','))
                        {
                            array.VerticalGridLines.Add(Convert.ToDouble(position));
                        }
                    }
                    else
                    {
                        reader.Read();
                    }

                    reader.MoveToContent();
                    if(!reader.IsEmptyElement)
                    {
                        //horiz divisions
                    }
                    else
                    {
                        reader.Read();
                    }

                    reader.MoveToContent();
                    if(!reader.IsEmptyElement)
                    {
                        //vert divisions
                    }
                    else
                    {
                        reader.Read();
                    }
                    break;
                case "CLPShape":
                    var shapeType = reader.ReadElementContentAsString();
                    switch(shapeType)
                    {
                        case "Rectangle":
                            (pageObject as CLPShape).ShapeType = CLPShape.CLPShapeType.Rectangle;
                            break;
                        case "Ellipse":
                            (pageObject as CLPShape).ShapeType = CLPShape.CLPShapeType.Ellipse;
                            break;
                        case "Triangle":
                            (pageObject as CLPShape).ShapeType = CLPShape.CLPShapeType.Triangle;
                            break;
                        case "HorizontalLine":
                            (pageObject as CLPShape).ShapeType = CLPShape.CLPShapeType.HorizontalLine;
                            break;
                        case "VerticalLine":
                            (pageObject as CLPShape).ShapeType = CLPShape.CLPShapeType.VerticalLine;
                            break;
                        case "Protractor":
                            (pageObject as CLPShape).ShapeType = CLPShape.CLPShapeType.Protractor;
                            break;
                        default:
                            Console.WriteLine("Unknown Shape Type for CLPShape, defaulting to Rectangle.");
                            (pageObject as CLPShape).ShapeType = CLPShape.CLPShapeType.Rectangle;
                            break;
                    }
                    break;
            }

            return pageObject;
        }

        public static ICLPHistoryItem ParseHistoryItem(XmlTextReader reader, ICLPPage page)
        {
            ICLPHistoryItem historyItem = null;
            
            var historyItemType = reader.GetAttribute("Type");

            switch(historyItemType)
            {
                case "HistoryStrokesChanged":
                {
                    reader.Read();
                    reader.MoveToContent();
                    var strokeIdsAdded = new List<string>();
                    if(!reader.IsEmptyElement)
                    {
                        var strokeIDsAddedReader = reader.ReadSubtree();
                        strokeIDsAddedReader.Read();
                        strokeIDsAddedReader.MoveToContent();
                        while(strokeIDsAddedReader.Read())
                        {
                            if(reader.NodeType != XmlNodeType.Element)
                            {
                                continue;
                            }

                            if(reader.Name != "StrokeID")
                            {
                                continue;
                            }
                            strokeIdsAdded.Add(strokeIDsAddedReader.GetAttribute("ID"));
                        }
                    }

                    reader.Read();
                    reader.MoveToContent();
                    var strokesAdded = new List<StrokeDTO>();
                    if(!reader.IsEmptyElement)
                    {
                        var strokesAddedReader = reader.ReadSubtree();
                        strokesAddedReader.Read();
                        strokesAddedReader.MoveToContent();
                        while(strokesAddedReader.Read())
                        {
                            if(reader.NodeType != XmlNodeType.Element)
                            {
                                continue;
                            }

                            if(reader.Name != "Stroke")
                            {
                                continue;
                            }

                            var stroke = ParseStroke(strokesAddedReader);
                            strokesAdded.Add(stroke);
                        }
                    }

                    reader.Read();
                    reader.MoveToContent();
                    var strokeIdsRemoved = new List<string>();
                    if(!reader.IsEmptyElement)
                    {
                        var strokeIDsRemovedReader = reader.ReadSubtree();
                        strokeIDsRemovedReader.Read();
                        strokeIDsRemovedReader.MoveToContent();
                        while(strokeIDsRemovedReader.Read())
                        {
                            if(reader.NodeType != XmlNodeType.Element)
                            {
                                continue;
                            }

                            if(reader.Name != "StrokeID")
                            {
                                continue;
                            }
                            strokeIdsRemoved.Add(strokeIDsRemovedReader.GetAttribute("ID"));
                        }
                    }

                    reader.Read();
                    reader.MoveToContent();
                    var strokesRemoved = new List<StrokeDTO>();
                    if(!reader.IsEmptyElement)
                    {
                        var strokesRemovedReader = reader.ReadSubtree();
                        strokesRemovedReader.Read();
                        strokesRemovedReader.MoveToContent();
                        while(strokesRemovedReader.Read())
                        {
                            if(reader.NodeType != XmlNodeType.Element)
                            {
                                continue;
                            }

                            if(reader.Name != "Stroke")
                            {
                                continue;
                            }

                            var stroke = ParseStroke(strokesRemovedReader);
                            strokesRemoved.Add(stroke);
                        }
                    }

                    historyItem = new CLPHistoryStrokesChanged(page, strokeIdsAdded, new List<Stroke>());
                    (historyItem as CLPHistoryStrokesChanged).StrokeIDsRemoved = strokeIdsRemoved;
                    (historyItem as CLPHistoryStrokesChanged).SerializedStrokesAdded = strokesAdded;
                    (historyItem as CLPHistoryStrokesChanged).SerializedStrokesRemoved = strokesRemoved;
                }
                    break;
                case "HistoryPageObjectAdd":
                {
                    var uniqueID = reader.GetAttribute("PageObjectUniqueID");
                    historyItem = new CLPHistoryPageObjectAdd(page, uniqueID, 0);

                    reader.Read();
                    reader.MoveToContent();

                    if(!reader.IsEmptyElement)
                    {
                        reader.Read();
                        reader.MoveToContent();
                        (historyItem as CLPHistoryPageObjectAdd).PageObject = ParsePageObject(reader, page);;
                    }
                }
                    break;
                case "HistoryPageObjectRemove":
                {
                    var uniqueID = reader.GetAttribute("PageObjectUniqueID");

                    reader.Read();
                    reader.MoveToContent();

                    ICLPPageObject pageObject = null;
                    if(!reader.IsEmptyElement)
                    {
                        reader.Read();
                        reader.MoveToContent();
                        pageObject = ParsePageObject(reader, page);
                    }

                    historyItem = new CLPHistoryPageObjectRemove(page, uniqueID, 0);
                    (historyItem as CLPHistoryPageObjectRemove).PageObject = pageObject;
                }
                    break;
                case "HistoryPageObjectResizeBatch":
                {
                    var uniqueID = reader.GetAttribute("PageObjectUniqueID");
                    var currentBatchTickIndex = Convert.ToInt32(reader.GetAttribute("CurrentBatchTickIndex"));

                    reader.Read();
                    reader.MoveToContent();
                    var dimensionsReader = reader.ReadSubtree();
                    dimensionsReader.Read();
                    dimensionsReader.MoveToContent();
                    var stretchedDimensions = new ObservableCollection<Point>();
                    while(dimensionsReader.Read())
                    {
                        if(reader.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }

                        if(reader.Name != "Point")
                        {
                            continue;
                        }

                        var x = Convert.ToDouble(dimensionsReader.GetAttribute("X"));
                        var y = Convert.ToDouble(dimensionsReader.GetAttribute("Y"));
                        stretchedDimensions.Add(new Point(x,y));
                    }

                    historyItem = new CLPHistoryPageObjectResizeBatch(page, uniqueID, new Point(0,0));
                    (historyItem as CLPHistoryPageObjectResizeBatch).StretchedDimensions = stretchedDimensions;
                    (historyItem as IHistoryBatch).CurrentBatchTickIndex = currentBatchTickIndex;
                }
                    break;
                case "HistoryPageObjectMoveBatch":
                {
                    var uniqueID = reader.GetAttribute("PageObjectUniqueID");
                    var currentBatchTickIndex = Convert.ToInt32(reader.GetAttribute("CurrentBatchTickIndex"));

                    reader.Read();
                    reader.MoveToContent();
                    var positionsReader = reader.ReadSubtree();
                    positionsReader.Read();
                    positionsReader.MoveToContent();
                    var travelledPositions = new List<Point>();
                    while(positionsReader.Read())
                    {
                        if(reader.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }

                        if(reader.Name != "Point")
                        {
                            continue;
                        }

                        var x = Convert.ToDouble(positionsReader.GetAttribute("X"));
                        var y = Convert.ToDouble(positionsReader.GetAttribute("Y"));
                        travelledPositions.Add(new Point(x,y));
                    }

                    historyItem = new CLPHistoryPageObjectMoveBatch(page, uniqueID, new Point(0,0));
                    (historyItem as CLPHistoryPageObjectMoveBatch).TravelledPositions = travelledPositions;
                    (historyItem as IHistoryBatch).CurrentBatchTickIndex = currentBatchTickIndex;
                }
                    break;
            }

            return historyItem;
        }
    }
}
