using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    [Serializable]
    public abstract class ANumberLineBaseTag: ATagBase
    {

        #region Constructors

        /// <summary>Initializes <see cref="ANumberLineBaseTag" /> from scratch.</summary>
        public ANumberLineBaseTag() { }

        public ANumberLineBaseTag(CLPPage parentPage, Origin origin, string numberLineID, int firstNumber, int lastNumber, int numberLineNumber)
            : base(parentPage, origin)
        {
            NumberLineID = numberLineID;
            FirstNumber = firstNumber;
            LastNumber = lastNumber;
            NumberLineNumber = numberLineNumber + 1;
        }

        public ANumberLineBaseTag(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion  //Constructors

        #region Properties

        /// <summary>ID of the Number Line against which this tag compares.</summary>
        public string NumberLineID
        {
            get { return GetValue<string>(NumberLineIDProperty); }
            set { SetValue(NumberLineIDProperty, value); }
        }
        
        public static readonly PropertyData NumberLineIDProperty = RegisterProperty("NumberLineID", typeof (string), string.Empty);

        /// <summary>First Number of the Number Line being compared against.</summary>
        public int FirstNumber
        {
            get { return GetValue<int>(FirstNumberProperty); }
            set { SetValue(FirstNumberProperty, value); }
        }

        public static readonly PropertyData FirstNumberProperty = RegisterProperty("FirstNumber", typeof (int));

        /// <summary>Last Number of the Number Line being compared against.</summary>
        public int LastNumber
        {
            get { return GetValue<int>(LastNumberProperty); }
            set { SetValue(LastNumberProperty, value); }
        }

        public static readonly PropertyData LastNumberProperty = RegisterProperty("LastNumber", typeof (int));


        /// <summary>Order in which the associated Number Line occured in the history.</summary>
        public int NumberLineNumber
        {
            get { return GetValue<int>(NumberLineNumberProperty); }
            set { SetValue(NumberLineNumberProperty, value); }
        }

        public static readonly PropertyData NumberLineNumberProperty = RegisterProperty("NumberLineNumber", typeof (int), 0);

        /// <summary>Determines if the NumberLine this tag applies to is still on the Parent Page or if it has been deleted from the page.</summary>
        public bool IsNumberLineStillOnPage
        {
            get { return ParentPage.GetPageObjectByID(NumberLineID) != null; }
        }

        #endregion //Properties

        #region ATagBase Overrides

        public override Category Category
        {
            get { return Category.NumberLine; }
        }

        #endregion //ATagBase Overrides

        #endregion //Properties
    }
}