using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Catel.Data;
using Catel.MVVM;
using CLP.CustomControls;
using CLP.Entities;

namespace Classroom_Learning_Partner.ViewModels
{
    public class MajorRibbonViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow
        {
            get { return App.MainWindowViewModel; }
        }

        public static CLPPage CurrentPage
        {
            get { return NotebookPagesPanelViewModel.GetCurrentPage(); }
        }

        public MajorRibbonViewModel()
        {
            InitializeCommands();
            SetRibbonButtons();
        }

        private void InitializeCommands()
        {
            ShowBackStageCommand = new Command(OnShowBackStageCommandExecute);
            InsertCircleCommand = new Command(OnInsertCircleCommandExecute);
        }

        #region Bindings

        /// <summary>
        /// List of the buttons currently on the Ribbon.
        /// </summary>
        public ObservableCollection<Button> Buttons
        {
            get { return GetValue<ObservableCollection<Button>>(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }

        public static readonly PropertyData ButtonsProperty = RegisterProperty("Buttons", typeof (ObservableCollection<Button>), () => new ObservableCollection<Button>()); 

        #endregion //Bindings
        

        #region Commands

        /// <summary>
        /// Brings up the BackStage.
        /// </summary>
        public Command ShowBackStageCommand { get; private set; }

        private void OnShowBackStageCommandExecute()
        {

        }

        #region Insert PageObject Commands

        /// <summary>
        /// Inserts a circle.
        /// </summary>
        public Command InsertCircleCommand { get; private set; }

        private void OnInsertCircleCommandExecute()
        {
            var circle = new Shape(CurrentPage, ShapeType.Ellipse);
            ACLPPageBaseViewModel.AddPageObjectToPage(circle);
        }

        #endregion //Insert PageObject Commands

        #endregion //Commands

        #region Methods

        public void SetRibbonButtons()
        {
            var insertShapeButton = new RibbonButton("Insert Circle", "pack://application:,,,/Resources/Images/Pile32.png", InsertCircleCommand);
            
            Buttons.Add(insertShapeButton);
        }

        #endregion //Methods
    }
}