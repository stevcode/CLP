using System;
using System.Runtime.Serialization;
using Catel.Data;

namespace CLP.Entities
{
    public class FuzzyFactorCard : ACLPArrayBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCard" /> from scratch.
        /// </summary>
        public FuzzyFactorCard() { }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCard" /> from 
        /// </summary>
        /// <param name="parentPage">The <see cref="CLPPage" /> the <see cref="FuzzyFactorCard" /> belongs to.</param>
        /// <param name="columns">The number of columns in the <see cref="FuzzyFactorCard" />.</param>
        /// <param name="rows">The number of rows in the <see cref="FuzzyFactorCard" />.</param>
        /// <param name="dividend">The total number the <see cref="FuzzyFactorCard" /> represents.</param>
        /// <param name="isRemainderRegionDisplayed">Signifies the <see cref="FuzzyFactorCard" /> is using a <see cref="RemainderRegion" />.</param>
        public FuzzyFactorCard(CLPPage parentPage, int columns, int rows, int dividend, bool isRemainderRegionDisplayed = false)
            : base(parentPage, columns, rows) { }

        /// <summary>
        /// Initializes <see cref="FuzzyFactorCard" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public FuzzyFactorCard(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Properties

        public override double LabelLength
        {
            get { return 35; }
        }

        public double LargeLabelLength
        {
            get { return LabelLength * 2 + 12.0; }
        }

        public override bool IsBackgroundInteractable
        {
            get { return true; }
        }

        private const double MIN_ARRAY_LENGTH = 185.0;

        public override double MinimumHeight
        {
            get { return MIN_ARRAY_LENGTH + (2 * LabelLength); }
        }

        public override double MinimumWidth
        {
            get { return MIN_ARRAY_LENGTH + LargeLabelLength + LabelLength; }
        }

        public override double MinimumGridSquareSize
        {
            get { return Columns < Rows ? MIN_ARRAY_LENGTH / Columns : MIN_ARRAY_LENGTH / Rows; }
        }

        /// <summary>
        /// The total number the <see cref="FuzzyFactorCard" /> represents.
        /// </summary>
        public int Dividend
        {
            get { return GetValue<int>(DividendProperty); }
            set { SetValue(DividendProperty, value); }
        }

        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(int), 1);

        /// <summary>
        /// Orientation of the <see cref="FuzzyFactorCard" />.
        /// </summary>
        public bool IsHorizontallyAligned
        {
            get { return GetValue<bool>(IsHorizontallyAlignedProperty); }
            set { SetValue(IsHorizontallyAlignedProperty, value); }
        }

        public static readonly PropertyData IsHorizontallyAlignedProperty = RegisterProperty("IsHorizontallyAligned", typeof(bool), true);

        #endregion //Properties

        #region Methods

        public override IPageObject Duplicate()
        {
            var newFuzzyFactorCard = Clone() as FuzzyFactorCard;
            if(newFuzzyFactorCard == null)
            {
                return null;
            }
            newFuzzyFactorCard.CreationDate = DateTime.Now;
            newFuzzyFactorCard.ID = Guid.NewGuid().ToString();
            newFuzzyFactorCard.VersionIndex = 0;
            newFuzzyFactorCard.LastVersionIndex = null;
            newFuzzyFactorCard.ParentPage = ParentPage;

            return newFuzzyFactorCard;
        }

        public override void SizeArrayToGridLevel(double toSquareSize = -1, bool recalculateDivisions = true)
        {
            var rightLabelLength = IsHorizontallyAligned ? LargeLabelLength : LabelLength;
            var bottomLabelLength = IsHorizontallyAligned ? LabelLength : LargeLabelLength;
            var initialSquareSize = 45.0;
            if(toSquareSize <= 0)
            {
                while(XPosition + LabelLength + rightLabelLength + initialSquareSize * Columns >= ParentPage.Width || 
                      YPosition + LabelLength + bottomLabelLength + initialSquareSize * Rows >= ParentPage.Height)
                {
                    initialSquareSize = Math.Abs(initialSquareSize - 45.0) < .0001 ? 22.5 : initialSquareSize / 4 * 3;
                }
            }
            else
            {
                initialSquareSize = toSquareSize;
            }

            Height = (initialSquareSize * Rows) + LabelLength + bottomLabelLength;
            Width = (initialSquareSize * Columns) + LabelLength + rightLabelLength;

            if(recalculateDivisions)
            {
              //  ResizeDivisions();
                RaisePropertyChanged("LastDivisionPosition");
            }
        }

        public void AnalyzeArrays()
        {
            //int arrayArea = 0;
            //foreach(var pageObject in ParentPage.PageObjects)
            //{
            //    if(pageObject.PageObjectType == "CLPArray")
            //    {
            //        arrayArea += (pageObject as CLPArray).Rows * (pageObject as CLPArray).Columns;
            //        if((pageObject as CLPArray).Columns == Dividend || ((pageObject as CLPArray).Rows == Dividend))
            //        {
            //            //Array with product as array dimension added
            //            var hasTag = false;
            //            foreach(Tag tag in ParentPage.PageTags.ToList())
            //            {
            //                if(tag.TagType.Name == FuzzyFactorCardIncorrectArrayTagType.Instance.Name)
            //                {
            //                    if(!tag.Value.Contains(new TagOptionValue("product as dimension")))
            //                    {
            //                        tag.Value.Add(new TagOptionValue("product as dimension"));
            //                    }
            //                    hasTag = true;
            //                    continue;
            //                }
            //            }
            //            if(!hasTag)
            //            {
            //                var tag = new Tag(Tag.Origins.Generated, FuzzyFactorCardIncorrectArrayTagType.Instance);
            //                tag.AddTagOptionValue(new TagOptionValue("product as dimension"));
            //                ParentPage.PageTags.Add(tag);
            //            }
            //        }
            //        if((pageObject as CLPArray).Rows != Rows && (pageObject as CLPArray).Columns == Rows)
            //        {
            //            //Array with wrong orientation added
            //            var hasTag = false;
            //            foreach(Tag tag in ParentPage.PageTags.ToList())
            //            {
            //                if(tag.TagType.Name == FuzzyFactorCardIncorrectArrayTagType.Instance.Name)
            //                {
            //                    if(!tag.Value.Contains(new TagOptionValue("wrong orientation")))
            //                    {
            //                        tag.Value.Add(new TagOptionValue("wrong orientation"));
            //                    }
            //                    hasTag = true;
            //                    continue;
            //                }
            //            }
            //            if(!hasTag)
            //            {
            //                var tag = new Tag(Tag.Origins.Generated, FuzzyFactorCardIncorrectArrayTagType.Instance);
            //                tag.AddTagOptionValue(new TagOptionValue("wrong orientation"));
            //                ParentPage.PageTags.Add(tag);
            //            }
            //        }
            //        else if((pageObject as CLPArray).Rows != Rows)
            //        {
            //            //Array with incorrect dimension added
            //            var hasTag = false;
            //            foreach(Tag tag in ParentPage.PageTags.ToList())
            //            {
            //                if(tag.TagType.Name == FuzzyFactorCardIncorrectArrayTagType.Instance.Name)
            //                {
            //                    if(!tag.Value.Contains(new TagOptionValue("incorrect dimension")))
            //                    {
            //                        tag.Value.Add(new TagOptionValue("incorrect dimension"));
            //                    }
            //                    hasTag = true;
            //                    continue;
            //                }
            //            }
            //            if(!hasTag)
            //            {
            //                var tag = new Tag(Tag.Origins.Generated, FuzzyFactorCardIncorrectArrayTagType.Instance);
            //                tag.AddTagOptionValue(new TagOptionValue("incorrect dimension"));
            //                ParentPage.PageTags.Add(tag);
            //            }
            //        }
            //    }
            //}
            //if(arrayArea > CurrentRemainder)
            //{
            //    //Too many arrays added
            //    var hasTag = false;
            //    foreach(Tag tag in ParentPage.PageTags.ToList())
            //    {
            //        if(tag.TagType.Name == FuzzyFactorCardIncorrectArrayTagType.Instance.Name)
            //        {
            //            if(!tag.Value.Contains(new TagOptionValue("too many")))
            //            {
            //                tag.Value.Add(new TagOptionValue("too many"));
            //            }
            //            hasTag = true;
            //            continue;
            //        }
            //    }
            //    if(!hasTag)
            //    {
            //        var tag = new Tag(Tag.Origins.Generated, FuzzyFactorCardIncorrectArrayTagType.Instance);
            //        tag.AddTagOptionValue(new TagOptionValue("too many"));
            //        ParentPage.PageTags.Add(tag);
            //    }
            //}
        }

        public void UpdateRemainderRegion()
        {
            //if(!IsRemainderRegionDisplayed)
            //{
            //    return;
            //}
            //if(CurrentRemainder > 0)
            //{
            //    CLPFuzzyFactorCardRemainder remainderRegion;
            //    if(RemainderRegionUniqueID == null)
            //    {
            //        if(RemainderRegion == null)
            //        {
            //            remainderRegion = new CLPFuzzyFactorCardRemainder(this, ParentPage);
            //        }
            //        else
            //        {
            //            remainderRegion = RemainderRegion;
            //            RemainderRegion = null;
            //        }
            //        ParentPage.PageObjects.Add(remainderRegion);
            //        RemainderRegionUniqueID = remainderRegion.UniqueID;
            //    }
            //    else
            //    {
            //        try
            //        {
            //            remainderRegion = ParentPage.GetPageObjectByUniqueID(RemainderRegionUniqueID) as CLPFuzzyFactorCardRemainder;
            //        }
            //        catch
            //        {
            //            Console.WriteLine("Couldn't find FFC Remainder Region");
            //            return;
            //        }
            //    }
            //    int numberOfBlackTiles = 0;
            //    foreach(var pageObject in ParentPage.PageObjects)
            //    {
            //        //TO DO Liz - update for rotating FFC
            //        if(pageObject.PageObjectType == "CLPArray" && (pageObject as CLPArray).Rows == Rows)
            //        {
            //            numberOfBlackTiles += (pageObject as CLPArray).Rows * (pageObject as CLPArray).Columns;
            //        }
            //    }
            //    numberOfBlackTiles = Math.Min(numberOfBlackTiles, CurrentRemainder);

            //    remainderRegion.TileOffsets.Clear();
            //    for(int i = 0; i < CurrentRemainder - numberOfBlackTiles; i++)
            //    {
            //        remainderRegion.TileOffsets.Add("DodgerBlue");
            //    }
            //    for(int i = 0; i < numberOfBlackTiles; i++)
            //    {
            //        remainderRegion.TileOffsets.Add("Black");
            //    }

            //    remainderRegion.Height = Math.Ceiling((double)remainderRegion.TileOffsets.Count / 5.0) * (remainderRegion.SquareSize + 16.0);
            //    remainderRegion.Width = 5.0 * (remainderRegion.SquareSize + 16.0);
            //}
            //else if(RemainderRegionUniqueID != null)
            //{
            //    CLPFuzzyFactorCardRemainder remainderRegion = ParentPage.GetPageObjectByUniqueID(RemainderRegionUniqueID) as CLPFuzzyFactorCardRemainder;
            //    ParentPage.PageObjects.Remove(remainderRegion);
            //    RemainderRegionUniqueID = null;
            //    RemainderRegion = remainderRegion;
            //}
        }

        #endregion //Methods
    }
}