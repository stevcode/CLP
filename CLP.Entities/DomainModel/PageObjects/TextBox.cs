﻿using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class TextBox : APageObjectBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="TextBox" /> from scratch.
        /// </summary>
        public TextBox() { }

        /// <summary>
        /// Initializes <see cref="TextBox" /> from <see cref="string" />.
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="TextBox" /> belongs to.</param>
        /// <param name="text">The RTF formatted text of the <see cref="TextBox" /></param>
        public TextBox(CLPPage parentPage, string text)
            : base(parentPage)
        {
            Text = text;
            XPosition = 50.0;
            YPosition = 50.0;
            Height = 200.0;
            Width = 400.0;
        }

        /// <summary>
        /// Initializes <see cref="TextBox" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public TextBox(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// The RTF formatted text of the <see cref="TextBox" />.
        /// </summary>
        public string Text
        {
            get { return GetValue<string>(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly PropertyData TextProperty = RegisterProperty("Text", typeof(string), string.Empty);

        #endregion //Properties

        #region Methods

        public override IPageObject Duplicate()
        {
            var newTextBox = Clone() as TextBox;
            if(newTextBox == null)
            {
                return null;
            }
            newTextBox.ID = Guid.NewGuid().ToString();
            newTextBox.ParentPageID = ParentPageID;
            newTextBox.ParentPage = ParentPage;

            return newTextBox;
        }

        #endregion //Methods
    }
}