using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.Serialization;
using Microsoft.Ink;
using System.Windows.Ink;
using Classroom_Learning_Partner.ViewModels;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPInkRegion : CLPPageObjectBase
    {

        #region Variables

        private string storedAnswer;
        private int numberOfResponses;

        #endregion

        #region Constructors

        public CLPInkRegion() : base()
        {
            _correctAnswer = "";
            _anaylsisType = 0;

            storedAnswer = "";
            numberOfResponses = 0;

            Position = new Point(100, 100);
            Height = 100;
            Width = 100;
        }

        public CLPInkRegion(string correct_answer, int analysis_type) : base()
        {
            _correctAnswer = correct_answer;
            _anaylsisType = analysis_type;

            storedAnswer = "";
            numberOfResponses = 0;

            Position = new Point(100, 100);
            Height = 100;
            Width = 100;
        }

        #endregion

        #region Properties

        // Correct answer that should be supplied in this region
        private string _correctAnswer;
        public string CorrectAnswer
        {
            get { return _correctAnswer; }
            set { _correctAnswer = value; }
        }

        // Ink interpretation type (default, words, numbers, number sentence, etc.)
        private int _anaylsisType;
        public int AnalysisType
        {
            get { return _anaylsisType; }
            set { _anaylsisType = value; }
        }

        #endregion

        #region Methods

        public void InterpretStrokes()
        {
            InkAnalyzer analyzer = new InkAnalyzer();
            AnalysisHintNode hint = analyzer.CreateAnalysisHint();
            hint.Location.MakeInfinite();
            if (AnalysisType != 0)
            {
                switch (AnalysisType)
                {
                    case 1:
                        // Number sentence or number
                        hint.Factoid = Factoid.Number;
                        break;
                    case 3:
                        // Digit
                        hint.Factoid = Factoid.Digit;
                        break;
                    case 4:
                        // Word
                        hint.Factoid = Factoid.SystemDictionary;
                        break;
                    default:
                        Console.WriteLine("Not a valid ink analysis type");
                        break;
                }
                hint.CoerceToFactoid = true;
            }

            if (PageObjectStrokes.Count > 0)
            {
                analyzer.AddStrokes(CLPPageViewModel.StringsToStrokes(PageObjectStrokes));
                if (analyzer.Analyze().Successful)
                {
                    string result = analyzer.GetRecognizedString();
                    if (result != storedAnswer)
                    {
                        numberOfResponses++;
                        storedAnswer = result;

                        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Logs\analysis_log.txt";
                        if (!File.Exists(path))
                        {
                            TextWriter tw = new StreamWriter(path);
                            tw.WriteLine("PositionX,PositionY,Try_Number,Answer,Result,Type");
                            tw.WriteLine(this.Position + "," + this.numberOfResponses + "," + this.CorrectAnswer + "," + result + "," + this.AnalysisType);
                            tw.Close();
                        }
                        else
                        {
                            TextWriter tw = File.AppendText(path);
                            tw.WriteLine(this.Position + "," + this.numberOfResponses + "," + this.CorrectAnswer + "," + result + "," + this.AnalysisType);
                            tw.Close();
                        }
                    }
                }
            }

        }

        [OnSerializing]
        void InterpretStrokes(StreamingContext sc)
        {
            InterpretStrokes();
        }

        #endregion
    }
}
