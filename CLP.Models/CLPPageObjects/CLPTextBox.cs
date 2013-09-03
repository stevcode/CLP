using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPTextBox : ACLPPageObjectBase
    {
        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPTextBox(ICLPPage page) : this("", page) { }

        public CLPTextBox(string text, ICLPPage page)
            : base(page)
        {
            Text = text;

            XPosition = 50;
            YPosition = 50;
            Height = 200;
            Width = 400;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPTextBox(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties

        /// <summary>
        /// The text and formatting of the text box.
        /// </summary>
        public string Text
        {
	        get { return GetValue<string>(TextProperty); }
	        set { SetValue(TextProperty, value); }
        }

        public static readonly PropertyData TextProperty = RegisterProperty("Text", typeof(string), "");

        #endregion

        #region Methods

        public override string PageObjectType
        {
            get { return "CLPTextBox"; }
        }

        public override ICLPPageObject Duplicate()
        {
            CLPTextBox newTextBox = this.Clone() as CLPTextBox;
            newTextBox.UniqueID = Guid.NewGuid().ToString();
            newTextBox.ParentPage = ParentPage;

            return newTextBox;
        }

        #endregion
    }
}
