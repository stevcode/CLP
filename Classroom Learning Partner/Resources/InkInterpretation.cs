using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Ink;
using Microsoft.Ink;

namespace Classroom_Learning_Partner.Resources
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

        public static void InterpretShading(StrokeCollection strokes)
        {

        }

        public static void InterpretGraph(StrokeCollection strokes)
        {

        }

        public static void InterpretTable(StrokeCollection strokes)
        {

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
