using System.Collections.ObjectModel;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

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
        }

        #endregion //Constructor

        #region Model

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ViewModelToModel("PageObject")]
        public ObservableCollection<string> TileColors
        {
            get { return GetValue<ObservableCollection<string>>(TileColorsProperty); }
            set { SetValue(TileColorsProperty, value); }
        }

        public static readonly PropertyData TileColorsProperty = RegisterProperty("TileColors", typeof(ObservableCollection<string>));

        #endregion //Model
    }
}