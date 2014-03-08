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
                TileOffsets.Add("DodgerBlue"); 
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
                //TODO Liz update to set appropriate tiles to black
                TileOffsets.Add("DodgerBlue");
            }
            if(TileOffsets.Count % 5 <= numberOfTiles && TileOffsets.Count % 5 > 0)
            {
                Height += SquareSize + 16.0;
            }
        }

        //public void FadeTiles(int numberOfTiles)
        //{
        //    for(int i = 0; i < numberOfTiles; i++)
        //    {
        //        TileOffsets.RemoveAt(TileOffsets.Count - 1);
        //    }
        //    for(int i = 0; i < numberOfTiles; i++)
        //    {
        //        TileOffsets.Add("Black");
        //    }
        //}

        public void UpdateTiles()
        {
            CLPFuzzyFactorCard ffc = ParentPage.GetPageObjectByUniqueID(FuzzyFactorCardUniqueID) as CLPFuzzyFactorCard;
            int numberOfBlackTiles = 0;
            foreach(var pageObject in ParentPage.PageObjects)
            {
                //TO DO Liz - update for rotating FFC
                if(pageObject.PageObjectType == "CLPArray" && (pageObject as CLPArray).Rows == ffc.Rows)
                {
                    numberOfBlackTiles += (pageObject as CLPArray).Rows * (pageObject as CLPArray).Columns;
                }
            }

            TileOffsets.Clear();
            for(int i = 0; i < ffc.CurrentRemainder - numberOfBlackTiles; i++)
            {
                TileOffsets.Add("DodgerBlue");
            }
            for(int i = 0; i < numberOfBlackTiles; i++)
            {
                TileOffsets.Add("Black");
            }

            Height = Math.Ceiling((double)TileOffsets.Count / 5.0) * (SquareSize + 16.0);
        }

        #endregion //Methods

    }
}
