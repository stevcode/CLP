using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using CLP.Models;

namespace Classroom_Learning_Partner.ViewModels
{
    /// <summary>
    /// UserControl view model.
    /// </summary>
    public class CLPGroupingRegionViewModel : ACLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CLPGroupingRegionViewModel"/> class.
        /// </summary>
        public CLPGroupingRegionViewModel(CLPGroupingRegion groupingRegion) : base()
        {
            PageObject = groupingRegion;
        }

        /// <summary>
        /// Gets the title of the view model.
        /// </summary>
        /// <value>The title.</value>
        public override string Title { get { return "GroupingRegionVM"; } }

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public string GroupsString
        {
            get { return GetValue<string>(GroupsStringProperty); }
            set { SetValue(GroupsStringProperty, value); }
        }

        /// <summary>
        /// Register the GroupsString property so it is known in the class.
        /// </summary>
        public static readonly PropertyData GroupsStringProperty = RegisterProperty("GroupsString", typeof(string));

        #endregion //Model

    }
}
