using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catel.Data;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Ink;
using Catel.Runtime.Serialization;

namespace CLP.Models
{
    [Serializable]
    public class CLPHandwritingRegion : ACLPInkRegion
    {
        #region Constructors

        public CLPHandwritingRegion(CLPPage page) : this(CLPHandwritingAnalysisType.DEFAULT, page)
        {
        }

        public CLPHandwritingRegion(CLPHandwritingAnalysisType analysis_type, CLPPage page) : base(page)
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
            : base(info, context) {

                //int analysisType = (int)info.GetValue("AnalysisType", typeof(int));
                //switch(analysisType)
                //{
                //    case 0:
                //        AnalysisType = CLPHandwritingAnalysisType.DEFAULT;
                //        break;
                //    case 1:
                //        AnalysisType = CLPHandwritingAnalysisType.NUMBER;
                //        break;
                //    case 2:
                //        AnalysisType = CLPHandwritingAnalysisType.DIGIT;
                //        break;
                //    case 3:
                //        AnalysisType = CLPHandwritingAnalysisType.WORDS;
                //        break;
                //    default:
                //        break;
                //}
        }

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
            ObservableCollection<List<byte>> StrokesNoDuplicates = new ObservableCollection<List<byte>>(PageObjectByteStrokes.Distinct().ToList());
            string result = InkInterpretation.InterpretHandwriting(CLPPage.BytesToStrokes(StrokesNoDuplicates), AnalysisType);
            if (result != null)
                StoredAnswer = result;
            Console.WriteLine("HW regions: " + StoredAnswer);
        }

        #endregion // Methods
    }
}
