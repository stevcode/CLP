using System.Collections.Generic;
using System.Windows;
using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class TemporaryGridViewModel : APageObjectBaseViewModel
    {
        public TemporaryGridViewModel(TemporaryGrid grid) { PageObject = grid; }

        [ViewModelToModel("PageObject")]
        public int CellSize
        {
            get { return GetValue<int>(CellSizeProperty); }
            set { SetValue(CellSizeProperty, value); }
        }

        public static readonly PropertyData CellSizeProperty = RegisterProperty("CellSize", typeof(int));

        [ViewModelToModel("PageObject")]
        public List<Point> OccupiedCells
        {
            get { return GetValue<List<Point>>(OccupiedCellsProperty); }
            set { SetValue(OccupiedCellsProperty, value); }
        }

        public static readonly PropertyData OccupiedCellsProperty = RegisterProperty("OccupiedCells", typeof(List<Point>));
    }
}
