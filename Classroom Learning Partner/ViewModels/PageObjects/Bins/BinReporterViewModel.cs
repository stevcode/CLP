using CLP.Entities.Demo;

namespace Classroom_Learning_Partner.ViewModels
{
    public class BinReporterViewModel : APageObjectBaseViewModel
    {
        #region Constructors

        /// <summary>Initializes a new instance of the BinReporterViewModel class.</summary>
        public BinReporterViewModel(BinReporter binReporter)
        {
            PageObject = binReporter;
            InitializeButtons();
        }

        private void InitializeButtons() { _contextButtons.Clear(); }

        #endregion //Constructors
    }
}