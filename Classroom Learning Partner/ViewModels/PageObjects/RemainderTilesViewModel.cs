using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities.Old;

namespace Classroom_Learning_Partner.ViewModels
{
    public class RemainderTilesViewModel : APageObjectBaseViewModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RemainderTilesViewModel" /> class.
        /// </summary>
        public RemainderTilesViewModel(RemainderTiles remainderTiles)
        {
            PageObject = remainderTiles;
            hoverTimer.Interval = 2300;
            CloseAdornerTimeOut = 0.15;
        }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<string> TileOffsets
        {
            get { return GetValue<ObservableCollection<string>>(TileOffsetsProperty); }
            set { SetValue(TileOffsetsProperty, value); }
        }

        public static readonly PropertyData TileOffsetsProperty = RegisterProperty("TileOffsets", typeof(ObservableCollection<string>));

        #endregion //Model
    }
}