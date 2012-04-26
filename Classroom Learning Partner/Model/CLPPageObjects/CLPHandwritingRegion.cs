using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classroom_Learning_Partner.Resources;
using Catel.Data;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPHandwritingRegion : CLPInkRegion
    {
        #region Constructors

        public CLPHandwritingRegion() : base()
        {
            AnalysisType = CLPHandwritingAnalysisType.DEFAULT;
            StoredAnswer = "";
        }

        public CLPHandwritingRegion(CLPHandwritingAnalysisType analysis_type) : base()
        {
            AnalysisType = analysis_type;
            StoredAnswer = "";
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPHandwritingRegion(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion // Constructors

        #region Properties

        public override string PageObjectType
        {
            get { return "CLPHandwritingRegion"; }
        }

        /// <summary>
        /// Ink interpretation type (default, words, numbers, number sentence, etc.).
        /// </summary>
        public CLPHandwritingAnalysisType AnalysisType
        {
            get { return GetValue<CLPHandwritingAnalysisType>(AnalysisTypeProperty); }
            set { SetValue(AnalysisTypeProperty, value); }
        }

        /// <summary>
        /// Register the AnalysisType property so it is known in the class.
        /// </summary>
        public static readonly PropertyData AnalysisTypeProperty = RegisterProperty("AnalysisType", typeof(CLPHandwritingAnalysisType), 0);

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

        #endregion // Properties

        #region Methods

        public override void DoInterpretation()
        {
            ObservableCollection<string> StrokesNoDuplicates = new ObservableCollection<string>(PageObjectStrokes.Distinct().ToList());
            string result = InkInterpretation.InterpretHandwriting(CLPPage.StringsToStrokes(StrokesNoDuplicates), AnalysisType);
            if (result != null)
                StoredAnswer = result;
        }

        #endregion // Methods
    }
}
