using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public class NumberLine : APageObjectBase
    {
        #region Constructors

        public NumberLine() { }

        public NumberLine(CLPPage parentPage, int numberLength)
            : base(parentPage)
        {
            NumberLineLength = numberLength;
            Height = 60;
            Width = 800;
        }

        public NumberLine(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        private const double MIN_NUMBERLINE_LENGTH = 25.0;

        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        /// <summary>Length of number line</summary>
        public int NumberLineLength
        {
            get { return GetValue<int>(NumberLineLengthProperty); }
            set { SetValue(NumberLineLengthProperty, value); }
        }

        public static readonly PropertyData NumberLineLengthProperty = RegisterProperty("NumberLineLength", typeof (int), 0);

        #endregion //Properties

        #region Methods

        public override IPageObject Duplicate()
        {
            var newNumberLine = Clone() as NumberLine;
            if (newNumberLine == null)
            {
                return null;
            }
            newNumberLine.CreationDate = DateTime.Now;
            newNumberLine.ID = Guid.NewGuid().ToCompactID();
            newNumberLine.VersionIndex = 0;
            newNumberLine.LastVersionIndex = null;
            newNumberLine.ParentPage = ParentPage;

            return newNumberLine;
        }

        #endregion //Methods
    }
}