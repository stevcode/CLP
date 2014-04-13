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
            //if (dataContext is CLPImage) return typeof(CLPImageViewModel);
            //if (dataContext is CLPInkShapeRegion) return typeof(CLPInkShapeRegionViewModel);
            //if (dataContext is CLPShadingRegion) return typeof(CLPShadingRegionViewModel);
            if(dataContext is Shape)
            {
                return typeof(ShapeViewModel);
            }
            //if (dataContext is CLPStamp) return typeof(CLPStampViewModel);
            //if (dataContext is CLPStampCopy) return typeof(CLPStampCopyViewModel);
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
            //if (dataContext is CLPRegion) return typeof(CLPRegionViewModel);

            return null;
        }
    }
}