using System;
using Classroom_Learning_Partner.ViewModels;
using CLP.Entities;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for PageObjectContainerView.xaml
    /// </summary>
    public partial class PageObjectContainerView
    {
        public PageObjectContainerView() { InitializeComponent(); }

        protected override Type GetViewModelType() { return typeof(APageObjectBaseViewModel); }

        protected override Type GetViewModelType(object dataContext)
        {
            //if (dataContext is CLPAudio) return typeof(CLPAudioViewModel);
            //if (dataContext is CLPDataTable) return typeof(CLPDataTableViewModel);
            //if (dataContext is CLPHandwritingRegion) return typeof(CLPHandwritingRegionViewModel);
            //if (dataContext is CLPGroupingRegion) return typeof(CLPGroupingRegionViewModel);
            if(dataContext is CLPImage)
            {
                return typeof(CLPImageViewModel);
            }
            //if (dataContext is CLPInkShapeRegion) return typeof(CLPInkShapeRegionViewModel);
            //if (dataContext is CLPShadingRegion) return typeof(CLPShadingRegionViewModel);
            if(dataContext is Shape)
            {
                return typeof(ShapeViewModel);
            }
            if(dataContext is Stamp)
            {
                return typeof(StampViewModel);
            }
            if(dataContext is StampedObject)
            {
                return typeof(StampedObjectViewModel);
            }
            if(dataContext is CLPTextBox)
            {
                return typeof(CLPTextBoxViewModel);
            }
            //if (dataContext is CLPAggregationDataTable) return typeof(CLPAggregationDataTableViewModel);
            if(dataContext is FuzzyFactorCard)
            {
                return typeof(FuzzyFactorCardViewModel);
            }
            if(dataContext is RemainderTiles)
            {
                return typeof(RemainderTilesViewModel);
            }
            if(dataContext is CLPArray)
            {
                return typeof(CLPArrayViewModel);
            }
            if (dataContext is NumberLine)
            {
                return typeof (NumberLineViewModel);
            }
            if(dataContext is LassoRegion)
            {
                return typeof(LassoRegionViewModel);
            }
            if (dataContext is TemporaryBoundary)
            {
                return typeof(TemporaryBoundaryViewModel);
            }
            if (dataContext is MultipleChoice)
            {
                return typeof(MultipleChoiceViewModel);
            }
            if (dataContext is Mark)
            {
                return typeof(MarkViewModel);
            }
            if (dataContext is Bin)
            {
                return typeof(BinViewModel);
            }
            if (dataContext is BinReporter)
            {
                return typeof(BinReporterViewModel);
            }

            return null;
        }
    }
}