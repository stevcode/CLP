using Classroom_Learning_Partner.Model.CLPPageObjects;
using System.Collections.ObjectModel;
using Catel.MVVM;
using Catel.Data;

namespace Classroom_Learning_Partner.ViewModels.PageObjects
{
    public class CLPSnapTileContainerViewModel : CLPPageObjectBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the CLPSnapTileViewModel class.
        /// </summary>
        public CLPSnapTileContainerViewModel(CLPSnapTileContainer tile)
            : base()
        {
            PageObject = tile;

            Tiles.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Tiles_CollectionChanged);
        }

        public override string Title { get { return "SnapTileContainerVM"; } }

        void Tiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Height = CLPSnapTileContainer.TILE_HEIGHT * Tiles.Count;
        }


        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<string> Tiles
        {
            get { return GetValue<ObservableCollection<string>>(TilesProperty); }
            set { SetValue(TilesProperty, value); }
        }

        /// <summary>
        /// Register the Tiles property so it is known in the class.
        /// </summary>
        public static readonly PropertyData TilesProperty = RegisterProperty("Tiles", typeof(ObservableCollection<string>));


    }
}
