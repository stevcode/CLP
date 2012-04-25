using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using Microsoft.Ink;
using Classroom_Learning_Partner.Resources;
using Catel.Data;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{

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

        protected override void OnDeserialized()
        {
            ObservableCollection<string> StrokesNoDuplicates = new ObservableCollection<string>(PageObjectStrokes.Distinct().ToList());
            string result = InkInterpretation.InterpretHandwriting(CLPPage.StringsToStrokes(StrokesNoDuplicates), AnalysisType);
            if (result != null)
                StoredAnswer = result;
            base.OnDeserialized();
        }

        [OnSerializing]
        void InterpretStrokes(StreamingContext sc)
        {
            ObservableCollection<string> StrokesNoDuplicates = new ObservableCollection<string>(PageObjectStrokes.Distinct().ToList());
            string result = InkInterpretation.InterpretHandwriting(CLPPage.StringsToStrokes(StrokesNoDuplicates), AnalysisType);
            if (result != null)
                StoredAnswer = result;
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
