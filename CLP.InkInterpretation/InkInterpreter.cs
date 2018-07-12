using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Input.Inking;

namespace CLP.InkInterpretation
{
    public static class InkInterpreter
    {
        public static bool IsDecimalInterpretation { get; set; } = true;

        public static void SaveStrokesToGif(StrokeCollection strokes)
        {
            var inkManager = new InkManager();
            var strokeBuilder = new InkStrokeBuilder();

            //Convert System.Windows.Ink.Stroke to Windows.UI.Input.Inking.InkStroke and add to InkManager
            foreach (var stroke in strokes.Select(s => s.StylusPoints.Select(sp => new Point(sp.X, sp.Y))).Select(pts => strokeBuilder.CreateStroke(pts)))
            {
                inkManager.AddStroke(stroke);
            }

            var desktopFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var desktopStorageFolder = StorageFolder.GetFolderFromPathAsync(desktopFolderPath).GetAwaiter().GetResult();
            var fileName = "strokes.gif";
            var storageFile = desktopStorageFolder.CreateFileAsync(fileName).GetAwaiter().GetResult();

            try
            {
                using (var stream = storageFile.OpenAsync(FileAccessMode.ReadWrite).GetAwaiter().GetResult())
                {
                    stream.Size = 0;
                    inkManager.SaveAsync(stream).GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
            }
        }

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

            IReadOnlyList<InkRecognitionResult> recognitionResults;

            try
            {
                //Asynchronously interpret strokes as text.
                var recognitionTask = inkManager.RecognizeAsync(InkRecognitionTarget.All).AsTask();
                recognitionResults = recognitionTask.Result;
            }
            catch (Exception)
            {
                return new List<string>();
            }

            //Doing a recognition does not update the storage of results (the results that are stored inside the ink manager). 
            //We do that ourselves by calling this below method.
            inkManager.UpdateRecognitionResults(recognitionResults);

            // Aggregate the 5 most likely result words, with spaces between.
            var allInterpretations = new List<string>();
            for (var i = 0; i < 5; i++)
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

        public static string MatchInterpretationToExpectedInt(List<string> interpretations, int number)
        {
            var trimmedInterpretations = interpretations.Select(i => i.Replace(" ", string.Empty)).ToList();
            foreach (var trimmed in trimmedInterpretations)
            {
                var adjustedInterpretation = trimmed.Replace("~", "2");
                adjustedInterpretation = adjustedInterpretation.Replace("&", "8");
                adjustedInterpretation = adjustedInterpretation.Replace(">", "7");
                adjustedInterpretation = adjustedInterpretation.Replace("l", "1");
                adjustedInterpretation = adjustedInterpretation.Replace("|", "1");
                adjustedInterpretation = adjustedInterpretation.Replace("Z", "2");
                adjustedInterpretation = adjustedInterpretation.Replace("z", "2");
                adjustedInterpretation = adjustedInterpretation.Replace("\"", "11");
                adjustedInterpretation = adjustedInterpretation.Replace(",", string.Empty);
                adjustedInterpretation = adjustedInterpretation.Replace("'", string.Empty);
                if (!IsDecimalInterpretation)
                {
                    adjustedInterpretation = adjustedInterpretation.Replace(".", string.Empty);
                }

                if (int.TryParse(adjustedInterpretation, out var parsedInterpretation))
                {
                    if (parsedInterpretation == number)
                    {
                        return adjustedInterpretation;
                    }
                }

                if (number == 18 &&
                    adjustedInterpretation.Contains("if"))
                {
                    return number.ToString();
                }

                if (number > 10 &&
                    adjustedInterpretation.Contains(number.ToString()))
                {
                    return number.ToString();
                }
            }

            return string.Empty;
        }

        public static string InterpretationClosestToANumber(List<string> interpretations)
        {
            if (!interpretations.Any())
            {
                return string.Empty;
            }

            var mostNumbers = interpretations.First();
            var percentageOfNumbers = 0.0;
            interpretations.Reverse();
            foreach (var interpretation in interpretations)
            {
                var numberOfDigits = interpretation.Count(char.IsDigit);
                var percentage = numberOfDigits / (interpretation.Length * 1.0);
                //var percentage = numberOfDigits / interpretation.Length;
                if (percentage >= percentageOfNumbers)
                {
                    percentageOfNumbers = percentage;
                    mostNumbers = interpretation;
                }
            }

            //return mostNumbers;
            var adjustedInterpretation = mostNumbers.Replace(",", string.Empty);
            adjustedInterpretation = adjustedInterpretation.Replace("'", string.Empty);
            if (!IsDecimalInterpretation)
            {
                adjustedInterpretation = adjustedInterpretation.Replace(".", string.Empty);
            }

            return adjustedInterpretation;
        }

        /// <summary>
        /// Interprets strokes as arithmetic. Returns null if no valid representation of arithmetic.
        /// </summary>
        /// <param name="strokes"></param>
        /// <returns></returns>
        public static string StrokesToArithmetic(StrokeCollection strokes)
        {
            const double INTERPRET_AS_ARITH_DIGIT_PERCENTAGE_THRESHOLD = 5.0;
            const string MULTIPLICATION_SYMBOL = "×";
            const string ADDITION_SYMBOL = "+";
            const string EQUALS_SYMBOL = "=";
            const string DIVISION_SYMBOL = "÷";

            var interpretations = StrokesToAllGuessesText(strokes);
            var interpretation = InterpretationClosestToANumber(interpretations);

            var definitelyInArith = new List<string> { MULTIPLICATION_SYMBOL, ADDITION_SYMBOL, EQUALS_SYMBOL, DIVISION_SYMBOL };
            var percentageOfDigits = GetPercentageOfDigits(interpretation);
            var isDefinitelyArith = definitelyInArith.Any(s => interpretation.Contains(s));

            if (percentageOfDigits < INTERPRET_AS_ARITH_DIGIT_PERCENTAGE_THRESHOLD &&
                !isDefinitelyArith)
            {
                return null;
            }

            return interpretation;
        }

        public static double GetPercentageOfDigits(string s)
        {
            var numberOfDigits = s.Where(char.IsDigit).Count();
            return numberOfDigits * 100.0 / s.Length;
        }
    }
}