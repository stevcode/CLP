using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using Microsoft.Ink;

namespace Classroom_Learning_Partner.Resources
{
    public enum ANALYSIS_TYPE { DEFAULT, NUMBER, DIGIT, WORDS };

    public static class InkInterpretation
    {

        public static string InterpretHandwriting(StrokeCollection strokes, ANALYSIS_TYPE type)
        {
            string result = null;
            InkAnalyzer analyzer = new InkAnalyzer();
            AnalysisHintNode hint = analyzer.CreateAnalysisHint();
            hint.Location.MakeInfinite();
            if (type != ANALYSIS_TYPE.DEFAULT)
            {
                switch (type)
                {
                    case ANALYSIS_TYPE.NUMBER:
                        // Number sentence or number
                        hint.Factoid = Factoid.Number;
                        break;
                    case ANALYSIS_TYPE.DIGIT:
                        // Digit
                        hint.Factoid = Factoid.Digit;
                        break;
                    case ANALYSIS_TYPE.WORDS:
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

        public static void InterpretShading(StrokeCollection strokes)
        {

        }

        public static void InterpretGraph(StrokeCollection strokes)
        {

        }

        public static void InterpretTable(StrokeCollection strokes)
        {

        }

        public static void InterpretCircle(StrokeCollection strokes)
        {

        }

    }
}
