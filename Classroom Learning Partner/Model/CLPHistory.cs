﻿
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using Classroom_Learning_Partner.ViewModels;
using Catel.Data;
using System.Runtime.Serialization;
using System.Windows;

namespace Classroom_Learning_Partner.Model
{
    /// <summary>
    /// CLPHistory Data object class which fully supports serialization, property changed notifications,
    /// backwards compatibility and error checking.
    /// </summary>
    [Serializable]
    public class CLPHistory : DataObjectBase<CLPHistory>
    {
        public const double SAMPLE_TIME = 100.0;

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPHistory()
        {
            HistoryItems = new ObservableCollection<CLPHistoryItem>();
            UndoneHistoryItems = new ObservableCollection<CLPHistoryItem>();
            TrashedPageObjects = new Dictionary<string, ICLPPageObject>();
            TrashedInkStrokes = new Dictionary<string, string>();
            IgnoreHistory = false;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHistory(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public bool IgnoreHistory
        {
            get { return GetValue<bool>(IgnoreHistoryProperty); }
            set { SetValue(IgnoreHistoryProperty, value); }
        }

        /// <summary>
        /// Register the IgnoreHistory property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IgnoreHistoryProperty = RegisterProperty("IgnoreHistory", typeof(bool), false);

        /// <summary>
        /// List of history items.
        /// </summary>
        public ObservableCollection<CLPHistoryItem> HistoryItems
        {
            get { return GetValue<ObservableCollection<CLPHistoryItem>>(HistoryItemsProperty); }
            set { SetValue(HistoryItemsProperty, value); }
        }

        /// <summary>
        /// Register the HistoryItems property so it is known in the class.
        /// </summary>
        public static readonly PropertyData HistoryItemsProperty = RegisterProperty("HistoryItems", typeof(ObservableCollection<CLPHistoryItem>), new ObservableCollection<CLPHistoryItem>());

        /// <summary>
        /// List to enable undo/redo functionality.
        /// </summary>
        public ObservableCollection<CLPHistoryItem> UndoneHistoryItems
        {
            get { return GetValue<ObservableCollection<CLPHistoryItem>>(UndoneHistoryItemsProperty); }
            set { SetValue(UndoneHistoryItemsProperty, value); }
        }

        /// <summary>
        /// Register the UndoneHistoryItems property so it is known in the class.
        /// </summary>
        public static readonly PropertyData UndoneHistoryItemsProperty = RegisterProperty("UndoneHistoryItems", typeof(ObservableCollection<CLPHistoryItem>), new ObservableCollection<CLPHistoryItem>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Dictionary<string, ICLPPageObject> TrashedPageObjects
        {
            get { return GetValue<Dictionary<string, ICLPPageObject>>(TrashedPageObjectsProperty); }
            set { SetValue(TrashedPageObjectsProperty, value); }
        }

        /// <summary>
        /// Register the TrashedObjects property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TrashedPageObjectsProperty = RegisterProperty("TrashedPageObjects", typeof(Dictionary<string, ICLPPageObject>), new Dictionary<string, ICLPPageObject>());

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public Dictionary<string, string> TrashedInkStrokes
        {
            get { return GetValue<Dictionary<string, string>>(TrashedInkStrokesProperty); }
            set { SetValue(TrashedInkStrokesProperty, value); }
        }

        /// <summary>
        /// Register the TrashedInkStrokes property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TrashedInkStrokesProperty = RegisterProperty("TrashedInkStrokes", typeof(Dictionary<string, string>), new Dictionary<string, string>());

        #endregion

        #region Methods
        //because the historyItems collection has a private set accessor
        public static void ReplaceHistoryItems(CLPHistory oldHistory, CLPHistory newHistory)
        {
            oldHistory.HistoryItems = newHistory.HistoryItems;
            return;
        }
        public static CLPHistory GenerateHistorySinceLastSubmission(CLPPage page)
        {
            CLPHistory historySubset = GetSegmentedHistory(page);
            bool start = true;
            foreach (CLPHistoryItem item in page.PageHistory.HistoryItems)
            {
                if (item.ItemType == HistoryItemType.Submit)
                {
                    start = true;
                }
                if (start)
                {
                    historySubset.HistoryItems.Clear();
                    historySubset.UndoneHistoryItems.Clear();
                    historySubset.TrashedPageObjects.Clear();
                    historySubset.TrashedInkStrokes.Clear();
                    start = false;
                }
                historySubset.HistoryItems.Add(item);
                if (item.ItemType == HistoryItemType.EraseInk) 
                {
                    if(page.PageHistory.TrashedInkStrokes.ContainsKey(item.ObjectID))
                    {
                        historySubset.TrashedInkStrokes.Add(item.ObjectID, page.PageHistory.TrashedInkStrokes[item.ObjectID]);
                    }
                    foreach (Stroke s in page.InkStrokes)
                    {
                        if (s.GetPropertyData(CLPPage.StrokeIDKey).ToString() == item.ObjectID)
                        {
                            page.InkStrokes.Remove(s);
                            historySubset.TrashedInkStrokes.Add(s.GetPropertyData(CLPPage.StrokeIDKey).ToString(), CLPPage.StrokeToString(s));
                            break;
                        }
                    }
                }
                else if (item.ItemType == HistoryItemType.RemovePageObject)
                {
                    if (page.PageHistory.TrashedPageObjects.ContainsKey(item.ObjectID))
                    {
                       historySubset.TrashedPageObjects.Add(item.ObjectID, page.PageHistory.TrashedPageObjects[item.ObjectID]);
                        break;
                    }
                    foreach (var pageObject in page.PageObjects)
                    {
                        if (pageObject.UniqueID == item.ObjectID)
                        {
                            historySubset.TrashedPageObjects.Add(item.ObjectID, pageObject);
                            break;
                        }
                    }
                
                }
            }
            return historySubset;
        }

        public static ObservableCollection<ICLPPageObject> PageObjectsSinceLastSubmission(CLPPage page, CLPHistory history)
        {
            ObservableCollection<ICLPPageObject> pageObjects = new ObservableCollection<ICLPPageObject>();
            foreach (CLPHistoryItem item in history.HistoryItems)
            {
                if (item.ItemType == HistoryItemType.AddPageObject)
                {
                    foreach (var pageObject in page.PageObjects)
                    {
                        if (pageObject.UniqueID == item.ObjectID)
                        {
                            pageObjects.Add(pageObject);
                            break;
                        }
                    }
                }
            }
            return pageObjects;
        }
        public void ClearHistory()
        {
            HistoryItems.Clear();
            UndoneHistoryItems.Clear();
            TrashedPageObjects.Clear();
            //should we clear the trashed inkstrokes too? -claire
            TrashedInkStrokes.Clear();
        }
        //IsSaved == true means that the history has not been updated since the last save
        //(to be used by Jessie- DB stuff)
        //dirty data returns false, pages 
        public bool IsSaved()
        {
            if (HistoryItems.Count > 0)
            {
                return HistoryItems[HistoryItems.Count - 1].ItemType == HistoryItemType.Save;  

            }
            else
            {
                //Logger.Instance.WriteToLog("Zero history items");
                return true;
            }
        }


        public static CLPHistory InterpolateHistory(CLPHistory history)
        {
            CLPHistory newHistory = new CLPHistory();
            for (int i = 0; i < history.HistoryItems.Count; i++)
            {
                CLPHistoryItem item = history.HistoryItems[i];
                if (((item.ItemType == HistoryItemType.MovePageObject && history.HistoryItems.Count - 1 > i && history.HistoryItems[i + 1].ItemType == HistoryItemType.MovePageObject) && item.ObjectID == history.HistoryItems[i + 1].ObjectID)) //|| (item.ItemType == "RESIZE" && history.HistoryItems.Count - 1 > i && history.HistoryItems[i + 1].ItemType == "RESIZE")) && item.ObjectID == history.HistoryItems[i + 1].ObjectID)
                {
                    newHistory.HistoryItems.Add(item);
                    //get dx and dy
                    //Logger.Instance.WriteToLog("Interpolated Endpt: " + Point.Parse(item.NewValue).X + ", " + Point.Parse(item.NewValue).Y + " " + item.MetaData.GetValue("CreationDate"));
                    double dx = Point.Parse(history.HistoryItems[i + 1].NewValue).X - Point.Parse(item.NewValue).X;
                    double dy = Point.Parse(history.HistoryItems[i + 1].NewValue).Y - Point.Parse(item.NewValue).Y;
                    //dist is the distance between 2 points 
                    double dist = Math.Sqrt((dx * dx) + (dy * dy));
                    DateTime t0 = item.CreationDate;
                    DateTime t1 = history.HistoryItems[i + 1].CreationDate;
                    double timeDiff = (t1 - t0).TotalMilliseconds;

                    if (timeDiff > SAMPLE_TIME)
                    {
                        int numNewPoints = (int)Math.Floor(timeDiff / SAMPLE_TIME);
                        double[,] values = new double[2, 2];
                        values[0, 0] = Point.Parse(item.NewValue).X;
                        values[0, 1] = Point.Parse(item.NewValue).Y;
                        values[1, 0] = Point.Parse(history.HistoryItems[i + 1].NewValue).X;
                        values[1, 1] = Point.Parse(history.HistoryItems[i + 1].NewValue).Y;
                        Tuple<double, double> fitLine = regress(values);
                        double slope = fitLine.Item1;
                        double intercept = fitLine.Item2;
                        double section = dist / numNewPoints;

                        //use quadratic eqn to get x displacement
                        /* double a = 1 + slope * slope;
                         double b = 2 * slope * intercept;
                         double c = intercept * intercept - section * section;
                         double d = b * b - 4 * a * c;
                         double x1 = 0;
                         double x2 = 0;
                         if (d == 0) // If the discriminant is 0, both solutions are equal.
                         {
                             x1 = x2 = -b / (2 * a);
                         }
                         else if (d < 0) // If the discriminant is negative, there are no solutions.
                         {
                             Logger.Instance.WriteToLog("No solutions for the equation to add history points.");
                            
                         }
                         else // In other cases the discriminant is positive, so there are two different solutions.
                         {
                             x1 = (-b - Math.Sqrt(d)) / (2 * a);
                             x2 = (-b + Math.Sqrt(d)) / (2 * a);
                         }*/
                        double theta = Math.Atan2(dy, dx);

                        for (int j = 1; j < numNewPoints; j++)
                        {
                            //double newX = Point.Parse(item.NewValue).X + x1;
                            //double newY = slope * newX + intercept;
                            double newdx = Math.Cos(theta) * (section * j);
                            double newdy = Math.Sin(theta) * (section * j);
                            double newX = Point.Parse(item.NewValue).X + newdx;
                            double newY = Point.Parse(item.NewValue).Y + newdy;
                            CLPHistoryItem newItem = new CLPHistoryItem(item.ItemType, item.ObjectID, newHistory.HistoryItems[newHistory.HistoryItems.Count - 1].NewValue, new Point(newX, newY).ToString());
                            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)((SAMPLE_TIME) * j));
                            newItem.CreationDate = item.CreationDate.Add(time);
                            newHistory.HistoryItems.Add(newItem);
                            //Logger.Instance.WriteToLog("Interpolated: " + Point.Parse(newItem.NewValue).X + ", " + Point.Parse(newItem.NewValue).Y + " " + newItem.MetaData.GetValue("CreationDate"));

                        }
                    }
                }
                else
                {
                    newHistory.HistoryItems.Add(item);
                }
            }

            return newHistory;
        }
        public static CLPHistory GetSegmentedHistory(CLPPage page)
        {
            bool resizing = false;
            bool moving = false;
            //int total = HistoryItems.Count;
            CLPHistory smallerHistory = new CLPHistory();
            //smallerHistory.ObjectReferences = History.ObjectReferences;
            smallerHistory.TrashedInkStrokes = page.PageHistory.TrashedInkStrokes;
            smallerHistory.TrashedPageObjects = page.PageHistory.TrashedPageObjects;
            int resizeCount = 0;
            int fullNumHistoryItems = page.PageHistory.HistoryItems.Count;
            double[] resizeX = new double[fullNumHistoryItems];
            double[] resizeY = new double[fullNumHistoryItems];
            DateTime[] resizeT = new DateTime[fullNumHistoryItems];
            CLPHistoryItem[] resizeItems = new CLPHistoryItem[fullNumHistoryItems];
            foreach (var item in page.PageHistory.HistoryItems)
            {


                /* if (item.ItemType == "RESIZE")
                 {
                     //not supporting this right now, the kids aren't doing it in lessons
                    
                     moving = false;
                     resizing = true;
                     resizeT[resizeCount] = DateTime.Parse(item.MetaData.GetValue("CreationDate"));
                     int indexOfComma = item.NewValue.IndexOf(',');
                     resizeY[resizeCount] = double.Parse(item.NewValue.Substring(indexOfComma + 2));
                     resizeX[resizeCount] = double.Parse(item.NewValue.Substring(0, item.NewValue.Length - indexOfComma));
                    // Logger.Instance.WriteToLog("Full: " + Point.Parse(item.NewValue).X + ", " +Point.Parse(item.NewValue).Y + " " + item.MetaData.GetValue("CreationDate"));
                     resizeItems[resizeCount] = item;
                    if(resizing == true)
                    {
                             int index = History.HistoryItems.IndexOf(item);
                             resizeCount++;
                            
                                 if (index == History.HistoryItems.Count-1 || (item.ObjectID != History.HistoryItems.ElementAt<CLPHistoryItem>(index + 1).ObjectID) || (HistoryItems.Count > index + 1 && History.HistoryItems.ElementAt<CLPHistoryItem>(index + 1).ItemType != "MOVE"))
                                 {
                                     moving = false;
                                    
                                     int[] indices = History.segmentPath(resizeX, resizeY, resizeT);
                                     for (int i = 0; i < indices.Length; i++)
                                     {
                                         smallerHistory.AddHistoryItem(resizeItems[indices[i]]);
                                         CLPHistoryItem add = resizeItems[indices[i]];
                                         //Logger.Instance.WriteToLog("Resize Segment: " + Point.Parse(add.NewValue).X + ", " + Point.Parse(add.NewValue).Y + " " + add.MetaData.GetValue("CreationDate"));
                    
                                        /* CLPHistoryItem it = new CLPHistoryItem("ADD");
                                         it.ObjectID = resizeItems[indices[i]].ObjectID;
                                         it.NewValue = resizeItems[indices[i]].NewValue;
                                         //item.MetaData = resizeItems[indices[i]].MetaData;
                                         smallerHistory.AddHistoryItem(it);
                                         */
                /*
                                    }
                                    resizeCount = 0;
                                    Array.Clear(resizeT, 0, resizeT.Length);
                                    Array.Clear(resizeX, 0, resizeX.Length);
                                    Array.Clear(resizeY, 0, resizeY.Length);
                                }
                            
                        
                    }
                    else
                    {
                        
                        resizing = true;
                        resizeCount++;
                    }
                }
                else */
                if (item.ItemType == HistoryItemType.MovePageObject)
                {
                    resizing = false;
                    resizeT[resizeCount] = item.CreationDate;
                    resizeY[resizeCount] = Point.Parse(item.NewValue).Y;
                    resizeX[resizeCount] = Point.Parse(item.NewValue).X;
                    //Logger.Instance.WriteToLog("Full: " + Point.Parse(item.NewValue).X + ", " + Point.Parse(item.NewValue).Y + " " + item.CreationDate);
                    resizeItems[resizeCount] = item;
                    if (moving == true)
                    {
                        int index = page.PageHistory.HistoryItems.IndexOf(item);
                        resizeCount++;

                        if (index == page.PageHistory.HistoryItems.Count - 1 || (item.ObjectID != page.PageHistory.HistoryItems.ElementAt<CLPHistoryItem>(index + 1).ObjectID) || (page.PageHistory.HistoryItems.Count > index + 1 && page.PageHistory.HistoryItems.ElementAt<CLPHistoryItem>(index + 1).ItemType != HistoryItemType.MovePageObject))
                        {
                            moving = false;

                            int[] indices = segmentPath(resizeX, resizeY, resizeT);
                            for (int i = 0; i < indices.Length; i++)
                            {
                                smallerHistory.HistoryItems.Add(resizeItems[indices[i]]);
                                //CLPHistoryItem add = resizeItems[indices[i]];
                                //Logger.Instance.WriteToLog("Segment: " + Point.Parse(add.NewValue).X + ", " + Point.Parse(add.NewValue).Y + " " + add.MetaData.GetValue("CreationDate"));

                                /* CLPHistoryItem it = new CLPHistoryItem("ADD");
                                 it.ObjectID = resizeItems[indices[i]].ObjectID;
                                 it.NewValue = resizeItems[indices[i]].NewValue;
                                 //item.MetaData = resizeItems[indices[i]].MetaData;
                                 smallerHistory.AddHistoryItem(it);
                                 */
                            }
                            resizeCount = 0;
                            Array.Clear(resizeT, 0, resizeT.Length);
                            Array.Clear(resizeX, 0, resizeX.Length);
                            Array.Clear(resizeY, 0, resizeY.Length);
                        }


                    }
                    else
                    {
                        moving = true;
                        resizeCount++;
                    }
                }
                else
                {
                    resizing = false;
                    moving = false;
                    smallerHistory.HistoryItems.Add(item);
                }
            }
            return smallerHistory;
        }
        public static int[] segmentPath(double[] xpoints, double[] ypoints, DateTime[] times)
        {
            int numPoints = 0;
            for (int i = 0; i < times.Length; i++)
            {
                if (times[i] != DateTime.MinValue)
                {
                    numPoints++;
                }
            }
            double[] dx = new double[numPoints - 1];
            double[] dy = new double[numPoints - 1];
            double[] p = new double[numPoints - 1];
            double[,] segPoints = new double[numPoints, 2];
            double[] distance = new double[numPoints];
            double[] speed = new double[numPoints];
            double[] linTan = new double[numPoints];
            double[] arcTan = new double[numPoints];
            double[] bSlope = new double[numPoints];
            double d0, d1;
            DateTime t0, t1;

            double currentDist = 0;
            for (int i = 0; i < numPoints - 1; i++)
            {
                //populate the dx and dy arrays
                dx[i] = xpoints[i + 1] - xpoints[i];
                dy[i] = ypoints[i + 1] - ypoints[i];
                //p[i] is the distance between 2 points 
                p[i] = (dx[i] * dx[i]) + (dy[i] * dy[i]);
                //distance is the total distance of the path so far
                currentDist += p[i];
                distance[i] = currentDist;
                int k;
                if (i < 2)
                {
                    k = 2;
                }
                else
                {
                    k = i;
                }
                if (distance.Length > 2)
                {
                    d0 = distance[k - 2];
                    d1 = distance[k];
                    t0 = times[k - 2];
                    t1 = times[k];

                    double timeDiff = (t1 - t0).TotalMilliseconds;
                    speed[i] = Math.Abs((d1 - d0) / timeDiff);

                }
                else
                {
                    speed[i] = 0;
                }
            } // now the arrays are populated and we can calculate slope, curvature

            double[,] values;
            for (int i = 1; i < numPoints; i++)
            {
                int k = 0;
                int windowSize = 11;
                if (i <= Math.Floor(windowSize / 2.0))
                {
                    if (i == 1)
                    {
                        k = 1;
                    }
                    else
                    {
                        k = i;
                    }
                    windowSize = 2 * k - 1;
                }
                else if (i >= numPoints + Math.Floor(windowSize / 2.0))
                {
                    if (i == numPoints)
                    {
                        k = numPoints - 1;
                    }
                    else
                    {
                        k = i;
                    }
                    windowSize = 2 * (numPoints - k) - 1;
                }
                values = new double[windowSize, 2];
                double winMin = i - Math.Floor(windowSize / 2.0);
                double winMax = i + Math.Floor(windowSize / 2.0);
                if (winMin == 0)
                {
                    winMin = 1;
                }
                int valCount = 0;
                for (int w = (int)winMin + i; w < winMax + i; w++)
                {
                    if (w < numPoints - 1)
                    {
                        //do a least squares regression to get the slope of the best fit line for this segment
                        values[valCount, 0] = xpoints[w];
                        values[valCount, 1] = ypoints[w];
                        valCount++;
                    }
                }

                linTan[i] = regress(values).Item1;
                arcTan[i] = Math.Atan(linTan[i]);
            }

            double[] correctedAngles = correctAngles(arcTan);
            double[] dAngles = new double[numPoints];
            double[] dDistance = new double[numPoints];
            double[] curvature = new double[numPoints];
            double curveSum = 0, speedSum = 0;
            for (int i = 0; i < numPoints; i++)
            {
                int k = i;
                if (i == 0)
                {
                    k = 1;
                }
                dAngles[i] = arcTan[k] - arcTan[k - 1];
                dDistance[i] = distance[k] - distance[k - 1];
                if (i == 0 || i == numPoints - 1)
                {
                    curvature[i] = 0;
                }
                else
                {
                    curvature[i] = dAngles[i] / dDistance[i];
                }
                curveSum += curvature[i];
                speedSum += speed[i];
            }
            double avgCurve = curveSum / numPoints;
            double curveThreshold = .6 * avgCurve;
            double avgSpeed = speedSum / numPoints;
            double speedThreshHigh = .8 * avgSpeed;
            double speedThreshLow = .25 * avgSpeed;
            double[] speedCorners = new double[numPoints];
            double[] curveCorners = new double[numPoints];
            int numSegPoints = 0;
            for (int i = 1; i < numPoints - 2; i++)
            {
                //find local speed minima to detect corners
                if (speed[i] < speedThreshHigh && speed[i] < speed[i + 1] && speed[i] < speed[i - 1])
                {
                    speedCorners[i] = 1;
                }
                //find local curvature maxima to detect corners
                else if (curvature[i] > curveThreshold && curvature[i] > curvature[i + 1] && curvature[i] > curvature[i - 1])
                {
                    if (speed[i] < speedThreshLow)
                    {
                        curveCorners[i] = 1;
                    }
                }
                //check if the corner points are too close together
                //or too close to the ends
                double pauseThreshold = 750; //double milliseconds
                int y = 20;
                if (speedCorners[i] == 1 || curveCorners[i] == 1)
                {
                    if (i > y && i < numPoints - y)
                    {
                        for (int k = i - y; k < i; k++)
                        {
                            DateTime time0 = times[k];
                            DateTime time1 = times[i];
                            double timeDiff = (time1 - time0).TotalMilliseconds;
                            if (curveCorners[k] == 1)
                            {
                                curveCorners[k] = 0;
                            }
                            else if (speedCorners[k] == 1 && timeDiff < pauseThreshold)
                            {
                                speedCorners[k] = 0;
                            }
                        }
                    }
                }

                //create segments from the corner points 
                if (speedCorners[i] == 1 || curveCorners[i] == 1)
                {
                    segPoints[i, 0] = xpoints[i];
                    segPoints[i, 1] = ypoints[i];
                    numSegPoints += 1;
                }
            }

            int[] indices = new int[numSegPoints + 2];
            int count = 0;
            for (int i = 0; i < numPoints; i++)
            {
                if (segPoints[i, 0] != 0 || i == numPoints - 1 || i == 0)
                {
                    indices[count] = i;
                    count++;
                }
            }
            double[] segTypes = new double[numSegPoints];

            return indices;
        }

        private static double[] correctAngles(double[] angles)
        {
            double b = 0;
            double[] corrected = new double[angles.Length];
            for (int i = 1; i < angles.Length; i++)
            {
                double dif = angles[i - 1] - angles[i];
                if (Math.Abs(dif) > 2.5)
                {
                    b = b + dif;
                }
                corrected[i] = b + angles[i];
            }
            return corrected;
        }

        private static Tuple<double, double> regress(double[,] values)
        {
            double xAvg = 0;
            double yAvg = 0;

            for (int x = 0; x < values.GetLength(0); x++)
            {
                xAvg += values[x, 0];
                yAvg += values[x, 1];
            }

            xAvg = xAvg / values.Length;
            yAvg = yAvg / values.Length;

            double v1 = 0;
            double v2 = 0;

            for (int x = 0; x < values.GetLength(0); x++)
            {
                v1 += (values[x, 0] - xAvg) * (values[x, 1] - yAvg);
                v2 += Math.Pow(values[x, 0] - xAvg, 2);
            }

            double a = v1 / v2;
            double b = yAvg - a * xAvg;
            return Tuple.Create<double, double>(a, b);
            //Console.WriteLine("y = ax + b");
            //Console.WriteLine("a = {0}, the slope of the trend line.", Math.Round(a, 2));
            //Console.WriteLine("b = {0}, the intercept of the trend line.", Math.Round(b, 2));

            //Console.ReadLine();
        }

        #endregion
    }

    //[Serializable]
    //public class CLPHistory
    //{
    //    public CLPHistory()
    //    {

    //    }

    //    private MetaDataContainer _metaData = new MetaDataContainer();
    //    public MetaDataContainer MetaData
    //    {
    //        get
    //        {
    //            return _metaData;
    //        }
    //    }

    //    private Dictionary<string, object> _objectReferences = new Dictionary<string, object>();
    //    public Dictionary<string, object> ObjectReferences
    //    {
    //        get
    //        {
    //            return _objectReferences;
    //        }
    //        set 
    //        {
    //            _objectReferences = value;
    //        }
    //    }

    //    private ObservableCollection<CLPHistoryItem> _historyItems = new ObservableCollection<CLPHistoryItem>();
    //    public ObservableCollection<CLPHistoryItem> HistoryItems
    //    {
    //        get
    //        {
    //            return _historyItems;
    //        }

    //    }

    //    //List to enable undo/redo functionality
    //    private ObservableCollection<CLPHistoryItem> _undoneHistoryItems = new ObservableCollection<CLPHistoryItem>();
    //    public ObservableCollection<CLPHistoryItem> UndoneHistoryItems
    //    {
    //        get
    //        {
    //            return _undoneHistoryItems;
    //        }

    //    }

    //    #region Public Methods

    //    public void AddHistoryItem(object obj, CLPHistoryItem historyItem)
    //    {
    //        string uniqueID = null;
    //        if (obj is CLPPageObjectBase)
    //        {
    //            uniqueID = (obj as CLPPageObjectBase).UniqueID;
    //        }
    //        else if (obj is Stroke)
    //        {
    //            uniqueID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
    //        }

    //        if (uniqueID != null && !ObjectReferences.ContainsKey(uniqueID))
    //        {
    //            AddObjectToReferences(uniqueID, obj);
    //        }

    //        historyItem.ObjectID = uniqueID;
    //        _historyItems.Add(historyItem);

    //        System.Console.WriteLine("AddHistoryItem: HistoryItems.Count: " + HistoryItems.Count());
    //        System.Console.WriteLine("ObjectRefIds: " + ObjectReferences.Count());
    //    }
    //    public void AddUndoneHistoryItem(object obj, CLPHistoryItem historyItem)
    //    {
    //        string uniqueID = null;
    //        if (obj is CLPPageObjectBase)
    //        {
    //            uniqueID = (obj as CLPPageObjectBase).UniqueID;
    //        }
    //        else if (obj is Stroke)
    //        {
    //            uniqueID = (obj as Stroke).GetPropertyData(CLPPage.StrokeIDKey) as string;
    //        }

    //        if (uniqueID != null && !ObjectReferences.ContainsKey(uniqueID))
    //        {
    //            AddObjectToReferences(uniqueID, obj);
    //        }

    //        historyItem.ObjectID = uniqueID;
    //        _undoneHistoryItems.Add(historyItem);
    //    }

    //    private void AddObjectToReferences(string key, object obj)
    //    {
    //        if (obj is Stroke)
    //        {
    //            ObjectReferences.Add(key, CLPPageViewModel.StrokeToString(obj as Stroke));
    //        }

    //        else if (obj is CLPPageObjectBase)
    //        {
    //            ObjectReferences.Add(key, obj);
    //        }
    //        else
    //        {
    //            Logger.Instance.WriteToLog("Unknown Object attempted to write to History");
    //        }
    //    }

    //    #endregion //Public Methods

    //    /*   public void add(object obj)
    //    {
    //        CLPHistoryItem item = createHistoryItem(obj);
    //        item.MetaData.Add("ADD",new CLPAttributeValue("ADD","true"));
    //        _historyItems.Add(item);
    //        Console.WriteLine("ADD to History");
    //    }
    //    public void erase(object obj)
    //    {
    //        CLPHistoryItem item = createHistoryItem(obj);
    //        item.MetaData.Add("ERASE", new CLPAttributeValue("ERASE", "true"));
    //        _historyItems.Add(item);
    //    }
    //    public void move(object obj)
    //    {
    //        CLPHistoryItem item = createHistoryItem(obj);
    //        item.MetaData.Add("MOVE", new CLPAttributeValue("MOVE", "true"));
    //        _historyItems.Add(item);
    //    }
    //    public void copy(object obj)
    //    {
    //        CLPHistoryItem item = createHistoryItem(obj);
    //        item.MetaData.Add("COPY", new CLPAttributeValue("COPY", "true"));
    //        _historyItems.Add(item);
    //    }
    //    private CLPHistoryItem createHistoryItem(object obj)
    //    {
    //        int itemID = obj.GetHashCode();
    //        if (!ObjectReferences.ContainsKey(itemID))
    //        {
    //            ObjectReferences.Add(itemID, obj);
    //        }
    //        CLPHistoryItem item = new CLPHistoryItem(itemID);
    //        return item;

    //    }
    //    public void undo(DateTime time)
    //    {
    //        if (HistoryItems.Count <= 0) { return; }
    //        CLPHistoryItem item = HistoryItems.ElementAt(HistoryItems.Count - 1);
    //        HistoryItems.Remove(item);
    //        UndoneHistoryItems.Add(item);
    //        Object obj = ObjectReferences[Convert.ToInt32(item.CLPHistoryObjectReference)];
    //        //Send message to dispatch agent
    //        Console.WriteLine("Undo message received");
    //        return;
    //    }
    //    public void redo()
    //    {
    //        if (UndoneHistoryItems.Count <= 0) { return; }
    //        CLPHistoryItem item = UndoneHistoryItems.ElementAt(UndoneHistoryItems.Count - 1);
    //        UndoneHistoryItems.Remove(item);
    //        HistoryItems.Add(item);
    //        Object obj = ObjectReferences[Convert.ToInt32(item.CLPHistoryObjectReference)];
    //        //Send message to dispatch agent
    //        return;
    //    }
    //  */
    //}
}
