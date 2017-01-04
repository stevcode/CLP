using Catel.Data;
using Catel.MVVM;
using CLP.Entities.Demo;

namespace Classroom_Learning_Partner.ViewModels
{
    public class TemporaryBoundaryViewModel : APageObjectBaseViewModel
    {
        public TemporaryBoundaryViewModel(TemporaryBoundary boundary) { PageObject = boundary; }

        [ViewModelToModel("PageObject")]
        public string RegionText
        {
            get { return GetValue<string>(RegionTextProperty); }
            set { SetValue(RegionTextProperty, value); }
        }

        public static readonly PropertyData RegionTextProperty = RegisterProperty("RegionText", typeof(string));
    }
}