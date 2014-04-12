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
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class CLPFuzzyFactorCardViewModel : CLPArrayViewModel
    {
                
        //#region Constructor

        ///// <summary>
        ///// Initializes a new instance of the <see cref="CLPFuzzyFactorCardViewModel"/> class.
        ///// </summary>
        //public CLPFuzzyFactorCardViewModel(CLPFuzzyFactorCard factorCard) : base(factorCard)
        //{
        //    RemoveFuzzyFactorCardCommand = new Command(OnRemoveFuzzyFactorCardCommandExecute);
        //    ResizeFuzzyFactorCardCommand = new Command<DragDeltaEventArgs>(OnResizeFuzzyFactorCardCommandExecute);
        //    RemoveLastArrayCommand = new Command(OnRemoveLastArrayCommandExecute);
        //}

        //#endregion //Constructor    

        //#region Properties

        ///// <summary>
        ///// True if FFC is aligned so that fuzzy edge is on the right
        ///// </summary>
        //[ViewModelToModel("PageObject")]
        //public bool IsHorizontallyAligned
        //{
        //    get
        //    {
        //        return GetValue<bool>(IsHorizontallyAlignedProperty);
        //    }
        //    set
        //    {
        //        SetValue(IsHorizontallyAlignedProperty, value);
        //    }
        //}

        //public static readonly PropertyData IsHorizontallyAlignedProperty = RegisterProperty("IsHorizontallyAligned", typeof(bool));

        ///// <summary>
        ///// Value of the Dividend.
        ///// </summary>
        //[ViewModelToModel("PageObject")]
        //public int Dividend
        //{
        //    get
        //    {
        //        return GetValue<int>(DividendProperty);
        //    }
        //    set
        //    {
        //        SetValue(DividendProperty, value);
        //    }
        //}

        ///// <summary>
        ///// Register the Dividend property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData DividendProperty = RegisterProperty("Dividend", typeof(int));

        ///// <summary>
        ///// Color of the border - usually black but flashes red when extra arrays are snapped to it.
        ///// </summary>
        //public string BorderColor
        //{
        //    get
        //    {
        //        return GetValue< string>(BorderColorProperty);
        //    }
        //    set
        //    {
        //        SetValue(BorderColorProperty, value);
        //    }
        //}

        //public static readonly PropertyData BorderColorProperty = RegisterProperty("BorderColor", typeof( string), "Black");


        ///// <summary>
        ///// Color of the fuzzy edge - usually gray but flashes red when extra arrays are snapped to it.
        ///// </summary>
        //public string FuzzyEdgeColor
        //{
        //    get
        //    {
        //        return GetValue<string>(FuzzyEdgeColorProperty);
        //    }
        //    set
        //    {
        //        SetValue(FuzzyEdgeColorProperty, value);
        //    }
        //}

        //public static readonly PropertyData FuzzyEdgeColorProperty = RegisterProperty("FuzzyEdgeColor", typeof(string), "DarkGray");

        ///// <summary>
        ///// ID of remainder region if it exists.
        ///// </summary>
        //[ViewModelToModel("PageObject")]
        //public string RemainderRegionUniqueID
        //{
        //    get
        //    {
        //        return GetValue<string>(RemainderRegionUniqueIDProperty);
        //    }
        //    set
        //    {
        //        SetValue(RemainderRegionUniqueIDProperty, value);
        //    }
        //}

        ///// <summary>
        ///// Register the RemainderRegionUniqueID property so it is known in the class.
        ///// </summary>
        //public static readonly PropertyData RemainderRegionUniqueIDProperty = RegisterProperty("RemainderRegionUniqueID", typeof(string));

        //#endregion //Properties

        //#region Methods

        //public void RejectSnappedArray()
        //{
        //    BorderColor = "Red";
        //    FuzzyEdgeColor = "Red";
        //    System.Threading.Tasks.Task.Run(async delegate
        //    {
        //        await System.Threading.Tasks.Task.Delay(400);
        //        BorderColor = "Black";
        //        FuzzyEdgeColor = "Gray";
        //    });
        //}

        //#endregion //Methods

        //#region Commands

        ///// <summary>
        ///// Removes pageObject from page when Delete button is pressed.
        ///// </summary>
        //public new Command RemoveFuzzyFactorCardCommand
        //{
        //    get;
        //    set;
        //}

        //private void OnRemoveFuzzyFactorCardCommandExecute()
        //{
        //    if(RemainderRegionUniqueID != null)
        //    {
        //        CLPFuzzyFactorCardRemainder remainderRegion = PageObject.ParentPage.GetPageObjectByUniqueID(RemainderRegionUniqueID) as CLPFuzzyFactorCardRemainder;
        //        var currentIndex = PageObject.ParentPage.PageObjects.IndexOf(remainderRegion);
        //        ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryPageObjectRemove(PageObject.ParentPage, remainderRegion, currentIndex));
        //        PageObject.ParentPage.PageObjects.Remove(remainderRegion);
        //    }
        //    ACLPPageBaseViewModel.RemovePageObjectFromPage(PageObject);
        //}

        ///// <summary>
        ///// Gets the ResizePageObjectCommand command.
        ///// </summary>
        //public Command<DragDeltaEventArgs> ResizeFuzzyFactorCardCommand
        //{
        //    get;
        //    set;
        //}

        //private void OnResizeFuzzyFactorCardCommandExecute(DragDeltaEventArgs e)
        //{
        //    var clpArray = PageObject as CLPFuzzyFactorCard;
        //    if(clpArray == null)
        //    {
        //        return;
        //    }
        //    var oldHeight = Height;
        //    var oldWidth = Width;

        //    double newArrayHeight;
        //    var isVerticalChange = e.VerticalChange > e.HorizontalChange;
        //    if(isVerticalChange)
        //    {
        //        newArrayHeight = ArrayHeight + e.VerticalChange;
        //    }
        //    else
        //    {
        //        newArrayHeight = (ArrayWidth + e.HorizontalChange) / Columns * Rows;
        //    }

        //    //TODO Liz - make min dimension depend on horizontal vs vertical alignment
        //    const double MIN_HEIGHT = 150.0; 
        //    const double MIN_WIDTH = 50.0;

        //    //Control Min Dimensions of Array.
        //    if(newArrayHeight < MIN_HEIGHT)
        //    {
        //        newArrayHeight = MIN_HEIGHT;
        //    }
        //    var newSquareSize = newArrayHeight / Rows;
        //    var newArrayWidth = newSquareSize * Columns;
        //    if(newArrayWidth < MIN_WIDTH)
        //    {
        //        newArrayWidth = MIN_WIDTH;
        //        newSquareSize = newArrayWidth / Columns;
        //        newArrayHeight = newSquareSize * Rows;
        //    }

        //    //Control Max Dimensions of Array.
        //    if(newArrayHeight + 2 * clpArray.LabelLength + YPosition > clpArray.ParentPage.Height)
        //    {
        //        newArrayHeight = clpArray.ParentPage.Height - YPosition - 2 * clpArray.LabelLength;
        //        newSquareSize = newArrayHeight / Rows;
        //        newArrayWidth = newSquareSize * Columns;
        //    }
        //    //TODO Liz - update this when rotating is enabled
        //    if(newArrayWidth + clpArray.LargeLabelLength + clpArray.LabelLength + XPosition > clpArray.ParentPage.Width)
        //    {
        //        newArrayWidth = clpArray.ParentPage.Width - XPosition - clpArray.LargeLabelLength - clpArray.LabelLength;
        //        newSquareSize = newArrayWidth / Columns;
        //    }

        //    clpArray.SizeArrayToGridLevel(newSquareSize);

        //    //Resize History
        //    var heightDiff = Math.Abs(oldHeight - Height);
        //    var widthDiff = Math.Abs(oldWidth - Width);
        //    var diff = heightDiff + widthDiff;
        //    if(!(diff > CLPHistory.SAMPLE_RATE))
        //    {
        //        return;
        //    }

        //    var batch = PageObject.ParentPage.PageHistory.CurrentHistoryBatch;
        //    if(batch is CLPHistoryPageObjectResizeBatch)
        //    {
        //        (batch as CLPHistoryPageObjectResizeBatch).AddResizePointToBatch(PageObject.UniqueID,
        //                                                                         new Point(Width, Height));
        //    }
        //    else
        //    {
        //        var batchHistoryItem = PageObject.ParentPage.PageHistory.EndBatch();
        //        ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, batchHistoryItem, true);
        //        //TODO: log this error
        //    }
        //}

        ///// <summary>
        ///// Gets the RemoveLastArrayCommand command.
        ///// </summary>
        //public Command RemoveLastArrayCommand { get; private set; }

        //private void OnRemoveLastArrayCommandExecute()
        //{
        //    if(VerticalDivisions.Count > 1)
        //    {
        //        var divisionValue = (VerticalDivisions[VerticalDivisions.Count - 2]).Value;
        //        (PageObject as CLPFuzzyFactorCard).RemoveLastDivision();

        //        ACLPPageBaseViewModel.AddHistoryItemToPage(PageObject.ParentPage, new CLPHistoryFFCDivisionRemoved(PageObject.ParentPage, PageObject.UniqueID, divisionValue));
        //    }
        //}

        //#endregion //Commands
    }
}
