using Catel.Data;
using Catel.MVVM;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class MarkViewModel : APageObjectBaseViewModel
    {
        #region Constructors

        /// <summary>Initializes a new instance of the BinReporterViewModel class.</summary>
        public MarkViewModel(Mark mark)
        {
            PageObject = mark;
            InitializeCommands();
            InitializeButtons();
        }

        private void InitializeCommands() { }

        private void InitializeButtons() { }

        #endregion //Constructors

        #region Model

        /// <summary>The shape of the Mark.</summary>
        [ViewModelToModel("PageObject")]
        public MarkShapes MarkShape
        {
            get { return GetValue<MarkShapes>(MarkShapeProperty); }
            set { SetValue(MarkShapeProperty, value); }
        }

        public static readonly PropertyData MarkShapeProperty = RegisterProperty("MarkShape", typeof(MarkShapes));

        /// <summary>The color of the Mark.</summary>
        [ViewModelToModel("PageObject")]
        public string MarkColor
        {
            get { return GetValue<string>(MarkColorProperty); }
            set { SetValue(MarkColorProperty, value); }
        }

        public static readonly PropertyData MarkColorProperty = RegisterProperty("MarkColor", typeof(string));
 
        #endregion //Model
    }
}