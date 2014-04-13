using System;
using System.Collections.ObjectModel;
using System.Linq;
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
            : base(parentPage, columns, rows)
        {
            Dividend = dividend;
            IsSnappable = false;
            if(isRemainderRegionDisplayed)
            {
                RemainderTiles = new RemainderTiles(parentPage, this);
                parentPage.PageObjects.Add(RemainderTiles);
            }
        }

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

        public override double ArrayWidth
        {
            get { return Width - (LargeLabelLength + LabelLength); }
        }

        public override double ArrayHeight
        {
            get { return Height - (2 * LabelLength); }
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

        public int GroupsSubtracted
        {
            get
            {
                return VerticalDivisions.Sum(division => division.Value);
            }
        }
        public int CurrentRemainder
        {
            get
            {
                return Dividend - GroupsSubtracted * Rows;
            }
        }

        public double LastDivisionPosition
        {
            get
            {
                return VerticalDivisions.Any() ? VerticalDivisions.Last().Position : 0.0;
            }
        }

        /// <summary>
        /// The total number the <see cref="FuzzyFactorCard" /> represents.
        /// </summary>
        public int Dividend
        {
            get { return GetValue<int>(DividendProperty); }
            set
            {
                SetValue(DividendProperty, value);
                RaisePropertyChanged("CurrentRemainder");
            }
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

        #region Navigation Properties

        /// <summary>
        /// Unique Identifier for the <see cref="FuzzyFactorCard" />'s <see cref="RemainderTiles" />.
        /// </summary>
        /// <remarks>
        /// Composite Foreing Key.
        /// </remarks>
        public string RemainderTilesID
        {
            get { return GetValue<string>(RemainderTilesIDProperty); }
            set { SetValue(RemainderTilesIDProperty, value); }
        }

        public static readonly PropertyData RemainderTilesIDProperty = RegisterProperty("RemainderTilesID", typeof(string), string.Empty);

        /// <summary>
        /// Unique Identifier of the <see cref="Person" /> who owns the <see cref="RemainderTiles" />.
        /// </summary>
        public string RemainderTilesOwnerID
        {
            get { return GetValue<string>(RemainderTilesOwnerIDProperty); }
            set { SetValue(RemainderTilesOwnerIDProperty, value); }
        }

        public static readonly PropertyData RemainderTilesOwnerIDProperty = RegisterProperty("RemainderTilesOwnerID", typeof(string), string.Empty);

        /// <summary>
        /// Version Index for the <see cref="RemainderTiles" />.
        /// </summary>
        public uint RemainderTilesVersionIndex
        {
            get { return GetValue<uint>(RemainderTilesVersionIndexProperty); }
            set { SetValue(RemainderTilesVersionIndexProperty, value); }
        }

        public static readonly PropertyData RemainderTilesVersionIndexProperty = RegisterProperty("RemainderTilesVersionIndex", typeof(uint), 0);

        /// <summary>
        /// <see cref="RemainderTiles" /> for the <see cref="FuzzyFactorCard" />.
        /// </summary>
        /// <remarks>
        /// Virtual to facilitate lazy loading of navigation property by Entity Framework.
        /// </remarks>
        public virtual RemainderTiles RemainderTiles
        {
            get { return GetValue<RemainderTiles>(RemainderTilesProperty); }
            set
            {
                SetValue(RemainderTilesProperty, value);
                if(value == null)
                {
                    return;
                }
                RemainderTilesID = value.ID;
                RemainderTilesOwnerID = value.OwnerID;
                RemainderTilesVersionIndex = value.VersionIndex;
            }
        }

        public static readonly PropertyData RemainderTilesProperty = RegisterProperty("RemainderTiles", typeof(RemainderTiles)); 

        #endregion //Navigation Properties

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

        public override void OnResized()
        {
            base.OnResized();
            RaisePropertyChanged("LastDivisionPosition");
        }

        public override void OnDeleted()
        {
            base.OnDeleted();

            // FFC deleted tag
            //ObservableCollection<Tag> tags = ParentPage.PageTags;
            //ProductRelation relation = null;
            //foreach(Tag tag in tags)
            //{
            //    if(tag.TagType.Name == PageDefinitionTagType.Instance.Name)
            //    {
            //        relation = (ProductRelation)tag.Value[0].Value;
            //        break;
            //    }
            //}

            //if(relation != null)
            //{
            //    int factor1 = Convert.ToInt32(relation.Factor1);
            //    int factor2 = Convert.ToInt32(relation.Factor2);
            //    int product = Convert.ToInt32(relation.Product);

            //    string tagValue;
            //    if(product == Dividend && ((factor1 == Rows && relation.Factor1Given) || (factor2 == Rows && relation.Factor2Given)))
            //    {
            //        tagValue = "deleted correct division object";
            //    }
            //    else
            //    {
            //        tagValue = "deleted incorrect division object";
            //    }

            //    bool hasTag = false;
            //    foreach(Tag tag in ParentPage.PageTags.ToList())
            //    {
            //        if(tag.TagType.Name == FuzzyFactorCardDeletedTagType.Instance.Name)
            //        {
            //            tag.Value.Add(new TagOptionValue(tagValue));
            //            hasTag = true;
            //            continue;
            //        }
            //    }
            //    if(!hasTag)
            //    {
            //        var tag = new Tag(Tag.Origins.Generated, FuzzyFactorCardDeletedTagType.Instance);
            //        tag.AddTagOptionValue(new TagOptionValue(tagValue));
            //        ParentPage.PageTags.Add(tag);
            //    }
            //}
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
                ResizeDivisions();
            }
            OnResized();
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
            if(RemainderTiles == null ||
               CurrentRemainder <= 0)
            {
                return;
            }

            var numberOfBlackTiles = ParentPage.PageObjects.Where(pageObject => pageObject is CLPArray && (pageObject as CLPArray).ArrayType == ArrayTypes.Array && (pageObject as CLPArray).Rows == Rows).Sum(pageObject =>
                                                                                                                                                                                                                   {
                                                                                                                                                                                                                       var clpArray = pageObject as CLPArray;
                                                                                                                                                                                                                       return clpArray != null ? clpArray.Rows * clpArray.Columns : 0;
                                                                                                                                                                                                                   });
            numberOfBlackTiles = Math.Min(numberOfBlackTiles, CurrentRemainder);

            RemainderTiles.TileOffsets.Clear();
            for(var i = 0; i < CurrentRemainder - numberOfBlackTiles; i++)
            {
                RemainderTiles.TileOffsets.Add("DodgerBlue");
            }
            for(var i = 0; i < numberOfBlackTiles; i++)
            {
                RemainderTiles.TileOffsets.Add("Black");
            }

            RemainderTiles.Height = Math.Ceiling(RemainderTiles.TileOffsets.Count / 5.0) * 61.0;
            RemainderTiles.Width = 66.0;
        }

        public void SnapInArray(int value)
        {
            if(IsHorizontallyAligned)
            {
                var position = LastDivisionPosition + value * (ArrayHeight / Rows);
                var divAbove = FindDivisionAbove(position, VerticalDivisions);
                var divBelow = FindDivisionBelow(position, VerticalDivisions);

                CLPArrayDivision topDiv;
                if(divAbove == null)
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, 0, position, value);
                }
                else
                {
                    topDiv = new CLPArrayDivision(ArrayDivisionOrientation.Vertical, divAbove.Position, position - divAbove.Position, value);
                    VerticalDivisions.Remove(divAbove);
                }
                VerticalDivisions.Add(topDiv);
                CLPArrayDivision bottomDiv;
                bottomDiv = divBelow == null ? new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, ArrayWidth - position, 0) : new CLPArrayDivision(ArrayDivisionOrientation.Vertical, position, divBelow.Position - position, 0);
                VerticalDivisions.Add(bottomDiv);
                UpdateRemainderRegion();
            }

            //TODO Liz: Add ability to snap in arrays when rotated

            RaisePropertyChanged("GroupsSubtracted");
            RaisePropertyChanged("CurrentRemainder");
            RaisePropertyChanged("LastDivisionPosition");
        }

        public void RemoveLastDivision()
        {
            if(VerticalDivisions.Count <= 1)
            {
                return;
            }
            var lastDiv = VerticalDivisions.Last();
            var prevDiv = VerticalDivisions[VerticalDivisions.Count - 2];

            VerticalDivisions.Add(new CLPArrayDivision(ArrayDivisionOrientation.Vertical, prevDiv.Position, prevDiv.Length + lastDiv.Length, 0));
            VerticalDivisions.Remove(lastDiv);
            VerticalDivisions.Remove(prevDiv);

            RaisePropertyChanged("GroupsSubtracted");
            RaisePropertyChanged("CurrentRemainder");
            RaisePropertyChanged("LastDivisionPosition");

            UpdateRemainderRegion();
        }

        #endregion //Methods
    }
}