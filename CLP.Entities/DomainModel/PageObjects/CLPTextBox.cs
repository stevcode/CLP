using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public enum TextContexts
    {
        None,
        WordProblem,
        NonWordProblem
    }

    [Serializable]
    public class CLPTextBox : APageObjectBase
    {
        #region Constructors

        /// <summary>Initializes <see cref="CLPTextBox" /> from scratch.</summary>
        public CLPTextBox() { }

        /// <summary>Initializes <see cref="CLPTextBox" /> from <see cref="string" />.</summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="CLPTextBox" /> belongs to.</param>
        /// <param name="text">The RTF formatted text of the <see cref="CLPTextBox" /></param>
        public CLPTextBox(CLPPage parentPage, string text)
            : base(parentPage)
        {
            Text = text;
            XPosition = 50.0;
            YPosition = 50.0;
            Height = 200.0;
            Width = 400.0;
        }

        /// <summary>Initializes <see cref="CLPTextBox" /> based on <see cref="SerializationInfo" />.</summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public CLPTextBox(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        /// <summary>The RTF formatted text of the <see cref="CLPTextBox" />.</summary>
        public string Text
        {
            get { return GetValue<string>(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly PropertyData TextProperty = RegisterProperty("Text", typeof (string), string.Empty);

        /// <summary>Context associated with <see cref="CLPTextBox" />.</summary>
        public TextContexts TextContext
        {
            get { return GetValue<TextContexts>(TextContextProperty); }
            set { SetValue(TextContextProperty, value); }
        }

        public static readonly PropertyData TextContextProperty = RegisterProperty("TextContext", typeof (TextContexts), TextContexts.None);
        

        #endregion //Properties

        #region APageObjectBase Overrides

        public override string FormattedName
        {
            get { return "Text Box"; }
        }

        public override string CodedName
        {
            get { return Codings.OBJECT_TEXT; }
        }

        public override string CodedID
        {
            get { return "A"; } // TODO: Make this work with IncrementID
        }

        public override int ZIndex
        {
            get { return 20; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return false; }
        }

        #endregion //APageObjectBase Overrides
    }
}