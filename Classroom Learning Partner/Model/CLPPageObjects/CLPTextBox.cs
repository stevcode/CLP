using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Catel.Data;
using System.Runtime.Serialization;
using Catel.Runtime.Serialization;

namespace Classroom_Learning_Partner.Model.CLPPageObjects
{
    [Serializable]
    public class CLPTextBox : CLPPageObjectBase
    {
        #region Variables
        #endregion

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPTextBox() : this("") { }

        public CLPTextBox(string text)
            : base()
        {
            Text = text;

            Position = new System.Windows.Point(50, 50);
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

            // Deserialization did not succeed for any reason, so retrieve the values manually
            // Luckily there is a helper class (SerializationHelper) 
            // that eases the deserialization of "old" style objects
            //FirstName = SerializationHelper.GetString(info, "FirstName", FirstNameProperty.GetDefaultValue());
            //LastName = SerializationHelper.GetString(info, "LastName", LastNameProperty.GetDefaultValue());
        }

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

        /// <summary>
        /// Register the Text property so it is known in the class.
        /// </summary>
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

            return newTextBox;
        }

        #endregion
    }
}
