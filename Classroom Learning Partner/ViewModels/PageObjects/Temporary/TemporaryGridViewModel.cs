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
        public int CellWidth
        {
            get { return GetValue<int>(CellWidthProperty); }
            set { SetValue(CellWidthProperty, value); }
        }

        public static readonly PropertyData CellWidthProperty = RegisterProperty("CellWidth", typeof(int), 0);

        [ViewModelToModel("PageObject")]
        public int CellHeight
        {
            get { return GetValue<int>(CellHeightProperty); }
            set { SetValue(CellHeightProperty, value); }
        }

        public static readonly PropertyData CellHeightProperty = RegisterProperty("CellHeight", typeof(int), 0);

        [ViewModelToModel("PageObject")]
        public List<Point> OccupiedCells
        {
            get { return GetValue<List<Point>>(OccupiedCellsProperty); }
            set { SetValue(OccupiedCellsProperty, value); }
        }

        public static readonly PropertyData OccupiedCellsProperty = RegisterProperty("OccupiedCells", typeof(List<Point>));
    }
}
