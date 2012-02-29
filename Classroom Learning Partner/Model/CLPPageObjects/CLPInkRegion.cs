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

        #region Constructors

        public CLPInkRegion() : base()
        {
            _correctAnswer = "";
            _anaylsisType = 0;

            _storedAnswer = "";
            _numberOfResponses = 0;

            Position = new Point(100, 100);
            Height = 100;
            Width = 100;
        }

        public CLPInkRegion(string correct_answer, int analysis_type) : base()
        {
            _correctAnswer = correct_answer;
            _anaylsisType = analysis_type;

            _storedAnswer = "";
            _numberOfResponses = 0;

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

        // Stored interpreted answer
        private string _storedAnswer;
        public string StoredAnswer
        {
            get { return _storedAnswer; }
        }

        // Number of different stored answers for this problem
        private int _numberOfResponses;
        public int NumberOfResponses
        {
            get { return _numberOfResponses; }
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
                    case 2:
                        // Digit
                        hint.Factoid = Factoid.Digit;
                        break;
                    case 3:
                        // Words
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
                    if (result != _storedAnswer)
                    {
                        _numberOfResponses++;
                        _storedAnswer = result;

                        /*string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Logs\analysis_log.csv";
                        if (!File.Exists(path))
                        {
                            TextWriter tw = new StreamWriter(path);
                            tw.WriteLine("PositionX,PositionY,Try_Number,Answer,Result,Type");
                            tw.WriteLine(this.Position + "," + this._numberOfResponses + "," + this.CorrectAnswer + "," + result + "," + this.AnalysisType);
                            tw.Close();
                        }
                        else
                        {
                            TextWriter tw = File.AppendText(path);
                            tw.WriteLine(this.Position + "," + this._numberOfResponses + "," + this.CorrectAnswer + "," + result + "," + this.AnalysisType);
                            tw.Close();
                        }*/
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
