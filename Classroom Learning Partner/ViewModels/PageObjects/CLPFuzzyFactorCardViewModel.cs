using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Catel.Data;
using Catel.MVVM;
using Classroom_Learning_Partner.Views.Modal_Windows;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPFuzzyFactorCardViewModel : CLPArrayViewModel
    {
                
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CLPFuzzyFactorCardViewModel"/> class.
        /// </summary>
        public CLPFuzzyFactorCardViewModel(CLPFuzzyFactorCard factorCard) : base(factorCard)
        {
            ResizeFuzzyFactorCardCommand = new Command<DragDeltaEventArgs>(OnResizeFuzzyFactorCardCommandExecute);
            RemoveLastArrayCommand = new Command(OnRemoveLastArrayCommandExecute);
        }

        #endregion //Constructor    

        #region Properties

        /// <summary>
        /// Whether or not the answer is displayed.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsAnswerVisible
        {
            get
            {
                return GetValue<bool>(IsAnswerVisibleProperty);
            }
            set
            {
                SetValue(IsAnswerVisibleProperty, value);
            }
        }

        /// <summary>
        /// True if division labels are on top and answer (if shown) is on bottom.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsArrayDivisionLabelOnTop
        {
            get
            {
                return GetValue<bool>(IsArrayDivisionLabelOnTopProperty);
            }
            set
            {
                SetValue(IsArrayDivisionLabelOnTopProperty, value);
            }
        }

        public static readonly PropertyData IsArrayDivisionLabelOnTopProperty = RegisterProperty("IsArrayDivisionLabelOnTop", typeof(bool));


        /// <summary>
        /// Register the IsAnswerVisible property so it is known in the class.
        /// </summary>
        public static readonly PropertyData IsAnswerVisibleProperty = RegisterProperty("IsAnswerVisible", typeof(bool));


        /// <summary>
        /// True if FFC is aligned so that fuzzy edge is on the right
        /// </summary>
        [ViewModelToModel("PageObject")]
        public bool IsHorizontallyAligned
        {
            get
            {
                return GetValue<bool>(IsHorizontallyAlignedProperty);
            }
            set
            {
                SetValue(IsHorizontallyAlignedProperty, value);
            }
        }

        public static readonly PropertyData IsHorizontallyAlignedProperty = RegisterProperty("IsHorizontallyAligned", typeof(bool));


        /// <summary>
        /// Value of the Dividend.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public int Dividend
        {
            get
            {
                return GetValue<int>(DividendProperty);
            }
            set
            {
                SetValue(DividendProperty, value);
            }
        }

        /// <summary>
        /// Register the Dividend property so it is known in the class.
        /// </summary>
        public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(int));

        /// <summary>
        /// Color of the border - usually black but flashes red when extra arrays are snapped to it.
        /// </summary>
        public string BorderColor
        {
            get
            {
                return GetValue< string>(BorderColorProperty);
            }
            set
            {
                SetValue(BorderColorProperty, value);
            }
        }

        public static readonly PropertyData BorderColorProperty = RegisterProperty("BorderColor", typeof( string), "Black");


        /// <summary>
        /// Color of the fuzzy edge - usually gray but flashes red when extra arrays are snapped to it.
        /// </summary>
        public string FuzzyEdgeColor
        {
            get
            {
                return GetValue<string>(FuzzyEdgeColorProperty);
            }
            set
            {
                SetValue(FuzzyEdgeColorProperty, value);
            }
        }

        public static readonly PropertyData FuzzyEdgeColorProperty = RegisterProperty("FuzzyEdgeColor", typeof(string), "Gray");

        #endregion //Properties

        #region Methods

        public void RejectSnappedArray()
        {
            BorderColor = "Red";
            FuzzyEdgeColor = "Red";
            System.Threading.Tasks.Task.Run(async delegate
            {
                await System.Threading.Tasks.Task.Delay(400);
                BorderColor = "Black";
                FuzzyEdgeColor = "Gray";
            });
        }

        #endregion //Methods

        #region Commands

        /// <summary>
        /// Gets the ResizePageObjectCommand command.
        /// </summary>
        public Command<DragDeltaEventArgs> ResizeFuzzyFactorCardCommand
        {
            get;
            set;
        }

        private void OnResizeFuzzyFactorCardCommandExecute(DragDeltaEventArgs e)
        {
            var clpArray = PageObject as CLPFuzzyFactorCard;
            if(clpArray == null)
            {
                return;
            }
            var oldHeight = Height;
            var oldWidth = Width;

            double newArrayHeight;
            var isVerticalChange = e.VerticalChange > e.HorizontalChange;
            if(isVerticalChange)
            {
                newArrayHeight = ArrayHeight + e.VerticalChange;
            }
            else
            {
                newArrayHeight = (ArrayWidth + e.HorizontalChange) / Columns * Rows;
            }

            //TODO Liz - make min dimension depend on horizontal vs vertical alignment
            const double MIN_HEIGHT = 150.0; //16.875; //11.25;
            const double MIN_WIDTH = 50.0;

            //Control Min Dimensions of Array.
            if(newArrayHeight < MIN_HEIGHT)
            {
                newArrayHeight = MIN_HEIGHT;
            }
            var newSquareSize = newArrayHeight / Rows;
            var newArrayWidth = newSquareSize * Columns;
            if(newArrayWidth < MIN_WIDTH)
            {
                newArrayWidth = MIN_WIDTH;
                newSquareSize = newArrayWidth / Columns;
                newArrayHeight = newSquareSize * Rows;
            }

            //Control Max Dimensions of Array.
            if(newArrayHeight + 2 * clpArray.LabelLength + YPosition > clpArray.ParentPage.PageHeight)
            {
                newArrayHeight = clpArray.ParentPage.PageHeight - YPosition - 2 * clpArray.LabelLength;
                newSquareSize = newArrayHeight / Rows;
                newArrayWidth = newSquareSize * Columns;
            }
            //TODO Liz - update this when rotating is enabled
            if(newArrayWidth + clpArray.LargeLabelLength + clpArray.LabelLength + XPosition > clpArray.ParentPage.PageWidth)
            {
                newArrayWidth = clpArray.ParentPage.PageWidth - XPosition - clpArray.LargeLabelLength - clpArray.LabelLength;
                newSquareSize = newArrayWidth / Columns;
                //newArrayHeight = newSquareSize * Rows;
            }

            clpArray.SizeArrayToGridLevel(newSquareSize);

            //Resize History
            var heightDiff = Math.Abs(oldHeight - Height);
            var widthDiff = Math.Abs(oldWidth - Width);
            var diff = heightDiff + widthDiff;
            if(!(diff > CLPHistory.SAMPLE_RATE))
            {
                return;
            }

            var batch = PageObject.ParentPage.PageHistory.CurrentHistoryBatch;
            if(batch is CLPHistoryPageObjectResizeBatch)
            {
                (batch as CLPHistoryPageObjectResizeBatch).AddResizePointToBatch(PageObject.UniqueID,
                                                                                 new Point(Width, Height));
            }
            else
            {
                var batchHistoryItem = PageObject.ParentPage.PageHistory.EndBatch();
                ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
                //TODO: log this error
            }
        }

        /// <summary>
        /// Gets the RemoveLastArrayCommand command.
        /// </summary>
        public Command RemoveLastArrayCommand { get; private set; }

        private void OnRemoveLastArrayCommandExecute()
        {
            (PageObject as CLPFuzzyFactorCard).RemoveLastDivision();
        }

        #endregion //Commands
    }
}
