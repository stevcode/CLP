using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using Microsoft.Ink;

namespace CLP.Models
{
    public enum CLPHandwritingAnalysisType 
    {
        DEFAULT,
        NUMBER,
        DIGIT,
        WORDS
    };

    public static class InkInterpretation
    {

        public static string InterpretHandwriting(StrokeCollection strokes, CLPHandwritingAnalysisType type)
        {
            //ZC look over this code
            string result = null;
            InkAnalyzer analyzer = new InkAnalyzer();
            AnalysisHintNode hint = analyzer.CreateAnalysisHint();
            hint.Location.MakeInfinite();
            if (type != CLPHandwritingAnalysisType.DEFAULT)
            {
                switch (type)
                {
                    case CLPHandwritingAnalysisType.NUMBER:
                        // Number sentence or number
                        hint.Factoid = Factoid.Number;
                        break;
                    case CLPHandwritingAnalysisType.DIGIT:
                        // Digit
                        hint.Factoid = Factoid.Digit;
                        break;
                    case CLPHandwritingAnalysisType.WORDS:
                        // Words
                        hint.Factoid = Factoid.SystemDictionary;
                        break;
                    default:
                        Console.WriteLine("Not a valid ink analysis type");
                        break;
                }
                hint.CoerceToFactoid = true;
            }

            if (strokes.Count > 0)
            {
                analyzer.AddStrokes(strokes);
                if (analyzer.Analyze().Successful)
                {
                    result = analyzer.GetRecognizedString();
                }
            }

            analyzer.Dispose();
            return result;
        }

        public static int[,] InterpretShading(StrokeCollection strokes, double width, double height, double spacing)
        {
            int rows = (int)Math.Floor(height / spacing);
            int cols = (int)Math.Floor(width / spacing);

            int[,] discretization_result = new int[cols,rows];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    discretization_result[j, i] = 0;
                }
            }

            foreach (System.Windows.Ink.Stroke s in strokes)
            {
                //Console.WriteLine("no of points: " + s.StylusPoints.Count);
                foreach (StylusPoint sp in s.StylusPoints)
                {
                    double pointX = sp.X;
                    double pointY = sp.Y;

                    // Discretize to grid
                    int idxX = (int)Math.Floor(pointX / (width / cols));
                    int idxY = (int)Math.Floor(pointY / (height / rows));

                    if (idxX >= 0 && idxY >= 0 && idxX < cols && idxY < rows)
                    {
                        discretization_result[idxX, idxY] = 1;
                    }

                }
            }

            return discretization_result;
        }

        public static void InterpretGraph(StrokeCollection strokes)
        {

        }

        public static List<Point> InterpretTable(StrokeCollection strokes, double width, double height, int rows, int cols)
        {
            List<Point> discretizaton_result = new List<Point>();
            foreach (System.Windows.Ink.Stroke s in strokes)
            {
                // Calculate centroids
                double centroidX = 0.0;
                double centroidY = 0.0;
                double total = (double) s.StylusPoints.Count;
                foreach (StylusPoint sp in s.StylusPoints)
                {
                    centroidX += sp.X;
                    centroidY += sp.Y;
                }
                centroidX /= total;
                centroidY /= total;

                // Discretize to grid
                int idxX = (int)Math.Floor(centroidX / (width / cols));
                int idxY = (int)Math.Floor(centroidY / (height / rows));

                Point p = new Point(idxX, idxY);
                discretizaton_result.Add(p);
            }
            return discretizaton_result;
        }

        public static ContextNodeCollection InterpretShapes(StrokeCollection strokes)
        {
            if (strokes.Count > 0)
            {
                InkAnalyzer analyzer = new InkAnalyzer();
                analyzer.AddStrokes(strokes);
                if (analyzer.Analyze().Successful)
                {
                    return analyzer.FindNodesOfType(ContextNodeType.InkDrawing);
                }
            }
            return null;
        }
    }
}
