using System.Collections.Generic;
using Catel.MVVM;
using Classroom_Learning_Partner.ViewModels;
using CLP.Models;

namespace Classroom_Learning_Partner.Views
{
    /// <summary>
    /// Interaction logic for PageObjectContainerView.xaml
    /// </summary>
    public partial class PageObjectContainerView : Catel.Windows.Controls.UserControl
    {
        public PageObjectContainerView()
        {
            InitializeComponent();
        }

        protected override System.Type GetViewModelType()
        {
            return typeof(ACLPPageObjectBaseViewModel);
        }

        protected override System.Type GetViewModelType(object dataContext)
        {
            if (dataContext is CLPAudio) return typeof(CLPAudioViewModel);
            if (dataContext is CLPDataTable) return typeof(CLPDataTableViewModel);
            if (dataContext is CLPHandwritingRegion) return typeof(CLPHandwritingRegionViewModel);
            if (dataContext is CLPGroupingRegion) return typeof(CLPGroupingRegionViewModel);
            if (dataContext is CLPImage) return typeof(CLPImageViewModel);
            if (dataContext is CLPInkShapeRegion) return typeof(CLPInkShapeRegionViewModel);
            if (dataContext is CLPShadingRegion) return typeof(CLPShadingRegionViewModel);
            if (dataContext is CLPShape) return typeof(CLPShapeViewModel);
            if (dataContext is CLPSnapTileContainer) return typeof(CLPSnapTileContainerViewModel);
            if (dataContext is CLPStamp) return typeof(CLPStampViewModel);
            if (dataContext is CLPStrokePathContainer) return typeof(CLPStrokePathContainerViewModel);
            if (dataContext is CLPTextBox) return typeof(CLPTextBoxViewModel);
            if (dataContext is CLPAggregationDataTable) return typeof(CLPAggregationDataTableViewModel);
            if(dataContext is CLPArray) return typeof(CLPArrayViewModel);

            return null;
        }

        public static readonly Dictionary<int, IViewModel> ModelViewModels = new Dictionary<int, IViewModel>();
        protected override IViewModel GetViewModelInstance(object dataContext)
        {
            if(dataContext == null)
            {
                // Let catel handle this one
                return null;
            }

            if(!ModelViewModels.ContainsKey(dataContext.GetHashCode()))
            {
                IViewModel vm;

                if(dataContext is CLPAudio) vm = new CLPAudioViewModel(dataContext as CLPAudio);
                else if(dataContext is CLPDataTable) vm = new CLPDataTableViewModel(dataContext as CLPDataTable);
                else if(dataContext is CLPHandwritingRegion) vm = new CLPHandwritingRegionViewModel(dataContext as CLPHandwritingRegion);
                else if(dataContext is CLPGroupingRegion) vm = new CLPGroupingRegionViewModel(dataContext as CLPGroupingRegion);
                else if(dataContext is CLPImage) vm = new CLPImageViewModel(dataContext as CLPImage);
                else if(dataContext is CLPInkShapeRegion) vm = new CLPInkShapeRegionViewModel(dataContext as CLPInkShapeRegion);
                else if(dataContext is CLPShadingRegion) vm = new CLPShadingRegionViewModel(dataContext as CLPShadingRegion);
                else if(dataContext is CLPShape) vm = new CLPShapeViewModel(dataContext as CLPShape);
                else if(dataContext is CLPSnapTileContainer) vm = new CLPSnapTileContainerViewModel(dataContext as CLPSnapTileContainer);
                else if(dataContext is CLPStamp) vm = new CLPStampViewModel(dataContext as CLPStamp);
                else if(dataContext is CLPStrokePathContainer) vm = new CLPStrokePathContainerViewModel(dataContext as CLPStrokePathContainer);
                else if(dataContext is CLPTextBox) vm = new CLPTextBoxViewModel(dataContext as CLPTextBox);
                else if(dataContext is CLPAggregationDataTable) vm = new CLPAggregationDataTableViewModel(dataContext as CLPAggregationDataTable);
                else if(dataContext is CLPArray) vm = new CLPArrayViewModel(dataContext as CLPArray);
                
                else
                {
                    vm = new CLPImageViewModel(dataContext as CLPImage);
                }

                ModelViewModels.Add(dataContext.GetHashCode(), vm);
            }

            // Reuse VM
            return ModelViewModels[dataContext.GetHashCode()];
        }
    }
}
