using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Ink;
using System.ComponentModel;
using Catel.Data;

namespace CLP.Models
{
    [Serializable]
    public class CLPFuzzyFactorCardRemainder : ACLPPageObjectBase
    {
        #region Constructors

        public double SquareSize { get { return 45; } }

        public CLPFuzzyFactorCardRemainder(CLPFuzzyFactorCard fuzzyFactorCard,  ICLPPage page)
            : base(page)
        {
            XPosition = fuzzyFactorCard.XPosition + fuzzyFactorCard.Width + 20.0;
            YPosition = fuzzyFactorCard.YPosition + fuzzyFactorCard.LabelLength;

            Height = Math.Ceiling((double)(fuzzyFactorCard.CurrentRemainder) / 5.0) * (SquareSize + 16.0); 
            Width = 5.0 * (SquareSize + 16.0);
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPFuzzyFactorCardRemainder(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override string PageObjectType
        {
            get
            {
                return "CLPFuzzyFactorCardRemainder";
            }
        }

        /// <summary>
        /// Offsets of each tile    
        /// </summary>
        public ObservableCollection<string> TileOffsets
        {
            get
            {
                return GetValue<ObservableCollection<string>>(TileOffsetsProperty);
            }
            set
            {
                SetValue(TileOffsetsProperty, value);
            }
        }

        /// <summary>
        /// Register the TileOffsets property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TileOffsetsProperty = RegisterProperty("TileOffsets", typeof(ObservableCollection<string>), () => new ObservableCollection<string>());

        #endregion //Properties

        #region Methods

        public override ICLPPageObject Duplicate()
        {
            var newFuzzyFactorCardRemainder = Clone() as CLPFuzzyFactorCardRemainder;
            if(newFuzzyFactorCardRemainder != null)
            {
                newFuzzyFactorCardRemainder.UniqueID = Guid.NewGuid().ToString();
                newFuzzyFactorCardRemainder.ParentPage = ParentPage;
                return newFuzzyFactorCardRemainder;
            }
            return null;
        }

        #endregion //Methods

    }
}
