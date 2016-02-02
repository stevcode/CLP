using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace CLP.InkInterpretation
{
    public static class InkInterpreter
    {
        public static string StrokesToBestGuessText(StrokeCollection strokes)
        {
            if (strokes == null ||
                !strokes.Any())
            {
                return string.Empty;
            }

            var inkManager = new InkManager();
            var strokeBuilder = new InkStrokeBuilder();

            //Convert System.Windows.Ink.Stroke to Windows.UI.Input.Inking.InkStroke and add to InkManager
            foreach (var stroke in strokes.Select(s => s.StylusPoints.Select(sp => new Point(sp.X, sp.Y))).Select(pts => strokeBuilder.CreateStroke(pts)))
            {
                inkManager.AddStroke(stroke);
            }

            //Set handwriting recognizer to US English
            const string RECOGNIZER_NAME = "Microsoft English (US) Handwriting Recognizer";
            var recognizers = inkManager.GetRecognizers();
            for (int i = 0, len = recognizers.Count; i < len; i++)
            {
                if (RECOGNIZER_NAME == recognizers[i].Name)
                {
                    inkManager.SetDefaultRecognizer(recognizers[i]);
                    break;
                }
            }

            try
            {
                //Asynchronously interpret strokes as text.
                var recognitionTask = inkManager.RecognizeAsync(InkRecognitionTarget.All).AsTask();
                var recognitionResults = recognitionTask.Result;

                //Doing a recognition does not update the storage of results (the results that are stored inside the ink manager). 
                //We do that ourselves by calling this below method.
                inkManager.UpdateRecognitionResults(recognitionResults);

                // Aggregate the most likely result words, with spaces between.
                var interpretation = recognitionResults.Select(result => result.GetTextCandidates()).Aggregate(string.Empty, (current, text) => current + " " + text[0]);

                return interpretation.Trim();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static List<string> StrokesToAllGuessesText(StrokeCollection strokes)
        {
            if (strokes == null ||
                !strokes.Any())
            {
                return new List<string>();
            }

            var inkManager = new InkManager();
            var strokeBuilder = new InkStrokeBuilder();

            //Convert System.Windows.Ink.Stroke to Windows.UI.Input.Inking.InkStroke and add to InkManager
            foreach (var stroke in strokes.Select(s => s.StylusPoints.Select(sp => new Point(sp.X, sp.Y))).Select(pts => strokeBuilder.CreateStroke(pts)))
            {
                inkManager.AddStroke(stroke);
            }

            //Set handwriting recognizer to US English
            const string RECOGNIZER_NAME = "Microsoft English (US) Handwriting Recognizer";
            var recognizers = inkManager.GetRecognizers();
            for (int i = 0, len = recognizers.Count; i < len; i++)
            {
                if (RECOGNIZER_NAME == recognizers[i].Name)
                {
                    inkManager.SetDefaultRecognizer(recognizers[i]);
                    break;
                }
            }

            //Asynchronously interpret strokes as text.
            var recognitionTask = inkManager.RecognizeAsync(InkRecognitionTarget.All).AsTask();
            var recognitionResults = recognitionTask.Result;

            //Doing a recognition does not update the storage of results (the results that are stored inside the ink manager). 
            //We do that ourselves by calling this below method.
            inkManager.UpdateRecognitionResults(recognitionResults);

            // Aggregate the 3 most likely result words, with spaces between.
            var allInterpretations = new List<string>();
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    var interpretation = recognitionResults.Select(result => result.GetTextCandidates()).Aggregate(string.Empty, (current, text) => current + " " + text[i]);
                    var trimmedInterpretation = interpretation.Trim();
                    allInterpretations.Add(trimmedInterpretation);
                }
                catch (Exception)
                {
                    break;
                }
            }

            return allInterpretations;
        }
    }
}