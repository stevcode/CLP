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

        public CLPFuzzyFactorCardRemainder(CLPFuzzyFactorCard fuzzyFactorCard, int remainderValue, ICLPPage page)
            : base(page)
        {
            FuzzyFactorCardUniqueID = fuzzyFactorCard.UniqueID;

            XPosition = fuzzyFactorCard.XPosition + fuzzyFactorCard.LabelLength;
            YPosition = fuzzyFactorCard.YPosition + fuzzyFactorCard.Height + 10.0;

            Height = 5.0 * (SquareSize + 5.0); //TODO Liz Change this
            Width = 5.0 * (SquareSize + 5.0);

            for(int i = 0; i < remainderValue; i++)
            {
                TileOffsets.Add(0.0); //TODO Liz Change to random number
            }
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
        /// UniqueID of corresponding FFC.
        /// </summary>
        public string FuzzyFactorCardUniqueID
        {
            get
            {
                return GetValue<string>(FuzzyFactorCardUniqueIDProperty);
            }
            set
            {
                SetValue(FuzzyFactorCardUniqueIDProperty, value);
            }
        }

        /// <summary>
        /// Register the FuzzyFactorCardUniqueID property so it is known in the class.
        /// </summary>
        public static readonly PropertyData FuzzyFactorCardUniqueIDProperty = RegisterProperty("FuzzyFactorCardUniqueID", typeof(string), null);

        /// <summary>
        /// Offsets of each tile    
        /// </summary>
        public ObservableCollection<double> TileOffsets
        {
            get
            {
                return GetValue<ObservableCollection<double>>(TileOffsetsProperty);
            }
            set
            {
                SetValue(TileOffsetsProperty, value);
            }
        }

        /// <summary>
        /// Register the TileOffsets property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TileOffsetsProperty = RegisterProperty("TileOffsets", typeof(ObservableCollection<double>), () => new ObservableCollection<double>());

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

        public void RemoveTiles(int numberOfTiles)
        {
            for(int i = 0; i < numberOfTiles; i++)
            {
                TileOffsets.RemoveAt(TileOffsets.Count - 1);
            }
            
            Console.Write("number of tiles: ");
            Console.WriteLine(TileOffsets.Count);
        }

        public void AddTiles(int numberOfTiles)
        {
            for(int i = 0; i < numberOfTiles; i++)
            {
                TileOffsets.Add(0.0);
            }
        }

        #endregion //Methods

    }
}
