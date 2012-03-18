using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Catel.Data;
using Microsoft.Ink;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    public class CLPInkRegion : CLPPageObjectBase
    {

        #region Constructors

        public CLPInkRegion()
            : base()
        {
            CorrectAnswer = "";
            AnalysisType = 0;

            StoredAnswer = "";
            NumberOfResponses = 0;

            Position = new Point(100, 100);
            Height = 100;
            Width = 100;
        }

        public CLPInkRegion(string correct_answer, int analysis_type)
            : base()
        {
            CorrectAnswer = correct_answer;
            AnalysisType = analysis_type;

            StoredAnswer = "";
            NumberOfResponses = 0;

            Position = new Point(100, 100);
            Height = 100;
            Width = 100;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPInkRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// Correct answer that should be supplied in this region.
        /// </summary>
        public string CorrectAnswer
        {
            get { return GetValue<string>(CorrectAnswerProperty); }
            set { SetValue(CorrectAnswerProperty, value); }
        }

        /// <summary>
        /// Register the CorrectAnswer property so it is known in the class.
        /// </summary>
        public static readonly PropertyData CorrectAnswerProperty = RegisterProperty("CorrectAnswer", typeof(string), "");

        /// <summary>
        /// Ink interpretation type (default, words, numbers, number sentence, etc.).
        /// Kelsey - convert this to an enum.
        /// </summary>
        public int AnalysisType
        {
            get { return GetValue<int>(AnalysisTypeProperty); }
            set { SetValue(AnalysisTypeProperty, value); }
        }

        /// <summary>
        /// Register the AnalysisType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AnalysisTypeProperty = RegisterProperty("AnalysisType", typeof(int), 0);

        /// <summary>
        /// Stored interpreted answer.
        /// </summary>
        public string StoredAnswer
        {
            get { return GetValue<string>(StoredAnswerProperty); }
            set { SetValue(StoredAnswerProperty, value); }
        }

        /// <summary>
        /// Register the StoredAnswer property so it is known in the class.
        /// </summary>
        public static readonly PropertyData StoredAnswerProperty = RegisterProperty("StoredAnswer", typeof(string), "");

        /// <summary>
        /// Number of different stored answers for this problem
        /// </summary>
        public int NumberOfResponses
        {
            get { return GetValue<int>(NumberOfResponsesProperty); }
            set { SetValue(NumberOfResponsesProperty, value); }
        }

        /// <summary>
        /// Register the NumberOfResponses property so it is known in the class.
        /// </summary>
        public static readonly PropertyData NumberOfResponsesProperty = RegisterProperty("NumberOfResponses", typeof(int), 0);

        #endregion //Properties

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
                analyzer.AddStrokes(CLPPage.StringsToStrokes(PageObjectStrokes));
                if (analyzer.Analyze().Successful)
                {
                    string result = analyzer.GetRecognizedString();
                    if (result != StoredAnswer)
                    {
                        NumberOfResponses++;
                        StoredAnswer = result;

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

        public override string PageObjectType
        {
            get { return "CLPInkRegion"; }
        }

        public override CLPPageObjectBase Duplicate()
        {
            CLPInkRegion newInkRegion = this.Clone() as CLPInkRegion;
            newInkRegion.UniqueID = Guid.NewGuid().ToString();

            return newInkRegion;
        }

        #endregion

        
    }
}
