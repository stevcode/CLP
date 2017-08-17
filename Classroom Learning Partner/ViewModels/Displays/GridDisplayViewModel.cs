using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Catel.Data;
using Classroom_Learning_Partner.Services;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class GridDisplayViewModel : AMultiDisplayViewModelBase
    {
        #region Constructor

        /// <summary>Initializes a new instance of the GridDisplayViewModel class.</summary>
        public GridDisplayViewModel(GridDisplay gridDisplay, IDataService dataService)
            : base(gridDisplay, dataService)
        {
            Pages.CollectionChanged += Pages_CollectionChanged;
            UGridRows = Pages.Count < 3 ? 1 : 0;
        }

        #region Overrides of ViewModelBase

        protected override async Task OnClosingAsync()
        {
            Pages.CollectionChanged -= Pages_CollectionChanged;
            await base.OnClosingAsync();
        }

        #endregion

        #endregion //Constructor

        #region Bindings

        /// <summary>Number of Rows in the UniformGrid</summary>
        public int UGridRows
        {
            get { return GetValue<int>(UGridRowsProperty); }
            set { SetValue(UGridRowsProperty, value); }
        }

        public static readonly PropertyData UGridRowsProperty = RegisterProperty("UGridRows", typeof(int), 1);

        #endregion //Bindings

        #region Methods

        private void Pages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UGridRows = Pages.Count < 3 ? 1 : 0;

            if (App.Network.ProjectorProxy == null ||
                App.MainWindowViewModel.CurrentProgramMode != ProgramRoles.Teacher ||
                IsDisplayPreview)
            {
                return;
            }

            if (e.OldItems != null)
            {
                foreach (var page in e.OldItems.OfType<CLPPage>())
                {
                    try
                    {
                        App.Network.ProjectorProxy.RemovePageFromDisplay(page.ID, page.OwnerID, page.DifferentiationLevel, page.VersionIndex, MultiDisplay.ID);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (var page in e.NewItems.OfType<CLPPage>())
                {
                    try
                    {
                        App.Network.ProjectorProxy.AddPageToDisplay(page.ID, page.OwnerID, page.DifferentiationLevel, page.VersionIndex, MultiDisplay.ID);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        #endregion //Methods
    }
}