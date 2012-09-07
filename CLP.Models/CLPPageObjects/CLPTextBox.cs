﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Catel.Data;
using System.Runtime.Serialization;
using Catel.Runtime.Serialization;

namespace CLP.Models
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
        public CLPTextBox(CLPPage page) : this("", page) { }

        public CLPTextBox(string text, CLPPage page)
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