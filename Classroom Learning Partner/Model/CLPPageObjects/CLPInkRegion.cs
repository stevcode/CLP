﻿using System;
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
    public enum ANALYSIS_TYPE { DEFAULT, NUMBER, DIGIT, WORDS };

    [Serializable]
    public class CLPInkRegion : CLPPageObjectBase
    {

        #region Constructors

        public CLPInkRegion() : base()
        {
            CanAcceptStrokes = true;
            AnalysisType = ANALYSIS_TYPE.DEFAULT;
            StoredAnswer = "";
            Position = new Point(100, 100);
            Height = 100;
            Width = 100;
        }

        public CLPInkRegion(ANALYSIS_TYPE analysis_type) : base()
        {
            CanAcceptStrokes = true;
            AnalysisType = analysis_type;
            StoredAnswer = "";
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

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Ink interpretation type (default, words, numbers, number sentence, etc.).
        /// Kelsey - convert this to an enum.
        /// </summary>
        public ANALYSIS_TYPE AnalysisType
        {
            get { return GetValue<ANALYSIS_TYPE>(AnalysisTypeProperty); }
            set { SetValue(AnalysisTypeProperty, value); }
        }

        /// <summary>
        /// Register the AnalysisType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AnalysisTypeProperty = RegisterProperty("AnalysisType", typeof(ANALYSIS_TYPE), 0);

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

        #endregion //Properties

        #region Methods

        public void InterpretStrokes()
        {
            InkAnalyzer analyzer = new InkAnalyzer();
            AnalysisHintNode hint = analyzer.CreateAnalysisHint();
            hint.Location.MakeInfinite();
            if (AnalysisType != ANALYSIS_TYPE.DEFAULT)
            {
                switch (AnalysisType)
                {
                    case ANALYSIS_TYPE.NUMBER:
                        Console.WriteLine("Number");
                        // Number sentence or number
                        hint.Factoid = Factoid.Number;
                        break;
                    case ANALYSIS_TYPE.DIGIT:
                        // Digit
                        Console.WriteLine("Digit");
                        hint.Factoid = Factoid.Digit;
                        break;
                    case ANALYSIS_TYPE.WORDS:
                        // Words
                        Console.WriteLine("Words");
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
                    Console.WriteLine("doing this");
                    foreach (System.Windows.Ink.Stroke stroke in CLPPage.StringsToStrokes(PageObjectStrokes))
                    {
                        stroke.DrawingAttributes.Color = System.Windows.Media.Colors.Red;
                    }
                    string result = analyzer.GetRecognizedString();
                    if (result != StoredAnswer)
                    {
                        StoredAnswer = result;
                    }
                }
            }

            //Steve/Kelsey - I added this at the recommendation of a Warning in the Error List, if it appears to cause problems, remove it.
            analyzer.Dispose();
        }



        protected override void OnDeserialized()
        {
            InterpretStrokes();
            base.OnDeserialized();
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

        public override ICLPPageObject Duplicate()
        {
            CLPInkRegion newInkRegion = this.Clone() as CLPInkRegion;
            newInkRegion.UniqueID = Guid.NewGuid().ToString();

            return newInkRegion;
        }

        public override void AcceptStrokes(StrokeCollection addedStrokes, StrokeCollection removedStrokes)
        {
            this.ProcessStrokes(addedStrokes, removedStrokes);
        }

        #endregion

        
    }
}
