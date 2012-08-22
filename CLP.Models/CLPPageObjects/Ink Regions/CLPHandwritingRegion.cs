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
            ObservableCollection<string> StrokesNoDuplicates = new ObservableCollection<string>(PageObjectStrokes.Distinct().ToList());
            string result = InkInterpretation.InterpretHandwriting(CLPPage.StringsToStrokes(StrokesNoDuplicates), AnalysisType);
            if (result != null)
                StoredAnswer = result;
        }

        /// <summary>
        /// Retrieves the actual data from the serialization info.
        /// </summary>
        /// <remarks>
        /// This method should only be implemented if backwards compatibility should be implemented for
        /// a class that did not previously implement the DataObjectBase class.
        /// </remarks>
        protected override void GetDataFromSerializationInfo(SerializationInfo info)
        {
            // Check if deserialization succeeded
            if(DeserializationSucceeded)
            {
                return;
            }

            Console.WriteLine("deserialize fail!");

            // Deserialization did not succeed for any reason, so retrieve the values manually
            // Luckily there is a helper class (SerializationHelper) 
            // that eases the deserialization of "old" style objects
            //FirstName = SerializationHelper.GetString(info, "FirstName", FirstNameProperty.GetDefaultValue());
            //LastName = SerializationHelper.GetString(info, "LastName", LastNameProperty.GetDefaultValue());
        }

        #endregion // Methods
    }
}
