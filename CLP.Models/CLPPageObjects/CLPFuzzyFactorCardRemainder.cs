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

            XPosition = fuzzyFactorCard.XPosition + fuzzyFactorCard.LabelLength + 20.0;
            YPosition = fuzzyFactorCard.YPosition + fuzzyFactorCard.Height + 20.0;

            Height = Math.Ceiling((double)remainderValue / 5.0) * (SquareSize + 16.0); // +20.0;
            Width = 5.0 * (SquareSize + 16.0); // +20.0;

            //TODO Liz Change to random number
            for(int i = 0; i < remainderValue; i++)
            {
                if(i % 2 == 0)
                {
                    TileOffsets.Add("-3 7 7 -3"); 
                }
                else
                {
                    TileOffsets.Add("7 -3 -3 7");
                }
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

        public void RemoveTiles(int numberOfTiles)
        {
            for(int i = 0; i < numberOfTiles; i++)
            {
                TileOffsets.RemoveAt(TileOffsets.Count - 1);
            }

            if((TileOffsets.Count + numberOfTiles) % 5 <= numberOfTiles && (TileOffsets.Count + numberOfTiles) % 5 > 0)
            {
                Height -= SquareSize + 16.0;
            }
        }

        public void AddTiles(int numberOfTiles)
        {
            for(int i = 0; i < numberOfTiles; i++)
            {
                //TODO Liz Change to random number
                if(TileOffsets.Count % 2 == 0)
                {
                    TileOffsets.Add("-3 7 7 -3");
                }
                else
                {
                    TileOffsets.Add("7 -3 -3 7");
                }
            }
            if(TileOffsets.Count % 5 <= numberOfTiles && TileOffsets.Count % 5 > 0)
            {
                Height += SquareSize + 16.0;
            }
        }

        #endregion //Methods

    }
}
