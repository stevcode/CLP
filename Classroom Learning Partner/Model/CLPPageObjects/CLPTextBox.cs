using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Catel.Data;
using System.Runtime.Serialization;

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

            Position = new System.Windows.Point(100, 100);
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

        /// <summary>
        /// Register the Text property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TextProperty = RegisterProperty("Text", typeof(string), "");

        #endregion

        #region Methods

        /// <summary>
        /// Validates the fields.
        /// </summary>
        protected override void ValidateFields()
        {
            // TODO: Implement any field validation of this object. Simply set any error by using the SetFieldError method
        }

        /// <summary>
        /// Validates the business rules.
        /// </summary>
        protected override void ValidateBusinessRules()
        {
            // TODO: Implement any business rules of this object. Simply set any error by using the SetBusinessRuleError method
        }

        public override string PageObjectType
        {
            get { return "CLPTextBox"; }
        }

        public override CLPPageObjectBase Duplicate()
        {
            CLPTextBox newTextBox = this.Clone() as CLPTextBox;
            newTextBox.UniqueID = Guid.NewGuid().ToString();

            return newTextBox;
        }

        #endregion

        //public CLPTextBox() : this("")
        //{ 
        //}

        //public CLPTextBox(string s) : base()
        //{
        //    _text = s;
        //    Position = new Point(100, 100);
        //    Height = 200;
        //    Width = 400;
        //}

        //private string _text;
        //public string Text
        //{
        //    get
        //    {
        //        return _text;
        //    }
        //    set
        //    {
        //        _text = value;
        //    }
        //}

    }
}
